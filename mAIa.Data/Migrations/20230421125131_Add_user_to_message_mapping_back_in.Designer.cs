﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using mAIa.Data;

#nullable disable

namespace mAIa.Data.Migrations
{
    [DbContext(typeof(mAIaDataContext))]
    [Migration("20230421125131_Add_user_to_message_mapping_back_in")]
    partial class Add_user_to_message_mapping_back_in
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "7.0.5")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder);

            modelBuilder.Entity("mAIa.Data.Model.Message", b =>
                {
                    b.Property<Guid>("MessageID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<decimal>("ChannelID")
                        .HasColumnType("decimal(20,0)");

                    b.Property<string>("Content")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("ContentTokenCount")
                        .HasColumnType("int");

                    b.Property<decimal?>("DiscordUserID")
                        .HasColumnType("decimal(20,0)");

                    b.Property<string>("DiscordUsername")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("MessageType")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("PlainText")
                        .HasColumnType("nvarchar(max)");

                    b.Property<decimal>("ServerID")
                        .HasColumnType("decimal(20,0)");

                    b.Property<string>("Summary")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int?>("SummaryTokenCount")
                        .HasColumnType("int");

                    b.Property<DateTime>("Timestamp")
                        .HasColumnType("datetime2");

                    b.HasKey("MessageID");

                    b.HasIndex("DiscordUserID");

                    b.ToTable("Messages", "Chat");
                });

            modelBuilder.Entity("mAIa.Data.Model.User", b =>
                {
                    b.Property<decimal>("DiscordUserID")
                        .HasColumnType("decimal(20,0)");

                    b.Property<string>("DiscordUsername")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("DiscordUserID");

                    b.ToTable("Users", "Chat");
                });

            modelBuilder.Entity("mAIa.Data.Model.UserTrait", b =>
                {
                    b.Property<Guid>("UserTraitID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<DateTime>("DateAdded")
                        .HasColumnType("datetime2");

                    b.Property<decimal>("DiscordUserID")
                        .HasColumnType("decimal(20,0)");

                    b.Property<string>("Key")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("RelevanceToUser")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Value")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("UserTraitID");

                    b.HasIndex("DiscordUserID");

                    b.ToTable("UserTraits", "Chat");
                });

            modelBuilder.Entity("mAIa.Data.Model.Message", b =>
                {
                    b.HasOne("mAIa.Data.Model.User", "User")
                        .WithMany("Messages")
                        .HasForeignKey("DiscordUserID");

                    b.Navigation("User");
                });

            modelBuilder.Entity("mAIa.Data.Model.UserTrait", b =>
                {
                    b.HasOne("mAIa.Data.Model.User", "User")
                        .WithMany("Traits")
                        .HasForeignKey("DiscordUserID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("User");
                });

            modelBuilder.Entity("mAIa.Data.Model.User", b =>
                {
                    b.Navigation("Messages");

                    b.Navigation("Traits");
                });
#pragma warning restore 612, 618
        }
    }
}
