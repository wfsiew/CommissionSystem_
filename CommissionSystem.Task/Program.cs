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
            SettingFactory sf = SettingFactory.Instance;
            DiscountedCallServiceTask o = new DiscountedCallServiceTask();

            DateTime dateFrom = new DateTime(2014, 6, 1);
            DateTime dateTo = new DateTime(2014, 6, 30);

            o.DateFrom = dateFrom;
            o.DateTo = dateTo.AddDays(1);

            o.Run();
            
            using (FileStream f = File.Create("CommViewDic.bin"))
            {
                Serializer.Serialize(f, o.CommissionViewDic);
            }

            using (FileStream f = File.Create("AgentViewList.bin"))
            {
                Serializer.Serialize<List<AgentView>>(f, o.AgentViewList);
            }

            Console.WriteLine("done");
            Console.ReadKey();
        }
    }
}
