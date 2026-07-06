using GCTL.Core.ViewModels.MasterSetup.CurrencyType;
using GCTL.Data.Models;
using GCTL.Service.Pagination;


namespace GCTL.Service.MasterSetup.CurrencyType
{
    public interface ICurrencyType
    {
        Task<List<CurrencyTypeVM>> GetAllAsync();
        Task<CurrencyTypeVM> GetByIdAsync(decimal id);
        Task<bool> SaveAsync(CurrencyTypeVM model);
        Task<bool> UpdateAsync(CurrencyTypeVM model);
        Task<bool> DeleteAsync(decimal id);
        Task<bool> IsExistAsync(decimal id);
        Task<PaginationService<CA_Def_Currency, CurrencyTypeVM>.PaginationResult<CurrencyTypeVM>> GetPaginatedAsync(int pageNumber = 1, int pageSize = 10, string searchTerm = "",
        string sortColumn = "CurrencyId", string sortOrder = "asc");
        Task<string> GetLastCurrencyAsync();
        Task<bool> BulkDeleteAsync(List<decimal> ids);
        Task<bool> IsDuplicateAsync(CurrencyTypeVM model);

        Task<List<CurrencyTypeVM>> GetAllCurrencyAsync();
    }
}
