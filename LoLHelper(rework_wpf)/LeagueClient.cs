using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Script.Serialization;

namespace LoLHelper_rework_wpf_
{
    class LeagueClient : ILeagueClient
    {
        private string host;
        private string port;
        private string connection_method;
        private string authorization;
        private string url_prefix;

        public LeagueClient(string lockfile)
        {
            FileStream fs = new FileStream(lockfile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            StreamReader sr = new StreamReader(fs);
            string _data = sr.ReadToEnd();
            string[] data = _data.Split(':');

            this.host = "127.0.0.1";
            this.port = data[2];
            this.connection_method = data[4];
            var authId = Convert.ToBase64String(Encoding.UTF8.GetBytes("riot:" + data[3]));
            Encoding.UTF8.GetString(Convert.FromBase64String(authId));
            this.authorization = "Basic " + authId;
            this.url_prefix = this.connection_method + "://" + this.host + ':' + this.port;
        }

        public LeagueClient()
        { }

        public string Get_Gameflow()
        {
            var url = this.url_prefix + "/lol-gameflow/v1/gameflow-phase";
            var req = Request(url, "GET");
            try
            {
                using (WebResponse response = req.GetResponse())
                {
                    var encoding = UTF8Encoding.UTF8;
                    using (var reader = new StreamReader(response.GetResponseStream(), encoding))
                    {
                        string responseText = reader.ReadToEnd();
                        return responseText;
                    }
                }
            }
            catch
            {
                return null;
            }
        }

        public void Accept_MatchMaking()
        {
            var url = this.url_prefix + "/lol-matchmaking/v1/ready-check/accept";
            var req = Request(url, "POST");
            try
            {
                using (WebResponse response = req.GetResponse()) { }
            }
            catch
            {
            }
        }

        public void Find_Match()
        {
            bool isAccepted = false;
            while (true)
            {
                try
                {
                    var gameflow = Get_Gameflow();
                    // Already find match and not accept yet
                    if (gameflow == "\"ReadyCheck\"" && !isAccepted)
                    {
                        Accept_MatchMaking();
                        isAccepted = true;
                    }
                    else if (gameflow != "\"ReadyCheck\"")
                    {
                        isAccepted = false;
                    }
                }
                catch
                {
                }
                Thread.Sleep(50);
            }
        }

        /// <summary>
        /// Send message to match chatroom
        /// </summary>
        /// <param name="message"></param>
        /// <param name="receiver">Receiver Id</param>
        /// <param name="champSelect">If false show message to other player</param>
        public void Send_Message(string message, string receiver, bool champSelect=false)
        {
            string roomId = Get_ChatRoom_Id();
            if (string.IsNullOrEmpty(roomId)) return;
            var url = this.url_prefix + "/lol-chat/v1/conversations/" + roomId + "/messages";
            var req = Request(url, "POST");
            try
            {
                using (var streamWriter = new StreamWriter(req.GetRequestStream()))
                {
                    string json = new JavaScriptSerializer().Serialize(new
                    {
                        body = message,
                        type = champSelect ? "champSelect" : ""
                    });
                    streamWriter.Write(json);
                }
                using (WebResponse response = req.GetResponse()) { }
            }
            catch
            {
            }
        }

        public string Get_ChatRoom_Id()
        {
            var url = this.url_prefix + "/lol-chat/v1/conversations/";
            var req = Request(url, "GET");
            string roomId = null;
            try
            {
                using (WebResponse response = req.GetResponse())
                {
                    var encoding = UTF8Encoding.UTF8;
                    using (var reader = new StreamReader(response.GetResponseStream(), encoding))
                    {
                        string text = reader.ReadToEnd();
                        dynamic jsonToArray = new JavaScriptSerializer().Deserialize<dynamic>(text);
                        var length = ((Array)jsonToArray).Length;

                        for (int i = length - 1; i >= 0; i--)
                        {
                            var json = jsonToArray[i];
                            if (json["type"] == "championSelect")
                            {
                                roomId = json["id"];
                                break;
                            }
                        }
                    }
                }
                return roomId;
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

        /// <summary>
        /// Get all champions Id that teammates choose
        /// </summary>
        /// <returns></returns>
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

        public void Pick_Champion(int championId, bool autoLock)
        {
            int? playerId;
            if (Get_Gameflow() != "\"ChampSelect\"") return;
            if (Get_Picked_ChampionsId().Contains(championId)) return;
            if ((playerId = Get_PlayerId()) == null) return;

            var url = this.url_prefix + "/lol-champ-select/v1/session/actions/" + playerId.ToString();
            var req = Request(url, "PATCH");
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
            if (Get_Gameflow() != "\"ChampSelect\"") return;
            try
            {
                DateTime start = DateTime.Now;
                DateTime end;
                TimeSpan ts;
                string roomId = null;
                while (string.IsNullOrEmpty(roomId))
                {
                    roomId = Get_ChatRoom_Id();
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
                        Send_Message(lane, roomId);
                        Thread.Sleep(200);
                    }
                }
            }
            catch
            {
            }
        }

        /// <summary>
        /// Get owned champions dict => pair<name, id>
        /// </summary>
        /// <returns></returns>
        public Dictionary<string, int> Get_Owned_Champions_Dict()
        {
            var url = this.url_prefix + "/lol-champions/v1/owned-champions-minimal";
            var req = Request(url, "GET");
            Dictionary<string, int> dict = new Dictionary<string, int>();
            try
            {
                using (WebResponse response = req.GetResponse())
                {
                    var encoding = UTF8Encoding.UTF8;
                    using (var reader = new StreamReader(response.GetResponseStream(), encoding))
                    {
                        string text = reader.ReadToEnd();
                        dynamic json = new JavaScriptSerializer().Deserialize<dynamic>(text);

                        foreach (var data in json)
                        {
                            dict.Add(data["alias"], data["id"]);
                        }
                    }
                }
                return dict;
            }
            catch
            {
                return null;
            }
        }

        public void Create_My_Lobby(string mode, int queId)
        {
            var url = this.url_prefix + "/lol-lobby/v2/lobby";
            var req = Request(url, "POST");
            try
            {
                using (var streamWriter = new StreamWriter(req.GetRequestStream()))
                {
                    string json = new JavaScriptSerializer().Serialize(new
                    {
                        gameMode = mode,
                        queueId = queId
                    });
                    streamWriter.Write(json);
                }
                using (WebResponse response = req.GetResponse()) { }
            }
            catch
            {
            }
        }

        public void Start_Queueing()
        {
            while (true)
            {
                if (Get_Gameflow() != "\"Lobby\"") continue;
                try
                {
                    if (Check_Can_Queueing() == false) continue;
                    var url = this.url_prefix + "/lol-lobby/v2/lobby/matchmaking/search";
                    var req = Request(url, "POST");
                    using (WebResponse response = req.GetResponse()) { }
                }
                catch
                {
                }
                Thread.Sleep(1000);
            }
        }

        public void Pick_Spell(int spell1, int spell2)
        {
            if (Get_Gameflow() != "\"ChampSelect\"") return;
            var url = this.url_prefix + "/lol-champ-select/v1/session/my-selection";
            var req = Request(url, "PATCH");
            try
            {
                using (var streamWriter = new StreamWriter(req.GetRequestStream()))
                {
                    string json = new JavaScriptSerializer().Serialize(new
                    {
                        spell1Id = spell1,
                        spell2Id = spell2
                    });
                    streamWriter.Write(json);
                }
                using (WebResponse response = req.GetResponse()) { }
            }
            catch
            {
            }
        }

        public Dictionary<string, int> Get_Rune_PageIds()
        {
            var url = this.url_prefix + "/lol-perks/v1/pages";
            var req = Request(url, "GET");
            Dictionary<string, int> runePageIds = new Dictionary<string, int>();

            try
            {
                using (WebResponse response = req.GetResponse())
                {
                    var encoding = UTF8Encoding.UTF8;
                    using (var reader = new StreamReader(response.GetResponseStream(), encoding))
                    {
                        string text = reader.ReadToEnd();
                        dynamic json = new JavaScriptSerializer().Deserialize<dynamic>(text);
                        int i = 1;
                        foreach (var rune in json)
                        {
                            if (rune["id"] == 50 || rune["id"] == 51 || rune["id"] == 52 ||
                                rune["id"] == 53 || rune["id"] == 54)
                            {
                                continue;
                            }
                            else
                            {
                                if (runePageIds.ContainsKey(rune["name"]))
                                    runePageIds.Add(rune["name"] + (i++).ToString(), rune["id"]);
                                else
                                    runePageIds.Add(rune["name"], rune["id"]);
                            }
                        }
                    }
                }
                return runePageIds;
            }
            catch
            {
                return null;
            }
        }

        public string Get_Rune_Detail_By_Id(int pageId)
        {
            var url = this.url_prefix + "/lol-perks/v1/pages";
            var req = Request(url, "GET");
            Dictionary<string, int> runePageIds = new Dictionary<string, int>();
            string detail = null;

            try
            {
                using (WebResponse response = req.GetResponse())
                {
                    var encoding = UTF8Encoding.UTF8;
                    using (var reader = new StreamReader(response.GetResponseStream(), encoding))
                    {
                        string text = reader.ReadToEnd();
                        dynamic json = new JavaScriptSerializer().Deserialize<dynamic>(text);
                        foreach (var rune in json)
                        {
                            if (rune["id"] == pageId)
                            {
                                detail = new JavaScriptSerializer().Serialize(new
                                {
                                    current = true,
                                    name = rune["name"],
                                    selectedPerkIds = rune["selectedPerkIds"],
                                    primaryStyleId = rune["primaryStyleId"],
                                    subStyleId = rune["subStyleId"]
                                });
                            }
                        }
                    }
                }
                return detail;
            }
            catch
            {
                return null;
            }
        }

        public void Create_Runepage(string pageInfo)
        {
            var url = this.url_prefix + "/lol-perks/v1/pages";
            var req = Request(url, "POST");
            try
            {
                using (var streamWriter = new StreamWriter(req.GetRequestStream()))
                {
                    if (string.IsNullOrEmpty(pageInfo)) return;
                    streamWriter.Write(pageInfo);
                }
                using (WebResponse response = req.GetResponse()) { }
            }
            catch
            { }
        }

        public void Delete_Runepage(int pageId)
        {
            if (pageId == 50 || pageId == 51 || pageId == 52 ||
                            pageId == 53 || pageId == 54)
            {
                return;
            }
            var url = this.url_prefix + "/lol-perks/v1/pages/" + pageId.ToString();
            var req = Request(url, "DELETE");
            try
            {
                using (WebResponse response = req.GetResponse()) { }
            }
            catch
            { }
        }

        public Dictionary<string, dynamic> Get_Current_RunePage()
        {
            var url = this.url_prefix + "/lol-perks/v1/currentpage";
            var req = Request(url, "GET");
            Dictionary<string, dynamic> json = new Dictionary<string, dynamic>();
            try
            {
                using (WebResponse response = req.GetResponse())
                {
                    var encoding = UTF8Encoding.UTF8;
                    using (var reader = new StreamReader(response.GetResponseStream(), encoding))
                    {
                        string text = reader.ReadToEnd();
                        json = new JavaScriptSerializer().Deserialize<dynamic>(text);
                    }
                }
                return json;
            }
            catch
            {
                return null;
            }
        }

        public Task<string> Get_Rune_Info(string champion, string position = "")
        {
            if (string.IsNullOrEmpty(position)) position = "";
            position = position.ToLower();
            if (position.Contains("mid")) position = "mid";
            if (position.Contains("bot")) position = "adc";
            if (position.Contains("utility")) position = "support";
            string url = $"https://tw.op.gg/champion/{champion}/statistics/{position}";

            try
            {
                using (HttpClient client = new HttpClient())
                {
                    //var result = client.GetAsync(url).Result.Content;
                    var content = client.GetStringAsync(url).Result;
                    string pattern = "<td class=.champion-overview__data.[^|]*?<div class=.perk-page-wrap.>[^|]*?</td>";
                    Regex reg = new Regex(pattern);
                    MatchCollection matches = reg.Matches(content);

                    content = matches[0].Value;
                    pattern = "perkStyle.([0-9]*)";
                    reg = new Regex(pattern);
                    matches = reg.Matches(content);
                    var primaryStyleId = Convert.ToInt32(matches[0].Groups[1].Value);
                    var subStyleId = Convert.ToInt32(matches[1].Groups[1].Value);
                    List<int> perkIds = new List<int>();

                    pattern = "<div[^|]*?perk-page__item--active[^|]*?<img[^|]*?perk.([0-9]*)";
                    reg = new Regex(pattern);
                    matches = reg.Matches(content);

                    foreach (Match match in matches)
                    {
                        perkIds.Add(Convert.ToInt32(match.Groups[1].Value));
                    }

                    pattern = "<div class=.fragment[^|]*?<img[^|]*?perkShard.([0-9]*)[^|]*?>";
                    reg = new Regex(pattern);
                    matches = reg.Matches(content);

                    foreach (Match match in matches)
                    {
                        if (match.Value.Contains("active"))
                            perkIds.Add(Convert.ToInt32(match.Groups[1].Value));
                    }

                    dynamic pageInfo = new JavaScriptSerializer().Serialize(new
                    {
                        current = true,
                        name = $"OP.GG<{champion}>",
                        selectedPerkIds = perkIds.ToArray(),
                        primaryStyleId = primaryStyleId,
                        subStyleId = subStyleId
                    });

                    return Task.FromResult(pageInfo);
                }
            }
            catch
            {
                return null;
            }
        }

        public void Set_Rune(string champion, string position = "")
        {
            try
            {
                Dictionary<string, int> pageIds = Get_Rune_PageIds();
                foreach (var key in pageIds.Keys)
                {
                    if (key.Contains("OP.GG"))
                    {
                        Delete_Runepage(pageIds[key]);
                        break;
                    }
                }
                string pageInfo = Get_Rune_Info(champion, position).Result;
                if (pageInfo != null)
                {
                    Create_Runepage(pageInfo);
                }
            }
            catch
            {

            }
        }

        public bool Check_Can_Queueing()
        {
            var url = this.url_prefix + "/lol-lobby/v2/lobby";
            var req = Request(url, "GET");
            try
            {
                // check all members ready and I am leader
                using (WebResponse response = req.GetResponse())
                {
                    var encoding = UTF8Encoding.UTF8;
                    using (var reader = new StreamReader(response.GetResponseStream(), encoding))
                    {
                        string text = reader.ReadToEnd();
                        dynamic json = new JavaScriptSerializer().Deserialize<dynamic>(text);
                        if (json["localMember"]["isLeader"] == false)
                            return false;

                        foreach (var elem in json["members"])
                        {
                            if (elem["ready"] == false)
                                return false;
                        }

                        return true;
                    }
                }
            }
            catch
            {
                return false;
            }
        }

        public dynamic Get_Champ_Select_Session()
        {
            if (Get_Gameflow() != "\"ChampSelect\"") return null;
            var url = this.url_prefix + "/lol-champ-select/v1/session";
            var req = Request(url, "GET");
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

        public dynamic Get_SummonerInfo_By_SummonerId(int summonerId)
        {
            var url = this.url_prefix + $"/lol-summoner/v1/summoners/{summonerId}";
            var req = Request(url, "GET");
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

        public List<KeyValuePair<string, string>> Get_Teammates_Ranked()
        {
            List<int> id_List = Get_Teammates_SummonerIds();
            List<dynamic> info_List = new List<dynamic>();
            List<KeyValuePair<string, string>> ranked_pair_List = new List<KeyValuePair<string, string>>();
            if (id_List == null) return ranked_pair_List;
            try
            {
                foreach (int id in id_List)
                {
                    info_List.Add(Get_SummonerInfo_By_SummonerId(id));                   
                }

                foreach (var info in info_List)
                {
                    string uid = info["puuid"];
                    string name = info["displayName"];
                    string ranked = Get_Ranked_By_Uid(uid);
                    if (ranked != null)
                    {
                        ranked_pair_List.Add(new KeyValuePair<string, string>(name, ranked));
                    }
                }
                return ranked_pair_List;
            }
            catch
            {
                return null;
            }
        }

        public string Get_Ranked_By_Uid(string uid)
        {
            var url = this.url_prefix + $"/lol-ranked/v1/ranked-stats/{uid}";
            var req = Request(url, "GET");
            try
            {
                using (WebResponse response = req.GetResponse())
                {
                    var encoding = UTF8Encoding.UTF8;
                    using (var reader = new StreamReader(response.GetResponseStream(), encoding))
                    {
                        string text = reader.ReadToEnd();
                        dynamic json = new JavaScriptSerializer().Deserialize<dynamic>(text);
                        string tier = json["queueMap"]["RANKED_SOLO_5x5"]["tier"];
                        string division = json["queueMap"]["RANKED_SOLO_5x5"]["division"];
                        int point = json["queueMap"]["RANKED_SOLO_5x5"]["leaguePoints"];
                        int win = json["queueMap"]["RANKED_SOLO_5x5"]["wins"];

                        return string.Format("[{0} {1}] {2} 分，勝場 : {3}\n", tier, division, point, win);
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
            var req = Request(url, "GET");
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

        public void Show_Teammates_Ranked()
        {
            if (Get_Gameflow() != "\"ChampSelect\"") return;
            try
            {
                DateTime start = DateTime.Now;
                DateTime end;
                TimeSpan ts;
                string roomId = null;
                while (string.IsNullOrEmpty(roomId))
                {
                    roomId = Get_ChatRoom_Id();
                    end = DateTime.Now;
                    ts = end - start;
                    if (ts.TotalSeconds > 5)
                    {
                        break;
                    }
                }
                var list = Get_Teammates_Ranked();
                foreach (var el in list)
                {
                    Send_Message(el.Key+"\n"+el.Value, roomId, true);
                }               
            }
            catch
            {
            }
        }

        public HttpWebRequest Request(string url, string method)
        {
            System.Net.ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
            HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(url);
            request.Method = method;
            request.Accept = "application/json";
            request.Headers.Add("Authorization", this.authorization);

            return request;
        }        

        public dynamic Get_Match_Info_By_GameId(int gameId)
        {
            var url = this.url_prefix + $"/lol-match-history/v1/games/{gameId}";
            var req = Request(url, "GET");
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

        public void Test()
        {
            var url = this.url_prefix + "/lol-match-history/v1/games/1898611006";
            var req = Request(url, "GET");
            try
            {
                using (WebResponse response = req.GetResponse())
                {
                    var encoding = UTF8Encoding.UTF8;
                    using (var reader = new StreamReader(response.GetResponseStream(), encoding))
                    {
                        string text = reader.ReadToEnd();
                        dynamic json = new JavaScriptSerializer().Deserialize<dynamic>(text);
                        Console.WriteLine(text);
                    }
                }
            }
            catch
            {
            }
        }
    }
}