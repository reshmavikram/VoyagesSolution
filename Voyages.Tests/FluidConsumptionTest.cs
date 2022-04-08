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
    class FluidConsumptionTest : IDisposable
    {
        public DatabaseContext _context;
        SetDatabaseContext dbcontext = new SetDatabaseContext();
        public void Dispose()
        {
            _context.Dispose();
        }

        [Test]
        public void GetAllFluidConsumption()
        {
            long formId = 123;
            List<Rob> robList = new List<Rob>();
            _context = dbcontext.SetDbContext();
            var robs = _context.Robs.Where(a => a.FormId == formId && a.EventRobsRowId == null).Include(x => x.Rob).ThenInclude(x => x.Allocation).FirstOrDefault();
            var _robList= (robs != null ? robs.Rob != null ? robs.Rob.Where(a => a.Allocation.Count > 0) : robList : robList).OrderByDescending(x => x.SFPM_RobId);
            Assert.IsTrue(true);
        }
        [Test]
        public void GetAllEventFluidConsumption()
        {
            long formId = 123;
            long eventRobsRowId = 1;
            List<Rob> robList = new List<Rob>();
            _context = dbcontext.SetDbContext();
            var robs = _context.Robs.Where(a => a.FormId == formId && a.EventRobsRowId == eventRobsRowId).Include(x => x.Rob).ThenInclude(x => x.Allocation).FirstOrDefault();
            var _robList = (robs != null ? robs.Rob != null ? robs.Rob.Where(a => a.Allocation.Count > 0) : robList : robList).OrderByDescending(x => x.SFPM_RobId);
            Assert.IsTrue(true);
        }
        [Test]
        public void GetFluidConsumptionById()
        {
            long robId = 12;
            _context = dbcontext.SetDbContext();
            _context.Rob.Where(a => a.SFPM_RobId == robId).Include(a => a.Allocation).FirstOrDefault();
            Assert.IsTrue(true);
        }

        [Test]
        public void CreateFluidConsumption()
        {
            try
            {
                FluidFuelConsumedViewModel fluidFuelConsumed = new FluidFuelConsumedViewModel();
                long allocationId = 123;
                fluidFuelConsumed.RobId = allocationId = 0;
                _context = dbcontext.SetDbContext();
                var robs = _context.Robs.Where(a => a.FormId == fluidFuelConsumed.FormId && a.EventRobsRowId == null).FirstOrDefault();
                if (robs == null)
                {
                    //create Robs if not exits
                    robs = new Robs();
                    robs.AsOfDate = DateTime.UtcNow;
                    robs.FormId = fluidFuelConsumed.FormId;

                    _context.Robs.Add(robs);
                    _context.SaveChanges();
                }
                CreateRob(fluidFuelConsumed, robs.SFPM_RobsId, ref allocationId);
                Assert.IsTrue(true);
            }
            catch (Exception ex)
            {
                Assert.Fail();
            }
           
        }
        [Test]
        public void CreateEventFluidConsumption()
        {
            try
            {
                FluidFuelConsumedViewModel fluidFuelConsumed = new FluidFuelConsumedViewModel();
                long allocationId = 123;
                fluidFuelConsumed.RobId = allocationId = 0;
                _context = dbcontext.SetDbContext();
                var robs = _context.Robs.Where(a => a.FormId == fluidFuelConsumed.FormId && a.EventRobsRowId == fluidFuelConsumed.EventRobsRowId).FirstOrDefault();
                if (robs == null)
                {
                    //create Robs if not exits
                    robs = new Robs();
                    robs.AsOfDate = DateTime.UtcNow;
                    robs.FormId = fluidFuelConsumed.FormId;
                    robs.EventRobsRowId = fluidFuelConsumed.EventRobsRowId;

                    _context.Robs.Add(robs);
                    _context.SaveChanges();
                }
                CreateRob(fluidFuelConsumed, robs.SFPM_RobsId, ref allocationId);
                Assert.IsTrue(true);
            }
            catch (Exception ex)
            {
                Assert.Fail();
            }
           
        }
        [Test]
        public int UpdateFluidConsumption()
        {
            FluidFuelConsumedViewModel fluidFuelConsumed = new FluidFuelConsumedViewModel();
            fluidFuelConsumed.FormId = 123;
            fluidFuelConsumed.EventRobsRowId = 12;
            fluidFuelConsumed.RobId = 1;
            fluidFuelConsumed.FluidType = "HSFO";
            fluidFuelConsumed.AllocationId = 12;
            _context = dbcontext.SetDbContext();
            if (fluidFuelConsumed.EventRobsRowId == 0 || fluidFuelConsumed.EventRobsRowId == null)
            {
                var Robs = _context.Robs.Where(x => x.FormId == fluidFuelConsumed.FormId).FirstOrDefault();
                var isRobduplicate = _context.Rob.Where(x => x.SFPM_RobId != fluidFuelConsumed.RobId && x.RobsSFPM_RobsId == Robs.SFPM_RobsId && x.FuelType.ToLower().Equals(fluidFuelConsumed.FluidType.ToLower())).FirstOrDefault();
                if (isRobduplicate != null)
                {
                    return (int)ResponseStatus.ALREADYEXIST;
                }
            }
            else
            { //For Event Rob Duplicate Check Logic
                var Robs = _context.Robs.Where(x => x.EventRobsRowId == fluidFuelConsumed.EventRobsRowId).FirstOrDefault();
                var isRobduplicate = _context.Rob.Where(x => x.SFPM_RobId != fluidFuelConsumed.RobId && x.RobsSFPM_RobsId == Robs.SFPM_RobsId && x.FuelType.ToLower().Equals(fluidFuelConsumed.FluidType.ToLower())).FirstOrDefault();
                if (isRobduplicate != null)
                {
                    return (int)ResponseStatus.ALREADYEXIST;
                }
            }
            var rob = _context.Rob.Where(x => x.SFPM_RobId == fluidFuelConsumed.RobId).FirstOrDefault();
            if (rob == null)
            {
                return (int)ResponseStatus.NOTFOUND;
            }
            else
            {
                rob.FuelType = fluidFuelConsumed.FluidType;
                rob.Units = fluidFuelConsumed.Unit;
                _context.Rob.Update(rob);

                var allocation = _context.Allocation.Where(x => x.RobSFPM_RobId == fluidFuelConsumed.RobId && x.SFPM_AllocationId == fluidFuelConsumed.AllocationId).FirstOrDefault();
                if (allocation != null)
                {
                    allocation.Name = fluidFuelConsumed.Category;
                    allocation.text = fluidFuelConsumed.Consumption.ToString();
                    _context.Allocation.Update(allocation);
                }
            }
            _context.SaveChanges();
            Assert.IsTrue(true);
            return (int)ResponseStatus.SAVED;
        }
        [Test]
        public void DeleteFluidConsumption()
        {
            try
            {
                long robId = 12;
                _context = dbcontext.SetDbContext();
                if (IsVoyageApproved(_context.Robs.Where(x => x.SFPM_RobsId == (_context.Rob.Where(y => y.SFPM_RobId == robId).Select(y => y.RobsSFPM_RobsId).FirstOrDefault())).Select(x => x.FormId).FirstOrDefault()))
                {
                    Assert.IsTrue(true);
                }
                var chkRob = _context.Rob.Where(x => x.SFPM_RobId == robId).SingleOrDefault();
                if (chkRob != null)
                {
                    var chkAllocation = _context.Allocation.Where(x => x.RobSFPM_RobId == robId).ToList();
                    if (chkAllocation != null)
                        _context.Allocation.RemoveRange(chkAllocation);
                    _context.Rob.Remove(chkRob);
                    _context.SaveChanges();
                    Assert.IsTrue(true);
                    
                }
                else
                    Assert.IsNull(chkRob);
            }
            catch (Exception ex)
            {
                Assert.Fail();
            }
           

        }

        private long CreateRob(FluidFuelConsumedViewModel fluidFuelConsumed, long robsId, ref long allocationId)
        {
            long id = 0;
            allocationId = 0;
            _context = dbcontext.SetDbContext();
            var rob = _context.Rob.Where(x => x.RobsSFPM_RobsId == robsId && x.FuelType.ToLower().Equals(fluidFuelConsumed.FluidType)).SingleOrDefault();
            if (rob != null)
            {
                var allocation = _context.Allocation.Where(x => x.RobSFPM_RobId == rob.SFPM_RobId && x.Name.ToLower() == fluidFuelConsumed.Category.ToLower()).SingleOrDefault();
                if (allocation == null)
                {
                    //create only allocation if rob exists
                    List<Allocation> allocationList = new List<Allocation>();
                    Allocation allocationObj = new Allocation();
                    allocationObj.Name = fluidFuelConsumed.Category;
                    allocationObj.text = fluidFuelConsumed.Consumption.ToString();
                    allocationList.Add(allocationObj);
                    rob.Allocation = allocationList;

                    _context.Rob.Update(rob);
                    _context.SaveChanges();
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

                List<Allocation> allocationList = new List<Allocation>();
                Allocation allocationObj = new Allocation();
                allocationObj.Name = fluidFuelConsumed.Category;
                allocationObj.text = fluidFuelConsumed.Consumption.ToString();
                allocationList.Add(allocationObj);
                robObj.Allocation = allocationList;

                _context.Rob.Add(robObj);
                _context.SaveChanges();
                id = robObj.SFPM_RobId;
                allocationId = robObj.Allocation.OrderByDescending(a => a.SFPM_AllocationId).FirstOrDefault().SFPM_AllocationId;
            }
            return id;
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
