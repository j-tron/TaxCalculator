using Moq;

namespace TaxCalculator.Tests;

public class SetCustomTaxRateTests
{
    [Theory]
    [InlineData(Commodity.Default, 0.2)]
    [InlineData(Commodity.Alcohol, 0.4)]
    [InlineData(Commodity.Food, 0.12)]
    [InlineData(Commodity.FoodServices, 0.123456)]
    public void SetCustomTaxRate_DuplicateValues_ReturnsLatestValue(Commodity commodity, double rate)
    {
        //arrange
        var sut = new TaxCalculator();
        var expected = rate;
        sut.SetCustomTaxRate(commodity, 0.5);
        sut.SetCustomTaxRate(commodity, expected);
        
        //act
        var actual = sut.GetCurrentTaxRate(commodity);

        //assert
        Assert.Equal(expected, actual);
    }

    [Theory]
    [InlineData(Commodity.Default, -0.2)]
    [InlineData(Commodity.Alcohol, 1.4)]
    public void SetCustomTaxRate_InvalidRate_ThrowsException(Commodity commodity, double rate)
    {
        //arrange
        var sut = new TaxCalculator();

        //act && assert
        Assert.Throws<ArgumentOutOfRangeException>(() => sut.SetCustomTaxRate(commodity, rate));
    }

    [Theory]
    [InlineData(Commodity.Alcohol, 0.4)]
    [InlineData(Commodity.Food, 0.12)]
    public void SetCustomTaxRate_OnMultipleInstances_ReturnsCorrectRate(Commodity commodity, double rate)
    {
        //arrange
        var expected = rate;

        //instance 1
        var sut1 = new TaxCalculator();
        sut1.SetCustomTaxRate(Commodity.Literature, 0.5);
        sut1.SetCustomTaxRate(commodity, expected);

        //instance 2
        var sut2 = new TaxCalculator();
        sut2.SetCustomTaxRate(Commodity.FoodServices, 0.3);
        sut2.SetCustomTaxRate(commodity, 0.1);

        //act
        var actual = sut1.GetCurrentTaxRate(commodity);

        //assert
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void SetCustomTaxRate_ReturnStandardIfEmpty()
    {
        //arrange
        var sut = new TaxCalculator();

        //act
        var actual = sut.GetCurrentTaxRate(Commodity.Alcohol);

        //assert
        Assert.Equal(0.25, actual);
    }

    [Theory]
    [InlineData(Commodity.Alcohol, 0.4)]
    [InlineData(Commodity.Food, 0.12)]
    public void SetCustomTaxRate_ReturnsOutOfOrderValue(Commodity commodity, double rate)
    {
        //arrange
        var expected = rate;
        Mock<TimeProvider> timeProviderMock = new();
        var sut = new TaxCalculator(timeProviderMock.Object);
        DateTimeOffset utcNow = DateTimeOffset.UtcNow;

        timeProviderMock.Setup(x => x.GetUtcNow()).Returns(utcNow);
        sut.SetCustomTaxRate(commodity, expected);
        timeProviderMock.Setup(x => x.GetUtcNow()).Returns(utcNow.AddHours(-1));
        sut.SetCustomTaxRate(commodity, 0.1);

        //act
        var actual = sut.GetCurrentTaxRate(commodity);

        //assert
        Assert.Equal(expected, actual);
    }
}