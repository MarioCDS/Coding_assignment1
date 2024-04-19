namespace Domain.Models
{
    //Class that holds the result from price calculator
    public class PriceCalculationModel
    {
        public decimal Subtotal { get; set; }
        public decimal Total { get; set; }
        public string OffersApplied { get; set; } = string.Empty;
    }
}
