using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Events.Migrations
{
    /// <inheritdoc />
    public partial class Init : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "shared");

            migrationBuilder.CreateSequence(
                name: "TicketNumbers",
                schema: "shared",
                startValue: 1000L);

            migrationBuilder.CreateTable(
                name: "Notifications",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    NotifyId = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "text", nullable: true),
                    Description = table.Column<string>(type: "text", nullable: true),
                    Picture = table.Column<string>(type: "text", nullable: true),
                    NotifyFor = table.Column<string>(type: "text", nullable: true),
                    Date = table.Column<DateTime>(type: "timestamptz", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    IsRead = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamptz", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Notifications", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SupportMessages",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PhoneNumber = table.Column<string>(type: "text", nullable: true),
                    FullName = table.Column<string>(type: "text", nullable: true),
                    Message = table.Column<string>(type: "text", nullable: true),
                    Deleted = table.Column<bool>(type: "boolean", nullable: false),
                    CreationDate = table.Column<DateTime>(type: "timestamptz", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SupportMessages", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Tags",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: true),
                    Image = table.Column<string>(type: "text", nullable: true),
                    Deleted = table.Column<bool>(type: "boolean", nullable: false),
                    CreationDate = table.Column<DateTime>(type: "timestamptz", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tags", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FullName = table.Column<string>(type: "text", nullable: true),
                    PhoneNumber = table.Column<string>(type: "text", nullable: true),
                    Password = table.Column<string>(type: "text", nullable: true),
                    WorkspacePublicKey = table.Column<string>(type: "text", nullable: true),
                    WorkspaceSecretKey = table.Column<string>(type: "text", nullable: true),
                    Role = table.Column<int>(type: "integer", nullable: false),
                    Deleted = table.Column<bool>(type: "boolean", nullable: false),
                    CreationDate = table.Column<DateTime>(type: "timestamptz", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Charts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    RelatedChartId = table.Column<long>(type: "bigint", nullable: false),
                    ChartKey = table.Column<string>(type: "text", nullable: false),
                    PublishedVersionThumbnailUrl = table.Column<string>(type: "text", nullable: false),
                    DraftVersionThumbnailUrl = table.Column<string>(type: "text", nullable: true),
                    WorkspaceKey = table.Column<string>(type: "text", nullable: true),
                    UserId = table.Column<Guid>(type: "uuid", nullable: true),
                    IsTemplate = table.Column<bool>(type: "boolean", nullable: false),
                    Deleted = table.Column<bool>(type: "boolean", nullable: false),
                    CreationDate = table.Column<DateTime>(type: "timestamptz", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Charts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Charts_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "PointOfSales",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PhoneNumbers = table.Column<List<string>>(type: "text[]", nullable: true),
                    Description = table.Column<string>(type: "text", nullable: true),
                    Address = table.Column<string>(type: "text", nullable: true),
                    Lat = table.Column<string>(type: "text", nullable: true),
                    Lng = table.Column<string>(type: "text", nullable: true),
                    Image = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PointOfSales", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PointOfSales_Users_Id",
                        column: x => x.Id,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Categories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Color = table.Column<string>(type: "text", nullable: false),
                    Price = table.Column<decimal>(type: "numeric", nullable: false),
                    ChartId = table.Column<Guid>(type: "uuid", nullable: false),
                    Deleted = table.Column<bool>(type: "boolean", nullable: false),
                    CreationDate = table.Column<DateTime>(type: "timestamptz", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Categories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Categories_Charts_ChartId",
                        column: x => x.ChartId,
                        principalTable: "Charts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Events",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: true),
                    StartEvent = table.Column<DateTime>(type: "timestamptz", nullable: true),
                    EndEvent = table.Column<DateTime>(type: "timestamptz", nullable: true),
                    Description = table.Column<string>(type: "text", nullable: true),
                    Attachments = table.Column<List<string>>(type: "text[]", nullable: true),
                    ChartId = table.Column<Guid>(type: "uuid", nullable: true),
                    EventKey = table.Column<string>(type: "text", nullable: true),
                    SlugHash = table.Column<string>(type: "text", nullable: true),
                    IsPublish = table.Column<bool>(type: "boolean", nullable: true),
                    StartReservationDate = table.Column<DateTime>(type: "timestamptz", nullable: true),
                    EndReservationDate = table.Column<DateTime>(type: "timestamptz", nullable: true),
                    UsedTickets = table.Column<int>(type: "integer", nullable: true),
                    Lat = table.Column<double>(type: "double precision", nullable: true),
                    Lng = table.Column<double>(type: "double precision", nullable: true),
                    Address = table.Column<string>(type: "text", nullable: true),
                    SeatAvailable = table.Column<int>(type: "integer", nullable: true),
                    SeatBooked = table.Column<int>(type: "integer", nullable: true),
                    IsFeature = table.Column<bool>(type: "boolean", nullable: true),
                    Deleted = table.Column<bool>(type: "boolean", nullable: false),
                    CreationDate = table.Column<DateTime>(type: "timestamptz", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Events", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Events_Charts_ChartId",
                        column: x => x.ChartId,
                        principalTable: "Charts",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Books",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EventId = table.Column<Guid>(type: "uuid", nullable: true),
                    TotalPrice = table.Column<decimal>(type: "numeric", nullable: false),
                    IsPaid = table.Column<bool>(type: "boolean", nullable: true),
                    UserId = table.Column<Guid>(type: "uuid", nullable: true),
                    BookHoldInfoHoldToken = table.Column<string>(name: "BookHoldInfo_HoldToken", type: "text", nullable: true),
                    BookHoldInfoExpiredMinutes = table.Column<int>(name: "BookHoldInfo_ExpiredMinutes", type: "integer", nullable: true),
                    Discount = table.Column<decimal>(type: "numeric", nullable: true),
                    Deleted = table.Column<bool>(type: "boolean", nullable: false),
                    CreationDate = table.Column<DateTime>(type: "timestamptz", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Books", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Books_Events_EventId",
                        column: x => x.EventId,
                        principalTable: "Events",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Books_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "EventFavorites",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: true),
                    EventId = table.Column<Guid>(type: "uuid", nullable: true),
                    Deleted = table.Column<bool>(type: "boolean", nullable: false),
                    CreationDate = table.Column<DateTime>(type: "timestamptz", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EventFavorites", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EventFavorites_Events_EventId",
                        column: x => x.EventId,
                        principalTable: "Events",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_EventFavorites_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "EventPointOfSale",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EventId = table.Column<Guid>(type: "uuid", nullable: true),
                    PointOfSaleId = table.Column<Guid>(type: "uuid", nullable: true),
                    EventEntityId = table.Column<Guid>(type: "uuid", nullable: true),
                    Deleted = table.Column<bool>(type: "boolean", nullable: false),
                    CreationDate = table.Column<DateTime>(type: "timestamptz", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EventPointOfSale", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EventPointOfSale_Events_EventEntityId",
                        column: x => x.EventEntityId,
                        principalTable: "Events",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_EventPointOfSale_Events_EventId",
                        column: x => x.EventId,
                        principalTable: "Events",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EventPointOfSale_PointOfSales_PointOfSaleId",
                        column: x => x.PointOfSaleId,
                        principalTable: "PointOfSales",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "EventTags",
                columns: table => new
                {
                    EventId = table.Column<Guid>(type: "uuid", nullable: false),
                    TagId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EventTags", x => new { x.EventId, x.TagId });
                    table.ForeignKey(
                        name: "FK_EventTags_Events_EventId",
                        column: x => x.EventId,
                        principalTable: "Events",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EventTags_Tags_TagId",
                        column: x => x.TagId,
                        principalTable: "Tags",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TicketTemplates",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EventId = table.Column<Guid>(type: "uuid", nullable: false),
                    Image = table.Column<string>(type: "text", nullable: true),
                    StageDetailswidth = table.Column<string>(name: "StageDetails_width", type: "text", nullable: true),
                    StageDetailsheight = table.Column<string>(name: "StageDetails_height", type: "text", nullable: true),
                    Fields = table.Column<string>(type: "jsonb", nullable: true),
                    Deleted = table.Column<bool>(type: "boolean", nullable: false),
                    CreationDate = table.Column<DateTime>(type: "timestamptz", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TicketTemplates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TicketTemplates_Events_EventId",
                        column: x => x.EventId,
                        principalTable: "Events",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Bills",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    BookId = table.Column<Guid>(type: "uuid", nullable: true),
                    BookId1 = table.Column<Guid>(type: "uuid", nullable: true),
                    TotalPrice = table.Column<decimal>(type: "numeric", nullable: true),
                    BillId = table.Column<string>(type: "text", nullable: true),
                    PaymentStatus = table.Column<int>(type: "integer", nullable: true),
                    PaymentDate = table.Column<DateTime>(type: "timestamptz", nullable: true),
                    Deleted = table.Column<bool>(type: "boolean", nullable: false),
                    CreationDate = table.Column<DateTime>(type: "timestamptz", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Bills", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Bills_Books_BookId",
                        column: x => x.BookId,
                        principalTable: "Books",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Bills_Books_BookId1",
                        column: x => x.BookId1,
                        principalTable: "Books",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "BookObjects",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FullName = table.Column<string>(type: "text", nullable: true),
                    PhoneNumber = table.Column<string>(type: "text", nullable: true),
                    Name = table.Column<string>(type: "text", nullable: true),
                    Price = table.Column<decimal>(type: "numeric", nullable: false),
                    CategoryId = table.Column<Guid>(type: "uuid", nullable: true),
                    Type = table.Column<string>(type: "text", nullable: true),
                    BookId = table.Column<Guid>(type: "uuid", nullable: false),
                    BookHoldInfoHoldToken = table.Column<string>(name: "BookHoldInfo_HoldToken", type: "text", nullable: true),
                    BookHoldInfoExpiredMinutes = table.Column<int>(name: "BookHoldInfo_ExpiredMinutes", type: "integer", nullable: true),
                    IsCanceled = table.Column<bool>(type: "boolean", nullable: false),
                    Deleted = table.Column<bool>(type: "boolean", nullable: false),
                    CreationDate = table.Column<DateTime>(type: "timestamptz", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BookObjects", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BookObjects_Books_BookId",
                        column: x => x.BookId,
                        principalTable: "Books",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BookObjects_Categories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "Categories",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Tickets",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Number = table.Column<long>(type: "bigint", nullable: false, defaultValueSql: "nextval('shared.\"TicketNumbers\"')"),
                    TicketSeating = table.Column<string>(type: "text", nullable: true),
                    SeatCategory = table.Column<string>(type: "text", nullable: true),
                    BookObjectId = table.Column<Guid>(type: "uuid", nullable: true),
                    IsUsed = table.Column<bool>(type: "boolean", nullable: false),
                    Deleted = table.Column<bool>(type: "boolean", nullable: false),
                    CreationDate = table.Column<DateTime>(type: "timestamptz", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tickets", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Tickets_BookObjects_BookObjectId",
                        column: x => x.BookObjectId,
                        principalTable: "BookObjects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "CreationDate", "Deleted", "FullName", "Password", "PhoneNumber", "Role", "WorkspacePublicKey", "WorkspaceSecretKey" },
                values: new object[] { new Guid("62d27f63-4355-4e11-ae12-a449a8df4272"), new DateTime(2025, 10, 30, 2, 4, 31, 221, DateTimeKind.Utc).AddTicks(7219), false, "Super Admin", "$2a$10$nIpRLxBC5Z7mV2rRYavAgeEQQHWft2cjE/RTjPCBxA/9bS0j.AuP6", "07709509877", 0, "4a337713-31a9-454e-8d18-51bc555eb390", "6e44c7c4-de85-495e-b7de-08f6fdc2b229" });

            migrationBuilder.CreateIndex(
                name: "IX_Bills_BookId",
                table: "Bills",
                column: "BookId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Bills_BookId1",
                table: "Bills",
                column: "BookId1");

            migrationBuilder.CreateIndex(
                name: "IX_BookObjects_BookId",
                table: "BookObjects",
                column: "BookId");

            migrationBuilder.CreateIndex(
                name: "IX_BookObjects_CategoryId",
                table: "BookObjects",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Books_EventId",
                table: "Books",
                column: "EventId");

            migrationBuilder.CreateIndex(
                name: "IX_Books_UserId",
                table: "Books",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Categories_ChartId",
                table: "Categories",
                column: "ChartId");

            migrationBuilder.CreateIndex(
                name: "IX_Charts_UserId",
                table: "Charts",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_EventFavorites_EventId",
                table: "EventFavorites",
                column: "EventId");

            migrationBuilder.CreateIndex(
                name: "IX_EventFavorites_UserId",
                table: "EventFavorites",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_EventPointOfSale_EventEntityId",
                table: "EventPointOfSale",
                column: "EventEntityId");

            migrationBuilder.CreateIndex(
                name: "IX_EventPointOfSale_EventId",
                table: "EventPointOfSale",
                column: "EventId");

            migrationBuilder.CreateIndex(
                name: "IX_EventPointOfSale_PointOfSaleId",
                table: "EventPointOfSale",
                column: "PointOfSaleId");

            migrationBuilder.CreateIndex(
                name: "IX_Events_ChartId",
                table: "Events",
                column: "ChartId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EventTags_TagId",
                table: "EventTags",
                column: "TagId");

            migrationBuilder.CreateIndex(
                name: "IX_Tickets_BookObjectId",
                table: "Tickets",
                column: "BookObjectId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TicketTemplates_EventId",
                table: "TicketTemplates",
                column: "EventId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Bills");

            migrationBuilder.DropTable(
                name: "EventFavorites");

            migrationBuilder.DropTable(
                name: "EventPointOfSale");

            migrationBuilder.DropTable(
                name: "EventTags");

            migrationBuilder.DropTable(
                name: "Notifications");

            migrationBuilder.DropTable(
                name: "SupportMessages");

            migrationBuilder.DropTable(
                name: "Tickets");

            migrationBuilder.DropTable(
                name: "TicketTemplates");

            migrationBuilder.DropTable(
                name: "PointOfSales");

            migrationBuilder.DropTable(
                name: "Tags");

            migrationBuilder.DropTable(
                name: "BookObjects");

            migrationBuilder.DropTable(
                name: "Books");

            migrationBuilder.DropTable(
                name: "Categories");

            migrationBuilder.DropTable(
                name: "Events");

            migrationBuilder.DropTable(
                name: "Charts");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropSequence(
                name: "TicketNumbers",
                schema: "shared");
        }
    }
}
