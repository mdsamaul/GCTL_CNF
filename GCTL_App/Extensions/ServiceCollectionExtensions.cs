#region References
using GCTL.Core.Configurations;
using GCTL.Core.Helpers.AttendenceHelper;
using GCTL.Core.Repository;
using GCTL.Data.Models;
using GCTL.Service.ActionLogAudit;
using GCTL.Service.AdminSettings.GeneralSettings;
using GCTL.Service.AdminSettings.OrganizationSettings.BranchService;
using GCTL.Service.AdminSettings.OrganizationSettings.CompanyService;
using GCTL.Service.AdminSettings.OrganizationSettings.DepartmentService;
using GCTL.Service.AdminSettings.OrganizationSettings.DesignationService;
using GCTL.Service.AdminSettings.OrganizationSettings.HolidayService;
using GCTL.Service.AdminSettings.SystemSettings.Emailsettingservice;
using GCTL.Service.AdminSettings.SystemSettings.EmailSettingService;
using GCTL.Service.AllNotifications;
using GCTL.Service.BankModule.BankAccount;
using GCTL.Service.BankModule.BankBranch;
using GCTL.Service.BankModule.BankInfo;
using GCTL.Service.BillService;
using GCTL.Service.ChartOfAccounts;
using GCTL.Service.ChartOfAccounts.ControlLedger;
using GCTL.Service.ChartOfAccounts.GeneralLedger;
using GCTL.Service.ChartOfAccounts.GroupLedger;
using GCTL.Service.ChartOfAccounts.SubControlLedger;
using GCTL.Service.ChartOfAccounts.SubSidiaryLedger;
using GCTL.Service.ClearAndF.SepDocumentation;
using GCTL.Service.ClearAndF.Update;
using GCTL.Service.CustomerRelationshipManagement.AddContactPerson;
using GCTL.Service.CustomerRelationshipManagement.AddSalesCustomer;
using GCTL.Service.CustomerRelationshipManagement.AddSalesDeliveryLocation;
using GCTL.Service.DeleteHistories;
using GCTL.Service.ElementPermission;
using GCTL.Service.Employees.EmployeeAdditional;
using GCTL.Service.Employees.EmployeeAllowance;
using GCTL.Service.Employees.EmployeeBenifit;
using GCTL.Service.Employees.EmployeeContact;
using GCTL.Service.Employees.EmployeeDetails;
using GCTL.Service.Employees.EmployeeEducational;
using GCTL.Service.Employees.EmployeeFamily;
using GCTL.Service.Employees.EmployeeList;
using GCTL.Service.Employees.EmployeeNavigation;
using GCTL.Service.Employees.EmployeeOfficial;
using GCTL.Service.Employees.EmployeePersonal;
using GCTL.Service.Employees.EmployeeReport;
using GCTL.Service.Employees.EmployeeSalary;
using GCTL.Service.Employees.EmployeeTraining;
using GCTL.Service.FileHandler;
using GCTL.Service.HRMsettings.ProbationService;
using GCTL.Service.ImageFileHandler;
using GCTL.Service.Language;
using GCTL.Service.MasterSetup.ActionTakens;
using GCTL.Service.MasterSetup.BloodGroups;
using GCTL.Service.MasterSetup.CoreBranch;
using GCTL.Service.MasterSetup.CoreCompany;
using GCTL.Service.MasterSetup.CoreCountries;
using GCTL.Service.MasterSetup.CoreServiceType;
using GCTL.Service.MasterSetup.Countries;
using GCTL.Service.MasterSetup.Currency;
using GCTL.Service.MasterSetup.CurrencyType;
using GCTL.Service.MasterSetup.Degrees;
using GCTL.Service.MasterSetup.Department;
using GCTL.Service.MasterSetup.Designation;
using GCTL.Service.MasterSetup.EducationBoards;
using GCTL.Service.MasterSetup.EducationLevel;
using GCTL.Service.MasterSetup.EmployeeTypes;
using GCTL.Service.MasterSetup.EmploymentNatures;
using GCTL.Service.MasterSetup.ExpenseHead;
using GCTL.Service.MasterSetup.Gender;
using GCTL.Service.MasterSetup.Grades;
using GCTL.Service.MasterSetup.HrmDefDepartment;
using GCTL.Service.MasterSetup.HrmDefDesignations;
using GCTL.Service.MasterSetup.LicenceType;
using GCTL.Service.MasterSetup.MaritalStatuses;
using GCTL.Service.MasterSetup.Organizations;
using GCTL.Service.MasterSetup.PassingYear;
using GCTL.Service.MasterSetup.PaymenPeriodType;
using GCTL.Service.MasterSetup.PaymentMode;
using GCTL.Service.MasterSetup.PaymentTerms;
using GCTL.Service.MasterSetup.PaymentType;
using GCTL.Service.MasterSetup.ProvisionPeriodTimeType;
using GCTL.Service.MasterSetup.Religion;
using GCTL.Service.MasterSetup.ResultType;
using GCTL.Service.MasterSetup.ShipmentMode;
using GCTL.Service.MasterSetup.Statuse;
using GCTL.Service.MasterSetup.TrainingYear;
using GCTL.Service.MasterSetup.UnitType;
using GCTL.Service.MasterSetup.VendorPrefix;
using GCTL.Service.MasterSetup.YearlyEndBonusType;
using GCTL.Service.MenuTabs;
using GCTL.Service.OFRAdjustApproval;
using GCTL.Service.OFRApproval;
using GCTL.Service.OFRBillAdjust;
using GCTL.Service.OpeningBalance;
using GCTL.Service.OperationalFund;
using GCTL.Service.RolePermissions;
using GCTL.Service.UserProfile;
using GCTL.Service.VisitingPath;
using GCTL.Service.VoucherEntry;
using GCTL.Service.VoucherType;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System;
using System.Data;


