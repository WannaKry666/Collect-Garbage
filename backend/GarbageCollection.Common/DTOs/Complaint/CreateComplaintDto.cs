using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace GarbageCollection.Common.DTOs.Complaint
{
    public class CreateComplaintDto
    {
        [Required(ErrorMessage = "title is required.")]
        [MaxLength(255)]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "description is required.")]
        public string Description { get; set; } = string.Empty;

        public IList<IFormFile> Images { get; set; } = [];
    }
}
