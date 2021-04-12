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
using LoLHelper_rework_wpf_.Interfaces;

namespace LoLHelper_rework_wpf_.Implements
{
    class LeagueClient : ILeagueClient
    {
        protected string host;
        protected string port;
        protected string connection_method;
        protected string authorization;
        public string url_prefix;

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
            var req = this.Request(url, "GET");
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

        public HttpWebRequest Request(string url, string method)
        {
            System.Net.ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
            HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(url);
            request.Method = method;
            request.Accept = "application/json";
            request.Headers.Add("Authorization", this.authorization);

            return request;
        }     
    }
}