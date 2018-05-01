using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using ReactSpa.Data;

namespace ReactSpa.Controllers
{
    [Authorize]
    [Route("api/account/[action]")]
    public class AccountApiController : Controller
    {
        private readonly UserManager<UserInfo> _userManager;
        private readonly UserInfoManager _userInfoManager;
        private readonly RecordManager _recordManager;

        public AccountApiController(
            UserManager<UserInfo> userManager,
            IOptions<ConnectionInfo> config)
        {
            _userManager = userManager;
            _userInfoManager = new UserInfoManager(config);
            _recordManager = new RecordManager(config);
        }

        //
        // get initial state at '/accountInfo'
        // POST: /api/account/GetInitState
        [HttpPost]
        public async Task<ActionResult> GetInitState()
        {
            var userList = await _userInfoManager.GetUserInfoAsync(User.FindFirstValue(ClaimTypes.NameIdentifier));
            if (userList == null)
                return Json(new {status = false});

            return Json(new {payload = userList});
        }

        //
        // get user data at '/manageaccount'
        // POST: /api/account/getUserInfo
        [HttpPost]
        [Authorize(Roles = "manager, admin, boss")]
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
                    k = role,
                    l = user.DateOfQuit
                }
            });
        }

        //
        // search for name and id by typing key words at '/report', '/manageaccount'
        // POST: /api/account/getNameList
        [HttpPost]
        [Authorize(Roles = "manager, admin, boss")]
        public async Task<ActionResult> GetNameList([FromBody] SearchNameListModel model)
        {
            var nameList = await _userInfoManager.SearchUserNameAsync(model.Param);
            return Json(new {payload = nameList});
        }

        // 
        // create a new user at '/manageaccount'
        // POST: /api/account/CreateUser
        [HttpPost]
        [Authorize(Roles = "manager, admin, boss")]
        public async Task<ActionResult> CreateUser([FromBody] UserModel model)
        {
            if (!ModelState.IsValid)
                return Json(new {status = false, message = "Data is not valid!"});
            DateTime dateOfEmployment, dateOfQuit;
            DateTime.TryParse(model.DateOfEmployment, out dateOfEmployment);
            DateTime? quitVal = null;
            if (DateTime.TryParse(model.DateOfQuit, out dateOfQuit))
                quitVal = dateOfQuit;

            var user = new UserInfo
            {
                UserName = model.UserName,
                Email = model.UserEmail,
                PhoneNumber = model.PhoneNumber,
                DateOfEmployment = dateOfEmployment,
                DateOfQuit = quitVal,
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


        // 
        // update an existed user data at '/manageaccount'
        // POST: /api/account/updateUser
        [HttpPost]
        [Authorize(Roles = "manager, admin, boss")]
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

        // 
        // delete an existed user at '/manageaccount'
        // POST: /api/account/deleteUser
        [HttpPost]
        [Authorize(Roles = "manager, admin, boss")]
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

        // 
        // check if email is unique and valid at '/manageaccount'
        // POST: /api/account/isEmailValid
        [HttpPost]
        [Authorize(Roles = "manager, admin, boss")]
        public async Task<ActionResult> IsEmailValid([FromBody] ValidModel model)
        {
            var result = await _userInfoManager.IsEmailValid(model.Id, model.Param);
            return Json(new {status = result});
        }

        [HttpGet]
        [Authorize(Roles = "manager, admin, boss")]
        public async Task<ActionResult> GetRemainOffExcel()
        {
            var obj = await _recordManager.GetRemainingDayOffAsync();
            using (ExcelPackage pkg = new ExcelPackage(new FileInfo($"{Guid.NewGuid()}.xlsx")))
            {
                ExcelWorksheet worksheet = pkg.Workbook.Worksheets.Add("報表1");
                for (int x = 1; x < 10; x++)
                {
                    worksheet.Column(x).Width = 15;
                    worksheet.Column(x).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                }
                worksheet.Cells[1, 1].Value = "剩餘天數狀態表";
                worksheet.Cells[1, 1, 1, 5].Merge = true;
                worksheet.Cells[1, 1, 1, 5].Style.Font.Bold = true;
                worksheet.Cells[1, 1, 1, 5].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                worksheet.Cells[2, 1].Value = "姓名";
                worksheet.Cells[2, 2].Value = "年分";
                worksheet.Cells[2, 3].Value = "特休剩餘天數";
                worksheet.Cells[2, 4].Value = "病假剩餘天數";
                worksheet.Cells[2, 5].Value = "家庭照顧假剩餘天數";

                for (int i = 0; i < obj.Count; i++)
                {
                    worksheet.Cells[$"A{i + 3}"].Value = obj[i].UserName;
                    worksheet.Cells[$"B{i + 3}"].Value = obj[i].Year;
                    worksheet.Cells[$"C{i + 3}"].Value = obj[i].AnnualLeaves;
                    worksheet.Cells[$"D{i + 3}"].Value = obj[i].SickLeaves;
                    worksheet.Cells[$"E{i + 3}"].Value = obj[i].FamilyCareLeaves;
                }

                byte[] file = pkg.GetAsByteArray();
                return File(file, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
            }
        }

        [HttpGet]
        [Authorize(Roles = "manager, admin, boss")]
        public async Task<ActionResult> GetStaffInfoExcel()
        {
            var obj = await _userInfoManager.GetUsersAsync();
            using (ExcelPackage pkg = new ExcelPackage(new FileInfo($"{Guid.NewGuid()}.xlsx")))
            {
                ExcelWorksheet worksheet = pkg.Workbook.Worksheets.Add("報表1");
                for (int x = 1; x < 10; x++)
                {
                    worksheet.Column(x).Width = 15;
                    worksheet.Column(x).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                }
                worksheet.Cells[1, 1].Value = "員工資料表";
                worksheet.Cells[1, 1, 1, 9].Merge = true;
                worksheet.Cells[1, 1, 1, 9].Style.Font.Bold = true;
                worksheet.Cells[1, 1, 1, 9].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                worksheet.Cells[2, 1].Value = "姓名";
                worksheet.Cells[2, 2].Value = "信箱";
                worksheet.Cells[2, 3].Value = "電話";
                worksheet.Cells[2, 4].Value = "入職日";
                worksheet.Cells[2, 5].Value = "離職日";
                worksheet.Cells[2, 6].Value = "職稱";
                worksheet.Cells[2, 7].Value = "剩餘特休天數";
                worksheet.Cells[2, 8].Value = "剩餘病假天數";
                worksheet.Cells[2, 9].Value = "剩餘家庭照顧假天數";

                for (int i = 0; i < obj.Count; i++)
                {
                    worksheet.Cells[$"A{i + 3}"].Value = obj[i].UserName;
                    worksheet.Cells[$"B{i + 3}"].Value = obj[i].Email;
                    worksheet.Cells[$"C{i + 3}"].Value = obj[i].PhoneNumber;
                    worksheet.Cells[$"D{i + 3}"].Value = obj[i].DateOfEmployment == null
                        ? null
                        : obj[i].DateOfEmployment.Value.ToString("yyyy-MM-dd");
                    worksheet.Cells[$"E{i + 3}"].Value = obj[i].DateOfQuit == null
                        ? null
                        : obj[i].DateOfQuit.Value.ToString("yyyy-MM-dd");
                    worksheet.Cells[$"F{i + 3}"].Value = obj[i].JobTitle;
                    worksheet.Cells[$"G{i + 3}"].Value = obj[i].AnnualLeaves;
                    worksheet.Cells[$"H{i + 3}"].Value = obj[i].SickLeaves;
                    worksheet.Cells[$"I{i + 3}"].Value = obj[i].FamilyCareLeaves;
                }

                byte[] file = pkg.GetAsByteArray();
                return File(file, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
            }
        }

        public class UserModel
        {
            public string Id { get; set; }
            public string UserName { get; set; }

            [EmailAddress]
            public string UserEmail { get; set; }

            public string PhoneNumber { get; set; }
            public string DateOfEmployment { get; set; }
            public string DateOfQuit { get; set; }
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