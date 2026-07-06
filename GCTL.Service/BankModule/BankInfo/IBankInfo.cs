using GCTL.Core.ViewModels.BankModule.BankInfo;
using GCTL.Data.Models;
using GCTL.Service.Pagination;


namespace GCTL.Service.BankModule.BankInfo
{
    public interface IBankInfo
    {
        Task<List<BankInfoVM>> GetAllAsync();
        Task<BankInfoVM> GetByIdAsync(int id);
        Task<bool> SaveAsync(BankInfoVM model);
        Task<bool> UpdateAsync(BankInfoVM model);
        Task<bool> DeleteAsync(decimal id);
        Task<bool> IsExistAsync(int id);
        Task<PaginationService<SALES_Def_BankInfo, BankInfoVM>.PaginationResult<BankInfoVM>> GetPaginatedAsync(int pageNumber = 1, int pageSize = 10, string searchTerm = "",
        string sortColumn = "BankID", string sortOrder = "desc");
        Task<string> GetLastBankIDAsync();
        Task<bool> BulkDeleteAsync(List<decimal> ids);
        Task<bool> IsDuplicateAsync(BankInfoVM model);

        //For Bank DropDown
        Task<List<BankInfoVM>> GetAllBankAsync();


    }
}
