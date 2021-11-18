using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace LoLHelper.Src.Service
{
    internal class Chat
    {
        private readonly LeagueClient leagueClient;

        public Chat(LeagueClient _leagueClient)
        {
            leagueClient = _leagueClient;
        }

        public string GetChatRoomId()
        {
            var url = leagueClient.url_prefix + "/lol-chat/v1/conversations/";
            var req = leagueClient.Request(url, "GET");
            string roomId = null;
            try
            {
                using (WebResponse response = req.GetResponse())
                {
                    var encoding = UTF8Encoding.UTF8;
                    using (var reader = new StreamReader(response.GetResponseStream(), encoding))
                    {
                        string text = reader.ReadToEnd();
                        dynamic jsonToArray = JsonConvert.DeserializeObject<dynamic>(text);
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

        public void SendMessage(string message, string receiver, bool champSelect = false)
        {
            string roomId = GetChatRoomId();
            if (string.IsNullOrEmpty(roomId)) return;
            var url = leagueClient.url_prefix + "/lol-chat/v1/conversations/" + roomId + "/messages";
            var req = leagueClient.Request(url, "POST");
            try
            {
                using (var streamWriter = new StreamWriter(req.GetRequestStream()))
                {
                    string json = JsonConvert.SerializeObject(new
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
    }
}
