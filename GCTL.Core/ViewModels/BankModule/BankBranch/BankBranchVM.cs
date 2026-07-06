using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Core.ViewModels.BankModule.BankBranch
{
    public class BankBranchVM
    {
        public decimal? AutoID { get; set; }

        public string BankBranchID { get; set; }

        public string BankBranchName { get; set; }

        public string? ShortName { get; set; }

        public string BankID { get; set; }

        public string SWIFTCode { get; set; }

        public string? Address { get; set; }

        public string? Phone { get; set; }

        public DateTime? LDate { get; set; }

        public string? LUser { get; set; }

        public string? LIP { get; set; }

        public string? LMAC { get; set; }

        public DateTime? ModifyDate { get; set; }
        public string? BankName { get; set; }
    }
}
