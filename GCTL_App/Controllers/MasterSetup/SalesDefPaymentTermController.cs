using GCTL.Core.ViewModels.MasterSetup.PaymentTerms;
using GCTL.Core.ViewModels.MasterSetup.PaymentType;
using GCTL.Service.Language;
using GCTL.Service.MasterSetup.PaymentTerms;
using GCTL.Service.UserProfile;
using GCTL_App.Controllers;
using Microsoft.AspNetCore.Mvc;

namespace GCTL_NBR.Controllers.MasterSetup
{
    public class SalesDefPaymentTermController : BaseController
    {
        private readonly IPaymentTerms _service;
        public SalesDefPaymentTermController(ITranslateService translateService, IUserProfileService userProfileService, IPaymentTerms service) : base(translateService, userProfileService)
        {
            _service = service;
        }
        public IActionResult Index()
        {
            return View();
        }
        public IActionResult PayemtTermForm()
        {
            return PartialView();
        }
        public IActionResult PaymentTermList()
        {
            return PartialView();
        }

        [HttpGet("next-payment-terms-id")]
        public async Task<string> GenerateNewIdAsync()
        {
            var lastId = await _service.GetLastPaymentTermsAsync();

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

        [HttpPost]
        [Route("payment-terms/check-duplicate")]
        public async Task<IActionResult> CheckDuplicate([FromBody] PaymentTermsVM model)
        {
            if (model == null) return BadRequest(new { message = "Invalid data." });

            var isDuplicate = await _service.IsDuplicateAsync(model);

            if (isDuplicate)
            {
                return Ok(new { isDuplicate = true, message = "Data Already Exists." });
            }

            return Ok(new { isDuplicate = false, message = "No duplicate found." });
        }

        [HttpGet]
        [Route("payment-terms-list")]
        public async Task<IActionResult> GetAllPaginated(int pageNumber = 1, int pageSize = 10, string searchTerm = "", string sortColumn = "PaymentTermsCode", string sortOrder = "desc")
        {
            try
            {
                var result = await _service.GetPaginatedAsync(pageNumber, pageSize, searchTerm, sortColumn, sortOrder);

                if (result.Data == null || !result.Data.Any())
                {
                    return Ok(new
                    {
                        Data = new List<PaymentTermsVM>(),
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
                        Message = "No Payment Terms Found."
                    });
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred.", error = ex.Message });
            }
        }

        [HttpPost]
        [Route("payment-terms")]
        public async Task<ActionResult> Create([FromBody] PaymentTermsVM model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                if (model == null)
                    return BadRequest(new { message = "Payment Terms Name is required." });

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

        [HttpPut]
        [Route("payment-terms/{id}")]
        public async Task<IActionResult> Edit(int id, [FromBody] PaymentTermsVM model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (model == null || id != model.TC)
                return BadRequest(new { message = "Data is invalid." });

            var typeExists = await _service.IsExistAsync(id);
            if (!typeExists)
                return NotFound(new { message = $"Payment Terms with ID {id} not found." });

            var result = await _service.UpdateAsync(model);

            if (!result)
                return BadRequest(new { message = "Updated Failed." });

            return Ok(new { success = true, message = "Data updated successfully." });
        }

        [HttpDelete]
        [Route("payment-terms/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var result = await _service.DeleteAsync(id);

                if (!result)
                    return NotFound(new { message = $"Payment Terms with ID {id} not found." });

                return Ok(new { message = "Data Deleted Successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred.", error = ex.Message });
            }
        }
        [HttpDelete]
        [Route("payment-terms-list")]
        public async Task<ActionResult> BulkDelete([FromBody] List<decimal> ids)
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
        public async Task<IActionResult> GetAll()
        {
            var data = await _service.GetAllAsync();
            return Ok(new { data });
        }
    }
}
