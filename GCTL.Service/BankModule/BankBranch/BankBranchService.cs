using GCTL.Core.Repository;
using GCTL.Core.ViewModels.BankModule.BankBranch;
using GCTL.Data.Models;
using GCTL.Service.Pagination;
using Microsoft.EntityFrameworkCore;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;


namespace GCTL.Service.BankModule.BankBranch
{
    public class BankBranchService:AppService<Sales_Def_BankBranchInfo>, IBankBranch
    {
        private readonly IGenericRepository<Sales_Def_BankBranchInfo> _repository;
        private readonly AppDbContext _context;
        public BankBranchService(IGenericRepository<Sales_Def_BankBranchInfo> genericRepository, AppDbContext context) : base(genericRepository)
        {
            _repository = genericRepository;
            _context = context;
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

        public async Task<List<BankBranchVM>> GetAllAsync()
        {
            var entity = await _repository.All().ToListAsync();

            var result = entity.Select(model => new BankBranchVM
            {
                BankBranchID = model.BankBranchID,
                BankBranchName = model.BankBranchName,
                ShortName = model.ShortName,
                BankID = model.BankID,
                SWIFTCode = model.SWIFTCode,
                Address = model.Address,
                Phone = model.Phone

            }).ToList();

            return result;
        }

        public async Task<BankBranchVM> GetByIdAsync(int id)
        {
            var data = await _repository.All().Where(pt => pt.AutoID == id)
                        .Select(pt => new BankBranchVM
                        {

                            BankBranchID = pt.BankBranchID,
                            BankBranchName = pt.BankBranchName,
                            ShortName = pt.ShortName,
                            BankID = pt.BankID,
                            SWIFTCode = pt.SWIFTCode,
                            Address = pt.Address,
                            Phone = pt.Phone,
                            LDate = pt.LDate,
                            ModifyDate = pt.ModifyDate,
                        }).FirstOrDefaultAsync();
            return data;
        }

        public async Task<string> GetLastBranchIDAsync()
        {
            var last = await _repository.All()
                    .OrderByDescending(c => c.BankBranchID)
                    .FirstOrDefaultAsync();

            return last?.BankBranchID;
        }

        public async Task<PaginationService<Sales_Def_BankBranchInfo, BankBranchVM>.PaginationResult<BankBranchVM>> GetPaginatedAsync(int pageNumber = 1, int pageSize = 10, string searchTerm = "", string sortColumn = "BankBranchID", string sortOrder = "desc", string bankID = "")
        {
            var query = _repository.All();
            if(!string.IsNullOrEmpty(bankID))
            {
                query = query.Where(b => b.BankID == bankID);
            }

          
            if (pageSize == -1)
            {
                pageSize = await query.CountAsync();
                pageNumber = 1;
            }

            var paginatedResult = await PaginationService<Sales_Def_BankBranchInfo, BankBranchVM>.GetPaginatedData(
                query,
                pageNumber,
                pageSize,
                searchTerm,
                sortColumn,
                sortOrder,
                term => sc =>
                    EF.Functions.Like(sc.BankBranchID ?? "", $"%{term}%") ||
                    EF.Functions.Like(sc.BankBranchName ?? "", $"{term}%") ||
                    EF.Functions.Like(sc.SWIFTCode ?? "", $"%{term}%") ||
                    EF.Functions.Like(sc.Address ?? "", $"{term}%") ||
                    EF.Functions.Like(sc.Phone ?? "", $"{term}%") ||
                    EF.Functions.Like(sc.ShortName ?? "", $"%{term}%")||
                    EF.Functions.Like(_context.SALES_Def_BankInfo
                                .Where(b => b.BankID == sc.BankID)
                                .Select(b => b.BankName)
                                .FirstOrDefault() ?? "",$"%{term}%"),
                pt => new BankBranchVM
                {
                    AutoID = pt.AutoID,
                    BankBranchID = pt.BankBranchID,
                    BankBranchName = pt.BankBranchName,
                    ShortName = pt.ShortName,
                    BankID = pt.BankID,
                    SWIFTCode = pt.SWIFTCode,
                    Address = pt.Address,
                    Phone = pt.Phone,
                    LDate = pt.LDate,
                    ModifyDate = pt.ModifyDate,
                    LIP = pt.LIP,
                    LMAC = pt.LMAC,
                    BankName = _context.Set<SALES_Def_BankInfo>()
                                            .Where(cs => cs.BankID == pt.BankID)
                                            .Select(cs => cs.BankName)
                                            .FirstOrDefault(),
                });

            return paginatedResult;
        }

        public async Task<bool> IsDuplicateAsync(BankBranchVM model)
        {
            if (model == null)
            {
                throw new ArgumentNullException(nameof(model));
            }

            var alldata = await _repository.All().ToListAsync();

            var isDuplicate = alldata.Any(pm =>
            string.Equals(pm.BankBranchName, model.BankBranchName, StringComparison.OrdinalIgnoreCase) &&
             pm.BankID == model.BankID &&
             pm.AutoID != model.AutoID

            );

            return isDuplicate;
        }

        public async Task<bool> IsExistAsync(int id)
        {
            return await _repository.All().AnyAsync(x => x.AutoID == id);
        }

        public async Task<bool> SaveAsync(BankBranchVM model)
        {
            try
            {
                var entity = new Sales_Def_BankBranchInfo
                {
                    BankBranchID = model.BankBranchID,
                    BankBranchName = model.BankBranchName ?? "",
                    ShortName = model.ShortName ?? "",
                    BankID = model.BankID,
                    SWIFTCode = model.SWIFTCode ?? "",
                    Address = model.Address ?? "",
                    Phone = model.Phone ?? "",
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

        public async Task<bool> UpdateAsync(BankBranchVM model)
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

                entity.BankBranchID = model.BankBranchID;
                entity.BankBranchName = model.BankBranchName ?? "";
                entity.ShortName = model.ShortName ?? "";
                entity.BankID = model.BankID;
                entity.SWIFTCode = model.SWIFTCode ?? "";
                entity.Address = model.Address ?? "";
                entity.Phone = model.Phone ?? "";
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

        //Bank Branch Dropdown
        public async Task<List<BankBranchVM>> GetAllBranchAsync(string bankId)
        {
            var data = await _repository.All()
                                        .Where(b => b.BankID == bankId)
                                        .ToListAsync();
            return data.Select(x => new BankBranchVM
            {
                BankBranchID = x.BankBranchID,
                BankBranchName = x.BankBranchName
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
