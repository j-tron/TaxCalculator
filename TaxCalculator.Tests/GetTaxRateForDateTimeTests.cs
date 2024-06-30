using Moq;

namespace TaxCalculator.Tests;

public class GetTaxRateForDateTimeTests
{
    [Theory]
    [InlineData(Commodity.Default, 0.2)]
    [InlineData(Commodity.Alcohol, 0.4)]
    [InlineData(Commodity.Food, 0.12)]
    [InlineData(Commodity.FoodServices, 0.123456)]
    public void GetTaxRateForDateTime__ReturnsCorrectValue(Commodity commodity, double rate)
    {
        //arrange
        Mock<TimeProvider> timeProviderMock = new();
        var sut = new TaxCalculator(timeProviderMock.Object);
        var expected = rate;
        var timeStamp = DateTime.UtcNow;

        //act
        sut.SetCustomTaxRate(Commodity.Literature, 0.7);
        sut.SetCustomTaxRate(commodity, 0.5);
        sut.SetCustomTaxRate(commodity, expected);
        var actual = sut.GetTaxRateForDateTime(commodity, timeStamp);

        //assert
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void GetTaxRateForDateTime_NoCustomRate_ReturnsStandardRate()
    {
        // Arrange
        Mock<TimeProvider> timeProviderMock = new();
        var sut = new TaxCalculator(timeProviderMock.Object);
        var utcNow = DateTimeOffset.UtcNow;
        var expected = sut.GetStandardTaxRate(Commodity.Transport);

        // Act
        var actual = sut.GetTaxRateForDateTime(Commodity.Transport, utcNow.UtcDateTime);

        // Assert
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void SetCustomTaxRate_TimeRangeBetweenStored_ReturnsCorrectRate()
    {
        //arrange
        var expected = 0.78;
        Mock<TimeProvider> timeProviderMock = new();
        var sut = new TaxCalculator(timeProviderMock.Object);
        DateTimeOffset utcNow = DateTimeOffset.UtcNow;

        timeProviderMock.Setup(x => x.GetUtcNow()).Returns(utcNow.AddHours(-2));
        sut.SetCustomTaxRate(Commodity.Literature, 0.5);
        sut.SetCustomTaxRate(Commodity.Transport, 0.1);

        timeProviderMock.Setup(x => x.GetUtcNow()).Returns(utcNow);
        sut.SetCustomTaxRate(Commodity.FoodServices, 0.3);
        sut.SetCustomTaxRate(Commodity.Transport, 0.2);

        //Time between first (-2) and second (0) stored Transport rate
        var utcSomeTimeAgo = utcNow.AddHours(-1).AddMinutes(3).AddSeconds(23);
        timeProviderMock.Setup(x => x.GetUtcNow()).Returns(utcSomeTimeAgo);
        sut.SetCustomTaxRate(Commodity.Transport, expected);

        //act
        var actual = sut.GetTaxRateForDateTime(Commodity.Transport, utcSomeTimeAgo.UtcDateTime);

        //assert
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void SetCustomTaxRate_DateTimeBeforeStored_ReturnsStandartRate()
    {
        //arrange
        var expected = 0.78;
        Mock<TimeProvider> timeProviderMock = new();
        var sut = new TaxCalculator(timeProviderMock.Object);
        DateTimeOffset utcNow = DateTimeOffset.UtcNow;

        timeProviderMock.Setup(x => x.GetUtcNow()).Returns(utcNow);
        sut.SetCustomTaxRate(Commodity.Literature, 0.5);
        sut.SetCustomTaxRate(Commodity.Transport, 0.1);
        sut.SetCustomTaxRate(Commodity.FoodServices, 0.3);

        var utcSomeTimeAgo = utcNow.AddHours(-1).AddMinutes(3).AddSeconds(23);
        timeProviderMock.Setup(x => x.GetUtcNow()).Returns(utcSomeTimeAgo);
        sut.SetCustomTaxRate(Commodity.Transport, expected);

        //act
        var actual = sut.GetTaxRateForDateTime(Commodity.Transport, utcSomeTimeAgo.UtcDateTime);

        //assert
        Assert.Equal(expected, actual);
    }
}
