namespace HospitalApi.Events
{
    public class PaymentMadeEvent
    {
        public Guid EventId { get; set; } = Guid.NewGuid();
        public string EventType { get; set; } = "PaymentMade";
        public DateTime OccurredAt { get; set; } = DateTime.UtcNow;

        // Bill info
        public int BillId { get; set; }
        public string InvoiceNumber { get; set; } = null!;

        // Payment info
        public decimal PaidAmount { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal RemainingBalance { get; set; }
        public string PaymentMethod { get; set; } = null!;
        public string? TransactionReference { get; set; }
        public string PaymentStatus { get; set; } = null!;

        // Context
        public int PatientId { get; set; }
        public string PatientName { get; set; } = null!;
        public int DoctorId { get; set; }
        public string DoctorName { get; set; } = null!;
        public int AppointmentId { get; set; }
        public int PaidByUserId { get; set; }
    }
}