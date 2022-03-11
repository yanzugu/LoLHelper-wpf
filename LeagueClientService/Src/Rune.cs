using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using LeagueClientService.Src.Enums;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace LeagueClientService.Src
{
    public class Rune
    {
        private readonly Dictionary<string, string> championToRuneDict;
        private readonly LeagueClient leagueClient;

        public Rune(LeagueClient _leagueClient)
        {
            leagueClient = _leagueClient;
            championToRuneDict = new Dictionary<string, string>();
        }

        public Dictionary<string, int> GetRunePageIds()
        {
            var url = leagueClient.url_prefix + "/lol-perks/v1/pages";
            var req = leagueClient.Request(url, "GET");
            Dictionary<string, int> runePageIds = new Dictionary<string, int>();

            try
            {
                using (WebResponse response = req.GetResponse())
                {
                    var encoding = UTF8Encoding.UTF8;
                    using (var reader = new StreamReader(response.GetResponseStream(), encoding))
                    {
                        string text = reader.ReadToEnd();
                        JArray jArray = JArray.Parse(text);
                        int i = 1;

                        foreach (var rune in jArray)
                        {
                            int id = Convert.ToInt32(rune["id"]);
                            string name = rune["name"].ToString();

                            if (id == 50 || id == 51 || id == 52 ||
                                id == 53 || id == 54)
                            {
                                continue;
                            }
                            else
                            {
                                if (runePageIds.ContainsKey(name))
                                    runePageIds.Add(name + (i++).ToString(), id);
                                else
                                    runePageIds.Add(name, id);
                            }
                        }
                    }
                }
                return runePageIds;
            }
            catch (Exception err)
            {
                WriteLog($"{err}", true);
            }

            return null;
        }

        public string GetRuneDetailById(int pageId)
        {
            var url = leagueClient.url_prefix + "/lol-perks/v1/pages";
            var req = leagueClient.Request(url, "GET");
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
                        JArray jArray = JArray.Parse(text);

                        foreach (var rune in jArray)
                        {
                            if (Convert.ToInt32(rune["id"]) == pageId)
                            {
                                detail = JsonConvert.SerializeObject(new
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
            catch (Exception err)
            {
                WriteLog($"{err}", true);
            }

            return null;
        }

        public void CreateRunepage(string pageInfo)
        {
            var url = leagueClient.url_prefix + "/lol-perks/v1/pages";
            var req = leagueClient.Request(url, "POST");
            try
            {
                using (var streamWriter = new StreamWriter(req.GetRequestStream()))
                {
                    if (string.IsNullOrEmpty(pageInfo)) return;
                    streamWriter.Write(pageInfo);
                }
                using (WebResponse response = req.GetResponse()) { }
            }
            catch (Exception err)
            {
                WriteLog($"{err}", true);
            }
        }

        public void DeleteRunepage(int pageId)
        {
            if (pageId == 50 || pageId == 51 || pageId == 52 ||
                            pageId == 53 || pageId == 54)
            {
                return;
            }
            var url = leagueClient.url_prefix + "/lol-perks/v1/pages/" + pageId.ToString();
            var req = leagueClient.Request(url, "DELETE");
            try
            {
                using (WebResponse response = req.GetResponse()) { }
            }
            catch (Exception err)
            {
                WriteLog($"{err}", true);
            }
        }

        public JArray GetCurrentRunePage()
        {
            var url = leagueClient.url_prefix + "/lol-perks/v1/currentpage";
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

        public Task<string> GetRuneInfo(string champion, Mode mode, string position = "")
        {
            if (string.IsNullOrEmpty(position)) position = "";
            position = position.ToLower();

            if (position.Contains("mid")) position = "mid";
            if (position.Contains("bot")) position = "adc";
            if (position.Contains("utility")) position = "support";

            string url = mode switch
            {
                Mode.Normal => $"https://tw.op.gg/champions/{champion}/statistics/{position}",
                Mode.Aram => $"https://tw.op.gg/modes/aram/{champion}/build",
                _ => $"https://tw.op.gg/champions/{champion}/statistics/{position}"
            };

            try
            {
                using (HttpClient client = new HttpClient())
                {
                    string content = client.GetStringAsync(url).Result;
                    string pattern = "<div class=\"rune_box\">[^|]*</div>";
                    Regex reg = new Regex(pattern);
                    MatchCollection matches = reg.Matches(content);

                    content = matches[0].Value;
                    pattern = "perkStyle.([0-9]*)";
                    reg = new Regex(pattern);
                    matches = reg.Matches(content);
                    var primaryStyleId = Convert.ToInt32(matches[0].Groups[1].Value);
                    var subStyleId = Convert.ToInt32(matches[1].Groups[1].Value);
                    List<int> perkIds = new List<int>();

                    pattern = "perk.([0-9]+)[^|]*?>";
                    reg = new Regex(pattern);
                    matches = reg.Matches(content);

                    for (int i = 0; i < matches.Count / 2; i++)
                    {
                        var match = matches[i];

                        if (!match.Groups[0].Value.ToLower().Contains("grayscale"))
                        {
                            perkIds.Add(Convert.ToInt32(match.Groups[1].Value));
                        }
                    }

                    pattern = "perkShard.([0-9]*)[^|]*?>";
                    reg = new Regex(pattern);
                    matches = reg.Matches(content);

                    for (int i = 0; i < matches.Count / 2; i++)
                    {
                        var match = matches[i];

                        if (!match.Groups[0].Value.ToLower().Contains("grayscale"))
                        {
                            perkIds.Add(Convert.ToInt32(match.Groups[1].Value));
                        }
                    }

                    string pageInfo = JsonConvert.SerializeObject(new
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
            catch (Exception err)
            {
                WriteLog($"{err}", true);
            }

            return null;
        }

        public void SetRune(string champion, string position = "", Mode mode = Mode.Normal)
        {
            try
            {
                Dictionary<string, int> pageIds = GetRunePageIds();

                foreach (var key in pageIds.Keys)
                {
                    if (key.Contains("OP.GG"))
                    {
                        DeleteRunepage(pageIds[key]);
                        break;
                    }
                }

                string championKey = $"{champion}-{position}-{mode}";
                string pageInfo;

                if (championToRuneDict.ContainsKey(championKey))
                {
                    pageInfo = championToRuneDict[championKey];
                }
                else
                {
                    pageInfo = GetRuneInfo(champion, mode, position).Result;
                    championToRuneDict.Add(championKey, pageInfo);
                }

                if (pageInfo != null)
                {
                    CreateRunepage(pageInfo);

                    WriteLog($"champion: {champion}, position: {position}, mode: {mode}");
                }
                else
                {
                    WriteLog("can not get rune info.");
                }
            }
            catch (Exception err)
            {
                WriteLog($"{err}", true);
            }
        }

        private void WriteLog(string msg, bool isException = false, [CallerMemberName] string callerName = null)
        {
            LogManager.WriteLog($"[Rune]{callerName}() {msg}", isException);
        }
    }
}
