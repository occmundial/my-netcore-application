using AutoMapper;

namespace my_netcore_application.Services
{
    public class MappingProfile : Profile 
    {
        public MappingProfile() 
        {
            // Add as many of these lines as you need to map your objects
            // configuramos el AutoMapper para indicarle como convertir nuestras entidades en objetos DTOs
            // ---------------------------------------------------------------------------------------
            // AutoMapper de manera automática hace coincidir las propiedades con el mismo nombre desde
            // el objeto fuente hasta el objeto destino, en el caso de que no exista la propiedad la ignora
            CreateMap<Models.FavoriteForCreationDto, Entities.FavoriteEntity>();
            CreateMap<Entities.FavoriteEntity, Models.FavoriteDto>();
        }
    }
}