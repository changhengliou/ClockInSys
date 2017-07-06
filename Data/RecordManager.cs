using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using OfficeOpenXml.FormulaParsing.Excel.Functions.DateTime;
using ReactSpa.Controllers;

namespace ReactSpa.Data
{
    public class RecordManager
    {
        private readonly DbContextOptionsBuilder<AppDbContext> builder = new DbContextOptionsBuilder<AppDbContext>();

        public RecordManager(IOptions<ConnectionInfo> configuration)
        {
            var config = configuration.Value;
            builder.UseSqlServer(config.LocalSQLServer);
        }

        public async Task<CheckRecord> GetRecordOfToday(string id)
        {
            using (var dbContext = new AppDbContext(builder.Options))
            {
                var now = DateTime.Today;
                var record = await dbContext.CheckRecord.FirstOrDefaultAsync(s => s.CheckedDate == now && s.UserId == id);
                return record;
            }
        }

        public async Task<TimeSpan?> CheckInAsync(string id, string geo)
        {
            using (var dbContext = new AppDbContext(builder.Options))
            {
                var now = DateTime.Now.TimeOfDay;
                try
                {
                    var record =
                        await dbContext.CheckRecord.FirstOrDefaultAsync(
                            s => s.CheckedDate == DateTime.Today && s.UserId == id);
                    if (record != null)
                    {
                        record.CheckInTime = now;
                        record.GeoLocation1 = geo;
                        dbContext.Entry(record).State = EntityState.Modified;
                    }
                    else
                    {
                        await dbContext.CheckRecord.AddAsync(new CheckRecord
                        {
                            UserId = id,
                            CheckedDate = DateTime.Today,
                            CheckInTime = now,
                            GeoLocation1 = geo
                        });
                    }
                    await dbContext.SaveChangesAsync();
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e.ToString());
                    return null;
                }
                return now;
            }
        }

        public async Task<TimeSpan?> CheckOutAsync(string id, string geo)
        {
            using (var dbContext = new AppDbContext(builder.Options))
            {
                var now = DateTime.Now.TimeOfDay;
                var record =
                    await dbContext.CheckRecord.FirstOrDefaultAsync(s => s.UserId == id && s.CheckedDate == DateTime.Today);
                if (record == null)
                {
                    await dbContext.CheckRecord.AddAsync(new CheckRecord
                    {
                        UserId = id,
                        CheckedDate = DateTime.Today,
                        CheckOutTime = now,
                        GeoLocation2 = geo
                    });
                }
                else
                {
                    record.CheckOutTime = now;
                    record.GeoLocation2 = geo;
                }
                
                await dbContext.SaveChangesAsync();
                return now;
            }
        }

        public async Task AddOTAsync(string id, DateTime d, string t)
        {
            using (var dbContext = new AppDbContext(builder.Options))
            {
                var record = await dbContext.CheckRecord.FirstOrDefaultAsync(s => s.UserId == id && s.CheckedDate == d);
                if (record != null)
                {
                    record.OvertimeEndTime = t;
                    record.StatusOfApprovalForOvertime = StatusOfApprovalEnum.PENDING();
                }
                else
                {
                    await dbContext.CheckRecord.AddAsync(new CheckRecord
                    {
                        UserId = id,
                        CheckedDate = d,
                        OvertimeEndTime = t,
                        StatusOfApprovalForOvertime = StatusOfApprovalEnum.PENDING(),
                    });
                }
                await dbContext.SaveChangesAsync();
            }
        }

        public async Task<List<OffRecordModel>> GetMonthOffRecordsAsync(string id, int year, int month)
        {
            using (var dbContext = new AppDbContext(builder.Options))
            {
                var now = new DateTime(year, month, 1);
                var firstDayOfMonth = new DateTime(now.Year, now.Month, 1);
                var lastDayOfMonth = firstDayOfMonth.AddMonths(1).AddSeconds(-1);
                var result = await dbContext.CheckRecord.Where(s =>
                        s.UserId == id &&
                        s.OffType != null &&
                        s.CheckedDate >= firstDayOfMonth &&
                        s.CheckedDate <= lastDayOfMonth)
                    .OrderBy(s => s.CheckedDate)
                    .Select(s => new OffRecordModel
                    {
                        CheckedDate = s.CheckedDate,
                        OffType = s.OffType,
                        OffTimeStart = s.OffTimeStart,
                        OffTimeEnd = s.OffTimeEnd,
                        OffEndDate = s.CheckedDate,
                        OffReason = s.OffReason,
                        StatusOfApproval = s.StatusOfApproval
                    }).ToListAsync();
                return result;
            }
        }


