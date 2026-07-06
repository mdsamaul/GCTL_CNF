
using System.Threading.Tasks;
using GCTL.Core.ViewModels.ClearAndF.BillEntry;
using GCTL.Core.ViewModels.ClearAndF.Update;

namespace GCTL.Service.BillService
{
    public interface IBillService
    {
        Task<SepPagedResult<JobDto>> GetBillPagedAsync(int page, int pageSize, string? customerName, string? shipmentMode, string? dateFrom, string? dateTo, string? search);
        Task<BillEntryJobVM> GetJobDetailAsync(int id);
    }
}