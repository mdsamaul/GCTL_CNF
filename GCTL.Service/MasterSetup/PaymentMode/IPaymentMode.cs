using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GCTL.Core.ViewModels.MasterSetup.PaymentMode;
using GCTL.Core.ViewModels.MasterSetup.VendorPrefix;
using GCTL.Data.Models;
using GCTL.Service.Pagination;

namespace GCTL.Service.MasterSetup.PaymentMode
{
    public interface IPaymentMode
    {
        Task<List<PaymentModeVM>> GetAllAsync();
        Task<PaymentModeVM> GetByIdAsync(int id);
        Task<bool> SaveAsync(PaymentModeVM model);
        Task<bool> UpdateAsync(PaymentModeVM model);
        Task<bool> DeleteAsync(int id);
        Task<bool> IsExistAsync(int id);
        Task<PaginationService<Sales_Def_PaymentMode, PaymentModeVM>.PaginationResult<PaymentModeVM>> GetPaginatedAsync(int pageNumber = 1, int pageSize = 5, string searchTerm = "",
        string sortColumn = "PaymentModeID", string sortOrder = "asc");
        Task<string> GetLastPaymentModeAsync();
        Task<bool> BulkDeleteAsync(List<int> ids);
        Task<bool> IsDuplicateAsync(PaymentModeVM model);
    }
}
