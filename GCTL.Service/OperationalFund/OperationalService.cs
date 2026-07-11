using GCTL.Core.Repository;
using GCTL.Core.ViewModels.OperationalFund;
using GCTL.Data.Models;
using GCTL.Service.ActionLogAudit;
using GCTL.Service.Pagination;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Web.Mvc;

namespace GCTL.Service.OperationalFund
{
    public class OperationalService : AppService<CF_OperationalFundRequisitionEntry>, Ioperational
    {
        #region Service
        private readonly IUserInfoService _userInfoService;
        private readonly IGenericRepository<CF_OperationalFundRequisitionEntry> _repository;
        private readonly IGenericRepository<CF_OperationalFundRequisitionDetails> _details;
        private readonly IGenericRepository<CF_OperationalFundRequisitionDetailsTemp> _Tmpdetails;
        private readonly IGenericRepository<Sep_Documentation> _genericRepository;
        private readonly IGenericRepository<CF_Def_ExpenseHead> _accountsHead;
        private readonly IGenericRepository<Core_ServiceType> _serviceType;
        private readonly IGenericRepository<CF_Def_ExpenseType> _shipmentmode;
        private readonly IGenericRepository<Sales_Customer> _customer;
        private readonly IGenericRepository<CA_Def_Currency> _currencies;

        public OperationalService(IGenericRepository<Sep_Documentation> genericRepository, IUserInfoService userInfoService, IGenericRepository<CF_OperationalFundRequisitionEntry> repository, IGenericRepository<CF_Def_ExpenseHead> accountsHead, IGenericRepository<Core_ServiceType> serviceType, IGenericRepository<CF_Def_ExpenseType> shipmentmode, IGenericRepository<Sales_Customer> customer, IGenericRepository<CA_Def_Currency> currencies, IGenericRepository<CF_OperationalFundRequisitionDetails> details, IGenericRepository<CF_OperationalFundRequisitionDetailsTemp> tmpdetails) : base(repository)
        {
            _userInfoService = userInfoService;
            _repository = repository;
            _genericRepository = genericRepository;
            _accountsHead = accountsHead;
            _serviceType = serviceType;
            _shipmentmode = shipmentmode;
            _customer = customer;
            _currencies = currencies;
            _details = details;
            _Tmpdetails = tmpdetails;
        }

        #endregion


        #region Get All Job Entry List
        public async Task<PaginationService<JobEntryListVM, JobEntryListVM>.PaginationResult<JobEntryListVM>> GetAllJonEntryList(int pageNumber = 1, int pageSize = 10, string searchTerm = "", string sortColumn = "JobNo", string sortOrder = "desc", string customerid = "", string shipmentmodeid = "")
        {
            var query = from d in _genericRepository.All()          // Sep_Documentation
                        join r in _repository.All()                  // CF_OperationalFundRequisitionEntry
                            on d.JobNo equals r.JobNo into rr
                        from r in rr.DefaultIfEmpty()

                            //Customer LEFT JOIN
                        join c in _customer.All()
                            on d.CustomerID equals c.CustomerID into cc
                        from c in cc.DefaultIfEmpty()

                            //Expense Type LEFT JOIN
                        join e in _shipmentmode.All()
                            on d.ExpenseTypeID equals e.ExpenseTypeID into ee
                        from e in ee.DefaultIfEmpty()

                            //MAIN CONDITION
                        where r == null

                        select new JobEntryListVM
                        {
                            JobNo = d.JobNo,
                            JobDate = d.Date,
                            DocReceivedDate = d.DocReceivedDate,

                            CustomerID = d.CustomerID,
                            Customer = c != null ? c.CustomerName : "",

                            ShipmentModeID = e != null ? e.ExpenseTypeID : "",
                            ShipmentMode = e != null ? e.ExpenseType : ""
                        };


            if (!string.IsNullOrEmpty(customerid))
            {
                query = query.Where(x => x.CustomerID == customerid);
            }

            if (!string.IsNullOrEmpty(shipmentmodeid))
            {
                query = query.Where(x => x.ShipmentModeID == shipmentmodeid);
            }
            if (pageSize == -1)
            {
                pageSize = await query.CountAsync();
                pageNumber = 1;
            }

            DateTime? parsedSearchDate = null;

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var dateFormats = new[]
                {
                    "dd/MM/yyyy", "d/M/yyyy", "dd/M/yy",
                    "dd-MM-yyyy", "d-M-yyyy", "dd-M-yy",
                    "yyyy-MM-dd", "yyyy/M/d", "yy/M/dd",
                    "yyyy/MM/dd", "yyyy-M-d", "yy-M-dd"
                };

                if (DateTime.TryParseExact(searchTerm, dateFormats, CultureInfo.InvariantCulture, DateTimeStyles.None, out var exactDate))
                {
                    parsedSearchDate = exactDate.Date;
                }
                else if (DateTime.TryParse(searchTerm, CultureInfo.InvariantCulture, DateTimeStyles.None, out var looseDate))
                {
                    parsedSearchDate = looseDate.Date;
                }
            }

