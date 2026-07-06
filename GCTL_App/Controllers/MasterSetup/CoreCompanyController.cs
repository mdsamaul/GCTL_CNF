using GCTL.Core.ViewModels.MasterSetup.CoreCompany;
using GCTL.Service.Language;
using GCTL.Service.MasterSetup.CoreCompany;
using GCTL.Service.MasterSetup.CoreCountries;
using GCTL.Service.MasterSetup.CurrencyType;
using GCTL.Service.UserProfile;
using GCTL_App.Controllers;
using Microsoft.AspNetCore.Mvc;

namespace GCTL_NBR.Controllers.MasterSetup
{
    public class CoreCompanyController : BaseController
    {
        #region Service

        private readonly ICoreCompany _service;
        private readonly ICoreCountryService _country;
        private readonly ICurrencyType _currency;
        public CoreCompanyController(ITranslateService translateService, IUserProfileService userProfileService, ICoreCompany service, ICoreCountryService coreCountry, ICurrencyType currency) : base(translateService, userProfileService)
        {
            _service = service;
            _country = coreCountry;
            _currency = currency;
        }

        #endregion


        #region View Page
        public IActionResult Index()
        {
            return View();
        }
        public IActionResult CompanyForm()
        {
            return PartialView();
        }
        public IActionResult CompanyList()
        {
            return PartialView();
        }
        #endregion


        #region Generate Company Code

        [HttpGet("next-Company-id")]
        public async Task<string> GenerateNewIdAsync()
        {
            var lastId = await _service.GetLastCompanyCodeAsync();

            string newId;
            if (!string.IsNullOrEmpty(lastId) && int.TryParse(lastId, out int lastNumericId))
            {
                int nextId = lastNumericId + 1;
                newId = nextId.ToString("D3");
            }
            else
            {
                newId = "001";
            }

            return newId;
        }

        #endregion


        #region Get All Company in Grid

        [HttpGet]
        [Route("core-company-list")]
        public async Task<IActionResult> GetAllPaginated(int pageNumber = 1, int pageSize = 10, string searchTerm = "", string sortColumn = "CompanyCode", string sortOrder = "desc")
        {
            try
            {
                var result = await _service.GetPaginatedAsync(pageNumber, pageSize, searchTerm, sortColumn, sortOrder);

                if (result.Data == null || !result.Data.Any())
                {
                    return Ok(new
                    {
                        Data = new List<CoreCompanyVM>(),
                        TotalCount = 0,
                        PaginationInfo = new
                        {
                            StartItem = 0,
                            EndItem = 0,
                            TotalItems = 0,
                            PageNumbers = new List<int>(),
                            TotalPages = 0,
                            CurrentPage = 0
                        },
                        Message = "No Bank Found."
                    });
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred.", error = ex.Message });
            }
        }

        #endregion


        #region Checking Duplicate

        [HttpPost]
        [Route("company/check-duplicate")]
        public async Task<IActionResult> CheckDuplicate([FromBody] CoreCompanyVM model)
        {
            if (model == null) return BadRequest(new { message = "Invalid data." });

            var isDuplicate = await _service.IsDuplicateAsync(model);

            if (isDuplicate)
            {
                return Ok(new { isDuplicate = true, message = "Data Already Exists." });
            }

            return Ok(new { isDuplicate = false, message = "No duplicate found." });
        }

        #endregion


        #region Save Compnay

        [HttpPost]
        [Route("core-company")]
        public async Task<ActionResult> Create([FromBody] CoreCompanyVM model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                if (model == null)
                    return BadRequest(new { message = "Company is required." });

                var result = await _service.SaveAsync(model);


                if (!result)
                    return BadRequest(new { message = "Insertion Failed." });



                return Ok(new { success = true, message = "Data Saved Successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred.", error = ex.Message });
            }
        }

        #endregion


        #region Edit Company

        [HttpPut]
        [Route("core-company/{id}")]
        public async Task<IActionResult> Edit(int id, [FromBody] CoreCompanyVM model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (model == null || id != model.AutoID)
                return BadRequest(new { message = "Data is invalid." });

            var typeExists = await _service.IsExistAsync(id);
            if (!typeExists)
                return NotFound(new { message = $"Company with ID {id} not found." });

            var result = await _service.UpdateAsync(model);

            if (!result)
                return BadRequest(new { message = "Updated Failed." });

            return Ok(new { success = true, message = "Data updated successfully." });
        }

        #endregion


        #region Delete Company

        [HttpDelete]
        [Route("core-company/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var result = await _service.DeleteAsync(id);

                if (!result)
                    return NotFound(new { message = $"Bank with ID {id} not found." });

                return Ok(new { message = "Data Deleted Successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred.", error = ex.Message });
            }
        }

        [HttpDelete]
        [Route("core-company-list")]
        public async Task<ActionResult> BulkDelete([FromBody] List<int> ids)
        {
            try
            {
                if (ids == null || !ids.Any() || ids.Count == 0)
                {
                    return Json(new { isSuccess = false, message = "No Data is selected to delete" });
                }

                var result = await _service.BulkDeleteAsync(ids);
                if (!result)
                {
                    return Json(new { isSuccess = false, message = "No Data found to delete" });
                }
                return Json(new { isSuccess = true, message = $"Data Deleted Successfully." });
            }
            catch (Exception ex)
            {
                return Json(new { isSuccess = false, message = ex.Message });
            }
        }

        #endregion


        #region Get All info
        public async Task<IActionResult> GetAll()
        {
            var data = await _service.GetAllAsync();
            return Ok(new { data = data });
        }

        #endregion


        #region Country Dropdown
        [HttpGet("core-country-dropdown")]
        public async Task<IActionResult> GetAllCountry()
        {
            var list = await _country.GetAllCountryAsync();
            return Ok(list);
        }

        [HttpGet("currency-type-dropdown")]
        public async Task<IActionResult> GetAllCurrency()
        {
            var list = await _currency.GetAllCurrencyAsync();
            return Ok(list);
        }

        #endregion


        #region Getting Details

        [HttpGet("core-company-details")]
        public async Task<ActionResult> Details(int id)
        {
            try
            {
                var data = await _service.GetByIdAsync(id);

                if (data == null)
                    return NotFound(new { message = $"Company with ID {id} not found." });

                return Ok(data);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred.", error = ex.Message });
            }
        }

        #endregion
    }
}
