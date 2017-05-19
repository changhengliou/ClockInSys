using System;
using System.Collections.Generic;
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

        [HttpPost]
        public async Task<ActionResult> GetInitNotifiedState()
        {
            return Json(new {});
        }
    }
}