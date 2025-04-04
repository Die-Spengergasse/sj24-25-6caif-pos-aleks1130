namespace SPG_Fachtheorie.Aufgabe1.Commands
{
    public class NewPaymentItemCommand
    {
        public string ArticleName { get; set; } = string.Empty;
        public int Amount { get; set; }
        public decimal Price { get; set; }
        public int PaymentId { get; set; }
    }
}
