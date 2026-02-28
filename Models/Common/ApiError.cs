namespace HospitalApi.Models.Common
{
    public class ApiError
    {
        public int StatusCode { get; init; }
        public string Message { get; set; } = string.Empty;
        public string? Details { get; set; }
    }
}