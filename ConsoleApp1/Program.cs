using Metro_system.Models;
using Metro_system.Operational;
using Metro_system.Operations;
using RO.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp1
{
    class Program
    {
        private static Log Log = new Log(typeof(Program));
        static void Main(string[] args)
        {
            //var x = SubWayOperation.GetSubway("洒金桥", "吉祥村");
            //Console.WriteLine(x.Keys.First());
            //Log.Error("11122");
            //decimal? x = CardBalanceOperation.Recharge("10000010", 10m);

            //decimal? y = CardBalanceOperation.GetCardBalance("10000010");
            //var x = MongoOperation.FindDocument<Card>(MongoCollectionName.Cards, new Dictionary<string, string> { { "CardId", "1" } });
            //Console.WriteLine(x.Count);

            //decimal? x = CardBalanceOperation.Recharge("10000000",20.1m);
            //Console.WriteLine(x);
            ////Console.WriteLine(card.CardBalance);

            CardBalanceOperation.Out("10000000","会展中心");



            Console.ReadKey();
        }
    }
}
