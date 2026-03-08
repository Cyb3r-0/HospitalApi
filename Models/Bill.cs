using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HospitalApi.Models
{
    public class Bill
    {
        public int Id { get; set; }

        // --- Relationships ---
        public int AppointmentId { get; set; }
        public Appointment Appointment { get; set; } = null!;

        public int PatientId { get; set; }
        public Patient Patient { get; set; } = null!;

        public int DoctorId { get; set; }
        public Doctor Doctor { get; set; } = null!;

        // --- Invoice details ---
        [StringLength(50)]
        public string InvoiceNumber { get; set; } = null!;  // e.g. INV-20250401-001

        [Column(TypeName = "decimal(10,2)")]
        public decimal ConsultationFee { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal MedicineFee { get; set; } = 0;

        [Column(TypeName = "decimal(10,2)")]
        public decimal OtherCharges { get; set; } = 0;

        [Column(TypeName = "decimal(10,2)")]
        public decimal Discount { get; set; } = 0;          // flat discount amount

        [Column(TypeName = "decimal(10,2)")]
        public decimal TaxAmount { get; set; } = 0;         // GST / VAT

        [Column(TypeName = "decimal(10,2)")]
        public decimal TotalAmount { get; set; }             // computed on create

        [Column(TypeName = "decimal(10,2)")]
        public decimal PaidAmount { get; set; } = 0;        // for partial payments

        // --- Payment ---
        public PaymentStatus PaymentStatus { get; set; } = PaymentStatus.Pending;
        public PaymentMethod? PaymentMethod { get; set; }   // null until paid

        public DateTime? PaidAt { get; set; }               // null until paid
        public DateTime DueDate { get; set; }               // payment deadline

        [StringLength(100)]
        public string? TransactionReference { get; set; }   // bank ref / receipt no

        // --- Notes ---
        [StringLength(500)]
        public string? Notes { get; set; }

        // --- Soft delete ---
        public bool IsDeleted { get; set; } = false;
        public DateTime? DeletedAt { get; set; }

        // --- Audit ---
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        public int? CreatedByUserId { get; set; }
        public User? CreatedByUser { get; set; }

        public int? UpdatedByUserId { get; set; }
        public User? UpdatedByUser { get; set; }
    }
}