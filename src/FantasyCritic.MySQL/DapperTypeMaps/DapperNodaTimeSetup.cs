namespace FantasyCritic.MySQL.DapperTypeMaps;

public static class DapperNodaTimeSetup
{
    public static void SetupDapperNodaTimeMappings()
    {
        SqlMapper.AddTypeHandler(InstantHandler.Default);
        SqlMapper.AddTypeHandler(LocalDateTimeHandler.Default);
        SqlMapper.AddTypeHandler(LocalDateHandler.Default);
    }
}
