using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GCTL.Core.Repository;
using GCTL.Core.ViewModels.MasterSetup;
using GCTL.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace GCTL.Service.MasterSetup.CoreServiceType
{
    public class ServiceTypeService : AppService<Core_ServiceType>, IServiceType
    {
        private readonly IGenericRepository<Core_ServiceType> _repository;  
        public ServiceTypeService(IGenericRepository<Core_ServiceType> genericRepository) : base(genericRepository)
        {
            _repository = genericRepository;
        }

        public async Task<List<Core_ServiceType>> GetAllCoreServiceAsync()
        {
            var data = await _repository.All()
                .Select(x => new Core_ServiceType
                {
                    ServiceTypeID = x.ServiceTypeID,
                    ServiceTypeName = x.ServiceTypeName
                })
                .ToListAsync();

            return data;
        }

    }
}

