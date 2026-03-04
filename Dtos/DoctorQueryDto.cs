namespace HospitalApi.Dtos
{
    public class DoctorQueryDto
    {
        private const int MaxPageSize = 50;

        public int Page { get; set; } = 1;

        private int _pageSize = 10;
        public int PageSize
        {
            get => _pageSize;
            set => _pageSize = value > MaxPageSize ? MaxPageSize : value;
        }

        // Filter by specialization e.g. ?specialization=cardiology
        public string? Specialization { get; set; }

        // Filter by availability e.g. ?isAvailable=true
        public bool? IsAvailable { get; set; }
    }
}
