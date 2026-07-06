using DocumentFormat.OpenXml.InkML;
using GCTL.Core.Repository;
using GCTL.Core.ViewModels.ClearAndF.Update;
using GCTL.Data.Models;
using GCTL.Service.BillService;
using GCTL.Service.ClearAndF.SepDocumentation;
using GCTL.Service.Language;
using GCTL.Service.UserProfile;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.CodeAnalysis.Elfie.Serialization;
using Microsoft.EntityFrameworkCore;

namespace GCTL_App.Controllers.CNF
{
    public class JobBillEntryController : BaseController
    {

        private readonly IBillService _billService;
        private readonly IGenericRepository<CF_Def_ExpenseType> _shipmentModeRepository;
        private readonly IGenericRepository<Sales_Def_PortName> _portNameRepository;
        private readonly IGenericRepository<CA_Def_Currency> _currencyRepository;
        private readonly IGenericRepository<RMG_Prod_Def_UnitType> _unitRepository;
        private readonly IGenericRepository<CF_Def_PlaceOfLoading> _placeofLoadingRepository;
        private readonly IGenericRepository<CF_Def_ShedYard> _shedYardRepository;
        private readonly IGenericRepository<CF_Def_ExpenseType> _shipmentStatusRepository;
        private readonly IGenericRepository<Sales_Def_ImporterInfo> _importerInfoRepository;
        private readonly IGenericRepository<Sep_Documentation> _sepDocumentionRepository;
        private readonly IGenericRepository<Sales_Customer> _customerRepository;
        private readonly IGenericRepository<Country> _countryRepository;
        private readonly IGenericRepository<Sales_ContactPerson> _contactPersionRepository;

        private readonly IGenericRepository<CF_OperationalFundRequisitionDetails> _ofrdRepository;
        private readonly IGenericRepository<CF_Def_ExpenseHead> _expHeadRepository;
        private readonly IGenericRepository<Core_ServiceType> _stRepository;
        private readonly IGenericRepository<CustomerWiseCostingDetailsEntry> _cwcdeRepository;
        private readonly IGenericRepository<CustomerWiseCostingEntry> _cwcRepository;
        private readonly IGenericRepository<CustomerHandilingCharge> _cRepository;

        private readonly ISepDocumentationService _service;
        public JobBillEntryController(ITranslateService translateService, IUserProfileService userProfileService, IBillService billService, IGenericRepository<CF_Def_ExpenseType> shipmentModeRepository, IGenericRepository<Sales_Def_PortName> portNameRepository, IGenericRepository<CA_Def_Currency> currencyRepository, IGenericRepository<RMG_Prod_Def_UnitType> unitRepository, IGenericRepository<CF_Def_PlaceOfLoading> placeofLoadingRepository, IGenericRepository<CF_Def_ShedYard> shedYardRepository, IGenericRepository<CF_Def_ExpenseType> shipmentStatusRepository, IGenericRepository<Sales_Def_ImporterInfo> importerInfoRepository, IGenericRepository<Sep_Documentation> sepDocumentionRepository, IGenericRepository<Sales_Customer> customerRepository, IGenericRepository<Country> countryRepository, IGenericRepository<Sales_ContactPerson> contactPersionRepository, ISepDocumentationService service, IGenericRepository<CF_OperationalFundRequisitionDetails> ofrdRepository, IGenericRepository<CF_Def_ExpenseHead> expHeadRepository, IGenericRepository<Core_ServiceType> stRepository, IGenericRepository<CustomerWiseCostingDetailsEntry> cwcdeRepository, IGenericRepository<CustomerHandilingCharge> cRepository, IGenericRepository<CustomerWiseCostingEntry> cwcRepository) : base(translateService, userProfileService)
        {
            _billService = billService;
            _shipmentModeRepository = shipmentModeRepository;
            _portNameRepository = portNameRepository;
            _currencyRepository = currencyRepository;
            _unitRepository = unitRepository;
            _placeofLoadingRepository = placeofLoadingRepository;
            _shedYardRepository = shedYardRepository;
            _shipmentStatusRepository = shipmentStatusRepository;
            _importerInfoRepository = importerInfoRepository;
            _sepDocumentionRepository = sepDocumentionRepository;
            _customerRepository = customerRepository;
            _countryRepository = countryRepository;
            _contactPersionRepository = contactPersionRepository;
            _service = service;
            _ofrdRepository = ofrdRepository;
            _expHeadRepository = expHeadRepository;
            _stRepository = stRepository;
            _cwcdeRepository = cwcdeRepository;
            _cRepository = cRepository;
            _cwcRepository = cwcRepository;
        }

