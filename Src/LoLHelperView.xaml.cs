using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using Hardcodet.Wpf.TaskbarNotification;
using LoLHelper.Src.Commands;

namespace LoLHelper.Src
{
    /// <summary>
    /// Interaction logic for ShellView.xaml
    /// </summary>
    public partial class LoLHelperView : Window
    {
        private TaskbarIcon WindowsNotifyIcon;
        private bool isInitialized = false;

        public LoLHelperView()
        {
            InitializeComponent();
            InitializeNotifyIcon();

            Task.Factory.StartNew(ProcessGameMonitor, TaskCreationOptions.LongRunning);
        }

        private void InitializeNotifyIcon()
        {
            RelayCommand iconDoubleClickCommand = new RelayCommand(ShowWindow);

            WindowsNotifyIcon = new TaskbarIcon();
            WindowsNotifyIcon.Icon = new System.Drawing.Icon("logo.ico");
            ContextMenu context = new ContextMenu();
            MenuItem exit = new MenuItem();

            exit.Header = "退出";
            exit.Click += delegate (object sender, RoutedEventArgs e)
            {
                Environment.Exit(0);
            };
            context.Items.Add(exit);

            WindowsNotifyIcon.ContextMenu = context;
            WindowsNotifyIcon.DoubleClickCommand = iconDoubleClickCommand;
            WindowsNotifyIcon.Visibility = Visibility.Collapsed;
        }

        private void ProcessGameMonitor()
        {
            while (true)
            {
                Dispatcher.Invoke(() =>
                {
                    if ((Tag as LoLHelperViewModel).IsClosedGame)
                    {
                        isInitialized = false;

                        HideWindow();

                        (Tag as LoLHelperViewModel).IsClosedGame = false;
                    }

                    if ((Tag as LoLHelperViewModel).IsInitialized == true && isInitialized == false)
                    {
                        isInitialized = true;

                        ShowWindow();
                    }
                });

                Thread.Sleep(200);
            }
        }

        private void Window_StateChanged(object sender, EventArgs e)
        {
            if (WindowState == WindowState.Minimized)
            {
                if ((Tag as LoLHelperViewModel) != null && ((Tag as LoLHelperViewModel).IsMinimizie || !(Tag as LoLHelperViewModel).IsInitialized))
                {
                    HideWindow();
                }
            }
        }

        private void ShowWindow()
        {
            WindowsNotifyIcon.Visibility = Visibility.Collapsed;
            Thread.Sleep(200);
            Show();
            WindowState = WindowState.Normal;
            Topmost = true;
            Topmost = false;
        }

        private void HideWindow()
        {
            WindowsNotifyIcon.Visibility = Visibility.Visible;
            Hide();
        }

        private void RunToggleButtonClick(object sender, RoutedEventArgs e)
        {
            ToggleButton toggleButton = sender as ToggleButton;

            if (toggleButton.IsChecked == true && (Tag as LoLHelperViewModel) != null && (Tag as LoLHelperViewModel).IsMinimizie)
            {
                HideWindow();
            }
        }
    }
}
