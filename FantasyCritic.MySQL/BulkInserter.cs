using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using FantasyCritic.Lib.Extensions;
using Faithlife.Utility.Dapper;

namespace FantasyCritic.MySQL
{
    public static class BulkInserter
    {
        public static Task BulkInsertAsync<TInsertType>(this DbConnection conn, IEnumerable<TInsertType> objects, string tableName, int batchSize = 0, DbTransaction transaction = null, IEnumerable<string> excludedFields = null)
        {
            List<TInsertType> objectList = objects.ToList();
            Type objectType = objectList.First().GetType();
            bool allSameType = objectList.All(x => x.GetType() == objectType);
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

            var propertyNames = includedProperties.Select(x => x.Name);
            var atPropertyNames = includedProperties.Select(x => $"@{x.Name}");
            string propertiesList = string.Join(", ", propertyNames);
            string atPropertiesList = string.Join(", ", atPropertyNames);
            string sqlCommand = $"insert into {tableName} ({propertiesList}) VALUES ({atPropertiesList}) ...";
            return conn.BulkInsertAsync(sqlCommand, objectList, transaction, batchSize);
        }
    }
}
