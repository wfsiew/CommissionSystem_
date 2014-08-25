using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using System.Data.SqlClient;
using CommissionSystem.Domain.Helpers;
using NLog;

namespace CommissionSystem.WebUI.Helpers
{
    public class DbHelper : IDisposable
    {
        public string ConnectionString { get; set; }
        public SqlConnection Connection { get; private set; }
        public SqlCommand Command { get; private set; }

        private static Logger Logger = LogManager.GetCurrentClassLogger();

        public DbHelper()
        {
        }

        public DbHelper(string c)
        {
            ConnectionString = c;
            OpenConnection();
            CreateCommand();
        }

        public void OpenConnection()
        {
            try
            {
                Connection = new SqlConnection(ConnectionString);
                Connection.Open();
            }

            catch (Exception e)
            {
                Logger.Debug("", e);
                throw e;
            }
        }

        public void CloseConnection()
        {
            try
            {
                Connection.Close();
            }

            catch (Exception)
            {
            }
        }

        public void CreateCommand()
        {
            try
            {
                Command = new SqlCommand();
                Command.Connection = Connection;
            }

            catch (Exception e)
            {
                Logger.Debug("", e);
                throw e;
            }
        }

        public void AddParameter(SqlParameter p)
        {
            try
            {
                Command.Parameters.Add(p);
            }

            catch (Exception e)
            {
                Logger.Debug("", e);
                throw e;
            }
        }

        public void BeginTransaction()
        {
            try
            {
                Command.Transaction = Connection.BeginTransaction();
            }
            
            catch (Exception e)
            {
                Logger.Debug("", e);
                throw e;
            }
        }

        public void CommitTransaction()
        {
            try
            {
                Command.Transaction.Commit();
            }

            catch (Exception e)
            {
                Logger.Debug("", e);
                throw e;
            }
        }

        public void RollbackTransaction()
        {
            try
            {
                Command.Transaction.Rollback();
            }

            catch (Exception e)
            {
                Logger.Debug("", e);
                throw e;
            }
        }

        public int ExecuteNonQuery(string q, CommandType cmdType)
        {
            Command.CommandText = q;
            Command.CommandType = cmdType;
            int i = -1;

            try
            {
                i = Command.ExecuteNonQuery();
            }

            catch (Exception e)
            {
                Logger.Debug("", e);
                throw e;
            }

            finally
            {
                Command.Parameters.Clear();
            }

            return i;
        }

        public object ExecuteScalar(string q, CommandType cmdType)
        {
            Command.CommandText = q;
            Command.CommandType = cmdType;
            object o = null;

            try
            {
                o = Command.ExecuteScalar();
            }

            catch (Exception e)
            {
                Logger.Debug("", e);
                throw e;
            }

            finally
            {
                Command.Parameters.Clear();
            }

            return o;
        }

        public SqlDataReader ExecuteReader(string q, CommandType cmdType)
        {
            Command.CommandText = q;
            Command.CommandType = cmdType;
            SqlDataReader rd = null;

            try
            {
                rd = Command.ExecuteReader();
            }

            catch (Exception e)
            {
                Logger.Debug("", e);
                throw e;
            }

            finally
            {
                Command.Parameters.Clear();
            }

            return rd;
        }

        public DataSet GetDataSet(string q, CommandType cmdType)
        {
            Command.CommandText = q;
            Command.CommandType = cmdType;
            SqlDataAdapter da = new SqlDataAdapter();
            da.SelectCommand = Command;
            DataSet ds = new DataSet();

            try
            {
                da.Fill(ds);
            }

            catch (Exception e)
            {
                Logger.Debug("", e);
                throw e;
            }

            finally
            {
                Command.Parameters.Clear();
            }

            return ds;
        }

        public static string GetConStr(string db)
        {
            string constr = string.Format("Server=192.168.138.120; Database={0}; User Id=CallBilling; Password=CBPWD12345", db);
            //constr = string.Format("Server=wfsiew-pc; Database={0}; User Id=sa; Password=root", db);
            return constr;
        }

        public void Dispose()
        {
            if (Command != null)
                Command.Dispose();

            if (Connection != null)
            {
                CloseConnection();
                Connection.Dispose();
            }   
        }
    }

    public static class Extensions
    {
        public static T Get<T>(this SqlDataReader rd, string c) where T : struct
        {
            object o = rd[c];
            T k = default(T);

            if (o != DBNull.Value)
                k = Utils.GetValue<T>(o.ToString());

            return k;
        }

        public static string Get(this SqlDataReader rd, string c, string v = null)
        {
            object o = rd[c];
            string a = null;

            if (o != DBNull.Value)
                a = Utils.GetValue(o.ToString(), v);

            return a;
        }

        public static DateTime GetDateTime(this SqlDataReader rd, string c)
        {
            object o = rd[c];
            DateTime k = default(DateTime);

            if (o != DBNull.Value)
                k = Utils.GetDateTime(o.ToString());

            return k;
        }

        public static Nullable<T> GetNullable<T>(this SqlDataReader rd, string c) where T : struct
        {
            object o = rd[c];
            T? k = null;

            if (o != DBNull.Value)
                k = Utils.GetNullableValue<T>(o.ToString(), null);

            return k;
        }
    }
}