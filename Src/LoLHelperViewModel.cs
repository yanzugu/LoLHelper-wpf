﻿using Stylet;
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
        public bool IsMinimizie { get; set; }
        public bool IsInitialized { get; set; }

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
        public ObservableCollection<string> LaneList { get; set; }
        public ObservableCollection<int> PickLaneTimesList { get; set; }

        private string _leagueClientPath;
        public string LeagueClientPath
        {
            get => _leagueClientPath;
            set
            {
                if (value.Contains(@"\LeagueClient") == false)
                {
                    _leagueClientPath = $"{value}\\LeagueClient";
                }
                else
                {
                    _leagueClientPath = value;
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
            leagueClient = new(LockFile);
            champSelect = new(leagueClient);
            chat = new(leagueClient);
            match = new(leagueClient);
            rune = new(leagueClient);
            summoner = new(leagueClient);

            championNameToIdDict = leagueClient.GetOwnedChampionsDict();

            Application.Current.Dispatcher.Invoke(() =>
            {
                for (int i = 1; i <= 10; i++)
                {
                    PickLaneTimesList.Add(i);
                }

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
        }

        private void ProcessAutoQueue()
        {
            try
            {
                while (true)
                {
                    SpinWait.SpinUntil(() => (IsRunning && AutoQueue && gameflow == Gameflow.Lobby && match.CheckCanQueueing()));

                    match.StartQueueing();

                    Thread.Sleep(15000);
                }
            }
            catch (Exception err)
            {
                WriteLog($"{err}", true);
            }
        }

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

        private void ProcessAutoPickLane()
        {
            //try
            {
                string preSelectedLane = null;

                while (true)
                {
                    SpinWait.SpinUntil(() =>
                    {
                        if (!IsRunning || !AutoPickLane)
                            return false;

                        if (gameflow != Gameflow.ChampSelect)
                        {
                            preSelectedLane = null;
                            return false;
                        }

                        return preSelectedLane != SelectedLane;
                    });

                    preSelectedLane = SelectedLane;

                    champSelect.PickLane(SelectedLane, PickLaneTimes);
                }
            }
            //catch (Exception err)
            {
                //WriteLog($"{err}", true);
            }
        }

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

        private void ProcessAutoChangeRune()
        {
            try
            {
                int? preChampionId = null;

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
                    string champion;

                    if (championId != null && championId != preChampionId)
                    {
                        champion = leagueClient.GetOwnedChampionsDict().FirstOrDefault(x => x.Value == championId).Key;

                        if (champion != null)
                        {
                            rune.SetRune(champion, position);
                            preChampionId = championId;
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
                        if (CheckGameLaunch() == false)
                        {
                            IsInitialized = false;
                            return false;
                        }

                        return true;
                    });

                    if (IsInitialized == false)
                    {
                        IsInitialized = true;

                        Properties.Settings.Default.LeagueClientPath = LeagueClientPath;
                        Properties.Settings.Default.Save();

                        Initial();
                        UseSetting();
                    }

                    Thread.Sleep(2000);
                }
            }
            catch
            {

            }
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
        }

        public void OnRunButtonClick()
        {
            SaveSetting();
        }

        private void WriteLog(string msg, bool isException = false)
        {

        }
    }
}