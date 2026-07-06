using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using GCTL.Core.Repository;
using GCTL.Core.ViewModels.MasterSetup.HrmDefDesignations;
using GCTL.Data.Models;
using Microsoft.EntityFrameworkCore;
using GCTL.Service.Pagination;

namespace GCTL.Service.MasterSetup.HrmDefDesignations
{
    public class HrmDefDesignationService : AppService<HRM_Def_Designation>, IHrmDefDesignationService
    {
        private readonly IGenericRepository<HRM_Def_Designation> _repository;

        public HrmDefDesignationService(IGenericRepository<HRM_Def_Designation> repository) : base(repository)
        {
            _repository = repository;
        }
        public async Task<bool> BulkDeleteAsync(List<decimal> ids)
        {
            await _repository.BeginTransactionAsync();

            try
            {
                var entity = await _repository.All().Where(c => ids.Contains(c.autoId)).ToListAsync();

                if (entity == null || entity.Count == 0)
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

        public async Task<HrmDefDesignationViewModel> GetByIdAsync(decimal id)
        {
            var data = await _repository.All().Where(pt => pt.autoId == id)
            .Select(pt => new HrmDefDesignationViewModel
            {
                AutoId = pt.autoId,
                DesignationCode = pt.DesignationCode ?? "",
                DesignationName = pt.DesignationName ?? "",
                DesignationShortName = pt.DesignationShortName ?? "",
                GradeCode = pt.GradeCode ?? "",
                Ldate = pt.LDate,
                ModifyDate = pt.ModifyDate,
            }).FirstOrDefaultAsync();
            return data;
        }

        public async Task<string> GetLastDesignationAsync()
        {
            var last = await _repository.All()
            .OrderByDescending(c => c.autoId)
            .FirstOrDefaultAsync();

            string newId = last.DesignationCode;


            return newId;
        }

        public async Task<PaginationService<HRM_Def_Designation, HrmDefDesignationViewModel>.PaginationResult<HrmDefDesignationViewModel>> GetPaginatedAsync(
            int pageNumber = 1,
            int pageSize = 5,
            string searchTerm = "",
            string sortColumn = "FullName",
            string sortOrder = "desc")
            {
            var query = _repository.All();

            // Search filter
            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(term =>
                    EF.Functions.Like(term.DesignationCode ?? "", $"%{searchTerm}%") ||
                    EF.Functions.Like(term.DesignationName ?? "", $"{searchTerm}%") ||
                    EF.Functions.Like(term.GradeCode ?? "", $"%{searchTerm}%") ||
                    EF.Functions.Like(term.DesignationShortName ?? "", $"%{searchTerm}%"));
            }

            // Sorting
            if (!string.IsNullOrEmpty(sortColumn))
            {
                var propertyInfo = typeof(HRM_Def_Designation).GetProperty(sortColumn);
                if (propertyInfo != null)
                {
                    query = sortOrder.ToLower() == "asc"
                        ? query.OrderBy(e => EF.Property<object>(e, sortColumn))
                        : query.OrderByDescending(e => EF.Property<object>(e, sortColumn));
                }
            }

            if (pageSize == -1)
            {
                var allData = query
                    .AsEnumerable() 
                    .Select(pt => new HrmDefDesignationViewModel
                    {
                        AutoId = pt.autoId,
                        DesignationCode = pt.DesignationCode ?? " ",
                        DesignationName = pt.DesignationName ?? " ",
                        DesignationShortName = pt.DesignationShortName ?? " ",
                        GradeCode = pt.GradeCode ?? " ",
                        StepNoId = pt.StepNoId ?? " ",
                        BanglaShortName = pt.BanglaShortName ?? " ",
                        BanglaDesignation = pt.BanglaDesignation ?? " ",
                        Ldate = pt.LDate,
                        ModifyDate = pt.ModifyDate,
                    })
                    .ToList();

                return new PaginationService<HRM_Def_Designation, HrmDefDesignationViewModel>.PaginationResult<HrmDefDesignationViewModel>
                {
                    Data = allData,
                    TotalCount = allData.Count,
                    PaginationInfo = new PaginationService<HRM_Def_Designation, HrmDefDesignationViewModel>.PaginationInfo
                    {
                        StartItem = allData.Count == 0 ? 0 : 1,
                        EndItem = allData.Count,
                        TotalItems = allData.Count,
                        PageNumbers = new List<int> { 1 },
                        TotalPages = 1,
                        CurrentPage = 1
                    }
                };
            }
            else
            {
                // normal pagination service call
                return await PaginationService<HRM_Def_Designation, HrmDefDesignationViewModel>.GetPaginatedData(
                    query,
                    pageNumber,
                    pageSize,
                    searchTerm,
                    sortColumn,
                    sortOrder,
                    term => sc => EF.Functions.Like(sc.DesignationCode ?? "", $"%{term}%") ||
                                  EF.Functions.Like(sc.DesignationName ?? "", $"{term}%") ||
                                  EF.Functions.Like(sc.GradeCode ?? "", $"%{term}%") ||
                                  EF.Functions.Like(sc.DesignationShortName ?? "", $"%{term}%"),
                    pt => new HrmDefDesignationViewModel
                    {
                        AutoId = pt.autoId,
                        DesignationCode = pt.DesignationCode ?? " ",
                        DesignationName = pt.DesignationName ?? " ",
                        DesignationShortName = pt.DesignationShortName ?? " ",
                        GradeCode = pt.GradeCode ?? " ",
                        StepNoId = pt.StepNoId ?? " ",
                        BanglaShortName = pt.BanglaShortName ?? " ",
                        BanglaDesignation = pt.BanglaDesignation ?? " ",
                        Ldate = pt.LDate,
                        ModifyDate = pt.ModifyDate,
                    });
            }
        }


        public async Task<bool> IsDuplicateAsync(  HrmDefDesignationViewModel model)
        {
            if (model == null)
            {
                throw new ArgumentNullException(nameof(model));
            }

            var paymentMode = await _repository.All().ToListAsync();
         
            var isDuplicate = paymentMode.Any(pm =>
                pm.DesignationName.ToLower() == model.DesignationName.ToLower()
                && pm.autoId != model.AutoId
            );

            return isDuplicate;
        }

        public async Task<bool> IsExistAsync(decimal id)
        {
            return await _repository.All().AnyAsync(x => x.autoId == id);
        }

        public async Task<bool> SaveAsync(HrmDefDesignationViewModel model)
        {
            try
            {
                var entity = new HRM_Def_Designation
                {
                    DesignationCode = model.DesignationCode,
                    DesignationName = model.DesignationName,
                    DesignationShortName = model.DesignationShortName ?? "",
                    GradeCode = model.GradeCode ?? "",
                    LUser = "",
                    LDate = DateTime.Now,
                    LIP = GetLocalIP() ?? "",
                    LMAC = GetMacAddress() ?? "",
                    StepNoId = model.StepNoId ?? "",
                    BanglaDesignation = model.BanglaDesignation ?? "",
                    BanglaShortName = model.BanglaShortName ?? ""
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

        public async Task<bool> UpdateAsync(HrmDefDesignationViewModel model)
        {
            await _repository.BeginTransactionAsync();
            try
            {
                var entity = await _repository.GetByIdAsync(model.AutoId);
                if (entity == null)
                {
                    await _repository.RollbackTransactionAsync();
                    return false;
                }

                entity.DesignationCode = model.DesignationCode;
                entity.DesignationName = model.DesignationName;
                entity.DesignationShortName = model.DesignationShortName ?? "";
                entity.GradeCode = model.GradeCode ?? "";
                entity.LUser = "";
                entity.StepNoId = model.StepNoId ?? "";
                entity.BanglaDesignation = model.BanglaDesignation ?? "";
                entity.BanglaShortName = model.BanglaShortName ?? "";
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

        public async Task<List<HrmDefDesignationViewModel>> GetAllAsync()
        {
            var entity = await _repository.All().ToListAsync();

            var result = entity.Select(model => new HrmDefDesignationViewModel
            {
                DesignationCode = model.DesignationCode,
                DesignationName = model.DesignationName,
                DesignationShortName = model.DesignationShortName,
                GradeCode = model.GradeCode,
            }).ToList();

            return result;
        }

        //Designation Dropdown
        public async Task<List<HrmDefDesignationViewModel>> DesignationDropdown()
        {
            var data = await _repository.All().ToListAsync();
            return data.Select(x => new HrmDefDesignationViewModel
            {
                DesignationCode = x.DesignationCode,
                DesignationName = x.DesignationName
            }).ToList();
        }


        #region IP & Mac Address
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