#endregion

namespace GCTL_App.Extensions
{
    public static class ServiceCollectionExtensions
    {
        #region Connection
        public static void ConfigureContext(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("connection"))
            .EnableSensitiveDataLogging().LogTo(Console.WriteLine, LogLevel.Information));
        }

        public static void ConfigureDapperConnection(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddScoped<IDbConnection>(sp =>
                new SqlConnection(configuration.GetConnectionString("connection")));
        }
        #endregion

        public static void ConfigureServices(this IServiceCollection services, IConfiguration configuration)
        {
            #region Main Settings Start
            services.Configure<ApplicationSettings>(configuration.GetSection("ApplicationSettings"));
            services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
            services.AddScoped<IMenuTabsService, MenuTabsService>();
           
            #endregion


            #region Added by Md. Rakib Hasan
            services.AddScoped<IActionTakenService, ActionTakenService>();
            services.AddScoped<IBloodGroupService, BloodGroupService>();
            services.AddScoped<ICountryService, CountryService>();
            services.AddScoped<ICurrencyService, CurrencyService>();
            services.AddScoped<IDegreeService, DegreeService>();
            services.AddScoped<IDepartmentService, GCTL.Service.MasterSetup.Department.DepartmentService>();
            services.AddScoped<IDesignationService, GCTL.Service.MasterSetup.Designation.DesignationSettingService>();
            services.AddScoped<IEducationBoardService, EducationBoardService>();
            services.AddScoped<IEducationLevelsService, EducationLevelService>();
            services.AddScoped<IEmployeeTypesService, EmployeeTypesService>();
            services.AddScoped<IEmploymentNatureService, EmploymentNatureService>();
            services.AddScoped<IGenderService, GenderService>();
            services.AddScoped<IGradeService, GradeService>();
            services.AddScoped<IMaritalStatusService, MaritalStatusService>();
            services.AddScoped<IPaymentPeriodsService, PaymentPeriodsService>();
            //services.AddScoped<IPaymentModeService, PaymentModesService>();
            services.AddScoped<IReligionService, ReligionService>();
            services.AddScoped<IStatusService, StatusService>();
            services.AddScoped<ILicenceTypeService, LicenceTypeService>();
            services.AddScoped<IOrganizationsService, OrganizationsService>();
            services.AddScoped<IPassingYearService, PassingYearService>();
            services.AddScoped<IProvisionPeriodTtimeTypesService, ProvisionPeriodTtimeTypesService>();
            services.AddScoped<IResultTypeService, ResultTypeService>();
          
            services.AddScoped<ITrainingYearService, TrainingYearService>();
            services.AddScoped<IYearlyEndBonusTypeService, YearlyEndBonusTypeService>();
            #endregion


            //Siam 
            services.AddScoped<IActionLogService, ActionLogService>();
            services.AddScoped<IUserInfoService, UserInfoService>();
            services.AddScoped<IVisitingPathService, VisitingPathService>();
        
            services.AddScoped<INotificationsService, NotificationsService>();
            services.AddScoped<ISepDocumentationService, SepDocumentationService>();
            services.AddScoped<IJobService, JobService>();
            services.AddScoped<IBillService, BillService>();
           


            #region Asad
            services.AddScoped<IUserProfileService, UserProfileService>();
            services.AddScoped<IRoleService, RoleService>();
            services.AddScoped<IElementPermissionService, ElementPermissionService>();
            services.AddScoped<IEmailSettingService, EmailSettingsService>();
            services.AddScoped<IHolidaySettingService, HolidaySettingService>();
            
            services.AddScoped<ICompanySettingService, CompanySettingService>();
            services.AddScoped<IBranchSettingService, BranchSettingService>();
            services.AddScoped<IDesignationSettingService, GCTL.Service.AdminSettings.OrganizationSettings.DesignationService.DesignationSettingService>();
            services.AddScoped<IDepartmentSettingService, DepartmentSettingService>();
            services.AddScoped<IProbationSettingService, ProbationSettingService>();
            services.AddTransient<HolidayHelper>();
            services.AddTransient<WeekendHelper>();
            services.AddTransient<LeaveHelper>();
            services.AddScoped<ILocalizationSettingService, LocalizationSettingService>();
          
            #endregion
            #region Language Services


            services.AddScoped<ITranslateService, TranslateService>();
            services.AddScoped<ILanguageTableService, LanguageTableService>();

            #endregion

            #region Employee Services

            services.AddScoped<IEmployeePersonalService, EmployeePersonalService>();
            services.AddScoped<IEmployeeOfficialService, EmployeeOfficialService>();
            services.AddScoped<IEmployeeSalaryService, EmployeeSalaryService>();
            services.AddScoped<IEmployeeBenifitService, EmployeeBenifitService>();
            services.AddScoped<IEmployeeAllowanceService, EmployeeAllowanceService>();

            services.AddScoped<IEmployeeAdditionalService, EmployeeAdditionalService>();
            services.AddScoped<IEmployeeTrainingService, EmployeeTrainingService>();
            services.AddScoped<IEmployeeEducationalService, EmployeeEducationalService>();
            services.AddScoped<IEmployeeFamilyService, EmployeeFamilyService>();
            services.AddScoped<IEmployeeContactService, EmployeeContactService>();
            services.AddScoped<IEmployeeListService, EmployeeListService>();
            services.AddScoped<IEmployeeDetailsService, EmployeeDetailsService>();

            services.AddScoped<IEmployeeNavigationService, EmployeeNavigationService>();
            services.AddScoped<IEmployeeReportService, EmployeeReportService>();

            #endregion

            #region Attendance Management Services

           

            #endregion

            #region File Handler

            services.AddScoped<IImageFileHandlerService, ImageFileHandlerService>();
            services.AddScoped<IPdfFileHandler, PdfFileHandler>();

            #endregion


            #region Added by Abu Sayed
            services.AddScoped<ICoreCountryService, CoreCountryService>();
            services.AddScoped<IHrmDefDesignationService, HrmDefDesignationService>();
            services.AddScoped<IVendorPrefixService, VendorPrefixService>();
            services.AddScoped<ICurrencyType, CurrencyTypeService>();
            services.AddScoped<IUnitType, UnitTypeService>();
            services.AddScoped<IShipmentMode, ShipmentModeService>();
            services.AddScoped<IPaymentMode, PaymentModeService>();
            services.AddScoped<IPaymentType, PaymentTypeService>();
            services.AddScoped<IPaymentTerms, PaymentTermService>();
            services.AddScoped<IExpenseHead, ExpenseHeadService>();
            services.AddScoped<IServiceType, ServiceTypeService>();
            services.AddScoped<ISalesCustomerService, SalesCustomerService>();
            services.AddScoped<ISalesDeliveryLocationService, SalesDeliveryLocationService>();
            services.AddScoped<IDepartment, GCTL.Service.MasterSetup.HrmDefDepartment.DepartmentService>();
            services.AddScoped<ICoreBranch, CoreBranchService>();
            services.AddScoped<IBankInfo, BankInfoService>();
            services.AddScoped<IBankBranch, BankBranchService>();
            services.AddScoped<IBankAccount, BankAccountService>();
            services.AddScoped<ICoreCompany, CoreCompanyService>();
            services.AddScoped<IContactPersonService, ContactPersonService>();
            services.AddScoped<IGroupLedger, GroupLedgerService>();
            services.AddScoped<IContrlLedger, ControlLedgerService>();
            services.AddScoped<ISubControlLedger, SubControlLedgerService>();
            services.AddScoped<ISubSidiaryLedger, SubSidiaryLedgerService>();
            services.AddScoped<IGeneralLedger, GeneralLedgerService>();
            services.AddScoped<LedgerReportService>();
            services.AddScoped<IVoucherType, VoucherTypeService>();
            services.AddScoped<IOpeningBalance, OpeningBalanceService>();
            services.AddScoped<IVoucherEntry, VoucherEntryService>();
            services.AddScoped<IVoucherDetails, VoucherDetailsService>();
            services.AddScoped<VoucherReportService>();
            services.AddScoped<Ioperational, OperationalService>();
            services.AddScoped<IDeleteHistoryService, DeleteHistoryService>();
            services.AddScoped<IOFRApproval, OFRApprovalService>();
            services.AddScoped<IOFRBillAdjust, OFRBillAdjustService>();
            services.AddScoped<IOFRAdjustApproval, OFRAdjustApprovalService>();
            #endregion

            #region e43

            #endregion
        }
    }
}
