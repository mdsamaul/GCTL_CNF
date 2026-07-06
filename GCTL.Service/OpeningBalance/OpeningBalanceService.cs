using GCTL.Core.Repository;
using GCTL.Core.ViewModels;
using GCTL.Core.ViewModels.OpeningBalance;
using GCTL.Data.Models;
using GCTL.Service.Pagination;
using Microsoft.EntityFrameworkCore;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;

namespace GCTL.Service.OpeningBalance
{
    public class OpeningBalanceService : AppService<Acc_Company_Opening_Balance>, IOpeningBalance
    {
        private readonly IGenericRepository<Acc_Company_Opening_Balance> _repository;
        private readonly IGenericRepository<Core_Company> _company;
        private readonly IGenericRepository<Core_Branch> _branch;
        private readonly IGenericRepository<Acc_SubSubsidiaryLedger> _generalLedger; 
        private readonly IGenericRepository<Acc_SubsidiaryLedger> _subsidiary;
        private readonly IGenericRepository<Acc_GeneralLedger> _subcontrol;
        private readonly IGenericRepository<Sales_Customer> _salesCustomer;
        public OpeningBalanceService(IGenericRepository<Acc_Company_Opening_Balance> genericRepository, IGenericRepository<Core_Company> company, IGenericRepository<Core_Branch> branch, IGenericRepository<Acc_SubSubsidiaryLedger> generalLedger, IGenericRepository<Sales_Customer> salesCustomer, IGenericRepository<Acc_SubsidiaryLedger> subsidiary, IGenericRepository<Acc_GeneralLedger> subcontrol) : base(genericRepository)
        {
            _repository = genericRepository;
            _company = company;
            _branch = branch;
            _generalLedger = generalLedger;
            _salesCustomer = salesCustomer;
            _subsidiary = subsidiary;
            _subcontrol = subcontrol;
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

        public async Task<List<OpeningBalanceVM>> GetAllAsync()
        {
            var entity = await _repository.All().ToListAsync();

            var result = entity.Select(model => new OpeningBalanceVM
            {
                ComOpeningBalanceCode = model.ComOpeningBalanceCode,
                CompanyCode = model.CompanyCode,
                BranchCode = model.BranchCode,
                SubSusidiaryLedgerCodeNo = model.SubSusidiaryLedgerCodeNo,
                OpeningBalance = model.OpeningBalance,
                TrType = model.TrType
            }).ToList();

            return result;
        }

        public async Task<OpeningBalanceVM> GetByIdAsync(int id)
        {
            var data = await (from openbalance in _repository.All()
                              join company in _company.All()
                                  on openbalance.CompanyCode equals company.CompanyCode
                              join branch in _branch.All()
                                  on openbalance.BranchCode equals branch.BranchCode
                                join accounthead in _generalLedger.All()
                                  on openbalance.SubSusidiaryLedgerCodeNo equals accounthead.SubSusidiaryLedgerCodeNo
                              where openbalance.autoId == id
                              select new OpeningBalanceVM
                              {
                                  ComOpeningBalanceCode = openbalance.ComOpeningBalanceCode,                                
                                  OpeningBalance = openbalance.OpeningBalance,
                                  TrType = openbalance.TrType,
                                  LDate = openbalance.LDate,
                                  ModifyDate = openbalance.ModifyDate,
                                  CompanyCode = company.CompanyCode,
                                  BranchCode = branch.BranchCode,
                                  SubSusidiaryLedgerCodeNo = accounthead.SubSusidiaryLedgerCodeNo,
                              }).FirstOrDefaultAsync();

            return data;

        }


        public async Task<string> GetLastVoucherTypeCodeAsync()
        {
            var allCodes = await _repository.All()
                                 .Select(c => c.ComOpeningBalanceCode)
                                 .ToListAsync();

            int maxCode = 0;

            foreach (var code in allCodes)
            {
                if (int.TryParse(code, out int numericCode))
                {
                    if (numericCode > maxCode)
                        maxCode = numericCode;
                }
            }

            int nextCode = maxCode + 1;

            return nextCode.ToString();
        }

        //public async Task<string> GetLastOpeiningBalanceCodeAsync()
        //{
           
        //    var last = await _repository.All()
        //        .OrderByDescending(c => c.ComOpeningBalanceCode)
        //        .FirstOrDefaultAsync();

        //    int nextCode = 1;

        //    if (last != null && int.TryParse(last.ComOpeningBalanceCode, out int lastCode))
        //    {
        //        nextCode = lastCode + 1;
        //    }

        //    string formattedCode = nextCode.ToString("D8");

        //    return formattedCode;
        //}
        public async Task<string> GetLastOpeiningBalanceCodeAsync()
        {
            var lastCodeString = await _repository.All()
                .MaxAsync(c => c.ComOpeningBalanceCode);

            long nextCode = 1;

            if (!string.IsNullOrEmpty(lastCodeString) && long.TryParse(lastCodeString, out long lastCode))
            {
                nextCode = lastCode + 1;
            }

            return nextCode.ToString("D8");
        }


        //public async Task<PaginationService<Acc_Company_Opening_Balance, OpeningBalanceVM>.PaginationResult<OpeningBalanceVM>> GetPaginatedAsync(int pageNumber = 1, int pageSize = 5, string searchTerm = "", string sortColumn = "ComOpeningBalanceCode", string sortOrder = "desc")
        //{
        //    var query = from ob in _repository.All()
        //                join c in _company.All() on ob.CompanyCode equals c.CompanyCode into comp
        //                from company in comp.DefaultIfEmpty() // left join
        //                join b in _branch.All() on new { ob.CompanyCode, ob.BranchCode } equals new { b.CompanyCode, b.BranchCode } into br
        //                from branch in br.DefaultIfEmpty() // left join
        //                join gl in _generalLedger.All() on ob.SubSusidiaryLedgerCodeNo equals gl.SubSusidiaryLedgerCodeNo into gls
        //                from ledger in gls.DefaultIfEmpty() // left join
        //                select new
        //                {
        //                    ob,
        //                    CompanyName = company != null ? company.CompanyName : "",
        //                    BranchName = branch != null ? branch.BranchName : "",
        //                    AccountHead = ledger != null ? ledger.SubSubsidiaryLedgerName : ""
        //                };

        //    if (pageSize == -1)
        //    {
        //        pageSize = await query.CountAsync();
        //        pageNumber = 1;
        //    }

        //    var paginatedResult = await PaginationService<Acc_Company_Opening_Balance, OpeningBalanceVM>.GetPaginatedData(query.Select(x => x.ob), pageNumber, pageSize, searchTerm, sortColumn, sortOrder,
        //        term => sc =>
        //            EF.Functions.Like(sc.CompanyCode ?? "", $"%{term}%") ||
        //            EF.Functions.Like(sc.BranchCode ?? "", $"{term}%") ||
        //            EF.Functions.Like(sc.SubSusidiaryLedgerCodeNo ?? "", $"%{term}%"),
        //        pt => new OpeningBalanceVM
        //        {
        //            autoId = pt.autoId,
        //            ComOpeningBalanceCode = pt.ComOpeningBalanceCode,
        //            CompanyCode = pt.CompanyCode,
        //            BranchCode = pt.BranchCode,
        //            SubSusidiaryLedgerCodeNo = pt.SubSusidiaryLedgerCodeNo,
        //            OpeningBalance = pt.OpeningBalance,
        //            TrType = pt.TrType,
        //            LIP = pt.LIP,
        //            LMAC = pt.LMAC,
        //            LDate = pt.LDate,
        //            ModifyDate = pt.ModifyDate,

        //            CompanyName = query.Where(x => x.ob.CompanyCode == pt.CompanyCode).Select(x => x.CompanyName).FirstOrDefault(),
        //            BranchName = query.Where(x => x.ob.BranchCode == pt.BranchCode).Select(x => x.BranchName).FirstOrDefault(),
        //            AccountHead = query.Where(x => x.ob.SubSusidiaryLedgerCodeNo == pt.SubSusidiaryLedgerCodeNo).Select(x => x.AccountHead).FirstOrDefault()
        //        });

        //    return paginatedResult;
        //}


        public async Task<PaginationService<OpeningBalanceVM, OpeningBalanceVM>.PaginationResult<OpeningBalanceVM>>GetPaginatedAsync( int pageNumber = 1, int pageSize = 5, string searchTerm = "", string sortColumn = "ComOpeningBalanceCode", string sortOrder = "desc")
        {

            var query = from ob in _repository.All() // Acc_Company_Opening_Balance
                        join c in _company.All() on ob.CompanyCode equals c.CompanyCode into companyJoin
                        from comp in companyJoin.DefaultIfEmpty()
                        join b in _branch.All() on ob.BranchCode equals b.BranchCode into branchJoin
                        from br in branchJoin.DefaultIfEmpty()
                        join gl in _generalLedger.All() on ob.SubSusidiaryLedgerCodeNo equals gl.SubSusidiaryLedgerCodeNo into ledgerJoin
                        from gl in ledgerJoin.DefaultIfEmpty()
                        join sc in _salesCustomer.All() on gl.SubSubsidiaryLedgerName equals sc.CustomerID into customerGroup
                        from customer in customerGroup.DefaultIfEmpty()
                        join ssl in _subsidiary.All() on gl.SubsidiaryLedgerCodeNo equals ssl.SusidiaryLedgerCodeNo into subsidiar
                        from ssl in subsidiar.DefaultIfEmpty()
                        join scl in _subcontrol.All() on ssl.GeneralLedgerCodeNo equals scl.GeneralLedgerCodeNo into subcontrolGroup
                        from scl in subcontrolGroup.DefaultIfEmpty()
                        select new OpeningBalanceVM
                        {
                            autoId = ob.autoId,
                            ComOpeningBalanceCode = ob.ComOpeningBalanceCode,
                            CompanyCode = ob.CompanyCode,
                            BranchCode = ob.BranchCode,
                            SubSusidiaryLedgerCodeNo = ob.SubSusidiaryLedgerCodeNo,
                            OpeningBalance = ob.OpeningBalance,
                            TrType = ob.TrType,
                            LIP = ob.LIP,
                            LMAC = ob.LMAC,
                            LDate = ob.LDate,
                            ModifyDate = ob.ModifyDate,
                            CompanyName = comp != null ? comp.CompanyName : null,
                            BranchName = br != null ? br.BranchName : null,
                            AccountHead = gl != null ? (gl.SubSubsidiaryLedgerName.StartsWith("CUS")
                            ? (customer.CustomerName ?? gl.SubSubsidiaryLedgerName) + " (" + gl.SubSubsidiaryLedgerName + ") (" + (scl.GeneralLedgerName ?? "") + ")"
                            : gl.SubSubsidiaryLedgerName + " (" + (scl.GeneralLedgerName ?? "") + ")")
                        : null
                        };


            if (pageSize == -1)
            {
                pageSize = await query.CountAsync();
                pageNumber = 1;
            }

            // PaginationService
            var paginatedResult = await PaginationService<OpeningBalanceVM, OpeningBalanceVM> .GetPaginatedData(query, pageNumber, pageSize, searchTerm, sortColumn, sortOrder,
                    term => sc =>
                        EF.Functions.Like(sc.CompanyName ?? "", $"%{term}%") ||
                        EF.Functions.Like(sc.BranchName ?? "", $"%{term}%") ||
                        EF.Functions.Like(sc.TrType ?? "", $"%{term}%") ||
                        EF.Functions.Like(sc.AccountHead ?? "", $"%{term}%"),
                    ob => ob 
             );

            return paginatedResult;
        }


        public async Task<bool> IsExistAsync(int id)
        {
            return await _repository.All().AnyAsync(x => x.autoId == id);
        }

        public async Task<bool> SaveAsync(OpeningBalanceVM model)
        {
            try
            {
                string newBalanceCode = await GetLastOpeiningBalanceCodeAsync();

                var entity = new Acc_Company_Opening_Balance
                {
                    ComOpeningBalanceCode = newBalanceCode,
                    CompanyCode = model.CompanyCode,
                    BranchCode = model.BranchCode,
                    SubSusidiaryLedgerCodeNo = model.SubSusidiaryLedgerCodeNo,
                    OpeningBalance = model.OpeningBalance,
                    TrType = model.TrType,
                    LUser = model.LUser,
                    LDate = DateTime.Now,
                    LIP = GetLocalIP() ?? "",
                    LMAC = GetMacAddress() ?? "",
                    Main_CompanyCode = "",
                    UserInfoEmployeeID = ""
                };

                await _repository.AddAsync(entity);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Insertion Failed: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> UpdateAsync(OpeningBalanceVM model)
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
                //string newVoucherCode = await GetLastVoucherTypeCodeAsync();

                //entity.VoucherType_Code = newVoucherCode;
                entity.CompanyCode = model.CompanyCode;
                entity.BranchCode = model.BranchCode;
                entity.SubSusidiaryLedgerCodeNo = model.SubSusidiaryLedgerCodeNo;
                entity.OpeningBalance = model.OpeningBalance;
                entity.TrType = model.TrType;
                entity.LUser = model.LUser;
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


        #region Company & Branch & General Ledger Dropdown

        public async Task<List<CommonChoiceVM>> GetCompanyDropdownInfo()
        {
            var dropdowndata = await _company.All().Select(e => new CommonChoiceVM
            {
                Id = e.CompanyCode,
                Name = e.CompanyName,
            }).ToListAsync();

            return dropdowndata;
        }

        public async Task<List<CommonChoiceVM>> GetBranchDropdownInfo(string companycode)
        {
            var dropdowndata = await _branch.All()
                .Where(b => b.CompanyCode == companycode)
                .Select(e => new CommonChoiceVM
            {
                Id = e.BranchCode,
                Name = e.BranchName,
            }).ToListAsync();

            return dropdowndata;
        }

        //public async Task<List<CommonChoiceVM>> GetGenetalLedegerDropdownInfo()
        //{
        //    var dropdowndata = await _generalLedger.All().Select(e => new CommonChoiceVM
        //    {
        //        Id = e.SubSusidiaryLedgerCodeNo,
        //        Name = e.SubSubsidiaryLedgerName,
        //    }).ToListAsync();

        //    return dropdowndata;
        //}
        public async Task<List<CommonChoiceVM>> GetGenetalLedegerDropdownInfo()
        {
            var dropdowndata = await (from gl in _generalLedger.All()
                                      join ssl in _subsidiary.All()
                                        on gl.SubsidiaryLedgerCodeNo equals ssl.SusidiaryLedgerCodeNo
                                      join scl in _subcontrol.All()
                                        on ssl.GeneralLedgerCodeNo equals scl.GeneralLedgerCodeNo
                                      join customer in _salesCustomer.All()
                                        on gl.SubSubsidiaryLedgerName equals customer.CustomerID into customerforgl
                                     from customer in customerforgl.DefaultIfEmpty()
                                      select new CommonChoiceVM
                                      {
                                          Id = gl.SubSusidiaryLedgerCodeNo,
                                          //Name = $"{gl.SubSubsidiaryLedgerName} ({scl.GeneralLedgerName})"
                                          Name = gl.SubSubsidiaryLedgerName.StartsWith("CUS")
                                                 ? $"{(customer.CustomerName ?? gl.SubSubsidiaryLedgerName)} ({gl.SubSubsidiaryLedgerName}) ({scl.GeneralLedgerName})"
                                                 : $"{gl.SubSubsidiaryLedgerName} ({scl.GeneralLedgerName})"

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