        public async Task<bool> AddLeaveAsync(string id, OffRecordModel model)
        {
            using (var dbContext = new AppDbContext(builder.Options))
            {
                dbContext.ChangeTracker.AutoDetectChangesEnabled = false;
                var recordList = new List<CheckRecord>();
                var records = dbContext.CheckRecord.Where(
                    s => s.UserId == id &&
                         s.CheckedDate >= model.CheckedDate &&
                         s.CheckedDate <= model.OffEndDate);
                for (var i = model.CheckedDate.Date;
                    model.OffEndDate != null && i <= model.OffEndDate.Value.Date;
                    i = i.AddDays(1))
                {
                    var record = records.FirstOrDefault(s => s.CheckedDate.Date == i.Date);
                    if (record == null)
                    {
                        recordList.Add(new CheckRecord
                        {
                            UserId = id,
                            CheckedDate = i,
                            OffEndDate = model.OffEndDate,
                            OffType = model.OffType,
                            OffReason = model.OffReason,
                            OffTimeStart = model.OffTimeStart,
                            OffTimeEnd = model.OffTimeEnd,
                            OffApplyDate = DateTime.Today
                        });
                    }
                    else
                    {
                        record.UserId = id;
                        record.OffEndDate = model.OffEndDate;
                        record.OffTimeStart = model.OffTimeStart;
                        record.OffTimeEnd = model.OffTimeEnd;
                        record.OffReason = model.OffReason;
                        record.OffType = model.OffType;
                        record.StatusOfApproval = StatusOfApprovalEnum.PENDING();
                        record.OffApplyDate = DateTime.Today;
                        dbContext.Entry(record).State = EntityState.Modified;
                    }
                }
                await dbContext.CheckRecord.AddRangeAsync(recordList);

                await dbContext.SaveChangesAsync();
                return true;
            }
        }

        public async Task DeleteOTAsync(string id, DateTime date)
        {
            using (var dbContext = new AppDbContext(builder.Options))
            {
                dbContext.ChangeTracker.AutoDetectChangesEnabled = false;
                var result =
                    await dbContext.CheckRecord.FirstOrDefaultAsync(s => s.UserId == id && s.CheckedDate == date);
                if (result == null)
                    return;
                if (result.CheckInTime == null && result.CheckOutTime == null &&
                    string.IsNullOrWhiteSpace(result.OvertimeEndTime))
                {
                    dbContext.Entry(result).State = EntityState.Deleted;
                }
                else
                {
                    result.OvertimeEndTime = null;
                    result.StatusOfApprovalForOvertime = StatusOfApprovalEnum.PENDING();
                    dbContext.Entry(result).State = EntityState.Modified;
                }
                await dbContext.SaveChangesAsync();
            }
        }

        public async Task<bool> DeleteLeaveAsync(string id, DateTime date)
        {
            using (var dbContext = new AppDbContext(builder.Options))
            {
                dbContext.ChangeTracker.AutoDetectChangesEnabled = false;
                var result =
                    await dbContext.CheckRecord.FirstOrDefaultAsync(s => s.UserId == id && s.CheckedDate == date);
                if (result == null)
                    return false;
                if (result.StatusOfApproval == StatusOfApprovalEnum.APPROVED())
                {
                    var user = await dbContext.UserInfo.FirstOrDefaultAsync(s => s.Id == result.UserId);
                    user = TypeEnum.CalcLeaves(user, result.OffTimeStart, result.OffTimeEnd,
                        result.OffType, inverseOp: true);
                    dbContext.Entry(user).State = EntityState.Modified;
                }
                if (result.CheckInTime == null && result.CheckOutTime == null &&
                    string.IsNullOrWhiteSpace(result.OvertimeEndTime))
                {
                    dbContext.Entry(result).State = EntityState.Deleted;
                }
                else
                {
                    result.OffApplyDate = null;
                    result.OffTimeStart = null;
                    result.OffTimeEnd = null;
                    result.OffEndDate = null;
                    result.OffReason = null;
                    result.OffType = null;
                    result.StatusOfApproval = StatusOfApprovalEnum.PENDING();
                    dbContext.Entry(result).State = EntityState.Modified;
                }
                await dbContext.SaveChangesAsync();
                return true;
            }
        }

        public async Task<List<NameListModel>> GetDeputiesAsync(string id)
        {
            using (var dbContext = new AppDbContext(builder.Options))
            {
                var deputies = await (from user in dbContext.UserInfo
                    join deputy in dbContext.UserDeputy on user.Id equals deputy.UserId
                    where user.Id == id
                    select new NameListModel {Label = user.UserName, Value = user.Id}).ToListAsync();
                return deputies;
            }
        }

