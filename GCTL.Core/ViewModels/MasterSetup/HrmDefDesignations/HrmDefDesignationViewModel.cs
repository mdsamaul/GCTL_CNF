using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Core.ViewModels.MasterSetup.HrmDefDesignations
{
    public class HrmDefDesignationViewModel
    {
        public decimal AutoId { get; set; }

        public string DesignationCode { get; set; } = null!;

        public string? DesignationName { get; set; }

        public string? DesignationShortName { get; set; }

        public string? GradeCode { get; set; }

        public string? Luser { get; set; }

        public DateTime? Ldate { get; set; }

        public string? Lip { get; set; }

        public string? Lmac { get; set; }

        public DateTime? ModifyDate { get; set; }

        public string? StepNoId { get; set; }

        public string? BanglaDesignation { get; set; }

        public string? BanglaShortName { get; set; }
    }
}
