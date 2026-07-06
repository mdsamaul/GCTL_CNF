using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GCTL.Core.ViewModels.MasterSetup.PaymentMode;
using GCTL.Core.ViewModels.MasterSetup.PaymentType;
using GCTL.Data.Models;
using GCTL.Service.Pagination;

namespace GCTL.Service.MasterSetup.PaymentType
{
    public interface IPaymentType
    {
        Task<List<PaymentTypeVM>> GetAllAsync();
        Task<PaymentTypeVM> GetByIdAsync(int id);
        Task<bool> SaveAsync(PaymentTypeVM model);
        Task<bool> UpdateAsync(PaymentTypeVM model);
        Task<bool> DeleteAsync(int id);
        Task<bool> IsExistAsync(int id);
        Task<PaginationService<Sales_Def_PaymentType, PaymentTypeVM>.PaginationResult<PaymentTypeVM>> GetPaginatedAsync(int pageNumber = 1, int pageSize = 5, string searchTerm = "",
        string sortColumn = "PaymentTypeID", string sortOrder = "asc");
        Task<string> GetLastPaymentTypeAsync();
        Task<bool> BulkDeleteAsync(List<int> ids);
        Task<bool> IsDuplicateAsync(PaymentTypeVM model);
    }
}