        public async Task<List<NameListModel>> GetSupervisorsAsync(string id)
        {
            using (var dbContext = new AppDbContext(builder.Options))
            {
                var supervisors = await (from user in dbContext.UserInfo
                    join supervisor in dbContext.UserSupervisor on user.Id equals supervisor.UserId
                    where user.Id == id
                    select new NameListModel {Label = user.UserName, Value = user.Id}).ToListAsync();
                return supervisors;
            }
        }

        /**
         * @params showAll, show all other people's checkRecord
         * @params showDeputy, only show if user is other people's deputy
         * @params showSupervisor, only show if user is other people's supervisor
         * @params range, only shows records after today - spacific days
         */

        public async Task<List<NotificationModel>> GetNotificationAsync(string id, bool showAll = false,
            bool showDeputy = false, bool showSupervisor = false, int range = -7)
        {
            using (var dbContext = new AppDbContext(builder.Options))
            {
                DateTime dateBound = DateTime.Today.AddDays(range);
                if (showAll)
                {
                    return await (from record in dbContext.CheckRecord
                        join user in dbContext.UserInfo on record.UserId equals user.Id
                        where record.CheckedDate > dateBound &&
                              (!string.IsNullOrWhiteSpace(record.OffType) ||
                               !string.IsNullOrWhiteSpace(record.OvertimeEndTime))
                        select new NotificationModel
                        {
                            Id = record.Id,
                            UserName = user.UserName,
                            CheckedDate = record.CheckedDate.ToString("yyyy-MM-dd"),
                            OffType = record.OffType,
                            OffTime = $"{record.OffTimeStart} - {record.OffTimeEnd}",
                            OffReason = record.OffReason,
                            StatusOfApproval = record.StatusOfApproval,
                            OvertimeEndTime = record.OvertimeEndTime,
                            StatusOfApprovalForOvertime = record.StatusOfApprovalForOvertime
                        }).ToListAsync();
                }
                List<NameListModel> userDeputy = new List<NameListModel>(), userSupervisor = new List<NameListModel>();
                if (showDeputy)
                {
                    userDeputy = await (from deputy in dbContext.UserDeputy
                        join user in dbContext.UserInfo on deputy.UserId equals user.Id
                        where deputy.DeputyId == id
                        select new NameListModel {Label = user.UserName, Value = user.Id}).ToListAsync();
                }
                if (showSupervisor)
                {
                    userSupervisor = await (from supervisor in dbContext.UserSupervisor
                        join user in dbContext.UserInfo on supervisor.UserId equals user.Id
                        where supervisor.SupervisorId == id
                        select new NameListModel {Label = user.UserName, Value = user.Id}).ToListAsync();
                }
                var ulist = userDeputy.Union(userSupervisor).ToList();
                var result = await dbContext.CheckRecord
                    .Where(s => ulist.Any(w => w.Value == s.UserId) &&
                                s.CheckedDate > dateBound &&
                                (!string.IsNullOrWhiteSpace(s.OffType) ||
                                 !string.IsNullOrWhiteSpace(s.OvertimeEndTime)))
                    .Select(s => new NotificationModel
                    {
                        Id = s.Id,
                        UserName = ulist.FirstOrDefault(z => z.Value == s.UserId).Label,
                        CheckedDate = s.CheckedDate.ToString("yyyy-MM-dd"),
                        OffType = s.OffType,
                        OffTime = $"{s.OffTimeStart.ToString()} - {s.OffTimeEnd.ToString()}",
                        OffReason = s.OffReason,
                        OvertimeEndTime = s.OvertimeEndTime,
                        StatusOfApproval = s.StatusOfApproval,
                        StatusOfApprovalForOvertime = s.StatusOfApprovalForOvertime
                    })
                    .ToListAsync();
                return result;
            }
        }

        public async Task<List<NotificationModel>> GetNotificationByIdAsync(string id, int range = -7)
        {
            using (var dbContext = new AppDbContext(builder.Options))
            {
                var dateBound = DateTime.Today.AddDays(range);
                var result = await dbContext.CheckRecord
                    .Where(s => (!string.IsNullOrWhiteSpace(s.OffType) ||
                                 !string.IsNullOrWhiteSpace(s.OvertimeEndTime)) &&
                                s.CheckedDate > dateBound &&
                                s.UserId == id)
                    .Select(s => new NotificationModel
                    {
                        CheckedDate = s.CheckedDate.ToString("yyyy-MM-dd"),
                        OffType = s.OffType,
                        OffTime = $"{s.OffTimeStart} - {s.OffTimeEnd}",
                        OffReason = s.OffReason,
                        OvertimeEndTime = s.OvertimeEndTime,
                        StatusOfApproval = s.StatusOfApproval,
                        StatusOfApprovalForOvertime = s.StatusOfApprovalForOvertime
                    }).ToListAsync();
                return result;
            }
        }

