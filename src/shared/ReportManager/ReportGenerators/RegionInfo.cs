using System.Globalization;

namespace Microsoft.Azure.CCME.Assessment.Managers.ReportGenerators
{
    class RegionInfo
    {
        public CultureInfo CurrencyCulture { get; set; }

        public string CurrencySymbol { get; set; }

        public string CurrencyUnit { get; set; }

        public string TargetRegionName { get; set; }
    }
}
