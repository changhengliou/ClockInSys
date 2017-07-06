using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using ReactSpa.Data;
using ReactSpa.Utils;

namespace ReactSpa.Controllers
{
    [Authorize]
    [Route("api/record/[action]")]
    public class RecordApi : Controller
    {
        private readonly UserManager<UserInfo> _userManager;
        private readonly SignInManager<UserInfo> _signInManager;
        private readonly ILogger _logger;
        private readonly string _externalCookieScheme;
        private readonly UserInfoManager _userInfoManager;
        private readonly RecordManager _recordManager;

        public RecordApi(
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
            _recordManager = new RecordManager(config);
        }

        //
        // Fetch user leave records for display at '/dayOff'
        // GET: /api/record/getInitState
        [HttpGet]
        public async Task<ActionResult> GetInitState(int y, int m)
        {
            try
            {
                var id = User.FindFirstValue(ClaimTypes.NameIdentifier);
                List<OffRecordModel> model =
                    await _recordManager.GetMonthOffRecordsAsync(id, y, m);
                UserInfoModel user = await _userInfoManager.GetUserInfoAsync(id);
                return Json(new
                {
                    payload = new
                    {
                        events = model,
                        a = user.AnnualLeaves,
                        s = user.SickLeaves,
                        f = user.FamilyCareLeaves
                    }
                });
            }
            catch (Exception)
            {
                return StatusCode(404);
            }
        }

        //
        // apply new leave at '/dayOff'
        // POST: /api/record/applyDayOff
        [HttpPost]
        public async Task<ActionResult> ApplyDayOff([FromBody] OffRecordModel model)
        {
            var result = await _recordManager.AddLeaveAsync(User.FindFirstValue(ClaimTypes.NameIdentifier), model);
            List<OffRecordModel> resultModel =
                await _recordManager.GetMonthOffRecordsAsync(User.FindFirstValue(ClaimTypes.NameIdentifier),
                    model.CheckedDate.Year, model.CheckedDate.Month);

            return Json(new
            {
                payload = resultModel
            });
        }

        //
        // cancel leave apply at '/dayOff', '/notification'
        // GET: /api/record/cancelRecord
        [HttpGet]
        public async Task<ActionResult> CancelRecord(string d, bool OT)
        {
            DateTime date;
            DateTime.TryParse(d, out date);
            if (OT)
                await _recordManager.DeleteOTAsync(User.FindFirstValue(ClaimTypes.NameIdentifier), date);
            else
                await _recordManager.DeleteLeaveAsync(User.FindFirstValue(ClaimTypes.NameIdentifier), date);
            List<OffRecordModel> resultModel =
                await _recordManager.GetMonthOffRecordsAsync(User.FindFirstValue(ClaimTypes.NameIdentifier),
                    date.Year, date.Month);
            return Json(new
            {
                payload = resultModel
            });
        }

        [HttpGet]
        public async Task<ActionResult> RemoveRecord(string d)
        {
            DateTime date;
            DateTime.TryParse(d, out date);
            var result = await _recordManager.DeleteLeaveAsync(User.FindFirstValue(ClaimTypes.NameIdentifier), date);
            List<NotificationModel> resultModel =
                await _recordManager.GetNotificationByIdAsync(User.FindFirstValue(ClaimTypes.NameIdentifier));
            return Json(new
            {
                payload = resultModel
            });
        }

        //
        // get pending inquiries at '/notification'
        // POST: /api/record/getInitNotifiedState
        [HttpPost]
        public async Task<ActionResult> GetInitNotifiedState()
        {
            List<NotificationModel> result = new List<NotificationModel>();
            if (User.IsInRole("admin") || User.IsInRole("boss"))
                result = await _recordManager.GetNotificationAsync(User.FindFirstValue(ClaimTypes.NameIdentifier),
                    showAll: true, showDeputy: true, showSupervisor: true);
            else
                result = await _recordManager.GetNotificationAsync(User.FindFirstValue(ClaimTypes.NameIdentifier),
                    showAll: false, showDeputy: true, showSupervisor: true);
            var returnVal = FormatData(result);
            return Json(new
            {
                payload = new
                {
                    data = returnVal.Item1,
                    OTData = returnVal.Item2
                }
            });
        }

