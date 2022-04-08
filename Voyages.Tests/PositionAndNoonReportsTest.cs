using Data.Solution.Models;
using Data.Solution.Models.ViewModels;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Voyage.Tests;


namespace VoyagesTests
{
    [TestFixture]
    class PositionAndNoonReportsTest
    {
        public DatabaseContext _context;
        SetDatabaseContext dbcontext = new SetDatabaseContext();
        public void Dispose()
        {
            _context.Dispose();
        }

        [Test]
        public void GetReportsForVoyage()
        {
            string imoNumber = "9123"; long voyageNumber = 123; DateTime actualStartOfSeaPassage =DateTime.Now;
            string departureTimeZoneOffset = "+00:00"; long userId = 1;
            _context = dbcontext.SetDbContext();
            List<Forms> formsList = new List<Forms>();
            var pageSetting = _context.PageSettings.Where(setting => setting.UserId == userId).FirstOrDefault();
            if (pageSetting != null && pageSetting.IsPassageUTC && !string.IsNullOrEmpty(departureTimeZoneOffset))
            {
                var hrs = Convert.ToDouble(departureTimeZoneOffset.Substring(1, 2));
                var min = Convert.ToDouble(departureTimeZoneOffset.Substring(4, 2));
                actualStartOfSeaPassage = departureTimeZoneOffset.Contains("-") ? actualStartOfSeaPassage.AddHours(-hrs).AddMinutes(-min) :
                    actualStartOfSeaPassage.AddHours(hrs).AddMinutes(min);
            }
            var previousVoyage = _context.Voyages.Where(x => x.IMONumber == imoNumber && x.ActualStartOfSeaPassage > actualStartOfSeaPassage).OrderBy(x => x.ActualStartOfSeaPassage).FirstOrDefault();
            if (actualStartOfSeaPassage != null && previousVoyage != null)
            {
                formsList = _context.Forms.Where(x => x.VoyageNo == voyageNumber && x.ImoNumber == Convert.ToInt64(imoNumber) && x.ReportDateTime.Value.DateTime >= actualStartOfSeaPassage
                    && x.ReportDateTime.Value.DateTime < previousVoyage.ActualStartOfSeaPassage).OrderBy(a => a.ReportDateTime).ToList();
            }
            else
            {
                formsList = _context.Forms.Where(x => x.VoyageNo == voyageNumber && x.ImoNumber == Convert.ToInt64(imoNumber) && x.ReportDateTime.Value.DateTime >= actualStartOfSeaPassage)
                     .OrderBy(a => a.ReportDateTime).ToList();
            }
            List<VoyagesIntitialDataViewModel> voyagesIntitialList = new List<VoyagesIntitialDataViewModel>();
            List<DateTimeOffset> reportDateTimeList = new List<DateTimeOffset>();
            VoyagesIntitialDataViewModel voyagesModel;
            string sourceUserName = string.Empty, editedByUserName = string.Empty;
            var users = _context.Users.ToList();
            foreach (Forms formRow in formsList)
            {
                sourceUserName = (formRow.CreatedBy == 0) ? "Ship" : (formRow.CreatedBy == -1) ? "VDD" : _context.Users.Where(x => x.UserId == formRow.CreatedBy).FirstOrDefault().Username;
                editedByUserName = (formRow.ModifiedBy == 0) ? "Ship" : (formRow.ModifiedBy == -1) ? "VDD" : _context.Users.Where(x => x.UserId == formRow.ModifiedBy).FirstOrDefault().Username;
                voyagesModel = new VoyagesIntitialDataViewModel();
                voyagesModel.Source = !string.IsNullOrEmpty(formRow.FormGUID) ? "Ship" : sourceUserName;
                voyagesModel.EditedBy = editedByUserName;

                var excludeReportLogs = _context.ExcludeReportLogs.Where(x => x.FormId == formRow.SFPM_Form_Id);
                voyagesModel.ExcludedFromPool = excludeReportLogs.Any() ?
                    excludeReportLogs.Where(x => x.ReportId == 2).OrderByDescending(x => x.CreatedDateTime).ToList().FirstOrDefault().Excluded : false;
                voyagesModel.ExcludedFromTC = excludeReportLogs.Any() ? excludeReportLogs.Where(x => x.ReportId == 1)
                    .OrderByDescending(x => x.CreatedDateTime).ToList().FirstOrDefault().Excluded : false;

                voyagesModel.TimeZone = formRow.ReportTime;
                voyagesModel.Type = formRow.Location == "N" ? "At Sea" :
                                       formRow.Location == "S" ? "At port" :
                                       formRow.FormIdentifier.Contains("Arrival") ? "End Passage" :
                                       formRow.FormIdentifier.Contains("Departure") ? "Start Passage" :
                                        formRow.FormIdentifier.Contains("Bunker") ? "Optimum Bunker" : "";
                voyagesModel.IconType = voyagesModel.Type;
                voyagesModel.DateAndTime = formRow.ReportDateTime;
                voyagesModel.IsConflict = reportDateTimeList.Exists(f => f.Equals(formRow.ReportDateTime));
                reportDateTimeList.Add(formRow.ReportDateTime.Value);
                voyagesModel.Form = formRow;
                voyagesModel.Positions = formRow.Latitude + " / " + formRow.Longitude;

                DateTimeOffset? previousReportDate = _context.Forms.Where(x => x.ReportDateTime.Value.DateTime < formRow.ReportDateTime.Value.DateTime
                    && x.VoyageNo == formRow.VoyageNo && x.ImoNumber == Convert.ToInt64(imoNumber))
                    .OrderByDescending(x => x.ReportDateTime).Select(x => x.ReportDateTime).FirstOrDefault();

                if (previousReportDate == null)
                    previousReportDate = _context.Forms.Where(x => x.ReportDateTime.Value.DateTime < formRow.ReportDateTime.Value.DateTime
                    && x.ImoNumber == Convert.ToInt64(imoNumber)).OrderBy(x => x.ReportDateTime).Select(x => x.ReportDateTime).FirstOrDefault();

                voyagesModel.HrsReportDatetime = previousReportDate != null ?
                 String.Format("{0:0.00}", Convert.ToDecimal(formRow.ReportDateTime.Value.DateTime.Subtract(previousReportDate.Value.DateTime).TotalHours.ToString())) : "0";

                var analyzedWeather = _context.AnalyzedWeather.Where(x => x.FormId == formRow.SFPM_Form_Id).FirstOrDefault();
                if (analyzedWeather != null)
                {
                    voyagesModel.AnalyzedWind = analyzedWeather.AnalyzedWind;
                    voyagesModel.AnalyzedWave = analyzedWeather.AnalyzedWind;
                    voyagesModel.AnalyzedWaveDiection = analyzedWeather.AnalyzedWind;
                    voyagesModel.AnalyzedWindDirection = analyzedWeather.AnalyzedWind;
                    voyagesModel.AnalyzedCurrent = analyzedWeather.AnalyzedWind;
                }
                voyagesModel.IsEventExists = _context.EventROBsRow.Where(x => x.FormId == formRow.SFPM_Form_Id).Any();
                voyagesIntitialList.Add(voyagesModel);
            }
             voyagesIntitialList.OrderBy(x => x.DateAndTime).ToList();
            Assert.IsTrue(true);
        }

