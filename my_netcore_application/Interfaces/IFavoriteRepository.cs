using System.Threading.Tasks;
using my_netcore_application.Entities;

namespace my_netcore_application.Interfaces
{
    public interface IFavoriteRepository
    {
        Task<FavoriteEntity> GetFavoriteByIdAsync(string userId, int jobId);
        void DeleteFavoriteByIdAsync(string userId, int jobId);
        void CreateFavoriteAsync(FavoriteEntity favoriteItem);
        Task<FavoritesListEntity> GetFavoritesAsync(string userId, int pageSize = 50, int? jobId = null);    
    }
}