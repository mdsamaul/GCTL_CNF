using GCTL.Core.Repository;
using GCTL.Core.ViewModels;
using GCTL.Core.ViewModels.OFRSection.OFRAdjustApproval;
using GCTL.Data.Models;
using GCTL.Service.ActionLogAudit;
using GCTL.Service.Pagination;
using Microsoft.EntityFrameworkCore;

namespace GCTL.Service.OFRAdjustApproval
{
    public class OFRAdjustApprovalService : AppService<CF_OperationalFundRequisitionEntry>, IOFRAdjustApproval
    {
        #region Service
        private readonly IUserInfoService _userInfoService;
        private readonly IGenericRepository<CF_OperationalFundRequisitionEntry> _repository;
        private readonly IGenericRepository<CF_OperationalFundRequisitionDetails> _details;
        private readonly IGenericRepository<CF_OperationalFundRequisitionDetailsTemp> _Tmpdetails;
        private readonly IGenericRepository<CF_Def_ExpenseType> _shipmentmode;
        private readonly IGenericRepository<Sales_Customer> _customer;
        private readonly IGenericRepository<Sep_Documentation> _documentation;
        private readonly IGenericRepository<CF_Def_ExpenseHead> _accountsHead;
        private readonly IGenericRepository<Core_ServiceType> _serviceType;
        private readonly IGenericRepository<Acc_SubSubsidiaryLedger> _generalLedger;
        private readonly IGenericRepository<Sep_Documentation> _sepDocumentation;


        public OFRAdjustApprovalService(IGenericRepository<CF_OperationalFundRequisitionEntry> genericRepository, IUserInfoService userInfoService, IGenericRepository<CF_OperationalFundRequisitionEntry> repository, IGenericRepository<CF_Def_ExpenseType> shipmentmode, IGenericRepository<Sales_Customer> customer, IGenericRepository<Sep_Documentation> documentation, IGenericRepository<CF_OperationalFundRequisitionDetails> details, IGenericRepository<CF_Def_ExpenseHead> accountsHead, IGenericRepository<Core_ServiceType> serviceType, IGenericRepository<CF_OperationalFundRequisitionDetailsTemp> tmpdetails, IGenericRepository<Acc_SubSubsidiaryLedger> generalLedger, IGenericRepository<Sep_Documentation> sepDocumentation) : base(genericRepository)
        {
            _userInfoService = userInfoService;
            _repository = repository;
            _shipmentmode = shipmentmode;
            _customer = customer;
            _documentation = documentation;
            _details = details;
            _accountsHead = accountsHead;
            _serviceType = serviceType;
            _Tmpdetails = tmpdetails;
            _generalLedger = generalLedger;
            _sepDocumentation = sepDocumentation;
        }
        #endregion

        #region Grid Section

        #region Get All Requisition for Bottom Grid
        public async Task<PaginationService<OFRAdjustApprovalBottomGridVM, OFRAdjustApprovalBottomGridVM>.PaginationResult<OFRAdjustApprovalBottomGridVM>> AllRequisitionforBottomGrid(int pageNumber = 1, int pageSize = 5, string searchTerm = "", string sortColumn = "JobNo", string sortOrder = "desc")
        {
            var query = from main in _repository.All()
                        join doc in _documentation.All()
                            on main.JobNo equals doc.JobNo
                        join cust in _customer.All()
                            on doc.CustomerID equals cust.CustomerID
                        join exp in _shipmentmode.All()
                            on main.ExpenseTypeID equals exp.ExpenseTypeID
                        select new OFRAdjustApprovalBottomGridVM
                        {
                            TC = main.TC,
                            JobNo = main.JobNo,
                            CustomerID = cust.CustomerID,
                            CustomerName = cust.CustomerName,
                            ShipmentModeID = exp.ExpenseTypeID,
                            ShipmentMode = exp.ExpenseType
                        };

            if (pageSize == -1)
            {
                pageSize = await query.CountAsync();
                pageNumber = 1;
            }

            // PaginationService
            var paginatedResult = await PaginationService<OFRAdjustApprovalBottomGridVM, OFRAdjustApprovalBottomGridVM>.GetPaginatedData(query, pageNumber, pageSize, searchTerm, sortColumn, sortOrder,
                    term => sc =>
                        EF.Functions.Like(sc.JobNo ?? "", $"%{term}%") ||
                        EF.Functions.Like(sc.CustomerName ?? "", $"%{term}%") ||
                        EF.Functions.Like(sc.ShipmentMode ?? "", $"%{term}%"),
                    ob => ob
             );

            return paginatedResult;
        }
        #endregion

