using TicketVendorMachine.Models;

namespace TicketVendorMachine.ViewModels
{
    public class SelectRouteViewModel
    {
        public List<string> TransportTypes { get; set; } = new();
        public List<TransportRoute> Routes { get; set; } = new();
        public string? SelectedTransport { get; set; }
        public string? SelectedPassengerType { get; set; }
        public int? SelectedRouteId { get; set; }
    }

    public class ConfirmTicketViewModel
    {
        public int RouteId { get; set; }
        public string TransportType { get; set; } = "";
        public string PassengerType { get; set; } = "";
        public string Origin { get; set; } = "";
        public string Destination { get; set; } = "";
        public decimal Price { get; set; }
    }

    public class SelectPaymentViewModel
    {
        public int TicketId { get; set; }
        public decimal Amount { get; set; }
        public List<string> PaymentMethods { get; set; } = new() { "QRCode", "CreditCard", "Cash" };
        public List<string> QRProviders { get; set; } = new() { "MoMo", "ZaloPay", "VNPay" };
    }

    public class PaymentViewModel
    {
        public int TicketId { get; set; }
        public string PaymentMethod { get; set; } = "";
        public string? Provider { get; set; }
        public decimal Amount { get; set; }
        public string? QRCodeBase64 { get; set; }
        public string? TransactionCode { get; set; }
    }

    public class PaymentResultViewModel
    {
        public bool IsSuccess { get; set; }
        public Ticket? Ticket { get; set; }
        public Transaction? Transaction { get; set; }
        public string? ErrorMessage { get; set; }
    }
}
