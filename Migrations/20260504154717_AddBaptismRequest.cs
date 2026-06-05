using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JCAM_CONNECT.Migrations
{
    /// <inheritdoc />
    public partial class AddBaptismRequest : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BaptismRequests",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    FullName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PreferredDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RequestCertificate = table.Column<bool>(type: "bit", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    BaptismDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    BaptismLocation = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    BaptismOfficiant = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CertificateSentAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CertificateFilePath = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RequestedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ApprovedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ApprovedBy = table.Column<int>(type: "int", nullable: true),
                    MemberProfileId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BaptismRequests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BaptismRequests_MemberProfiles_MemberProfileId",
                        column: x => x.MemberProfileId,
                        principalTable: "MemberProfiles",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_BaptismRequests_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BaptismRequests_MemberProfileId",
                table: "BaptismRequests",
                column: "MemberProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_BaptismRequests_UserId",
                table: "BaptismRequests",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BaptismRequests");
        }
    }
}
