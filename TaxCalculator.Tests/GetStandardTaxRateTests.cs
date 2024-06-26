namespace TaxCalculator.Tests;

public class GetStandardTaxRateTests
{
    [Theory]
    [InlineData(Commodity.Default, 0.25)]
    [InlineData(Commodity.Alcohol, 0.25)]
    [InlineData(Commodity.Food, 0.12)]
    [InlineData(Commodity.FoodServices, 0.12)]
    [InlineData(Commodity.Literature, 0.6)]
    [InlineData(Commodity.Transport, 0.6)]
    [InlineData(Commodity.CulturalServices, 0.6)]
    public void GetStandardTaxRate_ReturnsStandardValue(Commodity commodity, double rate)
    {
        //arrange
        var sut = new TaxCalculator();
        var expected = rate;

        //act
        var actual = sut.GetStandardTaxRate(commodity);

        //assert
        Assert.Equal(expected, actual);
    }
}