using Events.DATA.DTOs.Tickets.TicketTemplate;
using Events.Entities;
using Events.Entities.Book;
using Events.Entities.Ticket;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Newtonsoft.Json;
using SeatsioDotNet.Charts;

namespace Events.DATA
{
    public class DataContext : DbContext
    {
        public DataContext(DbContextOptions<DataContext> options) : base(options)
        {
        }


        public DbSet<AppUser> Users { get; set; }

        public DbSet<BaseChart> Charts { get; set; }
        public DbSet<BaseCategory> Categories { get; set; }

        public DbSet<EventEntity> Events { get; set; }

        public DbSet<Book> Books { get; set; }

        public DbSet<BookObject> BookObjects { get; set; }

        public DbSet<Ticket> Tickets { get; set; }

        public DbSet<TicketTemplate> TicketTemplates { get; set; }

        public DbSet<PointOfSale> PointOfSales { get; set; }

        public DbSet<Bill> Bills { get; set; }

        public DbSet<Tag> Tags { get; set; }

        public DbSet<EventTag> EventTags { get; set; }

        // EventFavorite
        public DbSet<EventFavorite> EventFavorites { get; set; }
        public DbSet<Notifications> Notifications { get; set; }
        public DbSet<SupportMessage> SupportMessages { get; set; }


        // public DbSet<Role> Roles { get; set; }
        // public DbSet<Permission> Permissions { get; set; }
        // public DbSet<RolePermission> RolePermissions { get; set; }


        // public DbSet<Article> Articles { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // modelBuilder.Entity<RolePermission>().HasKey(rp => new { rp.RoleId, rp.PermissionId });

            modelBuilder.Entity<AppUser>().HasData(
                new List<AppUser>()
                {
                    new AppUser()
                    {
                        Id = Guid.NewGuid(),
                        FullName = "Super Admin",
                        PhoneNumber = "07709509877",
                        Password = BCrypt.Net.BCrypt.HashPassword("123@root"),
                        WorkspacePublicKey = "4a337713-31a9-454e-8d18-51bc555eb390",
                        WorkspaceSecretKey = "6e44c7c4-de85-495e-b7de-08f6fdc2b229",
                        Role = UserRole.Admin
                    }
                }
            );


            modelBuilder.Entity<BookObject>()
                .OwnsOne(b => b.BookHoldInfo, bh =>
                {
                    bh.Property(b => b.ExpiredMinutes);
                    bh.Property(b => b.HoldToken);
                });


            // DataContext.cs
            modelBuilder.Entity<Tag>()
                .HasMany(t => t.EventTags)
                .WithOne(et => et.Tag)
                .HasForeignKey(et => et.TagId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<EventEntity>()
                .HasMany(e => e.EventTags)
                .WithOne(et => et.Event)
                .HasForeignKey(et => et.EventId)
                .OnDelete(DeleteBehavior.Cascade);


            modelBuilder.Entity<EventTag>()
                .HasKey(et => new { et.EventId, et.TagId });

            modelBuilder.Entity<EventTag>()
                .ToTable("EventTags");




            modelBuilder.Entity<PointOfSale>()
                .ToTable("PointOfSales")
                .HasBaseType<AppUser>();

            modelBuilder.Entity<EventPointOfSale>()
                .HasOne(epos => epos.Event)
                .WithMany()
                .HasForeignKey(epos => epos.EventId)
                .OnDelete(DeleteBehavior.Cascade);

            // modelBuilder.Entity<PointOfSale>()
            //     .HasOne(p => p.Provider)
            //     .WithMany() // You can specify a collection in AppUser if needed
            //     .HasForeignKey(p => p.ProviderId);
            //

            modelBuilder.Entity<AppUser>()
                .HasMany(pos => pos.Books)
                .WithOne(x => x.User)
                .HasForeignKey(b => b.UserId)
                .OnDelete(DeleteBehavior.SetNull);




            modelBuilder.Entity<BaseChart>()
                .HasMany(c => c.Categories)
                .WithOne(c => c.Chart)
                .HasForeignKey(c => c.ChartId)
                .OnDelete(DeleteBehavior.Cascade);


            // modelBuilder.Entity<Ticket>()
            //     .HasOne(x => x.BookObject)
            //     .WithOne(x => x.Ticket)
            //     .HasForeignKey<BookObject>(x => x.TicketId)
            //     .OnDelete(DeleteBehavior.Cascade);


            modelBuilder.Entity<BookObject>()
             .HasOne(x => x.Ticket)
             .WithOne(x => x.BookObject)
             .HasForeignKey<Ticket>(x => x.BookObjectId) 
             .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<BookObject>()
                .HasOne(x => x.Book)
                .WithMany()
                .HasForeignKey(x => x.BookId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Book>()
                .HasMany(b => b.Objects)
                .WithOne(bo => bo.Book)
                .HasForeignKey(bo => bo.BookId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Book>()
                .HasOne(b => b.Bill)
                .WithOne()
                .HasForeignKey<Bill>(b => b.BookId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Book>()
                .OwnsOne(b => b.BookHoldInfo, bh =>
                {
                    bh.Property(b => b.ExpiredMinutes);
                    bh.Property(b => b.HoldToken);
                });

            modelBuilder.HasSequence<long>("TicketNumbers", schema: "shared")
                .StartsAt(1000)
                .IncrementsBy(1);


            modelBuilder.Entity<Ticket>()
                .Property(t => t.Number)
                .HasDefaultValueSql("nextval('shared.\"TicketNumbers\"')");


            modelBuilder.Entity<TicketTemplate>()
                .OwnsOne(x => x.StageDetails, x =>
                {
                    x.Property(x => x.width);
                    x.Property(x => x.height);
                });


            modelBuilder.Entity<TicketTemplate>()
                .Property(e => e.Fields)
                .HasConversion(
                    // Convert List<TicketTemplateField> to JSON for storage
                    new ValueConverter<List<TicketTemplateField>, string>(
                        v => JsonConvert.SerializeObject(v),
                        v => JsonConvert.DeserializeObject<List<TicketTemplateField>>(v) ??
                             new List<TicketTemplateField>()
                    )
                )
                .HasColumnType("jsonb"); //

            ConfigureDateTimeProperties(modelBuilder);
        }

        private void ConfigureDateTimeProperties(ModelBuilder modelBuilder)
        {
            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                foreach (var property in entityType.GetProperties())
                {
                    if (property.ClrType == typeof(DateTime) || property.ClrType == typeof(DateTime?))
                    {
                        // Configure the property to use TIMESTAMP WITH TIME ZONE
                        property.SetColumnType("timestamptz");
                    }
                }
            }
        }
    }


}