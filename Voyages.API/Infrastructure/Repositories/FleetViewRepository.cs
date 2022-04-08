using Data.Solution.Models;
using Data.Solution.Models.ViewModels;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VoyagesAPIService.Infrastructure.Helper;

namespace VoyagesAPIService.Infrastructure.Repositories
{
    public class FleetViewRepository : IDisposable
    {

        private DatabaseContext _DbContext;
        private bool _disposed;
        private readonly UserContext _currentUser;

        public FleetViewRepository(DatabaseContext databaseContext, UserContext currentUser)
        {
            _DbContext = databaseContext;
            _DbContext.Database.SetCommandTimeout(Convert.ToInt32(AzureVaultKey.GetVaultValue("CommandTimeout")));
            this._currentUser = currentUser;
        }

        public int Save()
        {
            return _DbContext.SaveChanges();
        }
        public IEnumerable<FleetViewKPIStatusIndicator> GetFleetView(long vesselGroupId,long loginUserId)
        {
            var userrole = _DbContext.Roles.Where(x => x.RoleId == (_DbContext.UserRoleMapping.Where(y => y.UserId == loginUserId).Select(y => y.RoleId).FirstOrDefault())).Select(x => x.RoleName).AsNoTracking().FirstOrDefault();
            var vesselgroup = (from uvgm in _DbContext.UserVesselGroupMappings.AsEnumerable()
                               where uvgm.UserId == loginUserId
                               select uvgm.VesselGroupId).ToList();      ///remove inner join
            if (vesselgroup.Contains(vesselGroupId) || userrole.ToLower() == Constant.administrator)
            {
                return _DbContext.Query<FleetViewKPIStatusIndicator>().FromSql(string.Format("getFleetViewStatusIndicator {0}", vesselGroupId)).ToList();
            }
            else
            {
                return null;
            }
        }
        public FleetPassageLatestReportViewModel GetPassageDataLatestReport(long vesselId, long loginUserId)
        {
            var userrole = _DbContext.Roles.Where(x => x.RoleId == (_DbContext.UserRoleMapping.Where(y => y.UserId == loginUserId).Select(y => y.RoleId).FirstOrDefault())).Select(x => x.RoleName).AsNoTracking().FirstOrDefault();
            var vesselIDs = (from u in _DbContext.UserVesselGroupMappings.AsEnumerable()
                           join vu in _DbContext.VesselGroupVesselMapping.AsEnumerable() on u.VesselGroupId equals vu.VesselGroupId
                           join vsl in _DbContext.Vessels.AsEnumerable() on vu.VesselId equals vsl.SFPM_VesselId
                           where u.UserId == loginUserId
                             select vsl.SFPM_VesselId).ToList();

            if (vesselIDs.Contains(vesselId) || userrole.ToLower() == Constant.administrator)
                return _DbContext.Query<FleetPassageLatestReportViewModel>().FromSql(string.Format("getFleetViewPassageData {0},{1}", vesselId, "passagedata")).FirstOrDefault();
            else
                return null;
        }

        public List<FleetPassageDataViewModel> GetPassageData(long vesselId, long loginUserId)
        {
            var userrole = _DbContext.Roles.Where(x => x.RoleId == (_DbContext.UserRoleMapping.Where(y => y.UserId == loginUserId).Select(y => y.RoleId).FirstOrDefault())).Select(x => x.RoleName).AsNoTracking().FirstOrDefault();
            var vesselIDs = (from u in _DbContext.UserVesselGroupMappings.AsEnumerable()
                           join vu in _DbContext.VesselGroupVesselMapping.AsEnumerable() on u.VesselGroupId equals vu.VesselGroupId
                           join vsl in _DbContext.Vessels.AsEnumerable() on vu.VesselId equals vsl.SFPM_VesselId
                           where u.UserId == loginUserId
                             select vsl.SFPM_VesselId).ToList();

            if (vesselIDs.Contains(vesselId) || userrole.ToLower() == Constant.administrator)
                return _DbContext.Query<FleetPassageDataViewModel>().FromSql(string.Format("getFleetViewPassageData {0},{1}", vesselId, "passage")).ToList();
            else
                return null;
        }

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
    }
}
