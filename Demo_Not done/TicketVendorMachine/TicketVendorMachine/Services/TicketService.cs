using QRCoder;
using TicketVendorMachine.Data;
using TicketVendorMachine.Models;

namespace TicketVendorMachine.Services
{
    public interface ITicketService
    {
        Task<Ticket> CreateTicketAsync(int routeId, string passengerType, string paymentMethod);
        Task<Ticket?> GetTicketAsync(int ticketId);
        Task<string> GenerateQRCodeAsync(int ticketId, string provider);
        Task<Transaction> ProcessPaymentAsync(int ticketId, string provider, decimal amount);
        Task<bool> ConfirmPaymentAsync(int ticketId, string transactionCode);
    }

    public class TicketService : ITicketService
    {
        private readonly AppDbContext _db;
        private readonly IHttpClientFactory _httpClientFactory;

        // BIDV account info
        private const string BankBin = "970418"; // BIDV BIN
        private const string AccountNo = "8873031519";
        private const string AccountName = "SMART TVM";

        public TicketService(AppDbContext db, IHttpClientFactory httpClientFactory)
        {
            _db = db;
            _httpClientFactory = httpClientFactory;
        }

        public async Task<Ticket> CreateTicketAsync(int routeId, string passengerType, string paymentMethod)
        {
            var route = await _db.Routes.FindAsync(routeId)
                ?? throw new Exception("Route not found");

            var price = passengerType == "Student" ? route.StudentPrice : route.Price;

            var ticket = new Ticket
            {
                TicketCode = GenerateTicketCode(),
                PassengerType = passengerType,
                TransportType = route.TransportType,
                Origin = route.Origin,
                Destination = route.Destination,
                Price = price,
                PaymentMethod = paymentMethod,
                PaymentStatus = "Pending",
                CreatedAt = DateTime.Now
            };

            _db.Tickets.Add(ticket);
            await _db.SaveChangesAsync();
            return ticket;
        }

        public async Task<Ticket?> GetTicketAsync(int ticketId)
        {
            return await _db.Tickets.FindAsync(ticketId);
        }

        public async Task<string> GenerateQRCodeAsync(int ticketId, string provider)
        {
            var ticket = await _db.Tickets.FindAsync(ticketId)
                ?? throw new Exception("Ticket not found");

            string base64;

            if (provider == "VietQR" || provider == "BIDV" || provider == "BankTransfer")
            {
                // Dùng VietQR API - trả về ảnh QR trực tiếp
                base64 = await GenerateVietQRAsync((int)ticket.Price, ticket.TicketCode);
            }
            else if (provider == "MoMo")
            {
                // MoMo: tạo QR string chuẩn MoMo deeplink
                var momoContent = $"2|99|{AccountNo}|{AccountName}|TVM Ticket|{(int)ticket.Price}|0|{ticket.TicketCode}";
                base64 = GenerateLocalQR(momoContent);
            }
            else
            {
                // Fallback: generic QR
                var content = $"PAYMENT|{provider}|{ticket.TicketCode}|{ticket.Price}";
                base64 = GenerateLocalQR(content);
            }

            ticket.QRCodeBase64 = base64;
            await _db.SaveChangesAsync();

            return base64;
        }

        private async Task<string> GenerateVietQRAsync(int amount, string ticketCode)
        {
            try
            {
                // VietQR Quick Link - trả về ảnh PNG trực tiếp, không cần API key
                var addInfo = Uri.EscapeDataString($"Ve TVM {ticketCode}");
                var url = $"https://img.vietqr.io/image/{BankBin}-{AccountNo}-compact2.png" +
                          $"?amount={amount}&addInfo={addInfo}&accountName={Uri.EscapeDataString(AccountName)}";

                var client = _httpClientFactory.CreateClient();
                client.Timeout = TimeSpan.FromSeconds(10);
                var bytes = await client.GetByteArrayAsync(url);
                return Convert.ToBase64String(bytes);
            }
            catch
            {
                // Fallback nếu không có mạng: tạo QR local với chuỗi VietQR chuẩn
                var vietqrString = $"000201010211{BankBin.Length + AccountNo.Length + 4:D2}00{BankBin.Length:D2}{BankBin}{AccountNo.Length:D2}{AccountNo}5303704540{amount}5802VN6304";
                return GenerateLocalQR(vietqrString);
            }
        }

        private static string GenerateLocalQR(string content)
        {
            using var qrGenerator = new QRCodeGenerator();
            var qrData = qrGenerator.CreateQrCode(content, QRCodeGenerator.ECCLevel.Q);
            using var qrCode = new PngByteQRCode(qrData);
            var bytes = qrCode.GetGraphic(10);
            return Convert.ToBase64String(bytes);
        }

        public async Task<Transaction> ProcessPaymentAsync(int ticketId, string provider, decimal amount)
        {
            var transaction = new Transaction
            {
                TransactionCode = GenerateTransactionCode(),
                TicketId = ticketId,
                Amount = amount,
                PaymentProvider = provider,
                Status = "Pending",
                CreatedAt = DateTime.Now
            };

            _db.Transactions.Add(transaction);
            await _db.SaveChangesAsync();
            return transaction;
        }

        public async Task<bool> ConfirmPaymentAsync(int ticketId, string transactionCode)
        {
            var ticket = await _db.Tickets.FindAsync(ticketId);
            var transaction = _db.Transactions.FirstOrDefault(t => t.TransactionCode == transactionCode);

            if (ticket == null || transaction == null) return false;

            ticket.PaymentStatus = "Success";
            ticket.PaidAt = DateTime.Now;
            transaction.Status = "Success";

            await _db.SaveChangesAsync();
            return true;
        }

        private static string GenerateTicketCode()
            => $"TVM-{DateTime.Now:yyyyMMdd}-{Guid.NewGuid().ToString()[..8].ToUpper()}";

        private static string GenerateTransactionCode()
            => $"TXN-{DateTime.Now:yyyyMMddHHmmss}-{new Random().Next(1000, 9999)}";
    }
}
