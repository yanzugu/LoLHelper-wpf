using Stylet;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using LoLHelper.Src.Service;
using System.Threading;
using LoLHelper.Src.Enums;
using System.Collections.Generic;

namespace LoLHelper.Src
{
    public class LoLHelperViewModel : Screen
    {
        public bool IsRunning { get; set; }
        public bool IsFuntionCheckboxEnable => !IsRunning;
        public bool AutoQueue { get; set; }
        public bool AutoAccept { get; set; }
        public bool AutoPickLane { get; set; }
        public bool AutoPickChampion { get; set; }
        public bool AutoLockChampion { get; set; }
        public bool AutoChangeRune { get; set; }
        public bool IsMinimizie { get; set; }

        public string SelectedChampion { get; set; }
        public string SelectedLane { get; set; }
        public string LeagueClientPath { get; set; }

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

            for (int i = 1; i <= 10; i++)
            {
                PickLaneTimesList.Add(i);
            }

            leagueClient = new($@"C:\Garena\Games\32775\LeagueClient\lockfile");
            champSelect = new(leagueClient);
            chat = new(leagueClient);
            match = new(leagueClient);
            rune = new(leagueClient);
            summoner = new(leagueClient);

            championNameToIdDict = leagueClient.GetOwnedChampionsDict();

            foreach (var championName in championNameToIdDict.Keys)
            {
                ChampionList.Add(championName);
            }

            LaneList.Add("Top");
            LaneList.Add("JG");
            LaneList.Add("Mid");
            LaneList.Add("AD");
            LaneList.Add("Sup");

            Task.Factory.StartNew(ProcessAutoQueue, TaskCreationOptions.LongRunning);
            Task.Factory.StartNew(ProcessAutoAccept, TaskCreationOptions.LongRunning);
            Task.Factory.StartNew(ProcessAutoPickLane, TaskCreationOptions.LongRunning);
            Task.Factory.StartNew(ProcessAutoPickChampion, TaskCreationOptions.LongRunning);
            //Task.Factory.StartNew(ProcessAutoChangeRune, TaskCreationOptions.LongRunning);
            Task.Factory.StartNew(ProcessGetGameflow, TaskCreationOptions.LongRunning);
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

        private void ProcessGetGameflow()
        {
            while (true)
            {
                SpinWait.SpinUntil(() => IsRunning);

                gameflow = leagueClient.GetGameflow();

                Thread.Sleep(500);
            }
        }

        private void WriteLog(string msg, bool isException = false)
        {

        }
    }
}
