using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Core.ViewModels.MasterSetup.PaymentMode
{
    public class PaymentModeVM
    {
        public int? AutoId { get; set; }
        public string PaymentModeID { get; set; }

        [Required(ErrorMessage = "Payment Mode is required")]
        public string PaymentModeName { get; set; }

        public string? PaymentModeShortName { get; set; }

        public string? LUser { get; set; }

        public DateTime? LDate { get; set; }

        public string? LIP { get; set; }

        public string? LMAC { get; set; }

        public DateTime? ModifyDate { get; set; }

        public string? CompanyCode { get; set; }

        public string? EmployeeID { get; set; }
    }
}
