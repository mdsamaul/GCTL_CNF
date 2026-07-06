using GCTL.Core.ViewModels.DeleteHistories;
using GCTL.Core.ViewModels.MasterSetup.CoreBranch;
using GCTL.Service.Language;
using GCTL.Service.MasterSetup.CoreBranch;
using GCTL.Service.UserProfile;
using GCTL_App.Controllers;
using Microsoft.AspNetCore.Mvc;

namespace GCTL_NBR.Controllers.MasterSetup
{
    public class CoreBranchController : BaseController
    {
        private readonly ICoreBranch _service;
        public CoreBranchController(ITranslateService translateService, IUserProfileService userProfileService, ICoreBranch service) : base(translateService, userProfileService)
        {
            _service = service;
        }
        public IActionResult Index()
        {
            return View();
        }
        public IActionResult BranchForm()
        {
            return PartialView();
        }
        public IActionResult BranchtList()
        {
            return PartialView();
        }

        [HttpGet("next-core-branch-id")]
        public async Task<string> GenerateNewIdAsync()
        {
            var lastId = await _service.GetLastBranchCodeAsync();

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
        [Route("core-branch-list")]
        public async Task<IActionResult> GetAllPaginated(int pageNumber = 1, int pageSize = 10, string searchTerm = "", string sortColumn = "BranchCode", string sortOrder = "desc")
        {
            try
            {
                var result = await _service.GetPaginatedAsync(pageNumber, pageSize, searchTerm, sortColumn, sortOrder);

                if (result.Data == null || !result.Data.Any())
                {
                    return Ok(new
                    {
                        Data = new List<CoreBranchVM>(),
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
                        Message = "No Branch Found."
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
        [Route("branch/check-duplicate")]
        public async Task<IActionResult> CheckDuplicate([FromBody] CoreBranchVM model)
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
        [Route("core-branch")]
        public async Task<ActionResult> Create([FromBody] CoreBranchVM model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                if (model == null)
                    return BadRequest(new { message = "Branch is required." });

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
        [Route("core-branch/{id}")]
        public async Task<IActionResult> Edit(int id, [FromBody] CoreBranchVM model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (model == null || id != model.autoId)
                return BadRequest(new { message = "Data is invalid." });

            var typeExists = await _service.IsExistAsync(id);
            if (!typeExists)
                return NotFound(new { message = $"Branch with ID {id} not found." });

            var result = await _service.UpdateAsync(model);

            if (!result)
                return BadRequest(new { message = "Updated Failed." });

            return Ok(new { success = true, message = "Data updated successfully." });
        }

        [HttpDelete]
        [Route("core-branch/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var result = await _service.DeleteAsync(id);

                if (!result)
                    return NotFound(new { message = $"Branch with ID {id} not found." });

                return Ok(new { message = "Data Deleted Successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred.", error = ex.Message });
            }
        }
        [HttpDelete]
        [Route("core-branch-list")]
        public async Task<ActionResult> BulkDelete([FromBody] List<decimal> ids)
        {
            try
            {
                if (ids == null || !ids.Any() || ids.Count == 0)
                {
                    return Json(new { isSuccess = false, message = "No Data is selected to delete" });
                }

                DeleteHistoryViewModel model = new DeleteHistoryViewModel();
                model.CreatedBy = await GetCurrentEmployeeIdAsync();

                var result = await _service.BulkDeleteAsync(ids, model);
                //if (!result.Success)
                //{
                //    return Json(new { isSuccess = false, message = result.Message });
                //}
                return Json(new { isSuccess = result.Success, message = result.Message, refError = result.RefError });
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


        #region Core Company Dropdown

        [HttpGet]
        [Route("corebranch-core-company-dropdown")]
        public async Task<IActionResult> GetCompany()
        {
            var list = await _service.LoadCompanyDropdown();
            return Ok(new { data = list });
        }

        #endregion
    }
}
