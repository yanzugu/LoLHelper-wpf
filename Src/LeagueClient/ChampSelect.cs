using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using LoLHelper.Src.Enums;
using Newtonsoft.Json;

namespace LoLHelper.Src.LeagueClient
{
    internal class ChampSelect
    {
        private readonly LeagueClient leagueClient;
        private readonly Match match;
        private readonly Chat chat;

        public ChampSelect(LeagueClient _leagueClient)
        {
            leagueClient = _leagueClient;
            match = new Match(_leagueClient);
            chat = new Chat(_leagueClient);
        }

        public dynamic GetChampSelectSession()
        {
            if (leagueClient.GetGameflow() != Enums.Gameflow.ChampSelect) return null;

            var url = leagueClient.url_prefix + "/lol-champ-select/v1/session";
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

        public int? GetMyPickChampionId()
        {
            try
            {
                int? championId = null;
                var json = GetChampSelectSession();
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

        public string GetMyPosition()
        {

            try
            {
                string myPosition = null;
                var json = GetChampSelectSession();

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

        public List<int> GetPickedChampionsId()
        {
            try
            {
                List<int> IdList = new List<int>();
                var json = GetChampSelectSession();

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

        public int? GetPlayerId()
        {
            try
            {
                int localPlayerCellId;
                int? playerId = null;
                var json = GetChampSelectSession();

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

        public List<int> GetTeammatesSummonerIds()
        {
            List<int> list = new List<int>();

            try
            {
                dynamic json = GetChampSelectSession();

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

        public void PickChampion(int championId, bool autoLock)
        {
            int? playerId;
            if (leagueClient.GetGameflow() != Gameflow.ChampSelect) return;
            if (GetPickedChampionsId().Contains(championId)) return;
            if ((playerId = GetPlayerId()) == null) return;

            var url = leagueClient.url_prefix + "/lol-champ-select/v1/session/actions/" + playerId.ToString();
            var req = leagueClient.Request(url, "PATCH");

            try
            {
                using (var streamWriter = new StreamWriter(req.GetRequestStream()))
                {
                    string json = JsonConvert.SerializeObject(new
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

        public void PickSelectedLane(string lane, int times)
        {
            if (leagueClient.GetGameflow() != Gameflow.ChampSelect) return;

            try
            {
                DateTime start = DateTime.Now;
                DateTime end;
                TimeSpan ts;
                string roomId = null;

                while (string.IsNullOrEmpty(roomId))
                {
                    roomId = chat.GetChatRoomId();
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
                        chat.SendMessage(lane, roomId);
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
