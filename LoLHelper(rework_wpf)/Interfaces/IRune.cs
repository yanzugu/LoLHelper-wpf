using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoLHelper_rework_wpf_.Interfaces
{
    interface IRune
    {
        string Get_Rune_Detail_By_Id(int pageId);
        Task<string> Get_Rune_Info(string champion, string position);
        Dictionary<string, int> Get_Rune_PageIds();
        Dictionary<string, dynamic> Get_Current_RunePage();
        void Create_Runepage(string pageInfo);
        void Delete_Runepage(int pageId);
        void Set_Rune(string champion, string position);
    }
}
