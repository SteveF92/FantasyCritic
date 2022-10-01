namespace FantasyCritic.Lib.Interfaces;
public interface IExternalHypeFactorService
{
    Task<HypeConstants> GetHypeConstants(IEnumerable<MasterGameYear> allMasterGameYears);
}
