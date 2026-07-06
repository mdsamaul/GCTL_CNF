using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Core.ViewModels.BankModule.BankInfo
{
    public class BankInfoVM
    {
        public decimal? AutoID { get; set; }

        public string BankID { get; set; }

        public string BankName { get; set; }

        public string? ShortName { get; set; }

        public DateTime? LDate { get; set; }

        public string? LUser { get; set; }

        public string? LIP { get; set; }

        public string? LMAC { get; set; }

        public DateTime? ModifyDate { get; set; }
    }
}
