using Data.Solution.Models;
using Data.Solution.Models.ViewModels;
using VoyagesAPIService.Infrastructure.Repositories;
using VoyagesAPIService.Infrastructure.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.IO;
using OfficeOpenXml;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Http;

namespace VoyagesAPIService.Infrastructure.Services
{
    public class VoyagesService : IVoyagesService
    {
        protected VoyagesRepository _voyagesrepository;
        public VoyagesService(DatabaseContext databaseContext, UserContext context)
        {
            _voyagesrepository = new VoyagesRepository(databaseContext, context);
        }

        #region Passages / Voyages

        public Voyages GetVoyage(long voyagepassageId,long loginUserId)
        {
            return _voyagesrepository.GetVoyage(voyagepassageId, loginUserId);
        }
        
        public IEnumerable<Vessel> GetAllVesselsByYear()
        {
            return _voyagesrepository.GetAllVesselsByYear();
        }
        public IEnumerable<Voyages> GetAllVoyagesByVessel(string imoNumber, long userId)
        {
            return _voyagesrepository.GetAllVoyagesByVessel(imoNumber, userId);
        }

        public int CreateVoyages(Voyages voyages)
        {
            return _voyagesrepository.CreateVoyages(voyages);
        }

        public int UpdateVoyages(Voyages voyages,long loginUserId)
        {
            return _voyagesrepository.UpdateVoyages(voyages, loginUserId);
        }

        public int DeleteVoyage(string voyageIds,long loginUserId)
        {
            return _voyagesrepository.DeleteVoyage(voyageIds, loginUserId);
        }

        public long GetVesselId(string imoNumber)
        {
            return _voyagesrepository.GetVesselId(imoNumber);
        }
        public PassagesApprovalAudits GetLatestPassagesApprovalAudit(long voyagesId)
        {
            return _voyagesrepository.GetLatestPassagesApprovalAudit(voyagesId);
        }

        public IEnumerable<ApprovalAuditsViewModel> GetPassagesApprovalAuditList(long voyagesId,long loginUserId)
        {
            return _voyagesrepository.GetPassagesApprovalAuditList(voyagesId, loginUserId);
        }

        public int InitialApprove(bool isInitialApproved, long approvedBy, string voyageIds)
        {
            return _voyagesrepository.InitialApprove(isInitialApproved, approvedBy, voyageIds);
        }

        public int FinalApprove(bool isFinalApproved, long finalApprovedBy, string voyageIds)
        {
            return _voyagesrepository.FinalApprove(isFinalApproved, finalApprovedBy, voyageIds);
        }

        public IEnumerable<PassageWarning> GetPassageWarning(long voyageId,long loginUserId)
        {
            return _voyagesrepository.GetPassageWarning(voyageId, loginUserId);
        }

        public IEnumerable<PassageWarningAudit> GetPassageWarningAudit(long PassageWarningId)
        {
            return _voyagesrepository.GetPassageWarningAudit(PassageWarningId);
        }

        public int CreatePassageWarningAudit(PassageWarningAudit passageWarningAudit)
        {
            return _voyagesrepository.CreatePassageWarningAudit(passageWarningAudit);
        }

        public string GetVesselOwnerNameByImoNumber(string ImoNumber)
        {
            return _voyagesrepository.GetVesselOwnerNameByImoNumber(ImoNumber);
        }
        #endregion

        #region Position/Noon Reports

        public IEnumerable<VoyagesIntitialDataViewModel> GetReportsForVoyage(string imoNumber, long voyageNumber, DateTime actualStartOfSeaPassage, string departureTimeZoneOffset, long userId)
        {
            return _voyagesrepository.GetReportsForVoyage(imoNumber, voyageNumber, actualStartOfSeaPassage, departureTimeZoneOffset, userId);
        }

        public int CreatePosition(Forms forms,long loginUserId)
        {
            return _voyagesrepository.CreatePosition(forms, loginUserId);
        }

        public int DeletePosition(string formIds)
        {
            return _voyagesrepository.DeletePosition(formIds);
        }
        public int UpdatePositionRemark(long formId, string remark,long loginUserId)
        {
            return _voyagesrepository.UpdatePositionRemark(formId, remark, loginUserId);
        }

        public IEnumerable<PositionWarning> GetPositionWarning(long formId,long loginUserId)
        {
            return _voyagesrepository.GetPositionWarning(formId, loginUserId);
        }

        public IEnumerable<PositionWarningAudit> GetPositionWarningAudit(long positionWarningId)
        {
            return _voyagesrepository.GetPositionWarningAudit(positionWarningId);
        }

