﻿using AutoMapper;
using PeliculasApi.DTOs;
using PeliculasApi.Entities;

namespace PeliculasApi.Helpers
{
    public class AutoMapperProfile: Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<Genero, GeneroDTO>().ReverseMap();
            CreateMap<GeneroCreacionDTO, Genero>();

            CreateMap<SalaDeCine, SalaDeCineDTO>().ReverseMap();
            CreateMap<SalaDeCineCreacionDTO, SalaDeCine>();

            CreateMap<Actor, ActorDTO>().ReverseMap();
            CreateMap<ActorCreacionDTO, Actor>().ForMember(x => x.Foto, options => options.Ignore());
            CreateMap<ActorPatchDTO, Actor>().ReverseMap();


            CreateMap<Pelicula, PeliculaDTO>().ReverseMap();
            CreateMap<PeliculaCreacionDTO, Pelicula>().ForMember(x => x.Poster, options => options.Ignore())
                .ForMember(x => x.PeliculasGeneros, options => options.MapFrom(MapPeliculasGeneros))
                .ForMember(x => x.PeliculasActores, options => options.MapFrom(MapPeliculasActores));

            CreateMap<Pelicula, PeliculasDetallesDTO>()
                .ForMember(x => x.Generos, options => options.MapFrom(MapPeliculasGeneros));

            CreateMap<PeliculaPatchDTO, Pelicula>().ReverseMap();
        }

        private List<GeneroDTO> MapPeliculasGeneros(Pelicula pelicula, PeliculasDetallesDTO peliculasDetallesDTO)
        {
            var resultado = new List<GeneroDTO>();

            if(pelicula.PeliculasGeneros == null) { return resultado; }

            foreach (var generoPelicula in pelicula.PeliculasGeneros)
            {
                resultado.Add(new GeneroDTO() { Id = generoPelicula.GeneroId, Nombre = generoPelicula.Genero.Nombre });
            }

            return resultado;
        }
        private List<PeliculasGeneros> MapPeliculasGeneros(PeliculaCreacionDTO peliculaCreacionDTO, Pelicula pelicula)
        {
            var resultado = new List<PeliculasGeneros>();

            if (peliculaCreacionDTO.GenerosIDs == null) { return resultado; }

            foreach (var id in peliculaCreacionDTO.GenerosIDs)
            {
                resultado.Add(new PeliculasGeneros() { GeneroId = id });
            }

            return resultado;
        }

        private List<PeliculasActores> MapPeliculasActores(PeliculaCreacionDTO peliculaCreacionDTO, Pelicula pelicula)
        {
            var resultado = new List<PeliculasActores>();

            if (peliculaCreacionDTO.Actores == null) { return resultado; }

            foreach (var actor in peliculaCreacionDTO.Actores)
            {
                resultado.Add(new PeliculasActores() { ActorId = actor.ActorId, Personaje = actor.Personaje });
            }

            return resultado;
        }
    }
}
