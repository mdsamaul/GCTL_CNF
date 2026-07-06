using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using GCTL.Core.Repository;
using GCTL.Data.Models;
using Microsoft.EntityFrameworkCore;
using GCTL.Service.MasterSetup.PaymentMode;
using GCTL.Core.ViewModels.MasterSetup.HRMDefDepartment;
using GCTL.Service.Pagination;

namespace GCTL.Service.MasterSetup.HrmDefDepartment
{
    public class DepartmentService: AppService<HRM_Def_Department>, IDepartment
    {
        private readonly IGenericRepository<HRM_Def_Department> _repository;
        public DepartmentService(IGenericRepository<HRM_Def_Department> genericRepository) : base(genericRepository)
        {
            _repository = genericRepository;
        }

        public async Task<bool> BulkDeleteAsync(List<decimal> ids)
        {
            await _repository.BeginTransactionAsync();

            try
            {
                var entity = await _repository.All().Where(c => ids.Contains(c.autoId)).ToListAsync();

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

        public async Task<List<DepartmentVM>> GetAllAsync()
        {
            var entity = await _repository.All().ToListAsync();

            var result = entity.Select(model => new DepartmentVM
            {
                DepartmentCode = model.DepartmentCode,
                DepartmentName = model.DepartmentName,
                DepartmentShortName = model.DepartmentShortName,
                BanglaDepartment = model.BanglaDepartment,
                BanglaShortName = model.BanglaShortName

            }).ToList();

            return result;
        }

        public async Task<DepartmentVM> GetByIdAsync(int id)
        {
            var data = await _repository.All().Where(pt => pt.autoId == id)
                        .Select(pt => new DepartmentVM
                        {

                            DepartmentCode = pt.DepartmentCode,
                            DepartmentName = pt.DepartmentName,
                            DepartmentShortName = pt.DepartmentShortName,
                            BanglaDepartment = pt.BanglaDepartment,
                            BanglaShortName = pt.BanglaShortName,
                            LDate = pt.LDate,
                            ModifyDate = pt.ModifyDate,
                        }).FirstOrDefaultAsync();
            return data;
        }

        public async Task<string> GetLastDepartmentCodeAsync()
        {
            var last = await _repository.All()
                    .OrderByDescending(c => c.DepartmentCode)
                    .FirstOrDefaultAsync();

            return last?.DepartmentCode;
        }

        public async Task<PaginationService<HRM_Def_Department, DepartmentVM>.PaginationResult<DepartmentVM>> GetPaginatedAsync(int pageNumber = 1, int pageSize = 10, string searchTerm = "", string sortColumn = "DepartmentCode", string sortOrder = "desc")
        {
            var query = _repository.All();

            if (pageSize == -1)
            {
                pageSize = await query.CountAsync();
                pageNumber = 1;
            }

            var paginatedResult = await PaginationService<HRM_Def_Department, DepartmentVM>.GetPaginatedData(
                query,
                pageNumber,
                pageSize,
                searchTerm,
                sortColumn,
                sortOrder,
                term => sc =>
                    EF.Functions.Like(sc.DepartmentCode ?? "", $"%{term}%") ||
                    EF.Functions.Like(sc.DepartmentName ?? "", $"{term}%") ||
                    EF.Functions.Like(sc.DepartmentShortName ?? "", $"%{term}%")||
                    EF.Functions.Like(sc.BanglaDepartment ?? "", $"%{term}%") ||
                    EF.Functions.Like(sc.BanglaShortName ?? "", $"{term}%"),
                pt => new DepartmentVM
                {
                    autoId = pt.autoId,
                    DepartmentCode = pt.DepartmentCode,
                    DepartmentName = pt.DepartmentName,
                    DepartmentShortName = pt.DepartmentShortName,
                    BanglaDepartment = pt.BanglaDepartment,
                    BanglaShortName = pt.BanglaShortName,
                    LIP = pt.LIP ,
                    LMAC = pt.LMAC,
                    LDate = pt.LDate,
                    ModifyDate = pt.ModifyDate,
                });

            return paginatedResult;
        }

        public async Task<bool> IsDuplicateAsync(DepartmentVM model)
        {
            if (model == null)
            {
                throw new ArgumentNullException(nameof(model));
            }

            var alldata = await _repository.All().ToListAsync();

            var isDuplicate = alldata.Any(pm =>
            string.Equals(pm.DepartmentName, model.DepartmentName, StringComparison.OrdinalIgnoreCase) &&
             pm.autoId != model.autoId

            );

            return isDuplicate;
        }

        public async Task<bool> IsExistAsync(int id)
        {
            return await _repository.All().AnyAsync(x => x.autoId == id);
        }

        public async Task<bool> SaveAsync(DepartmentVM model)
        {
            try
            {
                var entity = new HRM_Def_Department
                {
                    DepartmentCode = model.DepartmentCode,
                    DepartmentName = model.DepartmentName,
                    DepartmentShortName = model.DepartmentShortName ?? "",
                    BanglaDepartment = model.BanglaDepartment ?? "",
                    BanglaShortName = model.BanglaShortName ?? "" ,
                    LUser = "", //If needed
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

        public async Task<bool> UpdateAsync(DepartmentVM model)
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

                entity.DepartmentCode = model.DepartmentCode;
                entity.DepartmentName = model.DepartmentName;
                entity.DepartmentShortName = model.DepartmentShortName ?? "";
                entity.BanglaDepartment = model.BanglaDepartment ?? "";
                entity.BanglaShortName = model.BanglaShortName ?? "";
                entity.LUser = "";
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
