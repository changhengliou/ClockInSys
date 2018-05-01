using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ValueGeneration.Internal;
using Microsoft.Extensions.Options;
using ReactSpa.Controllers;
using ReactSpa.Utils;

namespace ReactSpa.Data
{
    public class UserInfoManager
    {
        private readonly DbContextOptionsBuilder<AppDbContext> builder = new DbContextOptionsBuilder<AppDbContext>();

        public UserInfoManager(IOptions<ConnectionInfo> configuration)
        {
            ConnectionInfo config = configuration.Value;
            builder.UseSqlServer(config.LocalSQLServer); // changed when azure published
        }

        public async Task<IdentityResult> CreateUserAsync(UserInfo user)
        {
            using (var dbContext = new AppDbContext(builder.Options))
            {
                dbContext.ChangeTracker.AutoDetectChangesEnabled = false;
                if (!Utilities.IsValidEmail(user.Email))
                    return IdentityResult.Failed(new IdentityErrorDescriber().InvalidEmail(user.Email));

                var result = await dbContext.UserInfo.FirstOrDefaultAsync(s => s.Email == user.Email);
                if (result != null)
                    return IdentityResult.Failed(new IdentityErrorDescriber().DuplicateEmail(result.Email));
                user.ConcurrencyStamp = Guid.NewGuid().ToString();
                user.SecurityStamp = Guid.NewGuid().ToString();
                using (var sql = dbContext.Database.GetDbConnection().CreateCommand())
                {
                    string doe = null;
                    string doq = null;
                    if (user.DateOfEmployment != null)
                        doe = user.DateOfEmployment.Value.ToString("yyyy-MM-dd");
                    if (user.DateOfQuit != null)
                        doe = user.DateOfQuit.Value.ToString("yyyy-MM-dd");
                    string cmd = null;
                    sql.CommandText =
                        $"INSERT INTO [User] ([Id], [AnnualLeaves], [ConcurrencyStamp], [Email], [FamilyCareLeaves], " +
                        $"[JobTitle], [PhoneNumber], [SecurityStamp], [SickLeaves], [UserName], " +
                        $"[DateOfEmployment], [DateOfQuit]) VALUES (\'{user.Id}\', {user.AnnualLeaves}, \'{user.ConcurrencyStamp}\', " +
                        $"\'{user.Email}\', {user.FamilyCareLeaves}, \'{user.JobTitle}\', \'{user.PhoneNumber}\', " +
                        $"\'{user.SecurityStamp}\', {user.SickLeaves}, \'{user.UserName}\', " +
                        $"\'{doe}\', \'{doq}\')";
                    dbContext.Database.OpenConnection();
                    sql.ExecuteNonQuery();
                }
//                var newUser = new UserInfo
//                {
//                    Id = user.Id,
//                    AnnualLeaves = user.AnnualLeaves,
//                    ConcurrencyStamp = user.ConcurrencyStamp,
//                    DateOfEmployment = user.DateOfEmployment,
//                    DateOfQuit = user.DateOfQuit,
//                    Email = user.Email,
//                    FamilyCareLeaves = user.FamilyCareLeaves,
//                    JobTitle = user.JobTitle,
//                    PhoneNumber = user.PhoneNumber,
//                    SecurityStamp = user.SecurityStamp,
//                    SickLeaves = user.SickLeaves,
//                    UserName = user.UserName
//                };
//                dbContext.UserInfo.Attach(newUser);
//                dbContext.Entry(newUser).State = EntityState.Added;
                await dbContext.SaveChangesAsync();
                return IdentityResult.Success;
            }
        }

        public async Task<UserInfo> FindUserByEmailAsync(string email)
        {
            using (var dbContext = new AppDbContext(builder.Options))
            {
                return await dbContext.UserInfo.FirstOrDefaultAsync(s => s.Email == email);
            }
        }

        /** Get UserInfo, including deputies name and supervisors name by Id */

