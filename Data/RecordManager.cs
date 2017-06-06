using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using ReactSpa.Controllers;
using ReactSpa.Data;

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
                var record = await dbContext.CheckRecord.FirstOrDefaultAsync(s => s.CheckedDate == now);
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
                    var record = await dbContext.CheckRecord.FirstOrDefaultAsync(s => s.CheckedDate == DateTime.Today);
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
                var now = DateTime.Today;
                var record =
                    await dbContext.CheckRecord.FirstOrDefaultAsync(s => s.UserId == id && s.CheckedDate == now);
                if (record == null)
                    return null;
                record.CheckOutTime = DateTime.Now.TimeOfDay;
                record.GeoLocation2 = geo;
                await dbContext.SaveChangesAsync();
                return record.CheckOutTime;
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

        /** bug notify supervisor, deputy, role */

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
                            OffTimeEnd = model.OffTimeEnd
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
                        dbContext.Entry(record).State = EntityState.Modified;
                    }
                }
                await dbContext.CheckRecord.AddRangeAsync(recordList);

                await dbContext.SaveChangesAsync();
                return true;
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
                if (result.CheckInTime == null && result.CheckOutTime == null)
                {
                    dbContext.Entry(result).State = EntityState.Deleted;
                }
                else
                {
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
                              !string.IsNullOrWhiteSpace(record.OffType)
                        select new NotificationModel
                        {
                            Id = record.Id,
                            UserName = user.UserName,
                            CheckedDate = record.CheckedDate.ToString("yyyy-MM-dd"),
                            OffType = record.OffType,
                            OffTime = $"{record.OffTimeStart} - {record.OffTimeEnd}",
                            OffReason = record.OffReason,
                            StatusOfApproval = record.StatusOfApproval
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
                                !string.IsNullOrWhiteSpace(s.OffType))
                    .Select(s => new NotificationModel
                    {
                        Id = s.Id,
                        UserName = ulist.FirstOrDefault(z => z.Value == s.UserId).Label,
                        CheckedDate = s.CheckedDate.ToString("yyyy-MM-dd"),
                        OffType = s.OffType,
                        OffTime = $"{s.OffTimeStart.ToString()} - {s.OffTimeEnd.ToString()}",
                        OffReason = s.OffReason,
                        StatusOfApproval = s.StatusOfApproval
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
                    .Where(s => !string.IsNullOrWhiteSpace(s.OffType) && s.CheckedDate > dateBound && s.UserId == id)
                    .Select(s => new NotificationModel
                    {
                        CheckedDate = s.CheckedDate.ToString("yyyy-MM-dd"),
                        OffType = s.OffType,
                        OffTime = $"{s.OffTimeStart} - {s.OffTimeEnd}",
                        OffReason = s.OffReason,
                        StatusOfApproval = s.StatusOfApproval
                    }).ToListAsync();
                return result;
            }
        }

        public async Task<bool> SetStatusOfApprovalAsync(string recordId, string status)
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
                    else
                    {
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
                        OffReason = record.OffReason,
                        OffTimeStart = record.OffTimeStart.ToString(),
                        OffTimeEnd = record.OffTimeEnd.ToString(),
                        OffType = string.IsNullOrWhiteSpace(record.OffType) ? "" : record.OffType,
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
                        OffType = model.OffType,
                        OffTimeStart = model.OffTimeStart,
                        OffTimeEnd = model.OffTimeEnd,
                        OffReason = model.OffReason,
                        StatusOfApproval = model.StatusOfApproval
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

                        record.CheckInTime = model.CheckInTime;
                        record.CheckOutTime = model.CheckOutTime;
                        record.GeoLocation1 = model.GeoLocation1;
                        record.GeoLocation2 = model.GeoLocation2;
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

        public string OffType { get; set; }

        public string OffTimeStart { get; set; }

        public string OffTimeEnd { get; set; }

        public string OffReason { get; set; }

        public string StatusOfApproval { get; set; }
    }

    public enum RecordOptions
    {
        SelectAll = 0,
        SelectNormal = 1,
        SelectLeave = 2
    }
}