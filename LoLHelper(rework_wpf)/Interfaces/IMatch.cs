﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoLHelper_rework_wpf_.Interfaces
{
    interface IMatch
    {
        List<int> Get_GameId_List_By_AccountId(string accid, int beginIdx = 0, int endIdx = 5);
        dynamic Get_Match_Info_By_GameId(int gameId);
        void Accept_MatchMaking();
        void Start_Queueing();
        bool Check_Can_Queueing();
    }
}