        public async Task<bool> SetStatusOfApprovalAsync(string recordId, string status, bool OT = false)
        {
            using (var dbContext = new AppDbContext(builder.Options))
            {
                try
                {
                    dbContext.ChangeTracker.AutoDetectChangesEnabled = false;
                    var record = await dbContext.CheckRecord.FirstOrDefaultAsync(s => s.Id == recordId);
                    if (record == null)
                        return false;
                    if (status == StatusOfApprovalEnum.APPROVED())
                    {
                        if (OT)
                        {
                            record.StatusOfApprovalForOvertime = status;
                        }
                        else
                        {
                            var user = await dbContext.UserInfo.FirstOrDefaultAsync(s => s.Id == record.UserId);
                            user = TypeEnum.CalcLeaves(user, record.OffTimeStart, record.OffTimeEnd, record.OffType);
                            if (user == null)
                            {
                                record.StatusOfApproval = StatusOfApprovalEnum.REJECTED();
                            }
                            else
                            {
                                dbContext.Entry(user).State = EntityState.Modified;
                                record.StatusOfApproval = status;
                            }
                        }
                    }
                    else
                    {
                        if (OT)
                        {
                            record.StatusOfApprovalForOvertime = status;
                        }
                        else
                            record.StatusOfApproval = status;
                    }
                    dbContext.Entry(record).State = EntityState.Modified;
                    await dbContext.SaveChangesAsync();
                    return true;
                }
                catch (Exception)
                {
                    return false;
                }
            }
        }

        /** @params id, if null, select all 
          * @parmas from, begining time
          * @params to, ending selected time
          * @params options,
        */

        public async Task<List<ReportModel>> GetRecordsAsync(string id, DateTime fromT, DateTime toT,
            RecordOptions option = RecordOptions.SelectAll,
            bool includePending = false)
        {
            using (var dbContext = new AppDbContext(builder.Options))
            {
                IQueryable<ReportModel> records = from record in dbContext.CheckRecord
                    join user in dbContext.UserInfo on
                    record.UserId equals user.Id
                    where record.CheckedDate >= fromT.Date && record.CheckedDate <= toT.Date
                    select new ReportModel
                    {
                        UserId = user.Id,
                        UserName = user.UserName,
                        RecordId = record.Id,
                        CheckedDate = record.CheckedDate.ToString("yyyy-MM-dd"),
                        CheckInTime = record.CheckInTime.ToString(),
                        CheckOutTime = record.CheckOutTime.ToString(),
                        GeoLocation1 = record.GeoLocation1,
                        GeoLocation2 = record.GeoLocation2,
                        OffApplyDate = record.OffApplyDate.Value == null
                            ? ""
                            : record.OffApplyDate.Value.ToString("yyyy-MM-dd"),
                        OffReason = record.OffReason,
                        OffTimeStart = record.OffTimeStart.ToString(),
                        OffTimeEnd = record.OffTimeEnd.ToString(),
                        OffType = string.IsNullOrWhiteSpace(record.OffType) ? "" : record.OffType,
                        OvertimeEndTime = record.OvertimeEndTime,
                        StatusOfApprovalForOvertime = record.StatusOfApprovalForOvertime,
                        StatusOfApproval = record.StatusOfApproval
                    };
                if (!string.IsNullOrWhiteSpace(id))
                {
                    records = records.Where(s => s.UserId == id);
                }
                switch (option)
                {
                    case RecordOptions.SelectAll:
                        return await records.ToListAsync();
                    case RecordOptions.SelectLeave:
                        return await records.Where(s => !string.IsNullOrWhiteSpace(s.OffType)).ToListAsync();
                    case RecordOptions.SelectNormal:
                        return await records.Where(s => !string.IsNullOrWhiteSpace(s.CheckInTime)).ToListAsync();
                    case RecordOptions.SelectOT:
                        return await records.Where(s => !string.IsNullOrWhiteSpace(s.OvertimeEndTime)).ToListAsync();
                    default:
                        throw new Exception("Invalid Option");
                }
            }
        }