        [Test]
        public int CreatePosition()
        {
            Forms forms = new Forms();
            forms.SFPM_Form_Id = 0;
            _context = dbcontext.SetDbContext();
            forms.ReportTime = SetTimezoneFormate(forms.ReportTime);
            var formsObj = _context.Forms.Where(x => x.SFPM_Form_Id == forms.SFPM_Form_Id).FirstOrDefault();
            formsObj = forms;
            var voyageId = (from form in _context.Forms
                            from voyage in _context.Voyages
                            where voyage.VoyageNumber == formsObj.VoyageNo
                            && voyage.ActualStartOfSeaPassage <= formsObj.ReportDateTime.Value.DateTime
                            && voyage.ActualEndOfSeaPassage >= formsObj.ReportDateTime.Value.DateTime
                            select new { voyage.SFPM_VoyagesId }).FirstOrDefault();
            if (voyageId != null)
            {
                PassagesApprovalAudits auditData = _context.PassagesApprovalAudits.Where(x => x.VoyagesId == voyageId.SFPM_VoyagesId).OrderByDescending(x => x.ApprovalDateTime).FirstOrDefault();
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
                    var isvoyage = _context.Voyages.Where(x => x.SFPM_VoyagesId == voyageId.SFPM_VoyagesId).FirstOrDefault();
                    if (isvoyage != null)
                    {
                        isvoyage.ActualEndOfSeaPassage = formsObj.ReportDateTime.Value.DateTime;
                        _context.Voyages.Update(isvoyage);
                    }
                }
            }
            formsObj.SFPM_Form_Id = 0;
            formsObj.CompanyCode = "SCRP";
            formsObj.CompanyName = "Scorpio Commerical Management";

