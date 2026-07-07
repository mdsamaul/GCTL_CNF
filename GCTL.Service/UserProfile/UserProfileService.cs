using GCTL.Core.Repository;
using GCTL.Data.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Service.UserProfile
{
    public class UserProfileService : IUserProfileService
    {
        private readonly AppDbContext _context;
        private readonly IGenericRepository<Organization> _orgnRepository;
        private readonly IGenericRepository<GCTL.Data.Models.Employees> _empRepository;

        public UserProfileService(AppDbContext context, IGenericRepository<Organization> orgnRepository, IGenericRepository<Data.Models.Employees> empRepository)
        {
            _context = context;
            _orgnRepository = orgnRepository;
            _empRepository = empRepository;
        }

        public (string FullName, string ProfilePicturePath) GetUserProfileAsync(string userId)
        {
            string fullName = "Guest User";
            string profilePicturePath = "/media/employee/No_image_available.svg.png";

            if (!string.IsNullOrEmpty(userId))
            {
                var user = _context.Users
                    .FirstOrDefault(u => u.Id == userId);

                if (user != null)
                {
                    var empId = user.EmployeeId;



                    var employee = _context.Employees
                        .FirstOrDefault(e => e.EmployeeID == empId);

                    if (employee != null)
                    {
                        fullName = employee.FirstName + " " + employee.LastName ?? fullName;
                        profilePicturePath = !string.IsNullOrEmpty(employee.EmployeeImageFileName)
                            ? employee.EmployeeImageFileName
                            : profilePicturePath;
                    }
                }
            }

            return (fullName, profilePicturePath);
        }



        //public (string FullName, string ProfilePicturePath) GetUserProfileAsync(string userId)
        //{
        //    string fullName = "Guest User";
        //    string profilePicturePath = "/media/employee/No_image_available.svg.png";

        //    if (!string.IsNullOrEmpty(userId))
        //    {
        //        var user = _context.Users
        //            .FirstOrDefault(u => u.Id == userId);

        //        if (user != null)
        //        {
        //            var empId = user.EmployeeId;

        //            // Project to anonymous type with NULL handling
        //            var employee = _context.Employees
        //                .Where(e => e.EmployeeID == empId)
        //                .Select(e => new
        //                {
        //                    FirstName = e.FirstName ?? "",
        //                    LastName = e.LastName ?? "",
        //                    ImageFileName = e.EmployeeImageFileName ?? ""
        //                })
        //                .FirstOrDefault();

        //            if (employee != null)
        //            {
        //                fullName = $"{employee.FirstName} {employee.LastName}".Trim();
        //                if (string.IsNullOrWhiteSpace(fullName))
        //                {
        //                    fullName = "Guest User";
        //                }

        //                profilePicturePath = !string.IsNullOrEmpty(employee.ImageFileName)
        //                    ? employee.ImageFileName
        //                    : profilePicturePath;
        //            }
        //        }
        //    }

        //    return (fullName, profilePicturePath);
        //}

        public int? GetCurrentEmployeeId(string userId)
        {
            int? currentEmployeeId = null;

            if (!string.IsNullOrEmpty(userId))
            {
                var user = _context.Users
                    .FirstOrDefault(u => u.Id == userId); // synchronous query

                if (user != null)
                {
                    currentEmployeeId = user.EmployeeId;  // Fetch and return the Employee ID directly
                }
            }

            return currentEmployeeId;
        }


        public async Task<int?> GetCurrentEmployeeIdAsync(string userId)
        {
            int? currentEmployeeId = null;

            if (!string.IsNullOrEmpty(userId))
            {
                var user = await _context.Users
                    .FirstOrDefaultAsync(u => u.Id == userId);

                if (user != null)
                {
                    currentEmployeeId = user.EmployeeId;  // Fetch and return the Employee ID directly
                }
            }

            return currentEmployeeId;
        }
        public async Task<int?> GetCurrentOrganizationIdAsync(string userId)
        {
            int? currentEmployeeId = null;

            if (!string.IsNullOrEmpty(userId))
            {
                var user = await _context.Users
                    .FirstOrDefaultAsync(u => u.Id == userId);

                if (user != null)
                {
                    currentEmployeeId = user.OrganizationID;  // Fetch and return the Employee ID directly
                }
            }

            return currentEmployeeId;
        }

        public async Task<string> GetFaviconPathForUserAsync(ClaimsPrincipal user)
        {
            var userId = user?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return "/media/company/fevicon/default.png";

            var userFetch = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (userFetch == null || userFetch.OrganizationID == null)
                return "/media/company/fevicon/default.png";

            var faviconFileName = await _orgnRepository.AllActive()
                .Where(x => x.OrganizationID == userFetch.OrganizationID)
                .Select(x => x.FaviconLink)
                .FirstOrDefaultAsync();

            if (string.IsNullOrEmpty(faviconFileName))
                return "/media/company/fevicon/default.png";

            return $"/media/company/fevicon/{faviconFileName}";
        }



        public async Task<string?> GetCurrentRoleAsync(string userId)
        {
            if (string.IsNullOrEmpty(userId))
                return null;

            var roleName = await (from user in _context.Users
                                  join userRole in _context.UserRoles on user.Id equals userRole.UserId
                                  join role in _context.Roles on userRole.RoleId equals role.Id
                                  where user.Id == userId
                                  select role.Name.Substring(role.Name.IndexOf("_", role.Name.IndexOf("_") + 1) + 1))
                                 .FirstOrDefaultAsync();

            return roleName;
        }


        public async Task<string?> GetCurrentRoleIdAsync(string userId)
        {
            // Check if userId is null or empty first
            if (string.IsNullOrEmpty(userId))
                return null;

            // Async LINQ query to retrieve the role ID
            var roleId = await (from user in _context.Users
                                join userRole in _context.UserRoles on user.Id equals userRole.UserId
                                join role in _context.Roles on userRole.RoleId equals role.Id
                                where user.Id == userId
                                select role.Id) // Select the Role ID instead of Name
                                .FirstOrDefaultAsync();  // Asynchronous call to fetch the first matching role

            // Return the role ID
            return roleId;
        }
        

        public async Task<int?> GetCurrentCompanyIdAsync(string userId)
        {
            try
            {
                int? currentEmployeeId = null;

                if (!string.IsNullOrEmpty(userId))
                {
                    var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);

                    if (user != null)
                    {
                        currentEmployeeId = user.OrganizationID;  // Fetch and return the Employee ID directly
                    }
                }

                return currentEmployeeId;
            }
            catch (Exception)
            {

                throw;
            }
        }
    }


}