        public async Task InsertOrUpdateRecordAsync(EditRecordModel model)
        {
            using (var dbContext = new AppDbContext(builder.Options))
            {
                dbContext.ChangeTracker.AutoDetectChangesEnabled = false;
                bool isNewRecordNeedCalc = TypeEnum.LimitedLeaves.Contains(model.OffType) &&
                                           model.StatusOfApproval == StatusOfApprovalEnum.APPROVED();
                if (string.IsNullOrWhiteSpace(model.RecordId))
                {
                    DateTime? offApplyDate = null;
                    if (!string.IsNullOrWhiteSpace(model.OffType))
                        offApplyDate = DateTime.Today;
                    if (isNewRecordNeedCalc)
                    {
                        var user = await dbContext.UserInfo.FirstOrDefaultAsync(s => s.Id == model.UserId);
                        user = TypeEnum.CalcLeaves(user, model.OffTimeStart, model.OffTimeEnd, model.OffType);
                        if (user == null)
                            return;
                        dbContext.Entry(user).State = EntityState.Modified;
                    }
                    var r = new CheckRecord
                    {
                        UserId = model.UserId,
                        CheckedDate = model.CheckedDate,
                        CheckInTime = model.CheckInTime,
                        CheckOutTime = model.CheckOutTime,
                        GeoLocation1 = model.GeoLocation1,
                        GeoLocation2 = model.GeoLocation2,
                        OvertimeEndTime = model.OvertimeEndTime,
                        OffType = model.OffType,
                        OffTimeStart = model.OffTimeStart,
                        OffTimeEnd = model.OffTimeEnd,
                        OffReason = model.OffReason,
                        StatusOfApproval = model.StatusOfApproval,
                        OffApplyDate = offApplyDate
                    };
                    await dbContext.CheckRecord.AddAsync(r);
                }
                else
                {
                    var record = await dbContext.CheckRecord.FirstOrDefaultAsync(s => s.Id == model.RecordId);
                    if (record != null)
                    {
                        bool isOldRecordNeedCalc = TypeEnum.LimitedLeaves.Contains(record.OffType) &&
                                                   record.StatusOfApproval == StatusOfApprovalEnum.APPROVED();
                        if (isOldRecordNeedCalc || isNewRecordNeedCalc)
                        {
                            // new data is special, and approved -
                            var user = await dbContext.UserInfo.FirstOrDefaultAsync(s => s.Id == record.UserId);
                            if (user == null)
                                return;
                            if (isNewRecordNeedCalc)
                            {
                                var newCalc = TypeEnum.CalcLeaves(user, model.OffTimeStart, model.OffTimeEnd,
                                    model.OffType);
                                if (newCalc != null)
                                {
                                    user = newCalc;
                                    dbContext.Entry(user).State = EntityState.Modified;
                                }
                                else
                                {
                                    return;
                                }
                            }
                            // old data is special and approved +
                            if (isOldRecordNeedCalc)
                            {
                                user = TypeEnum.CalcLeaves(user, record.OffTimeStart, record.OffTimeEnd, record.OffType,
                                    inverseOp: true);
                                dbContext.Entry(user).State = EntityState.Modified;
                            }
                        }
                        if (string.IsNullOrWhiteSpace(record.OffType) && !string.IsNullOrWhiteSpace(model.OffType))
                        {
                            record.OffApplyDate = DateTime.Today;
                        }
                        record.CheckInTime = model.CheckInTime;
                        record.CheckOutTime = model.CheckOutTime;
                        record.GeoLocation1 = model.GeoLocation1;
                        record.GeoLocation2 = model.GeoLocation2;
                        record.OvertimeEndTime = model.OvertimeEndTime;
                        record.OffType = model.OffType;
                        record.OffTimeStart = model.OffTimeStart;
                        record.OffTimeEnd = model.OffTimeEnd;
                        record.OffReason = model.OffReason;
                        record.StatusOfApproval = model.StatusOfApproval;
                        dbContext.Entry(record).State = EntityState.Modified;
                    }
                }
                await dbContext.SaveChangesAsync();
            }
        }

        public async Task DeleteRecordByRecordIdAsync(string recordId)
        {
            using (var dbContext = new AppDbContext(builder.Options))
            {
                var record = await dbContext.CheckRecord.FirstOrDefaultAsync(s => s.Id == recordId);
                if (record == null)
                    return;
                if (TypeEnum.LimitedLeaves.Contains(record.OffType) &&
                    record.StatusOfApproval == StatusOfApprovalEnum.APPROVED())
                {
                    var user = await dbContext.UserInfo.FirstOrDefaultAsync(s => s.Id == record.UserId);
                    if (user == null)
                        return;
                    user = TypeEnum.CalcLeaves(user, record.OffTimeStart, record.OffTimeEnd, record.OffType,
                        inverseOp: true);
                    dbContext.Entry(user).State = EntityState.Modified;
                }
                dbContext.Entry(record).State = EntityState.Deleted;
                await dbContext.SaveChangesAsync();
            }
        }

