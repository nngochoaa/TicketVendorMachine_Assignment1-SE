namespace TicketVendorMachine.Models
{
    public class TransportRoute
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public string TransportType { get; set; } = ""; // Bus, MRT, Metro
        public string Origin { get; set; } = "";
        public string Destination { get; set; } = "";
        public decimal Price { get; set; }
        public decimal StudentPrice { get; set; }
    }

    public class Ticket
    {
        public int Id { get; set; }
        public string TicketCode { get; set; } = "";
        public string PassengerType { get; set; } = "Normal"; // Normal, Student
        public string TransportType { get; set; } = "";
        public string Origin { get; set; } = "";
        public string Destination { get; set; } = "";
        public decimal Price { get; set; }
        public string PaymentMethod { get; set; } = ""; // QRCode, CreditCard, Cash
        public string PaymentStatus { get; set; } = "Pending"; // Pending, Success, Failed
        public string? QRCodeBase64 { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? PaidAt { get; set; }
    }

    public class Transaction
    {
        public int Id { get; set; }
        public string TransactionCode { get; set; } = "";
        public int TicketId { get; set; }
        public decimal Amount { get; set; }
        public string PaymentProvider { get; set; } = ""; // MoMo, ZaloPay, VNPay, Bank, Cash
        public string Status { get; set; } = "Pending";
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public Ticket? TicketNav { get; set; }
    }
}
