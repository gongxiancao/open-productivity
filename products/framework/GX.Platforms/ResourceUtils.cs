using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.ComponentModel;
using System.IO;
using System.Xml;

namespace GX.Platforms
{
    public class ResourceReader
    {
        class RT_TYPES
        {
            public const uint RT_CURSOR = 0x00000001;
            public const uint RT_BITMAP = 0x00000002;
            public const uint RT_ICON = 0x00000003;
            public const uint RT_MENU = 0x00000004;
            public const uint RT_DIALOG = 0x00000005;
            public const uint RT_STRING = 0x00000006;
            public const uint RT_FONTDIR = 0x00000007;
            public const uint RT_FONT = 0x00000008;
            public const uint RT_ACCELERATOR = 0x00000009;
            public const uint RT_RCDATA = 0x00000010;
            public const uint RT_MESSAGETABLE = 0x00000011;
            public const uint RT_MANIFEST = 24;
        };

        private const uint LOAD_LIBRARY_AS_DATAFILE = 0x00000002;

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr LoadLibraryEx(
          string lpFileName,
          IntPtr hFile,
          uint dwFlags);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool FreeLibrary(IntPtr hModule);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        static extern int LoadString(IntPtr hInstance, uint uID, StringBuilder lpBuffer, int nBufferMax);

        [DllImport("kernel32.dll", EntryPoint = "EnumResourceNamesW",
          CharSet = CharSet.Unicode, SetLastError = true)]
        static extern bool EnumResourceNamesWithName(
          IntPtr hModule,
          string lpszType,
          EnumResNameDelegate lpEnumFunc,
          IntPtr lParam);

        [DllImport("kernel32.dll", EntryPoint = "EnumResourceNamesW",
          CharSet = CharSet.Unicode, SetLastError = true)]
        static extern bool EnumResourceNamesWithID(
          IntPtr hModule,
          uint lpszType,
          EnumResNameDelegate lpEnumFunc,
          IntPtr lParam);

        private delegate bool EnumResNameDelegate(
          IntPtr hModule,
          IntPtr lpszType,
          IntPtr lpszName,
          IntPtr lParam);

        [DllImport("kernel32.dll", EntryPoint = "FindResource")]
        public static extern IntPtr FindResource(IntPtr hModule, IntPtr lpName, IntPtr lpType);

        [DllImport("kernel32.dll", EntryPoint = "SizeofResource")]
        public static extern uint SizeofResource(IntPtr hModule, IntPtr hResInfo);

        [DllImport("kernel32.dll", EntryPoint = "LoadResource")]
        public static extern IntPtr LoadResource(IntPtr hModule, IntPtr hResInfo);

        [DllImport("kernel32.dll", EntryPoint = "LockResource")]
        public static extern IntPtr LockResource(IntPtr hResData);

        [DllImport("kernel32.dll", EntryPoint = "FreeResource")]
        public static extern int FreeResource(IntPtr hResData);

        private static bool IS_INTRESOURCE(IntPtr value)
        {
            if (((uint)value) > ushort.MaxValue)
                return false;
            return true;
        }
        private static uint GET_RESOURCE_ID(IntPtr value)
        {
            if (IS_INTRESOURCE(value))
                return (uint)value;
            throw new System.NotSupportedException("value is not an ID!");
        }
        private static string GET_RESOURCE_NAME(IntPtr value)
        {
            if (IS_INTRESOURCE(value))
                return value.ToString();
            return Marshal.PtrToStringUni((IntPtr)value);
        }

        private Dictionary<string, byte[]> resources = new Dictionary<string, byte[]>(StringComparer.OrdinalIgnoreCase);

        static void ThrowLastWin32Error(string fileName)
        {
            throw new Win32Exception(Marshal.GetLastWin32Error());
        }

        private bool EnumRes(IntPtr hModule, IntPtr lpType, IntPtr lpName, IntPtr lParam)
        {
            try
            {
                IntPtr hResInfo = FindResource(hModule, lpName, lpType);
                uint cbResource = SizeofResource(hModule, hResInfo);

                IntPtr hResData = LoadResource(hModule, hResInfo);
                IntPtr pResource = LockResource(hResData);

                string filename = null;
                if (IS_INTRESOURCE(lpName))
                {
                    filename = string.Format("#{0}", lpName);
                }
                else
                {
                    filename = string.Format("{0}", lpName);
                }

                byte[] data = new byte[cbResource];

                Marshal.Copy(pResource, data, 0, (int)cbResource);
                resources[filename] = data;

                FreeResource(hResData);

                return true;
            }
            catch (Exception)
            {
                throw;
            }
        }

        private StringBuilder buffer = new StringBuilder(1024);
        private Dictionary<uint, string> strings = new Dictionary<uint, string>();
        private bool EnumString(IntPtr hModule, IntPtr lpType, IntPtr lpName, IntPtr lParam)
        {
            try
            {
                uint stringId = (uint)lpName;
                LoadString(hModule, (uint)stringId, buffer, buffer.Capacity);
                strings[stringId] = buffer.ToString();
                return true;
            }
            catch (Exception)
            {
                throw;
            }
        }

        private string moduleName;
        public ResourceReader(string moduleName)
        {
            this.moduleName = moduleName;
        }

        public Dictionary<string, byte[]> ReadManifest()
        {
            IntPtr hModule = LoadLibraryEx(moduleName, IntPtr.Zero, LOAD_LIBRARY_AS_DATAFILE);

            resources.Clear();
            if (EnumResourceNamesWithID(hModule, RT_TYPES.RT_MANIFEST,
              new EnumResNameDelegate(EnumRes), IntPtr.Zero) == false)
            {
                ThrowLastWin32Error(moduleName);
            }

            FreeLibrary(hModule);
            return resources;
        }

        public Dictionary<uint, string> ReadString()
        {
            IntPtr hModule = LoadLibraryEx(moduleName, IntPtr.Zero, LOAD_LIBRARY_AS_DATAFILE);

            resources.Clear();
            if (EnumResourceNamesWithID(hModule, RT_TYPES.RT_STRING,
              new EnumResNameDelegate(EnumString), IntPtr.Zero) == false)
            {
                ThrowLastWin32Error(moduleName);
            }

            FreeLibrary(hModule);
            return strings;
        }
    }

    public class ResourceUtils
    {
        public static Dictionary<string, byte[]> GetManifest(string file)
        {
            ResourceReader reader = new ResourceReader(file);
            return reader.ReadManifest();
        }

        public static string GetManifestString(byte[] buffer, bool omitXmlDeclaration)
        {
            using (MemoryStream stream = new MemoryStream(buffer))
            {
                XmlDocument doc = new XmlDocument();
                doc.Load(stream);
                StringBuilder sb = new StringBuilder();
                XmlWriterSettings settings = new XmlWriterSettings();
                settings.OmitXmlDeclaration = true;
                using (XmlWriter xw = XmlWriter.Create(sb, settings))
                {
                    doc.WriteContentTo(xw);
                }
                return sb.ToString();
            }
        }

        public static string GetManifestString(string manifestFilePath, bool omitXmlDeclaration)
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(manifestFilePath);
            StringBuilder sb = new StringBuilder();
            XmlWriterSettings settings = new XmlWriterSettings();
            settings.OmitXmlDeclaration = true;
            using (XmlWriter xw = XmlWriter.Create(sb, settings))
            {
                doc.WriteContentTo(xw);
            }
            return sb.ToString();
        }


        public static Dictionary<uint, string> GetStrings(string file)
        {
            ResourceReader reader = new ResourceReader(file);
            return reader.ReadString();
        }
    }
}