        public async Task<List<AbsenceModel>> GetMonthlyOffRecordAsync(string y, string m)
        {
            using (var dbContext = new AppDbContext(builder.Options))
            {
                var year = Int32.Parse(y);
                var month = Int32.Parse(m);
                DateTime fromTime = new DateTime(year, month, 1);
                DateTime toTime = new DateTime(year, month + 1, 1).AddSeconds(-1);
                var data = await (from record in dbContext.CheckRecord
                    join user in dbContext.UserInfo on record.UserId equals user.Id
                    where record.CheckedDate >= fromTime && record.CheckedDate <= toTime &&
                          !string.IsNullOrWhiteSpace(record.OffType) &&
                          record.StatusOfApproval != StatusOfApprovalEnum.REJECTED()
                    select new AbsenceModel
                    {
                        UserName = user.UserName,
                        CheckedDate = record.CheckedDate.ToString("yyyy-MM-dd"),
                        OffType = record.OffType,
                        OffTimeStart = record.OffTimeStart.ToString(),
                        OffTimeEnd = record.OffTimeEnd.ToString(),
                        OffReason = record.OffReason,
                        StatusOfApproval = record.StatusOfApproval
                    }).ToListAsync();

                return data;
            }
        }

        /* bug here, still yet to test */
        // automatic fill up all empty records for administrator
        public async Task AutomaticAddRecordAsync(DateTime date, IList<UserInfo> excludeFrom = null)
        {
            using (var dbContext = new AppDbContext(builder.Options))
            {
                dbContext.ChangeTracker.AutoDetectChangesEnabled = false;
                var records = dbContext.CheckRecord.Where(s => s.CheckedDate == date);

                var users = dbContext.UserInfo.Where(
                    s => s.DateOfQuit == null && excludeFrom.FirstOrDefault(z => z.Id == s.Id) == null);

                var result = from user in users
                    join record in records on user.Id equals record.UserId into userRecord
                    from ur in userRecord.DefaultIfEmpty()
                    select new
                    {
                        userRecords = ur,
                        Id = user.Id
                    };

                var list = new List<CheckRecord>();
                var rnd = new Random();
                var checkInTime = new TimeSpan(9, 0, 0);
                var checkOutTime = new TimeSpan(18, 0, 0);
                foreach (var i in result)
                {
                    if (i.userRecords == null)
                    {
                        list.Add(new CheckRecord
                        {
                            UserId = i.Id,
                            CheckedDate = date,
                            CheckInTime = checkInTime.Add(new TimeSpan(0, rnd.Next(-30, 30), rnd.Next(-60, 60))),
                            CheckOutTime = checkOutTime.Add(new TimeSpan(0, rnd.Next(-30, 50), rnd.Next(-60, 60))),
                            GeoLocation1 = "24.997671, 121.53798",
                            GeoLocation2 = "24.997671, 121.53798"
                        });
                    }
                    else
                    {
                        if (!string.IsNullOrWhiteSpace(i.userRecords.OffType) &&
                            i.userRecords.StatusOfApproval == StatusOfApprovalEnum.APPROVED())
                            continue;
                        if (i.userRecords.CheckInTime == null) {
                            i.userRecords.CheckInTime = checkInTime.Add(new TimeSpan
                                (0, rnd.Next(-30, 30), rnd.Next(-60, 60)));
                            i.userRecords.GeoLocation1 = "24.997671, 121.53798";
                        }
                        if (i.userRecords.CheckOutTime == null) {
                            i.userRecords.CheckOutTime = checkOutTime.Add(new TimeSpan
                                (0, rnd.Next(-30, 50), rnd.Next(-60, 60)));
                            i.userRecords.GeoLocation2 = "24.997671, 121.53798";
                        }
                        if (string.IsNullOrWhiteSpace(i.userRecords.GeoLocation1) && 
                        i.userRecords.CheckInTime != null) {
                        i.userRecords.GeoLocation1 = "24.997671, 121.53798";
                    }
                    if (string.IsNullOrWhiteSpace(i.userRecords.GeoLocation2) && 
                        i.userRecords.CheckOutTime != null) {
                        i.userRecords.GeoLocation2 = "24.997671, 121.53798";
                    }
                        dbContext.Entry(i.userRecords).State = EntityState.Modified;
                    }
                }
                await dbContext.CheckRecord.AddRangeAsync(list);
                await dbContext.SaveChangesAsync();
            }
        }

