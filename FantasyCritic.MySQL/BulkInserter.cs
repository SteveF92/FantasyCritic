using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using FantasyCritic.Lib.Extensions;

namespace FantasyCritic.MySQL
{
    public static class BulkInserter
    {
        public static async Task BulkInsertAsync(this DbConnection conn, IEnumerable<object> objects, string tableName, int batchSize = 0, DbTransaction transaction = null, IEnumerable<string> excludedFields = null)
        {
            if (!objects.Any())
            {
                return;
            }

            IEnumerable<IEnumerable<object>> batches;

            if (batchSize > 0)
            {
                batches = objects.SplitList(batchSize);
            }
            else
            {
                batches = objects.SplitList(objects.Count());
            }

            Type objectType = objects.First().GetType();
            bool allSameType = objects.All(x => x.GetType() == objectType);
            if (!allSameType)
            {
                throw new Exception("All objects must be of the same type.");
            }

            if (excludedFields == null)
            {
                excludedFields = new List<string>();
            }

            PropertyInfo[] properties = objectType.GetProperties();
            List<PropertyInfo> includedProperties = properties.Where(x => !excludedFields.Contains(x.Name)).ToList();

            if (transaction == null)
            {
                using (var internalTransaction = await conn.BeginTransactionAsync())
                {
                    foreach (var batch in batches)
                    {
                        await InsertBatchAsync(conn, batch, tableName, internalTransaction, includedProperties);
                    }

                    await internalTransaction.CommitAsync();
                }
            }
            else
            {
                foreach (var batch in batches)
                {
                    await InsertBatchAsync(conn, batch, tableName, transaction, includedProperties);
                }
            }
        }

        private static async Task InsertBatchAsync(DbConnection conn, IEnumerable<object> batch, string tableName, DbTransaction transaction, IEnumerable<PropertyInfo> includedProperties)
        {
            var propertyNames = includedProperties.Select(x => x.Name);
            string propertiesList = string.Join(", ", propertyNames);
            string sqlCommand = $"insert into {tableName} ({propertiesList}) VALUES ";

            using (DbCommand myCmd = conn.CreateCommand())
            {
                myCmd.CommandType = CommandType.Text;
                myCmd.Transaction = transaction;

                int counter = 0;
                foreach (object currentObject in batch)
                {
                    string thisValue = "(";
                    foreach (PropertyInfo property in includedProperties)
                    {
                        string parameterName = "@" + property.Name + counter;
                        thisValue += parameterName + ", ";

                        DbParameter parameter = myCmd.CreateParameter();
                        parameter.ParameterName = parameterName;
                        object propertyValue = property.GetValue(currentObject, null);
                        parameter.Value = propertyValue;

                        myCmd.Parameters.Add(parameter);
                    }
                    thisValue = thisValue.Substring(0, thisValue.Length - 2);
                    thisValue += "), ";
                    sqlCommand += thisValue;

                    counter++;
                }

                myCmd.CommandText = sqlCommand.Substring(0, sqlCommand.Length - 2);
                await myCmd.ExecuteNonQueryAsync();
                myCmd.Dispose();
            }
        }
    }
}
