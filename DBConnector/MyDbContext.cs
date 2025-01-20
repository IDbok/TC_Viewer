using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using TcModels.Models;
using TcModels.Models.IntermediateTables;
using TcModels.Models.TcContent;
using TcModels.Models.TcContent.Work;

namespace TcDbConnector;

public class MyDbContext : DbContext
{
    public DbSet<TechnologicalCard> TechnologicalCards { get; set; } = null!; // Технические карты
    public DbSet<Author> Authors { get; set; } = null!; // авторизация
    public DbSet<TechnologicalProcess> TechnologicalProcesses { get; set; } = null!; // технологический процесс
    public DbSet<Staff> Staffs { get; set; } = null!; //персонал
    public DbSet<Component> Components { get; set; } = null!; //таблица 2
    public DbSet<Tool> Tools { get; set; } = null!;  // таблица 5 Инструменты
    public DbSet<Machine> Machines { get; set; } = null!; //таблица 3
    public DbSet<Protection> Protections { get; set; } = null!; //Таблица 4 Средства защиты

    public DbSet<TechOperation> TechOperations { get; set; } = null!;
    public DbSet<TechTransition> TechTransitions { get; set; } = null!;

    public DbSet<TechOperationWork> TechOperationWorks { get; set; } = null!;
    public DbSet<ExecutionWork> ExecutionWorks { get; set; } = null!;
    public DbSet<ExecutionWorkRepeat> ExecutionWorkRepeats { get; set; } = null!;

    public DbSet<ComponentWork> ComponentWorks { get; set; } = null!;
    public DbSet<ToolWork> ToolWorks { get; set; } = null!;

    public DbSet<Staff_TC> Staff_TCs { get; set; } = null!;
    public DbSet<Component_TC> Component_TCs { get; set; } = null!;
    public DbSet<Machine_TC> Machine_TCs { get; set; } = null!;
    public DbSet<Protection_TC> Protection_TCs { get; set; } = null!;
    public DbSet<Tool_TC> Tool_TCs { get; set; } = null!;

    public DbSet<TechTransitionTypical> TechTransitionTypicals { get; set; } = null!;

    public DbSet<StaffRelationship> StaffRelationship { get; set; } = null!;


    public DbSet<DiagamToWork> DiagamToWork { get; set; } = null!;
    public DbSet<DiagramParalelno> DiagramParalelno { get; set; } = null!;
    public DbSet<DiagramPosledov> DiagramPosledov { get; set; } = null!;
    public DbSet<DiagramShag> DiagramShag { get; set; } = null!;
    public DbSet<DiagramShagToolsComponent> DiagramShagToolsComponent { get; set; } = null!;

    public DbSet<ImageStorage> ImageStorage { get; set; }
    public DbSet<ObjectLocker> BlockedConcurrencyObjects { get; set; } = null!;
    public DbSet<Outlay> OutlaysTable { get; set; } = null!;

    public DbSet<Coefficient> Coefficients { get; set; } = null!;

	public MyDbContext()
    {
        //Database.EnsureDeleted();
        //Database.EnsureCreated(); // todo - create exception catch if db is unavailable

        Database.SetCommandTimeout(120);
    }
    //public MyDbContext(DbContextOptions<MyDbContext> options)
    //      : base(options)
    //{
    //}

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
	{
		var connectString = TcDbConnector.StaticClass.ConnectString;
		// Проверить, если TcDbConnector.StaticClass.ConnectString не пустой, то использовать его
		// иначе, использовать строку подключения из appsettings.json
		if (string.IsNullOrEmpty(connectString))
		{
#if DEBUG
			connectString = "server=localhost;database=tavrida_db_main;user=root;password=root";
#else
            throw new Exception("Не задана строка подключения к БД");
#endif
		}

		optionsBuilder
			.UseMySql(connectString,
			new MySqlServerVersion(new Version(5, 7, 24)));

	}

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        //modelBuilder
        //    .Entity<ExecutionWork>()
        //    .HasMany(c => c.ListexecutionWorkRepeat)
        //    .WithMany(s => s.ListexecutionWorkRepeat2)
        //    .UsingEntity(j => j.ToTable("ExecutionWorkRepeat"));

        modelBuilder
            .Entity<ExecutionWork>()
            .HasMany(e => e.ExecutionWorkRepeats)
            .WithOne(link => link.ParentExecutionWork)
            .HasForeignKey(link => link.ParentExecutionWorkId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder
            .Entity<ExecutionWorkRepeat>()
            .HasOne(link => link.ChildExecutionWork)
            .WithMany()
            .HasForeignKey(link => link.ChildExecutionWorkId)
            .OnDelete(DeleteBehavior.Cascade);

        // Исключил т.к. не используется (Игорь)
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
                    j.HasKey(t => new { t.ParentId, t.ChildId });
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
                    j.Property(f => f.IdAuto).ValueGeneratedOnAdd();
                    j.HasKey(t => new {  t.IdAuto}); //t.ParentId, t.ChildId,
                    j.ToTable("Staff_TC");
                    // j.HasIndex(t => t.Symbol).IsUnique();
                });
        //modelBuilder.Entity<Staff_TC>()
        //    .HasKey(st => st.IdAuto);
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
                    j.HasKey(t => new { t.ParentId, t.ChildId});
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
                    j.HasKey(t => new { t.ParentId, t.ChildId });
                    j.ToTable("Tool_TC");
                });
        modelBuilder
            .Entity<Staff>()
            .HasMany(cp => cp.RelatedStaffs)
            .WithMany()
            .UsingEntity<StaffRelationship>(
                j => j
                    .HasOne(sr => sr.RelatedStaff)
                    .WithMany() 
                    .HasForeignKey(sr => sr.RelatedStaffId)
                    .OnDelete(DeleteBehavior.Cascade),
                j => j
                    .HasOne(sr => sr.Staff)
                    .WithMany()
                    .HasForeignKey(sr => sr.StaffId)
                    .OnDelete(DeleteBehavior.Cascade),
                j =>
                {
                    j.HasKey(t => t.Id);
                    //j.ToTable("Instrument_kit");
                });

        modelBuilder
        .Entity<TechnologicalCard>()
        .HasOne(tc => tc.ExecutionSchemeImage)
        .WithMany()
        .HasForeignKey(tc => tc.ExecutionSchemeImageId)
        .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<ImageStorage>()
        .Property(e => e.StorageType)
        .HasConversion<int>();

        modelBuilder.Entity<ImageStorage>()
            .Property(e => e.Category)
            .HasConversion<int>();
        
        modelBuilder.Entity<TechOperationWork>()
            .Property<string>("ParallelIndex")
            .HasColumnName("ParallelIndex");

		// Настройка отношения ToolWork -> DiagramShagToolsComponent
		modelBuilder.Entity<DiagramShagToolsComponent>()
			.HasOne(dstc => dstc.toolWork)
			.WithMany()
			.HasForeignKey(dstc => dstc.toolWorkId)
			.OnDelete(DeleteBehavior.Cascade);

		// Настройка отношения ComponentWork -> DiagramShagToolsComponent
		modelBuilder.Entity<DiagramShagToolsComponent>()
			.HasOne(dstc => dstc.componentWork)
			.WithMany()
			.HasForeignKey(dstc => dstc.componentWorkId)
			.OnDelete(DeleteBehavior.Cascade);

	}

}
