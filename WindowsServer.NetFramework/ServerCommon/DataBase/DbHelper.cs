using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.SqlClient;
using NLog;
using System.Xml;
using System.IO;
using System.Data.Common;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Globalization;

namespace WindowsServer.DataBase
{
    public static class DbHelper
    {
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

        public static readonly DateTime InvalidDateTime = new DateTime(1900, 1, 1);

        private static readonly DbProviderFactory _factory = DbProviderFactories.GetFactory("System.Data.SqlClient");

        private class DBLogContext
        {
            public Stopwatch Stopwatch { get; set; }
            public Guid ClientConnectionId { get; set; }
            public string Name { get; set; }
            public string CommandText { get; set; }
            public string ParametersString { get; set; }
        }

        public static DbProviderFactory Factory
        {
            get
            {
                return _factory;
            }
        }

        public static string CaseInsensitiveCollate
        {
            get
            {
                return "latin1_general_ci_as";
            }
        }

        public static string CaseSensitiveCollate
        {
            get
            {
                return "latin1_general_cs_as";
            }
        }

        public static string ChineseCaseInsensitiveCollate
        {
            get
            {
                return "Chinese_PRC_ci_as";
            }
        }

        public static string ChineseCaseSensitiveCollate
        {
            get
            {
                return "Chinese_PRC_cs_as";
            }
        }

        private static string BuildParametersString(DbParameter[] parameters)
        {
            int count = (parameters == null) ? 0 : parameters.Length;
            StringBuilder sb = new StringBuilder();
            sb.Append("Count:");
            sb.Append(count);
            if (count != 0)
            {
                foreach(var parameter in parameters)
                {
                    sb.Append(" ");
                    sb.Append(parameter.ParameterName);
                    sb.Append('=');
                    sb.Append((parameter.Value == null) ? "null" : parameter.Value.ToString());
                }
            }
            return sb.ToString();
        }

        private static DBLogContext LogBegin(string name, DbConnection connection, string commandText, DbParameter[] parameters)
        {
            var logContext = new DBLogContext()
            {
                Stopwatch = new Stopwatch(),
                ClientConnectionId = (connection is SqlConnection) ? ((SqlConnection)connection).ClientConnectionId : Guid.Empty,
                Name = name,
                CommandText = commandText,
                ParametersString = BuildParametersString(parameters),
            };
            logContext.Stopwatch.Start();

            _logger.Debug("DB Start [" + logContext.Name + "][" + logContext.ClientConnectionId + "] " + logContext.CommandText + "[Parameters:" + logContext.ParametersString + "]");

            return logContext;
        }

        private static void LogEnd(DBLogContext logContext)
        {
            logContext.Stopwatch.Stop();
            _logger.Debug("DB Stop [" + logContext.Name + "][" + logContext.ClientConnectionId + "][" + logContext.Stopwatch.ElapsedMilliseconds + "ms] " + logContext.CommandText + "[Parameters:" + logContext.ParametersString + "]");
        }

        public static bool TableExists(string connectionString, string tableName)
        {
            object resultObject = DbHelper.ExecuteScalar(connectionString, @"select OBJECT_ID(N'" + tableName + "', N'U')", CommandType.Text);
            bool exists = !(resultObject is DBNull);
            return exists;
        }

        public static bool StoredProcedureExists(string connectionString, string storedProcedureName)
        {
            object resultObject = DbHelper.ExecuteScalar(connectionString, @"select OBJECT_ID(N'" + storedProcedureName + "', N'U')", CommandType.Text);
            bool exists = !(resultObject is DBNull);
            return exists;
        }

        public static long ExecuteTransaction(string connectionString, Func<DbCommand, long> func)
        {
            using (var connection = _factory.CreateConnection())
            {
                connection.ConnectionString = connectionString;
                connection.Open();

                var logContext = LogBegin("ExecuteTransaction", connection, string.Empty, null);

                try
                {
                    var transaction = connection.BeginTransaction();
                    using (var command = connection.CreateCommand())
                    {
                        command.Transaction = transaction;
                        try
                        {
                            long result = func(command);
                            transaction.Commit();
                            return result;
                        }
                        catch (Exception ex)
                        {
                            _logger.ErrorException("Failed to commit the trancation.", ex);
                            transaction.Rollback();
                            throw;
                        }
                    }
                }
                finally
                {
                    LogEnd(logContext);
                }
            }
        }

