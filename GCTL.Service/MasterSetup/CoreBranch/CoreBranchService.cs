using GCTL.Core.Repository;
using GCTL.Core.ViewModels;
using GCTL.Core.ViewModels.DeleteHistories;
using GCTL.Core.ViewModels.MasterSetup.CoreBranch;
using GCTL.Data.Models;
using GCTL.Service.DeleteHistories;
using GCTL.Service.Pagination;
using Microsoft.EntityFrameworkCore;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;

namespace GCTL.Service.MasterSetup.CoreBranch
{
    public class CoreBranchService : AppService<Core_Branch>, ICoreBranch
    {
        #region Service

        private readonly IGenericRepository<Core_Branch> _repository;
        private readonly IGenericRepository<Core_Company> _company;
        private readonly IDeleteHistoryService _deleteHistoryService;
        public CoreBranchService(IGenericRepository<Core_Branch> genericRepository, IGenericRepository<Core_Company> company, IDeleteHistoryService deleteHistoryService) : base(genericRepository)
        {
            _repository = genericRepository;
            _company = company;
            _deleteHistoryService = deleteHistoryService;
        }

        #endregion


        #region Delete
        public async Task<CommonReturnViewModel> BulkDeleteAsync(List<decimal> ids, DeleteHistoryViewModel model)
        {
            var entity = await _repository.All().AsNoTracking().Where(c => ids.Contains(c.autoId)).ToListAsync();

            if (entity == null || !entity.Any())
            {
                await _repository.RollbackTransactionAsync();
                return new CommonReturnViewModel { Success = false, Message = "No Ids found to delete" };
            }

            var column = "BranchCode";
            var dependencyCheck = await _deleteHistoryService.CheckDependenciesAsync(
                _repository.GetTableName(),
                column,
                entity.Select(x => x.BranchCode).Cast<string>().ToList()
            );

            await _repository.BeginTransactionAsync();

            try
            {
                if (dependencyCheck != null && !dependencyCheck.CanDelete)
                    return new CommonReturnViewModel { Success = false, Message = dependencyCheck.Message, RefError = true };


                await _repository.DeleteRangeAsync(entity);
                model.tableName = _repository.GetTableName();
                model.LMAC = GetMacAddress();
                model.LIP = GetLocalIP();
                
                await _deleteHistoryService.LogDeletedRecordsAsync(entity, model);

                await _repository.CommitTransactionAsync();

                return new CommonReturnViewModel { Success = true, Message = "Data Deleted Successfully"};
            }
            catch (Exception ex)
            {
                await _repository.RollbackTransactionAsync();
                Console.WriteLine($"Bulk delete error: {ex}");
                return new CommonReturnViewModel { Success = false, Message = "No Ids found to delete" };
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

                await _repository.DeleteAsync(entity.autoId);

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

        #endregion

        public async Task<List<CoreBranchVM>> GetAllAsync()
        {
            var entity = await _repository.All().ToListAsync();

            var result = entity.Select(model => new CoreBranchVM
            {
                BranchCode = model.BranchCode,
                BranchName = model.BranchName,
                Address = model.Address,
                Phone = model.Phone,
                Email = model.Email

            }).ToList();

            return result;
        }

        public async Task<CoreBranchVM> GetByIdAsync(int id)
        {
            //var data = await (from branch in _repository.All()
            //                join company in _company.All()
            //                    on branch.CompanyCode equals company.CompanyCode into compGroup
            //                from company in compGroup.DefaultIfEmpty() 
            //                where branch.autoId == id
            //                select new CoreBranchVM
            //                {
            //                    BranchCode = branch.BranchCode,
            //                    BranchName = branch.BranchName,
            //                    Address = branch.Address,
            //                    Phone = branch.Phone,
            //                    Email = branch.Email,
            //                    LDate = branch.LDate,
            //                    ModifyDate = branch.ModifyDate,
            //                    CompanyCode = branch.CompanyCode,
            //                    CompanyName = company != null ? company.CompanyName : null
            //                }
            //            ).FirstOrDefaultAsync();

            var data = await _repository.All().Where(pt => pt.autoId == id)
                        .Select(pt => new CoreBranchVM
                        {

                            BranchCode = pt.BranchCode,
                            BranchName = pt.BranchName,
                            Address = pt.Address,
                            Phone = pt.Phone,
                            Email = pt.Email,
                            LDate = pt.LDate,
                            ModifyDate = pt.ModifyDate,
                        }).FirstOrDefaultAsync();
            return data;
        }

        public async Task<string> GetLastBranchCodeAsync()
        {
            var last = await _repository.All()
                    .OrderByDescending(c => c.BranchCode)
                    .FirstOrDefaultAsync();

            return last?.BranchCode;
        }

        public async Task<PaginationService<Core_Branch, CoreBranchVM>.PaginationResult<CoreBranchVM>> GetPaginatedAsync(int pageNumber = 1, int pageSize = 10, string searchTerm = "", string sortColumn = "BranchCode", string sortOrder = "desc")
        {
            var query = _repository.All();

            if (pageSize == -1)
            {
                pageSize = await query.CountAsync();
                pageNumber = 1;
            }

            var paginatedResult = await PaginationService<Core_Branch, CoreBranchVM>.GetPaginatedData(query, pageNumber, pageSize, searchTerm, sortColumn, sortOrder,
                term => sc =>
                    EF.Functions.Like(sc.BranchCode ?? "", $"%{term}%") ||
                    EF.Functions.Like(sc.BranchName ?? "", $"{term}%") ||
                    EF.Functions.Like(sc.Address ?? "", $"%{term}%") ||
                    EF.Functions.Like(sc.Phone ?? "", $"%{term}%") ||
                    EF.Functions.Like(sc.Email ?? "", $"{term}%"),
                pt => new CoreBranchVM
                {
                    autoId = pt.autoId,
                    BranchCode = pt.BranchCode,
                    BranchName = pt.BranchName,
                    Address = pt.Address,
                    Phone = pt.Phone,
                    Email = pt.Email,
                    LIP = pt.LIP,
                    LMAC = pt.LMAC,
                    LDate = pt.LDate,
                    ModifyDate = pt.ModifyDate,
                    CompanyCode = pt.CompanyCode,
                    //CompanyName =
                });

            return paginatedResult;
        }

        public async Task<bool> IsDuplicateAsync(CoreBranchVM model)
        {
            if (model == null)
            {
                throw new ArgumentNullException(nameof(model));
            }

            var alldata = await _repository.All().ToListAsync();

            var isDuplicate = alldata.Any(pm =>
            string.Equals(pm.BranchName, model.BranchName, StringComparison.OrdinalIgnoreCase) &&
             pm.autoId != model.autoId

            );

            return isDuplicate;
        }

        public async Task<bool> IsExistAsync(int id)
        {
            return await _repository.All().AnyAsync(x => x.autoId == id);
        }

        public async Task<bool> SaveAsync(CoreBranchVM model)
        {
            try
            {
                var entity = new Core_Branch
                {
                    BranchCode = model.BranchCode,
                    BranchName = model.BranchName,
                    Address = model.Address ?? "",
                    BanglaBranch = model.BanglaBranch ?? "",
                    AddressBangla = model.AddressBangla ?? "" ,
                    Phone = model.Phone ?? "",
                    Email = model.Email ?? "",
                    Fax = model.Fax ?? "",
                    LUser = model.CreatedBy.ToString(),
                    LDate = DateTime.Now,
                    LIP = GetLocalIP() ?? "",
                    LMAC = GetMacAddress() ?? "",
                    CompanyCode = model.CompanyCode ?? "",
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

        public async Task<bool> UpdateAsync(CoreBranchVM model)
        {
            await _repository.BeginTransactionAsync();
            try
            {
                var entity = await _repository.GetByIdAsync(model.autoId);
                if (entity == null)
                {
                    await _repository.RollbackTransactionAsync();
                    return false;
                }

                entity.BranchCode = model.BranchCode;
                entity.BranchName = model.BranchName;
                entity.Address = model.Address ?? "";
                entity.BanglaBranch = model.BanglaBranch ?? "";
                entity.AddressBangla = model.AddressBangla ?? "";
                entity.Phone = model.Phone ?? "";
                entity.Email = model.Email ?? "";
                entity.LUser = model.UpdatedBy.ToString();
                entity.ModifyDate = DateTime.Now;
                entity.LIP = GetLocalIP() ?? "";
                entity.LMAC = GetMacAddress() ?? "";
                entity.CompanyCode = model.CompanyCode ?? "";


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

        #region Core Company Dropdown

        public async Task<List<CommonChoiceVM>> LoadCompanyDropdown()
        {
            var dropdowndata = await _company.All().Select(e => new CommonChoiceVM
            {
                Id = e.CompanyCode,
                Name = e.CompanyName,
            }).ToListAsync();

            return dropdowndata;
        }

        #endregion

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
