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
using GX.Architecture.Wpf.IO.Commands;
using System.Threading;
using GX.Architecture.IO.Commands;
using GX.IO;
using System.Windows.Threading;
using GX.Architecture.Configuration.CommandLine;
using GX.Architecture.Attributes;

namespace mtcopy
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        MultiThreadCopyCommandProgressMonitor monitor = null;
        MultiThreadCopyCommandUI commandUI = null;
        double ExpandedHeight = 0;
        double CollapsedHeight = 0;

        [ApplicationEntryPoint]
        public void Main(
            [Name(null), Alias(null)]
            string application,
            [Alias("src")]
            string source,
            [Alias("dest")]
            string destination,
            [Alias("inc")]
            string include)
        {
            this.Title = string.Format("Copy from {0} to {1}", source, destination);
            this.sourceText.Text = source;
            this.destinationText.Text = destination;
            ThreadPool.SetMaxThreads(30, 30);

            List<string> sources = new List<string>();
            List<string> destinations = new List<string>();
            if (!string.IsNullOrEmpty(include))
            {
                foreach (string inc in include.Split(','))
                {
                    sources.Add(System.IO.Path.Combine(source, inc));
                    destinations.Add(System.IO.Path.Combine(destination, inc));
                }
            }

            if (sources.Count == 0)
            {
                sources.Add(source);
                destinations.Add(destination);
            }
            commandUI = new MultiThreadCopyCommandUI(sources.ToArray(), destinations.ToArray(), 10);
            commandUI.Command.Complete += new EventHandler(commandUI_Complete);
            commandUI.Command.Error += new EventHandler<GX.Patterns.ErrorEventArgs>(Command_Error);
            monitor = new MultiThreadCopyCommandProgressMonitor(commandUI.Command);
            gridContainer.Children.Add(commandUI);

            DispatcherTimer timer = new DispatcherTimer(new TimeSpan(0, 0, 1), DispatcherPriority.ApplicationIdle, TimerUpdate, this.Dispatcher);
        }

        void Command_Error(object sender, GX.Patterns.ErrorEventArgs e)
        {
            if (MessageBox.Show(e.Error.Message + "\nContinue?", "Error", MessageBoxButton.YesNo, MessageBoxImage.Error) == MessageBoxResult.No)
            {
                this.Close();
            }
        }

        public MainWindow()
        {
            InitializeComponent();

            try
            {
                CommandInvoker.Invoke(this, Environment.CommandLine);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                this.Close();
            }

        }

        void TimerUpdate(object sender, EventArgs e)
        {
            this.timeText.Text = (DateTime.Now - monitor.StartTime).ToString("dd\\:hh\\:mm\\:ss");
            this.transferedText.Text = monitor.TransferedSize.FormatSize();
            this.averageSpeedText.Text = string.Format("{0}/s", monitor.Speed.FormatSize());
            this.speedText.Text = string.Format("{0}/s", monitor.RealTimeSpeed.FormatSize());
        }

        void UpdateSpeed(MultiThreadCopyCommandProgressMonitor monitor, EventArgs e)
        {
            this.transferedText.Text = monitor.TransferedSize.FormatSize();
        }

        void commandUI_Complete(object sender, EventArgs e)
        {
            //this.Close();
            this.Dispatcher.BeginInvoke(new VoidFunc(Close), System.Windows.Threading.DispatcherPriority.Send);
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            SizeToContent = System.Windows.SizeToContent.Manual;
            this.MinHeight = this.ActualHeight;
        }

        private void expandDetails_Collapsed(object sender, RoutedEventArgs e)
        {
            this.commandUI.IsExpanded = expandDetails.IsExpanded;

            if (this.WindowState != System.Windows.WindowState.Maximized)
            {
                ExpandedHeight = ActualHeight;

                expandAnimation.From = ActualHeight;
                expandAnimation.To = CollapsedHeight;

                BeginAnimation(Window.HeightProperty, expandAnimation);
            }
        }

        private void expandDetails_Expanded(object sender, RoutedEventArgs e)
        {
            this.commandUI.IsExpanded = expandDetails.IsExpanded;

            if (this.WindowState != System.Windows.WindowState.Maximized)
            {
                if (ExpandedHeight < 1.5 * MinHeight)
                {
                    ExpandedHeight = 1.5 * MinHeight;
                }

                CollapsedHeight = ActualHeight;

                expandAnimation.From = ActualHeight;
                expandAnimation.To = ExpandedHeight;
                BeginAnimation(Window.HeightProperty, expandAnimation);
            }

        }

        private void cancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
