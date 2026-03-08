using HospitalApi.Models;

namespace HospitalApi.Dtos
{
    public class BillDto
    {
        public int Id { get; set; }
        public string InvoiceNumber { get; set; } = null!;

        public int AppointmentId { get; set; }
        public int PatientId { get; set; }
        public string PatientName { get; set; } = null!;
        public int DoctorId { get; set; }
        public string DoctorName { get; set; } = null!;

        public decimal ConsultationFee { get; set; }
        public decimal MedicineFee { get; set; }
        public decimal OtherCharges { get; set; }
        public decimal Discount { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal PaidAmount { get; set; }
        public decimal BalanceDue => TotalAmount - PaidAmount;

        public PaymentStatus PaymentStatus { get; set; }
        public string PaymentStatusName => PaymentStatus.ToString();
        public PaymentMethod? PaymentMethod { get; set; }
        public string? PaymentMethodName => PaymentMethod?.ToString();

        public DateTime DueDate { get; set; }
        public DateTime? PaidAt { get; set; }
        public string? TransactionReference { get; set; }
        public string? Notes { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}