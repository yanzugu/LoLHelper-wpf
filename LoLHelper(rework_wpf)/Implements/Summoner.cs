using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using LoLHelper_rework_wpf_.Interfaces;

namespace LoLHelper_rework_wpf_.Implements
{
    class Summoner : ISummoner
    {
        private readonly LeagueClient _leagueClient;
        private readonly Match _match;
        private readonly Chat _chat;
        private ChampSelect _champSelect;

        public Summoner(LeagueClient leagueClient)
        {
            _leagueClient = leagueClient;
            _match = new Match(_leagueClient);
            _chat = new Chat(_leagueClient);
            _champSelect = new ChampSelect(_leagueClient);
        }

        public dynamic Get_SummonerInfo_By_SummonerId(int summonerId)
        {
            var url = _leagueClient.url_prefix + $"/lol-summoner/v1/summoners/{summonerId}";
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

        public List<KeyValuePair<string, string>> Get_Teammates_Ranked()
        {
            List<int> id_List = _champSelect.Get_Teammates_SummonerIds();
            List<dynamic> info_List = new List<dynamic>();
            List<KeyValuePair<string, string>> ranked_pair_List = new List<KeyValuePair<string, string>>();
            if (id_List == null) return null;
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
            var url = _leagueClient.url_prefix + $"/lol-ranked/v1/ranked-stats/{uid}";
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
                        string tier = json["queueMap"]["RANKED_SOLO_5x5"]["tier"];
                        string division = json["queueMap"]["RANKED_SOLO_5x5"]["division"];
                        int point = json["queueMap"]["RANKED_SOLO_5x5"]["leaguePoints"];
                        int win = json["queueMap"]["RANKED_SOLO_5x5"]["wins"];

                        return string.Format("[ {0} {1} ] {2} 分，勝場 : {3}", tier, division, point, win);
                    }
                }
            }
            catch
            {
                return null;
            }
        }

        public void Show_Teammates_Ranked()
        {
            if (_leagueClient.Get_Gameflow() != "\"ChampSelect\"") return;
            try
            {
                string roomId = _chat.Get_ChatRoom_Id();
                string rank = "";
                var list = Get_Teammates_Ranked();

                if (roomId != null && list != null)
                {
                    foreach (var el in list)
                    {
                        rank += ".\n[" + el.Key + "]" + "\n" + el.Value + "\n";               
                    }
                    _chat.Send_Message(rank, roomId, true);
                }                               
            }
            catch
            {
            }
        }
    }
}
