using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using LoLHelper_rework_wpf_.Interfaces;

namespace LoLHelper_rework_wpf_.Implements
{
    class Match : IMatch
    {
        private readonly LeagueClient _leagueClient;

        public Match(LeagueClient leagueClient)
        {
            _leagueClient = leagueClient;
        }

        public void Accept_MatchMaking()
        {
            var url = _leagueClient.url_prefix + "/lol-matchmaking/v1/ready-check/accept";
            var req = _leagueClient.Request(url, "POST");
            try
            {
                using (WebResponse response = req.GetResponse()) { }
            }
            catch
            {
            }
        }

        public dynamic Get_Match_Info_By_GameId(int gameId)
        {
            var url = _leagueClient.url_prefix + $"/lol-match-history/v1/games/{gameId}";
            var req = _leagueClient.Request(url, "GET");
            try
            {
                using (WebResponse response = req.GetResponse())
                {
                    var encoding = UTF8Encoding.UTF8;
                    using (var reader = new StreamReader(response.GetResponseStream(), encoding))
                    {
                        string text = reader.ReadToEnd();
                        dynamic json = new JavaScriptSerializer().Deserialize<dynamic>(text);
                        return json;
                    }
                }
            }
            catch
            {
                return null;
            }
        }

        public List<int> Get_GameId_List_By_AccountId(string accid, int beginIdx = 0, int endIdx = 5)
        {
            var url = $"https://acs-garena.leagueoflegends.com/v1/stats/player_history/TW/{accid}?begIndex={beginIdx}&endIndex={endIdx}";
            var req = _leagueClient.Request(url, "GET");
            List<int> list = new List<int>();
            try
            {
                using (WebResponse response = req.GetResponse())
                {
                    var encoding = UTF8Encoding.UTF8;
                    using (var reader = new StreamReader(response.GetResponseStream(), encoding))
                    {
                        string text = reader.ReadToEnd();
                        dynamic json = new JavaScriptSerializer().Deserialize<dynamic>(text);

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

        public void Start_Queueing()
        {
            var url = _leagueClient.url_prefix + "/lol-lobby/v2/lobby/matchmaking/search";
            var req = _leagueClient.Request(url, "POST");
            try
            {
                using (WebResponse response = req.GetResponse()) { }
            }
            catch
            {
            }
        }

        public bool Check_Can_Queueing()
        {
            var url = _leagueClient.url_prefix + "/lol-lobby/v2/lobby";
            var req = _leagueClient.Request(url, "GET");
            try
            {
                if (_leagueClient.Get_Gameflow() != "\"Lobby\"") return false;
                using (WebResponse response = req.GetResponse())
                {
                    var encoding = UTF8Encoding.UTF8;
                    using (var reader = new StreamReader(response.GetResponseStream(), encoding))
                    {
                        string text = reader.ReadToEnd();
                        dynamic json = new JavaScriptSerializer().Deserialize<dynamic>(text);
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
