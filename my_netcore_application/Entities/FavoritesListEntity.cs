using System.Collections.Generic;

namespace my_netcore_application.Entities
{
    public class FavoritesListEntity
    {
        public List<FavoriteEntity> Items { get; set; }
        public bool HasNext { get { return !string.IsNullOrWhiteSpace(LastUserId) && LastJobId != null;}}
        public string LastUserId { get; set; }
        public int? LastJobId { get; set; }
    }
}