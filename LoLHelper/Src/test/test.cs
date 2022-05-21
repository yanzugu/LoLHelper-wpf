using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoLHelper.Src.test
{
    static internal class test
    {
        internal static void OPGGRuneTest()
        {
            LeagueClientService.Src.Rune rune = new LeagueClientService.Src.Rune();
            rune.GetRuneInfo("annie", LeagueClientService.Src.Enums.Mode.Normal);
        }
    }
}
