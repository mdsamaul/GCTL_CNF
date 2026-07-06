using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using GCTL.Core.Repository;
using GCTL.Core.ViewModels.MasterSetup.PaymentMode;
using GCTL.Data.Models;
using Microsoft.EntityFrameworkCore;
using GCTL.Core.ViewModels.MasterSetup.VendorPrefix;
using GCTL.Service.Pagination;

namespace GCTL.Service.MasterSetup.PaymentMode
{
    public class PaymentModeService:AppService<Sales_Def_PaymentMode>,IPaymentMode
    {
        private readonly IGenericRepository<Sales_Def_PaymentMode> _repository;
        public PaymentModeService(IGenericRepository<Sales_Def_PaymentMode> genericRepository) : base(genericRepository)
        {
            _repository = genericRepository;
        }

        public async Task<bool>BulkDeleteAsync(List<int> ids)
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

        public async Task<bool>DeleteAsync(int id)
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

                await _repository.DeleteAsync(entity.PaymentModeID);

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

        public async Task<List<PaymentModeVM>> GetAllAsync()
        {
            var entity = await _repository.All().ToListAsync();

            var result = entity.Select(model => new PaymentModeVM
            {
                PaymentModeID = model.PaymentModeID,
                PaymentModeName = model.PaymentModeName,
                PaymentModeShortName = model.PaymentModeShortName

            }).ToList();

            return result;
        }

        public async Task<PaymentModeVM> GetByIdAsync(int id)
        {
            var data = await _repository.All().Where(pt => pt.AutoId == id)
                        .Select(pt => new PaymentModeVM
                        {
                            
                            PaymentModeID = pt.PaymentModeID,
                            PaymentModeName = pt.PaymentModeName,
                            PaymentModeShortName = pt.PaymentModeShortName,
                            LMAC = pt.LMAC,
                            LDate = pt.LDate,
                            ModifyDate = pt.ModifyDate,
                        }).FirstOrDefaultAsync();
            return data;
        }

        public async Task<string> GetLastPaymentModeAsync()
        {
            var last = await _repository.All()
                    .OrderByDescending(c => c.PaymentModeID)
                    .FirstOrDefaultAsync();

            return last?.PaymentModeID;
        }

        public async Task<PaginationService<Sales_Def_PaymentMode, PaymentModeVM>.PaginationResult<PaymentModeVM>>GetPaginatedAsync(int pageNumber = 1, int pageSize = 10, string searchTerm = "", string sortColumn = "PaymentModeID", string sortOrder = "desc")
        {
            var query = _repository.All();

            if (pageSize == -1)
            {
                pageSize = await query.CountAsync();
                pageNumber = 1;
            }

            var paginatedResult = await PaginationService<Sales_Def_PaymentMode, PaymentModeVM>.GetPaginatedData(
                query,
                pageNumber,
                pageSize,
                searchTerm,
                sortColumn,
                sortOrder,
                term => sc =>
                    EF.Functions.Like(sc.PaymentModeID ?? "", $"%{term}%") ||
                    EF.Functions.Like(sc.PaymentModeName ?? "", $"{term}%") ||
                    EF.Functions.Like(sc.PaymentModeShortName ?? "", $"%{term}%"),
                pt => new PaymentModeVM
                {
                    AutoId = pt.AutoId,
                    PaymentModeID = pt.PaymentModeID,
                    PaymentModeName = pt.PaymentModeName ?? "",
                    PaymentModeShortName = pt.PaymentModeShortName ?? "",
                    LIP = pt.LIP ?? "",
                    LMAC = pt.LMAC ?? "",
                    LDate = pt.LDate,
                    ModifyDate = pt.ModifyDate,
                });

            return paginatedResult;
        }

        public async Task<bool>IsDuplicateAsync(PaymentModeVM model)
        {
            if (model == null)
            {
                throw new ArgumentNullException(nameof(model));
            }

            var paymentMode = await _repository.All().ToListAsync();

            var isDuplicate = paymentMode.Any(pm =>
            string.Equals(pm.PaymentModeName, model.PaymentModeName, StringComparison.OrdinalIgnoreCase) &&
             pm.AutoId != model.AutoId

            );

            return isDuplicate;
        }

        public async Task<bool> IsExistAsync(int id)
        {
            return await _repository.All().AnyAsync(x => x.AutoId == id);
        }

        public async Task<bool>SaveAsync(PaymentModeVM model)
        {
            try
            {
                var entity = new Sales_Def_PaymentMode
                {
                    PaymentModeID = model.PaymentModeID,
                    PaymentModeName = model.PaymentModeName,
                    PaymentModeShortName = model.PaymentModeShortName ?? "",
                    LUser = "", //If needed
                    LDate = DateTime.Now,
                    LIP = GetLocalIP() ?? "",
                    LMAC = GetMacAddress() ?? "",
                    CompanyCode = model.CompanyCode ?? "",
                    EmployeeID = model.EmployeeID ?? "",
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

        public async Task<bool>UpdateAsync(PaymentModeVM model)
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

                entity.PaymentModeID = model.PaymentModeID;
                entity.PaymentModeName = model.PaymentModeName;
                entity.PaymentModeShortName = model.PaymentModeShortName ?? "";
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
