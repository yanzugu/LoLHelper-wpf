using LoLHelper_rework_wpf_.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Script.Serialization;

namespace LoLHelper_rework_wpf_.Implements
{
    class ChampSelect : IChampSelect
    {
        private readonly LeagueClient _leagueClient;
        private readonly Match _match;
        private readonly Chat _chat;

        public ChampSelect(LeagueClient leagueClient)
        {
            _leagueClient = leagueClient;
            _match = new Match(_leagueClient);
            _chat = new Chat(_leagueClient);
        }

        public dynamic Get_Champ_Select_Session()
        {
            if (_leagueClient.Get_Gameflow() != "\"ChampSelect\"") return null;
            var url = _leagueClient.url_prefix + "/lol-champ-select/v1/session";
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

        public int? Get_My_Pick_ChampionId()
        {
            try
            {
                int? championId = null;
                var json = Get_Champ_Select_Session();
                if (json == null) return null;

                var localPlayerCellId = json["localPlayerCellId"];
                foreach (var elem in json["myTeam"])
                {
                    if (elem["cellId"] == localPlayerCellId)
                    {
                        championId = elem["championId"];
                        break;
                    }
                }
                return championId;
            }
            catch
            {
                return null;
            }
        }

        public string Get_My_Position()
        {

            try
            {
                string myPosition = null;
                var json = Get_Champ_Select_Session();

                if (json == null) return null;

                var localPlayerCellId = json["localPlayerCellId"];
                foreach (var elem in json["myTeam"])
                {
                    if (elem["cellId"] == localPlayerCellId)
                    {
                        myPosition = elem["assignedPosition"];
                        break;
                    }
                }
                return myPosition;
            }
            catch
            {
                return null;
            }
        }

        public List<int> Get_Picked_ChampionsId()
        {
            try
            {
                List<int> IdList = new List<int>();
                var json = Get_Champ_Select_Session();

                if (json == null) return null;
                var myTeam = json["myTeam"];

                foreach (var elem in myTeam)
                {
                    IdList.Add(elem["championId"]);
                }
                return IdList;
            }
            catch
            {
                return null;
            }
        }

        public int? Get_PlayerId()
        {
            try
            {
                int localPlayerCellId;
                int? playerId = null;
                var json = Get_Champ_Select_Session();

                if (json == null) return null;

                localPlayerCellId = json["localPlayerCellId"];
                foreach (var elem in json["actions"][0])
                {
                    if (elem["actorCellId"] == localPlayerCellId)
                    {
                        playerId = elem["id"];
                        break;
                    }
                }
                return playerId;
            }
            catch
            {
                return null;
            }
        }

        public List<int> Get_Teammates_SummonerIds()
        {
            List<int> list = new List<int>();
            try
            {
                dynamic json = Get_Champ_Select_Session();
                foreach (var summoner in json["myTeam"])
                {
                    list.Add(summoner["summonerId"]);
                }
                return list;
            }
            catch
            {
                return null;
            }
        }

        public void Pick_Champion(int championId, bool autoLock)
        {
            int? playerId;
            if (_leagueClient.Get_Gameflow() != "\"ChampSelect\"") return;
            if (Get_Picked_ChampionsId().Contains(championId)) return;
            if ((playerId = Get_PlayerId()) == null) return;

            var url = _leagueClient.url_prefix + "/lol-champ-select/v1/session/actions/" + playerId.ToString();
            var req = _leagueClient.Request(url, "PATCH");
            try
            {
                using (var streamWriter = new StreamWriter(req.GetRequestStream()))
                {
                    string json = new JavaScriptSerializer().Serialize(new
                    {
                        championId = championId,
                        completed = autoLock,
                        type = "pick"
                    });
                    streamWriter.Write(json);
                }
                using (WebResponse response = req.GetResponse()) { }
            }
            catch
            {
            }
        }

        public void Pick_Selected_Lane(string lane, int times)
        {
            if (_leagueClient.Get_Gameflow() != "\"ChampSelect\"") return;
            try
            {
                DateTime start = DateTime.Now;
                DateTime end;
                TimeSpan ts;
                string roomId = null;
                while (string.IsNullOrEmpty(roomId))
                {
                    roomId = _chat.Get_ChatRoom_Id();
                    end = DateTime.Now;
                    ts = end - start;
                    if (ts.TotalSeconds > 5)
                    {
                        break;
                    }
                }
                if (string.IsNullOrEmpty(roomId) == false)
                {
                    for (int i = 0; i < times; ++i)
                    {
                        _chat.Send_Message(lane, roomId);
                        Thread.Sleep(200);
                    }
                }
            }
            catch
            {
            }
        }
    }
}
