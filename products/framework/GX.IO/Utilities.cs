using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Runtime.InteropServices;

namespace GX.IO
{
    public delegate bool ConfirmCopyCallback(FileSystemInfo source, string destFileName);
    public delegate void NotifyCopyCallback(FileSystemInfo source, string destFileName);
    public delegate bool ConfirmCreateDirectoryCallback(string directoryName);
    public delegate void NotifyCreateDirectoryCallback(string directoryName);
    public delegate CopyProgressResult ProgressUpdate<T>(T state, long total, long finished);

    public enum CopyProgressResult : uint
    {
        PROGRESS_CONTINUE = 0,
        PROGRESS_CANCEL = 1,
        PROGRESS_STOP = 2,
        PROGRESS_QUIET = 3
    }

    public static class Utilities
    {
        private class ProgressHandlerAdapter<T>
        {
            ProgressUpdate<T> progressUpdate;
            public T State { get; private set; }
            public ProgressHandlerAdapter(T state, ProgressUpdate<T> progressUpdate)
            {
                State = state;
                if (progressUpdate != null)
                {
                    this.progressUpdate = progressUpdate;
                }
                else
                {
                    this.progressUpdate = DefaultProgressUpdate;
                }
            }

            CopyProgressResult DefaultProgressUpdate(T state, long total, long finished)
            {
                return CopyProgressResult.PROGRESS_CONTINUE;
            }

            public CopyProgressResult CopyProgressRoutine(
                long TotalFileSize,
                long TotalBytesTransferred,
                long StreamSize,
                long StreamBytesTransferred,
                uint dwStreamNumber,
                CopyProgressCallbackReason dwCallbackReason,
                IntPtr hSourceFile,
                IntPtr hDestinationFile,
                IntPtr lpData)
            {
                return progressUpdate(State, TotalFileSize, TotalBytesTransferred);
            }
        }

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool CopyFileEx(string lpExistingFileName, string lpNewFileName,
           CopyProgressRoutine lpProgressRoutine, IntPtr lpData, ref Int32 pbCancel,
           CopyFileFlags dwCopyFlags);

        private delegate CopyProgressResult CopyProgressRoutine(
            long TotalFileSize,
            long TotalBytesTransferred,
            long StreamSize,
            long StreamBytesTransferred,
            uint dwStreamNumber,
            CopyProgressCallbackReason dwCallbackReason,
            IntPtr hSourceFile,
            IntPtr hDestinationFile,
            IntPtr lpData);

        enum CopyProgressCallbackReason : uint
        {
            CALLBACK_CHUNK_FINISHED = 0x00000000,
            CALLBACK_STREAM_SWITCH = 0x00000001
        }

        [Flags]
        enum CopyFileFlags : uint
        {
            COPY_FILE_FAIL_IF_EXISTS = 0x00000001,
            COPY_FILE_RESTARTABLE = 0x00000002,
            COPY_FILE_OPEN_SOURCE_FOR_WRITE = 0x00000004,
            COPY_FILE_ALLOW_DECRYPTED_DESTINATION = 0x00000008
        }

        public static bool CopyFile<T>(FileInfo source, string destination, int retryCount, ref int pbCancel, T progressState, ProgressUpdate<T> progressUpdate, ConfirmCopyCallback confirmCopy, NotifyCopyCallback notifyCopy)
        {
            Exception lastException = null;
            do
            {
                try
                {
                    return CopyFile<T>(source, destination, ref pbCancel, progressState, progressUpdate, confirmCopy, notifyCopy);
                }
                catch (Exception ex)
                {
                    lastException = ex;
                }
            } while (--retryCount >= 0);
            throw lastException;
        }

        public static bool CopyFile<T>(FileInfo source, string destination, ref int pbCancel, T progressState, ProgressUpdate<T> progressUpdate, ConfirmCopyCallback confirmCopy, NotifyCopyCallback notifyCopy)
        {
            if (ConfirmCopy(confirmCopy, source, destination))
            {
                ProgressHandlerAdapter<T> adapter = new ProgressHandlerAdapter<T>(progressState, progressUpdate);
                CopyFileEx(source.FullName, destination, adapter.CopyProgressRoutine, IntPtr.Zero, ref pbCancel, CopyFileFlags.COPY_FILE_RESTARTABLE);
                NotifyCopy(notifyCopy, source, destination);
                return true;
            }
            return false;
        }