        public async Task<UserInfoModel> GetUserInfoAsync(string id)
        {
            using (var dbContext = new AppDbContext(builder.Options))
            {
                var result = await dbContext.UserInfo.FirstOrDefaultAsync(s => s.Id == id);
                if (result != null)
                {
                    var deputies = await GetDeputyNameListAsync(id);
                    var supervisors = await GetSupervisorNameListAsync(id);
                    return new UserInfoModel
                    {
                        UserName = result.UserName,
                        UserEmail = result.Email,
                        UserPhone = result.PhoneNumber,
                        JobTitle = result.JobTitle,
                        DateOfEmployment =
                            result.DateOfEmployment == null ? "" : result.DateOfEmployment.Value.ToString("yyyy年M月d日"),
                        AnnualLeaves = result.AnnualLeaves,
                        SickLeaves = result.SickLeaves,
                        FamilyCareLeaves = result.FamilyCareLeaves,
                        DeputyName = deputies,
                        SupervisorName = supervisors
                    };
                }
                return null;
            }
        }

        /** Search by User's name, and return User name and id*/

        public async Task<List<NameListModel>> SearchUserNameAsync(string param)
        {
            using (var dbContext = new AppDbContext(builder.Options))
            {
                var result = await dbContext.UserInfo.Where(s => s.UserName.Contains(param))
                    .Select(s => new NameListModel {Value = s.Id, Label = s.UserName}).ToListAsync();
                if (!result.Any())
                    return null;
                return result;
            }
        }

        /** Add UserDeputy for specific User */

        public async Task<bool> AddUserDeputyAsync(string id, List<NameListModel> userDeputy)
        {
            using (var dbContext = new AppDbContext(builder.Options))
            {
                try
                {
                    dbContext.ChangeTracker.AutoDetectChangesEnabled = false;
                    foreach (var i in userDeputy)
                    {
                        await dbContext.UserDeputy.AddAsync(new UserDeputy
                        {
                            UserId = id,
                            DeputyId = i.Value,
                            DeputyName = i.Label
                        });
                    }
                    await dbContext.SaveChangesAsync();
                }
                catch (Exception)
                {
                    return false;
                }
                return true;
            }
        }

        /** Add UserSupervisor for specific Supervisor */

        public async Task<bool> AddUserSupervisorAsync(string id, List<NameListModel> userSupervisor)
        {
            using (var dbContext = new AppDbContext(builder.Options))
            {
                try
                {
                    dbContext.ChangeTracker.AutoDetectChangesEnabled = false;
                    foreach (var i in userSupervisor)
                    {
                        await dbContext.UserSupervisor.AddAsync(new UserSupervisor()
                        {
                            UserId = id,
                            SupervisorId = i.Value,
                            SupervisorName = i.Label
                        });
                    }
                    await dbContext.SaveChangesAsync();
                }
                catch (Exception)
                {
                    return false;
                }
                return true;
            }
        }

        /** Bug here, lack of functionality of [Authority], still yet to find a decent way to compare 2 diff sets */

        public async Task<UserInfo> UpdateUserAsync(AccountApiController.UserModel model)
        {
            using (var dbContext = new AppDbContext(builder.Options))
            {
                dbContext.ChangeTracker.AutoDetectChangesEnabled = false;
                var user = await dbContext.UserInfo.Where(s => s.Id == model.Id).FirstOrDefaultAsync();
                if (user != null)
                {
                    await dbContext.Database.ExecuteSqlCommandAsync(
                        $"DELETE FROM [dbo].[UserDeputy] WHERE UserId='{model.Id}'");
                    await dbContext.Database.ExecuteSqlCommandAsync(
                        $"DELETE FROM [dbo].[UserSupervisor] WHERE UserId='{model.Id}'");
                    await AddUserDeputyAsync(model.Id, model.Deputy);
                    await AddUserSupervisorAsync(model.Id, model.Supervisor);
                    DateTime date, dateOfQuit;
                    DateTime.TryParse(model.DateOfEmployment, out date);
                    DateTime? quitVal = null;
                    if (DateTime.TryParse(model.DateOfQuit, out dateOfQuit))
                        quitVal = dateOfQuit;
                    user.UserName = model.UserName;
                    user.Email = model.UserEmail;
                    user.PhoneNumber = model.PhoneNumber;
                    user.DateOfEmployment = date;
                    user.DateOfQuit = quitVal;
                    user.JobTitle = model.JobTitle;
                    user.AnnualLeaves = model.AnnualLeaves;
                    user.SickLeaves = model.SickLeaves;
                    user.FamilyCareLeaves = model.FamilyCareLeaves;
                    dbContext.Entry(user).State = EntityState.Modified;
                    await dbContext.SaveChangesAsync();
                    return user;
                }
                return null;
            }
        }

