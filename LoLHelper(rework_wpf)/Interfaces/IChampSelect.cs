using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoLHelper_rework_wpf_.Interfaces
{
    interface IChampSelect
    {
        int? Get_PlayerId();
        int? Get_My_Pick_ChampionId();
        List<int> Get_Picked_ChampionsId();
        List<int> Get_Teammates_SummonerIds();
        dynamic Get_Champ_Select_Session();
        string Get_My_Position();
        void Pick_Champion(int championId, bool autoLock);
        void Pick_Selected_Lane(string lane, int times);
    }
}