        public static bool CopyFile(FileInfo source, string destination, ConfirmCopyCallback confirmCopy, NotifyCopyCallback notifyCopy)
        {
            if (ConfirmCopy(confirmCopy, source, destination))
            {
                source.CopyTo(destination, true);
                NotifyCopy(notifyCopy, source, destination);
                return true;
            }
            return false;
        }

        public static bool CopyFile(FileInfo source, string destination, ConfirmCopyCallback confirmCopy, NotifyCopyCallback notifyCopy, ConfirmCreateDirectoryCallback confirmCreateDirectory, NotifyCreateDirectoryCallback notifyCreateDirectory)
        {
            string directoryName = Path.GetDirectoryName(destination);
            if (CreateDirectory(directoryName, confirmCreateDirectory, notifyCreateDirectory))
            {
                return CopyFile(source, destination, confirmCopy, notifyCopy);
            }
            return false;
        }

        public static bool CopyFile<T>(FileInfo source, string destination, ref int pbCancel, T progressState, ProgressUpdate<T> progressUpdate, ConfirmCopyCallback confirmCopy, NotifyCopyCallback notifyCopy, ConfirmCreateDirectoryCallback confirmCreateDirectory, NotifyCreateDirectoryCallback notifyCreateDirectory)
        {
            string directoryName = Path.GetDirectoryName(destination);
            if (CreateDirectory(directoryName, confirmCreateDirectory, notifyCreateDirectory))
            {
                return CopyFile(source, destination, ref pbCancel, progressState, progressUpdate, confirmCopy, notifyCopy);
            }
            return false;
        }

        public static bool CopyDirectory(DirectoryInfo source, string destination, ConfirmCopyCallback confirmCopy, NotifyCopyCallback notifyCopy, ConfirmCreateDirectoryCallback confirmCreateDirectory, NotifyCreateDirectoryCallback notifyCreateDirectory)
        {
            if (ConfirmCopy(confirmCopy, source, destination))
            {
                if (CreateDirectory(destination, confirmCreateDirectory, notifyCreateDirectory))
                {
                    foreach (FileSystemInfo fsi in source.GetFileSystemInfos())
                    {
                        if (fsi is FileInfo)
                        {
                            CopyFile((FileInfo)fsi, Path.Combine(destination, fsi.Name), confirmCopy, notifyCopy);
                        }
                        else if (fsi is DirectoryInfo)
                        {
                            CopyDirectory((DirectoryInfo)source, Path.Combine(destination, source.Name), confirmCopy, notifyCopy, confirmCreateDirectory, notifyCreateDirectory);
                        }
                    }
                }
                NotifyCopy(notifyCopy, source, destination);
                return true;
            }
            return false;
        }

        public static bool CreateDirectory(string directoryName, ConfirmCreateDirectoryCallback confirmCreateDirectory, NotifyCreateDirectoryCallback notifyCreateDirectory)
        {
            if (!Directory.Exists(directoryName))
            {
                if (ConfirmCreateDirectory(confirmCreateDirectory, directoryName))
                {
                    Directory.CreateDirectory(directoryName);
                    NotifyCreateDirectory(notifyCreateDirectory, directoryName);
                    return true;
                }
            }
            return false;
        }

        private static bool ConfirmCopy(ConfirmCopyCallback confirmCopy, FileSystemInfo source, string destination)
        {
            if (confirmCopy != null)
                return confirmCopy(source, destination);
            return true;
        }

        private static void NotifyCopy(NotifyCopyCallback notifyCopy, FileSystemInfo source, string destination)
        {
            if (notifyCopy != null)
                notifyCopy(source, destination);
        }

        private static bool ConfirmCreateDirectory(ConfirmCreateDirectoryCallback confirmCreateDirectory, string directoryName)
        {
            if (confirmCreateDirectory != null)
            {
                return confirmCreateDirectory(directoryName);
            }
            return true;
        }

        private static void NotifyCreateDirectory(NotifyCreateDirectoryCallback notifyCreateDirectory, string directoryName)
        {
            if (notifyCreateDirectory != null)
            {
                notifyCreateDirectory(directoryName);
            }
        }
    }
}
