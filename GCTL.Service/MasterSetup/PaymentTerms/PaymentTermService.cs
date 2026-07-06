using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GCTL.Core.Repository;
using GCTL.Core.ViewModels.MasterSetup.PaymentTerms;
using GCTL.Core.ViewModels.MasterSetup.PaymentType;
using GCTL.Data.Models;
using GCTL.Service.Pagination;
using Microsoft.EntityFrameworkCore;

namespace GCTL.Service.MasterSetup.PaymentTerms
{
    public class PaymentTermService:AppService<Sales_Def_PaymentTerms>,IPaymentTerms
    {
        private IGenericRepository<Sales_Def_PaymentTerms> _repository;
        public PaymentTermService(IGenericRepository<Sales_Def_PaymentTerms> genericRepository) : base(genericRepository)
        {
            _repository = genericRepository;
        }

        public async Task<bool> BulkDeleteAsync(List<decimal> ids)
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

        public async Task<PaymentTermsVM> GetByIdAsync(decimal id)
        {
            var data = await _repository.All().Where(pt => pt.TC == id)
                      .Select(pt => new PaymentTermsVM
                      {

                          PaymentTermsCode = pt.PaymentTermsCode,
                          PaymentTermsName = pt.PaymentTermsName,
                          Percentise = pt.Percentise,
                          Type = pt.Type,
                          CreditDays = pt.CreditDays
                      }).FirstOrDefaultAsync();
            return data;
        }

        public async Task<string> GetLastPaymentTermsAsync()
        {
            var last = await _repository.All()
                  .OrderByDescending(c => c.PaymentTermsCode)
                  .FirstOrDefaultAsync();

            return last?.PaymentTermsCode;
        }

        public async Task<PaginationService<Sales_Def_PaymentTerms, PaymentTermsVM>.PaginationResult<PaymentTermsVM>> GetPaginatedAsync(int pageNumber = 1, int pageSize = 5, string searchTerm = "", string sortColumn = "PaymentTermsCode", string sortOrder = "asc")
        {
            var query = _repository.All();

            if (pageSize == -1)
            {
                pageSize = await query.CountAsync();
                pageNumber = 1;
            }

            var paginatedResult = await PaginationService<Sales_Def_PaymentTerms, PaymentTermsVM>.GetPaginatedData(
                query,
                pageNumber,
                pageSize,
                searchTerm,
                sortColumn,
                sortOrder,
                term => sc =>
                    EF.Functions.Like(sc.PaymentTermsCode ?? "", $"%{term}%") ||
                    EF.Functions.Like(sc.PaymentTermsName ?? "", $"{term}%") ||
                    EF.Functions.Like(sc.Type ?? "", $"%{term}%") ||
                    EF.Functions.Like(sc.Percentise ?? "", $"%{term}%"),
                pt => new PaymentTermsVM
                {
                    TC = pt.TC,
                    PaymentTermsCode = pt.PaymentTermsCode,
                    PaymentTermsName = pt.PaymentTermsName,
                    Type = pt.Type,
                    Percentise = pt.Percentise,
                    CreditDays = pt.CreditDays
                });

            return paginatedResult;
        }

        public async Task<bool> IsDuplicateAsync(PaymentTermsVM model)
        {
            if (model == null)
            {
                throw new ArgumentNullException(nameof(model));
            }

            var paymentMode = await _repository.All().ToListAsync();

            var isDuplicate = paymentMode.Any(pm =>
            string.Equals(pm.PaymentTermsName, model.PaymentTermsName, StringComparison.OrdinalIgnoreCase) &&
             pm.TC != model.TC

            );

            return isDuplicate;
        }

        public async Task<bool> IsExistAsync(decimal id)
        {
            return await _repository.All().AnyAsync(x => x.TC == id);
        }

        public async Task<bool> SaveAsync(PaymentTermsVM model)
        {
            try
            {
                var entity = new Sales_Def_PaymentTerms
                {
                    PaymentTermsCode = model.PaymentTermsCode,
                    PaymentTermsName = model.PaymentTermsName,
                    Type = model.Type ?? "",
                    Percentise = model.Percentise ?? "",
                    CreditDays = model.CreditDays,
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

        public async Task<bool> UpdateAsync(PaymentTermsVM model)
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

                entity.PaymentTermsCode = model.PaymentTermsCode;
                entity.PaymentTermsName = model.PaymentTermsName;
                entity.Type = model.Type ?? "";
                entity.Percentise = model.Percentise ?? "";
                entity.CreditDays = model.CreditDays;

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

        public async Task<List<PaymentTermsVM>>GetAllAsync()
        {
            var entity = await _repository.All().ToListAsync();

            var result = entity.Select(model => new PaymentTermsVM
            {
                PaymentTermsCode = model.PaymentTermsCode,
                PaymentTermsName = model.PaymentTermsName,
                Type = model.Type,
                Percentise = model.Percentise,
                CreditDays = model.CreditDays
            }).ToList();

            return result;
        }
    }
}
