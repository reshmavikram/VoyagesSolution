using Data.Solution.Models;
using Data.Solution.Models.ViewModels;
using Data.Solution.Resources;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Net;
using VoyagesAPIService.Infrastructure.Services.Interfaces;

namespace VoyagesAPIService.Controllers
{
    [Route("api/[controller]")]


    public class ImoDcsController : Controller
    {
        private readonly IEmsService _emsservice;

        private readonly IHostingEnvironment _hostingEnvironment;
        public IConfiguration Configuration { get; }
        public ImoDcsController(IEmsService emsService, IHostingEnvironment hostingEnvironment, IConfiguration configuration)
        {
            _emsservice = emsService;

            _hostingEnvironment = hostingEnvironment;
            Configuration = configuration;

        }
        [HttpGet, Route("getAllIMOVesselByReportingYear")]
        public IActionResult getAllVesselByReportingYear()
        {
            IEnumerable<Forms> allVesselList = _emsservice.GetAllVesselsByYear();
            List<ImoDcsDataViewModel> vesselViewModelList = new List<ImoDcsDataViewModel>();
            if (allVesselList != null)
            {
                foreach (Forms vesselObj in allVesselList)
                {
                    ImoDcsDataViewModel objViewModel = new ImoDcsDataViewModel();
                    objViewModel.Description = vesselObj.VesselName;
                    objViewModel.ImoNumber = (int)vesselObj.ImoNumber;

                    // objViewModel.ArrivalPort

                    vesselViewModelList.Add(objViewModel);
                }
            }

            return Ok(vesselViewModelList);


        }


        [HttpGet, Route("getAllIMOVoyagesByVessel")]
        public IActionResult getAllVoyagesByVessel(string imoNumber, long userId, string year)
        {
            IEnumerable<Voyages> allVoyageList = _emsservice.GetAllVoyagesByVesselAndYear(imoNumber, userId, year);
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

