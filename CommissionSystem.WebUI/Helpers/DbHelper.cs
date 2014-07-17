using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.SqlClient;
using NLog;
using System.Data;

namespace CommissionSystem.WebUI.Helpers
{
    public class DbHelper
    {
        public string ConnectionString { get; set; }
        public SqlConnection Connection { get; private set; }
        public SqlCommand Command { get; private set; }

        private static Logger Logger = LogManager.GetCurrentClassLogger();

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
    }
}