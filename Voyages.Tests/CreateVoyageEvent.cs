using Data.Solution.Models;
using Master.Tests;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Voyage.Tests;

namespace Voyages.Tests
{
    [TestFixture]
    public class CreateVoyageEvent:IDisposable
    {
        public DatabaseContext _context;
        SetDatabaseContext dbcontext = new SetDatabaseContext();
        public void Dispose()
        {
            _context.Dispose();
        }

       
    }
}
