using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GCTL.Core.ViewModels.MasterSetup.CoreCountries;
using GCTL.Core.ViewModels.MasterSetup.HrmDefDesignations;
using GCTL.Data.Models;
using GCTL.Service.Pagination;

namespace GCTL.Service.MasterSetup.HrmDefDesignations
{
    public interface IHrmDefDesignationService
    {
        Task<List<HrmDefDesignationViewModel>> GetAllAsync();
        Task<HrmDefDesignationViewModel> GetByIdAsync(decimal id);
        Task<bool> SaveAsync(HrmDefDesignationViewModel model);
        Task<bool> UpdateAsync(HrmDefDesignationViewModel model);
        Task<bool> DeleteAsync(decimal id);
        Task<bool> IsExistAsync(decimal id);
        Task<PaginationService<HRM_Def_Designation, HrmDefDesignationViewModel>.PaginationResult<HrmDefDesignationViewModel>> GetPaginatedAsync(int pageNumber = 1, int pageSize = 5, string searchTerm = "",
        string sortColumn = "DesignationCode", string sortOrder = "asc");
        Task<string> GetLastDesignationAsync();
        Task<bool> BulkDeleteAsync(List<decimal> ids);
        Task<bool> IsDuplicateAsync(HrmDefDesignationViewModel model);

        // Designation Dropdown
        Task<List<HrmDefDesignationViewModel>> DesignationDropdown();
    }
}
