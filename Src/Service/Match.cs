using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using LoLHelper.Src.Enums;
using Newtonsoft.Json;

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
            }
            catch
            {
            }
        }

        public dynamic GetMatchInfoByGameId(int gameId)
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
                        dynamic json = JsonConvert.DeserializeObject<dynamic>(text);
                        return json;
                    }
                }
            }
            catch
            {
                return null;
            }
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
                        dynamic json = JsonConvert.DeserializeObject<dynamic>(text);

                        foreach (var game in json["games"]["games"])
                        {
                            list.Add(game["gameId"]);
                        }
                    }
                }
                return list;
            }
            catch
            {
                return null;
            }
        }

        public void StartQueueing()
        {
            var url = leagueClient.url_prefix + "/lol-lobby/v2/lobby/matchmaking/search";
            var req = leagueClient.Request(url, "POST");

            try
            {
                using (WebResponse response = req.GetResponse()) { }
            }
            catch
            {
            }
        }

        public bool CheckCanQueueing()
        {
            var url = leagueClient.url_prefix + "/lol-lobby/v2/lobby";
            var req = leagueClient.Request(url, "GET");
            try
            {
                if (leagueClient.GetGameflow() != Gameflow.Lobby) return false;

                using (WebResponse response = req.GetResponse())
                {
                    var encoding = UTF8Encoding.UTF8;
                    using (var reader = new StreamReader(response.GetResponseStream(), encoding))
                    {
                        string text = reader.ReadToEnd();
                        dynamic json = JsonConvert.DeserializeObject<dynamic>(text);
                        if (!json["localMember"]["isLeader"])
                            return false;

                        foreach (var elem in json["members"])
                        {
                            if (!elem["ready"])
                                return false;
                        }
                    }
                }
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