        public static int ExecuteNonQuery(string connectionString, string cmdText, params DbParameter[] parameters)
        {
            return ExecuteNonQuery(connectionString, cmdText, CommandType.Text, parameters);
        }

        public static int ExecuteNonQuery(string connectionString, string cmdText, CommandType commandType, params DbParameter[] parameters)
        {
            using (var connection = _factory.CreateConnection())
            {
                connection.ConnectionString = connectionString;
                connection.Open();

                var logContext = LogBegin("ExecuteNonQuery", connection, cmdText, parameters);

                try
                {
                    using (var command = _factory.CreateCommand())
                    {
                        command.Connection = connection;
                        command.CommandText = cmdText;
                        command.CommandType = commandType;
                        command.Parameters.AddRange(parameters);
                        return command.ExecuteNonQuery();
                    }
                }
                finally
                {
                    LogEnd(logContext);
                }
            }
        }

        public static int ExecuteNonQuery(string connectionString, string cmdText, CommandType commandType, out int result, params DbParameter[] parameters)
        {
            using (var connection = _factory.CreateConnection())
            {
                connection.ConnectionString = connectionString;
                connection.Open();

                var logContext = LogBegin("ExecuteNonQuery", connection, cmdText, parameters);

                try
                {
                    using (var command = _factory.CreateCommand())
                    {
                        command.Connection = connection;
                        command.CommandText = cmdText;
                        command.CommandType = commandType;
                        command.Parameters.AddRange(parameters);

                        var resultValue = _factory.CreateParameter();
                        resultValue.ParameterName = "@return";
                        resultValue.DbType = DbType.Int32;
                        resultValue.Direction = ParameterDirection.ReturnValue;
                        command.Parameters.Add(resultValue);

                        int numRowsAffected = command.ExecuteNonQuery();
                        result = Convert.ToInt32(resultValue.Value);

                        return numRowsAffected;
                    }
                }
                finally
                {
                    LogEnd(logContext);
                }
            }
        }

        public static async Task<int> ExecuteNonQueryAsync(string connectionString, string cmdText, params DbParameter[] parameters)
        {
            return await ExecuteNonQueryAsync(connectionString, cmdText, CommandType.Text, parameters);
        }

        public static async Task<int> ExecuteNonQueryAsync(string connectionString, string cmdText, CommandType commandType, params DbParameter[] parameters)
        {
            using (var connection = _factory.CreateConnection())
            {
                connection.ConnectionString = connectionString;
                await connection.OpenAsync();

                var logContext = LogBegin("ExecuteNonQueryAsync", connection, cmdText, parameters);

                try
                {
                    using (var command = _factory.CreateCommand())
                    {
                        command.Connection = connection;
                        command.CommandText = cmdText;
                        command.CommandType = commandType;
                        command.Parameters.AddRange(parameters);
                        return await command.ExecuteNonQueryAsync();
                    }
                }
                finally
                {
                    LogEnd(logContext);
                }
            }
        }

        public static int ExecuteIntScalar(string connectionString, string cmdText, CommandType commandType, int defaultValue, params DbParameter[] parameters)
        {
            object o = ExecuteScalar(connectionString, cmdText, commandType, defaultValue, parameters);
            if (o is Int64)
            {
                return (int)(long)o;
            }
            return (int)o;
        }

        public static int ExecuteIntScalar(string connectionString, string cmdText, CommandType commandType, params DbParameter[] parameters)
        {
            object o = ExecuteScalar(connectionString, cmdText, commandType, parameters);
            if (o is Int64)
            {
                return (int)(long)o;
            }
            return (int)o;
        }

        public static int ExecuteIntScalar(string connectionString, string cmdText, params DbParameter[] parameters)
        {
            object o = ExecuteScalar(connectionString, cmdText, CommandType.Text, parameters);
            if (o is Int64)
            {
                return (int)(long)o;
            }
            return (int)o;
        }

        public static object ExecuteScalar(string connectionString, string cmdText, CommandType commandType, object defaultValue, params DbParameter[] parameters)
        {
            object o = ExecuteScalar(connectionString, cmdText, commandType, parameters);
            return (o is DBNull) ? defaultValue : o;
        }

