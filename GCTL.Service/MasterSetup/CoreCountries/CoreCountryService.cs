using GCTL.Core.Repository;
using GCTL.Core.ViewModels.MasterSetup.CoreCountries;
//using GCTL.Core.ViewModels.PaymentManagements;
using GCTL.Data.Models;
using GCTL.Service.Pagination;
using Microsoft.EntityFrameworkCore;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;


namespace GCTL.Service.MasterSetup.CoreCountries
{
    public class CoreCountryService:AppService<Core_Country>,ICoreCountryService
    {
        private readonly IGenericRepository<Core_Country> _repository;

        public CoreCountryService(IGenericRepository<Core_Country> repository):base(repository) 
        {
            _repository = repository;
        }

        public async Task<bool> BulkDeleteAsync(List<int> ids)
        {
            await _repository.BeginTransactionAsync();

            try
            {
                var entity = await _repository.All().Where(c => ids.Contains(c.CountryCode)).ToListAsync();

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

                await _repository.DeleteAsync(entity.CountryCode);

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

        public async Task<CoreCountryViewModel> GetByIdAsync(int id)
        {
            var data = await _repository.All().Where(pt => pt.CountryCode == id)
            .Select(pt => new CoreCountryViewModel
            {
                CountryCode = pt.CountryCode,
                CountryId = pt.CountryID ?? "",
                CountryName = pt.CountryName ?? "",
                Ioccode = pt.IOCCode ?? "",
                Isocode=pt.ISOCode ?? "",
                Ldate = pt.LDate,
                ModifyDate = pt.ModifyDate,
            }).FirstOrDefaultAsync();
            return data;
        }

        public async Task<string> GetLastCountryCodeAsync()
        {
            var last = await _repository.All()
            .OrderByDescending(c => c.CountryCode)
            .FirstOrDefaultAsync();

            string newId = last.CountryID;


            return newId;
        }

        public async Task<PaginationService<Core_Country, CoreCountryViewModel>.PaginationResult<CoreCountryViewModel>> GetPaginatedAsync(
            int pageNumber = 1,
            int pageSize = 10,
            string searchTerm = "",
            string sortColumn = "CountryCode",
            string sortOrder = "desc")
        {
            var query = _repository.All();

            // Search filter
            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(term =>
                    EF.Functions.Like(term.CountryID ?? "", $"%{searchTerm}%") ||
                    EF.Functions.Like(term.CountryName ?? "", $"{searchTerm}%") ||
                    EF.Functions.Like(term.ISOCode ?? "", $"%{searchTerm}%") ||
                    EF.Functions.Like(term.IOCCode ?? "", $"%{searchTerm}%"));
            }

            // Sorting
            if (!string.IsNullOrEmpty(sortColumn))
            {
                var propertyInfo = typeof(Core_Country).GetProperty(sortColumn);
                if (propertyInfo != null)
                {
                    query = sortOrder.ToLower() == "asc"
                        ? query.OrderBy(e => EF.Property<object>(e, sortColumn))
                        : query.OrderByDescending(e => EF.Property<object>(e, sortColumn));
                }
            }

            if (pageSize == -1)
            {
                var allData = await query
                    .Select(pt => new CoreCountryViewModel
                    {
                        CountryCode = pt.CountryCode,
                        CountryId = pt.CountryID ?? "",
                        CountryName = pt.CountryName ?? "",
                        Ioccode = pt.IOCCode ?? "",
                        Isocode = pt.ISOCode ?? "",
                        Ldate = pt.LDate,
                        ModifyDate = pt.ModifyDate,
                    })
                    .ToListAsync();

                return new PaginationService<Core_Country, CoreCountryViewModel>.PaginationResult<CoreCountryViewModel>
                {
                    Data = allData,
                    TotalCount = allData.Count,
                    PaginationInfo = new PaginationService<Core_Country, CoreCountryViewModel>.PaginationInfo
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
                // Normal pagination service call
                return await PaginationService<Core_Country, CoreCountryViewModel>.GetPaginatedData(
                    query,
                    pageNumber,
                    pageSize,
                    searchTerm,
                    sortColumn,
                    sortOrder,
                    term => sc => EF.Functions.Like(sc.CountryID ?? "", $"%{term}%") ||
                                  EF.Functions.Like(sc.CountryName ?? "", $"{term}%") ||
                                  EF.Functions.Like(sc.ISOCode ?? "", $"%{term}%") ||
                                  EF.Functions.Like(sc.IOCCode ?? "", $"%{term}%"),
                    pt => new CoreCountryViewModel
                    {
                        CountryCode = pt.CountryCode,
                        CountryId = pt.CountryID ?? "",
                        CountryName = pt.CountryName ?? "",
                        Ioccode = pt.IOCCode ?? "",
                        Isocode = pt.ISOCode ?? "",
                        Ldate = pt.LDate,
                        ModifyDate = pt.ModifyDate,
                    });
            }
        }


        public async Task<bool> IsDuplicateAsync(CoreCountryViewModel model)
        {
            if (model == null)
            {
                throw new ArgumentNullException(nameof(model));
            }

            var paymentMode = await _repository.All().ToListAsync();

            var isDuplicate = paymentMode.Any(pm =>
                pm.CountryName.ToLower() == model.CountryName.ToLower()
                && pm.CountryCode !=model.CountryCode
            );

            return isDuplicate;
        }

        public async Task<bool> IsExistAsync(int id)
        {
            return await _repository.All().AnyAsync(x => x.CountryCode == id);
        }

        public async Task<bool> SaveAsync(CoreCountryViewModel model)
        {
            try
            {
                var entity = new Core_Country
                {
                    CountryID = model.CountryId,
                    CountryName = model.CountryName,
                    IOCCode = model.Ioccode ?? "",
                    ISOCode = model.Isocode ?? "",
                    LDate = DateTime.Now,
                    LUser ="",
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

        public async Task<bool> UpdateAsync(CoreCountryViewModel model)
        {
            await _repository.BeginTransactionAsync();
            try
            {
                var entity = await _repository.GetByIdAsync(model.CountryCode);
                if (entity == null)
                {
                    await _repository.RollbackTransactionAsync();
                    return false;
                }

                entity.CountryID = model.CountryId;
                entity.CountryName = model.CountryName;
                entity.IOCCode = model.Ioccode ?? "";
                entity.ISOCode = model.Isocode ?? "";
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

        public async Task<List<CoreCountryViewModel>> GetAllAsync()
        {
            var entity = await _repository.All().ToListAsync();

            var result = entity.Select(model => new CoreCountryViewModel
            {
                CountryId = model.CountryID,
                CountryName = model.CountryName,
                Ioccode = model.IOCCode,
                Isocode = model.ISOCode,
             
            }).ToList();

            return result;
        }

        // Country Dropdown
        public async Task<List<CoreCountryViewModel>> GetAllCountryAsync()
        {
            var data = await _repository.All().ToListAsync();
            return data.Select(x => new CoreCountryViewModel
            {
                CountryId = x.CountryID,
                CountryName = x.CountryName
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
