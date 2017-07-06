using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using OfficeOpenXml;
using ReactSpa.Data;
using ReactSpa.Extension;

namespace ReactSpa.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly UserManager<UserInfo> _userManager;
        private readonly RecordManager _recordManager;

        public HomeController(UserManager<UserInfo> userManager, IOptions<ConnectionInfo> config)
        {
            _userManager = userManager;
            _recordManager = new RecordManager(config);
        }

        public async Task<IActionResult> Index()
        {
            var user =
                await _userManager.Users.FirstOrDefaultAsync(s => s.Id == User.FindFirstValue(ClaimTypes.NameIdentifier));
            var roles = await _userManager.GetRolesAsync(user);

            return View(roles);
        }

        public IActionResult Error()
        {
            return View();
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Upload()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        public IActionResult Upload(IFormFile file)
        {
            if (file.Length > 3000000)
                return BadRequest("File is too large.");
            if (file.ContentType == "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")
            {
                using (var stream = file.OpenReadStream())
                {
                    ExcelPackage pkg = new ExcelPackage(stream);
                    try
                    {
                        IEnumerable<ExcelDto> objs =
                            pkg.Workbook.Worksheets[1].MapSheetToObjects<ExcelDto>(numOfRowSkips: 2, takeRows: 500);

                        return Json(new {payload = _recordManager.MapDtoWithId(objs.ToList())});
                    }
                    catch (Exception)
                    {
                        return BadRequest("File is corrupted.");
                    }
                }
            }
            return BadRequest("Expect an Excel 'xlsx' worksheet file.");
        }

        [HttpPost]
        public IActionResult BulkyUpdateDatabase([FromBody] BulkyUpdateModel list)
        {
            var s = _recordManager.BulkyUpdate(list.List);
            return Json(new {newCount = s.Item1, count = s.Item2});
        }
    }

    public class ExcelDto
    {
        public string Id { get; set; }

        public bool IsDataValid { get; set; }

        public bool IsInValid { get; set; }

        public bool IsOutValid { get; set; }

        public bool IsOTValid { get; set; }

        public bool IsOffValid { get; set; }

        [Date(DateType.Date)]
        [Column(1)]
        public string CheckedDate { get; set; }

        [Column(2)]
        public string UserName { get; set; }

        [Date(DateType.Time)]
        [Column(3)]
        public string CheckInTime { get; set; }

        [Date(DateType.Time)]
        [Column(4)]
        public string CheckOutTime { get; set; }

        [Column(5)]
        public string GeoLocation1 { get; set; }

        [Column(6)]
        public string GeoLocation2 { get; set; }

        [Date(DateType.Time)]
        [Column(7)]
        public string OvertimeEndTime { get; set; }

        [Date(DateType.Date)]
        [Column(8)]
        public string OffApplyDate { get; set; }

        [Column(9)]
        public string OffType { get; set; }

        [Column(10)]
        public string OffTime { get; set; }

        [Column(11)]
        public string OffReason { get; set; }
    }

    public class BulkyUpdateModel
    {
        public List<ExcelDto> List { get; set; }
    }
}