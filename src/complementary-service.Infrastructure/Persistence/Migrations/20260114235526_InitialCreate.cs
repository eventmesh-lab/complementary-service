using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace complementaryservice.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ComplementaryServices",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ReservationId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    EventId = table.Column<Guid>(type: "uuid", nullable: false),
                    ServiceType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ProviderId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Price = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Details = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    RequestedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ConfirmedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    RejectedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    RejectionReason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    ProviderIsAvailable = table.Column<bool>(type: "boolean", nullable: false),
                    ProviderResponseId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ProviderMessage = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    ProviderPrice = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    ProviderEstimatedTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ComplementaryServices", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ServiceProviders",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    ServiceType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ApiEndpoint = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    QueueName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    Priority = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServiceProviders", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ComplementaryServices_EventId",
                table: "ComplementaryServices",
                column: "EventId");

            migrationBuilder.CreateIndex(
                name: "IX_ComplementaryServices_ReservationId",
                table: "ComplementaryServices",
                column: "ReservationId");

            migrationBuilder.CreateIndex(
                name: "IX_ComplementaryServices_UserId",
                table: "ComplementaryServices",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_ComplementaryServices_UserId_ReservationId",
                table: "ComplementaryServices",
                columns: new[] { "UserId", "ReservationId" });

            migrationBuilder.CreateIndex(
                name: "IX_ServiceProviders_IsActive",
                table: "ServiceProviders",
                column: "IsActive");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ComplementaryServices");

            migrationBuilder.DropTable(
                name: "ServiceProviders");
        }
    }
}
