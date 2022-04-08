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
    public class VoyagesRepository
    {
        private DatabaseContext _DbContext;
        private bool _disposed;
        private readonly UserContext _currentUser;
        private static string MarineWeatherImageToken = "";
        static string logintickit = "";
        public VoyagesRepository(DatabaseContext databaseContext, UserContext currentUser)
        {
            _DbContext = databaseContext;
            this._currentUser = currentUser;

           
        }

        #region Methods

        #region Passages
        public Voyages GetVoyage(long voyagesId,long loginUserId)
        {
            var userrole = _DbContext.Roles.Where(x => x.RoleId == (_DbContext.UserRoleMapping.Where(y => y.UserId == loginUserId).Select(y => y.RoleId).FirstOrDefault())).Select(x => x.RoleName).AsNoTracking().FirstOrDefault();
            var imonumberfromformid = _DbContext.Voyages.Where(v => v.SFPM_VoyagesId == voyagesId).Select(v => v.IMONumber.ToString()).FirstOrDefault();
            var imoNumbers = (
                              from u in _DbContext.UserVesselGroupMappings
                              join vu in _DbContext.VesselGroupVesselMapping on u.VesselGroupId equals vu.VesselGroupId
                              join vsl in _DbContext.Vessels on vu.VesselId equals vsl.SFPM_VesselId
                              where u.UserId == loginUserId
                              select vsl.IMONumber).Distinct();


            if (imoNumbers.Contains(imonumberfromformid) || userrole.ToLower() == Constant.administrator)
                return _DbContext.Voyages.Where(x => x.SFPM_VoyagesId == voyagesId).FirstOrDefault();
            else
                return null;
        }
        public IEnumerable<Vessel> GetAllVesselsByYear()
        {
            return _DbContext.Vessels.ToList();
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

        public int CreateVoyages(Voyages voyages)
        {
            long userId = Convert.ToInt64(this._currentUser.UserId);
            voyages.DepartureTimezone = SetTimezoneFormate(voyages.DepartureTimezone);
            voyages.ArrivalTimezone = SetTimezoneFormate(voyages.ArrivalTimezone);
            voyages.CreatedBy = userId;
            voyages.CreatedDateTime = DateTime.UtcNow;
            Vessel vessel = _DbContext.Vessels.Where(x => x.IMONumber == voyages.IMONumber).AsNoTracking().FirstOrDefault();
            if (vessel != null)
            {
                voyages.VesselCode = vessel.VesselCode;
            }
            _DbContext.Voyages.Add(voyages);
            Save();
            CreatePassageWarning(voyages);
            return (int)ResponseStatus.SAVED;
        }

        public int UpdateVoyages(Voyages voyages,long loginUserId)
        {
            long userId = loginUserId;//Convert.ToInt64(this._currentUser.UserId);
            voyages.DepartureTimezone = SetTimezoneFormate(voyages.DepartureTimezone);
            voyages.ArrivalTimezone = SetTimezoneFormate(voyages.ArrivalTimezone);
            var chkVoyages = _DbContext.Voyages.Where(x => x.SFPM_VoyagesId == voyages.SFPM_VoyagesId).AsNoTracking().SingleOrDefault();
            if (chkVoyages != null)
            {
                voyages.IMONumber = chkVoyages.IMONumber;
                voyages.ModifiedBy = userId;
                voyages.ModifiedDateTime = DateTime.UtcNow;
                voyages.CreatedDateTime = chkVoyages.CreatedDateTime;
                voyages.CreatedBy = chkVoyages.CreatedBy;
                _DbContext.Voyages.Update(voyages);
                Save();
                CreatePassageWarning(voyages);
                return (int)ResponseStatus.SAVED;
            }
            return (int)ResponseStatus.NOTFOUND;
        }
        public int DeleteVoyage(string voyageIds,long loginUserId)
        {
            long userId = loginUserId;//Convert.ToInt64(this._currentUser.UserId);
            List<string> lstVoyageIds = voyageIds.Split(',').ToList();
            foreach (string voyageId in lstVoyageIds)
            {
                PassagesApprovalAudits auditData = _DbContext.PassagesApprovalAudits.Where(x => x.VoyagesId == Convert.ToInt64(voyageId)).OrderByDescending(x => x.ApprovalDateTime).FirstOrDefault();
                if (auditData != null)
                {
                    if (auditData.IsInitialApproved || auditData.IsFinalApproved)
                        return (int)ResponseStatus.ALREADYAPPROVED;
                }
                var isVoyages = _DbContext.Voyages.Where(x => x.SFPM_VoyagesId == Convert.ToInt64(voyageId)).FirstOrDefault();
                if (isVoyages != null)
                {
                    _DbContext.ExcludeReportLogs.RemoveRange(_DbContext.ExcludeReportLogs.Where(x => x.VoyagesId == isVoyages.SFPM_VoyagesId));
                    var ispassagewarnings = _DbContext.PassageWarnings.Where(x => x.VoyageId == isVoyages.SFPM_VoyagesId).ToList();
                    foreach (var passagewarning in ispassagewarnings)
                    {
                        _DbContext.PassageWarningAudits.RemoveRange(_DbContext.PassageWarningAudits.Where(x => x.PassageWarningId == passagewarning.SFPM_PassageWarningId));
                    }
                    _DbContext.PassageWarnings.RemoveRange(ispassagewarnings);
                    var formAllList = _DbContext.Forms.Where(x => x.ImoNumber.ToString() == isVoyages.IMONumber)
                    .OrderBy(a => a.ReportDateTime).ToList();
                    var formList = formAllList.Where(x => x.ReportDateTime.Value.DateTime >= isVoyages.ActualStartOfSeaPassage && x.ReportDateTime.Value.DateTime <= isVoyages.ActualEndOfSeaPassage)
                    .OrderBy(a => a.ReportDateTime).ToList();
                    formList.ForEach(x => x.HasNoParent = true);
                    formList.ForEach(x => x.ModifiedBy = userId);
                    formList.ForEach(x => x.ModifiedDateTime = DateTime.UtcNow);
                    _DbContext.Forms.UpdateRange(formList);
                    _DbContext.Voyages.RemoveRange(isVoyages);
                }
                else
                {
                    return (int)ResponseStatus.NOTFOUND;
                }
            }
            Save();
            return (int)ResponseStatus.SAVED;
        }


        public long GetVesselId(string imoNumber)
        {
            return _DbContext.Vessels.Where(x => x.IMONumber == imoNumber).Select(x => x.SFPM_VesselId).FirstOrDefault();
        }

        public PassagesApprovalAudits GetLatestPassagesApprovalAudit(long voyagesId)
        {
            return _DbContext.PassagesApprovalAudits.Where(x => x.VoyagesId == voyagesId).OrderByDescending(x => x.ApprovalDateTime).FirstOrDefault();
        }

        public IEnumerable<ApprovalAuditsViewModel> GetPassagesApprovalAuditList(long voyagesId,long loginUserId)
        {
            var userrole = _DbContext.Roles.Where(x => x.RoleId == (_DbContext.UserRoleMapping.Where(y => y.UserId == loginUserId).Select(y => y.RoleId).FirstOrDefault())).Select(x => x.RoleName).AsNoTracking().FirstOrDefault();
            var imonumberfromformid = _DbContext.Voyages.Where(v => v.SFPM_VoyagesId == voyagesId).Select(v => v.IMONumber.ToString()).FirstOrDefault();
            var imoNumbers = (
                              from u in _DbContext.UserVesselGroupMappings
                              join vu in _DbContext.VesselGroupVesselMapping on u.VesselGroupId equals vu.VesselGroupId
                              join vsl in _DbContext.Vessels on vu.VesselId equals vsl.SFPM_VesselId
                              where u.UserId == loginUserId
                              select vsl.IMONumber).Distinct();


            if (imoNumbers.Contains(imonumberfromformid) || userrole.ToLower() == Constant.administrator)
            {
                List<ApprovalAuditsViewModel> approvalAuditsList =
                                           (from audit in _DbContext.PassagesApprovalAudits
                                            join users in _DbContext.Users on audit.ApproverId equals users.UserId
                                            join userRoleMapping in _DbContext.UserRoleMapping on users.UserId equals userRoleMapping.UserId
                                            join roles in _DbContext.Roles on userRoleMapping.RoleId equals roles.RoleId
                                            where audit.VoyagesId == voyagesId
                                            select new ApprovalAuditsViewModel()
                                            {
                                                Action = audit.ApprovalAction,
                                                User = users.FirstName,
                                                Role = roles.RoleName,
                                                Approval = audit.ApprovalStatus,
                                                DateTime = audit.ApprovalDateTime.ToString()
                                            }).OrderBy(a => a.DateTime).ToList();

                return approvalAuditsList;
            }
            else
                return null;
                
        }


        public int InitialApprove(bool isInitialApproved, long initialApprovedBy, string voyageIds)
        {
            var isUservalid = _DbContext.Users.Where(x => x.UserId == initialApprovedBy).FirstOrDefault();
            if (isUservalid == null)
                return (int)ResponseStatus.INVALIDUSER;

            string[] voyageIdList = voyageIds.Split(",");

            foreach (var voyageId in voyageIdList)
            {
                var passagesApprovalAudit = _DbContext.PassagesApprovalAudits.Where(x => x.VoyagesId == Convert.ToInt64(voyageId)).OrderByDescending(x => x.ApprovalDateTime).FirstOrDefault();
                if (passagesApprovalAudit != null)
                {
                    if (passagesApprovalAudit.IsFinalApproved || (isInitialApproved && passagesApprovalAudit.IsInitialApproved))
                        return (int)ResponseStatus.ALREADYAPPROVED;
                    else if (!isInitialApproved && !passagesApprovalAudit.IsInitialApproved)
                        return (int)ResponseStatus.APPROVALREQUIRED;
                }
                PassagesApprovalAudits passageAudit = new PassagesApprovalAudits();
                passageAudit.IsInitialApproved = isInitialApproved;
                passageAudit.IsFinalApproved = false;
                passageAudit.ApprovalStatus = "Initial";
                passageAudit.ApprovalAction = isInitialApproved ? "Approved" : "Unapproved";
                passageAudit.ApproverId = initialApprovedBy;
                passageAudit.ApprovalDateTime = DateTime.UtcNow;
                passageAudit.VoyagesId = Convert.ToInt64(voyageId);
                _DbContext.PassagesApprovalAudits.Add(passageAudit);
            }
            Save();
            return (int)ResponseStatus.SAVED;
        }

        public int FinalApprove(bool isFinalApproved, long finalApprovedBy, string voyageIds)
        {
            var isUservalid = _DbContext.Users.Where(x => x.UserId == finalApprovedBy).FirstOrDefault();
            if (isUservalid == null)
                return (int)ResponseStatus.INVALIDUSER;

            string[] voyageIdList = voyageIds.Split(",");
            foreach (var voyageId in voyageIdList)
            {
                var passagesApprovalAudit = _DbContext.PassagesApprovalAudits.Where(x => x.VoyagesId == Convert.ToInt64(voyageId)).OrderByDescending(x => x.ApprovalDateTime).FirstOrDefault();
                if (passagesApprovalAudit != null)
                {
                    if (isFinalApproved && !passagesApprovalAudit.IsInitialApproved)
                        return (int)ResponseStatus.INITIALAPPROVALREQUIRED;
                    else if (isFinalApproved && passagesApprovalAudit.IsFinalApproved)
                        return (int)ResponseStatus.ALREADYAPPROVED;
                    else if ((!passagesApprovalAudit.IsInitialApproved) || (!isFinalApproved && !passagesApprovalAudit.IsFinalApproved))
                        return (int)ResponseStatus.APPROVALREQUIRED;
                }
                PassagesApprovalAudits passageAudit = new PassagesApprovalAudits();
                passageAudit.IsInitialApproved = true;
                passageAudit.IsFinalApproved = isFinalApproved;
                passageAudit.ApprovalStatus = "Final";
                passageAudit.ApprovalAction = isFinalApproved ? "Approved" : "Unapproved";
                passageAudit.ApproverId = finalApprovedBy;
                passageAudit.ApprovalDateTime = DateTime.UtcNow;
                passageAudit.VoyagesId = Convert.ToInt64(voyageId);
                _DbContext.PassagesApprovalAudits.Add(passageAudit);
            }
            Save();
            return (int)ResponseStatus.SAVED;
        }

        public IEnumerable<PassageWarning> GetPassageWarning(long voyageId,long loginUserId)
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
        public int CreatePassageWarningAudit(PassageWarningAudit passageWarningAudit)
        {
            long userId = Convert.ToInt64(this._currentUser.UserId);
            var passagesApprovalAudit = _DbContext.PassagesApprovalAudits.Where(x => x.VoyagesId == (_DbContext.PassageWarnings.Where(y => y.SFPM_PassageWarningId == passageWarningAudit.PassageWarningId).Select(y => y.VoyageId).FirstOrDefault())).OrderByDescending(x => x.ApprovalDateTime).FirstOrDefault();
            if (passagesApprovalAudit != null)
            {
                if (passagesApprovalAudit.IsFinalApproved || passagesApprovalAudit.IsInitialApproved)
                    return (int)ResponseStatus.ALREADYAPPROVED;
            }

            var isUservalid = _DbContext.Users.Where(x => x.UserId == passageWarningAudit.ReviewedBy).FirstOrDefault();
            if (isUservalid == null)
                return (int)ResponseStatus.INVALIDUSER;

            passageWarningAudit.ReviewDateTime = DateTime.Now;
            var chkPassageWarningAudit = _DbContext.PassageWarningAudits.Where(x => x.PassageWarningId == passageWarningAudit.PassageWarningId).OrderByDescending(x => x.SFPM_PassageWarningAuditId).FirstOrDefault();
            if (chkPassageWarningAudit == null)
            {
                passageWarningAudit.CreatedBy = userId;
                passageWarningAudit.CreatedDateTime = DateTime.UtcNow;
                _DbContext.PassageWarningAudits.Add(passageWarningAudit);
            }
            else
            {
                if (chkPassageWarningAudit.IsApproved != passageWarningAudit.IsApproved)
                {
                    passageWarningAudit.ModifiedBy = userId;
                    passageWarningAudit.ModifiedDateTime = DateTime.UtcNow;
                    _DbContext.PassageWarningAudits.Update(passageWarningAudit);
                }
                else
                {
                    return (int)ResponseStatus.ALREADYEXIST;
                }
            }
            Save();
            return (int)ResponseStatus.SAVED;
        }
        #endregion

        #region Position/Noon Reports

        public IEnumerable<VoyagesIntitialDataViewModel> GetReportsForVoyage(string imoNumber, long voyageNumber, DateTime actualStartOfSeaPassage, string departureTimeZoneOffset, long userId)
        {
            var userrole = _DbContext.Roles.Where(x => x.RoleId == (_DbContext.UserRoleMapping.Where(y => y.UserId == Convert.ToInt64(this._currentUser.UserId)).Select(y => y.RoleId).FirstOrDefault())).Select(x => x.RoleName).AsNoTracking().FirstOrDefault();
            long loginuserId = Convert.ToInt64(this._currentUser.UserId);
            var imoNumbers = (from u in _DbContext.UserVesselGroupMappings.AsEnumerable()
                              join vu in _DbContext.VesselGroupVesselMapping.AsEnumerable() on u.VesselGroupId equals vu.VesselGroupId
                              join vsl in _DbContext.Vessels.AsEnumerable() on vu.VesselId equals vsl.SFPM_VesselId
                              where u.UserId == Convert.ToInt64(this._currentUser.UserId)
                              select vsl.IMONumber).ToList();

            if (loginuserId.Equals(userId) && (imoNumbers.Contains(imoNumber) || userrole.ToLower() == Constant.administrator))
            {
                List<Forms> formsList = new List<Forms>();
                var previousVoyage = _DbContext.Voyages.Where(x => x.IMONumber == imoNumber && x.ActualStartOfSeaPassage > actualStartOfSeaPassage).OrderBy(x => x.ActualStartOfSeaPassage).FirstOrDefault();
                var imoNumberint = Convert.ToInt64(imoNumber);
                DateTimeOffset fromDatetime = actualStartOfSeaPassage;
                TimeSpan zeroOffsetTimeSpan = new TimeSpan(0, 0, 0, 0, 0);
                fromDatetime = fromDatetime.ToOffset(zeroOffsetTimeSpan);

                if (actualStartOfSeaPassage != null && previousVoyage != null)
                {
                    DateTimeOffset todatetime = Convert.ToDateTime(previousVoyage.ActualStartOfSeaPassage);
                    todatetime = todatetime.ToOffset(zeroOffsetTimeSpan);
                    fromDatetime = fromDatetime.ToOffset(zeroOffsetTimeSpan);
                    formsList = _DbContext.Forms.Where(x => x.ImoNumber == imoNumberint && x.ReportDateTime >= fromDatetime
                        && x.ReportDateTime < todatetime).OrderBy(a => a.ReportDateTime).ToList();
                }
                else
                {
                    formsList = _DbContext.Forms.Where(x => x.ImoNumber == imoNumberint && x.ReportDateTime >= fromDatetime)
                         .OrderBy(a => a.ReportDateTime).ToList();
                }
                List<VoyagesIntitialDataViewModel> voyagesIntitialList = new List<VoyagesIntitialDataViewModel>();
                List<DateTimeOffset> reportDateTimeList = new List<DateTimeOffset>();
                VoyagesIntitialDataViewModel voyagesModel;
                string sourceUserName = string.Empty, editedByUserName = string.Empty;
                var users = _DbContext.Users.ToList();
                foreach (Forms formRow in formsList)
                {
                    sourceUserName = (formRow.CreatedBy == 0) ? "Ship" : (formRow.CreatedBy == -1) ? "VDD" : _DbContext.Users.Where(x => x.UserId == formRow.CreatedBy).FirstOrDefault().Username;
                    editedByUserName = (formRow.ModifiedBy == 0) ? "Ship" : (formRow.ModifiedBy == -1) ? "VDD" : _DbContext.Users.Where(x => x.UserId == formRow.ModifiedBy).FirstOrDefault().Username;
                    voyagesModel = new VoyagesIntitialDataViewModel();
                    voyagesModel.Source = !string.IsNullOrEmpty(formRow.FormGUID) ? "Ship" : sourceUserName;
                    voyagesModel.EditedBy = editedByUserName;

                    var excludeReportLogs = _DbContext.ExcludeReportLogs.Where(x => x.FormId == formRow.SFPM_Form_Id);
                    voyagesModel.ExcludedFromPool = excludeReportLogs.Any() ?
                        excludeReportLogs.Where(x => x.ReportId == 2).OrderByDescending(x => x.CreatedDateTime).ToList().FirstOrDefault() != null ?
                        excludeReportLogs.Where(x => x.ReportId == 2).OrderByDescending(x => x.CreatedDateTime).ToList().FirstOrDefault().Excluded : false : false;
                    voyagesModel.ExcludedFromTC = excludeReportLogs.Any() ? excludeReportLogs.Where(x => x.ReportId == 1)
                     .OrderByDescending(x => x.CreatedDateTime).ToList().FirstOrDefault() != null ?
                     excludeReportLogs.Where(x => x.ReportId == 1)
                     .OrderByDescending(x => x.CreatedDateTime).ToList().FirstOrDefault().Excluded : false : false;

                    voyagesModel.TimeZone = formRow.ReportTime;
                    voyagesModel.Type = formRow.Location == "N" ? Constant.AtSea :
                                           formRow.Location == "S" ? Constant.Atport :
                                           formRow.FormIdentifier.Contains("Arrival") ? Constant.Arrival :
                                           formRow.FormIdentifier.Contains("Departure") ? Constant.Departure :
                                            formRow.FormIdentifier.Contains("Bunker") ? Constant.Bunker : "";
                    voyagesModel.IconType = voyagesModel.Type== Constant.Bunker ? Constant.bunckerIconType: voyagesModel.Type;
                    voyagesModel.DateAndTime = formRow.ReportDateTime;
                    voyagesModel.IsConflict = reportDateTimeList.Exists(f => f.Equals(formRow.ReportDateTime));
                    reportDateTimeList.Add(formRow.ReportDateTime.Value);
                    voyagesModel.Form = formRow;
                    voyagesModel.Positions = formRow.Latitude + " / " + formRow.Longitude;

                    DateTimeOffset previousfromDatetime = formRow.ReportDateTime.Value.DateTime;
                    previousfromDatetime = previousfromDatetime.ToOffset(zeroOffsetTimeSpan);
                    var previousReportDate = _DbContext.Forms.Where(x => x.ReportDateTime < previousfromDatetime
                         && x.ImoNumber == imoNumberint)
                        .OrderByDescending(x => x.ReportDateTime).FirstOrDefault();

                    DateTime previousfronUtcReportdatetime = DateTime.UtcNow;
                    if (previousReportDate != null)
                    {
                        var hrs = Convert.ToDouble(previousReportDate.ReportTime.Substring(1, 2));
                        var min = Convert.ToDouble(previousReportDate.ReportTime.Substring(4, 2));
                        previousfronUtcReportdatetime = previousReportDate.ReportTime.Contains("-") ? previousReportDate.ReportDateTime.Value.DateTime.AddHours(hrs).AddMinutes(min) :
                            previousReportDate.ReportDateTime.Value.DateTime.AddHours(-hrs).AddMinutes(-min);

                    }
                    var hrs1 = Convert.ToDouble(formRow.ReportTime.Substring(1, 2));
                    var min1 = Convert.ToDouble(formRow.ReportTime.Substring(4, 2));
                    DateTime fronUtcReportdatetime = formRow.ReportTime.Contains("-") ? formRow.ReportDateTime.Value.DateTime.AddHours(hrs1).AddMinutes(min1) :
                        formRow.ReportDateTime.Value.DateTime.AddHours(-hrs1).AddMinutes(-min1);

                    voyagesModel.HrsReportDatetime = previousReportDate != null ?
                     String.Format("{0:0.00}", Convert.ToDecimal(fronUtcReportdatetime.Subtract(previousfronUtcReportdatetime).TotalHours.ToString())) : "0";

                    var analyzedWeather = _DbContext.AnalyzedWeather.Where(x => x.FormId == formRow.SFPM_Form_Id && x.Is24Hour == true).FirstOrDefault();
                    if (analyzedWeather != null)
                    {
                        voyagesModel.AnalyzedWind = analyzedWeather.AnalyzedWind;
                        voyagesModel.AnalyzedWave = analyzedWeather.AnalyzedWave;
                        voyagesModel.AnalyzedWaveDiection = analyzedWeather.AnalyzedWaveDiection;
                        voyagesModel.AnalyzedWindDirection = analyzedWeather.AnalyzedWindDirection;
                        voyagesModel.AnalyzedCurrent = analyzedWeather.AnalyzedCurrent;
                    }
                    voyagesModel.IsPositionWarningExists = false;
                    voyagesModel.IsEventExists = _DbContext.EventROBsRow.Where(x => x.FormId == formRow.SFPM_Form_Id).Any();
                    foreach (var warnig in _DbContext.PositionWarnings.Where(x => x.FormId == formRow.SFPM_Form_Id).ToList())
                    {
                        if (!_DbContext.PositionWarningAudits.Where(x => x.PositionWarningId == warnig.SFPM_PositionWarningId).OrderByDescending(x => x.ReviewDateTime).Select(x => x.IsApproved).FirstOrDefault())
                        {
                            voyagesModel.IsPositionWarningExists = true;
                            break;
                        }

                    }
                    voyagesIntitialList.Add(voyagesModel);
                }
                return voyagesIntitialList.OrderBy(x => x.DateAndTime).ToList();
            }
            else
               return null;
        }

        public int CreatePosition(Forms forms,long loginUserId)
        {
            long userId = loginUserId;//Convert.ToInt64(this._currentUser.UserId);
            forms.SFPM_Form_Id = 0;
            forms.ReportTime = SetTimezoneFormate(forms.ReportTime);
            var formsObj = _DbContext.Forms.Where(x => x.SFPM_Form_Id == forms.SFPM_Form_Id).FirstOrDefault();
            formsObj = forms;
            var voyageId = (from form in _DbContext.Forms
                            from voyage in _DbContext.Voyages
                            where voyage.IMONumber == formsObj.ImoNumber.ToString()
                            && voyage.ActualStartOfSeaPassage <= formsObj.ReportDateTime.Value.DateTime
                            && voyage.ActualEndOfSeaPassage >= formsObj.ReportDateTime.Value.DateTime
                            select new { voyage.SFPM_VoyagesId }).FirstOrDefault();
            if (voyageId != null)
            {
                PassagesApprovalAudits auditData = _DbContext.PassagesApprovalAudits.Where(x => x.VoyagesId == voyageId.SFPM_VoyagesId).OrderByDescending(x => x.ApprovalDateTime).FirstOrDefault();
                if (auditData != null)
                {
                    if (auditData.IsInitialApproved || auditData.IsFinalApproved)
                        return (int)ResponseStatus.ALREADYAPPROVED;
                }
            }
            if (formsObj.FormIdentifier.Contains("Arrival"))
            {
                if (voyageId != null)
                {
                    var isvoyage = _DbContext.Voyages.Where(x => x.SFPM_VoyagesId == voyageId.SFPM_VoyagesId).FirstOrDefault();
                    if (isvoyage != null)
                    {
                        isvoyage.ActualEndOfSeaPassage = formsObj.ReportDateTime.Value.DateTime;
                        _DbContext.Voyages.Update(isvoyage);
                    }
                }
            }
            formsObj.SFPM_Form_Id = 0;
            formsObj.CompanyCode = "SCRP";
            formsObj.CompanyName = "Scorpio Commerical Management";
            formsObj.CreatedBy = userId;
            formsObj.CreatedDateTime = /*forms.ModifiedDateTime =*/ DateTime.UtcNow;
            formsObj.ModifiedDateTime = DateTime.UtcNow;//added by prashant
            formsObj.Constant = forms.Constant;//added by prashant
            formsObj.ReportDateTime = forms.ReportDateTime != null ? forms.ReportDateTime : DateTimeOffset.UtcNow;
            //formsObj.ModifiedBy = forms.CreatedBy;
            _DbContext.Forms.Add(formsObj);
            if (formsObj.FuelsRows != null)
            {
                Save();
                foreach (var fuelrow in formsObj.FuelsRows)
                {
                    fuelrow.FormId = formsObj.SFPM_Form_Id;
                    _DbContext.FuelsRows.Add(fuelrow);
                }
            }
            if (formsObj.FormUnits != null)
            {
                Save();
                formsObj.FormUnits.FormId = formsObj.SFPM_Form_Id;
                _DbContext.FormUnits.Add(formsObj.FormUnits);
            }
            Save();
            if (!formsObj.FormIdentifier.ToLower().Contains("bunker"))
            {
                CreatePositionWarning(formsObj);
                CreateAnalyzedWeatherBasedOnMeteoStratumPosition(formsObj);
                CreateAnalyzedWeather(formsObj);
            }
            if (voyageId != null)
            {
                CreatePassageWarning(_DbContext.Voyages.Where(x => x.SFPM_VoyagesId == voyageId.SFPM_VoyagesId).FirstOrDefault());
            }

            return (int)ResponseStatus.SAVED;
        }
        public int DeletePosition(string formIds)
        {
            return DeletePositionRecord(formIds);
        }
        public int UpdatePositionRemark(long formId, string remark,long loginUserId)
        {
            //long userId = Convert.ToInt64(this._currentUser.UserId);
            var isForms = _DbContext.Forms.Where(x => x.SFPM_Form_Id == formId).FirstOrDefault();
            if (isForms != null)
            {
                isForms.ModifiedBy = loginUserId;
                isForms.ModifiedDateTime = DateTime.UtcNow;
                isForms.Remarks = remark;
                _DbContext.Forms.Update(isForms);
            }
            return Save();
        }
        public IEnumerable<PositionWarning> GetPositionWarning(long formId,long loginUserId)
        {
            var userrole = _DbContext.Roles.Where(x => x.RoleId == (_DbContext.UserRoleMapping.Where(y => y.UserId == loginUserId).Select(y => y.RoleId).FirstOrDefault())).Select(x => x.RoleName).AsNoTracking().FirstOrDefault();
            var imonumberfromformid = _DbContext.Forms.Where(v => v.SFPM_Form_Id == formId).Select(v => v.ImoNumber.ToString()).FirstOrDefault();
            var imoNumbers = (
                              from u in _DbContext.UserVesselGroupMappings
                              join vu in _DbContext.VesselGroupVesselMapping on u.VesselGroupId equals vu.VesselGroupId
                              join vsl in _DbContext.Vessels on vu.VesselId equals vsl.SFPM_VesselId
                              where u.UserId == loginUserId
                              select vsl.IMONumber).Distinct();


            if (imoNumbers.Contains(imonumberfromformid) || userrole.ToLower() == Constant.administrator)
                return _DbContext.PositionWarnings.Where(x => x.FormId == formId).ToList();
            else
                return null;

        }

        public IEnumerable<PositionWarningAudit> GetPositionWarningAudit(long positionWarningId)
        {
                return (from positionWarningAudits in _DbContext.PositionWarningAudits
                        join users in _DbContext.Users on positionWarningAudits.ReviewedBy equals users.UserId
                        where positionWarningAudits.PositionWarningId == positionWarningId
                        select new PositionWarningAudit
                        {
                            SFPM_PositionWarningAuditId = positionWarningAudits.SFPM_PositionWarningAuditId,
                            PositionWarningId = positionWarningAudits.PositionWarningId,
                            IsApproved = positionWarningAudits.IsApproved,
                            ReviewedBy = positionWarningAudits.ReviewedBy,
                            ReviewedName = users.Username,
                            ReviewDateTime = positionWarningAudits.ReviewDateTime
                        }).ToList();
           
        }

        public int CreatePositionWarningAudit(PositionWarningAudit positionWarningAudit,long loginUserId)
        {
            long userId = loginUserId;
            var isUservalid = _DbContext.Users.Where(x => x.UserId == positionWarningAudit.ReviewedBy).FirstOrDefault();
            if (isUservalid == null)
                return (int)ResponseStatus.INVALIDUSER;

            if (IsVoyageApproved(_DbContext.PositionWarnings.Where(x => x.SFPM_PositionWarningId == positionWarningAudit.PositionWarningId).Select(x => x.FormId).FirstOrDefault()))
            {
                return (int)ResponseStatus.ALREADYAPPROVED;
            }
            positionWarningAudit.ReviewDateTime = DateTime.Now;
            positionWarningAudit.CreatedBy = userId;
            positionWarningAudit.CreatedDateTime = DateTime.UtcNow;
            var chkPositionWarningAudit = _DbContext.PositionWarningAudits.Where(x => x.PositionWarningId == positionWarningAudit.PositionWarningId).OrderByDescending(x => x.SFPM_PositionWarningAuditId).FirstOrDefault();
            if (chkPositionWarningAudit == null)
            {
                _DbContext.PositionWarningAudits.Add(positionWarningAudit);
            }
            else
            {
                if (chkPositionWarningAudit.IsApproved != positionWarningAudit.IsApproved)
                {
                    _DbContext.PositionWarningAudits.Add(positionWarningAudit);
                }
                else
                {
                    return (int)ResponseStatus.ALREADYEXIST;
                }
            }
            Save();
            return (int)ResponseStatus.SAVED;
        }
        public IEnumerable<FromsViewModel> GetViewOriginalEmail(long formId,long loginUserId)
        {

            var userrole = _DbContext.Roles.Where(x => x.RoleId == (_DbContext.UserRoleMapping.Where(y => y.UserId == loginUserId).Select(y => y.RoleId).FirstOrDefault())).Select(x => x.RoleName).AsNoTracking().FirstOrDefault();
            var imonumberfromformid = _DbContext.Forms.Where(v => v.SFPM_Form_Id == formId).Select(v => v.ImoNumber.ToString()).FirstOrDefault();
            var imoNumbers = (
                              from u in _DbContext.UserVesselGroupMappings
                              join vu in _DbContext.VesselGroupVesselMapping on u.VesselGroupId equals vu.VesselGroupId
                              join vsl in _DbContext.Vessels on vu.VesselId equals vsl.SFPM_VesselId
                              where u.UserId == loginUserId
                              select vsl.IMONumber).Distinct();


            if (imoNumbers.Contains(imonumberfromformid) || userrole.ToLower() == Constant.administrator)
            {
                return (from forms in _DbContext.Forms
                        where forms.SFPM_Form_Id == formId
                        select new FromsViewModel()
                        {
                            OriginalEmailText = forms.OriginalEmailText,
                            EmailAttachmentFileName = forms.EmailAttachmentFileName,
                            OriginalFormsXML = forms.OriginalFormsXML
                        }).ToList();
            }
            else
                return null;

        }

        public Forms GetPositionById(long formId,long loginUserId)
        {
            var userrole = _DbContext.Roles.Where(x => x.RoleId == (_DbContext.UserRoleMapping.Where(y => y.UserId == loginUserId).Select(y => y.RoleId).FirstOrDefault())).Select(x => x.RoleName).AsNoTracking().FirstOrDefault();
            var imonumberfromformid = _DbContext.Forms.Where(v => v.SFPM_Form_Id == formId).Select(v => v.ImoNumber.ToString()).FirstOrDefault();
            var imoNumbers = (
                              from u in _DbContext.UserVesselGroupMappings
                              join vu in _DbContext.VesselGroupVesselMapping on u.VesselGroupId equals vu.VesselGroupId
                              join vsl in _DbContext.Vessels on vu.VesselId equals vsl.SFPM_VesselId
                              where u.UserId == loginUserId
                              select vsl.IMONumber).Distinct();


            if (imoNumbers.Contains(imonumberfromformid) || userrole.ToLower() == Constant.administrator)
            {
                var forms = _DbContext.Forms.Where(x => x.SFPM_Form_Id == formId).FirstOrDefault();
                if (forms != null)
                {
                    forms.FormUnits = _DbContext.FormUnits.Where(x => x.FormId == formId).FirstOrDefault();
                    forms.FuelsRows = (from fuelsRows in _DbContext.FuelsRows
                                       where fuelsRows.FormId == formId
                                       select new FuelsRows
                                       {
                                           SFPM_FuelsRowsId = fuelsRows.SFPM_FuelsRowsId,
                                           FuelType = fuelsRows.FuelType != null ? fuelsRows.FuelType : "",
                                           QtyLifted = fuelsRows.QtyLifted != null ? fuelsRows.QtyLifted : 0,
                                           //added by prashant
                                           BDN_Number = fuelsRows.BDN_Number != null ? fuelsRows.BDN_Number : "",
                                           Fuel_Densitry = fuelsRows.Fuel_Densitry != null ? fuelsRows.Fuel_Densitry : 0,
                                           Sulphur_Content = fuelsRows.Sulphur_Content != null ? fuelsRows.Sulphur_Content : 0
                                       }).ToList();
                }
                return forms;
            }
            else
                return null;
                
        }
        public int UpdatePosition(Forms forms)
        {
            long userId = Convert.ToInt64(this._currentUser.UserId);
            if (IsVoyageApproved(forms.SFPM_Form_Id))
            {
                return (int)ResponseStatus.ALREADYAPPROVED;
            }
            forms.ReportTime = SetTimezoneFormate(forms.ReportTime);
            var position = _DbContext.Forms.Where(x => x.SFPM_Form_Id == forms.SFPM_Form_Id).FirstOrDefault();
            if (position != null)
            {
                position.VoyageNo = forms.VoyageNo;
                position.FormIdentifier = forms.FormIdentifier;
                position.ReportTime = forms.ReportTime;
                position.Latitude = forms.Latitude;
                position.Longitude = forms.Longitude;
                position.Heading = forms.Heading;
                position.ObsSpeed = forms.ObsSpeed;
                position.CPSpeed = forms.CPSpeed;
                position.SteamingHrs = forms.SteamingHrs;
                position.FWDDraft = forms.FWDDraft;
                position.AFTDraft = forms.AFTDraft;
                position.DistanceToGO = forms.DistanceToGO;
                position.EngineDistance = forms.EngineDistance;
                position.ObservedDistance = forms.ObservedDistance;
                position.Dist_by_Speed_Log_nm = forms.Dist_by_Speed_Log_nm;
                position.Slip = forms.Slip;
                position.RPM = forms.RPM;
                position.AvgBHP = forms.AvgBHP;
                position.Cargo_Temp = forms.Cargo_Temp;
                position.WindForce = forms.WindForce;
                position.WindDirection = forms.WindDirection;
                position.SeaHeight = forms.SeaHeight;
                position.SeaDir = forms.SeaDir;
                position.SeaTemp = forms.SeaDir;
                position.AirTemp = forms.AirTemp;
                position.BaroPressure = forms.BaroPressure;
                position.SlopsROB = forms.SlopsROB;
                position.Gen2Hrs = forms.Gen2Hrs;
                position.Slops_Water = forms.Slops_Water;
                position.DWT = forms.DWT;
                position.Displacement = forms.Displacement;
                position.ChartererCleaningKitOnboard = forms.ChartererCleaningKitOnboard;
                position.MIDDraft = forms.MIDDraft;
                position.Swell = forms.Swell;
                position.Slops_Oil = forms.Slops_Oil;
                position.Ops_Voyage_Number = forms.Ops_Voyage_Number;
                position.MainEngineHrs = forms.MainEngineHrs;
                position.MEOutputPct = forms.MEOutputPct;
                position.SwellHeight = forms.SwellHeight;
                position.SwellDir = forms.SwellDir;
                position.MECounter = forms.MECounter;
                position.Daily_FW_production = forms.Daily_FW_production;
                position.Total_Bilge_water_tank_ROB_CubM = forms.Total_Bilge_water_tank_ROB_CubM;
                position.Total_Bilge_water_tank_content__of_max = forms.Total_Bilge_water_tank_content__of_max;
                position.Total_Sludge_tank_ROB_CubM = forms.Total_Sludge_tank_ROB_CubM;
                position.Total_Sludge_tank_content__of_max = forms.Total_Sludge_tank_content__of_max;
                position.LSIFO = forms.LSIFO;
                position.LSMGO = forms.LSMGO;
                position.Location = forms.Location;
                position.ModifiedBy = forms.ModifiedBy;
                position.ReportDateTime = forms.ReportDateTime;
                position.ModifiedDateTime = DateTime.Now;
                position.CreatedBy = position.CreatedBy;
                position.CreatedDateTime = position.CreatedDateTime;
                position.SeaState = forms.SeaState;//added by prashant
                position.BunkerVendor = forms.BunkerVendor;//added by prashant
                position.Barge_Name = forms.Barge_Name; //added by prashant
                position.Barge_Alongside = forms.Barge_Alongside;//added by prashant
                position.Bunker_Hose_Connected = forms.Bunker_Hose_Connected;//added by prashant
                position.Commenced_Bunkering = forms.Commenced_Bunkering;//added by prashant
                position.Barge_Cast_Off = forms.Barge_Cast_Off;//added by prashant
                position.Remarks = forms.Remarks;//added by prashant
                position.Bunker_Hose_disconnected = forms.Bunker_Hose_disconnected;//added by prashant

                //added below feilds while updating
                position.ROB_LSMGO = forms.ROB_LSMGO;
                position.Water_Type = forms.Water_Type;
                position.WaterType = forms.WaterType;//added by prashant
                position.SirePSC_Inspection = forms.SirePSC_Inspection;
                position.ROB_IFO = forms.ROB_IFO;
                position.SBE_At_Berth = forms.SBE_At_Berth;
                position.HSIFO = forms.HSIFO;
                position.Gen1Hrs = forms.Gen1Hrs;
                position._24Hrs_Other_ConsTypeReasonDuration = forms._24Hrs_Other_ConsTypeReasonDuration;
                position.HSMGO = forms.HSMGO;
                position.ROB_LSIFO = forms.ROB_LSIFO;
                position.BoilerHrs =  forms.ROB_LSMGO;
                position.Current = forms.Current;
                position.SCIFO = forms.SCIFO;  //added by prashant becoz sajag pass all vlsfo value in scifo
                position.CurrentDirection = forms.CurrentDirection;
                position.Total_Cargo_Onboard = forms.Total_Cargo_Onboard;
                if(forms.Total_Cargo_Onboard!=null)
                  position.Constant = Convert.ToDecimal(forms.Total_Cargo_Onboard);
                position.Gen3Hrs = forms.Gen3Hrs;
                position.TotalBilgeWaterContent = forms.TotalBilgeWaterContent;
                position.TotalBilgeWaterROB = forms.TotalBilgeWaterROB;
                position.TotalSludgeContent = forms.TotalSludgeContent;
                position.TotalSludgeROB = forms.TotalSludgeROB;
                position.ROB_MGO = forms.ROB_MGO;
                position.Constant = forms.Constant; //added by prashant

                _DbContext.Forms.Update(position);

                
                if (forms.FuelsRows != null)
                {
                    _DbContext.FuelsRows.RemoveRange(_DbContext.FuelsRows.Where(x => x.FormId == position.SFPM_Form_Id));
                    foreach (var fuelrow in forms.FuelsRows)
                    {
                        fuelrow.FormId = forms.SFPM_Form_Id;
                        fuelrow.BDN_Number = fuelrow.BDN_Number;
                        fuelrow.Fuel_Densitry = fuelrow.Fuel_Densitry;
                        fuelrow.Sulphur_Content = fuelrow.Sulphur_Content;
                        fuelrow.QtyLifted = fuelrow.QtyLifted;
                        fuelrow.FuelType = fuelrow.FuelType;
                        fuelrow.SFPM_FuelsRowsId = 0;
                        fuelrow.ModifiedBy = userId;
                        fuelrow.ModifiedDateTime = DateTime.UtcNow;
                        _DbContext.FuelsRows.Add(fuelrow);
                        
                    }
                }
                if (forms.FormUnits != null)
                {
                    var formUnits = _DbContext.FormUnits.Where(x => x.FormId == forms.SFPM_Form_Id && x.SFPM_FormUnitsId == forms.FormUnits.SFPM_FormUnitsId).AsNoTracking().SingleOrDefault();
                    if (formUnits != null)
                    {
                        _DbContext.FormUnits.Update(forms.FormUnits);
                    }
                }
                Save();
                if (!forms.FormIdentifier.ToLower().Contains("bunker"))
                {
                    CreatePositionWarning(forms);
                    CreateAnalyzedWeatherBasedOnMeteoStratumPosition(forms);
                    CreateAnalyzedWeather(forms);
                }
                var voyageId = (from form in _DbContext.Forms
                                from voyage in _DbContext.Voyages
                                where voyage.VoyageNumber == forms.VoyageNo
                                && voyage.ActualStartOfSeaPassage <= forms.ReportDateTime.Value.DateTime
                                && voyage.ActualEndOfSeaPassage >= forms.ReportDateTime.Value.DateTime
                                select new { voyage.SFPM_VoyagesId }).FirstOrDefault();
                if (voyageId != null)
                {
                    CreatePassageWarning(_DbContext.Voyages.Where(x => x.SFPM_VoyagesId == voyageId.SFPM_VoyagesId).FirstOrDefault());
                }

                return (int)ResponseStatus.SAVED;
            }
            return (int)ResponseStatus.NOTFOUND;
        }

        public Voyages GetActualStartPassage(Forms forms)
        {
            var reportdatetime = forms.ReportDateTime.Value.DateTime;
            var imoNumber = forms.ImoNumber.ToString();
            return _DbContext.Voyages.Where(x => x.IMONumber == imoNumber && reportdatetime >= x.ActualStartOfSeaPassage).OrderByDescending(x => x.ActualStartOfSeaPassage).FirstOrDefault();
        }

        #endregion

        #region Event
        public List<EventROBsRow> GetAllEvents(long formId,long loginUserId)
        {
            var userrole = _DbContext.Roles.Where(x => x.RoleId == (_DbContext.UserRoleMapping.Where(y => y.UserId == loginUserId).Select(y => y.RoleId).FirstOrDefault())).Select(x => x.RoleName).AsNoTracking().FirstOrDefault();
            var imonumberfromformid = _DbContext.Forms.Where(v => v.SFPM_Form_Id == formId).Select(v => v.ImoNumber.ToString()).FirstOrDefault();
            var imoNumbers = (
                              from u in _DbContext.UserVesselGroupMappings
                              join vu in _DbContext.VesselGroupVesselMapping on u.VesselGroupId equals vu.VesselGroupId
                              join vsl in _DbContext.Vessels on vu.VesselId equals vsl.SFPM_VesselId
                              where u.UserId == loginUserId
                              select vsl.IMONumber).Distinct();


            if (imoNumbers.Contains(imonumberfromformid) || userrole.ToLower() == Constant.administrator)
            {
                List<EventROBsRow> eventROBsRow = _DbContext.EventROBsRow.Where(x => x.FormId == formId).ToList();
                var pageSetting = _DbContext.PageSettings.Where(setting => setting.UserId == loginUserId).FirstOrDefault();
                if (pageSetting != null && pageSetting.IsPositionUTC)
                {
                    eventROBsRow.ForEach(x => x.EventROBStartDateTime = x.EventROBStartDateTime.UtcDateTime);
                    eventROBsRow.ForEach(x => x.EventROBEndDateTime = x.EventROBEndDateTime.UtcDateTime);
                }
                return eventROBsRow;
            }
            else
                return null;
                
        }

        public EventROBsRow GetEventById(long eventId,long loginUserId)
        {
            var userrole = _DbContext.Roles.Where(x => x.RoleId == (_DbContext.UserRoleMapping.Where(y => y.UserId == loginUserId).Select(y => y.RoleId).FirstOrDefault())).Select(x => x.RoleName).AsNoTracking().FirstOrDefault();
            var formid= _DbContext.EventROBsRow.Where(v => v.SFPM_EventROBsRowId == eventId).Select(v => v.FormId.ToString()).FirstOrDefault();
            var imonumberfromformid = _DbContext.Forms.Where(v => v.SFPM_Form_Id == Convert.ToInt64(formid)).Select(v => v.ImoNumber.ToString()).FirstOrDefault();
            var imoNumbers = (
                              from u in _DbContext.UserVesselGroupMappings
                              join vu in _DbContext.VesselGroupVesselMapping on u.VesselGroupId equals vu.VesselGroupId
                              join vsl in _DbContext.Vessels on vu.VesselId equals vsl.SFPM_VesselId
                              where u.UserId == loginUserId
                              select vsl.IMONumber).Distinct();


            if (imoNumbers.Contains(imonumberfromformid) || userrole.ToLower() == Constant.administrator)
            {
                var events = _DbContext.EventROBsRow.Where(x => x.SFPM_EventROBsRowId == eventId).FirstOrDefault();
                events.EventType = events.EventType.ToUpper();
                return events;
            }
            else
                return null;
                
        }
        public int CreateEvent(EventROBsRow eventROBsRow)
        {
            long userId = Convert.ToInt64(this._currentUser.UserId);
            if (eventROBsRow.EventROBEndLatitude == null)
                eventROBsRow.EventROBEndLatitude = "N/A";
            if (eventROBsRow.EventROBStartLatitude == null)
                eventROBsRow.EventROBStartLatitude = "N/A";
            if (eventROBsRow.EventROBEndLongitude == null)
                eventROBsRow.EventROBEndLongitude = "N/A";
            if (eventROBsRow.EventROBStartLongitude == null)
                eventROBsRow.EventROBStartLongitude = "N/A";

            eventROBsRow.CreatedBy = userId;
            eventROBsRow.CreatedDateTime = DateTime.UtcNow;
            _DbContext.EventROBsRow.Add(eventROBsRow);
            _DbContext.SaveChanges();
            return (int)ResponseStatus.SAVED;

            //COMMENTED  on 20210118 as discussed on video call that no condition has to be checked for already exist and multiple event can be created with same type.
            //var eventObj = _DbContext.EventROBsRow.Where(x => x.EventType == eventROBsRow.EventType && x.FormId == eventROBsRow.FormId).FirstOrDefault();
            //if (eventObj == null)
            //{
            //    eventROBsRow.CreatedBy = userId;
            //    eventROBsRow.CreatedDateTime = DateTime.UtcNow;
            //    _DbContext.EventROBsRow.Add(eventROBsRow);
            //    _DbContext.SaveChanges();
            //    return (int)ResponseStatus.SAVED;
            //}
            //return (int)ResponseStatus.ALREADYEXIST;
        }

        public int UpdateEvent(EventROBsRow eventROBsRow,long loginUserId)
        {
            long userId = loginUserId;// Convert.ToInt64(this._currentUser.UserId);
            if (eventROBsRow.EventROBEndLatitude == null)
                eventROBsRow.EventROBEndLatitude = "N/A";
            if (eventROBsRow.EventROBStartLatitude == null)
                eventROBsRow.EventROBStartLatitude = "N/A";
            if (eventROBsRow.EventROBEndLongitude == null)
                eventROBsRow.EventROBEndLongitude = "N/A";
            if (eventROBsRow.EventROBStartLongitude == null)
                eventROBsRow.EventROBStartLongitude = "N/A";

            var eventObjchk = _DbContext.EventROBsRow.Where(x => x.SFPM_EventROBsRowId == eventROBsRow.SFPM_EventROBsRowId).AsNoTracking().FirstOrDefault();
            if (eventObjchk != null)
            {
                eventROBsRow.ModifiedBy = userId;
                eventROBsRow.ModifiedDateTime = DateTime.UtcNow;
                eventROBsRow.CreatedBy = eventObjchk.CreatedBy;
                eventROBsRow.CreatedDateTime = eventObjchk.CreatedDateTime;
                _DbContext.EventROBsRow.Update(eventROBsRow);
                _DbContext.SaveChanges();
                return (int)ResponseStatus.SAVED;
            }
            return (int)ResponseStatus.NOTFOUND;


            //COMMENTED  on 20210118 as discussed on video call that no condition has to be checked for already exist and multiple event can be created with same type.
            //var eventObj = _DbContext.EventROBsRow.Where(x => x.SFPM_EventROBsRowId != eventROBsRow.SFPM_EventROBsRowId && x.FormId == eventROBsRow.FormId && x.EventType == eventROBsRow.EventType).AsNoTracking().FirstOrDefault();
            //var eventObjchk = _DbContext.EventROBsRow.Where(x => x.SFPM_EventROBsRowId == eventROBsRow.SFPM_EventROBsRowId).AsNoTracking().FirstOrDefault();
            //if (eventObj == null && eventObjchk != null)
            //{
            //    eventROBsRow.ModifiedBy = userId;
            //    eventROBsRow.ModifiedDateTime = DateTime.UtcNow;
            //    eventROBsRow.CreatedBy = eventObjchk.CreatedBy;
            //    eventROBsRow.CreatedDateTime = eventObjchk.CreatedDateTime;
            //    _DbContext.EventROBsRow.Update(eventROBsRow);
            //    _DbContext.SaveChanges();
            //    return (int)ResponseStatus.SAVED;
            //}
            //return (int)ResponseStatus.ALREADYEXIST;
        }
        public int DeleteEvent(long eventId,long loginUserId)
        {
            long userId = loginUserId;//Convert.ToInt64(this._currentUser.UserId);
            if (IsVoyageApproved(_DbContext.EventROBsRow.Where(x => x.SFPM_EventROBsRowId == eventId).Select(x => x.FormId).FirstOrDefault()))
            {
                return (int)ResponseStatus.ALREADYAPPROVED;
            }
            var isrobs = _DbContext.Robs.Where(x => x.EventRobsRowId == eventId).FirstOrDefault();
            if (isrobs != null)
            {
                var isrob = _DbContext.Rob.Where(x => x.RobsSFPM_RobsId == isrobs.SFPM_RobsId).ToList();
                if (isrob.Count > 0)
                {
                    foreach (var rob in isrob)
                    {
                        var allocations = _DbContext.Allocation.Where(x => x.RobSFPM_RobId == rob.SFPM_RobId).ToList();
                        if (allocations.Count > 0)
                        {
                            _DbContext.Allocation.RemoveRange(allocations);
                        }
                    }
                    _DbContext.Rob.RemoveRange(isrob);
                    _DbContext.Robs.Remove(isrobs);
                    Save();
                }
                else
                {
                    _DbContext.Robs.Remove(isrobs);
                    Save();
                }
            }
            var eventObj = _DbContext.EventROBsRow.Where(x => x.SFPM_EventROBsRowId == eventId).FirstOrDefault();
            if (eventObj != null)
            {
                eventObj.ModifiedBy = userId;
                eventObj.ModifiedDateTime = DateTime.UtcNow;
                _DbContext.EventROBsRow.Remove(eventObj);
                _DbContext.SaveChanges();
                return (int)ResponseStatus.SAVED;
            }
            return (int)ResponseStatus.NOTFOUND;
        }
        #endregion

        #region Fluid Consumption (Postion / Event)

        public IEnumerable<Rob> GetAllFluidConsumption(long formId,long loginUserId)
        {
            var userrole = _DbContext.Roles.Where(x => x.RoleId == (_DbContext.UserRoleMapping.Where(y => y.UserId == loginUserId).Select(y => y.RoleId).FirstOrDefault())).Select(x => x.RoleName).AsNoTracking().FirstOrDefault();
            var imonumberfromformid = _DbContext.Forms.Where(v => v.SFPM_Form_Id == formId).Select(v => v.ImoNumber.ToString()).FirstOrDefault();
            var imoNumbers = (
                              from u in _DbContext.UserVesselGroupMappings
                              join vu in _DbContext.VesselGroupVesselMapping on u.VesselGroupId equals vu.VesselGroupId
                              join vsl in _DbContext.Vessels on vu.VesselId equals vsl.SFPM_VesselId
                              where u.UserId == loginUserId
                              select vsl.IMONumber).Distinct();


            if (imoNumbers.Contains(imonumberfromformid) || userrole.ToLower() == Constant.administrator)
            {
                List<Rob> robList = new List<Rob>();
                var robs = _DbContext.Robs.Where(a => a.FormId == formId && a.EventRobsRowId == null).Include(x => x.Rob).ThenInclude(x => x.Allocation).FirstOrDefault();
                return (robs != null ? robs.Rob != null ? robs.Rob.Where(a => a.Allocation.Count > 0) : robList : robList).OrderByDescending(x => x.SFPM_RobId);

            }
            else
                return null;
        }
        public IEnumerable<Rob> GetAllEventFluidConsumption(long formId, long eventRobsRowId,long loginUserId)
        {
            var userrole = _DbContext.Roles.Where(x => x.RoleId == (_DbContext.UserRoleMapping.Where(y => y.UserId == loginUserId).Select(y => y.RoleId).FirstOrDefault())).Select(x => x.RoleName).AsNoTracking().FirstOrDefault();
            var imonumberfromformid = _DbContext.Forms.Where(v => v.SFPM_Form_Id == formId).Select(v => v.ImoNumber.ToString()).FirstOrDefault();
            var imoNumbers = (
                              from u in _DbContext.UserVesselGroupMappings
                              join vu in _DbContext.VesselGroupVesselMapping on u.VesselGroupId equals vu.VesselGroupId
                              join vsl in _DbContext.Vessels on vu.VesselId equals vsl.SFPM_VesselId
                              where u.UserId == loginUserId
                              select vsl.IMONumber).Distinct();


            if (imoNumbers.Contains(imonumberfromformid) || userrole.ToLower() == Constant.administrator)
            {
                List<Rob> robList = new List<Rob>();
                var robs = _DbContext.Robs.Where(a => a.FormId == formId && a.EventRobsRowId == eventRobsRowId).Include(x => x.Rob).ThenInclude(x => x.Allocation).FirstOrDefault();
                return (robs != null ? robs.Rob != null ? robs.Rob.Where(a => a.Allocation.Count > 0) : robList : robList).OrderByDescending(x => x.SFPM_RobId);

            }
            else
                return null;
        }
        public Rob GetFluidConsumptionById(long robId)
        {
            return _DbContext.Rob.Where(a => a.SFPM_RobId == robId).Include(a => a.Allocation).FirstOrDefault();
        }

        public long CreateFluidConsumption(FluidFuelConsumedViewModel fluidFuelConsumed, out long allocationId)
        {
            long userId = Convert.ToInt64(this._currentUser.UserId);
            fluidFuelConsumed.RobId = allocationId = 0;
            var robs = _DbContext.Robs.Where(a => a.FormId == fluidFuelConsumed.FormId && a.EventRobsRowId == null).FirstOrDefault();
            if (robs == null)
            {
                //create Robs if not exits
                robs = new Robs();
                robs.AsOfDate = DateTime.UtcNow;
                robs.FormId = fluidFuelConsumed.FormId;
                robs.CreatedBy = userId;
                robs.CreatedDateTime = DateTime.UtcNow;

                _DbContext.Robs.Add(robs);
                Save();
            }
            return CreateRob(fluidFuelConsumed, robs.SFPM_RobsId, ref allocationId);
        }
        public long CreateEventFluidConsumption(FluidFuelConsumedViewModel fluidFuelConsumed, out long allocationId)
        {
            long userId = Convert.ToInt64(this._currentUser.UserId);
            fluidFuelConsumed.RobId = allocationId = 0;
            var robs = _DbContext.Robs.Where(a => a.FormId == fluidFuelConsumed.FormId && a.EventRobsRowId == fluidFuelConsumed.EventRobsRowId).FirstOrDefault();
            if (robs == null)
            {
                //create Robs if not exits
                robs = new Robs();
                robs.AsOfDate = DateTime.UtcNow;
                robs.FormId = fluidFuelConsumed.FormId;
                robs.EventRobsRowId = fluidFuelConsumed.EventRobsRowId;
                robs.CreatedBy = userId;
                robs.CreatedDateTime = DateTime.UtcNow;

                _DbContext.Robs.Add(robs);
                Save();
            }
            return CreateRob(fluidFuelConsumed, robs.SFPM_RobsId, ref allocationId);
        }
        public int UpdateFluidConsumption(FluidFuelConsumedViewModel fluidFuelConsumed)
        {
            long userId = Convert.ToInt64(this._currentUser.UserId);
            var robexist = _DbContext.Rob.Where(x => x.SFPM_RobId == fluidFuelConsumed.RobId).FirstOrDefault();
            if (robexist == null)
            {
                return (int)ResponseStatus.NOTFOUND;
            }
            var robs = new Robs();
            if (fluidFuelConsumed.EventRobsRowId == 0 || fluidFuelConsumed.EventRobsRowId == null)
            {
                robs = _DbContext.Robs.Where(x => x.FormId == fluidFuelConsumed.FormId).FirstOrDefault();
                //var isRobduplicate = _DbContext.Rob.Where(x => x.SFPM_RobId != fluidFuelConsumed.RobId && x.RobsSFPM_RobsId == robs.SFPM_RobsId && x.FuelType.ToLower().Equals(fluidFuelConsumed.FluidType.ToLower())).FirstOrDefault();
                //if (isRobduplicate != null && Convert.ToDouble(isRobduplicate.Remaining) > 0)
                //{
                //    return (int)ResponseStatus.ALREADYEXIST;
                //}
            }
            else
            { //For Event Rob Duplicate Check Logic
                robs = _DbContext.Robs.Where(x => x.EventRobsRowId == fluidFuelConsumed.EventRobsRowId && x.FormId == fluidFuelConsumed.FormId).FirstOrDefault();
                //var isRobduplicate = _DbContext.Rob.Where(x => x.SFPM_RobId != fluidFuelConsumed.RobId && x.RobsSFPM_RobsId == robs.SFPM_RobsId && x.FuelType.ToLower().Equals(fluidFuelConsumed.FluidType.ToLower())).FirstOrDefault();
                //if (isRobduplicate != null && Convert.ToDouble(isRobduplicate.Remaining) > 0)
                //{
                //    return (int)ResponseStatus.ALREADYEXIST;
                //}
            }
            var rob = _DbContext.Rob.Where(x => x.RobsSFPM_RobsId == robs.SFPM_RobsId && x.FuelType.ToLower().Equals(fluidFuelConsumed.FluidType.ToLower())).FirstOrDefault();
            if (rob != null)
            {
                rob.FuelType = fluidFuelConsumed.FluidType;
                rob.Units = fluidFuelConsumed.Unit;
                rob.ModifiedBy = userId;
                rob.Remaining = rob.Remaining;
                rob.ModifiedDateTime = DateTime.UtcNow;

                _DbContext.Rob.Update(rob);
            }
            else
            {
                rob = new Rob();
                rob.FuelType = fluidFuelConsumed.FluidType;
                rob.Units = fluidFuelConsumed.Unit;
                rob.RobsSFPM_RobsId = robs.SFPM_RobsId;
                rob.ModifiedBy = userId;
                rob.Remaining = "0";
                rob.ModifiedDateTime = DateTime.UtcNow;
                _DbContext.Rob.Add(rob);
                Save();
            }
            var allocation = _DbContext.Allocation.Where(x => x.SFPM_AllocationId == fluidFuelConsumed.AllocationId).FirstOrDefault();
            if (allocation != null)
            {
                allocation.Name = fluidFuelConsumed.Category;
                allocation.text = fluidFuelConsumed.Consumption.ToString();
                allocation.RobSFPM_RobId = rob.SFPM_RobId;
                allocation.ModifiedBy = userId;
                allocation.ModifiedDateTime = DateTime.UtcNow;
                _DbContext.Allocation.Update(allocation);
            }

            Save();
            return (int)ResponseStatus.SAVED;
        }
        public int DeleteFluidConsumption(long robId, long allocationId)
        {
            long userId = Convert.ToInt64(this._currentUser.UserId);
            if (IsVoyageApproved(_DbContext.Robs.Where(x => x.SFPM_RobsId == (_DbContext.Rob.Where(y => y.SFPM_RobId == robId).Select(y => y.RobsSFPM_RobsId).FirstOrDefault())).Select(x => x.FormId).FirstOrDefault()))
            {
                return (int)ResponseStatus.ALREADYAPPROVED;
            }
            var chkRob = _DbContext.Rob.Where(x => x.SFPM_RobId == robId).SingleOrDefault();
            if (chkRob != null)
            {
                var chkAllocation = _DbContext.Allocation.Where(x => x.RobSFPM_RobId == robId && x.SFPM_AllocationId == allocationId).FirstOrDefault();
                if (chkAllocation != null)
                    _DbContext.Allocation.RemoveRange(chkAllocation);
                chkRob.ModifiedBy = userId;
                chkRob.ModifiedDateTime = DateTime.UtcNow;
                Save();
                var allocationList = _DbContext.Allocation.Where(x => x.RobSFPM_RobId == robId).ToList();
                if (allocationList.Count == 0 && chkRob.Remaining == "0")
                {
                    _DbContext.Rob.Remove(chkRob);
                    Save();
                }
                return (int)ResponseStatus.SAVED;
            }
            else
                return (int)ResponseStatus.NOTFOUND;

        }
        #endregion

        #region Bunker

        public IEnumerable<FluidBunkerViewModel> GetAllFluidBunker(long formId,long loginUserId)
        {
            List<FluidBunkerViewModel> fluidBunkerObjList = new List<FluidBunkerViewModel>();
            FluidBunkerViewModel fluidBunkerViewModel;
            //Get ROB bunker type

            var userrole = _DbContext.Roles.Where(x => x.RoleId == (_DbContext.UserRoleMapping.Where(y => y.UserId == loginUserId).Select(y => y.RoleId).FirstOrDefault())).Select(x => x.RoleName).AsNoTracking().FirstOrDefault();
            var imonumberfromformid = _DbContext.Forms.Where(v => v.SFPM_Form_Id == formId).Select(v => v.ImoNumber.ToString()).FirstOrDefault();
            var imoNumbers = (  
                              from u in _DbContext.UserVesselGroupMappings
                              join vu in _DbContext.VesselGroupVesselMapping on u.VesselGroupId equals vu.VesselGroupId
                              join vsl in _DbContext.Vessels on vu.VesselId equals vsl.SFPM_VesselId 
                              where u.UserId == loginUserId
                              select  vsl.IMONumber).Distinct();


            if (imoNumbers.Contains(imonumberfromformid) || userrole.ToLower() == Constant.administrator)
            {
                var robs = _DbContext.Robs.Where(a => a.FormId == formId && a.EventRobsRowId == null).Include(x => x.Rob).FirstOrDefault();
                if (robs != null)
                {
                    var robList = robs.Rob.Where(a => Convert.ToDecimal(a.Remaining) > 0).ToList();
                    foreach (var rob in robList)
                    {
                        fluidBunkerViewModel = new FluidBunkerViewModel();
                        fluidBunkerViewModel.FormId = formId;
                        fluidBunkerViewModel.RobId = rob.SFPM_RobId;
                        fluidBunkerViewModel.BunkerType = "ROB";
                        fluidBunkerViewModel.FluidType = rob.FuelType;
                        fluidBunkerViewModel.Unit = rob.Units;
                        fluidBunkerViewModel.Consumption = Convert.ToDecimal(rob.Remaining);
                        fluidBunkerObjList.Add(fluidBunkerViewModel);
                    }
                }

                //For Bunker Lifted
                var isFuelsRows = (from fuelsRows in _DbContext.FuelsRows
                                   where fuelsRows.FormId == formId
                                   select new
                                   {
                                       SFPM_FuelsRowsId = fuelsRows.SFPM_FuelsRowsId,
                                       FuelType = fuelsRows.FuelType != null ? fuelsRows.FuelType : "",
                                       QtyLifted = fuelsRows.QtyLifted != null ? fuelsRows.QtyLifted : 0
                                   }).ToList();

                _DbContext.FuelsRows.Where(a => a.FormId == formId).ToListAsync();
                if (isFuelsRows != null && isFuelsRows.Count > 0)
                {
                    foreach (var rob in isFuelsRows)
                    {
                        fluidBunkerViewModel = new FluidBunkerViewModel();
                        fluidBunkerViewModel.FormId = formId;
                        fluidBunkerViewModel.RobId = rob.SFPM_FuelsRowsId;
                        fluidBunkerViewModel.BunkerType = "Bunker Lifted";
                        fluidBunkerViewModel.FluidType = rob.FuelType;
                        fluidBunkerViewModel.Unit = "MT";
                        fluidBunkerViewModel.Consumption = rob.QtyLifted;
                        fluidBunkerObjList.Add(fluidBunkerViewModel);
                    }
                }
                return fluidBunkerObjList;
            }
            else
                return null;
        }

        public FluidBunkerViewModel GetFluidBunkerById(long robId, string bunkerType)
        {
            FluidBunkerViewModel fluidBunkerObj = new FluidBunkerViewModel();
            if (bunkerType == "ROB")
            {
                var rob = _DbContext.Rob.Where(a => a.SFPM_RobId == robId).FirstOrDefault();
                if (rob != null)
                {
                    fluidBunkerObj = new FluidBunkerViewModel();
                    fluidBunkerObj.RobId = rob.SFPM_RobId;
                    fluidBunkerObj.BunkerType = "ROB";
                    fluidBunkerObj.FluidType = rob.FuelType;
                    fluidBunkerObj.Unit = rob.Units;
                    fluidBunkerObj.Consumption = Convert.ToDecimal(rob.Remaining);
                }
            }
            else
            {
                var fuelsRows = _DbContext.FuelsRows.Where(a => a.SFPM_FuelsRowsId == robId).FirstOrDefault();
                if (fuelsRows != null)
                {
                    fluidBunkerObj = new FluidBunkerViewModel();
                    fluidBunkerObj.RobId = fuelsRows.SFPM_FuelsRowsId;
                    fluidBunkerObj.BunkerType = "Bunker Lifted";
                    fluidBunkerObj.FluidType = fuelsRows.FuelType;
                    fluidBunkerObj.Unit = "MT";
                    fluidBunkerObj.Consumption = fuelsRows.QtyLifted;
                }
            }
            return fluidBunkerObj;
        }
        public IEnumerable<FluidBunkerViewModel> GetAllEventFluidBunker(long formId, long eventRobsRowId,long loginUserId)
        {
            var userrole = _DbContext.Roles.Where(x => x.RoleId == (_DbContext.UserRoleMapping.Where(y => y.UserId == loginUserId).Select(y => y.RoleId).FirstOrDefault())).Select(x => x.RoleName).AsNoTracking().FirstOrDefault();
            var imonumberfromformid = _DbContext.Forms.Where(v => v.SFPM_Form_Id == formId).Select(v => v.ImoNumber.ToString()).FirstOrDefault();
            var imoNumbers = (
                              from u in _DbContext.UserVesselGroupMappings
                              join vu in _DbContext.VesselGroupVesselMapping on u.VesselGroupId equals vu.VesselGroupId
                              join vsl in _DbContext.Vessels on vu.VesselId equals vsl.SFPM_VesselId
                              where u.UserId == loginUserId
                              select vsl.IMONumber).Distinct();


            if (imoNumbers.Contains(imonumberfromformid) || userrole.ToLower() == Constant.administrator)
            {
                List<FluidBunkerViewModel> fluidBunkerObjList = new List<FluidBunkerViewModel>();
                FluidBunkerViewModel fluidBunkerViewModel;
                //Get ROB bunker type
                var robs = _DbContext.Robs.Where(a => a.FormId == formId && a.EventRobsRowId == eventRobsRowId).Include(x => x.Rob).FirstOrDefault();
                if (robs != null)
                {
                    var robList = robs.Rob.Where(a => Convert.ToDecimal(a.Remaining) > 0).ToList();
                    foreach (var rob in robList)
                    {
                        fluidBunkerViewModel = new FluidBunkerViewModel();
                        fluidBunkerViewModel.FormId = formId;
                        fluidBunkerViewModel.RobId = rob.SFPM_RobId;
                        fluidBunkerViewModel.EventROBsRowId = eventRobsRowId;
                        fluidBunkerViewModel.BunkerType = "ROB";
                        fluidBunkerViewModel.FluidType = rob.FuelType;
                        fluidBunkerViewModel.Unit = rob.Units;
                        fluidBunkerViewModel.Consumption = Convert.ToDecimal(rob.Remaining);
                        fluidBunkerObjList.Add(fluidBunkerViewModel);
                    }
                }
                return fluidBunkerObjList;
            }
            else
                return null;

               
        }

        public long CreateFluidBunker(FluidBunkerViewModel fluidBunker)
        {
            long userId = Convert.ToInt64(this._currentUser.UserId);
            if (IsVoyageApproved(Convert.ToInt64(fluidBunker.FormId)))
            {
                return (int)ResponseStatus.ALREADYAPPROVED;
            }
            if (fluidBunker.BunkerType == "ROB")
            {
                fluidBunker.RobId = 0;
                var robs = _DbContext.Robs.Where(a => a.FormId == fluidBunker.FormId && a.EventRobsRowId == null).FirstOrDefault();
                if (robs == null)
                {
                    //create Robs if not exits
                    robs = new Robs();
                    robs.AsOfDate = DateTime.UtcNow;
                    robs.FormId = fluidBunker.FormId;
                    robs.CreatedBy = userId;
                    robs.CreatedDateTime = DateTime.UtcNow;

                    _DbContext.Robs.Add(robs);
                    Save();
                }
                return CreateBunkerRob(fluidBunker, robs.SFPM_RobsId);
            }
            else
            {
                fluidBunker.RobId = 0;
                var isFuelsRows = _DbContext.FuelsRows.Where(a => a.FormId == fluidBunker.FormId && a.FuelType.ToLower().Equals(fluidBunker.FluidType.ToLower())).FirstOrDefault();
                if (isFuelsRows == null)
                {
                    //create Rob for Lifted bunker
                    isFuelsRows = new FuelsRows();
                    isFuelsRows.FuelType = fluidBunker.FluidType;
                    isFuelsRows.QtyLifted = Convert.ToInt32(fluidBunker.Consumption);
                    isFuelsRows.FormId = fluidBunker.FormId;
                    isFuelsRows.CreatedBy = userId;
                    isFuelsRows.CreatedDateTime = DateTime.UtcNow;
                    _DbContext.FuelsRows.Add(isFuelsRows);
                    Save();
                    return isFuelsRows.SFPM_FuelsRowsId;
                }
            }
            return 0;
        }
        public int UpdateFluidBunker(FluidBunkerViewModel fluidBunker)
        {
            long userId = Convert.ToInt64(this._currentUser.UserId);
            if (fluidBunker.BunkerType == "ROB")
            {
                if (fluidBunker.EventROBsRowId == 0 || fluidBunker.EventROBsRowId == null)
                {
                    var Robs = _DbContext.Robs.Where(x => x.FormId == fluidBunker.FormId).FirstOrDefault();
                    var isRobduplicate = _DbContext.Rob.Where(x => x.SFPM_RobId != fluidBunker.RobId && x.RobsSFPM_RobsId == Robs.SFPM_RobsId && x.FuelType.ToLower().Equals(fluidBunker.FluidType.ToLower())).FirstOrDefault();
                    if (isRobduplicate != null)
                    {
                        return (int)ResponseStatus.ALREADYEXIST;
                    }
                }
                else
                { //For Event Rob Duplicate Check Logic
                    var Robs = _DbContext.Robs.Where(x => x.EventRobsRowId == fluidBunker.EventROBsRowId && x.FormId == fluidBunker.FormId).FirstOrDefault();
                    var isRobduplicate = _DbContext.Rob.Where(x => x.SFPM_RobId != fluidBunker.RobId && x.RobsSFPM_RobsId == Robs.SFPM_RobsId && x.FuelType.ToLower().Equals(fluidBunker.FluidType.ToLower())).FirstOrDefault();
                    if (isRobduplicate != null)
                    {
                        return (int)ResponseStatus.ALREADYEXIST;
                    }
                }
                var rob = _DbContext.Rob.Where(x => x.SFPM_RobId == fluidBunker.RobId).FirstOrDefault();
                if (rob == null)
                {
                    return (int)ResponseStatus.NOTFOUND;
                }
                else
                {
                    rob.FuelType = fluidBunker.FluidType;
                    rob.Units = fluidBunker.Unit;
                    rob.Remaining = fluidBunker.Consumption.ToString();
                    rob.ModifiedBy = userId;
                    rob.ModifiedDateTime = DateTime.UtcNow;
                    _DbContext.Rob.Update(rob);
                }
            }
            else
            {
                var fuelsRows = _DbContext.FuelsRows.Where(x => x.SFPM_FuelsRowsId == fluidBunker.RobId).FirstOrDefault();
                if (fuelsRows == null)
                {
                    return (int)ResponseStatus.NOTFOUND;
                }
                var isRobduplicate = _DbContext.FuelsRows.Where(x => x.SFPM_FuelsRowsId != fluidBunker.RobId && x.FormId == fluidBunker.FormId && x.FuelType.ToLower().Equals(fluidBunker.FluidType.ToLower())).FirstOrDefault();
                if (isRobduplicate != null)
                {
                    return (int)ResponseStatus.ALREADYEXIST;
                }
                else
                {
                    fuelsRows.FuelType = fluidBunker.FluidType;
                    fuelsRows.QtyLifted = Convert.ToInt32(fluidBunker.Consumption);
                    fuelsRows.ModifiedBy = userId;
                    fuelsRows.ModifiedDateTime = DateTime.UtcNow;
                    _DbContext.FuelsRows.Update(fuelsRows);
                }
            }
            Save();
            return (int)ResponseStatus.SAVED;
        }
        public int DeleteFluidBunker(int formId, int robId, string bunkerType)
        {
            try
            {
                long userId = Convert.ToInt64(this._currentUser.UserId);
                if (bunkerType == "ROB")
                {
                    var chkRob = _DbContext.Rob.Where(x => x.SFPM_RobId == robId).FirstOrDefault();
                    if (chkRob != null)
                    {
                        chkRob.Remaining = "0";
                        chkRob.ModifiedBy = userId;
                        chkRob.ModifiedDateTime = DateTime.UtcNow;
                        _DbContext.Rob.Update(chkRob);
                        Save();
                        var allocationList = _DbContext.Allocation.Where(x => x.RobSFPM_RobId == robId).ToList();
                        if (allocationList.Count == 0 && chkRob.Remaining == "0")
                        {
                            _DbContext.Rob.Remove(chkRob);
                            Save();
                        }
                    }
                }
                else
                {
                    var isFuelsRows = _DbContext.FuelsRows.Where(x => x.SFPM_FuelsRowsId == robId).SingleOrDefault();
                    if (isFuelsRows != null)
                    {
                        _DbContext.FuelsRows.RemoveRange(isFuelsRows);
                        Save();
                    }
                }
                return 1;
            }
            catch (Exception)
            {

                return 0;
            }
           
        }

        public long CreateEventFluidBunker(FluidBunkerViewModel fluidBunker)
        {
            long userId = Convert.ToInt64(this._currentUser.UserId);
            if (fluidBunker.BunkerType == "ROB")
            {
                fluidBunker.RobId = 0;
                var robs = _DbContext.Robs.Where(a => a.FormId == fluidBunker.FormId && a.EventRobsRowId == fluidBunker.EventROBsRowId).FirstOrDefault();
                if (robs == null)
                {
                    //create Robs if not exits
                    robs = new Robs();
                    robs.AsOfDate = DateTime.UtcNow;
                    robs.FormId = fluidBunker.FormId;
                    robs.EventRobsRowId = fluidBunker.EventROBsRowId;
                    robs.CreatedBy = userId;
                    robs.CreatedDateTime = DateTime.UtcNow;
                    _DbContext.Robs.Add(robs);
                    Save();
                }
                return CreateBunkerRob(fluidBunker, robs.SFPM_RobsId);
            }
            return 0;
        }
        public IEnumerable<BunkerType> GetAllBunkerType()
        {
            return _DbContext.BunkerTypes.ToList();
        }

        #endregion

        #region Exclusion

        public IEnumerable<ExcludeReportLog> GetPassageReportExclusion(long voyageId,long loginUserId)
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
                var exclusionList = _DbContext.ExcludeReportLogs.Where(x => x.VoyagesId == voyageId).OrderByDescending(x => x.CreatedDateTime).ToList();
                exclusionList.ForEach(x => x.excludesList = _DbContext.ExcludeReports.Where(y => y.SFPM_ExcludeReportId == x.ReportId).ToList());
                return exclusionList;
            }
            else
                return null;
        }
        public IEnumerable<ExcludeReportLog> GetPositionReportExclusion(long formId)
        {
            var exclusionList = _DbContext.ExcludeReportLogs.Where(x => x.FormId == formId).OrderByDescending(x => x.CreatedDateTime).ToList();
            exclusionList.ForEach(x => x.excludesList = _DbContext.ExcludeReports.Where(y => y.SFPM_ExcludeReportId == x.ReportId).ToList());
            return exclusionList;

        }
        public IEnumerable<ExcludeReportLogs> GetPassageReportExclusionlog(long voyageId,long loginUserId)
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
        public IEnumerable<ExcludeReportLogs> GetPositionReportExclusionlog(long formId)
        {
            var data = (from l in _DbContext.ExcludeReportLogs
                        join r in _DbContext.ExcludeReports on l.ReportId equals r.SFPM_ExcludeReportId
                        join u in _DbContext.Users on l.UserId equals u.UserId
                        where l.FormId == formId
                        select new ExcludeReportLogs { ExcludeReportLogId = l.SFPM_ExcludeReportLogId, Excluded = l.Excluded, ReportName = r.ReportName, Username = u.Username, CreatedDateTime = l.CreatedDateTime, Remarks = l.Remarks }).ToList();

            IEnumerable<ExcludeReportLogs> log = data as IEnumerable<ExcludeReportLogs>;
            return log;
        }
        public List<ExcludeReportLog> GetPssgReptExclResponse(long voyagesId)
        {
            var lst1 = _DbContext.ExcludeReportLogs.Where(x => x.VoyagesId == voyagesId && x.ReportId == 1).OrderByDescending(x => x.CreatedDateTime).AsNoTracking().Take(1).ToList();
            var lst2 = _DbContext.ExcludeReportLogs.Where(x => x.VoyagesId == voyagesId && x.ReportId == 2).OrderByDescending(x => x.CreatedDateTime).AsNoTracking().Take(1).ToList();
            return lst1.Concat(lst2).ToList();
        }
        public List<ExcludeReportLog> GetPostReptExclResponse(long formId)
        {
            var lst1 = _DbContext.ExcludeReportLogs.Where(x => x.VoyagesId == formId && x.ReportId == 1).OrderByDescending(x => x.CreatedDateTime).AsNoTracking().Take(1).ToList();
            var lst2 = _DbContext.ExcludeReportLogs.Where(x => x.VoyagesId == formId && x.ReportId == 2).OrderByDescending(x => x.CreatedDateTime).AsNoTracking().Take(1).ToList();
            return lst1.Concat(lst2).ToList();
        }
        public int CreatePassageReportExclusion(ExcludeReportLog exclude)
        {
            ExcludeReportLog ex = null;
            bool flag = false;
            bool expRpt1 = false, expRpt2 = false;
            long userId = Convert.ToInt64(this._currentUser.UserId);
            var Rpt1 = _DbContext.ExcludeReportLogs.Where(x => x.VoyagesId == exclude.VoyagesId && x.ReportId == 1).OrderByDescending(x => x.CreatedDateTime).AsNoTracking().FirstOrDefault();
            var Rpt2 = _DbContext.ExcludeReportLogs.Where(x => x.VoyagesId == exclude.VoyagesId && x.ReportId == 2).OrderByDescending(x => x.CreatedDateTime).AsNoTracking().FirstOrDefault();
            if (Rpt1 != null && Rpt2 != null)
            {
                foreach (var item in exclude.excludesList)
                {
                    if (item.SFPM_ExcludeReportId == 1)
                    {
                        if (Convert.ToBoolean(item.Status) == Rpt1.Excluded)
                            expRpt1 = true;
                    }
                    if (item.SFPM_ExcludeReportId == 2)
                    {
                        if (Convert.ToBoolean(item.Status) == Rpt2.Excluded)
                            expRpt2 = true;
                    }
                }
            }
            else
                flag = true;
            if (expRpt1 == true && expRpt2 == true)
                flag = false;
            else
                flag = true;

            if (flag == true)
            {
                foreach (var item in exclude.excludesList)
                {
                    if (item.SFPM_ExcludeReportId == 1)
                    {
                        if (expRpt1 != true)
                        {
                            ex = new ExcludeReportLog();
                            ex.UserId = exclude.UserId;
                            ex.VoyagesId = exclude.VoyagesId;
                            ex.FormId = exclude.FormId;
                            ex.Excluded = Convert.ToBoolean(item.Status);
                            ex.Remarks = exclude.Remarks;
                            ex.CreatedBy = userId;
                            ex.CreatedDateTime = DateTime.UtcNow;
                            ex.ReportId = item.SFPM_ExcludeReportId;
                            _DbContext.ExcludeReportLogs.Add(ex);
                            Save();
                        }
                    }
                    if (item.SFPM_ExcludeReportId == 2)
                    {
                        if (expRpt2 != true)
                        {
                            ex = new ExcludeReportLog();
                            ex.UserId = exclude.UserId;
                            ex.VoyagesId = exclude.VoyagesId;
                            ex.FormId = exclude.FormId;
                            ex.Excluded = Convert.ToBoolean(item.Status);
                            ex.Remarks = exclude.Remarks;
                            ex.CreatedBy = userId;
                            ex.CreatedDateTime = DateTime.UtcNow;
                            ex.ReportId = item.SFPM_ExcludeReportId;
                            _DbContext.ExcludeReportLogs.Add(ex);
                            Save();
                        }
                    }

                }
                return (int)ResponseStatus.SAVED;
            }
            else
                return 0;
        }

        public int CreatePositionReportExclusion(ExcludeReportLog exclude)
        {
            ExcludeReportLog ex = null;
            bool flag = false;
            bool expRpt1 = false, expRpt2 = false;
            long userId = Convert.ToInt64(this._currentUser.UserId);
            var Rpt1 = _DbContext.ExcludeReportLogs.Where(x => x.FormId == exclude.FormId && x.ReportId == 1).OrderByDescending(x => x.CreatedDateTime).AsNoTracking().FirstOrDefault();
            var Rpt2 = _DbContext.ExcludeReportLogs.Where(x => x.FormId == exclude.FormId && x.ReportId == 2).OrderByDescending(x => x.CreatedDateTime).AsNoTracking().FirstOrDefault();
            if (Rpt1 != null && Rpt2 != null)
            {
                foreach (var item in exclude.excludesList)
                {
                    if (item.SFPM_ExcludeReportId == 1)
                    {
                        if (Convert.ToBoolean(item.Status) == Rpt1.Excluded)
                            expRpt1 = true;
                    }
                    if (item.SFPM_ExcludeReportId == 2)
                    {
                        if (Convert.ToBoolean(item.Status) == Rpt2.Excluded)
                            expRpt2 = true;
                    }
                }
            }
            else
                flag = true;
            if (expRpt1 == true && expRpt2 == true)
                flag = false;
            else
                flag = true;

            if (flag == true)
            {
                foreach (var item in exclude.excludesList)
                {
                    if (item.SFPM_ExcludeReportId == 1)
                    {
                        if (expRpt1 != true)
                        {
                            ex = new ExcludeReportLog();
                            ex.UserId = Convert.ToInt64(_currentUser.UserId);
                            ex.VoyagesId = exclude.VoyagesId;
                            ex.FormId = exclude.FormId;
                            ex.Excluded = Convert.ToBoolean(item.Status);
                            ex.Remarks = exclude.Remarks;
                            ex.CreatedBy = userId;
                            ex.CreatedDateTime = DateTime.UtcNow;
                            ex.ReportId = item.SFPM_ExcludeReportId;
                            _DbContext.ExcludeReportLogs.Add(ex);
                            Save();
                        }

                    }
                    if (item.SFPM_ExcludeReportId == 2)
                    {
                        if (expRpt2 != true)
                        {
                            ex = new ExcludeReportLog();
                            ex.UserId = Convert.ToInt64(_currentUser.UserId);
                            ex.VoyagesId = exclude.VoyagesId;
                            ex.FormId = exclude.FormId;
                            ex.Excluded = Convert.ToBoolean(item.Status);
                            ex.Remarks = exclude.Remarks;
                            ex.CreatedBy = Convert.ToInt64(_currentUser.UserId);
                            ex.CreatedDateTime = DateTime.UtcNow;
                            ex.ReportId = item.SFPM_ExcludeReportId;
                            _DbContext.ExcludeReportLogs.Add(ex);
                            Save();
                        }

                    }

                }
                return (int)ResponseStatus.SAVED;
            }
            else
                return 0;
        }
        #endregion

        #region Others

        public bool CheckFormExists(long formId)
        {
            return _DbContext.Forms.Where(x => x.SFPM_Form_Id == formId).Any();
        }
        public bool CheckEventExists(long formId, long eventId)
        {
            return _DbContext.EventROBsRow.Where(x => x.SFPM_EventROBsRowId == eventId && x.FormId == formId).Any();
        }
        public int SavePageSettings(long userId, bool isPassageUTC, bool isPositionUTC)
        {
            var pageSettings = _DbContext.PageSettings.Where(setting => setting.UserId == userId).FirstOrDefault();
            long logedUserId = Convert.ToInt64(this._currentUser.UserId);
            if (pageSettings != null)
            {
                pageSettings.IsPassageUTC = isPassageUTC;
                pageSettings.IsPositionUTC = isPositionUTC;
                pageSettings.ModifiedBy = logedUserId;
                pageSettings.ModifiedDateTime = DateTime.UtcNow;
                _DbContext.PageSettings.Update(pageSettings);
            }
            else
            {
                pageSettings = new PageSettings();
                pageSettings.IsPassageUTC = isPassageUTC;
                pageSettings.IsPositionUTC = isPositionUTC;
                pageSettings.UserId = userId;
                pageSettings.CreatedBy = logedUserId;
                pageSettings.CreatedDateTime = DateTime.UtcNow;
                _DbContext.PageSettings.Add(pageSettings);
            }
            return Save();
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

        public string GetVesselOwnerNameByImoNumber(string imoNumber)
        {
            string vesselOwnerName = string.Empty;
            Vessel vessel = _DbContext.Vessels.Where(x => x.IMONumber == imoNumber).FirstOrDefault();
            if (vessel != null)
            {
                User user = _DbContext.Users.Where(x => x.UserId == vessel.VesselOwnerId).FirstOrDefault();
                if (user != null)
                    vesselOwnerName = user.FirstName + " " + user.LastName;
            }
            return vesselOwnerName;
        }

        #endregion

        #region ExportVoyage
        public IEnumerable<Voyages> ExportGetVoyages(string voyageId)
        {
            List<string> lstVoyageId = voyageId.Split(',').ToList();
            List<Voyages> lstVoyage = new List<Voyages>();

            for (int i = 0; i < lstVoyageId.Count; i++)
            {
                List<Voyages> lst = _DbContext.Voyages.Where(x => x.SFPM_VoyagesId == Convert.ToInt64(lstVoyageId[i])).ToList();
                lstVoyage.AddRange(lst);
                if (i == lstVoyageId.Count - 1)
                    return lstVoyage;
            }
            return null;
        }
        #endregion

        public int Save()
        {
            return _DbContext.SaveChanges();
        }

        #endregion

        #region Private Methods
        private long CreateRob(FluidFuelConsumedViewModel fluidFuelConsumed, long robsId, ref long allocationId)
        {
            long userId = Convert.ToInt64(this._currentUser.UserId);
            long id = 0;
            allocationId = 0;
            var rob = _DbContext.Rob.Where(x => x.RobsSFPM_RobsId == robsId && x.FuelType.ToLower().Equals(fluidFuelConsumed.FluidType)).SingleOrDefault();
            if (rob != null)
            {
                var allocation = _DbContext.Allocation.Where(x => x.RobSFPM_RobId == rob.SFPM_RobId && x.Name.ToLower() == fluidFuelConsumed.Category.ToLower()).SingleOrDefault();
                if (allocation == null)
                {
                    //create only allocation if rob exists
                    List<Allocation> allocationList = new List<Allocation>();
                    Allocation allocationObj = new Allocation();
                    allocationObj.Name = fluidFuelConsumed.Category;
                    allocationObj.text = fluidFuelConsumed.Consumption.ToString();
                    allocationObj.CreatedBy = userId;
                    allocationObj.CreatedDateTime = DateTime.UtcNow;
                    allocationList.Add(allocationObj);
                    rob.Allocation = allocationList;
                    rob.ModifiedBy = userId;
                    rob.ModifiedDateTime = DateTime.UtcNow;
                    _DbContext.Rob.Update(rob);
                    Save();
                    id = rob.SFPM_RobId;
                    allocationId = rob.Allocation.OrderByDescending(a => a.SFPM_AllocationId).FirstOrDefault().SFPM_AllocationId;
                }
                else
                    allocationId = allocation.SFPM_AllocationId;
            }
            else
            {
                //create Rob if not exists
                Rob robObj = new Rob();
                robObj.FuelType = fluidFuelConsumed.FluidType;
                robObj.Units = fluidFuelConsumed.Unit;
                robObj.RobsSFPM_RobsId = robsId;
                robObj.CreatedBy = userId;
                robObj.CreatedDateTime = DateTime.UtcNow;

                List<Allocation> allocationList = new List<Allocation>();
                Allocation allocationObj = new Allocation();
                allocationObj.Name = fluidFuelConsumed.Category;
                allocationObj.text = fluidFuelConsumed.Consumption.ToString();
                allocationObj.CreatedBy = userId;
                allocationObj.CreatedDateTime = DateTime.UtcNow;
                allocationList.Add(allocationObj);
                robObj.Allocation = allocationList;

                _DbContext.Rob.Add(robObj);
                Save();
                id = robObj.SFPM_RobId;
                allocationId = robObj.Allocation.OrderByDescending(a => a.SFPM_AllocationId).FirstOrDefault().SFPM_AllocationId;
            }
            return id;
        }
        private long CreateBunkerRob(FluidBunkerViewModel fluidBunker, long robsId)
        {
            long userId = Convert.ToInt64(this._currentUser.UserId);
            long id = 0;
            var rob = _DbContext.Rob.Where(x => x.RobsSFPM_RobsId == robsId && x.FuelType.ToLower().Equals(fluidBunker.FluidType.ToLower())).SingleOrDefault();
            if (rob == null)
            {
                //create Rob if not exists
                Rob robObj = new Rob();
                robObj.FuelType = fluidBunker.FluidType;
                robObj.Units = fluidBunker.Unit;
                robObj.RobsSFPM_RobsId = robsId;
                robObj.Remaining = fluidBunker.Consumption.ToString();
                robObj.CreatedBy = userId;
                robObj.CreatedDateTime = DateTime.UtcNow;
                _DbContext.Rob.Add(robObj);
                Save();
                id = robObj.SFPM_RobId;
            }
            else
            {
                rob.Units = fluidBunker.Unit;
                rob.Remaining = fluidBunker.Consumption.ToString();
                rob.ModifiedBy = userId;
                rob.ModifiedDateTime = DateTime.UtcNow;
                _DbContext.Rob.Update(rob);
                Save();
                id = rob.SFPM_RobId;
            }
            return id;
        }
        private int DeletePositionRecord(string formIds)
        {
            List<string> formIdList = formIds.Split(',').ToList();
            foreach (string formId in formIdList)
            {

                if (IsVoyageApproved(Convert.ToInt64(formId)))
                {
                    return (int)ResponseStatus.ALREADYAPPROVED;
                }

                var position = _DbContext.Forms.Where(x => x.SFPM_Form_Id == Convert.ToInt64(formId)).FirstOrDefault();
                if (position != null)
                {
                    var robs = _DbContext.Robs.Where(x => x.FormId == Convert.ToInt64(formId)).Include(x => x.Rob).ToList();
                    var isPositionWarnings = _DbContext.PositionWarnings.Where(x => x.FormId == Convert.ToInt64(formId)).ToList();
                    foreach (var warningUdits in isPositionWarnings)
                    {
                        _DbContext.PositionWarningAudits.RemoveRange(_DbContext.PositionWarningAudits.Where(x => x.PositionWarningId == warningUdits.SFPM_PositionWarningId));
                    }
                    _DbContext.PositionWarnings.RemoveRange(isPositionWarnings);
                    foreach (var robsObj in robs)
                    {
                        foreach (var robObj in robsObj.Rob)
                        {
                            _DbContext.Allocation.RemoveRange(_DbContext.Allocation.Where(x => x.RobSFPM_RobId == robObj.SFPM_RobId));
                        }
                        _DbContext.Rob.RemoveRange(robsObj.Rob);
                    }
                    _DbContext.Robs.RemoveRange(robs);
                    _DbContext.EventROBsRow.RemoveRange(_DbContext.EventROBsRow.Where(x => x.FormId == position.SFPM_Form_Id));
                    _DbContext.FormUnits.RemoveRange(_DbContext.FormUnits.Where(x => x.FormId == position.SFPM_Form_Id));
                    _DbContext.FuelsRows.RemoveRange(
                        (from fuelsRows in _DbContext.FuelsRows
                         where fuelsRows.FormId == position.SFPM_Form_Id
                         select new FuelsRows
                         {
                             SFPM_FuelsRowsId = fuelsRows.SFPM_FuelsRowsId,
                             FuelType = fuelsRows.FuelType != null ? fuelsRows.FuelType : "",
                             QtyLifted = fuelsRows.QtyLifted != null ? fuelsRows.QtyLifted : 0,
                             BDN_Number = fuelsRows.BDN_Number != null ? fuelsRows.BDN_Number : "",
                             FormId = fuelsRows.FormId != null ? fuelsRows.FormId : 0,
                             Fuel_Densitry = fuelsRows.Fuel_Densitry != null ? fuelsRows.Fuel_Densitry : 0
                         }).ToList()
                        );
                    _DbContext.AnalyzedWeather.RemoveRange(_DbContext.AnalyzedWeather.Where(x => x.FormId == position.SFPM_Form_Id));
                    _DbContext.ExcludeReportLogs.RemoveRange(_DbContext.ExcludeReportLogs.Where(x => x.FormId == position.SFPM_Form_Id));
                    _DbContext.Forms.Remove(position);

                }
                //CreatePositionWarning(position);
            }
            Save();
            if (formIdList.Count > 0)
            {
                var forms = _DbContext.Forms.Where(x => x.SFPM_Form_Id == Convert.ToInt64(formIdList.FirstOrDefault())).FirstOrDefault();
                if (forms != null)
                {
                    var voyageId = (from form in _DbContext.Forms
                                    from voyage in _DbContext.Voyages
                                    where voyage.VoyageNumber == forms.VoyageNo
                                    && voyage.ActualStartOfSeaPassage <= forms.ReportDateTime.Value.DateTime
                                    && voyage.ActualEndOfSeaPassage >= forms.ReportDateTime.Value.DateTime
                                    select new { voyage.SFPM_VoyagesId }).FirstOrDefault();
                    if (voyageId != null)
                    {
                        CreatePassageWarning(_DbContext.Voyages.Where(x => x.SFPM_VoyagesId == voyageId.SFPM_VoyagesId).FirstOrDefault());
                    }
                }
            }
            return (int)ResponseStatus.SAVED;
        }
        public bool IsVoyageApproved(long formId)
        {
            var forms = _DbContext.Forms.Where(x => x.SFPM_Form_Id == formId).AsNoTracking().FirstOrDefault();
            var imonumber = forms != null ? forms.ImoNumber.ToString() : "0";
            var formdatetime = forms.ReportDateTime.Value.DateTime;

            var voyageId = (from voyage in _DbContext.Voyages
                            where voyage.IMONumber == imonumber
                            && voyage.ActualStartOfSeaPassage <= formdatetime
                            && voyage.ActualEndOfSeaPassage >= formdatetime
                            && forms.SFPM_Form_Id == formId
                            select new { voyage.SFPM_VoyagesId }).FirstOrDefault();
            if (voyageId != null)
            {
                PassagesApprovalAudits auditData = _DbContext.PassagesApprovalAudits.Where(x => x.VoyagesId == voyageId.SFPM_VoyagesId).OrderByDescending(x => x.ApprovalDateTime).FirstOrDefault();
                if (auditData != null)
                {
                    if (auditData.IsInitialApproved || auditData.IsFinalApproved)
                        return true;
                }
            }
            return false;
        }
        #endregion

        #region Warning

        private void CreatePassageWarning(Voyages voyage)
        {
            long userId = Convert.ToInt64(this._currentUser.UserId);
            PassageWarning passageWarnings;
            List<Forms> positionReportList = new List<Forms>();
            var passageWarningList = _DbContext.PassageWarnings.Where(x => x.VoyageId == voyage.SFPM_VoyagesId).ToList();
            foreach (var passagewarning in passageWarningList)
            {
                var warningAudit = _DbContext.PassageWarningAudits.Where(x => x.PassageWarningId == passagewarning.SFPM_PassageWarningId).OrderByDescending(x => x.ReviewDateTime).FirstOrDefault();
                if (warningAudit != null && !warningAudit.IsApproved)
                {
                    _DbContext.PassageWarningAudits.RemoveRange(_DbContext.PassageWarningAudits.Where(x => x.PassageWarningId == passagewarning.SFPM_PassageWarningId).ToList());
                    _DbContext.PassageWarnings.Remove(passagewarning);
                }
                else if (warningAudit == null)
                {
                    _DbContext.PassageWarnings.Remove(passagewarning);
                }
            }
            Int64 imonumber = Convert.ToInt64(voyage.IMONumber);
            if (voyage.ActualStartOfSeaPassage != null && voyage.ActualEndOfSeaPassage != null)
            {
                positionReportList = _DbContext.Forms.Where(x => x.ReportDateTime >= voyage.ActualStartOfSeaPassage && x.ReportDateTime <= voyage.ActualEndOfSeaPassage && x.ImoNumber == imonumber).ToList();
            }
            else if (voyage.ActualStartOfSeaPassage != null)
            {
                positionReportList = _DbContext.Forms.Where(x => x.ReportDateTime >= voyage.ActualStartOfSeaPassage && x.ImoNumber == imonumber).ToList();
            }
            Voyages duplicateVoyage = _DbContext.Voyages.Where(x => x.VoyageNumber == voyage.VoyageNumber && x.IMONumber == voyage.IMONumber && x.ActualStartOfSeaPassage == voyage.ActualStartOfSeaPassage && x.SFPM_VoyagesId != voyage.SFPM_VoyagesId).FirstOrDefault();
            if (duplicateVoyage != null)
            {
                passageWarnings = new PassageWarning();
                passageWarnings.WarningText = "Duplicate data , record already exists";
                passageWarnings.VoyageId = voyage.SFPM_VoyagesId;
                passageWarnings.CreatedBy = userId;
                passageWarnings.CreatedDateTime = DateTime.UtcNow;
                _DbContext.PassageWarnings.Add(passageWarnings);
            }
            if (!positionReportList.Exists(x => x.FormIdentifier.Contains("Departure")))
            {
                passageWarnings = new PassageWarning();
                passageWarnings.WarningText = "Departure report does not exist";
                passageWarnings.VoyageId = voyage.SFPM_VoyagesId;
                passageWarnings.CreatedBy = userId;
                passageWarnings.CreatedDateTime = DateTime.UtcNow;
                _DbContext.PassageWarnings.Add(passageWarnings);
            }
            if (!positionReportList.Exists(x => x.FormIdentifier.Contains("Arrival")))
            {
                passageWarnings = new PassageWarning();
                passageWarnings.WarningText = "Arrival report does not exist";
                passageWarnings.VoyageId = voyage.SFPM_VoyagesId;
                passageWarnings.CreatedBy = userId;
                passageWarnings.CreatedDateTime = DateTime.UtcNow;
                _DbContext.PassageWarnings.Add(passageWarnings);
            }
            var previousVooyageArrivalReport = _DbContext.Forms.Where(x => x.ImoNumber == Convert.ToInt64(voyage.IMONumber)
            && x.ReportDateTime.Value.DateTime < voyage.ActualStartOfSeaPassage
            && x.VoyageNo == (_DbContext.Voyages.Where(y => y.ActualStartOfSeaPassage < voyage.ActualStartOfSeaPassage && y.IMONumber == voyage.IMONumber)
            .OrderByDescending(y => y.ActualStartOfSeaPassage).Select(y => y.VoyageNumber).FirstOrDefault()))
           .OrderByDescending(a => a.ReportDateTime).ToList();
            if (previousVooyageArrivalReport != null)
            {
                if (previousVooyageArrivalReport.Exists(x => x.FormIdentifier.Contains("Arrival")))
                {
                    passageWarnings = new PassageWarning();
                    passageWarnings.WarningText = "Arrival report datetime mismatch";
                    passageWarnings.VoyageId = voyage.SFPM_VoyagesId;
                    passageWarnings.CreatedBy = userId;
                    passageWarnings.CreatedDateTime = DateTime.UtcNow;
                    _DbContext.PassageWarnings.Add(passageWarnings);
                }
            }
            var currentVoyageDepartureReport = positionReportList.Where(x => x.FormIdentifier.Contains("Departure")).OrderByDescending(a => a.ReportDateTime).FirstOrDefault();
            if (currentVoyageDepartureReport != null && currentVoyageDepartureReport.ReportDateTime.Value.DateTime != voyage.ActualStartOfSeaPassage)
            {
                passageWarnings = new PassageWarning();
                passageWarnings.WarningText = "No departure report found";
                passageWarnings.VoyageId = voyage.SFPM_VoyagesId;
                passageWarnings.CreatedBy = userId;
                passageWarnings.CreatedDateTime = DateTime.UtcNow;
                _DbContext.PassageWarnings.Add(passageWarnings);
            }
            var currentVoyageArrivalReport = positionReportList.Where(x => x.FormIdentifier.Contains("Arrival")).OrderByDescending(a => a.ReportDateTime).FirstOrDefault();
            if (currentVoyageArrivalReport != null && currentVoyageArrivalReport.ReportDateTime.Value.DateTime != voyage.ActualEndOfSeaPassage)
            {
                passageWarnings = new PassageWarning();
                passageWarnings.WarningText = "Arrival report date/time mismatch";
                passageWarnings.VoyageId = voyage.SFPM_VoyagesId;
                passageWarnings.CreatedBy = userId;
                passageWarnings.CreatedDateTime = DateTime.UtcNow;
                _DbContext.PassageWarnings.Add(passageWarnings);
            }
            Save();
        }

        private int CreatePositionWarning(Forms forms)
        {
            long userId = Convert.ToInt64(this._currentUser.UserId);
            var positionWarningList = _DbContext.PositionWarnings.Where(x => x.FormId == forms.SFPM_Form_Id).ToList();
            foreach (var positionwarning in positionWarningList)
            {
                var warningAudit = _DbContext.PositionWarningAudits.Where(x => x.PositionWarningId == positionwarning.SFPM_PositionWarningId).OrderByDescending(x => x.ReviewDateTime).FirstOrDefault();
                if (warningAudit != null && !warningAudit.IsApproved)
                {
                    _DbContext.PositionWarningAudits.RemoveRange(_DbContext.PositionWarningAudits.Where(x => x.PositionWarningId == positionwarning.SFPM_PositionWarningId).ToList());
                    _DbContext.PositionWarnings.Remove(positionwarning);
                }
                else if (warningAudit == null)
                {
                    _DbContext.PositionWarnings.Remove(positionwarning);
                }
            }

            Forms formSearchResult = _DbContext.Forms.Where(x => x.ReportDateTime.Value.DateTime == forms.ReportDateTime.Value.DateTime && x.SFPM_Form_Id != forms.SFPM_Form_Id && x.ImoNumber == forms.ImoNumber).AsNoTracking().FirstOrDefault();
            if (formSearchResult != null)
            {
                PositionWarning positionWarning = new PositionWarning();
                positionWarning.FormId = formSearchResult.SFPM_Form_Id;
                positionWarning.WarningText = "Duplicate data , record already exists";
                positionWarning.CreatedBy = userId;
                positionWarning.CreatedDateTime = DateTime.UtcNow;
                _DbContext.PositionWarnings.Add(positionWarning);
            }
            var previousforms = _DbContext.Forms.Where(x => x.ImoNumber == forms.ImoNumber && x.VoyageNo == forms.VoyageNo && x.ReportDateTime.Value.DateTime < forms.ReportDateTime.Value.DateTime)
           .OrderByDescending(a => a.ReportDateTime).FirstOrDefault();
            if (previousforms != null)
            {
                decimal formsTimeOffset = Convert.ToInt64(forms.ReportTime.Substring(1, 2).ToString());
                decimal lastReportTimeOffset = Convert.ToInt64(previousforms.ReportTime.Substring(1, 2));
                if ((formsTimeOffset - lastReportTimeOffset) > 1 && (formsTimeOffset != 12 && lastReportTimeOffset != -11 || formsTimeOffset != -11 && lastReportTimeOffset != 12))
                {
                    PositionWarning positionWarning = new PositionWarning();
                    positionWarning.FormId = forms.SFPM_Form_Id;
                    positionWarning.WarningText = "Time zone change more than 1 hours between 2 reports";
                    positionWarning.CreatedBy = userId;
                    positionWarning.CreatedDateTime = DateTime.UtcNow;
                    _DbContext.PositionWarnings.Add(positionWarning);
                }
            }
            // alerts 3 
            var hrs = Convert.ToDouble(forms.ReportTime.Substring(1, 2));
            var min = Convert.ToDouble(forms.ReportTime.Substring(4, 2));
            DateTime utcReportDateTime = forms.ReportTime.Contains("-") ? forms.ReportDateTime.Value.DateTime.AddHours(-hrs).AddMinutes(-min) :
            forms.ReportDateTime.Value.DateTime.AddHours(-hrs).AddMinutes(-min);
            var stratumDataList = new List<MeteoStratumData>();
            if (forms.FormIdentifier.Contains("Departure"))
            {
                stratumDataList = _DbContext.MeteoStratumData.Where(x => x.IMONumber == forms.ImoNumber.ToString() && x.GPSTimeStamp < utcReportDateTime.AddHours(1)
               && x.GPSTimeStamp > utcReportDateTime).OrderByDescending(a => a.GPSTimeStamp).ToList();
            }
            else
            {
                stratumDataList = _DbContext.MeteoStratumData.Where(x => x.IMONumber == forms.ImoNumber.ToString() && x.GPSTimeStamp < utcReportDateTime.AddHours(1)
               && x.GPSTimeStamp > utcReportDateTime.AddHours(-1))
                                  .OrderByDescending(a => a.GPSTimeStamp).ToList();
            }
            double formlat = ConvertDegreeAngleToDouble(forms.Latitude);
            double formlog = ConvertDegreeAngleToDouble(forms.Longitude);
            var stratumposition = _DbContext.UnitOfMeasureThresholds.Select(x => x.SpeedThreasholdForPositionWarningDistanceAPI).FirstOrDefault();
            foreach (var stratumdata in stratumDataList)
            {
                double lat = ConvertDegreeAngleToDouble(stratumdata.Latitude);
                double log = ConvertDegreeAngleToDouble(stratumdata.Longitude);
                var distance = GetPositionWarningDistanceValue(formlat, formlog, lat, log, forms.SFPM_Form_Id);  //GetPositionWarningDistanceValue(18.9, 72.81666666666666,13.1,80.3);
                var timeDifference = (utcReportDateTime > stratumdata.GPSTimeStamp) ? Convert.ToDecimal(utcReportDateTime.Subtract(Convert.ToDateTime(stratumdata.GPSTimeStamp.ToString())).TotalHours) :
                    Convert.ToDecimal(Convert.ToDateTime(stratumdata.GPSTimeStamp.ToString()).Subtract(utcReportDateTime).TotalHours);
                if (stratumposition < (distance / timeDifference))
                {
                    PositionWarning positionWarning = new PositionWarning();
                    positionWarning.FormId = forms.SFPM_Form_Id;
                    positionWarning.WarningText = "Search for stratum positions within +/-1 hr if the distance between ( stratum positions and Veslink report position ) "
                                                    + "them is more than “X” Nautical miles then error message as per below “You submitted a noon report "
                                                    + " for " + utcReportDateTime + " UTC at position " + stratumdata.Latitude + " " + stratumdata.Longitude + " We compared your report against an "
                                                    + " automated data source(e.g., Satellite AIS, Inmarsat C polling, etc.) showing that on " + utcReportDateTime + " UTC "
                                                    + " you were in position " + stratumdata.Latitude + " " + stratumdata.Longitude + " Based upon the distance and time between these two points, "
                                                    + " the speed over ground between them would have to be " + stratumposition + " kts.Please check your reported values and "
                                                    + " if necessary, make any corrections and resubmit your report.Thank you.”  ";
                    positionWarning.CreatedBy = userId;
                    positionWarning.CreatedDateTime = DateTime.UtcNow;
                    _DbContext.PositionWarnings.Add(positionWarning);
                }

            }


            Save();
            return 0;
        }

        private string SetTimezoneFormate(string timeZone)
        {
            if (timeZone != null)
            {
                if (timeZone.Length == 6 && !timeZone.Contains("-"))
                    return timeZone;
                if (timeZone.Length == 6 && !timeZone.Contains("+"))
                    return timeZone;
                int min, sec;
                string[] parts;
                if (timeZone.Contains(":"))
                    parts = timeZone.Split(':');
                else
                    parts = timeZone.Split('.');
                if (parts.Length == 2)
                {
                    min = int.Parse(parts[0]);
                    sec = int.Parse(parts[1]);
                }
                else
                {
                    min = int.Parse(parts[0]);
                    sec = 0;
                }
                if (min < 0)
                    if (min < 0 && min > -10 && sec < 10)
                        timeZone = "-0" + Math.Abs(min) + ":" + sec + "0";
                    else if (min < 0 && min > -10 && sec > 9)
                        timeZone = "-0" + Math.Abs(min) + ":" + sec;
                    else
                    {
                        if (sec < 10)
                            timeZone = "-" + Math.Abs(min) + ":0" + sec;
                        else
                            timeZone = "-" + Math.Abs(min) + ":" + sec;
                    }
                else
                {
                    if (min < 10 && sec < 10)
                        timeZone = "+0" + min + ":" + sec + "0";
                    else if (min < 10 && sec > 9)
                        timeZone = "+0" + min + ":" + sec;
                    else
                    {
                        if (sec < 10)
                            timeZone = "+" + min + ":0" + sec;
                        else
                            timeZone = "+" + min + ":" + sec;
                    }
                }

            }
            return timeZone;
        }

        public double ConvertDegreeAngleToDouble(string point)
        {
            var pointArray = point.TrimStart().Split('°'); //split the string.
            var degrees = Double.Parse(pointArray[0]);
            var min = pointArray[1].ToString().TrimStart().Split("'");
            var minutes = Double.Parse(min[0]) / 60;
            var sec = min[1].ToString().TrimStart().Split("\"");
            var seconds = Double.Parse(sec[0]) / 3600;
            double res = (degrees + minutes + seconds);// * multiplier;
            if (sec[1].Trim().ToString() != null && (sec[1].Trim().ToString() == "S" || sec[1].Trim().ToString() == "W"))
            {
                res = res * -1;
            }
            return res;// * multiplier;
        }
        #endregion

        #region ExcelOperation

        public int ImportExcelForDepartureReport(List<IFormFile> fileslist)
        {
            //string[] subdirectoryEntries = Directory.GetDirectories(@"C:\VDD\");
            //foreach (string subdirectory in subdirectoryEntries)
            //{
            //DirectoryInfo d = new DirectoryInfo(subdirectory);
            //FileInfo[] Files = d.GetFiles();
            //if (Files != null)
            //{
            //    foreach (FileInfo fileInfo in Files)
            //        using (var fileStream = File.OpenRead(fileInfo.FullName))
            //        {
            string IMONumber = string.Empty;
            if (fileslist != null)
            {
                foreach (IFormFile fileInfo in fileslist)
                    using (var fileStream = fileInfo.OpenReadStream())
                    {
                        using (var package = new ExcelPackage(fileStream))
                        {
                            int RawsheetIndex = 0;
                            for (int sheetcout = 0; sheetcout < package.Workbook.Worksheets.Count(); sheetcout++)
                            {
                                if (package.Workbook.Worksheets[sheetcout].Name.ToString().Contains("Raw Detail"))
                                {
                                    RawsheetIndex = sheetcout;
                                    break;
                                }
                            }
                            ExcelWorksheet worksheet = package.Workbook.Worksheets[RawsheetIndex];
                            var rowCount = worksheet.Dimension.Rows;

                            #region LocalVariableTobeDeleted
                            string vasselName = string.Empty;
                            string vasselId = string.Empty;
                            string passegid = string.Empty;
                            string voyageNo = string.Empty;
                            string loadCondition = string.Empty;
                            string fromPortDescription = string.Empty;
                            string toPortDescription = string.Empty;
                            string ssp = string.Empty;
                            string esp = string.Empty;

                            string utcOffset = string.Empty;
                            string time = string.Empty;
                            string latitude = string.Empty;
                            string longitude = string.Empty;
                            string draftForward = string.Empty;
                            string draftAft = string.Empty;
                            string cargo = string.Empty;
                            string rPM = string.Empty;
                            string speed = string.Empty;
                            string reportedSpeed = string.Empty;
                            string distance = string.Empty;
                            string distanceToGo = string.Empty;
                            string engineDistance = string.Empty;
                            string slip = string.Empty;
                            string consumption = string.Empty;

                            #region VoyageColumnHeaderIndexDeclare
                            int VesselNameIndex = 0;
                            int VesselIdIndex = 0;
                            int PassegidIndex = 0;
                            int VoyageNoIndex = 0;
                            int LoadConditionIndex = 0;
                            int FromPortDescriptionIndex = 0;
                            int ToPortDescriptionIndex = 0;
                            int StartPassageIndex = 0;
                            int EndPassageIndex = 0;
                            #endregion

                            #region FormsColumnHeaderIndexDeclare
                            int ReportDatetimeIndex = 0;
                            int UTCOffsetIndex = 0;
                            int LatitudeIndex = 0;
                            int LongitudeIndex = 0;
                            int DraftForwardIndex = 0;
                            int DraftAftIndex = 0;
                            int CargoIndex = 0;
                            int RPMIndex = 0;
                            int OrderedSpeedIndex = 0;
                            int SpeedIndex = 0;
                            int ReportedSpeedIndex = 0;
                            int DistanceIndex = 0;
                            int DistanceToGoIndex = 0;
                            int EngineDistanceIndex = 0;
                            int SlipIndex = 0;
                            int ConsumptionIndex = 0;
                            int CommentsIndex = 0;
                            int HoursIndex = 0;
                            int COGIndex = 0;
                            int PowerIndex = 0;
                            int PositionReportTypeIndex = 0;
                            int ReportedSeaSurfaceTemperatureIndex = 0;
                            int ReportedAirTemperatureIndex = 0;
                            int ReportedAtmosphericPressureIndex = 0;

                            int ReportedWindSpeedIndex = 0;
                            int ReportedWindDirectionTrueIndex = 0;
                            int ReportedWaveHeightIndex = 0;
                            int ReportedWaveHeightDirectionTrueIndex = 0;
                            int ReportedCurrentSpeedIndex = 0;
                            #region analyzedweather
                            int WindDirectionTrueIndex = 0;
                            int AnalysisWindKtsIndex = 0;
                            int WaveHeightDirectionTrueIndex = 0;
                            int AnalysisCombinedWaveHeightMIndex = 0;
                            int CurrentFactorIndex = 0;
                            int AnalysisSwellHeightMIndex = 0;
                            DateTime utcReportdatetime = DateTime.Now;
                            #endregion

                            #endregion

                            #region FluidConsumptionColumnIndex

                            #region Rob

                            int RobLMGOIndex = 0;
                            int RobHSFOIndex = 0;
                            int RobVLSFIndex = 0;
                            int RobLIFOIndex = 0;
                            int Rob180Index = 0;
                            int Rob380Index = 0;
                            int RobCYLIndex = 0;
                            int RobDMAIndex = 0;
                            int RobDMBIndex = 0;
                            int RobDMCIndex = 0;
                            int RobIFOIndex = 0;
                            int RobLSG1Index = 0;
                            int RobLSMDOIndex = 0;
                            int RobLSIFOIndex = 0;
                            int RobLUBIndex = 0;
                            int RobMDOIndex = 0;
                            int RobMGOIndex = 0;
                            int RobUnknownIndex = 0;
                            string RobUnknownName = "";
                            #endregion

                            #region MAN

                            int ManeuverLMGOIndex = 0;
                            int ManeuverHSFOIndex = 0;
                            int ManeuverVLSFIndex = 0;
                            int ManeuverLIFOIndex = 0;
                            int Maneuver180Index = 0;
                            int Maneuver380Index = 0;
                            int ManeuverCYLIndex = 0;
                            int ManeuverDMAIndex = 0;
                            int ManeuverDMBIndex = 0;
                            int ManeuverDMCIndex = 0;
                            int ManeuverIFOIndex = 0;
                            int ManeuverLSG1Index = 0;
                            int ManeuverLSMDOIndex = 0;
                            int ManeuverLSIFOIndex = 0;
                            int ManeuverLUBIndex = 0;
                            int ManeuverMDOIndex = 0;
                            int ManeuverMGOIndex = 0;
                            int ManeuverUnknownIndex = 0;
                            string ManeuverUnknownName = "";

                            #endregion

                            #region PROP

                            int PropulsionLMGOIndex = 0;
                            int PropulsionHSFOIndex = 0;
                            int PropulsionVLSFIndex = 0;
                            int PropulsionLIFOIndex = 0;
                            int Propulsion180Index = 0;
                            int Propulsion380Index = 0;
                            int PropulsionCYLIndex = 0;
                            int PropulsionDMAIndex = 0;
                            int PropulsionDMBIndex = 0;
                            int PropulsionDMCIndex = 0;
                            int PropulsionIFOIndex = 0;
                            int PropulsionLSG1Index = 0;
                            int PropulsionLSMDOIndex = 0;
                            int PropulsionLSIFOIndex = 0;
                            int PropulsionLUBIndex = 0;
                            int PropulsionMDOIndex = 0;
                            int PropulsionMGOIndex = 0;
                            int PropulsionUnknownIndex = 0;
                            string PropulsionUnknownName = "";
                            #endregion

                            #region BOIL

                            int BoilerLMGOIndex = 0;
                            int BoilerHSFOIndex = 0;
                            int BoilerVLSFIndex = 0;
                            int BoilerLIFOIndex = 0;
                            int Boiler180Index = 0;
                            int Boiler380Index = 0;
                            int BoilerCYLIndex = 0;
                            int BoilerDMAIndex = 0;
                            int BoilerDMBIndex = 0;
                            int BoilerDMCIndex = 0;
                            int BoilerIFOIndex = 0;
                            int BoilerLSG1Index = 0;
                            int BoilerLSMDOIndex = 0;
                            int BoilerLSIFOIndex = 0;
                            int BoilerLUBIndex = 0;
                            int BoilerMDOIndex = 0;
                            int BoilerMGOIndex = 0;
                            int BoilerUnknownIndex = 0;
                            string BoilerUnknownName = "";


                            #endregion

                            #region DEBALLAST

                            int DeballastLMGOIndex = 0;
                            int DeballastHSFOIndex = 0;
                            int DeballastVLSFIndex = 0;
                            int DeballastLIFOIndex = 0;
                            int Deballast180Index = 0;
                            int Deballast380Index = 0;
                            int DeballastCYLIndex = 0;
                            int DeballastDMAIndex = 0;
                            int DeballastDMBIndex = 0;
                            int DeballastDMCIndex = 0;
                            int DeballastIFOIndex = 0;
                            int DeballastLSG1Index = 0;
                            int DeballastLSMDOIndex = 0;
                            int DeballastLSIFOIndex = 0;
                            int DeballastLUBIndex = 0;
                            int DeballastMDOIndex = 0;
                            int DeballastMGOIndex = 0;
                            int DeballastUnknownIndex = 0;
                            string DeballastUnknownName = "";
                            #endregion

                            #region GEN

                            int GeneratorLMGOIndex = 0;
                            int GeneratorHSFOIndex = 0;
                            int GeneratorVLSFIndex = 0;
                            int GeneratorLIFOIndex = 0;
                            int Generator180Index = 0;
                            int Generator380Index = 0;
                            int GeneratorCYLIndex = 0;
                            int GeneratorDMAIndex = 0;
                            int GeneratorDMBIndex = 0;
                            int GeneratorDMCIndex = 0;
                            int GeneratorIFOIndex = 0;
                            int GeneratorLSG1Index = 0;
                            int GeneratorLSMDOIndex = 0;
                            int GeneratorLSIFOIndex = 0;
                            int GeneratorLUBIndex = 0;
                            int GeneratorMDOIndex = 0;
                            int GeneratorMGOIndex = 0;
                            int GeneratorUnknownIndex = 0;
                            string GeneratorUnknownName = "";

                            #endregion

                            #region OTHER

                            int OtherLMGOIndex = 0;
                            int OtherHSFOIndex = 0;
                            int OtherVLSFIndex = 0;
                            int OtherLIFOIndex = 0;
                            int Other180Index = 0;
                            int Other380Index = 0;
                            int OtherCYLIndex = 0;
                            int OtherDMAIndex = 0;
                            int OtherDMBIndex = 0;
                            int OtherDMCIndex = 0;
                            int OtherIFOIndex = 0;
                            int OtherLSG1Index = 0;
                            int OtherLSMDOIndex = 0;
                            int OtherLSIFOIndex = 0;
                            int OtherLUBIndex = 0;
                            int OtherMDOIndex = 0;
                            int OtherMGOIndex = 0;
                            int OtherUnknownIndex = 0;
                            string OtherUnknownName = "";

                            #endregion

                            #region TANKCleaning

                            int TankCleaningLMGOIndex = 0;
                            int TankCleaningHSFOIndex = 0;
                            int TankCleaningVLSFIndex = 0;
                            int TankCleaningLIFOIndex = 0;
                            int TankCleaning180Index = 0;
                            int TankCleaning380Index = 0;
                            int TankCleaningCYLIndex = 0;
                            int TankCleaningDMAIndex = 0;
                            int TankCleaningDMBIndex = 0;
                            int TankCleaningDMCIndex = 0;
                            int TankCleaningIFOIndex = 0;
                            int TankCleaningLSG1Index = 0;
                            int TankCleaningLSMDOIndex = 0;
                            int TankCleaningLSIFOIndex = 0;
                            int TankCleaningLUBIndex = 0;
                            int TankCleaningMDOIndex = 0;
                            int TankCleaningMGOIndex = 0;
                            int TankCleaningUnknownIndex = 0;
                            string TankCleaningUnknownName = "";

                            #endregion

                            #region  Incinerator

                            int IncineratorLMGOIndex = 0;
                            int IncineratorHSFOIndex = 0;
                            int IncineratorVLSFIndex = 0;
                            int IncineratorLIFOIndex = 0;
                            int Incinerator180Index = 0;
                            int Incinerator380Index = 0;
                            int IncineratorCYLIndex = 0;
                            int IncineratorDMAIndex = 0;
                            int IncineratorDMBIndex = 0;
                            int IncineratorDMCIndex = 0;
                            int IncineratorIFOIndex = 0;
                            int IncineratorLSG1Index = 0;
                            int IncineratorLSMDOIndex = 0;
                            int IncineratorLSIFOIndex = 0;
                            int IncineratorLUBIndex = 0;
                            int IncineratorMDOIndex = 0;
                            int IncineratorMGOIndex = 0;
                            int IncineratorUnknownIndex = 0;
                            string IncineratorUnknownName = "";

                            #endregion

                            #region IGS

                            int InertGasGeneratorLMGOIndex = 0;
                            int InertGasGeneratorHSFOIndex = 0;
                            int InertGasGeneratorVLSFIndex = 0;
                            int InertGasGeneratorLIFOIndex = 0;
                            int InertGasGenerator180Index = 0;
                            int InertGasGenerator380Index = 0;
                            int InertGasGeneratorCYLIndex = 0;
                            int InertGasGeneratorDMAIndex = 0;
                            int InertGasGeneratorDMBIndex = 0;
                            int InertGasGeneratorDMCIndex = 0;
                            int InertGasGeneratorIFOIndex = 0;
                            int InertGasGeneratorLSG1Index = 0;
                            int InertGasGeneratorLSMDOIndex = 0;
                            int InertGasGeneratorLSIFOIndex = 0;
                            int InertGasGeneratorLUBIndex = 0;
                            int InertGasGeneratorMDOIndex = 0;
                            int InertGasGeneratorMGOIndex = 0;
                            int InertGasGeneratorUnknownIndex = 0;
                            string InertGasGeneratorUnknownName = "";

                            #endregion

                            #region CargoHeating

                            int CargoHeatingLMGOIndex = 0;
                            int CargoHeatingHSFOIndex = 0;
                            int CargoHeatingVLSFIndex = 0;
                            int CargoHeatingLIFOIndex = 0;
                            int CargoHeating180Index = 0;
                            int CargoHeating380Index = 0;
                            int CargoHeatingCYLIndex = 0;
                            int CargoHeatingDMAIndex = 0;
                            int CargoHeatingDMBIndex = 0;
                            int CargoHeatingDMCIndex = 0;
                            int CargoHeatingIFOIndex = 0;
                            int CargoHeatingLSG1Index = 0;
                            int CargoHeatingLSMDOIndex = 0;
                            int CargoHeatingLSIFOIndex = 0;
                            int CargoHeatingLUBIndex = 0;
                            int CargoHeatingMDOIndex = 0;
                            int CargoHeatingMGOIndex = 0;
                            int CargoHeatingUnknownIndex = 0;
                            string CargoHeatingUnknownName = "";

                            #endregion

                            #region UnknownCategory

                            int UnknownCategoryLMGOIndex = 0;
                            int UnknownCategoryHSFOIndex = 0;
                            int UnknownCategoryVLSFIndex = 0;
                            int UnknownCategoryLIFOIndex = 0;
                            int UnknownCategory180Index = 0;
                            int UnknownCategory380Index = 0;
                            int UnknownCategoryCYLIndex = 0;
                            int UnknownCategoryDMAIndex = 0;
                            int UnknownCategoryDMBIndex = 0;
                            int UnknownCategoryDMCIndex = 0;
                            int UnknownCategoryIFOIndex = 0;
                            int UnknownCategoryLSG1Index = 0;
                            int UnknownCategoryLSMDOIndex = 0;
                            int UnknownCategoryLSIFOIndex = 0;
                            int UnknownCategoryLUBIndex = 0;
                            int UnknownCategoryMDOIndex = 0;
                            int UnknownCategoryMGOIndex = 0;
                            int UnknownCategoryUnknownIndex = 0;
                            string UnknownCategoryUnknownName = "";
                            string UnknownCategoryName = "";

                            #endregion

                            #endregion


                            #endregion
                            #region SetColumnHeaderIndexValue
                            for (int column = 1; column <= 800; column++)
                            {
                                if (worksheet.Cells[1, column].Value == null)
                                {
                                    break;
                                }
                                switch (worksheet.Cells[1, column].Value.ToString())
                                {
                                    case "VesselName":
                                        VesselNameIndex = column;
                                        break;
                                    case "VesselId":
                                        VesselIdIndex = column;
                                        break;
                                    case "PassageId":
                                        PassegidIndex = column;
                                        break;
                                    case "VoyageNo":
                                        VoyageNoIndex = column;
                                        break;
                                    case "LoadCondition":
                                        LoadConditionIndex = column;
                                        break;
                                    case "FromPortDescription":
                                        FromPortDescriptionIndex = column;
                                        break;
                                    case "ToPortDescription":
                                        ToPortDescriptionIndex = column;
                                        break;
                                    case "Ssp":
                                        StartPassageIndex = column;
                                        break;
                                    case "Esp":
                                        EndPassageIndex = column;
                                        break;
                                    case "Time":
                                        ReportDatetimeIndex = column;
                                        break;
                                    case "UTCOffset":
                                        UTCOffsetIndex = column;
                                        break;
                                    case "Latitude":
                                        LatitudeIndex = column;
                                        break;
                                    case "Longitude":
                                        LongitudeIndex = column;
                                        break;
                                    case "DraftForward":
                                        DraftForwardIndex = column;
                                        break;
                                    case "DraftAft":
                                        DraftAftIndex = column;
                                        break;
                                    case "Cargo":
                                        CargoIndex = column;
                                        break;
                                    case "RPM":
                                        RPMIndex = column;
                                        break;
                                    case "OrderedSpeed":
                                        OrderedSpeedIndex = column;
                                        break;
                                    case "Speed":
                                        SpeedIndex = column;
                                        break;
                                    case "ReportedSpeed":
                                        ReportedSpeedIndex = column;
                                        break;
                                    case "Distance":
                                        DistanceIndex = column;
                                        break;
                                    case "DistanceToGo":
                                        DistanceToGoIndex = column;
                                        break;
                                    case "EngineDistance":
                                        EngineDistanceIndex = column;
                                        break;
                                    case "Slip":
                                        SlipIndex = column;
                                        break;
                                    case "Comments":
                                        CommentsIndex = column;
                                        break;
                                    case "Hours":
                                        HoursIndex = column;
                                        break;
                                    case "COG":
                                        COGIndex = column;
                                        break;
                                    case "Power":
                                        PowerIndex = column;
                                        break;
                                    case "PositionReportType":
                                        PositionReportTypeIndex = column;
                                        break;
                                    case "ROB":
                                        if (worksheet.Cells[2, column].Value.ToString() == "ALL")
                                        {
                                            switch (worksheet.Cells[3, column].Value.ToString())
                                            {
                                                case "LMGO":
                                                    RobLMGOIndex = column;
                                                    break;
                                                case "LSMGO":
                                                    RobLMGOIndex = column;
                                                    break;
                                                case "LSG":
                                                    RobLMGOIndex = column;
                                                    break;
                                                case "LSGO":
                                                    RobLMGOIndex = column;
                                                    break;
                                                case "LSMO":
                                                    RobLMGOIndex = column;
                                                    break;
                                                case "HSFO":
                                                    RobHSFOIndex = column;
                                                    break;
                                                case "HSF":
                                                    RobHSFOIndex = column;
                                                    break;
                                                case "HSIFO":
                                                    RobHSFOIndex = column;
                                                    break;
                                                case "VLSF":
                                                    RobVLSFIndex = column;
                                                    break;

                                                case "VLS":
                                                    RobVLSFIndex = column;
                                                    break;
                                                case "VLSFO":
                                                    RobVLSFIndex = column;
                                                    break;

                                                case "LIFO":
                                                    RobLIFOIndex = column;
                                                    break;
                                                case "LSF":
                                                    RobLIFOIndex = column;
                                                    break;
                                                case "LSIFO":
                                                    RobLIFOIndex = column;
                                                    break;
                                                case "LSFO":
                                                    RobLIFOIndex = column;
                                                    break;
                                                case "180":
                                                    Rob180Index = column;
                                                    break;
                                                case "380":
                                                    Rob380Index = column;
                                                    break;

                                                case "CYL":
                                                    RobCYLIndex = column;
                                                    break;
                                                case "DMA":
                                                    RobDMAIndex = column;
                                                    break;
                                                case "DMB":
                                                    RobDMBIndex = column;
                                                    break;

                                                case "DMC":
                                                    RobDMCIndex = column;
                                                    break;
                                                case "IFO":
                                                    RobIFOIndex = column;
                                                    break;
                                                case "LSG1":
                                                    RobLSG1Index = column;
                                                    break;
                                                case "LS1":
                                                    RobLSG1Index = column;
                                                    break;

                                                case "LSMDO":
                                                    RobLSMDOIndex = column;
                                                    break;
                                                case "LSD":
                                                    RobLSMDOIndex = column;
                                                    break;
                                                //case "LSIFO":
                                                //    RobLSIFOIndex = column;
                                                //    break;
                                                case "LUB":
                                                    RobLUBIndex = column;
                                                    break;
                                                case "MDO":
                                                    RobMDOIndex = column;
                                                    break;
                                                case "MGO":
                                                    RobMGOIndex = column;
                                                    break;
                                                default:
                                                    RobUnknownIndex = column;
                                                    RobUnknownName = worksheet.Cells[3, column].Value.ToString();
                                                    break;
                                            }
                                        }
                                        break;
                                    case "Consumption":
                                        switch (worksheet.Cells[2, column].Value.ToString())
                                        {
                                            case "MAN":
                                                switch (worksheet.Cells[3, column].Value.ToString())
                                                {
                                                    case "HSF":
                                                        ManeuverHSFOIndex = column;
                                                        break;
                                                    case "HSIFO":
                                                        ManeuverHSFOIndex = column;
                                                        break;
                                                    case "LSF":
                                                        ManeuverLIFOIndex = column;
                                                        break;
                                                    case "LSIFO":
                                                        ManeuverLIFOIndex = column;
                                                        break;
                                                    case "LSFO":
                                                        ManeuverLIFOIndex = column;
                                                        break;
                                                    case "LS1":
                                                        ManeuverLSG1Index = column;
                                                        break;
                                                    case "LSD":
                                                        ManeuverLSMDOIndex = column;
                                                        break;
                                                    case "LSMGO":
                                                        ManeuverLMGOIndex = column;
                                                        break;
                                                    case "LSG":
                                                        ManeuverLMGOIndex = column;
                                                        break;
                                                    case "LSGO":
                                                        ManeuverLMGOIndex = column;
                                                        break;
                                                    case "LSMO":
                                                        ManeuverLMGOIndex = column;
                                                        break;
                                                    case "VLS":
                                                        ManeuverVLSFIndex = column;
                                                        break;
                                                    case "VLSFO":
                                                        ManeuverVLSFIndex = column;
                                                        break;
                                                    case "HSFO":
                                                        ManeuverHSFOIndex = column;
                                                        break;
                                                    case "LMGO":
                                                        ManeuverLMGOIndex = column;
                                                        break;
                                                    case "VLSF":
                                                        ManeuverVLSFIndex = column;
                                                        break;

                                                    case "LIFO":
                                                        ManeuverLIFOIndex = column;
                                                        break;
                                                    case "180":
                                                        Maneuver180Index = column;
                                                        break;
                                                    case "380":
                                                        Maneuver380Index = column;
                                                        break;
                                                    case "CYL":
                                                        ManeuverCYLIndex = column;
                                                        break;
                                                    case "DMA":
                                                        ManeuverDMAIndex = column;
                                                        break;
                                                    case "DMB":
                                                        ManeuverDMBIndex = column;
                                                        break;
                                                    case "DMC":
                                                        ManeuverDMCIndex = column;
                                                        break;
                                                    case "IFO":
                                                        ManeuverIFOIndex = column;
                                                        break;
                                                    case "LSG1":
                                                        ManeuverLSG1Index = column;
                                                        break;
                                                    case "LSMDO":
                                                        ManeuverLSMDOIndex = column;
                                                        break;
                                                    //case "LSIFO":
                                                    //    ManeuverLSIFOIndex = column;
                                                    //    break;
                                                    case "LUB":
                                                        ManeuverLUBIndex = column;
                                                        break;
                                                    case "MDO":
                                                        ManeuverMDOIndex = column;
                                                        break;
                                                    case "MGO":
                                                        ManeuverMGOIndex = column;
                                                        break;
                                                    default:
                                                        ManeuverUnknownIndex = column;
                                                        ManeuverUnknownName = worksheet.Cells[3, column].Value.ToString();
                                                        break;
                                                }
                                                break;
                                            case "PROP":
                                                switch (worksheet.Cells[3, column].Value.ToString())
                                                {
                                                    case "HSF":
                                                        PropulsionHSFOIndex = column;
                                                        break;
                                                    case "HSIFO":
                                                        PropulsionHSFOIndex = column;
                                                        break;
                                                    case "LSF":
                                                        PropulsionLIFOIndex = column;
                                                        break;
                                                    case "LSIFO":
                                                        PropulsionLIFOIndex = column;
                                                        break;
                                                    case "LSFO":
                                                        PropulsionLIFOIndex = column;
                                                        break;
                                                    case "LS1":
                                                        PropulsionLSG1Index = column;
                                                        break;
                                                    case "LSD":
                                                        PropulsionLSMDOIndex = column;
                                                        break;
                                                    case "LSMGO":
                                                        PropulsionLMGOIndex = column;
                                                        break;
                                                    case "LSG":
                                                        PropulsionLMGOIndex = column;
                                                        break;
                                                    case "LSGO":
                                                        PropulsionLMGOIndex = column;
                                                        break;
                                                    case "LSMO":
                                                        PropulsionLMGOIndex = column;
                                                        break;
                                                    case "VLS":
                                                        PropulsionVLSFIndex = column;
                                                        break;
                                                    case "VLSFO":
                                                        PropulsionVLSFIndex = column;
                                                        break;
                                                    case "HSFO":
                                                        PropulsionHSFOIndex = column;
                                                        break;
                                                    case "LMGO":
                                                        PropulsionLMGOIndex = column;
                                                        break;
                                                    case "VLSF":
                                                        PropulsionVLSFIndex = column;
                                                        break;
                                                    case "LIFO":
                                                        PropulsionLIFOIndex = column;
                                                        break;
                                                    case "180":
                                                        Propulsion180Index = column;
                                                        break;
                                                    case "380":
                                                        Propulsion380Index = column;
                                                        break;
                                                    case "CYL":
                                                        PropulsionCYLIndex = column;
                                                        break;
                                                    case "DMA":
                                                        PropulsionDMAIndex = column;
                                                        break;
                                                    case "DMB":
                                                        PropulsionDMBIndex = column;
                                                        break;
                                                    case "DMC":
                                                        PropulsionDMCIndex = column;
                                                        break;
                                                    case "IFO":
                                                        PropulsionIFOIndex = column;
                                                        break;
                                                    case "LSG1":
                                                        PropulsionLSG1Index = column;
                                                        break;
                                                    case "LSMDO":
                                                        PropulsionLSMDOIndex = column;
                                                        break;
                                                    //case "LSIFO":
                                                    //    PropulsionLSIFOIndex = column;
                                                    //    break;
                                                    case "LUB":
                                                        PropulsionLUBIndex = column;
                                                        break;
                                                    case "MDO":
                                                        PropulsionMDOIndex = column;
                                                        break;
                                                    case "MGO":
                                                        PropulsionMGOIndex = column;
                                                        break;
                                                    default:
                                                        PropulsionUnknownIndex = column;
                                                        PropulsionUnknownName = worksheet.Cells[3, column].Value.ToString();
                                                        break;
                                                }
                                                break;
                                            case "BOIL":
                                                switch (worksheet.Cells[3, column].Value.ToString())
                                                {
                                                    case "HSF":
                                                        BoilerHSFOIndex = column;
                                                        break;
                                                    case "HSIFO":
                                                        BoilerHSFOIndex = column;
                                                        break;
                                                    case "LSF":
                                                        BoilerLIFOIndex = column;
                                                        break;
                                                    case "LSIFO":
                                                        BoilerLIFOIndex = column;
                                                        break;
                                                    case "LSFO":
                                                        BoilerLIFOIndex = column;
                                                        break;
                                                    case "LS1":
                                                        BoilerLSG1Index = column;
                                                        break;
                                                    case "LSD":
                                                        BoilerLSMDOIndex = column;
                                                        break;
                                                    case "LSMGO":
                                                        BoilerLMGOIndex = column;
                                                        break;
                                                    case "LSG":
                                                        BoilerLMGOIndex = column;
                                                        break;
                                                    case "LSGO":
                                                        BoilerLMGOIndex = column;
                                                        break;
                                                    case "LSMO":
                                                        BoilerLMGOIndex = column;
                                                        break;
                                                    case "VLS":
                                                        BoilerVLSFIndex = column;
                                                        break;
                                                    case "VLSFO":
                                                        BoilerVLSFIndex = column;
                                                        break;
                                                    case "HSFO":
                                                        BoilerHSFOIndex = column;
                                                        break;
                                                    case "LMGO":
                                                        BoilerLMGOIndex = column;
                                                        break;
                                                    case "VLSF":
                                                        BoilerVLSFIndex = column;
                                                        break;
                                                    case "LIFO":
                                                        BoilerLIFOIndex = column;
                                                        break;
                                                    case "180":
                                                        Boiler180Index = column;
                                                        break;
                                                    case "380":
                                                        Boiler380Index = column;
                                                        break;
                                                    case "CYL":
                                                        BoilerCYLIndex = column;
                                                        break;
                                                    case "DMA":
                                                        BoilerDMAIndex = column;
                                                        break;
                                                    case "DMB":
                                                        BoilerDMBIndex = column;
                                                        break;
                                                    case "DMC":
                                                        BoilerDMCIndex = column;
                                                        break;
                                                    case "IFO":
                                                        BoilerIFOIndex = column;
                                                        break;
                                                    case "LSG1":
                                                        BoilerLSG1Index = column;
                                                        break;
                                                    case "LSMDO":
                                                        BoilerLSMDOIndex = column;
                                                        break;
                                                    //case "LSIFO":
                                                    //    BoilerLSIFOIndex = column;
                                                    //    break;
                                                    case "LUB":
                                                        BoilerLUBIndex = column;
                                                        break;
                                                    case "MDO":
                                                        BoilerMDOIndex = column;
                                                        break;
                                                    case "MGO":
                                                        BoilerMGOIndex = column;
                                                        break;
                                                    default:
                                                        BoilerUnknownIndex = column;
                                                        BoilerUnknownName = worksheet.Cells[3, column].Value.ToString();
                                                        break;
                                                }
                                                break;
                                            case "DEBLST":
                                                switch (worksheet.Cells[3, column].Value.ToString())
                                                {
                                                    case "HSF":
                                                        DeballastHSFOIndex = column;
                                                        break;
                                                    case "HSIFO":
                                                        DeballastHSFOIndex = column;
                                                        break;
                                                    case "LSF":
                                                        DeballastLIFOIndex = column;
                                                        break;
                                                    case "LSIFO":
                                                        DeballastLIFOIndex = column;
                                                        break;
                                                    case "LSFO":
                                                        DeballastLIFOIndex = column;
                                                        break;
                                                    case "LS1":
                                                        DeballastLSG1Index = column;
                                                        break;
                                                    case "LSD":
                                                        DeballastLSMDOIndex = column;
                                                        break;
                                                    case "LSMGO":
                                                        DeballastLMGOIndex = column;
                                                        break;
                                                    case "LSG":
                                                        DeballastLMGOIndex = column;
                                                        break;
                                                    case "LSGO":
                                                        DeballastLMGOIndex = column;
                                                        break;
                                                    case "LSMO":
                                                        DeballastLMGOIndex = column;
                                                        break;
                                                    case "VLS":
                                                        DeballastVLSFIndex = column;
                                                        break;
                                                    case "VLSFO":
                                                        DeballastVLSFIndex = column;
                                                        break;
                                                    case "HSFO":
                                                        DeballastHSFOIndex = column;
                                                        break;
                                                    case "LMGO":
                                                        DeballastLMGOIndex = column;
                                                        break;
                                                    case "VLSF":
                                                        DeballastVLSFIndex = column;
                                                        break;
                                                    case "LIFO":
                                                        DeballastLIFOIndex = column;
                                                        break;
                                                    case "180":
                                                        Deballast180Index = column;
                                                        break;
                                                    case "380":
                                                        Deballast380Index = column;
                                                        break;
                                                    case "CYL":
                                                        DeballastCYLIndex = column;
                                                        break;
                                                    case "DMA":
                                                        DeballastDMAIndex = column;
                                                        break;
                                                    case "DMB":
                                                        DeballastDMBIndex = column;
                                                        break;
                                                    case "DMC":
                                                        DeballastDMCIndex = column;
                                                        break;
                                                    case "IFO":
                                                        DeballastIFOIndex = column;
                                                        break;
                                                    case "LSG1":
                                                        DeballastLSG1Index = column;
                                                        break;
                                                    case "LSMDO":
                                                        DeballastLSMDOIndex = column;
                                                        break;
                                                    //case "LSIFO":
                                                    //    DeballastLSIFOIndex = column;
                                                    //    break;
                                                    case "LUB":
                                                        DeballastLUBIndex = column;
                                                        break;
                                                    case "MDO":
                                                        DeballastMDOIndex = column;
                                                        break;
                                                    case "MGO":
                                                        DeballastMGOIndex = column;
                                                        break;
                                                    default:
                                                        DeballastUnknownIndex = column;
                                                        DeballastUnknownName = worksheet.Cells[3, column].Value.ToString();
                                                        break;
                                                }
                                                break;
                                            case "GEN":
                                                switch (worksheet.Cells[3, column].Value.ToString())
                                                {
                                                    case "HSF":
                                                        GeneratorHSFOIndex = column;
                                                        break;
                                                    case "HSIFO":
                                                        GeneratorHSFOIndex = column;
                                                        break;
                                                    case "LSF":
                                                        GeneratorLIFOIndex = column;
                                                        break;
                                                    case "LSIFO":
                                                        GeneratorLIFOIndex = column;
                                                        break;
                                                    case "LSFO":
                                                        GeneratorLIFOIndex = column;
                                                        break;
                                                    case "LS1":
                                                        GeneratorLSG1Index = column;
                                                        break;
                                                    case "LSD":
                                                        GeneratorLSMDOIndex = column;
                                                        break;
                                                    case "LSMGO":
                                                        GeneratorLMGOIndex = column;
                                                        break;
                                                    case "LSG":
                                                        GeneratorLMGOIndex = column;
                                                        break;
                                                    case "LSGO":
                                                        GeneratorLMGOIndex = column;
                                                        break;
                                                    case "LSMO":
                                                        GeneratorLMGOIndex = column;
                                                        break;
                                                    case "VLS":
                                                        GeneratorVLSFIndex = column;
                                                        break;
                                                    case "VLSFO":
                                                        GeneratorVLSFIndex = column;
                                                        break;
                                                    case "HSFO":
                                                        GeneratorHSFOIndex = column;
                                                        break;
                                                    case "LMGO":
                                                        GeneratorLMGOIndex = column;
                                                        break;
                                                    case "VLSF":
                                                        GeneratorVLSFIndex = column;
                                                        break;
                                                    case "LIFO":
                                                        GeneratorLIFOIndex = column;
                                                        break;
                                                    case "180":
                                                        Generator180Index = column;
                                                        break;
                                                    case "380":
                                                        Generator380Index = column;
                                                        break;
                                                    case "CYL":
                                                        GeneratorCYLIndex = column;
                                                        break;
                                                    case "DMA":
                                                        GeneratorDMAIndex = column;
                                                        break;
                                                    case "DMB":
                                                        GeneratorDMBIndex = column;
                                                        break;
                                                    case "DMC":
                                                        GeneratorDMCIndex = column;
                                                        break;
                                                    case "IFO":
                                                        GeneratorIFOIndex = column;
                                                        break;
                                                    case "LSG1":
                                                        GeneratorLSG1Index = column;
                                                        break;
                                                    case "LSMDO":
                                                        GeneratorLSMDOIndex = column;
                                                        break;
                                                    //case "LSIFO":
                                                    //    GeneratorLSIFOIndex = column;
                                                    //    break;
                                                    case "LUB":
                                                        GeneratorLUBIndex = column;
                                                        break;
                                                    case "MDO":
                                                        GeneratorMDOIndex = column;
                                                        break;
                                                    case "MGO":
                                                        GeneratorMGOIndex = column;
                                                        break;
                                                    default:
                                                        GeneratorUnknownIndex = column;
                                                        GeneratorUnknownName = worksheet.Cells[3, column].Value.ToString();
                                                        break;
                                                }
                                                break;
                                            case "OTHER":
                                                switch (worksheet.Cells[3, column].Value.ToString())
                                                {
                                                    case "HSF":
                                                        OtherHSFOIndex = column;
                                                        break;
                                                    case "HSIFO":
                                                        OtherHSFOIndex = column;
                                                        break;
                                                    case "LSF":
                                                        OtherLIFOIndex = column;
                                                        break;
                                                    case "LSIFO":
                                                        OtherLIFOIndex = column;
                                                        break;
                                                    case "LSFO":
                                                        OtherLIFOIndex = column;
                                                        break;
                                                    case "LS1":
                                                        OtherLSG1Index = column;
                                                        break;
                                                    case "LSD":
                                                        OtherLSMDOIndex = column;
                                                        break;
                                                    case "LSMGO":
                                                        OtherLMGOIndex = column;
                                                        break;
                                                    case "LSG":
                                                        OtherLMGOIndex = column;
                                                        break;
                                                    case "LSGO":
                                                        OtherLMGOIndex = column;
                                                        break;
                                                    case "LSMO":
                                                        OtherLMGOIndex = column;
                                                        break;
                                                    case "VLS":
                                                        OtherVLSFIndex = column;
                                                        break;
                                                    case "VLSFO":
                                                        OtherVLSFIndex = column;
                                                        break;
                                                    case "HSFO":
                                                        OtherHSFOIndex = column;
                                                        break;
                                                    case "LMGO":
                                                        OtherLMGOIndex = column;
                                                        break;
                                                    case "VLSF":
                                                        OtherVLSFIndex = column;
                                                        break;
                                                    case "LIFO":
                                                        OtherLIFOIndex = column;
                                                        break;
                                                    case "180":
                                                        Other180Index = column;
                                                        break;
                                                    case "380":
                                                        Other380Index = column;
                                                        break;
                                                    case "CYL":
                                                        OtherCYLIndex = column;
                                                        break;
                                                    case "DMA":
                                                        OtherDMAIndex = column;
                                                        break;
                                                    case "DMB":
                                                        OtherDMBIndex = column;
                                                        break;
                                                    case "DMC":
                                                        OtherDMCIndex = column;
                                                        break;
                                                    case "IFO":
                                                        OtherIFOIndex = column;
                                                        break;
                                                    case "LSG1":
                                                        OtherLSG1Index = column;
                                                        break;
                                                    case "LSMDO":
                                                        OtherLSMDOIndex = column;
                                                        break;
                                                    //case "LSIFO":
                                                    //    OtherLSIFOIndex = column;
                                                    //    break;
                                                    case "LUB":
                                                        OtherLUBIndex = column;
                                                        break;
                                                    case "MDO":
                                                        OtherMDOIndex = column;
                                                        break;
                                                    case "MGO":
                                                        OtherMGOIndex = column;
                                                        break;
                                                    default:
                                                        OtherUnknownIndex = column;
                                                        OtherUnknownName = worksheet.Cells[3, column].Value.ToString();
                                                        break;
                                                }
                                                break;
                                            case "TKCLN":
                                                switch (worksheet.Cells[3, column].Value.ToString())
                                                {
                                                    case "HSF":
                                                        TankCleaningHSFOIndex = column;
                                                        break;
                                                    case "HSIFO":
                                                        TankCleaningHSFOIndex = column;
                                                        break;
                                                    case "LSF":
                                                        TankCleaningLIFOIndex = column;
                                                        break;
                                                    case "LSIFO":
                                                        TankCleaningLIFOIndex = column;
                                                        break;
                                                    case "LSFO":
                                                        TankCleaningLIFOIndex = column;
                                                        break;
                                                    case "LS1":
                                                        TankCleaningLSG1Index = column;
                                                        break;
                                                    case "LSD":
                                                        TankCleaningLSMDOIndex = column;
                                                        break;
                                                    case "LSMGO":
                                                        TankCleaningLMGOIndex = column;
                                                        break;
                                                    case "LSG":
                                                        TankCleaningLMGOIndex = column;
                                                        break;
                                                    case "LSGO":
                                                        TankCleaningLMGOIndex = column;
                                                        break;
                                                    case "LSMO":
                                                        TankCleaningLMGOIndex = column;
                                                        break;
                                                    case "VLS":
                                                        TankCleaningVLSFIndex = column;
                                                        break;
                                                    case "VLSFO":
                                                        TankCleaningVLSFIndex = column;
                                                        break;
                                                    case "HSFO":
                                                        TankCleaningHSFOIndex = column;
                                                        break;
                                                    case "LMGO":
                                                        TankCleaningLMGOIndex = column;
                                                        break;
                                                    case "VLSF":
                                                        TankCleaningVLSFIndex = column;
                                                        break;
                                                    case "LIFO":
                                                        TankCleaningLIFOIndex = column;
                                                        break;
                                                    case "180":
                                                        TankCleaning180Index = column;
                                                        break;
                                                    case "380":
                                                        TankCleaning380Index = column;
                                                        break;
                                                    case "CYL":
                                                        TankCleaningCYLIndex = column;
                                                        break;
                                                    case "DMA":
                                                        TankCleaningDMAIndex = column;
                                                        break;
                                                    case "DMB":
                                                        TankCleaningDMBIndex = column;
                                                        break;
                                                    case "DMC":
                                                        TankCleaningDMCIndex = column;
                                                        break;
                                                    case "IFO":
                                                        TankCleaningIFOIndex = column;
                                                        break;
                                                    case "LSG1":
                                                        TankCleaningLSG1Index = column;
                                                        break;
                                                    case "LSMDO":
                                                        TankCleaningLSMDOIndex = column;
                                                        break;
                                                    //case "LSIFO":
                                                    //    TankCleaningLSIFOIndex = column;
                                                    //    break;
                                                    case "LUB":
                                                        TankCleaningLUBIndex = column;
                                                        break;
                                                    case "MDO":
                                                        TankCleaningMDOIndex = column;
                                                        break;
                                                    case "MGO":
                                                        TankCleaningMGOIndex = column;
                                                        break;
                                                    default:
                                                        TankCleaningUnknownIndex = column;
                                                        TankCleaningUnknownName = worksheet.Cells[3, column].Value.ToString();
                                                        break;
                                                }
                                                break;
                                            case "INC":
                                                switch (worksheet.Cells[3, column].Value.ToString())
                                                {
                                                    case "HSF":
                                                        IncineratorHSFOIndex = column;
                                                        break;
                                                    case "HSIFO":
                                                        IncineratorHSFOIndex = column;
                                                        break;
                                                    case "LSF":
                                                        IncineratorLIFOIndex = column;
                                                        break;
                                                    case "LSIFO":
                                                        IncineratorLIFOIndex = column;
                                                        break;
                                                    case "LSFO":
                                                        IncineratorLIFOIndex = column;
                                                        break;
                                                    case "LS1":
                                                        IncineratorLSG1Index = column;
                                                        break;
                                                    case "LSD":
                                                        IncineratorLSMDOIndex = column;
                                                        break;
                                                    case "LSMGO":
                                                        IncineratorLMGOIndex = column;
                                                        break;
                                                    case "LSG":
                                                        IncineratorLMGOIndex = column;
                                                        break;
                                                    case "LSGO":
                                                        IncineratorLMGOIndex = column;
                                                        break;
                                                    case "LSMO":
                                                        IncineratorLMGOIndex = column;
                                                        break;
                                                    case "VLS":
                                                        IncineratorVLSFIndex = column;
                                                        break;
                                                    case "VLSFO":
                                                        IncineratorVLSFIndex = column;
                                                        break;
                                                    case "HSFO":
                                                        IncineratorHSFOIndex = column;
                                                        break;
                                                    case "LMGO":
                                                        IncineratorLMGOIndex = column;
                                                        break;
                                                    case "VLSF":
                                                        IncineratorVLSFIndex = column;
                                                        break;
                                                    case "LIFO":
                                                        IncineratorLIFOIndex = column;
                                                        break;
                                                    case "180":
                                                        Incinerator180Index = column;
                                                        break;
                                                    case "380":
                                                        Incinerator380Index = column;
                                                        break;
                                                    case "CYL":
                                                        IncineratorCYLIndex = column;
                                                        break;
                                                    case "DMA":
                                                        IncineratorDMAIndex = column;
                                                        break;
                                                    case "DMB":
                                                        IncineratorDMBIndex = column;
                                                        break;
                                                    case "DMC":
                                                        IncineratorDMCIndex = column;
                                                        break;
                                                    case "IFO":
                                                        IncineratorIFOIndex = column;
                                                        break;
                                                    case "LSG1":
                                                        IncineratorLSG1Index = column;
                                                        break;
                                                    case "LSMDO":
                                                        IncineratorLSMDOIndex = column;
                                                        break;
                                                    //case "LSIFO":
                                                    //    IncineratorLSIFOIndex = column;
                                                    //    break;
                                                    case "LUB":
                                                        IncineratorLUBIndex = column;
                                                        break;
                                                    case "MDO":
                                                        IncineratorMDOIndex = column;
                                                        break;
                                                    case "MGO":
                                                        IncineratorMGOIndex = column;
                                                        break;
                                                    default:
                                                        IncineratorUnknownIndex = column;
                                                        IncineratorUnknownName = worksheet.Cells[3, column].Value.ToString();
                                                        break;
                                                }
                                                break;
                                            case "INS":
                                                switch (worksheet.Cells[3, column].Value.ToString())
                                                {
                                                    case "HSF":
                                                        InertGasGeneratorHSFOIndex = column;
                                                        break;
                                                    case "HSIFO":
                                                        InertGasGeneratorHSFOIndex = column;
                                                        break;
                                                    case "LSF":
                                                        InertGasGeneratorLIFOIndex = column;
                                                        break;
                                                    case "LSIFO":
                                                        InertGasGeneratorLIFOIndex = column;
                                                        break;
                                                    case "LSFO":
                                                        InertGasGeneratorLIFOIndex = column;
                                                        break;
                                                    case "LS1":
                                                        InertGasGeneratorLSG1Index = column;
                                                        break;
                                                    case "LSD":
                                                        InertGasGeneratorLSMDOIndex = column;
                                                        break;
                                                    case "LSMGO":
                                                        InertGasGeneratorLMGOIndex = column;
                                                        break;
                                                    case "LSG":
                                                        InertGasGeneratorLMGOIndex = column;
                                                        break;
                                                    case "LSGO":
                                                        InertGasGeneratorLMGOIndex = column;
                                                        break;
                                                    case "LSMO":
                                                        InertGasGeneratorLMGOIndex = column;
                                                        break;
                                                    case "VLS":
                                                        InertGasGeneratorVLSFIndex = column;
                                                        break;
                                                    case "VLSFO":
                                                        InertGasGeneratorVLSFIndex = column;
                                                        break;
                                                    case "HSFO":
                                                        InertGasGeneratorHSFOIndex = column;
                                                        break;
                                                    case "LMGO":
                                                        InertGasGeneratorLMGOIndex = column;
                                                        break;
                                                    case "VLSF":
                                                        InertGasGeneratorVLSFIndex = column;
                                                        break;
                                                    case "LIFO":
                                                        InertGasGeneratorLIFOIndex = column;
                                                        break;
                                                    case "180":
                                                        InertGasGenerator180Index = column;
                                                        break;
                                                    case "380":
                                                        InertGasGenerator380Index = column;
                                                        break;
                                                    case "CYL":
                                                        InertGasGeneratorCYLIndex = column;
                                                        break;
                                                    case "DMA":
                                                        InertGasGeneratorDMAIndex = column;
                                                        break;
                                                    case "DMB":
                                                        InertGasGeneratorDMBIndex = column;
                                                        break;
                                                    case "DMC":
                                                        InertGasGeneratorDMCIndex = column;
                                                        break;
                                                    case "IFO":
                                                        InertGasGeneratorIFOIndex = column;
                                                        break;
                                                    case "LSG1":
                                                        InertGasGeneratorLSG1Index = column;
                                                        break;
                                                    case "LSMDO":
                                                        InertGasGeneratorLSMDOIndex = column;
                                                        break;
                                                    //case "LSIFO":
                                                    //    InertGasGeneratorLSIFOIndex = column;
                                                    //    break;
                                                    case "LUB":
                                                        InertGasGeneratorLUBIndex = column;
                                                        break;
                                                    case "MDO":
                                                        InertGasGeneratorMDOIndex = column;
                                                        break;
                                                    case "MGO":
                                                        InertGasGeneratorMGOIndex = column;
                                                        break;
                                                    default:
                                                        InertGasGeneratorUnknownIndex = column;
                                                        InertGasGeneratorUnknownName = worksheet.Cells[3, column].Value.ToString();
                                                        break;
                                                }
                                                break;
                                            case "CARHT":
                                                switch (worksheet.Cells[3, column].Value.ToString())
                                                {
                                                    case "HSF":
                                                        CargoHeatingHSFOIndex = column;
                                                        break;
                                                    case "HSIFO":
                                                        CargoHeatingHSFOIndex = column;
                                                        break;
                                                    case "LSF":
                                                        CargoHeatingLIFOIndex = column;
                                                        break;
                                                    case "LSIFO":
                                                        CargoHeatingLIFOIndex = column;
                                                        break;
                                                    case "LSFO":
                                                        CargoHeatingLIFOIndex = column;
                                                        break;
                                                    case "LS1":
                                                        CargoHeatingLSG1Index = column;
                                                        break;
                                                    case "LSD":
                                                        CargoHeatingLSMDOIndex = column;
                                                        break;
                                                    case "LSMGO":
                                                        CargoHeatingLMGOIndex = column;
                                                        break;
                                                    case "LSG":
                                                        CargoHeatingLMGOIndex = column;
                                                        break;
                                                    case "LSGO":
                                                        CargoHeatingLMGOIndex = column;
                                                        break;
                                                    case "LSMO":
                                                        CargoHeatingLMGOIndex = column;
                                                        break;
                                                    case "VLS":
                                                        CargoHeatingVLSFIndex = column;
                                                        break;
                                                    case "VLSFO":
                                                        CargoHeatingVLSFIndex = column;
                                                        break;
                                                    case "HSFO":
                                                        CargoHeatingHSFOIndex = column;
                                                        break;
                                                    case "LMGO":
                                                        CargoHeatingLMGOIndex = column;
                                                        break;
                                                    case "VLSF":
                                                        CargoHeatingVLSFIndex = column;
                                                        break;
                                                    case "LIFO":
                                                        CargoHeatingLIFOIndex = column;
                                                        break;
                                                    case "180":
                                                        CargoHeating180Index = column;
                                                        break;
                                                    case "380":
                                                        CargoHeating380Index = column;
                                                        break;
                                                    case "CYL":
                                                        CargoHeatingCYLIndex = column;
                                                        break;
                                                    case "DMA":
                                                        CargoHeatingDMAIndex = column;
                                                        break;
                                                    case "DMB":
                                                        CargoHeatingDMBIndex = column;
                                                        break;
                                                    case "DMC":
                                                        CargoHeatingDMCIndex = column;
                                                        break;
                                                    case "IFO":
                                                        CargoHeatingIFOIndex = column;
                                                        break;
                                                    case "LSG1":
                                                        CargoHeatingLSG1Index = column;
                                                        break;
                                                    case "LSMDO":
                                                        CargoHeatingLSMDOIndex = column;
                                                        break;
                                                    //case "LSIFO":
                                                    //CargoHeatingLSIFOIndex = column;
                                                    //break;
                                                    case "LUB":
                                                        CargoHeatingLUBIndex = column;
                                                        break;
                                                    case "MDO":
                                                        CargoHeatingMDOIndex = column;
                                                        break;
                                                    case "MGO":
                                                        CargoHeatingMGOIndex = column;
                                                        break;
                                                    default:
                                                        CargoHeatingUnknownIndex = column;
                                                        CargoHeatingUnknownName = worksheet.Cells[3, column].Value.ToString();
                                                        break;
                                                }
                                                break;
                                            default:
                                                UnknownCategoryName = worksheet.Cells[2, column].Value.ToString();
                                                switch (worksheet.Cells[3, column].Value.ToString())
                                                {
                                                    case "HSF":
                                                        UnknownCategoryHSFOIndex = column;
                                                        break;
                                                    case "HSIFO":
                                                        UnknownCategoryHSFOIndex = column;
                                                        break;
                                                    case "LSF":
                                                        UnknownCategoryLIFOIndex = column;
                                                        break;
                                                    case "LSIFO":
                                                        UnknownCategoryLIFOIndex = column;
                                                        break;
                                                    case "LSFO":
                                                        UnknownCategoryLIFOIndex = column;
                                                        break;
                                                    case "LS1":
                                                        UnknownCategoryLSG1Index = column;
                                                        break;
                                                    case "LSD":
                                                        UnknownCategoryLSMDOIndex = column;
                                                        break;
                                                    case "LSMGO":
                                                        UnknownCategoryLMGOIndex = column;
                                                        break;
                                                    case "LSG":
                                                        UnknownCategoryLMGOIndex = column;
                                                        break;
                                                    case "LSGO":
                                                        UnknownCategoryLMGOIndex = column;
                                                        break;
                                                    case "LSMO":
                                                        UnknownCategoryLMGOIndex = column;
                                                        break;
                                                    case "VLS":
                                                        UnknownCategoryVLSFIndex = column;
                                                        break;
                                                    case "VLSFO":
                                                        UnknownCategoryVLSFIndex = column;
                                                        break;
                                                    case "HSFO":
                                                        UnknownCategoryHSFOIndex = column;
                                                        break;
                                                    case "LMGO":
                                                        UnknownCategoryLMGOIndex = column;
                                                        break;
                                                    case "VLSF":
                                                        UnknownCategoryVLSFIndex = column;
                                                        break;
                                                    case "LIFO":
                                                        UnknownCategoryLIFOIndex = column;
                                                        break;
                                                    case "180":
                                                        UnknownCategory180Index = column;
                                                        break;
                                                    case "380":
                                                        UnknownCategory380Index = column;
                                                        break;
                                                    case "CYL":
                                                        UnknownCategoryCYLIndex = column;
                                                        break;
                                                    case "DMA":
                                                        UnknownCategoryDMAIndex = column;
                                                        break;
                                                    case "DMB":
                                                        UnknownCategoryDMBIndex = column;
                                                        break;
                                                    case "DMC":
                                                        UnknownCategoryDMCIndex = column;
                                                        break;
                                                    case "IFO":
                                                        UnknownCategoryIFOIndex = column;
                                                        break;
                                                    case "LSG1":
                                                        UnknownCategoryLSG1Index = column;
                                                        break;
                                                    case "LSMDO":
                                                        UnknownCategoryLSMDOIndex = column;
                                                        break;
                                                    //case "LSIFO":
                                                    //    UnknownCategoryLSIFOIndex = column;
                                                    //    break;
                                                    case "LUB":
                                                        UnknownCategoryLUBIndex = column;
                                                        break;
                                                    case "MDO":
                                                        UnknownCategoryMDOIndex = column;
                                                        break;
                                                    case "MGO":
                                                        UnknownCategoryMGOIndex = column;
                                                        break;
                                                    default:
                                                        UnknownCategoryUnknownIndex = column;
                                                        UnknownCategoryUnknownName = worksheet.Cells[3, column].Value.ToString();
                                                        break;
                                                }

                                                break;

                                        }
                                        break;
                                    case "ReportedSeaSurfaceTemperature":
                                        ReportedSeaSurfaceTemperatureIndex = column;
                                        break;
                                    case "ReportedAirTemperature":
                                        ReportedAirTemperatureIndex = column;
                                        break;
                                    case "ReportedAtmosphericPressure":
                                        ReportedAtmosphericPressureIndex = column;
                                        break;

                                    case "ReportedWindSpeed":
                                        ReportedWindSpeedIndex = column;
                                        break;
                                    case "ReportedWindDirectionTrue":
                                        ReportedWindDirectionTrueIndex = column;
                                        break;
                                    case "ReportedWaveHeight":
                                        ReportedWaveHeightIndex = column;
                                        break;
                                    case "ReportedWaveHeightDirectionTrue":
                                        ReportedWaveHeightDirectionTrueIndex = column;
                                        break;
                                    case "ReportedCurrentSpeed":
                                        ReportedCurrentSpeedIndex = column;
                                        break;

                                    #region analyzedweather 
                                    case "WindDirectionTrue":
                                        WindDirectionTrueIndex = column;
                                        break;
                                    case "AnalysisWindKts":
                                        AnalysisWindKtsIndex = column;
                                        break;
                                    case "WaveHeightDirectionTrue":
                                        WaveHeightDirectionTrueIndex = column;
                                        break;
                                    case "AnalysisCombinedWaveHeightM":
                                        AnalysisCombinedWaveHeightMIndex = column;
                                        break;
                                    case "CurrentFactor":
                                        CurrentFactorIndex = column;
                                        break;
                                    case "AnalysisSwellHeightM":
                                        AnalysisSwellHeightMIndex = column;
                                        break;
                                        #endregion
                                }
                            }
                            #endregion

                            for (int row = 7; row <= rowCount; row++)
                            {
                                Voyages voyages = new Voyages();
                                Forms forms = new Forms();

                                Voyages voyagesToSearch = new Voyages();
                                Forms formsToSearch = new Forms();

                                #region Voyages


                                if (worksheet.Cells[row, VesselNameIndex].Value != null)
                                {
                                    forms.VesselName = worksheet.Cells[row, VesselNameIndex].Value.ToString().Trim();
                                }
                                if (worksheet.Cells[row, VesselIdIndex].Value != null)
                                {
                                    vasselId = worksheet.Cells[row, VesselIdIndex].Value.ToString().Trim();
                                }
                                if (worksheet.Cells[row, PassegidIndex].Value != null)
                                {
                                    passegid = worksheet.Cells[row, PassegidIndex].Value.ToString().Trim();
                                }
                                if (worksheet.Cells[row, VoyageNoIndex].Value != null)
                                {
                                    string[] numbers = Regex.Split(worksheet.Cells[row, VoyageNoIndex].Value.ToString(), @"\D+");
                                    if (numbers.Length > 0)
                                    {
                                        if (Regex.IsMatch(numbers[0], "^[0-9]+$", RegexOptions.Compiled))
                                        {
                                            voyagesToSearch.VoyageNumber = voyages.VoyageNumber = Convert.ToInt64(numbers[0].ToString().Trim());
                                            forms.VoyageNo = Convert.ToInt32(numbers[0].Trim());
                                        }
                                        else if (Regex.IsMatch(numbers[1], "^[0-9]+$", RegexOptions.Compiled))
                                        {
                                            voyagesToSearch.VoyageNumber = voyages.VoyageNumber = Convert.ToInt64(numbers[1].ToString().Trim());
                                            forms.VoyageNo = Convert.ToInt32(numbers[1].Trim());
                                        }
                                    }
                                }
                                if (worksheet.Cells[row, LoadConditionIndex].Value != null)
                                {
                                    forms.VesselCondition = voyages.LoadCondition = worksheet.Cells[row, LoadConditionIndex].Value.ToString().Trim();
                                }
                                if (worksheet.Cells[row, FromPortDescriptionIndex].Value != null)
                                {
                                    fromPortDescription = voyages.DeparturePort = voyagesToSearch.DeparturePort = worksheet.Cells[row, FromPortDescriptionIndex].Value.ToString().Trim();
                                }
                                if (worksheet.Cells[row, ToPortDescriptionIndex].Value != null)
                                {
                                    toPortDescription = voyages.ArrivalPort = voyagesToSearch.ArrivalPort = worksheet.Cells[row, ToPortDescriptionIndex].Value.ToString().Trim();
                                }
                                if (worksheet.Cells[row, UTCOffsetIndex].Value != null)
                                {
                                    utcOffset = SetTimezoneFormate(worksheet.Cells[row, UTCOffsetIndex].Value.ToString().Trim());
                                    forms.ReportTime = utcOffset;
                                }
                                if (worksheet.Cells[row, StartPassageIndex].Value != null)
                                {
                                    if (utcOffset != null)
                                    {
                                        var hrs = Convert.ToDouble(utcOffset.Substring(1, 2));
                                        var min = Convert.ToDouble(utcOffset.Substring(4, 2));
                                        voyages.ActualStartOfSeaPassage = utcOffset.Contains("-") ? Convert.ToDateTime(worksheet.Cells[row, StartPassageIndex].Value.ToString()).AddHours(-hrs).AddMinutes(-min) :
                                            Convert.ToDateTime(worksheet.Cells[row, StartPassageIndex].Value.ToString()).AddHours(hrs).AddMinutes(min);
                                    }
                                }
                                if (worksheet.Cells[row, EndPassageIndex].Value != null)
                                {
                                    if (utcOffset != null)
                                    {
                                        var hrs = Convert.ToDouble(utcOffset.Substring(1, 2));
                                        var min = Convert.ToDouble(utcOffset.Substring(4, 2));
                                        voyages.ActualEndOfSeaPassage = utcOffset.Contains("-") ? Convert.ToDateTime(worksheet.Cells[row, EndPassageIndex].Value.ToString()).AddHours(-hrs).AddMinutes(-min) :
                                            Convert.ToDateTime(worksheet.Cells[row, EndPassageIndex].Value.ToString()).AddHours(hrs).AddMinutes(min);
                                    }
                                }

                                #endregion

                                #region Form

                                if (worksheet.Cells[row, UTCOffsetIndex].Value != null)
                                {
                                    utcOffset = SetTimezoneFormate(worksheet.Cells[row, UTCOffsetIndex].Value.ToString().Trim());
                                    forms.ReportTime = utcOffset;
                                }
                                if (worksheet.Cells[row, ReportDatetimeIndex].Value != null)
                                {
                                    if (Convert.ToDateTime(worksheet.Cells[row, ReportDatetimeIndex].Value.ToString()) >= Convert.ToDateTime("2020-05-01 00:00:00"))
                                    {
                                        break;
                                    }
                                    if (utcOffset != null)
                                    {
                                        var hrs = Convert.ToDouble(utcOffset.Substring(1, 2));
                                        var min = Convert.ToDouble(utcOffset.Substring(4, 2));
                                        DateTime ReportDateTime = utcOffset.Contains("-") ? Convert.ToDateTime(worksheet.Cells[row, ReportDatetimeIndex].Value.ToString()).AddHours(-hrs).AddMinutes(-min) :
                                            Convert.ToDateTime(worksheet.Cells[row, ReportDatetimeIndex].Value.ToString()).AddHours(hrs).AddMinutes(min);
                                        forms.ReportDateTime = new DateTimeOffset(ReportDateTime, TimeSpan.FromHours(0));
                                    }
                                    utcReportdatetime = Convert.ToDateTime(worksheet.Cells[row, ReportDatetimeIndex].Value.ToString());
                                }
                                if (worksheet.Cells[row, LatitudeIndex].Value != null)
                                {
                                    forms.Latitude = DecimalToDegreesLatitudeLongitude.DDtoDMS(Convert.ToDouble(worksheet.Cells[row, LatitudeIndex].Value), CoordinateType.latitude);
                                }
                                if (worksheet.Cells[row, LongitudeIndex].Value != null)
                                {
                                    forms.Longitude = DecimalToDegreesLatitudeLongitude.DDtoDMS(Convert.ToDouble(worksheet.Cells[row, LongitudeIndex].Value), CoordinateType.longitude);
                                }
                                if (worksheet.Cells[row, DraftForwardIndex].Value != null)
                                {
                                    forms.FWDDraft = String.Format("{0:0.00}", Convert.ToDecimal(worksheet.Cells[row, DraftForwardIndex].Value.ToString().Trim()));
                                }

                                if (worksheet.Cells[row, DraftAftIndex].Value != null)
                                {
                                    forms.AFTDraft = String.Format("{0:0.00}", Convert.ToDecimal(worksheet.Cells[row, DraftAftIndex].Value.ToString().Trim()));
                                }
                                if (worksheet.Cells[row, CargoIndex].Value != null)
                                {
                                    if (worksheet.Cells[row, PositionReportTypeIndex].Value.ToString().Contains("Start") &&
                                        worksheet.Cells[row, LoadConditionIndex].Value.ToString().Contains("Laden"))
                                    {
                                        forms.Cargo_Temp = String.Format("{0:0.00}", Convert.ToDecimal(worksheet.Cells[row, CargoIndex].Value.ToString().Trim()));
                                    }
                                }
                                if (worksheet.Cells[row, RPMIndex].Value != null)
                                {
                                    forms.RPM = String.Format("{0:0.00}", Convert.ToDecimal(worksheet.Cells[row, RPMIndex].Value.ToString().Trim()));
                                }
                                if (worksheet.Cells[row, OrderedSpeedIndex].Value != null)
                                {
                                    speed = String.Format("{0:0.00}", Convert.ToDecimal(worksheet.Cells[row, OrderedSpeedIndex].Value.ToString().Trim()));
                                }
                                if (worksheet.Cells[row, SpeedIndex].Value != null)
                                {
                                    forms.ObsSpeed = String.Format("{0:0.00}", Convert.ToDecimal(worksheet.Cells[row, SpeedIndex].Value.ToString().Trim()));
                                }
                                if (worksheet.Cells[row, DistanceIndex].Value != null)
                                {
                                    forms.ObservedDistance = String.Format("{0:0.00}", Convert.ToDecimal(worksheet.Cells[row, DistanceIndex].Value.ToString().Trim()));
                                }
                                if (worksheet.Cells[row, DistanceToGoIndex].Value != null)
                                {
                                    forms.DistanceToGO = String.Format("{0:0.00}", Convert.ToDecimal(worksheet.Cells[row, DistanceToGoIndex].Value.ToString().Trim()));
                                }
                                if (worksheet.Cells[row, EngineDistanceIndex].Value != null)
                                {
                                    forms.EngineDistance = String.Format("{0:0.00}", Convert.ToDecimal(worksheet.Cells[row, EngineDistanceIndex].Value.ToString().Trim()));
                                }
                                if (worksheet.Cells[row, SlipIndex].Value != null)
                                {
                                    forms.Slip = String.Format("{0:0.00}", Convert.ToDecimal(worksheet.Cells[row, SlipIndex].Value.ToString()));
                                }


                                //Added new code

                                if (worksheet.Cells[row, CommentsIndex].Value != null)
                                {
                                    forms.Remarks = worksheet.Cells[row, CommentsIndex].Value.ToString().Trim();
                                }
                                if (worksheet.Cells[row, HoursIndex].Value != null)
                                {
                                    forms.SteamingHrs = String.Format("{0:0.00}", Convert.ToDecimal(worksheet.Cells[row, HoursIndex].Value.ToString().Trim()));
                                }
                                if (worksheet.Cells[row, OrderedSpeedIndex].Value != null)
                                {
                                    forms.CPSpeed = String.Format("{0:0.00}", Convert.ToDecimal(worksheet.Cells[row, OrderedSpeedIndex].Value.ToString().Trim())); //ordered speed
                                }
                                if (worksheet.Cells[row, COGIndex].Value != null)
                                {
                                    forms.Heading = String.Format("{0:0.00}", Convert.ToDecimal(worksheet.Cells[row, COGIndex].Value.ToString().Trim())); // COG
                                }
                                if (worksheet.Cells[row, PowerIndex].Value != null)
                                {
                                    forms.AvgBHP = String.Format("{0:0}", Convert.ToDecimal(worksheet.Cells[row, PowerIndex].Value.ToString())); // Power
                                }

                                if (worksheet.Cells[row, ReportedSeaSurfaceTemperatureIndex].Value != null)
                                {
                                    forms.SeaTemp = String.Format("{0:0.00}", Convert.ToDecimal(worksheet.Cells[row, ReportedSeaSurfaceTemperatureIndex].Value.ToString().Trim())); //ordered speed
                                }
                                if (worksheet.Cells[row, ReportedAirTemperatureIndex].Value != null)
                                {
                                    forms.AirTemp = String.Format("{0:0.00}", Convert.ToDecimal(worksheet.Cells[row, ReportedAirTemperatureIndex].Value.ToString().Trim())); // COG
                                }
                                if (worksheet.Cells[row, ReportedAtmosphericPressureIndex].Value != null)
                                {
                                    forms.BaroPressure = String.Format("{0:0}", Convert.ToDecimal(worksheet.Cells[row, ReportedAtmosphericPressureIndex].Value.ToString())); // Power
                                }

                                if (worksheet.Cells[row, ReportedWindSpeedIndex].Value != null)
                                {
                                    forms.WindForce = String.Format("{0:0.00}", Convert.ToDecimal(worksheet.Cells[row, ReportedWindSpeedIndex].Value.ToString().Trim()));
                                }
                                if (worksheet.Cells[row, ReportedWindDirectionTrueIndex].Value != null)
                                {
                                    forms.WindDirection = String.Format("{0:0.00}", Convert.ToDecimal(worksheet.Cells[row, ReportedWindDirectionTrueIndex].Value.ToString().Trim()));
                                }
                                if (worksheet.Cells[row, ReportedWaveHeightIndex].Value != null)
                                {
                                    forms.Swell = String.Format("{0:0.00}", Convert.ToDecimal(worksheet.Cells[row, ReportedWaveHeightIndex].Value.ToString().Trim()));
                                }
                                if (worksheet.Cells[row, ReportedWaveHeightDirectionTrueIndex].Value != null)
                                {
                                    forms.SwellDir = String.Format("{0:0.00}", Convert.ToDecimal(worksheet.Cells[row, ReportedWaveHeightDirectionTrueIndex].Value.ToString().Trim()));
                                }
                                if (worksheet.Cells[row, ReportedCurrentSpeedIndex].Value != null)
                                {
                                    forms.Current = String.Format("{0:0}", Convert.ToDecimal(worksheet.Cells[row, ReportedCurrentSpeedIndex].Value.ToString()));
                                }
                                #endregion

                                #region Analyzedweather 
                                AnalyzedWeather objAnalyzedweather = new AnalyzedWeather();
                                if (worksheet.Cells[row, WindDirectionTrueIndex].Value != null)
                                {
                                    objAnalyzedweather.AnalyzedWindDirection = String.Format("{0:0.00}", Convert.ToDecimal(worksheet.Cells[row, WindDirectionTrueIndex].Value.ToString().Trim()));
                                }
                                if (worksheet.Cells[row, AnalysisWindKtsIndex].Value != null)
                                {
                                    objAnalyzedweather.AnalyzedWind = String.Format("{0:0.00}", Convert.ToDecimal(worksheet.Cells[row, AnalysisWindKtsIndex].Value.ToString().Trim()));
                                }
                                if (worksheet.Cells[row, WaveHeightDirectionTrueIndex].Value != null)
                                {
                                    objAnalyzedweather.AnalyzedWaveDiection = String.Format("{0:0.00}", Convert.ToDecimal(worksheet.Cells[row, WaveHeightDirectionTrueIndex].Value.ToString().Trim()));
                                }
                                if (worksheet.Cells[row, AnalysisCombinedWaveHeightMIndex].Value != null)
                                {
                                    objAnalyzedweather.AnalyzedWave = String.Format("{0:0.00}", Convert.ToDecimal(worksheet.Cells[row, AnalysisCombinedWaveHeightMIndex].Value.ToString().Trim()));
                                }
                                if (worksheet.Cells[row, CurrentFactorIndex].Value != null)
                                {
                                    objAnalyzedweather.AnalyzedCurrent = String.Format("{0:0.00}", Convert.ToDecimal(worksheet.Cells[row, CurrentFactorIndex].Value.ToString().Trim()));
                                }

                                if (worksheet.Cells[row, AnalysisSwellHeightMIndex].Value != null)
                                {
                                    objAnalyzedweather.AnalyzedSwellHeight = String.Format("{0:0.00}", Convert.ToDecimal(worksheet.Cells[row, AnalysisSwellHeightMIndex].Value.ToString().Trim()));
                                }
                                #endregion

                                Robs robs = new Robs();
                                Rob rob = new Rob();
                                Rob robbinker;
                                Allocation allocation = new Allocation();

                                robs.Rob = new List<Rob>();
                                robs.Rob = new List<Rob>();

                                #region FluidConsuptionForBunker


                                if (RobHSFOIndex != 0 && worksheet.Cells[row, RobHSFOIndex].Value != null)
                                {
                                    robbinker = new Rob();
                                    robbinker.FuelType = "HSFO"; //fueltype
                                    robbinker.Units = worksheet.Cells[6, RobHSFOIndex].Value.ToString().Trim(); // MT
                                    robbinker.Remaining = worksheet.Cells[row, RobHSFOIndex].Value.ToString().Trim(); // consumption
                                    robs.Rob.Add(robbinker);
                                }
                                if (RobLMGOIndex != 0 && worksheet.Cells[row, RobLMGOIndex].Value != null)
                                {
                                    robbinker = new Rob();
                                    robbinker.FuelType = "LSMGO"; //fueltype
                                    robbinker.Units = worksheet.Cells[6, RobLMGOIndex].Value.ToString().Trim(); // MT
                                    robbinker.Remaining = worksheet.Cells[row, RobLMGOIndex].Value.ToString().Trim(); // consumption
                                    robs.Rob.Add(robbinker);

                                }
                                if (RobVLSFIndex != 0 && worksheet.Cells[row, RobVLSFIndex].Value != null)
                                {
                                    robbinker = new Rob();
                                    robbinker.FuelType = "VLSFO"; //fueltype
                                    robbinker.Units = worksheet.Cells[6, RobVLSFIndex].Value.ToString().Trim(); // MT
                                    robbinker.Remaining = worksheet.Cells[row, RobVLSFIndex].Value.ToString().Trim(); // consumption
                                    robs.Rob.Add(robbinker);
                                }

                                if (RobLIFOIndex != 0 && worksheet.Cells[row, RobLIFOIndex].Value != null)
                                {
                                    robbinker = new Rob();
                                    robbinker.FuelType = "LSIFO"; //fueltype
                                    robbinker.Units = worksheet.Cells[6, RobLIFOIndex].Value.ToString().Trim(); // MT
                                    robbinker.Remaining = worksheet.Cells[row, RobLIFOIndex].Value.ToString().Trim(); // consumption
                                    robs.Rob.Add(robbinker);
                                }

                                if (Rob180Index != 0 && worksheet.Cells[row, Rob180Index].Value != null)
                                {
                                    robbinker = new Rob();
                                    robbinker.FuelType = "180"; //fueltype
                                    robbinker.Units = worksheet.Cells[6, Rob180Index].Value.ToString().Trim(); // MT
                                    robbinker.Remaining = worksheet.Cells[row, Rob180Index].Value.ToString().Trim(); // consumption
                                    robs.Rob.Add(robbinker);

                                }

                                if (Rob380Index != 0 && worksheet.Cells[row, Rob380Index].Value != null)
                                {
                                    robbinker = new Rob();
                                    robbinker.FuelType = "VLSFO"; //fueltype
                                    robbinker.Units = worksheet.Cells[6, Rob380Index].Value.ToString().Trim(); // MT
                                    robbinker.Remaining = worksheet.Cells[row, Rob380Index].Value.ToString().Trim(); // consumption
                                    robs.Rob.Add(robbinker);
                                }

                                if (RobCYLIndex != 0 && worksheet.Cells[row, RobCYLIndex].Value != null)
                                {
                                    robbinker = new Rob();
                                    robbinker.FuelType = "CYL"; //fueltype
                                    robbinker.Units = worksheet.Cells[6, RobCYLIndex].Value.ToString().Trim(); // MT
                                    robbinker.Remaining = worksheet.Cells[row, RobCYLIndex].Value.ToString().Trim(); // consumption
                                    robs.Rob.Add(robbinker);
                                }

                                if (RobDMAIndex != 0 && worksheet.Cells[row, RobDMAIndex].Value != null)
                                {
                                    robbinker = new Rob();
                                    robbinker.FuelType = "DMA"; //fueltype
                                    robbinker.Units = worksheet.Cells[6, RobDMAIndex].Value.ToString().Trim(); // MT
                                    robbinker.Remaining = worksheet.Cells[row, RobDMAIndex].Value.ToString().Trim(); // consumption
                                    robs.Rob.Add(robbinker);

                                }

                                if (RobDMBIndex != 0 && worksheet.Cells[row, RobDMBIndex].Value != null)
                                {
                                    robbinker = new Rob();
                                    robbinker.FuelType = "DMB"; //fueltype
                                    robbinker.Units = worksheet.Cells[6, RobDMBIndex].Value.ToString().Trim(); // MT
                                    robbinker.Remaining = worksheet.Cells[row, RobDMBIndex].Value.ToString().Trim(); // consumption
                                    robs.Rob.Add(robbinker);
                                }

                                if (RobDMCIndex != 0 && worksheet.Cells[row, RobDMCIndex].Value != null)
                                {
                                    robbinker = new Rob();
                                    robbinker.FuelType = "DMC"; //fueltype
                                    robbinker.Units = worksheet.Cells[6, RobDMCIndex].Value.ToString().Trim(); // MT
                                    robbinker.Remaining = worksheet.Cells[row, RobDMCIndex].Value.ToString().Trim(); // consumption
                                    robs.Rob.Add(robbinker);
                                }

                                if (RobIFOIndex != 0 && worksheet.Cells[row, RobIFOIndex].Value != null)
                                {
                                    robbinker = new Rob();
                                    robbinker.FuelType = "IFO"; //fueltype
                                    robbinker.Units = worksheet.Cells[6, RobIFOIndex].Value.ToString().Trim(); // MT
                                    robbinker.Remaining = worksheet.Cells[row, RobIFOIndex].Value.ToString().Trim(); // consumption
                                    robs.Rob.Add(robbinker);

                                }

                                if (RobLSG1Index != 0 && worksheet.Cells[row, RobLSG1Index].Value != null)
                                {
                                    robbinker = new Rob();
                                    robbinker.FuelType = "LSG1"; //fueltype
                                    robbinker.Units = worksheet.Cells[6, RobLSG1Index].Value.ToString().Trim(); // MT
                                    robbinker.Remaining = worksheet.Cells[row, RobLSG1Index].Value.ToString().Trim(); // consumption
                                    robs.Rob.Add(robbinker);
                                }

                                if (RobLSMDOIndex != 0 && worksheet.Cells[row, RobLSMDOIndex].Value != null)
                                {
                                    robbinker = new Rob();
                                    robbinker.FuelType = "LSMDO"; //fueltype
                                    robbinker.Units = worksheet.Cells[6, RobLSMDOIndex].Value.ToString().Trim(); // MT
                                    robbinker.Remaining = worksheet.Cells[row, RobLSMDOIndex].Value.ToString().Trim(); // consumption
                                    robs.Rob.Add(robbinker);
                                }

                                if (RobLSIFOIndex != 0 && worksheet.Cells[row, RobLSIFOIndex].Value != null)
                                {
                                    robbinker = new Rob();
                                    robbinker.FuelType = "LSIFO"; //fueltype
                                    robbinker.Units = worksheet.Cells[6, RobLSIFOIndex].Value.ToString().Trim(); // MT
                                    robbinker.Remaining = worksheet.Cells[row, RobLSIFOIndex].Value.ToString().Trim(); // consumption
                                    robs.Rob.Add(robbinker);
                                }

                                if (RobLUBIndex != 0 && worksheet.Cells[row, RobLUBIndex].Value != null)
                                {
                                    robbinker = new Rob();
                                    robbinker.FuelType = "LUB"; //fueltype
                                    robbinker.Units = worksheet.Cells[6, RobLUBIndex].Value.ToString().Trim(); // MT
                                    robbinker.Remaining = worksheet.Cells[row, RobLUBIndex].Value.ToString().Trim(); // consumption
                                    robs.Rob.Add(robbinker);

                                }

                                if (RobMDOIndex != 0 && worksheet.Cells[row, RobMDOIndex].Value != null)
                                {
                                    robbinker = new Rob();
                                    robbinker.FuelType = "MDO"; //fueltype
                                    robbinker.Units = worksheet.Cells[6, RobMDOIndex].Value.ToString().Trim(); // MT
                                    robbinker.Remaining = worksheet.Cells[row, RobMDOIndex].Value.ToString().Trim(); // consumption
                                    robs.Rob.Add(robbinker);
                                }

                                if (RobMGOIndex != 0 && worksheet.Cells[row, RobMGOIndex].Value != null)
                                {
                                    robbinker = new Rob();
                                    robbinker.FuelType = "MGO"; //fueltype
                                    robbinker.Units = worksheet.Cells[6, RobMGOIndex].Value.ToString().Trim(); // MT
                                    robbinker.Remaining = worksheet.Cells[row, RobMGOIndex].Value.ToString().Trim(); // consumption
                                    robs.Rob.Add(robbinker);
                                }

                                if (RobUnknownIndex != 0 && worksheet.Cells[row, RobUnknownIndex].Value != null)
                                {
                                    robbinker = new Rob();
                                    robbinker.FuelType = RobUnknownName; //fueltype
                                    robbinker.Units = worksheet.Cells[6, RobUnknownIndex].Value.ToString().Trim(); // MT
                                    robbinker.Remaining = worksheet.Cells[row, RobUnknownIndex].Value.ToString().Trim(); // consumption
                                    robs.Rob.Add(robbinker);
                                    sendEmails("Rob", RobUnknownName, (worksheet.Cells[row, VesselNameIndex].Value.ToString().Trim()), utcReportdatetime);
                                }


                                #endregion

                                #region FluidConsuption

                                #region MAN 
                                if (ManeuverHSFOIndex != 0 && worksheet.Cells[row, ManeuverHSFOIndex].Value != null)
                                {
                                    RobDataDump(ref robs, "HSFO", ref rob, ref allocation, "Maneuver",
                                        worksheet.Cells[row, ManeuverHSFOIndex].Value.ToString(),
                                        worksheet.Cells[6, ManeuverHSFOIndex].Value.ToString().Trim());
                                }
                                if (ManeuverLMGOIndex != 0 && worksheet.Cells[row, ManeuverLMGOIndex].Value != null)
                                {
                                    RobDataDump(ref robs, "LSMGO", ref rob, ref allocation, "Maneuver",
                                       worksheet.Cells[row, ManeuverLMGOIndex].Value.ToString(),
                                       worksheet.Cells[6, ManeuverLMGOIndex].Value.ToString().Trim());
                                }
                                if (ManeuverVLSFIndex != 0 && worksheet.Cells[row, ManeuverVLSFIndex].Value != null)
                                {
                                    RobDataDump(ref robs, "VLSFO", ref rob, ref allocation, "Maneuver",
                                      worksheet.Cells[row, ManeuverVLSFIndex].Value.ToString(),
                                      worksheet.Cells[6, ManeuverVLSFIndex].Value.ToString().Trim());
                                }

                                if (ManeuverLIFOIndex != 0 && worksheet.Cells[row, ManeuverLIFOIndex].Value != null)
                                {
                                    RobDataDump(ref robs, "LSIFO", ref rob, ref allocation, "Maneuver",
                                        worksheet.Cells[row, ManeuverLIFOIndex].Value.ToString(),
                                        worksheet.Cells[6, ManeuverLIFOIndex].Value.ToString().Trim());
                                }
                                if (Maneuver180Index != 0 && worksheet.Cells[row, Maneuver180Index].Value != null)
                                {
                                    RobDataDump(ref robs, "180", ref rob, ref allocation, "Maneuver",
                                       worksheet.Cells[row, Maneuver180Index].Value.ToString(),
                                       worksheet.Cells[6, Maneuver180Index].Value.ToString().Trim());
                                }
                                if (Maneuver380Index != 0 && worksheet.Cells[row, Maneuver380Index].Value != null)
                                {
                                    RobDataDump(ref robs, "380", ref rob, ref allocation, "Maneuver",
                                      worksheet.Cells[row, Maneuver380Index].Value.ToString(),
                                      worksheet.Cells[6, Maneuver380Index].Value.ToString().Trim());
                                }

                                if (ManeuverDMAIndex != 0 && worksheet.Cells[row, ManeuverDMAIndex].Value != null)
                                {
                                    RobDataDump(ref robs, "DMA", ref rob, ref allocation, "Maneuver",
                                        worksheet.Cells[row, ManeuverDMAIndex].Value.ToString(),
                                        worksheet.Cells[6, ManeuverDMAIndex].Value.ToString().Trim());
                                }
                                if (ManeuverDMBIndex != 0 && worksheet.Cells[row, ManeuverDMBIndex].Value != null)
                                {
                                    RobDataDump(ref robs, "DMB", ref rob, ref allocation, "Maneuver",
                                       worksheet.Cells[row, ManeuverDMBIndex].Value.ToString(),
                                       worksheet.Cells[6, ManeuverDMBIndex].Value.ToString().Trim());
                                }
                                if (ManeuverDMCIndex != 0 && worksheet.Cells[row, ManeuverDMCIndex].Value != null)
                                {
                                    RobDataDump(ref robs, "DMC", ref rob, ref allocation, "Maneuver",
                                      worksheet.Cells[row, ManeuverDMCIndex].Value.ToString(),
                                      worksheet.Cells[6, ManeuverDMCIndex].Value.ToString().Trim());
                                }

                                if (ManeuverIFOIndex != 0 && worksheet.Cells[row, ManeuverIFOIndex].Value != null)
                                {
                                    RobDataDump(ref robs, "IFO", ref rob, ref allocation, "Maneuver",
                                        worksheet.Cells[row, ManeuverIFOIndex].Value.ToString(),
                                        worksheet.Cells[6, ManeuverIFOIndex].Value.ToString().Trim());
                                }
                                if (ManeuverLSG1Index != 0 && worksheet.Cells[row, ManeuverLSG1Index].Value != null)
                                {
                                    RobDataDump(ref robs, "LSG1", ref rob, ref allocation, "Maneuver",
                                       worksheet.Cells[row, ManeuverLSG1Index].Value.ToString(),
                                       worksheet.Cells[6, ManeuverLSG1Index].Value.ToString().Trim());
                                }
                                if (ManeuverLSMDOIndex != 0 && worksheet.Cells[row, ManeuverLSMDOIndex].Value != null)
                                {
                                    RobDataDump(ref robs, "LSMDO", ref rob, ref allocation, "Maneuver",
                                      worksheet.Cells[row, ManeuverLSMDOIndex].Value.ToString(),
                                      worksheet.Cells[6, ManeuverLSMDOIndex].Value.ToString().Trim());
                                }

                                if (ManeuverLSIFOIndex != 0 && worksheet.Cells[row, ManeuverLSIFOIndex].Value != null)
                                {
                                    RobDataDump(ref robs, "LSIFO", ref rob, ref allocation, "Maneuver",
                                        worksheet.Cells[row, ManeuverLSIFOIndex].Value.ToString(),
                                        worksheet.Cells[6, ManeuverLSIFOIndex].Value.ToString().Trim());
                                }
                                if (ManeuverLUBIndex != 0 && worksheet.Cells[row, ManeuverLUBIndex].Value != null)
                                {
                                    RobDataDump(ref robs, "LUB", ref rob, ref allocation, "Maneuver",
                                       worksheet.Cells[row, ManeuverLUBIndex].Value.ToString(),
                                       worksheet.Cells[6, ManeuverLUBIndex].Value.ToString().Trim());
                                }
                                if (ManeuverMDOIndex != 0 && worksheet.Cells[row, ManeuverMDOIndex].Value != null)
                                {
                                    RobDataDump(ref robs, "MDO", ref rob, ref allocation, "Maneuver",
                                      worksheet.Cells[row, ManeuverMDOIndex].Value.ToString(),
                                      worksheet.Cells[6, ManeuverMDOIndex].Value.ToString().Trim());
                                }
                                if (ManeuverMGOIndex != 0 && worksheet.Cells[row, ManeuverMGOIndex].Value != null)
                                {
                                    RobDataDump(ref robs, "MGO", ref rob, ref allocation, "Maneuver",
                                        worksheet.Cells[row, ManeuverMGOIndex].Value.ToString(),
                                        worksheet.Cells[6, ManeuverMGOIndex].Value.ToString().Trim());
                                }

                                if (ManeuverUnknownIndex != 0 && worksheet.Cells[row, ManeuverUnknownIndex].Value != null)
                                {
                                    RobDataDump(ref robs, ManeuverUnknownName, ref rob, ref allocation, "Maneuver",
                                        worksheet.Cells[row, ManeuverUnknownIndex].Value.ToString(),
                                        worksheet.Cells[6, ManeuverUnknownIndex].Value.ToString().Trim());
                                    sendEmails("Maneuver", ManeuverUnknownName, (worksheet.Cells[row, VesselNameIndex].Value.ToString().Trim()), utcReportdatetime);
                                }
                                #endregion

                                #region PROP

                                if (PropulsionUnknownIndex != 0 && worksheet.Cells[row, PropulsionUnknownIndex].Value != null)
                                {
                                    RobDataDump(ref robs, PropulsionUnknownName, ref rob, ref allocation, "Propulsion",
                                        worksheet.Cells[row, PropulsionUnknownIndex].Value.ToString(),
                                        worksheet.Cells[6, PropulsionUnknownIndex].Value.ToString().Trim());
                                    sendEmails("Propulsion", PropulsionUnknownName, (worksheet.Cells[row, VesselNameIndex].Value.ToString().Trim()), utcReportdatetime);
                                }

                                if (PropulsionHSFOIndex != 0 && worksheet.Cells[row, PropulsionHSFOIndex].Value != null)
                                {
                                    RobDataDump(ref robs, "HSFO", ref rob, ref allocation, "Propulsion",
                                        worksheet.Cells[row, PropulsionHSFOIndex].Value.ToString(),
                                        worksheet.Cells[6, PropulsionHSFOIndex].Value.ToString().Trim());
                                }
                                if (PropulsionLMGOIndex != 0 && worksheet.Cells[row, PropulsionLMGOIndex].Value != null)
                                {
                                    RobDataDump(ref robs, "LSMGO", ref rob, ref allocation, "Propulsion",
                                       worksheet.Cells[row, PropulsionLMGOIndex].Value.ToString(),
                                       worksheet.Cells[6, PropulsionLMGOIndex].Value.ToString().Trim());
                                }
                                if (PropulsionVLSFIndex != 0 && worksheet.Cells[row, PropulsionVLSFIndex].Value != null)
                                {
                                    RobDataDump(ref robs, "VLSFO", ref rob, ref allocation, "Propulsion",
                                      worksheet.Cells[row, PropulsionVLSFIndex].Value.ToString(),
                                      worksheet.Cells[6, PropulsionVLSFIndex].Value.ToString().Trim());
                                }

                                if (PropulsionLIFOIndex != 0 && worksheet.Cells[row, PropulsionLIFOIndex].Value != null)
                                {
                                    RobDataDump(ref robs, "LSIFO", ref rob, ref allocation, "Propulsion",
                                        worksheet.Cells[row, PropulsionLIFOIndex].Value.ToString(),
                                        worksheet.Cells[6, PropulsionLIFOIndex].Value.ToString().Trim());
                                }
                                if (Propulsion180Index != 0 && worksheet.Cells[row, Propulsion180Index].Value != null)
                                {
                                    RobDataDump(ref robs, "180", ref rob, ref allocation, "Propulsion",
                                       worksheet.Cells[row, Propulsion180Index].Value.ToString(),
                                       worksheet.Cells[6, Propulsion180Index].Value.ToString().Trim());
                                }
                                if (Propulsion380Index != 0 && worksheet.Cells[row, Propulsion380Index].Value != null)
                                {
                                    RobDataDump(ref robs, "380", ref rob, ref allocation, "Propulsion",
                                      worksheet.Cells[row, Propulsion380Index].Value.ToString(),
                                      worksheet.Cells[6, Propulsion380Index].Value.ToString().Trim());
                                }

                                if (PropulsionDMAIndex != 0 && worksheet.Cells[row, PropulsionDMAIndex].Value != null)
                                {
                                    RobDataDump(ref robs, "DMA", ref rob, ref allocation, "Propulsion",
                                        worksheet.Cells[row, PropulsionDMAIndex].Value.ToString(),
                                        worksheet.Cells[6, PropulsionDMAIndex].Value.ToString().Trim());
                                }
                                if (PropulsionDMBIndex != 0 && worksheet.Cells[row, PropulsionDMBIndex].Value != null)
                                {
                                    RobDataDump(ref robs, "DMB", ref rob, ref allocation, "Propulsion",
                                       worksheet.Cells[row, PropulsionDMBIndex].Value.ToString(),
                                       worksheet.Cells[6, PropulsionDMBIndex].Value.ToString().Trim());
                                }
                                if (PropulsionDMCIndex != 0 && worksheet.Cells[row, PropulsionDMCIndex].Value != null)
                                {
                                    RobDataDump(ref robs, "DMC", ref rob, ref allocation, "Propulsion",
                                      worksheet.Cells[row, PropulsionDMCIndex].Value.ToString(),
                                      worksheet.Cells[6, PropulsionDMCIndex].Value.ToString().Trim());
                                }

                                if (PropulsionIFOIndex != 0 && worksheet.Cells[row, PropulsionIFOIndex].Value != null)
                                {
                                    RobDataDump(ref robs, "IFO", ref rob, ref allocation, "Propulsion",
                                        worksheet.Cells[row, PropulsionIFOIndex].Value.ToString(),
                                        worksheet.Cells[6, PropulsionIFOIndex].Value.ToString().Trim());
                                }
                                if (PropulsionLSG1Index != 0 && worksheet.Cells[row, PropulsionLSG1Index].Value != null)
                                {
                                    RobDataDump(ref robs, "LSG1", ref rob, ref allocation, "Propulsion",
                                       worksheet.Cells[row, PropulsionLSG1Index].Value.ToString(),
                                       worksheet.Cells[6, PropulsionLSG1Index].Value.ToString().Trim());
                                }
                                if (PropulsionLSMDOIndex != 0 && worksheet.Cells[row, PropulsionLSMDOIndex].Value != null)
                                {
                                    RobDataDump(ref robs, "LSMDO", ref rob, ref allocation, "Propulsion",
                                      worksheet.Cells[row, PropulsionLSMDOIndex].Value.ToString(),
                                      worksheet.Cells[6, PropulsionLSMDOIndex].Value.ToString().Trim());
                                }

                                if (PropulsionLSIFOIndex != 0 && worksheet.Cells[row, PropulsionLSIFOIndex].Value != null)
                                {
                                    RobDataDump(ref robs, "LSIFO", ref rob, ref allocation, "Propulsion",
                                        worksheet.Cells[row, PropulsionLSIFOIndex].Value.ToString(),
                                        worksheet.Cells[6, PropulsionLSIFOIndex].Value.ToString().Trim());
                                }
                                if (PropulsionLUBIndex != 0 && worksheet.Cells[row, PropulsionLUBIndex].Value != null)
                                {
                                    RobDataDump(ref robs, "LUB", ref rob, ref allocation, "Propulsion",
                                       worksheet.Cells[row, PropulsionLUBIndex].Value.ToString(),
                                       worksheet.Cells[6, PropulsionLUBIndex].Value.ToString().Trim());
                                }
                                if (PropulsionMDOIndex != 0 && worksheet.Cells[row, PropulsionMDOIndex].Value != null)
                                {
                                    RobDataDump(ref robs, "MDO", ref rob, ref allocation, "Propulsion",
                                      worksheet.Cells[row, PropulsionMDOIndex].Value.ToString(),
                                      worksheet.Cells[6, PropulsionMDOIndex].Value.ToString().Trim());
                                }
                                if (PropulsionMGOIndex != 0 && worksheet.Cells[row, PropulsionMGOIndex].Value != null)
                                {
                                    RobDataDump(ref robs, "MGO", ref rob, ref allocation, "Propulsion",
                                        worksheet.Cells[row, PropulsionMGOIndex].Value.ToString(),
                                        worksheet.Cells[6, PropulsionMGOIndex].Value.ToString().Trim());
                                }

                                #endregion

                                #region Boiler

                                if (BoilerUnknownIndex != 0 && worksheet.Cells[row, BoilerUnknownIndex].Value != null)
                                {
                                    RobDataDump(ref robs, BoilerUnknownName, ref rob, ref allocation, "Boiler",
                                        worksheet.Cells[row, BoilerUnknownIndex].Value.ToString(),
                                        worksheet.Cells[6, BoilerUnknownIndex].Value.ToString().Trim());
                                    sendEmails("Boiler", BoilerUnknownName, (worksheet.Cells[row, VesselNameIndex].Value.ToString().Trim()), utcReportdatetime);
                                }

                                if (BoilerHSFOIndex != 0 && worksheet.Cells[row, BoilerHSFOIndex].Value != null)
                                {
                                    RobDataDump(ref robs, "HSFO", ref rob, ref allocation, "Boiler",
                                        worksheet.Cells[row, BoilerHSFOIndex].Value.ToString(),
                                        worksheet.Cells[6, BoilerHSFOIndex].Value.ToString().Trim());
                                }
                                if (BoilerLMGOIndex != 0 && worksheet.Cells[row, BoilerLMGOIndex].Value != null)
                                {
                                    RobDataDump(ref robs, "LSMGO", ref rob, ref allocation, "Boiler",
                                       worksheet.Cells[row, BoilerLMGOIndex].Value.ToString(),
                                       worksheet.Cells[6, BoilerLMGOIndex].Value.ToString().Trim());
                                }
                                if (BoilerVLSFIndex != 0 && worksheet.Cells[row, BoilerVLSFIndex].Value != null)
                                {
                                    RobDataDump(ref robs, "VLSFO", ref rob, ref allocation, "Boiler",
                                      worksheet.Cells[row, BoilerVLSFIndex].Value.ToString(),
                                      worksheet.Cells[6, BoilerVLSFIndex].Value.ToString().Trim());
                                }

                                if (BoilerLIFOIndex != 0 && worksheet.Cells[row, BoilerLIFOIndex].Value != null)
                                {
                                    RobDataDump(ref robs, "LSIFO", ref rob, ref allocation, "Boiler",
                                        worksheet.Cells[row, BoilerLIFOIndex].Value.ToString(),
                                        worksheet.Cells[6, BoilerLIFOIndex].Value.ToString().Trim());
                                }
                                if (Boiler180Index != 0 && worksheet.Cells[row, Boiler180Index].Value != null)
                                {
                                    RobDataDump(ref robs, "180", ref rob, ref allocation, "Boiler",
                                       worksheet.Cells[row, Boiler180Index].Value.ToString(),
                                       worksheet.Cells[6, Boiler180Index].Value.ToString().Trim());
                                }
                                if (Boiler380Index != 0 && worksheet.Cells[row, Boiler380Index].Value != null)
                                {
                                    RobDataDump(ref robs, "380", ref rob, ref allocation, "Boiler",
                                      worksheet.Cells[row, Boiler380Index].Value.ToString(),
                                      worksheet.Cells[6, Boiler380Index].Value.ToString().Trim());
                                }

                                if (BoilerDMAIndex != 0 && worksheet.Cells[row, BoilerDMAIndex].Value != null)
                                {
                                    RobDataDump(ref robs, "DMA", ref rob, ref allocation, "Boiler",
                                        worksheet.Cells[row, BoilerDMAIndex].Value.ToString(),
                                        worksheet.Cells[6, BoilerDMAIndex].Value.ToString().Trim());
                                }
                                if (BoilerDMBIndex != 0 && worksheet.Cells[row, BoilerDMBIndex].Value != null)
                                {
                                    RobDataDump(ref robs, "DMB", ref rob, ref allocation, "Boiler",
                                       worksheet.Cells[row, BoilerDMBIndex].Value.ToString(),
                                       worksheet.Cells[6, BoilerDMBIndex].Value.ToString().Trim());
                                }
                                if (BoilerDMCIndex != 0 && worksheet.Cells[row, BoilerDMCIndex].Value != null)
                                {
                                    RobDataDump(ref robs, "DMC", ref rob, ref allocation, "Boiler",
                                      worksheet.Cells[row, BoilerDMCIndex].Value.ToString(),
                                      worksheet.Cells[6, BoilerDMCIndex].Value.ToString().Trim());
                                }

                                if (BoilerIFOIndex != 0 && worksheet.Cells[row, BoilerIFOIndex].Value != null)
                                {
                                    RobDataDump(ref robs, "IFO", ref rob, ref allocation, "Boiler",
                                        worksheet.Cells[row, BoilerIFOIndex].Value.ToString(),
                                        worksheet.Cells[6, BoilerIFOIndex].Value.ToString().Trim());
                                }
                                if (BoilerLSG1Index != 0 && worksheet.Cells[row, BoilerLSG1Index].Value != null)
                                {
                                    RobDataDump(ref robs, "LSG1", ref rob, ref allocation, "Boiler",
                                       worksheet.Cells[row, BoilerLSG1Index].Value.ToString(),
                                       worksheet.Cells[6, BoilerLSG1Index].Value.ToString().Trim());
                                }
                                if (BoilerLSMDOIndex != 0 && worksheet.Cells[row, BoilerLSMDOIndex].Value != null)
                                {
                                    RobDataDump(ref robs, "LSMDO", ref rob, ref allocation, "Boiler",
                                      worksheet.Cells[row, BoilerLSMDOIndex].Value.ToString(),
                                      worksheet.Cells[6, BoilerLSMDOIndex].Value.ToString().Trim());
                                }

                                if (BoilerLSIFOIndex != 0 && worksheet.Cells[row, BoilerLSIFOIndex].Value != null)
                                {
                                    RobDataDump(ref robs, "LSIFO", ref rob, ref allocation, "Boiler",
                                        worksheet.Cells[row, BoilerLSIFOIndex].Value.ToString(),
                                        worksheet.Cells[6, BoilerLSIFOIndex].Value.ToString().Trim());
                                }
                                if (BoilerLUBIndex != 0 && worksheet.Cells[row, BoilerLUBIndex].Value != null)
                                {
                                    RobDataDump(ref robs, "LUB", ref rob, ref allocation, "Boiler",
                                       worksheet.Cells[row, BoilerLUBIndex].Value.ToString(),
                                       worksheet.Cells[6, BoilerLUBIndex].Value.ToString().Trim());
                                }
                                if (BoilerMDOIndex != 0 && worksheet.Cells[row, BoilerMDOIndex].Value != null)
                                {
                                    RobDataDump(ref robs, "MDO", ref rob, ref allocation, "Boiler",
                                      worksheet.Cells[row, BoilerMDOIndex].Value.ToString(),
                                      worksheet.Cells[6, BoilerMDOIndex].Value.ToString().Trim());
                                }
                                if (BoilerMGOIndex != 0 && worksheet.Cells[row, BoilerMGOIndex].Value != null)
                                {
                                    RobDataDump(ref robs, "MGO", ref rob, ref allocation, "Boiler",
                                        worksheet.Cells[row, BoilerMGOIndex].Value.ToString(),
                                        worksheet.Cells[6, BoilerMGOIndex].Value.ToString().Trim());
                                }


                                #endregion

                                #region Deballast

                                if (DeballastUnknownIndex != 0 && worksheet.Cells[row, DeballastUnknownIndex].Value != null)
                                {
                                    RobDataDump(ref robs, DeballastUnknownName, ref rob, ref allocation, "Deballast",
                                        worksheet.Cells[row, DeballastUnknownIndex].Value.ToString(),
                                        worksheet.Cells[6, DeballastUnknownIndex].Value.ToString().Trim());
                                    sendEmails("Deballast", DeballastUnknownName, (worksheet.Cells[row, VesselNameIndex].Value.ToString().Trim()), utcReportdatetime);
                                }
                                if (DeballastHSFOIndex != 0 && worksheet.Cells[row, DeballastHSFOIndex].Value != null)
                                {
                                    RobDataDump(ref robs, "HSFO", ref rob, ref allocation, "Deballast",
                                        worksheet.Cells[row, DeballastHSFOIndex].Value.ToString(),
                                        worksheet.Cells[6, DeballastHSFOIndex].Value.ToString().Trim());
                                }
                                if (DeballastLMGOIndex != 0 && worksheet.Cells[row, DeballastLMGOIndex].Value != null)
                                {
                                    RobDataDump(ref robs, "LSMGO", ref rob, ref allocation, "Deballast",
                                       worksheet.Cells[row, DeballastLMGOIndex].Value.ToString(),
                                       worksheet.Cells[6, DeballastLMGOIndex].Value.ToString().Trim());
                                }
                                if (DeballastVLSFIndex != 0 && worksheet.Cells[row, DeballastVLSFIndex].Value != null)
                                {
                                    RobDataDump(ref robs, "VLSFO", ref rob, ref allocation, "Deballast",
                                      worksheet.Cells[row, DeballastVLSFIndex].Value.ToString(),
                                      worksheet.Cells[6, DeballastVLSFIndex].Value.ToString().Trim());
                                }

                                if (DeballastLIFOIndex != 0 && worksheet.Cells[row, DeballastLIFOIndex].Value != null)
                                {
                                    RobDataDump(ref robs, "LSIFO", ref rob, ref allocation, "Deballast",
                                        worksheet.Cells[row, DeballastLIFOIndex].Value.ToString(),
                                        worksheet.Cells[6, DeballastLIFOIndex].Value.ToString().Trim());
                                }
                                if (Deballast180Index != 0 && worksheet.Cells[row, Deballast180Index].Value != null)
                                {
                                    RobDataDump(ref robs, "180", ref rob, ref allocation, "Deballast",
                                       worksheet.Cells[row, Deballast180Index].Value.ToString(),
                                       worksheet.Cells[6, Deballast180Index].Value.ToString().Trim());
                                }
                                if (Deballast380Index != 0 && worksheet.Cells[row, Deballast380Index].Value != null)
                                {
                                    RobDataDump(ref robs, "380", ref rob, ref allocation, "Deballast",
                                      worksheet.Cells[row, Deballast380Index].Value.ToString(),
                                      worksheet.Cells[6, Deballast380Index].Value.ToString().Trim());
                                }

                                if (DeballastDMAIndex != 0 && worksheet.Cells[row, DeballastDMAIndex].Value != null)
                                {
                                    RobDataDump(ref robs, "DMA", ref rob, ref allocation, "Deballast",
                                        worksheet.Cells[row, DeballastDMAIndex].Value.ToString(),
                                        worksheet.Cells[6, DeballastDMAIndex].Value.ToString().Trim());
                                }
                                if (DeballastDMBIndex != 0 && worksheet.Cells[row, DeballastDMBIndex].Value != null)
                                {
                                    RobDataDump(ref robs, "DMB", ref rob, ref allocation, "Deballast",
                                       worksheet.Cells[row, DeballastDMBIndex].Value.ToString(),
                                       worksheet.Cells[6, DeballastDMBIndex].Value.ToString().Trim());
                                }
                                if (DeballastDMCIndex != 0 && worksheet.Cells[row, DeballastDMCIndex].Value != null)
                                {
                                    RobDataDump(ref robs, "DMC", ref rob, ref allocation, "Deballast",
                                      worksheet.Cells[row, DeballastDMCIndex].Value.ToString(),
                                      worksheet.Cells[6, DeballastDMCIndex].Value.ToString().Trim());
                                }

                                if (DeballastIFOIndex != 0 && worksheet.Cells[row, DeballastIFOIndex].Value != null)
                                {
                                    RobDataDump(ref robs, "IFO", ref rob, ref allocation, "Deballast",
                                        worksheet.Cells[row, DeballastIFOIndex].Value.ToString(),
                                        worksheet.Cells[6, DeballastIFOIndex].Value.ToString().Trim());
                                }
                                if (DeballastLSG1Index != 0 && worksheet.Cells[row, DeballastLSG1Index].Value != null)
                                {
                                    RobDataDump(ref robs, "LSG1", ref rob, ref allocation, "Deballast",
                                       worksheet.Cells[row, DeballastLSG1Index].Value.ToString(),
                                       worksheet.Cells[6, DeballastLSG1Index].Value.ToString().Trim());
                                }
                                if (DeballastLSMDOIndex != 0 && worksheet.Cells[row, DeballastLSMDOIndex].Value != null)
                                {
                                    RobDataDump(ref robs, "LSMDO", ref rob, ref allocation, "Deballast",
                                      worksheet.Cells[row, DeballastLSMDOIndex].Value.ToString(),
                                      worksheet.Cells[6, DeballastLSMDOIndex].Value.ToString().Trim());
                                }

                                if (DeballastLSIFOIndex != 0 && worksheet.Cells[row, DeballastLSIFOIndex].Value != null)
                                {
                                    RobDataDump(ref robs, "LSIFO", ref rob, ref allocation, "Deballast",
                                        worksheet.Cells[row, DeballastLSIFOIndex].Value.ToString(),
                                        worksheet.Cells[6, DeballastLSIFOIndex].Value.ToString().Trim());
                                }
                                if (DeballastLUBIndex != 0 && worksheet.Cells[row, DeballastLUBIndex].Value != null)
                                {
                                    RobDataDump(ref robs, "LUB", ref rob, ref allocation, "Deballast",
                                       worksheet.Cells[row, DeballastLUBIndex].Value.ToString(),
                                       worksheet.Cells[6, DeballastLUBIndex].Value.ToString().Trim());
                                }
                                if (DeballastMDOIndex != 0 && worksheet.Cells[row, DeballastMDOIndex].Value != null)
                                {
                                    RobDataDump(ref robs, "MDO", ref rob, ref allocation, "Deballast",
                                      worksheet.Cells[row, DeballastMDOIndex].Value.ToString(),
                                      worksheet.Cells[6, DeballastMDOIndex].Value.ToString().Trim());
                                }
                                if (DeballastMGOIndex != 0 && worksheet.Cells[row, DeballastMGOIndex].Value != null)
                                {
                                    RobDataDump(ref robs, "MGO", ref rob, ref allocation, "Deballast",
                                        worksheet.Cells[row, DeballastMGOIndex].Value.ToString(),
                                        worksheet.Cells[6, DeballastMGOIndex].Value.ToString().Trim());
                                }


                                #endregion

                                #region Generator

                                if (GeneratorUnknownIndex != 0 && worksheet.Cells[row, GeneratorUnknownIndex].Value != null)
                                {
                                    RobDataDump(ref robs, GeneratorUnknownName, ref rob, ref allocation, "Generator",
                                        worksheet.Cells[row, GeneratorUnknownIndex].Value.ToString(),
                                        worksheet.Cells[6, GeneratorUnknownIndex].Value.ToString().Trim());
                                    sendEmails("Generator", GeneratorUnknownName, (worksheet.Cells[row, VesselNameIndex].Value.ToString().Trim()), utcReportdatetime);
                                }
                                if (GeneratorHSFOIndex != 0 && worksheet.Cells[row, GeneratorHSFOIndex].Value != null)
                                {
                                    RobDataDump(ref robs, "HSFO", ref rob, ref allocation, "Generator",
                                        worksheet.Cells[row, GeneratorHSFOIndex].Value.ToString(),
                                        worksheet.Cells[6, GeneratorHSFOIndex].Value.ToString().Trim());
                                }
                                if (GeneratorLMGOIndex != 0 && worksheet.Cells[row, GeneratorLMGOIndex].Value != null)
                                {
                                    RobDataDump(ref robs, "LSMGO", ref rob, ref allocation, "Generator",
                                       worksheet.Cells[row, GeneratorLMGOIndex].Value.ToString(),
                                       worksheet.Cells[6, GeneratorLMGOIndex].Value.ToString().Trim());
                                }
                                if (GeneratorVLSFIndex != 0 && worksheet.Cells[row, GeneratorVLSFIndex].Value != null)
                                {
                                    RobDataDump(ref robs, "VLSFO", ref rob, ref allocation, "Generator",
                                      worksheet.Cells[row, GeneratorVLSFIndex].Value.ToString(),
                                      worksheet.Cells[6, GeneratorVLSFIndex].Value.ToString().Trim());
                                }

                                if (GeneratorLIFOIndex != 0 && worksheet.Cells[row, GeneratorLIFOIndex].Value != null)
                                {
                                    RobDataDump(ref robs, "LSIFO", ref rob, ref allocation, "Generator",
                                        worksheet.Cells[row, GeneratorLIFOIndex].Value.ToString(),
                                        worksheet.Cells[6, GeneratorLIFOIndex].Value.ToString().Trim());
                                }
                                if (Generator180Index != 0 && worksheet.Cells[row, Generator180Index].Value != null)
                                {
                                    RobDataDump(ref robs, "180", ref rob, ref allocation, "Generator",
                                       worksheet.Cells[row, Generator180Index].Value.ToString(),
                                       worksheet.Cells[6, Generator180Index].Value.ToString().Trim());
                                }
                                if (Generator380Index != 0 && worksheet.Cells[row, Generator380Index].Value != null)
                                {
                                    RobDataDump(ref robs, "380", ref rob, ref allocation, "Generator",
                                      worksheet.Cells[row, Generator380Index].Value.ToString(),
                                      worksheet.Cells[6, Generator380Index].Value.ToString().Trim());
                                }

                                if (GeneratorDMAIndex != 0 && worksheet.Cells[row, GeneratorDMAIndex].Value != null)
                                {
                                    RobDataDump(ref robs, "DMA", ref rob, ref allocation, "Generator",
                                        worksheet.Cells[row, GeneratorDMAIndex].Value.ToString(),
                                        worksheet.Cells[6, GeneratorDMAIndex].Value.ToString().Trim());
                                }
                                if (GeneratorDMBIndex != 0 && worksheet.Cells[row, GeneratorDMBIndex].Value != null)
                                {
                                    RobDataDump(ref robs, "DMB", ref rob, ref allocation, "Generator",
                                       worksheet.Cells[row, GeneratorDMBIndex].Value.ToString(),
                                       worksheet.Cells[6, GeneratorDMBIndex].Value.ToString().Trim());
                                }
                                if (GeneratorDMCIndex != 0 && worksheet.Cells[row, GeneratorDMCIndex].Value != null)
                                {
                                    RobDataDump(ref robs, "DMC", ref rob, ref allocation, "Generator",
                                      worksheet.Cells[row, GeneratorDMCIndex].Value.ToString(),
                                      worksheet.Cells[6, GeneratorDMCIndex].Value.ToString().Trim());
                                }

                                if (GeneratorIFOIndex != 0 && worksheet.Cells[row, GeneratorIFOIndex].Value != null)
                                {
                                    RobDataDump(ref robs, "IFO", ref rob, ref allocation, "Generator",
                                        worksheet.Cells[row, GeneratorIFOIndex].Value.ToString(),
                                        worksheet.Cells[6, GeneratorIFOIndex].Value.ToString().Trim());
                                }
                                if (GeneratorLSG1Index != 0 && worksheet.Cells[row, GeneratorLSG1Index].Value != null)
                                {
                                    RobDataDump(ref robs, "LSG1", ref rob, ref allocation, "Generator",
                                       worksheet.Cells[row, GeneratorLSG1Index].Value.ToString(),
                                       worksheet.Cells[6, GeneratorLSG1Index].Value.ToString().Trim());
                                }
                                if (GeneratorLSMDOIndex != 0 && worksheet.Cells[row, GeneratorLSMDOIndex].Value != null)
                                {
                                    RobDataDump(ref robs, "LSMDO", ref rob, ref allocation, "Generator",
                                      worksheet.Cells[row, GeneratorLSMDOIndex].Value.ToString(),
                                      worksheet.Cells[6, GeneratorLSMDOIndex].Value.ToString().Trim());
                                }

                                if (GeneratorLSIFOIndex != 0 && worksheet.Cells[row, GeneratorLSIFOIndex].Value != null)
                                {
                                    RobDataDump(ref robs, "LSIFO", ref rob, ref allocation, "Generator",
                                        worksheet.Cells[row, GeneratorLSIFOIndex].Value.ToString(),
                                        worksheet.Cells[6, GeneratorLSIFOIndex].Value.ToString().Trim());
                                }
                                if (GeneratorLUBIndex != 0 && worksheet.Cells[row, GeneratorLUBIndex].Value != null)
                                {
                                    RobDataDump(ref robs, "LUB", ref rob, ref allocation, "Generator",
                                       worksheet.Cells[row, GeneratorLUBIndex].Value.ToString(),
                                       worksheet.Cells[6, GeneratorLUBIndex].Value.ToString().Trim());
                                }
                                if (GeneratorMDOIndex != 0 && worksheet.Cells[row, GeneratorMDOIndex].Value != null)
                                {
                                    RobDataDump(ref robs, "MDO", ref rob, ref allocation, "Generator",
                                      worksheet.Cells[row, GeneratorMDOIndex].Value.ToString(),
                                      worksheet.Cells[6, GeneratorMDOIndex].Value.ToString().Trim());
                                }
                                if (GeneratorMGOIndex != 0 && worksheet.Cells[row, GeneratorMGOIndex].Value != null)
                                {
                                    RobDataDump(ref robs, "MGO", ref rob, ref allocation, "Generator",
                                        worksheet.Cells[row, GeneratorMGOIndex].Value.ToString(),
                                        worksheet.Cells[6, GeneratorMGOIndex].Value.ToString().Trim());
                                }


                                #endregion

                                #region Other

                                if (OtherUnknownIndex != 0 && worksheet.Cells[row, OtherUnknownIndex].Value != null)
                                {
                                    RobDataDump(ref robs, OtherUnknownName, ref rob, ref allocation, "Other",
                                        worksheet.Cells[row, OtherUnknownIndex].Value.ToString(),
                                        worksheet.Cells[6, OtherUnknownIndex].Value.ToString().Trim());
                                    sendEmails("Other", OtherUnknownName, (worksheet.Cells[row, VesselNameIndex].Value.ToString().Trim()), utcReportdatetime);
                                }
                                if (OtherHSFOIndex != 0 && worksheet.Cells[row, OtherHSFOIndex].Value != null)
                                {
                                    RobDataDump(ref robs, "HSFO", ref rob, ref allocation, "Other",
                                        worksheet.Cells[row, OtherHSFOIndex].Value.ToString(),
                                        worksheet.Cells[6, OtherHSFOIndex].Value.ToString().Trim());
                                }
                                if (OtherLMGOIndex != 0 && worksheet.Cells[row, OtherLMGOIndex].Value != null)
                                {
                                    RobDataDump(ref robs, "LSMGO", ref rob, ref allocation, "Other",
                                       worksheet.Cells[row, OtherLMGOIndex].Value.ToString(),
                                       worksheet.Cells[6, OtherLMGOIndex].Value.ToString().Trim());
                                }
                                if (OtherVLSFIndex != 0 && worksheet.Cells[row, OtherVLSFIndex].Value != null)
                                {
                                    RobDataDump(ref robs, "VLSFO", ref rob, ref allocation, "Other",
                                      worksheet.Cells[row, OtherVLSFIndex].Value.ToString(),
                                      worksheet.Cells[6, OtherVLSFIndex].Value.ToString().Trim());
                                }

                                if (OtherLIFOIndex != 0 && worksheet.Cells[row, OtherLIFOIndex].Value != null)
                                {
                                    RobDataDump(ref robs, "LSIFO", ref rob, ref allocation, "Other",
                                        worksheet.Cells[row, OtherLIFOIndex].Value.ToString(),
                                        worksheet.Cells[6, OtherLIFOIndex].Value.ToString().Trim());
                                }
                                if (Other180Index != 0 && worksheet.Cells[row, Other180Index].Value != null)
                                {
                                    RobDataDump(ref robs, "180", ref rob, ref allocation, "Other",
                                       worksheet.Cells[row, Other180Index].Value.ToString(),
                                       worksheet.Cells[6, Other180Index].Value.ToString().Trim());
                                }
                                if (Other380Index != 0 && worksheet.Cells[row, Other380Index].Value != null)
                                {
                                    RobDataDump(ref robs, "380", ref rob, ref allocation, "Other",
                                      worksheet.Cells[row, Other380Index].Value.ToString(),
                                      worksheet.Cells[6, Other380Index].Value.ToString().Trim());
                                }

                                if (OtherDMAIndex != 0 && worksheet.Cells[row, OtherDMAIndex].Value != null)
                                {
                                    RobDataDump(ref robs, "DMA", ref rob, ref allocation, "Other",
                                        worksheet.Cells[row, OtherDMAIndex].Value.ToString(),
                                        worksheet.Cells[6, OtherDMAIndex].Value.ToString().Trim());
                                }
                                if (OtherDMBIndex != 0 && worksheet.Cells[row, OtherDMBIndex].Value != null)
                                {
                                    RobDataDump(ref robs, "DMB", ref rob, ref allocation, "Other",
                                       worksheet.Cells[row, OtherDMBIndex].Value.ToString(),
                                       worksheet.Cells[6, OtherDMBIndex].Value.ToString().Trim());
                                }
                                if (OtherDMCIndex != 0 && worksheet.Cells[row, OtherDMCIndex].Value != null)
                                {
                                    RobDataDump(ref robs, "DMC", ref rob, ref allocation, "Other",
                                      worksheet.Cells[row, OtherDMCIndex].Value.ToString(),
                                      worksheet.Cells[6, OtherDMCIndex].Value.ToString().Trim());
                                }

                                if (OtherIFOIndex != 0 && worksheet.Cells[row, OtherIFOIndex].Value != null)
                                {
                                    RobDataDump(ref robs, "IFO", ref rob, ref allocation, "Other",
                                        worksheet.Cells[row, OtherIFOIndex].Value.ToString(),
                                        worksheet.Cells[6, OtherIFOIndex].Value.ToString().Trim());
                                }
                                if (OtherLSG1Index != 0 && worksheet.Cells[row, OtherLSG1Index].Value != null)
                                {
                                    RobDataDump(ref robs, "LSG1", ref rob, ref allocation, "Other",
                                       worksheet.Cells[row, OtherLSG1Index].Value.ToString(),
                                       worksheet.Cells[6, OtherLSG1Index].Value.ToString().Trim());
                                }
                                if (OtherLSMDOIndex != 0 && worksheet.Cells[row, OtherLSMDOIndex].Value != null)
                                {
                                    RobDataDump(ref robs, "LSMDO", ref rob, ref allocation, "Other",
                                      worksheet.Cells[row, OtherLSMDOIndex].Value.ToString(),
                                      worksheet.Cells[6, OtherLSMDOIndex].Value.ToString().Trim());
                                }

                                if (OtherLSIFOIndex != 0 && worksheet.Cells[row, OtherLSIFOIndex].Value != null)
                                {
                                    RobDataDump(ref robs, "LSIFO", ref rob, ref allocation, "Other",
                                        worksheet.Cells[row, OtherLSIFOIndex].Value.ToString(),
                                        worksheet.Cells[6, OtherLSIFOIndex].Value.ToString().Trim());
                                }
                                if (OtherLUBIndex != 0 && worksheet.Cells[row, OtherLUBIndex].Value != null)
                                {
                                    RobDataDump(ref robs, "LUB", ref rob, ref allocation, "Other",
                                       worksheet.Cells[row, OtherLUBIndex].Value.ToString(),
                                       worksheet.Cells[6, OtherLUBIndex].Value.ToString().Trim());
                                }
                                if (OtherMDOIndex != 0 && worksheet.Cells[row, OtherMDOIndex].Value != null)
                                {
                                    RobDataDump(ref robs, "MDO", ref rob, ref allocation, "Other",
                                      worksheet.Cells[row, OtherMDOIndex].Value.ToString(),
                                      worksheet.Cells[6, OtherMDOIndex].Value.ToString().Trim());
                                }
                                if (OtherMGOIndex != 0 && worksheet.Cells[row, OtherMGOIndex].Value != null)
                                {
                                    RobDataDump(ref robs, "MGO", ref rob, ref allocation, "Other",
                                        worksheet.Cells[row, OtherMGOIndex].Value.ToString(),
                                        worksheet.Cells[6, OtherMGOIndex].Value.ToString().Trim());
                                }

                                #endregion

                                #region TankCleaning

                                if (TankCleaningUnknownIndex != 0 && worksheet.Cells[row, TankCleaningUnknownIndex].Value != null)
                                {
                                    RobDataDump(ref robs, TankCleaningUnknownName, ref rob, ref allocation, "TankCleaning",
                                        worksheet.Cells[row, TankCleaningUnknownIndex].Value.ToString(),
                                        worksheet.Cells[6, TankCleaningUnknownIndex].Value.ToString().Trim());
                                    sendEmails("TankCleaning", TankCleaningUnknownName, (worksheet.Cells[row, VesselNameIndex].Value.ToString().Trim()), utcReportdatetime);
                                }
                                if (TankCleaningHSFOIndex != 0 && worksheet.Cells[row, TankCleaningHSFOIndex].Value != null)
                                {
                                    RobDataDump(ref robs, "HSFO", ref rob, ref allocation, "TankCleaning",
                                        worksheet.Cells[row, TankCleaningHSFOIndex].Value.ToString(),
                                        worksheet.Cells[6, TankCleaningHSFOIndex].Value.ToString().Trim());
                                }
                                if (TankCleaningLMGOIndex != 0 && worksheet.Cells[row, TankCleaningLMGOIndex].Value != null)
                                {
                                    RobDataDump(ref robs, "LSMGO", ref rob, ref allocation, "TankCleaning",
                                       worksheet.Cells[row, TankCleaningLMGOIndex].Value.ToString(),
                                       worksheet.Cells[6, TankCleaningLMGOIndex].Value.ToString().Trim());
                                }
                                if (TankCleaningVLSFIndex != 0 && worksheet.Cells[row, TankCleaningVLSFIndex].Value != null)
                                {
                                    RobDataDump(ref robs, "VLSFO", ref rob, ref allocation, "TankCleaning",
                                      worksheet.Cells[row, TankCleaningVLSFIndex].Value.ToString(),
                                      worksheet.Cells[6, TankCleaningVLSFIndex].Value.ToString().Trim());
                                }

                                if (TankCleaningLIFOIndex != 0 && worksheet.Cells[row, TankCleaningLIFOIndex].Value != null)
                                {
                                    RobDataDump(ref robs, "LSIFO", ref rob, ref allocation, "TankCleaning",
                                        worksheet.Cells[row, TankCleaningLIFOIndex].Value.ToString(),
                                        worksheet.Cells[6, TankCleaningLIFOIndex].Value.ToString().Trim());
                                }
                                if (TankCleaning180Index != 0 && worksheet.Cells[row, TankCleaning180Index].Value != null)
                                {
                                    RobDataDump(ref robs, "180", ref rob, ref allocation, "TankCleaning",
                                       worksheet.Cells[row, TankCleaning180Index].Value.ToString(),
                                       worksheet.Cells[6, TankCleaning180Index].Value.ToString().Trim());
                                }
                                if (TankCleaning380Index != 0 && worksheet.Cells[row, TankCleaning380Index].Value != null)
                                {
                                    RobDataDump(ref robs, "380", ref rob, ref allocation, "TankCleaning",
                                      worksheet.Cells[row, TankCleaning380Index].Value.ToString(),
                                      worksheet.Cells[6, TankCleaning380Index].Value.ToString().Trim());
                                }

                                if (TankCleaningDMAIndex != 0 && worksheet.Cells[row, TankCleaningDMAIndex].Value != null)
                                {
                                    RobDataDump(ref robs, "DMA", ref rob, ref allocation, "TankCleaning",
                                        worksheet.Cells[row, TankCleaningDMAIndex].Value.ToString(),
                                        worksheet.Cells[6, TankCleaningDMAIndex].Value.ToString().Trim());
                                }
                                if (TankCleaningDMBIndex != 0 && worksheet.Cells[row, TankCleaningDMBIndex].Value != null)
                                {
                                    RobDataDump(ref robs, "DMB", ref rob, ref allocation, "TankCleaning",
                                       worksheet.Cells[row, TankCleaningDMBIndex].Value.ToString(),
                                       worksheet.Cells[6, TankCleaningDMBIndex].Value.ToString().Trim());
                                }
                                if (TankCleaningDMCIndex != 0 && worksheet.Cells[row, TankCleaningDMCIndex].Value != null)
                                {
                                    RobDataDump(ref robs, "DMC", ref rob, ref allocation, "TankCleaning",
                                      worksheet.Cells[row, TankCleaningDMCIndex].Value.ToString(),
                                      worksheet.Cells[6, TankCleaningDMCIndex].Value.ToString().Trim());
                                }

                                if (TankCleaningIFOIndex != 0 && worksheet.Cells[row, TankCleaningIFOIndex].Value != null)
                                {
                                    RobDataDump(ref robs, "IFO", ref rob, ref allocation, "TankCleaning",
                                        worksheet.Cells[row, TankCleaningIFOIndex].Value.ToString(),
                                        worksheet.Cells[6, TankCleaningIFOIndex].Value.ToString().Trim());
                                }
                                if (TankCleaningLSG1Index != 0 && worksheet.Cells[row, TankCleaningLSG1Index].Value != null)
                                {
                                    RobDataDump(ref robs, "LSG1", ref rob, ref allocation, "TankCleaning",
                                       worksheet.Cells[row, TankCleaningLSG1Index].Value.ToString(),
                                       worksheet.Cells[6, TankCleaningLSG1Index].Value.ToString().Trim());
                                }
                                if (TankCleaningLSMDOIndex != 0 && worksheet.Cells[row, TankCleaningLSMDOIndex].Value != null)
                                {
                                    RobDataDump(ref robs, "LSMDO", ref rob, ref allocation, "TankCleaning",
                                      worksheet.Cells[row, TankCleaningLSMDOIndex].Value.ToString(),
                                      worksheet.Cells[6, TankCleaningLSMDOIndex].Value.ToString().Trim());
                                }

                                if (TankCleaningLSIFOIndex != 0 && worksheet.Cells[row, TankCleaningLSIFOIndex].Value != null)
                                {
                                    RobDataDump(ref robs, "LSIFO", ref rob, ref allocation, "TankCleaning",
                                        worksheet.Cells[row, TankCleaningLSIFOIndex].Value.ToString(),
                                        worksheet.Cells[6, TankCleaningLSIFOIndex].Value.ToString().Trim());
                                }
                                if (TankCleaningLUBIndex != 0 && worksheet.Cells[row, TankCleaningLUBIndex].Value != null)
                                {
                                    RobDataDump(ref robs, "LUB", ref rob, ref allocation, "TankCleaning",
                                       worksheet.Cells[row, TankCleaningLUBIndex].Value.ToString(),
                                       worksheet.Cells[6, TankCleaningLUBIndex].Value.ToString().Trim());
                                }
                                if (TankCleaningMDOIndex != 0 && worksheet.Cells[row, TankCleaningMDOIndex].Value != null)
                                {
                                    RobDataDump(ref robs, "MDO", ref rob, ref allocation, "TankCleaning",
                                      worksheet.Cells[row, TankCleaningMDOIndex].Value.ToString(),
                                      worksheet.Cells[6, TankCleaningMDOIndex].Value.ToString().Trim());
                                }
                                if (TankCleaningMGOIndex != 0 && worksheet.Cells[row, TankCleaningMGOIndex].Value != null)
                                {
                                    RobDataDump(ref robs, "MGO", ref rob, ref allocation, "TankCleaning",
                                        worksheet.Cells[row, TankCleaningMGOIndex].Value.ToString(),
                                        worksheet.Cells[6, TankCleaningMGOIndex].Value.ToString().Trim());
                                }


                                #endregion

                                #region InertGasGenerator

                                if (InertGasGeneratorUnknownIndex != 0 && worksheet.Cells[row, InertGasGeneratorUnknownIndex].Value != null)
                                {
                                    RobDataDump(ref robs, InertGasGeneratorUnknownName, ref rob, ref allocation, "InertGasGenerator",
                                        worksheet.Cells[row, InertGasGeneratorUnknownIndex].Value.ToString(),
                                        worksheet.Cells[6, InertGasGeneratorUnknownIndex].Value.ToString().Trim());
                                    sendEmails("InertGasGenerator", InertGasGeneratorUnknownName, (worksheet.Cells[row, VesselNameIndex].Value.ToString().Trim()), utcReportdatetime);
                                }
                                if (InertGasGeneratorHSFOIndex != 0 && worksheet.Cells[row, InertGasGeneratorHSFOIndex].Value != null)
                                {
                                    RobDataDump(ref robs, "HSFO", ref rob, ref allocation, "InertGasGenerator",
                                        worksheet.Cells[row, InertGasGeneratorHSFOIndex].Value.ToString(),
                                        worksheet.Cells[6, InertGasGeneratorHSFOIndex].Value.ToString().Trim());
                                }
                                if (InertGasGeneratorLMGOIndex != 0 && worksheet.Cells[row, InertGasGeneratorLMGOIndex].Value != null)
                                {
                                    RobDataDump(ref robs, "LSMGO", ref rob, ref allocation, "InertGasGenerator",
                                       worksheet.Cells[row, InertGasGeneratorLMGOIndex].Value.ToString(),
                                       worksheet.Cells[6, InertGasGeneratorLMGOIndex].Value.ToString().Trim());
                                }
                                if (InertGasGeneratorVLSFIndex != 0 && worksheet.Cells[row, InertGasGeneratorVLSFIndex].Value != null)
                                {
                                    RobDataDump(ref robs, "VLSFO", ref rob, ref allocation, "InertGasGenerator",
                                      worksheet.Cells[row, InertGasGeneratorVLSFIndex].Value.ToString(),
                                      worksheet.Cells[6, InertGasGeneratorVLSFIndex].Value.ToString().Trim());
                                }

                                if (InertGasGeneratorLIFOIndex != 0 && worksheet.Cells[row, InertGasGeneratorLIFOIndex].Value != null)
                                {
                                    RobDataDump(ref robs, "LSIFO", ref rob, ref allocation, "InertGasGenerator",
                                        worksheet.Cells[row, InertGasGeneratorLIFOIndex].Value.ToString(),
                                        worksheet.Cells[6, InertGasGeneratorLIFOIndex].Value.ToString().Trim());
                                }
                                if (InertGasGenerator180Index != 0 && worksheet.Cells[row, InertGasGenerator180Index].Value != null)
                                {
                                    RobDataDump(ref robs, "180", ref rob, ref allocation, "InertGasGenerator",
                                       worksheet.Cells[row, InertGasGenerator180Index].Value.ToString(),
                                       worksheet.Cells[6, InertGasGenerator180Index].Value.ToString().Trim());
                                }
                                if (InertGasGenerator380Index != 0 && worksheet.Cells[row, InertGasGenerator380Index].Value != null)
                                {
                                    RobDataDump(ref robs, "380", ref rob, ref allocation, "InertGasGenerator",
                                      worksheet.Cells[row, InertGasGenerator380Index].Value.ToString(),
                                      worksheet.Cells[6, InertGasGenerator380Index].Value.ToString().Trim());
                                }

                                if (InertGasGeneratorDMAIndex != 0 && worksheet.Cells[row, InertGasGeneratorDMAIndex].Value != null)
                                {
                                    RobDataDump(ref robs, "DMA", ref rob, ref allocation, "InertGasGenerator",
                                        worksheet.Cells[row, InertGasGeneratorDMAIndex].Value.ToString(),
                                        worksheet.Cells[6, InertGasGeneratorDMAIndex].Value.ToString().Trim());
                                }
                                if (InertGasGeneratorDMBIndex != 0 && worksheet.Cells[row, InertGasGeneratorDMBIndex].Value != null)
                                {
                                    RobDataDump(ref robs, "DMB", ref rob, ref allocation, "InertGasGenerator",
                                       worksheet.Cells[row, InertGasGeneratorDMBIndex].Value.ToString(),
                                       worksheet.Cells[6, InertGasGeneratorDMBIndex].Value.ToString().Trim());
                                }
                                if (InertGasGeneratorDMCIndex != 0 && worksheet.Cells[row, InertGasGeneratorDMCIndex].Value != null)
                                {
                                    RobDataDump(ref robs, "DMC", ref rob, ref allocation, "InertGasGenerator",
                                      worksheet.Cells[row, InertGasGeneratorDMCIndex].Value.ToString(),
                                      worksheet.Cells[6, InertGasGeneratorDMCIndex].Value.ToString().Trim());
                                }

                                if (InertGasGeneratorIFOIndex != 0 && worksheet.Cells[row, InertGasGeneratorIFOIndex].Value != null)
                                {
                                    RobDataDump(ref robs, "IFO", ref rob, ref allocation, "InertGasGenerator",
                                        worksheet.Cells[row, InertGasGeneratorIFOIndex].Value.ToString(),
                                        worksheet.Cells[6, InertGasGeneratorIFOIndex].Value.ToString().Trim());
                                }
                                if (InertGasGeneratorLSG1Index != 0 && worksheet.Cells[row, InertGasGeneratorLSG1Index].Value != null)
                                {
                                    RobDataDump(ref robs, "LSG1", ref rob, ref allocation, "InertGasGenerator",
                                       worksheet.Cells[row, InertGasGeneratorLSG1Index].Value.ToString(),
                                       worksheet.Cells[6, InertGasGeneratorLSG1Index].Value.ToString().Trim());
                                }
                                if (InertGasGeneratorLSMDOIndex != 0 && worksheet.Cells[row, InertGasGeneratorLSMDOIndex].Value != null)
                                {
                                    RobDataDump(ref robs, "LSMDO", ref rob, ref allocation, "InertGasGenerator",
                                      worksheet.Cells[row, InertGasGeneratorLSMDOIndex].Value.ToString(),
                                      worksheet.Cells[6, InertGasGeneratorLSMDOIndex].Value.ToString().Trim());
                                }

                                if (InertGasGeneratorLSIFOIndex != 0 && worksheet.Cells[row, InertGasGeneratorLSIFOIndex].Value != null)
                                {
                                    RobDataDump(ref robs, "LSIFO", ref rob, ref allocation, "InertGasGenerator",
                                        worksheet.Cells[row, InertGasGeneratorLSIFOIndex].Value.ToString(),
                                        worksheet.Cells[6, InertGasGeneratorLSIFOIndex].Value.ToString().Trim());
                                }
                                if (InertGasGeneratorLUBIndex != 0 && worksheet.Cells[row, InertGasGeneratorLUBIndex].Value != null)
                                {
                                    RobDataDump(ref robs, "LUB", ref rob, ref allocation, "InertGasGenerator",
                                       worksheet.Cells[row, InertGasGeneratorLUBIndex].Value.ToString(),
                                       worksheet.Cells[6, InertGasGeneratorLUBIndex].Value.ToString().Trim());
                                }
                                if (InertGasGeneratorMDOIndex != 0 && worksheet.Cells[row, InertGasGeneratorMDOIndex].Value != null)
                                {
                                    RobDataDump(ref robs, "MDO", ref rob, ref allocation, "InertGasGenerator",
                                      worksheet.Cells[row, InertGasGeneratorMDOIndex].Value.ToString(),
                                      worksheet.Cells[6, InertGasGeneratorMDOIndex].Value.ToString().Trim());
                                }
                                if (InertGasGeneratorMGOIndex != 0 && worksheet.Cells[row, InertGasGeneratorMGOIndex].Value != null)
                                {
                                    RobDataDump(ref robs, "MGO", ref rob, ref allocation, "InertGasGenerator",
                                        worksheet.Cells[row, InertGasGeneratorMGOIndex].Value.ToString(),
                                        worksheet.Cells[6, InertGasGeneratorMGOIndex].Value.ToString().Trim());
                                }


                                #endregion

                                #region CargoHeating

                                if (CargoHeatingUnknownIndex != 0 && worksheet.Cells[row, CargoHeatingUnknownIndex].Value != null)
                                {
                                    RobDataDump(ref robs, InertGasGeneratorUnknownName, ref rob, ref allocation, "CargoHeating",
                                        worksheet.Cells[row, CargoHeatingUnknownIndex].Value.ToString(),
                                        worksheet.Cells[6, CargoHeatingUnknownIndex].Value.ToString().Trim());
                                    sendEmails("InertGasGenerator", InertGasGeneratorUnknownName, (worksheet.Cells[row, VesselNameIndex].Value.ToString().Trim()), utcReportdatetime);
                                }
                                if (CargoHeatingHSFOIndex != 0 && worksheet.Cells[row, CargoHeatingHSFOIndex].Value != null)
                                {
                                    RobDataDump(ref robs, "HSFO", ref rob, ref allocation, "CargoHeating",
                                        worksheet.Cells[row, CargoHeatingHSFOIndex].Value.ToString(),
                                        worksheet.Cells[6, CargoHeatingHSFOIndex].Value.ToString().Trim());
                                }
                                if (CargoHeatingLMGOIndex != 0 && worksheet.Cells[row, CargoHeatingLMGOIndex].Value != null)
                                {
                                    RobDataDump(ref robs, "LSMGO", ref rob, ref allocation, "CargoHeating",
                                       worksheet.Cells[row, CargoHeatingLMGOIndex].Value.ToString(),
                                       worksheet.Cells[6, CargoHeatingLMGOIndex].Value.ToString().Trim());
                                }
                                if (CargoHeatingVLSFIndex != 0 && worksheet.Cells[row, CargoHeatingVLSFIndex].Value != null)
                                {
                                    RobDataDump(ref robs, "VLSFO", ref rob, ref allocation, "CargoHeating",
                                      worksheet.Cells[row, CargoHeatingVLSFIndex].Value.ToString(),
                                      worksheet.Cells[6, CargoHeatingVLSFIndex].Value.ToString().Trim());
                                }

                                if (CargoHeatingLIFOIndex != 0 && worksheet.Cells[row, CargoHeatingLIFOIndex].Value != null)
                                {
                                    RobDataDump(ref robs, "LSIFO", ref rob, ref allocation, "CargoHeating",
                                        worksheet.Cells[row, CargoHeatingLIFOIndex].Value.ToString(),
                                        worksheet.Cells[6, CargoHeatingLIFOIndex].Value.ToString().Trim());
                                }
                                if (CargoHeating180Index != 0 && worksheet.Cells[row, CargoHeating180Index].Value != null)
                                {
                                    RobDataDump(ref robs, "180", ref rob, ref allocation, "CargoHeating",
                                       worksheet.Cells[row, CargoHeating180Index].Value.ToString(),
                                       worksheet.Cells[6, CargoHeating180Index].Value.ToString().Trim());
                                }
                                if (CargoHeating380Index != 0 && worksheet.Cells[row, CargoHeating380Index].Value != null)
                                {
                                    RobDataDump(ref robs, "380", ref rob, ref allocation, "CargoHeating",
                                      worksheet.Cells[row, CargoHeating380Index].Value.ToString(),
                                      worksheet.Cells[6, CargoHeating380Index].Value.ToString().Trim());
                                }

                                if (CargoHeatingDMAIndex != 0 && worksheet.Cells[row, CargoHeatingDMAIndex].Value != null)
                                {
                                    RobDataDump(ref robs, "DMA", ref rob, ref allocation, "CargoHeating",
                                        worksheet.Cells[row, CargoHeatingDMAIndex].Value.ToString(),
                                        worksheet.Cells[6, CargoHeatingDMAIndex].Value.ToString().Trim());
                                }
                                if (CargoHeatingDMBIndex != 0 && worksheet.Cells[row, CargoHeatingDMBIndex].Value != null)
                                {
                                    RobDataDump(ref robs, "DMB", ref rob, ref allocation, "CargoHeating",
                                       worksheet.Cells[row, CargoHeatingDMBIndex].Value.ToString(),
                                       worksheet.Cells[6, CargoHeatingDMBIndex].Value.ToString().Trim());
                                }
                                if (CargoHeatingDMCIndex != 0 && worksheet.Cells[row, CargoHeatingDMCIndex].Value != null)
                                {
                                    RobDataDump(ref robs, "DMC", ref rob, ref allocation, "CargoHeating",
                                      worksheet.Cells[row, CargoHeatingDMCIndex].Value.ToString(),
                                      worksheet.Cells[6, CargoHeatingDMCIndex].Value.ToString().Trim());
                                }

                                if (CargoHeatingIFOIndex != 0 && worksheet.Cells[row, CargoHeatingIFOIndex].Value != null)
                                {
                                    RobDataDump(ref robs, "IFO", ref rob, ref allocation, "CargoHeating",
                                        worksheet.Cells[row, CargoHeatingIFOIndex].Value.ToString(),
                                        worksheet.Cells[6, CargoHeatingIFOIndex].Value.ToString().Trim());
                                }
                                if (CargoHeatingLSG1Index != 0 && worksheet.Cells[row, CargoHeatingLSG1Index].Value != null)
                                {
                                    RobDataDump(ref robs, "LSG1", ref rob, ref allocation, "CargoHeating",
                                       worksheet.Cells[row, CargoHeatingLSG1Index].Value.ToString(),
                                       worksheet.Cells[6, CargoHeatingLSG1Index].Value.ToString().Trim());
                                }
                                if (CargoHeatingLSMDOIndex != 0 && worksheet.Cells[row, CargoHeatingLSMDOIndex].Value != null)
                                {
                                    RobDataDump(ref robs, "LSMDO", ref rob, ref allocation, "CargoHeating",
                                      worksheet.Cells[row, CargoHeatingLSMDOIndex].Value.ToString(),
                                      worksheet.Cells[6, CargoHeatingLSMDOIndex].Value.ToString().Trim());
                                }

                                if (CargoHeatingLSIFOIndex != 0 && worksheet.Cells[row, CargoHeatingLSIFOIndex].Value != null)
                                {
                                    RobDataDump(ref robs, "LSIFO", ref rob, ref allocation, "CargoHeating",
                                        worksheet.Cells[row, CargoHeatingLSIFOIndex].Value.ToString(),
                                        worksheet.Cells[6, CargoHeatingLSIFOIndex].Value.ToString().Trim());
                                }
                                if (CargoHeatingLUBIndex != 0 && worksheet.Cells[row, CargoHeatingLUBIndex].Value != null)
                                {
                                    RobDataDump(ref robs, "LUB", ref rob, ref allocation, "CargoHeating",
                                       worksheet.Cells[row, CargoHeatingLUBIndex].Value.ToString(),
                                       worksheet.Cells[6, CargoHeatingLUBIndex].Value.ToString().Trim());
                                }
                                if (CargoHeatingMDOIndex != 0 && worksheet.Cells[row, CargoHeatingMDOIndex].Value != null)
                                {
                                    RobDataDump(ref robs, "MDO", ref rob, ref allocation, "CargoHeating",
                                      worksheet.Cells[row, CargoHeatingMDOIndex].Value.ToString(),
                                      worksheet.Cells[6, CargoHeatingMDOIndex].Value.ToString().Trim());
                                }
                                if (CargoHeatingMGOIndex != 0 && worksheet.Cells[row, CargoHeatingMGOIndex].Value != null)
                                {
                                    RobDataDump(ref robs, "MGO", ref rob, ref allocation, "CargoHeating",
                                        worksheet.Cells[row, CargoHeatingMGOIndex].Value.ToString(),
                                        worksheet.Cells[6, CargoHeatingMGOIndex].Value.ToString().Trim());
                                }


                                #endregion

                                #region UnknownCategory

                                if (UnknownCategoryName != "")
                                {
                                    sendEmails(UnknownCategoryName, "", (worksheet.Cells[row, VesselNameIndex].Value.ToString().Trim()), utcReportdatetime);
                                }
                                if (UnknownCategoryUnknownIndex != 0 && worksheet.Cells[row, UnknownCategoryUnknownIndex].Value != null)
                                {
                                    RobDataDump(ref robs, UnknownCategoryUnknownName, ref rob, ref allocation, UnknownCategoryName,
                                        worksheet.Cells[row, UnknownCategoryUnknownIndex].Value.ToString(),
                                        worksheet.Cells[6, UnknownCategoryUnknownIndex].Value.ToString().Trim());
                                    sendEmails(UnknownCategoryName, UnknownCategoryUnknownName, (worksheet.Cells[row, VesselNameIndex].Value.ToString().Trim()), utcReportdatetime);
                                }
                                if (UnknownCategoryHSFOIndex != 0 && worksheet.Cells[row, UnknownCategoryHSFOIndex].Value != null)
                                {
                                    RobDataDump(ref robs, "HSFO", ref rob, ref allocation, UnknownCategoryName,
                                        worksheet.Cells[row, UnknownCategoryHSFOIndex].Value.ToString(),
                                        worksheet.Cells[6, UnknownCategoryHSFOIndex].Value.ToString().Trim());
                                }
                                if (UnknownCategoryLMGOIndex != 0 && worksheet.Cells[row, UnknownCategoryLMGOIndex].Value != null)
                                {
                                    RobDataDump(ref robs, "LSMGO", ref rob, ref allocation, UnknownCategoryName,
                                       worksheet.Cells[row, UnknownCategoryLMGOIndex].Value.ToString(),
                                       worksheet.Cells[6, UnknownCategoryLMGOIndex].Value.ToString().Trim());
                                }
                                if (UnknownCategoryVLSFIndex != 0 && worksheet.Cells[row, UnknownCategoryVLSFIndex].Value != null)
                                {
                                    RobDataDump(ref robs, "VLSFO", ref rob, ref allocation, UnknownCategoryName,
                                      worksheet.Cells[row, UnknownCategoryVLSFIndex].Value.ToString(),
                                      worksheet.Cells[6, UnknownCategoryVLSFIndex].Value.ToString().Trim());
                                }

                                if (UnknownCategoryLIFOIndex != 0 && worksheet.Cells[row, UnknownCategoryLIFOIndex].Value != null)
                                {
                                    RobDataDump(ref robs, "LSIFO", ref rob, ref allocation, UnknownCategoryName,
                                        worksheet.Cells[row, UnknownCategoryLIFOIndex].Value.ToString(),
                                        worksheet.Cells[6, UnknownCategoryLIFOIndex].Value.ToString().Trim());
                                }
                                if (UnknownCategory180Index != 0 && worksheet.Cells[row, UnknownCategory180Index].Value != null)
                                {
                                    RobDataDump(ref robs, "180", ref rob, ref allocation, UnknownCategoryName,
                                       worksheet.Cells[row, UnknownCategory180Index].Value.ToString(),
                                       worksheet.Cells[6, UnknownCategory180Index].Value.ToString().Trim());
                                }
                                if (UnknownCategory380Index != 0 && worksheet.Cells[row, UnknownCategory380Index].Value != null)
                                {
                                    RobDataDump(ref robs, "380", ref rob, ref allocation, UnknownCategoryName,
                                      worksheet.Cells[row, UnknownCategory380Index].Value.ToString(),
                                      worksheet.Cells[6, UnknownCategory380Index].Value.ToString().Trim());
                                }

                                if (UnknownCategoryDMAIndex != 0 && worksheet.Cells[row, UnknownCategoryDMAIndex].Value != null)
                                {
                                    RobDataDump(ref robs, "DMA", ref rob, ref allocation, UnknownCategoryName,
                                        worksheet.Cells[row, UnknownCategoryDMAIndex].Value.ToString(),
                                        worksheet.Cells[6, UnknownCategoryDMAIndex].Value.ToString().Trim());
                                }
                                if (UnknownCategoryDMBIndex != 0 && worksheet.Cells[row, UnknownCategoryDMBIndex].Value != null)
                                {
                                    RobDataDump(ref robs, "DMB", ref rob, ref allocation, UnknownCategoryName,
                                       worksheet.Cells[row, UnknownCategoryDMBIndex].Value.ToString(),
                                       worksheet.Cells[6, UnknownCategoryDMBIndex].Value.ToString().Trim());
                                }
                                if (UnknownCategoryDMCIndex != 0 && worksheet.Cells[row, UnknownCategoryDMCIndex].Value != null)
                                {
                                    RobDataDump(ref robs, "DMC", ref rob, ref allocation, UnknownCategoryName,
                                      worksheet.Cells[row, UnknownCategoryDMCIndex].Value.ToString(),
                                      worksheet.Cells[6, UnknownCategoryDMCIndex].Value.ToString().Trim());
                                }

                                if (UnknownCategoryIFOIndex != 0 && worksheet.Cells[row, UnknownCategoryIFOIndex].Value != null)
                                {
                                    RobDataDump(ref robs, "IFO", ref rob, ref allocation, UnknownCategoryName,
                                        worksheet.Cells[row, UnknownCategoryIFOIndex].Value.ToString(),
                                        worksheet.Cells[6, UnknownCategoryIFOIndex].Value.ToString().Trim());
                                }
                                if (UnknownCategoryLSG1Index != 0 && worksheet.Cells[row, UnknownCategoryLSG1Index].Value != null)
                                {
                                    RobDataDump(ref robs, "LSG1", ref rob, ref allocation, UnknownCategoryName,
                                       worksheet.Cells[row, UnknownCategoryLSG1Index].Value.ToString(),
                                       worksheet.Cells[6, UnknownCategoryLSG1Index].Value.ToString().Trim());
                                }
                                if (UnknownCategoryLSMDOIndex != 0 && worksheet.Cells[row, UnknownCategoryLSMDOIndex].Value != null)
                                {
                                    RobDataDump(ref robs, "LSMDO", ref rob, ref allocation, UnknownCategoryName,
                                      worksheet.Cells[row, UnknownCategoryLSMDOIndex].Value.ToString(),
                                      worksheet.Cells[6, UnknownCategoryLSMDOIndex].Value.ToString().Trim());
                                }

                                if (UnknownCategoryLSIFOIndex != 0 && worksheet.Cells[row, UnknownCategoryLSIFOIndex].Value != null)
                                {
                                    RobDataDump(ref robs, "LSIFO", ref rob, ref allocation, UnknownCategoryName,
                                        worksheet.Cells[row, UnknownCategoryLSIFOIndex].Value.ToString(),
                                        worksheet.Cells[6, UnknownCategoryLSIFOIndex].Value.ToString().Trim());
                                }
                                if (UnknownCategoryLUBIndex != 0 && worksheet.Cells[row, UnknownCategoryLUBIndex].Value != null)
                                {
                                    RobDataDump(ref robs, "LUB", ref rob, ref allocation, UnknownCategoryName,
                                       worksheet.Cells[row, UnknownCategoryLUBIndex].Value.ToString(),
                                       worksheet.Cells[6, UnknownCategoryLUBIndex].Value.ToString().Trim());
                                }
                                if (UnknownCategoryMDOIndex != 0 && worksheet.Cells[row, UnknownCategoryMDOIndex].Value != null)
                                {
                                    RobDataDump(ref robs, "MDO", ref rob, ref allocation, UnknownCategoryName,
                                      worksheet.Cells[row, UnknownCategoryMDOIndex].Value.ToString(),
                                      worksheet.Cells[6, UnknownCategoryMDOIndex].Value.ToString().Trim());
                                }
                                if (UnknownCategoryMGOIndex != 0 && worksheet.Cells[row, UnknownCategoryMGOIndex].Value != null)
                                {
                                    RobDataDump(ref robs, "MGO", ref rob, ref allocation, UnknownCategoryName,
                                        worksheet.Cells[row, UnknownCategoryMGOIndex].Value.ToString(),
                                        worksheet.Cells[6, UnknownCategoryMGOIndex].Value.ToString().Trim());
                                }


                                #endregion

                                #endregion

                                string reportTime = worksheet.Cells[row, ReportDatetimeIndex].Value.ToString().Trim();
                                string formIdentifier = worksheet.Cells[row, PositionReportTypeIndex].Value.ToString().Trim();

                                string locnIfNoonReport = null;
                                string formGuid = null;

                                if (worksheet.Cells[row, PositionReportTypeIndex].Value != null)
                                {
                                    if (worksheet.Cells[row, PositionReportTypeIndex].Value.ToString().Trim().Contains("Start"))
                                    {
                                        formIdentifier = "Departure";
                                    }
                                    else if (worksheet.Cells[row, PositionReportTypeIndex].Value.ToString().Trim().Contains("End"))
                                    {
                                        formIdentifier = "Arrival";
                                    }
                                    else if (worksheet.Cells[row, PositionReportTypeIndex].Value.ToString().Trim().Contains("Sea"))
                                    {
                                        formIdentifier = "Noon";
                                        locnIfNoonReport = "N";
                                    }
                                    else if (worksheet.Cells[row, PositionReportTypeIndex].Value.ToString().Trim().Contains("Port"))
                                    {
                                        formIdentifier = "Noon";
                                        locnIfNoonReport = "S";
                                    }
                                }
                                //get vessel code on vessel name
                                Vessel vessel = _DbContext.Vessels.Where(x => x.VesselName == forms.VesselName).FirstOrDefault();
                                if (vessel != null)
                                {
                                    forms.VesselCode = voyages.VesselCode = voyagesToSearch.VesselCode = vessel.VesselCode;
                                    forms.ImoNumber = Convert.ToInt32(vessel.IMONumber);
                                    voyages.IMONumber = voyagesToSearch.IMONumber = vessel.IMONumber;
                                }
                                else
                                {
                                    string LogString = "Vessel: " + forms.VesselName + " Not Found , File Name : " + fileInfo.Name + " , Date : " + DateTime.Now;
                                    LogUtility.WriteLog(LogString);
                                    LogUtility.WriteLog("-------------------------------------------------------------------------------------------");
                                    Utility.Utility.SendEmail(LogString, false);

                                    return (int)ResponseStatus.NOTFOUND;
                                }

                                var formList = _DbContext.Forms.Where(x => x.ReportDateTime == forms.ReportDateTime && x.FormIdentifier.Contains(formIdentifier) && x.ImoNumber == forms.ImoNumber).ToList();

                                if (formList.Count > 0)
                                {
                                    var voyagesRes = _DbContext.Voyages.Where(x =>
                                                                               x.DeparturePort == fromPortDescription &&
                                                                                x.IMONumber == forms.ImoNumber.ToString() &&
                                                                               x.ActualStartOfSeaPassage == forms.ReportDateTime.Value.DateTime).FirstOrDefault();
                                    if (voyagesRes != null)
                                    {
                                        InitialApprove(false, Convert.ToInt64(_currentUser.UserId), voyagesRes.SFPM_VoyagesId.ToString());
                                        // DeleteVoyage(voyagesRes.SFPM_VoyagesId.ToString());
                                    }
                                    foreach (var formid in formList)
                                        DeletePosition(formid.SFPM_Form_Id.ToString());
                                }
                                var reportdatetime = Convert.ToDateTime(reportTime);
                                Forms form = _DbContext.Forms.Where(x => x.ReportDateTime == reportdatetime && x.FormIdentifier.Contains(formIdentifier) && x.ImoNumber == forms.ImoNumber).AsNoTracking().FirstOrDefault();


                                #region Default values
                                forms.CompanyCode = "SCRP";
                                forms.CompanyName = "Scorpio Commercial Management";
                                forms.CreatedDateTime = voyages.CreatedDateTime = DateTime.UtcNow;
                                forms.ModifiedDateTime = voyages.ModifiedDateTime = DateTime.UtcNow;
                                forms.CreatedBy = -1;
                                forms.ModifiedBy = -1;

                                #endregion

                                if (form == null)
                                {
                                    if (formIdentifier == "Departure")
                                    {
                                        forms.FormIdentifier = "Departure VDD";
                                        forms.Port = fromPortDescription;

                                        this._DbContext.Forms.Add(forms);

                                        voyages.ActualEndOfSeaPassage = null;
                                        voyages.ArrivalPort = null;
                                        voyages.ArrivalTimezone = null;
                                        voyages.DepartureTimezone = utcOffset;
                                        voyages.CreatedBy = -1;
                                        voyages.ModifiedBy = -1;
                                        var voyagesRes = _DbContext.Voyages.Where(x =>
                                                                                   x.DeparturePort == fromPortDescription &&
                                                                                    x.IMONumber == forms.ImoNumber.ToString() &&
                                                                                   x.ActualStartOfSeaPassage == forms.ReportDateTime.Value.DateTime).ToList();
                                        if (voyagesRes.Count > 0)
                                        {
                                            foreach (var voyage in voyagesRes)
                                            {
                                                InitialApprove(false, Convert.ToInt64(_currentUser.UserId), voyage.SFPM_VoyagesId.ToString());
                                                DeleteVoyage(voyage.SFPM_VoyagesId.ToString(), Convert.ToInt64(_currentUser.UserId));
                                            }
                                        }


                                        this._DbContext.Voyages.Add(voyages);

                                        Save();

                                        Voyages voyagesResForApprovalAudit = _DbContext.Voyages.Where(x => x.ActualStartOfSeaPassage == voyages.ActualStartOfSeaPassage
                                                                                       && x.ActualEndOfSeaPassage == voyages.ActualEndOfSeaPassage
                                                                                       && x.IMONumber == voyages.IMONumber
                                                                                       && x.VoyageNumber == voyages.VoyageNumber
                                                                                       && x.VesselCode == voyages.VesselCode).FirstOrDefault();
                                        if (voyagesResForApprovalAudit != null)
                                        {
                                            PassagesApprovalAudits passagesApprovalAudits = new PassagesApprovalAudits();
                                            passagesApprovalAudits.IsInitialApproved = true;
                                            passagesApprovalAudits.IsFinalApproved = false;
                                            passagesApprovalAudits.ApprovalStatus = "Initial";
                                            passagesApprovalAudits.ApprovalAction = "Approved";
                                            passagesApprovalAudits.ApproverId = Convert.ToInt64(_currentUser.UserId);
                                            passagesApprovalAudits.ApprovalDateTime = DateTime.UtcNow;
                                            passagesApprovalAudits.VoyagesId = voyagesResForApprovalAudit.SFPM_VoyagesId;
                                            _DbContext.PassagesApprovalAudits.Add(passagesApprovalAudits);
                                            Save();
                                        }
                                        VoyagesController.departureCount++;

                                    }
                                    else if (formIdentifier == "Arrival")
                                    {
                                        forms.FormIdentifier = "Arrival VDD";
                                        forms.Port = toPortDescription;
                                        this._DbContext.Forms.Add(forms);
                                        Save();
                                        VoyagesController.arrivalCount++;
                                    }
                                    else if (formIdentifier == "Noon")
                                    {
                                        forms.FormIdentifier = "Noon VDD";
                                        forms.Location = locnIfNoonReport;
                                        forms.Port = fromPortDescription;
                                        this._DbContext.Forms.Add(forms);
                                        Save();
                                        if (locnIfNoonReport == "N")
                                            VoyagesController.noonReportAtSeaCount++;
                                        else
                                            VoyagesController.noonReportAtPortCount++;
                                    }
                                    if (forms.SFPM_Form_Id != 0 && robs != null)
                                    {
                                        robs.FormId = forms.SFPM_Form_Id;
                                        _DbContext.Robs.Add(robs);
                                        Save();
                                    }
                                    if (forms.SFPM_Form_Id != 0)
                                    {
                                        objAnalyzedweather.FormId = forms.SFPM_Form_Id;
                                        objAnalyzedweather.CalculatedTimeStamp = utcReportdatetime;
                                        objAnalyzedweather.Is24Hour = true;
                                        _DbContext.AnalyzedWeather.Add(objAnalyzedweather);
                                        Save();
                                    }
                                }

                            }
                        }
                        fileStream.Close();
                    }
            }
            ImportExcelForArrivalReport(fileslist);
            // }
            return (int)ResponseStatus.SAVED;
            //return ImportExcelForArrivalReport(fileslist);
        }
        void CountVDDReportInsertedInDatabase(Int64 imoNumber)
        {
            VoyagesController.databaseDepartureCount = _DbContext.Forms.Where(x => x.ImoNumber == imoNumber && x.CreatedBy == -1 && x.FormIdentifier.ToLower().Contains("departure")).Count();
            VoyagesController.databaseArrivalCount = _DbContext.Forms.Where(x => x.ImoNumber == imoNumber && x.CreatedBy == -1 && x.FormIdentifier.ToLower().Contains("arrival")).Count();
            VoyagesController.DatabaseNoonReportAtSeaCount = _DbContext.Forms.Where(x => x.ImoNumber == imoNumber && x.CreatedBy == -1 && x.FormIdentifier.ToLower().Contains("noon") && x.Location == "N").Count();
            VoyagesController.DatabaseNoonReportAtPortCount = _DbContext.Forms.Where(x => x.ImoNumber == imoNumber && x.CreatedBy == -1 && x.FormIdentifier.ToLower().Contains("noon") && x.Location == "S").Count();
        }

        public int ImportExcelForArrivalReport(List<IFormFile> fileslist)
        {
            //IList<IFormFile> fileslist = new List<IFormFile>();
            //fileslist.Add(files);
            string IMONumber = string.Empty;
            if (fileslist != null)
            {
                foreach (IFormFile fileInfo in fileslist)
                    using (var fileStream = fileInfo.OpenReadStream())
                    {
                        using (var package = new ExcelPackage(fileStream))
                        {
                            int RawsheetIndex = 0;
                            for (int sheetcout = 0; sheetcout < package.Workbook.Worksheets.Count(); sheetcout++)
                            {
                                if (package.Workbook.Worksheets[sheetcout].Name.ToString().Contains("Raw Detail"))
                                {
                                    RawsheetIndex = sheetcout;
                                    break;
                                }
                            }
                            ExcelWorksheet worksheet = package.Workbook.Worksheets[RawsheetIndex];
                            var rowCount = worksheet.Dimension.Rows;

                            #region LocalVariableTobeDeleted
                            string vasselName = string.Empty;
                            string vasselId = string.Empty;
                            string vesselCode = string.Empty;
                            string passegid = string.Empty;
                            string voyageNo = string.Empty;
                            string loadCondition = string.Empty;
                            string fromPortDescription = string.Empty;
                            string toPortDescription = string.Empty;
                            string ssp = string.Empty;
                            string esp = string.Empty;

                            string utcOffset = string.Empty;
                            string time = string.Empty;
                            string latitude = string.Empty;
                            string longitude = string.Empty;
                            string draftForward = string.Empty;
                            string draftAft = string.Empty;
                            string cargo = string.Empty;
                            string rPM = string.Empty;
                            string speed = string.Empty;
                            string reportedSpeed = string.Empty;
                            string distance = string.Empty;
                            string distanceToGo = string.Empty;
                            string engineDistance = string.Empty;
                            string slip = string.Empty;
                            string consumption = string.Empty;

                            DateTime voyagestartdate = new DateTime();
                            #region VoyageColumnHeaderIndexDeclare
                            int VesselNameIndex = 0;
                            int VesselIdIndex = 0;
                            int PassegidIndex = 0;
                            int VoyageNoIndex = 0;
                            int LoadConditionIndex = 0;
                            int FromPortDescriptionIndex = 0;
                            int ToPortDescriptionIndex = 0;
                            int StartPassageIndex = 0;
                            int EndPassageIndex = 0;
                            #endregion
                            #region FormsColumnHeaderIndexDeclare
                            int ReportDatetimeIndex = 0;
                            int UTCOffsetIndex = 0;
                            int LatitudeIndex = 0;
                            int LongitudeIndex = 0;
                            int DraftForwardIndex = 0;
                            int DraftAftIndex = 0;
                            int CargoIndex = 0;
                            int RPMIndex = 0;
                            int SpeedIndex = 0;
                            int ReportedSpeedIndex = 0;
                            int DistanceIndex = 0;
                            int DistanceToGoIndex = 0;
                            int EngineDistanceIndex = 0;
                            int SlipIndex = 0;
                            int ConsumptionIndex = 0;
                            int CommentsIndex = 0;
                            int HoursIndex = 0;
                            int COGIndex = 0;
                            int PowerIndex = 0;
                            int PositionReportTypeIndex = 0;
                            #endregion

                            #endregion

                            #region SetColumnHeaderIndexValue
                            for (int column = 1; column <= 130; column++)
                            {
                                switch (worksheet.Cells[1, column].Value.ToString())
                                {
                                    case "VesselName":
                                        VesselNameIndex = column;
                                        break;
                                    case "VesselId":
                                        VesselIdIndex = column;
                                        break;
                                    case "PassageId":
                                        PassegidIndex = column;
                                        break;
                                    case "VoyageNo":
                                        VoyageNoIndex = column;
                                        break;
                                    case "LoadCondition":
                                        LoadConditionIndex = column;
                                        break;
                                    case "FromPortDescription":
                                        FromPortDescriptionIndex = column;
                                        break;
                                    case "ToPortDescription":
                                        ToPortDescriptionIndex = column;
                                        break;
                                    case "Ssp":
                                        StartPassageIndex = column;
                                        break;
                                    case "Esp":
                                        EndPassageIndex = column;
                                        break;
                                    case "Time":
                                        ReportDatetimeIndex = column;
                                        break;
                                    case "UTCOffset":
                                        UTCOffsetIndex = column;
                                        break;
                                    case "Latitude":
                                        LatitudeIndex = column;
                                        break;
                                    case "Longitude":
                                        LongitudeIndex = column;
                                        break;
                                    case "DraftForward":
                                        DraftForwardIndex = column;
                                        break;
                                    case "DraftAft":
                                        DraftAftIndex = column;
                                        break;
                                    case "Cargo":
                                        CargoIndex = column;
                                        break;
                                    case "RPM":
                                        RPMIndex = column;
                                        break;
                                    case "Speed":
                                        SpeedIndex = column;
                                        break;
                                    case "ReportedSpeed":
                                        ReportedSpeedIndex = column;
                                        break;
                                    case "Distance":
                                        DistanceIndex = column;
                                        break;
                                    case "DistanceToGo":
                                        DistanceToGoIndex = column;
                                        break;
                                    case "EngineDistance":
                                        EngineDistanceIndex = column;
                                        break;
                                    case "Slip":
                                        SlipIndex = column;
                                        break;
                                    case "Consumption":
                                        ConsumptionIndex = column;
                                        break;
                                    case "Comments":
                                        CommentsIndex = column;
                                        break;
                                    case "Hours":
                                        HoursIndex = column;
                                        break;
                                    case "COG":
                                        COGIndex = column;
                                        break;
                                    case "Power":
                                        PowerIndex = column;
                                        break;
                                    case "PositionReportType":
                                        PositionReportTypeIndex = column;
                                        break;

                                }
                            }
                            #endregion

                            DateTime ActualStartOfSeaPassage = new DateTime();
                            DateTime ActualEndOfSeaPassage = new DateTime();

                            for (int row = 7; row <= rowCount; row++)
                            {
                                Forms forms = new Forms();
                                Forms formsToSearch = new Forms();
                                #region Voyages


                                if (worksheet.Cells[row, VesselNameIndex].Value != null)
                                {
                                    forms.VesselName = worksheet.Cells[row, VesselNameIndex].Value.ToString().Trim();
                                }
                                if (worksheet.Cells[row, VesselIdIndex].Value != null)
                                {
                                    vasselId = worksheet.Cells[row, VesselIdIndex].Value.ToString().Trim();
                                }
                                if (worksheet.Cells[row, PassegidIndex].Value != null)
                                {
                                    passegid = worksheet.Cells[row, PassegidIndex].Value.ToString().Trim();
                                }
                                if (worksheet.Cells[row, VoyageNoIndex].Value != null)
                                {
                                    string[] numbers = Regex.Split(worksheet.Cells[row, VoyageNoIndex].Value.ToString(), @"\D+");
                                    if (numbers.Length > 0)
                                    {
                                        if (Regex.IsMatch(numbers[0], "^[0-9]+$", RegexOptions.Compiled))
                                        {
                                            voyageNo = numbers[0].Trim();
                                            forms.VoyageNo = Convert.ToInt32(numbers[0].Trim());
                                        }
                                        else if (Regex.IsMatch(numbers[1], "^[0-9]+$", RegexOptions.Compiled))
                                        {
                                            voyageNo = numbers[1].Trim();
                                            forms.VoyageNo = Convert.ToInt32(numbers[1].Trim());
                                        }

                                    }
                                }
                                if (worksheet.Cells[row, LoadConditionIndex].Value != null)
                                {
                                    forms.VesselCondition = worksheet.Cells[row, LoadConditionIndex].Value.ToString().Trim();
                                }
                                if (worksheet.Cells[row, FromPortDescriptionIndex].Value != null)
                                {
                                    fromPortDescription = worksheet.Cells[row, FromPortDescriptionIndex].Value.ToString().Trim();
                                }
                                if (worksheet.Cells[row, ToPortDescriptionIndex].Value != null)
                                {
                                    toPortDescription = worksheet.Cells[row, ToPortDescriptionIndex].Value.ToString().Trim();
                                }
                                if (worksheet.Cells[row, UTCOffsetIndex].Value != null)
                                {
                                    utcOffset = SetTimezoneFormate(worksheet.Cells[row, UTCOffsetIndex].Value.ToString().Trim());
                                    forms.ReportTime = utcOffset;
                                }
                                if (worksheet.Cells[row, StartPassageIndex].Value != null)
                                {
                                    if (utcOffset != null)
                                    {
                                        var hrs = Convert.ToDouble(utcOffset.Substring(1, 2));
                                        var min = Convert.ToDouble(utcOffset.Substring(4, 2));
                                        ActualStartOfSeaPassage = utcOffset.Contains("-") ? Convert.ToDateTime(worksheet.Cells[row, StartPassageIndex].Value.ToString()).AddHours(-hrs).AddMinutes(-min) :
                                            Convert.ToDateTime(worksheet.Cells[row, StartPassageIndex].Value.ToString()).AddHours(hrs).AddMinutes(min);
                                    }
                                }
                                if (worksheet.Cells[row, EndPassageIndex].Value != null)
                                {
                                    if (utcOffset != null)
                                    {
                                        var hrs = Convert.ToDouble(utcOffset.Substring(1, 2));
                                        var min = Convert.ToDouble(utcOffset.Substring(4, 2));
                                        ActualEndOfSeaPassage = utcOffset.Contains("-") ? Convert.ToDateTime(worksheet.Cells[row, EndPassageIndex].Value.ToString()).AddHours(-hrs).AddMinutes(-min) :
                                            Convert.ToDateTime(worksheet.Cells[row, EndPassageIndex].Value.ToString()).AddHours(hrs).AddMinutes(min);
                                    }
                                }



                                #endregion

                                #region Form

                                if (worksheet.Cells[row, UTCOffsetIndex].Value != null)
                                {
                                    utcOffset = SetTimezoneFormate(worksheet.Cells[row, UTCOffsetIndex].Value.ToString().Trim());
                                    forms.ReportTime = utcOffset;
                                }
                                if (worksheet.Cells[row, ReportDatetimeIndex].Value != null)
                                {
                                    if (Convert.ToDateTime(worksheet.Cells[row, ReportDatetimeIndex].Value.ToString()) >= Convert.ToDateTime("2020-05-01 00:00:00"))
                                    {
                                        break;
                                    }
                                    if (utcOffset != null)
                                    {
                                        var hrs = Convert.ToDouble(utcOffset.Substring(1, 2));
                                        var min = Convert.ToDouble(utcOffset.Substring(4, 2));
                                        DateTime ReportDateTime = utcOffset.Contains("-") ? Convert.ToDateTime(worksheet.Cells[row, ReportDatetimeIndex].Value.ToString()).AddHours(-hrs).AddMinutes(-min) :
                                           Convert.ToDateTime(worksheet.Cells[row, ReportDatetimeIndex].Value.ToString()).AddHours(hrs).AddMinutes(min);
                                        forms.ReportDateTime = new DateTimeOffset(ReportDateTime, TimeSpan.FromHours(0));
                                    }

                                }
                                if (worksheet.Cells[row, LatitudeIndex].Value != null)
                                {
                                    forms.Latitude = DecimalToDegreesLatitudeLongitude.DDtoDMS(Convert.ToDouble(worksheet.Cells[row, LatitudeIndex].Value), CoordinateType.latitude);
                                }
                                if (worksheet.Cells[row, LongitudeIndex].Value != null)
                                {
                                    forms.Longitude = DecimalToDegreesLatitudeLongitude.DDtoDMS(Convert.ToDouble(worksheet.Cells[row, LongitudeIndex].Value), CoordinateType.longitude);
                                }
                                if (worksheet.Cells[row, DraftForwardIndex].Value != null)
                                {
                                    forms.FWDDraft = worksheet.Cells[row, DraftForwardIndex].Value.ToString().Trim();
                                }

                                if (worksheet.Cells[row, DraftAftIndex].Value != null)
                                {
                                    forms.AFTDraft = worksheet.Cells[row, DraftAftIndex].Value.ToString().Trim();
                                }
                                if (worksheet.Cells[row, CargoIndex].Value != null)
                                {
                                    forms.Total_Cargo_Onboard = worksheet.Cells[row, CargoIndex].Value.ToString().Trim();
                                }
                                if (worksheet.Cells[row, RPMIndex].Value != null)
                                {
                                    forms.RPM = worksheet.Cells[row, RPMIndex].Value.ToString().Trim();
                                }
                                if (worksheet.Cells[row, SpeedIndex].Value != null)
                                {
                                    speed = worksheet.Cells[row, SpeedIndex].Value.ToString().Trim();
                                }
                                if (worksheet.Cells[row, SpeedIndex].Value != null)
                                {
                                    forms.ObsSpeed = worksheet.Cells[row, SpeedIndex].Value.ToString().Trim();
                                }
                                if (worksheet.Cells[row, DistanceIndex].Value != null)
                                {
                                    forms.ObservedDistance = worksheet.Cells[row, DistanceIndex].Value.ToString().Trim();
                                }
                                if (worksheet.Cells[row, DistanceToGoIndex].Value != null)
                                {
                                    forms.DistanceToGO = worksheet.Cells[row, DistanceToGoIndex].Value.ToString().Trim();
                                }
                                if (worksheet.Cells[row, EngineDistanceIndex].Value != null)
                                {
                                    forms.EngineDistance = worksheet.Cells[row, EngineDistanceIndex].Value.ToString().Trim();
                                }
                                if (worksheet.Cells[row, SlipIndex].Value != null)
                                {
                                    forms.Slip = worksheet.Cells[row, SlipIndex].Value.ToString().Trim();
                                }
                                if (worksheet.Cells[row, ConsumptionIndex].Value != null)
                                {
                                    consumption = worksheet.Cells[row, ConsumptionIndex].Value.ToString().Trim();
                                }

                                //Added new code

                                if (worksheet.Cells[row, CommentsIndex].Value != null)
                                {
                                    forms.Remarks = worksheet.Cells[row, CommentsIndex].Value.ToString().Trim();
                                }
                                if (worksheet.Cells[row, HoursIndex].Value != null)
                                {
                                    forms.SteamingHrs = String.Format("{0:0.00}", Convert.ToDecimal(worksheet.Cells[row, HoursIndex].Value.ToString().Trim()));
                                }
                                if (worksheet.Cells[row, SpeedIndex].Value != null)
                                {
                                    forms.ObsSpeed = worksheet.Cells[row, SpeedIndex].Value.ToString().Trim(); //ordered speed
                                }
                                if (worksheet.Cells[row, COGIndex].Value != null)
                                {
                                    forms.Heading = worksheet.Cells[row, COGIndex].Value.ToString().Trim(); // COG
                                }
                                if (worksheet.Cells[row, PowerIndex].Value != null)
                                {
                                    forms.AvgBHP = worksheet.Cells[row, PowerIndex].Value.ToString().Trim(); // Power
                                }



                                #endregion


                                string formIdentifier = worksheet.Cells[row, PositionReportTypeIndex].Value.ToString().Trim();



                                string locnIfNoonReport = null;
                                string formGuid = null;

                                if (worksheet.Cells[row, PositionReportTypeIndex].Value != null)
                                {
                                    if (worksheet.Cells[row, PositionReportTypeIndex].Value.ToString().Trim().Contains("Start"))
                                    {
                                        formIdentifier = "Departure";
                                        voyagestartdate = ActualStartOfSeaPassage;
                                    }
                                    else if (worksheet.Cells[row, PositionReportTypeIndex].Value.ToString().Trim().Contains("End"))
                                    {
                                        formIdentifier = "Arrival";
                                    }
                                    else if (worksheet.Cells[row, PositionReportTypeIndex].Value.ToString().Trim().Contains("Sea"))
                                    {
                                        formIdentifier = "Noon";
                                        locnIfNoonReport = "N";
                                    }
                                    else if (worksheet.Cells[row, PositionReportTypeIndex].Value.ToString().Trim().Contains("Port"))
                                    {
                                        formIdentifier = "Noon";
                                        locnIfNoonReport = "S";
                                    }
                                }
                                //get vessel code on vessel name
                                Vessel vessel = _DbContext.Vessels.Where(x => x.VesselName == forms.VesselName).FirstOrDefault();

                                if (vessel != null)
                                {
                                    vesselCode = forms.VesselCode = vessel.VesselCode;
                                    forms.ImoNumber = Convert.ToInt32(vessel.IMONumber);
                                    IMONumber = vessel.IMONumber;
                                }
                                else
                                {
                                    string LogString = "Vessel: " + forms.VesselName + " Not Found , File Name : " + fileInfo.Name + " , Date : " + DateTime.Now;
                                    LogUtility.WriteLog(LogString);
                                    LogUtility.WriteLog("-------------------------------------------------------------------------------------------");
                                    Utility.Utility.SendEmail(LogString, false);

                                    return (int)ResponseStatus.NOTFOUND;
                                }
                                if (formIdentifier == "Arrival")
                                {

                                    Voyages voyagesRes = _DbContext.Voyages.Where(x => x.VoyageNumber == Convert.ToInt64(voyageNo) &&
                                                                                    x.DeparturePort == fromPortDescription &&
                                                                                    x.VesselCode == vesselCode &&
                                                                                     x.IMONumber == IMONumber &&
                                                                                    x.ActualStartOfSeaPassage == voyagestartdate).FirstOrDefault();

                                    //update mapping for voyages
                                    if (voyagesRes != null)
                                    {
                                        voyagesRes.ActualEndOfSeaPassage = ActualEndOfSeaPassage;
                                        voyagesRes.ArrivalTimezone = utcOffset;
                                        voyagesRes.ArrivalPort = toPortDescription;
                                        voyagesRes.ModifiedDateTime = DateTime.UtcNow;
                                        voyagesRes.ModifiedBy = -1;

                                        this._DbContext.Voyages.Update(voyagesRes);
                                        Save();
                                    }

                                    PassagesApprovalAudits passagesApprovalAudits = new PassagesApprovalAudits();
                                    if (voyagesRes != null)
                                    {
                                        passagesApprovalAudits = _DbContext.PassagesApprovalAudits.Where(x => x.VoyagesId == voyagesRes.SFPM_VoyagesId).FirstOrDefault();
                                    }
                                    if (passagesApprovalAudits != null && voyagesRes != null)
                                    {
                                        passagesApprovalAudits.IsInitialApproved = true;
                                        passagesApprovalAudits.IsFinalApproved = false;
                                        passagesApprovalAudits.ApprovalStatus = "Initial";
                                        passagesApprovalAudits.ApprovalAction = "Approved";
                                        passagesApprovalAudits.ApproverId = Convert.ToInt64(_currentUser.UserId);
                                        passagesApprovalAudits.ApprovalDateTime = DateTime.UtcNow;
                                        passagesApprovalAudits.VoyagesId = voyagesRes.SFPM_VoyagesId;

                                        _DbContext.PassagesApprovalAudits.Update(passagesApprovalAudits);
                                        Save();
                                    }
                                }
                            }
                        }
                        fileStream.Close();
                    }
            }
            CountVDDReportInsertedInDatabase(Convert.ToInt64(IMONumber));
            return (int)ResponseStatus.SAVED;
        }

        private void RobDataDump(ref Robs robs, string fluidType, ref Rob rob, ref Allocation allocation, string fluidCategory, string consumption, string units)
        {

            if (robs.Rob.Count > 0)
            {
                var robobj = robs.Rob.Where(x => x.FuelType == fluidType).FirstOrDefault();
                if (robobj != null)
                {
                    allocation = new Allocation();
                    allocation.text = consumption; // consumption
                    allocation.Name = fluidCategory;
                    if (robobj.Allocation == null)
                    {
                        robobj.Allocation = new List<Allocation>();
                    }
                    robobj.Allocation.Add(allocation);
                    //robs.Rob.Add(rob);
                }
                else
                {
                    rob.Allocation = new List<Allocation>();
                    allocation = new Allocation();
                    rob.FuelType = fluidType; //fueltype
                    rob.Units = units;
                    allocation.text = consumption; // consumption
                    allocation.Name = fluidCategory;
                    rob.Allocation.Add(allocation);
                    robs.Rob.Add(rob);
                }
            }
            else
            {
                rob.Allocation = new List<Allocation>();
                allocation = new Allocation();
                rob.FuelType = fluidType; //fueltype
                rob.Units = units;
                allocation.text = String.Format("{0:0.000}", Convert.ToDecimal(consumption)); // consumption
                allocation.Name = fluidCategory; //category
                rob.Allocation.Add(allocation);
                robs.Rob.Add(rob);
            }
        }

        public int ImportVesselListExcel()
        {
            DirectoryInfo d = new DirectoryInfo(@"C:\VesselList\");
            FileInfo[] Files = d.GetFiles();
            if (Files != null)
            {
                foreach (FileInfo fileInfo in Files)
                    if (File.Exists(fileInfo.FullName))
                    {
                        using (FileStream fileStream = File.OpenRead(fileInfo.FullName))
                        {
                            using (var package = new ExcelPackage(fileStream))
                            {
                                int RawsheetIndex = 0;
                                for (int sheetcout = 0; sheetcout < package.Workbook.Worksheets.Count(); sheetcout++)
                                {
                                    if (package.Workbook.Worksheets[sheetcout].Name.ToString().Contains("Vessel List"))
                                    {
                                        RawsheetIndex = sheetcout;
                                        break;
                                    }
                                }
                                ExcelWorksheet worksheet = package.Workbook.Worksheets[RawsheetIndex];
                                var rowCount = worksheet.Dimension.Rows;
                                for (int row = 2; row <= rowCount; row++)
                                {
                                    if (worksheet.Cells[row, 2].Value != null)
                                    {
                                        var vessel = _DbContext.Vessels.Where(x => x.VesselName == worksheet.Cells[row, 2].Value.ToString()).FirstOrDefault();
                                        if (vessel == null)
                                        {
                                            vessel = new Vessel();
                                            vessel.VesselName = worksheet.Cells[row, 2].Value.ToString();
                                            if (worksheet.Cells[row, 4].Value != null)
                                            {
                                                var vesselClass = _DbContext.VesselClass.Where(x => x.VesselClassName == worksheet.Cells[row, 4].Value.ToString()).FirstOrDefault();
                                                if (vesselClass != null)
                                                {
                                                    vessel.VesselClassId = vesselClass.SFPM_VesselClassId;
                                                }
                                            }
                                            if (worksheet.Cells[row, 3].Value != null)
                                            {
                                                var vesselType = _DbContext.VesselTypes.Where(x => x.VesselTypeName == worksheet.Cells[row, 3].Value.ToString()).FirstOrDefault();
                                                if (vesselType != null)
                                                {
                                                    vessel.VesselTypeId = vesselType.SFPM_VesselTypeId;
                                                }
                                            }
                                            if (worksheet.Cells[row, 7].Value != null)
                                            {
                                                var vesselOwner = _DbContext.Users.Where(x => (x.FirstName + " " + x.LastName).IndexOf(worksheet.Cells[row, 7].Value.ToString()) > 0).FirstOrDefault();
                                                if (vesselOwner != null)
                                                {
                                                    vessel.VesselOwnerId = vesselOwner.UserId;
                                                }
                                            }
                                            if (worksheet.Cells[row, 5].Value != null)
                                            {
                                                vessel.VesselCode = worksheet.Cells[row, 5].Value.ToString();
                                            }
                                            if (worksheet.Cells[row, 6].Value != null)
                                            {
                                                vessel.Flag = worksheet.Cells[row, 6].Value.ToString();
                                            }
                                            if (worksheet.Cells[row, 10].Value != null)
                                            {
                                                vessel.CallSign = worksheet.Cells[row, 10].Value.ToString();
                                            }
                                            if (worksheet.Cells[row, 11].Value != null)
                                            {
                                                vessel.IMONumber = worksheet.Cells[row, 11].Value.ToString();
                                            }
                                            if (worksheet.Cells[row, 12].Value != null)
                                            {
                                                if (worksheet.Cells[row, 12].Value.ToString().Contains(" "))
                                                {
                                                    vessel.MMSI = 0;
                                                }
                                                else
                                                {
                                                    vessel.MMSI = Convert.ToInt64(worksheet.Cells[row, 12].Value.ToString().Trim());
                                                }
                                            }
                                            if (worksheet.Cells[row, 13].Value != null)
                                            {
                                                if (worksheet.Cells[row, 13].Value.ToString().Contains(" "))
                                                {
                                                    vessel.LOA = 0;
                                                }
                                                else
                                                {
                                                    vessel.LOA = Convert.ToDecimal(worksheet.Cells[row, 13].Value.ToString().Trim());
                                                }
                                            }
                                            if (worksheet.Cells[row, 14].Value != null)
                                            {
                                                if (worksheet.Cells[row, 14].Value.ToString().Contains(" "))
                                                {
                                                    vessel.Beam = 0;
                                                }
                                                else
                                                {
                                                    vessel.Beam = Convert.ToDecimal(worksheet.Cells[row, 14].Value.ToString().Trim());
                                                }
                                            }
                                            if (worksheet.Cells[row, 15].Value != null)
                                            {
                                                vessel.DWT = Convert.ToDecimal(worksheet.Cells[row, 15].Value.ToString());
                                            }
                                            if (worksheet.Cells[row, 17].Value != null)
                                            {
                                                vessel.Capacity = Convert.ToDecimal(worksheet.Cells[row, 17].Value.ToString());
                                            }
                                            vessel.CreatedBy = vessel.ModifiedBy = -1;
                                            vessel.Status = Status.ACTIVE;
                                            vessel.CreatedDateTime = vessel.ModifiedDateTime = DateTime.Now;
                                            _DbContext.Vessels.Add(vessel);
                                            Save();
                                        }
                                    }

                                }
                            }
                            fileStream.Close();
                        }
                    }
            }
            return (int)ResponseStatus.SAVED;
        }

        public PassageDataChartViewModel GetPassageDataForChart(PassageDataChartViewModel passageDataCharts,long loginUserId)
        {
            PassageDataViewModel passageDataObj;
            List<PassageDataViewModel> passageDataListObj = new List<PassageDataViewModel>();
            var userrole = _DbContext.Roles.Where(x => x.RoleId == (_DbContext.UserRoleMapping.Where(y => y.UserId == loginUserId).Select(y => y.RoleId).FirstOrDefault())).Select(x => x.RoleName).AsNoTracking().FirstOrDefault();
            var imoNumbers = (from u in _DbContext.UserVesselGroupMappings.AsEnumerable()
                             join vu in _DbContext.VesselGroupVesselMapping.AsEnumerable() on u.VesselGroupId equals vu.VesselGroupId
                             join vsl in _DbContext.Vessels.AsEnumerable() on vu.VesselId equals vsl.SFPM_VesselId
                             where u.UserId == loginUserId
                              select vsl.IMONumber).ToList();

            foreach (var passageDataChart in passageDataCharts.PassageData)
            {
                passageDataObj = new PassageDataViewModel();
                passageDataObj.MeteostratumDataList = new List<MeteoStratumDataList>();
                passageDataObj.AnalyzedWeatherDataList = new List<AnalyzedWeatherDataList>();
                if (imoNumbers.Contains(passageDataChart.IMONumber.ToString()) || userrole.ToLower() == Constant.administrator)
                {
                    passageDataObj.IMONumber = passageDataChart.IMONumber;
                    passageDataObj.StartDateTime = passageDataChart.StartDateTime;
                    passageDataObj.EndDateTime = passageDataChart.EndDateTime;
                    passageDataObj.MeteostratumDataList.AddRange(_DbContext.Query<MeteoStratumDataList>().FromSql("sp_getPassageDataForChart @p0, @p1, @p2", passageDataChart.IMONumber.ToString(), passageDataChart.StartDateTime, passageDataChart.EndDateTime).ToList());
                    passageDataObj.AnalyzedWeatherDataList.AddRange(_DbContext.Query<AnalyzedWeatherDataList>().FromSql("sp_getPassageDataForanalyzedweather @p0, @p1, @p2", passageDataChart.IMONumber.ToString(), passageDataChart.StartDateTime, passageDataChart.EndDateTime).ToList());
                    passageDataListObj.Add(passageDataObj);
                }
            }

            return new PassageDataChartViewModel() { PassageData = passageDataListObj };
        }

        public List<ECADataViewModel> GetECAData(string type)
        {
            var eCADataList = type == "HRA" ? _DbContext.ECADatas.Where(x => x.Region.Contains("HRA ")).Select(x => new { x.Region }).Distinct().ToList()
                : _DbContext.ECADatas.Where(x => !x.Region.Contains("HRA ")).Select(x => new { x.Region }).Distinct().ToList();
            List<ECADataViewModel> ECADataListObj = new List<ECADataViewModel>();
            ECADataViewModel ECADataObj;
            foreach (var eCAData in eCADataList)
            {
                ECADataObj = new ECADataViewModel();
                ECADataObj.LatLogList = new List<ECALatlongViewModel>();
                ECADataObj.Region = eCAData.Region;
                ECADataObj.LatLogList = _DbContext.ECADatas.Where(x => x.Region == eCAData.Region)
                    .Select(x => new ECALatlongViewModel
                    { Latitude = x.Latitude, Longitude = x.Longitude }).ToList();
                ECADataListObj.Add(ECADataObj);
            }
            return ECADataListObj;
        }
        #endregion

        #region Others

        public IEnumerable<DirectionValueMapping> GetDirectionList()
        {
            return _DbContext.DirectionValueMappings.ToList();
        }
        public string AnalyzedWeatherCal(long formId)
        {
            var form = _DbContext.Forms.Where(x => x.SFPM_Form_Id == formId).FirstOrDefault();
            if (form != null)
            {
                CreateAnalyzedWeatherBasedOnMeteoStratumPosition(form);
                CreateAnalyzedWeather(form);
                return "Success: Analyzed weather calculation done";
            }
            return "Failed: No record found";
        }
        public string GetMarineWeatherImageToken()
        {
            IConfigurationSection section = Startup.StaticConfig.GetSection("MarineWeatherAPICredentials");
            var client = new RestSharp.RestClient(AzureVaultKey.GetVaultValue("MeteoWeatherImageBaseURLForToken"));
            var request = new RestSharp.RestRequest(RestSharp.Method.POST);
            request.AddHeader("Authorization", "Basic YXBpQ2xpZW50Nzo9SEJSIXM2TGcmTCorVUNn");
            request.AddHeader("Content-Type", "String");
            string encodedBody = string.Format("grant_type=password&username=" + AzureVaultKey.GetVaultValue("MeteoWeatherImageUsername") + "&password=" + AzureVaultKey.GetVaultValue("MeteoWeatherImagePassword") + "&scope=web default rights claims openid");
            request.AddParameter("application/x-www-form-urlencoded", encodedBody, RestSharp.ParameterType.RequestBody);
            RestSharp.IRestResponse response = client.Execute(request);
            MarineWeatherImageToken = JObject.Parse(response.Content)["access_token"].ToString();
            return JObject.Parse(response.Content)["access_token"].ToString();
        }

        public RestSharp.IRestResponse GetMarineWeatherImage(string SERVICE, string VERSION, string REQUEST, string FORMAT, string TRANSPARENT,
            string map, string CURRENT_DATE, string CURRENT_HOUR, string TILES, string LAYERS, string TIME,
            string FORECAST_DATE, string FORECAST_HOUR, string WIDTH, string HEIGHT, string CRS, string STYLES,
            string BBOX)
        {
            IConfigurationSection section = Startup.StaticConfig.GetSection("MarineWeatherAPICredentials");
        Loop:
            var client = new RestSharp.RestClient(AzureVaultKey.GetVaultValue("MarineWeatherImageBaseUrl"));
            var request = new RestSharp.RestRequest(RestSharp.Method.GET);
            request.AddHeader("Authorization", "Bearer " + MarineWeatherImageToken);
            request.AddHeader("Content-Type", "application/json; charset=utf-8");
            request.AddParameter("SERVICE", SERVICE, RestSharp.ParameterType.QueryString);
            request.AddParameter("VERSION", VERSION, RestSharp.ParameterType.QueryString);
            request.AddParameter("REQUEST", REQUEST, RestSharp.ParameterType.QueryString);
            request.AddParameter("FORMAT", FORMAT, RestSharp.ParameterType.QueryString);
            request.AddParameter("TRANSPARENT", TRANSPARENT, RestSharp.ParameterType.QueryString);

            request.AddParameter("map", map, RestSharp.ParameterType.QueryString);
            request.AddParameter("CURRENT_DATE", CURRENT_DATE, RestSharp.ParameterType.QueryString);
            request.AddParameter("CURRENT_HOUR", CURRENT_HOUR, RestSharp.ParameterType.QueryString);
            request.AddParameter("TILES", TILES, RestSharp.ParameterType.QueryString);
            request.AddParameter("LAYERS", LAYERS, RestSharp.ParameterType.QueryString);
            request.AddParameter("TIME", TIME, RestSharp.ParameterType.QueryString);

            request.AddParameter("FORECAST_DATE", FORECAST_DATE, RestSharp.ParameterType.QueryString);
            request.AddParameter("FORECAST_HOUR", FORECAST_HOUR, RestSharp.ParameterType.QueryString);
            request.AddParameter("WIDTH", WIDTH, RestSharp.ParameterType.QueryString);
            request.AddParameter("HEIGHT", HEIGHT, RestSharp.ParameterType.QueryString);
            request.AddParameter("CRS", CRS, RestSharp.ParameterType.QueryString);

            request.AddParameter("STYLES", STYLES, RestSharp.ParameterType.QueryString);
            request.AddParameter("BBOX", BBOX, RestSharp.ParameterType.QueryString);

            RestSharp.IRestResponse response = client.Execute(request);
            if (response.StatusCode != HttpStatusCode.OK)
            {
                MarineWeatherImageToken = GetMarineWeatherImageToken();
                goto Loop;
            }
            return response;
        }
        public int DeleteMeteoStratumData(string imoNumber, DateTime gpsDatetime)
        {

            var meteostratum = _DbContext.MeteoStratumData.Where(x => x.IMONumber == imoNumber && x.GPSTimeStamp == gpsDatetime).ToList();
            if (meteostratum.Count > 0)
            {
                _DbContext.MeteoStratumData.RemoveRange(meteostratum);
                Save();

                var formList = _DbContext.Forms.Where(x => x.ImoNumber == Convert.ToInt64(imoNumber)).Select(x => new {
                    x.SFPM_Form_Id,
                    x.ReportDateTime,
                    x.ReportTime
                }).ToList();
                var formDetail = formList.Where(x => x.ReportTime.Contains("-") ?
                x.ReportDateTime.Value.DateTime >= gpsDatetime.AddHours(-Convert.ToInt32(x.ReportTime.Substring(1, 2))).AddMinutes(-Convert.ToInt32(x.ReportTime.Substring(4, 2)))
                : x.ReportDateTime.Value.DateTime >= gpsDatetime.AddHours(Convert.ToInt32(x.ReportTime.Substring(1, 2))).AddMinutes(Convert.ToInt32(x.ReportTime.Substring(4, 2))))
                 .OrderBy(x => x.ReportDateTime).FirstOrDefault();

                var form = _DbContext.Forms.Where(x => x.SFPM_Form_Id == formDetail.SFPM_Form_Id).FirstOrDefault();


                //var formList = _DbContext.Forms.Where(x => x.ImoNumber == Convert.ToInt64(imoNumber)).OrderByDescending(x => x.ReportDateTime).ToList();
                //var form = formList.Where(x => x.ReportTime.Contains("-") ? x.ReportDateTime.Value.DateTime > gpsDatetime.AddHours(-Convert.ToInt32(x.ReportTime.Substring(1, 2))).AddMinutes(-Convert.ToInt32(x.ReportTime.Substring(4, 2))) && x.ReportDateTime.Value.DateTime >= gpsDatetime.AddHours(-Convert.ToInt32(x.ReportTime.Substring(1, 2))).AddMinutes(-Convert.ToInt32(x.ReportTime.Substring(4, 2)))
                //: x.ReportDateTime.Value.DateTime < gpsDatetime.AddHours(Convert.ToInt32(x.ReportTime.Substring(1, 2))).AddMinutes(Convert.ToInt32(x.ReportTime.Substring(4, 2))) && x.ReportDateTime.Value.DateTime >= gpsDatetime.AddHours(Convert.ToInt32(x.ReportTime.Substring(1, 2))).AddMinutes(Convert.ToInt32(x.ReportTime.Substring(4, 2)))).OrderByDescending(x => x.ReportDateTime).FirstOrDefault();
                if (form != null)
                {
                    CreateAnalyzedWeatherBasedOnMeteoStratumPosition(form);
                    CreateAnalyzedWeather(form);
                }
                return 1;
            }
            return 0;
        }

        public string GetPositionWarningDistanceAPIToken()
        {
            try
            {
                var client = new RestSharp.RestClient(AzureVaultKey.GetVaultValue("DistanceApiEndpointLogin"));
                client.Timeout = -1;
                var request = new RestSharp.RestRequest(RestSharp.Method.POST);
                request.AddHeader("Content-Type", "text/xml; charset=utf-8");
                request.AddParameter("text/xml; charset=utf-8,application/xml", "<?xml version=\"1.0\" encoding=\"utf-8\"?>\n<soap:Envelope xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" xmlns:soap=\"http://schemas.xmlsoap.org/soap/envelope/\">\n  <soap:Body>\n    <Login xmlns=\"http://veson.com/webservices/\">\n      <username>" + AzureVaultKey.GetVaultValue("DistanceApiEndpointUser") + "</username>\n      <password>" + AzureVaultKey.GetVaultValue("DistanceApiEndpointPass") + "</password>\n    </Login>\n  </soap:Body>\n</soap:Envelope>", RestSharp.ParameterType.RequestBody);
                RestSharp.IRestResponse response = client.Execute(request);
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(response.Content);
                XmlNodeList node = doc.GetElementsByTagName("LoginResult");
                return node[0].InnerText;
            }
            catch (Exception ex)
            {
                string LogString = "Distance API Exception   when generating distance api token  in  " + AzureVaultKey.GetVaultValue("SfpmApiUrl") + "  Environment  and  UTCDate : " + DateTime.UtcNow;
                LogUtility.WriteLog(LogString);
                LogUtility.WriteLog("-------------------------------------------------------------------------------------------");
                Utility.Utility.SendEmail(LogString, true);
                return "";
            }
        }

        public decimal GetPositionWarningDistanceValue(double formLat, double formLong, double toLat, double toLong, long formId)
        {
            try
            {
                //Utility.Utility.SendEmail("Distance ApI called for FormId : " + formId, true);
                int count = 0;
            newLoginTickit:
                var client = new RestSharp.RestClient(AzureVaultKey.GetVaultValue("DistanceApiEndpointDistance"));
                var request = new RestSharp.RestRequest(RestSharp.Method.POST);
                request.AddHeader("postman-token", "07440bd8-36ff-7211-758c-8dfed06a01c2");
                request.AddHeader("cache-control", "no-cache");
                request.AddHeader("content-type", "text/xml");
                request.AddParameter("text/xml", "<?xml version=\"1.0\" encoding=\"utf-8\"?>\n<soap:Envelope xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" xmlns:soap=\"http://schemas.xmlsoap.org/soap/envelope/\">\n <soap:Header>\n <DistanceHeader xmlns=\"http://veson.com/webservices/\">\n <loginTicket>" + logintickit + "</loginTicket>\n </DistanceHeader>\n </soap:Header>\n <soap:Body>\n <PointToPoint xmlns=\"http://veson.com/webservices/\">\n <prefs>\n <viaList>\n <int>0</int>\n <int>0</int>\n </viaList>\n <viaPrefs>\n <int>0</int>\n <int>0</int>\n </viaPrefs>\n <disabledRegions>\n <int>0</int>\n <int>0</int>\n </disabledRegions>\n <avoidanceRegions>\n <int>0</int>\n <int>0</int>\n </avoidanceRegions>\n <avoidInshore>true</avoidInshore>\n <avoidRivers>true</avoidRivers>\n <deepWaterFactor>0</deepWaterFactor>\n <minDepth>0</minDepth>\n <minHeight>0</minHeight>\n </prefs>\n <latFrom>" + formLat + "</latFrom>\n <lonFrom>" + formLong + "</lonFrom>\n <latTo>" + toLat + "</latTo>\n <lonTo>" + toLong + "</lonTo>\n </PointToPoint>\n </soap:Body>\n</soap:Envelope>", RestSharp.ParameterType.RequestBody);
                RestSharp.IRestResponse response1 = client.Execute(request);
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(response1.Content);
                XmlNodeList authNode = doc.GetElementsByTagName("faultstring");

                if (authNode.Count != 0 && count < 1)
                {
                    logintickit = GetPositionWarningDistanceAPIToken();
                    count++;
                    goto newLoginTickit;

                }
                XmlNodeList resultNode = doc.GetElementsByTagName("distance");
                return Convert.ToDecimal(resultNode[0].InnerText);
            }
            catch (Exception ex)
            {
                string LogString = "Distance API Exception  FromLat : " + formLat + "  ,FromLon : " + formLong + " ,ToLat : " + toLat + " ,ToLon : " + toLong + "  in  " + AzureVaultKey.GetVaultValue("SfpmApiUrl") + "  Environment  and  UTCDate : " + DateTime.UtcNow;
                LogUtility.WriteLog(LogString);
                LogUtility.WriteLog("-------------------------------------------------------------------------------------------");
                Utility.Utility.SendEmail(LogString, true);
                return 0;
            }
        }

        public void CreateAnalyzedWeather(Forms form)
        {
            if (((form.FormIdentifier.ToLower().Contains("noon")) && (form.Location.ToLower() == "n")) || form.FormIdentifier.ToLower().Contains("arrival"))
            {

                //var previousFormReportDatetime = _DbContext.Forms.Where(x => x.ImoNumber == form.ImoNumber && x.VoyageNo == form.VoyageNo && x.ReportDateTime < form.ReportDateTime).OrderByDescending(x => x.ReportDateTime).Select(x => x.ReportDateTime).FirstOrDefault();
                var previousForm = _DbContext.Forms.Where(x => x.ImoNumber == form.ImoNumber && x.ReportDateTime < form.ReportDateTime
                && (((form.FormIdentifier.ToLower().Contains("noon")) && (form.Location.ToLower() == "n")) || form.FormIdentifier.ToLower().Contains("arrival"))).OrderByDescending(x => x.ReportDateTime).FirstOrDefault();
                //if (previousFormReportDatetime != null)
                if (previousForm != null)
                {
                    var hrs1 = Convert.ToDouble(previousForm.ReportTime.Substring(1, 2));
                    var min1 = Convert.ToDouble(previousForm.ReportTime.Substring(4, 2));
                    DateTime fromUtcReportdatetime = previousForm.ReportTime.Contains("-") ? previousForm.ReportDateTime.Value.DateTime.AddHours(hrs1).AddMinutes(min1) :
                                   previousForm.ReportDateTime.Value.DateTime.AddHours(-hrs1).AddMinutes(-min1);
                    var totalhour = form.ReportDateTime.Value.DateTime.Subtract(previousForm.ReportDateTime.Value.DateTime).TotalHours;

                    if (totalhour <= 1)
                        return;


                    var previousAnalyzedWeather = new AnalyzedWeather();
                    double timeDelta = 0, totTimeDelta = 0;
                    double totalCurrentdirection = 0;
                    double windSpeed = 0;
                    double totalWaveHeight = 0;
                    double current = 0;
                    double windDirection = 0;
                    double waveDirection = 0;
                    double seaHeight = 0;
                    double swell_Direction = 0;
                    double swell_Height = 0;
                    double seaCurrentSpeed = 0;

                    var analyzedWeatherList = _DbContext.AnalyzedWeather.Where(x => x.FormId == form.SFPM_Form_Id && x.Is24Hour == false).OrderBy(x => x.CalculatedTimeStamp).ToList();
                    foreach (var analyzedWeather in analyzedWeatherList)
                    {
                        if (previousAnalyzedWeather == null || previousAnalyzedWeather.FormId == 0)
                        {
                            //CHANGED LOGIC INSTEAD OF PICKING 1ST TIMEDELTA AS 0.0001 WE WILL SUBSTRACT CURRENT ANALYZED WEATHER CALCULATEDTIMESTAMP WITH PREVIOUS NOON REPORT REPORTDATETIME in utc as calculatedtimestamp is in utc.
                            //timeDelta = 0.0001;
                            timeDelta = ((Convert.ToDateTime(analyzedWeather.CalculatedTimeStamp.ToString()).Subtract(Convert.ToDateTime(fromUtcReportdatetime))).TotalSeconds) / 86400;
                        }
                        else
                        {
                            timeDelta = ((Convert.ToDateTime(analyzedWeather.CalculatedTimeStamp.ToString()).Subtract(Convert.ToDateTime(previousAnalyzedWeather.CalculatedTimeStamp))).TotalSeconds) / 86400;
                        }
                        totTimeDelta += timeDelta;
                        if (analyzedWeather.AnalyzedCurrentDirection != null && analyzedWeather.AnalyzedCurrentDirection != string.Empty)
                        {
                            totalCurrentdirection += Convert.ToDouble(analyzedWeather.AnalyzedCurrentDirection) * timeDelta;
                        }
                        if (analyzedWeather.AnalyzedWind != null && analyzedWeather.AnalyzedWind != string.Empty)
                        {
                            windSpeed += Convert.ToDouble(analyzedWeather.AnalyzedWind) * timeDelta;
                        }
                        if (analyzedWeather.AnalyzedWave != null && analyzedWeather.AnalyzedWave != string.Empty)
                        {
                            totalWaveHeight += Convert.ToDouble(analyzedWeather.AnalyzedWave) * timeDelta;
                        }
                        if (analyzedWeather.AnalyzedCurrent != null && analyzedWeather.AnalyzedCurrent != string.Empty)
                        {
                            current += Convert.ToDouble(analyzedWeather.AnalyzedCurrent) * timeDelta;
                        }
                        if (analyzedWeather.AnalyzedWindDirection != null && analyzedWeather.AnalyzedWindDirection != string.Empty)
                        {
                            windDirection += Convert.ToDouble(analyzedWeather.AnalyzedWindDirection) * timeDelta;
                        }
                        if (analyzedWeather.AnalyzedWaveDiection != null && analyzedWeather.AnalyzedWaveDiection != string.Empty)
                        {
                            waveDirection += Convert.ToDouble(analyzedWeather.AnalyzedWaveDiection) * timeDelta;
                        }
                        if (analyzedWeather.AnalyzedSeaHeight != null && analyzedWeather.AnalyzedSeaHeight != string.Empty)
                        {
                            seaHeight += Convert.ToDouble(analyzedWeather.AnalyzedSeaHeight) * timeDelta;
                        }
                        if (analyzedWeather.AnalyzedSwellDirection != null && analyzedWeather.AnalyzedSwellDirection != string.Empty)
                        {
                            swell_Direction += Convert.ToDouble(analyzedWeather.AnalyzedSwellDirection) * timeDelta;
                        }
                        if (analyzedWeather.AnalyzedSwellHeight != null && analyzedWeather.AnalyzedSwellHeight != string.Empty)
                        {
                            swell_Height += Convert.ToDouble(analyzedWeather.AnalyzedSwellHeight) * timeDelta;
                        }
                        if (analyzedWeather.AnalyzedSeaCurrentSpeedInKnots != null)
                        {
                            seaCurrentSpeed += Convert.ToDouble(analyzedWeather.AnalyzedSeaCurrentSpeedInKnots) * timeDelta;
                        }

                        previousAnalyzedWeather = analyzedWeather;

                    }
                    var analyzedWeatherObj = _DbContext.AnalyzedWeather.Where(x => x.FormId == form.SFPM_Form_Id && x.Is24Hour == true).FirstOrDefault();
                    if (analyzedWeatherObj == null)
                    {
                        if (totTimeDelta == 0)
                        {
                            try
                            {
                                MailMessage mail = new MailMessage();
                                SmtpClient SmtpServer = new SmtpClient(AzureVaultKey.GetVaultValue("SmtpHostName"));
                                mail.From = new MailAddress(AzureVaultKey.GetVaultValue("SmtpFromEmail"));
                                var mailTo = AzureVaultKey.GetVaultValue("SmtpToEmail");
                                var lstofToData = mailTo.Split(';').ToList();
                                foreach (var item in lstofToData)
                                {
                                    mail.To.Add(item);
                                }
                                string env = "";
                                #if Dev
                                   env = "Dev";
                                #elif Uat
                                  env = "Uat";
                                #endif

                                if (env.Equals("Dev"))
                                    mail.Subject = "DEV:Total TimeDelta is 0 in analyzed weather calculation 'NaN occur'";
                                else if (env.Equals("Uat"))
                                    mail.Subject = "UAT:Total TimeDelta is 0 in analyzed weather calculation 'NaN occur'";
                                else
                                    mail.Subject = "Total TimeDelta is 0 in analyzed weather calculation 'NaN occur'";


                                mail.Body = "NaN  set to 0 for this form Id : " + form.SFPM_Form_Id;
                                SmtpServer.EnableSsl = true;
                                SmtpServer.Port = Convert.ToInt32(AzureVaultKey.GetVaultValue("SmtpPort"));
                                SmtpServer.Credentials = new System.Net.NetworkCredential(mail.From.Address, AzureVaultKey.GetVaultValue("SmtpPassword"));
                                SmtpServer.Send(mail);
                            }
                            catch (Exception ex) { }

                        }
                        analyzedWeatherObj = new AnalyzedWeather();
                        analyzedWeatherObj.FormId = form.SFPM_Form_Id;
                        analyzedWeatherObj.AnalyzedWind = String.Format("{0:0.00}", (totTimeDelta == 0 ? 0 : windSpeed / totTimeDelta));
                        analyzedWeatherObj.AnalyzedWave = String.Format("{0:0.00}", (totTimeDelta == 0 ? 0 : totalWaveHeight / totTimeDelta));
                        analyzedWeatherObj.AnalyzedCurrent = String.Format("{0:0.00}", (totTimeDelta == 0 ? 0 : current / totTimeDelta));

                        analyzedWeatherObj.AnalyzedWindDirection = String.Format("{0:0.00}", (totTimeDelta == 0 ? 0 : windDirection / totTimeDelta));
                        analyzedWeatherObj.AnalyzedWaveDiection = String.Format("{0:0.00}", (totTimeDelta == 0 ? 0 : waveDirection / totTimeDelta));
                        analyzedWeatherObj.AnalyzedSeaHeight = String.Format("{0:0.00}", (totTimeDelta == 0 ? 0 : seaHeight / totTimeDelta));
                        analyzedWeatherObj.AnalyzedSwellDirection = String.Format("{0:0.00}", (totTimeDelta == 0 ? 0 : swell_Direction / totTimeDelta));

                        analyzedWeatherObj.AnalyzedSwellHeight = String.Format("{0:0.00}", (totTimeDelta == 0 ? 0 : swell_Height / totTimeDelta));
                        analyzedWeatherObj.AnalyzedCurrentDirection = String.Format("{0:0.00}", (totTimeDelta == 0 ? 0 : totalCurrentdirection / totTimeDelta));
                        analyzedWeatherObj.AnalyzedSeaCurrentSpeedInKnots = Convert.ToDecimal(String.Format("{0:0.00}", (totTimeDelta == 0 ? 0 : seaCurrentSpeed / totTimeDelta)));
                        analyzedWeatherObj.CreatedBy = Convert.ToInt64(_currentUser.UserId);
                        analyzedWeatherObj.CreatedDateTime = DateTime.UtcNow;
                        analyzedWeatherObj.Latitude = form.Latitude;
                        analyzedWeatherObj.Longitude = form.Longitude;
                        analyzedWeatherObj.Is24Hour = true;
                        decimal heading = 0;
                        var headingParse = Decimal.TryParse(form.Heading, out heading);
                        analyzedWeatherObj.Bearing = headingParse ? heading : 0;
                        var hrs = Convert.ToDouble(form.ReportTime.Substring(1, 2));
                        var min = Convert.ToDouble(form.ReportTime.Substring(4, 2));
                        analyzedWeatherObj.CalculatedTimeStamp = form.ReportTime.Contains("-") ? form.ReportDateTime.Value.DateTime.AddHours(hrs).AddMinutes(min) :
                            form.ReportDateTime.Value.DateTime.AddHours(-hrs).AddMinutes(-min);

                        _DbContext.AnalyzedWeather.Add(analyzedWeatherObj);
                        Save();
                    }


                }
            }
        }

        #region

        //TRYING TO CONVERT HOUR INTO MINUTE BUT FOUND DIFICULT AND WHOLE CODE NEED TO BE REWORKED 
        //public void CreateAnalyzedWeatherBasedOnMeteoStratumPosition(Forms form)
        //{
        //    var analyzedWeatherIntervalInMinutes = 30;
        //    var analyzedWeatherContantIntervalInMinutes = 30;
        //    double currentPositionLat = 0;
        //    double currentPositionLong = 0;
        //    double CalculatedLatcurrentPosition = 0;
        //    double CalculatedLongcurrentPosition = 0;
        //    var hrcount = 0; var currenthrcout = 0; var distancecounter = 0;
        //    var minutecount = 0;
        //    if (((form.FormIdentifier.ToLower().Contains("noon")) && (form.Location.ToLower() == "n")) || form.FormIdentifier.ToLower().Contains("arrival"))
        //    {
        //        _DbContext.AnalyzedWeather.RemoveRange(_DbContext.AnalyzedWeather.Where(x => x.FormId == form.SFPM_Form_Id).ToList());
        //        Save();
        //        var hrs = Convert.ToDouble(form.ReportTime.Substring(1, 2));
        //        var min = Convert.ToDouble(form.ReportTime.Substring(4, 2));
        //        DateTime toUtcReportdatetime = form.ReportTime.Contains("-") ? form.ReportDateTime.Value.DateTime.AddHours(hrs).AddMinutes(min) :
        //            form.ReportDateTime.Value.DateTime.AddHours(-hrs).AddMinutes(-min);

        //        var previousForm = _DbContext.Forms.Where(x => x.ImoNumber == form.ImoNumber && x.ReportDateTime < form.ReportDateTime && (((x.FormIdentifier.ToLower().Contains("noon")) && (x.Location.ToLower() == "n")) || x.FormIdentifier.ToLower().Contains("arrival")) ).OrderByDescending(x => x.ReportDateTime).FirstOrDefault();
        //        //var previousForm = _DbContext.Forms.Where(x => x.ImoNumber == form.ImoNumber && x.VoyageNo == form.VoyageNo && x.ReportDateTime < form.ReportDateTime).OrderByDescending(x => x.ReportDateTime).FirstOrDefault();
        //        if (previousForm != null)
        //        {
        //            PointList pointArrayList = new PointList();
        //            pointArrayList.items = new List<items>();
        //            List<items> itemList = new List<items>();
        //            var hrs1 = Convert.ToDouble(previousForm.ReportTime.Substring(1, 2));
        //            var min1 = Convert.ToDouble(previousForm.ReportTime.Substring(4, 2));
        //            DateTime fromUtcReportdatetime = previousForm.ReportTime.Contains("-") ? previousForm.ReportDateTime.Value.DateTime.AddHours(hrs1).AddMinutes(min1) :
        //                           previousForm.ReportDateTime.Value.DateTime.AddHours(-hrs1).AddMinutes(-min1);

        //            //HERE WE WILL NEED TO REMOVE EVENTS TIME
        //            var totalminutes = form.ReportDateTime.Value.DateTime.Subtract(previousForm.ReportDateTime.Value.DateTime).TotalMinutes;
        //            var avgDistance = (Convert.ToDouble(form.ObservedDistance) * analyzedWeatherIntervalInMinutes) / totalminutes;
        //            var f = 0;
        //        Loop:
        //            if (f != 1)
        //            {
        //                //var fromReportdatetime = fromUtcReportdatetime.AddHours(hrcount);
        //                //var toReportdatetime = fromUtcReportdatetime.AddHours(hrcount + 1);
        //                var fromReportdatetime = fromUtcReportdatetime.AddMinutes(minutecount);
        //                var toReportdatetime = fromUtcReportdatetime.AddMinutes(minutecount + analyzedWeatherContantIntervalInMinutes);
        //                // actual logic  increase 1 hr
        //                var meteostratumList = _DbContext.MeteoStratumData.Where(x => x.GPSTimeStamp >= fromReportdatetime && x.GPSTimeStamp <= toReportdatetime && x.IMONumber == form.ImoNumber.ToString()).OrderByDescending(x => x.GPSTimeStamp).ToList();
        //                double bearing = 0;
        //                if (meteostratumList.Count == 1)
        //                {
        //                    var loopcondition = 1 + hrcount - currenthrcout;
        //                    for (int i = 0; i < loopcondition; i++)
        //                    {
        //                        if (distancecounter >= Convert.ToInt32(AzureVaultKey.GetVaultValue("Analyzedweathercalculationhour")))
        //                        {
        //                            XmlNodeList pointList;
        //                            var meteostratum = meteostratumList.FirstOrDefault();
        //                            var meteoLat = ConvertDegreesToRadians(ParseCoordinate(meteostratum.Latitude));
        //                            var meteoLong = ConvertDegreesToRadians(ParseCoordinate(meteostratum.Longitude));
        //                            //if (Convert.ToDouble(currentPositionLat) == 0 && Convert.ToDouble(currentPositionLong) == 0)
        //                            if (Convert.ToDouble(CalculatedLatcurrentPosition) == 0 && Convert.ToDouble(CalculatedLongcurrentPosition) == 0)
        //                            {
        //                                pointList = GetPositionFromDistance(ParseCoordinate(previousForm.Latitude), ParseCoordinate(previousForm.Longitude), ParseCoordinate(meteostratum.Latitude), ParseCoordinate(meteostratum.Longitude));
        //                            }
        //                            else
        //                            {
        //                                //pointList = GetPositionFromDistance(Convert.ToDouble(currentPositionLat), Convert.ToDouble(currentPositionLong), ParseCoordinate(meteostratum.Latitude), ParseCoordinate(meteostratum.Longitude));
        //                                pointList = GetPositionFromDistance(Convert.ToDouble(CalculatedLatcurrentPosition), Convert.ToDouble(CalculatedLongcurrentPosition), ParseCoordinate(meteostratum.Latitude), ParseCoordinate(meteostratum.Longitude));
        //                            }
        //                            var counter = 0;
        //                            for (int j = 0; j <= pointList.Count - 1; j++)
        //                            {

        //                                //     pointList[i].ChildNodes.Item(0).InnerText.Trim();
        //                                var pointlat1 = pointList[j].ChildNodes.Item(1).ChildNodes.Item(0).InnerText;
        //                                var pointlong1 = pointList[j].ChildNodes.Item(1).ChildNodes.Item(1).InnerText;
        //                            pointLoop:
        //                                //if (Convert.ToDouble(currentPositionLat) == 0 && Convert.ToDouble(currentPositionLong) == 0)
        //                                bearing = 0;
        //                                if (Convert.ToDouble(CalculatedLatcurrentPosition) == 0 && Convert.ToDouble(CalculatedLongcurrentPosition) == 0)
        //                                {
        //                                    bearing = GetBearing(ParseCoordinate(previousForm.Latitude), ParseCoordinate(previousForm.Longitude), Convert.ToDouble(pointlat1), Convert.ToDouble(pointlong1));
        //                                }
        //                                else
        //                                {
        //                                    //bearing = GetBearing(currentPositionLat, currentPositionLong, Convert.ToDouble(pointlat1), Convert.ToDouble(pointlong1));
        //                                    bearing = GetBearing(CalculatedLatcurrentPosition, CalculatedLongcurrentPosition, Convert.ToDouble(pointlat1), Convert.ToDouble(pointlong1));
        //                                }
        //                                bearing = ConvertDegreesToRadians(bearing);
        //                                var angular_distance = avgDistance * 1.60934 / 6371;//#radis 6371 in KM of earth

        //                                counter++;
        //                                double lat = 0;
        //                                double lon = 0;
        //                                //if (Convert.ToDouble(currentPositionLat) == 0 && Convert.ToDouble(currentPositionLong) == 0)
        //                                if (Convert.ToDouble(CalculatedLatcurrentPosition) == 0 && Convert.ToDouble(CalculatedLongcurrentPosition) == 0)
        //                                {
        //                                    lat = ConvertDegreesToRadians((ParseCoordinate(previousForm.Latitude)));
        //                                    lon = ConvertDegreesToRadians(ParseCoordinate(previousForm.Longitude));
        //                                }
        //                                else
        //                                {
        //                                    lat = ConvertDegreesToRadians(Convert.ToDouble(currentPositionLat));
        //                                    lon = ConvertDegreesToRadians(Convert.ToDouble(currentPositionLong));
        //                                }
        //                                var newLat = Math.Asin(Math.Sin(lat) * Math.Cos(angular_distance) + Math.Cos(lat) * Math.Sin(angular_distance) * Math.Cos(bearing));
        //                                var newLon = lon + Math.Atan2(Math.Sin(bearing) * Math.Sin(angular_distance) * Math.Cos(lat), Math.Cos(angular_distance) - Math.Sin(lat) * Math.Sin(newLat));

        //                                newLat = ConvertRadiansToDegrees(newLat);
        //                                newLon = ConvertRadiansToDegrees(newLon);
        //                                bearing = ConvertRadiansToDegrees(bearing);

        //                                if (i + 1 == loopcondition)
        //                                {
        //                                    var analyzedObj = new AnalyzedWeather();
        //                                    analyzedObj.AnalyzedCurrent = meteostratum.SeaCurrentSpeedInKnots.ToString();//
        //                                    analyzedObj.AnalyzedCurrentDirection = meteostratum.SeaCurrentDirectionInDegrees.ToString(); //
        //                                    analyzedObj.AnalyzedSeaCurrentSpeedInKnots = meteostratum.SeaCurrentSpeedInKnots;
        //                                    analyzedObj.AnalyzedSeaHeight = meteostratum.SeaHeightInMeters.ToString();
        //                                    analyzedObj.AnalyzedSwellDirection = meteostratum.SeaCurrentDirectionInDegrees.ToString();
        //                                    analyzedObj.AnalyzedSwellHeight = meteostratum.SwellHeightInMeters.ToString();
        //                                    analyzedObj.AnalyzedWave = meteostratum.RiskWaveHeightInMeters.ToString(); //
        //                                    analyzedObj.AnalyzedWaveDiection = meteostratum.TotalWaveDirectionInDegrees.ToString();
        //                                    analyzedObj.AnalyzedWind = meteostratum.WindGustInKnots.ToString(); //
        //                                    analyzedObj.AnalyzedWindDirection = meteostratum.WindDirectionInDegrees.ToString();
        //                                    analyzedObj.Bearing = Convert.ToDecimal(bearing);
        //                                    //analyzedObj.CalculatedTimeStamp = fromUtcReportdatetime.AddHours(currenthrcout + 1);
        //                                    analyzedObj.CalculatedTimeStamp = fromUtcReportdatetime.AddHours(currenthrcout + 1);
        //                                    analyzedObj.CreatedBy = Convert.ToInt64(_currentUser.UserId);
        //                                    analyzedObj.CreatedDateTime = DateTime.UtcNow;
        //                                    analyzedObj.Latitude = DecimalToDegreesLatitudeLongitude.DDtoDMS(newLat, CoordinateType.latitude);
        //                                    analyzedObj.Longitude = DecimalToDegreesLatitudeLongitude.DDtoDMS(newLon, CoordinateType.longitude);
        //                                    analyzedObj.Is24Hour = false;
        //                                    analyzedObj.FormId = form.SFPM_Form_Id;
        //                                    _DbContext.AnalyzedWeather.Add(analyzedObj);
        //                                    Save();

        //                                    currentPositionLat = newLat;
        //                                    currentPositionLong = newLon;

        //                                    CalculatedLatcurrentPosition = newLat;
        //                                    CalculatedLongcurrentPosition = newLon;
        //                                }
        //                                else
        //                                {
        //                                    //CALL METEOSTRATUM API AND GET ANALYZED WEATHER.

        //                                    var analyzedObj = new AnalyzedWeather();
        //                                    analyzedObj.AnalyzedCurrent = "1";//
        //                                    analyzedObj.AnalyzedCurrentDirection = "1"; //
        //                                    analyzedObj.AnalyzedSeaCurrentSpeedInKnots = 1;
        //                                    analyzedObj.AnalyzedSeaHeight = "1";
        //                                    analyzedObj.AnalyzedSwellDirection = "1";
        //                                    analyzedObj.AnalyzedSwellHeight = "1";
        //                                    analyzedObj.AnalyzedWave = "1"; //
        //                                    analyzedObj.AnalyzedWaveDiection = "1";
        //                                    analyzedObj.AnalyzedWind = "1"; //
        //                                    analyzedObj.AnalyzedWindDirection = "1";
        //                                    analyzedObj.Bearing = Convert.ToDecimal(bearing);
        //                                    analyzedObj.CalculatedTimeStamp = fromUtcReportdatetime.AddHours(currenthrcout + 1);
        //                                    analyzedObj.CreatedBy = Convert.ToInt64(_currentUser.UserId);
        //                                    analyzedObj.CreatedDateTime = DateTime.UtcNow;
        //                                    analyzedObj.Latitude = DecimalToDegreesLatitudeLongitude.DDtoDMS(newLat, CoordinateType.latitude);
        //                                    analyzedObj.Longitude = DecimalToDegreesLatitudeLongitude.DDtoDMS(newLon, CoordinateType.longitude);
        //                                    analyzedObj.Is24Hour = false;
        //                                    analyzedObj.FormId = form.SFPM_Form_Id;
        //                                    _DbContext.AnalyzedWeather.Add(analyzedObj);
        //                                    Save();

        //                                    currentPositionLat = newLat;
        //                                    currentPositionLong = newLon;
        //                                }

        //                                itemList.Add(new items() { lat = newLat, log = newLon, reportdatetime = fromUtcReportdatetime.AddHours(currenthrcout + 1) });
        //                                //currentPositionLat = newLat;
        //                                //currentPositionLong = newLon;
        //                                hrcount++;
        //                                currenthrcout++;
        //                                distancecounter = 0;
        //                                //exit  calculation 
        //                                var distancepointtotdist = CalculateDistance(ParseCoordinate(previousForm.Latitude), ParseCoordinate(previousForm.Longitude), Convert.ToDouble(pointlat1), Convert.ToDouble(pointlong1));
        //                                var totdistforcurrpoint = avgDistance * currenthrcout;

        //                                if (fromUtcReportdatetime.AddHours(currenthrcout) < toUtcReportdatetime)
        //                                {
        //                                    if (counter >= loopcondition)
        //                                    {
        //                                        goto Loop;
        //                                    }
        //                                    if (distancepointtotdist > totdistforcurrpoint)
        //                                    {
        //                                        goto pointLoop;
        //                                    }
        //                                }
        //                                else
        //                                {
        //                                    f = 1;
        //                                    break;
        //                                }
        //                            }
        //                        }
        //                        else
        //                        {
        //                            hrcount = currenthrcout;
        //                            var meteostratum = meteostratumList.FirstOrDefault();
        //                            var meteoLat = ConvertDegreesToRadians(ParseCoordinate(meteostratum.Latitude));
        //                            var meteoLong = ConvertDegreesToRadians(ParseCoordinate(meteostratum.Longitude));
        //                            //if (currentPositionLat == 0 && currentPositionLong == 0)
        //                            bearing = 0;
        //                            if (CalculatedLatcurrentPosition == 0 && CalculatedLongcurrentPosition == 0)
        //                            {
        //                                bearing = GetBearing(ParseCoordinate(previousForm.Latitude), ParseCoordinate(previousForm.Longitude), ParseCoordinate(meteostratum.Latitude), ParseCoordinate(meteostratum.Longitude));
        //                            }
        //                            else
        //                            {
        //                                //bearing = GetBearing(currentPositionLat, currentPositionLong, ParseCoordinate(meteostratum.Latitude), ParseCoordinate(meteostratum.Longitude));
        //                                bearing = GetBearing(CalculatedLatcurrentPosition, CalculatedLongcurrentPosition, ParseCoordinate(meteostratum.Latitude), ParseCoordinate(meteostratum.Longitude));
        //                            }
        //                            bearing = ConvertDegreesToRadians(bearing);
        //                            var angular_distance = avgDistance * 1.60934 / 6371;//#radis 6371 in KM of earth
        //                            double lat = 0; double lon = 0;
        //                            //if (Convert.ToDouble(currentPositionLat) == 0 && Convert.ToDouble(currentPositionLong) == 0)
        //                            if (Convert.ToDouble(CalculatedLatcurrentPosition) == 0 && Convert.ToDouble(CalculatedLongcurrentPosition) == 0)
        //                            {
        //                                lat = ConvertDegreesToRadians((ParseCoordinate(previousForm.Latitude)));
        //                                lon = ConvertDegreesToRadians(ParseCoordinate(previousForm.Longitude));

        //                            }
        //                            else
        //                            {
        //                                lat = ConvertDegreesToRadians(Convert.ToDouble(currentPositionLat));
        //                                lon = ConvertDegreesToRadians(Convert.ToDouble(currentPositionLong));

        //                            }
        //                            var newLat = Math.Asin(Math.Sin(lat) * Math.Cos(angular_distance) + Math.Cos(lat) * Math.Sin(angular_distance) * Math.Cos(bearing)); // add current position lat long not for meteostratum
        //                            var newLon = lon + Math.Atan2(Math.Sin(bearing) * Math.Sin(angular_distance) * Math.Cos(lat), Math.Cos(angular_distance) - Math.Sin(lat) * Math.Sin(newLat));

        //                            newLat = ConvertRadiansToDegrees(newLat);
        //                            newLon = ConvertRadiansToDegrees(newLon);
        //                            bearing = ConvertRadiansToDegrees(bearing);

        //                            if (i + 1 == loopcondition)
        //                            {
        //                                var analyzedObj = new AnalyzedWeather();
        //                                analyzedObj.AnalyzedCurrent = meteostratum.SeaCurrentSpeedInKnots.ToString();//
        //                                analyzedObj.AnalyzedCurrentDirection = meteostratum.SeaCurrentDirectionInDegrees.ToString(); //
        //                                analyzedObj.AnalyzedSeaCurrentSpeedInKnots = meteostratum.SeaCurrentSpeedInKnots;
        //                                analyzedObj.AnalyzedSeaHeight = meteostratum.SeaHeightInMeters.ToString();
        //                                analyzedObj.AnalyzedSwellDirection = meteostratum.SeaCurrentDirectionInDegrees.ToString();
        //                                analyzedObj.AnalyzedSwellHeight = meteostratum.SwellHeightInMeters.ToString();
        //                                analyzedObj.AnalyzedWave = meteostratum.RiskWaveHeightInMeters.ToString(); //
        //                                analyzedObj.AnalyzedWaveDiection = meteostratum.TotalWaveDirectionInDegrees.ToString();
        //                                analyzedObj.AnalyzedWind = meteostratum.WindGustInKnots.ToString(); //
        //                                analyzedObj.AnalyzedWindDirection = meteostratum.WindDirectionInDegrees.ToString();
        //                                analyzedObj.Bearing = Convert.ToDecimal(bearing);
        //                                analyzedObj.CalculatedTimeStamp = fromUtcReportdatetime.AddHours(currenthrcout + 1);
        //                                analyzedObj.CreatedBy = Convert.ToInt64(_currentUser.UserId);
        //                                analyzedObj.CreatedDateTime = DateTime.UtcNow;
        //                                analyzedObj.Latitude = DecimalToDegreesLatitudeLongitude.DDtoDMS(newLat, CoordinateType.latitude);
        //                                analyzedObj.Longitude = DecimalToDegreesLatitudeLongitude.DDtoDMS(newLon, CoordinateType.longitude);
        //                                analyzedObj.Is24Hour = false;
        //                                analyzedObj.FormId = form.SFPM_Form_Id;
        //                                _DbContext.AnalyzedWeather.Add(analyzedObj);
        //                                Save();

        //                                currentPositionLat = newLat;
        //                                currentPositionLong = newLon;

        //                                CalculatedLatcurrentPosition = newLat;
        //                                CalculatedLongcurrentPosition = newLon;
        //                            }
        //                            else
        //                            {
        //                                //CALL METEOSTRATUM API AND GET ANALYZED WEATHER.

        //                                var analyzedObj = new AnalyzedWeather();
        //                                analyzedObj.AnalyzedCurrent = "1";//
        //                                analyzedObj.AnalyzedCurrentDirection = "1"; //
        //                                analyzedObj.AnalyzedSeaCurrentSpeedInKnots = 1;
        //                                analyzedObj.AnalyzedSeaHeight = "1";
        //                                analyzedObj.AnalyzedSwellDirection = "1";
        //                                analyzedObj.AnalyzedSwellHeight = "1";
        //                                analyzedObj.AnalyzedWave = "1"; //
        //                                analyzedObj.AnalyzedWaveDiection = "1";
        //                                analyzedObj.AnalyzedWind = "1"; //
        //                                analyzedObj.AnalyzedWindDirection = "1";
        //                                analyzedObj.Bearing = Convert.ToDecimal(bearing);
        //                                analyzedObj.CalculatedTimeStamp = fromUtcReportdatetime.AddHours(currenthrcout + 1);
        //                                analyzedObj.CreatedBy = Convert.ToInt64(_currentUser.UserId);
        //                                analyzedObj.CreatedDateTime = DateTime.UtcNow;
        //                                analyzedObj.Latitude = DecimalToDegreesLatitudeLongitude.DDtoDMS(newLat, CoordinateType.latitude);
        //                                analyzedObj.Longitude = DecimalToDegreesLatitudeLongitude.DDtoDMS(newLon, CoordinateType.longitude);
        //                                analyzedObj.Is24Hour = false;
        //                                analyzedObj.FormId = form.SFPM_Form_Id;
        //                                _DbContext.AnalyzedWeather.Add(analyzedObj);
        //                                Save();

        //                                currentPositionLat = newLat;
        //                                currentPositionLong = newLon;
        //                            }



        //                            hrcount++;
        //                            currenthrcout++;
        //                            distancecounter = 0;


        //                            //exit  calculation 
        //                            if (fromUtcReportdatetime.AddHours(currenthrcout) > toUtcReportdatetime)
        //                            {
        //                                f = 1;
        //                                break;
        //                            }
        //                        }
        //                    }
        //                    goto Loop;
        //                    //}
        //                }
        //                else if (meteostratumList.Count > 1)
        //                {
        //                    var loopcondition = 1 + hrcount - currenthrcout;
        //                    for (int i = 0; i < loopcondition; i++)
        //                    {
        //                        if (distancecounter >= Convert.ToInt32(AzureVaultKey.GetVaultValue("Analyzedweathercalculationhour")))
        //                        {
        //                            XmlNodeList pointList;
        //                            var meteostratum = meteostratumList.Take(1).FirstOrDefault();
        //                            var meteoLat = ConvertDegreesToRadians(ParseCoordinate(meteostratum.Latitude));
        //                            var meteoLong = ConvertDegreesToRadians(ParseCoordinate(meteostratum.Longitude));
        //                            //if (currentPositionLat == 0 && currentPositionLong == 0)
        //                            if (CalculatedLatcurrentPosition == 0 && CalculatedLongcurrentPosition == 0)
        //                            {
        //                                pointList = GetPositionFromDistance(ParseCoordinate(previousForm.Latitude), ParseCoordinate(previousForm.Longitude), ParseCoordinate(form.Latitude), ParseCoordinate(form.Longitude));
        //                            }
        //                            else
        //                            {
        //                                //pointList = GetPositionFromDistance(currentPositionLat, currentPositionLong, ParseCoordinate(meteostratum.Latitude), ParseCoordinate(meteostratum.Longitude));
        //                                pointList = GetPositionFromDistance(CalculatedLatcurrentPosition, CalculatedLongcurrentPosition, ParseCoordinate(meteostratum.Latitude), ParseCoordinate(meteostratum.Longitude));
        //                            }
        //                            var counter = 0;
        //                            for (int j = 0; j <= pointList.Count - 1; j++)
        //                            {
        //                                //     pointList[i].ChildNodes.Item(0).InnerText.Trim();
        //                                var pointlat1 = pointList[j].ChildNodes.Item(1).ChildNodes.Item(0).InnerText;
        //                                var pointlong1 = pointList[j].ChildNodes.Item(1).ChildNodes.Item(1).InnerText;
        //                                //if (currentPositionLat == 0 && currentPositionLong == 0)
        //                                bearing = 0;
        //                                if (CalculatedLatcurrentPosition == 0 && CalculatedLongcurrentPosition == 0)
        //                                {
        //                                    bearing = GetBearing(ParseCoordinate(previousForm.Latitude), ParseCoordinate(previousForm.Longitude), Convert.ToDouble(pointlat1), Convert.ToDouble(pointlong1));
        //                                }
        //                                else
        //                                {
        //                                    //bearing = GetBearing(currentPositionLat, currentPositionLong, Convert.ToDouble(pointlat1), Convert.ToDouble(pointlong1));
        //                                    bearing = GetBearing(CalculatedLatcurrentPosition, CalculatedLongcurrentPosition, Convert.ToDouble(pointlat1), Convert.ToDouble(pointlong1));
        //                                }
        //                                bearing = ConvertDegreesToRadians(bearing);
        //                                var angular_distance = avgDistance * 1.60934 / 6371;//#radis 6371 in KM of earth
        //                            pointLoop:
        //                                counter++;
        //                                double pointlat = 0;
        //                                double pointlong = 0;
        //                                //if (currentPositionLat == 0 && currentPositionLong == 0)
        //                                if (CalculatedLatcurrentPosition == 0 && CalculatedLongcurrentPosition == 0)
        //                                {
        //                                    pointlat = ConvertDegreesToRadians((ParseCoordinate(previousForm.Latitude)));
        //                                    pointlong = ConvertDegreesToRadians(ParseCoordinate(previousForm.Longitude));

        //                                }
        //                                else
        //                                {
        //                                    pointlat = ConvertDegreesToRadians(Convert.ToDouble(currentPositionLat));
        //                                    pointlong = ConvertDegreesToRadians(Convert.ToDouble(currentPositionLong));

        //                                }
        //                                var newLat = Math.Asin(Math.Sin(pointlat) * Math.Cos(angular_distance) + Math.Cos(pointlat) * Math.Sin(angular_distance) * Math.Cos(bearing));
        //                                var newLon = pointlong + Math.Atan2(Math.Sin(bearing) * Math.Sin(angular_distance) * Math.Cos(pointlat), Math.Cos(angular_distance) - Math.Sin(pointlat) * Math.Sin(newLat));

        //                                newLat = ConvertRadiansToDegrees(newLat);
        //                                newLon = ConvertRadiansToDegrees(newLon);
        //                                bearing = ConvertRadiansToDegrees(bearing);
        //                                itemList.Add(new items() { lat = newLat, log = newLon, reportdatetime = fromUtcReportdatetime.AddHours(currenthrcout + 1) });
        //                                currentPositionLat = newLat;
        //                                currentPositionLong = newLon;
        //                                hrcount++;
        //                                currenthrcout++;
        //                                distancecounter = 0;
        //                                //exit  calculation 
        //                                var distancepointtotdist = CalculateDistance(ParseCoordinate(previousForm.Latitude), ParseCoordinate(previousForm.Longitude), Convert.ToDouble(pointlat1), Convert.ToDouble(pointlong1));
        //                                var totdistforcurrpoint = avgDistance * currenthrcout;

        //                                if (fromUtcReportdatetime.AddHours(currenthrcout) < toUtcReportdatetime)
        //                                {
        //                                    if (counter >= loopcondition)
        //                                    {
        //                                        goto Loop;
        //                                    }
        //                                    if (distancepointtotdist > totdistforcurrpoint)
        //                                    {
        //                                        goto pointLoop;
        //                                    }

        //                                }
        //                                else
        //                                {
        //                                    f = 1;
        //                                    break;
        //                                }
        //                            }
        //                        }
        //                        else
        //                        {
        //                            hrcount = currenthrcout;
        //                            var meteostratum = meteostratumList.FirstOrDefault();
        //                            var meteoLat = ConvertDegreesToRadians(ParseCoordinate(meteostratum.Latitude));
        //                            var meteoLong = ConvertDegreesToRadians(ParseCoordinate(meteostratum.Longitude));
        //                            //if (currentPositionLat == 0 && currentPositionLong == 0)
        //                            bearing = 0;
        //                            if (CalculatedLatcurrentPosition == 0 && CalculatedLongcurrentPosition == 0)
        //                            {
        //                                bearing = GetBearing(ParseCoordinate(previousForm.Latitude), ParseCoordinate(previousForm.Longitude), ParseCoordinate(meteostratum.Latitude), ParseCoordinate(meteostratum.Longitude));
        //                            }
        //                            else
        //                            {
        //                                //bearing = GetBearing(currentPositionLat, currentPositionLong, ParseCoordinate(meteostratum.Latitude), ParseCoordinate(meteostratum.Longitude));
        //                                bearing = GetBearing(CalculatedLatcurrentPosition, CalculatedLongcurrentPosition, ParseCoordinate(meteostratum.Latitude), ParseCoordinate(meteostratum.Longitude));
        //                            }
        //                            bearing = ConvertDegreesToRadians(bearing);
        //                            var angular_distance = avgDistance * 1.60934 / 6371;//#radis 6371 in KM of earth

        //                            double lat = 0; double lon = 0;
        //                            //if (currentPositionLat == 0 && currentPositionLong == 0)
        //                            if (CalculatedLatcurrentPosition == 0 && CalculatedLongcurrentPosition == 0)
        //                            {
        //                                lat = ConvertDegreesToRadians((ParseCoordinate(previousForm.Latitude)));
        //                                lon = ConvertDegreesToRadians(ParseCoordinate(previousForm.Longitude));

        //                            }
        //                            else
        //                            {
        //                                lat = ConvertDegreesToRadians(currentPositionLat);
        //                                lon = ConvertDegreesToRadians(currentPositionLong);

        //                            }
        //                            var newLat = Math.Asin(Math.Sin(lat) * Math.Cos(angular_distance) + Math.Cos(lat) * Math.Sin(angular_distance) * Math.Cos(bearing));
        //                            var newLon = lon + Math.Atan2(Math.Sin(bearing) * Math.Sin(angular_distance) * Math.Cos(lat), Math.Cos(angular_distance) - Math.Sin(lat) * Math.Sin(newLat));
        //                            newLat = ConvertRadiansToDegrees(newLat);
        //                            newLon = ConvertRadiansToDegrees(newLon);
        //                            bearing = ConvertRadiansToDegrees(bearing);

        //                            if (i + 1 == loopcondition)
        //                            {
        //                                AnalyzedWeatherCalculation(meteostratumList, form, form.ReportDateTime.Value.DateTime.AddHours(hrcount), newLat.ToString(), newLon.ToString(), fromUtcReportdatetime.AddHours(currenthrcout + 1), bearing);

        //                                currentPositionLat = newLat;
        //                                currentPositionLong = newLon;

        //                                CalculatedLatcurrentPosition = newLat;
        //                                CalculatedLongcurrentPosition = newLon;
        //                            }
        //                            else
        //                            {
        //                                //CALL METEOSTRATUM API AND GET ANALYZED WEATHER.

        //                                var analyzedObj = new AnalyzedWeather();
        //                                analyzedObj.AnalyzedCurrent = "1";//
        //                                analyzedObj.AnalyzedCurrentDirection = "1"; //
        //                                analyzedObj.AnalyzedSeaCurrentSpeedInKnots = 1;
        //                                analyzedObj.AnalyzedSeaHeight = "1";
        //                                analyzedObj.AnalyzedSwellDirection = "1";
        //                                analyzedObj.AnalyzedSwellHeight = "1";
        //                                analyzedObj.AnalyzedWave = "1"; //
        //                                analyzedObj.AnalyzedWaveDiection = "1";
        //                                analyzedObj.AnalyzedWind = "1"; //
        //                                analyzedObj.AnalyzedWindDirection = "1";
        //                                analyzedObj.Bearing = Convert.ToDecimal(bearing);
        //                                analyzedObj.CalculatedTimeStamp = fromUtcReportdatetime.AddHours(currenthrcout + 1);
        //                                analyzedObj.CreatedBy = Convert.ToInt64(_currentUser.UserId);
        //                                analyzedObj.CreatedDateTime = DateTime.UtcNow;
        //                                analyzedObj.Latitude = DecimalToDegreesLatitudeLongitude.DDtoDMS(newLat, CoordinateType.latitude);
        //                                analyzedObj.Longitude = DecimalToDegreesLatitudeLongitude.DDtoDMS(newLon, CoordinateType.longitude);
        //                                analyzedObj.Is24Hour = false;
        //                                analyzedObj.FormId = form.SFPM_Form_Id;
        //                                _DbContext.AnalyzedWeather.Add(analyzedObj);
        //                                Save();

        //                                currentPositionLat = newLat;
        //                                currentPositionLong = newLon;
        //                            }


        //                            //degree canvert demain
        //                            //AnalyzedWeatherCalculation(meteostratumList, form, form.ReportDateTime.Value.DateTime.AddHours(hrcount), newLat.ToString(), newLon.ToString(), fromUtcReportdatetime.AddHours(currenthrcout + 1), bearing);
        //                            //currentPositionLat = newLat;
        //                            //currentPositionLong = newLon;
        //                            hrcount++;
        //                            currenthrcout++;
        //                            distancecounter = 0;

        //                            //exit  calculation 
        //                            if (fromUtcReportdatetime.AddHours(currenthrcout) > toUtcReportdatetime)
        //                            {
        //                                f = 1;
        //                                break;
        //                            }
        //                        }
        //                    }
        //                    goto Loop;
        //                }
        //                else
        //                {
        //                    //exit  calculation 
        //                    if (fromUtcReportdatetime.AddHours(hrcount + 1) > toUtcReportdatetime)
        //                    {
        //                        var loopcondition = hrcount - currenthrcout;
        //                        for (int i = 1; i <= loopcondition; i++)
        //                        {
        //                            if (distancecounter >= Convert.ToInt32(AzureVaultKey.GetVaultValue("Analyzedweathercalculationhour")))
        //                            {
        //                                hrcount = currenthrcout;
        //                                if (fromUtcReportdatetime.AddHours(hrcount) < toUtcReportdatetime)
        //                                {
        //                                    //call distance api
        //                                    XmlNodeList pointList;
        //                                    //if (Convert.ToDouble(currentPositionLat) == 0 && Convert.ToDouble(currentPositionLong) == 0)
        //                                    if (Convert.ToDouble(CalculatedLatcurrentPosition) == 0 && Convert.ToDouble(CalculatedLongcurrentPosition) == 0)
        //                                    {
        //                                        pointList = GetPositionFromDistance(ParseCoordinate(previousForm.Latitude), ParseCoordinate(previousForm.Longitude), ParseCoordinate(form.Latitude), ParseCoordinate(form.Longitude));
        //                                    }
        //                                    else
        //                                    {
        //                                        pointList = GetPositionFromDistance(Convert.ToDouble(currentPositionLat), Convert.ToDouble(currentPositionLong), ParseCoordinate(form.Latitude), ParseCoordinate(form.Longitude));
        //                                    }
        //                                    var counter = 0;
        //                                    for (int j = 0; j <= pointList[0].ChildNodes.Count - 1; j++)
        //                                    {

        //                                        //     pointList[i].ChildNodes.Item(0).InnerText.Trim();
        //                                        var pointlat1 = pointList[j].ChildNodes.Item(1).ChildNodes.Item(0).InnerText;
        //                                        var pointlong1 = pointList[j].ChildNodes.Item(1).ChildNodes.Item(1).InnerText;
        //                                        //if (Convert.ToDouble(currentPositionLat) == 0 && Convert.ToDouble(currentPositionLong) == 0)
        //                                        bearing = 0;
        //                                        if (Convert.ToDouble(CalculatedLatcurrentPosition) == 0 && Convert.ToDouble(CalculatedLongcurrentPosition) == 0)
        //                                        {
        //                                            bearing = GetBearing(ParseCoordinate(previousForm.Latitude), ParseCoordinate(previousForm.Longitude), Convert.ToDouble(pointlat1), Convert.ToDouble(pointlong1));
        //                                        }
        //                                        else
        //                                        {
        //                                            //bearing = GetBearing(currentPositionLat, currentPositionLong, Convert.ToDouble(pointlat1), Convert.ToDouble(pointlong1));
        //                                            bearing = GetBearing(CalculatedLatcurrentPosition, CalculatedLongcurrentPosition, Convert.ToDouble(pointlat1), Convert.ToDouble(pointlong1));
        //                                        }
        //                                    pointLoop:
        //                                        counter++;
        //                                        var angular_distance = avgDistance * 1.60934 / 6371;//#radis 6371 in KM of earth
        //                                        double lat = 0;
        //                                        double lon = 0;
        //                                        bearing = ConvertDegreesToRadians(bearing);
        //                                        //if (Convert.ToDouble(currentPositionLat) == 0 && Convert.ToDouble(currentPositionLong) == 0)
        //                                        if (Convert.ToDouble(CalculatedLatcurrentPosition) == 0 && Convert.ToDouble(CalculatedLongcurrentPosition) == 0)
        //                                        {
        //                                            lat = ConvertDegreesToRadians((ParseCoordinate(previousForm.Latitude)));
        //                                            lon = ConvertDegreesToRadians(ParseCoordinate(previousForm.Longitude));

        //                                        }
        //                                        else
        //                                        {
        //                                            lat = ConvertDegreesToRadians(Convert.ToDouble(currentPositionLat));
        //                                            lon = ConvertDegreesToRadians(Convert.ToDouble(currentPositionLong));

        //                                        }
        //                                        var newLat = Math.Asin(Math.Sin(lat) * Math.Cos(angular_distance) + Math.Cos(lat) * Math.Sin(angular_distance) * Math.Cos(bearing));
        //                                        var newLon = lon + Math.Atan2(Math.Sin(bearing) * Math.Sin(angular_distance) * Math.Cos(lat), Math.Cos(angular_distance) - Math.Sin(lat) * Math.Sin(newLat));

        //                                        newLat = ConvertRadiansToDegrees(newLat);
        //                                        newLon = ConvertRadiansToDegrees(newLon);
        //                                        bearing = ConvertRadiansToDegrees(bearing);
        //                                        itemList.Add(new items() { lat = newLat, log = newLon, reportdatetime = fromUtcReportdatetime.AddHours(currenthrcout + 1) });
        //                                        currentPositionLat = newLat;
        //                                        currentPositionLong = newLon;

        //                                        hrcount++;
        //                                        currenthrcout++;
        //                                        distancecounter = 0;
        //                                        //exit  calculation 
        //                                        var totaldistance = CalculateDistance(ParseCoordinate(previousForm.Latitude), ParseCoordinate(previousForm.Longitude), ParseCoordinate(form.Latitude), ParseCoordinate(form.Longitude));// Convert.ToDouble(pointlat1), Convert.ToDouble(pointlong1));
        //                                        var totaldistancerevals = avgDistance * currenthrcout;
        //                                        if (fromUtcReportdatetime.AddHours(currenthrcout) < toUtcReportdatetime)
        //                                        {
        //                                            if (counter >= loopcondition)
        //                                            {
        //                                                goto Loop;
        //                                            }
        //                                            if (totaldistance > totaldistancerevals)
        //                                            {
        //                                                goto pointLoop;
        //                                            }

        //                                        }
        //                                        else
        //                                        {
        //                                            f = 1;
        //                                            break;
        //                                        }
        //                                    }
        //                                    if (f != 1)
        //                                    {
        //                                        hrcount++;
        //                                        distancecounter++;
        //                                        goto Loop;
        //                                    }
        //                                }
        //                            }
        //                            else
        //                            {
        //                                if (fromUtcReportdatetime.AddHours(hrcount) < toUtcReportdatetime)
        //                                {
        //                                    hrcount = currenthrcout;
        //                                    //   var meteostratum = meteostratumList.FirstOrDefault();
        //                                    var meteoLat = ConvertDegreesToRadians(ParseCoordinate(form.Latitude));
        //                                    var meteoLong = ConvertDegreesToRadians(ParseCoordinate(form.Longitude));
        //                                    //if (currentPositionLat == 0 && currentPositionLong == 0)
        //                                    bearing = 0;
        //                                    if (CalculatedLatcurrentPosition == 0 && CalculatedLongcurrentPosition == 0)
        //                                    {
        //                                        bearing = GetBearing(ParseCoordinate(previousForm.Latitude), ParseCoordinate(previousForm.Longitude), ParseCoordinate(form.Latitude), ParseCoordinate(form.Longitude));
        //                                    }
        //                                    else
        //                                    {
        //                                        //bearing = GetBearing(currentPositionLat, currentPositionLong, ParseCoordinate(form.Latitude), ParseCoordinate(form.Longitude));
        //                                        bearing = GetBearing(CalculatedLatcurrentPosition, CalculatedLongcurrentPosition, ParseCoordinate(form.Latitude), ParseCoordinate(form.Longitude));
        //                                    }
        //                                    bearing = ConvertDegreesToRadians(bearing);
        //                                    var angular_distance = avgDistance * 1.60934 / 6371;//#radis 6371 in KM of earth

        //                                    double lat = 0; double lon = 0;
        //                                    //if (currentPositionLat == 0 && currentPositionLong == 0)
        //                                    if (CalculatedLatcurrentPosition == 0 && CalculatedLongcurrentPosition == 0)
        //                                    {
        //                                        lat = ConvertDegreesToRadians((ParseCoordinate(previousForm.Latitude)));
        //                                        lon = ConvertDegreesToRadians(ParseCoordinate(previousForm.Longitude));

        //                                    }
        //                                    else
        //                                    {
        //                                        lat = ConvertDegreesToRadians(currentPositionLat);
        //                                        lon = ConvertDegreesToRadians(currentPositionLong);

        //                                    }
        //                                    var newLat = Math.Asin(Math.Sin(lat) * Math.Cos(angular_distance) + Math.Cos(lat) * Math.Sin(angular_distance) * Math.Cos(bearing));
        //                                    var newLon = lon + Math.Atan2(Math.Sin(bearing) * Math.Sin(angular_distance) * Math.Cos(lat), Math.Cos(angular_distance) - Math.Sin(lat) * Math.Sin(newLat));
        //                                    newLat = ConvertRadiansToDegrees(newLat);
        //                                    newLon = ConvertRadiansToDegrees(newLon);
        //                                    bearing = ConvertRadiansToDegrees(bearing);
        //                                    //degree canvert demain
        //                                    itemList.Add(new items() { lat = newLat, log = newLon, reportdatetime = fromUtcReportdatetime.AddHours(currenthrcout + 1) });
        //                                    //AnalyzedWeatherCalculation(meteostratumList, form, form.ReportDateTime.Value.DateTime.AddHours(hrcount), newLat.ToString(), newLon.ToString(), fromUtcReportdatetime.AddHours(currenthrcout + 1), bearing);
        //                                    currentPositionLat = newLat;
        //                                    currentPositionLong = newLon;
        //                                    CalculatedLatcurrentPosition = newLat;
        //                                    CalculatedLongcurrentPosition = newLon;
        //                                    hrcount++;
        //                                    currenthrcout++;
        //                                    distancecounter = 0;

        //                                    //exit  calculation 
        //                                    if (fromUtcReportdatetime.AddHours(currenthrcout) > toUtcReportdatetime)
        //                                    {
        //                                        f = 1;
        //                                        break;
        //                                    }
        //                                }
        //                            }
        //                        }
        //                    }
        //                    else
        //                    {
        //                        hrcount++;
        //                        distancecounter++;
        //                        goto Loop;
        //                    }
        //                }
        //            }
        //            pointArrayList.items = itemList;
        //            MeteoApiCallToGetToken(pointArrayList, form.SFPM_Form_Id);
        //        }
        //    }
        //}

        #endregion

        #region
        //3 hour interval code
        //public void CreateAnalyzedWeatherBasedOnMeteoStratumPosition(Forms form)
        //{
        //    double currentPositionLat = 0;
        //    double currentPositionLong = 0;
        //    double CalculatedLatcurrentPosition = 0;
        //    double CalculatedLongcurrentPosition = 0;
        //    var hrcount = 0; var currenthrcout = 0; var distancecounter = 0;
        //    if (((form.FormIdentifier.ToLower().Contains("noon")) && (form.Location.ToLower() == "n")) || form.FormIdentifier.ToLower().Contains("arrival"))
        //    {
        //        _DbContext.AnalyzedWeather.RemoveRange(_DbContext.AnalyzedWeather.Where(x => x.FormId == form.SFPM_Form_Id).ToList());
        //        Save();
        //        var hrs = Convert.ToDouble(form.ReportTime.Substring(1, 2));
        //        var min = Convert.ToDouble(form.ReportTime.Substring(4, 2));
        //        DateTime toUtcReportdatetime = form.ReportTime.Contains("-") ? form.ReportDateTime.Value.DateTime.AddHours(hrs).AddMinutes(min) :
        //            form.ReportDateTime.Value.DateTime.AddHours(-hrs).AddMinutes(-min);

        //        var previousForm = _DbContext.Forms.Where(x => x.ImoNumber == form.ImoNumber && x.ReportDateTime < form.ReportDateTime).OrderByDescending(x => x.ReportDateTime).FirstOrDefault();
        //        if (previousForm != null)
        //        {
        //            PointList pointArrayList = new PointList();
        //            pointArrayList.items = new List<items>();
        //            List<items> itemList = new List<items>();
        //            var hrs1 = Convert.ToDouble(previousForm.ReportTime.Substring(1, 2));
        //            var min1 = Convert.ToDouble(previousForm.ReportTime.Substring(4, 2));
        //            DateTime fromUtcReportdatetime = previousForm.ReportTime.Contains("-") ? previousForm.ReportDateTime.Value.DateTime.AddHours(hrs1).AddMinutes(min1) :
        //                           previousForm.ReportDateTime.Value.DateTime.AddHours(-hrs1).AddMinutes(-min1);
        //            var totalhour = form.ReportDateTime.Value.DateTime.Subtract(previousForm.ReportDateTime.Value.DateTime).TotalHours;
        //            var avgDistance = Convert.ToDouble(form.ObservedDistance) / totalhour;
        //            //var avgDistance = (Convert.ToDouble(form.ObservedDistance) / totalhour)*3;
        //            var f = 0;
        //        Loop:
        //            if (f != 1)
        //            {
        //                var fromReportdatetime = fromUtcReportdatetime.AddHours(hrcount);
        //                //var toReportdatetime = fromUtcReportdatetime.AddHours(hrcount + 1);
        //                var toReportdatetime = fromUtcReportdatetime.AddHours(hrcount + 3);
        //                // actual logic  increase 1 hr
        //                var meteostratumList = _DbContext.MeteoStratumData.Where(x => x.GPSTimeStamp >= fromReportdatetime && x.GPSTimeStamp <= toReportdatetime && x.IMONumber == form.ImoNumber.ToString()).OrderByDescending(x => x.GPSTimeStamp).ToList();
        //                double bearing = 0;
        //                if (meteostratumList.Count == 1)
        //                {
        //                    var loopcondition = 1 + hrcount - currenthrcout;
        //                    for (int i = 0; i < loopcondition; i++)
        //                    {
        //                        if (distancecounter >= Convert.ToInt32(AzureVaultKey.GetVaultValue("Analyzedweathercalculationhour")))
        //                        {
        //                            XmlNodeList pointList;
        //                            var meteostratum = meteostratumList.FirstOrDefault();
        //                            var meteoLat = ConvertDegreesToRadians(ParseCoordinate(meteostratum.Latitude));
        //                            var meteoLong = ConvertDegreesToRadians(ParseCoordinate(meteostratum.Longitude));

        //                            if (Convert.ToDouble(CalculatedLatcurrentPosition) == 0 && Convert.ToDouble(CalculatedLongcurrentPosition) == 0)
        //                            {
        //                                pointList = GetPositionFromDistance(ParseCoordinate(previousForm.Latitude), ParseCoordinate(previousForm.Longitude), ParseCoordinate(meteostratum.Latitude), ParseCoordinate(meteostratum.Longitude));
        //                            }
        //                            else
        //                            {

        //                                pointList = GetPositionFromDistance(Convert.ToDouble(CalculatedLatcurrentPosition), Convert.ToDouble(CalculatedLongcurrentPosition), ParseCoordinate(meteostratum.Latitude), ParseCoordinate(meteostratum.Longitude));
        //                            }
        //                            var counter = 0;
        //                            //for (int j = 0; j <= pointList.Count - 1; j++)
        //                            for (int j = 0; j <= pointList.Count - 1; j = j + 3)
        //                            {

        //                                var pointlat1 = pointList[j].ChildNodes.Item(1).ChildNodes.Item(0).InnerText;
        //                                var pointlong1 = pointList[j].ChildNodes.Item(1).ChildNodes.Item(1).InnerText;
        //                            pointLoop:
        //                                bearing = 0;
        //                                if (Convert.ToDouble(CalculatedLatcurrentPosition) == 0 && Convert.ToDouble(CalculatedLongcurrentPosition) == 0)
        //                                {
        //                                    bearing = GetBearing(ParseCoordinate(previousForm.Latitude), ParseCoordinate(previousForm.Longitude), Convert.ToDouble(pointlat1), Convert.ToDouble(pointlong1));
        //                                }
        //                                else
        //                                {
        //                                    bearing = GetBearing(CalculatedLatcurrentPosition, CalculatedLongcurrentPosition, Convert.ToDouble(pointlat1), Convert.ToDouble(pointlong1));
        //                                }
        //                                bearing = ConvertDegreesToRadians(bearing);
        //                                //var angular_distance = avgDistance * 1.60934 / 6371;//#radis 6371 in KM of earth
        //                                var angular_distance = avgDistance * 3 * 1.60934 / 6371;//#radis 6371 in KM of earth

        //                                counter++;
        //                                double lat = 0;
        //                                double lon = 0;
        //                                if (Convert.ToDouble(CalculatedLatcurrentPosition) == 0 && Convert.ToDouble(CalculatedLongcurrentPosition) == 0)
        //                                {
        //                                    lat = ConvertDegreesToRadians((ParseCoordinate(previousForm.Latitude)));
        //                                    lon = ConvertDegreesToRadians(ParseCoordinate(previousForm.Longitude));
        //                                }
        //                                else
        //                                {
        //                                    lat = ConvertDegreesToRadians(Convert.ToDouble(currentPositionLat));
        //                                    lon = ConvertDegreesToRadians(Convert.ToDouble(currentPositionLong));
        //                                }
        //                                var newLat = Math.Asin(Math.Sin(lat) * Math.Cos(angular_distance) + Math.Cos(lat) * Math.Sin(angular_distance) * Math.Cos(bearing));
        //                                var newLon = lon + Math.Atan2(Math.Sin(bearing) * Math.Sin(angular_distance) * Math.Cos(lat), Math.Cos(angular_distance) - Math.Sin(lat) * Math.Sin(newLat));

        //                                newLat = ConvertRadiansToDegrees(newLat);
        //                                newLon = ConvertRadiansToDegrees(newLon);
        //                                bearing = ConvertRadiansToDegrees(bearing);

        //                                //if (i + 1 == loopcondition)
        //                                if (i + 3 == loopcondition || i + 3 > loopcondition)
        //                                {
        //                                    var analyzedObj = new AnalyzedWeather();
        //                                    analyzedObj.AnalyzedCurrent = meteostratum.SeaCurrentSpeedInKnots.ToString();//
        //                                    analyzedObj.AnalyzedCurrentDirection = meteostratum.SeaCurrentDirectionInDegrees.ToString(); //
        //                                    analyzedObj.AnalyzedSeaCurrentSpeedInKnots = meteostratum.SeaCurrentSpeedInKnots;
        //                                    analyzedObj.AnalyzedSeaHeight = meteostratum.SeaHeightInMeters.ToString();
        //                                    analyzedObj.AnalyzedSwellDirection = meteostratum.SeaCurrentDirectionInDegrees.ToString();
        //                                    analyzedObj.AnalyzedSwellHeight = meteostratum.SwellHeightInMeters.ToString();
        //                                    analyzedObj.AnalyzedWave = meteostratum.RiskWaveHeightInMeters.ToString(); //
        //                                    analyzedObj.AnalyzedWaveDiection = meteostratum.TotalWaveDirectionInDegrees.ToString();
        //                                    analyzedObj.AnalyzedWind = meteostratum.WindGustInKnots.ToString(); //
        //                                    analyzedObj.AnalyzedWindDirection = meteostratum.WindDirectionInDegrees.ToString();
        //                                    analyzedObj.Bearing = Convert.ToDecimal(bearing);
        //                                    //analyzedObj.CalculatedTimeStamp = fromUtcReportdatetime.AddHours(currenthrcout + 1);
        //                                    analyzedObj.CalculatedTimeStamp = fromUtcReportdatetime.AddHours(currenthrcout + 3);
        //                                    analyzedObj.CreatedBy = Convert.ToInt64(_currentUser.UserId);
        //                                    analyzedObj.CreatedDateTime = DateTime.UtcNow;
        //                                    analyzedObj.Latitude = DecimalToDegreesLatitudeLongitude.DDtoDMS(newLat, CoordinateType.latitude);
        //                                    analyzedObj.Longitude = DecimalToDegreesLatitudeLongitude.DDtoDMS(newLon, CoordinateType.longitude);
        //                                    analyzedObj.Is24Hour = false;
        //                                    analyzedObj.FormId = form.SFPM_Form_Id;
        //                                    _DbContext.AnalyzedWeather.Add(analyzedObj);
        //                                    Save();

        //                                    currentPositionLat = newLat;
        //                                    currentPositionLong = newLon;

        //                                    CalculatedLatcurrentPosition = newLat;
        //                                    CalculatedLongcurrentPosition = newLon;
        //                                }
        //                                else
        //                                {
        //                                    //CALL METEOSTRATUM API AND GET ANALYZED WEATHER.

        //                                    var analyzedObj = new AnalyzedWeather();
        //                                    analyzedObj.AnalyzedCurrent = "1";//
        //                                    analyzedObj.AnalyzedCurrentDirection = "1"; //
        //                                    analyzedObj.AnalyzedSeaCurrentSpeedInKnots = 1;
        //                                    analyzedObj.AnalyzedSeaHeight = "1";
        //                                    analyzedObj.AnalyzedSwellDirection = "1";
        //                                    analyzedObj.AnalyzedSwellHeight = "1";
        //                                    analyzedObj.AnalyzedWave = "1"; //
        //                                    analyzedObj.AnalyzedWaveDiection = "1";
        //                                    analyzedObj.AnalyzedWind = "1"; //
        //                                    analyzedObj.AnalyzedWindDirection = "1";
        //                                    analyzedObj.Bearing = Convert.ToDecimal(bearing);
        //                                    //analyzedObj.CalculatedTimeStamp = fromUtcReportdatetime.AddHours(currenthrcout + 1);
        //                                    analyzedObj.CalculatedTimeStamp = fromUtcReportdatetime.AddHours(currenthrcout + 3);
        //                                    analyzedObj.CreatedBy = Convert.ToInt64(_currentUser.UserId);
        //                                    analyzedObj.CreatedDateTime = DateTime.UtcNow;
        //                                    analyzedObj.Latitude = DecimalToDegreesLatitudeLongitude.DDtoDMS(newLat, CoordinateType.latitude);
        //                                    analyzedObj.Longitude = DecimalToDegreesLatitudeLongitude.DDtoDMS(newLon, CoordinateType.longitude);
        //                                    analyzedObj.Is24Hour = false;
        //                                    analyzedObj.FormId = form.SFPM_Form_Id;
        //                                    _DbContext.AnalyzedWeather.Add(analyzedObj);
        //                                    Save();

        //                                    currentPositionLat = newLat;
        //                                    currentPositionLong = newLon;
        //                                }

        //                                //itemList.Add(new items() { lat = newLat, log = newLon, reportdatetime = fromUtcReportdatetime.AddHours(currenthrcout + 1) });
        //                                itemList.Add(new items() { lat = newLat, log = newLon, reportdatetime = fromUtcReportdatetime.AddHours(currenthrcout + 3) });
        //                                //hrcount++;
        //                                //currenthrcout++;
        //                                hrcount = hrcount + 3;
        //                                currenthrcout = currenthrcout + 3;
        //                                distancecounter = 0;
        //                                //exit  calculation 
        //                                var distancepointtotdist = CalculateDistance(ParseCoordinate(previousForm.Latitude), ParseCoordinate(previousForm.Longitude), Convert.ToDouble(pointlat1), Convert.ToDouble(pointlong1));
        //                                var totdistforcurrpoint = avgDistance * currenthrcout;

        //                                if (fromUtcReportdatetime.AddHours(currenthrcout) < toUtcReportdatetime)
        //                                {
        //                                    if (counter >= loopcondition)
        //                                    {
        //                                        goto Loop;
        //                                    }
        //                                    if (distancepointtotdist > totdistforcurrpoint)
        //                                    {
        //                                        goto pointLoop;
        //                                    }
        //                                }
        //                                else
        //                                {
        //                                    f = 1;
        //                                    break;
        //                                }
        //                            }
        //                        }
        //                        else
        //                        {
        //                            hrcount = currenthrcout;
        //                            var meteostratum = meteostratumList.FirstOrDefault();
        //                            var meteoLat = ConvertDegreesToRadians(ParseCoordinate(meteostratum.Latitude));
        //                            var meteoLong = ConvertDegreesToRadians(ParseCoordinate(meteostratum.Longitude));
        //                            bearing = 0;
        //                            if (CalculatedLatcurrentPosition == 0 && CalculatedLongcurrentPosition == 0)
        //                            {
        //                                bearing = GetBearing(ParseCoordinate(previousForm.Latitude), ParseCoordinate(previousForm.Longitude), ParseCoordinate(meteostratum.Latitude), ParseCoordinate(meteostratum.Longitude));
        //                            }
        //                            else
        //                            {
        //                                bearing = GetBearing(CalculatedLatcurrentPosition, CalculatedLongcurrentPosition, ParseCoordinate(meteostratum.Latitude), ParseCoordinate(meteostratum.Longitude));
        //                            }
        //                            bearing = ConvertDegreesToRadians(bearing);
        //                            //var angular_distance = avgDistance * 1.60934 / 6371;//#radis 6371 in KM of earth
        //                            var angular_distance = avgDistance * 3 * 1.60934 / 6371;//#radis 6371 in KM of earth
        //                            double lat = 0; double lon = 0;
        //                            if (Convert.ToDouble(CalculatedLatcurrentPosition) == 0 && Convert.ToDouble(CalculatedLongcurrentPosition) == 0)
        //                            {
        //                                lat = ConvertDegreesToRadians((ParseCoordinate(previousForm.Latitude)));
        //                                lon = ConvertDegreesToRadians(ParseCoordinate(previousForm.Longitude));

        //                            }
        //                            else
        //                            {
        //                                lat = ConvertDegreesToRadians(Convert.ToDouble(currentPositionLat));
        //                                lon = ConvertDegreesToRadians(Convert.ToDouble(currentPositionLong));

        //                            }
        //                            var newLat = Math.Asin(Math.Sin(lat) * Math.Cos(angular_distance) + Math.Cos(lat) * Math.Sin(angular_distance) * Math.Cos(bearing)); // add current position lat long not for meteostratum
        //                            var newLon = lon + Math.Atan2(Math.Sin(bearing) * Math.Sin(angular_distance) * Math.Cos(lat), Math.Cos(angular_distance) - Math.Sin(lat) * Math.Sin(newLat));

        //                            newLat = ConvertRadiansToDegrees(newLat);
        //                            newLon = ConvertRadiansToDegrees(newLon);
        //                            bearing = ConvertRadiansToDegrees(bearing);

        //                            //if(i+1 == loopcondition)
        //                            if (i + 3 == loopcondition || i + 3 > loopcondition)
        //                            {
        //                                var analyzedObj = new AnalyzedWeather();
        //                                analyzedObj.AnalyzedCurrent = meteostratum.SeaCurrentSpeedInKnots.ToString();//
        //                                analyzedObj.AnalyzedCurrentDirection = meteostratum.SeaCurrentDirectionInDegrees.ToString(); //
        //                                analyzedObj.AnalyzedSeaCurrentSpeedInKnots = meteostratum.SeaCurrentSpeedInKnots;
        //                                analyzedObj.AnalyzedSeaHeight = meteostratum.SeaHeightInMeters.ToString();
        //                                analyzedObj.AnalyzedSwellDirection = meteostratum.SeaCurrentDirectionInDegrees.ToString();
        //                                analyzedObj.AnalyzedSwellHeight = meteostratum.SwellHeightInMeters.ToString();
        //                                analyzedObj.AnalyzedWave = meteostratum.RiskWaveHeightInMeters.ToString(); //
        //                                analyzedObj.AnalyzedWaveDiection = meteostratum.TotalWaveDirectionInDegrees.ToString();
        //                                analyzedObj.AnalyzedWind = meteostratum.WindGustInKnots.ToString(); //
        //                                analyzedObj.AnalyzedWindDirection = meteostratum.WindDirectionInDegrees.ToString();
        //                                analyzedObj.Bearing = Convert.ToDecimal(bearing);
        //                                //analyzedObj.CalculatedTimeStamp = fromUtcReportdatetime.AddHours(currenthrcout + 1);
        //                                analyzedObj.CalculatedTimeStamp = fromUtcReportdatetime.AddHours(currenthrcout + 3);
        //                                analyzedObj.CreatedBy = Convert.ToInt64(_currentUser.UserId);
        //                                analyzedObj.CreatedDateTime = DateTime.UtcNow;
        //                                analyzedObj.Latitude = DecimalToDegreesLatitudeLongitude.DDtoDMS(newLat, CoordinateType.latitude);
        //                                analyzedObj.Longitude = DecimalToDegreesLatitudeLongitude.DDtoDMS(newLon, CoordinateType.longitude);
        //                                analyzedObj.Is24Hour = false;
        //                                analyzedObj.FormId = form.SFPM_Form_Id;
        //                                _DbContext.AnalyzedWeather.Add(analyzedObj);
        //                                Save();

        //                                currentPositionLat = newLat;
        //                                currentPositionLong = newLon;

        //                                CalculatedLatcurrentPosition = newLat;
        //                                CalculatedLongcurrentPosition = newLon;
        //                            }
        //                            else
        //                            {
        //                                //CALL METEOSTRATUM API AND GET ANALYZED WEATHER.

        //                                var analyzedObj = new AnalyzedWeather();
        //                                analyzedObj.AnalyzedCurrent = "1";//
        //                                analyzedObj.AnalyzedCurrentDirection = "1"; //
        //                                analyzedObj.AnalyzedSeaCurrentSpeedInKnots = 1;
        //                                analyzedObj.AnalyzedSeaHeight = "1";
        //                                analyzedObj.AnalyzedSwellDirection = "1";
        //                                analyzedObj.AnalyzedSwellHeight = "1";
        //                                analyzedObj.AnalyzedWave = "1"; //
        //                                analyzedObj.AnalyzedWaveDiection = "1";
        //                                analyzedObj.AnalyzedWind = "1"; //
        //                                analyzedObj.AnalyzedWindDirection = "1";
        //                                analyzedObj.Bearing = Convert.ToDecimal(bearing);
        //                                //analyzedObj.CalculatedTimeStamp = fromUtcReportdatetime.AddHours(currenthrcout + 1);
        //                                analyzedObj.CalculatedTimeStamp = fromUtcReportdatetime.AddHours(currenthrcout + 3);
        //                                analyzedObj.CreatedBy = Convert.ToInt64(_currentUser.UserId);
        //                                analyzedObj.CreatedDateTime = DateTime.UtcNow;
        //                                analyzedObj.Latitude = DecimalToDegreesLatitudeLongitude.DDtoDMS(newLat, CoordinateType.latitude);
        //                                analyzedObj.Longitude = DecimalToDegreesLatitudeLongitude.DDtoDMS(newLon, CoordinateType.longitude);
        //                                analyzedObj.Is24Hour = false;
        //                                analyzedObj.FormId = form.SFPM_Form_Id;
        //                                _DbContext.AnalyzedWeather.Add(analyzedObj);
        //                                Save();

        //                                currentPositionLat = newLat;
        //                                currentPositionLong = newLon;
        //                            }



        //                            //hrcount++;
        //                            //currenthrcout++;
        //                            hrcount = hrcount + 3;
        //                            currenthrcout = currenthrcout + 3;
        //                            distancecounter = 0;


        //                            //exit  calculation 
        //                            if (fromUtcReportdatetime.AddHours(currenthrcout) > toUtcReportdatetime)
        //                            {
        //                                f = 1;
        //                                break;
        //                            }
        //                        }
        //                    }
        //                    goto Loop;
        //                    //}
        //                }
        //                else if (meteostratumList.Count > 1)
        //                {
        //                    var loopcondition = 1 + hrcount - currenthrcout;
        //                    //for (int i = 0; i < loopcondition; i++)
        //                    for (int i = 0; i < loopcondition; i = i + 3)
        //                    {
        //                        if (distancecounter >= Convert.ToInt32(AzureVaultKey.GetVaultValue("Analyzedweathercalculationhour")))
        //                        {
        //                            XmlNodeList pointList;
        //                            var meteostratum = meteostratumList.Take(1).FirstOrDefault();
        //                            var meteoLat = ConvertDegreesToRadians(ParseCoordinate(meteostratum.Latitude));
        //                            var meteoLong = ConvertDegreesToRadians(ParseCoordinate(meteostratum.Longitude));
        //                            if (CalculatedLatcurrentPosition == 0 && CalculatedLongcurrentPosition == 0)
        //                            {
        //                                pointList = GetPositionFromDistance(ParseCoordinate(previousForm.Latitude), ParseCoordinate(previousForm.Longitude), ParseCoordinate(form.Latitude), ParseCoordinate(form.Longitude));
        //                            }
        //                            else
        //                            {
        //                                pointList = GetPositionFromDistance(CalculatedLatcurrentPosition, CalculatedLongcurrentPosition, ParseCoordinate(meteostratum.Latitude), ParseCoordinate(meteostratum.Longitude));
        //                            }
        //                            var counter = 0;
        //                            for (int j = 0; j <= pointList.Count - 1; j++)
        //                            {
        //                                var pointlat1 = pointList[j].ChildNodes.Item(1).ChildNodes.Item(0).InnerText;
        //                                var pointlong1 = pointList[j].ChildNodes.Item(1).ChildNodes.Item(1).InnerText;
        //                                bearing = 0;
        //                                if (CalculatedLatcurrentPosition == 0 && CalculatedLongcurrentPosition == 0)
        //                                {
        //                                    bearing = GetBearing(ParseCoordinate(previousForm.Latitude), ParseCoordinate(previousForm.Longitude), Convert.ToDouble(pointlat1), Convert.ToDouble(pointlong1));
        //                                }
        //                                else
        //                                {

        //                                    bearing = GetBearing(CalculatedLatcurrentPosition, CalculatedLongcurrentPosition, Convert.ToDouble(pointlat1), Convert.ToDouble(pointlong1));
        //                                }
        //                                bearing = ConvertDegreesToRadians(bearing);
        //                                //var angular_distance = avgDistance * 1.60934 / 6371;//#radis 6371 in KM of earth
        //                                var angular_distance = avgDistance * 3 * 1.60934 / 6371;//#radis 6371 in KM of earth
        //                            pointLoop:
        //                                counter++;
        //                                double pointlat = 0;
        //                                double pointlong = 0;
        //                                if (CalculatedLatcurrentPosition == 0 && CalculatedLongcurrentPosition == 0)
        //                                {
        //                                    pointlat = ConvertDegreesToRadians((ParseCoordinate(previousForm.Latitude)));
        //                                    pointlong = ConvertDegreesToRadians(ParseCoordinate(previousForm.Longitude));

        //                                }
        //                                else
        //                                {
        //                                    pointlat = ConvertDegreesToRadians(Convert.ToDouble(currentPositionLat));
        //                                    pointlong = ConvertDegreesToRadians(Convert.ToDouble(currentPositionLong));

        //                                }
        //                                var newLat = Math.Asin(Math.Sin(pointlat) * Math.Cos(angular_distance) + Math.Cos(pointlat) * Math.Sin(angular_distance) * Math.Cos(bearing));
        //                                var newLon = pointlong + Math.Atan2(Math.Sin(bearing) * Math.Sin(angular_distance) * Math.Cos(pointlat), Math.Cos(angular_distance) - Math.Sin(pointlat) * Math.Sin(newLat));

        //                                newLat = ConvertRadiansToDegrees(newLat);
        //                                newLon = ConvertRadiansToDegrees(newLon);
        //                                bearing = ConvertRadiansToDegrees(bearing);
        //                                //itemList.Add(new items() { lat = newLat, log = newLon, reportdatetime = fromUtcReportdatetime.AddHours(currenthrcout + 1) });
        //                                itemList.Add(new items() { lat = newLat, log = newLon, reportdatetime = fromUtcReportdatetime.AddHours(currenthrcout + 3) });
        //                                currentPositionLat = newLat;
        //                                currentPositionLong = newLon;
        //                                //hrcount++;
        //                                //currenthrcout++;
        //                                hrcount = hrcount + 3;
        //                                currenthrcout = currenthrcout + 3;
        //                                distancecounter = 0;
        //                                //exit  calculation 
        //                                var distancepointtotdist = CalculateDistance(ParseCoordinate(previousForm.Latitude), ParseCoordinate(previousForm.Longitude), Convert.ToDouble(pointlat1), Convert.ToDouble(pointlong1));
        //                                var totdistforcurrpoint = avgDistance * currenthrcout;

        //                                if (fromUtcReportdatetime.AddHours(currenthrcout) < toUtcReportdatetime)
        //                                {
        //                                    if (counter >= loopcondition)
        //                                    {
        //                                        goto Loop;
        //                                    }
        //                                    if (distancepointtotdist > totdistforcurrpoint)
        //                                    {
        //                                        goto pointLoop;
        //                                    }

        //                                }
        //                                else
        //                                {
        //                                    f = 1;
        //                                    break;
        //                                }
        //                            }
        //                        }
        //                        else
        //                        {
        //                            hrcount = currenthrcout;
        //                            var meteostratum = meteostratumList.FirstOrDefault();
        //                            var meteoLat = ConvertDegreesToRadians(ParseCoordinate(meteostratum.Latitude));
        //                            var meteoLong = ConvertDegreesToRadians(ParseCoordinate(meteostratum.Longitude));
        //                            bearing = 0;
        //                            if (CalculatedLatcurrentPosition == 0 && CalculatedLongcurrentPosition == 0)
        //                            {
        //                                bearing = GetBearing(ParseCoordinate(previousForm.Latitude), ParseCoordinate(previousForm.Longitude), ParseCoordinate(meteostratum.Latitude), ParseCoordinate(meteostratum.Longitude));
        //                            }
        //                            else
        //                            {
        //                                bearing = GetBearing(CalculatedLatcurrentPosition, CalculatedLongcurrentPosition, ParseCoordinate(meteostratum.Latitude), ParseCoordinate(meteostratum.Longitude));
        //                            }
        //                            bearing = ConvertDegreesToRadians(bearing);
        //                            //var angular_distance = avgDistance * 1.60934 / 6371;//#radis 6371 in KM of earth
        //                            var angular_distance = avgDistance * 3 * 1.60934 / 6371;//#radis 6371 in KM of earth

        //                            double lat = 0; double lon = 0;
        //                            if (CalculatedLatcurrentPosition == 0 && CalculatedLongcurrentPosition == 0)
        //                            {
        //                                lat = ConvertDegreesToRadians((ParseCoordinate(previousForm.Latitude)));
        //                                lon = ConvertDegreesToRadians(ParseCoordinate(previousForm.Longitude));

        //                            }
        //                            else
        //                            {
        //                                lat = ConvertDegreesToRadians(currentPositionLat);
        //                                lon = ConvertDegreesToRadians(currentPositionLong);

        //                            }
        //                            var newLat = Math.Asin(Math.Sin(lat) * Math.Cos(angular_distance) + Math.Cos(lat) * Math.Sin(angular_distance) * Math.Cos(bearing));
        //                            var newLon = lon + Math.Atan2(Math.Sin(bearing) * Math.Sin(angular_distance) * Math.Cos(lat), Math.Cos(angular_distance) - Math.Sin(lat) * Math.Sin(newLat));
        //                            newLat = ConvertRadiansToDegrees(newLat);
        //                            newLon = ConvertRadiansToDegrees(newLon);
        //                            bearing = ConvertRadiansToDegrees(bearing);

        //                            //if (i + 1 == loopcondition)
        //                            if (i + 3 == loopcondition || i + 3 > loopcondition)
        //                            {
        //                                //AnalyzedWeatherCalculation(meteostratumList, form, form.ReportDateTime.Value.DateTime.AddHours(hrcount), newLat.ToString(), newLon.ToString(), fromUtcReportdatetime.AddHours(currenthrcout + 1), bearing);
        //                                AnalyzedWeatherCalculation(meteostratumList, form, form.ReportDateTime.Value.DateTime.AddHours(hrcount), newLat.ToString(), newLon.ToString(), fromUtcReportdatetime.AddHours(currenthrcout + 3), bearing);

        //                                currentPositionLat = newLat;
        //                                currentPositionLong = newLon;

        //                                CalculatedLatcurrentPosition = newLat;
        //                                CalculatedLongcurrentPosition = newLon;
        //                            }
        //                            else
        //                            {
        //                                //CALL METEOSTRATUM API AND GET ANALYZED WEATHER.

        //                                var analyzedObj = new AnalyzedWeather();
        //                                analyzedObj.AnalyzedCurrent = "1";//
        //                                analyzedObj.AnalyzedCurrentDirection = "1"; //
        //                                analyzedObj.AnalyzedSeaCurrentSpeedInKnots = 1;
        //                                analyzedObj.AnalyzedSeaHeight = "1";
        //                                analyzedObj.AnalyzedSwellDirection = "1";
        //                                analyzedObj.AnalyzedSwellHeight = "1";
        //                                analyzedObj.AnalyzedWave = "1"; //
        //                                analyzedObj.AnalyzedWaveDiection = "1";
        //                                analyzedObj.AnalyzedWind = "1"; //
        //                                analyzedObj.AnalyzedWindDirection = "1";
        //                                analyzedObj.Bearing = Convert.ToDecimal(bearing);
        //                                //analyzedObj.CalculatedTimeStamp = fromUtcReportdatetime.AddHours(currenthrcout + 1);
        //                                analyzedObj.CalculatedTimeStamp = fromUtcReportdatetime.AddHours(currenthrcout + 3);
        //                                analyzedObj.CreatedBy = Convert.ToInt64(_currentUser.UserId);
        //                                analyzedObj.CreatedDateTime = DateTime.UtcNow;
        //                                analyzedObj.Latitude = DecimalToDegreesLatitudeLongitude.DDtoDMS(newLat, CoordinateType.latitude);
        //                                analyzedObj.Longitude = DecimalToDegreesLatitudeLongitude.DDtoDMS(newLon, CoordinateType.longitude);
        //                                analyzedObj.Is24Hour = false;
        //                                analyzedObj.FormId = form.SFPM_Form_Id;
        //                                _DbContext.AnalyzedWeather.Add(analyzedObj);
        //                                Save();

        //                                currentPositionLat = newLat;
        //                                currentPositionLong = newLon;
        //                            }

        //                            //hrcount++;
        //                            //currenthrcout++;

        //                            hrcount = hrcount + 3;
        //                            currenthrcout = currenthrcout + 3;
        //                            distancecounter = 0;

        //                            //exit  calculation 
        //                            if (fromUtcReportdatetime.AddHours(currenthrcout) > toUtcReportdatetime)
        //                            {
        //                                f = 1;
        //                                break;
        //                            }
        //                        }
        //                    }
        //                    goto Loop;
        //                }
        //                else
        //                {
        //                    //exit  calculation 
        //                    //if (fromUtcReportdatetime.AddHours(hrcount+1) > toUtcReportdatetime)
        //                    if (fromUtcReportdatetime.AddHours(hrcount + 3) > toUtcReportdatetime)
        //                    {
        //                        var loopcondition = hrcount - currenthrcout;
        //                        for (int i = 1; i <= loopcondition; i++)
        //                        {
        //                            if (distancecounter >= Convert.ToInt32(AzureVaultKey.GetVaultValue("Analyzedweathercalculationhour")))
        //                            {
        //                                hrcount = currenthrcout;
        //                                if (fromUtcReportdatetime.AddHours(hrcount) < toUtcReportdatetime)
        //                                {
        //                                    //call distance api
        //                                    XmlNodeList pointList;
        //                                    if (Convert.ToDouble(CalculatedLatcurrentPosition) == 0 && Convert.ToDouble(CalculatedLongcurrentPosition) == 0)
        //                                    {
        //                                        pointList = GetPositionFromDistance(ParseCoordinate(previousForm.Latitude), ParseCoordinate(previousForm.Longitude), ParseCoordinate(form.Latitude), ParseCoordinate(form.Longitude));
        //                                    }
        //                                    else
        //                                    {
        //                                        pointList = GetPositionFromDistance(Convert.ToDouble(currentPositionLat), Convert.ToDouble(currentPositionLong), ParseCoordinate(form.Latitude), ParseCoordinate(form.Longitude));
        //                                    }
        //                                    var counter = 0;
        //                                    for (int j = 0; j <= pointList[0].ChildNodes.Count - 1; j++)
        //                                    {
        //                                        var pointlat1 = pointList[j].ChildNodes.Item(1).ChildNodes.Item(0).InnerText;
        //                                        var pointlong1 = pointList[j].ChildNodes.Item(1).ChildNodes.Item(1).InnerText;
        //                                        bearing = 0;
        //                                        if (Convert.ToDouble(CalculatedLatcurrentPosition) == 0 && Convert.ToDouble(CalculatedLongcurrentPosition) == 0)
        //                                        {
        //                                            bearing = GetBearing(ParseCoordinate(previousForm.Latitude), ParseCoordinate(previousForm.Longitude), Convert.ToDouble(pointlat1), Convert.ToDouble(pointlong1));
        //                                        }
        //                                        else
        //                                        {
        //                                            bearing = GetBearing(CalculatedLatcurrentPosition, CalculatedLongcurrentPosition, Convert.ToDouble(pointlat1), Convert.ToDouble(pointlong1));
        //                                        }
        //                                    pointLoop:
        //                                        counter++;
        //                                        //var angular_distance =  avgDistance * 1.60934 / 6371;//#radis 6371 in KM of earth
        //                                        var angular_distance = avgDistance * 3 * 1.60934 / 6371;//#radis 6371 in KM of earth
        //                                        double lat = 0;
        //                                        double lon = 0;
        //                                        bearing = ConvertDegreesToRadians(bearing);
        //                                        if (Convert.ToDouble(CalculatedLatcurrentPosition) == 0 && Convert.ToDouble(CalculatedLongcurrentPosition) == 0)
        //                                        {
        //                                            lat = ConvertDegreesToRadians((ParseCoordinate(previousForm.Latitude)));
        //                                            lon = ConvertDegreesToRadians(ParseCoordinate(previousForm.Longitude));

        //                                        }
        //                                        else
        //                                        {
        //                                            lat = ConvertDegreesToRadians(Convert.ToDouble(currentPositionLat));
        //                                            lon = ConvertDegreesToRadians(Convert.ToDouble(currentPositionLong));

        //                                        }
        //                                        var newLat = Math.Asin(Math.Sin(lat) * Math.Cos(angular_distance) + Math.Cos(lat) * Math.Sin(angular_distance) * Math.Cos(bearing));
        //                                        var newLon = lon + Math.Atan2(Math.Sin(bearing) * Math.Sin(angular_distance) * Math.Cos(lat), Math.Cos(angular_distance) - Math.Sin(lat) * Math.Sin(newLat));

        //                                        newLat = ConvertRadiansToDegrees(newLat);
        //                                        newLon = ConvertRadiansToDegrees(newLon);
        //                                        bearing = ConvertRadiansToDegrees(bearing);
        //                                        //itemList.Add(new items() { lat = newLat, log = newLon, reportdatetime = fromUtcReportdatetime.AddHours(currenthrcout + 1) });
        //                                        itemList.Add(new items() { lat = newLat, log = newLon, reportdatetime = fromUtcReportdatetime.AddHours(currenthrcout + 3) });
        //                                        currentPositionLat = newLat;
        //                                        currentPositionLong = newLon;

        //                                        //hrcount++;
        //                                        //currenthrcout++;

        //                                        hrcount = hrcount + 3;
        //                                        currenthrcout = currenthrcout + 3;
        //                                        distancecounter = 0;
        //                                        //exit  calculation 
        //                                        var totaldistance = CalculateDistance(ParseCoordinate(previousForm.Latitude), ParseCoordinate(previousForm.Longitude), ParseCoordinate(form.Latitude), ParseCoordinate(form.Longitude));// Convert.ToDouble(pointlat1), Convert.ToDouble(pointlong1));
        //                                        var totaldistancerevals = avgDistance * currenthrcout;
        //                                        if (fromUtcReportdatetime.AddHours(currenthrcout) < toUtcReportdatetime)
        //                                        {
        //                                            if (counter >= loopcondition)
        //                                            {
        //                                                goto Loop;
        //                                            }
        //                                            if (totaldistance > totaldistancerevals)
        //                                            {
        //                                                goto pointLoop;
        //                                            }

        //                                        }
        //                                        else
        //                                        {
        //                                            f = 1;
        //                                            break;
        //                                        }
        //                                    }
        //                                    if (f != 1)
        //                                    {
        //                                        hrcount++;
        //                                        distancecounter++;
        //                                        goto Loop;
        //                                    }
        //                                }
        //                            }
        //                            else
        //                            {
        //                                if (fromUtcReportdatetime.AddHours(hrcount) < toUtcReportdatetime)
        //                                {
        //                                    hrcount = currenthrcout;
        //                                    var meteoLat = ConvertDegreesToRadians(ParseCoordinate(form.Latitude));
        //                                    var meteoLong = ConvertDegreesToRadians(ParseCoordinate(form.Longitude));
        //                                    bearing = 0;
        //                                    if (CalculatedLatcurrentPosition == 0 && CalculatedLongcurrentPosition == 0)
        //                                    {
        //                                        bearing = GetBearing(ParseCoordinate(previousForm.Latitude), ParseCoordinate(previousForm.Longitude), ParseCoordinate(form.Latitude), ParseCoordinate(form.Longitude));
        //                                    }
        //                                    else
        //                                    {
        //                                        bearing = GetBearing(CalculatedLatcurrentPosition, CalculatedLongcurrentPosition, ParseCoordinate(form.Latitude), ParseCoordinate(form.Longitude));
        //                                    }
        //                                    bearing = ConvertDegreesToRadians(bearing);
        //                                    //var angular_distance = avgDistance * 1.60934 / 6371;//#radis 6371 in KM of earth
        //                                    var angular_distance = avgDistance * 3 * 1.60934 / 6371;//#radis 6371 in KM of earth

        //                                    double lat = 0; double lon = 0;
        //                                    if (CalculatedLatcurrentPosition == 0 && CalculatedLongcurrentPosition == 0)
        //                                    {
        //                                        lat = ConvertDegreesToRadians((ParseCoordinate(previousForm.Latitude)));
        //                                        lon = ConvertDegreesToRadians(ParseCoordinate(previousForm.Longitude));

        //                                    }
        //                                    else
        //                                    {
        //                                        lat = ConvertDegreesToRadians(currentPositionLat);
        //                                        lon = ConvertDegreesToRadians(currentPositionLong);

        //                                    }
        //                                    var newLat = Math.Asin(Math.Sin(lat) * Math.Cos(angular_distance) + Math.Cos(lat) * Math.Sin(angular_distance) * Math.Cos(bearing));
        //                                    var newLon = lon + Math.Atan2(Math.Sin(bearing) * Math.Sin(angular_distance) * Math.Cos(lat), Math.Cos(angular_distance) - Math.Sin(lat) * Math.Sin(newLat));
        //                                    newLat = ConvertRadiansToDegrees(newLat);
        //                                    newLon = ConvertRadiansToDegrees(newLon);
        //                                    bearing = ConvertRadiansToDegrees(bearing);
        //                                    //degree canvert demain
        //                                    //itemList.Add(new items() { lat = newLat, log = newLon, reportdatetime = fromUtcReportdatetime.AddHours(currenthrcout + 1) });
        //                                    itemList.Add(new items() { lat = newLat, log = newLon, reportdatetime = fromUtcReportdatetime.AddHours(currenthrcout + 3) });
        //                                    currentPositionLat = newLat;
        //                                    currentPositionLong = newLon;
        //                                    CalculatedLatcurrentPosition = newLat;
        //                                    CalculatedLongcurrentPosition = newLon;
        //                                    //hrcount++;
        //                                    //currenthrcout++;
        //                                    hrcount = hrcount + 3;
        //                                    currenthrcout = currenthrcout + 3;
        //                                    distancecounter = 0;

        //                                    //exit  calculation 
        //                                    if (fromUtcReportdatetime.AddHours(currenthrcout) > toUtcReportdatetime)
        //                                    {
        //                                        f = 1;
        //                                        break;
        //                                    }
        //                                }
        //                            }
        //                        }
        //                    }
        //                    else
        //                    {
        //                        //hrcount++;
        //                        //distancecounter++;
        //                        hrcount = hrcount + 3;
        //                        distancecounter = distancecounter + 3;
        //                        goto Loop;
        //                    }
        //                }
        //            }
        //            pointArrayList.items = itemList;
        //            MeteoApiCallToGetToken(pointArrayList, form.SFPM_Form_Id);
        //        }
        //    }
        //}
        #endregion

        #region
        public void CreateAnalyzedWeatherBasedOnMeteoStratumPosition(Forms form)
        {
            double currentPositionLat = 0;
            double currentPositionLong = 0;
            double CalculatedLatcurrentPosition = 0;
            double CalculatedLongcurrentPosition = 0;
            DateTime calculatedUTCDatetime = DateTime.Now;
            double angular_distance = 0;

            var hrcount = 0; var currenthrcout = 0; var distancecounter = 0;
            if (((form.FormIdentifier.ToLower().Contains("noon")) && (form.Location.ToLower() == "n")) || form.FormIdentifier.ToLower().Contains("arrival"))
            {
                _DbContext.AnalyzedWeather.RemoveRange(_DbContext.AnalyzedWeather.Where(x => x.FormId == form.SFPM_Form_Id).ToList());
                Save();
                var hrs = Convert.ToDouble(form.ReportTime.Substring(1, 2));
                var min = Convert.ToDouble(form.ReportTime.Substring(4, 2));
                DateTime toUtcReportdatetime = form.ReportTime.Contains("-") ? form.ReportDateTime.Value.DateTime.AddHours(hrs).AddMinutes(min) :
                    form.ReportDateTime.Value.DateTime.AddHours(-hrs).AddMinutes(-min);

                var previousForm = _DbContext.Forms.Where(x => x.ImoNumber == form.ImoNumber && x.ReportDateTime < form.ReportDateTime
                && (((form.FormIdentifier.ToLower().Contains("noon")) && (form.Location.ToLower() == "n")) || form.FormIdentifier.ToLower().Contains("arrival"))).OrderByDescending(x => x.ReportDateTime).FirstOrDefault();
                if (previousForm != null)
                {
                    PointList pointArrayList = new PointList();
                    pointArrayList.items = new List<items>();
                    List<items> itemList = new List<items>();
                    var hrs1 = Convert.ToDouble(previousForm.ReportTime.Substring(1, 2));
                    var min1 = Convert.ToDouble(previousForm.ReportTime.Substring(4, 2));
                    DateTime fromUtcReportdatetime = previousForm.ReportTime.Contains("-") ? previousForm.ReportDateTime.Value.DateTime.AddHours(hrs1).AddMinutes(min1) :
                                   previousForm.ReportDateTime.Value.DateTime.AddHours(-hrs1).AddMinutes(-min1);
                    var totalhour = form.ReportDateTime.Value.DateTime.Subtract(previousForm.ReportDateTime.Value.DateTime).TotalHours;

                    calculatedUTCDatetime = fromUtcReportdatetime;

                    if (totalhour <= 1)
                        return;

                    var avgDistance = Convert.ToDouble(form.ObservedDistance) / totalhour;
                    var meteostratumCompleteList = _DbContext.MeteoStratumData.Where(x => x.IMONumber == form.ImoNumber.ToString() && x.GPSTimeStamp >= fromUtcReportdatetime && x.GPSTimeStamp <= toUtcReportdatetime).OrderBy(x => x.GPSTimeStamp).ToList();
                    //angular_distance = avgDistance * 1.852 / 6371;//#radis 6371 in KM of earth
                    var f = 0;
                Loop:
                    if (f != 1)
                    {
                        var fromReportdatetime = fromUtcReportdatetime.AddHours(hrcount).AddMinutes(5);
                        var toReportdatetime = fromUtcReportdatetime.AddHours(hrcount + 1);
                        // actual logic  increase 1 hr
                        //var meteostratumList = _DbContext.MeteoStratumData.Where(x => x.GPSTimeStamp >= fromReportdatetime && x.GPSTimeStamp <= toReportdatetime && x.IMONumber == form.ImoNumber.ToString()).OrderByDescending(x => x.GPSTimeStamp).ToList();
                        var meteostratumList = meteostratumCompleteList.Where(x => x.GPSTimeStamp >= fromReportdatetime && x.GPSTimeStamp <= toReportdatetime).OrderBy(x => x.GPSTimeStamp).ToList();
                        double bearing = 0;
                        if (meteostratumList.Count == 1)
                        {
                            var loopcondition = 1 + hrcount - currenthrcout;
                            for (int i = 0; i < loopcondition; i++)
                            {
                                if (distancecounter >= Convert.ToInt32(AzureVaultKey.GetVaultValue("Analyzedweathercalculationhour")))
                                {
                                    hrcount = currenthrcout;
                                    if (fromUtcReportdatetime.AddHours(hrcount) < toUtcReportdatetime)
                                    {
                                        //call distance api
                                        XmlNodeList pointList;
                                        if (Convert.ToDouble(CalculatedLatcurrentPosition) == 0 && Convert.ToDouble(CalculatedLongcurrentPosition) == 0)
                                        {
                                            pointList = GetPositionFromDistance(ParseCoordinate(previousForm.Latitude), ParseCoordinate(previousForm.Longitude), ParseCoordinate(meteostratumList.Last().Latitude), ParseCoordinate(meteostratumList.Last().Longitude), form.SFPM_Form_Id);
                                        }
                                        else
                                        {
                                            pointList = GetPositionFromDistance(Convert.ToDouble(currentPositionLat), Convert.ToDouble(currentPositionLong), ParseCoordinate(meteostratumList.Last().Latitude), ParseCoordinate(meteostratumList.Last().Longitude), form.SFPM_Form_Id);
                                        }
                                        var counter = 0;

                                        if (pointList == null || pointList.Count == 0 || pointList[0].ChildNodes.Count - 1 == 0)
                                        {
                                            //ERROR: WRONG LAT LONG PASSED TO DISTANCE API.
                                            f = 1;
                                            break;

                                        }
                                        for (int j = 0; j < pointList[0].ChildNodes.Count - 1; j++)
                                        {
                                            var pointlat1 = pointList[0].ChildNodes.Item(j + 1).ChildNodes.Item(0).InnerText;
                                            var pointlong1 = pointList[0].ChildNodes.Item(j + 1).ChildNodes.Item(1).InnerText;
                                            bearing = 0;
                                            double totaldistance = 0;
                                            double distanceBetweenDistanceAPIPoint = 0;
                                            double totalHour1 = 0;
                                            bool distanceApiInBetweenPointPlotting = false;
                                            counter = 0;
                                            if (Convert.ToDouble(CalculatedLatcurrentPosition) == 0 && Convert.ToDouble(CalculatedLongcurrentPosition) == 0)
                                            {
                                                bearing = GetBearing(ParseCoordinate(previousForm.Latitude), ParseCoordinate(previousForm.Longitude), Convert.ToDouble(pointlat1), Convert.ToDouble(pointlong1));

                                                if (pointList[0].ChildNodes.Count > 2)
                                                {
                                                    for (int k = j; k < pointList[0].ChildNodes.Count - 1; k++)
                                                    {
                                                        distanceBetweenDistanceAPIPoint = distanceBetweenDistanceAPIPoint + CalculateAngularDistance(Convert.ToDouble(pointList[0].ChildNodes.Item(k).ChildNodes.Item(0).InnerText), Convert.ToDouble(pointList[0].ChildNodes.Item(k).ChildNodes.Item(1).InnerText), Convert.ToDouble(pointList[0].ChildNodes.Item(k + 1).ChildNodes.Item(0).InnerText), Convert.ToDouble(pointList[0].ChildNodes.Item(k + 1).ChildNodes.Item(1).InnerText));
                                                    }
                                                    totalHour1 = (meteostratumList.Last().GPSTimeStamp.Value - calculatedUTCDatetime).TotalHours;
                                                    angular_distance = (distanceBetweenDistanceAPIPoint / totalHour1) / 6371000;

                                                }
                                                else if (pointList[0].ChildNodes.Count == 2)
                                                {
                                                    distanceBetweenDistanceAPIPoint = distanceBetweenDistanceAPIPoint + CalculateAngularDistance(Convert.ToDouble(pointList[0].ChildNodes.Item(0).ChildNodes.Item(0).InnerText), Convert.ToDouble(pointList[0].ChildNodes.Item(0).ChildNodes.Item(1).InnerText), Convert.ToDouble(pointList[0].ChildNodes.Item(1).ChildNodes.Item(0).InnerText), Convert.ToDouble(pointList[0].ChildNodes.Item(1).ChildNodes.Item(1).InnerText));
                                                    totalHour1 = (meteostratumList.Last().GPSTimeStamp.Value - calculatedUTCDatetime).TotalHours;
                                                    angular_distance = (distanceBetweenDistanceAPIPoint / totalHour1) / 6371000;
                                                }
                                                else
                                                {
                                                    angular_distance = CalculateVesselSpeed(ParseCoordinate(previousForm.Latitude), ParseCoordinate(previousForm.Longitude), ParseCoordinate(form.Latitude), ParseCoordinate(form.Longitude), fromUtcReportdatetime, toUtcReportdatetime);
                                                }

                                                totaldistance = CalculateDistance(ParseCoordinate(previousForm.Latitude), ParseCoordinate(previousForm.Longitude), Convert.ToDouble(pointlat1), Convert.ToDouble(pointlong1));
                                            }
                                            else
                                            {
                                                bearing = GetBearing(CalculatedLatcurrentPosition, CalculatedLongcurrentPosition, Convert.ToDouble(pointlat1), Convert.ToDouble(pointlong1));

                                                if (pointList[0].ChildNodes.Count > 2)
                                                {
                                                    for (int k = j; k < pointList[0].ChildNodes.Count - 1; k++)
                                                    {
                                                        distanceBetweenDistanceAPIPoint = distanceBetweenDistanceAPIPoint + CalculateAngularDistance(Convert.ToDouble(pointList[0].ChildNodes.Item(k).ChildNodes.Item(0).InnerText), Convert.ToDouble(pointList[0].ChildNodes.Item(k).ChildNodes.Item(1).InnerText), Convert.ToDouble(pointList[0].ChildNodes.Item(k + 1).ChildNodes.Item(0).InnerText), Convert.ToDouble(pointList[0].ChildNodes.Item(k + 1).ChildNodes.Item(1).InnerText));
                                                    }
                                                    totalHour1 = (meteostratumList.Last().GPSTimeStamp.Value - calculatedUTCDatetime).TotalHours;
                                                    angular_distance = (distanceBetweenDistanceAPIPoint / totalHour1) / 6371000;

                                                }
                                                else if (pointList[0].ChildNodes.Count == 2)
                                                {
                                                    distanceBetweenDistanceAPIPoint = distanceBetweenDistanceAPIPoint + CalculateAngularDistance(Convert.ToDouble(pointList[0].ChildNodes.Item(0).ChildNodes.Item(0).InnerText), Convert.ToDouble(pointList[0].ChildNodes.Item(0).ChildNodes.Item(1).InnerText), Convert.ToDouble(pointList[0].ChildNodes.Item(1).ChildNodes.Item(0).InnerText), Convert.ToDouble(pointList[0].ChildNodes.Item(1).ChildNodes.Item(1).InnerText));
                                                    totalHour1 = (meteostratumList.Last().GPSTimeStamp.Value - calculatedUTCDatetime).TotalHours;
                                                    angular_distance = (distanceBetweenDistanceAPIPoint / totalHour1) / 6371000;
                                                }
                                                else
                                                {
                                                    angular_distance = CalculateVesselSpeed(CalculatedLatcurrentPosition, CalculatedLongcurrentPosition, ParseCoordinate(form.Latitude), ParseCoordinate(form.Longitude), calculatedUTCDatetime, toUtcReportdatetime);
                                                }

                                                totaldistance = CalculateDistance(CalculatedLatcurrentPosition, CalculatedLongcurrentPosition, Convert.ToDouble(pointlat1), Convert.ToDouble(pointlong1));
                                            }
                                                                                        
                                            double totaldistancerevals = 0;

                                        pointLoop:
                                            counter++;
                                            //var angular_distance = avgDistance * 1.852 / 6371;//#radis 6371 in KM of earth
                                            double lat = 0;
                                            double lon = 0;
                                            bearing = ConvertDegreesToRadians(bearing);
                                            if (Convert.ToDouble(currentPositionLat) == 0 && Convert.ToDouble(currentPositionLong) == 0)
                                            {
                                                lat = ConvertDegreesToRadians((ParseCoordinate(previousForm.Latitude)));
                                                lon = ConvertDegreesToRadians(ParseCoordinate(previousForm.Longitude));
                                            }
                                            else
                                            {
                                                lat = ConvertDegreesToRadians(Convert.ToDouble(currentPositionLat));
                                                lon = ConvertDegreesToRadians(Convert.ToDouble(currentPositionLong));
                                            }

                                            if (distanceApiInBetweenPointPlotting)
                                            {
                                                angular_distance = CalculateVesselSpeed(CalculatedLatcurrentPosition, CalculatedLongcurrentPosition, Convert.ToDouble(pointlat1), Convert.ToDouble(pointlong1), calculatedUTCDatetime, calculatedUTCDatetime.AddHours(1));
                                            }

                                            var newLat = Math.Asin(Math.Sin(lat) * Math.Cos(angular_distance) + Math.Cos(lat) * Math.Sin(angular_distance) * Math.Cos(bearing));
                                            var newLon = lon + Math.Atan2(Math.Sin(bearing) * Math.Sin(angular_distance) * Math.Cos(lat), Math.Cos(angular_distance) - Math.Sin(lat) * Math.Sin(newLat));

                                            avgDistance = (distanceBetweenDistanceAPIPoint / 1000) / totalHour1;
                                            totaldistancerevals = avgDistance * counter;

                                            newLat = ConvertRadiansToDegrees(newLat);
                                            newLon = ConvertRadiansToDegrees(newLon);
                                            bearing = ConvertRadiansToDegrees(bearing);
                                            var analyzedObj = new AnalyzedWeather();
                                            analyzedObj.AnalyzedCurrent = "0";//
                                            analyzedObj.AnalyzedCurrentDirection = "0"; //
                                            analyzedObj.AnalyzedSeaCurrentSpeedInKnots = 0;
                                            analyzedObj.AnalyzedSeaHeight = "0";
                                            analyzedObj.AnalyzedSwellDirection = "0";
                                            analyzedObj.AnalyzedSwellHeight = "0";
                                            analyzedObj.AnalyzedWave = "0"; //
                                            analyzedObj.AnalyzedWaveDiection = "0";
                                            analyzedObj.AnalyzedWind = "0"; //
                                            analyzedObj.AnalyzedWindDirection = "0";
                                            analyzedObj.Bearing = Convert.ToDecimal(bearing);
                                            analyzedObj.CalculatedTimeStamp = fromUtcReportdatetime.AddHours(currenthrcout + 1);
                                            analyzedObj.CreatedBy = Convert.ToInt64(_currentUser.UserId);
                                            analyzedObj.CreatedDateTime = DateTime.UtcNow;
                                            analyzedObj.Latitude = DecimalToDegreesLatitudeLongitude.DDtoDMS(newLat, CoordinateType.latitude);
                                            analyzedObj.Longitude = DecimalToDegreesLatitudeLongitude.DDtoDMS(newLon, CoordinateType.longitude);
                                            analyzedObj.Is24Hour = false;
                                            analyzedObj.FormId = form.SFPM_Form_Id;
                                            _DbContext.AnalyzedWeather.Add(analyzedObj);
                                            //Save();

                                            itemList.Add(new items() { lat = newLat, log = newLon, reportdatetime = fromUtcReportdatetime.AddHours(currenthrcout + 1) });
                                            currentPositionLat = newLat;
                                            currentPositionLong = newLon;
                                            CalculatedLatcurrentPosition = newLat;
                                            CalculatedLongcurrentPosition = newLon;
                                            calculatedUTCDatetime = fromUtcReportdatetime.AddHours(currenthrcout + 1);
                                            if (counter > 1)
                                            {
                                                i++;
                                            }

                                            hrcount++;
                                            currenthrcout++;
                                            distancecounter = 0;
                                            //exit  calculation 
                                            if (fromUtcReportdatetime.AddHours(currenthrcout + 1) < toUtcReportdatetime)
                                            {
                                                if (counter >= loopcondition)
                                                {
                                                    goto Loop;
                                                }
                                                else if (totaldistance >= totaldistancerevals + avgDistance)
                                                {
                                                    goto pointLoop;
                                                }
                                                else if (totaldistance - totaldistancerevals < avgDistance && totaldistance - totaldistancerevals > (avgDistance / 2))
                                                {
                                                    distanceApiInBetweenPointPlotting = true;
                                                    goto pointLoop;
                                                }
                                                else if (counter == loopcondition - 1)
                                                {
                                                    goto Loop;
                                                }
                                                else
                                                {
                                                    f = 1;
                                                    //break;
                                                }

                                            }
                                            else
                                            {
                                                f = 1;
                                                break;
                                            }
                                        }
                                        if (f != 1)
                                        {
                                            hrcount++;
                                            distancecounter++;
                                            goto Loop;
                                        }
                                    }
                                }
                                else
                                {
                                    hrcount = currenthrcout;
                                    var meteostratum = meteostratumList.LastOrDefault();
                                    var meteoLat = ConvertDegreesToRadians(ParseCoordinate(meteostratum.Latitude));
                                    var meteoLong = ConvertDegreesToRadians(ParseCoordinate(meteostratum.Longitude));
                                    bearing = 0;

                                    if (CalculatedLatcurrentPosition == 0 && CalculatedLongcurrentPosition == 0)
                                    {
                                        bearing = GetBearing(ParseCoordinate(previousForm.Latitude), ParseCoordinate(previousForm.Longitude), ParseCoordinate(meteostratum.Latitude), ParseCoordinate(meteostratum.Longitude));
                                        angular_distance = CalculateVesselSpeed(ParseCoordinate(previousForm.Latitude), ParseCoordinate(previousForm.Longitude), ParseCoordinate(meteostratum.Latitude), ParseCoordinate(meteostratum.Longitude), fromUtcReportdatetime, (meteostratum.GPSTimeStamp ?? DateTime.Now));
                                    }
                                    else
                                    {
                                        bearing = GetBearing(CalculatedLatcurrentPosition, CalculatedLongcurrentPosition, ParseCoordinate(meteostratum.Latitude), ParseCoordinate(meteostratum.Longitude));
                                        angular_distance = CalculateVesselSpeed(CalculatedLatcurrentPosition, CalculatedLongcurrentPosition, ParseCoordinate(meteostratum.Latitude), ParseCoordinate(meteostratum.Longitude), calculatedUTCDatetime, (meteostratum.GPSTimeStamp ?? DateTime.Now));
                                    }
                                    bearing = ConvertDegreesToRadians(bearing);
                                    //var angular_distance = avgDistance * 1.852 / 6371;//#radis 6371 in KM of earth                                    
                                    double lat = 0; double lon = 0;
                                    if (Convert.ToDouble(currentPositionLat) == 0 && Convert.ToDouble(currentPositionLong) == 0)
                                    {
                                        lat = ConvertDegreesToRadians((ParseCoordinate(previousForm.Latitude)));
                                        lon = ConvertDegreesToRadians(ParseCoordinate(previousForm.Longitude));
                                    }
                                    else
                                    {
                                        lat = ConvertDegreesToRadians(Convert.ToDouble(currentPositionLat));
                                        lon = ConvertDegreesToRadians(Convert.ToDouble(currentPositionLong));
                                    }
                                    var newLat = Math.Asin(Math.Sin(lat) * Math.Cos(angular_distance) + Math.Cos(lat) * Math.Sin(angular_distance) * Math.Cos(bearing)); // add current position lat long not for meteostratum
                                    var newLon = lon + Math.Atan2(Math.Sin(bearing) * Math.Sin(angular_distance) * Math.Cos(lat), Math.Cos(angular_distance) - Math.Sin(lat) * Math.Sin(newLat));

                                    newLat = ConvertRadiansToDegrees(newLat);
                                    newLon = ConvertRadiansToDegrees(newLon);
                                    bearing = ConvertRadiansToDegrees(bearing);

                                    if (i + 1 == loopcondition)
                                    {
                                        var analyzedObj = new AnalyzedWeather();
                                        var differenceSeaCurrentBearing = ConvertDegreesToRadians(Convert.ToDouble(meteostratum.SeaCurrentDirectionInDegrees - Convert.ToDecimal(meteostratum.Cog)));
                                        analyzedObj.AnalyzedCurrent = String.Format("{0:0.00}", Convert.ToDouble(Convert.ToDouble(meteostratum.SeaCurrentSpeedInKnots) * Math.Cos(differenceSeaCurrentBearing)));
                                        //analyzedObj.AnalyzedCurrent = (meteostratum.SeaCurrentSpeedInKnots * Math.Cos(Math.Abs(differenceSeaCurrentBearing))).ToString();
                                        analyzedObj.AnalyzedCurrentDirection = meteostratum.SeaCurrentDirectionInDegrees.ToString(); //
                                        analyzedObj.AnalyzedSeaCurrentSpeedInKnots = meteostratum.SeaCurrentSpeedInKnots;
                                        analyzedObj.AnalyzedSeaHeight = meteostratum.SeaHeightInMeters.ToString();
                                        analyzedObj.AnalyzedSwellDirection = meteostratum.SwellDirectionInDegrees.ToString();
                                        analyzedObj.AnalyzedSwellHeight = meteostratum.SwellHeightInMeters.ToString();
                                        analyzedObj.AnalyzedWave = meteostratum.TotalWaveHeightInMeters.ToString(); //
                                        analyzedObj.AnalyzedWaveDiection = meteostratum.TotalWaveDirectionInDegrees.ToString();
                                        analyzedObj.AnalyzedWind = meteostratum.WindSpeedInKnots.ToString(); //
                                        //analyzedObj.AnalyzedWind = meteostratum.WindGustInKnots.ToString(); //
                                        analyzedObj.AnalyzedWindDirection = meteostratum.WindDirectionInDegrees.ToString();
                                        analyzedObj.Bearing = Convert.ToDecimal(bearing);
                                        analyzedObj.CalculatedTimeStamp = fromUtcReportdatetime.AddHours(currenthrcout + 1);
                                        analyzedObj.CreatedBy = Convert.ToInt64(_currentUser.UserId);
                                        analyzedObj.CreatedDateTime = DateTime.UtcNow;
                                        analyzedObj.Latitude = DecimalToDegreesLatitudeLongitude.DDtoDMS(newLat, CoordinateType.latitude);
                                        analyzedObj.Longitude = DecimalToDegreesLatitudeLongitude.DDtoDMS(newLon, CoordinateType.longitude);
                                        analyzedObj.Is24Hour = false;
                                        analyzedObj.FormId = form.SFPM_Form_Id;
                                        _DbContext.AnalyzedWeather.Add(analyzedObj);
                                        //Save();

                                        currentPositionLat = newLat;
                                        currentPositionLong = newLon;

                                        CalculatedLatcurrentPosition = newLat;
                                        CalculatedLongcurrentPosition = newLon;
                                        calculatedUTCDatetime = fromUtcReportdatetime.AddHours(currenthrcout + 1);
                                    }
                                    else
                                    {
                                        //CALL METEOSTRATUM API AND GET ANALYZED WEATHER.

                                        var analyzedObj = new AnalyzedWeather();
                                        analyzedObj.AnalyzedCurrent = "0";//
                                        analyzedObj.AnalyzedCurrentDirection = "0"; //
                                        analyzedObj.AnalyzedSeaCurrentSpeedInKnots = 0;
                                        analyzedObj.AnalyzedSeaHeight = "0";
                                        analyzedObj.AnalyzedSwellDirection = "0";
                                        analyzedObj.AnalyzedSwellHeight = "0";
                                        analyzedObj.AnalyzedWave = "0"; //
                                        analyzedObj.AnalyzedWaveDiection = "0";
                                        analyzedObj.AnalyzedWind = "0"; //
                                        analyzedObj.AnalyzedWindDirection = "0";
                                        analyzedObj.Bearing = Convert.ToDecimal(bearing);
                                        analyzedObj.CalculatedTimeStamp = fromUtcReportdatetime.AddHours(currenthrcout + 1);
                                        analyzedObj.CreatedBy = Convert.ToInt64(_currentUser.UserId);
                                        analyzedObj.CreatedDateTime = DateTime.UtcNow;
                                        analyzedObj.Latitude = DecimalToDegreesLatitudeLongitude.DDtoDMS(newLat, CoordinateType.latitude);
                                        analyzedObj.Longitude = DecimalToDegreesLatitudeLongitude.DDtoDMS(newLon, CoordinateType.longitude);
                                        analyzedObj.Is24Hour = false;
                                        analyzedObj.FormId = form.SFPM_Form_Id;
                                        _DbContext.AnalyzedWeather.Add(analyzedObj);
                                        //Save();

                                        currentPositionLat = newLat;
                                        currentPositionLong = newLon;

                                        CalculatedLatcurrentPosition = newLat;
                                        CalculatedLongcurrentPosition = newLon;

                                        calculatedUTCDatetime = fromUtcReportdatetime.AddHours(currenthrcout + 1);

                                        itemList.Add(new items() { lat = newLat, log = newLon, reportdatetime = fromUtcReportdatetime.AddHours(currenthrcout + 1) });
                                    }

                                    hrcount++;
                                    currenthrcout++;
                                    distancecounter = 0;

                                    //exit  calculation 
                                    if (fromUtcReportdatetime.AddHours(currenthrcout + 1) >= toUtcReportdatetime)
                                    {
                                        f = 1;
                                        break;
                                    }
                                }
                            }
                            goto Loop;
                        }
                        else if (meteostratumList.Count > 1)
                        {
                            var loopcondition = 1 + hrcount - currenthrcout;
                            for (int i = 0; i < loopcondition; i++)
                            {
                                if (distancecounter >= Convert.ToInt32(AzureVaultKey.GetVaultValue("Analyzedweathercalculationhour")))
                                {
                                    hrcount = currenthrcout;
                                    if (fromUtcReportdatetime.AddHours(hrcount) < toUtcReportdatetime)
                                    {
                                        //call distance api
                                        XmlNodeList pointList;
                                        if (Convert.ToDouble(CalculatedLatcurrentPosition) == 0 && Convert.ToDouble(CalculatedLongcurrentPosition) == 0)
                                        {
                                            pointList = GetPositionFromDistance(ParseCoordinate(previousForm.Latitude), ParseCoordinate(previousForm.Longitude), ParseCoordinate(meteostratumList.Last().Latitude), ParseCoordinate(meteostratumList.Last().Longitude), form.SFPM_Form_Id);

                                        }
                                        else
                                        {                                            
                                            pointList = GetPositionFromDistance(Convert.ToDouble(currentPositionLat), Convert.ToDouble(currentPositionLong), ParseCoordinate(meteostratumList.Last().Latitude), ParseCoordinate(meteostratumList.Last().Longitude), form.SFPM_Form_Id);
                                        }
                                        var counter = 0;
                                        
                                        if (pointList == null || pointList.Count == 0 || pointList[0].ChildNodes.Count - 1 == 0)
                                        {
                                            //ERROR: WRONG LAT LONG PASSED TO DISTANCE API.
                                            f = 1;
                                            break;

                                        }
                                        for (int j = 0; j < pointList[0].ChildNodes.Count - 1; j++)
                                        {
                                            var pointlat1 = pointList[0].ChildNodes.Item(j + 1).ChildNodes.Item(0).InnerText;
                                            var pointlong1 = pointList[0].ChildNodes.Item(j + 1).ChildNodes.Item(1).InnerText;
                                            bearing = 0;
                                            double totaldistance = 0;
                                            double distanceBetweenDistanceAPIPoint = 0;
                                            double totalHour1 = 0;
                                            bool distanceApiInBetweenPointPlotting = false;
                                            counter = 0;
                                            if (Convert.ToDouble(CalculatedLatcurrentPosition) == 0 && Convert.ToDouble(CalculatedLongcurrentPosition) == 0)
                                            {
                                                bearing = GetBearing(ParseCoordinate(previousForm.Latitude), ParseCoordinate(previousForm.Longitude), Convert.ToDouble(pointlat1), Convert.ToDouble(pointlong1));

                                                if (pointList[0].ChildNodes.Count > 2)
                                                {
                                                    for (int k = j; k < pointList[0].ChildNodes.Count - 1; k++)
                                                    {
                                                        distanceBetweenDistanceAPIPoint = distanceBetweenDistanceAPIPoint + CalculateAngularDistance(Convert.ToDouble(pointList[0].ChildNodes.Item(k).ChildNodes.Item(0).InnerText), Convert.ToDouble(pointList[0].ChildNodes.Item(k).ChildNodes.Item(1).InnerText), Convert.ToDouble(pointList[0].ChildNodes.Item(k + 1).ChildNodes.Item(0).InnerText), Convert.ToDouble(pointList[0].ChildNodes.Item(k + 1).ChildNodes.Item(1).InnerText));
                                                    }
                                                    totalHour1 = (meteostratumList.Last().GPSTimeStamp.Value - calculatedUTCDatetime).TotalHours;
                                                    angular_distance = (distanceBetweenDistanceAPIPoint / totalHour1) / 6371000;

                                                }
                                                else if (pointList[0].ChildNodes.Count == 2)
                                                {
                                                    distanceBetweenDistanceAPIPoint = distanceBetweenDistanceAPIPoint + CalculateAngularDistance(Convert.ToDouble(pointList[0].ChildNodes.Item(0).ChildNodes.Item(0).InnerText), Convert.ToDouble(pointList[0].ChildNodes.Item(0).ChildNodes.Item(1).InnerText), Convert.ToDouble(pointList[0].ChildNodes.Item(1).ChildNodes.Item(0).InnerText), Convert.ToDouble(pointList[0].ChildNodes.Item(1).ChildNodes.Item(1).InnerText));
                                                    totalHour1 = (meteostratumList.Last().GPSTimeStamp.Value - calculatedUTCDatetime).TotalHours;
                                                    angular_distance = (distanceBetweenDistanceAPIPoint / totalHour1) / 6371000;
                                                }
                                                else
                                                {
                                                    angular_distance = CalculateVesselSpeed(ParseCoordinate(previousForm.Latitude), ParseCoordinate(previousForm.Longitude), ParseCoordinate(form.Latitude), ParseCoordinate(form.Longitude), fromUtcReportdatetime, toUtcReportdatetime);
                                                }

                                                totaldistance = CalculateDistance(ParseCoordinate(previousForm.Latitude), ParseCoordinate(previousForm.Longitude), Convert.ToDouble(pointlat1), Convert.ToDouble(pointlong1));
                                            }
                                            else
                                            {
                                                bearing = GetBearing(CalculatedLatcurrentPosition, CalculatedLongcurrentPosition, Convert.ToDouble(pointlat1), Convert.ToDouble(pointlong1));

                                                if (pointList[0].ChildNodes.Count > 2)
                                                {
                                                    for (int k = j; k < pointList[0].ChildNodes.Count - 1; k++)
                                                    {
                                                        distanceBetweenDistanceAPIPoint = distanceBetweenDistanceAPIPoint + CalculateAngularDistance(Convert.ToDouble(pointList[0].ChildNodes.Item(k).ChildNodes.Item(0).InnerText), Convert.ToDouble(pointList[0].ChildNodes.Item(k).ChildNodes.Item(1).InnerText), Convert.ToDouble(pointList[0].ChildNodes.Item(k + 1).ChildNodes.Item(0).InnerText), Convert.ToDouble(pointList[0].ChildNodes.Item(k + 1).ChildNodes.Item(1).InnerText));
                                                    }
                                                    totalHour1 = (meteostratumList.Last().GPSTimeStamp.Value - calculatedUTCDatetime).TotalHours;
                                                    angular_distance = (distanceBetweenDistanceAPIPoint / totalHour1) / 6371000;

                                                }
                                                else if (pointList[0].ChildNodes.Count == 2)
                                                {
                                                    distanceBetweenDistanceAPIPoint = distanceBetweenDistanceAPIPoint + CalculateAngularDistance(Convert.ToDouble(pointList[0].ChildNodes.Item(0).ChildNodes.Item(0).InnerText), Convert.ToDouble(pointList[0].ChildNodes.Item(0).ChildNodes.Item(1).InnerText), Convert.ToDouble(pointList[0].ChildNodes.Item(1).ChildNodes.Item(0).InnerText), Convert.ToDouble(pointList[0].ChildNodes.Item(1).ChildNodes.Item(1).InnerText));
                                                    totalHour1 = (meteostratumList.Last().GPSTimeStamp.Value - calculatedUTCDatetime).TotalHours;
                                                    angular_distance = (distanceBetweenDistanceAPIPoint / totalHour1) / 6371000;
                                                }
                                                else
                                                {
                                                    angular_distance = CalculateVesselSpeed(CalculatedLatcurrentPosition, CalculatedLongcurrentPosition, ParseCoordinate(form.Latitude), ParseCoordinate(form.Longitude), calculatedUTCDatetime, toUtcReportdatetime);
                                                }

                                                totaldistance = CalculateDistance(CalculatedLatcurrentPosition, CalculatedLongcurrentPosition, Convert.ToDouble(pointlat1), Convert.ToDouble(pointlong1));
                                            }

                                            double totaldistancerevals = 0;

                                        pointLoop:
                                            counter++;
                                            //var angular_distance = avgDistance * 1.852 / 6371;//#radis 6371 in KM of earth
                                            double lat = 0;
                                            double lon = 0;
                                            bearing = ConvertDegreesToRadians(bearing);
                                            if (Convert.ToDouble(currentPositionLat) == 0 && Convert.ToDouble(currentPositionLong) == 0)
                                            {
                                                lat = ConvertDegreesToRadians((ParseCoordinate(previousForm.Latitude)));
                                                lon = ConvertDegreesToRadians(ParseCoordinate(previousForm.Longitude));

                                            }
                                            else
                                            {
                                                lat = ConvertDegreesToRadians(Convert.ToDouble(currentPositionLat));
                                                lon = ConvertDegreesToRadians(Convert.ToDouble(currentPositionLong));

                                            }

                                            if (distanceApiInBetweenPointPlotting)
                                            {
                                                angular_distance = CalculateVesselSpeed(CalculatedLatcurrentPosition, CalculatedLongcurrentPosition, Convert.ToDouble(pointlat1), Convert.ToDouble(pointlong1), calculatedUTCDatetime, calculatedUTCDatetime.AddHours(1));
                                            }

                                            var newLat = Math.Asin(Math.Sin(lat) * Math.Cos(angular_distance) + Math.Cos(lat) * Math.Sin(angular_distance) * Math.Cos(bearing));
                                            var newLon = lon + Math.Atan2(Math.Sin(bearing) * Math.Sin(angular_distance) * Math.Cos(lat), Math.Cos(angular_distance) - Math.Sin(lat) * Math.Sin(newLat));

                                            avgDistance = (distanceBetweenDistanceAPIPoint / 1000) / totalHour1;
                                            totaldistancerevals = avgDistance * counter;

                                            newLat = ConvertRadiansToDegrees(newLat);
                                            newLon = ConvertRadiansToDegrees(newLon);
                                            bearing = ConvertRadiansToDegrees(bearing);
                                            var analyzedObj = new AnalyzedWeather();
                                            analyzedObj.AnalyzedCurrent = "0";//
                                            analyzedObj.AnalyzedCurrentDirection = "0"; //
                                            analyzedObj.AnalyzedSeaCurrentSpeedInKnots = 0;
                                            analyzedObj.AnalyzedSeaHeight = "0";
                                            analyzedObj.AnalyzedSwellDirection = "0";
                                            analyzedObj.AnalyzedSwellHeight = "0";
                                            analyzedObj.AnalyzedWave = "0"; //
                                            analyzedObj.AnalyzedWaveDiection = "0";
                                            analyzedObj.AnalyzedWind = "0"; //
                                            analyzedObj.AnalyzedWindDirection = "0";
                                            analyzedObj.Bearing = Convert.ToDecimal(bearing);
                                            analyzedObj.CalculatedTimeStamp = fromUtcReportdatetime.AddHours(currenthrcout + 1);
                                            analyzedObj.CreatedBy = Convert.ToInt64(_currentUser.UserId);
                                            analyzedObj.CreatedDateTime = DateTime.UtcNow;
                                            analyzedObj.Latitude = DecimalToDegreesLatitudeLongitude.DDtoDMS(newLat, CoordinateType.latitude);
                                            analyzedObj.Longitude = DecimalToDegreesLatitudeLongitude.DDtoDMS(newLon, CoordinateType.longitude);
                                            analyzedObj.Is24Hour = false;
                                            analyzedObj.FormId = form.SFPM_Form_Id;
                                            _DbContext.AnalyzedWeather.Add(analyzedObj);
                                            //Save();

                                            itemList.Add(new items() { lat = newLat, log = newLon, reportdatetime = fromUtcReportdatetime.AddHours(currenthrcout + 1) });
                                            currentPositionLat = newLat;
                                            currentPositionLong = newLon;
                                            CalculatedLatcurrentPosition = newLat;
                                            CalculatedLongcurrentPosition = newLon;
                                            calculatedUTCDatetime = fromUtcReportdatetime.AddHours(currenthrcout + 1);

                                            hrcount++;
                                            currenthrcout++;
                                            distancecounter = 0;
                                            if (counter > 1)
                                            {
                                                i++;
                                            }
                                            //exit  calculation 
                                            if (fromUtcReportdatetime.AddHours(currenthrcout + 1) < toUtcReportdatetime)
                                            {
                                                if (counter >= loopcondition)
                                                {
                                                    goto Loop;
                                                }
                                                else if (totaldistance >= totaldistancerevals + avgDistance)
                                                {
                                                    goto pointLoop;
                                                }
                                                else if (totaldistance - totaldistancerevals < avgDistance && totaldistance - totaldistancerevals > (avgDistance / 2))
                                                {
                                                    distanceApiInBetweenPointPlotting = true;
                                                    goto pointLoop;
                                                }
                                                else if (counter == loopcondition - 1)
                                                {
                                                    goto Loop;
                                                }
                                                else
                                                {
                                                    f = 1;
                                                    //break;
                                                }

                                            }
                                            else
                                            {
                                                f = 1;
                                                break;
                                            }
                                        }
                                        if (f != 1)
                                        {
                                            hrcount++;
                                            distancecounter++;
                                            goto Loop;
                                        }
                                    }
                                }
                                else
                                {
                                    hrcount = currenthrcout;
                                    var meteostratum = meteostratumList.LastOrDefault();
                                    var meteoLat = ConvertDegreesToRadians(ParseCoordinate(meteostratum.Latitude));
                                    var meteoLong = ConvertDegreesToRadians(ParseCoordinate(meteostratum.Longitude));
                                    bearing = 0;
                                    if (CalculatedLatcurrentPosition == 0 && CalculatedLongcurrentPosition == 0)
                                    {
                                        bearing = GetBearing(ParseCoordinate(previousForm.Latitude), ParseCoordinate(previousForm.Longitude), ParseCoordinate(meteostratum.Latitude), ParseCoordinate(meteostratum.Longitude));
                                        angular_distance = CalculateVesselSpeed(ParseCoordinate(previousForm.Latitude), ParseCoordinate(previousForm.Longitude), ParseCoordinate(meteostratum.Latitude), ParseCoordinate(meteostratum.Longitude), fromUtcReportdatetime, (meteostratum.GPSTimeStamp ?? DateTime.Now));
                                    }
                                    else
                                    {
                                        bearing = GetBearing(CalculatedLatcurrentPosition, CalculatedLongcurrentPosition, ParseCoordinate(meteostratum.Latitude), ParseCoordinate(meteostratum.Longitude));
                                        angular_distance = CalculateVesselSpeed(CalculatedLatcurrentPosition, CalculatedLongcurrentPosition, ParseCoordinate(meteostratum.Latitude), ParseCoordinate(meteostratum.Longitude), calculatedUTCDatetime, (meteostratum.GPSTimeStamp ?? DateTime.Now));
                                    }
                                    bearing = ConvertDegreesToRadians(bearing);
                                    //var angular_distance = avgDistance * 1.852 / 6371;//#radis 6371 in KM of earth

                                    double lat = 0; double lon = 0;
                                    if (currentPositionLat == 0 && currentPositionLong == 0)
                                    {
                                        lat = ConvertDegreesToRadians((ParseCoordinate(previousForm.Latitude)));
                                        lon = ConvertDegreesToRadians(ParseCoordinate(previousForm.Longitude));
                                    }
                                    else
                                    {
                                        lat = ConvertDegreesToRadians(currentPositionLat);
                                        lon = ConvertDegreesToRadians(currentPositionLong);

                                    }
                                    var newLat = Math.Asin(Math.Sin(lat) * Math.Cos(angular_distance) + Math.Cos(lat) * Math.Sin(angular_distance) * Math.Cos(bearing));
                                    var newLon = lon + Math.Atan2(Math.Sin(bearing) * Math.Sin(angular_distance) * Math.Cos(lat), Math.Cos(angular_distance) - Math.Sin(lat) * Math.Sin(newLat));
                                    newLat = ConvertRadiansToDegrees(newLat);
                                    newLon = ConvertRadiansToDegrees(newLon);
                                    bearing = ConvertRadiansToDegrees(bearing);

                                    if (i + 1 == loopcondition)
                                    {
                                        AnalyzedWeatherCalculation(meteostratumList, form, form.ReportDateTime.Value.DateTime.AddHours(hrcount), newLat.ToString(), newLon.ToString(), fromUtcReportdatetime.AddHours(currenthrcout + 1), bearing);

                                        currentPositionLat = newLat;
                                        currentPositionLong = newLon;

                                        CalculatedLatcurrentPosition = newLat;
                                        CalculatedLongcurrentPosition = newLon;
                                        calculatedUTCDatetime = fromUtcReportdatetime.AddHours(currenthrcout + 1);
                                    }
                                    else
                                    {
                                        //CALL METEOSTRATUM API AND GET ANALYZED WEATHER.

                                        var analyzedObj = new AnalyzedWeather();
                                        analyzedObj.AnalyzedCurrent = "0";//
                                        analyzedObj.AnalyzedCurrentDirection = "0"; //
                                        analyzedObj.AnalyzedSeaCurrentSpeedInKnots = 0;
                                        analyzedObj.AnalyzedSeaHeight = "0";
                                        analyzedObj.AnalyzedSwellDirection = "0";
                                        analyzedObj.AnalyzedSwellHeight = "0";
                                        analyzedObj.AnalyzedWave = "0"; //
                                        analyzedObj.AnalyzedWaveDiection = "0";
                                        analyzedObj.AnalyzedWind = "0"; //
                                        analyzedObj.AnalyzedWindDirection = "0";
                                        analyzedObj.Bearing = Convert.ToDecimal(bearing);
                                        analyzedObj.CalculatedTimeStamp = fromUtcReportdatetime.AddHours(currenthrcout + 1);
                                        analyzedObj.CreatedBy = Convert.ToInt64(_currentUser.UserId);
                                        analyzedObj.CreatedDateTime = DateTime.UtcNow;
                                        analyzedObj.Latitude = DecimalToDegreesLatitudeLongitude.DDtoDMS(newLat, CoordinateType.latitude);
                                        analyzedObj.Longitude = DecimalToDegreesLatitudeLongitude.DDtoDMS(newLon, CoordinateType.longitude);
                                        analyzedObj.Is24Hour = false;
                                        analyzedObj.FormId = form.SFPM_Form_Id;
                                        _DbContext.AnalyzedWeather.Add(analyzedObj);
                                        //Save();

                                        currentPositionLat = newLat;
                                        currentPositionLong = newLon;

                                        CalculatedLatcurrentPosition = newLat;
                                        CalculatedLongcurrentPosition = newLon;

                                        calculatedUTCDatetime = fromUtcReportdatetime.AddHours(currenthrcout + 1);
                                        itemList.Add(new items() { lat = newLat, log = newLon, reportdatetime = fromUtcReportdatetime.AddHours(currenthrcout + 1) });
                                    }

                                    hrcount++;
                                    currenthrcout++;
                                    distancecounter = 0;

                                    //exit  calculation 
                                    if (fromUtcReportdatetime.AddHours(currenthrcout + 1) >= toUtcReportdatetime)
                                    {
                                        f = 1;
                                        break;
                                    }
                                }
                            }
                            goto Loop;
                        }
                        else
                        {
                            //exit  calculation 
                            if (fromUtcReportdatetime.AddHours(hrcount + 1) >= toUtcReportdatetime)
                            {
                                var loopcondition = hrcount - currenthrcout;
                                for (int i = 1; i <= loopcondition; i++)
                                {
                                    if (distancecounter >= Convert.ToInt32(AzureVaultKey.GetVaultValue("Analyzedweathercalculationhour")))
                                    {
                                        hrcount = currenthrcout;
                                        if (fromUtcReportdatetime.AddHours(hrcount) < toUtcReportdatetime)
                                        {
                                            //call distance api
                                            XmlNodeList pointList;
                                            if (Convert.ToDouble(CalculatedLatcurrentPosition) == 0 && Convert.ToDouble(CalculatedLongcurrentPosition) == 0)
                                            {
                                                pointList = GetPositionFromDistance(ParseCoordinate(previousForm.Latitude), ParseCoordinate(previousForm.Longitude), ParseCoordinate(form.Latitude), ParseCoordinate(form.Longitude), form.SFPM_Form_Id);

                                            }
                                            else
                                            {
                                                pointList = GetPositionFromDistance(Convert.ToDouble(currentPositionLat), Convert.ToDouble(currentPositionLong), ParseCoordinate(form.Latitude), ParseCoordinate(form.Longitude), form.SFPM_Form_Id);
                                            }
                                            var counter = 0;
                                            
                                            if (pointList == null || pointList.Count == 0 || pointList[0].ChildNodes.Count - 1 == 0)
                                            {
                                                //ERROR: WRONG LAT LONG PASSED TO DISTANCE API.
                                                f = 1;
                                                break;

                                            }
                                            for (int j = 0; j < pointList[0].ChildNodes.Count - 1; j++)
                                            {
                                                var pointlat1 = pointList[0].ChildNodes.Item(j + 1).ChildNodes.Item(0).InnerText;
                                                var pointlong1 = pointList[0].ChildNodes.Item(j + 1).ChildNodes.Item(1).InnerText;
                                                bearing = 0;
                                                double totaldistance = 0;
                                                double distanceBetweenDistanceAPIPoint = 0;
                                                double totalHour1 = 0;
                                                bool distanceApiInBetweenPointPlotting = false;
                                                counter = 0;
                                                if (Convert.ToDouble(CalculatedLatcurrentPosition) == 0 && Convert.ToDouble(CalculatedLongcurrentPosition) == 0)
                                                {
                                                    bearing = GetBearing(ParseCoordinate(previousForm.Latitude), ParseCoordinate(previousForm.Longitude), Convert.ToDouble(pointlat1), Convert.ToDouble(pointlong1));

                                                    if (pointList[0].ChildNodes.Count > 2)
                                                    {
                                                        for (int k = 0; k < pointList[0].ChildNodes.Count - 1; k++)
                                                        {
                                                            distanceBetweenDistanceAPIPoint = distanceBetweenDistanceAPIPoint + CalculateAngularDistance(Convert.ToDouble(pointList[0].ChildNodes.Item(k).ChildNodes.Item(0).InnerText), Convert.ToDouble(pointList[0].ChildNodes.Item(k).ChildNodes.Item(1).InnerText), Convert.ToDouble(pointList[0].ChildNodes.Item(k + 1).ChildNodes.Item(0).InnerText), Convert.ToDouble(pointList[0].ChildNodes.Item(k + 1).ChildNodes.Item(1).InnerText));
                                                        }
                                                        totalHour1 = (toUtcReportdatetime - fromUtcReportdatetime).TotalHours;
                                                        angular_distance = (distanceBetweenDistanceAPIPoint / totalHour1) / 6371000;

                                                    }
                                                    else
                                                    {
                                                        angular_distance = CalculateVesselSpeed(ParseCoordinate(previousForm.Latitude), ParseCoordinate(previousForm.Longitude), ParseCoordinate(form.Latitude), ParseCoordinate(form.Longitude), fromUtcReportdatetime, toUtcReportdatetime);
                                                    }

                                                    totaldistance = CalculateDistance(ParseCoordinate(previousForm.Latitude), ParseCoordinate(previousForm.Longitude), Convert.ToDouble(pointlat1), Convert.ToDouble(pointlong1));
                                                }
                                                else
                                                {
                                                    bearing = GetBearing(CalculatedLatcurrentPosition, CalculatedLongcurrentPosition, Convert.ToDouble(pointlat1), Convert.ToDouble(pointlong1));

                                                    if (pointList[0].ChildNodes.Count > 2)
                                                    {
                                                        for (int k = j; k < pointList[0].ChildNodes.Count - 1; k++)
                                                        {
                                                            distanceBetweenDistanceAPIPoint = distanceBetweenDistanceAPIPoint + CalculateAngularDistance(Convert.ToDouble(pointList[0].ChildNodes.Item(k).ChildNodes.Item(0).InnerText), Convert.ToDouble(pointList[0].ChildNodes.Item(k).ChildNodes.Item(1).InnerText), Convert.ToDouble(pointList[0].ChildNodes.Item(k + 1).ChildNodes.Item(0).InnerText), Convert.ToDouble(pointList[0].ChildNodes.Item(k + 1).ChildNodes.Item(1).InnerText));
                                                        }
                                                        totalHour1 = (toUtcReportdatetime - calculatedUTCDatetime).TotalHours;
                                                        angular_distance = (distanceBetweenDistanceAPIPoint / totalHour1) / 6371000;

                                                    }
                                                    else if (pointList[0].ChildNodes.Count == 2)
                                                    {
                                                        distanceBetweenDistanceAPIPoint = distanceBetweenDistanceAPIPoint + CalculateAngularDistance(Convert.ToDouble(pointList[0].ChildNodes.Item(0).ChildNodes.Item(0).InnerText), Convert.ToDouble(pointList[0].ChildNodes.Item(0).ChildNodes.Item(1).InnerText), Convert.ToDouble(pointList[0].ChildNodes.Item(1).ChildNodes.Item(0).InnerText), Convert.ToDouble(pointList[0].ChildNodes.Item(1).ChildNodes.Item(1).InnerText));
                                                        totalHour1 = (toUtcReportdatetime - calculatedUTCDatetime).TotalHours;
                                                        angular_distance = (distanceBetweenDistanceAPIPoint / totalHour1) / 6371000;
                                                    }
                                                    else
                                                    {
                                                        angular_distance = CalculateVesselSpeed(CalculatedLatcurrentPosition, CalculatedLongcurrentPosition, ParseCoordinate(form.Latitude), ParseCoordinate(form.Longitude), calculatedUTCDatetime, toUtcReportdatetime);
                                                    }
                                                      
                                                    totaldistance = CalculateDistance(CalculatedLatcurrentPosition, CalculatedLongcurrentPosition, Convert.ToDouble(pointlat1), Convert.ToDouble(pointlong1));
                                                }

                                                double totaldistancerevals = 0;

                                            pointLoop:
                                                counter++;
                                                //var angular_distance = avgDistance * 1.852 / 6371;//#radis 6371 in KM of earth
                                                double lat = 0;
                                                double lon = 0;
                                                bearing = ConvertDegreesToRadians(bearing);
                                                if (Convert.ToDouble(currentPositionLat) == 0 && Convert.ToDouble(currentPositionLong) == 0)
                                                {
                                                    lat = ConvertDegreesToRadians((ParseCoordinate(previousForm.Latitude)));
                                                    lon = ConvertDegreesToRadians(ParseCoordinate(previousForm.Longitude));
                                                }
                                                else
                                                {
                                                    lat = ConvertDegreesToRadians(Convert.ToDouble(currentPositionLat));
                                                    lon = ConvertDegreesToRadians(Convert.ToDouble(currentPositionLong));
                                                }

                                                if (distanceApiInBetweenPointPlotting)
                                                {
                                                    angular_distance = CalculateVesselSpeed(CalculatedLatcurrentPosition, CalculatedLongcurrentPosition, Convert.ToDouble(pointlat1), Convert.ToDouble(pointlong1), calculatedUTCDatetime, calculatedUTCDatetime.AddHours(1));
                                                }

                                                var newLat = Math.Asin(Math.Sin(lat) * Math.Cos(angular_distance) + Math.Cos(lat) * Math.Sin(angular_distance) * Math.Cos(bearing));
                                                var newLon = lon + Math.Atan2(Math.Sin(bearing) * Math.Sin(angular_distance) * Math.Cos(lat), Math.Cos(angular_distance) - Math.Sin(lat) * Math.Sin(newLat));

                                                avgDistance = (distanceBetweenDistanceAPIPoint / 1000) / totalHour1;
                                                totaldistancerevals = avgDistance * counter;

                                                newLat = ConvertRadiansToDegrees(newLat);
                                                newLon = ConvertRadiansToDegrees(newLon);
                                                bearing = ConvertRadiansToDegrees(bearing);
                                                var analyzedObj = new AnalyzedWeather();
                                                analyzedObj.AnalyzedCurrent = "0";//
                                                analyzedObj.AnalyzedCurrentDirection = "0"; //
                                                analyzedObj.AnalyzedSeaCurrentSpeedInKnots = 0;
                                                analyzedObj.AnalyzedSeaHeight = "0";
                                                analyzedObj.AnalyzedSwellDirection = "0";
                                                analyzedObj.AnalyzedSwellHeight = "0";
                                                analyzedObj.AnalyzedWave = "0"; //
                                                analyzedObj.AnalyzedWaveDiection = "0";
                                                analyzedObj.AnalyzedWind = "0"; //
                                                analyzedObj.AnalyzedWindDirection = "0";
                                                analyzedObj.Bearing = Convert.ToDecimal(bearing);
                                                analyzedObj.CalculatedTimeStamp = fromUtcReportdatetime.AddHours(currenthrcout + 1);
                                                analyzedObj.CreatedBy = Convert.ToInt64(_currentUser.UserId);
                                                analyzedObj.CreatedDateTime = DateTime.UtcNow;
                                                analyzedObj.Latitude = DecimalToDegreesLatitudeLongitude.DDtoDMS(newLat, CoordinateType.latitude);
                                                analyzedObj.Longitude = DecimalToDegreesLatitudeLongitude.DDtoDMS(newLon, CoordinateType.longitude);
                                                analyzedObj.Is24Hour = false;
                                                analyzedObj.FormId = form.SFPM_Form_Id;
                                                _DbContext.AnalyzedWeather.Add(analyzedObj);
                                                //Save();

                                                itemList.Add(new items() { lat = newLat, log = newLon, reportdatetime = fromUtcReportdatetime.AddHours(currenthrcout + 1) });
                                                currentPositionLat = newLat;
                                                currentPositionLong = newLon;
                                                CalculatedLatcurrentPosition = newLat;
                                                CalculatedLongcurrentPosition = newLon;
                                                calculatedUTCDatetime = fromUtcReportdatetime.AddHours(currenthrcout + 1);

                                                hrcount++;
                                                currenthrcout++;
                                                distancecounter = 0;
                                                if (counter > 1)
                                                {
                                                    i++;
                                                }

                                                if (fromUtcReportdatetime.AddHours(currenthrcout + 1) < toUtcReportdatetime)
                                                {
                                                    if (counter >= loopcondition)
                                                    {
                                                        goto Loop;
                                                    }
                                                    else if (totaldistance >= totaldistancerevals + avgDistance)
                                                    {
                                                        goto pointLoop;
                                                    }
                                                    else if (totaldistance - totaldistancerevals < avgDistance && totaldistance - totaldistancerevals > (avgDistance / 2))
                                                    {
                                                        distanceApiInBetweenPointPlotting = true;
                                                        goto pointLoop;
                                                    }
                                                    else if (counter == loopcondition - 1)
                                                    {
                                                        goto Loop;
                                                    }
                                                    else
                                                    {
                                                        f = 1;
                                                    }

                                                }
                                                else
                                                {
                                                    f = 1;
                                                    break;
                                                }
                                            }
                                            if (f != 1)
                                            {
                                                hrcount++;
                                                distancecounter++;
                                                goto Loop;
                                            }
                                        }
                                    }
                                    else
                                    {
                                        if (fromUtcReportdatetime.AddHours(currenthrcout) < toUtcReportdatetime)
                                        {
                                            hrcount = currenthrcout;
                                            var meteoLat = ConvertDegreesToRadians(ParseCoordinate(form.Latitude));
                                            var meteoLong = ConvertDegreesToRadians(ParseCoordinate(form.Longitude));
                                            bearing = 0;
                                            if (CalculatedLatcurrentPosition == 0 && CalculatedLongcurrentPosition == 0)
                                            {
                                                bearing = GetBearing(ParseCoordinate(previousForm.Latitude), ParseCoordinate(previousForm.Longitude), ParseCoordinate(form.Latitude), ParseCoordinate(form.Longitude));
                                                angular_distance = CalculateVesselSpeed(ParseCoordinate(previousForm.Latitude), ParseCoordinate(previousForm.Longitude), ParseCoordinate(form.Latitude), ParseCoordinate(form.Longitude), fromUtcReportdatetime, toUtcReportdatetime);
                                            }
                                            else
                                            {
                                                bearing = GetBearing(CalculatedLatcurrentPosition, CalculatedLongcurrentPosition, ParseCoordinate(form.Latitude), ParseCoordinate(form.Longitude));
                                                angular_distance = CalculateVesselSpeed(CalculatedLatcurrentPosition, CalculatedLongcurrentPosition, ParseCoordinate(form.Latitude), ParseCoordinate(form.Longitude), calculatedUTCDatetime, toUtcReportdatetime);
                                            }
                                            bearing = ConvertDegreesToRadians(bearing);
                                            
                                            double lat = 0; double lon = 0;
                                            if (currentPositionLat == 0 && currentPositionLong == 0)
                                            {
                                                lat = ConvertDegreesToRadians((ParseCoordinate(previousForm.Latitude)));
                                                lon = ConvertDegreesToRadians(ParseCoordinate(previousForm.Longitude));

                                            }
                                            else
                                            {
                                                lat = ConvertDegreesToRadians(currentPositionLat);
                                                lon = ConvertDegreesToRadians(currentPositionLong);

                                            }
                                            var newLat = Math.Asin(Math.Sin(lat) * Math.Cos(angular_distance) + Math.Cos(lat) * Math.Sin(angular_distance) * Math.Cos(bearing));
                                            var newLon = lon + Math.Atan2(Math.Sin(bearing) * Math.Sin(angular_distance) * Math.Cos(lat), Math.Cos(angular_distance) - Math.Sin(lat) * Math.Sin(newLat));
                                            newLat = ConvertRadiansToDegrees(newLat);
                                            newLon = ConvertRadiansToDegrees(newLon);
                                            bearing = ConvertRadiansToDegrees(bearing);

                                            var analyzedObj = new AnalyzedWeather();
                                            analyzedObj.AnalyzedCurrent = "0";//
                                            analyzedObj.AnalyzedCurrentDirection = "0"; //
                                            analyzedObj.AnalyzedSeaCurrentSpeedInKnots = 0;
                                            analyzedObj.AnalyzedSeaHeight = "0";
                                            analyzedObj.AnalyzedSwellDirection = "0";
                                            analyzedObj.AnalyzedSwellHeight = "0";
                                            analyzedObj.AnalyzedWave = "0"; //
                                            analyzedObj.AnalyzedWaveDiection = "0";
                                            analyzedObj.AnalyzedWind = "0"; //
                                            analyzedObj.AnalyzedWindDirection = "0";
                                            analyzedObj.Bearing = Convert.ToDecimal(bearing);
                                            analyzedObj.CalculatedTimeStamp = fromUtcReportdatetime.AddHours(currenthrcout + 1);
                                            analyzedObj.CreatedBy = Convert.ToInt64(_currentUser.UserId);
                                            analyzedObj.CreatedDateTime = DateTime.UtcNow;
                                            analyzedObj.Latitude = DecimalToDegreesLatitudeLongitude.DDtoDMS(newLat, CoordinateType.latitude);
                                            analyzedObj.Longitude = DecimalToDegreesLatitudeLongitude.DDtoDMS(newLon, CoordinateType.longitude);
                                            analyzedObj.Is24Hour = false;
                                            analyzedObj.FormId = form.SFPM_Form_Id;
                                            _DbContext.AnalyzedWeather.Add(analyzedObj);
                                            //Save();

                                            //degree canvert demain
                                            itemList.Add(new items() { lat = newLat, log = newLon, reportdatetime = fromUtcReportdatetime.AddHours(currenthrcout + 1) });
                                            currentPositionLat = newLat;
                                            currentPositionLong = newLon;
                                            CalculatedLatcurrentPosition = newLat;
                                            CalculatedLongcurrentPosition = newLon;
                                            calculatedUTCDatetime = fromUtcReportdatetime.AddHours(currenthrcout + 1);
                                            hrcount++;
                                            currenthrcout++;
                                            distancecounter = 0;

                                            //exit  calculation 
                                            if (fromUtcReportdatetime.AddHours(currenthrcout + 1) >= toUtcReportdatetime)
                                            {
                                                f = 1;
                                                break;
                                            }
                                        }
                                    }
                                }
                            }
                            else
                            {
                                hrcount++;
                                distancecounter++;
                                goto Loop;
                            }
                        }
                    }
                    Save();
                    pointArrayList.items = itemList;
                    MeteoApiCallToGetToken(pointArrayList, form.SFPM_Form_Id);
                }
            }
        }

        private double CalculateVesselSpeed(double sLatitude, double sLongitude, double eLatitude, double eLongitude, DateTime currentPositionUTCTime, DateTime nextStratumNoonUTCTime)
        {
            //METHOD1
            //var sCoord = new GeoCoordinate(sLatitude, sLongitude);
            //var eCoord = new GeoCoordinate(eLatitude, eLongitude);

            //double distanceBetweenTwoLatLong =  sCoord.GetDistanceTo(eCoord);


            //METHOD2
            //double distanceBetweenTwoLatLong = Double.MinValue;
            //double dLat1InRad = sLatitude * (Math.PI / 180.0);
            //double dLong1InRad = sLongitude * (Math.PI / 180.0);
            //double dLat2InRad = eLatitude * (Math.PI / 180.0);
            //double dLong2InRad = eLongitude * (Math.PI / 180.0);

            //double dLongitude = dLong2InRad - dLong1InRad;
            //double dLatitude = dLat2InRad - dLat1InRad;

            //// Intermediate result a.
            //double a = Math.Pow(Math.Sin(dLatitude / 2.0), 2.0) +
            //           Math.Cos(dLat1InRad) * Math.Cos(dLat2InRad) *
            //           Math.Pow(Math.Sin(dLongitude / 2.0), 2.0);

            //// Intermediate result c (great circle distance in Radians).
            //double c = 2.0 * Math.Asin(Math.Sqrt(a));

            //// Distance.
            //// const Double kEarthRadiusMiles = 3956.0;
            //const Double kEarthRadiusKms = 6376.5;
            //distanceBetweenTwoLatLong = kEarthRadiusKms * c;

            //METHOD3
            //double distanceBetweenTwoLatLong = 0;

            //double dLat = (eLatitude - sLatitude) / 180 * Math.PI;
            //double dLong = (eLongitude - sLongitude) / 180 * Math.PI;

            //double a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2)
            //            + Math.Cos(eLatitude) * Math.Sin(dLong / 2) * Math.Sin(dLong / 2);
            //double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

            ////Calculate radius of earth
            //// For this you can assume any of the two points.
            //double radiusE = 6378135; // Equatorial radius, in metres
            //double radiusP = 6356750; // Polar Radius

            ////Numerator part of function
            //double nr = Math.Pow(radiusE * radiusP * Math.Cos(sLatitude / 180 * Math.PI), 2);
            ////Denominator part of the function
            //double dr = Math.Pow(radiusE * Math.Cos(sLatitude / 180 * Math.PI), 2)
            //                + Math.Pow(radiusP * Math.Sin(sLatitude / 180 * Math.PI), 2);
            //double radius = Math.Sqrt(nr / dr);

            ////Calaculate distance in metres.
            //distanceBetweenTwoLatLong = (radius * c)/1000;



            //METHOD4
            //double rlat1 = Math.PI * sLatitude / 180;
            //double rlat2 = Math.PI * eLatitude / 180;
            //double theta = sLongitude - eLongitude;
            //double rtheta = Math.PI * theta / 180;
            //double distanceBetweenTwoLatLong =
            //    Math.Sin(rlat1) * Math.Sin(rlat2) + Math.Cos(rlat1) *
            //    Math.Cos(rlat2) * Math.Cos(rtheta);
            //distanceBetweenTwoLatLong = Math.Acos(distanceBetweenTwoLatLong);
            //distanceBetweenTwoLatLong = distanceBetweenTwoLatLong * 180 / Math.PI;
            //distanceBetweenTwoLatLong = distanceBetweenTwoLatLong * 60 * 1.1515;


            ////switch (unit)
            ////{
            ////    case 'K': //Kilometers -> default
            ////        return dist * 1.609344;
            ////    case 'N': //Nautical Miles 
            ////        return dist * 0.8684;
            ////    case 'M': //Miles
            ////        return dist;
            ////}

            ////return dist;
            ///


            double d1 = 0;
            d1 = sLatitude * (Math.PI / 180.0);
            double num1 = 0;
            num1 = sLongitude * (Math.PI / 180.0);
            double d2 = 0;
            d2 = eLatitude * (Math.PI / 180.0);
            double num2 = 0;
            num2 = eLongitude * (Math.PI / 180.0) - num1;
            double d3 = 0;
            d3 = Math.Pow(Math.Sin((d2 - d1) / 2.0), 2.0) + Math.Cos(d1) * Math.Cos(d2) * Math.Pow(Math.Sin(num2 / 2.0), 2.0);

            double distanceBetweenTwoLatLong = 0;
            distanceBetweenTwoLatLong = (6376500.0 * (2.0 * Math.Atan2(Math.Sqrt(d3), Math.Sqrt(1.0 - d3))));

            double totalHour = 0;
            totalHour = (nextStratumNoonUTCTime - currentPositionUTCTime).TotalHours;

            double vesselSailingSpeed = 0;
            vesselSailingSpeed = ((distanceBetweenTwoLatLong / totalHour) / 6371000);

            return vesselSailingSpeed;

        }

        private double CalculateAngularDistance(double sLatitude, double sLongitude, double eLatitude, double eLongitude)
        {
            double d1 = 0;
            d1 = sLatitude * (Math.PI / 180.0);
            double num1 = 0;
            num1 = sLongitude * (Math.PI / 180.0);
            double d2 = 0;
            d2 = eLatitude * (Math.PI / 180.0);
            double num2 = 0;
            num2 = eLongitude * (Math.PI / 180.0) - num1;
            double d3 = 0;
            d3 = Math.Pow(Math.Sin((d2 - d1) / 2.0), 2.0) + Math.Cos(d1) * Math.Cos(d2) * Math.Pow(Math.Sin(num2 / 2.0), 2.0);

            double distanceBetweenTwoLatLong = 0;
            distanceBetweenTwoLatLong = (6376500.0 * (2.0 * Math.Atan2(Math.Sqrt(d3), Math.Sqrt(1.0 - d3))));

            return distanceBetweenTwoLatLong;

        }

        #endregion

        public void AnalyzedWeatherCalculation(List<MeteoStratumData> meteoStratumList, Forms form, DateTime previousFormReportDatetime, string latitude, string longitude, DateTime dateTimeStamp, double bearing)
        {
            var previousMeteoStratum = new MeteoStratumData();
            double timeDelta = 0, totTimeDelta = 0;
            double totalCurrentdirection = 0;
            double windSpeed = 0;
            double totalWaveHeight = 0;
            double current = 0;
            double windDirection = 0;
            double waveDirection = 0;
            double seaHeight = 0;
            double swell_Direction = 0;
            double swell_Height = 0;
            double seaCurrentSpeed = 0;
            foreach (var meteostratum in meteoStratumList)
            {
                if (previousMeteoStratum.WindSpeedInKnots == 0)
                {
                    //CHANGED LOGIC INSTEAD OF PICKING 1ST TIMEDELTA AS 0.0001 WE WILL SUBSTRACT CURRENT ANALYZED WEATHER CALCULATEDTIMESTAMP WITH PREVIOUS NOON REPORT REPORTDATETIME.
                    //timeDelta = 0.0001;
                    timeDelta = ((Convert.ToDateTime(meteostratum.GPSTimeStamp).Subtract(Convert.ToDateTime(dateTimeStamp.AddHours(-1))).TotalSeconds) / 86400);
                }
                else
                {
                    timeDelta = ((Convert.ToDateTime(meteostratum.GPSTimeStamp).Subtract(Convert.ToDateTime(previousMeteoStratum.GPSTimeStamp.ToString()))).TotalSeconds) / 86400;
                }
                totTimeDelta += timeDelta;
                totalCurrentdirection += meteostratum.SeaCurrentDirectionInDegrees * timeDelta;
                windSpeed += Convert.ToDouble(meteostratum.WindSpeedInKnots) * timeDelta;
                totalWaveHeight += Convert.ToDouble(meteostratum.TotalWaveHeightInMeters) * timeDelta;
                var differenceSeaCurrentBearing = ConvertDegreesToRadians(Convert.ToDouble(meteostratum.SeaCurrentDirectionInDegrees - Convert.ToDouble(meteostratum.Cog)));
                current += Convert.ToDouble(meteostratum.SeaCurrentSpeedInKnots) * Math.Cos(differenceSeaCurrentBearing) * timeDelta;
                windDirection += meteostratum.WindDirectionInDegrees * timeDelta;
                waveDirection += meteostratum.TotalWaveDirectionInDegrees * timeDelta;
                seaHeight += Convert.ToDouble(meteostratum.SeaHeightInMeters) * timeDelta;
                swell_Direction += meteostratum.SwellDirectionInDegrees * timeDelta;
                swell_Height += Convert.ToDouble(meteostratum.SwellHeightInMeters) * timeDelta;
                seaCurrentSpeed += Convert.ToDouble(meteostratum.SeaCurrentSpeedInKnots) * timeDelta;
                previousMeteoStratum = meteostratum;

            }
            var analyzedWeather = _DbContext.AnalyzedWeather.Where(x => x.FormId == form.SFPM_Form_Id
            && x.Latitude == DecimalToDegreesLatitudeLongitude.DDtoDMS(Convert.ToDouble(latitude), CoordinateType.latitude)
            && x.Longitude == DecimalToDegreesLatitudeLongitude.DDtoDMS(Convert.ToDouble(longitude), CoordinateType.longitude)).FirstOrDefault();
            if (analyzedWeather == null)
            {
                analyzedWeather = new AnalyzedWeather();
                analyzedWeather.FormId = form.SFPM_Form_Id;
                analyzedWeather.AnalyzedWind = String.Format("{0:0.00}", Math.Abs(windSpeed / totTimeDelta));
                analyzedWeather.AnalyzedWave = String.Format("{0:0.00}", Math.Abs(totalWaveHeight / totTimeDelta));
                analyzedWeather.AnalyzedCurrent = String.Format("{0:0.00}", current / totTimeDelta);
                analyzedWeather.AnalyzedWindDirection = String.Format("{0:0.00}", Math.Abs(windDirection / totTimeDelta));
                analyzedWeather.AnalyzedWaveDiection = String.Format("{0:0.00}", Math.Abs(waveDirection / totTimeDelta));
                analyzedWeather.AnalyzedSeaHeight = String.Format("{0:0.00}", Math.Abs(seaHeight / totTimeDelta));
                analyzedWeather.AnalyzedSwellDirection = String.Format("{0:0.00}", Math.Abs(swell_Direction / totTimeDelta));
                analyzedWeather.AnalyzedSwellHeight = String.Format("{0:0.00}", Math.Abs(swell_Height / totTimeDelta));
                analyzedWeather.AnalyzedCurrentDirection = String.Format("{0:0.00}", Math.Abs(totalCurrentdirection / totTimeDelta));
                analyzedWeather.AnalyzedSeaCurrentSpeedInKnots = Convert.ToDecimal(String.Format("{0:0.00}", Math.Abs(seaCurrentSpeed / totTimeDelta)));
                analyzedWeather.CreatedBy = Convert.ToInt64(_currentUser.UserId);
                analyzedWeather.CreatedDateTime = DateTime.UtcNow;

                analyzedWeather.Latitude = DecimalToDegreesLatitudeLongitude.DDtoDMS(Convert.ToDouble(latitude), CoordinateType.latitude);
                analyzedWeather.Longitude = DecimalToDegreesLatitudeLongitude.DDtoDMS(Convert.ToDouble(longitude), CoordinateType.longitude);
                analyzedWeather.Is24Hour = false;
                analyzedWeather.CalculatedTimeStamp = dateTimeStamp;
                analyzedWeather.Bearing = Convert.ToDecimal(String.Format("{0:0.00}", Math.Abs(bearing)));

                _DbContext.AnalyzedWeather.Add(analyzedWeather);
            }
            else
            {
                analyzedWeather.FormId = form.SFPM_Form_Id;
                analyzedWeather.AnalyzedWind = String.Format("{0:0.00}", Math.Abs(windSpeed / totTimeDelta));
                analyzedWeather.AnalyzedWave = String.Format("{0:0.00}", Math.Abs(totalWaveHeight / totTimeDelta));
                analyzedWeather.AnalyzedCurrent = String.Format("{0:0.00}", current / totTimeDelta);
                analyzedWeather.AnalyzedWindDirection = String.Format("{0:0.00}", Math.Abs(windDirection / totTimeDelta));
                analyzedWeather.AnalyzedWaveDiection = String.Format("{0:0.00}", Math.Abs(waveDirection / totTimeDelta));
                analyzedWeather.AnalyzedSeaHeight = String.Format("{0:0.00}", Math.Abs(seaHeight / totTimeDelta));
                analyzedWeather.AnalyzedSwellDirection = String.Format("{0:0.00}", Math.Abs(swell_Direction / totTimeDelta));
                analyzedWeather.AnalyzedSwellHeight = String.Format("{0:0.00}", Math.Abs(swell_Height / totTimeDelta));
                analyzedWeather.AnalyzedCurrentDirection = String.Format("{0:0.00}", Math.Abs(totalCurrentdirection / totTimeDelta));
                analyzedWeather.AnalyzedSeaCurrentSpeedInKnots = Convert.ToDecimal(String.Format("{0:0.00}", Math.Abs(seaCurrentSpeed / totTimeDelta)));
                analyzedWeather.ModifiedBy = Convert.ToInt64(_currentUser.UserId);
                analyzedWeather.ModifiedDateTime = DateTime.UtcNow;

                analyzedWeather.Latitude = DecimalToDegreesLatitudeLongitude.DDtoDMS(Convert.ToDouble(latitude), CoordinateType.latitude);
                analyzedWeather.Longitude = DecimalToDegreesLatitudeLongitude.DDtoDMS(Convert.ToDouble(longitude), CoordinateType.longitude);
                analyzedWeather.Is24Hour = false;
                analyzedWeather.CalculatedTimeStamp = dateTimeStamp;
                analyzedWeather.Bearing = Convert.ToDecimal(String.Format("{0:0.00}", Math.Abs(bearing)));
                _DbContext.AnalyzedWeather.Update(analyzedWeather);
            }
            //Save();
        }


        //Dispance api for geting stratum position

        public XmlNodeList GetPositionFromDistance(double formLat, double formLong, double toLat, double toLong, long formId)
        {
            try
            {
                //Utility.Utility.SendEmail("Distance ApI called for FormId :" + formId, true);
                int count = 0;
            newLoginTickit:
                var client = new RestSharp.RestClient(AzureVaultKey.GetVaultValue("DistanceApiEndpointDistance"));
                var request = new RestSharp.RestRequest(RestSharp.Method.POST);
                request.AddHeader("postman-token", "07440bd8-36ff-7211-758c-8dfed06a01c2");
                request.AddHeader("cache-control", "no-cache");
                request.AddHeader("content-type", "text/xml");
                request.AddParameter("text/xml", "<?xml version=\"1.0\" encoding=\"utf-8\"?>\n<soap:Envelope xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" xmlns:soap=\"http://schemas.xmlsoap.org/soap/envelope/\">\n <soap:Header>\n <DistanceHeader xmlns=\"http://veson.com/webservices/\">\n <loginTicket>" + logintickit + "</loginTicket>\n </DistanceHeader>\n </soap:Header>\n <soap:Body>\n <PointToPoint xmlns=\"http://veson.com/webservices/\">\n <prefs>\n <viaList>\n <int>0</int>\n <int>0</int>\n </viaList>\n <viaPrefs>\n <int>0</int>\n <int>0</int>\n </viaPrefs>\n <disabledRegions>\n <int>0</int>\n <int>0</int>\n </disabledRegions>\n <avoidanceRegions>\n <int>0</int>\n <int>0</int>\n </avoidanceRegions>\n <avoidInshore>true</avoidInshore>\n <avoidRivers>true</avoidRivers>\n <deepWaterFactor>0</deepWaterFactor>\n <minDepth>0</minDepth>\n <minHeight>0</minHeight>\n </prefs>\n <latFrom>" + formLat + "</latFrom>\n <lonFrom>" + formLong + "</lonFrom>\n <latTo>" + toLat + "</latTo>\n <lonTo>" + toLong + "</lonTo>\n </PointToPoint>\n </soap:Body>\n</soap:Envelope>", RestSharp.ParameterType.RequestBody);
                RestSharp.IRestResponse response1 = client.Execute(request);
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(response1.Content);
                XmlNodeList authNode = doc.GetElementsByTagName("faultstring");
                if (response1.Content.Contains("Object reference not set to an instance of an object."))
                {
                    string LogString = "Distance API Exception  FromLat : " + formLat + "  ,FromLon : " + formLong + " ,ToLat : " + toLat + " ,ToLon : " + toLong + "  in  " + AzureVaultKey.GetVaultValue("SfpmApiUrl") + "  Environment  and  UTCDate : " + DateTime.UtcNow + " ,formId : " + formId;
                    LogUtility.WriteLog(LogString);
                    LogUtility.WriteLog("-------------------------------------------------------------------------------------------");
                    Utility.Utility.SendEmail(LogString, true);
                    return null;
                }
                if (authNode.Count != 0 && count < 2)
                {
                    if (response1.Content.Contains("Invalid login ticket"))
                    {
                        logintickit = GetPositionWarningDistanceAPIToken();
                        count++;
                        goto newLoginTickit;
                    }
                }
                XmlNodeList resultNode = doc.GetElementsByTagName("routePoints");
                return resultNode;
            }
            catch (Exception ex)
            {
                string LogString = "Distance API Exception  FromLat : " + formLat + "  ,FromLon : " + formLong + " ,ToLat : " + toLat + " ,ToLon : " + toLong + "  in  " + AzureVaultKey.GetVaultValue("SfpmApiUrl") + "  Environment  and  UTCDate : " + DateTime.UtcNow + " ,formId : " + formId;
                LogUtility.WriteLog(LogString);
                LogUtility.WriteLog("-------------------------------------------------------------------------------------------");
                Utility.Utility.SendEmail(LogString, true);
                return null;

            }
        }

        // calculate  Bearing ***********************************************
        private double GetBearing(double lat11, double lon11, double lat22, double lon22)
        {
            var lat1 = ConvertDegreesToRadians(lat11);
            var lat2 = ConvertDegreesToRadians(lat22);
            var long1 = ConvertDegreesToRadians(lon22);
            var long2 = ConvertDegreesToRadians(lon11);
            var dLon = long1 - long2;

            var y = Math.Sin(dLon) * Math.Cos(lat2);
            var x = Math.Cos(lat1) * Math.Sin(lat2) - Math.Sin(lat1) * Math.Cos(lat2) * Math.Cos(dLon);
            var brng = Math.Atan2(y, x);

            return (ConvertRadiansToDegrees(brng) + 360) % 360;
        }

        private double ConvertDegreesToRadians(double angle)
        {
            return Math.PI * angle / 180.0;
        }

        private double ConvertRadiansToDegrees(double angle)
        {
            return 180.0 * angle / Math.PI;
        }

        private double ParseCoordinate(string point)
        {
            var pointArray = point.TrimStart().Split('°'); //split the string.
            var degrees = Double.Parse(pointArray[0]);
            var min = pointArray[1].ToString().TrimStart().Split("'");
            var minutes = Double.Parse(min[0]) / 60;
            var sec = min[1].ToString().TrimStart().Split("\"");
            var seconds = Double.Parse(sec[0]) / 3600;
            double res = (degrees + minutes + seconds);// * multiplier;
            if (sec[1].Trim().ToString() != null && (sec[1].Trim().ToString() == "S" || sec[1].Trim().ToString() == "W"))
            {
                res = res * -1;
            }
            return res;// * multiplier;
        }
        // End bearing calculation**********************************************

        //calculate distance based on lat long
        private double CalculateDistance(double lat1, double long1, double lat2, double long2)
        {
            double rlat1 = Math.PI * lat1 / 180;
            double rlat2 = Math.PI * lat2 / 180;
            double theta = long1 - long2;
            double rtheta = Math.PI * theta / 180;
            double dist =
                Math.Sin(rlat1) * Math.Sin(rlat2) + Math.Cos(rlat1) *
                Math.Cos(rlat2) * Math.Cos(rtheta);
            dist = Math.Acos(dist);
            dist = dist * 180 / Math.PI;
            dist = dist * 60 * 1.1515;


            return dist * 1.609344;

        }


        // getanalyzed weather api 

        public void MeteoApiCallToGetToken(PointList pointList, long formId)
        {
            try
            {
                if (pointList.items.Count > 0)
                {
                    var client = new RestSharp.RestClient(AzureVaultKey.GetVaultValue("MeteoWeatherImageBaseURLForToken"));
                    var request = new RestSharp.RestRequest(RestSharp.Method.POST);
                    request.AddHeader("Authorization", AzureVaultKey.GetVaultValue("MeteoStratumWeatherAPIAuthorization"));
                    request.AddHeader("Content-Type", "String");
                    request.AddHeader("Cache-Control", "no-cache");
                    string encodedBody = string.Format("grant_type=password&username=" + AzureVaultKey.GetVaultValue("MeteoWeatherImageUsername") + "&password=" + AzureVaultKey.GetVaultValue("MeteoWeatherImagePassword") + "&scope=web default rights claims openid");
                    request.AddParameter("application/x-www-form-urlencoded", encodedBody, RestSharp.ParameterType.RequestBody);
                    RestSharp.IRestResponse response = client.Execute(request);
                    ApiAuthResponse apiResponse = JsonConvert.DeserializeObject<ApiAuthResponse>(response.Content);
                    ExecuteWeatherApi(apiResponse.access_token, pointList, formId);
                }
            }
            catch (Exception ex)
            {
                //Console.WriteLine($"Exception : " + ex.Message);
                // log.LogError(ex.StackTrace, "Error");
            }
        }

        public void ExecuteWeatherApi(string token, PointList pointList, long formId)
        {
            try
            {
                ApiWeatherRequestObject objList = new ApiWeatherRequestObject();
                ApiWeatherRequestObject excludedList = new ApiWeatherRequestObject();
                objList.items = new List<ApiWeatherRequest>();
                excludedList.items = new List<ApiWeatherRequest>();

                foreach (var item in pointList.items)
                {
                    ApiWeatherRequest listItem = new ApiWeatherRequest();
                    double Log = item.log;
                    double Lat = item.lat;
                    if (Log > -181 && Log < 181 && Lat > -76 && Lat < 81)
                    {
                        listItem.locatedAt = new List<double> { Log, Lat };
                        listItem.validAt = item.reportdatetime.ToString("s") + "Z";
                        objList.items.Add(listItem);
                    }
                    else
                    {
                        listItem.locatedAt = new List<double> { Log, Lat };
                        listItem.validAt = item.reportdatetime.ToString("s") + "Z";
                        excludedList.items.Add(listItem);

                    }
                }
                var client = new RestSharp.RestClient(AzureVaultKey.GetVaultValue("MeteostratumBaseURLForWetherAPI"));// Environment.GetEnvironmentVariable("BaseURLForWetherAPI"));
                var request = new RestSharp.RestRequest(RestSharp.Method.POST);
                request.AddHeader("Content-Type", "application/json");
                request.AddHeader("Authorization", "Bearer " + token);
                request.AddJsonBody(JsonConvert.SerializeObject(objList));
                RestSharp.IRestResponse response = client.Execute(request);
                ApiWeatherResponseRootObject apiResponse = JsonConvert.DeserializeObject<ApiWeatherResponseRootObject>(response.Content);
                if (apiResponse.items != null)
                {
                    var meteoStratumDataArray = (pointList.items).ToArray();
                    var apiReponseArray = apiResponse.items.ToArray();
                    AnalyzedWeather analyzedWeather = null;
                    for (int i = 0; i < meteoStratumDataArray.Length; i++)
                    {
                        analyzedWeather = _DbContext.AnalyzedWeather.Where(x => x.FormId == formId && x.CalculatedTimeStamp == meteoStratumDataArray[i].reportdatetime).FirstOrDefault();
                        if (Math.Round(meteoStratumDataArray[i].log, 1) == Math.Round(apiReponseArray[i].locatedAt[0], 1) &&
                        Math.Round(meteoStratumDataArray[i].lat, 1) == Math.Round(apiReponseArray[i].locatedAt[1], 1) &&
                        meteoStratumDataArray[i].reportdatetime.ToString("s").Substring(0, 16) + "Z" == apiReponseArray[i].validAt)
                        {
                            analyzedWeather.AnalyzedWind = apiReponseArray[i].windSpeedInKnots.ToString();
                            analyzedWeather.AnalyzedWave = Convert.ToString(apiReponseArray[i].totalWaveHeightInMeters);
                            var differenceSeaCurrentBearing = ConvertDegreesToRadians(Convert.ToDouble(apiReponseArray[i].seaCurrentDirectionInDegrees - analyzedWeather.Bearing));
                            analyzedWeather.AnalyzedCurrent = String.Format("{0:0.00}", Convert.ToDouble(apiReponseArray[i].seaCurrentSpeedInKnots * Math.Cos(differenceSeaCurrentBearing)));
                            //analyzedWeather.AnalyzedCurrent = (apiReponseArray[i].seaCurrentSpeedInKnots * Math.Cos(Math.Abs(differenceSeaCurrentBearing))).ToString();
                            analyzedWeather.AnalyzedWindDirection = apiReponseArray[i].windDirectionInDegrees.ToString();
                            analyzedWeather.AnalyzedWaveDiection = Convert.ToString(apiReponseArray[i].totalWaveDirectionInDegrees);
                            analyzedWeather.AnalyzedSeaHeight = Convert.ToString(apiReponseArray[i].seaHeightInMeters);
                            analyzedWeather.AnalyzedSwellDirection = Convert.ToString(apiReponseArray[i].swellDirectionInDegrees);
                            analyzedWeather.AnalyzedSwellHeight = Convert.ToString(apiReponseArray[i].swellHeightInMeters);
                            analyzedWeather.AnalyzedCurrentDirection = Convert.ToString(apiReponseArray[i].seaCurrentDirectionInDegrees);
                            analyzedWeather.AnalyzedSeaCurrentSpeedInKnots = Convert.ToDecimal(apiReponseArray[i].seaCurrentSpeedInKnots);
                            analyzedWeather.ModifiedDateTime = DateTime.UtcNow;
                            analyzedWeather.ModifiedBy = analyzedWeather.CreatedBy = Convert.ToInt64(_currentUser.UserId);
                            _DbContext.AnalyzedWeather.Update(analyzedWeather);
                            //Save();
                        }
                    }
                    Save();
                }
            }
            catch (Exception ex)
            {
                //Console.WriteLine($"Exception : " + ex.Message);
                // log.LogError(ex.StackTrace, "Error");
            }

        }

        public class ApiAuthResponse
        {
            public string access_token { get; set; }
            public int expires_in { get; set; }
            public string token_type { get; set; }
        }
        public class ApiWeatherRequest
        {
            public List<double> locatedAt { get; set; }
            public string validAt { get; set; }
        }

        public class ApiWeatherRequestObject
        {
            public List<ApiWeatherRequest> items { get; set; }
        }


        public class ApiWeatherResponseRootObject
        {
            public List<ApiWeatherResponse> items { get; set; }
        }

        [JsonObject]
        public class ApiWeatherResponse
        {
            public List<double> locatedAt { get; set; } = new List<double>();
            public string validAt { get; set; }
            public string source { get; set; }
            public double airPressureInHectoPascal { get; set; }
            public int windSpeedInKnots { get; set; }
            public int windDirectionInDegrees { get; set; }
            public double seaHeightInMeters { get; set; }
            public int seaPeriodInSeconds { get; set; }
            public int swellDirectionInDegrees { get; set; }
            public double swellHeightInMeters { get; set; }
            public int swellPeriodInSeconds { get; set; }
            public int visibilityCode { get; set; }
            public string visibility { get; set; }
            public int weatherCode { get; set; }
            public string weather { get; set; }
            public int precipitationProbabilityInPercent { get; set; }
            public int heightOf500HectoPascalLevelInMeters { get; set; }
            public int seaTemperatureInCelsius { get; set; }
            public int airTemperatureInCelsius { get; set; }
            public int windSpeedAt50MetersInKnots { get; set; }
            public int windDirectionAt50MetersInDegrees { get; set; }
            public int icingClass { get; set; }
            public string icing { get; set; }
            public int riskWindSpeedInKnots { get; set; }
            public int windGustInKnots { get; set; }
            public int windGustAt50MetersInKnots { get; set; }
            public double totalWaveHeightInMeters { get; set; }
            public int totalWaveDirectionInDegrees { get; set; }
            public double riskWaveHeightInMeters { get; set; }
            public double seaCurrentSpeedInKnots { get; set; }
            public int seaCurrentDirectionInDegrees { get; set; }
        }
        #endregion

        #region Dispose

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _DbContext.Dispose();
                }
            }
            _disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion

        #region SendEmail

        public void sendEmails(string category, string fuelType, string vesselName, DateTime reportdatetime)
        {
            try
            {
                if (category != "" && fuelType != "")
                {
                    MailMessage mail = new MailMessage();
                    SmtpClient SmtpServer = new SmtpClient(AzureVaultKey.GetVaultValue("SmtpHostName"));
                    mail.From = new MailAddress(AzureVaultKey.GetVaultValue("SmtpFromEmail"));
                    var mailTo = AzureVaultKey.GetVaultValue("SmtpToEmail");
                    var lstofToData = mailTo.Split(';').ToList();
                    foreach (var item in lstofToData)
                    {
                        mail.To.Add(item);
                    }
                    string env = "";
                    #if Dev
                            env = "Dev";
                    #elif Uat
                            env = "Uat";
                    #endif

                    if (env.Equals("Dev"))
                        mail.Subject = "DEV:VDD report upload New Fuel Or Category Found ";
                    else if (env.Equals("Uat"))
                        mail.Subject = "UAT:VDD report upload New Fuel Or Category Found ";
                    else
                        mail.Subject = "VDD report upload New Fuel Or Category Found ";

                    mail.Body = "New Category or Fuel type found in VDD upload that is " +
                        " VesselName : " + vesselName + ", UTC Reportdatetime : " + reportdatetime + " , Category : " + category + " , FuelType : " + fuelType;
                    mail.IsBodyHtml = true;
                    SmtpServer.EnableSsl = true;
                    SmtpServer.Port = Convert.ToInt32(AzureVaultKey.GetVaultValue("SmtpPort"));
                    SmtpServer.Credentials = new System.Net.NetworkCredential(mail.From.Address, AzureVaultKey.GetVaultValue("SmtpPassword"));
                    SmtpServer.Send(mail);
                }
            }
            catch (Exception ex)
            {

            }
        }

        #endregion

    }
    public class PointList
    {
        public List<items> items { get; set; }
    }
    public class items
    {
        public double lat { get; set; }
        public double log { get; set; }
        public DateTime reportdatetime { get; set; }
        public double finalbearing { get; set; }
    }


}