using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaxCalculator;

public interface ITaxRepository
{
    public void SetCustomTaxRate(Commodity commodity, double rate);
    public double GetCustomTaxRate(string taxRate);
}
internal class TaxRepository
{

}
