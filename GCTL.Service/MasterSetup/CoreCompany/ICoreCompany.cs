using GCTL.Core.ViewModels.MasterSetup.CoreCompany;
using GCTL.Data.Models;
using GCTL.Service.Pagination;


namespace GCTL.Service.MasterSetup.CoreCompany
{
    public interface ICoreCompany
    {
        Task<List<CoreCompanyVM>> GetAllAsync();
        Task<CoreCompanyVM> GetByIdAsync(int id);
        Task<bool> SaveAsync(CoreCompanyVM model);
        Task<bool> UpdateAsync(CoreCompanyVM model);
        Task<bool> DeleteAsync(int id);
        Task<bool> IsExistAsync(int id);
        Task<PaginationService<Core_Company, CoreCompanyVM>.PaginationResult<CoreCompanyVM>> GetPaginatedAsync(int pageNumber = 1, int pageSize = 10, string searchTerm = "",string sortColumn = "CompanyCode", string sortOrder = "asc");
        Task<string> GetLastCompanyCodeAsync();
        Task<bool> BulkDeleteAsync(List<int> ids);
        Task<bool> IsDuplicateAsync(CoreCompanyVM model);

        //For Comapany DropDown
        Task<List<CoreCompanyVM>> DropdownCompany();
    }
}