            var paginatedResult = await PaginationService<JobEntryListVM, JobEntryListVM>.GetPaginatedData(
                query, pageNumber, pageSize, searchTerm, sortColumn, sortOrder,
                term => sc =>
                    EF.Functions.Like(sc.JobNo ?? "", $"%{term}%") ||
                    EF.Functions.Like(sc.Customer ?? "", $"%{term}%") ||
                    EF.Functions.Like(sc.ShipmentMode ?? "", $"%{term}%") ||

                    // Exact-date match when the term parsed successfully as a date
                    (parsedSearchDate.HasValue && sc.JobDate.HasValue && sc.JobDate.Value.Date == parsedSearchDate.Value) ||
                    (parsedSearchDate.HasValue && sc.DocReceivedDate.HasValue && sc.DocReceivedDate.Value.Date == parsedSearchDate.Value),

                sc => sc
            );

            return paginatedResult;
        }

        #endregion

        #region Get Requisition Info
        public async Task<RequisitionHeaderVM> GetRequisitionHeaderAsync(string jobno)
        {
            var data = await (from d in _genericRepository.All()
                              join c in _customer.All()
                                  on d.CustomerID equals c.CustomerID into cc
                              from c in cc.DefaultIfEmpty()
                              join e in _shipmentmode.All()
                                  on d.ExpenseTypeID equals e.ExpenseTypeID into ee
                              from e in ee.DefaultIfEmpty()

                              join curr in _currencies.All()
                                on d.CurrencyId equals curr.CurrencyId into currs
                              from curr in currs.DefaultIfEmpty()
                              where d.JobNo == jobno
                              select new RequisitionHeaderVM
                              {
                                  TC = d.TC,
                                  JobNo = d.JobNo,
                                  ShipmentModeID = e.ExpenseTypeID,
                                  CustomerID = c.CustomerID,
                                  ShipmentMode = e != null ? e.ExpenseType : "",
                                  CustomerName = c != null ? c.CustomerName : "",
                                  LcValue = d.LCValue,
                                  InvoiceNo = d.InvoiceNo,
                                  InvoiceValue = d.InvoiceValue,
                                  HAWB = d.HAWB,
                                  MaterialDescription = d.MaterialDescription,
                                  Weight = d.Quntity2,
                                  Qty = d.Quntity1,
                                  CurrencyID = curr != null ? curr.CurrencyId : ""

                              }).FirstOrDefaultAsync();

            return data;
        }

        #endregion

        #region Generate Requisition No
        public async Task<string> GenerateRequisitionNoAsync(string jobNo)
        {
            if (string.IsNullOrEmpty(jobNo))
                return string.Empty;

            // Take only part before first '_'
            var firstPart = jobNo.Split('_').First();
            // firstPart = "CouImp/Dha"

            var prefix = firstPart + "/Ofr";
            // CouImp/Dha/Ofr

            var monthYear = DateTime.Now.ToString("MMyy"); // 1225

            var lastRequisition = await _repository.All()
                .Where(r =>
                    r.OFRNo.StartsWith(prefix) &&
                    r.OFRNo.Contains($"_{monthYear}_"))
                .OrderByDescending(r => r.OFRNo)
                .FirstOrDefaultAsync();

            int serial = 1;

            if (lastRequisition != null)
            {
                var lastSerialStr = lastRequisition.OFRNo.Split('_').Last();
                if (int.TryParse(lastSerialStr, out int lastSerial))
                    serial = lastSerial + 1;
            }

            return $"{prefix}_{monthYear}_{serial:D6}";
        }
        #endregion

