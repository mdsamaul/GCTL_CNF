using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Core.ViewModels.MasterSetup.HRMDefDepartment
{
    public class DepartmentVM
    {
        public decimal? autoId { get; set; }

        public string DepartmentCode { get; set; }

        public string DepartmentName { get; set; }

        public string? DepartmentShortName { get; set; }

        public string? LUser { get; set; }

        public DateTime? LDate { get; set; }

        public string? LIP { get; set; }

        public string? LMAC { get; set; }

        public DateTime? ModifyDate { get; set; }

        public string? BanglaDepartment { get; set; }

        public string? BanglaShortName { get; set; }

        public string? CompanyCode { get; set; }
    }
}
