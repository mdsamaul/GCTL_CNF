using GCTL.Core.Repository;
using GCTL.Core.ViewModels.MasterSetup.CurrencyType;
using GCTL.Data.Models;
using GCTL.Service.Pagination;
using Microsoft.EntityFrameworkCore;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;


namespace GCTL.Service.MasterSetup.CurrencyType
{
    public class CurrencyTypeService: AppService<CA_Def_Currency>, ICurrencyType
    {
        private readonly IGenericRepository<CA_Def_Currency> _repository;

        public CurrencyTypeService(IGenericRepository<CA_Def_Currency> repository) : base(repository)
        {
            _repository = repository;
        }
        public async Task<bool> BulkDeleteAsync(List<decimal> ids)
        {
            await _repository.BeginTransactionAsync();

            try
            {
                var entity = await _repository.All().Where(c => ids.Contains(c.TC)).ToListAsync();

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

        public async Task<CurrencyTypeVM> GetByIdAsync(decimal id)
        {
            var data = await _repository.All().Where(pt => pt.TC == id)
            .Select(pt => new CurrencyTypeVM
            {
                TC = pt.TC,
                CurrencyId = pt.CurrencyId ?? "",
                CurrencyName = pt.CurrencyName ?? "",
                ShortName = pt.ShortName ?? "",
                Symbol = pt.Symbol ?? "",
                DecimalPlaces = pt.DecimalPlaces ,
                NegativeFormat = pt.NegativeFormat,
                LUser = pt.LUser,
                LDate = pt.LDate,
                LIP = pt.LIP,
                LMAC = pt.LMAC,
                ModifyDate = pt.ModifyDate,
            }).FirstOrDefaultAsync();
            return data;
        }

        public async Task<string> GetLastCurrencyAsync()
        {
            var last = await _repository.All()
            .OrderByDescending(c => c.TC)
            .FirstOrDefaultAsync();
            return last?.CurrencyId;
        }

        public async Task<PaginationService<CA_Def_Currency, CurrencyTypeVM>.PaginationResult<CurrencyTypeVM>> GetPaginatedAsync(
        int pageNumber = 1,
        int pageSize = 10,
        string searchTerm = "",
        string sortColumn = "CurrencyId",
        string sortOrder = "desc")
        {
            var query = _repository.All();

            // Search filter
            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(term =>
                    EF.Functions.Like(term.CurrencyId ?? "", $"%{searchTerm}%") ||
                    EF.Functions.Like(term.CurrencyName ?? "", $"{searchTerm}%") ||
                    EF.Functions.Like(term.ShortName ?? "", $"%{searchTerm}%") ||
                    EF.Functions.Like(term.Symbol ?? "", $"%{searchTerm}%") 
                   
                    );
            }

            // Sorting
            if (!string.IsNullOrEmpty(sortColumn))
            {
                var propertyInfo = typeof(CA_Def_Currency).GetProperty(sortColumn);
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
                    .Select(pt => new CurrencyTypeVM
                    {
                        TC = pt.TC,
                        CurrencyId = pt.CurrencyId ?? " ",
                        CurrencyName = pt.CurrencyName ?? " ",
                        ShortName = pt.ShortName ?? " ",
                        Symbol = pt.Symbol ?? " ",
                        DecimalPlaces = pt.DecimalPlaces ,
                        NegativeFormat = pt.NegativeFormat ?? " ",
                        LUser = pt.LUser ?? " ",
                        LDate = pt.LDate,
                        ModifyDate = pt.ModifyDate,
                    })
                    .ToList();

                return new PaginationService<CA_Def_Currency, CurrencyTypeVM>.PaginationResult<CurrencyTypeVM>
                {
                    Data = allData,
                    TotalCount = allData.Count,
                    PaginationInfo = new PaginationService<CA_Def_Currency, CurrencyTypeVM>.PaginationInfo
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
                return await PaginationService<CA_Def_Currency, CurrencyTypeVM>.GetPaginatedData(
                    query,
                    pageNumber,
                    pageSize,
                    searchTerm,
                    sortColumn,
                    sortOrder,
                    term => sc => EF.Functions.Like(sc.CurrencyId ?? "", $"%{term}%") ||
                                  EF.Functions.Like(sc.CurrencyName ?? "", $"{term}%") ||
                                  EF.Functions.Like(sc.ShortName ?? "", $"%{term}%") ||
                                  EF.Functions.Like(sc.Symbol ?? "", $"%{term}%"),
                    pt => new CurrencyTypeVM
                    {
                        TC = pt.TC,
                        CurrencyId = pt.CurrencyId ?? " ",
                        CurrencyName = pt.CurrencyName ?? " ",
                        ShortName = pt.ShortName ?? " ",
                        Symbol = pt.Symbol ?? " ",
                        DecimalPlaces = pt.DecimalPlaces ,
                        NegativeFormat = pt.NegativeFormat ?? " ",
                        LDate = pt.LDate,
                        ModifyDate = pt.ModifyDate,
                    });
            }
        }


        public async Task<bool> IsDuplicateAsync(CurrencyTypeVM model)
        {
            if (model == null)
            {
                throw new ArgumentNullException(nameof(model));
            }

            var paymentMode = await _repository.All().ToListAsync();

            var isDuplicate = paymentMode.Any(pm =>
                pm.CurrencyName.ToLower() == model.CurrencyName.ToLower()
                && pm.TC != model.TC
            );

            return isDuplicate;
        }

        public async Task<bool> IsExistAsync(decimal id)
        {
            return await _repository.All().AnyAsync(x => x.TC == id);
        }

        public async Task<bool> SaveAsync(CurrencyTypeVM model)
        {
            try
            {
                var entity = new CA_Def_Currency
                {
                    CurrencyId = model.CurrencyId,
                    CurrencyName = model.CurrencyName,
                    ShortName = model.ShortName,
                    Symbol = model.Symbol,
                    DecimalPlaces = model.DecimalPlaces,
                    NegativeFormat = model.NegativeFormat,
                    LUser ="",
                    LDate = DateTime.Now,
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

        public async Task<bool> UpdateAsync(CurrencyTypeVM model)
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

                entity.CurrencyId = model.CurrencyId;
                entity.CurrencyName = model.CurrencyName;
                entity.ShortName = model.ShortName;
                entity.Symbol = model.Symbol;
                entity.DecimalPlaces = model.DecimalPlaces;
                entity.NegativeFormat = model.NegativeFormat;
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

        public async Task<List<CurrencyTypeVM>> GetAllAsync()
        {
            var entity = await _repository.All().ToListAsync();

            var result = entity.Select(model => new CurrencyTypeVM
            {
                CurrencyId = model.CurrencyId,
                CurrencyName = model.CurrencyName,
                ShortName = model.ShortName,
                Symbol = model.Symbol,
                DecimalPlaces = model.DecimalPlaces,
                NegativeFormat = model.NegativeFormat,
            }).ToList();

            return result;
        }

        // Currency Dropdown
        public async Task<List<CurrencyTypeVM>> GetAllCurrencyAsync()
        {
            var data = await _repository.All().ToListAsync();
            return data.Select(x => new CurrencyTypeVM
            {
                CurrencyId = x.CurrencyId,
                CurrencyName = x.CurrencyName
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