        public IActionResult Index()
        {

            ViewBag.ShipmentMode = new SelectList(_shipmentModeRepository.All().Select(e => new { id = e.ExpenseTypeID, name = e.ExpenseType }), "id", "name");
            ViewBag.CustomerName = new SelectList(_service.GetCustomers(), "Value", "Text");
            ViewBag.CustomerDeliveryAddress = new SelectList(_service.GetAddresses(), "Value", "Text");
            ViewBag.PortName = new SelectList(_portNameRepository.All().Select(e => new { id = e.PortNameId, name = e.PortName }), "id", "name");
            ViewBag.CurrencyForLC = new SelectList(_currencyRepository.All().Select(e => new { id = e.CurrencyId, name = e.CurrencyName }), "id", "name");
            ViewBag.CurrencyForInvoice = new SelectList(_currencyRepository.All().Select(e => new { id = e.CurrencyId, name = e.CurrencyName }), "id", "name");
            ViewBag.QuantityUnit = new SelectList(_unitRepository.All().Select(e => new { id = e.UnitTypID, name = e.UnitTypeName }), "id", "name");
            ViewBag.WeightUnit = new SelectList(_unitRepository.All().Select(e => new { id = e.UnitTypID, name = e.UnitTypeName }), "id", "name");
            ViewBag.PlaceOfLoading = new SelectList(_placeofLoadingRepository.All().Select(e => new { id = e.PlaceOfLoadingID, name = e.PlaceOfLoadingName }), "id", "name");
            ViewBag.ShedYard = new SelectList(_shedYardRepository.All().Select(e => new { id = e.ShedYardID, name = e.ShedYardName }), "id", "name");
            ViewBag.ShipmentStatus = new SelectList(_shipmentStatusRepository.All().Select(e => new { id = e.ExpenseTypeID, name = e.ExpenseType }), "id", "name");
            ViewBag.ImporterName = new SelectList(_importerInfoRepository.All().Select(e => new { id = e.ImporterID, name = e.Name }), "id", "name");
            ViewBag.FreightCharge = new SelectList(new List<string> { "Prepaid", "Collect" }, null);

            ViewBag.CountryList = new SelectList(_countryRepository.All().Select(e => new { id = e.CountryCode, name = e.CountryName }), "id", "name");
            ViewBag.CustomerList = new SelectList(_customerRepository.All().Select(e => new { id = e.CustomerCode, name = e.CustomerName }), "id", "name");
            ViewBag.ContactP = new SelectList(_contactPersionRepository.All().Select(e => new { id = e.CPID, name = e.ContactPersonName }), "id", "name");
            ViewBag.Head = new SelectList(_cRepository.All().Select(e => new { id = e.TC, name = e.CHCHead }), "id", "name");

            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GetJobs(int page = 1, int pageSize = 5, string? customerName = null, string? shipmentMode = null, string? dateFrom = null, string? dateTo = null, string? search = null)
        {
            var result = await _billService.GetBillPagedAsync(page, pageSize, customerName, shipmentMode, dateFrom, dateTo, search);

            return Json(new
            {
                data = result.Data,
                totalRecords = result.TotalRecords,
                filteredRecords = result.FilteredRecords // usually same as total if no search, but good to have
            });
        }



        [HttpGet]
        public async Task<IActionResult> GetExpDetailTbl(int id)
        {
            var job =  await _sepDocumentionRepository.All().Select(h => new
            {
                Id = Convert.ToInt16(h.TC),
                JobNo = h.JobNo
            }).FirstOrDefaultAsync(e=>e.Id == id);

            if (job == null || job.JobNo == null)
            {
                return Json(new {success = false });
            }

            var result = from fr in _ofrdRepository.All()
                         join eh in _expHeadRepository.All()
                             on fr.ExpenseHeadID equals eh.ExpenseHeadID
                         join st in _stRepository.All()
                             on fr.ServiceTypeID equals st.ServiceTypeID
                         where fr.JobNo == job.JobNo
                         select new
                         {
                             head =  eh.ExpenseHead,
                             isRec =  eh.IsReceiptable,
                             serviceType = st.ServiceTypeName,
                             confirmAmt = fr.ActualAmount,
                             billAmt = fr.ActualAmount
                         };


            return Json(new { success = true  , data = result});

        }

        [HttpGet]
        public async Task<IActionResult> GetChrgTblData(int id)
        {
            try
            {
                var job = await _sepDocumentionRepository.All()
                    .Select(h => new
                    {
                        Id = Convert.ToInt16(h.TC),
                        JobNo = h.JobNo,
                        customer = h.CustomerID,
                        shipment = h.ExpenseTypeID
                    })
                    .FirstOrDefaultAsync(e => e.Id == id);

                if (job == null || job.JobNo == null)
                {
                    return Json(new { success = false });
                }

                var query = from cwde in _cwcdeRepository.All()
                            where cwde.CustomerID == job.customer
                                  && cwde.ExpenseTypeID == job.shipment
                            join chc in _cRepository.All()
                                on cwde.CHCID equals chc.CHCID into gj
                            from subChc in gj.DefaultIfEmpty()
                            select new { cwde, subChc };

                var result = (await query.ToListAsync())
                             .Select(x => new {
                                 isRec = x.subChc?.IsReceiptable ?? "",
                                 chc = x.subChc?.CHCHead,
                                 description = x.subChc?.CHCHeadDescription,
                                 billAmt = x.cwde.BillAmount,
                                 tc = x.cwde.TC
                             });

                return Json(new { success = true, data = result });
            }
            catch (Exception ex)
            {
                // optional: log ex.Message
                return Json(new { success = false, error = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetJobDetail(int id)
        {
            var job = await _billService.GetJobDetailAsync(id);
            return Json(job);
        }

        [HttpPost]
        public async Task<IActionResult> AddCost(int headId, decimal cost, string jobNo, int customerId, decimal tc)
        {
            await _cwcRepository.BeginTransactionAsync();

            try
            {
                // Find CHCID from headId
                var head = await _cRepository.All()
                    .Where(e => e.TC == headId)
                    .Select(e => e.CHCID)
                    .FirstOrDefaultAsync();

                // Find job info from sepDocumention
                var sep = await _sepDocumentionRepository.All()
                    .Where(e => e.TC == tc)
                    .Select(e => new
                    {
                        tc = e.TC,
                        jobNo = e.JobNo,
                        customerId = e.CustomerID,
                        expenceType = e.ExpenseTypeID
                    })
                    .FirstOrDefaultAsync();

                if (sep == null)
                {
                    return Json(new { success = false, error = "Job not found" });
                }

                int cemp = await GetCurrentEmployeeIdAsync() ?? 0;
                
                var lastEntry = await _cwcdeRepository.All().OrderByDescending(x => x.TC).FirstOrDefaultAsync();
                var lastEntry11 = await _cwcRepository.All().OrderByDescending(x => x.TC).FirstOrDefaultAsync();

                string nextId = lastEntry == null ? "0001" : (int.Parse(lastEntry.CostingDetailsID) + 1).ToString("D4");
                string nextId11 = lastEntry11 == null ? "0001" : (int.Parse(lastEntry11.CostingID) + 1).ToString("D4");

                var entry1 = new CustomerWiseCostingEntry
                {
                    CostingID = nextId,
                   
                    CustomerID = sep.customerId,
                    ExpenseTypeID = sep.expenceType,
                    CompanyCode = "GCTL",
                    EmployeeID = cemp.ToString()

                };

                await _cwcRepository.AddAsync(entry1);

                // Save to DB
                var entry = new CustomerWiseCostingDetailsEntry
                {
                    CostingDetailsID = nextId,
                    BillAmount = cost,
                    CostingID = entry1.CostingID,
                    
                    CustomerID = sep.customerId,
                    CHCID = head,
                    CompanyCode = "GCTL",
                    ExpenseTypeID = sep.expenceType,
                    EmployeeID = cemp.ToString()

                };

                await _cwcdeRepository.AddAsync(entry);
                await _cwcRepository.CommitTransactionAsync();


                return Json(new { success = true , sepTC = sep.tc });
            }
            catch (Exception ex)
            {
                await _cwcRepository.RollbackTransactionAsync();
                return Json(new { success = false, error = ex.Message });
            }
        }



        [HttpPost]
        public async Task<IActionResult> DeleteCharge(decimal tc)
        {
            try
            {
                var entry = await _cwcdeRepository.All()
                    .FirstOrDefaultAsync(e => e.TC == tc);

                if (entry == null)
                    return Json(new { success = false, error = "Not found" });

                await _cwcdeRepository.DeleteAsync(tc);
                //await _cwcdeRepository.SaveChangesAsync();

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = ex.Message });
            }
        }



    }
}
