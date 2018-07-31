using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Oracle.ManagedDataAccess.Client;
using System.Data;
using System.Data.OleDb;
using System.Configuration;
using System.Data.Common;

namespace WindowsServer.DataBase.Oracle
{
    public class OracleDataAccess
    {
        public static string connstr = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;
        public static OracleCommand cmd = null;
        public static OracleConnection conn = null;

        #region 设置OracleCommand对象
        /// <summary>
        /// 设置OracleCommand对象 
        /// </summary>
        /// <param name="cmd">OracleCommand对象 </param>
        /// <param name="cmdText">命令文本</param>
        /// <param name="cmdType">命令类型</param>
        /// <param name="cmdParms">参数集合</param>
        private static void SetCommand(OracleCommand cmd, string cmdText, CommandType cmdType, OracleParameter[] cmdParms)
        {
            cmd.Connection = conn;
            cmd.CommandText = cmdText;
            cmd.CommandType = cmdType;
            if (cmdParms != null)
            {
                cmd.Parameters.AddRange(cmdParms);
            }
        }
        #endregion

        #region 执行相应的sql语句，返回相应的DataSet对象
        /// <summary>
        /// 执行相应的sql语句，返回相应的DataSet对象
        /// </summary>
        /// <param name="sqlstr">sql语句</param>
        /// <returns>返回相应的DataSet对象</returns>
        public static DataSet GetDataSet(string sqlstr)
        {
            DataSet set = new DataSet();
            try
            {
                using (var connection = new OracleConnection(connstr))
                {
                    connection.Open();

                    OracleDataAdapter adp = new OracleDataAdapter(sqlstr, conn);
                    adp.Fill(set);
                }
            }
            catch (Exception e)
            {
                throw;
            }

            return set;
        }
        #endregion

        #region 执行相应的sql语句，返回相应的DataSet对象
        /// <summary>
        /// 执行相应的sql语句，返回相应的DataSet对象
        /// </summary>
        /// <param name="sqlstr">sql语句</param>
        /// <param name="tableName">表名</param>
        /// <returns>返回相应的DataSet对象</returns>
        public static DataSet GetDataSet(string sqlstr, string tableName)
        {
            var set = new DataSet();
            try
            {
                using (var connection = new OracleConnection(connstr))
                {
                    connection.Open();

                    OracleDataAdapter adp = new OracleDataAdapter(sqlstr, conn);
                    adp.Fill(set, tableName);
                }
            }
            catch (Exception e)
            {
                throw;
            }

            return set;
        }
        #endregion

        #region 执行不带参数sql语句，返回所影响的行数
        /// <summary>
        /// 执行不带参数sql语句，返回所影响的行数
        /// </summary>
        /// <param name="cmdstr">增，删，改sql语句</param>
        /// <returns>返回所影响的行数</returns>
        public static int ExecuteNonQuery(string cmdText)
        {
            int count;

            using (var connection = new OracleConnection(connstr))
            {
                connection.Open();

                try
                {
                    var transaction = connection.BeginTransaction();
                    using (var command = connection.CreateCommand())
                    {
                        command.Transaction = transaction;
                        command.CommandText = cmdText;
                        count = command.ExecuteNonQuery();
                    }

                    transaction.Commit();
                }
                catch (Exception ex)
                {
                    throw;
                }
            }

            return count;
        }
        #endregion

        #region 执行带参数sql语句或存储过程，返回所影响的行数
        /// <summary>
        /// 执行带参数sql语句或存储过程，返回所影响的行数
        /// </summary>
        /// <param name="cmdText">带参数的sql语句和存储过程名</param>
        /// <param name="cmdType">命令类型</param>
        /// <param name="cmdParms">参数集合</param>
        /// <returns>返回所影响的行数</returns>
        public static int ExecuteNonQuery(string cmdText, CommandType cmdType, OracleParameter[] cmdParms)
        {
            int count;

            using (var connection = new OracleConnection(connstr))
            {
                connection.Open();

                try
                {
                    var transaction = connection.BeginTransaction();
                    using (var command = connection.CreateCommand())
                    {
                        command.Transaction = transaction;
                        command.CommandText = cmdText;
                        command.Parameters.AddRange(cmdParms);

                        count = command.ExecuteNonQuery();
                    }

                    transaction.Commit();
                }
                catch (Exception ex)
                {
                    throw;
                }

            }

            return count;
        }
        #endregion

