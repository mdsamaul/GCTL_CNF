using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Core.ViewModels.MasterSetup.CoreCountries
{
    public class CoreCountryViewModel
    {
        public int CountryCode { get; set; }

        public string CountryId { get; set; } = null!;

        public string? CountryName { get; set; }

        public string? Ioccode { get; set; }

        public string? Isocode { get; set; }

        public DateTime? Ldate { get; set; }

        public string? Luser { get; set; }

        public string? Lip { get; set; }

        public string? Lmac { get; set; }

        public DateTime? ModifyDate { get; set; }
    }
}
