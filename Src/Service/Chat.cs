using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

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
                        JArray jArray = JArray.Parse(text);

                        for (int i = jArray.Count - 1; i >= 0; i--)
                        {
                            if (jArray[i]["type"].ToString() == "championSelect")
                            {
                                roomId = jArray[i]["id"].ToString();
                                break;
                            }
                        }
                    }
                }

                return roomId;
            }
            catch (Exception err)
            {
                WriteLog($"{err}", true);
            }
            
            return null;
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
            catch (Exception err)
            {
                WriteLog($"{err}", true);
            }
        }

        private void WriteLog(string msg, bool isException = false, [CallerMemberName] string callerName = null)
        {
            LogManager.WriteLog($"[Chat]{callerName}() {msg}", isException);
        }
    }
}
