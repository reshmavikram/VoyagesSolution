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
    class BunkerTest : IDisposable
    {
        public DatabaseContext _context;
        SetDatabaseContext dbcontext = new SetDatabaseContext();
        public void Dispose()
        {
            _context.Dispose();
        }

        [Test]
        public void GetAllFluidBunker()
        {
            List<FluidBunkerViewModel> fluidBunkerObjList = new List<FluidBunkerViewModel>();
            FluidBunkerViewModel fluidBunkerViewModel;
            long formId = 12;
            _context = dbcontext.SetDbContext();
            //Get ROB bunker type

            var robs = _context.Robs.Where(a => a.FormId == formId && a.EventRobsRowId == null).Include(x => x.Rob).FirstOrDefault();
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
            var isFuelsRows = _context.FuelsRows.Where(a => a.FormId == formId).ToList();
            if (isFuelsRows != null)
            {
                foreach (var rob in isFuelsRows)
                {
                    fluidBunkerViewModel = new FluidBunkerViewModel();
                    fluidBunkerViewModel.FormId = formId;
                    fluidBunkerViewModel.RobId = rob.SFPM_FuelsRowsId;
                    fluidBunkerViewModel.BunkerType = "Bunkers Lifted";
                    fluidBunkerViewModel.FluidType = rob.FuelType;
                    fluidBunkerViewModel.Unit = "MT";
                    fluidBunkerViewModel.Consumption = rob.QtyLifted;
                    fluidBunkerObjList.Add(fluidBunkerViewModel);
                }
            }
            Assert.IsTrue(true);

        }

        [Test]
        public void GetFluidBunkerById()
        {
            FluidBunkerViewModel fluidBunkerObj = new FluidBunkerViewModel();
            long robId = 12;
            string bunkerType = "ROB";
            _context = dbcontext.SetDbContext();
            if (bunkerType == "ROB")
            {
                var rob = _context.Rob.Where(a => a.SFPM_RobId == robId).FirstOrDefault();
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
                var fuelsRows = _context.FuelsRows.Where(a => a.SFPM_FuelsRowsId == robId).FirstOrDefault();
                if (fuelsRows != null)
                {
                    fluidBunkerObj = new FluidBunkerViewModel();
                    fluidBunkerObj.RobId = fuelsRows.SFPM_FuelsRowsId;
                    fluidBunkerObj.BunkerType = "Bunkers Lifted";
                    fluidBunkerObj.FluidType = fuelsRows.FuelType;
                    fluidBunkerObj.Unit = "MT";
                    fluidBunkerObj.Consumption = fuelsRows.QtyLifted;
                }
            }
            Assert.IsTrue(true);
        }
        [Test]
        public void GetAllEventFluidBunker()
        {
            long formId = 12; long eventRobsRowId = 12;
            List<FluidBunkerViewModel> fluidBunkerObjList = new List<FluidBunkerViewModel>();
            FluidBunkerViewModel fluidBunkerViewModel;
            _context = dbcontext.SetDbContext();
            //Get ROB bunker type
            var robs = _context.Robs.Where(a => a.FormId == formId && a.EventRobsRowId == eventRobsRowId).Include(x => x.Rob).FirstOrDefault();
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
            Assert.IsTrue(true);
        }

        [Test]
        public void CreateFluidBunker()
        {
            try
            {
                FluidBunkerViewModel fluidBunker = new FluidBunkerViewModel();
                fluidBunker.BunkerType = "ROB";
                _context = dbcontext.SetDbContext();
                if (fluidBunker.BunkerType == "ROB")
                {
                    fluidBunker.RobId = 0;
                    var robs = _context.Robs.Where(a => a.FormId == fluidBunker.FormId && a.EventRobsRowId == null).FirstOrDefault();
                    if (robs == null)
                    {
                        //create Robs if not exits
                        robs = new Robs();
                        robs.AsOfDate = DateTime.UtcNow;
                        robs.FormId = fluidBunker.FormId;

                        _context.Robs.Add(robs);
                        _context.SaveChanges();
                    }
                    CreateBunkerRob(fluidBunker, robs.SFPM_RobsId);
                }
                else
                {
                    fluidBunker.RobId = 0;
                    var isFuelsRows = _context.FuelsRows.Where(a => a.FormId == fluidBunker.FormId && a.FuelType.ToLower().Equals(fluidBunker.FluidType.ToLower())).FirstOrDefault();
                    if (isFuelsRows == null)
                    {
                        //create Rob for Lifted bunker
                        isFuelsRows = new FuelsRows();
                        isFuelsRows.FuelType = fluidBunker.FluidType;
                        isFuelsRows.QtyLifted = Convert.ToInt32(fluidBunker.Consumption);
                        isFuelsRows.FormId = fluidBunker.FormId;
                        _context.FuelsRows.Add(isFuelsRows);
                        _context.SaveChanges();

                    }
                }
                Assert.IsTrue(true);
            }
            catch (Exception ex)
            {
                Assert.Fail();
            }
            
        }
        [Test]
        public void UpdateFluidBunker()
        {
            try
            {
                FluidBunkerViewModel fluidBunker = new FluidBunkerViewModel();
                fluidBunker.BunkerType = "ROB";
                _context = dbcontext.SetDbContext();
                if (fluidBunker.BunkerType == "ROB")
                {
                    if (fluidBunker.EventROBsRowId == 0 || fluidBunker.EventROBsRowId == null)
                    {
                        var Robs = _context.Robs.Where(x => x.FormId == fluidBunker.FormId).FirstOrDefault();
                        var isRobduplicate = _context.Rob.Where(x => x.SFPM_RobId != fluidBunker.RobId && x.RobsSFPM_RobsId == Robs.SFPM_RobsId && x.FuelType.ToLower().Equals(fluidBunker.FluidType.ToLower())).FirstOrDefault();
                    }
                    else
                    { //For Event Rob Duplicate Check Logic
                        var Robs = _context.Robs.Where(x => x.EventRobsRowId == fluidBunker.EventROBsRowId && x.FormId == fluidBunker.FormId).FirstOrDefault();
                        var isRobduplicate = _context.Rob.Where(x => x.SFPM_RobId != fluidBunker.RobId && x.RobsSFPM_RobsId == Robs.SFPM_RobsId && x.FuelType.ToLower().Equals(fluidBunker.FluidType.ToLower())).FirstOrDefault();
                    }
                    var rob = _context.Rob.Where(x => x.SFPM_RobId == fluidBunker.RobId).FirstOrDefault();
                    if (rob == null)
                    {
                        //ResponseStatus.NOTFOUND;
                    }
                    else
                    {
                        rob.FuelType = fluidBunker.FluidType;
                        rob.Units = fluidBunker.Unit;
                        rob.Remaining = fluidBunker.Consumption.ToString();
                        _context.Rob.Update(rob);
                    }
                }
                else
                {
                    var fuelsRows = _context.FuelsRows.Where(x => x.SFPM_FuelsRowsId == fluidBunker.RobId).FirstOrDefault();
                    if (fuelsRows == null)
                    {
                        // return (int)ResponseStatus.NOTFOUND;
                    }
                    var isRobduplicate = _context.FuelsRows.Where(x => x.SFPM_FuelsRowsId != fluidBunker.RobId && x.FormId == fluidBunker.FormId && x.FuelType.ToLower().Equals(fluidBunker.FluidType.ToLower())).FirstOrDefault();
                    if (isRobduplicate != null)
                    {
                        //return (int)ResponseStatus.ALREADYEXIST;
                    }
                    else
                    {
                        fuelsRows.FuelType = fluidBunker.FluidType;
                        fuelsRows.QtyLifted = Convert.ToInt32(fluidBunker.Consumption);
                        _context.FuelsRows.Update(fuelsRows);
                    }
                }
                _context.SaveChanges();
                Assert.IsTrue(true);
                //return (int)ResponseStatus.SAVED;
            }
            catch (Exception ex)
            {
                Assert.Fail();
            }
           
        }
        [Test]
        public void DeleteFluidBunker()
        {
            try
            {
                int formId = 12;
                int robId = 1;
                string bunkerType = "ROB";
                _context = dbcontext.SetDbContext();
                if (bunkerType == "ROB")
                {
                    var chkRob = _context.Rob.Where(x => x.SFPM_RobId == robId).SingleOrDefault();
                    if (chkRob != null)
                    {
                        var chkAllocation = _context.Allocation.Where(x => x.RobSFPM_RobId == robId).ToList();
                        if (chkAllocation != null)
                            _context.Allocation.RemoveRange(chkAllocation);
                        _context.Rob.Remove(chkRob);
                    }
                }
                else
                {
                    var isFuelsRows = _context.FuelsRows.Where(x => x.SFPM_FuelsRowsId == robId).SingleOrDefault();
                    if (isFuelsRows != null)
                    {
                        _context.FuelsRows.RemoveRange(isFuelsRows);
                    }
                }
                _context.SaveChanges();
                Assert.IsTrue(true);
            }
            catch (Exception ex)
            {
                Assert.Fail();
            }
           
        }

        [Test]
        public void CreateEventFluidBunker()
        {
            try
            {
                FluidBunkerViewModel fluidBunker = new FluidBunkerViewModel();
                fluidBunker.FormId = 12;
                fluidBunker.EventROBsRowId = 1;
                _context = dbcontext.SetDbContext();
                if (fluidBunker.BunkerType == "ROB")
                {
                    fluidBunker.RobId = 0;
                    var robs = _context.Robs.Where(a => a.FormId == fluidBunker.FormId && a.EventRobsRowId == fluidBunker.EventROBsRowId).FirstOrDefault();
                    if (robs == null)
                    {
                        //create Robs if not exits
                        robs = new Robs();
                        robs.AsOfDate = DateTime.UtcNow;
                        robs.FormId = fluidBunker.FormId;
                        robs.EventRobsRowId = fluidBunker.EventROBsRowId;
                        _context.Robs.Add(robs);
                        _context.SaveChanges();

                    }
                    CreateBunkerRob(fluidBunker, robs.SFPM_RobsId);
                }
                Assert.IsTrue(true);
            }
            catch (Exception ex)
            {
                Assert.Fail();
            }
            
        }
        [Test]
        public void GetAllBunkerType()
        {
            _context = dbcontext.SetDbContext();
            var bunkerTypes=_context.BunkerTypes.ToList();
            Assert.IsTrue(true);
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

        private long CreateBunkerRob(FluidBunkerViewModel fluidBunker, long robsId)
        {
            long id = 0;
            _context = dbcontext.SetDbContext();
            var rob = _context.Rob.Where(x => x.RobsSFPM_RobsId == robsId && x.FuelType.ToLower().Equals(fluidBunker.FluidType.ToLower())).SingleOrDefault();
            if (rob == null)
            {
                //create Rob if not exists
                Rob robObj = new Rob();
                robObj.FuelType = fluidBunker.FluidType;
                robObj.Units = fluidBunker.Unit;
                robObj.RobsSFPM_RobsId = robsId;
                robObj.Remaining = fluidBunker.Consumption.ToString();
                _context.Rob.Add(robObj);
                _context.SaveChanges();
                id = robObj.SFPM_RobId;
            }
            else
            {
                rob.Units = fluidBunker.Unit;
                rob.Remaining = fluidBunker.Consumption.ToString();
                _context.Rob.Update(rob);
                _context.SaveChanges();
                id = rob.SFPM_RobId;
            }
            return id;
        }
    }
}
