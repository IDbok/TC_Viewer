using Microsoft.EntityFrameworkCore;
using TcModels.Models;
using TcModels.Models.IntermediateTables;
using TcModels.Models.TcContent;

namespace TcDbConnector
{
    public class MyDbContext : DbContext
    {
        public DbSet<TechnologicalProcess> TechnologicalProcesses { get; set; } = null!; // технологический процесс
        public DbSet<TechnologicalCard> TechnologicalCards { get; set; } = null!; // Технические карты
        public DbSet<Author> Authors { get; set; } = null!; // авторизация
        public DbSet<Staff> Staffs { get; set; } = null!; //персонал
        public DbSet<Component> Components { get; set; } = null!; //таблица 2
        public DbSet<Tool> Tools { get; set; } = null!;  // таблица 5 Инструменты
        public DbSet<Machine> Machines { get; set; } = null!; //таблица 3
        public DbSet<Protection> Protections { get; set; } = null!; //Таблица 4 Средства защиты
        //public DbSet<WorkStep> WorkSteps { get; set; } = null!;

        public DbSet<Staff_TC> Staff_TCs { get; set; } = null!;
        public DbSet<Component_TC> Component_TCs { get; set; } = null!;
        public DbSet<Machine_TC> Machine_TCs { get; set; } = null!;
        public DbSet<Protection_TC> Protection_TCs { get; set; } = null!;
        public DbSet<Tool_TC> Tool_TCs { get; set; } = null!;


        public MyDbContext()
        {
            // Database.EnsureDeleted();
            //Database.EnsureCreated(); // todo - create exception catch if db is unavailable
        }
      
        
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseMySql("server=localhost;database=tavrida_db_v5;user=root;password=root",
                new MySqlServerVersion(new Version(5, 7, 24)));

        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder
                .Entity<Component>()
                .HasMany(cp => cp.Parents)
                .WithMany(ch => ch.Children)
                .UsingEntity<Instrument_kit<Component>>(
                    j => j
                        .HasOne(sttc => sttc.Parent)
                        .WithMany(st => st.Kit)
                        .HasForeignKey(sttc => sttc.ParentId),
                    j => j
                        .HasOne(sttc => sttc.Child)
                        .WithMany() //st => st.Kit)
                        .HasForeignKey(sttc => sttc.ChildId),
                    j =>
                    {
                        j.Property(sttc => sttc.Quantity).IsRequired().HasDefaultValue(0);
                        j.Property(sttc => sttc.Order).IsRequired().HasDefaultValue(0);
                        j.HasKey(t => new { t.ParentId, t.ChildId, t.Order });
                        //j.ToTable("Instrument_kit");
                    });
            
            modelBuilder
                .Entity<TechnologicalCard>()
                .HasMany(tc => tc.Staffs)
                .WithMany(st => st.TechnologicalCards)
                .UsingEntity<Staff_TC>(
                    j => j
                        .HasOne(sttc => sttc.Child)
                        .WithMany(st => st.Staff_TCs)
                        .HasForeignKey(sttc => sttc.ChildId),
                    j => j
                        .HasOne(sttc => sttc.Parent)
                        .WithMany(st => st.Staff_TCs)
                        .HasForeignKey(sttc => sttc.ParentId),
                    j =>
                    {
                        j.Property(sttc => sttc.Symbol).HasDefaultValue("-");
                        j.Property(sttc => sttc.Order).HasDefaultValue(0);
                        j.HasKey(t => new { t.ParentId, t.ChildId, t.Symbol});
                        j.ToTable("Staff_TC");
                        // j.HasIndex(t => t.Symbol).IsUnique();
                    });
            modelBuilder
                .Entity<TechnologicalCard>()
                .HasMany(tc => tc.Components)
                .WithMany(st => st.TechnologicalCards)
                .UsingEntity<Component_TC>(
                    j => j
                        .HasOne(sttc => sttc.Child)
                        .WithMany(st => st.Component_TCs)
                        .HasForeignKey(sttc => sttc.ChildId),
                    j => j
                        .HasOne(sttc => sttc.Parent)
                        .WithMany(st => st.Component_TCs)
                        .HasForeignKey(sttc => sttc.ParentId),
                    j =>
                    {
                        j.Property(sttc => sttc.Quantity).HasDefaultValue(0);
                        j.Property(sttc => sttc.Order).HasDefaultValue(0);
                        j.HasKey(t => new { t.ParentId, t.ChildId, t.Order });
                        j.ToTable("Component_TC");
                    });
            modelBuilder
                .Entity<TechnologicalCard>()
                .HasMany(tc => tc.Machines)
                .WithMany(st => st.TechnologicalCards)
                .UsingEntity<Machine_TC>(
                    j => j
                        .HasOne(sttc => sttc.Child)
                        .WithMany(st => st.Machine_TCs)
                        .HasForeignKey(sttc => sttc.ChildId),
                    j => j
                        .HasOne(sttc => sttc.Parent)
                        .WithMany(st => st.Machine_TCs)
                        .HasForeignKey(sttc => sttc.ParentId),
                    j =>
                    {
                        j.Property(sttc => sttc.Quantity).HasDefaultValue(0);
                        j.Property(sttc => sttc.Order).HasDefaultValue(0);
                        j.HasKey(t => new { t.ParentId, t.ChildId });
                        j.ToTable("Machine_TC");
                    });
            modelBuilder
                .Entity<TechnologicalCard>()
                .HasMany(tc => tc.Protections)
                .WithMany(st => st.TechnologicalCards)
                .UsingEntity<Protection_TC>(
                    j => j
                        .HasOne(sttc => sttc.Child)
                        .WithMany(st => st.Protection_TCs)
                        .HasForeignKey(sttc => sttc.ChildId),
                    j => j
                        .HasOne(sttc => sttc.Parent)
                        .WithMany(st => st.Protection_TCs)
                        .HasForeignKey(sttc => sttc.ParentId),
                    j =>
                    {
                        j.Property(sttc => sttc.Quantity).HasDefaultValue(0);
                        j.Property(sttc => sttc.Order).HasDefaultValue(0);
                        j.HasKey(t => new { t.ParentId, t.ChildId });
                        j.ToTable("Protection_TC");
                    });
            modelBuilder
                .Entity<TechnologicalCard>()
                .HasMany(tc => tc.Tools)
                .WithMany(st => st.TechnologicalCards)
                .UsingEntity<Tool_TC>(
                    j => j
                        .HasOne(sttc => sttc.Child)
                        .WithMany(st => st.Tool_TCs)
                        .HasForeignKey(sttc => sttc.ChildId),
                    j => j
                        .HasOne(sttc => sttc.Parent)
                        .WithMany(st => st.Tool_TCs)
                        .HasForeignKey(sttc => sttc.ParentId),
                    j =>
                    {
                        j.Property(sttc => sttc.Quantity).HasDefaultValue(0);
                        j.Property(sttc => sttc.Order).HasDefaultValue(0);
                        j.HasKey(t => new { t.ParentId, t.ChildId, t.Order });
                        j.ToTable("Tool_TC");
                    });

        }

    }
}
