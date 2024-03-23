﻿// <auto-generated />
using System;
using InstrukcijeDotNet.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace InstrukcijeDotNet.Migrations
{
    [DbContext(typeof(AppContextHandler))]
    [Migration("20240319175953_SetupDatabase1")]
    partial class SetupDatabase1
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder.HasAnnotation("ProductVersion", "8.0.3");

            modelBuilder.Entity("InstrukcijeDotNet.Models.InstructionDate", b =>
                {
                    b.Property<int>("id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<DateTime?>("dateTime")
                        .HasColumnType("TEXT");

                    b.Property<int>("professorId")
                        .HasColumnType("INTEGER");

                    b.Property<string>("status")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<int>("studentId")
                        .HasColumnType("INTEGER");

                    b.HasKey("id");

                    b.ToTable("InstructionDates");
                });

            modelBuilder.Entity("InstrukcijeDotNet.Models.Professor", b =>
                {
                    b.Property<int>("id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("email")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<int>("instructionsCount")
                        .HasColumnType("INTEGER");

                    b.Property<string>("name")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("password")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("profilePictureUrl")
                        .HasColumnType("TEXT");

                    b.Property<string>("surname")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.HasKey("id");

                    b.ToTable("Professors");
                });

            modelBuilder.Entity("InstrukcijeDotNet.Models.ProfessorSubject", b =>
                {
                    b.Property<int>("id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<int>("professorId")
                        .HasColumnType("INTEGER");

                    b.Property<int>("subjectId")
                        .HasColumnType("INTEGER");

                    b.HasKey("id");

                    b.ToTable("ProfessorSubjects");
                });

            modelBuilder.Entity("InstrukcijeDotNet.Models.Student", b =>
                {
                    b.Property<int>("id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("email")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("name")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("password")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("profilePictureUrl")
                        .HasColumnType("TEXT");

                    b.Property<string>("surname")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.HasKey("id");

                    b.ToTable("Students");
                });

            modelBuilder.Entity("InstrukcijeDotNet.Models.Subject", b =>
                {
                    b.Property<int>("id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("description")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("title")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("url")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.HasKey("id");

                    b.ToTable("Subjects");
                });
#pragma warning restore 612, 618
        }
    }
}
