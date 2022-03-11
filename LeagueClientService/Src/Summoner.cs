using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using LeagueClientService.Src.Enums;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace LeagueClientService.Src
{
    public class Summoner
    {
        private readonly LeagueClient leagueClient;
        private readonly Match _match;
        private readonly Chat _chat;
        private ChampSelect _champSelect;

        public Summoner(LeagueClient _leagueClient)
        {
            leagueClient = _leagueClient;
            _match = new Match(_leagueClient);
            _chat = new Chat(_leagueClient);
            _champSelect = new ChampSelect(_leagueClient);
        }

        public JArray GetSummonerInfoBySummonerId(int summonerId)
        {
            var url = leagueClient.url_prefix + $"/lol-summoner/v1/summoners/{summonerId}";
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

        public List<KeyValuePair<string, string>> GetTeammatesRanked()
        {
            List<int> id_List = _champSelect.GetTeammatesSummonerIds();
            List<dynamic> info_List = new List<dynamic>();
            List<KeyValuePair<string, string>> ranked_pair_List = new List<KeyValuePair<string, string>>();

            if (id_List == null) return null;

            try
            {
                foreach (int id in id_List)
                {
                    info_List.Add(GetSummonerInfoBySummonerId(id));
                }

                foreach (var info in info_List)
                {
                    string uid = info["puuid"];
                    string name = info["displayName"];
                    string ranked = GetRankedByUid(uid);
                    if (ranked != null)
                    {
                        ranked_pair_List.Add(new KeyValuePair<string, string>(name, ranked));
                    }
                }
                return ranked_pair_List;
            }
            catch (Exception err)
            {
                WriteLog($"{err}", true);
            }

            return null;
        }

        public string GetRankedByUid(string uid)
        {
            var url = leagueClient.url_prefix + $"/lol-ranked/v1/ranked-stats/{uid}";
            var req = leagueClient.Request(url, "GET");

            try
            {
                using (WebResponse response = req.GetResponse())
                {
                    var encoding = UTF8Encoding.UTF8;

                    using (var reader = new StreamReader(response.GetResponseStream(), encoding))
                    {
                        string text = reader.ReadToEnd();
                        JObject json = JObject.Parse(text);
                        string tier = json["queueMap"]["RANKED_SOLO_5x5"]["tier"].ToString();
                        string division = json["queueMap"]["RANKED_SOLO_5x5"]["division"].ToString();
                        int point = Convert.ToInt32(json["queueMap"]["RANKED_SOLO_5x5"]["leaguePoints"]);
                        int win = Convert.ToInt32(json["queueMap"]["RANKED_SOLO_5x5"]["wins"]);

                        return string.Format("[ {0} {1} ] {2} 分，勝場 : {3}", tier, division, point, win);
                    }
                }
            }
            catch (Exception err)
            {
                WriteLog($"{err}", true);
            }

            return null;
        }

        public void ShowTeammatesRanked()
        {
            try
            {
                string roomId = _chat.GetChatRoomId();
                string rank = "";
                var list = GetTeammatesRanked();

                if (roomId != null && list != null)
                {
                    foreach (var el in list)
                    {
                        rank += ".\n[" + el.Key + "]" + "\n" + el.Value + "\n";
                    }
                    _chat.SendMessage(rank, roomId, true);
                }
            }
            catch (Exception err)
            {
                WriteLog($"{err}", true);
            }
        }

        private void WriteLog(string msg, bool isException = false, [CallerMemberName] string callerName = null)
        {
            LogManager.WriteLog($"[Summoner]{callerName}() {msg}", isException);
        }
    }
}
