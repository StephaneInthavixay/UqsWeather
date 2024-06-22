using Microsoft.Extensions.Logging.Abstractions;
using Uqs.Weather.Controllers;
using Uqs.Weather.Tests.Unit.Fixtures;

namespace Uqs.Weather.Tests.Unit;

public class WeatherForecastControllerTests
{
    [Theory]
    [InlineData(-100, -148)]
    [InlineData(-10.1, 13.8)]
    [InlineData(10, 50)]
    public void ConvertCToF_Celsius_CorrectFahrenheit(double celsius, double expected)
    {
        // Arrange
        var logger = NullLogger<WeatherForecastController>.Instance;
        
        var controller = new WeatherForecastController(logger, null!, null!, null!);
        
        // Act
        double actual = controller.ConvertCToF(celsius);
        
        // Assert
        Assert.Equal(expected, actual, 1);
    }

    [Fact]
    public async Task GetReal_NotInterestedInTodayWeather_WFStartsFromNextDay()
    {
        // Arrange
        const double nextDayTemp = 3.3;
        const double day5Temp = 7.7;
        var today = new DateTime(2022, 1, 1);
        var realWeatherTemps = new double[]
        {
            2, nextDayTemp, 4, 5.5, 6, day5Temp, 8
        };
        var clientStub = new ClientStub(today, realWeatherTemps);
        var controller = new WeatherForecastController(null!, clientStub, null!, null!);

        // Act
        IEnumerable<WeatherForecast> wfs = await controller.GetReal();

        // Assert
        Assert.Equal(-3, wfs.First().TemperatureC);
    }
}