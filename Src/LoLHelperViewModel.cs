using Stylet;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using LoLHelper.Src.Service;
using System.Threading;
using LoLHelper.Src.Enums;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Windows;
using LoLHelper.Src.Commands;
using System.Runtime.CompilerServices;

namespace LoLHelper.Src
{
    public class LoLHelperViewModel : Screen
    {
        public bool IsRunning { get; set; }
        public bool AutoQueue { get; set; }
        public bool AutoAccept { get; set; }
        public bool AutoPickLane { get; set; }
        public bool AutoPickChampion { get; set; }
        public bool AutoLockChampion { get; set; }
        public bool AutoChangeRune { get; set; }
        public bool ChangeRuneForARAM { get; set; }
        public bool IsMinimizie { get; set; }
        public bool IsInitialized { get; set; }
        public bool IsClosedGame { get; set; } = false;
        public bool IsShowChampionPopup { get => PopupChampionList != null && PopupChampionList.Count > 0; }

        public string SelectedChampion { get; set; }
        public string SelectedLane { get; set; }
        public string LockFile { get => @$"{LeagueClientPath}\lockfile"; }

        public int PickLaneTimes { get; set; }
        public int? SelectedChampionId
        {
            get
            {
                if (championNameToIdDict.ContainsKey(SelectedChampion))
                    return championNameToIdDict[SelectedChampion];
                else
                    return null;
            }
        }

        public ObservableCollection<string> ChampionList { get; set; }
        public ObservableCollection<string> PopupChampionList { get; set; }
        public ObservableCollection<string> LaneList { get; set; }
        public ObservableCollection<int> PickLaneTimesList { get; set; }

        private string _leagueClientPath;
        public string LeagueClientPath
        {
            get => _leagueClientPath;
            set
            {
                if (value.Contains(@"\LeagueClient") == false && value.Contains(@"\League of Legends") == false)
                {
                    _leagueClientPath = $"{value}\\LeagueClient";
                }
                else
                {
                    _leagueClientPath = value;
                }
            }
        }

        private string _searchChampionText;
        public string SearchChampionText
        {
            get => _searchChampionText;
            set
            {
                if (value == null) return;

                _searchChampionText = value;

                if (ChampionList.Contains(value) == false)
                {
                    if (value != "")
                    {
                        PopupChampionList = new ObservableCollection<string>(ChampionList.Where(i => i.Contains(value)).Select(i => i).ToList());

                        if (PopupChampionList.Count == 0)
                        {
                            PopupChampionList = new ObservableCollection<string>(
                                leagueClient.ChampionNameChToEnDict.Where(i => i.Value.Contains(value.ToLower())).Select(i => i.Key).ToList());
                        }
                    }
                    else
                    {
                        PopupChampionList = null;
                    }
                }
                else
                {
                    PopupChampionList = null;
                    SelectedChampion = SearchChampionText;
                }
            }
        }

        private Dictionary<string, int> championNameToIdDict;
        private LeagueClient leagueClient;
        private ChampSelect champSelect;
        private Chat chat;
        private Match match;
        private Rune rune;
        private Summoner summoner;
        private Gameflow gameflow;

        public LoLHelperViewModel()
        {
            ChampionList = new();
            LaneList = new();
            PickLaneTimesList = new();
            championNameToIdDict = new();
            gameflow = Gameflow.None;
            LeagueClientPath = Properties.Settings.Default.LeagueClientPath;

            Task.Factory.StartNew(ProcessGameMonitor, TaskCreationOptions.LongRunning);
            Task.Factory.StartNew(ProcessAutoQueue, TaskCreationOptions.LongRunning);
            Task.Factory.StartNew(ProcessAutoAccept, TaskCreationOptions.LongRunning);
            Task.Factory.StartNew(ProcessAutoPickLane, TaskCreationOptions.LongRunning);
            Task.Factory.StartNew(ProcessAutoPickChampion, TaskCreationOptions.LongRunning);
            Task.Factory.StartNew(ProcessAutoChangeRune, TaskCreationOptions.LongRunning);
            Task.Factory.StartNew(ProcessGetGameflow, TaskCreationOptions.LongRunning);
        }

