﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using LoLHelper_rework_wpf_.Interfaces;

namespace LoLHelper_rework_wpf_.Implements
{
    class Rune : IRune
    {
        private readonly LeagueClient _leagueClient;

        public Rune(LeagueClient leagueClient)
        {
            _leagueClient = leagueClient;
        }

        public Dictionary<string, int> Get_Rune_PageIds()
        {
            var url = _leagueClient.url_prefix + "/lol-perks/v1/pages";
            var req = _leagueClient.Request(url, "GET");
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
            var url = _leagueClient.url_prefix + "/lol-perks/v1/pages";
            var req = _leagueClient.Request(url, "GET");
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
            var url = _leagueClient.url_prefix + "/lol-perks/v1/pages";
            var req = _leagueClient.Request(url, "POST");
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
            var url = _leagueClient.url_prefix + "/lol-perks/v1/pages/" + pageId.ToString();
            var req = _leagueClient.Request(url, "DELETE");
            try
            {
                using (WebResponse response = req.GetResponse()) { }
            }
            catch
            { }
        }

        public Dictionary<string, dynamic> Get_Current_RunePage()
        {
            var url = _leagueClient.url_prefix + "/lol-perks/v1/currentpage";
            var req = _leagueClient.Request(url, "GET");
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

                    foreach (System.Text.RegularExpressions.Match match in matches)
                    {
                        perkIds.Add(Convert.ToInt32(match.Groups[1].Value));
                    }

                    pattern = "<div class=.fragment[^|]*?<img[^|]*?perkShard.([0-9]*)[^|]*?>";
                    reg = new Regex(pattern);
                    matches = reg.Matches(content);

                    foreach (System.Text.RegularExpressions.Match match in matches)
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
    }
}
