using Microsoft.AspNetCore.Mvc;
using VoyagesAPIService.Infrastructure.Services.Interfaces;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Data.Solution.Models;
using Data.Solution.Models.ViewModels;
using Data.Solution.Resources;


using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.IO;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System.Drawing;
using System.Threading.Tasks;
using VoyagesAPIService.Helper;
using Microsoft.AspNetCore.Authorization;
using VoyagesAPIService.Filter;
using System.Web;
using VoyagesAPIService.Infrastructure.Helper;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;

namespace VoyagesAPIService.Controllers
{
    // add auth here at last



    public class EuMrvController : Controller
    {
        //private readonly IEmsService _emsservice;
        private readonly IVoyagesService _voyagesservice;   
        private readonly IHostingEnvironment _hostingEnvironment;
        public IConfiguration Configuration { get; }
        public EuMrvController(IVoyagesService voyagesService,IHostingEnvironment hostingEnvironment,IConfiguration configuration)
        {
            //_emsservice = emsService;
            _voyagesservice = voyagesService;   
            _hostingEnvironment=hostingEnvironment; 
            Configuration = configuration;
   
        }
        [HttpGet, Route("getAllVesselByReportingYear")]
        public IActionResult getAllVesselByReportingYear()
        {
             IEnumerable<Vessel> allVesselList=_voyagesservice.GetAllVesselsByYear();
             List<EUMRDataViewModel> vesselViewModelList = new List<EUMRDataViewModel>();
             if (allVesselList != null)
             {
                foreach (Vessel vesselObj in allVesselList)
                {
                    EUMRDataViewModel objViewModel = new EUMRDataViewModel();
                    objViewModel.VesselName = vesselObj.VesselName;
                    objViewModel.IMONumber = vesselObj.IMONumber;
                    vesselViewModelList.Add(objViewModel);
                }
             }

             return Ok(vesselViewModelList);   
           

        }

        [HttpGet,Route("getAllVoyagesByVessel")]
        public IActionResult getAllVoyagesByVessel(string imoNumber, long userId)
        {
            IEnumerable<Voyages> allVoyageList = _voyagesservice.GetAllVoyagesByVessel(imoNumber, userId);
            List<VoyagesViewModel> voyagesViewModelList = new List<VoyagesViewModel>();
            if (allVoyageList != null)
            {
                foreach (Voyages voyageObj in allVoyageList)
                {
                    //View model mapping
                    VoyagesViewModel objViewModel = new VoyagesViewModel();

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
                    foreach (var warnig in _voyagesservice.GetPassageWarning(voyageObj.SFPM_VoyagesId, userId))
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
                    var reportExclusion = _voyagesservice.GetPassageReportExclusionlog(voyageObj.SFPM_VoyagesId, userId);
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

       

        }
}
