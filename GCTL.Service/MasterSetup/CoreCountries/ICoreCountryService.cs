using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GCTL.Core.ViewModels.MasterSetup.CoreCountries;
//using GCTL.Core.ViewModels.PaymentManagements;
using GCTL.Data.Models;
using GCTL.Service.Pagination;


namespace GCTL.Service.MasterSetup.CoreCountries
{
    public interface ICoreCountryService
    {
        Task<List<CoreCountryViewModel>> GetAllAsync();
        Task<CoreCountryViewModel> GetByIdAsync(int id);
        Task<bool> SaveAsync(CoreCountryViewModel model);
        Task<bool> UpdateAsync(CoreCountryViewModel model);
        Task<bool> DeleteAsync(int id);
        Task<bool> IsExistAsync(int id);
        Task<PaginationService<Core_Country, CoreCountryViewModel>.PaginationResult<CoreCountryViewModel>> GetPaginatedAsync(int pageNumber = 1, int pageSize = 5, string searchTerm = "",
        string sortColumn = "FullName", string sortOrder = "asc");
        Task<string> GetLastCountryCodeAsync();
        Task<bool> BulkDeleteAsync(List<int> ids);
        Task<bool> IsDuplicateAsync(CoreCountryViewModel model);


        //For Country DropDown
        Task<List<CoreCountryViewModel>> GetAllCountryAsync();
    }
}
