using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WindowsServer.DataBase
{
    public class SingleDb
    {
        private string _connectionString;

        public SingleDb(string connectionString)
        {
            _connectionString = connectionString;
        }

        public bool TableExists(string tableName)
        {
            return DbHelper.TableExists(_connectionString, tableName);
        }

        public bool StoredProcedureExists(string storedProcedureName)
        {
            return DbHelper.StoredProcedureExists(_connectionString, storedProcedureName);
        }

        public long ExecuteTransaction(Func<DbCommand, long> func)
        {
            return DbHelper.ExecuteTransaction(_connectionString, func);
        }

        public int ExecuteNonQuery(string cmdText, params DbParameter[] parameters)
        {
            return DbHelper.ExecuteNonQuery(_connectionString, cmdText, parameters);
        }

        public int ExecuteNonQuery(string cmdText, CommandType commandType, params DbParameter[] parameters)
        {
            return DbHelper.ExecuteNonQuery(_connectionString, cmdText, commandType, parameters);
        }

        public async Task<int> ExecuteNonQueryAsync(string cmdText, params DbParameter[] parameters)
        {
            return await DbHelper.ExecuteNonQueryAsync(_connectionString, cmdText, parameters);
        }

        public async Task<int> ExecuteNonQueryAsync(string cmdText, CommandType commandType, params DbParameter[] parameters)
        {
            return await DbHelper.ExecuteNonQueryAsync(_connectionString, cmdText, commandType, parameters);
        }

        public int ExecuteIntScalar(string cmdText, CommandType commandType, int defaultValue, params DbParameter[] parameters)
        {
            return DbHelper.ExecuteIntScalar(_connectionString, cmdText, commandType, defaultValue, parameters);
        }

        public int ExecuteIntScalar(string cmdText, CommandType commandType, params DbParameter[] parameters)
        {
            return DbHelper.ExecuteIntScalar(_connectionString, cmdText, commandType, parameters);
        }

        public int ExecuteIntScalar(string cmdText, params DbParameter[] parameters)
        {
            return DbHelper.ExecuteIntScalar(_connectionString, cmdText, parameters);
        }

        public object ExecuteScalar(string cmdText, CommandType commandType, object defaultValue, params DbParameter[] parameters)
        {
            return DbHelper.ExecuteScalar(_connectionString, cmdText, commandType, defaultValue, parameters);
        }

        public object ExecuteScalar(string cmdText, CommandType commandType, params DbParameter[] parameters)
        {
            return DbHelper.ExecuteScalar(_connectionString, cmdText, commandType, parameters);
        }

        public async Task<object> ExecuteScalarAsync(string cmdText, params DbParameter[] parameters)
        {
            return await DbHelper.ExecuteScalarAsync(_connectionString, cmdText, CommandType.Text, parameters);
        }

        public async Task<int> ExecuteIntScalarAsync(string cmdText, CommandType commandType, int defaultValue, params DbParameter[] parameters)
        {
            return await DbHelper.ExecuteIntScalarAsync(_connectionString, cmdText, commandType, defaultValue, parameters);
        }

        public async Task<int> ExecuteIntScalarAsync(string cmdText, CommandType commandType, params DbParameter[] parameters)
        {
            return await DbHelper.ExecuteIntScalarAsync(_connectionString, cmdText, commandType, parameters);
        }

        public async Task<int> ExecuteIntScalarAsync(string cmdText, params DbParameter[] parameters)
        {
            return await DbHelper.ExecuteIntScalarAsync(_connectionString, cmdText, parameters);
        }

        public async Task<object> ExecuteScalarAsync(string cmdText, CommandType commandType, object defaultValue, params DbParameter[] parameters)
        {
            return await DbHelper.ExecuteScalarAsync(_connectionString, cmdText, commandType, defaultValue, parameters);
        }

        public async Task<object> ExecuteScalarAsync(string cmdText, CommandType commandType, params DbParameter[] parameters)
        {
            return await DbHelper.ExecuteScalarAsync(_connectionString, cmdText, commandType, parameters);
        }

        public void ExecuteReader(string commandText, DbExecutionParameters executionParameters, Action<DbDataReader> handleOneRow, params DbParameter[] parameters)
        {
            DbHelper.ExecuteReader(_connectionString, commandText, executionParameters, handleOneRow, parameters);
        }

        public void ExecuteReader(string commandText, Action<DbDataReader> handleOneRow, params DbParameter[] parameters)
        {
            DbHelper.ExecuteReader(_connectionString, commandText, handleOneRow, parameters);
        }

        public void ExecuteReader(string commandText, CommandType commandType, Action<DbDataReader> handleOneRow, params DbParameter[] parameters)
        {
            DbHelper.ExecuteReader(_connectionString, commandText, commandType, handleOneRow, parameters);
        }

        public void ExecuteReaderSingleRow(string commandText, Action<DbDataReader> handleOneRow, params DbParameter[] parameters)
        {
            DbHelper.ExecuteReaderSingleRow(_connectionString, commandText, handleOneRow, parameters);
        }

        public void ExecuteReaderSingleRow(string commandText, CommandType commandType, Action<DbDataReader> handleOneRow, params DbParameter[] parameters)
        {
            DbHelper.ExecuteReaderSingleRow(_connectionString, commandText, commandType, handleOneRow, parameters);
        }

        public void ExecuteReader(bool singleRow, string commandText, CommandType commandType, Action<DbDataReader> handleOneRow, params DbParameter[] parameters)
        {
            DbHelper.ExecuteReader(_connectionString, singleRow, commandText, commandType, handleOneRow, parameters);
        }

        public void ExecuteReader(bool singleRow, string commandText, CommandType commandType, DbExecutionParameters executionParameters, Action<DbDataReader> handleOneRow, params DbParameter[] parameters)
        {
            DbHelper.ExecuteReader(_connectionString, singleRow, commandText, commandType, executionParameters, handleOneRow, parameters);
        }

        public async Task ExecuteReaderAsync(string commandText, DbExecutionParameters executionParameters, Action<DbDataReader> handleOneRow, params DbParameter[] parameters)
        {
            await DbHelper.ExecuteReaderAsync(_connectionString, commandText, executionParameters, handleOneRow, parameters);
        }

        public async Task ExecuteReaderAsync(string commandText, Action<DbDataReader> handleOneRow, params DbParameter[] parameters)
        {
            await DbHelper.ExecuteReaderAsync(_connectionString, commandText, handleOneRow, parameters);
        }

        public async Task ExecuteReaderAsync(string commandText, CommandType commandType, Action<DbDataReader> handleOneRow, params DbParameter[] parameters)
        {
            await DbHelper.ExecuteReaderAsync(_connectionString, commandText, commandType, handleOneRow, parameters);
        }

        public async Task ExecuteReaderSingleRowAsync(string commandText, Action<DbDataReader> handleOneRow, params DbParameter[] parameters)
        {
            await DbHelper.ExecuteReaderSingleRowAsync(_connectionString, commandText, handleOneRow, parameters);
        }

        public async Task ExecuteReaderSingleRowAsync(string commandText, CommandType commandType, Action<DbDataReader> handleOneRow, params DbParameter[] parameters)
        {
            await DbHelper.ExecuteReaderSingleRowAsync(_connectionString, commandText, commandType, handleOneRow, parameters);
        }

        public async Task ExecuteReaderAsync(bool singleRow, string commandText, CommandType commandType, Action<DbDataReader> handleOneRow, params DbParameter[] parameters)
        {
            await DbHelper.ExecuteReaderAsync(_connectionString, singleRow, commandText, commandType, handleOneRow, parameters);
        }

        public async Task ExecuteReaderAsync(bool singleRow, string commandText, CommandType commandType, DbExecutionParameters executionParameters, Action<DbDataReader> handleOneRow, params DbParameter[] parameters)
        {
            await DbHelper.ExecuteReaderAsync(_connectionString, singleRow, commandText, commandType, executionParameters, handleOneRow, parameters);
        }

        public void ExecuteInterruptibleReader(bool singleRow, string commandText, CommandType commandType, Func<DbDataReader, bool> handleOneRow, params DbParameter[] parameters)
        {
            DbHelper.ExecuteInterruptibleReader(_connectionString, singleRow, commandText, commandType, handleOneRow, parameters);
        }

        public void ExecuteInterruptibleReader(bool singleRow, string commandText, CommandType commandType, DbExecutionParameters executionParameters, Func<DbDataReader, bool> handleOneRow, params DbParameter[] parameters)
        {
            DbHelper.ExecuteInterruptibleReader(_connectionString, singleRow, commandText, commandType, executionParameters, handleOneRow, parameters);
        }

    }
}
