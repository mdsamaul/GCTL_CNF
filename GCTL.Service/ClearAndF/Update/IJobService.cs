using GCTL.Core.ViewModels;
using GCTL.Core.ViewModels.ClearAndF.Update;

namespace GCTL.Service.ClearAndF.Update
{
    public interface IJobService
    {
        Task<ShipmentUpdateViewModel> GetJobBottomDetailsAsync(string jobNo, decimal id);
        Task<JobDetailDto> GetJobDetailAsync(int id);
        Task<SepPagedResult<JobDto>> GetJobsPagedAsync(int page, int pageSize, string? customerName, string? shipmentMode, string? dateFrom, string? dateTo, string? search);
        Task<SepPagedResult<JobStatusDto>> GetJobStatusesPagedAsync(int page, int pageSize, string? search);
        Task<JobDetailDto> GetJobTopDetailsAsync(string jobNo);
        Task<CommonReturnViewModel> UpdateJobs(ShipmentUpdateViewModel model, int? company);
    }
}