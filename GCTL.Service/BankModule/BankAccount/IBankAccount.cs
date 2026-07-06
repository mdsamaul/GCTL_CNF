using GCTL.Core.ViewModels.BankModule.CoreAccount;
using GCTL.Data.Models;
using GCTL.Service.Pagination;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Service.BankModule.BankAccount
{
    public interface IBankAccount
    {
        Task<List<CoreAccountVM>> GetAllAsync();
        Task<CoreAccountVM> GetByIdAsync(int id);
        Task<bool> SaveAsync(CoreAccountVM model);
        Task<bool> UpdateAsync(CoreAccountVM model);
        Task<bool> DeleteAsync(decimal id);
        Task<bool> IsExistAsync(int id);
        Task<PaginationService<Core_BankAccountInformation, CoreAccountVM>.PaginationResult<CoreAccountVM>> GetPaginatedAsync(int pageNumber = 1, int pageSize = 10, string searchTerm = "",
        string sortColumn = "AccInfoID", string sortOrder = "desc");
        Task<string> GetLastAccountInfoIDAsync();
        Task<bool> BulkDeleteAsync(List<decimal> ids);
        Task<bool> IsDuplicateAsync(CoreAccountVM model);

        //For DropDown
        //Task<List<BankInfoVM>> GetAllBankAsync();
    }
}
