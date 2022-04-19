using Data.Solution.Models;
using Data.Solution.Models.ViewModels;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Data.SqlClient;
using System.Linq;
using System.IO;
using OfficeOpenXml;
using VoyagesAPIService.Helper;
using VoyagesAPIService.Utility;
using Newtonsoft.Json;
using System.Net.Http;
using System.Net.Http.Headers;
using Newtonsoft.Json.Linq;
using System.Net;
using Serilog;
using System.Xml;
using Microsoft.Extensions.Configuration;
using VoyagesAPIService.Infrastructure.Helper;
using Microsoft.AspNetCore.Http;
using System.Text.RegularExpressions;
using VoyagesAPIService.Controllers;
using GeoCoordinatePortable;
using System.Net.Mail;
namespace VoyagesAPIService.Infrastructure.Repositories
{
    public class EmsRepository
    {
        private DatabaseContext _DbContext;
        private readonly UserContext _currentUser;
       
        public EmsRepository(DatabaseContext databaseContext, UserContext currentUser)
        {
            _DbContext = databaseContext;
            this._currentUser = currentUser;

        }

        public IEnumerable<Voyages> GetAllVoyagesByVesselAndYear(string imoNumber, long userId, string year)
        {
            var userrole = _DbContext.Roles.Where(x => x.RoleId == (_DbContext.UserRoleMapping.Where(y => y.UserId == Convert.ToInt64(this._currentUser.UserId)).Select(y => y.RoleId).FirstOrDefault())).Select(x => x.RoleName).AsNoTracking().FirstOrDefault();


            long loginuserId = Convert.ToInt64(this._currentUser.UserId);
            var imoNumbers = (from u in _DbContext.UserVesselGroupMappings.AsEnumerable()
                              join vu in _DbContext.VesselGroupVesselMapping.AsEnumerable() on u.VesselGroupId equals vu.VesselGroupId
                              join vsl in _DbContext.Vessels.AsEnumerable() on vu.VesselId equals vsl.SFPM_VesselId
                              where u.UserId == Convert.ToInt64(this._currentUser.UserId)
                              select vsl.IMONumber).ToList();

            if (loginuserId.Equals(userId) && (imoNumbers.Contains(imoNumber) || userrole.ToLower() == Constant.administrator))
                return _DbContext.Voyages.FromSql($"select * from Voyages where IMONumber='" + imoNumber + "' and YEAR(ActualEndOfSeaPassage)='" + year + "'").ToList();
            else
                return null;
        }

        public IEnumerable<Forms> GetAllVesselsByYear()
        {
            return _DbContext.Forms.FromSql($"select * from Forms where year(ReportDateTime)=2022 ").ToList();
        }


    }
}
