using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Core.ViewModels.BankModule.CoreAccount
{
    public class CoreAccountVM
    {
        public decimal? AutoID { get; set; }

        public string AccInfoID { get; set; }

        public string AccountName { get; set; }

        public string AccountNO { get; set; }

        public string BankID { get; set; }

        public string BranchID { get; set; }

        public string? LUser { get; set; }

        public DateTime? LDate { get; set; }

        public string? LIP { get; set; }

        public string? LMAC { get; set; }

        public DateTime? ModifyDate { get; set; }

        public string? UserInfoEmployeeID { get; set; }

        public string? CompanyCode { get; set; }
        public string? BankName { get; set; }
        public string? BankBranchName { get; set; }
    }
}
