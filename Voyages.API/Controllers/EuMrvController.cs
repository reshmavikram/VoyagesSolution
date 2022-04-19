﻿using Microsoft.AspNetCore.Mvc;
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

    [Route("api/[controller]")]

    public class EuMrvController : Controller
    {
      private readonly IEmsService _emsservice;
      
        private readonly IHostingEnvironment _hostingEnvironment;
        public IConfiguration Configuration { get; }
        public EuMrvController(IEmsService emsService, IHostingEnvironment hostingEnvironment,IConfiguration configuration)
        {
            _emsservice = emsService;
          
            _hostingEnvironment=hostingEnvironment; 
            Configuration = configuration;
   
        }
   /*   [HttpGet, Route("getAllVesselByReportingYear")]
        public IActionResult getAllVesselByReportingYear()
        {
             IEnumerable<Forms> allVesselList= _emsservice.GetAllVesselsByYear();
             List<EUMRDataViewModel> vesselViewModelList = new List<EUMRDataViewModel>();
             if (allVesselList != null)
             {
                foreach (Forms vesselObj in allVesselList)
                {
                    EUMRDataViewModel objViewModel = new EUMRDataViewModel();
                    objViewModel.VesselName = vesselObj.VesselName;
                    //objViewModel.IMONumber = vesselObj.ImoNumber;
                   // objViewModel.ArrivalPort
                   
                    vesselViewModelList.Add(objViewModel);
                }
             }

             return Ok(vesselViewModelList);   
           

        }
       */

        [HttpGet,Route("getAllVoyagesByVessel")]
        public IActionResult getAllVoyagesByVessel(string imoNumber, long userId, string year)
        {
            IEnumerable<Voyages> allVoyageList = _emsservice.GetAllVoyagesByVesselAndYear(imoNumber, userId, year );
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
