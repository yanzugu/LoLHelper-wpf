using LoLHelper.Src.Enums;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;

namespace LoLHelper.Src.Service
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

        public JObject GetChampSelectSession()
        {
            Gameflow gameflow = leagueClient.GetGameflow();

            if (gameflow != Gameflow.ChampSelect) return null;

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
                        JObject jObject = JObject.Parse(text);

                        return jObject;
                    }
                }
            }
            catch (Exception err)
            {
                WriteLog($"{err}", true);
            }

            return null;
        }

        public int? GetMyPickChampionId()
        {
            try
            {
                int? championId = null;
                JObject jObject = GetChampSelectSession();

                if (jObject == null) return null;

                int localPlayerCellId = Convert.ToInt32(jObject["localPlayerCellId"]);

                foreach (JObject myTeam in jObject["myTeam"])
                {
                    if (Convert.ToInt32(myTeam["cellId"]) == localPlayerCellId)
                    {
                        championId = Convert.ToInt32(myTeam["championId"]);

                        break;
                    }
                }

                return championId;
            }
            catch (Exception err)
            {
                WriteLog($"{err}", true);
            }

            return null;
        }

        public string GetMyPosition()
        {

            try
            {
                string myPosition = null;
                JObject jObject = GetChampSelectSession();

                if (jObject == null) return null;

                int localPlayerCellId = Convert.ToInt32(jObject["localPlayerCellId"]);

                foreach (JObject myTeam in jObject["myTeam"])
                {
                    if (Convert.ToInt32(myTeam["cellId"]) == localPlayerCellId)
                    {
                        myPosition = myTeam["assignedPosition"].ToString();
                        break;
                    }
                }

                return myPosition;
            }
            catch (Exception err)
            {
                WriteLog($"{err}", true);
            }

            return null;
        }

        public List<int> GetPickedChampionsId()
        {
            try
            {
                List<int> IdList = new List<int>();
                JObject jObject = GetChampSelectSession();

                if (jObject == null) return null;

                foreach (JObject myTeam in jObject["myTeam"])
                {
                    IdList.Add(Convert.ToInt32(myTeam["championId"]));
                }

                return IdList;
            }
            catch (Exception err)
            {
                WriteLog($"{err}", true);
            }

            return null;
        }

        public int? GetPlayerId()
        {
            try
            {
                int localPlayerCellId;
                int? playerId = null;
                JObject jObject = GetChampSelectSession();

                if (jObject == null) return null;

                localPlayerCellId = Convert.ToInt32(jObject["localPlayerCellId"]);

                foreach (JObject actions in jObject["actions"][0])
                {
                    if (Convert.ToInt32(actions["actorCellId"]) == localPlayerCellId)
                    {
                        playerId = Convert.ToInt32(actions["id"]);
                        break;
                    }
                }

                return playerId;
            }
            catch (Exception err)
            {
                WriteLog($"{err}", true);
            }

            return null;
        }

        public List<int> GetTeammatesSummonerIds()
        {
            List<int> list = new List<int>();

            try
            {
                JObject jObject = GetChampSelectSession();

                foreach (JObject summoner in jObject["myTeam"])
                {
                    list.Add(Convert.ToInt32(summoner["summonerId"]));
                }

                return list;
            }
            catch (Exception err)
            {
                WriteLog($"{err}", true);
            }

            return null;
        }

        public void PickChampion(int championId, bool autoLock)
        {
            int? playerId;

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

                WriteLog($"PickChampion() championId: {championId}, autoLock: {autoLock}");
            }
            catch (Exception err)
            {
                WriteLog($"{err}", true);
            }
        }

        public void PickLane(string lane, int times)
        {
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

                    Thread.Sleep(200);
                }

                if (string.IsNullOrEmpty(roomId) == false)
                {
                    for (int i = 0; i < times; ++i)
                    {
                        chat.SendMessage(lane, roomId);
                        Thread.Sleep(200);
                    }

                    WriteLog($"PickLane() lane: {lane}, times: {times}");
                }
                else
                {
                    WriteLog("PickLane() can not get chat room id");
                }
            }
            catch (Exception err)
            {
                WriteLog($"{err}", true);
            }
        }

        private void WriteLog(string msg, bool isException = false)
        {
            LogManager.WriteLog($"[ChampSelect]{msg}", isException);
        }
    }
}