        #region Get All Requisition Entry in Top Grid
        public async Task<PaginationService<OFRAdjustApprovalTopGridVM, OFRAdjustApprovalTopGridVM>.PaginationResult<OFRAdjustApprovalTopGridVM>> GetAllRequisition(int pageNumber = 1, int pageSize = 5, string searchTerm = "", string sortColumn = "JobNo", string sortOrder = "desc", string customerid = "", string shipmentmodeid = "")
        {
            var query = from main in _repository.All()
                        join doc in _documentation.All()
                            on main.JobNo equals doc.JobNo
                        join cust in _customer.All()
                            on doc.CustomerID equals cust.CustomerID
                        join exp in _shipmentmode.All()
                            on main.ExpenseTypeID equals exp.ExpenseTypeID
                        select new OFRAdjustApprovalTopGridVM
                        {
                            TC = main.TC,
                            JobNo = main.JobNo,
                            CustomerID = cust.CustomerID,
                            CustomerName = cust.CustomerName,
                            ShipmentModeID = exp.ExpenseTypeID,
                            ShipmentMode = exp.ExpenseType
                        };
            //Filtering with Customer ID
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

            // PaginationService
            var paginatedResult = await PaginationService<OFRAdjustApprovalTopGridVM, OFRAdjustApprovalTopGridVM>.GetPaginatedData(query, pageNumber, pageSize, searchTerm, sortColumn, sortOrder,
                    term => sc =>
                        EF.Functions.Like(sc.JobNo ?? "", $"%{term}%") ||
                        EF.Functions.Like(sc.CustomerName ?? "", $"%{term}%") ||
                        EF.Functions.Like(sc.ShipmentMode ?? "", $"%{term}%"),
                    ob => ob
             );

            return paginatedResult;
        }
        #endregion
        #endregion

        #region When User Checked on radion then fill input fields and Detaisl data Copied to Tmp Details
        #region Get Requisition Info in Input fields
        public async Task<OFRAdjustApprovalMasterVM> GetRequisitionMasterByJobNo(string jobNo)
        {
            var query = from ofr in _repository.All()
                        join doc in _documentation.All() on ofr.JobNo equals doc.JobNo
                        join cust in _customer.All() on doc.CustomerID equals cust.CustomerID into custGroup
                        from cust in custGroup.DefaultIfEmpty()
                        join expType in _shipmentmode.All() on ofr.ExpenseTypeID equals expType.ExpenseTypeID into expTypeGroup
                        from expType in expTypeGroup.DefaultIfEmpty()
                        where ofr.JobNo == jobNo
                        select new OFRAdjustApprovalMasterVM
                        {
                            OFRNo = ofr.OFRNo,
                            JobNo = ofr.JobNo,
                            OFRDate = ofr.OFRDate,                            
                            CustomerID = cust.CustomerID,
                            CustomerName = cust.CustomerName ?? "",
                            InvoiceNo = doc.InvoiceNo,
                            Quantity = doc.Quntity1,
                            ShipmentModeID = expType.ExpenseTypeID,
                            ShipmentMode = expType.ExpenseType,
                            LCValue = doc.LCValue,
                            MaterialDescription = doc.MaterialDescription,
                            Weight = doc.Quntity2
                        };

            return await query.FirstOrDefaultAsync();
        }
        #endregion

