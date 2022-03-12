namespace FantasyCritic.Lib.Domain;

public class SystemWideSettings
{
    public SystemWideSettings(bool actionProcessingMode, bool refreshOpenCritic)
    {
        ActionProcessingMode = actionProcessingMode;
        RefreshOpenCritic = refreshOpenCritic;
    }

    public bool ActionProcessingMode { get; }
    public bool RefreshOpenCritic { get; }
}
