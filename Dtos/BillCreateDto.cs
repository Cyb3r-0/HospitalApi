using HospitalApi.Models;

namespace HospitalApi.Dtos
{
    public class BillCreateDto
    {
        public int AppointmentId { get; set; }
        public decimal ConsultationFee { get; set; }
        public decimal MedicineFee { get; set; } = 0;
        public decimal OtherCharges { get; set; } = 0;
        public decimal Discount { get; set; } = 0;
        public decimal TaxAmount { get; set; } = 0;
        public DateTime DueDate { get; set; }
        public string? Notes { get; set; }
    }
}