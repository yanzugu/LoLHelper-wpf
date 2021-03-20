using LoLHelper_rework_wpf_.Implements;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Threading;

namespace LoLHelper_rework_wpf_
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        LeagueClient leagueClient;
        ChampSelect champSelect;
        LoLHelper_rework_wpf_.Implements.Match match;
        Rune rune;
        Summoner summoner;

        bool isRunning = false, isInitializing = false;
        List<string> threadNames;
        Dictionary<string, Thread> threadPool;
        Dictionary<string, ManualResetEvent> eventPool;
        Zh_Ch zh_ch;
        string lane;
        int times, championId;
        bool isLock;
        string lockfile;
        Thread rkThread;
        System.Windows.Forms.NotifyIcon ni;

        public MainWindow()
        {
            InitializeComponent();      
            zh_ch = new Zh_Ch();
            TB_Path.Text = Properties.Settings.Default.TB_Path;

            ni = new System.Windows.Forms.NotifyIcon();
            ni.Icon = new System.Drawing.Icon("lh2_5jL_icon.ico");
            ni.DoubleClick += PopUp;

            Monitor();
        }

        private delegate void Btn_Click_Delegate(Control control);

        private delegate void Initailize_Delegate();

        private delegate void Reset_Delegate();

        private void Btn_Confirm_Click(object sender, RoutedEventArgs e)
        {
            Initialize();
        }

        private void Btn_Click(Control control)
        {
            if (!Dispatcher.CheckAccess())
            {
                Dispatcher.Invoke(DispatcherPriority.Send, new Btn_Click_Delegate(Btn_Click), control);
            }
            else
            {
                ((Button)control).RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));
                ((Button)control).IsEnabled = !((Button)control).IsEnabled;
            }
        }   

        private void Btn_Run_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                isRunning = !isRunning;
                Button button = sender as Button;

                if (isRunning)
                {
                    foreach (var name in threadNames)
                    {
                        eventPool[name].Set();
                    }
                    Grid_CB.IsEnabled = false;
                    button.Content = "結束";
                    if (CB_Minimize.IsChecked == true)
                    {
                        this.WindowState = WindowState.Minimized;
                    }
                }
                else
                {
                    foreach (string name in threadNames)
                    {
                        eventPool[name].Reset();
                    }
                    Grid_CB.IsEnabled = true;
                    button.Content = "開始";
                }
                Remember_Setting();
            }
            catch { }
        }

        private bool Check_Game_Launch()
        {
            return File.Exists(lockfile);
        }

        private void Create_Champion_ComboBox_Items()
        {
            try
            {
                CBB_Champion.Items.Clear();
                CBB_Champion.DisplayMemberPath = "Key";
                CBB_Champion.SelectedValuePath = "Value";
                Dictionary<string, int> champions = null;
                int c = 0;
                while ((champions == null || champions.Count == 0) && c < 15)
                {
                    c++;
                    champions = leagueClient.Get_Owned_Champions_Dict();
                    Thread.Sleep(1000);
                }
                CBB_Champion.ItemsSource = champions.
                    Select(s => s = new KeyValuePair<string, int>(zh_ch.en_to_ch(s.Key), s.Value));
                if (CBB_Champion.Items.Count != 0)
                {
                    CBB_Champion.SelectedItem = CBB_Champion.Items[0];
                }
            }
            catch { }
        }

        private void Create_Lane_ComboBox_Items()
        {
            try
            {
                CBB_Lane.Items.Clear();
                CBB_Lane.Items.Add("Top");
                CBB_Lane.Items.Add("JG");
                CBB_Lane.Items.Add("Mid");
                CBB_Lane.Items.Add("Sup");
                CBB_Lane.Items.Add("AD");
                //CBB_Lane.SelectedIndex = 0;
            }
            catch { }
        }

        private void Create_ThreadPool()
        {
            try
            {
                threadPool = new Dictionary<string, Thread>();
                eventPool = new Dictionary<string, ManualResetEvent>();
                Thread thread;

                eventPool.Add("CB_Queue", new ManualResetEvent(false));
                thread = new Thread(() =>
                {
                    eventPool["CB_Queue"].WaitOne();
                });
                threadPool.Add("CB_Queue", thread);
                thread.IsBackground = true;
                thread.Start();

                eventPool.Add("CB_Accept", new ManualResetEvent(false));
                thread = new Thread(() =>
                {
                    eventPool["CB_Accept"].WaitOne();
                    match.Find_Match();
                });
                threadPool.Add("CB_Accept", thread);
                thread.IsBackground = true;
                thread.Start();

                eventPool.Add("CB_PickLane", new ManualResetEvent(false));
                thread = new Thread(() =>
                {
                    string preLane = null;
                    while (true)
                    {
                        eventPool["CB_PickLane"].WaitOne();
                        if (match.Get_Gameflow() != "\"ChampSelect\"")
                        {
                            preLane = null;
                        }
                        else
                        {
                            if (lane != preLane)
                            {
                                champSelect.Pick_Selected_Lane(lane, times);
                                preLane = lane;
                            }
                        }

                        Thread.Sleep(500);
                    }
                });
                threadPool.Add("CB_PickLane", thread);
                thread.IsBackground = true;
                thread.Start();

                eventPool.Add("CB_PickChamp", new ManualResetEvent(false));
                thread = new Thread(() =>
                {
                    int? preChampionId = null;
                    while (true)
                    {
                        eventPool["CB_PickChamp"].WaitOne();
                        if (match.Get_Gameflow() != "\"ChampSelect\"")
                        {
                            preChampionId = null;
                        }
                        else
                        {
                            if (championId != preChampionId)
                            {
                                champSelect.Pick_Champion(championId, isLock);
                                preChampionId = championId;
                            }
                        }
                        Thread.Sleep(500);
                    }
                });
                threadPool.Add("CB_PickChamp", thread);
                thread.IsBackground = true;
                thread.Start();

                eventPool.Add("CB_ChangeSpell", new ManualResetEvent(false));
                thread = new Thread(() =>
                {
                    while (true)
                    {
                        eventPool["CB_ChangeSpell"].WaitOne();
                    }
                });
                threadPool.Add("CB_ChangeSpell", thread);
                thread.IsBackground = true;
                thread.Start();

                eventPool.Add("CB_ChangeRune", new ManualResetEvent(false));
                thread = new Thread(() =>
                {
                    string preChampion = null;
                    while (true)
                    {
                        eventPool["CB_ChangeRune"].WaitOne();
                        if (match.Get_Gameflow() == "\"ChampSelect\"")
                        {
                            var championId = champSelect.Get_My_Pick_ChampionId();
                            var position = champSelect.Get_My_Position();
                            string champion;
                            if (championId != null)
                            {
                                champion = leagueClient.Get_Owned_Champions_Dict().FirstOrDefault(x => x.Value == championId).Key;
                                if (champion != null && champion != preChampion)
                                {
                                    rune.Set_Rune(champion, position);
                                    preChampion = champion;
                                }
                            }
                        }
                        Thread.Sleep(500);
                    }
                });
                threadPool.Add("CB_ChangeRune", thread);
                thread.IsBackground = true;
                thread.Start();

                if (rkThread != null)
                    rkThread.Abort();
                rkThread = new Thread(() =>
                {
                    bool isShowed = false;
                    while (true)
                    {
                        if (match.Get_Gameflow() == "\"ChampSelect\"")
                        {
                            if (isShowed == false)
                            {
                                isShowed = true;
                                summoner.Show_Teammates_Ranked();
                            }
                        }
                        else
                        {
                            isShowed = false;
                        }
                        Thread.Sleep(2000);
                    }
                });
                rkThread.IsBackground = true;
                rkThread.Start();
            }
            catch { }
        }

        private void CB_Lock_Change(object sender, RoutedEventArgs e)
        {
            try
            {
                isLock = (bool)((CheckBox)sender).IsChecked;
            }
            catch { }
        }

        private void CB_Change(object sender, RoutedEventArgs e)
        {
            try
            {
                CheckBox checkBox = sender as CheckBox;

                if (checkBox.IsChecked == true)
                {
                    threadNames.Add(checkBox.Name);
                }
                else
                {
                    threadNames.Remove(
                        threadNames.FirstOrDefault(s => s == checkBox.Name)
                        );
                }
            }
            catch { }
        }

        private void CBB_Champion_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                LST_Champion.Visibility = Visibility.Hidden;
                ComboBox comboBox = sender as ComboBox;
                championId = ((KeyValuePair<string, int>)comboBox.SelectedItem).Value;
            }
            catch { }
        }

        private void CBB_Champion_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                ComboBox comboBox = sender as ComboBox;
                string textToSearch = comboBox.Text.ToLower();
                comboBox.IsDropDownOpen = false;
                if (string.IsNullOrEmpty(textToSearch))
                {
                    LST_Champion.Visibility = Visibility.Hidden;
                    return;
                }
                string[] result = comboBox.Items.Cast<KeyValuePair<string, int>>().
                    Where(s => s.Key.Contains(textToSearch) && ((KeyValuePair<string, int>)comboBox.SelectedItem).Key != s.Key).
                    OrderBy(s => s.Key.Length).
                    Select(s => s.Key).ToArray();
                if (result.Length == 0)
                {
                    var championList = leagueClient.Get_Owned_Champions_Dict();
                    result = (from i in championList.Keys
                              where i.ToLower().Contains(textToSearch)
                              orderby zh_ch.en_to_ch(i).Length
                              select zh_ch.en_to_ch(i)).ToArray();
                }

                if (result.Length == 0) return;
                LST_Champion.ItemsSource = result;
                LST_Champion.Visibility = Visibility.Visible;
            }
            catch { }
        }

        private void CBB_Lane_TextChanged(object sender, TextChangedEventArgs e)
        {
            ComboBox comboBox = sender as ComboBox;
            lane = comboBox.Text;
        }

        private void Initialize()
        {
            try
            {
                if (Check_Game_Launch())
                {
                    if (!Dispatcher.CheckAccess())
                    {
                        Dispatcher.Invoke(DispatcherPriority.Send, new Initailize_Delegate(Initialize));
                    }
                    else
                    {
                        Btn_Run.IsEnabled = true;
                        Grid_CB.IsEnabled = true;
                        lockfile = TB_Path.Text + "\\lockfile";
                        TB_Path.IsEnabled = false;
                        Btn_Confirm.IsEnabled = false;

                        threadNames = new List<string>();
                        leagueClient = new LeagueClient(lockfile);
                        match = new LoLHelper_rework_wpf_.Implements.Match(leagueClient);
                        champSelect = new ChampSelect(leagueClient);
                        rune = new Rune(leagueClient);
                        summoner = new Summoner(leagueClient);

                        Create_ThreadPool();
                        Create_Lane_ComboBox_Items();
                        Create_Champion_ComboBox_Items();
                        Use_Remember_Setting();
                        if (this.WindowState == WindowState.Minimized)
                        {
                            PopUp(this, null);
                        }
                    }
                }
            }
            catch { }
        } 

        private void LST_Champion_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                ListBox listBox = sender as ListBox;
                if (listBox.SelectedItem == null) return;
                var item = CBB_Champion.Items.Cast<KeyValuePair<string, int>>().
                    Where(s => s.Key == listBox.SelectedItem.ToString()).
                    Select(s => s).FirstOrDefault();
                if (item.Key != null)
                {
                    CBB_Champion.SelectedItem = item;
                    CBB_Champion.Text = item.Key;
                }
                listBox.SelectedItem = null;
                listBox.Visibility = Visibility.Hidden;
            }
            catch { }
        }

        private void Monitor()
        {
            Thread thread = new Thread(() =>
            {
                while (true)
                {
                    if (Check_Game_Launch() == false)
                    {
                        if (isRunning)
                        {
                            Btn_Click(Btn_Run);
                            foreach (var t in threadPool)
                            {
                                t.Value.Abort();
                            }
                            Reset();
                            isRunning = false;
                        }
                        isInitializing = false;
                    }
                    else
                    {
                        if (isInitializing == false)
                        {
                            Initialize();
                            isInitializing = true;
                        }
                    }
                    Thread.Sleep(1000);
                }
            });
            thread.IsBackground = true;
            thread.Start();
        }

        protected override void OnStateChanged(EventArgs e)
        {
            if (WindowState == System.Windows.WindowState.Minimized)
            {
                if (CB_Minimize.IsChecked == true || Check_Game_Launch() == false)
                {
                    ni.Visible = true;
                    this.Hide();
                }
            }
            base.OnStateChanged(e);
        }

        private void PopUp(object sender, EventArgs args)
        {
            Thread.Sleep(300);
            ni.Visible = false;
            this.Show();
            this.Activate();
            this.WindowState = WindowState.Normal;
        }

        private void Remember_Setting()
        {
            try
            {
                Properties.Settings.Default.CB_Accept = (bool)CB_Accept.IsChecked;
                Properties.Settings.Default.CB_ChangeRune = (bool)CB_ChangeRune.IsChecked;
                Properties.Settings.Default.CB_ChangeSpell = (bool)CB_ChangeSpell.IsChecked;
                Properties.Settings.Default.CB_Lock = (bool)CB_Lock.IsChecked;
                Properties.Settings.Default.CB_Minimize = (bool)CB_Minimize.IsChecked;
                Properties.Settings.Default.CB_PickChamp = (bool)CB_PickChamp.IsChecked;
                Properties.Settings.Default.CB_PickLane = (bool)CB_PickLane.IsChecked;
                Properties.Settings.Default.CB_Queue = (bool)CB_Queue.IsChecked;
                Properties.Settings.Default.CB_Startup = (bool)CB_Startup.IsChecked;

                Properties.Settings.Default.CBB_Champion = CBB_Champion.Text;
                Properties.Settings.Default.CBB_Lane = CBB_Lane.Text;

                Properties.Settings.Default.TB_Path = TB_Path.Text;
                Properties.Settings.Default.TB_Times = TB_Times.Text;

                Properties.Settings.Default.Save();
            }
            catch { }
        }

        private void Reset()
        {
            if (!Dispatcher.CheckAccess())
            {
                Dispatcher.Invoke(DispatcherPriority.Send, new Reset_Delegate(Reset));
            }
            else
            {
                try
                {
                    CB_Accept.IsChecked = false;
                    CB_ChangeRune.IsChecked = false;
                    CB_ChangeSpell.IsChecked = false;
                    CB_Lock.IsChecked = false;
                    CB_Minimize.IsChecked = false;
                    CB_PickChamp.IsChecked = false;
                    CB_PickLane.IsChecked = false;
                    CB_Queue.IsChecked = false;
                    CB_Startup.IsChecked = false;
                    CBB_Champion.Text = "";
                    CBB_Lane.Text = "";
                    TB_Times.Text = "";
                }
                catch { }
            }    
        }

        private void TB_Times_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                TextBox textBox = sender as TextBox;
                times = Convert.ToInt32(textBox.Text);
            }
            catch { }
        }

        private void TB_Path_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                TextBox textBox = sender as TextBox;
                if (textBox.Text.Contains("LeagueClient") == false)
                {
                    textBox.Text += @"\LeagueClient";
                }
                lockfile = textBox.Text + "\\lockfile";
            }
            catch { }
        }

        private void TB_Times_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }

        private void Use_Remember_Setting()
        {
            try
            {
                CB_Accept.IsChecked = Properties.Settings.Default.CB_Accept;
                CB_ChangeRune.IsChecked = Properties.Settings.Default.CB_ChangeRune;
                CB_ChangeSpell.IsChecked = Properties.Settings.Default.CB_ChangeSpell;
                CB_Lock.IsChecked = Properties.Settings.Default.CB_Lock;
                CB_Minimize.IsChecked = Properties.Settings.Default.CB_Minimize;
                CB_PickChamp.IsChecked = Properties.Settings.Default.CB_PickChamp;
                CB_PickLane.IsChecked = Properties.Settings.Default.CB_PickLane;
                CB_Queue.IsChecked = Properties.Settings.Default.CB_Queue;
                CB_Startup.IsChecked = Properties.Settings.Default.CB_Startup;

                CBB_Champion.SelectedItem = CBB_Champion.Items.Cast<KeyValuePair<string, int>>().
                    Where(s => s.Key == Properties.Settings.Default.CBB_Champion).
                    Select(s => s).FirstOrDefault();

                CBB_Lane.Text = Properties.Settings.Default.CBB_Lane;

                TB_Times.Text = Properties.Settings.Default.TB_Times;
            }
            catch { }
        }      
    }
}
