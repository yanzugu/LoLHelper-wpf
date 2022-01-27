using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using LoLHelper.Src.Enums;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace LoLHelper.Src.Service
{
    internal class Match
    {
        private readonly LeagueClient leagueClient;

        public Match(LeagueClient _leagueClient)
        {
            leagueClient = _leagueClient;
        }

        public void AcceptMatchMaking()
        {
            var url = leagueClient.url_prefix + "/lol-matchmaking/v1/ready-check/accept";
            var req = leagueClient.Request(url, "POST");

            try
            {
                using (WebResponse response = req.GetResponse()) { }
                WriteLog("");
            }
            catch (Exception err)
            {
                WriteLog($"{err}", true);
            }
        }

        public JArray GetMatchInfoByGameId(int gameId)
        {
            var url = leagueClient.url_prefix + $"/lol-match-history/v1/games/{gameId}";
            var req = leagueClient.Request(url, "GET");

            try
            {
                using (WebResponse response = req.GetResponse())
                {
                    var encoding = UTF8Encoding.UTF8;

                    using (var reader = new StreamReader(response.GetResponseStream(), encoding))
                    {
                        string text = reader.ReadToEnd();
                        JArray jArray = JArray.Parse(text);

                        return jArray;
                    }
                }
            }
            catch (Exception err)
            {
                WriteLog($"{err}", true);
            }

            return null;
        }

        public List<int> GetGameIdListByAccountId(string accid, int beginIdx = 0, int endIdx = 5)
        {
            var url = $"https://acs-garena.leagueoflegends.com/v1/stats/player_history/TW/{accid}?begIndex={beginIdx}&endIndex={endIdx}";
            var req = leagueClient.Request(url, "GET");
            List<int> list = new List<int>();

            try
            {
                using (WebResponse response = req.GetResponse())
                {
                    var encoding = UTF8Encoding.UTF8;
                    using (var reader = new StreamReader(response.GetResponseStream(), encoding))
                    {
                        string text = reader.ReadToEnd();
                        JArray jArray = JArray.Parse(text);

                        foreach (var game in jArray["games"]["games"])
                        {
                            list.Add(Convert.ToInt32(game["gameId"]));
                        }
                    }
                }

                return list;
            }
            catch (Exception err)
            {
                WriteLog($"{err}", true);
            }

            return null;
        }

        public void StartQueueing()
        {
            var url = leagueClient.url_prefix + "/lol-lobby/v2/lobby/matchmaking/search";
            var req = leagueClient.Request(url, "POST");

            try
            {
                using (WebResponse response = req.GetResponse()) { }
                WriteLog("");
            }
            catch (Exception err)
            {
                WriteLog($"{err}", true);
            }
        }

        public bool CheckCanQueueing()
        {
            var url = leagueClient.url_prefix + "/lol-lobby/v2/lobby";
            var req = leagueClient.Request(url, "GET");

            try
            {
                using (WebResponse response = req.GetResponse())
                {
                    var encoding = UTF8Encoding.UTF8;

                    using (var reader = new StreamReader(response.GetResponseStream(), encoding))
                    {
                        string text = reader.ReadToEnd();
                        JObject jObject = JObject.Parse(text);

                        if (!Convert.ToBoolean(jObject["localMember"]["isLeader"]))
                            return false;

                        foreach (JObject members in jObject["members"])
                        {
                            if (!Convert.ToBoolean(members["ready"]))
                                return false;
                        }
                    }
                }

                return true;
            }
            catch (Exception err)
            {
                WriteLog($"{err}", true);
            }

            return false;
        }

        private void WriteLog(string msg, bool isException = false, [CallerMemberName] string callerName = null)
        {
            LogManager.WriteLog($"[Match]{callerName}() {msg}", isException);
        }
    }
}
