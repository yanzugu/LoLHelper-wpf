using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoLHelper_rework_wpf_.Interfaces
{
    interface IChat
    {
        void Send_Message(string message, string receiver, bool champSelect = false);
        string Get_ChatRoom_Id();
    }
}