        public static object ExecuteScalar(string connectionString, string cmdText, CommandType commandType, params DbParameter[] parameters)
        {
            using (var connection = _factory.CreateConnection())
            {
                connection.ConnectionString = connectionString;
                connection.Open();

                var logContext = LogBegin("ExecuteScalar", connection, cmdText, parameters);

                try
                {
                    using (var command = _factory.CreateCommand())
                    {
                        command.CommandText = cmdText;
                        command.Connection = connection;
                        command.CommandType = commandType;
                        command.Parameters.AddRange(parameters);
                        return command.ExecuteScalar();
                    }
                }
                finally
                {
                    LogEnd(logContext);
                }
            }
        }


        public static async Task<int> ExecuteIntScalarAsync(string connectionString, string cmdText, CommandType commandType, int defaultValue, params DbParameter[] parameters)
        {
            object o = await ExecuteScalarAsync(connectionString, cmdText, commandType, defaultValue, parameters);
            if (o is Int64)
            {
                return (int)(long)o;
            }
            return (int)o;
        }

        public static async Task<int> ExecuteIntScalarAsync(string connectionString, string cmdText, CommandType commandType, params DbParameter[] parameters)
        {
            object o = await ExecuteScalarAsync(connectionString, cmdText, commandType, parameters);
            if (o is Int64)
            {
                return (int)(long)o;
            }
            return (int)o;
        }

        public static async Task<int> ExecuteIntScalarAsync(string connectionString, string cmdText, params DbParameter[] parameters)
        {
            object o = await ExecuteScalarAsync(connectionString, cmdText, CommandType.Text, parameters);
            if (o is Int64)
            {
                return (int)(long)o;
            }
            return (int)o;
        }

        public static async Task<object> ExecuteScalarAsync(string connectionString, string cmdText, CommandType commandType, object defaultValue, params DbParameter[] parameters)
        {
            object o = await ExecuteScalarAsync(connectionString, cmdText, commandType, parameters);
            return (o is DBNull) ? defaultValue : o;
        }

        public static async Task<object> ExecuteScalarAsync(string connectionString, string cmdText, CommandType commandType, params DbParameter[] parameters)
        {
            using (var connection = _factory.CreateConnection())
            {
                connection.ConnectionString = connectionString;
                connection.Open();

                var logContext = LogBegin("ExecuteScalarAsync", connection, cmdText, parameters);

                try
                {
                    using (var command = _factory.CreateCommand())
                    {
                        command.CommandText = cmdText;
                        command.Connection = connection;
                        command.CommandType = commandType;
                        command.Parameters.AddRange(parameters);
                        return await command.ExecuteScalarAsync();
                    }
                }
                finally
                {
                    LogEnd(logContext);
                }
            }
        }

        public static void ExecuteReader(string connectionString, string commandText, DbExecutionParameters executionParameters, Action<DbDataReader> handleOneRow, params DbParameter[] parameters)
        {
            ExecuteReader(connectionString, false, commandText, CommandType.Text, executionParameters, handleOneRow, parameters);
        }

        public static void ExecuteReader(string connectionString, string commandText, Action<DbDataReader> handleOneRow, params DbParameter[] parameters)
        {
            ExecuteReader(connectionString, false, commandText, CommandType.Text, null, handleOneRow, parameters);
        }

        public static void ExecuteReader(string connectionString, string commandText, CommandType commandType, Action<DbDataReader> handleOneRow, params DbParameter[] parameters)
        {
            ExecuteReader(connectionString, false, commandText, commandType, null, handleOneRow, parameters);
        }

        public static void ExecuteReaderSingleRow(string connectionString, string commandText, Action<DbDataReader> handleOneRow, params DbParameter[] parameters)
        {
            ExecuteReader(connectionString, true, commandText, CommandType.Text, null, handleOneRow, parameters);
        }

        public static void ExecuteReaderSingleRow(string connectionString, string commandText, CommandType commandType, Action<DbDataReader> handleOneRow, params DbParameter[] parameters)
        {
            ExecuteReader(connectionString, true, commandText, commandType, null, handleOneRow, parameters);
        }

        public static void ExecuteReader(string connectionString, bool singleRow, string commandText, CommandType commandType, Action<DbDataReader> handleOneRow, params DbParameter[] parameters)
        {
            ExecuteReader(connectionString, singleRow, commandText, commandType, null, handleOneRow, parameters);
        }

