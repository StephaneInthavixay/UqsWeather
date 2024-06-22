using AdamTibi.OpenWeather;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
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
        var realWeatherTemps = new[]
        {
            2, nextDayTemp, 4, 5.5, 6, day5Temp, 8
        };
        var clientStub = new ClientStub(today, realWeatherTemps);
        var controller = new WeatherForecastController(null!, clientStub, null!, null!);

        // Act
        IEnumerable<WeatherForecast> wfs = await controller.GetReal();

        // Assert
        Assert.Equal(3, wfs.First().TemperatureC);
    }

    [Fact]
    public async Task GetReal_RequestsToOpenWeather_MetricUnitIsUsed()
    {
        // Arrange
        var realWeatherTemps = new double[]
        {
            1, 2, 3, 4, 5, 6, 7
        };
        var clientStub = new ClientStub(default(DateTime), realWeatherTemps);
        var controller = new WeatherForecastController(null!, clientStub, null!, null!);
        
        // Act
        var _ = await controller.GetReal();
        
        // Assert
        Assert.NotNull(clientStub.LastUnitSpy);
        Assert.Equal(Units.Metric, clientStub.LastUnitSpy!.Value);
    }
    
    [Fact]
    public async Task GetReal_NotInterestedInTodayWeather_WFStartsFromNextDayWithNSubstitute()
    {
        // Arrange
        const double nextDayTemp = 3.3;
        const double day5Temp = 7.7;
        var today = new DateTime(2022, 1, 1);
        var realWeatherTemps = new[]
        {
            2, nextDayTemp, 4, 5.5, 6, day5Temp, 8
        };
        
        var clientMock = Substitute.For<IClient>();
        clientMock.OneCallAsync(Arg.Any<decimal>(), Arg.Any<decimal>(), Arg.Any<IEnumerable<Excludes>>(),
            Arg.Any<Units>())
            .Returns(x =>
            {
                const int DAYS = 7;
                OneCallResponse res = new OneCallResponse();
                res.Daily = new Daily[DAYS];
                for (int i = 0; i < DAYS; i++)
                {
                    res.Daily[i] = new Daily();
                    res.Daily[i].Dt = today.AddDays(i);
                    res.Daily[i].Temp = new Temp();
                    res.Daily[i].Temp.Day = realWeatherTemps.ElementAt(i);
                }
                return Task.FromResult(res);
            });
        var controller = new WeatherForecastController(null!, clientMock, null!, null!);

        // Act
        IEnumerable<WeatherForecast> wfs = await controller.GetReal();

        // Assert
        Assert.Equal(3, wfs.First().TemperatureC);
    }
    
    [Fact]
    public async Task GetReal_RequestsToOpenWeather_MetricUnitIsUsedWithNSubtitute()
    {
        // Arrange
        var realWeatherTemps = new double[]
        {
            1, 2, 3, 4, 5, 6, 7
        };
        var clientMock = Substitute.For<IClient>();
        clientMock.OneCallAsync(Arg.Any<decimal>(), Arg.Any<decimal>(), Arg.Any<IEnumerable<Excludes>>(),
                Arg.Any<Units>())
            .Returns(x =>
            {
                const int DAYS = 7;
                OneCallResponse res = new OneCallResponse();
                res.Daily = new Daily[DAYS];
                for (int i = 0; i < DAYS; i++)
                {
                    res.Daily[i] = new Daily();
                    res.Daily[i].Dt = default(DateTime).AddDays(i);
                    res.Daily[i].Temp = new Temp();
                    res.Daily[i].Temp.Day = realWeatherTemps.ElementAt(i);
                }
                return Task.FromResult(res);
            });
        var controller = new WeatherForecastController(null!, clientMock, null!, null!);
        
        // Act
        var _ = await controller.GetReal();
        
        // Assert
        await clientMock.Received().OneCallAsync(
            Arg.Any<decimal>(), Arg.Any<decimal>(), Arg.Any<IEnumerable<Excludes>>(),
            Arg.Is<Units>(x => x == Units.Metric));
    }
}