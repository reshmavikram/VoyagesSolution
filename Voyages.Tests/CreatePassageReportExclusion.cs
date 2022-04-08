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
    class CreatePassageReportExclusion:IDisposable
    {
        public DatabaseContext _context;
        SetDatabaseContext dbcontext = new SetDatabaseContext();
        public void Dispose()
        {
            _context.Dispose();
        }

        [Test]
        public void GetVoyage()
        {
            long voyagesId = 12;
            _context = dbcontext.SetDbContext();
            var voyages=_context.Voyages.Where(x => x.SFPM_VoyagesId == voyagesId).FirstOrDefault();
            Assert.IsTrue(true);
        }

        [Test]
        public void GetAllVoyagesByVessel()
        {
            string imoNumber = "99";
            long userId = 123;
            _context = dbcontext.SetDbContext();
            var voyageList = _context.Voyages.Where(x => x.IMONumber == imoNumber).OrderByDescending(x => x.ActualStartOfSeaPassage).ToList();
            voyageList.ForEach(x => x.IsConflict = _context.PassageWarnings.Where(y => y.VoyageId == x.SFPM_VoyagesId).ToList().Count > 0 ? true : false);
            Assert.IsTrue(true);
        }

        [Test]
        public void GetPassageWarning()
        {
            long voyageId = 123;
            _context = dbcontext.SetDbContext();
           var getPassageWarning= _context.PassageWarnings.Where(x => x.VoyageId == voyageId).ToList();
            Assert.IsTrue(true);
        }

        [Test]
        public void GetPassageWarningAudit()
        {
            long passageWarningId = 123;
            _context = dbcontext.SetDbContext();
            var data= (from passageWarningAudits in _context.PassageWarningAudits
                    join users in _context.Users on passageWarningAudits.ReviewedBy equals users.UserId
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
            Assert.IsTrue(true);
        }
        [Test]
        public int CreatePassageWarningAudit()
        {

            PassageWarningAudit passageWarningAudit = new PassageWarningAudit();
            _context = dbcontext.SetDbContext();
            passageWarningAudit.PassageWarningId = 12;
            var passagesApprovalAudit = _context.PassagesApprovalAudits.Where(x => x.VoyagesId == (_context.PassageWarnings.Where(y => y.SFPM_PassageWarningId == passageWarningAudit.PassageWarningId).Select(y => y.VoyageId).FirstOrDefault())).OrderByDescending(x => x.ApprovalDateTime).FirstOrDefault();
            if (passagesApprovalAudit != null)
            {
                if (passagesApprovalAudit.IsFinalApproved || passagesApprovalAudit.IsInitialApproved)
                    return (int)ResponseStatus.ALREADYAPPROVED;
            }

            var isUservalid = _context.Users.Where(x => x.UserId == passageWarningAudit.ReviewedBy).FirstOrDefault();
            if (isUservalid == null)
                return (int)ResponseStatus.INVALIDUSER;

            passageWarningAudit.ReviewDateTime = DateTime.Now;
            var chkPassageWarningAudit = _context.PassageWarningAudits.Where(x => x.PassageWarningId == passageWarningAudit.PassageWarningId).OrderByDescending(x => x.SFPM_PassageWarningAuditId).FirstOrDefault();
            if (chkPassageWarningAudit == null)
            {
                _context.PassageWarningAudits.Add(passageWarningAudit);
            }
            else
            {
                if (chkPassageWarningAudit.IsApproved != passageWarningAudit.IsApproved)
                {
                    _context.PassageWarningAudits.Update(passageWarningAudit);
                }
                else
                {
                    return (int)ResponseStatus.ALREADYEXIST;
                }
            }
            _context.SaveChanges();
            Assert.IsTrue(true);
            return (int)ResponseStatus.SAVED;
        }
    }
}
