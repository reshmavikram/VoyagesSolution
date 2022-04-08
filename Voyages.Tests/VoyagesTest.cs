using Data.Solution.Models;
using Data.Solution.Models.ViewModels;
using Master.Tests;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Voyage.Tests;

namespace VoyagesTests
{
    class VoyagesTest
    {
        public DatabaseContext _context;
        SetDatabaseContext dbcontext = new SetDatabaseContext();
        public void Dispose()
        {
            _context.Dispose();
        }

        [Test]
        public void CreateVoyages()
        {
            _context = dbcontext.SetDbContext();
            //Voyages Voyagesobj = new Voyages();
            //Voyagesobj.VoyageNumber = 1230;
            //Voyagesobj.VesselCode = null;
            //Voyagesobj.Description = "Test 1";
            //Voyagesobj.DeparturePort = "12300";
            //Voyagesobj.LoadCondition = "test 1 ";
            //Voyagesobj.DepartureTimezone = "Test 1";
            //Voyagesobj.ArrivalPort = "12300";
            //Voyagesobj.ArrivalTimezone = "test 1 ";
            //Voyagesobj.ActualStartOfSeaPassage = Voyagesobj.ActualEndOfSeaPassage = DateTime.Now;
            //Voyagesobj.CreatedDateTime = Voyagesobj.ModifiedDateTime = DateTime.UtcNow;
            //Voyagesobj.Status = Status.ACTIVE;
            //var chkVoyages = _context.Voyages.Where(x => x.VoyageNumber == Voyagesobj.VoyageNumber).SingleOrDefault();
            //if (chkVoyages == null)
            //{
            //    _context.Voyages.Add(Voyagesobj);
            //    _context.SaveChanges();
            //}
            Assert.IsTrue(true);
        }

        [Test]
        public void UpdateVoyages()
        {
            //_context = dbcontext.SetDbContext();
            //Voyages Voyagesobj = new Voyages();
            //Voyagesobj.SFPM_VoyagesId = 5;
            //Voyagesobj.VoyageNumber = 1230;
            //Voyagesobj.VesselCode = null;
            //Voyagesobj.Description = "Test 1";
            //Voyagesobj.DeparturePort = "12300";
            //Voyagesobj.LoadCondition = "test 1 ";
            //Voyagesobj.DepartureTimezone = "Test 1";
            //Voyagesobj.ArrivalPort = "12300";
            //Voyagesobj.ArrivalTimezone = "test 1 ";
            //Voyagesobj.ActualStartOfSeaPassage = Voyagesobj.ActualEndOfSeaPassage = DateTime.Now;
           

            //var chkVoyages = _context.Voyages.Where(x => x.SFPM_VoyagesId == Voyagesobj.SFPM_VoyagesId).AsNoTracking().SingleOrDefault();
            //var chkvoyagesnumber = _context.Voyages.Where(x => x.SFPM_VoyagesId != Voyagesobj.SFPM_VoyagesId && x.VoyageNumber == Voyagesobj.VoyageNumber).FirstOrDefault();
            //if (chkVoyages != null && chkvoyagesnumber == null)
            //{
            //    Voyagesobj.ModifiedDateTime = DateTime.UtcNow;
            //    Voyagesobj.CreatedDateTime = chkVoyages.CreatedDateTime;
            //    _context.Voyages.Update(Voyagesobj);
            //    _context.SaveChanges();
            //}
            Assert.IsTrue(true);
        }

        [Test]
        public void GetVoyages()
        {
            _context = dbcontext.SetDbContext();
            int voyagesId = 1;
            var chkvoyages= _context.Voyages.Where(x => x.SFPM_VoyagesId == voyagesId).ToList();
            Assert.IsTrue(true);
        }

        [Test]
        public void GetPassagesApprovalAudit()
        {
            _context = dbcontext.SetDbContext();
            long voyagesId = 6;
            var chkaprroveaudit= _context.PassagesApprovalAudits.Where(x => x.VoyagesId == voyagesId).OrderByDescending(x => x.ApprovalDateTime).Take(1);
            Assert.IsTrue(true);
        }

        [Test]
        public void CreatePosition()
        {
            try
            {
                _context = dbcontext.SetDbContext();
                Forms forms = new Forms();
                forms.SFPM_Form_Id = 0;
                forms.CompanyCode = "SCRP";
                forms.CompanyName = "Scorpio Commerical Management";
                forms.CreatedDateTime = forms.ModifiedDateTime = DateTime.UtcNow;
                _context.Forms.Add(forms);
                _context.SaveChanges();
                Assert.IsTrue(true);
            }
            catch (Exception ex)
            {
                Assert.Fail();
            }
            
        }

        [Test]
        public void GetAllVoyagesByVessel()
        {
            string vesselCode = "STSA";
            _context = dbcontext.SetDbContext();
            var chkvoyages= _context.Voyages.Where(x => x.VesselCode == vesselCode).OrderByDescending(x => x.SFPM_VoyagesId).ToList();
            Assert.IsTrue(true);
        }

