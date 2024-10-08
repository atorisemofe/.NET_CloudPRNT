﻿// <auto-generated />
using CloudPRNT_Solution.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace CloudPRNT_Solution.Migrations.LocationTable
{
    [DbContext(typeof(LocationTableContext))]
    [Migration("20240719160328_createLocationTable")]
    partial class createLocationTable
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder.HasAnnotation("ProductVersion", "7.0.12");

            modelBuilder.Entity("CloudPRNT_Solution.Models.LocationTable", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("LocationName")
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.ToTable("LocationTable");
                });
#pragma warning restore 612, 618
        }
    }
}
