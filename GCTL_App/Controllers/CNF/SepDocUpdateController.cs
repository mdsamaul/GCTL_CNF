using GCTL.Core.Repository;
using GCTL.Core.ViewModels.ClearAndF.Update;
using GCTL.Data.Models;
using GCTL.Service.ClearAndF.SepDocumentation;
using GCTL.Service.ClearAndF.Update;
using GCTL.Service.Language;
using GCTL.Service.UserProfile;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Web.Helpers;

namespace GCTL_App.Controllers.ClearAndF.Update
{
    public class SepDocUpdateController : BaseController
    {

        #region CTOR

        private readonly IJobService _jobService;

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
        private readonly ISepDocumentationService _service;

        public SepDocUpdateController(ITranslateService translateService, IUserProfileService userProfileService, IJobService jobService, IGenericRepository<CF_Def_ExpenseType> shipmentModeRepository, IGenericRepository<Sales_Def_PortName> portNameRepository, IGenericRepository<CA_Def_Currency> currencyRepository, IGenericRepository<RMG_Prod_Def_UnitType> unitRepository, IGenericRepository<CF_Def_PlaceOfLoading> placeofLoadingRepository, IGenericRepository<CF_Def_ShedYard> shedYardRepository, IGenericRepository<CF_Def_ExpenseType> shipmentStatusRepository, IGenericRepository<Sales_Def_ImporterInfo> importerInfoRepository, IGenericRepository<Sep_Documentation> sepDocumentionRepository, IGenericRepository<Sales_Customer> customerRepository, IGenericRepository<Country> countryRepository, IGenericRepository<Sales_ContactPerson> contactPersionRepository, ISepDocumentationService service) : base(translateService, userProfileService)
        {
            _jobService = jobService;
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
        }
        #endregion

        #region Index
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

            return View();
        }

        #endregion

        [HttpGet]
        public async Task<IActionResult> GetJobs(int page = 1, int pageSize = 5, string? customerId = null, string? shipmentMode = null, DateTime? dateFrom = null, DateTime? dateTo = null, string? search = null)
        {
            var result = await _jobService.GetJobsPagedAsync(page, pageSize, customerId, shipmentMode, dateFrom, dateTo, search);

            return Json(new
            {
                data = result.Data,
                totalRecords = result.TotalRecords,
                filteredRecords = result.FilteredRecords // usually same as total if no search, but good to have
            });
        }

        [HttpGet]
        public async Task<IActionResult> GetJobDetail(int id)
        {
            var job = await _jobService.GetJobDetailAsync(id);
            return Json(job);
        }


        [HttpGet]
        public async Task<IActionResult> GetJobDetails(string jobNo , decimal id)
        {
            var jobTop = await _jobService.GetJobTopDetailsAsync(jobNo);
            var jobBottom = await _jobService.GetJobBottomDetailsAsync(jobNo , id);

            // Anonymous object হিসেবে JSON রিটার্ন করা হবে
            return Ok(new { jobTop, jobBottom });
        }




        [HttpGet]
        public async Task<IActionResult> GetJobStatuses(int page = 1,int pageSize = 10,string? search = null,string? customerCode = null,DateTime? fromDate = null,DateTime? toDate = null)
        {
            var result = await _jobService.GetJobStatusesPagedAsync(
                page,
                pageSize,
                search,
                customerCode,
                fromDate,
                toDate);

            return Json(new
            {
                data = result.Data,
                filteredRecords = result.FilteredRecords
            });
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ShipmentUpdateViewModel model)
        {

            //var company = 1;// await GetCurrentOrganizationIdAsync();
            var company = await GetCurrentOrganizationIdAsync();
            var result = await _jobService.UpdateJobs(model, company);
            return Ok(result);

        }


    }
}
