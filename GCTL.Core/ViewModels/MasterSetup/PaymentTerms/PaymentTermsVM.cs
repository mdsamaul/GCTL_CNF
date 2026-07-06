using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Core.ViewModels.MasterSetup.PaymentTerms
{
    public class PaymentTermsVM
    {
        public string PaymentTermsCode { get; set; }

        public string PaymentTermsName { get; set; }

        public string Percentise { get; set; }

        public string Type { get; set; }

        public int? CreditDays { get; set; }

        public decimal TC { get; set; }
    }
}
