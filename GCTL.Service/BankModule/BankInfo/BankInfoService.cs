using GCTL.Core.Repository;
using GCTL.Core.ViewModels.BankModule.BankInfo;
using GCTL.Data.Models;
using GCTL.Service.Pagination;
using Microsoft.EntityFrameworkCore;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;


namespace GCTL.Service.BankModule.BankInfo
{
    public class BankInfoService: AppService<SALES_Def_BankInfo>,IBankInfo
    {
        private readonly IGenericRepository<SALES_Def_BankInfo> _repository;
        public BankInfoService(IGenericRepository<SALES_Def_BankInfo> genericRepository) : base(genericRepository)
        {
            _repository = genericRepository;
        }

        public async Task<bool> BulkDeleteAsync(List<decimal> ids)
        {
            await _repository.BeginTransactionAsync();

            try
            {
                var entity = await _repository.All().Where(c => ids.Contains(c.AutoID)).ToListAsync();

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

        public async Task<bool> DeleteAsync(decimal id)
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

                await _repository.DeleteAsync(entity.AutoID);

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

        public async Task<List<BankInfoVM>> GetAllAsync()
        {
            var entity = await _repository.All().ToListAsync();

            var result = entity.Select(model => new BankInfoVM
            {
                BankID = model.BankID,
                BankName = model.BankName,
                ShortName = model.ShortName
            }).ToList();

            return result;
        }

        public async Task<BankInfoVM> GetByIdAsync(int id)
        {
            var data = await _repository.All().Where(pt => pt.AutoID == id)
                        .Select(pt => new BankInfoVM
                        {

                            BankID = pt.BankID,
                            BankName = pt.BankName,
                            ShortName = pt.ShortName,
                            LDate = pt.LDate,
                            ModifyDate = pt.ModifyDate,
                        }).FirstOrDefaultAsync();
            return data;
        }

        public async Task<string> GetLastBankIDAsync()
        {
            var last = await _repository.All()
                    .OrderByDescending(c => c.BankID)
                    .FirstOrDefaultAsync();

            return last?.BankID;
        }

        public async Task<PaginationService<SALES_Def_BankInfo, BankInfoVM>.PaginationResult<BankInfoVM>> GetPaginatedAsync(int pageNumber = 1, int pageSize = 10, string searchTerm = "", string sortColumn = "BankID", string sortOrder = "desc")
        {
            var query = _repository.All();

            if (pageSize == -1)
            {
                pageSize = await query.CountAsync();
                pageNumber = 1;
            }

            var paginatedResult = await PaginationService<SALES_Def_BankInfo, BankInfoVM>.GetPaginatedData(
                query,
                pageNumber,
                pageSize,
                searchTerm,
                sortColumn,
                sortOrder,
                term => sc =>
                    EF.Functions.Like(sc.BankID ?? "", $"%{term}%") ||
                    EF.Functions.Like(sc.BankName ?? "", $"{term}%") ||
                    EF.Functions.Like(sc.ShortName ?? "", $"%{term}%"),
                pt => new BankInfoVM
                {
                    AutoID = pt.AutoID,
                    BankID = pt.BankID,
                    BankName = pt.BankName,
                    ShortName = pt.ShortName,
                    LIP = pt.LIP,
                    LMAC = pt.LMAC,
                    LDate = pt.LDate,
                    ModifyDate = pt.ModifyDate,
                });

            return paginatedResult;
        }

        public async Task<bool> IsDuplicateAsync(BankInfoVM model)
        {
            if (model == null)
            {
                throw new ArgumentNullException(nameof(model));
            }

            var alldata = await _repository.All().ToListAsync();

            var isDuplicate = alldata.Any(pm =>
            string.Equals(pm.BankName, model.BankName, StringComparison.OrdinalIgnoreCase) &&
             pm.AutoID != model.AutoID

            );

            return isDuplicate;
        }

        public async Task<bool> IsExistAsync(int id)
        {
            return await _repository.All().AnyAsync(x => x.AutoID == id);
        }

        public async Task<bool> SaveAsync(BankInfoVM model)
        {
            try
            {
                var entity = new SALES_Def_BankInfo
                {
                    BankID = model.BankID,
                    BankName = model.BankName,
                    ShortName = model.ShortName ?? "",
                    LUser = "", 
                    LDate = DateTime.Now,
                    LIP = GetLocalIP() ?? "",
                    LMAC = GetMacAddress() ?? ""
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

        public async Task<bool> UpdateAsync(BankInfoVM model)
        {
            await _repository.BeginTransactionAsync();
            try
            {
                var entity = await _repository.GetByIdAsync(model.AutoID);
                if (entity == null)
                {
                    await _repository.RollbackTransactionAsync();
                    return false;
                }

                entity.BankID = model.BankID;
                entity.BankName = model.BankName;
                entity.ShortName = model.ShortName ?? "";
                entity.LUser = "";
                entity.ModifyDate = DateTime.Now;
                entity.LIP = GetLocalIP() ?? "";
                entity.LMAC = GetMacAddress() ?? "";

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

        //Bank Dropdown
        public async Task<List<BankInfoVM>> GetAllBankAsync()
        {
            var data = await _repository.All().ToListAsync();
            return data.Select(x => new BankInfoVM
            {
                BankID = x.BankID,
                BankName = x.BankName
            }).ToList();
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
