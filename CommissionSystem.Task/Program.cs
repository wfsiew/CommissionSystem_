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

            ProcessData(dateFrom, dateTo);
            ProcessDCS(dateFrom, dateTo);
            ProcessSIP(dateFrom, dateTo);
            ProcessE1(dateFrom, dateTo);

            Console.ReadKey();
        }

        private static void ProcessData(DateTime dateFrom, DateTime dateTo)
        {
            DataTask o = new DataTask();

            o.DateFrom = dateFrom;
            o.DateTo = dateTo;

            Action a = new Action(o.Run);
            AsyncCallback cb = new AsyncCallback(DataCompleteCallback);
            Logger.Trace("Data process started: {0}", DateTime.Now);
            IAsyncResult ar = a.BeginInvoke(cb, o);
            ar.AsyncWaitHandle.WaitOne();
        }

        private static void ProcessDCS(DateTime dateFrom, DateTime dateTo)
        {
            DiscountedCallServiceTask o = new DiscountedCallServiceTask();

            o.DateFrom = dateFrom;
            o.DateTo = dateTo;

            Action a = new Action(o.Run);
            AsyncCallback cb = new AsyncCallback(DCSCompleteCallback);
            Logger.Trace("DCS process started: {0}", DateTime.Now);
            IAsyncResult ar = a.BeginInvoke(cb, o);
            ar.AsyncWaitHandle.WaitOne();
        }

        private static void ProcessSIP(DateTime dateFrom, DateTime dateTo)
        {
            SIPTask o = new SIPTask();

            o.DateFrom = dateFrom;
            o.DateTo = dateTo;

            Action a = new Action(o.Run);
            AsyncCallback cb = new AsyncCallback(SIPCompleteCallback);
            Logger.Trace("SIP process started: {0}", DateTime.Now);
            IAsyncResult ar = a.BeginInvoke(cb, o);
            ar.AsyncWaitHandle.WaitOne();
        }

        private static void ProcessE1(DateTime dateFrom, DateTime dateTo)
        {
            E1Task o = new E1Task();

            o.DateFrom = dateFrom;
            o.DateTo = dateTo;

            Action a = new Action(o.Run);
            AsyncCallback cb = new AsyncCallback(E1CompleteCallback);
            Logger.Trace("E1 process started: {0}", DateTime.Now);
            IAsyncResult ar = a.BeginInvoke(cb, o);
            ar.AsyncWaitHandle.WaitOne();
        }

        private static void DataCompleteCallback(IAsyncResult ar)
        {
            FileStream fs = null;
            DataTask o = null;

            try
            {
                Action x = (Action)((AsyncResult)ar).AsyncDelegate;
                x.EndInvoke(ar);

                o = (DataTask)ar.AsyncState;
                ar.AsyncWaitHandle.Close();

                string path = "../result/data";
                CreateDir(path);

                DateTime dt = o.DateFrom;
                string year = Path.Combine(path, dt.Year.ToString());
                CreateDir(year);

                string month = Path.Combine(year, string.Format("{0:MM}", dt));
                CreateDir(month);

                string file = Path.Combine(month, "CommResult_.bin");

                CommissionResult re = new CommissionResult();
                re.CommissionViewDic = o.CommissionViewDic;
                re.AgentViewList = o.AgentViewList;

                fs = new FileStream(file, FileMode.Create, FileAccess.Write, FileShare.Read);
                Serializer.Serialize<CommissionResult>(fs, re);
                fs.Close();

                Logger.Trace("DCata process ended: {0}", DateTime.Now);
            }

            catch (Exception e)
            {
                Logger.Debug("", e);
            }

            finally
            {
                if (fs != null)
                    fs.Dispose();

                if (o != null)
                    o.Dispose();
            }

            Console.WriteLine("done data");
        }

        private static void DCSCompleteCallback(IAsyncResult ar)
        {
            FileStream fs = null;
            DiscountedCallServiceTask o = null;

            try
            {
                Action x = (Action)((AsyncResult)ar).AsyncDelegate;
                x.EndInvoke(ar);

                o = (DiscountedCallServiceTask)ar.AsyncState;
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

                string file = Path.Combine(month, "CommResult_.bin");

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

                if (o != null)
                    o.Dispose();
            }

            Console.WriteLine("done dcs");
        }

        private static void SIPCompleteCallback(IAsyncResult ar)
        {
            FileStream fs = null;
            SIPTask o = null;

            try
            {
                Action x = (Action)((AsyncResult)ar).AsyncDelegate;
                x.EndInvoke(ar);

                o = (SIPTask)ar.AsyncState;
                ar.AsyncWaitHandle.Close();

                string path = "../result/voice";
                CreateDir(path);

                string dcspath = Path.Combine(path, "sip");
                CreateDir(dcspath);

                DateTime dt = o.DateFrom;
                string year = Path.Combine(dcspath, dt.Year.ToString());
                CreateDir(year);

                string month = Path.Combine(year, string.Format("{0:MM}", dt));
                CreateDir(month);

                string file = Path.Combine(month, "CommResult_.bin");

                VoiceCommissionResult re = new VoiceCommissionResult();
                re.CommissionViewDic = o.CommissionViewDic;
                re.AgentViewList = o.AgentViewList;

                fs = new FileStream(file, FileMode.Create, FileAccess.Write, FileShare.Read);
                Serializer.Serialize<VoiceCommissionResult>(fs, re);
                fs.Close();

                Logger.Trace("SIP process ended: {0}", DateTime.Now);
            }

            catch (Exception e)
            {
                Logger.Debug("", e);
            }

            finally
            {
                if (fs != null)
                    fs.Dispose();

                if (o != null)
                    o.Dispose();
            }

            Console.WriteLine("done sip");
        }

        private static void E1CompleteCallback(IAsyncResult ar)
        {
            FileStream fs = null;
            E1Task o = null;

            try
            {
                Action x = (Action)((AsyncResult)ar).AsyncDelegate;
                x.EndInvoke(ar);

                o = (E1Task)ar.AsyncState;
                ar.AsyncWaitHandle.Close();

                string path = "../result/voice";
                CreateDir(path);

                string dcspath = Path.Combine(path, "e1");
                CreateDir(dcspath);

                DateTime dt = o.DateFrom;
                string year = Path.Combine(dcspath, dt.Year.ToString());
                CreateDir(year);

                string month = Path.Combine(year, string.Format("{0:MM}", dt));
                CreateDir(month);

                string file = Path.Combine(month, "CommResult_.bin");

                VoiceCommissionResult re = new VoiceCommissionResult();
                re.CommissionViewDic = o.CommissionViewDic;
                re.AgentViewList = o.AgentViewList;

                fs = new FileStream(file, FileMode.Create, FileAccess.Write, FileShare.Read);
                Serializer.Serialize<VoiceCommissionResult>(fs, re);
                fs.Close();

                Logger.Trace("E1 process ended: {0}", DateTime.Now);
            }

            catch (Exception e)
            {
                Logger.Debug("", e);
            }

            finally
            {
                if (fs != null)
                    fs.Dispose();

                if (o != null)
                    o.Dispose();
            }

            Console.WriteLine("done e1");
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
