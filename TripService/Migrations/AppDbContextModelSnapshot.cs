﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using TripService.Data;

#nullable disable

namespace TripService.Migrations
{
    [DbContext(typeof(AppDbContext))]
    partial class AppDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "9.0.1")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("TripService.Models.Location", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint")
                        .HasColumnName("id");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<long>("Id"));

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("name");

                    b.HasKey("Id")
                        .HasName("pk_locations");

                    b.HasIndex("Name")
                        .IsUnique()
                        .HasDatabaseName("ix_locations_name");

                    b.ToTable("locations", (string)null);
                });

            modelBuilder.Entity("TripService.Models.Trip", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint")
                        .HasColumnName("id");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<long>("Id"));

                    b.Property<long>("DriverId")
                        .HasColumnType("bigint")
                        .HasColumnName("driver_id");

                    b.Property<DateTime?>("EndTime")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("end_time");

                    b.Property<DateTime>("StartTime")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("start_time");

                    b.Property<int>("Status")
                        .HasColumnType("integer")
                        .HasColumnName("status");

                    b.Property<long>("TripOfferId")
                        .HasColumnType("bigint")
                        .HasColumnName("trip_offer_id");

                    b.HasKey("Id")
                        .HasName("pk_trips");

                    b.HasIndex("TripOfferId")
                        .IsUnique()
                        .HasDatabaseName("ix_trips_trip_offer_id");

                    b.ToTable("trips", (string)null);
                });

            modelBuilder.Entity("TripService.Models.TripOffer", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint")
                        .HasColumnName("id");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<long>("Id"));

                    b.Property<int>("AvailableSeats")
                        .HasColumnType("integer")
                        .HasColumnName("available_seats");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("created_at");

                    b.Property<DateTime>("DepartureTime")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("departure_time");

                    b.Property<string>("Description")
                        .HasColumnType("text")
                        .HasColumnName("description");

                    b.Property<long>("DriverId")
                        .HasColumnType("bigint")
                        .HasColumnName("driver_id");

                    b.Property<long>("EndLocationId")
                        .HasColumnType("bigint")
                        .HasColumnName("end_location_id");

                    b.Property<decimal>("PricePerSeat")
                        .HasColumnType("numeric")
                        .HasColumnName("price_per_seat");

                    b.Property<long>("StartLocationId")
                        .HasColumnType("bigint")
                        .HasColumnName("start_location_id");

                    b.Property<int>("Status")
                        .HasColumnType("integer")
                        .HasColumnName("status");

                    b.HasKey("Id")
                        .HasName("pk_trip_offers");

                    b.HasIndex("EndLocationId")
                        .HasDatabaseName("ix_trip_offers_end_location_id");

                    b.HasIndex("StartLocationId")
                        .HasDatabaseName("ix_trip_offers_start_location_id");

                    b.ToTable("trip_offers", (string)null);
                });

            modelBuilder.Entity("TripService.Models.TripRequest", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint")
                        .HasColumnName("id");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<long>("Id"));

                    b.Property<long>("PassengerId")
                        .HasColumnType("bigint")
                        .HasColumnName("passenger_id");

                    b.Property<DateTime>("RequestTime")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("request_time");

                    b.Property<int>("RequestedSeats")
                        .HasColumnType("integer")
                        .HasColumnName("requested_seats");

                    b.Property<int>("Status")
                        .HasColumnType("integer")
                        .HasColumnName("status");

                    b.Property<long?>("TripId")
                        .HasColumnType("bigint")
                        .HasColumnName("trip_id");

                    b.Property<long>("TripOfferId")
                        .HasColumnType("bigint")
                        .HasColumnName("trip_offer_id");

                    b.HasKey("Id")
                        .HasName("pk_trip_requests");

                    b.HasIndex("TripId")
                        .HasDatabaseName("ix_trip_requests_trip_id");

                    b.HasIndex("TripOfferId")
                        .HasDatabaseName("ix_trip_requests_trip_offer_id");

                    b.ToTable("trip_requests", (string)null);
                });

            modelBuilder.Entity("TripService.Models.Trip", b =>
                {
                    b.HasOne("TripService.Models.TripOffer", "TripOffer")
                        .WithOne("ConfirmedTrip")
                        .HasForeignKey("TripService.Models.Trip", "TripOfferId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired()
                        .HasConstraintName("fk_trips_trip_offers_trip_offer_id");

                    b.Navigation("TripOffer");
                });

            modelBuilder.Entity("TripService.Models.TripOffer", b =>
                {
                    b.HasOne("TripService.Models.Location", "EndLocation")
                        .WithMany()
                        .HasForeignKey("EndLocationId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired()
                        .HasConstraintName("fk_trip_offers_locations_end_location_id");

                    b.HasOne("TripService.Models.Location", "StartLocation")
                        .WithMany()
                        .HasForeignKey("StartLocationId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired()
                        .HasConstraintName("fk_trip_offers_locations_start_location_id");

                    b.Navigation("EndLocation");

                    b.Navigation("StartLocation");
                });

            modelBuilder.Entity("TripService.Models.TripRequest", b =>
                {
                    b.HasOne("TripService.Models.Trip", "Trip")
                        .WithMany("Passengers")
                        .HasForeignKey("TripId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .HasConstraintName("fk_trip_requests_trips_trip_id");

                    b.HasOne("TripService.Models.TripOffer", "TripOffer")
                        .WithMany("TripRequests")
                        .HasForeignKey("TripOfferId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired()
                        .HasConstraintName("fk_trip_requests_trip_offers_trip_offer_id");

                    b.Navigation("Trip");

                    b.Navigation("TripOffer");
                });

            modelBuilder.Entity("TripService.Models.Trip", b =>
                {
                    b.Navigation("Passengers");
                });

            modelBuilder.Entity("TripService.Models.TripOffer", b =>
                {
                    b.Navigation("ConfirmedTrip");

                    b.Navigation("TripRequests");
                });
#pragma warning restore 612, 618
        }
    }
}
