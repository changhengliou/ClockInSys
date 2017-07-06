using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using ReactSpa.Data;

namespace ReactSpa.Controllers
{
    [Authorize]
    [Route("api/home/[action]")]
    public class HomeApiController : Controller
    {
        private readonly RecordManager _recordManager;

        public HomeApiController(IOptions<ConnectionInfo> config)
        {
            _recordManager = new RecordManager(config);
        }

        //
        // Fetch init data for checkIn at '/'
        // POST: /api/home/getInitState
        [HttpPost]
        public async Task<ActionResult> GetInitState()
        {
            var record = await _recordManager.GetRecordOfToday(User.FindFirstValue(ClaimTypes.NameIdentifier));
            if (record != null)
            {
                bool shouldCheckInDisable = true, shouldCheckOutDisable = true;
                string offStatus = null;
                TimeSpan timeStart, timeEnd;
                if (record.StatusOfApproval == StatusOfApprovalEnum.APPROVED())
                {
                    if (record.OffTimeStart != null)
                        timeStart = record.OffTimeStart.Value;
                    if (record.OffTimeEnd != null)
                        timeEnd = record.OffTimeEnd.Value;
                    offStatus = $"{timeStart.ToString("h'時'")}-{timeEnd.ToString("h'時'")} {record.OffType}";
                }

                if (record.CheckInTime == null)
                {
                    shouldCheckInDisable = false;
                }
                if (record.CheckOutTime == null)
                {
                    shouldCheckOutDisable = false;
                }
                return
                    Json(
                        new
                        {
                            status = true,
                            payload = new
                            {
                                shouldCheckInDisable = shouldCheckInDisable,
                                shouldCheckOutDisable = shouldCheckOutDisable,
                                currentDate = DateTime.Today.ToString("yyyy年MM月dd日 (ddd)"),
                                currentTime = DateTime.Now.ToString("hh:mm:ss tt"),
                                checkIn = record.CheckInTime,
                                checkOut = record.CheckOutTime,
                                offStatus = offStatus
                            }
                        }
                    );
            }
            return Json(new
            {
                status = true,
                payload = new
                {
                    shouldCheckInDisable = false,
                    shouldCheckOutDisable = false,
                    currentDate = DateTime.Today.ToString("yyyy年MM月dd日 (ddd)"),
                    currentTime = DateTime.Now.ToString("h:mm:ss tt")
                }
            });
        }

        //
        // checkIn '/'
        // POST: /api/home/proceedCheckIn
        [HttpPost]
        public async Task<ActionResult> ProceedCheckIn([FromBody] CheckingModel model)
        {
            var result = await _recordManager.CheckInAsync(User.FindFirstValue(ClaimTypes.NameIdentifier), model.Geo);
            if (result != null)
                return
                    Json(
                        new
                        {
                            status = true,
                            payload = new
                            {
                                checkIn = result,
                                shouldCheckInDisable = true
                            }
                        });
            return Json(new {status = false, payload = new {}});
        }

        //
        // checkOut '/'
        // POST: /api/home/proceedCheckOut
        [HttpPost]
        public async Task<ActionResult> ProceedCheckOut([FromBody] CheckingModel model)
        {
            var result = await _recordManager.CheckOutAsync(User.FindFirstValue(ClaimTypes.NameIdentifier), model.Geo);
            if (result != null)
                return
                    Json(
                        new
                        {
                            status = true,
                            payload = new
                            {
                                checkOut = result,
                                shouldCheckOutDisable = true
                            }
                        });
            return Json(new {status = false, payload = new {}});
        }

        //
        // apply overtime working '/'
        // GET: /api/home/OTsubmit
        public async Task<ActionResult> OTsubmit(DateTime d, string t)
        {
            await _recordManager.AddOTAsync(User.FindFirstValue(ClaimTypes.NameIdentifier), d, t);
            return Json(new {d, t});
        }

        public class CheckingModel
        {
            public string Geo { get; set; }
        }
    }
}