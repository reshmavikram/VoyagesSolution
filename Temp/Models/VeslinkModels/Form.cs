using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using static Data.Solution.Helpers.ApiHelper;

namespace Data.Solution.Models
{
    public class FormClasses
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long SFPM_FormId { get; set; }
        public List<Forms> Form { get; set; }
    }
    [Serializable]
    public class Forms
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long SFPM_Form_Id { get; set; }

        public string FormIdentifier { get; set; }

        public string CompanyCode { get; set; }

        public string CompanyName { get; set; }

        public string VesselCode { get; set; }

        public DateTimeOffset? SubmittedDate { get; set; }

        public string Status { get; set; }

        public DateTimeOffset? ApprovedDate { get; set; }

        public string FormGUID { get; set; }

        public int? ImoNumber { get; set; }

        public string UnCode { get; set; }
        public string VesselName { get; set; }
        public DateTimeOffset? ReportDateTime { get; set; }
        public int? VoyageNo { get; set; }
        public string Port { get; set; }
        public string PortOrPassage { get; set; }
        public string ForcedBoilOff { get; set; }
        public string Slip { get; set; }
        public string ChartererCleaningKitOnboard { get; set; }
        public string ReferencePort { get; set; }
        public string Latitude { get; set; }
        public string Longitude { get; set; }
        public DateTimeOffset? ETD { get; set; }
        public string LngReadingType { get; set; }
        public string Remarks { get; set; }
        public string DraftUOM { get; set; }
        public string CPSpeed { get; set; }
        public string ObsSpeed { get; set; }
        public string ObservedDistance { get; set; }
        public string EngineDistance { get; set; }
        public string MECounter { get; set; }
        public string MEOutputPct { get; set; }
        public string BoilerHrs { get; set; }
        public string IncineratorHrs { get; set; }
        public string GenFWHrs { get; set; }
        public string Salinity { get; set; }
        public string Ballast { get; set; }
        public string Gen1Hrs { get; set; }
        public string Gen2Hrs { get; set; }
        public string Gen3Hrs { get; set; }
        public string Gen4Hrs { get; set; }
        public string MEKWhrs { get; set; }
        public string Gen1KWhrs { get; set; }
        public string Gen2KWhrs { get; set; }
        public string Gen3KWhrs { get; set; }
        public string Gen4KWhrs { get; set; }
        public string MainEngineHrs { get; set; }
        public string RPM { get; set; }
        public string AvgBHP { get; set; }
        public string FWDDraft { get; set; }
        public string MIDDraft { get; set; }
        public string AFTDraft { get; set; }
        public string Heading { get; set; }
        public string SteamingHrs { get; set; }
        public string DWT { get; set; }
        public string Displacement { get; set; }
        public string VesselCondition { get; set; }
        public string CrewStores { get; set; }
        public string Lubes { get; set; }
        public decimal? Constant { get; set; }
        public int? CargoWeight { get; set; }
        public string DraftPortFwd { get; set; }
        public string DraftPortMid { get; set; }
        public string DraftPortAft { get; set; }
        public string DraftStarboardFwd { get; set; }
        public string DraftStarboardMid { get; set; }
        public string DraftStarboardAft { get; set; }
        public string CurrentLocGaugeName { get; set; }
        public string RestrictedLocGaugeName { get; set; }
        public string CurrentLocWaterLevel { get; set; }
        public string RestrictedLocWaterLevel { get; set; }
        public string CurrentLocWaterLevelUnit { get; set; }
        public string RestrictedLocWaterLevelUnit { get; set; }
        public string AirTemp { get; set; }
        public string BaroPressure { get; set; }
        public string SeaState { get; set; }
        public string Swell { get; set; }
        public string WindForce { get; set; }
        public string Current { get; set; }
        public string SeaTemp { get; set; }
        public string BaroMovement { get; set; }
        public string SeaDir { get; set; }
        public string SwellDir { get; set; }
        public string WindDirection { get; set; }
        public string CurrentDirection { get; set; }
        public string TimeAboveCP { get; set; }
        public string DistAboveCP { get; set; }
        public string SeaHeight { get; set; }
        public string SwellHeight { get; set; }
        public string DistilledWaterMade { get; set; }
        public string DistilledWaterCon { get; set; }
        public string DistilledWaterROB { get; set; }
        public string DistilledWaterProd { get; set; }
        public string FreshWaterMade { get; set; }
        public string FreshWaterCon { get; set; }
        public string FreshWaterROB { get; set; }
        public string SlopsMade { get; set; }
        public string SlopsROB { get; set; }
        public string ChartererCleaningChemicalsROB { get; set; }
        public string __PEId { get; set; }
        public string __PSId { get; set; }
        public string _24Hrs_Other_ConsTypeReasonDuration { get; set; }
        public string AEng_sea_Load_KW { get; set; }
        public string AEngSeaLoad { get; set; }
        public string AllFast { get; set; }
        public string Air_cooler_tempin_degC { get; set; }
        public string Air_cooler_tempout_degC { get; set; }
        public string AirCoolerTempIn { get; set; }
        public string AirCoolerTempOut { get; set; }
        public string AnchorAW { get; set; }
        public string AnchorTime { get; set; }
        public string Average_Speed { get; set; }
        public string AvgSpeed { get; set; }
        public string Berthing_Prospect { get; set; }
        public string BerthName { get; set; }
        public string BerthProsp { get; set; }
        public string Cargo_Temp { get; set; }
        public string CargoTemp { get; set; }
        public string CharteringVoyageNumber { get; set; }
        public string Daily_FW_production { get; set; }
        public string DailyFWProduction { get; set; }
        public string Dist_by_Speed_Log_nm { get; set; }
        public string DistanceToGO { get; set; }
        public string DistBySpeedLog { get; set; }
        public string DraftAmid { get; set; }
        public string Drop_after_air_cooler { get; set; }
        public string DropAfterAirCooler { get; set; }
        public string EOSP { get; set; }
        public string EOSPID { get; set; }
        public string Exh_gas_tempin_degC { get; set; }
        public string Exh_gas_tempout_degC { get; set; }
        public string ExhGasTempIn { get; set; }
        public string ExhGasTempOut { get; set; }
        public string FAOP { get; set; }
        public string FAOPID { get; set; }
        public string FirstLine { get; set; }
        public string Fuel_Temp_At_Flow_Meter { get; set; }
        public string FuelTempAtFlowMeter { get; set; }

        public string GmtOffset { get; set; }
        public string HSIFO { get; set; }
        public string HSMGO { get; set; }
        public string IFORobA { get; set; }
        public string IFORobAllF { get; set; }
        public string Location { get; set; }
        public string LSIFO { get; set; }
        public string LSIFORobA { get; set; }
        public string LSIFORobAllF { get; set; }
        public string LSMGO { get; set; }
        public string LSMGORobA { get; set; }
        public string LSMGORobAllF { get; set; }
        public string LT_NOR_Tendered { get; set; }
        public string LT_Time_Anchored { get; set; }

        public string MECylOilCons { get; set; }
        public string MGORobA { get; set; }
        public string MGORobAllF { get; set; }
        public string Miles_in_Seca_ { get; set; }
        public string NorTime { get; set; }
        public string Ops_Voyage_Number { get; set; }
        public string OtherCons24hrs { get; set; }
        public string PilotAB { get; set; }
        public string PrjSpeed { get; set; }
        public string ReportTime { get; set; }
        public string ROB_Aux_lub_oil_ltr { get; set; }
        public string ROB_Cnk_lub_oil_ltr { get; set; }
        public string ROB_High_TBN_Cylinder_Oilltr { get; set; }
        public string ROB_IFO { get; set; }
        public string ROB_LOW_TBN_Cylinder_Oilltr { get; set; }
        public string ROB_LSIFO { get; set; }
        public string ROB_LSMGO { get; set; }
        public string ROB_MGO { get; set; }
        public string ROBAuxLub { get; set; }
        public string ROBCnkLub { get; set; }
        public string ROBCylLub { get; set; }
        public string SBE_At_Berth { get; set; }
        public string Scavenge_air_pressure_mmWg { get; set; }
        public string ScavengeAirPressure { get; set; }
        public string SCIFO { get; set; }
        public string SCLSIFO { get; set; }
        public string SCLSMGO { get; set; }
        public string SCMGO { get; set; }
        public string SecaDist { get; set; }
        public string ShaftPower { get; set; }
        public string SirePSC_Inspection { get; set; }
        public string Slops_Oil { get; set; }
        public string Slops_Water { get; set; }
        public string Stern_tube_Lub_lost_to_sea_ltr { get; set; }
        public string SternTubeLubeLostToSea { get; set; }
        public string Tank_Cleaning { get; set; }
        public string TChrgr_RPM { get; set; }
        public string TChrgrRPM { get; set; }
        public string Thrust { get; set; }
        public string Torque { get; set; }
        public string Total_Bilge_water_tank_content__of_max { get; set; }
        public string Total_Bilge_water_tank_ROB_CubM { get; set; }
        public string Total_Cargo_Onboard { get; set; }
        public string Total_Sludge_tank_content__of_max { get; set; }
        public string Total_Sludge_tank_ROB_CubM { get; set; }
        public string TotalBilgeWaterContent { get; set; }
        public string TotalBilgeWaterROB { get; set; }
        public string TotalSludgeContent { get; set; }
        public string TotalSludgeROB { get; set; }
        public string Tug1 { get; set; }
        public string Tug1End { get; set; }
        public string Tug1Start { get; set; }
        public string Tug2 { get; set; }
        public string Tug2End { get; set; }
        public string Tug2Start { get; set; }
        public string Tug3 { get; set; }
        public string Tug3End { get; set; }
        public string Tug3Start { get; set; }
        public string Water_in_air_coolerin_degC { get; set; }
        public string Water_in_air_coolerout_degC { get; set; }
        public string Water_Type { get; set; }
        public string WaterInAirCoolerIn { get; set; }
        public string WaterInAirCoolerOut { get; set; }
        public string WaterType { get; set; }
        public string X { get; set; }
        public string X1 { get; set; }
        public string X2 { get; set; }
        public string X3 { get; set; }
        public string X4 { get; set; }
        public string X5 { get; set; }
        public string X6 { get; set; }
        public string X7 { get; set; }
        public string X8 { get; set; }
        public string X9 { get; set; }
        public string X10 { get; set; }
        
        public DateTimeOffset? CreatedDateTime { get; set; }
        public long CreatedBy { get; set; }
        public long ModifiedBy { get; set; }
        public DateTimeOffset ModifiedDateTime { get; set; }

        [Column(TypeName = "nvarchar(MAX)")]
        public string OriginalEmailText { get; set; }
        [Column(TypeName = "varchar(250)")]
        public string EmailAttachmentFileName { get; set; }
        [Column(TypeName = "xml")]
        public string OriginalFormsXML { get; set; }
        public Scrubber Scrubber { get; set; }
        //public FormCargo Cargo { get; set; }
        public Upcoming Upcoming { get; set; }
        [NotMapped]
        public virtual ICollection<FuelsRows> FuelsRows { get; set; }
        public string No_1_HFO_Tank_Port { get; set; }
        public string No_1_HFO_Tank_Stbd_Capacity_in_100__cub_m { get; set; }
        public string No_2_HFO_Tank_Port { get; set; }
        public string No_2_HFO_Tank_Stbd_Capacity_in_100__cub_m { get; set; }
        public string No_3_HFO_Tank_Port_Capacity_in_100__cub_m { get; set; }
        public string No_3_HFO_Tank_Stbd_Capacity_in_100__cub_m { get; set; }
        public string Grade9 { get; set; }
        public string Grade8 { get; set; }
        public string Grade7 { get; set; }
        public string Grade6 { get; set; }
        public string Grade5 { get; set; }
        public string Grade4 { get; set; }
        public string Grade3 { get; set; }
        public string Grade18 { get; set; }
        public string Grade17 { get; set; }
        public string Grade16 { get; set; }
        public string Grade15 { get; set; }
        public string Grade14 { get; set; }
        public string Grade13 { get; set; }
        public string Grade12 { get; set; }
        public string Grade11 { get; set; }
        public string Grade10 { get; set; }
        public string ROB_MT9 { get; set; }
        public string ROB_MT4 { get; set; }
        public string ROB_MT3 { get; set; }
        public string ROB_MT5 { get; set; }
        public string ROB_MT6 { get; set; }
        public string ROB_MT8 { get; set; }
        public string ROB_MT7 { get; set; }
        public string ROB_MT10 { get; set; }
        public string ROB_MT12 { get; set; }
        public string ROB_MT11 { get; set; }
        public string ROB_MT13 { get; set; }
        public string ROB_MT14 { get; set; }
        public string ROB_MT15 { get; set; }
        public string ROB_MT16 { get; set; }
        public string ROB_MT17 { get; set; }
        public string ROB_MT18 { get; set; }
        public string MGO05_GO_MT { get; set; }
        public string HSFO { get; set; }
        public string VLSFO { get; set; }
        public string ULSFO { get; set; }
        public string LSMGO1 { get; set; }
        public string Operation { get; set; }
        public string Location2 { get; set; }
        public string Storage_Tank__1_Capacity_in_100__cub_m { get; set; }
        public string Storage_Tank__2_Capacity_in_100__cub_m { get; set; }
        public string Storage_Tank__3_Capacity_in_100__cub_m { get; set; }
        public string Settling_Tank_Capacity_in_100__cub_m { get; set; }
        public string Service_Tank_Capacity_in_100__cub_m { get; set; }
        public string HFO_Settling_Tank__1_Capacity_in_100__cub_m { get; set; }
        public string HFO_Service_Tank__1_Capacity_in_100__cub_m { get; set; }
        public string HFO_Settling_Tank__2_Capacity_in_100__cub_m { get; set; }
        public string HFO_Service_Tank__2_Capacity_in_100__cub_m { get; set; }
        public string BunkerVendor { get; set; }
        public DateTimeOffset? Barge_Alongside { get; set; }
        public DateTimeOffset? Bunker_Hose_Connected { get; set; }
        public DateTimeOffset? Commenced_Bunkering { get; set; }
        public DateTimeOffset? BunkeringCompleted { get; set; }
        public DateTimeOffset? Bunker_Hose_disconnected { get; set; }
        public DateTimeOffset? Barge_Cast_Off { get; set; }
        public string Barge_Name { get; set; }
        public string HFO_Tank_Centre { get; set; }
        public DateTimeOffset? LatestResubmissionDate { get; set; }
        public bool HasNoParent { get; set; }
        [NotMapped]
        public virtual FormUnits FormUnits { get; set; }
        public string Scrubber_in_Operation { get; set; }
        public string Current_Mode_of_Scrubber { get; set; }
        public string Avg_Cargo_Temp { get; set; }
        public string ROB_VLSFO { get; set; }
        public string Aux_Boiler_Hrs { get; set; }
    }
    [Serializable]
    public class Scrubber
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long SFPM_ScrubberId { get; set; }
        [ForeignKey("Forms")]
        public long FormId { get; set; }
        public virtual Forms Forms { get; set; }
        public List<ScrubberRow> ScrubberRow { get; set; }
    }

    public class ScrubberRow
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long SFPM_ScrubberRowId { get; set; }

        public DateTimeOffset ScrubberBreakdownStart { get; set; }
        public DateTimeOffset ScrubberBreakdownend { get; set; }
        public string ScrubbeRemarks { get; set; }
    }
    //[Serializable]
    //public class FormCargo
    //{
    //    [Key]
    //    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    //    public long SFPM_FormCargoId { get; set; }
    //    [ForeignKey("Forms")]
    //    public long FormId { get; set; }
    //    public virtual Forms Forms { get; set; }
    //    public Cargoes Cargoes { get; set; }
    //}
    //public class Cargoes
    //{
    //    [Key]
    //    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    //    public long SFPM_CargoesId { get; set; }
    //    public List<CargoesCargo> Cargo { get; set; }
    //}
    //public class CargoesCargo
    //{
    //    [Key]
    //    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    //    public long SFPM_CargoesCargoId { get; set; }

    //    public string CargoTypeID { get; set; }
    //    public string Function { get; set; }
    //    public string BLDate { get; set; }
    //    public string BLCode { get; set; }
    //    public string BLGross { get; set; }
    //    public string CargoName { get; set; }
    //    public string ShipGross { get; set; }
    //    public string LoadTemp { get; set; }
    //    public string APIGravity { get; set; }
    //    public string UnitCode { get; set; }
    //    public string AltUnitCode { get; set; }
    //    public string AltBLGross { get; set; }
    //    public string AltShipGross { get; set; }
    //    public string Charterer { get; set; }
    //    public string Consignee { get; set; }
    //    public string Receiver { get; set; }
    //    public string Shipper { get; set; }
    //    public string Destination { get; set; }
    //    public string LetterOfProtest { get; set; }
    //    public string Stowage { get; set; }
    //}
    
    public class BerthingUnberthingDetailsRow
    {
        [key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long BerthingUnberthingDetailsRowId { get; set; }
        public string Terminal_name { get; set; }
        public string Berth_Name { get; set; }
        public DateTimeOffset LT_First_Line_Ashore { get; set; }
        public DateTimeOffset LT_All_fast { get; set; }
        public DateTimeOffset LT_All_clear_berth { get; set; }
        public string No_of_Tugs { get; set; }
        public string No_of_tugs_Departure { get; set; }
    }
    [Serializable]
    public class Upcoming
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long SFPM_UpcomingId { get; set; }
        [ForeignKey("Forms")]
        public long FormId { get; set; }
        public virtual Forms Forms { get; set; }
        public UpcomingPort UpcomingPort { get; set; }

    }

    public class UpcomingPort
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long SFPM_UpcomingPortId { get; set; }
        public string PortName { get; set; }
        public string DistToGo { get; set; }
        public string ProjSpeed { get; set; }
        public string ETA { get; set; }
        public string Via { get; set; }
        public string UnCode { get; set; }

    }
    public class EventROBsRow
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long SFPM_EventROBsRowId { get; set; }
        public string EventType { get; set; }
        public string EventROBObsDistance { get; set; }
        public string EventROBRemarks { get; set; }
        public DateTimeOffset EventROBStartDateTime { get; set; }
        public DateTimeOffset EventROBEndDateTime { get; set; }
        public string EventROBStartLatitude { get; set; }
        public string EventROBStartLongitude { get; set; }
        public string EventROBEndLatitude { get; set; }
        public string EventROBEndLongitude { get; set; }
        [ForeignKey("Forms")]
        public long FormId { get; set; }
        public virtual Forms Forms { get; set; }
        public long? CreatedBy { get; set; }
        public DateTime? CreatedDateTime { get; set; }
        public long? ModifiedBy { get; set; }
        public DateTime? ModifiedDateTime { get; set; }
    }
    public class Robs
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long SFPM_RobsId { get; set; }
        public DateTimeOffset? AsOfDate { get; set; }
        [ForeignKey("Forms")]
        public long FormId { get; set; }
        public virtual Forms Forms { get; set; }
        [ForeignKey("EventRobsRow")]
        public long? EventRobsRowId { get; set; }
        public virtual EventROBsRow EventROBsRow { get; set; }
        public List<Rob> Rob { get; set; }
        public long? CreatedBy { get; set; }
        public DateTime? CreatedDateTime { get; set; }
        public long? ModifiedBy { get; set; }
        public DateTime? ModifiedDateTime { get; set; }
    }
    public class Rob
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long SFPM_RobId { get; set; }
        public string FuelType { get; set; }
        public string Start { get; set; }
        public string Remaining { get; set; }
        public string AuxEngineConsumption { get; set; }
        public string BoilerEngineConsumption { get; set; }
        public string Units { get; set; }
        public string Received { get; set; }
        public string Consumption { get; set; }
        public string ConsOverWeatherThreshold { get; set; }
        public List<Allocation> Allocation { get; set; }
        [ForeignKey("Robs")]    
        public long? RobsSFPM_RobsId { get; set; }
        public long? CreatedBy { get; set; }
        public DateTime? CreatedDateTime { get; set; }
        public long? ModifiedBy { get; set; }
        public DateTime? ModifiedDateTime { get; set; }

    }
    public class Allocation
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long SFPM_AllocationId { get; set; }
        public string Name { get; set; }
        public string text { get; set; }
        [ForeignKey("Rob")] 
        public long? RobSFPM_RobId { get; set; }
        public DateTime? CreatedDateTime { get; set; }
        public long? CreatedBy { get; set; }
        public long? ModifiedBy { get; set; }
        public DateTime? ModifiedDateTime { get; set; }
    }
    public class BerthingUnberthingDetails
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long SFPM_BerthingUnberthingDetailsId { get; set; }
        [ForeignKey("Forms")]
        public long FormId { get; set; }
        public virtual Forms Forms { get; set; }
        public long? CreatedBy { get; set; }
        public DateTime? CreatedDateTime { get; set; }
        public long? ModifiedBy { get; set; }
        public DateTime? ModifiedDateTime { get; set; }
    }
    public class BerthUnberthdetailsRow
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long SFPM_BerthUnberthdetailsRowId { get; set; }
        public string TerminalName { get; set; }
        public string BerthName { get; set; }
        public DateTime? LTFirstLineAshore { get; set; }
        public string LTFirstLineAshore_Timezone { get; set; }
        public DateTime? LTAllFast { get; set; }
        public string LTAllFast_Timezone { get; set; }
        public DateTime? LTAllClearBerth { get; set; }
        public string LTAllClearBerth_Timezone { get; set; }
        public int NoOfTugs { get; set; }
        public int NoOfTugsDeparture { get; set; }
        [ForeignKey("BerthingUnberthingDetails")]
        public long BerthingUnberthingDetailsId { get; set; }
        public virtual BerthingUnberthingDetails BerthingUnberthingDetails { get; set; }
        public long? CreatedBy { get; set; }
        public DateTime? CreatedDateTime { get; set; }
        public long? ModifiedBy { get; set; }
        public DateTime? ModifiedDateTime { get; set; }
    }  
    public class FuelsRows
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long SFPM_FuelsRowsId { get; set; }
        [ForeignKey("Forms")]
        public long FormId { get; set; }
        public virtual Forms Forms { get; set; }
        public string FuelType { get; set; }
        public decimal QtyLifted { get; set; }
        public string BDN_Number { get; set; }
        public decimal Fuel_Densitry { get; set; }
        public decimal Sulphur_Content { get; set; }
        public long? CreatedBy { get; set; }
        public DateTime? CreatedDateTime { get; set; }
        public long? ModifiedBy { get; set; }
        public DateTime? ModifiedDateTime { get; set; }
    }
    public class FormPortActivities
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long SFPM_FormPortActivitiesId { get; set; }
        public string Name { get; set; }
        public string  Time { get; set; }
        public string  CargoName { get; set; }
        public string  Charterer { get; set; }
        public string  Remark { get; set; }
        public string  Berth { get; set; }
    }

}









