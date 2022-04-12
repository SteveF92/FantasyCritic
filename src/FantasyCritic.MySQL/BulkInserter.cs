using System.Data.Common;
using System.Reflection;
using Faithlife.Utility.Dapper;

namespace FantasyCritic.MySQL;

internal static class BulkInserter
{
    public static Task BulkInsertAsync<TInsertType>(this DbConnection conn, IEnumerable<TInsertType> objects, string tableName, int batchSize = 0, DbTransaction? transaction = null, IEnumerable<string>? excludedFields = null, bool insertIgnore = false)
    {
        if (!objects.Any())
        {
            return Task.CompletedTask;
        }

        List<TInsertType> objectList = objects.ToList();
        Type objectType = objectList.First()!.GetType();
        bool allSameType = objectList.All(x => x!.GetType() == objectType);
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

        string ignore = "";
        if (insertIgnore)
        {
            ignore = "IGNORE";
        }

        var propertyNames = includedProperties.Select(x => x.Name);
        var atPropertyNames = includedProperties.Select(x => $"@{x.Name}");
        string propertiesList = string.Join(", ", propertyNames);
        string atPropertiesList = string.Join(", ", atPropertyNames);
        string sqlCommand = $"insert {ignore} into {tableName} ({propertiesList}) VALUES ({atPropertiesList}) ...";
        return conn.BulkInsertAsync(sqlCommand, objectList, transaction, batchSize);
    }
}
