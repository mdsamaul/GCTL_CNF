using GCTL.Service.Language;
using GCTL.Service.UserProfile;

using Microsoft.AspNetCore.Mvc;

namespace GCTL_App.Controllers.CharOfAccount
{
    public class LedgerController : BaseController
    {
        public LedgerController(ITranslateService translateService, IUserProfileService userProfileService) : base(translateService, userProfileService)
        {

        }

        public IActionResult Index()
        {
            return View();
        }



    }
}
