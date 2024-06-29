using Moq;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System;

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
        var sut = new TaxCalculator();
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

    //[Fact]
    ////[MemberData(nameof(CalculatorData.Data), MemberType = typeof(CalculatorData))]
    //public void SetCustomTaxRate_OnMsultipleInstances_ReturnsCorrectRate()//(Commodity commodity, double rate, DateTime dateTime)
    //{
    //    //arrange
    //    var expected = 0.78;
    //    Mock<TimeProvider> timeProviderMock = new();
    //    timeProviderMock.Setup(x => x.GetUtcNow()).Returns(new DateTimeOffset(2023, 2, 13, 12, 00, 00, TimeSpan.Zero));
    //    var mockedTimeProvider = timeProviderMock.Object;
    //    //instance 1
    //    var sut1 = new TaxCalculator();
    //    sut1.SetCustomTaxRate(Commodity.Literature, 0.5);
    //    sut1.SetCustomTaxRate(commodity, expected);

    //    //instance 2
    //    var sut2 = new TaxCalculator(mockedTimeProvider);
    //    sut2.SetCustomTaxRate(Commodity.FoodServices, 0.3);
    //    timeProviderMock.Setup(x => x.GetUtcNow()).Returns(DateTimeOffset.UtcNow.AddHours(-1));
    //    sut2.SetCustomTaxRate(commodity, rate);

    //    //act
    //    var actual = sut2.GetTaxRateForDateTime(commodity, dateTime.AddHours(-1));

    //    //assert
    //    Assert.Equal(expected, actual);
    //}

    [Fact]
    //[MemberData(nameof(CalculatorData.Data), MemberType = typeof(CalculatorData))]
    public void SetCustomTaxRate_PreviouslyAddedRate_ReturnsCorrectRate()
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
    public class CalculatorData
    {
        public static IEnumerable<object[]> Data =>
        [
            [Commodity.Transport, 1, DateTime.UtcNow],
        ];
    }

    public class TestTimeProvider(DateTimeOffset dateTimeOffset = default) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow()
        {
            return dateTimeOffset;//new DateTimeOffset(2023, 12, 1, 1, 0, 0, TimeSpan.Zero); // 1 AM
        }
    }
}
