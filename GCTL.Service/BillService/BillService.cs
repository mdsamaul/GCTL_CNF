using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DocumentFormat.OpenXml.InkML;
using GCTL.Core.Repository;
using GCTL.Core.ViewModels.ClearAndF.BillEntry;
using GCTL.Core.ViewModels.ClearAndF.Update;
using GCTL.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace GCTL.Service.BillService
{
    public class BillService : IBillService
    {
        private readonly IGenericRepository<Sep_Documentation> _sepDocumentionRepository;
        private readonly IGenericRepository<CF_BillEntry> _billEntryRepository;
        private readonly IGenericRepository<CF_DailyJobUpdateEntry> _jobUpdateRepository;
        private readonly IGenericRepository<CF_Def_ExpenseType> _shipmentModeRepository;
        private readonly IGenericRepository<Sales_Customer> _customerRepository;

        public BillService(IGenericRepository<Sep_Documentation> sepDocumentionRepository, IGenericRepository<CF_BillEntry> billEntryRepository, IGenericRepository<CF_DailyJobUpdateEntry> jobUpdateRepository, IGenericRepository<CF_Def_ExpenseType> shipmentModeRepository, IGenericRepository<Sales_Customer> customerRepository)
        {
            _sepDocumentionRepository = sepDocumentionRepository;
            _billEntryRepository = billEntryRepository;
            _jobUpdateRepository = jobUpdateRepository;
            _shipmentModeRepository = shipmentModeRepository;
            _customerRepository = customerRepository;
        }

        public async Task<SepPagedResult<JobDto>> GetBillPagedAsync(int page, int pageSize, string? customerName, string? shipmentMode, string? dateFrom, string? dateTo, string? search)
        {
            try
            {

                var jobNos = _billEntryRepository.All().Where(s => s.JobNo != null).Select(s => s.JobNo);

               

                var query = _sepDocumentionRepository.All().Where(c => !jobNos.Contains(c.JobNo)).Select(h => new JobDto
                {
                    Id = Convert.ToInt16(h.TC),
                    JobNo = h.JobNo,
                    ShipmentMode = _shipmentModeRepository.All().Where(e => e.ExpenseTypeID == h.ExpenseTypeID).Select(e => e.ExpenseType).FirstOrDefault() ?? "",
                    JobDate = h.Date,
                    CustomerName = _customerRepository.All().Where(e => e.CustomerID == h.CustomerID).Select(e => e.CustomerName).FirstOrDefault() ?? "",
                    DocReceivedDate = h.DocReceivedDate
                }).AsNoTracking();


                var totalCount = query.Count();



                // Apply filters
                if (!string.IsNullOrWhiteSpace(customerName))
                    query = query.Where(j => j.CustomerName == customerName);

                if (!string.IsNullOrWhiteSpace(shipmentMode))
                    query = query.Where(j => j.ShipmentMode == shipmentMode);

                if (DateTime.TryParse(dateFrom, out var fromDate))
                    query = query.Where(j => j.JobDate >= fromDate);

                if (DateTime.TryParse(dateTo, out var toDate))
                    query = query.Where(j => j.JobDate <= toDate.AddDays(1).AddTicks(-1)); // include full day

                if (!string.IsNullOrWhiteSpace(search))
                {
                    search = search.ToLower();
                    query = query.Where(j =>
                        j.JobNo.ToLower().Contains(search) ||
                        j.CustomerName.ToLower().Contains(search) ||
                        j.ShipmentMode.ToLower().Contains(search));
                }

                var filteredCount = query.Count();
                

                var data = query
                    .OrderBy(j => j.JobNo) // Important: consistent ordering for stable pagination
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();

                return new SepPagedResult<JobDto>
                {
                    Data = data,
                    TotalRecords = totalCount,
                    FilteredRecords = filteredCount
                };
            }
            catch (Exception)
            {

                return new SepPagedResult<JobDto>
                {
                    Data = new List<JobDto>(),
                    TotalRecords = 0,
                    FilteredRecords = 0
                };
            }
        }




        public async Task<BillEntryJobVM> GetJobDetailAsync(int id)
        {



            var query1 = await _sepDocumentionRepository.All()
             .Select(h => new BillEntryJobVM
             {
                 Id = Convert.ToInt16(h.TC),
                 JobNo = h.JobNo,
                 ShipmentMode = h.ExpenseTypeID,          // h থেকে bind করুন
                 ShipmentModeId = h.ExpenseTypeID,
                 IsCustomJobNo = h.IsCustomJobNo,
                 JobDate = h.Date.Value.ToShortDateString(),
                 CustomerId = h.CustomerID,
                 CustomerName = h.CustomerID,
                 CustomerDeliveryAddress = h.DeliveryLocationCode, //TODO: DElivery add
                 PortId = h.PortId,
                 DocsReceivedDate = h.DocReceivedDate.Value.ToShortDateString(),
                 LcExpNo = h.ExpNo,
                 LcValue = h.LCValue,
                 IpEpNo = h.IPNo,
                 IpDate = h.IPDate.Value.ToString(),
                 ImporterId = h.ImporterID,
                 InvoiceNo = h.InvoiceNo,
                 InvoiceDate = h.InvoiceDate.Value.ToString(),
                 BlNo = h.BENo,
                 BeNo = h.BENo,
                 BeDate = h.BEDate.Value.ToString(),
                 BlDate = h.BLDate.Value.ToString(),
                 ContainerNo = h.ContainerNo,
                 ContainerSize = h.ContainerSize,
                 LcaNo = h.LCANo,
                 DischargeDate = h.Dischargedate.Value.ToString(),
                 MaterialDescription = h.MaterialDescription,
                 Quantity = h.Quntity1,
                 QuantityUnitId = h.Unit1,
                 Weight = h.Quntity2,
                 WeightUnitId = h.Unit2,
                 ForwarderId = h.NameOfFreightForwarder,
                 VesselRottNo = h.Vessel_RottNo,
                 Remarks = h.Remarks,
                 DocReceivedDate = h.DocReceivedDate.Value.ToString(),
                 CreatedAt = h.LDate,
                 UpdatedAt = h.LDate,
             })
             .FirstOrDefaultAsync(j => j.Id == id);


            if (query1 == null)
                throw new Exception("Job not found");

            return query1;
        }







    }
}
