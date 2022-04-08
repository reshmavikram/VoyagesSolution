using Data.Solution.Models.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace Data.Solution.Models
{
    public class DatabaseContext : DbContext
    {
        private string connection;
        public DatabaseContext(DbContextOptions<DatabaseContext> options) : base(options)
        {

        }
        public DatabaseContext()
        {

        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
        }
        public DatabaseContext(string _connection)
        {
            this.connection = _connection;
        }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlServer(this.connection);
            }
        }
        // Entities
        public DbSet<Policy> Policies { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<RolePolicyMapping> RolePolicyMapping { get; set; }
        public DbSet<UserRoleMapping> UserRoleMapping { get; set; }
        public DbSet<Port> Ports { get; set; }
        public DbSet<Vessel> Vessels { get; set; }
        public DbSet<FuelType> FuelTypes { get; set; }
        public DbSet<Event> Event { get; set; }
        public DbSet<VesselClass> VesselClass { get; set; }
        public DbSet<TermsType> TermsType { get; set; }
        public DbSet<CurrentFactor> CurrentFactor { get; set; }
        public DbSet<PassageTerms> PassageTerms { get; set; }
        public DbSet<PassageTermVesselMappings> PassageTermVesselMappings { get; set; }
        public DbSet<DraftLimit> DraftLimit { get; set; }
        public DbSet<EvaluationType> EvaluationType { get; set; }
        public DbSet<Speed> Speed { get; set; }
        public DbSet<WeatherSource> WeatherSource { get; set; }
        public DbSet<UnitofMeasure> UnitofMeasures { get; set; }
        public DbSet<VesselGroup> VesselGroup { get; set; }
        public DbSet<VesselGroupVesselMapping> VesselGroupVesselMapping { get; set; }
        public DbSet<Forms> Forms { get; set; }
        public DbSet<FuelCategory> FuelCategories { get; set; }
        public DbSet<ConsumptionCategory> ConsumptionCategory { get; set; }
        public DbSet<EngineCategory> EngineCategories { get; set; }
        public DbSet<NOXPMFactor> NOXPMFactor { get; set; }
        public DbSet<Performance> Performance { get; set; }
        public DbSet<PassageTermPerformance> PassageTermPerformance { get; set; }
        public DbSet<PoolTermsAssignedToVessel> PoolTermsAssignedToVessel { get; set; }
        public DbSet<PoolTermPerformance> PoolTermPerformance { get; set; }
        public DbSet<UnitOfMeasureThreshold> UnitOfMeasureThresholds { get; set; }
        public DbSet<PositionKPI> PositionKPIs { get; set; }
        public DbSet<PoolTerms> PoolTerms { get; set; }
        public DbSet<TermsType> TermsTypes { get; set; }
        public DbSet<PerformanceTermMapping> PerformanceTermMappings { get; set; }
        public DbSet<PassageTermsConsumptionCategoryMapping> PassageTermsConsumptionCategoryMapping { get; set; }
        public DbSet<PoolTermsConsumptionCategoryMapping> PoolTermsConsumptionCategoryMapping { get; set; }
        public DbSet<PassagePerformanceConsumptionCategoryMapping> PassagePerformanceConsumptionCategoryMapping { get; set; }
        public DbSet<VesselType> VesselTypes { get; set; }
        public DbSet<VesselClassGroupMapping> VesselClassGroupMapping { get; set; }
        public DbSet<FuelKPI> FuelKPIs { get; set; }
        public DbSet<VesselPooltype> VesselPooltypes { get; set; }
        public DbSet<CustomVesselField> CustomVesselFields { get; set; }
        public DbSet<AlertType> AlertTypes { get; set; }
        public DbSet<Rules> Rules { get; set; }
        public DbSet<Machinery> Machinery { get; set; }
        public DbSet<TcTerms> TcTerms { get; set; }
        public DbSet<Voyages> Voyages { get; set; }
        public DbSet<Module> Modules { get; set; }
        public DbSet<RoleModuleMapping> RoleModuleMappings { get; set; }
        public DbSet<DistanceUnits> DistanceUnits { get; set; }
        public DbSet<SevereWaveThresholdUnit> SevereWaveThresholdUnit { get; set; }
        public DbSet<WindSpeedUnits> WindSpeedUnits { get; set; }
        public DbSet<SeaHeightUnits> SeaHeightUnits { get; set; }
        public DbSet<FuelUnits> FuelUnits { get; set; }
        public DbSet<PowerUnits> PowerUnits { get; set; }
        public DbSet<ReportInPortActivityDaily> ReportInPortActivityDaily { get; set; }
        public DbSet<SevereWindThreshold> SevereWindThreshold { get; set; }
        public DbSet<VesselOffRouteDistanceUnit> VesselOffRouteDistanceUnit { get; set; }
        public DbSet<VelocityUnits> VelocityUnits { get; set; }
        public DbSet<UserVesselGroupMapping> UserVesselGroupMappings { get; set; }
        public DbQuery<FleetViewKPIStatusIndicator> SP_FleetViewKPIStatusIndicator { get; set; }
        public DbSet<Reports> Reports { get; set; }
        public DbSet<RoleReportMapping> RoleReportMappings { get; set; }
        public DbSet<PassagesApprovalAudits> PassagesApprovalAudits { get; set; }
        public DbSet<FuelsRows> FuelsRows { get; set; }
        public DbSet<EventROBsRow> EventROBsRow { get; set; }
        public DbSet<Robs> Robs { get; set; }
        public DbSet<Rob> Rob { get; set; }
        public DbSet<Allocation> Allocation { get; set; }
        public DbSet<BunkerType> BunkerTypes { get; set; }
        public DbSet<BerthingUnberthingDetails> BerthingUnberthingDetails { get; set; }
        public DbSet<BerthUnberthdetailsRow> BerthUnberthdetailsRows { get; set; }
        public DbSet<PassageWarning> PassageWarnings { get; set; }
        public DbSet<PositionWarning> PositionWarnings { get; set; }
        public DbSet<PositionWarningAudit> PositionWarningAudits { get; set; }
        public DbSet<PassageWarningAudit> PassageWarningAudits { get; set; }
        public DbSet<ExcludeReport> ExcludeReports { get; set; }
        public DbSet<ExcludeReportLog> ExcludeReportLogs { get; set; }
        public DbSet<PageSettings> PageSettings { get; set; }
        public DbSet<TemperatureUnits> TemperatureUnits { get; set; }
        public DbSet<PressureUnits> PressureUnits { get; set; }
        public DbSet<DirectionUnits> DirectionUnits { get; set; }
        public DbSet<FormUnits> FormUnits { get; set; }
        public DbSet<MeteoStratumData> MeteoStratumData { get; set; }
        public DbQuery<MeteoStratumDataList> MeteoStratumDataList { get; set; }
        public DbQuery<AnalyzedWeatherDataList> AnalyzedWeatherDataList { get; set; }
        public DbSet<AnalyzedWeather> AnalyzedWeather { get; set; }

        public DbSet<AlertSettings> AlertSettings { get; set; }
        public DbSet<AlertSettingsVesselMapping> AlertSettingsVesselMappings { get; set; }
        public DbSet<AlertSettingMappingType> AlertSettingMappingTypes { get; set; }
        public DbSet<StandardPort> StandardPorts { get; set; }
        public DbSet<CustomPort> CustomPorts { get; set; }
        public DbSet<ECAData> ECADatas { get; set; }
        public DbSet<About> Abouts { get; set; }
        public DbSet<AlertSettingEmailMapping> AlertSettingEmailMappings { get; set; }
        public DbSet<FleetViewKPISetting> FleetViewKPISettings { get; set; }
        public DbSet<LoginLogoutLog> LoginLogoutLogs { get; set; }
        public DbQuery<FleetPassageLatestReportViewModel> SP_getFleetViewPassageData { get; set; }
        public DbQuery<FleetPassageDataViewModel> SP_getFleetViewPassage { get; set; }
        public DbSet<BunkerVeslinkFuelType> BunkerVeslinkFuelTypes { get; set; }
        public DbSet<DirectionValueMapping> DirectionValueMappings { get; set; }
        public DbSet<PoolTermConsumptionCategoryFuelGrouping> PoolTermConsumptionCategoryFuelGroupings { get; set; }

        public DbSet<EmissionReportConstantValue> EmissionReportConstantValues { get; set; } //added by prashant
        public DbSet<AdditionalColumn> AdditionalColumns { get; set; } //added by prashant on 23-12-21
    }
}