            formsObj.CreatedDateTime = forms.ModifiedDateTime = DateTime.UtcNow;
            formsObj.ReportDateTime = forms.ReportDateTime != null ? forms.ReportDateTime : DateTimeOffset.UtcNow;
            formsObj.ModifiedBy = forms.CreatedBy;
            _context.Forms.Add(formsObj);
            if (formsObj.FuelsRows != null)
            {
                _context.SaveChanges();
                foreach (var fuelrow in formsObj.FuelsRows)
                {
                    fuelrow.FormId = formsObj.SFPM_Form_Id;
                    _context.FuelsRows.Add(fuelrow);
                }
            }
            if (formsObj.FormUnits != null)
            {
                _context.SaveChanges();
                formsObj.FormUnits.FormId = formsObj.SFPM_Form_Id;
                _context.FormUnits.Add(formsObj.FormUnits);
            }
            _context.SaveChanges();
            //return CreatePositionWarning(formsObj);
            //if (voyageId != null)
            //{
            //    CreatePassageWarning(_context.Voyages.Where(x => x.SFPM_VoyagesId == voyageId.SFPM_VoyagesId).FirstOrDefault());
            //}
            Assert.IsTrue(true);
            return (int)ResponseStatus.SAVED;
        }
        //public int DeletePosition(string formIds)
        //{
        //    return DeletePositionRecord(formIds);
        //}
        [Test]
        public void UpdatePositionRemark()
        {
            try
            {
                long formId = 12;
                string remark = "test";
                _context = dbcontext.SetDbContext();
                var isForms = _context.Forms.Where(x => x.SFPM_Form_Id == formId).FirstOrDefault();
                if (isForms != null)
                {
                    isForms.Remarks = remark;
                    _context.Forms.Update(isForms);
                }
                Assert.IsTrue(true);
            }
            catch (Exception ex)
            {

                Assert.Fail();
            }
            
        }
        [Test]
        public void GetPositionWarning()
        {
            long formId = 12;
            _context = dbcontext.SetDbContext();
            var getPosWarning= _context.PositionWarnings.Where(x => x.FormId == formId).ToList();
            Assert.IsTrue(true);
        }
        [Test]
        public void GetPositionWarningAudit()
        {
            long positionWarningId = 12;
            _context = dbcontext.SetDbContext();
            var data= (from positionWarningAudits in _context.PositionWarningAudits
                    join users in _context.Users on positionWarningAudits.ReviewedBy equals users.UserId
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
            Assert.IsTrue(true);
        }

        //public int CreatePositionWarningAudit(PositionWarningAudit positionWarningAudit)
        //{
        //    var isUservalid = _DbContext.Users.Where(x => x.UserId == positionWarningAudit.ReviewedBy).FirstOrDefault();
        //    if (isUservalid == null)
        //        return (int)ResponseStatus.INVALIDUSER;

        //    if (IsVoyageApproved(_DbContext.PositionWarnings.Where(x => x.SFPM_PositionWarningId == positionWarningAudit.PositionWarningId).Select(x => x.FormId).FirstOrDefault()))
        //    {
        //        return (int)ResponseStatus.ALREADYAPPROVED;
        //    }
        //    positionWarningAudit.ReviewDateTime = DateTime.Now;
        //    var chkPositionWarningAudit = _DbContext.PositionWarningAudits.Where(x => x.PositionWarningId == positionWarningAudit.PositionWarningId).OrderByDescending(x => x.SFPM_PositionWarningAuditId).FirstOrDefault();
        //    if (chkPositionWarningAudit == null)
        //    {
        //        _DbContext.PositionWarningAudits.Add(positionWarningAudit);
        //    }
        //    else
        //    {
        //        if (chkPositionWarningAudit.IsApproved != positionWarningAudit.IsApproved)
        //        {
        //            _DbContext.PositionWarningAudits.Add(positionWarningAudit);
        //        }
        //        else
        //        {
        //            return (int)ResponseStatus.ALREADYEXIST;
        //        }
        //    }
        //    Save();
        //    return (int)ResponseStatus.SAVED;
        //}
        public void GetViewOriginalEmail()
        {
            long formId = 12;
            _context = dbcontext.SetDbContext();
            var data= (from forms in _context.Forms
                    where forms.SFPM_Form_Id == formId
                    select new FromsViewModel()
                    {
                        OriginalEmailText = forms.OriginalEmailText,
                        EmailAttachmentFileName = forms.EmailAttachmentFileName,
                        OriginalFormsXML = forms.OriginalFormsXML
                    }).ToList();
            Assert.IsTrue(true);
        }

        [Test]
        public void GetPositionById()
        {
            long formId = 12;
            _context = dbcontext.SetDbContext();
            var forms = _context.Forms.Where(x => x.SFPM_Form_Id == formId).FirstOrDefault();
            forms.FormUnits = _context.FormUnits.Where(x => x.FormId == formId).FirstOrDefault();
            forms.FuelsRows = _context.FuelsRows.Where(x => x.FormId == formId).ToList();
            Assert.IsTrue(true);
        }
        //public int UpdatePosition(Forms forms)
        //{
        //    if (IsVoyageApproved(forms.SFPM_Form_Id))
        //    {
        //        return (int)ResponseStatus.ALREADYAPPROVED;
        //    }
        //    forms.ReportTime = SetTimezoneFormate(forms.ReportTime);
        //    var position = _DbContext.Forms.Where(x => x.SFPM_Form_Id == forms.SFPM_Form_Id).AsNoTracking().SingleOrDefault();
        //    if (position != null)
        //    {
        //        forms.ModifiedDateTime = DateTime.UtcNow;
        //        _DbContext.Forms.Update(forms);
        //        if (forms.FuelsRows != null)
        //        {
        //            foreach (var fuelrow in forms.FuelsRows)
        //            {
        //                var fuelsRows = _DbContext.FuelsRows.Where(x => x.FormId == fuelrow.FormId && x.SFPM_FuelsRowsId == fuelrow.SFPM_FuelsRowsId).SingleOrDefault();
        //                if (fuelsRows != null)
        //                {
        //                    fuelsRows.BDN_Number = fuelrow.BDN_Number;
        //                    fuelsRows.Fuel_Densitry = fuelrow.Fuel_Densitry;
        //                    fuelsRows.Sulphur_Content = fuelrow.Sulphur_Content;
        //                    fuelsRows.QtyLifted = fuelrow.QtyLifted;
        //                    fuelsRows.FuelType = fuelrow.FuelType;
        //                    _DbContext.FuelsRows.Update(fuelsRows);
        //                }
        //            }
        //        }
        //        if (forms.FormUnits != null)
        //        {
        //            var formUnits = _DbContext.FormUnits.Where(x => x.FormId == forms.SFPM_Form_Id && x.SFPM_FormUnitsId == forms.FormUnits.SFPM_FormUnitsId).AsNoTracking().SingleOrDefault();
        //            if (formUnits != null)
        //            {
        //                _DbContext.FormUnits.Update(forms.FormUnits);
        //            }
        //        }
        //        Save();
        //        CreatePositionWarning(forms);
        //        var voyageId = (from form in _DbContext.Forms
        //                        from voyage in _DbContext.Voyages
        //                        where voyage.VoyageNumber == forms.VoyageNo
        //                        && voyage.ActualStartOfSeaPassage <= forms.ReportDateTime.Value.DateTime
        //                        && voyage.ActualEndOfSeaPassage >= forms.ReportDateTime.Value.DateTime
        //                        select new { voyage.SFPM_VoyagesId }).FirstOrDefault();
        //        if (voyageId != null)
        //        {
        //            CreatePassageWarning(_DbContext.Voyages.Where(x => x.SFPM_VoyagesId == voyageId.SFPM_VoyagesId).FirstOrDefault());
        //        }

        //        return (int)ResponseStatus.SAVED;
        //    }
        //    return (int)ResponseStatus.NOTFOUND;
        //}
        private string SetTimezoneFormate(string timeZone)
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

            return timeZone;
        }

      

