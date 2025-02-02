﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using ProjectoR.Examples.CustomSubscription.Data;

#nullable disable

namespace ProjectoR.Examples.CustomSubscription.Migrations
{
    [DbContext(typeof(ApplicationContext))]
    partial class ApplicationContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "8.0.0")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("ProjectoR.Core.Checkpointing.CheckpointState", b =>
                {
                    b.Property<string>("ProjectionName")
                        .HasColumnType("text");

                    b.Property<long>("Position")
                        .HasColumnType("bigint");

                    b.HasKey("ProjectionName");

                    b.ToTable("Checkpoint", "ProjectoR");
                });

            modelBuilder.Entity("ProjectoR.Examples.Common.Data.AmountOfStudentsPerCityProjection", b =>
                {
                    b.Property<string>("City")
                        .HasColumnType("text");

                    b.Property<int>("Amount")
                        .HasColumnType("integer");

                    b.HasKey("City");

                    b.ToTable("AmountOfStudentsPerCity", "Projection");
                });

            modelBuilder.Entity("ProjectoR.Examples.Common.Data.AmountOfStudentsPerCountryProjection", b =>
                {
                    b.Property<string>("CountryCode")
                        .HasColumnType("text");

                    b.Property<int>("Amount")
                        .HasColumnType("integer");

                    b.HasKey("CountryCode");

                    b.ToTable("AmountOfStudentsPerCountry", "Projection");
                });

            modelBuilder.Entity("ProjectoR.Examples.Common.Data.StudentProjection", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("text");

                    b.Property<string>("FirstName")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("LastName")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.ToTable("Student", "Projection");
                });

            modelBuilder.Entity("ProjectoR.Examples.CustomSubscription.Data.EventRecord", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<DateTimeOffset>("Created")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("timestamp with time zone")
                        .HasDefaultValueSql("now()");

                    b.Property<byte[]>("Data")
                        .IsRequired()
                        .HasColumnType("bytea");

                    b.Property<string>("EventName")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<long>("Position")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint");

                    NpgsqlPropertyBuilderExtensions.UseIdentityAlwaysColumn(b.Property<long>("Position"));

                    b.Property<string>("StreamName")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.HasIndex("EventName");

                    b.HasIndex("Position")
                        .IsUnique()
                        .IsDescending();

                    b.HasIndex("StreamName");

                    b.ToTable("Event", (string)null);
                });

            modelBuilder.Entity("ProjectoR.Examples.Common.Data.StudentProjection", b =>
                {
                    b.OwnsOne("ProjectoR.Examples.Common.Data.Address", "Address", b1 =>
                        {
                            b1.Property<string>("StudentProjectionId")
                                .HasColumnType("text");

                            b1.Property<string>("City")
                                .IsRequired()
                                .HasColumnType("text");

                            b1.Property<string>("CountryCode")
                                .IsRequired()
                                .HasColumnType("text");

                            b1.Property<string>("PostalCode")
                                .IsRequired()
                                .HasColumnType("text");

                            b1.Property<string>("Street")
                                .IsRequired()
                                .HasColumnType("text");

                            b1.HasKey("StudentProjectionId");

                            b1.ToTable("Student", "Projection");

                            b1.WithOwner()
                                .HasForeignKey("StudentProjectionId");
                        });

                    b.OwnsOne("ProjectoR.Examples.Common.Data.ContactInformation", "ContactInformation", b1 =>
                        {
                            b1.Property<string>("StudentProjectionId")
                                .HasColumnType("text");

                            b1.Property<string>("Email")
                                .IsRequired()
                                .HasColumnType("text");

                            b1.Property<string>("Mobile")
                                .IsRequired()
                                .HasColumnType("text");

                            b1.HasKey("StudentProjectionId");

                            b1.ToTable("Student", "Projection");

                            b1.WithOwner()
                                .HasForeignKey("StudentProjectionId");
                        });

                    b.Navigation("Address")
                        .IsRequired();

                    b.Navigation("ContactInformation")
                        .IsRequired();
                });
#pragma warning restore 612, 618
        }
    }
}