        #region Get All Requisition Entry
        public async Task<PaginationService<RequisitionVM, RequisitionVM>.PaginationResult<RequisitionVM>> GetAllRequisition(int pageNumber = 1, int pageSize = 5, string searchTerm = "", string sortColumn = "OFRNo", string sortOrder = "desc")
        {

            //var query = from main in _repository.All()                     // CF_OperationalFundRequisitionEntry
            //            join cust in _customer.All()                        // Sales_Customer
            //                on main.EmployeeID equals cust.EmployeeID
            //            join exp in _shipmentmode.All()                     // CF_Def_ExpenseType
            //                on main.ExpenseTypeID equals exp.ExpenseTypeID
            //            select new RequisitionVM
            //            {
            //                TC = main.TC,
            //                OFRNo = main.OFRNo,
            //                OFRDate = main.OFRDate,
            //                JobNo = main.JobNo,

            //                CustomerName = cust.CustomerName,
            //                BillingAddress = cust.CustomerAddress,

            //                ShipmentMode = exp.ExpenseType
            //            };
            var query = from main in _repository.All()                 // CF_OperationalFundRequisitionEntry
                        join doc in _genericRepository.All()               // Sep_Documentation
                            on main.JobNo equals doc.JobNo
                        join cust in _customer.All()                   // Sales_Customer
                            on doc.CustomerID equals cust.CustomerID
                        join exp in _shipmentmode.All()                // CF_Def_ExpenseType
                            on main.ExpenseTypeID equals exp.ExpenseTypeID
                        select new RequisitionVM
                        {
                            TC = main.TC,
                            OFRNo = main.OFRNo,
                            OFRDate = main.OFRDate,
                            JobNo = main.JobNo,

                            CustomerName = cust.CustomerName,
                            BillingAddress = cust.CustomerAddress,

                            ShipmentMode = exp.ExpenseType
                        };

            if (pageSize == -1)
            {
                pageSize = await query.CountAsync();
                pageNumber = 1;
            }

            DateTime? parsedSearchDate = null;

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var dateFormats = new[]
                {
                    "dd/MM/yyyy", "d/M/yyyy", "dd/M/yy",
                    "dd-MM-yyyy", "d-M-yyyy", "dd-M-yy",
                    "yyyy-MM-dd", "yyyy/M/d", "yy/M/dd",
                    "yyyy/MM/dd", "yyyy-M-d", "yy-M-dd"
                };

                if (DateTime.TryParseExact(searchTerm, dateFormats, CultureInfo.InvariantCulture, DateTimeStyles.None, out var exactDate))
                {
                    parsedSearchDate = exactDate.Date;
                }
                else if (DateTime.TryParse(searchTerm, out var looseDate))
                {
                    parsedSearchDate = looseDate.Date;
                }
            }

            // PaginationService
            var paginatedResult = await PaginationService<RequisitionVM, RequisitionVM>.GetPaginatedData(query, pageNumber, pageSize, searchTerm, sortColumn, sortOrder,
                    term => sc =>
                        EF.Functions.Like(sc.OFRNo ?? "", $"%{term}%") ||
                        EF.Functions.Like(sc.JobNo ?? "", $"%{term}%") ||
                        EF.Functions.Like(sc.CustomerName ?? "", $"%{term}%") ||
                        EF.Functions.Like(sc.BillingAddress ?? "", $"%{term}%") ||
                        (parsedSearchDate.HasValue && sc.OFRDate.HasValue && sc.OFRDate.Value.Date == parsedSearchDate.Value) ||
                        EF.Functions.Like(sc.ShipmentMode ?? "", $"%{term}%"),
                    ob => ob
             );

