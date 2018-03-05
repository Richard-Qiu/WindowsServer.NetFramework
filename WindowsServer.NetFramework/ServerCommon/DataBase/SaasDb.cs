using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WindowsServer.DataBase
{
    public class SaasDb<T>
    {
        private SaasDbSelector<T> _selector;

        public SaasDb(SaasDbSelector<T> selector)
        {
            _selector = selector;
        }

        public bool TableExists(T arg, string tableName)
        {
            return DbHelper.TableExists(_selector.GetConnectionString(arg), tableName);
        }

        public bool StoredProcedureExists(T arg, string storedProcedureName)
        {
            return DbHelper.StoredProcedureExists(_selector.GetConnectionString(arg), storedProcedureName);
        }

        public long ExecuteTransaction(T arg, Func<DbCommand, long> func)
        {
            return DbHelper.ExecuteTransaction(_selector.GetConnectionString(arg), func);
        }

        public int ExecuteNonQuery(T arg, string cmdText, params DbParameter[] parameters)
        {
            return DbHelper.ExecuteNonQuery(_selector.GetConnectionString(arg), cmdText, parameters);
        }

        public int ExecuteNonQuery(T arg, string cmdText, CommandType commandType, params DbParameter[] parameters)
        {
            return DbHelper.ExecuteNonQuery(_selector.GetConnectionString(arg), cmdText, commandType, parameters);
        }

        public async Task<int> ExecuteNonQueryAsync(T arg, string cmdText, params DbParameter[] parameters)
        {
            return await DbHelper.ExecuteNonQueryAsync(_selector.GetConnectionString(arg), cmdText, parameters);
        }

        public async Task<int> ExecuteNonQueryAsync(T arg, string cmdText, CommandType commandType, params DbParameter[] parameters)
        {
            return await DbHelper.ExecuteNonQueryAsync(_selector.GetConnectionString(arg), cmdText, commandType, parameters);
        }

        public int ExecuteIntScalar(T arg, string cmdText, CommandType commandType, int defaultValue, params DbParameter[] parameters)
        {
            return DbHelper.ExecuteIntScalar(_selector.GetConnectionString(arg), cmdText, commandType, defaultValue, parameters);
        }

        public int ExecuteIntScalar(T arg, string cmdText, CommandType commandType, params DbParameter[] parameters)
        {
            return DbHelper.ExecuteIntScalar(_selector.GetConnectionString(arg), cmdText, commandType, parameters);
        }

        public int ExecuteIntScalar(T arg, string cmdText, params DbParameter[] parameters)
        {
            return DbHelper.ExecuteIntScalar(_selector.GetConnectionString(arg), cmdText, parameters);
        }

        public object ExecuteScalar(T arg, string cmdText, CommandType commandType, object defaultValue, params DbParameter[] parameters)
        {
            return DbHelper.ExecuteScalar(_selector.GetConnectionString(arg), cmdText, commandType, defaultValue, parameters);
        }

        public object ExecuteScalar(T arg, string cmdText, CommandType commandType, params DbParameter[] parameters)
        {
            return DbHelper.ExecuteScalar(_selector.GetConnectionString(arg), cmdText, commandType, parameters);
        }


        public async Task<int> ExecuteIntScalarAsync(T arg, string cmdText, CommandType commandType, int defaultValue, params DbParameter[] parameters)
        {
            return await DbHelper.ExecuteIntScalarAsync(_selector.GetConnectionString(arg), cmdText, commandType, defaultValue, parameters);
        }

        public async Task<int> ExecuteIntScalarAsync(T arg, string cmdText, CommandType commandType, params DbParameter[] parameters)
        {
            return await DbHelper.ExecuteIntScalarAsync(_selector.GetConnectionString(arg), cmdText, commandType, parameters);
        }

        public async Task<int> ExecuteIntScalarAsync(T arg, string cmdText, params DbParameter[] parameters)
        {
            return await DbHelper.ExecuteIntScalarAsync(_selector.GetConnectionString(arg), cmdText, parameters);
        }

        public async Task<object> ExecuteScalarAsync(T arg, string cmdText, CommandType commandType, object defaultValue, params DbParameter[] parameters)
        {
            return await DbHelper.ExecuteScalarAsync(_selector.GetConnectionString(arg), cmdText, commandType, defaultValue, parameters);
        }

        public async Task<object> ExecuteScalarAsync(T arg, string cmdText, CommandType commandType, params DbParameter[] parameters)
        {
            return await DbHelper.ExecuteScalarAsync(_selector.GetConnectionString(arg), cmdText, commandType, parameters);
        }

        public async Task<object> ExecuteScalarAsync(T arg, string cmdText, params DbParameter[] parameters)
        {
            return await DbHelper.ExecuteScalarAsync(_selector.GetConnectionString(arg), cmdText, CommandType.Text, parameters);
        }

        public void ExecuteReader(T arg, string commandText, DbExecutionParameters executionParameters, Action<DbDataReader> handleOneRow, params DbParameter[] parameters)
        {
            DbHelper.ExecuteReader(_selector.GetConnectionString(arg), commandText, executionParameters, handleOneRow, parameters);
        }

        public void ExecuteReader(T arg, string commandText, Action<DbDataReader> handleOneRow, params DbParameter[] parameters)
        {
            DbHelper.ExecuteReader(_selector.GetConnectionString(arg), commandText, handleOneRow, parameters);
        }

        public void ExecuteReader(T arg, string commandText, CommandType commandType, Action<DbDataReader> handleOneRow, params DbParameter[] parameters)
        {
            DbHelper.ExecuteReader(_selector.GetConnectionString(arg), commandText, commandType, handleOneRow, parameters);
        }

        public void ExecuteReaderSingleRow(T arg, string commandText, Action<DbDataReader> handleOneRow, params DbParameter[] parameters)
        {
            DbHelper.ExecuteReaderSingleRow(_selector.GetConnectionString(arg), commandText, handleOneRow, parameters);
        }

        public void ExecuteReaderSingleRow(T arg, string commandText, CommandType commandType, Action<DbDataReader> handleOneRow, params DbParameter[] parameters)
        {
            DbHelper.ExecuteReaderSingleRow(_selector.GetConnectionString(arg), commandText, commandType, handleOneRow, parameters);
        }

        public void ExecuteReader(T arg, bool singleRow, string commandText, CommandType commandType, Action<DbDataReader> handleOneRow, params DbParameter[] parameters)
        {
            DbHelper.ExecuteReader(_selector.GetConnectionString(arg), singleRow, commandText, commandType, handleOneRow, parameters);
        }

        public void ExecuteReader(T arg, bool singleRow, string commandText, CommandType commandType, DbExecutionParameters executionParameters, Action<DbDataReader> handleOneRow, params DbParameter[] parameters)
        {
            DbHelper.ExecuteReader(_selector.GetConnectionString(arg), singleRow, commandText, commandType, executionParameters, handleOneRow, parameters);
        }

        public async Task ExecuteReaderAsync(T arg, string commandText, DbExecutionParameters executionParameters, Action<DbDataReader> handleOneRow, params DbParameter[] parameters)
        {
            await DbHelper.ExecuteReaderAsync(_selector.GetConnectionString(arg), commandText, executionParameters, handleOneRow, parameters);
        }

        public async Task ExecuteReaderAsync(T arg, string commandText, Action<DbDataReader> handleOneRow, params DbParameter[] parameters)
        {
            await DbHelper.ExecuteReaderAsync(_selector.GetConnectionString(arg), commandText, handleOneRow, parameters);
        }

        public async Task ExecuteReaderAsync(T arg, string commandText, CommandType commandType, Action<DbDataReader> handleOneRow, params DbParameter[] parameters)
        {
            await DbHelper.ExecuteReaderAsync(_selector.GetConnectionString(arg), commandText, commandType, handleOneRow, parameters);
        }

        public async Task ExecuteReaderSingleRowAsync(T arg, string commandText, Action<DbDataReader> handleOneRow, params DbParameter[] parameters)
        {
            await DbHelper.ExecuteReaderSingleRowAsync(_selector.GetConnectionString(arg), commandText, handleOneRow, parameters);
        }

        public async Task ExecuteReaderSingleRowAsync(T arg, string commandText, CommandType commandType, Action<DbDataReader> handleOneRow, params DbParameter[] parameters)
        {
            await DbHelper.ExecuteReaderSingleRowAsync(_selector.GetConnectionString(arg), commandText, commandType, handleOneRow, parameters);
        }

        public async Task ExecuteReaderAsync(T arg, bool singleRow, string commandText, CommandType commandType, Action<DbDataReader> handleOneRow, params DbParameter[] parameters)
        {
            await DbHelper.ExecuteReaderAsync(_selector.GetConnectionString(arg), singleRow, commandText, commandType, handleOneRow, parameters);
        }

        public async Task ExecuteReaderAsync(T arg, bool singleRow, string commandText, CommandType commandType, DbExecutionParameters executionParameters, Action<DbDataReader> handleOneRow, params DbParameter[] parameters)
        {
            await DbHelper.ExecuteReaderAsync(_selector.GetConnectionString(arg), singleRow, commandText, commandType, executionParameters, handleOneRow, parameters);
        }

        public void ExecuteInterruptibleReader(T arg, bool singleRow, string commandText, CommandType commandType, Func<DbDataReader, bool> handleOneRow, params DbParameter[] parameters)
        {
            DbHelper.ExecuteInterruptibleReader(_selector.GetConnectionString(arg), singleRow, commandText, commandType, handleOneRow, parameters);
        }

        public async Task ExecuteInterruptibleReaderAsync(T arg, bool singleRow, string commandText, CommandType commandType, Func<DbDataReader, Task<bool>> handleOneRow, params DbParameter[] parameters)
        {
            await DbHelper.ExecuteInterruptibleReaderAsync(_selector.GetConnectionString(arg), singleRow, commandText, commandType, handleOneRow, parameters);
        }

        public void ExecuteInterruptibleReader(T arg, bool singleRow, string commandText, CommandType commandType, DbExecutionParameters executionParameters, Func<DbDataReader, bool> handleOneRow, params DbParameter[] parameters)
        {
            DbHelper.ExecuteInterruptibleReader(_selector.GetConnectionString(arg), singleRow, commandText, commandType, executionParameters, handleOneRow, parameters);
        }

        public async Task ExecuteInterruptibleReaderAsync(T arg, bool singleRow, string commandText, CommandType commandType, DbExecutionParameters executionParameters, Func<DbDataReader, Task<bool>> handleOneRow, params DbParameter[] parameters)
        {
            await DbHelper.ExecuteInterruptibleReaderAsync(_selector.GetConnectionString(arg), singleRow, commandText, commandType, executionParameters, handleOneRow, parameters);
        }
    }
}
