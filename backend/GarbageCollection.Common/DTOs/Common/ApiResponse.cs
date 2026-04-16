using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace GarbageCollection.Common.DTOs.Common
{
    public sealed class ApiResponse<T>
    {
        [JsonPropertyName("status")]
        public string Status { get; set; } = null!;

        [JsonPropertyName("message")]
        public string Message { get; set; } = null!;

        [JsonPropertyName("data")]
        public T? Data { get; set; }

        [JsonPropertyName("error")]
        public ApiError? Error { get; set; }

        // SUCCESS
        public static ApiResponse<T> Success(string message, T data) => new()
        {
            Status = "success",
            Message = message,
            Data = data,
            Error = null
        };

        // FAIL
        public static ApiResponse<T> Fail(string message, string code, string description) => new()
        {
            Status = "failed",
            Message = message,
            Data = default,
            Error = new ApiError
            {
                Code = code,
                Description = description
            }
        };
    }
}
