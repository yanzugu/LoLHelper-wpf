using System;
using System.IO;
using System.Threading.Tasks;

namespace LeagueClientService.Src
{
    internal static class LogManager
    {
        static string logPath = $"Log\\log{DateTime.Now.ToString("yyyyMM")}-s.txt";

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
