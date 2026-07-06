using GCTL.Core.ViewModels.MasterSetup.HrmDefDesignations;
using GCTL.Service.Language;

//using GCTL.Core.ViewModels.PaymentManagements;
using GCTL.Service.MasterSetup.HrmDefDesignations;
using GCTL.Service.UserProfile;
using GCTL_App.Controllers;
using Microsoft.AspNetCore.Mvc;

namespace GCTL_NBR.Controllers.MasterSetup
{
    public class HrmDefDesignationController : BaseController
    {
        private readonly IHrmDefDesignationService _service;

        public HrmDefDesignationController(ITranslateService translateService, IUserProfileService userProfileService, IHrmDefDesignationService service) : base(translateService, userProfileService)
        {
            _service = service;
        }

        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> GetAllPaginated(int pageNumber = 1, int pageSize = 10, string searchTerm = "", string sortColumn = "DesignationCode", string sortOrder = "desc")
        {
            try
            {
                var result = await _service.GetPaginatedAsync(pageNumber, pageSize, searchTerm, sortColumn, sortOrder);

                if (result.Data == null || !result.Data.Any())
                {
                    return Ok(new
                    {
                        Data = new List<HrmDefDesignationViewModel>(),
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
                        Message = "No Designation Found."
                    });
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred.", error = ex.Message });
            }
        }

        public async Task<ActionResult> Details(int id)
        {
            try
            {
                var data = await _service.GetByIdAsync(id);

                if (data == null)
                    return NotFound(new { message = $"Designation with ID {id} not found." });

                return Ok(data);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred.", error = ex.Message });
            }
        }

        public async Task<ActionResult> Create(HrmDefDesignationViewModel model)
        {
            try
            {
                if (model == null)
                    return BadRequest(new { message = "Designation data is required." });

                var result = await _service.SaveAsync(model);


                if (!result)
                    return BadRequest(new { message = "Insertion Failed." });



                return Json(new { isSuccess = true, message = "Data Saved Successfully.", });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred.", error = ex.Message });
            }
        }

        public async Task<IActionResult> Edit(int id, [FromForm] HrmDefDesignationViewModel model)
        {
            try
            {
                if (model == null || id != model.AutoId)
                    return BadRequest(new { message = "Designation data is invalid." });

                var typeExists = await _service.IsExistAsync(id);
                if (!typeExists)
                    return NotFound(new { message = $"Designation with ID {id} not found." });

                var result = await _service.UpdateAsync(model);

                if (!result)
                    return BadRequest(new { message = "Updated Failed" });

                return Ok(new { message = "Designation updated successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred.", error = ex.Message });
            }
        }

        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var result = await _service.DeleteAsync(id);

                if (!result)
                    return NotFound(new { message = $"Designation with ID {id} not found." });

                return Ok(new { message = "Data deleted successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred.", error = ex.Message });
            }
        }

        public async Task<ActionResult> BulkDelete(List<decimal> ids)
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
                return Json(new { isSuccess = true, message = $" Data Deleted Successfully." });
            }
            catch (Exception ex)
            {
                return Json(new { isSuccess = false, message = ex.Message });
            }
        }

        [Route("HrmDefDesignation/GenerateNewIdAsync")]
        public async Task<string> GenerateNewIdAsync()
        {
            var lastId = await _service.GetLastDesignationAsync();

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

        public async Task<IActionResult> GetAll()
        {
            var data = await _service.GetAllAsync();
            return Ok(new { data = data });
        }

        public async Task<IActionResult> CheckDuplicate([FromForm] HrmDefDesignationViewModel model)
        {
            var isDuplicate = await _service.IsDuplicateAsync(
                model
            );

            if (isDuplicate)
            {
                return Ok(new { isDuplicate = true, message = "Data Already Exixts." });
            }

            return Ok(new { isDuplicate = false, message = "No duplicate found." });
        }
    }
}
