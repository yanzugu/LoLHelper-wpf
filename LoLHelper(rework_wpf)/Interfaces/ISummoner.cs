using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoLHelper_rework_wpf_.Interfaces
{
    interface ISummoner
    {
        dynamic Get_SummonerInfo_By_SummonerId(int summonerId);
        string Get_Ranked_By_Uid(string uid);
        List<KeyValuePair<string, string>> Get_Teammates_Ranked();
        void Show_Teammates_Ranked();
    }
}
