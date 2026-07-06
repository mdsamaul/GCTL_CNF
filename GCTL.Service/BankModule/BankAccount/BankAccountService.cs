using GCTL.Core.Repository;
using GCTL.Core.ViewModels.BankModule.BankInfo;
using GCTL.Core.ViewModels.BankModule.CoreAccount;
using GCTL.Data.Models;
using GCTL.Service.Pagination;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Service.BankModule.BankAccount
{
    public class BankAccountService : AppService<Core_BankAccountInformation>, IBankAccount
    {
        private readonly IGenericRepository<Core_BankAccountInformation> _repository;
        private readonly AppDbContext _context;
        public BankAccountService(IGenericRepository<Core_BankAccountInformation> genericRepository, AppDbContext context) : base(genericRepository)
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

        public async Task<List<CoreAccountVM>> GetAllAsync()
        {
            var entity = await _repository.All().ToListAsync();

            var result = entity.Select(model => new CoreAccountVM
            {
                AccInfoID = model.AccInfoID,
                AccountName = model.AccountName,
                BankID = model.BankID,
                BranchID = model.BranchID
            }).ToList();

            return result;
        }

        public async Task<CoreAccountVM> GetByIdAsync(int id)
        {
            var data = await _repository.All().Where(pt => pt.AutoID == id)
                        .Select(pt => new CoreAccountVM
                        {

                            AccInfoID = pt.AccInfoID,
                            AccountName = pt.AccountName,
                            BankID = pt.BankID,
                            BranchID = pt.BranchID,
                            AccountNO = pt.AccountNO,
                            LDate = pt.LDate,
                            ModifyDate = pt.ModifyDate,
                        }).FirstOrDefaultAsync();
            return data;
        }

        public async Task<string> GetLastAccountInfoIDAsync()
        {
            var last = await _repository.All()
                    .OrderByDescending(c => c.AccInfoID)
                    .FirstOrDefaultAsync();

            return last?.AccInfoID;
        }

        public async Task<PaginationService<Core_BankAccountInformation, CoreAccountVM>.PaginationResult<CoreAccountVM>> GetPaginatedAsync(int pageNumber = 1, int pageSize = 10, string searchTerm = "", string sortColumn = "AccInfoID", string sortOrder = "desc")
        {
            var query = _repository.All();

            if (pageSize == -1)
            {
                pageSize = await query.CountAsync();
                pageNumber = 1;
            }

            var paginatedResult = await PaginationService<Core_BankAccountInformation, CoreAccountVM>.GetPaginatedData(
                query,
                pageNumber,
                pageSize,
                searchTerm,
                sortColumn,
                sortOrder,
                term => sc =>
                    EF.Functions.Like(sc.AccInfoID ?? "", $"%{term}%") ||
                    EF.Functions.Like(sc.AccountName ?? "", $"{term}%") ||
                     EF.Functions.Like(sc.AccountNO ?? "", $"{term}%") ||
                    EF.Functions.Like(_context.SALES_Def_BankInfo
                                .Where(b => b.BankID == sc.BankID)
                                .Select(b => b.BankName)
                                .FirstOrDefault() ?? "", $"%{term}%") ||
                    EF.Functions.Like(_context.Sales_Def_BankBranchInfo
                                .Where(b => b.BankBranchID == sc.BranchID)
                                .Select(b => b.BankBranchName)
                                .FirstOrDefault() ?? "", $"%{term}%"),
                pt => new CoreAccountVM
                {
                    AutoID = pt.AutoID,
                    AccInfoID = pt.AccInfoID,
                    AccountName = pt.AccountName,
                    BankID = pt.BankID,
                    BranchID = pt.BranchID,
                    BankName = _context.Set<SALES_Def_BankInfo>()
                                            .Where(cs => cs.BankID == pt.BankID)
                                            .Select(cs => cs.BankName)
                                            .FirstOrDefault(),
                    BankBranchName = _context.Set<Sales_Def_BankBranchInfo>()
                                            .Where(b => b.BankBranchID == pt.BranchID)
                                            .Select(br => br.BankBranchName).FirstOrDefault(),
                    AccountNO = pt.AccountNO,
                    LIP = pt.LIP,
                    LMAC = pt.LMAC,
                    LDate = pt.LDate,
                    ModifyDate = pt.ModifyDate,
                });

            return paginatedResult;
        }

        public async Task<bool> IsDuplicateAsync(CoreAccountVM model)
        {
            if (model == null)
            {
                throw new ArgumentNullException(nameof(model));
            }

            var alldata = await _repository.All().ToListAsync();

            var isDuplicate = alldata.Any(pm =>
            string.Equals(pm.AccountNO, model.AccountNO, StringComparison.OrdinalIgnoreCase) &&
             pm.BankID == model.BankID &&
             pm.BranchID == model.BranchID &&
             pm.AutoID != model.AutoID

            );

            return isDuplicate;
        }

        public async Task<bool> IsExistAsync(int id)
        {
            return await _repository.All().AnyAsync(x => x.AutoID == id);
        }

        public async Task<bool> SaveAsync(CoreAccountVM model)
        {
            try
            {
                var entity = new Core_BankAccountInformation
                {
                    AccInfoID = model.AccInfoID,
                    AccountName = model.AccountName,
                    AccountNO = model.AccountNO,
                    BankID = model.BankID,
                    BranchID = model.BranchID,
                    UserInfoEmployeeID = model.UserInfoEmployeeID ?? "",
                    CompanyCode = model.CompanyCode ?? "",
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

        public async Task<bool> UpdateAsync(CoreAccountVM model)
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

                entity.AccountNO = model.AccountNO;
                entity.AccountName = model.AccountName;
                entity.BankID = model.BankID;
                entity.BranchID = model.BranchID;
                entity.UserInfoEmployeeID = model.UserInfoEmployeeID ?? "";
                entity.CompanyCode = model.CompanyCode ?? "";
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
