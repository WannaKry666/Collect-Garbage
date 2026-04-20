using Microsoft.AspNetCore.Http;
using GarbageCollection.Common.Enums;
using System.ComponentModel.DataAnnotations;

namespace GarbageCollection.Common.DTOs.WasteReport
{
    public class UpdateWasteReportDto
    {
        public IList<WasteType>? Type { get; set; }

        public WasteSize? Size { get; set; }

        [MaxLength(500, ErrorMessage = "Mô tả không vượt quá 500 ký tự.")]
        public string? Description { get; set; }

        public IList<IFormFile>? Images { get; set; }
    }
}