        public static void ExecuteReader(string connectionString, bool singleRow, string commandText, CommandType commandType, DbExecutionParameters executionParameters, Action<DbDataReader> handleOneRow, params DbParameter[] parameters)
        {
            using (var connection = _factory.CreateConnection())
            {
                if (executionParameters != null && executionParameters.ConnectionTimeout.HasValue)
                {
                    var connectionStringBuilder = _factory.CreateConnectionStringBuilder();
                    connectionStringBuilder.ConnectionString = connectionString;
                    if (connectionStringBuilder is SqlConnectionStringBuilder)
                    {
                        ((SqlConnectionStringBuilder)connectionStringBuilder).ConnectTimeout = executionParameters.ConnectionTimeout.Value;
                    }
                    else
                    {
                        throw new Exception("Do not support setting ConnectionTimeout on " + connectionStringBuilder.GetType().ToString());
                    }
                    connectionString = connectionStringBuilder.ToString();
                }
                connection.ConnectionString = connectionString;
                connection.Open();

                var logContext = LogBegin("ExecuteReader", connection, commandText, parameters);

                try
                {
                    using (var cmd = _factory.CreateCommand())
                    {
                        cmd.Connection = connection;
                        cmd.CommandText = commandText;
                        cmd.CommandType = commandType;
                        if (executionParameters != null && executionParameters.CommandTimeout.HasValue)
                        {
                            cmd.CommandTimeout = executionParameters.CommandTimeout.Value;
                        }

                        cmd.Parameters.AddRange(parameters);

                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                handleOneRow(reader);

                                if (singleRow)
                                {
                                    break;
                                }
                            }
                        }
                    }
                }
                finally
                {
                    LogEnd(logContext);
                }
            }
        }

        public static async Task ExecuteReaderAsync(string connectionString, string commandText, DbExecutionParameters executionParameters, Action<DbDataReader> handleOneRow, params DbParameter[] parameters)
        {
            await ExecuteReaderAsync(connectionString, false, commandText, CommandType.Text, executionParameters, handleOneRow, parameters);
        }

        public static async Task ExecuteReaderAsync(string connectionString, string commandText, Action<DbDataReader> handleOneRow, params DbParameter[] parameters)
        {
            await ExecuteReaderAsync(connectionString, false, commandText, CommandType.Text, null, handleOneRow, parameters);
        }

        public static async Task ExecuteReaderAsync(string connectionString, string commandText, CommandType commandType, Action<DbDataReader> handleOneRow, params DbParameter[] parameters)
        {
            await ExecuteReaderAsync(connectionString, false, commandText, commandType, null, handleOneRow, parameters);
        }

        public static async Task ExecuteReaderSingleRowAsync(string connectionString, string commandText, Action<DbDataReader> handleOneRow, params DbParameter[] parameters)
        {
            await ExecuteReaderAsync(connectionString, true, commandText, CommandType.Text, null, handleOneRow, parameters);
        }

        public static async Task ExecuteReaderSingleRowAsync(string connectionString, string commandText, CommandType commandType, Action<DbDataReader> handleOneRow, params DbParameter[] parameters)
        {
            await ExecuteReaderAsync(connectionString, true, commandText, commandType, null, handleOneRow, parameters);
        }

        public static async Task ExecuteReaderAsync(string connectionString, bool singleRow, string commandText, CommandType commandType, Action<DbDataReader> handleOneRow, params DbParameter[] parameters)
        {
            await ExecuteReaderAsync(connectionString, singleRow, commandText, commandType, null, handleOneRow, parameters);
        }

        public static async Task ExecuteReaderAsync(string connectionString, bool singleRow, string commandText, CommandType commandType, DbExecutionParameters executionParameters, Action<DbDataReader> handleOneRow, params DbParameter[] parameters)
        {
            using (var connection = _factory.CreateConnection())
            {
                connection.ConnectionString = connectionString;
                await connection.OpenAsync();

                var logContext = LogBegin("ExecuteReaderAsync", connection, commandText, parameters);

                try
                {
                    using (var cmd = _factory.CreateCommand())
                    {
                        cmd.Connection = connection;
                        cmd.CommandText = commandText;
                        cmd.CommandType = commandType;
                        if (executionParameters != null && executionParameters.CommandTimeout.HasValue)
                        {
                            cmd.CommandTimeout = executionParameters.CommandTimeout.Value;
                        }

                        cmd.Parameters.AddRange(parameters);

                        using (var reader = await cmd.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                handleOneRow(reader);

                                if (singleRow)
                                {
                                    break;
                                }
                            }
                        }
                    }
                }
                finally
                {
                    LogEnd(logContext);
                }
            }
        }

        public static void ExecuteInterruptibleReader(string connectionString, bool singleRow, string commandText, CommandType commandType, Func<DbDataReader, bool> handleOneRow, params DbParameter[] parameters)
        {
            ExecuteInterruptibleReader(connectionString, singleRow, commandText, commandType, null, handleOneRow, parameters);
        }

        public static void ExecuteInterruptibleReader(string connectionString, bool singleRow, string commandText, CommandType commandType, DbExecutionParameters executionParameters, Func<DbDataReader, bool> handleOneRow, params DbParameter[] parameters)
        {
            using (var connection = _factory.CreateConnection())
            {
                connection.ConnectionString = connectionString;
                connection.Open();

                var logContext = LogBegin("ExecuteInterruptibleReader", connection, commandText, parameters);

                try
                {
                    using (var cmd = _factory.CreateCommand())
                    {
                        cmd.Connection = connection;
                        cmd.CommandText = commandText;
                        cmd.CommandType = commandType;
                        if (executionParameters != null && executionParameters.CommandTimeout.HasValue)
                        {
                            cmd.CommandTimeout = executionParameters.CommandTimeout.Value;
                        }

                        cmd.Parameters.AddRange(parameters);

                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                if (handleOneRow(reader))
                                {
                                    break;
                                }

                                if (singleRow)
                                {
                                    break;
                                }
                            }
                        }
                    }
                }
                finally
                {
                    LogEnd(logContext);
                }
            }
        }

        public async static Task ExecuteInterruptibleReaderAsync(string connectionString, bool singleRow, string commandText, CommandType commandType, Func<DbDataReader, Task<bool>> handleOneRow, params DbParameter[] parameters)
        {
            await ExecuteInterruptibleReaderAsync(connectionString, singleRow, commandText, commandType, null, handleOneRow, parameters);
        }

        public async static Task ExecuteInterruptibleReaderAsync(string connectionString, bool singleRow, string commandText, CommandType commandType, DbExecutionParameters executionParameters, Func<DbDataReader, Task<bool>> handleOneRow, params DbParameter[] parameters)
        {
            using (var connection = _factory.CreateConnection())
            {
                connection.ConnectionString = connectionString;
                connection.Open();

                var logContext = LogBegin("ExecuteInterruptibleReaderAsync", connection, commandText, parameters);

                try
                {
                    using (var cmd = _factory.CreateCommand())
                    {
                        cmd.Connection = connection;
                        cmd.CommandText = commandText;
                        cmd.CommandType = commandType;
                        if (executionParameters != null && executionParameters.CommandTimeout.HasValue)
                        {
                            cmd.CommandTimeout = executionParameters.CommandTimeout.Value;
                        }

                        cmd.Parameters.AddRange(parameters);

                        using (var reader = await cmd.ExecuteReaderAsync())
                        {
                            while (reader.Read())
                            {
                                if (await handleOneRow(reader))
                                {
                                    break;
                                }

                                if (singleRow)
                                {
                                    break;
                                }
                            }
                        }
                    }
                }
                finally
                {
                    LogEnd(logContext);
                }
            }
        }

        public static DbParameter CreateParameter(string name, DbType type, object value)
        {
            var parameter = _factory.CreateParameter();
            parameter.ParameterName = name;
            parameter.DbType = type;
            parameter.Value = value;
            return new SqlParameter(name, type) { SqlValue = value };
        }

        public static SqlParameter CreateGuidParameter(string name, Guid value)
        {
            return new SqlParameter(name, SqlDbType.UniqueIdentifier) { SqlValue = value };
        }

        public static SqlParameter CreateIntParameter(string name, int value)
        {
            return new SqlParameter(name, SqlDbType.Int) { SqlValue = value };
        }

        public static SqlParameter CreateBigIntParameter(string name, long value)
        {
            return new SqlParameter(name, SqlDbType.BigInt) { SqlValue = value };
        }

        public static SqlParameter CreateDateTimeParameter(string name, DateTime value)
        {
            return new SqlParameter(name, SqlDbType.DateTime) { SqlValue = value };
        }

        public static SqlParameter CreateNVarCharParameter(string name, string value)
        {
            return new SqlParameter(name, SqlDbType.NVarChar) { SqlValue = value };
        }

        public static SqlParameter CreateBitParameter(string name, bool value)
        {
            return new SqlParameter(name, SqlDbType.Bit) { SqlValue = value };
        }

        public static SqlParameter CreateVarBinaryParameter(string name, byte[] value)
        {
            return new SqlParameter(name, SqlDbType.VarBinary) { SqlValue = value };
        }

        public static SqlParameter CreateFloatParameter(string name, double value)
        {
            return new SqlParameter(name, SqlDbType.Float) { SqlValue = value };
        }

        public static SqlParameter CreateDecimalParameter(string name, double value)
        {
            return new SqlParameter(name, SqlDbType.Decimal) { SqlValue = value };
        }

        public static SqlParameter CreateGuidArrayParameter(string name, IList<Guid> guids)
        {
            const int GuidSize = 16;
            SqlParameter param = new SqlParameter(name, SqlDbType.VarBinary);

            byte[] b = new byte[guids.Count * GuidSize];
            for (int i = 0; i < guids.Count; i++)
            {
                Array.Copy(
                           guids[i].ToByteArray(),
                           0,
                           b,
                           i * GuidSize,
                           GuidSize);
            }

            param.Value = b;

            return param;
        }

        public static SqlParameter CreateInt32ArrayParameter(string name, Int32[] int32s)
        {
            const int Int32Size = 4;
            SqlParameter param = new SqlParameter(name, SqlDbType.VarBinary);

            byte[] b = new byte[int32s.Length * Int32Size];
            for (int i = 0; i < int32s.Length; i++)
            {
                byte[] int32Bytes = BitConverter.GetBytes(int32s[i]);
                Array.Reverse(int32Bytes);
                Array.Copy(
                           int32Bytes,
                           0,
                           b,
                           i * Int32Size,
                           Int32Size);
            }

            param.Value = b;

            return param;
        }

        public static SqlParameter CreateInt64ArrayParameter(string name, Int64[] int64s)
        {
            const int Int64Size = 8;
            SqlParameter param = new SqlParameter(name, SqlDbType.VarBinary);

            byte[] b = new byte[int64s.Length * Int64Size];
            for (int i = 0; i < int64s.Length; i++)
            {
                byte[] int64Bytes = BitConverter.GetBytes(int64s[i]);
                Array.Reverse(int64Bytes);
                Array.Copy(
                            int64Bytes,
                            0,
                            b,
                            i * Int64Size,
                            Int64Size);
            }
            param.Value = b;

            return param;
        }

        public static SqlParameter CreateStringArrayParameter(string name, string[] strings)
        {
            StringBuilder sb = new StringBuilder(512);
            using (XmlTextWriter writer = new XmlTextWriter(new StringWriter(sb)))
            {
                for (int i = 0; i < strings.Length; i++)
                {
                    string s = strings[i];
                    writer.WriteStartElement("e");
                    writer.WriteAttributeString("i", (i + 1).ToString()); // 1-based index
                    writer.WriteAttributeString("s", s);
                    writer.WriteEndElement();
                }
            }
            SqlParameter param = new SqlParameter(name, SqlDbType.NVarChar);
            param.Value = sb.ToString();
            return param;
        }


        public static string CreateValue(string v)
        {
            return "N'" + v + "'";
        }

        public static string CreateValue(Guid v)
        {
            return "N'" + v.ToString("D") + "'";
        }

        public static string CreateValue(DateTime v)
        {
            return "'" + v.ToString("yyyy-MM-dd HH:mm:ss.fff") + "'";
        }

        public static string CreateValue(bool v)
        {
            return v ? "1" : "0";
        }

        public static string CreateValue(int v)
        {
            return v.ToString(CultureInfo.InvariantCulture);
        }

        public static string CreateValue(long v)
        {
            return v.ToString(CultureInfo.InvariantCulture);
        }

    }
}