        private void Initial()
        {
            WriteLog("Start");

            leagueClient = new(LockFile);
            champSelect = new(leagueClient);
            chat = new(leagueClient);
            match = new(leagueClient);
            rune = new(leagueClient);
            summoner = new(leagueClient);

            SpinWait.SpinUntil(() =>
            {
                if ((championNameToIdDict = leagueClient.GetOwnedChampionsDict()) == null)
                {
                    Thread.Sleep(500);
                    return false;
                }

                return championNameToIdDict.Count > 0;
            });

            Application.Current.Dispatcher.Invoke(() =>
            {
                for (int i = 1; i <= 10; i++)
                {
                    PickLaneTimesList.Add(i);
                }

                PickLaneTimesList.Add(20);
                PickLaneTimesList.Add(30);
                PickLaneTimesList.Add(50);
                PickLaneTimesList.Add(100);

                foreach (var championName in championNameToIdDict.Keys)
                {
                    ChampionList.Add(championName);
                }

                LaneList.Add("Top");
                LaneList.Add("JG");
                LaneList.Add("Mid");
                LaneList.Add("AD");
                LaneList.Add("Sup");
            });

            PickLaneTimes = 1;
            SelectedLane = "Mid";
            SelectedChampion = "安妮";

            WriteLog("End");

            UseSetting();
        }

        // 自動列隊
        private void ProcessAutoQueue()
        {
            try
            {
                while (true)
                {
                    SpinWait.SpinUntil(() => (IsRunning && AutoQueue && gameflow == Gameflow.Lobby && match.CheckCanQueueing()));

                    match.StartQueueing();
                    Thread.Sleep(10000);
                }
            }
            catch (Exception err)
            {
                WriteLog($"{err}", true);
            }
        }

        // 自動接受
        private void ProcessAutoAccept()
        {
            try
            {
                bool canAccept = true;

                while (true)
                {
                    SpinWait.SpinUntil(() =>
                    {
                        if (!IsRunning || !AutoAccept)
                            return false;

                        if (gameflow != Gameflow.ReadyCheck)
                        {
                            canAccept = true;
                            return false;
                        }

                        return canAccept;
                    });

                    canAccept = false;

                    match.AcceptMatchMaking();
                }
            }
            catch (Exception err)
            {
                WriteLog($"{err}", true);
            }
        }

        // 自動喊路
        private void ProcessAutoPickLane()
        {
            try
            {
                string preSelectedLane = null;
                int prePickLaneTimes = -1;

                while (true)
                {
                    SpinWait.SpinUntil(() =>
                    {
                        if (!IsRunning || !AutoPickLane)
                            return false;

                        if (gameflow != Gameflow.ChampSelect)
                        {
                            preSelectedLane = null;
                            prePickLaneTimes = -1;
                            return false;
                        }

                        return (preSelectedLane != SelectedLane || prePickLaneTimes != PickLaneTimes);
                    });

                    preSelectedLane = SelectedLane;
                    prePickLaneTimes = PickLaneTimes;

                    champSelect.PickLane(SelectedLane, PickLaneTimes);
                }
            }
            catch (Exception err)
            {
                WriteLog($"{err}", true);
            }
        }

        // 自動選角
        private void ProcessAutoPickChampion()
        {
            try
            {
                int? preChampionId = null;

                while (true)
                {
                    SpinWait.SpinUntil(() =>
                    {
                        if (!IsRunning || !AutoPickChampion || SelectedChampionId == null)
                            return false;

                        if (gameflow != Gameflow.ChampSelect)
                        {
                            preChampionId = null;
                            return false;
                        }

                        return preChampionId != SelectedChampionId;
                    });

                    preChampionId = SelectedChampionId;

                    champSelect.PickChampion((int)SelectedChampionId, AutoLockChampion);
                }
            }
            catch (Exception err)
            {
                WriteLog($"{err}", true);
            }
        }

        // 更換符文
        private void ProcessAutoChangeRune()
        {
            try
            {
                int? preChampionId = null;
                Mode preMode = Mode.None;

                while (true)
                {
                    SpinWait.SpinUntil(() =>
                    {
                        if (!IsRunning || !AutoChangeRune)
                            return false;

                        return gameflow == Gameflow.ChampSelect;
                    });

                    int? championId = champSelect.GetMyPickChampionId();
                    string position = champSelect.GetMyPosition();
                    Mode mode = ChangeRuneForARAM ? Mode.Aram : Mode.Normal;
                    string champion;

                    if ((championId != null && championId != preChampionId) || mode != preMode)
                    {
                        champion = championNameToIdDict.FirstOrDefault(x => x.Value == championId).Key;

                        if (leagueClient.ChampionNameChToEnDict.Values.Contains(champion) == false)
                        {
                            champion = leagueClient.ChampionNameChToEnDict[champion];
                        }

                        if (champion != null)
                        {
                            rune.SetRune(champion, position, mode);

                            preChampionId = championId;
                            preMode = mode;
                        }
                    }

                    Thread.Sleep(1000);
                }
            }
            catch (Exception err)
            {
                WriteLog($"{err}", true);
            }
        }

