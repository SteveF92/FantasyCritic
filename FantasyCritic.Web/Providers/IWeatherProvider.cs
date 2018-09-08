using System.Collections.Generic;
using FantasyCritic.Web.Models;

namespace FantasyCritic.Web.Providers
{
    public interface IWeatherProvider
    {
        List<WeatherForecast> GetForecasts();
    }
}
