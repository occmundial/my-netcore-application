using System.ComponentModel.DataAnnotations;

namespace my_netcore_application.Models
{
    public class FavoriteForCreationDto
    {
        [Required(ErrorMessage = "You should provide a jobId.")]
        public int JobId {get; set;}   

        [Required(ErrorMessage = "You should provide a job title.")]
        [MaxLength(200)]
        public string JobTitle { get; set; }                
    }
}