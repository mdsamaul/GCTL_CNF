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
using GCTL.Core.ViewModels.MasterSetup.PaymentType;
using GCTL.Data.Models;
using Microsoft.EntityFrameworkCore;
using GCTL.Service.Pagination;

namespace GCTL.Service.MasterSetup.PaymentType
{
    public class PaymentTypeService : AppService<Sales_Def_PaymentType>, IPaymentType
    {
        private readonly IGenericRepository<Sales_Def_PaymentType> _repository;
        public PaymentTypeService(IGenericRepository<Sales_Def_PaymentType> genericRepository) : base(genericRepository)
        {
               _repository = genericRepository;
        }

        public async Task<bool> BulkDeleteAsync(List<int> ids)
        {
            await _repository.BeginTransactionAsync();
            try
            {
                var entity = await _repository.All().Where(c => ids.Contains(c.TC)).ToListAsync();

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

        public async Task<PaymentTypeVM> GetByIdAsync(int id)
        {
            var data = await _repository.All().Where(pt => pt.TC == id)
                        .Select(pt => new PaymentTypeVM
                        {

                            PaymentTypeID = pt.PaymentTypeID,
                            PaymentType = pt.PaymentType,
                            ShortName = pt.ShortName,
                            LMAC = pt.LMAC,
                            LDate = pt.LDate,
                            ModifyDate = pt.ModifyDate,
                        }).FirstOrDefaultAsync();
            return data;
        }

        public async Task<string> GetLastPaymentTypeAsync()
        {
            var last = await _repository.All()
                 .OrderByDescending(c => c.PaymentTypeID)
                 .FirstOrDefaultAsync();

            return last?.PaymentTypeID;
        }

        public async Task<PaginationService<Sales_Def_PaymentType, PaymentTypeVM>.PaginationResult<PaymentTypeVM>> GetPaginatedAsync(int pageNumber = 1, int pageSize = 5, string searchTerm = "", string sortColumn = "PaymentTypeID", string sortOrder = "asc")
        {
            var query = _repository.All();

            if (pageSize == -1)
            {
                pageSize = await query.CountAsync();
                pageNumber = 1;
            }

            var paginatedResult = await PaginationService<Sales_Def_PaymentType, PaymentTypeVM>.GetPaginatedData(
                query,
                pageNumber,
                pageSize,
                searchTerm,
                sortColumn,
                sortOrder,
                term => sc =>
                    EF.Functions.Like(sc.PaymentTypeID ?? "", $"%{term}%") ||
                    EF.Functions.Like(sc.PaymentType ?? "", $"{term}%") ||
                    EF.Functions.Like(sc.ShortName ?? "", $"%{term}%"),
                pt => new PaymentTypeVM
                {
                    TC = pt.TC,
                    PaymentTypeID = pt.PaymentTypeID,
                    PaymentType = pt.PaymentType,
                    ShortName = pt.ShortName,
                    LIP = pt.LIP ?? "",
                    LMAC = pt.LMAC ?? "",
                    LDate = pt.LDate,
                    ModifyDate = pt.ModifyDate,
                });

            return paginatedResult;
        }

        public async Task<bool> IsDuplicateAsync(PaymentTypeVM model)
        {
            if (model == null)
            {
                throw new ArgumentNullException(nameof(model));
            }

            var paymentMode = await _repository.All().ToListAsync();

            var isDuplicate = paymentMode.Any(pm =>
            string.Equals(pm.PaymentType, model.PaymentType, StringComparison.OrdinalIgnoreCase) &&
             pm.TC != model.TC

            );

            return isDuplicate;
        }

        public async Task<bool> IsExistAsync(int id)
        {
            return await _repository.All().AnyAsync(x => x.TC == id);
        }

        public async Task<bool> SaveAsync(PaymentTypeVM model)
        {
            try
            {
                var entity = new Sales_Def_PaymentType
                {
                    PaymentTypeID = model.PaymentTypeID,
                    PaymentType = model.PaymentType,
                    ShortName = model.ShortName ?? "",
                    LUser = "", //If needed
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

        public async Task<bool> UpdateAsync(PaymentTypeVM model)
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

                entity.PaymentTypeID = model.PaymentTypeID;
                entity.PaymentType = model.PaymentType;
                entity.ShortName = model.ShortName ?? "";
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

        public async Task<List<PaymentTypeVM>>GetAllAsync()
        {
            var entity = await _repository.All().ToListAsync();

            var result = entity.Select(model => new PaymentTypeVM
            {
                PaymentTypeID = model.PaymentTypeID,
                PaymentType = model.PaymentType,
                ShortName = model.ShortName
            }).ToList();

            return result;
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