            return paginatedResult;
        }
        #endregion

        #region Dropdown

        #region Accounts Head Dropdown
        public async Task<IEnumerable<SelectListItem>> GetAllAccountsHead(string serviceTypeId)
        {
            var typelist = await _accountsHead.All().Where(e => e.ServiceTypeID == serviceTypeId).Select(t => new SelectListItem
            {
                Value = t.ExpenseHeadID.ToString(),
                Text = t.ExpenseHead
            }).ToListAsync();

            return typelist;
        }
        #endregion

        #region Ship Mode Dropdown
        public async Task<IEnumerable<SelectListItem>> GetAllShipmentMode()
        {
            var typelist = await _shipmentmode.All().Select(t => new SelectListItem
            {
                Value = t.ExpenseTypeID.ToString(),
                Text = t.ExpenseType
            }).ToListAsync();

            return typelist;
        }
        #endregion

        #region Service Type Dropdown
        public async Task<IEnumerable<SelectListItem>> GetAllServiceType()
        {
            var typelist = await _serviceType.All().Select(t => new SelectListItem
            {
                Value = t.ServiceTypeID.ToString(),
                Text = t.ServiceTypeName,
            }).ToListAsync();

            return typelist;
        }
        #endregion

        #region  Customer Dropdown
        public async Task<IEnumerable<SelectListItem>> GetAllCustomer()
        {
            return await _customer.All()
                .Select(e => new SelectListItem
                {
                    Value = e.CustomerID.ToString(),
                    Text = e.CustomerName
                }).ToListAsync();
        }
        #endregion

        #region  Currencies Dropdown
        public async Task<IEnumerable<SelectListItem>> GetAllCurencies()
        {
            return await _currencies.All()
                .Select(e => new SelectListItem
                {
                    Value = e.CurrencyId.ToString(),
                    Text = e.CurrencyName
                }).ToListAsync();
        }


        #endregion

        #endregion

        #region Generate Ofr Details ID
        private async Task<string> GenerateOfrDetailsId()
        {
            string lastId = await _details.All().OrderByDescending(x => x.OFR_DetailsID).Select(x => x.OFR_DetailsID).MaxAsync();

            if (string.IsNullOrEmpty(lastId))
            {
                return "00000001";
            }

            int nextId = int.Parse(lastId) + 1;
            return nextId.ToString("D8");
        }

        #endregion

        #region TMP Section

        #region Tmp Details Requisition Save and Update
        public async Task<bool> TmpDetailsSaveAsync(RequistionDetailsTmpVM model)
        {
            await _Tmpdetails.BeginTransactionAsync();
            try
            {
                if (string.IsNullOrEmpty(model.ReqNo) || model.RequDate == DateTime.MinValue || string.IsNullOrEmpty(model.ServiceTypeID) || string.IsNullOrEmpty(model.CustomerNameID) || string.IsNullOrEmpty(model.ShipmentModeID))
                {
                    return false;
                }
                string newOffrNO = await GenerateRequisitionNoAsync(model.JobNo);
                string newOFRDetailsID = await GenerateOfrDetailsId();


                var entity = new CF_OperationalFundRequisitionDetailsTemp
                {
                    //OFR_DetailsID = newOFRDetailsID,
                    //OFRNo = newOffrNO,
                    OFR_DetailsID = "",
                    OFRNo = "",
                    JobNo = model.JobNo,
                    ExpenseHeadID = model.AccountHeadID,
                    ServiceTypeID = model.ServiceTypeID,
                    EstimatedAmount = model.Amount,
                    ActualAmount = 0,
                    DifferentAmount = 0,
                    DifferentAmount2 = 0,
                    BillAdjustAmount = 0,
                    LDate = DateTime.Now,
                    LUser = model.CreatedBy.ToString(),
                    LIP = model.LIP,
                    LMAC = model.LMAC,
                    CompanyCode = "",
                    ExpenseHeadRemarks = model.Remark,
                    EmployeeID = model.CustomerNameID
                };

                await _Tmpdetails.AddAsync(entity);
                await _Tmpdetails.CommitTransactionAsync();

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Insertion Failed: {ex.Message}");
                await _Tmpdetails.RollbackTransactionAsync();

                return false;
            }
        }


        //public async Task<bool> UpdateAsync(RequistionDetailsTmpVM model)
        //{
        //    await _Tmpdetails.BeginTransactionAsync();
        //    try
        //    {
        //        if (string.IsNullOrEmpty(model.ReqNo) || model.RequDate == DateTime.MinValue || string.IsNullOrEmpty(model.ServiceTypeID) || string.IsNullOrEmpty(model.CustomerName) || string.IsNullOrEmpty(model.ShipmentMode))
        //        {
        //            return false;
        //        }
        //        var entity = await _repository.GetByIdAsync(model.Tc);
        //        if (entity == null)
        //        {
        //            await _repository.RollbackTransactionAsync();
        //            return false;
        //        }

        //        entity.OFRNo = "";
        //        entity.JobNo = "";
        //        entity.ExpenseTypeID = model.AccountHeadID;
        //        //entity.S = model.ServiceTypeID;

        //        entity.LDate = DateTime.MinValue;
        //        entity.LIP = model.LIP;
        //        entity.LMAC = model.LMAC;
        //        entity.CompanyCode = "";
        //        entity.Ex = model.Remark;
        //        entity.EmployeeID = model.EmployeeID;
        //        entity.LUser = model.UpdatedBy.ToString();

        //        await _Tmpdetails.UpdateAsync(entity);
        //        await _Tmpdetails.CommitTransactionAsync();
        //        return true;
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine(ex.ToString());
        //        await _Tmpdetails.RollbackTransactionAsync();
        //        return false;
        //    }
        //}

        #endregion

        #region Delete Tmp Details with filtering Current User ID and Specific Row from table
        public async Task<bool> DeleteTmpDetailAsync(decimal tc, int? currentUserId)
        {
            await _Tmpdetails.BeginTransactionAsync();

            try
            {
                if (currentUserId == null) return false;

                var entity = await _Tmpdetails.All()
                    .FirstOrDefaultAsync(x => x.TC == tc && x.LUser == currentUserId.ToString());

                if (entity == null) return false;

                await _Tmpdetails.DeleteAsync(entity.TC);
                await _Tmpdetails.CommitTransactionAsync();
                return true;
            }
            catch (Exception ex)
            {
                await _Tmpdetails.RollbackTransactionAsync();
                return false;
            }
        }
        #endregion

        #region When User Click on CLear Button then Current User Related Data Delete from Tmp Details Table
        public async Task<bool> ClearTmp(int? currentUserId)
        {
            await _Tmpdetails.BeginTransactionAsync();

            try
            {
                if (currentUserId == null) return false;

                var entity = await _Tmpdetails.All().Where(x => x.LUser == currentUserId.ToString()).ToListAsync();

                if (entity == null) return false;

                await _Tmpdetails.DeleteRangeAsync(entity);
                await _Tmpdetails.CommitTransactionAsync();
                return true;
            }
            catch (Exception ex)
            {
                await _Tmpdetails.RollbackTransactionAsync();
                return false;
            }
        }
        #endregion

        #region Get All Tmp Details
        public async Task<List<RequistionDetailsTmpVM>> GetAllTmpAsync(int? currentUser)
        {
            var result = await (from tmp in _Tmpdetails.All()
                                join head in _accountsHead.All()
                                    on tmp.ExpenseHeadID equals head.ExpenseHeadID
                                    into headJoin
                                from head in headJoin.DefaultIfEmpty()
                                join serv in _serviceType.All()
                              on tmp.ServiceTypeID equals serv.ServiceTypeID
                              into servicJoin
                                from servic in servicJoin.DefaultIfEmpty()
                                where tmp.LUser == currentUser.ToString()
                                orderby tmp.TC

                                select new RequistionDetailsTmpVM
                                {
                                    Tc = tmp.TC,
                                    SerialNo = head.SerialNo,
                                    AccountHeadName = head.ExpenseHead,
                                    IsReceivetable = head.IsReceiptable,
                                    ServiceTypeName = servic.ServiceTypeName,
                                    Amount = tmp.EstimatedAmount,
                                    Remark = tmp.ExpenseHeadRemarks
                                }).ToListAsync();

            return result;
        }

        #endregion

        #endregion

        #region Operntional Fund Entry Master & Detail Save
        public async Task<bool> MasterDetailsSaveAsync(RequsitionMasterVM model)
        {
            if (string.IsNullOrEmpty(model.ExpenseTypeID) || model.OFFRDate == DateTime.MinValue)
                return false;

            await _repository.BeginTransactionAsync();
            try
            {
                //Check if Master already exists
                var existingMaster = await _repository.All().FirstOrDefaultAsync(x => x.JobNo == model.JobNo);

                string ofrNo;

                if (existingMaster == null)
                {
                    //If Master doesn't exist → create new Master
                    ofrNo = await GenerateRequisitionNoAsync(model.JobNo);
                    var master = new CF_OperationalFundRequisitionEntry
                    {
                        OFRNo = ofrNo,
                        OFRDate = model.OFFRDate,
                        JobNo = model.JobNo,
                        ExpenseTypeID = model.ExpenseTypeID,
                        LUser = model.CreatedBy.ToString(),
                        LIP = model.LIP,
                        LDate = DateTime.Now,
                        LMAC = model.LMAC,
                        PortCharge = 0,
                        AsycodaCharge = 0,
                        DutyByChallanCharge = 0,
                        ShippingCharge = 0,
                        PrifixId = "",
                        CompanyCode = "",
                        EmployeeID = model.CreatedBy.ToString(),
                        CashBank = "",
                        CashBank2 = "",
                        BankAccount = "",
                        BankAccount2 = "",
                    };

                    await _repository.AddAsync(master);
                }
                else
                {
                    // Master exists → don't update anything
                    ofrNo = existingMaster.OFRNo;
                }

                //Update TmpDetails with Amounts from frontend
                if (model.Details != null && model.Details.Any())
                {
                    foreach (var inputDetail in model.Details)
                    {
                        var tmp = await _Tmpdetails.All()
                                    .FirstOrDefaultAsync(d => d.TC == inputDetail.Tc);

                        if (tmp != null)
                        {
                            tmp.EstimatedAmount = inputDetail.Amount ?? 0;
                            await _Tmpdetails.UpdateAsync(tmp);
                        }
                    }
                }

                //Get Tmp Details for this JobNo + User
                var tmpDetails = await _Tmpdetails.All().Where(d => d.LUser == model.CreatedBy.ToString() && d.JobNo == model.JobNo).ToListAsync();

                //Move Tmp Details → Actual Details Table
                foreach (var tmp in tmpDetails)
                {
                    //Check if detail already exists
                    var existingDetail = await _details.All().FirstOrDefaultAsync(d => d.OFRNo == ofrNo && d.JobNo == tmp.JobNo && d.OFR_DetailsID == tmp.OFR_DetailsID);

                    if (existingDetail != null)
                    {
                        //Update existing
                        existingDetail.ExpenseHeadID = tmp.ExpenseHeadID;
                        existingDetail.ServiceTypeID = tmp.ServiceTypeID;
                        existingDetail.EstimatedAmount = tmp.EstimatedAmount ?? 0;
                        existingDetail.ActualAmount = tmp.ActualAmount ?? 0;
                        existingDetail.DifferentAmount = tmp.DifferentAmount ?? 0;
                        existingDetail.BillAdjustAmount = tmp.BillAdjustAmount ?? 0;
                        existingDetail.DifferentAmount2 = tmp.DifferentAmount2 ?? 0;
                        existingDetail.LUser = tmp.LUser;
                        existingDetail.LIP = tmp.LIP;
                        existingDetail.LMAC = tmp.LMAC;
                        existingDetail.LDate = DateTime.Now;
                        existingDetail.EmployeeID = model.CreatedBy.ToString();
                        existingDetail.DetailsBankAccount = "";
                        existingDetail.AdjustDetailsBankAccount = "";
                        existingDetail.AdjustDetailsCashBank = "";
                        existingDetail.AdjustApprovalAmount = 0;
                        existingDetail.AdjustUser = "";
                        existingDetail.DetailsCashBank = "";
                        existingDetail.ExpenseHeadRemarks = tmp.ExpenseHeadRemarks ?? "";

                        await _details.UpdateAsync(existingDetail);
                    }
                    else
                    {
                        //Insert new
                        var newDetail = new CF_OperationalFundRequisitionDetails
                        {
                            OFR_DetailsID = await GenerateOfrDetailsId(),
                            OFRNo = ofrNo,
                            JobNo = tmp.JobNo,
                            ExpenseHeadID = tmp.ExpenseHeadID,
                            ServiceTypeID = tmp.ServiceTypeID,
                            EstimatedAmount = tmp.EstimatedAmount ?? 0,
                            ActualAmount = tmp.ActualAmount ?? 0,
                            DifferentAmount = tmp.DifferentAmount ?? 0,
                            BillAdjustAmount = tmp.BillAdjustAmount ?? 0,
                            DifferentAmount2 = tmp.DifferentAmount2 ?? 0,
                            LUser = tmp.LUser,
                            LIP = tmp.LIP,
                            LMAC = tmp.LMAC,
                            LDate = DateTime.Now,
                            CompanyCode = "",
                            EmployeeID = model.CreatedBy.ToString(),
                            DetailsBankAccount = "",
                            AdjustDetailsBankAccount = "",
                            AdjustDetailsCashBank = "",
                            AdjustApprovalAmount = 0,
                            AdjustUser = "",
                            DetailsCashBank = "",
                            ExpenseHeadRemarks = tmp.ExpenseHeadRemarks ?? "",
                        };

                        await _details.AddAsync(newDetail);
                    }
                }

                // Find real detail rows for this OFR that no longer exist in tmp
                var currentDetailIds = tmpDetails.Select(t => t.OFR_DetailsID).ToList(); // or your matching key
                var detailsToRemove = await _details.All()
                    .Where(d => d.OFRNo == ofrNo && !currentDetailIds.Contains(d.OFR_DetailsID))
                    .ToListAsync();

                if (detailsToRemove.Any())
                    await _details.DeleteRangeAsync(detailsToRemove);

                //Delete Tmp Details
                await _Tmpdetails.DeleteRangeAsync(tmpDetails);
                await _repository.CommitTransactionAsync();
                return true;
            }
            catch (Exception ex)
            {
                await _repository.RollbackTransactionAsync();
                Console.WriteLine($"Insertion Failed: {ex.Message}");
                return false;
            }
        }

        #endregion

        #region Requisition Entry Delete
        public async Task<bool> BulkDeleteAsync(List<string> ofrNos)
        {
            await _repository.BeginTransactionAsync();

            try
            {
                var masters = await _repository.All().Where(m => ofrNos.Contains(m.OFRNo)).ToListAsync();

                if (!masters.Any())
                {
                    await _repository.RollbackTransactionAsync();
                    return false;
                }
                var details = await _details.All().Where(d => ofrNos.Contains(d.OFRNo)).ToListAsync();

                if (details.Any())
                {
                    await _details.DeleteRangeAsync(details);
                }

                await _repository.DeleteRangeAsync(masters);

                await _repository.CommitTransactionAsync();
                return true;
            }
            catch (Exception ex)
            {
                await _repository.RollbackTransactionAsync();
                Console.WriteLine($"Bulk delete error: {ex}");
                return false;
            }
        }
        #endregion

        #region When click Edit Button

        #region Getting Master Data
        public async Task<RequisitionHeaderVM> GetRequisitionDetails(string offrNo)
        {
            var data = await (from req in _repository.All()
                              join det in _details.All()
                               on req.OFRNo equals det.OFRNo into detGroup
                              from det in detGroup.DefaultIfEmpty()  // left join with details

                              join doc in _genericRepository.All()
                                on req.JobNo equals doc.JobNo into docGroup
                              from doc in docGroup.DefaultIfEmpty()  // left join with documentation

                              join st in _serviceType.All()
                                on det.ServiceTypeID equals st.ServiceTypeID into stGroup
                              from st in stGroup.DefaultIfEmpty()   // ServiceType lookup

                              join eh in _accountsHead.All()
                                on det.ExpenseHeadID equals eh.ExpenseHeadID into ehGroup
                              from eh in ehGroup.DefaultIfEmpty()   // ExpenseHead lookup


                              join cust in _customer.All()
                                on doc.CustomerID equals cust.CustomerID into custGroup
                              from cust in custGroup.DefaultIfEmpty()   // Customer

                              join ship in _shipmentmode.All()
                                on req.ExpenseTypeID equals ship.ExpenseTypeID into shipGroup
                              from ship in shipGroup.DefaultIfEmpty() //ShipmentMode

                              where req.OFRNo == offrNo
                              select new RequisitionHeaderVM
                              {
                                  // Master fields
                                  JobNo = req.JobNo,
                                  ShipmentModeID = req.ExpenseTypeID,
                                  //CustomerID = req.EmployeeID,
                                  ReqDate = req.OFRDate,
                                  ReqNo = req.OFRNo,
                                  Amount = det.EstimatedAmount,
                                  Remark = det.ExpenseHeadRemarks,
                                  CustomerName = cust.CustomerName,
                                  CustomerID = cust.CustomerID,
                                  ShipmentMode = ship.ExpenseType,
                                  ServiceTypeID = st != null ? st.ServiceTypeID : null,
                                  AccountHeadID = eh != null ? eh.ExpenseHeadID : null,

                                  // Documentation fields
                                  InvoiceNo = doc != null ? doc.InvoiceNo : null,
                                  LcValue = doc != null ? doc.LCValue : null,
                                  InvoiceValue = doc != null ? doc.InvoiceValue : null,
                                  HAWB = doc != null ? doc.HAWB : null,
                                  MaterialDescription = doc != null ? doc.MaterialDescription : null,
                                  Qty = doc != null ? doc.Quntity1 : null,
                                  Weight = doc != null ? doc.Quntity2 : null,
                                  CurrencyID = doc != null ? doc.CurrencyId : null
                              }).FirstOrDefaultAsync();

            return data;
        }
        #endregion

        #region Copied To Tmp Details Table
        public async Task<bool> CopyDetailsToTmp(string offrNo, int? currentUserId)
        {
            var details = await _details.All().Where(d => d.OFRNo == offrNo).ToListAsync();

            if (!details.Any()) return false;

            var existingTmp = await _Tmpdetails.All()
                .Where(x => x.LUser == currentUserId.ToString())
                .ToListAsync();

            if (existingTmp.Any())
                await _Tmpdetails.DeleteRangeAsync(existingTmp);

            // Map to temporary table (use the Temp model)
            var tmpDetails = details.Select(d => new CF_OperationalFundRequisitionDetailsTemp
            {
                OFR_DetailsID = d.OFR_DetailsID,
                OFRNo = d.OFRNo,
                JobNo = d.JobNo,
                ExpenseHeadID = d.ExpenseHeadID,
                ServiceTypeID = d.ServiceTypeID,
                EstimatedAmount = d.EstimatedAmount,
                ActualAmount = d.ActualAmount,
                DifferentAmount = d.DifferentAmount,
                DifferentAmount2 = d.DifferentAmount2,
                BillAdjustAmount = d.BillAdjustAmount,
                LUser = currentUserId.ToString(),
                LDate = d.LDate,
                LMAC = d.LMAC,
                CompanyCode = d.CompanyCode,
                EmployeeID = d.EmployeeID,
                ExpenseHeadRemarks = d.ExpenseHeadRemarks
            }).ToList();

            await _Tmpdetails.AddRangeAsync(tmpDetails);
            return true;
        }
        #endregion

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

