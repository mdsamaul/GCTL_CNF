using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GCTL.Core.ViewModels.MasterSetup.CoreCountries;
using GCTL.Core.ViewModels.MasterSetup.VendorPrefix;
using GCTL.Data.Models;
using GCTL.Service.Pagination;

namespace GCTL.Service.MasterSetup.VendorPrefix
{
    public interface IVendorPrefixService
    {
        Task<List<VendorPrefixVM>> GetAllAsync();
        Task<VendorPrefixVM> GetByIdAsync(int id);
        Task<bool> SaveAsync(VendorPrefixVM model);
        Task<bool> UpdateAsync(VendorPrefixVM model);
        Task<bool> DeleteAsync(int id);
        Task<bool> IsExistAsync(int id);
        Task<PaginationService<CF_Def_VendorPrefix, VendorPrefixVM>.PaginationResult<VendorPrefixVM>> GetPaginatedAsync(int pageNumber = 1, int pageSize = 5, string searchTerm = "",
        string sortColumn = "VendorPrifixId", string sortOrder = "asc");
        Task<string> GetLastCountryCodeAsync();
        Task<bool> BulkDeleteAsync(List<int> ids);
        Task<bool> IsDuplicateAsync(VendorPrefixVM model);
    }
}
