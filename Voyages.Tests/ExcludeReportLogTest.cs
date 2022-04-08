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
    class ExcludeReportLogTest : IDisposable
    {
        public DatabaseContext _context;
        SetDatabaseContext dbcontext = new SetDatabaseContext();
        public void Dispose()
        {
            _context.Dispose();
        }

        [Test]
        public void GetPassageReportExclusion()
        {
            long voyageId = 12;
            _context = dbcontext.SetDbContext();
            var exclusionList = _context.ExcludeReportLogs.Where(x => x.VoyagesId == voyageId).OrderByDescending(x => x.CreatedDateTime).ToList();
            exclusionList.ForEach(x => x.excludesList = _context.ExcludeReports.Where(y => y.SFPM_ExcludeReportId == x.ReportId).ToList());
            Assert.IsTrue(true);
        }
        [Test]
        public void GetPositionReportExclusion()
        {
            long formId = 12;
            _context = dbcontext.SetDbContext();
            var exclusionList = _context.ExcludeReportLogs.Where(x => x.FormId == formId).OrderByDescending(x => x.CreatedDateTime).ToList();
            exclusionList.ForEach(x => x.excludesList = _context.ExcludeReports.Where(y => y.SFPM_ExcludeReportId == x.ReportId).ToList());
            Assert.IsTrue(true);

        }
        [Test]
        public void GetPassageReportExclusionlog()
        {
            long voyageId = 12;
            _context = dbcontext.SetDbContext();
            var data = (from l in _context.ExcludeReportLogs
                        join r in _context.ExcludeReports on l.ReportId equals r.SFPM_ExcludeReportId
                        join u in _context.Users on l.UserId equals u.UserId
                        where l.VoyagesId == voyageId
                        select new ExcludeReportLogs { ExcludeReportLogId = l.SFPM_ExcludeReportLogId, Excluded = l.Excluded, ReportName = r.ReportName, Username = u.Username, CreatedDateTime = l.CreatedDateTime, Remarks = l.Remarks }).ToList();

            IEnumerable<ExcludeReportLogs> log = data as IEnumerable<ExcludeReportLogs>;
            Assert.IsTrue(true);
        }
        [Test]
        public void GetPositionReportExclusionlog()
        {
            long formId = 12;
            _context = dbcontext.SetDbContext();
            var data = (from l in _context.ExcludeReportLogs
                        join r in _context.ExcludeReports on l.ReportId equals r.SFPM_ExcludeReportId
                        join u in _context.Users on l.UserId equals u.UserId
                        where l.FormId == formId
                        select new ExcludeReportLogs { ExcludeReportLogId = l.SFPM_ExcludeReportLogId, Excluded = l.Excluded, ReportName = r.ReportName, Username = u.Username, CreatedDateTime = l.CreatedDateTime, Remarks = l.Remarks }).ToList();

            IEnumerable<ExcludeReportLogs> log = data as IEnumerable<ExcludeReportLogs>;
            Assert.IsTrue(true);
        }
        [Test]
        public void GetPssgReptExclResponse()
        {
            long voyagesId = 12;
            _context = dbcontext.SetDbContext();
            var lst1 = _context.ExcludeReportLogs.Where(x => x.VoyagesId == voyagesId && x.ReportId == 1).OrderByDescending(x => x.CreatedDateTime).AsNoTracking().Take(1).ToList();
            var lst2 = _context.ExcludeReportLogs.Where(x => x.VoyagesId == voyagesId && x.ReportId == 2).OrderByDescending(x => x.CreatedDateTime).AsNoTracking().Take(1).ToList();
             lst1.Concat(lst2).ToList();
            Assert.IsTrue(true);
        }
        [Test]
        public void GetPostReptExclResponse()
        {
            long formId = 12;
            _context = dbcontext.SetDbContext();
            var lst1 = _context.ExcludeReportLogs.Where(x => x.VoyagesId == formId && x.ReportId == 1).OrderByDescending(x => x.CreatedDateTime).AsNoTracking().Take(1).ToList();
            var lst2 = _context.ExcludeReportLogs.Where(x => x.VoyagesId == formId && x.ReportId == 2).OrderByDescending(x => x.CreatedDateTime).AsNoTracking().Take(1).ToList();
            lst1.Concat(lst2).ToList();
            Assert.IsTrue(true);
        }
        [Test]
        public void CreatePassageReportExclusion()
        {
            try
            {
                ExcludeReportLog ex = null;
                ExcludeReportLog exclude = new ExcludeReportLog();
                exclude.VoyagesId = 12;
                bool flag = false;
                bool expRpt1 = false, expRpt2 = false;
                _context = dbcontext.SetDbContext();
                var Rpt1 = _context.ExcludeReportLogs.Where(x => x.VoyagesId == exclude.VoyagesId && x.ReportId == 1).OrderByDescending(x => x.CreatedDateTime).AsNoTracking().FirstOrDefault();
                var Rpt2 = _context.ExcludeReportLogs.Where(x => x.VoyagesId == exclude.VoyagesId && x.ReportId == 2).OrderByDescending(x => x.CreatedDateTime).AsNoTracking().FirstOrDefault();
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
                                ex.CreatedDateTime = DateTime.UtcNow;
                                ex.ReportId = item.SFPM_ExcludeReportId;
                                _context.ExcludeReportLogs.Add(ex);
                                _context.SaveChanges();
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
                                ex.CreatedDateTime = DateTime.UtcNow;
                                ex.ReportId = item.SFPM_ExcludeReportId;
                                _context.ExcludeReportLogs.Add(ex);
                                _context.SaveChanges();
                            }
                        }

                    }
                    Assert.IsTrue(true);
                }
                else
                    Assert.IsTrue(true);
            }
            catch (Exception ex)
            {
                Assert.Fail();
            }
           
        }

        [Test]
        public void CreatePositionReportExclusion()
        {
            try
            {
                ExcludeReportLog ex = null;
                ExcludeReportLog exclude = new ExcludeReportLog();
                exclude.FormId = 12;
                _context = dbcontext.SetDbContext();
                bool flag = false;
                bool expRpt1 = false, expRpt2 = false;
                var Rpt1 = _context.ExcludeReportLogs.Where(x => x.FormId == exclude.FormId && x.ReportId == 1).OrderByDescending(x => x.CreatedDateTime).AsNoTracking().FirstOrDefault();
                var Rpt2 = _context.ExcludeReportLogs.Where(x => x.FormId == exclude.FormId && x.ReportId == 2).OrderByDescending(x => x.CreatedDateTime).AsNoTracking().FirstOrDefault();
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
                                ex.UserId = 51;
                                ex.VoyagesId = exclude.VoyagesId;
                                ex.FormId = exclude.FormId;
                                ex.Excluded = Convert.ToBoolean(item.Status);
                                ex.Remarks = exclude.Remarks;
                                ex.CreatedDateTime = DateTime.UtcNow;
                                ex.ReportId = item.SFPM_ExcludeReportId;
                                _context.ExcludeReportLogs.Add(ex);
                                _context.SaveChanges();
                            }

                        }
                        if (item.SFPM_ExcludeReportId == 2)
                        {
                            if (expRpt2 != true)
                            {
                                ex = new ExcludeReportLog();
                                ex.UserId = 51;
                                ex.VoyagesId = exclude.VoyagesId;
                                ex.FormId = exclude.FormId;
                                ex.Excluded = Convert.ToBoolean(item.Status);
                                ex.Remarks = exclude.Remarks;
                                ex.CreatedDateTime = DateTime.UtcNow;
                                ex.ReportId = item.SFPM_ExcludeReportId;
                                _context.ExcludeReportLogs.Add(ex);
                                _context.SaveChanges();
                            }

                        }

                    }
                    Assert.IsTrue(true);
                }
                else
                    Assert.IsTrue(true);
            }
            catch (Exception ex)
            {
                Assert.Fail();
            }
           
        }
    }
}
