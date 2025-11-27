using System.Data;

namespace FantasyCritic.MySQL.DapperTypeMaps;

public sealed class LocalDateTimeHandler
        : SqlMapper.TypeHandler<LocalDateTime>
{
    public static readonly LocalDateTimeHandler Default = new();

    private LocalDateTimeHandler()
    {
    }

    public override void SetValue(IDbDataParameter parameter, LocalDateTime value)
    {
        parameter.Value = value.ToDateTimeUnspecified();

        if (parameter is MySqlParameter sqlParameter)
        {
            sqlParameter.DbType = DbType.DateTime2;
            sqlParameter.MySqlDbType = MySqlDbType.DateTime;
        }
    }

    public override LocalDateTime Parse(object value)
    {
        if (value is DateTime dateTime)
        {
            return LocalDateTime.FromDateTime(dateTime);
        }

        throw new DataException("Cannot convert " + value.GetType() + " to NodaTime.LocalDateTime");
    }
}