        #region Get Requisition Details into Tmp Details
        public async Task<bool> CopyDetailsToTmp(string jobNo, int? currentUserId, BaseViewModel model)
        {
            var details = await _details.All().Where(d => d.JobNo == jobNo).ToListAsync();

            if (!details.Any()) return false;

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
                BillAdjustAmount = d.BillAdjustAmount,
                LUser = currentUserId?.ToString(),
                LDate = DateTime.Now,
                LMAC = model.LMAC,
                LIP = model.LIP,
                CompanyCode = d.CompanyCode,
                EmployeeID = currentUserId.ToString(),
                ExpenseHeadRemarks = d.ExpenseHeadRemarks
            }).ToList();
            if (tmpDetails == null || !tmpDetails.Any())
            {
                throw new InvalidOperationException("Failed to map details to temporary table");
            }
            await _Tmpdetails.AddRangeAsync(tmpDetails);
            return true;
        }
        #endregion
        #endregion

        #region Get All Tmp Details
        public async Task<List<OFRAdjustApprovalDetailsVM>> GetDetailsWithCashBank(string jobno, int? userId)
        {
            var result = await (from tmp in _Tmpdetails.All()
                                join head in _accountsHead.All()
                                    on tmp.ExpenseHeadID equals head.ExpenseHeadID
                                join serv in _serviceType.All()
                                    on tmp.ServiceTypeID equals serv.ServiceTypeID
                                join ofrDet in _details.All()   // Original table for Cash/Bank
                                    on tmp.OFR_DetailsID equals ofrDet.OFR_DetailsID
                                where tmp.LUser == userId.ToString() && tmp.JobNo == jobno
                                orderby tmp.TC
                                select new OFRAdjustApprovalDetailsVM
                                {
                                    Tc = tmp.TC,
                                    OFRDetailsID = tmp.OFR_DetailsID,
                                    SerialNo = head.SerialNo,
                                    AccountHeadName = head.ExpenseHead,
                                    IsReceivetable = head.IsReceiptable,
                                    ServiceTypeName = serv.ServiceTypeName,
                                    Amount = tmp.EstimatedAmount,
                                    ApprovalAmount = tmp.ActualAmount,
                                    AdjustAmount = tmp.BillAdjustAmount,
                                    DiffentAmount = tmp.DifferentAmount,
                                    Remark = tmp.ExpenseHeadRemarks,
                                    CashBank = ofrDet.AdjustDetailsCashBank,   
                                    BankAccount = ofrDet.AdjustDetailsBankAccount
                                }).ToListAsync();

            return result;
        }

        //public async Task<List<OFRAdjustApprovalDetailsVM>> GetAllTmpAsync(int? currentUser)
        //{
        //    var result = await (from tmp in _Tmpdetails.All()
        //                        join head in _accountsHead.All()
        //                            on tmp.ExpenseHeadID equals head.ExpenseHeadID
        //                            into headJoin
        //                        from head in headJoin.DefaultIfEmpty()
        //                        join serv in _serviceType.All()
        //                      on tmp.ServiceTypeID equals serv.ServiceTypeID
        //                      into servicJoin
        //                        from servic in servicJoin.DefaultIfEmpty()
        //                        where tmp.LUser == currentUser.ToString()
        //                        orderby tmp.TC

        //                        select new OFRAdjustApprovalDetailsVM
        //                        {
        //                            Tc = tmp.TC,
        //                            OFRDetailsID = tmp.OFR_DetailsID,
        //                            SerialNo = head.SerialNo,
        //                            AccountHeadName = head.ExpenseHead,
        //                            IsReceivetable = head.IsReceiptable,
        //                            ServiceTypeName = servic.ServiceTypeName,
        //                            Amount = tmp.EstimatedAmount,
        //                            ApprovalAmount = tmp.ActualAmount,
        //                            AdjustAmount = tmp.BillAdjustAmount,
        //                            DiffentAmount = tmp.DifferentAmount,
        //                            Remark = tmp.ExpenseHeadRemarks
        //                        }).ToListAsync();

        //    return result;
        //}

        #endregion

        #region Approval  Adjust Amount Update
        public async Task<bool> SaveAdjustApprovalAsync(List<OFRAdjustApprovalDetailsSaveVM> model, int? Approveduser)
        {
            if (model == null || !model.Any())  return false;

            await _repository.BeginTransactionAsync();

            try
            {
                string ofrNo = model.First().OFRNo;

                  // ENTRY (HEADER) UPDATE
                var entry = await _repository.All().FirstOrDefaultAsync(x => x.OFRNo == ofrNo);

                if (entry == null) throw new Exception("OFR Entry not found");

                // Header-level Cash/Bank (first non-empty from details)
                entry.CashBank2 = model.Select(x => x.CashBank).FirstOrDefault(x => !string.IsNullOrWhiteSpace(x));

                entry.BankAccount2 = model.Select(x => x.BankAcount).FirstOrDefault(x => !string.IsNullOrWhiteSpace(x));

                await _repository.UpdateAsync(entry);

                   //DETAILS UPDATE (ROW-WISE)
                foreach (var item in model)
                {
                    var details = await _details.All().FirstOrDefaultAsync(x =>x.OFR_DetailsID == item.OFR_DetailsID && x.OFRNo == ofrNo);

                    if (details == null)  throw new Exception($"Details not found: {item.OFR_DetailsID}");

                    details.AdjustApprovalAmount = item.ConfirmAmount;
                    details.DifferentAmount2 = item.DifferentAmount;

                    // Row-wise Cash / Bank
                    details.AdjustDetailsCashBank = item.CashBank;
                    details.AdjustDetailsBankAccount = item.BankAcount;

                    await _details.UpdateAsync(details);
                }

                   //TEMP DATA CLEANUP
                var tmpToDelete = await _Tmpdetails.All().Where(x => x.OFRNo == ofrNo && x.LUser == Approveduser.ToString()).ToListAsync();

                foreach (var tmp in tmpToDelete)
                {
                    await _Tmpdetails.DeleteAsync(tmp.TC);
                }

                await _repository.CommitTransactionAsync();
                return true;
            }
            catch (Exception ex)
            {
                await _repository.RollbackTransactionAsync();
                Console.WriteLine("SaveAdjustApproval Failed: " + ex.Message);
                return false;
            }
        }


        #endregion
    }
}
