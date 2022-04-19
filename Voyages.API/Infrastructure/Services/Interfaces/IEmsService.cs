using Data.Solution.Models;
using Data.Solution.Models.ViewModels;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;

namespace VoyagesAPIService.Infrastructure.Services.Interfaces
{
    public interface IEmsService
    {
        IEnumerable<Voyages> GetAllVoyagesByVesselAndYear(string imoNumber, long userId, string year);

        IEnumerable<Forms> GetAllVesselsByYear();





    }
}
