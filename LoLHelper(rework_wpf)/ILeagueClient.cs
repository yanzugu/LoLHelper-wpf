using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoLHelper_rework_wpf_
{
    interface ILeagueClient
    {
        string Get_Gameflow();
        string Get_ChatRoom_Id();
        string Get_My_Position();
        string Get_Rune_Detail_By_Id(int pageId);
        Task<string> Get_Rune_Info(string champion, string position);
        int? Get_PlayerId();
        int? Get_My_Pick_ChampionId();
        List<int> Get_Picked_ChampionsId();
        List<int> Get_Teammates_SummonerIds();
        List<KeyValuePair<string, string>> Get_Teammates_Ranked();
        Dictionary<string, int> Get_Owned_Champions_Dict();
        Dictionary<string, int> Get_Rune_PageIds();
        Dictionary<string, dynamic> Get_Current_RunePage();
        void Accept_MatchMaking();
        void Find_Match();
        void Send_Message(string message, string receiver, bool champSelect=false);
        void Pick_Champion(int championId, bool autoLock);
        void Pick_Selected_Lane(string lane, int times);
        void Create_My_Lobby(string mode, int queId);
        void Start_Queueing();
        void Pick_Spell(int spell1, int spell2);
        void Create_Runepage(string pageInfo);
        void Delete_Runepage(int pageId);
        void Set_Rune(string champion, string position);
        dynamic Get_Champ_Select_Session();
        dynamic Get_SummonerInfo_By_SummonerId(int summonerId);
    }
}
