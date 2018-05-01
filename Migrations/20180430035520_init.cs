using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Metadata;

namespace ReactSpa.Migrations
{
    public partial class init : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Role",
                columns: table => new
                {
                    RoleId = table.Column<string>(nullable: false),
                    ConcurrencyStamp = table.Column<string>(nullable: true),
                    Name = table.Column<string>(maxLength: 256, nullable: true),
                    NormalizedName = table.Column<string>(maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Role", x => x.RoleId);
                });

            migrationBuilder.CreateTable(
                name: "UserToken",
                columns: table => new
                {
                    UserId = table.Column<string>(nullable: false),
                    LoginProvider = table.Column<string>(nullable: false),
                    Name = table.Column<string>(nullable: false),
                    Value = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserToken", x => new { x.UserId, x.LoginProvider, x.Name });
                });

            migrationBuilder.CreateTable(
                name: "User",
                columns: table => new
                {
                    Id = table.Column<string>(nullable: false),
                    AnnualLeaves = table.Column<decimal>(type: "decimal(5, 2)", nullable: false, defaultValue: 0m),
                    ConcurrencyStamp = table.Column<string>(nullable: true),
                    DateOfEmployment = table.Column<DateTime>(type: "Date", nullable: true),
                    DateOfQuit = table.Column<DateTime>(type: "Date", nullable: true),
                    Email = table.Column<string>(maxLength: 256, nullable: true),
                    FamilyCareLeaves = table.Column<decimal>(type: "decimal(5, 2)", nullable: false, defaultValue: 7m),
                    JobTitle = table.Column<string>(nullable: true),
                    PhoneNumber = table.Column<string>(nullable: true),
                    SecurityStamp = table.Column<string>(nullable: true),
                    SickLeaves = table.Column<decimal>(type: "decimal(5, 2)", nullable: false, defaultValue: 30m),
                    UserName = table.Column<string>(maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_User", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RoleClaim",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    ClaimType = table.Column<string>(nullable: true),
                    ClaimValue = table.Column<string>(nullable: true),
                    RoleId = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RoleClaim", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RoleClaim_Role_RoleId",
                        column: x => x.RoleId,
                        principalTable: "Role",
                        principalColumn: "RoleId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserClaim",
                columns: table => new
                {
                    ClaimId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    ClaimType = table.Column<string>(nullable: true),
                    ClaimValue = table.Column<string>(nullable: true),
                    UserId = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserClaim", x => x.ClaimId);
                    table.ForeignKey(
                        name: "FK_UserClaim_User_UserId",
                        column: x => x.UserId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserLogin",
                columns: table => new
                {
                    LoginProvider = table.Column<string>(nullable: false),
                    ProviderKey = table.Column<string>(nullable: false),
                    ProviderDisplayName = table.Column<string>(nullable: true),
                    UserId = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserLogin", x => new { x.LoginProvider, x.ProviderKey });
                    table.ForeignKey(
                        name: "FK_UserLogin_User_UserId",
                        column: x => x.UserId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserRole",
                columns: table => new
                {
                    UserId = table.Column<string>(nullable: false),
                    RoleId = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserRole", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_UserRole_Role_RoleId",
                        column: x => x.RoleId,
                        principalTable: "Role",
                        principalColumn: "RoleId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserRole_User_UserId",
                        column: x => x.UserId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CheckRecord",
                columns: table => new
                {
                    Id = table.Column<string>(nullable: false),
                    CheckInTime = table.Column<TimeSpan>(nullable: true),
                    CheckOutTime = table.Column<TimeSpan>(nullable: true),
                    CheckedDate = table.Column<DateTime>(type: "Date", nullable: false),
                    GeoLocation1 = table.Column<string>(nullable: true),
                    GeoLocation2 = table.Column<string>(nullable: true),
                    OffApplyDate = table.Column<DateTime>(type: "Date", nullable: true),
                    OffEndDate = table.Column<DateTime>(type: "Date", nullable: true),
                    OffReason = table.Column<string>(nullable: true),
                    OffTimeEnd = table.Column<TimeSpan>(nullable: true),
                    OffTimeStart = table.Column<TimeSpan>(nullable: true),
                    OffType = table.Column<string>(nullable: true),
                    OvertimeEndTime = table.Column<string>(nullable: true),
                    StatusOfApproval = table.Column<string>(nullable: true),
                    StatusOfApprovalForOvertime = table.Column<string>(nullable: true),
                    UserId = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CheckRecord", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CheckRecord_User_UserId",
                        column: x => x.UserId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RemainingDayOff",
                columns: table => new
                {
                    Id = table.Column<string>(nullable: false),
                    AnnualLeaves = table.Column<decimal>(type: "decimal(5, 2)", nullable: false, defaultValue: 0m),
                    FamilyCareLeaves = table.Column<decimal>(type: "decimal(5, 2)", nullable: false, defaultValue: 0m),
                    SickLeaves = table.Column<decimal>(type: "decimal(5, 2)", nullable: false, defaultValue: 0m),
                    UserId = table.Column<string>(nullable: true),
                    Year = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RemainingDayOff", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RemainingDayOff_User_UserId",
                        column: x => x.UserId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserDeputy",
                columns: table => new
                {
                    Id = table.Column<string>(nullable: false),
                    DeputyId = table.Column<string>(nullable: true),
                    DeputyName = table.Column<string>(nullable: true),
                    UserId = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserDeputy", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserDeputy_User_UserId",
                        column: x => x.UserId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserSupervisor",
                columns: table => new
                {
                    Id = table.Column<string>(nullable: false),
                    SupervisorId = table.Column<string>(nullable: true),
                    SupervisorName = table.Column<string>(nullable: true),
                    UserId = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserSupervisor", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserSupervisor_User_UserId",
                        column: x => x.UserId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                table: "Role",
                column: "NormalizedName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RoleClaim_RoleId",
                table: "RoleClaim",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_UserClaim_UserId",
                table: "UserClaim",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserLogin_UserId",
                table: "UserLogin",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserRole_RoleId",
                table: "UserRole",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_CheckRecord_UserId",
                table: "CheckRecord",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_RemainingDayOff_UserId",
                table: "RemainingDayOff",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserDeputy_UserId",
                table: "UserDeputy",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_User_Email",
                table: "User",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserSupervisor_UserId",
                table: "UserSupervisor",
                column: "UserId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RoleClaim");

            migrationBuilder.DropTable(
                name: "UserClaim");

            migrationBuilder.DropTable(
                name: "UserLogin");

            migrationBuilder.DropTable(
                name: "UserRole");

            migrationBuilder.DropTable(
                name: "UserToken");

            migrationBuilder.DropTable(
                name: "CheckRecord");

            migrationBuilder.DropTable(
                name: "RemainingDayOff");

            migrationBuilder.DropTable(
                name: "UserDeputy");

            migrationBuilder.DropTable(
                name: "UserSupervisor");

            migrationBuilder.DropTable(
                name: "Role");

            migrationBuilder.DropTable(
                name: "User");
        }
    }
}