        public async Task<List<string>> GetDeputyNameListAsync(string id)
        {
            using (var dbContext = new AppDbContext(builder.Options))
            {
                var deputies =
                    await dbContext.UserDeputy.Where(s => s.UserId == id).Select(s => s.DeputyName).ToListAsync();
                return deputies;
            }
        }

        public async Task<List<string>> GetSupervisorNameListAsync(string id)
        {
            using (var dbContext = new AppDbContext(builder.Options))
            {
                var supervisors =
                    await dbContext.UserSupervisor.Where(s => s.UserId == id)
                        .Select(s => s.SupervisorName)
                        .ToListAsync();
                return supervisors;
            }
        }

        /** Get UserDeputy */

        public async Task<List<NameListModel>> GetUserDeputyAsync(string id)
        {
            using (var dbContext = new AppDbContext(builder.Options))
            {
                var deputies = await dbContext.UserDeputy.Where(s => s.UserId == id)
                    .Select(s => new NameListModel {Value = s.DeputyId, Label = s.DeputyName}).ToListAsync();
                return deputies;
            }
        }

        /** Get UserSupervisor */

        public async Task<List<NameListModel>> GetUserSupervisorAsync(string id)
        {
            using (var dbContext = new AppDbContext(builder.Options))
            {
                var supervisors = await dbContext.UserSupervisor.Where(s => s.UserId == id)
                    .Select(s => new NameListModel {Value = s.SupervisorId, Label = s.SupervisorName}).ToListAsync();
                return supervisors;
            }
        }

        public async Task<bool> IsEmailValid(string id, string email)
        {
            if (Utilities.IsValidEmail(email))
            {
                using (var dbContext = new AppDbContext(builder.Options))
                {
                    var result = await dbContext.UserInfo.FirstOrDefaultAsync(s => s.Email == email);
                    if (result == null)
                        return true;
                    if (result.Id == id)
                        return true;
                    return false;
                }
            }
            return false;
        }

        public async Task<List<UserInfo>> GetUsersAsync()
        {
            using (var dbContext = new AppDbContext(builder.Options))
            {
                return await dbContext.UserInfo.Select(s => s).ToListAsync();
            }
        }
    }

    public class NameListModel
    {
        public string Value { get; set; } //Id
        public string Label { get; set; } //Name

        public override bool Equals(object obj)
        {
            NameListModel z = obj as NameListModel;
            return z != null && z.Value == Value && z.Label == Label;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = (int) 2166136261;
                hash = (hash * 16777619) ^ Value.GetHashCode();
                hash = (hash * 16777619) ^ Label.GetHashCode();
                return hash;
            }
        }
    }

    public class UserInfoModel
    {
        public string UserName { get; set; }
        public string UserEmail { get; set; }
        public string UserPhone { get; set; }
        public string DateOfEmployment { get; set; }
        public string JobTitle { get; set; }
        public decimal AnnualLeaves { get; set; }
        public decimal SickLeaves { get; set; }
        public decimal FamilyCareLeaves { get; set; }
        public List<string> DeputyName { get; set; }
        public List<string> SupervisorName { get; set; }
    }
}