        private void ProcessGetGameflow()
        {
            while (true)
            {
                SpinWait.SpinUntil(() => IsRunning);

                gameflow = leagueClient.GetGameflow();

                Thread.Sleep(500);
            }
        }

        private void ProcessGameMonitor()
        {
            try
            {
                while (true)
                {
                    SpinWait.SpinUntil(() =>
                    {
                        Thread.Sleep(200);

                        if (CheckGameLaunch() == false)
                        {
                            // Close game
                            if (IsInitialized)
                            {
                                Reset();
                                IsClosedGame = true;

                                WriteLog("Game close");
                            }

                            IsInitialized = false;

                            return false;
                        }

                        return true;
                    });

                    if (IsInitialized == false)
                    {
                        WriteLog("Game start");

                        Properties.Settings.Default.LeagueClientPath = LeagueClientPath;
                        Properties.Settings.Default.Save();

                        Initial();
                        IsInitialized = true;
                    }
                }
            }
            catch (Exception err)
            {
                WriteLog($"{err}", true);
            }
        }

        private void Reset()
        {
            IsRunning = false;
            AutoQueue = false;
            AutoAccept = false;
            AutoPickLane = false;
            AutoPickChampion = false;
            AutoLockChampion = false;
            AutoChangeRune = false;
            ChangeRuneForARAM = false;
            IsMinimizie = false;
            SelectedChampion = "";
            SelectedLane = "";

            Application.Current.Dispatcher.Invoke(() =>
            {
                ChampionList.Clear();
                LaneList.Clear();
                PickLaneTimesList.Clear();
                championNameToIdDict.Clear();
            });
        }

        private bool CheckGameLaunch()
        {
            return File.Exists(LockFile);
        }

        private void SaveSetting()
        {
            Properties.Settings.Default.AutoAccept = AutoAccept;
            Properties.Settings.Default.AutoChangeRune = AutoChangeRune;
            Properties.Settings.Default.AutoLockChampion = AutoLockChampion;
            Properties.Settings.Default.AutoPickChampion = AutoPickChampion;
            Properties.Settings.Default.AutoPickLane = AutoPickLane;
            Properties.Settings.Default.AutoQueue = AutoQueue;
            Properties.Settings.Default.IsMinimizie = IsMinimizie;
            Properties.Settings.Default.PickLaneTimes = PickLaneTimes;
            Properties.Settings.Default.SelectedChampion = SelectedChampion;
            Properties.Settings.Default.SelectedLane = SelectedLane;
            Properties.Settings.Default.ChangeRuneForARAM = ChangeRuneForARAM;

            Properties.Settings.Default.Save();
        }

        private void UseSetting()
        {
            AutoAccept = Properties.Settings.Default.AutoAccept;
            AutoChangeRune = Properties.Settings.Default.AutoChangeRune;
            AutoLockChampion = Properties.Settings.Default.AutoLockChampion;
            AutoPickChampion = Properties.Settings.Default.AutoPickChampion;
            AutoPickLane = Properties.Settings.Default.AutoPickLane;
            AutoQueue = Properties.Settings.Default.AutoQueue;
            IsMinimizie = Properties.Settings.Default.IsMinimizie;
            PickLaneTimes = Properties.Settings.Default.PickLaneTimes;
            SelectedChampion = Properties.Settings.Default.SelectedChampion;
            SelectedLane = Properties.Settings.Default.SelectedLane;
            ChangeRuneForARAM = Properties.Settings.Default.ChangeRuneForARAM;
        }

        public void OnRunButtonClick()
        {
            SaveSetting();
        }

        private void WriteLog(string msg, bool isException = false, [CallerMemberName]string callerName = null)
        {
            LogManager.WriteLog($"[LoLHelperViewModel]{callerName}() {msg}", isException);
        }
    }
}
