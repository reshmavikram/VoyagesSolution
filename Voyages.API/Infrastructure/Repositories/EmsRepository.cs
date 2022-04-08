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
        public IEnumerable<Voyages> GetAllVoyagesByVessel(string imoNumber, long userId)
        {
            var userrole = _DbContext.Roles.Where(x => x.RoleId == (_DbContext.UserRoleMapping.Where(y => y.UserId == Convert.ToInt64(this._currentUser.UserId)).Select(y => y.RoleId).FirstOrDefault())).Select(x => x.RoleName).AsNoTracking().FirstOrDefault();
            long loginuserId = Convert.ToInt64(this._currentUser.UserId);
            var imoNumbers = (from u in _DbContext.UserVesselGroupMappings.AsEnumerable()
                              join vu in _DbContext.VesselGroupVesselMapping.AsEnumerable() on u.VesselGroupId equals vu.VesselGroupId
                              join vsl in _DbContext.Vessels.AsEnumerable() on vu.VesselId equals vsl.SFPM_VesselId
                              where u.UserId == Convert.ToInt64(this._currentUser.UserId)
                              select vsl.IMONumber).ToList();

            if (loginuserId.Equals(userId) && (imoNumbers.Contains(imoNumber) || userrole.ToLower() == Constant.administrator))
                return _DbContext.Voyages.Where(x => x.IMONumber == imoNumber).OrderByDescending(x => x.ActualStartOfSeaPassage).ToList();
            else
                return null;
        }
        public PassagesApprovalAudits GetLatestPassagesApprovalAudit(long voyagesId)
        {
            return _DbContext.PassagesApprovalAudits.Where(x => x.VoyagesId == voyagesId).OrderByDescending(x => x.ApprovalDateTime).FirstOrDefault();
        }
        public PageSettings GetPageSettings(long userId)
        {
            long loginUserId = Convert.ToInt64(this._currentUser.UserId);

            if (loginUserId.Equals(userId))
            {
                var pageSettings = _DbContext.PageSettings.Where(setting => setting.UserId == userId).FirstOrDefault();

                if (pageSettings == null)
                {
                    pageSettings = new PageSettings();
                    pageSettings.IsPassageUTC = true;
                    pageSettings.IsPositionUTC = true;
                    pageSettings.UserId = userId;
                }

                return pageSettings;
            }
            else
                return null;

        }
        public IEnumerable<ExcludeReportLogs> GetPassageReportExclusionlog(long voyageId, long loginUserId)
        {

            var userrole = _DbContext.Roles.Where(x => x.RoleId == (_DbContext.UserRoleMapping.Where(y => y.UserId == loginUserId).Select(y => y.RoleId).FirstOrDefault())).Select(x => x.RoleName).AsNoTracking().FirstOrDefault();
            var imonumberfromformid = _DbContext.Voyages.Where(v => v.SFPM_VoyagesId == voyageId).Select(v => v.IMONumber.ToString()).FirstOrDefault();
            var imoNumbers = (
                              from u in _DbContext.UserVesselGroupMappings
                              join vu in _DbContext.VesselGroupVesselMapping on u.VesselGroupId equals vu.VesselGroupId
                              join vsl in _DbContext.Vessels on vu.VesselId equals vsl.SFPM_VesselId
                              where u.UserId == loginUserId
                              select vsl.IMONumber).Distinct();


            if (imoNumbers.Contains(imonumberfromformid) || userrole.ToLower() == Constant.administrator)
            {
                var data = (from l in _DbContext.ExcludeReportLogs
                            join r in _DbContext.ExcludeReports on l.ReportId equals r.SFPM_ExcludeReportId
                            join u in _DbContext.Users on l.UserId equals u.UserId
                            where l.VoyagesId == voyageId
                            select new ExcludeReportLogs { ExcludeReportLogId = l.SFPM_ExcludeReportLogId, Excluded = l.Excluded, ReportName = r.ReportName, Username = u.Username, CreatedDateTime = l.CreatedDateTime, Remarks = l.Remarks }).ToList();

                IEnumerable<ExcludeReportLogs> log = data as IEnumerable<ExcludeReportLogs>;
                return log;
            }
            else
                return null;

        }
        public IEnumerable<PassageWarning> GetPassageWarning(long voyageId, long loginUserId)
        {
            var userrole = _DbContext.Roles.Where(x => x.RoleId == (_DbContext.UserRoleMapping.Where(y => y.UserId == loginUserId).Select(y => y.RoleId).FirstOrDefault())).Select(x => x.RoleName).AsNoTracking().FirstOrDefault();
            var imonumberfromformid = _DbContext.Voyages.Where(v => v.SFPM_VoyagesId == voyageId).Select(v => v.IMONumber.ToString()).FirstOrDefault();
            var imoNumbers = (
                              from u in _DbContext.UserVesselGroupMappings
                              join vu in _DbContext.VesselGroupVesselMapping on u.VesselGroupId equals vu.VesselGroupId
                              join vsl in _DbContext.Vessels on vu.VesselId equals vsl.SFPM_VesselId
                              where u.UserId == loginUserId
                              select vsl.IMONumber).Distinct();


            if (imoNumbers.Contains(imonumberfromformid) || userrole.ToLower() == Constant.administrator)
                return _DbContext.PassageWarnings.Where(x => x.VoyageId == voyageId).ToList();
            else
                return null;
        }
        public IEnumerable<PassageWarningAudit> GetPassageWarningAudit(long passageWarningId)
        {
            return (from passageWarningAudits in _DbContext.PassageWarningAudits
                    join users in _DbContext.Users on passageWarningAudits.ReviewedBy equals users.UserId
                    where passageWarningAudits.PassageWarningId == passageWarningId
                    select new PassageWarningAudit
                    {
                        SFPM_PassageWarningAuditId = passageWarningAudits.SFPM_PassageWarningAuditId,
                        PassageWarningId = passageWarningAudits.PassageWarningId,
                        IsApproved = passageWarningAudits.IsApproved,
                        ReviewedBy = passageWarningAudits.ReviewedBy,
                        ReviewedName = users.Username,
                        ReviewDateTime = passageWarningAudits.ReviewDateTime
                    }).ToList();

        }
    }
}
