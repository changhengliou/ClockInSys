using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Linq;
using System.Security.Claims;
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

        [HttpGet]
        public async Task<ActionResult> GetInitState(int y, int m)
        {
            ;
            try
            {
                List<OffRecordModel> model =
                    await _recordManager.GetMonthOffRecordsAsync(User.FindFirstValue(ClaimTypes.NameIdentifier), y, m);
                return Json(new
                {
                    payload = model
                });
            }
            catch (Exception)
            {
                return StatusCode(404);
            }
        }

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

        [HttpGet]
        public async Task<ActionResult> CancelDayOff(string d)
        {
            DateTime date;
            DateTime.TryParse(d, out date);
            var result = await _recordManager.DeleteLeaveAsync(User.FindFirstValue(ClaimTypes.NameIdentifier), date);
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

        /**bug options: showall for role, show deputy and show supervisor */
        /**bug options: set display date range */
        [HttpPost]
        public async Task<ActionResult> GetInitNotifiedState()
        {
            var result = await _recordManager.GetNotificationAsync(User.FindFirstValue(ClaimTypes.NameIdentifier),
                showAll: false, showDeputy: true, showSupervisor: true);
            return Json(new {payload = result});
        }

        [HttpPost]
        public async Task<ActionResult> GetSelfNotifiedState()
        {
            var result = await _recordManager.GetNotificationByIdAsync(User.FindFirstValue(ClaimTypes.NameIdentifier));
            return Json(new {payload = result});
        }

        [HttpPost]
        public async Task<ActionResult> SetNotification([FromBody] SetNotificationModel m)
        {
            var result = await _recordManager.SetStatusOfApprovalAsync(m.RecordId, m.Status);
            if (result)
            {
                List<NotificationModel> resultModel = await _recordManager.GetNotificationByIdAsync(
                    User.FindFirstValue(ClaimTypes.NameIdentifier));
                return Json(new {payload = resultModel});
            }
            return Json(new {payload = new List<NotificationModel>(), status = false});
        }

        [HttpPost]
        public async Task<ActionResult> Query([FromBody] QueryModel model)
        {
            return Json(new { model });
        }
    }

    public class SetNotificationModel
    {
        public string RecordId { get; set; }
        public string Status { get; set; }
    }

    public class QueryModel
    {
        public string Id  { get; set; }
        public string Options { get; set; }
        
        [DataType(DataType.Date)]
        public DateTime FromDate { get; set; }
        
        [DataType(DataType.Date)]
        public DateTime ToDate { get; set; }
    }
}