using GCTL.Core.ViewModels.BankModule.BankBranch;
using GCTL.Data.Models;
using GCTL.Service.Pagination;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Service.BankModule.BankBranch
{
    public interface IBankBranch
    {
        Task<List<BankBranchVM>> GetAllAsync();
        Task<BankBranchVM> GetByIdAsync(int id);
        Task<bool> SaveAsync(BankBranchVM model);
        Task<bool> UpdateAsync(BankBranchVM model);
        Task<bool> DeleteAsync(decimal id);
        Task<bool> IsExistAsync(int id);
        Task<PaginationService<Sales_Def_BankBranchInfo, BankBranchVM>.PaginationResult<BankBranchVM>> GetPaginatedAsync(int pageNumber = 1, int pageSize = 10, string searchTerm = "",
        string sortColumn = "BankBranchID", string sortOrder = "asc", string bankID = "");
        Task<string> GetLastBranchIDAsync();
        Task<bool> BulkDeleteAsync(List<decimal> ids);
        Task<bool> IsDuplicateAsync(BankBranchVM model);

        //For Bank Branch DropDown
        Task<List<BankBranchVM>> GetAllBranchAsync(string bankId);

    }
}
