using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Text;
using System.Data;
using System.Data.SqlClient;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using CommissionSystem.WebUI.Models;
using CommissionSystem.WebUI.Helpers;
using CommissionSystem.Domain.Models;
using NLog;

namespace CommissionSystem.WebUI.Areas.Commission.Models
{
    public class VoiceCommission : IDisposable
    {
        public DbHelper Db { get; set; }
        public int AgentID { get; set; }
        public DateTime DateFrom { get; set; }
        public DateTime DateTo { get; set; }
        public List<SalesParent> AgentList { get; set; }
        public Dictionary<int, List<SalesParent>> AgentDic { get; set; }
        public Dictionary<string, List<CommissionView>> CommissionViewDic { get; set; }
        public List<AgentView> AgentViewList { get; set; }

        private Regex IDDRegex { get; set; }
        private Regex STDMOBRegex { get; set; }
        private Regex STDRegex { get; set; }
        private Regex MOBRegex { get; set; }

        private static Logger Logger = LogManager.GetCurrentClassLogger();

        public VoiceCommission()
        {
            IDDRegex = new Regex(@"IDD_\d+", RegexOptions.Compiled | RegexOptions.IgnoreCase);
            STDMOBRegex = new Regex(@"STDMOB\d+", RegexOptions.Compiled | RegexOptions.IgnoreCase);
            STDRegex = new Regex(@"STD\d+", RegexOptions.Compiled | RegexOptions.IgnoreCase);
            MOBRegex = new Regex(@"MOB\d+", RegexOptions.Compiled | RegexOptions.IgnoreCase);

            Db = new DbHelper(DbHelper.GetConStr(Constants.CALLBILLING2));
        }

        public void Dispose()
        {
            if (Db != null)
                Db.Dispose();
        }

        protected CallRate GetIDDSTDMOBRate(string s)
        {
            CallRate o = new CallRate();

            try
            {
                Match idd = IDDRegex.Match(s);
                Match stdmob = STDMOBRegex.Match(s);
                Match std = STDRegex.Match(s);
                Match mob = MOBRegex.Match(s);

                if (idd.Success)
                    o.IDD = Utils.GetValue<int>(idd.Value);

                if (stdmob.Success)
                {
                    o.STD = Utils.GetValue<int>(stdmob.Value);
                    o.MOB = o.STD;
                }

                if (std.Success)
                    o.STD = Utils.GetValue<int>(std.Value);

                if (mob.Success)
                    o.MOB = Utils.GetValue<int>(mob.Value);
            }

            catch (Exception e)
            {
                Logger.Debug("", e);
                throw e;
            }

            return o;
        }

        protected bool IsCustomerExist(int magentid, int custid)
        {
            bool a = false;
            SqlDataReader rd = null;

            try
            {
                StringBuilder sb = new StringBuilder();
                sb.Append("select top 1 custid from salesforcedetail where magentid = @magentid and custid = @custid");
                string q = sb.ToString();

                SqlParameter p = new SqlParameter("@magentid", magentid);
                p.Value = magentid;
                Db.AddParameter(p);

                p = new SqlParameter("@custid", SqlDbType.Int);
                p.Value = custid;
                Db.AddParameter(p);

                rd = Db.ExecuteReader(q, CommandType.Text);
                if (rd.Read())
                    a = true;

                rd.Close();
            }

            catch (Exception e)
            {
                Logger.Debug("", e);
                throw e;
            }

            finally
            {
                if (rd != null)
                    rd.Dispose();
            }

            return a;
        }
    }
}