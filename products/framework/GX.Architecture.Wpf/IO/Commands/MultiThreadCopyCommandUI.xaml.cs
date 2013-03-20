using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using GX.Architecture.IO.Commands;
using System.IO;
using System.Threading;
using GX.Patterns;
using GX.Patterns.Progress;
using System.Windows.Threading;
using GX.IO;

namespace GX.Architecture.Wpf.IO.Commands
{
    /// <summary>
    /// Interaction logic for MultiThreadCopyCommandUI.xaml
    /// </summary>
    public partial class MultiThreadCopyCommandUI : UserControl
    {
        public MultiThreadCopyCommand Command { get; private set; }

        public bool IsExpanded
        {
            get { return details.Visibility == System.Windows.Visibility.Visible; }
            set
            {
                if (value)
                    details.Visibility = System.Windows.Visibility.Visible;
                else
                    details.Visibility = System.Windows.Visibility.Collapsed;
            }
        }

        private long started = 0;
        private long completed = 0;
        private long Started
        {
            get { return started; }
            set
            {
                started = value;
                UpdateStatus();
            }
        }
        private long Completed
        {
            get { return completed; }
            set
            {
                completed = value;
                UpdateStatus();
            }
        }
        private string lastCopiedFile = null;

        public MultiThreadCopyCommandUI(string[] sources, string[] destinations, int retryCount)
        {
            Command = new MultiThreadCopyCommand(sources, destinations, retryCount, null, null, null, null);
            InitializeComponent();
            Command.CopyFileStart += new EventHandler<Patterns.WorkItemEventArgs<Architecture.IO.Commands.CopyFileWorkItem>>(Command_CopyFileStart);
            Command.CopyFileProgressUpdate += new EventHandler<Patterns.Progress.ProgressEventArgs<Architecture.IO.Commands.CopyFileWorkItem>>(Command_CopyFileProgressUpdate);
            Command.CopyFileComplete += new EventHandler<Patterns.WorkItemEventArgs<Architecture.IO.Commands.CopyFileWorkItem>>(Command_CopyFileComplete);
            Command.Start += new EventHandler(Command_Start);
            Command.Complete += new EventHandler(Command_Complete);
        }

        void Command_Complete(object sender, EventArgs e)
        {
            this.Dispatcher.BeginInvoke(new VoidFunc(MainProgressComplete), DispatcherPriority.Send);
        }

        void Command_Start(object sender, EventArgs e)
        {
            this.Dispatcher.BeginInvoke(new VoidFunc(MainProgressStart));
        }

        void UpdateStatus(string text)
        {
            textStatus.Text = text;
        }

        void UpdateStatus()
        {
            UpdateStatus(string.Format("{0} ({1}/{2})", lastCopiedFile, Completed, Started));
        }

        void MainProgressStart()
        {
            mainProgress.Value = mainProgress.Minimum;
        }

        void MainProgressComplete()
        {
            mainProgress.Value = mainProgress.Maximum;
        }

        void MainProgressUpdate(CopyFileWorkItem workItem)
        {
            mainProgress.Value += mainProgress.Maximum * workItem.ProgressWeight;
        }

        void RemoveProgressBar(CopyFileWorkItem workItem)
        {
            Grid grid = workItem.Tag as Grid;
            if (grid != null)
            {
                progressPanel.Children.Remove(grid);
                workItem.Tag = null;
            }
        }

        void AddProgressBar(CopyFileWorkItem workItem)
        {
            FileInfo fi = workItem.Item as FileInfo;
            if (fi != null)
            {
                ProgressBar progressBar = new ProgressBar();
                progressBar.Height = 19;
                Grid grid = new Grid();
                workItem.Tag = grid;
                TextBlock text = new TextBlock();
                text.HorizontalAlignment = System.Windows.HorizontalAlignment.Center;
                text.VerticalAlignment = System.Windows.VerticalAlignment.Center;
                text.Text = string.Format("{0}({1})", workItem.Item.FullName, fi.Length.FormatSize());
                grid.Children.Add(progressBar);
                grid.Children.Add(text);
                progressPanel.Children.Add(grid);
            }
        }

        void Command_CopyFileStart(WorkItemEventArgs<CopyFileWorkItem> e)
        {
            ++Started;

            AddProgressBar(e.WorkItem);
        }

        void Command_CopyFileComplete(WorkItemEventArgs<CopyFileWorkItem> e)
        {
            if (e.WorkItem.Item is FileInfo)
            {
                lastCopiedFile = e.WorkItem.Item.FullName;
                RemoveProgressBar(e.WorkItem);
            }
            ++Completed;

            Grid grid = e.WorkItem.Tag as Grid;
            if (grid != null)
            {
                ProgressBar progressBar = (ProgressBar)grid.Children[0];
                progressBar.Value = progressBar.Maximum;
            }
            MainProgressUpdate(e.WorkItem);
        }

        void Command_CopyFileProgressUpdate(ProgressEventArgs<CopyFileWorkItem> e)
        {
            Grid grid = e.WorkItem.Tag as Grid;
            if (grid != null)
            {
                if (e.Total > 0)
                {
                    ProgressBar progressBar = (ProgressBar)grid.Children[0];
                    progressBar.Value = e.Finished * progressBar.Maximum / e.Total;
                }
            }
        }

        void Command_CopyFileStart(object sender, WorkItemEventArgs<CopyFileWorkItem> e)
        {
            this.Dispatcher.BeginInvoke(new VoidFunc<WorkItemEventArgs<CopyFileWorkItem>>(Command_CopyFileStart), DispatcherPriority.ApplicationIdle, e);
        }

        void Command_CopyFileComplete(object sender, WorkItemEventArgs<CopyFileWorkItem> e)
        {
            this.Dispatcher.BeginInvoke(new VoidFunc<WorkItemEventArgs<CopyFileWorkItem>>(Command_CopyFileComplete), DispatcherPriority.ApplicationIdle, e);
        }

        void Command_CopyFileProgressUpdate(object sender, ProgressEventArgs<CopyFileWorkItem> e)
        {
            this.Dispatcher.BeginInvoke(new VoidFunc<ProgressEventArgs<CopyFileWorkItem>>(Command_CopyFileProgressUpdate), DispatcherPriority.ApplicationIdle, e);
        }

        private Grid FindProgressBar(CopyFileWorkItem workItem)
        {
            return workItem.Tag as Grid;
        }

        public string[] Source { get { return Command.Sources; } }

        public string[] Destination { get { return Command.Destinations; } }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            Command.Do();
        }
    }
}
