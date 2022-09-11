using System.Xml.Linq;
using FantasyCritic.Lib.DependencyInjection;
using FantasyCritic.MySQL.Entities;
using Microsoft.AspNetCore.DataProtection.Repositories;

namespace FantasyCritic.MySQL;
public class MySQLXmlRepository : IXmlRepository
{

    private readonly string _connectionString;

    public MySQLXmlRepository(RepositoryConfiguration configuration)
    {
        _connectionString = configuration.ConnectionString;
    }

    public IReadOnlyCollection<XElement> GetAllElements()
    {
        using var connection = new MySqlConnection(_connectionString);
        var results = connection.Query<XmlKey>("select * from tbl_system_xmlkey")
            .Select(x => XElement.Parse(x.Xml))
            .ToList();
        return results;
    }

    public void StoreElement(XElement element, string friendlyName)
    {
        var key = new XmlKey
        {
            Id = Guid.NewGuid(),
            FriendlyName = friendlyName,
            Xml = element.ToString(SaveOptions.DisableFormatting)
        };

        using var connection = new MySqlConnection(_connectionString);
        connection.Execute("INSERT into tbl_system_xmlkey (Id, FriendlyName, Xml) values (@Id, @FriendlyName, @Xml)", key);
    }
}
