using GCTL.Core.Repository;
using GCTL.Core.ViewModels.MasterSetup.CoreCompany;
using GCTL.Data.Models;
using GCTL.Service.Pagination;
using Microsoft.EntityFrameworkCore;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;


namespace GCTL.Service.MasterSetup.CoreCompany
{
    public class CoreCompanyService:AppService<Core_Company>,ICoreCompany
    {
        #region Service
        private readonly IGenericRepository<Core_Company> _repository;
        public CoreCompanyService(IGenericRepository<Core_Company> genericRepository) : base(genericRepository)
        {
            _repository = genericRepository;
        }

        #endregion


        #region Delete
        public async Task<bool> BulkDeleteAsync(List<int> ids)
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

        #endregion


        #region Get All Company
        public async Task<List<CoreCompanyVM>> GetAllAsync()
        {
            var entity = await _repository.All().ToListAsync();

            var result = entity.Select(model => new CoreCompanyVM
            {
                CompanyCode = model.CompanyCode,
                CompanyName = model.CompanyName,
                CompanyShortName = model.CompanyShortName,
                Address1 = model.Address1,
                Address2 = model.Address2,
                Country = model.Country,
                BaseCurrency = model.BaseCurrency
            }).ToList();

            return result;
        }
        #endregion


        #region Get Company info by a specific ID
        public async Task<CoreCompanyVM> GetByIdAsync(int id)
        {
            var data = await _repository.All().Where(pt => pt.AutoID == id)
                        .Select(pt => new CoreCompanyVM
                        {
                            AutoID = pt.AutoID,
                            CompanyCode = pt.CompanyCode,
                            CompanyName = pt.CompanyName,
                            CompanyShortName = pt.CompanyShortName,
                            CompanyBanglaName = pt.CompanyBanglaName,
                            AddressBangla = pt.AddressBangla,
                            Address1 = pt.Address1,
                            Address2 = pt.Address2,
                            Phone1 = pt.Phone1,
                            Phone2 = pt.Phone2,
                            BIN = pt.BIN,
                            ZipCode = pt.ZipCode,
                            City = pt.City,
                            State = pt.State,
                            Country = pt.Country,
                            TIN = pt.TIN,
                            HotLine = pt.HotLine,
                            Email = pt.Email,
                            URL = pt.URL,
                            RegNo = pt.RegNo,
                            BaseCurrency =pt.BaseCurrency,
                            LDate = pt.LDate,
                            ModifyDate = pt.ModifyDate,
                        }).FirstOrDefaultAsync();
            return data;
        }

        #endregion


        #region Generate Company Code
        public async Task<string> GetLastCompanyCodeAsync()
        {
            var last = await _repository.All()
                    .OrderByDescending(c => c.CompanyCode)
                    .FirstOrDefaultAsync();

            return last?.CompanyCode;
        }

        #endregion


        #region Get all Company in table
        public async Task<PaginationService<Core_Company, CoreCompanyVM>.PaginationResult<CoreCompanyVM>> GetPaginatedAsync(int pageNumber = 1, int pageSize = 10, string searchTerm = "", string sortColumn = "CompanyCode", string sortOrder = "desc")
        {
            var query = _repository.All();

            if (pageSize == -1)
            {
                pageSize = await query.CountAsync();
                pageNumber = 1;
            }

            var paginatedResult = await PaginationService<Core_Company, CoreCompanyVM>.GetPaginatedData(
                query,
                pageNumber,
                pageSize,
                searchTerm,
                sortColumn,
                sortOrder,
                term => sc =>
                    EF.Functions.Like(sc.CompanyCode ?? "", $"%{term}%") ||
                    EF.Functions.Like(sc.CompanyName ?? "", $"{term}%") ||
                    EF.Functions.Like(sc.CompanyShortName ?? "", $"%{term}%") ||
                    EF.Functions.Like(sc.Address1 ?? "", $"{term}%") ||
                    EF.Functions.Like(sc.Address2 ?? "", $"{term}%"),
                pt => new CoreCompanyVM
                {
                    AutoID = pt.AutoID,
                    CompanyCode = pt.CompanyCode,
                    CompanyName = pt.CompanyName,
                    CompanyShortName = pt.CompanyShortName,
                    Address1 = pt.Address1,
                    Address2 = pt.Address2,
                    LDate = pt.LDate,
                    ModifyDate = pt.ModifyDate,
                    LIP = pt.LIP,
                    LMAC = pt.LMAC,
                    //BankName = _context.Set<SALES_Def_BankInfo>()
                    //                        .Where(cs => cs.BankID == pt.BankID)
                    //                        .Select(cs => cs.BankName)
                    //                        .FirstOrDefault(),
                });

            return paginatedResult;
        }

