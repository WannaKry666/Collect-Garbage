using System.ComponentModel.DataAnnotations;

namespace GarbageCollection.Common.DTOs.Complaint
{
    public class SendComplaintMessageDto
    {
        [Required]
        public string Message { get; set; } = string.Empty;
    }
}
