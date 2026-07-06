using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using GCTL.Core.Repository;
using GCTL.Core.ViewModels.MasterSetup.VendorPrefix;
using GCTL.Data.Models;
using GCTL.Core.ViewModels.MasterSetup.CoreCountries;
using Microsoft.EntityFrameworkCore;
using GCTL.Core.ViewModels.MasterSetup.HrmDefDesignations;
using GCTL.Service.Pagination;

namespace GCTL.Service.MasterSetup.VendorPrefix
{
    public class VendorPrefixService : AppService<CF_Def_VendorPrefix>, IVendorPrefixService
    {
        private readonly IGenericRepository<CF_Def_VendorPrefix> _repository;
        public VendorPrefixService(IGenericRepository<CF_Def_VendorPrefix> genericRepository) : base(genericRepository)
        {
            _repository = genericRepository;
        }

        public async Task<bool> BulkDeleteAsync(List<int> ids)
        {
            await _repository.BeginTransactionAsync();

            try
            {
                var entity = await _repository.All().Where(c => ids.Contains(c.TC)).ToListAsync();

                if (entity == null || !entity.Any())
                {
                    await _repository.RollbackTransactionAsync();
                    return false;
                }

                await _repository.DeleteRangeAsync(entity);

                await _repository.CommitTransactionAsync();

                return true;
            }
            catch (Exception ex)
            {
                await _repository.RollbackTransactionAsync();
                Console.WriteLine($"Bulk delete error: {ex}");
                return (false);
            }
        }

        public async Task<bool> DeleteAsync(int id)
        {
            await _repository.BeginTransactionAsync();

            try
            {
                var entity = await _repository.GetByIdAsync(id);

                if (entity == null)
                {
                    await _repository.RollbackTransactionAsync();
                    return false;
                }

                await _repository.DeleteAsync(entity.TC);

                await _repository.CommitTransactionAsync();

                return true;
            }
            catch (Exception ex)
            {
                await _repository.RollbackTransactionAsync();
                Console.WriteLine($"Bulk delete error: {ex}");
                return (false);
            }
        }

        public async Task<List<VendorPrefixVM>> GetAllAsync()
        {
            var entity = await _repository.All().ToListAsync();

            var result = entity.Select(model => new VendorPrefixVM
            {
                VendorPrifixID = model.VendorPrifixId,
                PrifixName = model.PrifixName,
                ShortName = model.ShortName
                
            }).ToList();

            return result;
        }

        public async Task<VendorPrefixVM> GetByIdAsync(int id)
        {
            var data = await _repository.All().Where(pt => pt.TC == id)
            .Select(pt => new VendorPrefixVM
            {
                VendorPrifixID = pt.VendorPrifixId,
                PrifixName = pt.PrifixName ?? "",
                ShortName = pt.ShortName ?? "",
              
                LMAC = pt.LMAC ?? "",
                LDate = pt.LDate,
                ModifyDate = pt.ModifyDate,
            }).FirstOrDefaultAsync();
            return data;
        }

        public async Task<string> GetLastCountryCodeAsync()
        {
            var last = await _repository.All()
            .OrderByDescending(c => c.VendorPrifixId)
            .FirstOrDefaultAsync();

            return last?.VendorPrifixId;

        }

        public async Task<PaginationService<CF_Def_VendorPrefix, VendorPrefixVM>.PaginationResult<VendorPrefixVM>> GetPaginatedAsync( int pageNumber = 1,int pageSize = 10, string searchTerm = "", string sortColumn = "VendorPrifixId", string sortOrder = "desc")
        {
            var query = _repository.All();

            if (pageSize == -1)
            {
                pageSize = await query.CountAsync();
                pageNumber = 1;  
            }

            var paginatedResult = await PaginationService<CF_Def_VendorPrefix, VendorPrefixVM>.GetPaginatedData(
                query,
                pageNumber,
                pageSize,
                searchTerm,
                sortColumn,
                sortOrder,
                term => sc =>
                    EF.Functions.Like(sc.VendorPrifixId ?? "", $"%{term}%") ||
                    EF.Functions.Like(sc.PrifixName ?? "", $"{term}%") ||
                    EF.Functions.Like(sc.ShortName ?? "", $"%{term}%"),
                pt => new VendorPrefixVM
                {
                    TC = pt.TC,
                    VendorPrifixID = pt.VendorPrifixId,
                    PrifixName = pt.PrifixName ?? "",
                    ShortName = pt.ShortName ?? "",
                    LIP = pt.LIP ?? "",
                    LMAC = pt.LMAC ?? "",
                    LDate = pt.LDate,
                    ModifyDate = pt.ModifyDate,
                });

            return paginatedResult;
        }

        public async Task<bool> IsDuplicateAsync(VendorPrefixVM model)
        {
            if (model == null)
            {
                throw new ArgumentNullException(nameof(model));
            }

            var paymentMode = await _repository.All().ToListAsync();

            var isDuplicate = paymentMode.Any(pm =>
            string.Equals(pm.PrifixName, model.PrifixName, StringComparison.OrdinalIgnoreCase) &&
             pm.TC != model.TC

            );

            return isDuplicate;
        }

        public async Task<bool> IsExistAsync(int id)
        {
            return await _repository.All().AnyAsync(x => x.TC == id);
        }

        public async Task<bool> SaveAsync(VendorPrefixVM model)
        {
            try
            {
                var entity = new CF_Def_VendorPrefix
                {
                    VendorPrifixId = model.VendorPrifixID,
                    PrifixName = model.PrifixName,
                    ShortName = model.ShortName ?? "",
                    LUser = "", //If needed
                    LDate = DateTime.Now,
                    ModifyDate = model.ModifyDate,
                    LIP = GetLocalIP(),
                    LMAC = GetMacAddress()
                };

                await _repository.AddAsync(entity);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Save failed: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> UpdateAsync(VendorPrefixVM model)
        {
            await _repository.BeginTransactionAsync();
            try
            {
                var entity = await _repository.GetByIdAsync(model.TC);
                if (entity == null)
                {
                    await _repository.RollbackTransactionAsync();
                    return false;
                }

                entity.VendorPrifixId = model.VendorPrifixID;
                entity.PrifixName = model.PrifixName;
                entity.ShortName = model.ShortName ?? "";
                entity.LUser = "";
                entity.ModifyDate = DateTime.Now;
                entity.LIP = GetLocalIP();
                entity.LMAC = GetMacAddress();


                await _repository.UpdateAsync(entity);
                await _repository.CommitTransactionAsync();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                await _repository.RollbackTransactionAsync();
                return false;
            }
        }


        #region Ip & Mac
        public string GetLocalIP()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    return ip.ToString();
                }
            }
            return string.Empty;
        }

        public string GetMacAddress()
        {
            var nics = NetworkInterface.GetAllNetworkInterfaces();
            var macAddress = string.Empty;
            foreach (var adapter in nics)
            {
                if (adapter.NetworkInterfaceType == NetworkInterfaceType.Ethernet)
                {
                    macAddress = adapter.GetPhysicalAddress().ToString();
                    break;
                }
            }
            return macAddress;
        }
        #endregion
    }
}
