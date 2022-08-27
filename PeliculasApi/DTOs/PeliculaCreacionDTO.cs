using PeliculasApi.Validations;
using System.ComponentModel.DataAnnotations;

namespace PeliculasApi.DTOs
{
    public class PeliculaCreacionDTO: PeliculaPatchDTO
    {

        [PesoArchivoValidacion(pesoMaximoEnMegaBytes: 4)]
        [TipoArchivoValidacion(GrupoTipoArchivo.Imagen)]
        public IFormFile Poster { get; set; }
        public List<int> GenerosIDs { get; set; }
    }
}
