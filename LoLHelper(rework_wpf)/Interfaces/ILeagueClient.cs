using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace LoLHelper_rework_wpf_.Interfaces
{
    interface ILeagueClient
    {                     
        Dictionary<string, int> Get_Owned_Champions_Dict();
        string Get_Gameflow();
        HttpWebRequest Request(string url, string method);
    }
}
