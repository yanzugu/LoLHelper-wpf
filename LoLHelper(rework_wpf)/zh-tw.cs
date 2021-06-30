using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using System.Windows;
using LoLHelper_rework_wpf_.Interfaces;

namespace LoLHelper_rework_wpf_
{
    class Zh_Tw
    {
        ILeagueClient leagueClient;
        private Dictionary<string, string> Name_Dict;

        public Zh_Tw(ILeagueClient _leagueClient)
        {
            leagueClient = _leagueClient;
            Create_Dict();
        }



        private void Create_Dict()
        {
            var req = leagueClient.Request("https://yanzugu.github.io/LH/champions.json", "GET");
            Name_Dict = new Dictionary<string, string>();
            try
            {
                using (WebResponse response = req.GetResponse())
                {
                    var encoding = UTF8Encoding.UTF8;
                    using (var reader = new StreamReader(response.GetResponseStream(), encoding))
                    {
                        string text = reader.ReadToEnd();
                        dynamic json = new JavaScriptSerializer().Deserialize<dynamic>(text);
                        foreach (var data in json)
                        {
                            Name_Dict.Add(data.Key, data.Value);
                        }
                    }
                }
            }
            catch
            {
                Name_Dict = new Dictionary<string, string>()
                {
                    { "Annie", "安妮" },
                    { "Olaf", "歐拉夫" },
                    { "Galio", "加里歐" },
                    { "TwistedFate", "逆命" },
                    { "XinZhao", "趙信" },
                    { "Urgot", "烏爾加特" },
                    { "Leblanc", "勒布朗" },
                    { "Vladimir", "弗拉迪米爾" },
                    { "FiddleSticks", "費德提克" },
                    { "Kayle", "凱爾" },
                    { "MasterYi", "易大師" },
                    { "Alistar", "亞歷斯塔" },
                    { "Ryze", "雷茲" },
                    { "Sion", "賽恩" },
                    { "Sivir", "希維爾" },
                    { "Soraka", "索拉卡" },
                    { "Teemo", "提摩" },
                    { "Tristana", "崔絲塔娜" },
                    { "Warwick", "沃維克" },
                    { "Nunu", "努努" },
                    { "MissFortune", "好運姐" },
                    { "Ashe", "艾希" },
                    { "Tryndamere", "泰達米爾" },
                    { "Jax", "賈克斯" },
                    { "Morgana", "魔甘娜" },
                    { "Zilean", "極靈" },
                    { "Singed", "辛吉德" },
                    { "Evelynn", "伊芙琳" },
                    { "Twitch", "圖奇" },
                    { "Karthus", "卡爾瑟斯" },
                    { "Chogath", "科加斯" },
                    { "Amumu", "阿姆姆" },
                    { "Rammus", "拉姆斯" },
                    { "Anivia", "艾妮維亞" },
                    { "Shaco", "薩科" },
                    { "DrMundo", "蒙多醫生" },
                    { "Sona", "索娜" },
                    { "Kassadin", "卡薩丁" },
                    { "Irelia", "伊瑞莉雅" },
                    { "Janna", "珍娜" },
                    { "Gangplank", "剛普朗克" },
                    { "Corki", "庫奇" },
                    { "Karma", "卡瑪" },
                    { "Taric", "塔里克" },
                    { "Veigar", "維迦" },
                    { "Trundle", "特朗德" },
                    { "Swain", "斯溫" },
                    { "Caitlyn", "凱特琳" },
                    { "Blitzcrank", "布里茨" },
                    { "Malphite", "墨菲特" },
                    { "Katarina", "卡特蓮娜" },
                    { "Nocturne", "夜曲" },
                    { "Maokai", "茂凱" },
                    { "Renekton", "雷尼克頓" },
                    { "JarvanIV", "嘉文四世" },
                    { "Elise", "伊莉絲" },
                    { "Orianna", "奧莉安娜" },
                    { "MonkeyKing", "悟空" },
                    { "Brand", "布蘭德" },
                    { "LeeSin", "李星" },
                    { "Vayne", "汎" },
                    { "Rumble", "藍寶" },
                    { "Cassiopeia", "卡莎碧雅" },
                    { "Skarner", "史加納" },
                    { "Heimerdinger", "漢默丁格" },
                    { "Nasus", "納瑟斯" },
                    { "Nidalee", "奈德麗" },
                    { "Udyr", "烏迪爾" },
                    { "Poppy", "波比" },
                    { "Gragas", "古拉格斯" },
                    { "Pantheon", "潘森" },
                    { "Ezreal", "伊澤瑞爾" },
                    { "Mordekaiser", "魔鬥凱薩" },
                    { "Yorick", "約瑞科" },
                    { "Akali", "阿卡莉" },
                    { "Kennen", "凱能" },
                    { "Garen", "蓋倫" },
                    { "Leona", "雷歐娜" },
                    { "Malzahar", "馬爾札哈" },
                    { "Talon", "塔龍" },
                    { "Riven", "雷玟" },
                    { "KogMaw", "寇格魔" },
                    { "Shen", "慎" },
                    { "Lux", "拉克絲" },
                    { "Xerath", "齊勒斯" },
                    { "Shyvana", "希瓦娜" },
                    { "Ahri", "阿璃" },
                    { "Graves", "葛雷夫" },
                    { "Fizz", "飛斯" },
                    { "Volibear", "弗力貝爾" },
                    { "Rengar", "雷葛爾" },
                    { "Varus", "法洛士" },
                    { "Nautilus", "納帝魯斯" },
                    { "Viktor", "維克特" },
                    { "Sejuani", "史瓦妮" },
                    { "Fiora", "菲歐拉" },
                    { "Ziggs", "希格斯" },
                    { "Lulu", "露露" },
                    { "Draven", "達瑞文" },
                    { "Hecarim", "赫克林" },
                    { "Khazix", "卡力斯" },
                    { "Darius", "達瑞斯" },
                    { "Jayce", "杰西" },
                    { "Lissandra", "麗珊卓" },
                    { "Diana", "黛安娜" },
                    { "Quinn", "葵恩" },
                    { "Syndra", "星朵拉" },
                    { "AurelionSol", "翱銳龍獸" },
                    { "Kayn", "慨影" },
                    { "Zoe", "柔依" },
                    { "Zyra", "枷蘿" },
                    { "Kaisa", "凱莎" },
                    { "Gnar", "吶兒" },
                    { "Zac", "札克" },
                    { "Yasuo", "犽宿" },
                    { "Velkoz", "威寇茲" },
                    { "Taliyah", "塔莉雅" },
                    { "Camille", "卡蜜兒" },
                    { "Braum", "布郎姆" },
                    { "Jhin", "燼" },
                    { "Kindred", "鏡爪" },
                    { "Jinx", "吉茵珂絲" },
                    { "TahmKench", "貪啃奇" },
                    { "Senna", "姍娜" },
                    { "Lucian", "路西恩" },
                    { "Zed", "劫" },
                    { "Kled", "克雷德" },
                    { "Ekko", "艾克" },
                    { "Qiyana", "姬亞娜" },
                    { "Vi", "菲艾" },
                    { "Aatrox", "厄薩斯" },
                    { "Nami", "娜米" },
                    { "Azir", "阿祈爾" },
                    { "Yuumi", "悠咪" },
                    { "Thresh", "瑟雷西" },
                    { "Illaoi", "伊羅旖" },
                    { "RekSai", "雷珂煞" },
                    { "Ivern", "埃爾文" },
                    { "Kalista", "克黎思妲" },
                    { "Bard", "巴德" },
                    { "Rakan", "銳空" },
                    { "Xayah", "剎雅" },
                    { "Ornn", "鄂爾" },
                    { "Sylas", "賽勒斯" },
                    { "Neeko", "妮可" },
                    { "Aphelios", "亞菲利歐" },
                    { "Pyke", "派克" },
                    { "Sett", "賽特" },
                    { "Lillia", "莉莉亞" },
                    { "Yone", "犽凝" },
                    { "Samira", "煞蜜拉" },
                    { "Seraphine", "瑟菈紛" },
                    { "Viego", "維爾戈" },
                    { "Rell", "銳兒" },
                    { "Gwen", "關"},
                };
            }
        }

        public string en_to_ch(string en)
        {
            string value = null;
            if (Name_Dict.ContainsKey(en))
                value = Name_Dict[en];
            else
                value = en;
            return value;
        }
    }
}