        public List<ExcelDto> MapDtoWithId(List<ExcelDto> list)
        {
            using (var dbContext = new AppDbContext(builder.Options))
            {
                TimeSpan time;
                DateTime date;
                var names = list.Select(s => s.UserName).Distinct();
                var maps = dbContext.UserInfo.Where(s => names.Contains(s.UserName))
                    .Select(s => new NameListModel {Label = s.UserName, Value = s.Id}).ToList();
                for (int i = 0; i < list.Count; i++)
                {
                    if (string.IsNullOrWhiteSpace(list[i].UserName))
                        continue;
                    var obj = maps.FirstOrDefault(r => r.Label == list[i].UserName);
                    if (obj == null)
                        continue;

                    list[i].Id = obj.Value;
                    list[i].IsDataValid = DateTime.TryParseExact(list[i].CheckedDate,
                        "yyyy-MM-dd",
                        CultureInfo.InvariantCulture,
                        DateTimeStyles.None,
                        out date);
                    if (!list[i].IsDataValid)
                    {
                        continue;
                    }
                    list[i].IsInValid = TimeSpan.TryParse(list[i].CheckInTime, out time);
                    list[i].IsOutValid = TimeSpan.TryParse(list[i].CheckOutTime, out time);
                    list[i].IsOTValid = TimeSpan.TryParse(list[i].OvertimeEndTime, out time);
                    list[i].IsOffValid = DateTime.TryParseExact(list[i].OffApplyDate,
                        "yyyy-MM-dd",
                        CultureInfo.InvariantCulture,
                        DateTimeStyles.None,
                        out date);
                    if (!list[i].IsOffValid)
                        continue;
                    try
                    {
                        var offT = list[i].OffTime.Split('-');
                        if ((offT.Length != 2) ||
                            (!TimeSpan.TryParse(offT[0], out time) || !TimeSpan.TryParse(offT[1], out time)))
                        {
                            list[i].IsOffValid = false;
                            continue;
                        }
                    }
                    catch (Exception)
                    {
                        list[i].IsOffValid = false;
                        continue;
                    }
                    if (!TypeEnum.Leaves.Contains(list[i].OffType))
                    {
                        list[i].IsOffValid = false;
                    }
                }
                return list;
            }
        }

        public Tuple<int, int> BulkyUpdate(List<ExcelDto> list)
        {
            using (var dbContext = new AppDbContext(builder.Options))
            {
                dbContext.ChangeTracker.AutoDetectChangesEnabled = false;
                DateTime date, checkedDate;
                List<CheckRecord> newList = new List<CheckRecord>();
                int count = 0;
                foreach (var i in list)
                {
                    DateTime? offapplyDate;
                    TimeSpan? checkIn, checkOut, offStart, offEnd;
                    DateTime.TryParse(i.CheckedDate, out checkedDate);
                    var obj = dbContext.CheckRecord.FirstOrDefault(s => s.UserId == i.Id && s.CheckedDate == checkedDate);

                    if (i.IsInValid)
                    {
                        DateTime.TryParse(i.CheckInTime, out date);
                        checkIn = date.TimeOfDay;
                    }
                    else
                        checkIn = null;
                    if (i.IsOutValid)
                    {
                        DateTime.TryParse(i.CheckOutTime, out date);
                        checkOut = date.TimeOfDay;
                    }
                    else
                        checkOut = null;
                    if (i.IsOTValid)
                    {
                        DateTime.TryParse(i.OffApplyDate, out date);
                        offapplyDate = date;
                    }
                    else
                        offapplyDate = null;
                    if (i.IsOffValid)
                    {
                        var off = i.OffTime.Split('-');
                        DateTime.TryParse(off[0], out date);
                        offStart = date.TimeOfDay;
                        DateTime.TryParse(off[1], out date);
                        offEnd = date.TimeOfDay;
                    }
                    else
                    {
                        offStart = null;
                        offEnd = null;
                    }
                    if (obj == null)
                    {
                        newList.Add(new CheckRecord
                        {
                            CheckedDate = checkedDate,
                            CheckInTime = checkIn,
                            CheckOutTime = checkOut,
                            GeoLocation1 = i.GeoLocation1,
                            GeoLocation2 = i.GeoLocation2,
                            OvertimeEndTime = i.OvertimeEndTime,
                            OffType = i.OffType,
                            OffApplyDate = offapplyDate,
                            OffReason = i.OffReason,
                            OffTimeStart = offStart,
                            OffTimeEnd = offEnd,
                            StatusOfApproval = i.IsOffValid ? StatusOfApprovalEnum.APPROVED() : null,
                            StatusOfApprovalForOvertime = i.IsOTValid ? StatusOfApprovalEnum.APPROVED() : null,
                        });
                        continue;
                    }
                    obj.CheckedDate = checkedDate;
                    obj.CheckInTime = checkIn;
                    obj.CheckOutTime = checkOut;
                    obj.GeoLocation1 = i.GeoLocation1;
                    obj.GeoLocation2 = i.GeoLocation2;
                    obj.OvertimeEndTime = i.OvertimeEndTime;
                    obj.OffType = i.OffType;
                    obj.OffApplyDate = offapplyDate;
                    obj.OffReason = i.OffReason;
                    obj.OffTimeStart = offStart;
                    obj.OffTimeEnd = offEnd;
                    obj.StatusOfApproval = i.IsOffValid ? StatusOfApprovalEnum.APPROVED() : null;
                    obj.StatusOfApprovalForOvertime = i.IsOTValid ? StatusOfApprovalEnum.APPROVED() : null;
                    dbContext.Entry(obj).State = EntityState.Modified;
                    count++;
                }
                dbContext.CheckRecord.AddRange(newList);
                dbContext.SaveChanges();
                return new Tuple<int, int>(newList.Count, count);
            }
        }

