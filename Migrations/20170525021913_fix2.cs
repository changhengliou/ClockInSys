using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

namespace ReactSpa.Migrations
{
    public partial class fix2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LeavesReview");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "LeavesReview",
                columns: table => new
                {
                    Id = table.Column<string>(nullable: false),
                    RecordId = table.Column<string>(nullable: true),
                    ReviewerId = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LeavesReview", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LeavesReview_CheckRecord_RecordId",
                        column: x => x.RecordId,
                        principalTable: "CheckRecord",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_LeavesReview_RecordId",
                table: "LeavesReview",
                column: "RecordId");
        }
    }
}