        public int CreatePositionWarningAudit(PositionWarningAudit positionWarningAudit,long loginUserId)
        {
            return _voyagesrepository.CreatePositionWarningAudit(positionWarningAudit, loginUserId);
        }

        public IEnumerable<FromsViewModel> GetViewOriginalEmail(long formId,long loginUserId)
        {
            return _voyagesrepository.GetViewOriginalEmail(formId, loginUserId);
        }

        public Forms GetPositionById(long formId,long loginUserId)
        {
            return _voyagesrepository.GetPositionById(formId, loginUserId);
        }
        public int UpdatePosition(Forms forms)
        {
            return _voyagesrepository.UpdatePosition(forms);
        }
        public Voyages GetActualStartPassage(Forms forms)
        {
            return _voyagesrepository.GetActualStartPassage(forms);
        }
        #endregion

        #region Event
        public List<EventROBsRow> GetAllEvents(long formId,long loginUserId)
        {
            return _voyagesrepository.GetAllEvents(formId, loginUserId);
        }
        public EventROBsRow GetEventById(long eventId,long loginUserId)
        {
            return _voyagesrepository.GetEventById(eventId, loginUserId);
        }
        public int CreateEvent(EventROBsRow eventROBsRow)
        {
            return _voyagesrepository.CreateEvent(eventROBsRow);
        }
        public int UpdateEvent(EventROBsRow eventROBsRow,long loginUserId)
        {
            return _voyagesrepository.UpdateEvent(eventROBsRow, loginUserId);
        }
        public int DeleteEvent(long eventId,long loginUserId)
        {
            return _voyagesrepository.DeleteEvent(eventId, loginUserId);
        }
        #endregion

        #region Fluid Consumption (Postion / Event)
        public IEnumerable<Rob> GetAllFluidConsumption(long formId,long loginUserId)
        {
            return _voyagesrepository.GetAllFluidConsumption(formId, loginUserId);
        }
        public Rob GetFluidConsumptionById(long robId)
        {
            return _voyagesrepository.GetFluidConsumptionById(robId);
        }
        public IEnumerable<Rob> GetAllEventFluidConsumption(long formId, long eventId,long loginUserId)
        {
            return _voyagesrepository.GetAllEventFluidConsumption(formId, eventId, loginUserId);
        }

        public long CreateFluidConsumption(FluidFuelConsumedViewModel fluidFuelConsumed, out long allocationId)
        {
            return _voyagesrepository.CreateFluidConsumption(fluidFuelConsumed, out allocationId);
        }

        public int UpdateFluidConsumption(FluidFuelConsumedViewModel fluidFuelConsumed)
        {
            return _voyagesrepository.UpdateFluidConsumption(fluidFuelConsumed);
        }
        public int DeleteFluidConsumption(long robId, long allocationId)
        {
            return _voyagesrepository.DeleteFluidConsumption(robId, allocationId);
        }

        public long CreateEventFluidConsumption(FluidFuelConsumedViewModel fluidFuelConsumed, out long allocationId)
        {
            return _voyagesrepository.CreateEventFluidConsumption(fluidFuelConsumed, out allocationId);
        }

        #endregion

        #region Bunker

        public IEnumerable<FluidBunkerViewModel> GetAllFluidBunker(long formId,long loginUserId)
        {
            return _voyagesrepository.GetAllFluidBunker(formId, loginUserId);
        }
        public FluidBunkerViewModel GetFluidBunkerById(long robId, string bunkerType)
        {
            return _voyagesrepository.GetFluidBunkerById(robId, bunkerType);
        }
        public IEnumerable<FluidBunkerViewModel> GetAllEventFluidBunker(long formId, long eventRobsRowId,long loginUserId)
        {
            return _voyagesrepository.GetAllEventFluidBunker(formId, eventRobsRowId, loginUserId);
        }
        public long CreateFluidBunker(FluidBunkerViewModel fluidBunker)
        {
            return _voyagesrepository.CreateFluidBunker(fluidBunker);
        }
        public int UpdateFluidBunker(FluidBunkerViewModel fluidBunker)
        {
            return _voyagesrepository.UpdateFluidBunker(fluidBunker);
        }
        public int DeleteFluidBunker(int formId, int robId, string bunkerType)
        {
            return _voyagesrepository.DeleteFluidBunker(formId, robId, bunkerType);
        }
        public long CreateEventFluidBunker(FluidBunkerViewModel fluidBunker)
        {
            return _voyagesrepository.CreateEventFluidBunker(fluidBunker);
        }
        public IEnumerable<BunkerType> GetAllBunkerType()
        {
            return _voyagesrepository.GetAllBunkerType();
        }