        public async Task<List<RemainingDayoffModel>> GetRemainingDayOffAsync()
        {
            using (var dbContext = new AppDbContext(builder.Options))
            {
                var obj = await (from record in dbContext.RemainingDayOff
                    join user in dbContext.UserInfo on record.UserId equals user.Id
                    select new RemainingDayoffModel()
                    {
                        UserName = user.UserName,
                        Year = record.Year,
                        AnnualLeaves = record.AnnualLeaves.ToString(),
                        SickLeaves = record.SickLeaves.ToString(),
                        FamilyCareLeaves = record.FamilyCareLeaves.ToString()
                    }).ToListAsync();
                return obj;
            }
        }
    }

    public class ApplyOffModel
    {
        public DateTime CheckedDate { get; set; }

        public string OffType { get; set; }

        public int OffTimeStart { get; set; }

        public int OffTimeEnd { get; set; }

        public DateTime? OffEndDate { get; set; }

        public string OffReason { get; set; }
    }

    public class AbsenceModel
    {
        public string UserName { get; set; }
        public string CheckedDate { get; set; }
        public string OffType { get; set; }
        public string OffTimeStart { get; set; }
        public string OffTimeEnd { get; set; }
        public string OffReason { get; set; }
        public string StatusOfApproval { get; set; }
    }

    public class OffRecordModel
    {
        public DateTime CheckedDate { get; set; }

        public string OffType { get; set; }

        public TimeSpan? OffTimeStart { get; set; }

        public TimeSpan? OffTimeEnd { get; set; }

        public DateTime? OffEndDate { get; set; }

        public int FromTime
        {
            set { OffTimeStart = new TimeSpan(value, 0, 0); }
        }

        public int ToTime
        {
            set { OffTimeEnd = new TimeSpan(value, 0, 0); }
        }


        public string OffReason { get; set; }

        public string StatusOfApproval { get; set; }
    }

    public class NotificationModel
    {
        public string Id { get; set; }

        public string UserName { get; set; }

        public string CheckedDate { get; set; }

        public string OffType { get; set; }

        public string OffTime { get; set; }

        public string OffReason { get; set; }

        public string StatusOfApproval { get; set; }

        public string OvertimeEndTime { get; set; }

        public string StatusOfApprovalForOvertime { get; set; }
    }

    public class ReportModel
    {
        public string UserId { get; set; }

        public string UserName { get; set; }

        public string RecordId { get; set; }

        public string CheckedDate { get; set; }

        public string CheckInTime { get; set; }

        public string CheckOutTime { get; set; }

        public string GeoLocation1 { get; set; }

        public string GeoLocation2 { get; set; }

        public string OffApplyDate { get; set; }

        public string OffType { get; set; }

        public string OffTimeStart { get; set; }

        public string OffTimeEnd { get; set; }

        public string OffReason { get; set; }

        public string OvertimeEndTime { get; set; }

        public string StatusOfApprovalForOvertime { get; set; }

        public string StatusOfApproval { get; set; }
    }

    public class RemainingDayoffModel
    {
        public string UserName { get; set; }
        public string Year { get; set; }
        public string AnnualLeaves { get; set; }
        public string SickLeaves { get; set; }
        public string FamilyCareLeaves { get; set; }
    }

    public enum RecordOptions
    {
        SelectAll = 0,
        SelectNormal = 1,
        SelectLeave = 2,
        SelectOT = 3,
    }
}