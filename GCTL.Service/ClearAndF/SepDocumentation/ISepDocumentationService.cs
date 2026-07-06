using GCTL.Core.ViewModels;
using GCTL.Data.Models;
using System.Collections;

namespace GCTL.Service.ClearAndF.SepDocumentation
{
    public interface ISepDocumentationService
    {
        Task<CommonReturnViewModel> AddDocumentation(Sep_Documentation model);
        Task<CommonReturnViewModel> UpdateDocumentation(Sep_Documentation model);
        Task<CommonReturnViewModel> DeleteDocumentation(string jobNo);
        Task<(List<DocumentationResult> Data, int TotalRecords)> GetDocumentationList(int page, int pageSize, string search, string sortColumn, string sortDirection, DateTime? dateFrom, DateTime? dateTo, string customerId);
        List<SelectListItem> GetAddresses();
        List<SelectListItem> GetCustomers();
        Sep_Documentation GetDocumentationByTC(decimal tc);
    }
}