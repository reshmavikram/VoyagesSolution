using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Data.Solution.Models;
using Data.Solution.Models.ViewModels;
using Data.Solution.Resources;
using VoyagesAPIService.Infrastructure.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Hosting;
using System.IO;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System.Drawing;
using System.Threading.Tasks;
using VoyagesAPIService.Helper;
using Microsoft.AspNetCore.Authorization;
using VoyagesAPIService.Filter;
using System.Web;
using Microsoft.Extensions.Configuration;
using VoyagesAPIService.Infrastructure.Helper;
using Microsoft.AspNetCore.Http;

namespace VoyagesAPIService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [ServiceFilter(typeof(AuthServer))]

    public class VoyagesController : Controller
    {
        private readonly IVoyagesService _voyagesservice;
        private readonly IHostingEnvironment _hostingEnvironment;
        public IConfiguration Configuration { get; }
        public static int departureCount = 0;
        public static int arrivalCount = 0;
        public static int noonReportAtSeaCount = 0;
        public static int noonReportAtPortCount = 0;
        public static int databaseDepartureCount = 0;
        public static int databaseArrivalCount = 0;
        public static int DatabaseNoonReportAtSeaCount = 0;
        public static int DatabaseNoonReportAtPortCount = 0;
        public VoyagesController(IVoyagesService voyagesservice, IHostingEnvironment hostingEnvironment, IConfiguration configuration)
        {
            _voyagesservice = voyagesservice;
            _hostingEnvironment = hostingEnvironment;
            Configuration = configuration;
        }

        #region Passages

        [HttpGet, Route("getAllVoyagesByVessel")]
        public IActionResult GetAllVoyagesByVessel(string imoNumber, long userId)
        {
            //IEnumerable<Voyages> allVoyageList = RedisCacheHelper.Get<List<Voyages>>("voyages:" + imoNumber);
            //if (allVoyageList == null)
            //{
            //    allVoyageList = _voyagesservice.GetAllVoyagesByVessel(imoNumber, userId);
            //    RedisCacheHelper.Set("voyages:" + imoNumber, allVoyageList);
            //}
            IEnumerable<Voyages> allVoyageList = _voyagesservice.GetAllVoyagesByVessel(imoNumber, userId);
            List<VoyagesViewModel> voyagesViewModelList = new List<VoyagesViewModel>();
            if (allVoyageList != null)
            {
                foreach (Voyages voyageObj in allVoyageList)
                {
                    //View model mapping
                    VoyagesViewModel objViewModel = new VoyagesViewModel();
                    var isLocalDatetime = _voyagesservice.GetPageSettings(userId);
                    if (isLocalDatetime != null && isLocalDatetime.IsPassageUTC)
                    {
                        if (voyageObj.DepartureTimezone != null && !string.IsNullOrEmpty(voyageObj.DepartureTimezone))
                        {
                            var hrs = Convert.ToDouble(voyageObj.DepartureTimezone.Substring(1, 2));
                            var min = Convert.ToDouble(voyageObj.DepartureTimezone.Substring(4, 2));
                            objViewModel.ActualStartOfSeaPassage = voyageObj.DepartureTimezone.Contains("-") ? voyageObj.ActualStartOfSeaPassage.AddHours(hrs).AddMinutes(min)
                            : voyageObj.ActualStartOfSeaPassage.AddHours(-hrs).AddMinutes(-min);
                        }
                        if (voyageObj.ActualEndOfSeaPassage != null && !string.IsNullOrEmpty(voyageObj.ArrivalTimezone))
                        {
                            var hrs = Convert.ToDouble(voyageObj.ArrivalTimezone.Substring(1, 2));
                            var min = Convert.ToDouble(voyageObj.ArrivalTimezone.Substring(4, 2));
                            objViewModel.ActualEndOfSeaPassage = voyageObj.ArrivalTimezone.Contains("-") ? Convert.ToDateTime(voyageObj.ActualEndOfSeaPassage.ToString()).AddHours(hrs).AddMinutes(min)
                            : Convert.ToDateTime(voyageObj.ActualEndOfSeaPassage.ToString()).AddHours(-hrs).AddMinutes(-min);
                        }
                    }
                    else
                    {
                        objViewModel.ActualEndOfSeaPassage = voyageObj.ActualEndOfSeaPassage;
                        objViewModel.ActualStartOfSeaPassage = voyageObj.ActualStartOfSeaPassage;

                    }
                    objViewModel.ArrivalPort = voyageObj.ArrivalPort;
                    objViewModel.ArrivalTimezone = voyageObj.ArrivalTimezone;
                    objViewModel.CreatedBy = voyageObj.CreatedBy;
                    objViewModel.CreatedDateTime = voyageObj.CreatedDateTime;
                    objViewModel.DeparturePort = voyageObj.DeparturePort;
                    objViewModel.DepartureTimezone = voyageObj.DepartureTimezone;
                    objViewModel.Description = voyageObj.Description;
                    objViewModel.LoadCondition = voyageObj.LoadCondition;
                    objViewModel.ModifiedBy = voyageObj.ModifiedBy;
                    objViewModel.ModifiedDateTime = voyageObj.ModifiedDateTime;
                    objViewModel.SFPM_VoyagesId = voyageObj.SFPM_VoyagesId;
                    objViewModel.IMONumber = voyageObj.IMONumber;
                    objViewModel.VesselCode = voyageObj.VesselCode;
                    objViewModel.VoyageNumber = voyageObj.VoyageNumber;
                    foreach (var warnig in _voyagesservice.GetPassageWarning(voyageObj.SFPM_VoyagesId,userId))
                    {
                        if (!_voyagesservice.GetPassageWarningAudit(warnig.SFPM_PassageWarningId).OrderByDescending(x => x.ReviewDateTime).Select(x => x.IsApproved).FirstOrDefault())
                        {
                            objViewModel.isPassageWarningExists = true;
                            break;
                        }
                    }
                    PassagesApprovalAudits auditData = _voyagesservice.GetLatestPassagesApprovalAudit(voyageObj.SFPM_VoyagesId);
                    if (auditData != null)
                    {
                        objViewModel.Approval = auditData.ApprovalStatus;
                        objViewModel.Action = auditData.ApprovalAction;
                    }
                    var reportExclusion = _voyagesservice.GetPassageReportExclusionlog(voyageObj.SFPM_VoyagesId,userId);
                    if (reportExclusion != null)
                    {
                        objViewModel.IsCharterPartyExclude = reportExclusion.Where(x => x.ReportName == "Charter Party").Select(x => x.Excluded).FirstOrDefault();
                        objViewModel.IsConflict = reportExclusion.Where(x => x.ReportName == "Pool").Select(x => x.Excluded).FirstOrDefault();
                    }
                    voyagesViewModelList.Add(objViewModel);
                }
                Response.Headers.Add("Status", HttpStatusCode.OK.ToString());
                return Ok(voyagesViewModelList);
            }
            else
            {
                Response.Headers.Add("Status", HttpStatusCode.Conflict.ToString());
                Response.Headers.Add("Message", MessagesResource.RecordNotFound);
                return Conflict();
            }

        }

        [HttpGet, Route("getvoyage")]
        public IActionResult GetVoyageById(long voyagesId)
        {
            long loginUserId = Convert.ToInt64(RouteData.Values["UserId"]);
            Voyages voyageObj = _voyagesservice.GetVoyage(voyagesId,loginUserId);
            if (voyageObj != null)
            {
                //View model mapping
                VoyagesViewModel objViewModel = new VoyagesViewModel();
                objViewModel.ActualEndOfSeaPassage = voyageObj.ActualEndOfSeaPassage;
                objViewModel.ActualStartOfSeaPassage = voyageObj.ActualStartOfSeaPassage;
                objViewModel.ArrivalPort = voyageObj.ArrivalPort;
                objViewModel.ArrivalTimezone = voyageObj.ArrivalTimezone;
                objViewModel.CreatedBy = voyageObj.CreatedBy;
                objViewModel.CreatedDateTime = voyageObj.CreatedDateTime;
                objViewModel.DeparturePort = voyageObj.DeparturePort;
                objViewModel.DepartureTimezone = voyageObj.DepartureTimezone;
                objViewModel.Description = voyageObj.Description;
                objViewModel.LoadCondition = voyageObj.LoadCondition;
                objViewModel.ModifiedBy = voyageObj.ModifiedBy;
                objViewModel.ModifiedDateTime = voyageObj.ModifiedDateTime;
                objViewModel.SFPM_VoyagesId = voyageObj.SFPM_VoyagesId;

                objViewModel.VesselCode = voyageObj.VesselCode;
                objViewModel.VoyageNumber = voyageObj.VoyageNumber;
                objViewModel.IMONumber = voyageObj.IMONumber;

                //long loginUserId = Convert.ToInt64(RouteData.Values["UserId"]);
                IEnumerable<ApprovalAuditsViewModel> approvalAuditsList = _voyagesservice.GetPassagesApprovalAuditList(voyagesId, loginUserId);
                if (approvalAuditsList.Count() > 0)
                {
                    ApprovalAuditsViewModel approvalAuditsViewModel = approvalAuditsList.OrderByDescending(x => Convert.ToDateTime(x.DateTime)).First();
                    objViewModel.Approval = approvalAuditsViewModel.Approval;
                    objViewModel.Action = approvalAuditsViewModel.Action;
                }

                Response.Headers.Add("Status", HttpStatusCode.OK.ToString());
                return Ok(objViewModel);
            }
            else
            {
                Response.Headers.Add("Status", HttpStatusCode.Conflict.ToString());
                Response.Headers.Add("Message", MessagesResource.RecordNotFound);
                return Conflict();
            }
        }

        [HttpPost, Route("createvoyage")]
        public IActionResult CreateVoyage([FromBody]Voyages voyagesData)
        {

            if (!string.IsNullOrEmpty(voyagesData.DeparturePort) && !string.IsNullOrEmpty(voyagesData.IMONumber))
            {
                int id = _voyagesservice.CreateVoyages(voyagesData);
                if (id != 0)
                {
                    //bool keyExists = RedisCacheHelper.IsKeyExists("voyages:" + voyagesData.IMONumber);
                    //if (keyExists)
                    //    RedisCacheHelper.DeleteKey("voyages:" + voyagesData.IMONumber);

                    var vesselId = _voyagesservice.GetVesselId(voyagesData.IMONumber);
                    //bool VesselkeyExists = RedisCacheHelper.IsKeyExists("voyages:" + vesselId);
                    //if (keyExists)
                    //    RedisCacheHelper.DeleteKey("passageData:" + vesselId);

                    Response.Headers.Add("Status", HttpStatusCode.Created.ToString());
                    Response.Headers.Add("Message", MessagesResource.InsertedSuccessfully);
                    return Ok(voyagesData);
                }
                else
                {
                    Response.Headers.Add("Status", HttpStatusCode.Conflict.ToString());
                    Response.Headers.Add("Message", MessagesResource.DuplicateData);
                    return Conflict();

                }
            }
            else
            {
                Response.Headers.Add("Status", HttpStatusCode.Conflict.ToString());
                Response.Headers.Add("Message", MessagesResource.DatabaseInsertFailed);
                return BadRequest();
            }


        }
        [HttpPost, Route("copyposition")]
        public IActionResult CopyPosition([FromBody]Forms formData)
        {
            formData.OriginalEmailText = "";
            formData.EmailAttachmentFileName = "";
            long loginUserId = Convert.ToInt64(RouteData.Values["UserId"]);
            if (!string.IsNullOrEmpty(formData.FormIdentifier) && !string.IsNullOrWhiteSpace(formData.FormIdentifier) && formData.ReportDateTime != null
                 && formData.Longitude != null && formData.Latitude != null && formData.SteamingHrs != null && formData.FWDDraft != null && formData.DistanceToGO != null
                 && formData.EngineDistance != null && formData.Slip != null && formData.AvgBHP != null && formData.WindDirection != null && formData.SeaDir != null)
            {

                int id = _voyagesservice.CreatePosition(formData,loginUserId);
                if (id != 0)
                {
                    Voyages objVoyage = _voyagesservice.GetActualStartPassage(formData);
                    var newDate = objVoyage.ActualStartOfSeaPassage.Year + "/" + objVoyage.ActualStartOfSeaPassage.Month + "/" + objVoyage.ActualStartOfSeaPassage.Day + " " + objVoyage.ActualStartOfSeaPassage.Hour + ":" + objVoyage.ActualStartOfSeaPassage.Minute + ":" + objVoyage.ActualStartOfSeaPassage.Second;
                    //bool keyExists = RedisCacheHelper.IsKeyExists("noonreports:" + formData.ImoNumber + "|" + formData.VoyageNo + "|" + newDate);
                    //if (keyExists)
                    //    RedisCacheHelper.DeleteKey("noonreports:" + formData.ImoNumber + "|" + formData.VoyageNo + "|" + newDate);

                    Response.Headers.Add("Status", HttpStatusCode.Created.ToString());
                    Response.Headers.Add("Message", MessagesResource.InsertedSuccessfully);
                    return Ok(formData);
                }
                else
                {
                    Response.Headers.Add("Status", HttpStatusCode.Conflict.ToString());
                    Response.Headers.Add("Message", MessagesResource.DuplicateData);
                    return Conflict();
                }
            }
            else
            {
                Response.Headers.Add("Status", HttpStatusCode.Conflict.ToString());
                Response.Headers.Add("Message", MessagesResource.DatabaseInsertFailed);
                return BadRequest();
            }
        }

        [HttpPut, Route("updatevoyage")]
        public IActionResult UpdateVoyage([FromBody]Voyages voyages)
        {
            PassagesApprovalAudits auditData = _voyagesservice.GetLatestPassagesApprovalAudit(voyages.SFPM_VoyagesId);
            long loginUserId = Convert.ToInt64(RouteData.Values["UserId"]);
            if (auditData == null || (!auditData.IsInitialApproved && !auditData.IsFinalApproved) && !string.IsNullOrEmpty(voyages.IMONumber))
            {
                if (!string.IsNullOrEmpty(voyages.DeparturePort) && voyages.SFPM_VoyagesId != 0)
                {
                    switch (_voyagesservice.UpdateVoyages(voyages, loginUserId))
                    {
                        case (int)ResponseStatus.NOTFOUND:
                            Response.Headers.Add("Status", HttpStatusCode.NotFound.ToString());
                            Response.Headers.Add("Message", MessagesResource.RecordNotFound);
                            return NotFound();
                        default:
                            //bool keyExists = RedisCacheHelper.IsKeyExists("voyages:" + voyages.IMONumber);
                            //if (keyExists)
                            //    RedisCacheHelper.DeleteKey("voyages:" + voyages.IMONumber);

                            var vesselId = _voyagesservice.GetVesselId(voyages.IMONumber);
                            //bool VesselkeyExists = RedisCacheHelper.IsKeyExists("voyages:" + vesselId);
                            //if (keyExists)
                            //    RedisCacheHelper.DeleteKey("passageData:" + vesselId);

                            Response.Headers.Add("Status", HttpStatusCode.Created.ToString());
                            Response.Headers.Add("Message", MessagesResource.UpdatedSuccessfully);
                            return Ok(voyages);
                    }

                }
                else
                {
                    Response.Headers.Add("Status", HttpStatusCode.Conflict.ToString());
                    Response.Headers.Add("Message", MessagesResource.DatabaseInsertFailed);
                    return BadRequest();
                }
            }
            else
            {
                Response.Headers.Add("Status", HttpStatusCode.Conflict.ToString());
                Response.Headers.Add("Message", MessagesResource.PassageAlreadyApproved);
                return Conflict();
            }
        }

        [HttpDelete, Route("deletevoyage")]
        public IActionResult DeleteVoyage(string voyageIds)
        {
            long loginUserId = Convert.ToInt64(RouteData.Values["UserId"]);
            if (!string.IsNullOrEmpty(voyageIds))
            {
                string lstVoyageIds = voyageIds.Split(',').FirstOrDefault();
                Voyages voyage = _voyagesservice.GetVoyage(Convert.ToInt64(lstVoyageIds), loginUserId);

                switch (_voyagesservice.DeleteVoyage(voyageIds,loginUserId))
                {
                    case (int)ResponseStatus.ALREADYAPPROVED:
                        Response.Headers.Add("Status", HttpStatusCode.BadGateway.ToString());
                        //Response.Headers.Add("Message", MessagesResource.MultiPassagesApproved);
                        return BadRequest();
                    case (int)ResponseStatus.NOTFOUND:
                        Response.Headers.Add("Status", HttpStatusCode.NotFound.ToString());
                        Response.Headers.Add("Message", MessagesResource.RecordNotFound);
                        return NotFound();
                    default:
                        //bool keyExists = RedisCacheHelper.IsKeyExists("voyages:" + voyage.IMONumber);
                        //if (keyExists)
                        //    RedisCacheHelper.DeleteKey("voyages:" + voyage.IMONumber);

                        var vesselId = _voyagesservice.GetVesselId(voyage.IMONumber);
                        //bool VesselkeyExists = RedisCacheHelper.IsKeyExists("voyages:" + vesselId);
                        //if (keyExists)
                        //    RedisCacheHelper.DeleteKey("passageData:" + vesselId);
                        //position cache delete 
                        List<string> VoyageIds = voyageIds.Split(',').ToList();
                        foreach (var voyageId in VoyageIds)
                        {
                            var voyages = _voyagesservice.GetAllVoyagesByVessel(voyage.IMONumber, 0).OrderByDescending(x=>x.SFPM_VoyagesId).ToList();
                            if (voyages.Count > 0)
                            {
                                var beloveDeletePassage = voyages.Where(x => x.SFPM_VoyagesId < Convert.ToInt64(voyageId)).OrderByDescending(x => x.SFPM_VoyagesId).FirstOrDefault();
                                var aboveDeletePassage = voyages.Where(x => x.SFPM_VoyagesId > Convert.ToInt64(voyageId)).OrderByDescending(x => x.SFPM_VoyagesId).FirstOrDefault();
                                if (beloveDeletePassage != null)
                                {
                                    var newDate = beloveDeletePassage.ActualStartOfSeaPassage.Year + "/" + beloveDeletePassage.ActualStartOfSeaPassage.Month + "/" + beloveDeletePassage.ActualStartOfSeaPassage.Day + " " + beloveDeletePassage.ActualStartOfSeaPassage.Hour + ":" + beloveDeletePassage.ActualStartOfSeaPassage.Minute + ":" + beloveDeletePassage.ActualStartOfSeaPassage.Second;
                                    //bool positionkeyExists = RedisCacheHelper.IsKeyExists("noonreports:" + Convert.ToInt32(beloveDeletePassage.IMONumber) + "|" + Convert.ToInt32(beloveDeletePassage.VoyageNumber) + "|" + newDate);
                                    //if (positionkeyExists)
                                    //    RedisCacheHelper.DeleteKey("noonreports:" + Convert.ToInt32(beloveDeletePassage.IMONumber) + "|" + Convert.ToInt32(beloveDeletePassage.VoyageNumber) + "|" + newDate);
                                }
                                if (aboveDeletePassage != null)
                                {
                                    var newDate = aboveDeletePassage.ActualStartOfSeaPassage.Year + "/" + aboveDeletePassage.ActualStartOfSeaPassage.Month + "/" + aboveDeletePassage.ActualStartOfSeaPassage.Day + " " + aboveDeletePassage.ActualStartOfSeaPassage.Hour + ":" + aboveDeletePassage.ActualStartOfSeaPassage.Minute + ":" + aboveDeletePassage.ActualStartOfSeaPassage.Second;
                                    //bool positionkeyExists = RedisCacheHelper.IsKeyExists("noonreports:" + Convert.ToInt32(aboveDeletePassage.IMONumber) + "|" + Convert.ToInt32(aboveDeletePassage.VoyageNumber) + "|" + newDate);
                                    //if (positionkeyExists)
                                    //    RedisCacheHelper.DeleteKey("noonreports:" + Convert.ToInt32(aboveDeletePassage.IMONumber) + "|" + Convert.ToInt32(aboveDeletePassage.VoyageNumber) + "|" + newDate);
                                }
                            }
                        }

                        Response.Headers.Add("Status", HttpStatusCode.Created.ToString());
                        Response.Headers.Add("Message", MessagesResource.RecordDeleted);
                        return Ok();
                }
            }
            else
            {
                Response.Headers.Add("Status", HttpStatusCode.Conflict.ToString());
                Response.Headers.Add("Message", MessagesResource.InvalidDetails);
                return Conflict();
            }
        }

        [HttpPost, Route("initialapprove")]
        public IActionResult InitialApprove(bool isInitialApproved, long initialApprovedBy, string voyageIds)
        {
            if (initialApprovedBy != 0 && voyageIds.Count() > 0)
            {
                switch (_voyagesservice.InitialApprove(isInitialApproved, initialApprovedBy, voyageIds))
                {
                    case (int)ResponseStatus.INVALIDUSER:
                        Response.Headers.Add("Status", HttpStatusCode.Conflict.ToString());
                        Response.Headers.Add("Message", MessagesResource.InvalidUser);
                        return Conflict();
                    case (int)ResponseStatus.ALREADYAPPROVED:
                        Response.Headers.Add("Status", HttpStatusCode.Conflict.ToString());
                        //Response.Headers.Add("Message", MessagesResource.MultiPassagesApproved);
                        return Conflict();
                    case (int)ResponseStatus.APPROVALREQUIRED:
                        Response.Headers.Add("Status", HttpStatusCode.Conflict.ToString());
                        Response.Headers.Add("Message", MessagesResource.ApprovalRequired);
                        return Conflict();
                    default:
                        Response.Headers.Add("Status", HttpStatusCode.Created.ToString());
                        //Response.Headers.Add("Message", isInitialApproved ? MessagesResource.ApprovedSuccessfully : MessagesResource.UnapprovedSuccessfully);
                        return Ok();
                }
            }
            else
            {
                Response.Headers.Add("Status", HttpStatusCode.BadRequest.ToString());
                Response.Headers.Add("Message", MessagesResource.DatabaseInsertFailed);
                return BadRequest();
            }
        }

        [HttpPost, Route("finalapprove")]
        public IActionResult FinalApprove(bool isFinalApproved, long finalApprovedBy, string voyageIds)
        {
            if (finalApprovedBy != 0 && voyageIds.Count() > 0)
            {
                switch (_voyagesservice.FinalApprove(isFinalApproved, finalApprovedBy, voyageIds))
                {
                    case (int)ResponseStatus.INVALIDUSER:
                        Response.Headers.Add("Status", HttpStatusCode.Conflict.ToString());
                        Response.Headers.Add("Message", MessagesResource.InvalidUser);
                        return Conflict();
                    case (int)ResponseStatus.ALREADYAPPROVED:
                        Response.Headers.Add("Status", HttpStatusCode.Conflict.ToString());
                        //Response.Headers.Add("Message", MessagesResource.MultiPassagesApproved);
                        return Conflict();
                    case (int)ResponseStatus.APPROVALREQUIRED:
                        Response.Headers.Add("Status", HttpStatusCode.Conflict.ToString());
                        Response.Headers.Add("Message", MessagesResource.ApprovalRequired);
                        return Conflict();
                    case (int)ResponseStatus.INITIALAPPROVALREQUIRED:
                        Response.Headers.Add("Status", HttpStatusCode.BadRequest.ToString());
                        Response.Headers.Add("Message", MessagesResource.InitialApprovalRequired);
                        return BadRequest();
                    default:
                        Response.Headers.Add("Status", HttpStatusCode.Created.ToString());
                        //Response.Headers.Add("Message", isFinalApproved ? MessagesResource.ApprovedSuccessfully : MessagesResource.UnapprovedSuccessfully);
                        return Ok();
                }
            }
            else
            {
                Response.Headers.Add("Status", HttpStatusCode.BadRequest.ToString());
                Response.Headers.Add("Message", MessagesResource.DatabaseInsertFailed);
                return BadRequest();
            }
        }

        [HttpGet, Route("getpassagesapprovalauditList")]
        public IActionResult GetPassagesApprovalAuditList(long voyagesId)
        {
            long loginUserId = Convert.ToInt64(RouteData.Values["UserId"]);
            var getPassagesApprovalAuditList = _voyagesservice.GetPassagesApprovalAuditList(voyagesId, loginUserId);

            if (getPassagesApprovalAuditList != null)
            {
                Response.Headers.Add("Status", HttpStatusCode.Created.ToString());
                return Ok(getPassagesApprovalAuditList);
            }
            else
            {
                Response.Headers.Add("Status", HttpStatusCode.Conflict.ToString());
                Response.Headers.Add("Message", MessagesResource.InvalidDetails);
                return Conflict();
            }
            
        }

        [HttpGet, Route("getpassagewarning")]
        public IActionResult GetPassageWarning(long voyageId)
        {
            long loginUserId = Convert.ToInt64(RouteData.Values["UserId"]);
            var  getPAssageWarning= _voyagesservice.GetPassageWarning(voyageId, loginUserId);

            if(getPAssageWarning != null)
            {
                Response.Headers.Add("Status", HttpStatusCode.OK.ToString());
                return Ok(getPAssageWarning);
            }
            else
            {
                Response.Headers.Add("Status", HttpStatusCode.Conflict.ToString());
                Response.Headers.Add("Message", MessagesResource.InvalidDetails);
                return Conflict();
            }
           
        }

        [HttpGet, Route("getpassagewarningaudit")]
        public IActionResult GetPassageWarningAudit(long passageWarningId)
        {
            var getPassageWarningAudit = _voyagesservice.GetPassageWarningAudit(passageWarningId);
            Response.Headers.Add("Status", HttpStatusCode.OK.ToString());
            return Ok(getPassageWarningAudit);
        }

        [HttpPost, Route("createpassagewarningaudit")]
        public IActionResult CreatePassageWarningAudit(string IMONumber, [FromBody]PassageWarningAudit passageWarningAudit)
        {
            if (passageWarningAudit.PassageWarningId != 0 && passageWarningAudit.ReviewedBy != 0)
            {
                switch (_voyagesservice.CreatePassageWarningAudit(passageWarningAudit))
                {
                    case (int)ResponseStatus.INVALIDUSER:
                        Response.Headers.Add("Status", HttpStatusCode.Conflict.ToString());
                        Response.Headers.Add("Message", MessagesResource.InvalidUser);
                        return Conflict();
                    case (int)ResponseStatus.ALREADYEXIST:
                        Response.Headers.Add("Status", HttpStatusCode.Conflict.ToString());
                        Response.Headers.Add("Message", MessagesResource.DuplicateData);
                        return Conflict();
                    case (int)ResponseStatus.ALREADYAPPROVED:
                        Response.Headers.Add("Status", HttpStatusCode.BadRequest.ToString());
                        Response.Headers.Add("Message", MessagesResource.PassageAlreadyApproved);
                        return BadRequest();
                    default:
                        //bool keyExists = RedisCacheHelper.IsKeyExists("voyages:" + IMONumber);
                        //if (keyExists)
                        //    RedisCacheHelper.DeleteKey("voyages:" + IMONumber);
                        Response.Headers.Add("Status", HttpStatusCode.Created.ToString());
                        Response.Headers.Add("Message", MessagesResource.UpdatedSuccessfully);
                        return Ok();
                }
            }
            else
            {
                Response.Headers.Add("Status", HttpStatusCode.Conflict.ToString());
                Response.Headers.Add("Message", MessagesResource.DatabaseInsertFailed);
                return BadRequest();
            }
        }

        [HttpGet, Route("getvesselownernamebyimonumber")]
        public IActionResult GetVesselOwnerNameByImoNumber(string ImoNumber)
        {
            if (!string.IsNullOrEmpty(ImoNumber))
            {
                string OwnerName = _voyagesservice.GetVesselOwnerNameByImoNumber(ImoNumber);
                Response.Headers.Add("Status", HttpStatusCode.OK.ToString());
                return Ok(OwnerName);
            }
            else
            {
                Response.Headers.Add("Status", HttpStatusCode.NotFound.ToString());
                Response.Headers.Add("Message", MessagesResource.CodeUnmatched);
                return BadRequest();
            }
        }
        #endregion

        #region Position/Noon Reports

        [HttpGet, Route("getReportsForVoyage")]
        public IActionResult GetReportsForVoyage(string imoNumber, int voyageNo, DateTime actualStartOfSeaPassage, string departureTimeZoneOffset, long userId)
        {
            var pageSetting = _voyagesservice.GetPageSettings(userId);
            if (pageSetting != null && pageSetting.IsPassageUTC && !string.IsNullOrEmpty(departureTimeZoneOffset))
            {
                var hrs = Convert.ToDouble(departureTimeZoneOffset.Substring(1, 2));
                var min = Convert.ToDouble(departureTimeZoneOffset.Substring(4, 2));
                actualStartOfSeaPassage = departureTimeZoneOffset.Contains("-") ? actualStartOfSeaPassage.AddHours(-hrs).AddMinutes(-min) :
                    actualStartOfSeaPassage.AddHours(hrs).AddMinutes(min);
            }
            var newDate = actualStartOfSeaPassage.Year + "/" + actualStartOfSeaPassage.Month + "/" + actualStartOfSeaPassage.Day + " " + actualStartOfSeaPassage.Hour + ":" + actualStartOfSeaPassage.Minute + ":" + actualStartOfSeaPassage.Second;
            //IEnumerable<VoyagesIntitialDataViewModel> reportsList = RedisCacheHelper.Get<List<VoyagesIntitialDataViewModel>>("noonreports:" + imoNumber + "|" + voyageNo + "|" + newDate);
            //if (reportsList == null)
            //{
            //    reportsList = _voyagesservice.GetReportsForVoyage(imoNumber, voyageNo, actualStartOfSeaPassage, departureTimeZoneOffset, userId);
            //    RedisCacheHelper.Set("noonreports:" + imoNumber + "|" + voyageNo + "|" + newDate, reportsList);
            //}
            IEnumerable<VoyagesIntitialDataViewModel> reportsList = _voyagesservice.GetReportsForVoyage(imoNumber, voyageNo, actualStartOfSeaPassage, departureTimeZoneOffset, userId);
            //date formatting
            if (reportsList != null)
            {
                foreach (var report in reportsList)
                {
                    if (pageSetting != null && pageSetting.IsPositionUTC && !string.IsNullOrEmpty(report.TimeZone))
                    {
                        var hrs = Convert.ToDouble(report.TimeZone.Substring(1, 2));
                        var min = Convert.ToDouble(report.TimeZone.Substring(4, 2));
                        report.DateAndTime = report.TimeZone.Contains("-") ? report.DateAndTime.Value.DateTime.AddHours(hrs).AddMinutes(min) :
                            report.DateAndTime.Value.DateTime.AddHours(-hrs).AddMinutes(-min);
                    }
                    else
                    {
                        break;
                    }
                }
                Response.Headers.Add("Status", HttpStatusCode.OK.ToString());
                return Ok(reportsList);

            }
            else
            {
                Response.Headers.Add("Status", HttpStatusCode.Conflict.ToString());
                Response.Headers.Add("Message", MessagesResource.InvalidDetails);
                return Conflict();
            }
            
        }

        [HttpPost, Route("createposition")]
        public IActionResult CreatePosition([FromBody]Forms formData)
        {
            int id = 0;
            long loginUserId = Convert.ToInt64(RouteData.Values["UserId"]);
            if (!string.IsNullOrEmpty(formData.FormIdentifier) && formData.ReportDateTime != null && !string.IsNullOrWhiteSpace(formData.FormIdentifier) && formData.FormIdentifier.Contains("Optimum Bunker")
                && formData.VoyageNo != null && formData.ImoNumber != null)
            {
                id = _voyagesservice.CreatePosition(formData,loginUserId);
            }
            else if (!string.IsNullOrEmpty(formData.FormIdentifier) && !string.IsNullOrWhiteSpace(formData.FormIdentifier) && formData.ReportDateTime != null
                 && formData.Longitude != null && formData.Latitude != null && formData.SteamingHrs != null && formData.FWDDraft != null && formData.DistanceToGO != null
                 && formData.EngineDistance != null && formData.Slip != null && formData.AvgBHP != null && formData.WindDirection != null && formData.SeaDir != null && formData.VoyageNo != null && formData.ImoNumber != null)
            {
                id = _voyagesservice.CreatePosition(formData,loginUserId);
            }
            else
            {
                Response.Headers.Add("Status", HttpStatusCode.Conflict.ToString());
                Response.Headers.Add("Message", MessagesResource.DatabaseInsertFailed);
                return BadRequest();
            }
            switch (id)
            {
                case (int)ResponseStatus.ALREADYEXIST:
                    Response.Headers.Add("Status", HttpStatusCode.Conflict.ToString());
                    Response.Headers.Add("Message", MessagesResource.DuplicateData);
                    return Conflict();
                case (int)ResponseStatus.ALREADYAPPROVED:
                    Response.Headers.Add("Status", HttpStatusCode.BadRequest.ToString());
                    Response.Headers.Add("Message", MessagesResource.PassageAlreadyApproved);
                    return BadRequest();
                default:
                    Voyages objVoyage = _voyagesservice.GetActualStartPassage(formData);
                    if (objVoyage != null)
                    {
                        var newDate = objVoyage.ActualStartOfSeaPassage.Year + "/" + objVoyage.ActualStartOfSeaPassage.Month + "/" + objVoyage.ActualStartOfSeaPassage.Day + " " + objVoyage.ActualStartOfSeaPassage.Hour + ":" + objVoyage.ActualStartOfSeaPassage.Minute + ":" + objVoyage.ActualStartOfSeaPassage.Second;
                        //bool keyExists = RedisCacheHelper.IsKeyExists("noonreports:" + formData.ImoNumber + "|" + formData.VoyageNo + "|" + newDate);
                        //if (keyExists)
                        //    RedisCacheHelper.DeleteKey("noonreports:" + formData.ImoNumber + "|" + formData.VoyageNo + "|" + newDate);
                    }
                    Response.Headers.Add("Status", HttpStatusCode.Created.ToString());
                    Response.Headers.Add("Message", MessagesResource.InsertedSuccessfully);
                    return Ok(formData);
            }
        }

        [HttpDelete, Route("deleteposition")]
        public IActionResult DeletePosition(string formIds)
        {
            long loginUserId = Convert.ToInt64(RouteData.Values["UserId"]);
            if (!string.IsNullOrEmpty(formIds))
            {
                string formId = formIds.Split(',').FirstOrDefault();
                Forms form = _voyagesservice.GetPositionById(Convert.ToInt64(formId), loginUserId);

                int id = _voyagesservice.DeletePosition(formIds);
                if (id != 0)
                {
                    Voyages objVoyage = _voyagesservice.GetActualStartPassage(form);
                    var newDate = objVoyage.ActualStartOfSeaPassage.Year + "/" + objVoyage.ActualStartOfSeaPassage.Month + "/" + objVoyage.ActualStartOfSeaPassage.Day + " " + objVoyage.ActualStartOfSeaPassage.Hour + ":" + objVoyage.ActualStartOfSeaPassage.Minute + ":" + objVoyage.ActualStartOfSeaPassage.Second;
                    //bool keyExists = RedisCacheHelper.IsKeyExists("noonreports:" + form.ImoNumber + "|" + form.VoyageNo + "|" + newDate);
                    //if (keyExists)
                    //    RedisCacheHelper.DeleteKey("noonreports:" + form.ImoNumber + "|" + form.VoyageNo + "|" + newDate);

                    Response.Headers.Add("Status", HttpStatusCode.OK.ToString());
                    Response.Headers.Add("Message", MessagesResource.RecordDeleted);
                    return Ok();
                }
                else
                {
                    Response.Headers.Add("Status", HttpStatusCode.NotFound.ToString());
                    Response.Headers.Add("Message", MessagesResource.RecordNotFound);
                    return NotFound();
                }
            }
            else
            {
                Response.Headers.Add("Status", HttpStatusCode.Conflict.ToString());
                Response.Headers.Add("Message", MessagesResource.InvalidDetails);
                return Conflict();
            }
        }

        [HttpPut, Route("updatepositionremark")]
        public IActionResult UpdatePositionRemark(long formId, string remark)
        {
            long loginUserId = Convert.ToInt64(RouteData.Values["UserId"]);
            if (formId != 0)
            {
                if (_voyagesservice.IsVoyageApproved(formId))
                {
                    Response.Headers.Add("Status", HttpStatusCode.BadRequest.ToString());
                    Response.Headers.Add("Message", MessagesResource.PassageAlreadyApproved);
                    return BadRequest();
                }
                int id = _voyagesservice.UpdatePositionRemark(formId, remark,loginUserId);
                if (id != 0)
                {
                    Response.Headers.Add("Status", HttpStatusCode.Created.ToString());
                    Response.Headers.Add("Message", MessagesResource.UpdatedSuccessfully);
                    return Ok();
                }
                else
                {
                    Response.Headers.Add("Status", HttpStatusCode.NotFound.ToString());
                    Response.Headers.Add("Message", MessagesResource.RecordNotFound);
                    return NotFound();
                }
            }
            else
            {
                Response.Headers.Add("Status", HttpStatusCode.BadRequest.ToString());
                Response.Headers.Add("Message", MessagesResource.DatabaseInsertFailed);
                return BadRequest();

            }
        }

        [HttpGet, Route("getpositionwarning")]
        public IActionResult GetPositionWarning(long formId)
        {
            long loginUserId = Convert.ToInt64(RouteData.Values["UserId"]);
            var getPostionWarning = _voyagesservice.GetPositionWarning(formId, loginUserId);

            if(getPostionWarning != null)
            {
                Response.Headers.Add("Status", HttpStatusCode.OK.ToString());
                return Ok(getPostionWarning);
            }
            else
            {
                Response.Headers.Add("Status", HttpStatusCode.Conflict.ToString());
                Response.Headers.Add("Message", MessagesResource.InvalidDetails);
                return Conflict();
            }
        }

        [HttpGet, Route("getpositionwarningaudit")]
        public IActionResult GetPositionWarningAudit(long positionWarningId)
        {
            var getPositionWarningAudit = _voyagesservice.GetPositionWarningAudit(positionWarningId);
            Response.Headers.Add("Status", HttpStatusCode.OK.ToString());
            return Ok(getPositionWarningAudit);
           
        }

        [HttpPost, Route("createpositionwarningaudit")]
        public IActionResult CreatePositionWarningAudit(long formId, [FromBody]PositionWarningAudit positionWarningAudit)
        {
            long loginUserId = Convert.ToInt64(RouteData.Values["UserId"]);
            if (positionWarningAudit.PositionWarningId != 0 && positionWarningAudit.ReviewedBy != 0)
            {
                switch (_voyagesservice.CreatePositionWarningAudit(positionWarningAudit,loginUserId))
                {
                    case (int)ResponseStatus.INVALIDUSER:
                        Response.Headers.Add("Status", HttpStatusCode.Conflict.ToString());
                        Response.Headers.Add("Message", MessagesResource.InvalidUser);
                        return Conflict();
                    case (int)ResponseStatus.ALREADYEXIST:
                        Response.Headers.Add("Status", HttpStatusCode.Conflict.ToString());
                        Response.Headers.Add("Message", MessagesResource.DuplicateData);
                        return Conflict();
                    case (int)ResponseStatus.ALREADYAPPROVED:
                        Response.Headers.Add("Status", HttpStatusCode.BadRequest.ToString());
                        Response.Headers.Add("Message", MessagesResource.PassageAlreadyApproved);
                        return BadRequest();
                    default:
                        var forms = _voyagesservice.GetPositionById(formId, loginUserId);
                        if (forms != null)
                        {
                            Voyages objVoyage = _voyagesservice.GetActualStartPassage(forms);
                            var newDate = objVoyage.ActualStartOfSeaPassage.Year + "/" + objVoyage.ActualStartOfSeaPassage.Month + "/" + objVoyage.ActualStartOfSeaPassage.Day + " " + objVoyage.ActualStartOfSeaPassage.Hour + ":" + objVoyage.ActualStartOfSeaPassage.Minute + ":" + objVoyage.ActualStartOfSeaPassage.Second;
                            //bool keyExists = RedisCacheHelper.IsKeyExists("noonreports:" + forms.ImoNumber + "|" + forms.VoyageNo + "|" + newDate);
                            //if (keyExists)
                            //    RedisCacheHelper.DeleteKey("noonreports:" + forms.ImoNumber + "|" + forms.VoyageNo + "|" + newDate);

                        }
                        Response.Headers.Add("Status", HttpStatusCode.Created.ToString());
                        Response.Headers.Add("Message", MessagesResource.UpdatedSuccessfully);
                        return Ok();
                }

            }
            else
            {
                Response.Headers.Add("Status", HttpStatusCode.BadRequest.ToString());
                Response.Headers.Add("Message", MessagesResource.DatabaseInsertFailed);
                return BadRequest();
            }
        }

        [HttpGet, Route("getvieworiginalemail")]
        public IActionResult GetViewOriginalEmail(long formId)
        {
            long loginUserId = Convert.ToInt64(RouteData.Values["UserId"]);
            var getOriginalEmail = _voyagesservice.GetViewOriginalEmail(formId, loginUserId);
            if(getOriginalEmail != null)
            {
                Response.Headers.Add("Status", HttpStatusCode.OK.ToString());
                return Ok(getOriginalEmail);
            }
            else
            {
                Response.Headers.Add("Status", HttpStatusCode.Conflict.ToString());
                Response.Headers.Add("Message", MessagesResource.InvalidDetails);
                return Conflict();
            }
        }

        [HttpPut, Route("updateposition")]
        public IActionResult UpdatePosition([FromBody]Forms forms)
        {
            int id = 0;
            if (!string.IsNullOrEmpty(forms.FormIdentifier) && !string.IsNullOrWhiteSpace(forms.FormIdentifier) && forms.ReportDateTime != null && forms.FormIdentifier.ToLower().Contains(BunkerConstant.Bunker)
            && forms.VoyageNo != null && forms.ImoNumber != null && forms.SFPM_Form_Id != 0)
            {
                id = _voyagesservice.UpdatePosition(forms);
            }
            else if (!string.IsNullOrEmpty(forms.FormIdentifier) && !string.IsNullOrWhiteSpace(forms.FormIdentifier) && forms.ReportDateTime != null
                 && forms.Longitude != null && forms.Latitude != null && forms.SteamingHrs != null && forms.FWDDraft != null && forms.DistanceToGO != null
                 && forms.EngineDistance != null && forms.Slip != null && forms.AvgBHP != null && forms.WindDirection != null && forms.SeaDir != null
                 && forms.VoyageNo != null && forms.ImoNumber != null && forms.SFPM_Form_Id != 0)
            {
                id = _voyagesservice.UpdatePosition(forms);
            }
            else
            {
                Response.Headers.Add("Status", HttpStatusCode.Conflict.ToString());
                Response.Headers.Add("Message", MessagesResource.DatabaseInsertFailed);
                return BadRequest();
            }
            switch (id)
            {
                case (int)ResponseStatus.NOTFOUND:
                    Response.Headers.Add("Status", HttpStatusCode.NotFound.ToString());
                    Response.Headers.Add("Message", MessagesResource.RecordNotFound);
                    return NotFound();
                case (int)ResponseStatus.ALREADYAPPROVED:
                    Response.Headers.Add("Status", HttpStatusCode.BadRequest.ToString());
                    Response.Headers.Add("Message", MessagesResource.PassageAlreadyApproved);
                    return BadRequest();
                default:
                    Voyages objVoyage = _voyagesservice.GetActualStartPassage(forms);
                    if (objVoyage != null)
                    {
                        var newDate = objVoyage.ActualStartOfSeaPassage.Year + "/" + objVoyage.ActualStartOfSeaPassage.Month + "/" + objVoyage.ActualStartOfSeaPassage.Day + " " + objVoyage.ActualStartOfSeaPassage.Hour + ":" + objVoyage.ActualStartOfSeaPassage.Minute + ":" + objVoyage.ActualStartOfSeaPassage.Second;
                        //bool keyExists = RedisCacheHelper.IsKeyExists("noonreports:" + forms.ImoNumber + "|" + forms.VoyageNo + "|" + newDate);
                        //if (keyExists)
                        //    RedisCacheHelper.DeleteKey("noonreports:" + forms.ImoNumber + "|" + forms.VoyageNo + "|" + newDate);
                    }
                    Response.Headers.Add("Status", HttpStatusCode.Created.ToString());
                    Response.Headers.Add("Message", MessagesResource.UpdatedSuccessfully);
                    return Ok();
            }
        }

        [HttpGet, Route("getpositionbyid")]
        public IActionResult GetPositionById(long formId)
        {
            long loginUserId = Convert.ToInt64(RouteData.Values["UserId"]);
            var getPositionbyId = _voyagesservice.GetPositionById(formId, loginUserId);

            if(getPositionbyId != null)
            {
                Response.Headers.Add("Status", HttpStatusCode.OK.ToString());
                return Ok(getPositionbyId);
            }
            else
            {
                Response.Headers.Add("Status", HttpStatusCode.Conflict.ToString());
                Response.Headers.Add("Message", MessagesResource.InvalidDetails);
                return Conflict();
            }
           

        }


        public class BunkerConstant
        {
            public const string Bunker = "bunker";
        }
        #endregion

        #region Event

        [HttpGet, Route("getallevents")]
        public IActionResult GetAllEvents(long formId)
        {

            //List<EventROBsRow> allEventsList = RedisCacheHelper.Get<List<EventROBsRow>>("events:" + formId);
            //if (allEventsList == null)
            //{
            long loginUserId = Convert.ToInt64(RouteData.Values["UserId"]);
            List<EventROBsRow> allEventsList = _voyagesservice.GetAllEvents(formId, loginUserId);
            //    RedisCacheHelper.Set("events:" + formId, allEventsList);
            //}
            if(allEventsList.Count() >0)
            {
                Response.Headers.Add("Status", HttpStatusCode.OK.ToString());
                return Ok(allEventsList);
            }
            else
            {
                Response.Headers.Add("Status", HttpStatusCode.Conflict.ToString());
                Response.Headers.Add("Message", MessagesResource.InvalidDetails);
                return Conflict();
            }
            
        }
        [HttpGet, Route("geteventbyid")]
        public IActionResult GetEventById(long eventId)
        {
            long loginUserId = Convert.ToInt64(RouteData.Values["UserId"]);
            var getEventById = _voyagesservice.GetEventById(eventId, loginUserId);
            if(getEventById != null)
            {
                Response.Headers.Add("Status", HttpStatusCode.OK.ToString());
                return Ok(getEventById);
            }
            else
            {
                Response.Headers.Add("Status", HttpStatusCode.Conflict.ToString());
                Response.Headers.Add("Message", MessagesResource.InvalidDetails);
                return Conflict();
            }
            
        }
        [HttpPost, Route("createevent")]
        public IActionResult CreateEvent([FromBody]EventROBsRow eventROBsRow)
        {


            if (!string.IsNullOrEmpty(eventROBsRow.EventType) && eventROBsRow.FormId != 0)
            {

                if (_voyagesservice.IsVoyageApproved(eventROBsRow.FormId))
                {
                    Response.Headers.Add("Status", HttpStatusCode.BadRequest.ToString());
                    Response.Headers.Add("Message", MessagesResource.PassageAlreadyApproved);
                    return BadRequest();
                }
                int create = _voyagesservice.CreateEvent(eventROBsRow);
                if (create == (int)ResponseStatus.SAVED)
                {
                    //we removed all redis cache code,so no use of this code

                    //var formData = _voyagesservice.GetPositionById(eventROBsRow.FormId);
                    //Voyages objVoyage = _voyagesservice.GetActualStartPassage(formData);
                    // var newDate = objVoyage.ActualStartOfSeaPassage.Year + "/" + objVoyage.ActualStartOfSeaPassage.Month + "/" + objVoyage.ActualStartOfSeaPassage.Day + " " + objVoyage.ActualStartOfSeaPassage.Hour + ":" + objVoyage.ActualStartOfSeaPassage.Minute + ":" + objVoyage.ActualStartOfSeaPassage.Second;
                    // bool keyExists = RedisCacheHelper.IsKeyExists("noonreports:" + formData.ImoNumber + "|" + formData.VoyageNo + "|" + newDate);
                    //if (keyExists)
                    //    RedisCacheHelper.DeleteKey("noonreports:" + formData.ImoNumber + "|" + formData.VoyageNo + "|" + newDate);

                    Response.Headers.Add("Status", HttpStatusCode.Created.ToString());
                    Response.Headers.Add("Message", MessagesResource.InsertedSuccessfully);
                    return Ok(eventROBsRow);
                }
                else
                {
                    Response.Headers.Add("Status", HttpStatusCode.Conflict.ToString());
                    Response.Headers.Add("Message", MessagesResource.DuplicateData);
                    return Conflict();
                }
            }
            else
            {
                Response.Headers.Add("Status", HttpStatusCode.BadRequest.ToString());
                Response.Headers.Add("Message", MessagesResource.RequiredField);
                return BadRequest();
            }
        }

        [HttpPut, Route("updateevent")]
        public IActionResult UpdateEvent([FromBody]EventROBsRow eventROBsRow)
        {
            long loginUserId = Convert.ToInt64(RouteData.Values["UserId"]);
            if (!string.IsNullOrEmpty(eventROBsRow.EventType) && eventROBsRow.FormId != 0 && eventROBsRow.SFPM_EventROBsRowId != 0)
            {
                if (_voyagesservice.IsVoyageApproved(eventROBsRow.FormId))
                {
                    Response.Headers.Add("Status", HttpStatusCode.BadRequest.ToString());
                    Response.Headers.Add("Message", MessagesResource.PassageAlreadyApproved);
                    return BadRequest();
                }
                int create = _voyagesservice.UpdateEvent(eventROBsRow, loginUserId);
                if (create == (int)ResponseStatus.SAVED)
                {
                    //we removed all redis cache code,so no use of this code

                    //var formData = _voyagesservice.GetPositionById(eventROBsRow.FormId);
                    //Voyages objVoyage = _voyagesservice.GetActualStartPassage(formData);
                    //var newDate = objVoyage.ActualStartOfSeaPassage.Year + "/" + objVoyage.ActualStartOfSeaPassage.Month + "/" + objVoyage.ActualStartOfSeaPassage.Day + " " + objVoyage.ActualStartOfSeaPassage.Hour + ":" + objVoyage.ActualStartOfSeaPassage.Minute + ":" + objVoyage.ActualStartOfSeaPassage.Second;
                    //bool keyExists = RedisCacheHelper.IsKeyExists("noonreports:" + formData.ImoNumber + "|" + formData.VoyageNo + "|" + newDate);
                    //if (keyExists)
                    //    RedisCacheHelper.DeleteKey("noonreports:" + formData.ImoNumber + "|" + formData.VoyageNo + "|" + newDate);

                    Response.Headers.Add("Status", HttpStatusCode.Created.ToString());
                    Response.Headers.Add("Message", MessagesResource.UpdatedSuccessfully);
                    return Ok(eventROBsRow);
                }
                else
                {
                    Response.Headers.Add("Status", HttpStatusCode.Conflict.ToString());
                    Response.Headers.Add("Message", MessagesResource.DuplicateData);
                    return Conflict();
                }

            }
            else
            {
                Response.Headers.Add("Status", HttpStatusCode.BadRequest.ToString());
                Response.Headers.Add("Message", string.Format(MessagesResource.DatabaseInsertFailed));
                return BadRequest();
            }
        }
        [HttpDelete, Route("deleteevent")]
        public IActionResult DeleteEvent(long eventId)
        {
            long loginUserId = Convert.ToInt64(RouteData.Values["UserId"]);
            var formId = _voyagesservice.GetEventById(eventId, loginUserId).FormId;
            switch (_voyagesservice.DeleteEvent(eventId, loginUserId))
            {
                case (int)ResponseStatus.NOTFOUND:
                    Response.Headers.Add("Status", HttpStatusCode.Conflict.ToString());
                    Response.Headers.Add("Message", MessagesResource.RecordNotFound);
                    return Conflict();
                case (int)ResponseStatus.CURRENTLYINUSE:
                    Response.Headers.Add("Status", HttpStatusCode.BadRequest.ToString());
                    Response.Headers.Add("Message", MessagesResource.RecordInUse);
                    return BadRequest();
                case (int)ResponseStatus.ALREADYAPPROVED:
                    Response.Headers.Add("Status", HttpStatusCode.BadRequest.ToString());
                    Response.Headers.Add("Message", MessagesResource.PassageAlreadyApproved);
                    return BadRequest();
                default:
                    var formData = _voyagesservice.GetPositionById(formId, loginUserId);
                    Voyages objVoyage = _voyagesservice.GetActualStartPassage(formData);
                    var newDate = objVoyage.ActualStartOfSeaPassage.Year + "/" + objVoyage.ActualStartOfSeaPassage.Month + "/" + objVoyage.ActualStartOfSeaPassage.Day + " " + objVoyage.ActualStartOfSeaPassage.Hour + ":" + objVoyage.ActualStartOfSeaPassage.Minute + ":" + objVoyage.ActualStartOfSeaPassage.Second;
                    //bool keyExists = RedisCacheHelper.IsKeyExists("noonreports:" + formData.ImoNumber + "|" + formData.VoyageNo + "|" + newDate);
                    //if (keyExists)
                    //    RedisCacheHelper.DeleteKey("noonreports:" + formData.ImoNumber + "|" + formData.VoyageNo + "|" + newDate);

                    Response.Headers.Add("Status", HttpStatusCode.Created.ToString());
                    Response.Headers.Add("Message", MessagesResource.RecordDeleted);
                    return Ok();
            }
        }


        #endregion

        #region Fluid Consumption (Postion / Event)

        [HttpGet, Route("getallfluidconsumption")]
        public IActionResult GetAllFluidConsumption(long formId)
        {

            //List<Rob> allFluidConsumptionList = RedisCacheHelper.Get<List<Rob>>("fliud:" + formId);
            //if (allFluidConsumptionList == null)
            //{
            long loginUserId = Convert.ToInt64(RouteData.Values["UserId"]);
            var allFluidConsumptionList = _voyagesservice.GetAllFluidConsumption(formId, loginUserId);
            //    RedisCacheHelper.Set("fliud:" + formId, allFluidConsumptionList);
            //}

            if(allFluidConsumptionList != null)
            {
                Response.Headers.Add("Status", HttpStatusCode.OK.ToString());
                return Ok(allFluidConsumptionList);
            }
              
            else
            {
                Response.Headers.Add("Status", HttpStatusCode.Conflict.ToString());
                Response.Headers.Add("Message", MessagesResource.InvalidDetails);
                return Conflict();
            }
        }
        [HttpGet, Route("getfluidconsumptionbyId")]
        public IActionResult GetFluidConsumptionById(long robId)
        {
            var rob = _voyagesservice.GetFluidConsumptionById(robId);
            if (rob != null)
            {
                Response.Headers.Add("Status", HttpStatusCode.OK.ToString());
                return Ok(rob);
            }
            else
            {
                Response.Headers.Add("Status", HttpStatusCode.NotFound.ToString());
                Response.Headers.Add("Message", MessagesResource.RecordNotFound);
                return BadRequest();
            }
        }
        [HttpGet, Route("getalleventfluidconsumption")]
        public IActionResult GetAllEventFluidConsumption(long formId, long eventId)
        {
            //IEnumerable<Rob> allEventFluidConsumptionList = RedisCacheHelper.Get<List<Rob>>("eventFluid:" + formId);
            //if (allEventFluidConsumptionList == null)
            //{
            long loginUserId = Convert.ToInt64(RouteData.Values["UserId"]);
            var allEventFluidConsumptionList = _voyagesservice.GetAllEventFluidConsumption(formId, eventId, loginUserId);
            //    RedisCacheHelper.Set("eventFluid:" + formId, allEventFluidConsumptionList);
            //}
            if(allEventFluidConsumptionList != null)
            {
                Response.Headers.Add("Status", HttpStatusCode.OK.ToString());
                return Ok(allEventFluidConsumptionList);
            }
            else
            {
                Response.Headers.Add("Status", HttpStatusCode.Conflict.ToString());
                Response.Headers.Add("Message", MessagesResource.InvalidDetails);
                return Conflict();
            }
           
        }
        [HttpPost, Route("createfluidconsumption")]
        public IActionResult CreateFluidConsumption([FromBody]FluidFuelConsumedViewModel fluidFuelConsumed)
        {
            if (!_voyagesservice.CheckFormExists(fluidFuelConsumed.FormId) || string.IsNullOrEmpty(fluidFuelConsumed.FluidType) || string.IsNullOrEmpty(fluidFuelConsumed.Unit)
                || string.IsNullOrEmpty(fluidFuelConsumed.Category) || fluidFuelConsumed.Consumption == 0)
            {
                Response.Headers.Add("Status", HttpStatusCode.Conflict.ToString());
                Response.Headers.Add("Message", MessagesResource.InvalidDetails);
                return BadRequest();
            }
            if (_voyagesservice.IsVoyageApproved(fluidFuelConsumed.FormId))
            {
                Response.Headers.Add("Status", HttpStatusCode.BadRequest.ToString());
                Response.Headers.Add("Message", MessagesResource.PassageAlreadyApproved);
                return BadRequest();
            }
            long allocationId = 0;
            long id = _voyagesservice.CreateFluidConsumption(fluidFuelConsumed, out allocationId);
            if (id != 0)
            {
                fluidFuelConsumed.RobId = id;
                fluidFuelConsumed.AllocationId = allocationId;
                Response.Headers.Add("Status", HttpStatusCode.Created.ToString());
                Response.Headers.Add("Message", MessagesResource.InsertedSuccessfully);
                return Ok(fluidFuelConsumed);
            }
            else
            {
                Response.Headers.Add("Status", HttpStatusCode.Conflict.ToString());
                Response.Headers.Add("Message", MessagesResource.DuplicateData);
                return Conflict();
            }
        }
        [HttpPost, Route("createeventfluidconsumption")]
        public IActionResult CreateEventFluidConsumption([FromBody]FluidFuelConsumedViewModel fluidFuelConsumed)
        {
            if (!_voyagesservice.CheckEventExists(fluidFuelConsumed.FormId, (long)fluidFuelConsumed.EventRobsRowId) || string.IsNullOrEmpty(fluidFuelConsumed.FluidType)
                || string.IsNullOrEmpty(fluidFuelConsumed.Unit) || string.IsNullOrEmpty(fluidFuelConsumed.Category) || fluidFuelConsumed.Consumption == 0)
            {
                Response.Headers.Add("Status", HttpStatusCode.Conflict.ToString());
                Response.Headers.Add("Message", MessagesResource.InvalidDetails);
                return BadRequest();
            }
            if (_voyagesservice.IsVoyageApproved(fluidFuelConsumed.FormId))
            {
                Response.Headers.Add("Status", HttpStatusCode.BadRequest.ToString());
                Response.Headers.Add("Message", MessagesResource.PassageAlreadyApproved);
                return BadRequest();
            }
            long allocationId = 0;
            long id = _voyagesservice.CreateEventFluidConsumption(fluidFuelConsumed, out allocationId);
            if (id != 0)
            {
                fluidFuelConsumed.RobId = id;
                fluidFuelConsumed.AllocationId = allocationId;
                Response.Headers.Add("Status", HttpStatusCode.Created.ToString());
                Response.Headers.Add("Message", MessagesResource.InsertedSuccessfully);
                return Ok(fluidFuelConsumed);
            }
            else
            {
                Response.Headers.Add("Status", HttpStatusCode.Conflict.ToString());
                Response.Headers.Add("Message", MessagesResource.DuplicateData);
                return Conflict();
            }
        }
        [HttpPut, Route("updatefluidconsumption")]
        public IActionResult UpdateFluidConsumption([FromBody]FluidFuelConsumedViewModel fluidFuelConsumed)
        {
            if (!_voyagesservice.CheckFormExists(fluidFuelConsumed.FormId) || string.IsNullOrEmpty(fluidFuelConsumed.FluidType) || string.IsNullOrEmpty(fluidFuelConsumed.Unit)
                || string.IsNullOrEmpty(fluidFuelConsumed.Category) || fluidFuelConsumed.Consumption == 0)
            {
                Response.Headers.Add("Status", HttpStatusCode.Conflict.ToString());
                Response.Headers.Add("Message", MessagesResource.InvalidDetails);
                return BadRequest();
            }
            if (_voyagesservice.IsVoyageApproved(fluidFuelConsumed.FormId))
            {
                Response.Headers.Add("Status", HttpStatusCode.BadRequest.ToString());
                Response.Headers.Add("Message", MessagesResource.PassageAlreadyApproved);
                return BadRequest();
            }
            switch (_voyagesservice.UpdateFluidConsumption(fluidFuelConsumed))
            {
                case (int)ResponseStatus.NOTFOUND:
                    Response.Headers.Add("Status", HttpStatusCode.Conflict.ToString());
                    Response.Headers.Add("Message", MessagesResource.RecordNotFound);
                    return Conflict();
                case (int)ResponseStatus.ALREADYEXIST:
                    Response.Headers.Add("Status", HttpStatusCode.BadRequest.ToString());
                    Response.Headers.Add("Message", MessagesResource.DuplicateData);
                    return Conflict();
                default:
                    Response.Headers.Add("Status", HttpStatusCode.Created.ToString());
                    Response.Headers.Add("Message", MessagesResource.UpdatedSuccessfully);
                    return Ok(fluidFuelConsumed);
            }
        }
        [HttpDelete, Route("deletefluidconsumption")]
        public IActionResult DeleteFluidConsumption(long robId, long allocationId)
        {
            switch (_voyagesservice.DeleteFluidConsumption(robId, allocationId))
            {
                case (int)ResponseStatus.NOTFOUND:
                    Response.Headers.Add("Status", HttpStatusCode.Conflict.ToString());
                    Response.Headers.Add("Message", MessagesResource.RecordNotFound);
                    return BadRequest();
                case (int)ResponseStatus.ALREADYAPPROVED:
                    Response.Headers.Add("Status", HttpStatusCode.BadRequest.ToString());
                    Response.Headers.Add("Message", MessagesResource.PassageAlreadyApproved);
                    return BadRequest();
                default:
                    Response.Headers.Add("Status", HttpStatusCode.Created.ToString());
                    Response.Headers.Add("Message", MessagesResource.RecordDeleted);
                    return Ok();
            }
        }
        #endregion

        #region Bunker (Postion / Event)

        [HttpGet, Route("getAllfluidbunker")]
        public IActionResult GetAllFluidBunker(int formId)
        {
            //IEnumerable<FluidBunkerViewModel> allFluidBunkerList = RedisCacheHelper.Get<List<FluidBunkerViewModel>>("fluidBunker:" + formId);
            //if (allFluidBunkerList == null)
            //{
            long loginUserId = Convert.ToInt64(RouteData.Values["UserId"]);
            var allFluidBunkerList = _voyagesservice.GetAllFluidBunker(formId, loginUserId);
            //    RedisCacheHelper.Set("fluidBunker:" + formId, allFluidBunkerList);
            //}
            if(allFluidBunkerList != null)
            {
                Response.Headers.Add("Status", HttpStatusCode.OK.ToString());
                return Ok(allFluidBunkerList);
            }
            else
            {
                Response.Headers.Add("Status", HttpStatusCode.Conflict.ToString());
                Response.Headers.Add("Message", MessagesResource.InvalidDetails);
                return Conflict();
            }
        }
        [HttpGet, Route("getfluidbunkerbyid")]
        public IActionResult GetFluidBunkerById(long robId, string bunkerType)
        {
            Response.Headers.Add("Status", HttpStatusCode.OK.ToString());
            return Ok(_voyagesservice.GetFluidBunkerById(robId, bunkerType));
        }
        [HttpGet, Route("getalleventfluidbunker")]
        public IActionResult GetAllEventFluidBunker(long formId, long eventRobsRowId)
        {

            //IEnumerable<FluidBunkerViewModel> allEventFluidBunkerList = RedisCacheHelper.Get<List<FluidBunkerViewModel>>("eventFluidBunker:" + formId);
            //if (allEventFluidBunkerList == null)
            //{
            long loginUserId = Convert.ToInt64(RouteData.Values["UserId"]);
            var allEventFluidBunkerList = _voyagesservice.GetAllEventFluidBunker(formId, eventRobsRowId, loginUserId);
            //    RedisCacheHelper.Set("eventFluidBunker:" + formId, allEventFluidBunkerList);
            //}

            if(allEventFluidBunkerList != null)
            {
                Response.Headers.Add("Status", HttpStatusCode.OK.ToString());
                return Ok(allEventFluidBunkerList);
            }
            else
            {
                Response.Headers.Add("Status", HttpStatusCode.Conflict.ToString());
                Response.Headers.Add("Message", MessagesResource.InvalidDetails);
                return Conflict();
            }
        }

        [HttpPost, Route("createfluidbunker")]
        public IActionResult CreateFluidBunker([FromBody]FluidBunkerViewModel fluidBunker)
        {
            if (!_voyagesservice.CheckFormExists(fluidBunker.FormId) || string.IsNullOrEmpty(fluidBunker.FluidType) || string.IsNullOrEmpty(fluidBunker.Unit)
                || string.IsNullOrEmpty(fluidBunker.BunkerType) || fluidBunker.Consumption == 0)
            {
                Response.Headers.Add("Status", HttpStatusCode.BadRequest.ToString());
                Response.Headers.Add("Message", MessagesResource.InvalidDetails);
                return BadRequest();
            }
            if (_voyagesservice.IsVoyageApproved(fluidBunker.FormId))
            {
                Response.Headers.Add("Status", HttpStatusCode.BadRequest.ToString());
                Response.Headers.Add("Message", MessagesResource.PassageAlreadyApproved);
                return BadRequest();
            }
            long id = _voyagesservice.CreateFluidBunker(fluidBunker);
            if (id != 0)
            {
                fluidBunker.RobId = id;
                Response.Headers.Add("Status", HttpStatusCode.Created.ToString());
                Response.Headers.Add("Message", MessagesResource.InsertedSuccessfully);
                return Ok(fluidBunker);
            }
            else
            {
                Response.Headers.Add("Status", HttpStatusCode.Conflict.ToString());
                Response.Headers.Add("Message", MessagesResource.DuplicateData);
                return Conflict();
            }
        }
        [HttpPut, Route("updatefluidbunker")]
        public IActionResult UpdateFluidBunker([FromBody]FluidBunkerViewModel fluidBunker)
        {

            if (!_voyagesservice.CheckFormExists(fluidBunker.FormId) || string.IsNullOrEmpty(fluidBunker.FluidType) || string.IsNullOrEmpty(fluidBunker.Unit)
               || string.IsNullOrEmpty(fluidBunker.BunkerType) || fluidBunker.Consumption == 0)
            {
                Response.Headers.Add("Status", HttpStatusCode.BadRequest.ToString());
                Response.Headers.Add("Message", MessagesResource.InvalidDetails);
                return BadRequest();
            }
            if (_voyagesservice.IsVoyageApproved(fluidBunker.FormId))
            {
                Response.Headers.Add("Status", HttpStatusCode.BadRequest.ToString());
                Response.Headers.Add("Message", MessagesResource.PassageAlreadyApproved);
                return BadRequest();
            }

            switch (_voyagesservice.UpdateFluidBunker(fluidBunker))
            {
                case (int)ResponseStatus.NOTFOUND:
                    Response.Headers.Add("Status", HttpStatusCode.Conflict.ToString());
                    Response.Headers.Add("Message", MessagesResource.RecordNotFound);
                    return Conflict();
                case (int)ResponseStatus.ALREADYEXIST:
                    Response.Headers.Add("Status", HttpStatusCode.BadRequest.ToString());
                    Response.Headers.Add("Message", MessagesResource.DuplicateData);
                    return Conflict();
                default:
                    Response.Headers.Add("Status", HttpStatusCode.Created.ToString());
                    Response.Headers.Add("Message", MessagesResource.UpdatedSuccessfully);
                    return Ok(fluidBunker);
            }
        }
        [HttpDelete, Route("deletefluidbunker")]
        public IActionResult DeleteFluidBunker(int formId, int robId, string bunkerType)
        {
            if (!_voyagesservice.CheckFormExists(formId)
               || string.IsNullOrEmpty(bunkerType) || robId == 0)
            {
                Response.Headers.Add("Status", HttpStatusCode.BadRequest.ToString());
                Response.Headers.Add("Message", MessagesResource.InvalidDetails);
                return BadRequest();
            }
            if (_voyagesservice.IsVoyageApproved(formId))
            {
                Response.Headers.Add("Status", HttpStatusCode.BadRequest.ToString());
                Response.Headers.Add("Message", MessagesResource.PassageAlreadyApproved);
                return BadRequest();
            }
            int id = _voyagesservice.DeleteFluidBunker(formId, robId, bunkerType);
            if (id != 0)
            {
                Response.Headers.Add("Status", HttpStatusCode.Created.ToString());
                Response.Headers.Add("Message", MessagesResource.RecordDeleted);
                return Ok();
            }
            else
            {
                Response.Headers.Add("Status", HttpStatusCode.Conflict.ToString());
                Response.Headers.Add("Message", MessagesResource.RecordNotFound);
                return BadRequest();
            }
        }
        [HttpPost, Route("createeventfluidbunker")]
        public IActionResult CreateEventFluidBunker([FromBody]FluidBunkerViewModel fluidBunker)
        {
            if (fluidBunker.FormId == 0 || fluidBunker.EventROBsRowId == 0)
            {
                Response.Headers.Add("Status", HttpStatusCode.BadRequest.ToString());
                Response.Headers.Add("Message", MessagesResource.InvalidDetails);
                return BadRequest();
            }
            if (_voyagesservice.IsVoyageApproved(fluidBunker.FormId))
            {
                Response.Headers.Add("Status", HttpStatusCode.BadRequest.ToString());
                Response.Headers.Add("Message", MessagesResource.PassageAlreadyApproved);
                return BadRequest();
            }
            long id = _voyagesservice.CreateEventFluidBunker(fluidBunker);
            if (id != 0)
            {
                fluidBunker.RobId = id;
                Response.Headers.Add("Status", HttpStatusCode.Created.ToString());
                Response.Headers.Add("Message", MessagesResource.InsertedSuccessfully);
                return Ok(fluidBunker);
            }
            else
            {
                Response.Headers.Add("Status", HttpStatusCode.Conflict.ToString());
                Response.Headers.Add("Message", MessagesResource.DuplicateData);
                return BadRequest();
            }
        }
        [HttpGet, Route("getallbunkertype")]
        public IActionResult GetAllBunkerType()
        {
            Response.Headers.Add("Status", HttpStatusCode.OK.ToString());
            //IEnumerable<BunkerType> allBunkerType = RedisCacheHelper.Get<List<BunkerType>>("bunker");
            //if (allBunkerType == null)
            //{
            IEnumerable<BunkerType> allBunkerType = _voyagesservice.GetAllBunkerType();
            //    RedisCacheHelper.Set("bunker", allBunkerType);
            //}
            return Ok(allBunkerType);
        }

        #endregion

        #region Exclusion

        [HttpGet, Route("getpassagereportexclusion")]
        public IActionResult GetPassageReportExclusion(long voyageId)
        {

            //List<ExcludeReportLog> passageReportExclusion = RedisCacheHelper.Get<List<ExcludeReportLog>>("passageReportExclusion:" + voyageId);
            //if (passageReportExclusion == null)
            //{
            long loginUserId = Convert.ToInt64(RouteData.Values["UserId"]);
            var passageReportExclusion = _voyagesservice.GetPassageReportExclusion(voyageId, loginUserId);
            //    RedisCacheHelper.Set("passageReportExclusion:" + voyageId, passageReportExclusion);
            //}

            if(passageReportExclusion != null)
            {
                Response.Headers.Add("Status", HttpStatusCode.OK.ToString());
                return Ok(passageReportExclusion);
            }
            else
            {
                Response.Headers.Add("Status", HttpStatusCode.Conflict.ToString());
                Response.Headers.Add("Message", MessagesResource.InvalidDetails);
                return Conflict();
            }
            
        }

        [HttpGet, Route("getpositionreportexclusion")]
        public IActionResult GetPositionReportExclusion(long formId)
        {
            Response.Headers.Add("Status", HttpStatusCode.OK.ToString());
            //IEnumerable<ExcludeReportLog> positionReportExclusion = RedisCacheHelper.Get<List<ExcludeReportLog>>("positionReportExclusion:" + formId);
            //if (positionReportExclusion == null)
            //{
            IEnumerable<ExcludeReportLog> positionReportExclusion = _voyagesservice.GetPositionReportExclusion(formId);
            //    RedisCacheHelper.Set("positionReportExclusion:" + formId, positionReportExclusion);
            //}
            return Ok(positionReportExclusion);
        }

        [HttpGet, Route("getpassagereportexclusionlog")]
        public IActionResult GetPassageReportExclusionlog(long voyageId)
        {

            //IEnumerable<ExcludeReportLogs> passageReportExclusionLog = RedisCacheHelper.Get<List<ExcludeReportLogs>>("passageReportExclusionLog:" + voyageId);
            //if (passageReportExclusionLog == null)
            //{
            long loginUserId = Convert.ToInt64(RouteData.Values["UserId"]);
            var passageReportExclusionLog = _voyagesservice.GetPassageReportExclusionlog(voyageId, loginUserId);
            //    RedisCacheHelper.Set("passageReportExclusionLog:" + voyageId, passageReportExclusionLog);
            //}
            if(passageReportExclusionLog != null)
            {
                Response.Headers.Add("Status", HttpStatusCode.OK.ToString());
                return Ok(passageReportExclusionLog);
            }
            else
            {
                Response.Headers.Add("Status", HttpStatusCode.Conflict.ToString());
                Response.Headers.Add("Message", MessagesResource.InvalidDetails);
                return Conflict();
            }
           
        }

        [HttpGet, Route("getpositionreportexclusionlog")]
        public IActionResult GetPositionReportExclusionlog(long formId)
        {
            Response.Headers.Add("Status", HttpStatusCode.OK.ToString());
            //IEnumerable<ExcludeReportLogs> positionReportExclusionLog = RedisCacheHelper.Get<List<ExcludeReportLogs>>("positionReportExclusionLog:" + formId);
            //if (positionReportExclusionLog == null)
            //{
            IEnumerable<ExcludeReportLogs> positionReportExclusionLog = _voyagesservice.GetPositionReportExclusionlog(formId);
            //    RedisCacheHelper.Set("positionReportExclusionLog:" + formId, positionReportExclusionLog);
            //}
            return Ok(positionReportExclusionLog);
        }

        [HttpPost, Route("createpassagereportexclusion")]
        public IActionResult CreatePassageReportExclusion([FromBody] ExcludeReportLog exclude)
        {
            if ((exclude.VoyagesId == null && exclude.FormId == null) || (exclude.VoyagesId == 0 && exclude.FormId == 0)
               || (exclude.VoyagesId == 0 && exclude.FormId == null) || (exclude.VoyagesId == null && exclude.FormId == 0))
            {
                Response.Headers.Add("Status", HttpStatusCode.BadRequest.ToString());
                Response.Headers.Add("Message", MessagesResource.DatabaseInsertFailed);
                return BadRequest();
            }
            if (exclude.excludesList.Count < 2)
            {
                Response.Headers.Add("Status", HttpStatusCode.BadRequest.ToString());
                Response.Headers.Add("Message", MessagesResource.DatabaseInsertFailed);
                return BadRequest();
            }
            foreach (var item in exclude.excludesList)
            {
                if (item.SFPM_ExcludeReportId == null || item.SFPM_ExcludeReportId == 0)
                {
                    Response.Headers.Add("Status", HttpStatusCode.BadRequest.ToString());
                    Response.Headers.Add("Message", MessagesResource.DatabaseInsertFailed);
                    return BadRequest();
                }
            }
            int create = _voyagesservice.CreatePassageReportExclusion(exclude);


            if (create == (int)ResponseStatus.SAVED)
            {
                var data = _voyagesservice.GetPssgReptExclResponse(Convert.ToInt64(exclude.VoyagesId));
                Response.Headers.Add("Status", HttpStatusCode.Created.ToString());
                Response.Headers.Add("Message", MessagesResource.InsertedSuccessfully);
                return Ok(data);
            }
            else if (create == (int)ResponseStatus.ALREADYEXIST)
            {
                Response.Headers.Add("Status", HttpStatusCode.Conflict.ToString());
                Response.Headers.Add("Message", MessagesResource.DuplicateData);
                return Conflict();
            }
            else
            {
                Response.Headers.Add("Status", HttpStatusCode.Conflict.ToString());
                Response.Headers.Add("Message", MessagesResource.DatabaseInsertFailed);
                return Conflict();
            }

        }

        [HttpPost, Route("createpositionreportexclusion")]
        public IActionResult CreatePositionReportExclusion([FromBody] ExcludeReportLog exclude)
        {

            if ((exclude.VoyagesId == null && exclude.FormId == null) || (exclude.VoyagesId == 0 && exclude.FormId == 0)
               || (exclude.VoyagesId == 0 && exclude.FormId == null) || (exclude.VoyagesId == null && exclude.FormId == 0))
            {
                Response.Headers.Add("Status", HttpStatusCode.BadRequest.ToString());
                Response.Headers.Add("Message", MessagesResource.DatabaseInsertFailed);
                return BadRequest();
            }
            if (exclude.excludesList.Count < 2)
            {
                Response.Headers.Add("Status", HttpStatusCode.BadRequest.ToString());
                Response.Headers.Add("Message", MessagesResource.DatabaseInsertFailed);
                return BadRequest();
            }
            foreach (var item in exclude.excludesList)
            {
                if (item.SFPM_ExcludeReportId == null || item.SFPM_ExcludeReportId == 0)
                {
                    Response.Headers.Add("Status", HttpStatusCode.BadRequest.ToString());
                    Response.Headers.Add("Message", MessagesResource.DatabaseInsertFailed);
                    return BadRequest();
                }
            }
            int create = _voyagesservice.CreatePositionReportExclusion(exclude);
            if (create == (int)ResponseStatus.SAVED)
            {
                var data = _voyagesservice.GetPostReptExclResponse(Convert.ToInt64(exclude.FormId));
                Response.Headers.Add("Status", HttpStatusCode.Created.ToString());
                Response.Headers.Add("Message", MessagesResource.InsertedSuccessfully);
                return Ok(data);
            }
            else if (create == (int)ResponseStatus.ALREADYEXIST)
            {
                Response.Headers.Add("Status", HttpStatusCode.Conflict.ToString());
                Response.Headers.Add("Message", MessagesResource.DuplicateData);
                return Conflict();
            }
            else
            {
                Response.Headers.Add("Status", HttpStatusCode.Conflict.ToString());
                Response.Headers.Add("Message", MessagesResource.DatabaseInsertFailed);
                return Conflict();
            }

        }

        #endregion

        #region Others
        [HttpPut, Route("savePageSettings")]
        public IActionResult SavePageSettings(long userId, bool isPassageUTC, bool isPositionUTC)
        {
            int create = _voyagesservice.SavePageSettings(userId, isPassageUTC, isPositionUTC);
            if (create == (int)ResponseStatus.SAVED)
            {
                Response.Headers.Add("Status", HttpStatusCode.Created.ToString());
                Response.Headers.Add("Message", MessagesResource.UpdatedSuccessfully);
                return Ok(create);
            }
            else
            {
                Response.Headers.Add("Status", HttpStatusCode.Conflict.ToString());
                Response.Headers.Add("Message", MessagesResource.DatabaseInsertFailed);
                return Conflict();
            }
        }

        [HttpGet, Route("getPageSettings")]
        public IActionResult GetPageSettings(long userId)
        {
            var pagesetting = _voyagesservice.GetPageSettings(userId);
            if(pagesetting != null)
            {
                Response.Headers.Add("Status", HttpStatusCode.OK.ToString());
                return Ok(pagesetting);
            }
            else
            {
                Response.Headers.Add("Status", HttpStatusCode.Conflict.ToString());
                Response.Headers.Add("Message", MessagesResource.InvalidDetails);
                return Conflict();
            }
            
        }

        [HttpGet, Route("exportvoyage")]
        public async Task<IActionResult> ExportVoyage(string vessel, string voyagesId)
        {
            var lstVoyage = _voyagesservice.ExportGetVoyages(voyagesId);
            var memory = new MemoryStream();
            string date = System.DateTime.Now.ToString();
            string sFileName = @"VoyageReport_" + DateTime.Now.ToFileTime() + ".xlsx";
            ExcelPackage excel = new ExcelPackage();
            var workSheet = excel.Workbook.Worksheets.Add("Sheet1");
            workSheet.TabColor = System.Drawing.Color.Black;
            workSheet.DefaultRowHeight = 12;
            workSheet.Row(1).Height = 20;
            workSheet.Row(1).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            workSheet.Row(1).Style.Font.Bold = true;
            workSheet.Cells[1, 1].Value = vessel;
            workSheet.Cells[1, 1].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
            workSheet.Cells[1, 1].Style.Fill.BackgroundColor.SetColor(Color.Orange);

            workSheet.Row(2).Height = 20;
            workSheet.Row(2).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            workSheet.Row(2).Style.Font.Bold = true;
            workSheet.Cells[2, 1].Value = "VoyageNumber";
            workSheet.Cells[2, 1].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
            workSheet.Cells[2, 1].Style.Fill.BackgroundColor.SetColor(Color.Bisque);
            workSheet.Cells[2, 2].Value = "VesselCode";
            workSheet.Cells[2, 2].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
            workSheet.Cells[2, 2].Style.Fill.BackgroundColor.SetColor(Color.Bisque);
            workSheet.Cells[2, 3].Value = "Description";
            workSheet.Cells[2, 3].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
            workSheet.Cells[2, 3].Style.Fill.BackgroundColor.SetColor(Color.Bisque);
            workSheet.Cells[2, 4].Value = "LoadCondition";
            workSheet.Cells[2, 4].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
            workSheet.Cells[2, 4].Style.Fill.BackgroundColor.SetColor(Color.Bisque);
            workSheet.Cells[2, 5].Value = "DeparturePort";
            workSheet.Cells[2, 5].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
            workSheet.Cells[2, 5].Style.Fill.BackgroundColor.SetColor(Color.Bisque);
            workSheet.Cells[2, 6].Value = "DepartureTimezone";
            workSheet.Cells[2, 6].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
            workSheet.Cells[2, 6].Style.Fill.BackgroundColor.SetColor(Color.Bisque);
            workSheet.Cells[2, 7].Value = "ArrivalPort";
            workSheet.Cells[2, 7].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
            workSheet.Cells[2, 7].Style.Fill.BackgroundColor.SetColor(Color.Bisque);
            workSheet.Cells[2, 8].Value = "ArrivalTimezone";
            workSheet.Cells[2, 8].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
            workSheet.Cells[2, 8].Style.Fill.BackgroundColor.SetColor(Color.Bisque);
            workSheet.Cells[2, 9].Value = "ActualStartOfSeaPassage";
            workSheet.Cells[2, 9].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
            workSheet.Cells[2, 9].Style.Fill.BackgroundColor.SetColor(Color.Bisque);
            workSheet.Cells[2, 10].Value = "ActualEndOfSeaPassage";
            workSheet.Cells[2, 10].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
            workSheet.Cells[2, 10].Style.Fill.BackgroundColor.SetColor(Color.Bisque);
            workSheet.Cells[2, 11].Value = "CreatedDateTime";
            workSheet.Cells[2, 11].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
            workSheet.Cells[2, 11].Style.Fill.BackgroundColor.SetColor(Color.Bisque);
            workSheet.Cells[2, 12].Value = "ModifiedDateTime";
            workSheet.Cells[2, 12].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
            workSheet.Cells[2, 12].Style.Fill.BackgroundColor.SetColor(Color.Bisque);
            workSheet.Cells[2, 13].Value = "IMONumber";
            workSheet.Cells[2, 13].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
            workSheet.Cells[2, 13].Style.Fill.BackgroundColor.SetColor(Color.Bisque);

            int recordIndex = 3;
            foreach (var item in lstVoyage)
            {
                workSheet.Cells[recordIndex, 1].Value = item.VoyageNumber;
                workSheet.Cells[recordIndex, 2].Value = item.VesselCode;
                workSheet.Cells[recordIndex, 3].Value = item.Description;
                workSheet.Cells[recordIndex, 4].Value = item.LoadCondition;
                workSheet.Cells[recordIndex, 5].Value = item.DeparturePort;
                workSheet.Cells[recordIndex, 6].Value = item.DepartureTimezone;
                workSheet.Cells[recordIndex, 7].Value = item.ArrivalPort;
                workSheet.Cells[recordIndex, 8].Value = item.ArrivalTimezone;
                workSheet.Cells[recordIndex, 9].Value = item.ActualStartOfSeaPassage.ToString();
                workSheet.Cells[recordIndex, 10].Value = item.ActualEndOfSeaPassage.ToString();
                workSheet.Cells[recordIndex, 11].Value = item.CreatedDateTime;
                workSheet.Cells[recordIndex, 12].Value = item.ModifiedDateTime;
                workSheet.Cells[recordIndex, 13].Value = item.IMONumber;
                recordIndex++;
            }
            workSheet.Column(1).AutoFit();
            workSheet.Column(2).AutoFit();
            workSheet.Column(3).AutoFit();
            workSheet.Column(4).AutoFit();
            workSheet.Column(5).AutoFit();
            workSheet.Column(6).AutoFit();
            workSheet.Column(7).AutoFit();
            workSheet.Column(8).AutoFit();
            workSheet.Column(9).AutoFit();
            workSheet.Column(10).AutoFit();
            workSheet.Column(11).AutoFit();
            workSheet.Column(12).AutoFit();
            workSheet.Column(13).AutoFit();
            excel.SaveAs(memory);
            byte[] bt = memory.ToArray();
            memory.Close();


            Response.Headers.Add("Status", HttpStatusCode.Created.ToString());
            Response.Headers.Add("Message", MessagesResource.ReportCreatedSuccessfully);
            return File(bt, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
        }
        #endregion

        #region ExcelOperation
        //[HttpPost, Route("importVDDexcelreport")]
        //public IActionResult ImportExcelForDepartureReport(IFormFile files1, IFormFile files2, IFormFile files3, IFormFile files4, IFormFile files5)
        //{
        //    departureCount = 0;
        //    arrivalCount = 0;
        //    noonReportAtSeaCount = 0;
        //    noonReportAtPortCount = 0;
        //    List<IFormFile> files = new List<IFormFile>();
        //    if (files1 != null)
        //        files.Add(files1);
        //    if (files2 != null)
        //        files.Add(files2);
        //    if (files3 != null)
        //        files.Add(files3);
        //    if (files4 != null)
        //        files.Add(files4);
        //    if (files5 != null)
        //        files.Add(files5);
        //    int status = this._voyagesservice.ImportExcelForDepartureReport(files);
        //    if (status == (int)ResponseStatus.SAVED)
        //    {
        //        Response.Headers.Add("Status", HttpStatusCode.Created.ToString());
        //        Response.Headers.Add("Message", MessagesResource.InsertedSuccessfully);
        //        return Ok(new
        //        {
        //            VDDDeparture = departureCount,
        //            VDDArrival = arrivalCount,
        //            VDDNoonReportAtSea = noonReportAtSeaCount,
        //            VDDNoonreportatPort = noonReportAtPortCount,
        //            DatabaseDeparture = databaseDepartureCount,
        //            DatabaseArrival = databaseArrivalCount,
        //            DatabaseNoonAtSea = DatabaseNoonReportAtSeaCount,
        //            DatabaseNoonAtPort = DatabaseNoonReportAtPortCount
        //        });
        //    }
        //    else
        //    {
        //        Response.Headers.Add("Status", HttpStatusCode.Conflict.ToString());
        //        Response.Headers.Add("Message", MessagesResource.TechnicalError);
        //        return Conflict();
        //    }

        //}

        [HttpGet, Route("importvessellistexcel")]
        public IActionResult ImportVesselListExcel()
        {
            int status = this._voyagesservice.ImportVesselListExcel();
            if (status == (int)ResponseStatus.SAVED)
            {
                Response.Headers.Add("Status", HttpStatusCode.Created.ToString());
                Response.Headers.Add("Message", MessagesResource.InsertedSuccessfully);
                return Ok();
            }
            else
            {
                Response.Headers.Add("Status", HttpStatusCode.Conflict.ToString());
                Response.Headers.Add("Message", MessagesResource.TechnicalError);
                return Conflict();
            }

        }


        #endregion

        #region Other
        [HttpGet, Route("getimage")]
        public IActionResult GetImage([FromQuery]string BBox)
        {
            WebClient client = new WebClient();
            string imageUrl = AzureVaultKey.GetVaultValue("BlockStorageImageUrl");
            switch (HttpUtility.UrlDecode(BBox))
            {

                case "0,0,1252344.2714243277,1252344.2714243277":
                    return File(client.OpenRead(imageUrl + "1.png"), "image/jpeg");
                case "1252344.271424327,0,2504688.542848655,1252344.2714243277":
                    return File(client.OpenRead(imageUrl + "2.png"), "image/jpeg");
                case "-1252344.271424327,0,6.984919309616089e-10,1252344.2714243277":
                    return File(client.OpenRead(imageUrl + "3.png"), "image/jpeg");
                case "0,-1252344.271424327,1252344.2714243277,6.984919309616089e-10":
                    return File(client.OpenRead(imageUrl + "4.png"), "image/jpeg");
                case "1252344.271424327,-1252344.271424327,2504688.542848655,6.984919309616089e-10":
                    return File(client.OpenRead(imageUrl + "5.png"), "image/jpeg");
                case "-1252344.271424327,-1252344.271424327,6.984919309616089e-10,6.984919309616089e-10":
                    return File(client.OpenRead(imageUrl + "6.png"), "image/jpeg");
                case "-2504688.542848654,0,-1252344.2714243263,1252344.2714243277":
                    return File(client.OpenRead(imageUrl + "7.png"), "image/jpeg");
                case "-2504688.542848654,-1252344.271424327,-1252344.2714243263,6.984919309616089e-10":
                    return File(client.OpenRead(imageUrl + "8.png"), "image/jpeg");
                default:
                    return Json("NotFound");
            }
        }

        [HttpPost, Route("getpassagedataforchart")]
        public IActionResult GetPassageDataForChart(PassageDataChartViewModel passageDataCharts)
        {
            long loginUserId = Convert.ToInt64(RouteData.Values["UserId"]);
            var passageDataChanrt = _voyagesservice.GetPassageDataForChart(passageDataCharts, loginUserId);
            if (passageDataChanrt.PassageData.Count() > 0)
                return Ok(passageDataChanrt);
            else
            {
                Response.Headers.Add("Status", HttpStatusCode.Conflict.ToString());
                Response.Headers.Add("Message", MessagesResource.InvalidDetails);
                return Conflict();
            }
        }

        [HttpGet, Route("getecadata")]
        public IActionResult GetECAData([FromQuery]string type)
        {
            return Ok(_voyagesservice.GetECAData(type));
        }


        [HttpGet, Route("getmarineweatherimage")]
        public IActionResult GetMarineWeatherImage(string SERVICE, string VERSION, string REQUEST, string FORMAT, string TRANSPARENT,
            string map, string CURRENT_DATE, string CURRENT_HOUR, string TILES, string LAYERS, string TIME,
            string FORECAST_DATE, string FORECAST_HOUR, string WIDTH, string HEIGHT, string CRS, string STYLES,
            string BBOX)
        {
            return File(_voyagesservice.GetMarineWeatherImage(SERVICE, VERSION, REQUEST, FORMAT, TRANSPARENT,
             map, CURRENT_DATE, CURRENT_HOUR, TILES, LAYERS, TIME,
             FORECAST_DATE, FORECAST_HOUR, WIDTH, HEIGHT, CRS, STYLES,
             BBOX).RawBytes, "image/png");
        }

        [HttpDelete, Route("deletemeteostratumdata")]
        public IActionResult DeleteMeteoStratumData(string imoNumber, DateTime gpsDatetime)
        {
            int id = _voyagesservice.DeleteMeteoStratumData(imoNumber, gpsDatetime);
            if (id != 0)
            {
                Response.Headers.Add("Status", HttpStatusCode.Created.ToString());
                Response.Headers.Add("Message", MessagesResource.RecordDeleted);
                return Ok();
            }
            else
            {
                Response.Headers.Add("Status", HttpStatusCode.NotFound.ToString());
                Response.Headers.Add("Message", MessagesResource.RecordNotFound);
                return NotFound();
            }
        }


        [HttpDelete, Route("deletevoyagerediscache")]
        public IActionResult DeleteVoyageRedisCache([FromQuery] string IMONumbers, long userId)
        {

            var IMONumberlist = IMONumbers.Split(',').ToList();
            foreach (var IMONumber in IMONumberlist)
            {

                //bool keyExists = RedisCacheHelper.IsKeyExists("voyages:" + IMONumber);
                //if (keyExists)
                //{
                //    RedisCacheHelper.DeleteKey("voyages:" + IMONumber);
                //}

                var allVoyageList = _voyagesservice.GetAllVoyagesByVessel(IMONumber.ToString(), userId);
                //foreach (var voyage in allVoyageList)
                //{
                //    var newDate = voyage.ActualStartOfSeaPassage.Year + "/" + voyage.ActualStartOfSeaPassage.Month + "/" + voyage.ActualStartOfSeaPassage.Day + " " + voyage.ActualStartOfSeaPassage.Hour + ":" + voyage.ActualStartOfSeaPassage.Minute + ":" + voyage.ActualStartOfSeaPassage.Second;
                //    //bool keyExists1 = RedisCacheHelper.IsKeyExists("noonreports:" + voyage.IMONumber + "|" + voyage.VoyageNumber + "|" + newDate);
                //    //if (keyExists1)
                //    //{
                //    //    RedisCacheHelper.DeleteKey("noonreports:" + voyage.IMONumber + "|" + voyage.VoyageNumber + "|" + newDate);
                //    //}
                //}
            }
            Response.Headers.Add("Status", HttpStatusCode.OK.ToString());
            Response.Headers.Add("Message", "vessel voyage List Cache Deleted");
            return Ok();

        }

        [HttpGet, Route("getdirection")]
        public IActionResult GetDirection()
        {
            return Ok(_voyagesservice.GetDirectionList());
        }

        [HttpGet, Route("analyzedweathercalculation")]
        public IActionResult AnalyzedWeatherCal(string formIds)
        {
            var formIdList = formIds.Split(",");
            var result = "";
            foreach (var formId in formIdList)
            {
                result = _voyagesservice.AnalyzedWeatherCal(Convert.ToInt64(formId));
            }
            Response.Headers.Add("Status", HttpStatusCode.OK.ToString());
            Response.Headers.Add("Message", "analyzed weather calculated ");
            return Ok(result);
        }

        
        #endregion
    }
}