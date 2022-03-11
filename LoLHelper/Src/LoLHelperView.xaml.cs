using System;
using System.IO;
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
        private readonly Uri darkThemeUri = new Uri("../Src/Resources/DarkTheme.xaml", UriKind.RelativeOrAbsolute);
        private readonly Uri lightThemeUri = new Uri("../Src/Resources/LightTheme.xaml", UriKind.RelativeOrAbsolute);

        public LoLHelperView()
        {
            InitializeComponent();
            InitializeNotifyIcon();

            if (switchToggleButton.IsChecked == true)
            {
                if (Application.LoadComponent(darkThemeUri) is ResourceDictionary resourceDict)
                {
                    Application.Current.Resources.MergedDictionaries.Add(resourceDict);
                }
            }
            else
            {
                if (Application.LoadComponent(lightThemeUri) is ResourceDictionary resourceDict)
                {
                    Application.Current.Resources.MergedDictionaries.Add(resourceDict);
                }
            }

            switchToggleButton.IsChecked = Properties.Settings.Default.DarkMode;

            Task.Factory.StartNew(ProcessGameMonitor, TaskCreationOptions.LongRunning);
        }

        private void InitializeNotifyIcon()
        {
            RelayCommand iconClickCommand = new RelayCommand(ShowWindow);

            WindowsNotifyIcon = new TaskbarIcon();
            if (File.Exists("logo.ico"))
            {
                WindowsNotifyIcon.Icon = new System.Drawing.Icon("logo.ico");
            }

            ContextMenu context = new ContextMenu();
            MenuItem exit = new MenuItem();

            exit.Header = "退出";
            exit.Click += delegate (object sender, RoutedEventArgs e)
            {
                Environment.Exit(0);
            };
            context.Items.Add(exit);

            WindowsNotifyIcon.ContextMenu = context;
            WindowsNotifyIcon.LeftClickCommand = iconClickCommand;
            WindowsNotifyIcon.Visibility = Visibility.Collapsed;
        }

        private void ProcessGameMonitor()
        {
            try
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
            catch (Exception err)
            {
                LogManager.WriteLog($"{err}");
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

        public void OnChecked()
        {
            if (Application.LoadComponent(darkThemeUri) is ResourceDictionary resourceDict)
            {
                Application.Current.Resources.MergedDictionaries.Add(resourceDict);
            }

            Properties.Settings.Default.DarkMode = !switchToggleButton.IsChecked;
            Properties.Settings.Default.Save();
        }

        public void OnUnChecked()
        {
            if (Application.LoadComponent(lightThemeUri) is ResourceDictionary resourceDict)
            {
                Application.Current.Resources.MergedDictionaries.Add(resourceDict);
            }

            Properties.Settings.Default.DarkMode = !switchToggleButton.IsChecked;
            Properties.Settings.Default.Save();
        }
    }
}