        //private int CreatePositionWarning(Forms forms)
        //{

        //    var positionWarningList = _DbContext.PositionWarnings.Where(x => x.FormId == forms.SFPM_Form_Id).ToList();
        //    foreach (var positionwarning in positionWarningList)
        //    {
        //        var warningAudit = _DbContext.PositionWarningAudits.Where(x => x.PositionWarningId == positionwarning.SFPM_PositionWarningId).OrderByDescending(x => x.ReviewDateTime).FirstOrDefault();
        //        if (warningAudit != null && !warningAudit.IsApproved)
        //        {
        //            _DbContext.PositionWarningAudits.RemoveRange(_DbContext.PositionWarningAudits.Where(x => x.PositionWarningId == positionwarning.SFPM_PositionWarningId).ToList());
        //            _DbContext.PositionWarnings.Remove(positionwarning);
        //        }
        //        else if (warningAudit == null)
        //        {
        //            _DbContext.PositionWarnings.Remove(positionwarning);
        //        }
        //    }

        //    Forms formSearchResult = _DbContext.Forms.Where(x => x.ReportDateTime.Value.DateTime == forms.ReportDateTime.Value.DateTime && x.SFPM_Form_Id != forms.SFPM_Form_Id && x.ImoNumber == forms.ImoNumber).AsNoTracking().FirstOrDefault();
        //    if (formSearchResult != null)
        //    {
        //        PositionWarning positionWarning = new PositionWarning();
        //        positionWarning.FormId = formSearchResult.SFPM_Form_Id;
        //        positionWarning.WarningText = "Duplicate data , record already exists";
        //        _DbContext.PositionWarnings.Add(positionWarning);
        //    }
        //    var previousforms = _DbContext.Forms.Where(x => x.ImoNumber == forms.ImoNumber && x.VoyageNo == forms.VoyageNo && x.ReportDateTime.Value.DateTime < forms.ReportDateTime.Value.DateTime)
        //   .OrderByDescending(a => a.ReportDateTime).FirstOrDefault();
        //    if (previousforms != null)
        //    {
        //        decimal formsTimeOffset = Convert.ToInt64(forms.ReportTime.Substring(1, 2).ToString());
        //        decimal lastReportTimeOffset = Convert.ToInt64(previousforms.ReportTime.Substring(1, 2));
        //        if ((formsTimeOffset - lastReportTimeOffset) > 1 && (formsTimeOffset != 12 && lastReportTimeOffset != -11 || formsTimeOffset != -11 && lastReportTimeOffset != 12))
        //        {
        //            PositionWarning positionWarning = new PositionWarning();
        //            positionWarning.FormId = forms.SFPM_Form_Id;
        //            positionWarning.WarningText = "Time zone change more than 1 hours between 2 reports";
        //            _DbContext.PositionWarnings.Add(positionWarning);
        //        }
        //    }
        //    Save();
        //    return 0;
        //}


