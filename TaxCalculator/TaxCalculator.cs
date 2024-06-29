namespace TaxCalculator;

/// <summary>
/// This is the public inteface used by our client and may not be changed
/// </summary>
public interface ITaxCalculator
{
    double GetStandardTaxRate(Commodity commodity);
    void SetCustomTaxRate(Commodity commodity, double rate);
    double GetTaxRateForDateTime(Commodity commodity, DateTime date);
    double GetCurrentTaxRate(Commodity commodity);
}

/// <summary>
/// Implements a tax calculator for our client.
/// The calculator has a set of standard tax rates that are hard-coded in the class.
/// It also allows our client to remotely set new, custom tax rates.
/// Finally, it allows the fetching of tax rate information for a specific commodity and point in time.
/// TODO: We know there are a few bugs in the code below, since the calculations look messed up every now and then.
///       There are also a number of things that have to be implemented.
/// </summary>
public class TaxCalculator(TimeProvider? timeProvider = null) : ITaxCalculator
{
    private readonly Dictionary<Commodity, SortedList<RateTimeStamp, double>> _timestampRates = [];
    private readonly TimeProvider _timeProvider = timeProvider ?? TimeProvider.System;

    /// <summary>
    /// Get the standard tax rate for a specific commodity.
    /// </summary>
    public double GetStandardTaxRate(Commodity commodity) => commodity switch
    {
        Commodity.Default => 0.25,
        Commodity.Alcohol => 0.25,
        Commodity.Food => 0.12,
        Commodity.FoodServices => 0.12,
        Commodity.Literature => 0.6,
        Commodity.Transport => 0.6,
        Commodity.CulturalServices => 0.6,
        _ => 0.25
    };


    /// <summary>
    /// This method allows the client to remotely set new custom tax rates.
    /// When they do, we save the commodity/rate information as well as the UTC timestamp of when it was done.
    /// NOTE: Each instance of this object supports a different set of custom rates, since we run one thread per customer.
    /// </summary>
    public void SetCustomTaxRate(Commodity commodity, double rate)
    {

        ArgumentOutOfRangeException.ThrowIfNegative(rate);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(rate, 1);
        var currentTime = _timeProvider.GetUtcNow();

        if (_timestampRates.TryGetValue(commodity, out var timeStampList)
            && !timeStampList.TryAdd(new(currentTime), rate))
        {
            timeStampList[new(currentTime)] = rate;
        }
        else
        {
            _timestampRates.TryAdd(commodity, new SortedList<RateTimeStamp, double> { { new(currentTime), rate } });
        }
    }

    /// <summary>
    /// Gets the tax rate that is active for a specific point in time (in UTC).
    /// A custom tax rate is seen as the currently active rate for a period from its starting timestamp until a new custom rate is set.
    /// If there is no custom tax rate for the specified date, use the standard tax rate.
    /// </summary>
    public double GetTaxRateForDateTime(Commodity commodity, DateTime date)
    {
        var utcDate = date.ToUniversalTime();
        if (_timestampRates.TryGetValue(commodity, out var timestampList)
            && timestampList.TryGetValue(new(utcDate), out var taxRate))
        {
            return taxRate;
        }
        return GetStandardTaxRate(commodity);
    }
    public record RateTimeStamp(DateTimeOffset TimeStamp) : IComparable<RateTimeStamp>
    {
        public int CompareTo(RateTimeStamp? date)
        {
            if (date == null) return 1;

            if (Math.Abs((TimeStamp - date.TimeStamp).TotalSeconds) < 1)
            {
                return 0;
            }

            return DateTime.Compare(TimeStamp.DateTime, date.TimeStamp.DateTime);
        }
    }

    /// <summary>
    /// Gets the tax rate that is active for the current point in time.
    /// A custom tax rate is seen as the currently active rate for a period from its starting timestamp until a new custom rate is set.
    /// If there is no custom tax currently active, use the standard tax rate.
    /// </summary>
    public double GetCurrentTaxRate(Commodity commodity)
    {
        if (_timestampRates.TryGetValue(commodity, out var timestampList))
        {
            return timestampList.Last().Value;
        }

        return GetStandardTaxRate(commodity);
    }

}

public enum Commodity
{
    //PLEASE NOTE: THESE ARE THE ACTUAL TAX RATES THAT SHOULD APPLY, WE JUST GOT THEM FROM THE CLIENT!
    Default,            //25%
    Alcohol,            //25%
    Food,               //12%
    FoodServices,       //12%
    Literature,         //6%
    Transport,          //6%
    CulturalServices    //6%
}
