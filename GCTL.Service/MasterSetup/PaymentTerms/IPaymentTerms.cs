using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GCTL.Core.ViewModels.MasterSetup.PaymentTerms;
using GCTL.Data.Models;
using GCTL.Service.Pagination;

namespace GCTL.Service.MasterSetup.PaymentTerms
{
    public interface IPaymentTerms
    {
        Task<List<PaymentTermsVM>> GetAllAsync();
        Task<PaymentTermsVM> GetByIdAsync(decimal id);
        Task<bool> SaveAsync(PaymentTermsVM model);
        Task<bool> UpdateAsync(PaymentTermsVM model);
        Task<bool> DeleteAsync(decimal id);
        Task<bool> IsExistAsync(decimal id);
        Task<PaginationService<Sales_Def_PaymentTerms, PaymentTermsVM>.PaginationResult<PaymentTermsVM>> GetPaginatedAsync(int pageNumber = 1, int pageSize = 5, string searchTerm = "",
        string sortColumn = "PaymentTermsCode", string sortOrder = "asc");
        Task<string> GetLastPaymentTermsAsync();
        Task<bool> BulkDeleteAsync(List<decimal> ids);
        Task<bool> IsDuplicateAsync(PaymentTermsVM model);
    }
}
