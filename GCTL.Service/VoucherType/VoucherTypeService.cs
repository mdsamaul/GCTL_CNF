using GCTL.Core.Repository;
using GCTL.Core.ViewModels;
using GCTL.Core.ViewModels.VoucherType;
using GCTL.Data.Models;
using GCTL.Service.Pagination;
using Microsoft.EntityFrameworkCore;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;

namespace GCTL.Service.VoucherType
{
    public class VoucherTypeService : AppService<Acc_VoucherType>, IVoucherType
    {
        private readonly IGenericRepository<Acc_VoucherType> _repository;
        private readonly IGenericRepository<Acc_Duration_Type> _durationType;
        public VoucherTypeService(IGenericRepository<Acc_VoucherType> genericRepository, IGenericRepository<Acc_Duration_Type> durationType) : base(genericRepository)
        {
            _repository = genericRepository;
            _durationType = durationType;
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

        public async Task<List<VoucherTypeVM>> GetAllAsync()
        {
            var entity = await _repository.All().ToListAsync();

            var result = entity.Select(model => new VoucherTypeVM
            {
                VoucherType_Code = model.VoucherType_Code,
                Voucher_TypeName = model.Voucher_TypeName,
                Description = model.Description,
                numberingMethod = model.numberingMethod,
                startingNumber = model.startingNumber,
                resetDuration = model.resetDuration,
                resetDurationType_Code = model.resetDurationType_Code,
                prefix = model.prefix,
                suffix = model.suffix,
                padding = model.padding
            }).ToList();

            return result;
        }

        public async Task<VoucherTypeVM> GetByIdAsync(int id)
        {
            var data = await _repository.All().Where(pt => pt.autoId == id)
                        .Select(pt => new VoucherTypeVM
                        {

                            VoucherType_Code = pt.VoucherType_Code,
                            Voucher_TypeName = pt.Voucher_TypeName,
                            Description = pt.Description,
                            numberingMethod = pt.numberingMethod,
                            startingNumber = pt.startingNumber,
                            resetDuration = pt.resetDuration,
                            resetDurationType_Code = pt.resetDurationType_Code,
                            prefix = pt.prefix,
                            suffix = pt.suffix,
                            padding = pt.padding,
                            LDate = pt.LDate,
                            ModifyDate = pt.ModifyDate,
                        }).FirstOrDefaultAsync();
            return data;
        }


        public async Task<string> GetLastVoucherTypeCodeAsync()
        {
            var allCodes = await _repository.All()
                                 .Select(c => c.VoucherType_Code)
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

        //public async Task<PaginationService<Acc_VoucherType, VoucherTypeVM>.PaginationResult<VoucherTypeVM>> GetPaginatedAsync(int pageNumber = 1, int pageSize = 5, string searchTerm = "", string sortColumn = "VoucherType_Code", string sortOrder = "desc")
        //{
        //    var query = _repository.All();

        //    // Apply search first (can still be translated to SQL)
        //    if (!string.IsNullOrEmpty(searchTerm))
        //    {
        //        searchTerm = searchTerm.Trim().ToLower();
        //        query = query.Where(sc =>
        //            EF.Functions.Like(sc.VoucherType_Code ?? "", $"%{searchTerm}%") ||
        //            EF.Functions.Like(sc.Voucher_TypeName ?? "", $"{searchTerm}%") ||
        //            EF.Functions.Like(sc.prefix ?? "", $"{searchTerm}%") ||
        //            EF.Functions.Like(sc.Description ?? "", $"%{searchTerm}%"));
        //    }

        //    // Force client-side for numeric string sorting
        //    var list = query.AsEnumerable(); // Data fetched into memory

        //    list = sortColumn == "VoucherType_Code"
        //        ? (sortOrder.ToLower() == "desc"
        //            ? list.OrderByDescending(x => int.Parse(x.VoucherType_Code))
        //            : list.OrderBy(x => int.Parse(x.VoucherType_Code)))
        //        : (sortOrder.ToLower() == "desc"
        //            ? list.OrderByDescending(x => EF.Property<object>(x, sortColumn))
        //            : list.OrderBy(x => EF.Property<object>(x, sortColumn)));

        //    // Pagination
        //    if (pageSize == -1)
        //    {
        //        pageSize = list.Count();
        //        pageNumber = 1;
        //    }

        //    var paginatedData = list
        //        .Skip((pageNumber - 1) * pageSize)
        //        .Take(pageSize)
        //        .Select(pt => new VoucherTypeVM
        //        {
        //            autoId = pt.autoId,
        //            VoucherType_Code = pt.VoucherType_Code,
        //            Voucher_TypeName = pt.Voucher_TypeName,
        //            Description = pt.Description,
        //            numberingMethod = pt.numberingMethod,
        //            startingNumber = pt.startingNumber,
        //            resetDuration = pt.resetDuration,
        //            resetDurationType_Code = pt.resetDurationType_Code,
        //            prefix = pt.prefix,
        //            suffix = pt.suffix,
        //            padding = pt.padding,
        //            LIP = pt.LIP,
        //            LMAC = pt.LMAC,
        //            LDate = pt.LDate,
        //            ModifyDate = pt.ModifyDate,
        //        }).ToList();

        //    // Pagination Info
        //    var totalItems = list.Count();
        //    var totalPages = (int)Math.Ceiling((double)totalItems / pageSize);
        //    var startItem = totalItems == 0 ? 0 : (pageNumber - 1) * pageSize + 1;
        //    var endItem = Math.Min(pageNumber * pageSize, totalItems);
        //    var pageNumbers = Enumerable.Range(1, totalPages).ToList();

        //    return new PaginationService<Acc_VoucherType, VoucherTypeVM>.PaginationResult<VoucherTypeVM>
        //    {
        //        Data = paginatedData,
        //        TotalCount = totalItems,
        //        PaginationInfo = new PaginationService<Acc_VoucherType, VoucherTypeVM>.PaginationInfo
        //        {
        //            StartItem = startItem,
        //            EndItem = endItem,
        //            TotalItems = totalItems,
        //            PageNumbers = pageNumbers,
        //            TotalPages = totalPages,
        //            CurrentPage = pageNumber
        //        }
        //    };
        //}

        public async Task<PaginationService<Acc_VoucherType, VoucherTypeVM>.PaginationResult<VoucherTypeVM>> GetPaginatedAsync(int pageNumber = 1, int pageSize = 5, string searchTerm = "", string sortColumn = "VoucherType_Code", string sortOrder = "desc")
        {
            // Base query
            var query = from v in _repository.All()
                        join d in _durationType.All() // Acc_Duration_Type repository
                            on v.resetDurationType_Code equals d.Duration_TypeCode into vd
                        from duration in vd.DefaultIfEmpty() // Left join
                        select new
                        {
                            Voucher = v,
                            DurationName = duration.durationType
                        };

            // Apply search
            if (!string.IsNullOrEmpty(searchTerm))
            {
                searchTerm = searchTerm.Trim().ToLower();
                query = query.Where(x =>
                    EF.Functions.Like(x.Voucher.VoucherType_Code ?? "", $"%{searchTerm}%") ||
                    EF.Functions.Like(x.Voucher.Voucher_TypeName ?? "", $"{searchTerm}%") ||
                    EF.Functions.Like(x.Voucher.prefix ?? "", $"{searchTerm}%") ||
                    EF.Functions.Like(x.Voucher.Description ?? "", $"%{searchTerm}%") ||
                    EF.Functions.Like(x.DurationName ?? "", $"%{searchTerm}%")
                );
            }

            // Fetch data into memory for numeric string sort
            var list = await query.AsNoTracking().ToListAsync();

            // Sorting
            list = sortColumn switch
            {
                "VoucherType_Code" => sortOrder.ToLower() == "desc"
                    ? list.OrderByDescending(x => int.Parse(x.Voucher.VoucherType_Code)).ToList()
                    : list.OrderBy(x => int.Parse(x.Voucher.VoucherType_Code)).ToList(),

                _ => sortOrder.ToLower() == "desc"
                    ? list.OrderByDescending(x => EF.Property<object>(x.Voucher, sortColumn)).ToList()
                    : list.OrderBy(x => EF.Property<object>(x.Voucher, sortColumn)).ToList()
            };

            // Pagination
            if (pageSize == -1)
            {
                pageSize = list.Count();
                pageNumber = 1;
            }

            var paginatedData = list
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new VoucherTypeVM
                {
                    autoId = x.Voucher.autoId,
                    VoucherType_Code = x.Voucher.VoucherType_Code,
                    Voucher_TypeName = x.Voucher.Voucher_TypeName,
                    Description = x.Voucher.Description,
                    numberingMethod = x.Voucher.numberingMethod,
                    startingNumber = x.Voucher.startingNumber,
                    resetDuration = x.Voucher.resetDuration,
                    resetDurationType_Code = x.Voucher.resetDurationType_Code,
                    DurationTypeName = x.DurationName,
                    prefix = x.Voucher.prefix,
                    suffix = x.Voucher.suffix,
                    padding = x.Voucher.padding,
                    LIP = x.Voucher.LIP,
                    LMAC = x.Voucher.LMAC,
                    LDate = x.Voucher.LDate,
                    ModifyDate = x.Voucher.ModifyDate
                }).ToList();

            // Pagination Info
            var totalItems = list.Count();
            var totalPages = (int)Math.Ceiling((double)totalItems / pageSize);
            var startItem = totalItems == 0 ? 0 : (pageNumber - 1) * pageSize + 1;
            var endItem = Math.Min(pageNumber * pageSize, totalItems);
            var pageNumbers = Enumerable.Range(1, totalPages).ToList();

            return new PaginationService<Acc_VoucherType, VoucherTypeVM>.PaginationResult<VoucherTypeVM>
            {
                Data = paginatedData,
                TotalCount = totalItems,
                PaginationInfo = new PaginationService<Acc_VoucherType, VoucherTypeVM>.PaginationInfo
                {
                    StartItem = startItem,
                    EndItem = endItem,
                    TotalItems = totalItems,
                    PageNumbers = pageNumbers,
                    TotalPages = totalPages,
                    CurrentPage = pageNumber
                }
            };
        }

        public async Task<bool> IsDuplicateAsync(VoucherTypeVM model)
        {
            if (model == null)
            {
                throw new ArgumentNullException(nameof(model));
            }

            var alldata = await _repository.All().ToListAsync();

            var isDuplicate = alldata.Any(pm =>
            string.Equals(pm.Voucher_TypeName, model.Voucher_TypeName, StringComparison.OrdinalIgnoreCase) &&
             pm.autoId != model.autoId

            );

            return isDuplicate;
        }

        public async Task<bool> IsExistAsync(int id)
        {
            return await _repository.All().AnyAsync(x => x.autoId == id);
        }

        public async Task<bool> SaveAsync(VoucherTypeVM model)
        {
            try
            {
                string newVoucherCode = await GetLastVoucherTypeCodeAsync();

                var entity = new Acc_VoucherType
                {
                    VoucherType_Code = newVoucherCode,
                    Voucher_TypeName = model.Voucher_TypeName,
                    Description = model.Description,
                    numberingMethod = model.numberingMethod,
                    startingNumber = model.startingNumber ?? 0,
                    resetDuration = model.resetDuration,
                    resetDurationType_Code = model.resetDurationType_Code,
                    prefix = model.prefix,
                    suffix = model.suffix,
                    padding = model.padding,
                    LUser = model.LUser,
                    LDate = DateTime.Now,
                    LIP = GetLocalIP() ?? "",
                    LMAC = GetMacAddress() ?? ""
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

        public async Task<bool> UpdateAsync(VoucherTypeVM model)
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
                entity.Voucher_TypeName = model.Voucher_TypeName;
                entity.Description = model.Description;
                entity.numberingMethod = model.numberingMethod;
                entity.startingNumber = model.startingNumber ?? 0;
                entity.resetDuration = model.resetDuration;
                entity.resetDurationType_Code = model.resetDurationType_Code;
                entity.prefix = model.prefix;
                entity.suffix = model.suffix;
                entity.padding = model.padding;
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


        #region Reset Duration Type Dropdown

        public async Task<List<CommonChoiceVM>> DurationTypeDropdown()
        {
            var dropdowndata = await _durationType.All().Select(e => new CommonChoiceVM
            {
                Id = e.Duration_TypeCode,
                Name = e.durationType,
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
