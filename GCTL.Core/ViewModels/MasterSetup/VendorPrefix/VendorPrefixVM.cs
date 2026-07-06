using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Core.ViewModels.MasterSetup.VendorPrefix
{
    public class VendorPrefixVM
    {
        public int TC { get; set; }
        public string VendorPrifixID { get; set; }
        public string PrifixName { get; set; }
        public string? ShortName { get; set; }
        public string? LUser { get; set; }
        public DateTime? LDate { get; set; }
        public string? LIP { get; set; }
        public string? LMAC { get; set; }
        public DateTime? ModifyDate { get; set; }
    }
}
