using FantasyCritic.Lib.Interfaces;

namespace FantasyCritic.Lib.Services;
public class DefaultHypeFactorService : IHypeFactorService
{
    public Task<HypeConstants> GetHypeConstants()
    {
        //These were the correct values on 2025-01-21 and are likely good enough.
        var defaults = new HypeConstants(72.95706879412415, 2.2150845010046707, -5.817742823276648, 0.10208646257897502);
        return Task.FromResult(defaults);
    }
}
