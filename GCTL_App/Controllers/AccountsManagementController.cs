using GCTL.Service.Language;
using GCTL.Service.UserProfile;
using GCTL_App.Controllers;
using Microsoft.AspNetCore.Mvc;

namespace GCTL_NBR.Controllers
{
    public class AccountsManagementController : BaseController
    {
        public AccountsManagementController(ITranslateService translateService, IUserProfileService userProfileService) : base(translateService, userProfileService)
        {
            
        }

        public IActionResult Index()
        {
            return View();
        }
        public IActionResult BankInfo()
        {
            return PartialView();
        }
        public IActionResult BankBranchInfo()
        {
            return PartialView();
        }
        public IActionResult BankAccountInfo()
        {
            return PartialView();
        }
    }
}
