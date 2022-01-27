using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace LoLHelper.Src
{
    static class LogManager
    {
        static string logPath = $"Log\\log{DateTime.Now.ToString("yyyyMM")}.txt";

        static public void WriteLog(string msg, bool isException = false)
        {
            Task.Run(() =>
            {
                using (StreamWriter sw = File.AppendText(logPath))
                {
                    if (isException)
                    {
                        sw.WriteLine($"{DateTime.Now} [LoLHelper][Exception]{msg}");
                    }
                    else
                    {
                        sw.WriteLine($"{DateTime.Now} [LoLHelper][Info]{msg}");
                    }
                }
            });
        }
    }
}
