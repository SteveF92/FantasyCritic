using System.Data;

namespace FantasyCritic.MySQL.DapperTypeMaps;

public class InstantHandler : SqlMapper.TypeHandler<Instant>
{
    private InstantHandler()
    {
    }

    public static readonly InstantHandler Default = new InstantHandler();

    public override void SetValue(IDbDataParameter parameter, Instant value)
    {
        parameter.Value = value.ToDateTimeUtc();

        if (parameter is MySqlParameter sqlParameter)
        {
            sqlParameter.DbType = DbType.DateTime2;
            sqlParameter.MySqlDbType = MySqlDbType.DateTime;
        }
    }

    public override Instant Parse(object value)
    {
        if (value is DateTime dateTime)
        {
            var dt = DateTime.SpecifyKind(dateTime, DateTimeKind.Utc);
            return Instant.FromDateTimeUtc(dt);
        }

        if (value is DateTimeOffset dateTimeOffset)
        {
            return Instant.FromDateTimeOffset(dateTimeOffset);
        }

        throw new DataException("Cannot convert " + value.GetType() + " to NodaTime.Instant");
    }
}
