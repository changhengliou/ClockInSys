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
            ConnectionInfo config = configuration.Value;
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
                var record = await dbContext.CheckRecord.FirstOrDefaultAsync(s => s.UserId == id && s.CheckedDate == now);
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
                        await dbContext.CheckRecord.AddAsync(new CheckRecord
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

                await dbContext.SaveChangesAsync();
                return true;
            }
        }

        public async Task<bool> DeleteLeaveAsync(string id, DateTime date)
        {
            using (var dbContext = new AppDbContext(builder.Options))
            {
                var result = await dbContext.CheckRecord.FirstOrDefaultAsync(s => s.UserId == id && s.CheckedDate == date);
                if (result == null)
                    return false;
                if (result.CheckInTime == null)
                    dbContext.Entry(result).State = EntityState.Deleted;
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

        public async Task<bool> GetNotification(string id)
        {
            using (var dbContext = new AppDbContext(builder.Options))
            {
                return false;
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
}