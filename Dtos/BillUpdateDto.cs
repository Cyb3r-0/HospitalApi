namespace HospitalApi.Dtos
{
    public class BillUpdateDto
    {
        public decimal ConsultationFee { get; set; }
        public decimal MedicineFee { get; set; }
        public decimal OtherCharges { get; set; }
        public decimal Discount { get; set; }
        public decimal TaxAmount { get; set; }
        public DateTime DueDate { get; set; }
        public string? Notes { get; set; }
    }
}