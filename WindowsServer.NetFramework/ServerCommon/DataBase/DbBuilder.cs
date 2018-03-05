using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WindowsServer.Log;

namespace WindowsServer.DataBase
{
    public static class DbBuilder
    {
        private static Logger _logger = LogManager.GetCurrentClassLogger();

        public static async Task<bool> BuildNumbers10K(string connectionString)
        {
            if (DbHelper.TableExists(connectionString, "Numbers10K"))
            {
                return false;
            }

            _logger.Info("Table 'Numbers10K' does not exist. Create it.");

            string cmd = "CREATE TABLE [Numbers10K] ([Number] int NOT NULL, CONSTRAINT [PK_Numbers10K] PRIMARY KEY ([Number]) )";
            await DbHelper.ExecuteNonQueryAsync(connectionString, cmd);

            cmd = "IF NOT EXISTS(SELECT TOP 1 1 FROM Numbers10K)\r\n"
                + "BEGIN\r\n"
                + "   WITH digits (d) AS (\r\n"
                + "       SELECT 1 UNION SELECT 2 UNION SELECT 3 UNION\r\n"
                + "       SELECT 4 UNION SELECT 5 UNION SELECT 6 UNION\r\n"
                + "       SELECT 7 UNION SELECT 8 UNION SELECT 9 UNION\r\n"
                + "       SELECT 0)\r\n"
                + "   INSERT Numbers10K (Number)\r\n"
                + "       SELECT Number\r\n"
                + "       FROM   (SELECT i.d + ii.d * 10 + iii.d * 100 + iv.d * 1000 AS Number\r\n"
                + "               FROM   digits i\r\n"
                + "               CROSS  JOIN digits ii\r\n"
                + "               CROSS  JOIN digits iii\r\n"
                + "               CROSS  JOIN digits iv) AS Numbers10K\r\n"
                + "       WHERE  Number > 0\r\n"
                + "END";
            await DbHelper.ExecuteNonQueryAsync(connectionString, cmd);

            if (!DbHelper.StoredProcedureExists(connectionString, "ParseBigIntArray"))
            {
                _logger.Info("StoredProcedure 'ParseBigIntArray' does not exist. Create it.");
                cmd = "CREATE FUNCTION [dbo].[ParseBigIntArray](@bin varbinary(MAX))\r\n"
                    + "RETURNS TABLE AS\r\n"
                    + "RETURN(SELECT [Index] = n.Number,\r\n"
                    + "         [BigInt] = convert(bigint, substring(@bin, 8 * (n.Number - 1) + 1, 8))\r\n"
                    + "     FROM   [Numbers10K] n\r\n"
                    + "     WHERE  n.Number <= datalength(@bin) / 8 )";
                await DbHelper.ExecuteNonQueryAsync(connectionString, cmd);
            }

            if (!DbHelper.StoredProcedureExists(connectionString, "ParseGuidArray"))
            {
                _logger.Info("StoredProcedure 'ParseGuidArray' does not exist. Create it.");
                cmd = "CREATE FUNCTION [dbo].[ParseGuidArray](@bin varbinary(MAX))\r\n"
                    + "RETURNS TABLE AS\r\n"
                    + "RETURN(SELECT [Index] = n.Number,\r\n"
                    + "         [Guid] = convert(uniqueidentifier, substring(@bin, 16 * (n.Number - 1) + 1, 16))\r\n"
                    + "     FROM   [Numbers10K] n\r\n"
                    + "     WHERE n.Number <= datalength(@bin) / 16 )";
                await DbHelper.ExecuteNonQueryAsync(connectionString, cmd);
            }

            if (!DbHelper.StoredProcedureExists(connectionString, "ParseGuidArrayWithoutIndex"))
            {
                _logger.Info("StoredProcedure 'ParseGuidArrayWithoutIndex' does not exist. Create it.");
                cmd = "CREATE FUNCTION [dbo].[ParseGuidArrayWithoutIndex](@bin varbinary(MAX))\r\n"
                    + "RETURNS @t TABLE\r\n"
                    + "(\r\n"
                    + "[Guid] uniqueidentifier NOT NULL primary key\r\n"
                    + ") AS\r\n"
                    + "BEGIN\r\n"
                    + "     INSERT @t\r\n"
                    + "     SELECT [Guid] = convert(uniqueidentifier, substring(@bin, 16 * (n.Number - 1) + 1, 16))\r\n"
                    + "     FROM   [Numbers10K] n\r\n"
                    + "     WHERE  n.Number <= datalength(@bin) / 16\r\n"
                    + " RETURN\r\n"
                    + "END";
                await DbHelper.ExecuteNonQueryAsync(connectionString, cmd);
            }

            if (!DbHelper.StoredProcedureExists(connectionString, "ParseInt32Array"))
            {
                _logger.Info("StoredProcedure 'ParseInt32Array' does not exist. Create it.");
                cmd = "CREATE FUNCTION [dbo].[ParseInt32Array](@bin varbinary(MAX))\r\n"
                    + "RETURNS TABLE AS\r\n"
                    + "RETURN(SELECT [Index] = n.Number,\r\n"
                    + "         [Int32] = convert(bigint, substring(@bin, 4 * (n.Number - 1) + 1, 4))\r\n"
                    + "     FROM   [Numbers10K] n\r\n"
                    + "     WHERE  n.Number <= datalength(@bin) / 4 )";
                await DbHelper.ExecuteNonQueryAsync(connectionString, cmd);
            }

            return true;
        }
    }
}
