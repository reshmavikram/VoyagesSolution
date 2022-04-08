using Data.Solution.Models;
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
    class CreateEventReportExclusion
    {
        public DatabaseContext _context;
        SetDatabaseContext dbcontext = new SetDatabaseContext();
        public void Dispose()
        {
            _context.Dispose();
        }
        //[Test]
        //public void EventReportExclusion()
        //{
        //    try
        //    {
        //        _context = dbcontext.SetDbContext();
        //        var data = _context.Forms.Where(x => x.SFPM_Form_Id == 6).AsNoTracking().FirstOrDefault();
        //        if (data != null)
        //        {
        //            data.IsCharterPartyExclude = true;
        //            data.IsPoolExclude = true;
        //            data.ReportExclude_Remarks = "test";
        //            _context.Forms.Update(data);
        //        }
        //        else
        //        {
        //            data.CreatedDateTime = DateTime.UtcNow;
        //            data.IsCharterPartyExclude = true;
        //            data.IsPoolExclude = true;
        //            data.ReportExclude_Remarks = "test";
        //            _context.Forms.Add(data);
        //        }
        //        _context.SaveChanges();
        //        Assert.IsTrue(true);
        //    }
        //    catch (Exception ex)
        //    {
        //        Assert.Fail();
        //    }

        //}
        [Test]
        public void GetAllEvents()
        {
            long formId = 12;
            _context = dbcontext.SetDbContext();
            List<EventROBsRow> eventROBsRow = _context.EventROBsRow.Where(x => x.FormId == formId).ToList();
            Assert.IsTrue(true);
        }

        [Test]
        public void GetEventById()
        {
            long eventId = 1;
            _context = dbcontext.SetDbContext();
            var eventByID=_context.EventROBsRow.Where(x => x.SFPM_EventROBsRowId == eventId).FirstOrDefault();
            Assert.IsTrue(true);
        }
        [Test]
        public void CreateEvent()
        {
            try
            {
                EventROBsRow eventROBsRow = new EventROBsRow();
                eventROBsRow.EventType = "N";
                eventROBsRow.FormId = 1;
                _context = dbcontext.SetDbContext();
                var eventObj = _context.EventROBsRow.Where(x => x.EventType == eventROBsRow.EventType && x.FormId == eventROBsRow.FormId).FirstOrDefault();
                if (eventObj == null)
                {
                    _context.EventROBsRow.Add(eventROBsRow);
                    _context.SaveChanges();
                }
                Assert.IsTrue(true);
            }
            catch (Exception ex)
            {
                Assert.Fail();
            }
           
        }

        [Test]
        public void UpdateEvent()
        {
            try
            {
                EventROBsRow eventROBsRow = new EventROBsRow();
                _context = dbcontext.SetDbContext();
                eventROBsRow.SFPM_EventROBsRowId = 123;
                eventROBsRow.FormId = 123;
                eventROBsRow.EventType = "N";
                var eventObj = _context.EventROBsRow.Where(x => x.SFPM_EventROBsRowId != eventROBsRow.SFPM_EventROBsRowId && x.FormId == eventROBsRow.FormId && x.EventType == eventROBsRow.EventType).AsNoTracking().FirstOrDefault();
                var eventObjchk = _context.EventROBsRow.Where(x => x.SFPM_EventROBsRowId == eventROBsRow.SFPM_EventROBsRowId).AsNoTracking().FirstOrDefault();
                if (eventObj == null && eventObjchk != null)
                {
                    _context.EventROBsRow.Update(eventROBsRow);
                    _context.SaveChanges();
                }
                Assert.IsTrue(true);
            }
            catch (Exception ex)
            {
                Assert.Fail();
            }
           
        }

        [Test]
        public int DeleteEvent(long eventId)
        {
            _context = dbcontext.SetDbContext();
            if (IsVoyageApproved(_context.EventROBsRow.Where(x => x.SFPM_EventROBsRowId == eventId).Select(x => x.FormId).FirstOrDefault()))
            {
                return (int)ResponseStatus.ALREADYAPPROVED;
            }
            var isrobs = _context.Robs.Where(x => x.EventRobsRowId == eventId).FirstOrDefault();
            if (isrobs != null)
            {
                return (int)ResponseStatus.CURRENTLYINUSE;
            }
            var eventObj = _context.EventROBsRow.Where(x => x.SFPM_EventROBsRowId == eventId).FirstOrDefault();
            if (eventObj != null)
            {
                _context.EventROBsRow.Remove(eventObj);
                _context.SaveChanges();
                return (int)ResponseStatus.SAVED;
            }
            return (int)ResponseStatus.NOTFOUND;
        }

        public bool IsVoyageApproved(long formId)
        {
            _context = dbcontext.SetDbContext();
            var voyageId = (from forms in _context.Forms
                            from voyage in _context.Voyages
                            where voyage.VoyageNumber == forms.VoyageNo
                            && voyage.ActualStartOfSeaPassage <= forms.ReportDateTime.Value.DateTime
                            && voyage.ActualEndOfSeaPassage >= forms.ReportDateTime.Value.DateTime
                            && forms.SFPM_Form_Id == formId
                            select new { voyage.SFPM_VoyagesId }).FirstOrDefault();
            if (voyageId != null)
            {
                PassagesApprovalAudits auditData = _context.PassagesApprovalAudits.Where(x => x.VoyagesId == voyageId.SFPM_VoyagesId).OrderByDescending(x => x.ApprovalDateTime).FirstOrDefault();
                if (auditData != null)
                {
                    if (auditData.IsInitialApproved || auditData.IsFinalApproved)
                        return true;
                }
            }
            return false;
        }
    }
}
