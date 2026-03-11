using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HospitalApi.Models
{
    public class PaymentEvent
    {
        public int Id { get; set; }

        public Guid EventId { get; set; }           // unique Kafka event ID
        public string EventType { get; set; } = null!;
        public DateTime OccurredAt { get; set; }

        public int BillId { get; set; }

        [StringLength(50)]
        public string InvoiceNumber { get; set; } = null!;

        [Column(TypeName = "decimal(10,2)")]
        public decimal PaidAmount { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal TotalAmount { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal RemainingBalance { get; set; }

        [StringLength(50)]
        public string PaymentMethod { get; set; } = null!;

        [StringLength(100)]
        public string? TransactionReference { get; set; }

        [StringLength(50)]
        public string PaymentStatus { get; set; } = null!;

        public int PatientId { get; set; }

        [StringLength(100)]
        public string PatientName { get; set; } = null!;

        public int DoctorId { get; set; }

        [StringLength(100)]
        public string DoctorName { get; set; } = null!;

        public int AppointmentId { get; set; }
        public int PaidByUserId { get; set; }

        // Raw event JSON for full audit trail
        public string RawPayload { get; set; } = null!;

        public DateTime ConsumedAt { get; set; } = DateTime.UtcNow;
    }
}