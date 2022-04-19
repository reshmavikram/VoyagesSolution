using Data.Solution.Models;
using Data.Solution.Models.ViewModels;
using System.Collections.Generic;
using VoyagesAPIService.Infrastructure.Repositories;
using VoyagesAPIService.Infrastructure.Services.Interfaces;

namespace VoyagesAPIService.Infrastructure.Services
{
    public class EmsService : IEmsService
    {
        protected EmsRepository _emsrepository;
   
        public EmsService(DatabaseContext databaseContext, UserContext context)
        {
            _emsrepository = new EmsRepository(databaseContext, context);
        }


        public IEnumerable<Voyages> GetAllVoyagesByVesselAndYear(string imoNumber, long userId, string year)
        {
            return _emsrepository.GetAllVoyagesByVesselAndYear(imoNumber, userId, year);
        }

        public IEnumerable<Forms> GetAllVesselsByYear()
        {
            return _emsrepository.GetAllVesselsByYear();
        }




    }
}
