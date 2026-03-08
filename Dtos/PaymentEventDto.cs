namespace HospitalApi.Dtos
{
    public class PaymentEventDto
    {
        public int Id { get; set; }
        public Guid EventId { get; set; }
        public string EventType { get; set; } = null!;
        public DateTime OccurredAt { get; set; }

        public int BillId { get; set; }
        public string InvoiceNumber { get; set; } = null!;

        public decimal PaidAmount { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal RemainingBalance { get; set; }
        public string PaymentMethod { get; set; } = null!;
        public string? TransactionReference { get; set; }
        public string PaymentStatus { get; set; } = null!;

        public int PatientId { get; set; }
        public string PatientName { get; set; } = null!;
        public int DoctorId { get; set; }
        public string DoctorName { get; set; } = null!;
        public int AppointmentId { get; set; }
        public int PaidByUserId { get; set; }

        public DateTime ConsumedAt { get; set; }
    }
}