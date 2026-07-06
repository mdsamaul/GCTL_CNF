using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GCTL.Core.ViewModels.MasterSetup.HRMDefDepartment;
using GCTL.Data.Models;
using GCTL.Service.Pagination;

namespace GCTL.Service.MasterSetup.HrmDefDepartment
{
    public interface IDepartment
    {
        Task<List<DepartmentVM>> GetAllAsync();
        Task<DepartmentVM> GetByIdAsync(int id);
        Task<bool> SaveAsync(DepartmentVM model);
        Task<bool> UpdateAsync(DepartmentVM model);
        Task<bool> DeleteAsync(decimal id);
        Task<bool> IsExistAsync(int id);
        Task<PaginationService<HRM_Def_Department, DepartmentVM>.PaginationResult<DepartmentVM>> GetPaginatedAsync(int pageNumber = 1, int pageSize = 10, string searchTerm = "",
        string sortColumn = "DepartmentCode", string sortOrder = "desc");
        Task<string> GetLastDepartmentCodeAsync();
        Task<bool> BulkDeleteAsync(List<decimal> ids);
        Task<bool> IsDuplicateAsync(DepartmentVM model);
    }
}
