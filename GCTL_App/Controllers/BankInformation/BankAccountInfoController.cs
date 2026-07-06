using GCTL.Core.ViewModels.BankModule.CoreAccount;
using GCTL.Service.BankModule.BankAccount;
using GCTL.Service.BankModule.BankBranch;
using GCTL.Service.BankModule.BankInfo;
using GCTL.Service.Language;
using GCTL.Service.UserProfile;
using GCTL_App.Controllers;
using Microsoft.AspNetCore.Mvc;


namespace GCTL_NBR.Controllers.BankInformation
{
    public class BankAccountInfoController : BaseController
    {

        private readonly IBankAccount _service;
        private readonly IBankInfo _bankInfo;
        private readonly IBankBranch _bankbranch;
        public BankAccountInfoController(ITranslateService translateService, IUserProfileService userProfileService, IBankAccount service, IBankInfo bankInfo, IBankBranch bank) : base(translateService, userProfileService)
        {
            _service = service;
            _bankInfo = bankInfo;
            _bankbranch = bank;
        }
        public IActionResult Form()
        {
            return PartialView();
        }
        public IActionResult List()
        {
            return PartialView();
        }

        [HttpGet("next-account-id")]
        public async Task<string> GenerateNewIdAsync()
        {
            var lastId = await _service.GetLastAccountInfoIDAsync();

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

        [HttpGet]
        [Route("account-list")]
        public async Task<IActionResult> GetAllPaginated(int pageNumber = 1, int pageSize = 10, string searchTerm = "", string sortColumn = "AccInfoID", string sortOrder = "desc")
        {
            try
            {
                var result = await _service.GetPaginatedAsync(pageNumber, pageSize, searchTerm, sortColumn, sortOrder);

                if (result.Data == null || !result.Data.Any())
                {
                    return Ok(new
                    {
                        Data = new List<CoreAccountVM>(),
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
                        Message = "No Bank Account Found."
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
        [Route("account/check-duplicate")]
        public async Task<IActionResult> CheckDuplicate([FromBody] CoreAccountVM model)
        {
            if (model == null) return BadRequest(new { message = "Invalid data." });

            var isDuplicate = await _service.IsDuplicateAsync(model);

            if (isDuplicate)
            {
                return Ok(new { isDuplicate = true, message = "Data Already Exists." });
            }

            return Ok(new { isDuplicate = false, message = "No duplicate found." });
        }
        [HttpPost]
        [Route("account-info")]
        public async Task<ActionResult> Create([FromBody] CoreAccountVM model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                if (model == null)
                    return BadRequest(new { message = "Account is required." });

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
        [Route("account-info/{id}")]
        public async Task<IActionResult> Edit(int id, [FromBody] CoreAccountVM model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (model == null || id != model.AutoID)
                return BadRequest(new { message = "Data is invalid." });

            var typeExists = await _service.IsExistAsync(id);
            if (!typeExists)
                return NotFound(new { message = $"Account with ID {id} not found." });

            var result = await _service.UpdateAsync(model);

            if (!result)
                return BadRequest(new { message = "Updated Failed." });

            return Ok(new { success = true, message = "Data updated successfully." });
        }

        [HttpDelete]
        [Route("account-info/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var result = await _service.DeleteAsync(id);

                if (!result)
                    return NotFound(new { message = $"Account with ID {id} not found." });

                return Ok(new { message = "Data Deleted Successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred.", error = ex.Message });
            }
        }
        [HttpDelete]
        [Route("account-info-list")]
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
            return Ok(new { data = data });
        }

        [HttpGet("bank-info-dropdown-for-account")]
        public async Task<IActionResult> GetAllBanks()
        {
            var list = await _bankInfo.GetAllBankAsync();
            return Ok(list);
        }


        [HttpGet("branches-with-banks")]
        public async Task<IActionResult> GetBranchesByBank(string id)
        {
            if (string.IsNullOrEmpty(id))
                return BadRequest("BankID is required");

            var list = await _bankbranch.GetAllBranchAsync(id);
            return Ok(list);
        }


    }
}









