using GCTL.Core.Repository;
using GCTL.Core.ViewModels.AddSalesCustomer;
using GCTL.Data.Models;
using GCTL.Service.Pagination;
using Microsoft.EntityFrameworkCore;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;

namespace GCTL.Service.CustomerRelationshipManagement.AddContactPerson
{
    public class ContactPersonService : AppService<Sales_ContactPerson>, IContactPersonService
    {
        private readonly IGenericRepository<Sales_ContactPerson> _repository;
        private readonly IGenericRepository<HRM_Def_Designation> _hrmDefDesignationService;
        private readonly AppDbContext _context;

        public ContactPersonService(IGenericRepository<Sales_ContactPerson> contactPerson, AppDbContext context, IGenericRepository<HRM_Def_Designation> hrmDefDesignationService) : base(contactPerson)
        {
            _repository = contactPerson;
            _context = context;
            _hrmDefDesignationService = hrmDefDesignationService;
        }

        public async Task<bool> BulkDeleteAsync(List<decimal> ids)
        {
            await _repository.BeginTransactionAsync();

            try
            {
                var entity = await _repository.All().Where(c => ids.Contains(c.AutoId)).ToListAsync();

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

                await _repository.DeleteAsync(entity.AutoId);

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

        public async Task<List<SalesContactPersonViewModel>> GetAllAsync()
        {
            var entity = await _repository.All().ToListAsync();

            var result = entity.Select(model => new SalesContactPersonViewModel
            {
                Cpid = model.CPID,
                ContactPersonName = model.ContactPersonName,
                DesignationCode = model.DesignationCode,
                ContactPersonMobile = model.ContactPersonMobile,
                ContactPersonEmail = model.ContactPersonEmail,
            }).ToList();

            return result;
        }

        public async Task<SalesContactPersonViewModel> GetByIdAsync(decimal id)
        {
            var data = await _repository.All().Where(pt => pt.AutoId == id)
                        .Select(pt => new SalesContactPersonViewModel
                        {
                            AutoId = pt.AutoId,
                            Cpid = pt.CPID,
                            ContactPersonName = pt.ContactPersonName,
                            ContactPersonMobile = pt.ContactPersonMobile,
                            ContactPersonEmail = pt.ContactPersonEmail,
                            DesignationCode = pt.DesignationCode,
                            CompanyCode = pt.CompanyCode,
                            EmployeeId = pt.EmployeeID,
                            Ldate = pt.LDate,
                            ModifyDate = pt.ModifyDate,
                        }).FirstOrDefaultAsync();
            return data;
        }

        public async Task<string> GetLastCPIDAsync()
        {
            var last = await _repository.All()
                    .OrderByDescending(c => c.CPID)
                    .FirstOrDefaultAsync();

            return last?.CPID;
        }

        public async Task<PaginationService<Sales_ContactPerson, SalesContactPersonViewModel>.PaginationResult<SalesContactPersonViewModel>> GetPaginatedAsync(int pageNumber = 1, int pageSize = 10, string searchTerm = "", string sortColumn = "CPID", string sortOrder = "desc")
        {
            var query = _repository.All();

            if (pageSize == -1)
            {
                pageSize = await query.CountAsync();
                pageNumber = 1;
            }

            var paginatedResult = await PaginationService<Sales_ContactPerson, SalesContactPersonViewModel>.GetPaginatedData(
                query,
                pageNumber,
                pageSize,
                searchTerm,
                sortColumn,
                sortOrder,
                term => sc =>
                    EF.Functions.Like(sc.CPID ?? "", $"%{term}%") ||
                    EF.Functions.Like(sc.ContactPersonName ?? "", $"{term}%") ||
                    EF.Functions.Like(_context.HRM_Def_Designation
                                .Where(b => b.DesignationCode == sc.DesignationCode)
                                .Select(b => b.DesignationName)
                                .FirstOrDefault() ?? "", $"%{term}%") ||
                    EF.Functions.Like(sc.ContactPersonMobile ?? "", $"{term}%") ||
                    EF.Functions.Like(sc.ContactPersonEmail ?? "", $"{term}%"),
                pt => new SalesContactPersonViewModel
                {
                    AutoId = pt.AutoId,
                    Cpid = pt.CPID,
                    ContactPersonName = pt.ContactPersonName,
                    DesignationCode = pt.DesignationCode,
                    ContactPersonMobile = pt.ContactPersonMobile,
                    ContactPersonEmail = pt.ContactPersonEmail,
                    Ldate = pt.LDate,
                    ModifyDate = pt.ModifyDate,
                    Lip = pt.LIP,
                    Lmac = pt.LMAC,
                    DesignationName = _context.Set<HRM_Def_Designation>()
                                            .Where(cs => cs.DesignationCode == pt.DesignationCode)
                                            .Select(cs => cs.DesignationName)
                                            .FirstOrDefault(),
                });

            return paginatedResult;
        }

        public async Task<bool> IsDuplicateAsync(SalesContactPersonViewModel model)
        {
            if (model == null)
            {
                throw new ArgumentNullException(nameof(model));
            }

            var alldata = await _repository.All().ToListAsync();

            var isDuplicate = alldata.Any(pm =>
            string.Equals(pm.ContactPersonName, model.ContactPersonName, StringComparison.OrdinalIgnoreCase) &&
             pm.AutoId != model.AutoId

            );

            return isDuplicate;
        }

        public async Task<bool> IsExistAsync(decimal id)
        {
            return await _repository.All().AnyAsync(x => x.AutoId == id);
        }

        public async Task<bool> SaveAsync(SalesContactPersonViewModel model)
        {
            try
            {
                var entity = new Sales_ContactPerson
                {
                    CPID = model.Cpid,
                    ContactPersonName = model.ContactPersonName,
                    DesignationCode = model.DesignationCode,
                    ContactPersonMobile = model.ContactPersonMobile ?? "",
                    ContactPersonEmail = model.ContactPersonEmail ?? "",
                    CompanyCode = model.CompanyCode ?? "",
                    EmployeeID = model.EmployeeId ?? "",
                    LUser = "",
                    LDate = DateTime.Now,
                    LIP = GetLocalIP() ?? "",
                    LMAC = GetMacAddress() ?? "",
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

        public async Task<bool> UpdateAsync(SalesContactPersonViewModel model)
        {
            await _repository.BeginTransactionAsync();
            try
            {
                //var entity = await _repository.All().FirstOrDefaultAsync(e => e.AutoId == model.AutoId);
                var entity = await _repository.GetByIdAsync(model.AutoId);
                if (entity == null)
                {
                    await _repository.RollbackTransactionAsync();
                    return false;
                }

                entity.CPID = model.Cpid;
                entity.ContactPersonName = model.ContactPersonName;
                entity.DesignationCode = model.DesignationCode;
                entity.ContactPersonMobile = model.ContactPersonMobile ?? "";
                entity.ContactPersonEmail = model.ContactPersonEmail ?? "";
                entity.CompanyCode = model.CompanyCode ?? "";
                entity.EmployeeID = model.EmployeeId ?? "";
                entity.LUser = model.Luser; //"";
                entity.ModifyDate = DateTime.Now;
                entity.LIP = GetLocalIP() ?? "";
                entity.LMAC = GetMacAddress() ?? "";



                await _repository.UpdateAsync(entity);

                //await _repository.SaveChangesAsync();

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


        public async Task<List<SalesContactPersonViewModel>> DropdownContact()
        {
            var data = await (from cp in _repository.All()
                              join des in _hrmDefDesignationService.All()
                              on cp.DesignationCode equals des.DesignationCode into desGroup
                              from desItem in desGroup.DefaultIfEmpty() 
                              select new SalesContactPersonViewModel
                              {
                                  Cpid = cp.CPID,
                                  ContactPersonName = cp.ContactPersonName,
                                  DesignationCode = cp.DesignationCode,
                                  DesignationName = desItem != null ? desItem.DesignationName : "", 
                                  ContactPersonMobile = cp.ContactPersonMobile,
                                  ContactPersonEmail = cp.ContactPersonEmail
                              }).ToListAsync();

            return data;
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