        public static DataTable GetDatatable(string cmdText, CommandType cmdType, OracleParameter[] cmdParms)
        {
            using (OracleConnection conn = new OracleConnection(connstr))
            {
                conn.Open();
                using (OracleCommand cmd = new OracleCommand())
                {
                    cmd.Connection = conn;
                    cmd.CommandText = cmdText;
                    cmd.Parameters.AddRange(cmdParms);
                    OracleDataAdapter ada = new OracleDataAdapter(cmd);
                    DataSet ds = new DataSet();
                    ada.Fill(ds);
                    return ds.Tables[0];
                }
            }
        }


        #region 执行不带参数sql语句，返回一个从数据源读取数据的OracleDataReader对象
        /// <summary>
        /// 执行不带参数sql语句，返回一个从数据源读取数据的OracleDataReader对象
        /// </summary>
        /// <param name="cmdstr">相应的sql语句</param>
        /// <returns>返回一个从数据源读取数据的OracleDataReader对象</returns>
        public static void ExecuteReader(string cmdText, Action<DbDataReader> handleOneRow)
        {
            using (var connection = new OracleConnection(connstr))
            {
                connection.Open();

                try
                {
                    var transaction = connection.BeginTransaction();
                    using (var command = connection.CreateCommand())
                    {
                        command.Transaction = transaction;
                        command.CommandText = cmdText;

                        using (var reader = command.ExecuteReader())
                        {
                            //while (reader.Read())
                            //{
                            handleOneRow(reader);
                            //}
                        }
                    }

                    transaction.Commit();
                }
                catch (Exception ex)
                {
                    throw;
                }
            }
        }
        #endregion

        #region 执行带参数的sql语句或存储过程，返回一个从数据源读取数据的OracleDataReader对象
        /// <summary>
        /// 执行带参数的sql语句或存储过程，返回一个从数据源读取数据的OracleDataReader对象
        /// </summary>
        /// <param name="cmdText">sql语句或存储过程名</param>
        /// <param name="cmdType">命令类型</param>
        /// <param name="cmdParms">参数集合</param>
        /// <returns>返回一个从数据源读取数据的OracleDataReader对象</returns>
        public static void ExecuteReader(string cmdText, CommandType cmdType, Action<DbDataReader> handleOneRow, OracleParameter[] cmdParms)
        {
            using (var connection = new OracleConnection(connstr))
            {
                connection.Open();

                try
                {
                    var transaction = connection.BeginTransaction();
                    using (var command = connection.CreateCommand())
                    {
                        command.Transaction = transaction;
                        command.CommandType = cmdType;
                        command.CommandText = cmdText;
                        command.Parameters.AddRange(cmdParms);

                        using (var reader = command.ExecuteReader())
                        {
                            //while (reader.Read())
                            //{
                            handleOneRow(reader);
                            //}
                        }
                    }

                    transaction.Commit();
                }
                catch (Exception ex)
                {
                    throw;
                }
            }
        }
        #endregion

        #region 执行不带参数sql语句,返回结果集首行首列的值object
        /// <summary>
        /// 执行不带参数sql语句,返回结果集首行首列的值object
        /// </summary>
        /// <param name="cmdstr">相应的sql语句</param>
        /// <returns>返回结果集首行首列的值object</returns>
        public static object ExecuteScalar(string cmdText)
        {
            object obj;
            try
            {
                using (var connection = new OracleConnection(connstr))
                {
                    connection.Open();

                    cmd = new OracleCommand(cmdText, conn);
                    obj = cmd.ExecuteScalar();
                }
            }
            catch (Exception ex)
            {
                throw;
            }
            return obj;
        }
        #endregion

        #region 执行带参数sql语句或存储过程,返回结果集首行首列的值object
        /// <summary>
        /// 执行带参数sql语句或存储过程,返回结果集首行首列的值object
        /// </summary>
        /// <param name="cmdText">sql语句或存储过程名</param>
        /// <param name="cmdType">命令类型</param>
        /// <param name="cmdParms">返回结果集首行首列的值object</param>
        /// <returns></returns>
        public static object ExecuteScalar(string cmdText, CommandType cmdType, OracleParameter[] cmdParms)
        {
            object obj;
            try
            {
                using (var connection = new OracleConnection(connstr))
                {
                    connection.Open();

                    cmd = new OracleCommand();
                    SetCommand(cmd, cmdText, cmdType, cmdParms);
                    obj = cmd.ExecuteScalar();
                }
            }
            catch (Exception ex)
            {
                throw;
            }
            return obj;
        }
        #endregion
    }
}
