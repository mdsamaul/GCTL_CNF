using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Core.ViewModels.MasterSetup.PaymentType
{
    public class PaymentTypeVM
    {
        public int? TC { get; set; }

        public string PaymentTypeID { get; set; }

        [Required(ErrorMessage = "Payment Type is required")]
        public string PaymentType { get; set; }

        public string? ShortName { get; set; }

        public string? LUser { get; set; }

        public DateTime? LDate { get; set; }

        public string? LIP { get; set; }

        public string? LMAC { get; set; }

        public DateTime? ModifyDate { get; set; }
    }
}
