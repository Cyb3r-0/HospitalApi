using HospitalApi.Models;

namespace HospitalApi.Dtos
{
    public class BillPayDto
    {
        public decimal PaidAmount { get; set; }
        public PaymentMethod PaymentMethod { get; set; }
        public string? TransactionReference { get; set; }
    }
    //LAST WAS THIS
}