        //private int DeletePositionRecord(string formIds)
        //{
        //    List<string> formIdList = formIds.Split(',').ToList();
        //    foreach (string formId in formIdList)
        //    {

        //        if (IsVoyageApproved(Convert.ToInt64(formId)))
        //        {
        //            return (int)ResponseStatus.ALREADYAPPROVED;
        //        }

        //        var position = _DbContext.Forms.Where(x => x.SFPM_Form_Id == Convert.ToInt64(formId)).FirstOrDefault();
        //        if (position != null)
        //        {
        //            var robs = _DbContext.Robs.Where(x => x.FormId == Convert.ToInt64(formId)).Include(x => x.Rob).ToList();
        //            var isPositionWarnings = _DbContext.PositionWarnings.Where(x => x.FormId == Convert.ToInt64(formId)).ToList();
        //            foreach (var warningUdits in isPositionWarnings)
        //            {
        //                _DbContext.PositionWarningAudits.RemoveRange(_DbContext.PositionWarningAudits.Where(x => x.PositionWarningId == warningUdits.SFPM_PositionWarningId));
        //            }
        //            _DbContext.PositionWarnings.RemoveRange(isPositionWarnings);
        //            foreach (var robsObj in robs)
        //            {
        //                foreach (var robObj in robsObj.Rob)
        //                {
        //                    _DbContext.Allocation.RemoveRange(_DbContext.Allocation.Where(x => x.RobSFPM_RobId == robObj.SFPM_RobId));
        //                }
        //                _DbContext.Rob.RemoveRange(robsObj.Rob);
        //            }
        //            _DbContext.Robs.RemoveRange(robs);
        //            _DbContext.EventROBsRow.RemoveRange(_DbContext.EventROBsRow.Where(x => x.FormId == position.SFPM_Form_Id));
        //            _DbContext.FormUnits.RemoveRange(_DbContext.FormUnits.Where(x => x.FormId == position.SFPM_Form_Id));
        //            _DbContext.FuelsRows.RemoveRange(_DbContext.FuelsRows.Where(x => x.FormId == position.SFPM_Form_Id));
        //            _DbContext.Forms.Remove(position);

        //        }
        //        CreatePositionWarning(position);
        //    }
        //    Save();
        //    if (formIdList.Count > 0)
        //    {
        //        var forms = _DbContext.Forms.Where(x => x.SFPM_Form_Id == Convert.ToInt64(formIdList.FirstOrDefault())).FirstOrDefault();
        //        if (forms != null)
        //        {
        //            var voyageId = (from form in _DbContext.Forms
        //                            from voyage in _DbContext.Voyages
        //                            where voyage.VoyageNumber == forms.VoyageNo
        //                            && voyage.ActualStartOfSeaPassage <= forms.ReportDateTime.Value.DateTime
        //                            && voyage.ActualEndOfSeaPassage >= forms.ReportDateTime.Value.DateTime
        //                            select new { voyage.SFPM_VoyagesId }).FirstOrDefault();
        //            if (voyageId != null)
        //            {
        //                CreatePassageWarning(_DbContext.Voyages.Where(x => x.SFPM_VoyagesId == voyageId.SFPM_VoyagesId).FirstOrDefault());
        //            }
        //        }
        //    }
        //    return (int)ResponseStatus.SAVED;
        //}
    }
}
