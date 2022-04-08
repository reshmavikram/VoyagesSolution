using Data.Solution.Models;
using Data.Solution.Models.ViewModels;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;

namespace VoyagesAPIService.Infrastructure.Services.Interfaces
{
    public interface IVoyagesService
    {
        //Voyage
        IEnumerable<Vessel> GetAllVesselsByYear();
        int CreateVoyages(Voyages voyages);
        int UpdateVoyages(Voyages voyages,long loginUserId);
        Voyages GetVoyage(long voyagesId,long loginUserId);
        int DeleteVoyage(string voyageIds,long loginUserId);
        long GetVesselId(string imoNumber);
        IEnumerable<Voyages> GetAllVoyagesByVessel(string imoNumber, long userId);
        IEnumerable<VoyagesIntitialDataViewModel> GetReportsForVoyage(string imoNumber, long voyageNumber, DateTime actualStartOfSeaPassage, string departureTimeZoneOffset, long userId);
        IEnumerable<FromsViewModel> GetViewOriginalEmail(long formId,long loginUserId);

        //PassagesApprovalAudit
        int InitialApprove(bool isInitialApproved, long approvedBy, string voyageIds);
        int FinalApprove(bool isFinalApproved, long finalApprovedBy, string voyageIds);
        PassagesApprovalAudits GetLatestPassagesApprovalAudit(long voyagesId);
        IEnumerable<ApprovalAuditsViewModel> GetPassagesApprovalAuditList(long voyagesId,long loginUserId);

        //CreatePosition
        int CreatePosition(Forms forms,long loginUserId);
        int CreatePositionReportExclusion(ExcludeReportLog exclude);
        int CreatePassageReportExclusion(ExcludeReportLog exclude);

        int UpdatePositionRemark(long formId, string remark,long loginUserId);

        long CreateFluidConsumption(FluidFuelConsumedViewModel fluidFuelConsumed, out long allocationId);
        long CreateEventFluidConsumption(FluidFuelConsumedViewModel fluidFuelConsumed, out long allocationId);
        int UpdateFluidConsumption(FluidFuelConsumedViewModel fluidFuelConsumed);
        int DeleteFluidConsumption(long robId, long allocationId);



        long CreateFluidBunker(FluidBunkerViewModel fluidBunker);
        int UpdateFluidBunker(FluidBunkerViewModel fluidBunker);
        int DeleteFluidBunker(int formId, int robId, string bunkerType);

        long CreateEventFluidBunker(FluidBunkerViewModel fluidBunker);


        List<EventROBsRow> GetAllEvents(long formId,long loginUserId);
        EventROBsRow GetEventById(long eventId,long loginUserId);
        int CreateEvent(EventROBsRow eventROBsRow);
        int UpdateEvent(EventROBsRow eventROBsRow,long loginUserId);
        int DeleteEvent(long eventId,long loginUserId);
        IEnumerable<BunkerType> GetAllBunkerType();
        IEnumerable<PassageWarning> GetPassageWarning(long voyageId,long loginUserId);
        IEnumerable<PassageWarningAudit> GetPassageWarningAudit(long PassageWarningId);
        int CreatePassageWarningAudit(PassageWarningAudit passageWarningAudit);
        IEnumerable<PositionWarning> GetPositionWarning(long FormId,long loginUserId);
        IEnumerable<PositionWarningAudit> GetPositionWarningAudit(long PositionWarningId);
        int CreatePositionWarningAudit(PositionWarningAudit positionWarningAudit,long loginUserId);

        int DeletePosition(string formIds);

        IEnumerable<FluidBunkerViewModel> GetAllFluidBunker(long formId,long loginUserId);
        FluidBunkerViewModel GetFluidBunkerById(long robId, string bunkerType);
        IEnumerable<FluidBunkerViewModel> GetAllEventFluidBunker(long formId, long eventRobsRowId,long loginUserId);


        IEnumerable<Rob> GetAllFluidConsumption(long formId,long loginUserId);
        IEnumerable<Rob> GetAllEventFluidConsumption(long formId, long eventId,long loginUserId);
        Rob GetFluidConsumptionById(long robId);

        IEnumerable<ExcludeReportLog> GetPassageReportExclusion(long voyageId,long loginUserId);
        IEnumerable<ExcludeReportLog> GetPositionReportExclusion(long formId);
        IEnumerable<ExcludeReportLogs> GetPassageReportExclusionlog(long voyageId,long loginUserId);
        IEnumerable<ExcludeReportLogs> GetPositionReportExclusionlog(long formId);

        bool CheckFormExists(long formId);
        bool CheckEventExists(long formId, long eventId);

        int UpdatePosition(Forms forms);
        Voyages GetActualStartPassage(Forms forms);
        Forms GetPositionById(long Id,long loginUserId);
        List<ExcludeReportLog> GetPssgReptExclResponse(long voyagesId);
        List<ExcludeReportLog> GetPostReptExclResponse(long formId);
        int SavePageSettings(long userId, bool isPassageUTC, bool isPositionUTC);
        PageSettings GetPageSettings(long userId);
        string GetVesselOwnerNameByImoNumber(string ImoNumber);
        IEnumerable<Voyages> ExportGetVoyages(string voyageId);
        bool IsVoyageApproved(long formId);

        int ImportExcelForDepartureReport(List<IFormFile> files);
      
        int ImportVesselListExcel();
        PassageDataChartViewModel GetPassageDataForChart(PassageDataChartViewModel passageDataCharts,long loginUserId);
        List<ECADataViewModel> GetECAData(string type);

        RestSharp.IRestResponse GetMarineWeatherImage(string SERVICE, string VERSION, string REQUEST, string FORMAT, string TRANSPARENT,
             string map, string CURRENT_DATE, string CURRENT_HOUR, string TILES, string LAYERS, string TIME,
             string FORECAST_DATE, string FORECAST_HOUR, string WIDTH, string HEIGHT, string CRS, string STYLES,
             string BBOX);
        int DeleteMeteoStratumData(string imoNumber, DateTime gpsDatetime);
        IEnumerable<DirectionValueMapping> GetDirectionList();
        string AnalyzedWeatherCal(long formId);
    }
}