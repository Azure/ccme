using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Azure.CCME.Assessment.Managers.ReportGenerators
{
    class RegionInfo
    {
        public bool IsChinaRegion { get; set; }

        public CultureInfo CurrencyCulture { get; set; }

        public string CurrencySymbol { get; set; }

        public string CurrencyUnit { get; set; }

        public string TargetRegionName { get; set; }

        public List<string> TargetResoureTypes { get; set; }
    }
}
