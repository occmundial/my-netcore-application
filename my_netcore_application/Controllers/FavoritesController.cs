using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using my_netcore_application.Entities;
using my_netcore_application.Interfaces;
using my_netcore_application.Models;
using Newtonsoft.Json;

namespace my_netcore_application.Controllers
{
    // https://aws.amazon.com/es/blogs/developer/configuring-aws-sdk-with-net-core/
    [Route("/users/{userId}/favorites-jobs")]
    public class FavoritesController : Controller
    {
        public FavoritesController(IFavoriteRepository favoriteRepository, IMapper mapper, IUrlHelper urlHelper)
        //public FavoritesController(IFavoriteRepository favoriteRepository, IMapper mapper)
        {
            _favoriteRepository = favoriteRepository;
            _mapper = mapper;
            _urlHelper = urlHelper;
        }

        private readonly IUrlHelper _urlHelper;
        private readonly IFavoriteRepository _favoriteRepository;
        private readonly IMapper _mapper;

        // Get a la instancia de jobs favoritos
        // con el "Name" me puedo referir a esta url
        // https://stackoverflow.com/questions/31695900/what-is-the-purpose-of-nameof
        [HttpGet("{jobId:int}", Name = nameof(GetFavoriteAsync))]
        public async Task<IActionResult> GetFavoriteAsync(string userId, int jobId)
        {
            var jobEntity = await _favoriteRepository.GetFavoriteByIdAsync(userId, jobId);

            if (jobEntity == null)
                return NotFound();

            var jobDto = _mapper.Map<FavoriteDto>(jobEntity);                

            return Ok(jobDto);
        }

        // Get al recurso de jobs favoritos
        [HttpGet(Name = nameof(GetFavoritesAsync))]
        public async Task<IActionResult> GetFavoritesAsync(string userId, int pageSize = 50, int? lastkey = null)
        {
            var jobsEntity = await _favoriteRepository.GetFavoritesAsync(userId, pageSize, jobId: lastkey);

            var jobsDto = _mapper.Map<IEnumerable<FavoriteDto>>(jobsEntity.Items);    

            // https://github.com/FabianGosebrink/ASPNETCore-Angular-Material-HATEOAS-Paging/blob/master/server/src/ASPNETCoreWebAPI/Controllers/CustomersController.cs
            var paginationMetadata = new
            {
                totalCount = jobsEntity.Items.Count,
                pageSize = pageSize
                //currentPage = queryParameters.Page,
                //totalPages = queryParameters.GetTotalPages(allItemCount)
            };

            Response.Headers.Add("X-Pagination", JsonConvert.SerializeObject(paginationMetadata));            

            return Ok(new { items = jobsDto, links = CreateLinks(userId, pageSize, lastkey, jobsEntity.LastJobId) });
        }

        // Delete al job favorito
        [HttpDelete("{jobId:int}")]
        public async Task<IActionResult> DeleteAsync(string userId, int jobId)
        {
            // aquí podríamos considerar que si no existe el elemento a borrar entonces todo Ok
            var jobEntity = await _favoriteRepository.GetFavoriteByIdAsync(userId, jobId);

            if (jobEntity == null)
                return NotFound();

            _favoriteRepository.DeleteFavoriteByIdAsync(userId, jobId);

            return NoContent();
        }

        // Creación del job favorito
        [HttpPost(Name = nameof(CreateFavoriteAsync))]
        public IActionResult CreateFavoriteAsync(string userId, [FromBody] FavoriteForCreationDto jobFavorite)
        {
            if (jobFavorite == null)
                return BadRequest();

            if (!ModelState.IsValid) 
                return BadRequest(ModelState);

            // mapeo el objeto DTO de entrada "jobFavorite" a la entidad "Entities.FavoriteEntity"
            // para utilizar la entidad para el almacenamiento de la información en DB
            var finalFavorite = _mapper.Map<FavoriteEntity>(jobFavorite);

            finalFavorite.UserId = userId;

            _favoriteRepository.CreateFavoriteAsync(finalFavorite);

            // mapeo la entidad que se guardó en la DB (FavoriteEntity) como el objeto DTO de respuesta (FavoriteDto)
            var createdFavoriteToReturn = _mapper.Map<FavoriteDto>(finalFavorite);

            // regresa un Created (201), el objeto recién creado y la llave "Location" en
            // el header indicando donde se puede obtener el recurso recién creado
            return CreatedAtRoute(
                nameof(GetFavoriteAsync), // referencia al "Name" del método "GetFavoriteAsync" para obtener la url
                new { userId = userId, jobId = finalFavorite.JobId }, // clase anónima con las instancias de la url
                createdFavoriteToReturn); // objeto recién creado
        }

        // Creamos los links del paginado con HATEOAS
        // https://offering.solutions/blog/articles/2017/11/29/crud-operations-angular-with-aspnetcore-hateoas/
        // https://github.com/FabianGosebrink/ASPNETCore-Angular-Material-HATEOAS-Paging
        private List<LinkDto> CreateLinks(string userId, int pageSize, int? lastjobIdIn = null,
            int? lastjobIdOut = null)
        {
            var links = new List<LinkDto>();

            links.Add(
                new LinkDto(_urlHelper.Link(nameof(CreateFavoriteAsync), 
                    new
                    {
                        userId = userId
                    }), 
                "create", 
                "POST"));
 
            // self 
            links.Add(
                 new LinkDto(_urlHelper.Link(nameof(GetFavoritesAsync),
                    new
                    {
                        userId = userId,
                        pageSize = pageSize,
                        lastkey = lastjobIdIn
                    }), 
                "self", 
                "GET"));

            if (lastjobIdOut != null)
            {
                links.Add(
                    new LinkDto(_urlHelper.Link(nameof(GetFavoritesAsync), 
                        new
                        {
                            userId = userId,
                            pageSize = pageSize,
                            lastkey = lastjobIdOut
                    }), 
                    "next", 
                    "GET"));
            }

            return links;
        }        

    }
}
