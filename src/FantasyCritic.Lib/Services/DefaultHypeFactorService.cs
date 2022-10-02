using FantasyCritic.Lib.Interfaces;

namespace FantasyCritic.Lib.Services;
public class DefaultHypeFactorService : IHypeFactorService
{
    public Task<HypeConstants> GetHypeConstants()
    {
        //These were the correct values on 2022-03-28 and are likely good enough.
        var defaults = new HypeConstants(72.15607447516923, 1.599796665594027, -2.4574452697921627, 0.10576458996831795);
        return Task.FromResult(defaults);
    }
}
