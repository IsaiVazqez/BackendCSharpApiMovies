using Microsoft.AspNetCore.Mvc;
using PeliculasApi.Helpers;
using PeliculasApi.Validations;
using System.ComponentModel.DataAnnotations;

namespace PeliculasApi.DTOs
{
    public class PeliculaCreacionDTO: PeliculaPatchDTO
    {

        [PesoArchivoValidacion(pesoMaximoEnMegaBytes: 4)]
        [TipoArchivoValidacion(GrupoTipoArchivo.Imagen)]
        public IFormFile Poster { get; set; }

        [ModelBinder(BinderType = typeof(TypeBlinder<List<int>>))]
        public List<int> GenerosIDs { get; set; }

        [ModelBinder(BinderType = typeof(TypeBlinder<List<ActorPeliculasCreacionDTO>>))]
        public List<ActorPeliculasCreacionDTO> Actores { get; set; }
    }
}
