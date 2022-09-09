using AutoMapper;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite;
using PeliculasApi;
using PeliculasApi.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PeliculasApiTest
{
    public class BasePruebas
    {
        protected ApplicationDbContext ConstruirContext(string nombreDb)
        {
            var opciones = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(nombreDb).Options;

            var dbContext = new ApplicationDbContext(opciones);

            return dbContext;
        }

        protected IMapper ConfigurarAutoMapper()
        {
            var config = new MapperConfiguration(options =>
            {
                var geometryFactory = NtsGeometryServices.Instance.CreateGeometryFactory(srid: 4326);
                options.AddProfile(new AutoMapperProfile(geometryFactory));
            });

            return config.CreateMapper();
        }
    }
}
