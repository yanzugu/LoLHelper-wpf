using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Threading;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows.Controls.Primitives;

namespace LoLHelper_rework_wpf_
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        bool isRunning = false;
        List<KeyValuePair<string, Thread>> threads;
        Dictionary<string, Thread> threadPool;
        Dictionary<string, ManualResetEvent> eventPool;
        LeagueClient leagueClient;
        Zh_Ch zh_ch;
        string lane, champion;
        int times, championId;
        bool isLock;
        int spell1, spell2;
        string lockfile;

        public MainWindow()
        {
            InitializeComponent();
            threads = new List<KeyValuePair<string, Thread>>();
            zh_ch = new Zh_Ch();
            TB_Path.Text = Properties.Settings.Default.TB_Path;
            Initialize();
            Monitor();
        }

        private bool Check_Game_Launch()
        {
            return File.Exists(lockfile);
        }

        private void Initialize()
        {
            try
            {
                if (Check_Game_Launch())
                {
                    lockfile = TB_Path.Text + "\\lockfile";
                    TB_Path.IsEnabled = false;
                    Btn_Confirm.IsEnabled = false;
                    leagueClient = new LeagueClient(lockfile);
                    Create_ThreadPool();
                    Create_Lane_ComboBox_Items();
                    Create_Champion_ComboBox_Items();
                    Use_Remember_Setting();
                }
            }
            catch { }
        }

        private void Btn_Run_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                isRunning = !isRunning;
                Button button = sender as Button;

                if (isRunning)
                {
                    foreach (KeyValuePair<string, Thread> pair in threads)
                    {
                        eventPool[pair.Key].Set();
                    }
                    Grid_CB.IsEnabled = false;
                    button.Content = "結束";
                }
                else
                {
                    foreach (KeyValuePair<string, Thread> pair in threads)
                    {
                        eventPool[pair.Key].Reset();
                    }
                    Grid_CB.IsEnabled = true;
                    button.Content = "開始";
                }
                Remember_Setting();
            }
            catch { }
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

                CBB_Lane.SelectedItem = CBB_Lane.Items.Cast<object>().
                    Where(s => s.ToString() == Properties.Settings.Default.CBB_Lane).
                    Select(s => s).FirstOrDefault();

                TB_Times.Text = Properties.Settings.Default.TB_Times;
            }
            catch { }
        }

        private void Create_Champion_ComboBox_Items()
        {
            try
            {
                CBB_Lane.Items.Clear();
                CBB_Champion.DisplayMemberPath = "Key";
                CBB_Champion.SelectedValuePath = "Value";
                CBB_Champion.ItemsSource = leagueClient.Get_Owned_Champions_Dict().
                    Select(s => s = new KeyValuePair<string, int>(zh_ch.en_to_ch(s.Key), s.Value));
                CBB_Champion.SelectedItem = CBB_Champion.Items[0];
            }
            catch { }
        }

        private void CBB_Champion_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                LST_Champion.Visibility = Visibility.Hidden;
                ComboBox comboBox = sender as ComboBox;
                champion = leagueClient.Get_Owned_Champions_Dict().
                    Where(s => s.Value == ((KeyValuePair<string, int>)comboBox.SelectedItem).Value).
                    Select(s => s.Key).FirstOrDefault();
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

        private void TB_Times_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                TextBox textBox = sender as TextBox;
                times = Convert.ToInt32(textBox.Text);
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
                    threads.Add(new KeyValuePair<string, Thread>(checkBox.Name, threadPool[checkBox.Name]));
                }
                else
                {
                    threads.Remove(
                        threads.FirstOrDefault(s => s.Key == checkBox.Name)
                        );
                }
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

        private void Btn_Confirm_Click(object sender, RoutedEventArgs e)
        {
            Initialize();
        }

        private void CBB_Lane_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                lane = ((ComboBox)sender).SelectedItem.ToString();
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
                }
                listBox.Visibility = Visibility.Hidden;
            }
            catch { }
        }

        private void TB_Times_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
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
                CBB_Lane.SelectedItem = CBB_Lane.Items[0];
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
                    leagueClient.Find_Match();
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
                        if (leagueClient.Get_Gameflow() != "\"ChampSelect\"")
                        {
                            preLane = null;
                            continue;
                        }                         
                        if (lane != preLane)
                        {
                            leagueClient.Pick_Selected_Lane(lane, times);
                            preLane = lane;
                        }
                        Thread.Sleep(1000);
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
                        if (leagueClient.Get_Gameflow() != "\"ChampSelect\"")
                        {
                            preChampionId = null;
                            continue;
                        }
                        if (championId != preChampionId)
                        {
                            leagueClient.Pick_Champion(championId, isLock);
                            preChampionId = championId;
                        }
                        Thread.Sleep(1000);
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
                        if (leagueClient.Get_Gameflow() != "\"ChampSelect\"") continue;
                        leagueClient.Pick_Spell(spell1, spell2);
                        Thread.Sleep(5000);
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
                        if (champion != preChampion)
                        {
                            leagueClient.Set_Rune(champion, lane);
                            preChampion = champion;
                        }
                    }
                });
                threadPool.Add("CB_ChangeRune", thread);
                thread.IsBackground = true;
                thread.Start();
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
                            Btn_Run.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));
                            isRunning = false;
                            Btn_Run.IsEnabled = false;
                        }
                    }
                    else
                    {
                        Initialize();
                        Btn_Run.IsEnabled = true;
                    }
                    Thread.Sleep(30);
                }
            });
        }
    }
}
