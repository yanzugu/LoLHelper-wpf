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
    class Chat : IChat
    {
        private readonly LeagueClient _leagueClient;

        public Chat(LeagueClient leagueClient)
        {
            _leagueClient = leagueClient;
        }

        public string Get_ChatRoom_Id()
        {
            var url = _leagueClient.url_prefix + "/lol-chat/v1/conversations/";
            var req = _leagueClient.Request(url, "GET");
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

        public void Send_Message(string message, string receiver, bool champSelect = false)
        {
            string roomId = Get_ChatRoom_Id();
            if (string.IsNullOrEmpty(roomId)) return;
            var url = _leagueClient.url_prefix + "/lol-chat/v1/conversations/" + roomId + "/messages";
            var req = _leagueClient.Request(url, "POST");
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
    }
}