        #endregion


        #region Checking Duplicate
        public async Task<bool> IsDuplicateAsync(CoreCompanyVM model)
        {
            if (model == null)
            {
                throw new ArgumentNullException(nameof(model));
            }

            var alldata = await _repository.All().ToListAsync();

            var isDuplicate = alldata.Any(pm =>
            string.Equals(pm.CompanyName, model.CompanyName, StringComparison.OrdinalIgnoreCase) &&
             pm.AutoID != model.AutoID

            );

            return isDuplicate;
        }

        #endregion


        #region Getting Exist ID
        public async Task<bool> IsExistAsync(int id)
        {
            return await _repository.All().AnyAsync(x => x.AutoID == id);
        }

        #endregion


        #region Save Compnay
        public async Task<bool> SaveAsync(CoreCompanyVM model)
        {
            try
            {
                var entity = new Core_Company
                {
                    CompanyCode = model.CompanyCode,
                    CompanyName = model.CompanyName,
                    CompanyShortName = model.CompanyShortName ?? "",
                    CompanyBanglaName = model.CompanyBanglaName ?? "",
                    AddressBangla = model.AddressBangla ?? "",
                    Address1 = model.Address1,
                    Address2 = model.Address2 ?? "",
                    Phone1 = model.Phone1 ?? "",
                    Phone2 = model.Phone2 ?? "",
                    BIN = model.BIN ?? "",
                    ZipCode = model.ZipCode ?? "",
                    City = model.City ?? "",
                    State = model.State ?? "",
                    Country = model.Country,
                    TIN = model.TIN ?? "",
                    HotLine = model.HotLine ?? "",
                    Email = model.Email ?? "",
                    URL = model.URL ?? "",
                    RegNo = model.RegNo ?? "",
                    BaseCurrency = model.BaseCurrency ?? "",
                    LUser = "",
                    LDate = DateTime.Now,
                    LIP = GetLocalIP() ?? "",
                    LMAC = GetMacAddress() ?? "",
                    ImgTitle = model.ImgTitle ?? "",
                    Fax = model.Fax ?? "",
                    BackImage = model.BackImage ?? ""
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

        #endregion


        #region UPdate Company
        public async Task<bool> UpdateAsync(CoreCompanyVM model)
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

                entity.CompanyCode = model.CompanyCode;
                entity.CompanyName = model.CompanyName;
                entity.CompanyShortName = model.CompanyShortName ?? "";
                entity.CompanyBanglaName = model.CompanyBanglaName ?? "";
                entity.AddressBangla = model.AddressBangla ?? "";
                entity.Address1 = model.Address1;
                entity.Address2 = model.Address2 ?? "";
                entity.Phone1 = model.Phone1 ?? "";
                entity.Phone2 = model.Phone2 ?? "";
                entity.BIN = model.BIN ?? "";
                entity.ZipCode = model.ZipCode ?? "";
                entity.City = model.City ?? "";
                entity.State = model.State ?? "";
                entity.Country = model.Country;
                entity.TIN = model.TIN ?? "";
                entity.HotLine = model.HotLine ?? "";
                entity.Email = model.Email ?? "";
                entity.URL = model.URL ?? "";
                entity.RegNo = model.RegNo ?? "";
                entity.BaseCurrency = model.BaseCurrency ?? "";
                entity.LUser = "";
                entity.ModifyDate = DateTime.Now;
                entity.LIP = GetLocalIP() ?? "";
                entity.LMAC = GetMacAddress() ?? "";
                entity.ImgTitle = model.ImgTitle ?? "";
                entity.Fax = model.Fax ?? "";
                entity.BackImage = model.BackImage ?? "";

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
        #endregion


        #region Company Dropdown
        public async Task<List<CoreCompanyVM>> DropdownCompany()
        {
            var data = await _repository.All().ToListAsync();
            return data.Select(x => new CoreCompanyVM
            {
                CompanyCode = x.CompanyCode,
                CompanyName = x.CompanyName
            }).ToList();
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
