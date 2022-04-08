using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Data.Solution.Models;
using VoyagesAPIService.Infrastructure.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using VoyagesAPIService.Helper;
using Data.Solution.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using VoyagesAPIService.Filter;
using VoyagesAPIService.Utility;
using System.Net;
using Data.Solution.Resources;

namespace VoyagesAPIService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [ServiceFilter(typeof(AuthServer))]
    public class FleetViewController : Controller
    {
        private readonly IFleetViewService _fleetViewService;
       
        public FleetViewController(IFleetViewService IFleetViewManagementService)
        {
            _fleetViewService = IFleetViewManagementService;
           
        }
        [HttpGet, Route("getfleetview")]
        public IActionResult GetFleetView(long vesselGroupId)
        {
            long loginUserId = Convert.ToInt64(RouteData.Values["UserId"]);
            var vesselGroupData = _fleetViewService.GetFleetView(vesselGroupId, loginUserId);
            if (vesselGroupData != null)
                return Ok(vesselGroupData.OrderBy(x => x.VesselName).ToList());
            else
            {
                Response.Headers.Add("Status", HttpStatusCode.Conflict.ToString());
                Response.Headers.Add("Message", MessagesResource.InvalidDetails);
                return Conflict();
            }
        }
        [HttpGet, Route("getpassagedatalatestreport")]
        public IActionResult GetPassageDataLatestReport(long vesselId)
        {
            long loginUserId = Convert.ToInt64(RouteData.Values["UserId"]);
            var passageReportData = _fleetViewService.GetPassageDataLatestReport(vesselId, loginUserId);
            if (passageReportData != null)
                return Ok(passageReportData);
            else
            {
                Response.Headers.Add("Status", HttpStatusCode.Conflict.ToString());
                Response.Headers.Add("Message", MessagesResource.InvalidDetails);
                return Conflict();
            }
        }
        [HttpGet, Route("getpassagedata")]
        public IActionResult GetPassageData(long vesselId)
        {
            //List<FleetPassageDataViewModel> getPassageData = RedisCacheHelper.Get<List<FleetPassageDataViewModel>>("passageData:" + vesselId);
            //if (getPassageData == null)
            //{
            //    getPassageData = _fleetViewService.GetPassageData(vesselId);
            //    RedisCacheHelper.Set("passageData:" + vesselId, getPassageData);
            //}
            long loginUserId = Convert.ToInt64(RouteData.Values["UserId"]);
            List<FleetPassageDataViewModel> getPassageData = _fleetViewService.GetPassageData(vesselId, loginUserId);
            if (getPassageData != null)
                return Ok(getPassageData);
            else
            {
                Response.Headers.Add("Status", HttpStatusCode.Conflict.ToString());
                Response.Headers.Add("Message", MessagesResource.InvalidDetails);
                return Conflict();
            }
        }

        //[HttpGet, Route("getlatlongs")]
        //public IActionResult GetLatLongs(string latslongs)
        //{
        //    List<string> latslongslist = latslongs.Split(',').ToList();
        //    string list = "";
        //    foreach (string latlong in latslongslist)
        //    {
        //        string[] latlongs = latlong.Split('=');
        //        var lat = ParseCoordinate(latlongs[0].ToString());
        //        var longs = ParseCoordinate(latlongs[1].ToString());
        //        list = list + " = " + lat + "," + longs;
        //    }
                
        //    return Ok(list);
        //}

        //private double ParseCoordinate(string point)
        //{
        //    var pointArray = point.TrimStart().Split('°'); //split the string.
        //    var degrees = Double.Parse(pointArray[0]);
        //    var min = pointArray[1].ToString().TrimStart().Split("'");
        //    var minutes = Double.Parse(min[0]) / 60;
        //    var sec = min[1].ToString().TrimStart().Split("\"");
        //    var seconds = Double.Parse(sec[0]) / 3600;
        //    double res = (degrees + minutes + seconds);// * multiplier;
        //    if (sec[1].Trim().ToString() != null && (sec[1].Trim().ToString() == "S" || sec[1].Trim().ToString() == "W"))
        //    {
        //        res = res * -1;
        //    }
        //    return res;// * multiplier;
        //}
        //[HttpDelete, Route("deletefleetrediscache")]
        //public IActionResult DeleteFleetRedisCache(long vesselId)
        //{
        //    bool keyExists = RedisCacheHelper.IsKeyExists("passageData:" + vesselId);
        //    if (keyExists)
        //    {
        //        RedisCacheHelper.DeleteKey("passageData:" + vesselId);
        //        Response.Headers.Add("Status", HttpStatusCode.OK.ToString());
        //        Response.Headers.Add("Message", "passage List Cache Deleted");
        //        return Ok();
        //    }
        //    else
        //    {
        //        Response.Headers.Add("Status", HttpStatusCode.NotFound.ToString());
        //        Response.Headers.Add("Message", "passage List Cache not found");
        //        return NotFound();
        //    }

        //}
    }


}