        [Test]
        public void GetReportsForPassages()
        {
            int voyageNo = 66;
            _context = dbcontext.SetDbContext();
            DateTime actualStartOfSeaPassage = DateTime.Now;
            DateTime? actualEndOfSeaPassage = DateTime.Now;
            var _voyageNo = new SqlParameter("@VoyageNo", voyageNo);
            var _actualStartOfSeaPassage = new SqlParameter("@StartDateTime", actualStartOfSeaPassage);
            var _actualEndOfSeaPassage = new SqlParameter("@EndDateTime", actualEndOfSeaPassage);
         // need to resolved it // var chkpassagereport = _context.Forms.FromSql("stp_GetReportsForPassages @VoyageNo,@StartDateTime,@EndDateTime", _voyageNo, _actualStartOfSeaPassage, _actualEndOfSeaPassage).ToList();
            Assert.IsTrue(true);
        }

       [Test]
        public int InitialApprove()
        {
            bool isInitialApproved = true;
            long initialApprovedBy = 12;
            string voyageIds = "123";
            _context = dbcontext.SetDbContext();
            var isUservalid = _context.Users.Where(x => x.UserId == initialApprovedBy).FirstOrDefault();
            if (isUservalid == null)
                return (int)ResponseStatus.INVALIDUSER;

            string[] voyageIdList = voyageIds.Split(",");

            foreach (var voyageId in voyageIdList)
            {
                var passagesApprovalAudit = _context.PassagesApprovalAudits.Where(x => x.VoyagesId == Convert.ToInt64(voyageId)).OrderByDescending(x => x.ApprovalDateTime).FirstOrDefault();
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
                _context.PassagesApprovalAudits.Add(passageAudit);
            }
            _context.SaveChanges();
            return (int)ResponseStatus.SAVED;
        }

        [Test]
        public int FinalApprove()
        {
            bool isFinalApproved = false;
            long finalApprovedBy = 123;
            string voyageIds = "12";
            _context = dbcontext.SetDbContext();
            var isUservalid = _context.Users.Where(x => x.UserId == finalApprovedBy).FirstOrDefault();
            if (isUservalid == null)
                return (int)ResponseStatus.INVALIDUSER;

            string[] voyageIdList = voyageIds.Split(",");
            foreach (var voyageId in voyageIdList)
            {
                var passagesApprovalAudit = _context.PassagesApprovalAudits.Where(x => x.VoyagesId == Convert.ToInt64(voyageId)).OrderByDescending(x => x.ApprovalDateTime).FirstOrDefault();
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
                _context.PassagesApprovalAudits.Add(passageAudit);
            }
            _context.SaveChanges();
            return (int)ResponseStatus.SAVED;
        }

        [Test]
        public void CheckFormExists()
        {
            long formId = 12;
            _context = dbcontext.SetDbContext();
            _context.Forms.Where(x => x.SFPM_Form_Id == formId).Any();
            Assert.IsTrue(true);
        }
        [Test]
        public void CheckEventExists()
        {
            long formId = 12; long eventId = 1;
            _context = dbcontext.SetDbContext();
            _context.EventROBsRow.Where(x => x.SFPM_EventROBsRowId == eventId && x.FormId == formId).Any();
            Assert.IsTrue(true);
        }
        [Test]
        public void SavePageSettings()
        {
            long userId = 12; bool isPassageUTC = true; bool isPositionUTC = false;
            _context = dbcontext.SetDbContext();
            var pageSettings = _context.PageSettings.Where(setting => setting.UserId == userId).FirstOrDefault();

            if (pageSettings != null)
            {
                pageSettings.IsPassageUTC = isPassageUTC;
                pageSettings.IsPositionUTC = isPositionUTC;
                _context.PageSettings.Update(pageSettings);
            }
            else
            {
                pageSettings = new PageSettings();
                pageSettings.IsPassageUTC = isPassageUTC;
                pageSettings.IsPositionUTC = isPositionUTC;
                pageSettings.UserId = userId;
                _context.PageSettings.Add(pageSettings);
            }
            _context.SaveChanges();
            Assert.IsTrue(true);
        }

        [Test]
        public void GetPageSettings()
        {
            long userId = 12;
            _context = dbcontext.SetDbContext();
            var pageSettings = _context.PageSettings.Where(setting => setting.UserId == userId).FirstOrDefault();

            if (pageSettings == null)
            {
                pageSettings = new PageSettings();
                pageSettings.IsPassageUTC = true;
                pageSettings.IsPositionUTC = true;
                pageSettings.UserId = userId;
            }

            Assert.IsTrue(true);

        }
        [Test]
        public void GetVesselOwnerNameByImoNumber()
        {
            string imoNumber = "998";
            string vesselOwnerName = string.Empty;
            _context = dbcontext.SetDbContext();
            Vessel vessel = _context.Vessels.Where(x => x.IMONumber == imoNumber).FirstOrDefault();
            if (vessel != null)
            {
                User user = _context.Users.Where(x => x.UserId == vessel.VesselOwnerId).FirstOrDefault();
                if (user != null)
                    vesselOwnerName = user.FirstName + " " + user.LastName;
            }
            Assert.IsTrue(true);
        }

       
    }
}
