using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Runtime.Remoting.Messaging;
using CommissionSystem.Task.Models;
using CommissionSystem.Domain.ProtoBufModels;
using CommissionSystem.Domain.Helpers;
using ProtoBuf;
using NLog;

namespace CommissionSystem.Task
{
    class Program
    {
        private static Logger Logger = LogManager.GetCurrentClassLogger();

        private delegate void DCSDelegate(); 

        static void Main(string[] args)
        {
            SettingFactory sf = SettingFactory.Instance;
            DateTime dt = DateTime.Now.AddMonths(-1);
            DateTime dateFrom = new DateTime(dt.Year, dt.Month, 1);
            DateTime dateTo = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);

            if (args != null && args.Length >= 2)
            {
                dt = new DateTime(Utils.GetValue<int>(args[0]), Utils.GetValue<int>(args[1]), 1);
                DateTime x = dt.AddMonths(1);
                dateFrom = new DateTime(dt.Year, dt.Month, 1);
                dateTo = new DateTime(x.Year, x.Month, 1);
            }

            ProcessDCS(dateFrom, dateTo);

            Console.ReadKey();
        }

        private static void ProcessDCS(DateTime dateFrom, DateTime dateTo)
        {
            DiscountedCallServiceTask o = new DiscountedCallServiceTask();

            o.DateFrom = dateFrom;
            o.DateTo = dateTo;

            Action a = new Action(o.Run);
            AsyncCallback cbdcs = new AsyncCallback(DCSCompleteCallback);
            Logger.Trace("DCS process started: {0}", DateTime.Now);
            IAsyncResult ar = a.BeginInvoke(cbdcs, o);
            ar.AsyncWaitHandle.WaitOne();
        }

        private static void DCSCompleteCallback(IAsyncResult ar)
        {
            FileStream fs = null;

            try
            {
                Action X = (Action)((AsyncResult)ar).AsyncDelegate;
                X.EndInvoke(ar);

                DiscountedCallServiceTask o = (DiscountedCallServiceTask)ar.AsyncState;
                ar.AsyncWaitHandle.Close();

                string path = "../result/voice";
                CreateDir(path);

                string dcspath = Path.Combine(path, "dcs");
                CreateDir(dcspath);

                DateTime dt = o.DateFrom;
                string year = Path.Combine(dcspath, dt.Year.ToString());
                CreateDir(year);

                string month = Path.Combine(year, string.Format("{0:MM}", dt));
                CreateDir(month);

                string file = Path.Combine(month, "CommResult.bin");

                VoiceCommissionResult re = new VoiceCommissionResult();
                re.CommissionViewDic = o.CommissionViewDic;
                re.AgentViewList = o.AgentViewList;

                fs = new FileStream(file, FileMode.Create, FileAccess.Write, FileShare.Read);
                Serializer.Serialize<VoiceCommissionResult>(fs, re);
                fs.Close();

                Logger.Trace("DCS process ended: {0}", DateTime.Now);
            }
            
            catch (Exception e)
            {
                Logger.Debug("", e);
            }

            finally
            {
                if (fs != null)
                    fs.Dispose();
            }

            Console.WriteLine("done");
        }

        private static void CreateDir(string path)
        {
            try
            {
                if (!Directory.Exists(path))
                    Directory.CreateDirectory(path);
            }

            catch (Exception e)
            {
                Logger.Debug("", e);
                throw e;
            }
        }
    }
}
