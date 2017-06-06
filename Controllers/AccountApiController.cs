using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ReactSpa.Data;

namespace ReactSpa.Controllers
{
    [Authorize]
    [Route("api/account/[action]")]
    public class AccountApiController : Controller
    {
        private readonly UserManager<UserInfo> _userManager;
        private readonly SignInManager<UserInfo> _signInManager;
        private readonly ILogger _logger;
        private readonly string _externalCookieScheme;
        private readonly UserInfoManager _userInfoManager;

        public AccountApiController(
            UserManager<UserInfo> userManager,
            SignInManager<UserInfo> signInManager,
            IOptions<IdentityCookieOptions> identityCookieOptions,
            ILoggerFactory loggerFactory,
            IOptions<ConnectionInfo> config)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _externalCookieScheme = identityCookieOptions.Value.ExternalCookieAuthenticationScheme;
            _logger = loggerFactory.CreateLogger<AccountApiController>();
            _userInfoManager = new UserInfoManager(config);
        }

        /** Bug here, lack of functionality of [Deputy], [Supervisor] */

        [HttpPost]
        public async Task<ActionResult> GetInitState()
        {
            var userList = await _userInfoManager.GetUserInfoAsync(User.FindFirstValue(ClaimTypes.NameIdentifier));
            if (userList == null)
                return Json(new {status = false});

            return Json(new {payload = userList});
        }

        /** Bug here, lack of functionality of [Role] */

        [HttpPost]
        public async Task<ActionResult> GetUserInfo([FromBody] DeleteUserModel model)
        {
            var user = await _userManager.FindByIdAsync(model.Id);
            if (user == null)
                return Json(new {status = false});
            string date = user.DateOfEmployment == null ? null : user.DateOfEmployment.Value.ToString("yyyy-M-d");

            var deputies = await _userInfoManager.GetUserDeputyAsync(model.Id);
            var supervisors = await _userInfoManager.GetUserSupervisorAsync(model.Id);
            var roles = await _userManager.GetRolesAsync(user);
            string role = roles.Any() ? roles[0] : "default";
            return Json(new
            {
                status = true,
                payload = new
                {
                    a = user.UserName,
                    b = user.Email,
                    c = user.PhoneNumber,
                    d = date,
                    e = user.JobTitle,
                    f = user.AnnualLeaves,
                    g = user.SickLeaves,
                    h = user.FamilyCareLeaves,
                    i = deputies,
                    j = supervisors,
                    k = role
                }
            });
        }

        [HttpPost]
        public async Task<ActionResult> GetNameList([FromBody] SearchNameListModel model)
        {
            var nameList = await _userInfoManager.SearchUserNameAsync(model.Param);
            return Json(new {payload = nameList});
        }

        /** Bug here, lack of functionality of [Authority] */

        [HttpPost]
        public async Task<ActionResult> CreateUser([FromBody] UserModel model)
        {
            if (!ModelState.IsValid)
                return Json(new {status = false, message = "Data is not valid!"});
            DateTime dateOfEmployment;
            DateTime.TryParse(model.DateOfEmployment, out dateOfEmployment);
            var user = new UserInfo
            {
                UserName = model.UserName,
                Email = model.UserEmail,
                PhoneNumber = model.PhoneNumber,
                DateOfEmployment = dateOfEmployment,
                JobTitle = model.JobTitle,
                AnnualLeaves = model.AnnualLeaves,
                SickLeaves = model.SickLeaves,
                FamilyCareLeaves = model.FamilyCareLeaves
            };
            var result = await _userInfoManager.CreateUserAsync(user);

            if (result.Succeeded)
            {
                bool deputyResult = await _userInfoManager.AddUserDeputyAsync(user.Id, model.Deputy);
                bool supervisorResult = await _userInfoManager.AddUserSupervisorAsync(user.Id, model.Supervisor);
                if (deputyResult && supervisorResult)
                {
                    var roleResult = await _userManager.AddToRoleAsync(user, model.Authority);
                    if (roleResult.Succeeded)
                        return Json(new {status = true});
                }
            }
            return Json(new {status = false});
        }

        /** Bug here, lack of functionality of [Authority] */

        [HttpPost]
        public async Task<ActionResult> UpdateUser([FromBody] UserModel model)
        {
            if (!ModelState.IsValid)
                return Json(new {status = false, message = "Data is not valid!"});
            UserInfo user = await _userInfoManager.UpdateUserAsync(model);
            var roles = await _userManager.GetRolesAsync(user);
            var role = roles.Any() ? roles[0] : "default";
            if (role == model.Authority)
                return Json(new {status = true});
            var removeRoleResult = await _userManager.RemoveFromRoleAsync(user, role);
            var addRoleResult = await _userManager.AddToRoleAsync(user, model.Authority);
            if (removeRoleResult.Succeeded && addRoleResult.Succeeded)
                return Json(new {status = true});
            return Json(new {status = false});
        }

        [HttpPost]
        public async Task<ActionResult> DeleteUser([FromBody] DeleteUserModel model)
        {
            if (model.Id != User.FindFirstValue(ClaimTypes.NameIdentifier))
            {
                var user = await _userManager.FindByIdAsync(model.Id);
                var result = await _userManager.DeleteAsync(user);
                if (result.Succeeded)
                    return Json(new {status = true});
            }
            return Json(new {status = false});
        }

        [HttpPost]
        public async Task<ActionResult> IsEmailValid([FromBody] ValidModel model)
        {
            var result = await _userInfoManager.IsEmailValid(model.Id, model.Param);
            return Json(new {status = result});
        }

        public class UserModel
        {
            public string Id { get; set; }
            public string UserName { get; set; }

            [EmailAddress]
            public string UserEmail { get; set; }

            public string PhoneNumber { get; set; }
            public string DateOfEmployment { get; set; }
            public string JobTitle { get; set; }

            [Range(0, 100)]
            public decimal AnnualLeaves { get; set; }

            [Range(0, 100)]
            public decimal SickLeaves { get; set; }

            [Range(0, 100)]
            public decimal FamilyCareLeaves { get; set; }

            public List<NameListModel> Supervisor { get; set; }
            public List<NameListModel> Deputy { get; set; }
            public string Authority { get; set; }
        }

        public class SearchNameListModel
        {
            public string Param { get; set; }
        }

        public class DeleteUserModel
        {
            public string Id { get; set; }
        }

        public class ValidModel
        {
            public string Id { get; set; }
            public string Param { get; set; }
        }
    }
}