        //
        // get self application records at '/notification'
        // POST: /api/record/getSelfNotifiedState
        [HttpPost]
        public async Task<ActionResult> GetSelfNotifiedState()
        {
            var result = await _recordManager.GetNotificationByIdAsync(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var returnVal = new List<Object>();
            foreach (var i in result)
            {
                if (!string.IsNullOrWhiteSpace(i.OvertimeEndTime))
                {
                    returnVal.Add(new
                    {
                        checkedDate = i.CheckedDate,
                        offType = "加班",
                        offTime = $"19:00 - {i.OvertimeEndTime}",
                        offReason = "",
                        statusOfApproval = i.StatusOfApprovalForOvertime
                    });
                }
                if (!string.IsNullOrWhiteSpace(i.OffType))
                {
                    returnVal.Add(new
                    {
                        checkedDate = i.CheckedDate,
                        offType = i.OffType,
                        offTime = i.OffTime,
                        offReason = i.OffReason,
                        statusOfApproval = i.StatusOfApproval
                    });
                }
            }
            return Json(new
            {
                payload = returnVal
            });
        }

        //
        // set notification status at '/notification'
        // POST: /api/record/setNotification
        [HttpPost]
        public async Task<ActionResult> SetNotification([FromBody] SetNotificationModel m)
        {
            if (await _recordManager.SetStatusOfApprovalAsync(m.RecordId, m.Status))
            {
                List<NotificationModel> result = new List<NotificationModel>();
                if (User.IsInRole("admin") || User.IsInRole("boss"))
                    result = await _recordManager.GetNotificationAsync(User.FindFirstValue(ClaimTypes.NameIdentifier),
                        showAll: true, showDeputy: true, showSupervisor: true);
                else
                    result = await _recordManager.GetNotificationAsync(User.FindFirstValue(ClaimTypes.NameIdentifier),
                        showAll: false, showDeputy: true, showSupervisor: true);
                var returnVal = FormatData(result);
                return Json(new
                {
                    payload = new
                    {
                        data = returnVal.Item1,
                        OTData = returnVal.Item2
                    }
                });
            }
            return Json(new {payload = new List<NotificationModel>(), status = false});
        }

        //
        // set Overtime status status at '/notification'
        // POST: /api/record/setOTStatus
        [HttpPost]
        public async Task<ActionResult> SetOTStatus([FromBody] SetNotificationModel m)
        {
            if (await _recordManager.SetStatusOfApprovalAsync(m.RecordId, m.Status, OT: true))
            {
                List<NotificationModel> result = new List<NotificationModel>();
                if (User.IsInRole("admin") || User.IsInRole("boss"))
                    result = await _recordManager.GetNotificationAsync(User.FindFirstValue(ClaimTypes.NameIdentifier),
                        showAll: true, showDeputy: true, showSupervisor: true);
                else
                    result = await _recordManager.GetNotificationAsync(User.FindFirstValue(ClaimTypes.NameIdentifier),
                        showAll: false, showDeputy: true, showSupervisor: true);
                var returnVal = FormatData(result);
                return Json(new
                {
                    payload = new
                    {
                        data = returnVal.Item1,
                        OTData = returnVal.Item2
                    }
                });
            }
            return Json(new {payload = new List<NotificationModel>(), status = false});
        }

        //
        // query all records by time and id at '/report'
        // POST: /api/record/query
        [HttpPost]
        [Authorize(Roles = "boss, manager, admin")]
        public async Task<ActionResult> Query([FromBody] QueryModel model)
        {
            var result = await QueryRecord(model);
            var nullVal = new
            {
                model = result,
                a = 0,
                s = 0,
                f = 0
            };
            if (string.IsNullOrWhiteSpace(model.Id))
                return Json(new {payload = nullVal});

            UserInfo user = await _userManager.FindByIdAsync(model.Id);
            if (user == null)
                return Json(new {payload = nullVal});

            return Json(new
            {
                payload = new
                {
                    model = result,
                    a = user.AnnualLeaves,
                    s = user.SickLeaves,
                    f = user.FamilyCareLeaves
                }
            });
        }

        //
        // query all records by time and id at '/report'
        // POST: /api/record/query
        [HttpGet]
        [Authorize(Roles = "boss, manager, admin")]
        public async Task<ActionResult> Query(string id)
        {
            UserInfo user = await _userManager.FindByIdAsync(id);
            if (user == null)
                return Json(new {payload = new {a = 0, s = 0, f = 0}});
            return Json(new
            {
                payload = new
                {
                    a = user.AnnualLeaves,
                    s = user.SickLeaves,
                    f = user.FamilyCareLeaves
                }
            });
        }

        //
        // insert or update record at '/report'
        // POST: /api/record/editRecord
        [HttpPost]
        [Authorize(Roles = "boss, manager, admin")]
        public async Task<ActionResult> EditRecord([FromBody] EditRecordModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest();
            await _recordManager.InsertOrUpdateRecordAsync(model);
            var result = await QueryRecord(new QueryModel
            {
                FromDate = model.FromDate,
                Id = model.Id,
                Options = model.Options,
                RecordId = model.RecordId,
                ToDate = model.ToDate
            });
            return Json(new {payload = result, model});
        }

        //
        // drop record at '/report'
        // POST: /api/record/deleteRecord
        [HttpPost]
        [Authorize(Roles = "boss, manager, admin")]
        public async Task<ActionResult> DeleteRecord([FromBody] QueryModel model)
        {
            await _recordManager.DeleteRecordByRecordIdAsync(model.RecordId);
            var result = await QueryRecord(model);
            return Json(new {payload = result});
        }

        //
        // This is not an open API!
        // function used by /api/record/deleteRecord and /api/record/editRecord
        public async Task<List<ReportModel>> QueryRecord(QueryModel model)
        {
            var options = RecordOptions.SelectAll;
            if (model.Options == "1")
                options = RecordOptions.SelectNormal;
            else if (model.Options == "2")
                options = RecordOptions.SelectLeave;
            else if (model.Options == "3")
                options = RecordOptions.SelectOT;
            else if (model.Options == "4")
            {
                IList<UserInfo> excludedList = await _userManager.GetUsersInRoleAsync("boss");
                await _recordManager.AutomaticAddRecordAsync(model.FromDate, excludeFrom: excludedList);
            }
            else if (model.Options != "0")
                throw new Exception("Invalid Options");
            return await _recordManager.GetRecordsAsync(model.Id, model.FromDate, model.ToDate, options);
        }

        //
        // export query result and export as excel XLSX file at '/report'
        // GET: /api/record/exportXlsx
        [HttpGet]
        [Authorize(Roles = "boss, manager, admin")]
        public async Task<ActionResult> ExportXlsx(string a, string b, string c, string d)
        {
            try
            {
                DateTime from = DateTime.Parse(c);
                DateTime to = DateTime.Parse(d);
                var result = await QueryRecord(new QueryModel
                {
                    Id = a,
                    Options = b,
                    FromDate = from,
                    ToDate = to
                });

                using (ExcelPackage pkg = new ExcelPackage(new FileInfo($"{Guid.NewGuid()}.xlsx")))
                {
                    ExcelWorksheet worksheet = pkg.Workbook.Worksheets.Add("報表1");
                    for (int x = 1; x < 10; x++)
                    {
                        worksheet.Column(x).Width = 15;
                        worksheet.Column(x).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    }
                    worksheet.Cells[1, 1].Value = $"{c} - {d} 出勤狀況表";
                    worksheet.Cells[1, 1, 1, 11].Merge = true;
                    worksheet.Cells[1, 1, 1, 11].Style.Font.Bold = true;
                    worksheet.Cells[1, 1, 1, 11].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    worksheet.Cells[2, 1].Value = "日期";
                    worksheet.Cells[2, 2].Value = "姓名";
                    worksheet.Cells[2, 3].Value = "上班時間";
                    worksheet.Cells[2, 4].Value = "下班時間";
                    worksheet.Cells[2, 5].Value = "上班打卡座標";
                    worksheet.Cells[2, 6].Value = "下班打卡座標";
                    worksheet.Cells[2, 7].Value = "加班時間";
                    worksheet.Cells[2, 8].Value = "請假申請日期";
                    worksheet.Cells[2, 9].Value = "請假類別";
                    worksheet.Cells[2, 10].Value = "請假時間";
                    worksheet.Cells[2, 11].Value = "請假原因";

                    for (int i = 0; i < result.Count; i++)
                    {
                        if (result[i].StatusOfApproval == StatusOfApprovalEnum.APPROVED())
                        {
                            worksheet.Cells[$"H{i + 3}"].Value = result[i].OffApplyDate;
                            worksheet.Cells[$"I{i + 3}"].Value = result[i].OffType;
                            worksheet.Cells[$"J{i + 3}"].Value = $"{result[i].OffTimeStart} - {result[i].OffTimeEnd}";
                            worksheet.Cells[$"K{i + 3}"].Value = result[i].OffReason;
                        }
                        else if (string.IsNullOrWhiteSpace(result[i].CheckInTime) &&
                                 string.IsNullOrWhiteSpace(result[i].CheckOutTime))
                            continue;
                        worksheet.Cells[$"A{i + 3}"].Value = result[i].CheckedDate;
                        worksheet.Cells[$"B{i + 3}"].Value = result[i].UserName;
                        worksheet.Cells[$"C{i + 3}"].Value = result[i].CheckInTime;
                        worksheet.Cells[$"D{i + 3}"].Value = result[i].CheckOutTime;
                        worksheet.Cells[$"E{i + 3}"].Value = result[i].GeoLocation1;
                        worksheet.Cells[$"F{i + 3}"].Value = result[i].GeoLocation2;
                        if(result[i].StatusOfApprovalForOvertime == StatusOfApprovalEnum.APPROVED())
                            worksheet.Cells[$"G{i + 3}"].Value = result[i].OvertimeEndTime;
                    }

                    byte[] file = pkg.GetAsByteArray();
                    return File(file, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
                }
            }
            catch (ArgumentNullException)
            {
                return BadRequest("Invalid parameters.");
            }
            catch (FormatException)
            {
                return BadRequest("Invalid parameters.");
            }
        }

        //
        // get init absent records from everyone for display at '/absent'
        // GET: /api/record/getAbsentStatus
        [HttpGet]
        public async Task<ActionResult> GetAbsentStatus(string y, string m)
        {
            var result = await _recordManager.GetMonthlyOffRecordAsync(y, m);
            return Json(new {payload = result});
        }

        public Tuple<List<Object>, List<Object>> FormatData(List<NotificationModel> obj)
        {
            var data = new List<Object>();
            var ot = new List<Object>();
            foreach (var i in obj)
            {
                if (!string.IsNullOrWhiteSpace(i.OffType))
                    data.Add(new
                    {
                        id = i.Id,
                        userName = i.UserName,
                        checkedDate = i.CheckedDate,
                        offType = i.OffType,
                        offTime = i.OffTime,
                        offReason = i.OffReason,
                        statusOfApproval = i.StatusOfApproval
                    });
                if (!string.IsNullOrWhiteSpace(i.OvertimeEndTime))
                    ot.Add(new
                    {
                        id = i.Id,
                        userName = i.UserName,
                        checkedDate = i.CheckedDate,
                        overtimeEndTime = i.OvertimeEndTime,
                        statusOfApprovalForOvertime = i.StatusOfApprovalForOvertime
                    });
            }
            return Tuple.Create(data, ot);
        }
    }

    public class SetNotificationModel
    {
        public string RecordId { get; set; }
        public string Status { get; set; }
    }

    public class QueryModel
    {
        public string RecordId { get; set; }
        public string Id { get; set; }
        public string Options { get; set; }

        [DataType(DataType.Date)]
        public DateTime FromDate { get; set; }

        [DataType(DataType.Date)]
        public DateTime ToDate { get; set; }
    }

    public class EditRecordModel
    {
        public string UserId { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime CheckedDate { get; set; }

        [DataType((DataType.Time))]
        public TimeSpan? CheckInTime { get; set; }

        [DataType((DataType.Time))]
        public TimeSpan? CheckOutTime { get; set; }

        public string GeoLocation1 { get; set; }
        public string GeoLocation2 { get; set; }
        public string OvertimeEndTime { get; set; }
        public string OffType { get; set; }

        [DataType((DataType.Time))]
        public TimeSpan? OffTimeStart { get; set; }

        [DataType((DataType.Time))]
        public TimeSpan? OffTimeEnd { get; set; }

        public string OffReason { get; set; }
        public string StatusOfApproval { get; set; }
        public string RecordId { get; set; }
        public string Id { get; set; }
        public string Options { get; set; }

        [DataType(DataType.Date)]
        public DateTime FromDate { get; set; }

        [DataType(DataType.Date)]
        public DateTime ToDate { get; set; }
    }
}