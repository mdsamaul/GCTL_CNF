using GCTL.Core.ViewModels;
using GCTL.Core.ViewModels.DeleteHistories;
using GCTL.Core.ViewModels.MasterSetup.CoreBranch;
using GCTL.Data.Models;
using GCTL.Service.Pagination;

namespace GCTL.Service.MasterSetup.CoreBranch
{
    public interface ICoreBranch
    {
        Task<List<CoreBranchVM>> GetAllAsync();
        Task<CoreBranchVM> GetByIdAsync(int id);
        Task<bool> SaveAsync(CoreBranchVM model);
        Task<bool> UpdateAsync(CoreBranchVM model);
        Task<bool> DeleteAsync(decimal id);
        Task<bool> IsExistAsync(int id);
        Task<PaginationService<Core_Branch, CoreBranchVM>.PaginationResult<CoreBranchVM>> GetPaginatedAsync(int pageNumber = 1, int pageSize = 10, string searchTerm = "", string sortColumn = "BranchCode", string sortOrder = "desc");
        Task<string> GetLastBranchCodeAsync();
        Task<CommonReturnViewModel> BulkDeleteAsync(List<decimal> ids, DeleteHistoryViewModel model);
        Task<bool> IsDuplicateAsync(CoreBranchVM model);

        #region Core Company Dropdown

        Task<List<CommonChoiceVM>> LoadCompanyDropdown();

        #endregion
    }
}
