using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using CommissionSystem.Task.Models;
using ProtoBuf;

namespace CommissionSystem.Task
{
    class Program
    {
        static void Main(string[] args)
        {
            //SettingFactory sf = SettingFactory.Instance;
            //DiscountedCallServiceTask o = new DiscountedCallServiceTask();

            //DateTime dateFrom = new DateTime(2014, 6, 1);
            //DateTime dateTo = new DateTime(2014, 6, 30);

            //o.DateFrom = dateFrom;
            //o.DateTo = dateTo.AddDays(1);

            //o.Run();
            
            //using (FileStream f = File.Create("CommViewDic.bin"))
            //{
            //    Serializer.Serialize<Dictionary<string, List<VoiceCommissionView>>>(f, o.CommissionViewDic);
            //}

            //using (FileStream f = File.Create("AgentViewList.bin"))
            //{
            //    Serializer.Serialize<List<AgentView>>(f, o.AgentViewList);
            //}

            Dictionary<string, List<VoiceCommissionView>> x = null;
            using (var f = File.OpenRead("CommViewDic.bin"))
            {
                x = Serializer.Deserialize<Dictionary<string, List<VoiceCommissionView>>>(f);
            }

            List<AgentView> l = null;
            using (var f = File.OpenRead("AgentViewList.bin"))
            {
                l = Serializer.Deserialize<List<AgentView>>(f);
            }

            var k = l.Find(a => a.AgentID == 2220310);
            Console.WriteLine("{0}: {1}", k.AgentID, k.AgentName);
            var v = x[k.AgentID.ToString()];
            Console.WriteLine(v.Count);
            foreach (var g in v)
            {
                if (g.Customer == null)
                {
                    Console.WriteLine("null");
                    continue;
                }

                foreach (var s in g.Customer.SettlementList)
                {
                    foreach (var i in s.InvoiceList)
                    {
                        Console.WriteLine("{0} {1} {2} {3}", s.RealDate, i.CallChargesIDD, i.CallChargesSTD, i.CallChargesMOB);
                    }

                    Console.WriteLine();
                }
            }

            Console.WriteLine("done");
            Console.ReadKey();
        }
    }
}