        #endregion

        #region Exclusion
        public int CreatePassageReportExclusion(ExcludeReportLog exclude)
        {
            return _voyagesrepository.CreatePassageReportExclusion(exclude);
        }
        public int CreatePositionReportExclusion(ExcludeReportLog exclude)
        {
            return _voyagesrepository.CreatePositionReportExclusion(exclude);
        }
        public IEnumerable<ExcludeReportLogs> GetPassageReportExclusionlog(long voyageId,long loginUserId)
        {
            return _voyagesrepository.GetPassageReportExclusionlog(voyageId, loginUserId);
        }
        public IEnumerable<ExcludeReportLogs> GetPositionReportExclusionlog(long formId)
        {
            return _voyagesrepository.GetPositionReportExclusionlog(formId);
        }
        public IEnumerable<ExcludeReportLog> GetPassageReportExclusion(long voyageId,long loginUserId)
        {
            return _voyagesrepository.GetPassageReportExclusion(voyageId, loginUserId);
        }
        public IEnumerable<ExcludeReportLog> GetPositionReportExclusion(long formId)
        {
            return _voyagesrepository.GetPositionReportExclusion(formId);
        }
        public List<ExcludeReportLog> GetPssgReptExclResponse(long voyagesId)
        {
            return _voyagesrepository.GetPssgReptExclResponse(voyagesId);
        }
        public List<ExcludeReportLog> GetPostReptExclResponse(long formId)
        {
            return _voyagesrepository.GetPostReptExclResponse(formId);
        }
        public IEnumerable<Voyages> ExportGetVoyages(string voyageId)
        {
            return _voyagesrepository.ExportGetVoyages(voyageId);
        }
        #endregion

        #region Others

        public bool CheckFormExists(long formId)
        {
            return _voyagesrepository.CheckFormExists(formId);
        }
        public bool CheckEventExists(long formId, long eventId)
        {
            return _voyagesrepository.CheckEventExists(formId, eventId);
        }
        public int SavePageSettings(long userId, bool isPassageUTC, bool isPositionUTC)
        {
            return _voyagesrepository.SavePageSettings(userId, isPassageUTC, isPositionUTC);
        }
        public PageSettings GetPageSettings(long userId)
        {
            return _voyagesrepository.GetPageSettings(userId);
        }
        public bool IsVoyageApproved(long formId)
        {
            return _voyagesrepository.IsVoyageApproved(formId);
        }
        public PassageDataChartViewModel GetPassageDataForChart(PassageDataChartViewModel passageDataCharts,long loginUserId)
        {
            return _voyagesrepository.GetPassageDataForChart(passageDataCharts, loginUserId);
        }
        public List<ECADataViewModel> GetECAData(string type)
        {
            return _voyagesrepository.GetECAData(type);
        }
        #endregion

        #region ExcelOperation
        public int ImportExcelForDepartureReport(List<IFormFile> files)
        {
            return this._voyagesrepository.ImportExcelForDepartureReport(files);
        }
       
        public int ImportVesselListExcel()
        {
            return this._voyagesrepository.ImportVesselListExcel();
        }
        #endregion

        #region Others
        public RestSharp.IRestResponse GetMarineWeatherImage(string SERVICE, string VERSION, string REQUEST, string FORMAT, string TRANSPARENT,
             string map, string CURRENT_DATE, string CURRENT_HOUR, string TILES, string LAYERS, string TIME,
             string FORECAST_DATE, string FORECAST_HOUR, string WIDTH, string HEIGHT, string CRS, string STYLES,
             string BBOX)
        {
            return _voyagesrepository.GetMarineWeatherImage(SERVICE, VERSION, REQUEST, FORMAT, TRANSPARENT,
             map, CURRENT_DATE, CURRENT_HOUR, TILES, LAYERS, TIME,
             FORECAST_DATE, FORECAST_HOUR, WIDTH, HEIGHT, CRS, STYLES,
             BBOX);
        }
        public int DeleteMeteoStratumData(string imoNumber, DateTime gpsDatetime)
        {
            return _voyagesrepository.DeleteMeteoStratumData(imoNumber, gpsDatetime);
        }
        public IEnumerable<DirectionValueMapping> GetDirectionList()
        {
            return _voyagesrepository.GetDirectionList();
        }

        public void GetFromsI()
        {

        }
        public string AnalyzedWeatherCal(long formId)
        {
            return _voyagesrepository.AnalyzedWeatherCal(formId);
        }

       
        #endregion

    }
}
