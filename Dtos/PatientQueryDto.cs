namespace HospitalApi.Dtos
{
    public class PatientQueryDto
    {
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 100;
        public string? Disease { get; set; }
    }
}
