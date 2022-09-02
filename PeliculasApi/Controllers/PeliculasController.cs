using AutoMapper;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PeliculasApi.DTOs;
using PeliculasApi.Entities;
using PeliculasApi.Helpers;
using PeliculasApi.Services;

namespace PeliculasApi.Controllers
{
    [ApiController]
    [Route("api/peliculas")]
    public class PeliculasController: ControllerBase
    {
        private readonly ApplicationDbContext context;
        private readonly IMapper mapper;
        private readonly IAlmacenadorArchivos almacenadorArchivos;
        private readonly string contenedor = "peliculas";

        public PeliculasController(ApplicationDbContext context,
            IMapper mapper, IAlmacenadorArchivos almacenadorArchivos)
        {
            this.context = context;
            this.mapper = mapper;
            this.almacenadorArchivos = almacenadorArchivos;
        }

        [HttpGet]
        public async Task<ActionResult<PeliculasIndexDTO>> Get()
        {
            var top = 5;

            var hoy = DateTime.Today;

            var proximosEstrenos = await context.Peliculas
                .Where(x => x.FechaEstreno > hoy)
                .OrderBy(x => x.FechaEstreno)
                .Take(top)
                .ToListAsync();

            var enCines = await context.Peliculas
                .Where(x => x.EnCines)
                .Take(top)
                .ToListAsync();

            var resultado = new PeliculasIndexDTO();

            resultado.FuturosEstrenos = mapper.Map<List<PeliculaDTO>>(proximosEstrenos);

            resultado.EnCines = mapper.Map<List<PeliculaDTO>>(enCines);

            return resultado;
        }

        [HttpGet("filtro")]
        public async Task<ActionResult<List<PeliculaDTO>>> Filtrar([FromQuery] FiltroPeliculasDTO filtroPeliculasDTO)
        {
            var peliculasQueryable = context.Peliculas.AsQueryable();

            if (!string.IsNullOrEmpty(filtroPeliculasDTO.Titulo))
            {
                peliculasQueryable = peliculasQueryable.Where(x => x.Titulo.Contains(filtroPeliculasDTO.Titulo));
            }

            if(filtroPeliculasDTO.EnCines)
            {
                peliculasQueryable = peliculasQueryable.Where(x => x.EnCines);
            }

            if (filtroPeliculasDTO.ProximosEstrenos)
            {
                var hoy = DateTime.Today;

                peliculasQueryable = peliculasQueryable.Where(x => x.FechaEstreno > hoy);
            }

            if (filtroPeliculasDTO.GeneroId !=0)
            {
                peliculasQueryable = peliculasQueryable
                    .Where(x => x.PeliculasGeneros.Select(y => y.GeneroId)
                    .Contains(filtroPeliculasDTO.GeneroId));
            }

            await HttpContext.InsertarParametrosPaginacion(peliculasQueryable,
                filtroPeliculasDTO.CantidadRegistrosPorPagina);

            var peliculas = await peliculasQueryable.Paginar(filtroPeliculasDTO.Paginacion).ToListAsync();

            return mapper.Map<List<PeliculaDTO>>(peliculas);
        }

        [HttpGet("{id}", Name = "obtenerPelicula")]
        public async Task<ActionResult<PeliculasDetallesDTO>> GetId(int id)
        {
            var pelicula = await context.Peliculas
                .Include(x  => x.PeliculasActores).ThenInclude(x=> x.Actor)
                .Include(x => x.PeliculasGeneros).ThenInclude(x => x.Genero)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (pelicula == null)
            {
                return NotFound();
            }

            pelicula.PeliculasActores = pelicula.PeliculasActores.OrderBy(x => x.Orden).ToList();

            return mapper.Map<PeliculasDetallesDTO>(pelicula);
        }

        [HttpPost]
        public async Task<ActionResult> Post([FromForm]PeliculaCreacionDTO peliculaCreacionDTO)
        {

            var pelicula = mapper.Map<Pelicula>(peliculaCreacionDTO);

            if (peliculaCreacionDTO.Poster != null)
            {
                using (var memoryStream = new MemoryStream())
                {
                    await peliculaCreacionDTO.Poster.CopyToAsync(memoryStream);

                    var contenido = memoryStream.ToArray();

                    var extension = Path.GetExtension(peliculaCreacionDTO.Poster.FileName);

                    pelicula.Poster = await almacenadorArchivos.GuardarArchivo(contenido, extension, contenedor,
                        peliculaCreacionDTO.Poster.ContentType);
                }
            }

            context.Add(pelicula);

            await context.SaveChangesAsync();

            var peliculaDTO = mapper.Map<PeliculaDTO>(pelicula);

            return new CreatedAtRouteResult("obtenerPelicula", new { id = pelicula.Id}, peliculaDTO);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult> Put([FromForm]PeliculaCreacionDTO peliculaCreacionDTO)
        {
            var pelicula = mapper.Map<PeliculaDTO>(peliculaCreacionDTO);


            if (peliculaCreacionDTO.Poster != null)
            {
                using (var memoryStream = new MemoryStream())
                {
                    await peliculaCreacionDTO.Poster.CopyToAsync(memoryStream);

                    var contenido = memoryStream.ToArray();

                    var extension = Path.GetExtension(peliculaCreacionDTO.Poster.FileName);

                    pelicula.Poster = await almacenadorArchivos.GuardarArchivo(contenido, extension, contenedor,
                        peliculaCreacionDTO.Poster.ContentType);
                }
            }

            await context.SaveChangesAsync();

            return NoContent();
        }

        [HttpPatch("{id}")]
        public async Task<ActionResult> Patch(int id, [FromBody] JsonPatchDocument<PeliculaPatchDTO> patchDocument)
        {
            if (patchDocument == null)
            {
                return BadRequest();
            }

            var entidadDB = await context.Peliculas.FirstOrDefaultAsync(x => x.Id == id);

            if (entidadDB == null)
            { return BadRequest(); }

            var entidadDTO = mapper.Map<PeliculaPatchDTO>(entidadDB);

            patchDocument.ApplyTo(entidadDTO, ModelState);

            var esValido = TryValidateModel(entidadDTO);

            if (!esValido)
            { return BadRequest(ModelState); }

            await context.SaveChangesAsync();

            return NoContent();

        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(int id)
        {
            var existe = await context.Peliculas.AnyAsync(x => x.Id == id);

            if (!existe)
            {
                return NotFound();
            }

            context.Remove(new Pelicula() { Id = id });

            await context.SaveChangesAsync();

            return NoContent();
        }

    }
}
