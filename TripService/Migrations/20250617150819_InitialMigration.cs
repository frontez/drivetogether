using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace TripService.Migrations
{
    /// <inheritdoc />
    public partial class InitialMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "locations",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_locations", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "trip_offers",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    driver_id = table.Column<long>(type: "bigint", nullable: false),
                    start_location_id = table.Column<long>(type: "bigint", nullable: false),
                    end_location_id = table.Column<long>(type: "bigint", nullable: false),
                    departure_time = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    available_seats = table.Column<int>(type: "integer", nullable: false),
                    price_per_seat = table.Column<decimal>(type: "numeric", nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_trip_offers", x => x.id);
                    table.ForeignKey(
                        name: "fk_trip_offers_locations_end_location_id",
                        column: x => x.end_location_id,
                        principalTable: "locations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_trip_offers_locations_start_location_id",
                        column: x => x.start_location_id,
                        principalTable: "locations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "trips",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    trip_offer_id = table.Column<long>(type: "bigint", nullable: false),
                    driver_id = table.Column<long>(type: "bigint", nullable: false),
                    start_time = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    end_time = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    status = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_trips", x => x.id);
                    table.ForeignKey(
                        name: "fk_trips_trip_offers_trip_offer_id",
                        column: x => x.trip_offer_id,
                        principalTable: "trip_offers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "trip_requests",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    trip_offer_id = table.Column<long>(type: "bigint", nullable: false),
                    passenger_id = table.Column<long>(type: "bigint", nullable: false),
                    requested_seats = table.Column<int>(type: "integer", nullable: false),
                    request_time = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false),
                    trip_id = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_trip_requests", x => x.id);
                    table.ForeignKey(
                        name: "fk_trip_requests_trip_offers_trip_offer_id",
                        column: x => x.trip_offer_id,
                        principalTable: "trip_offers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_trip_requests_trips_trip_id",
                        column: x => x.trip_id,
                        principalTable: "trips",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_locations_name",
                table: "locations",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_trip_offers_end_location_id",
                table: "trip_offers",
                column: "end_location_id");

            migrationBuilder.CreateIndex(
                name: "ix_trip_offers_start_location_id",
                table: "trip_offers",
                column: "start_location_id");

            migrationBuilder.CreateIndex(
                name: "ix_trip_requests_trip_id",
                table: "trip_requests",
                column: "trip_id");

            migrationBuilder.CreateIndex(
                name: "ix_trip_requests_trip_offer_id",
                table: "trip_requests",
                column: "trip_offer_id");

            migrationBuilder.CreateIndex(
                name: "ix_trips_trip_offer_id",
                table: "trips",
                column: "trip_offer_id",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "trip_requests");

            migrationBuilder.DropTable(
                name: "trips");

            migrationBuilder.DropTable(
                name: "trip_offers");

            migrationBuilder.DropTable(
                name: "locations");
        }
    }
}
