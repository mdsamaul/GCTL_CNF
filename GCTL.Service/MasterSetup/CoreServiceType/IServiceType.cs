using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GCTL.Core.ViewModels.MasterSetup;
using GCTL.Data.Models;

namespace GCTL.Service.MasterSetup.CoreServiceType
{
    public interface IServiceType
    {
        Task<List<Core_ServiceType>> GetAllCoreServiceAsync();
    }
}
