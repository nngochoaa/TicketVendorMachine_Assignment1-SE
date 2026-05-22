using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TicketVendorMachine.Data;
using TicketVendorMachine.Services;
using TicketVendorMachine.ViewModels;

namespace TicketVendorMachine.Controllers
{
    public class TicketController : Controller
    {
        private readonly AppDbContext _db;
        private readonly ITicketService _ticketService;

        public TicketController(AppDbContext db, ITicketService ticketService)
        {
            _db = db;
            _ticketService = ticketService;
        }

        // Step 1: Select transport & destination
        public async Task<IActionResult> SelectRoute(string? transport)
        {
            var routes = await _db.Routes.ToListAsync();
            var vm = new SelectRouteViewModel
            {
                TransportTypes = routes.Select(r => r.TransportType).Distinct().ToList(),
                Routes = string.IsNullOrEmpty(transport) ? routes : routes.Where(r => r.TransportType == transport).ToList(),
                SelectedTransport = transport
            };
            return View(vm);
        }

        // Step 2: Select ticket type & confirm
        [HttpGet]
        public async Task<IActionResult> Confirm(int routeId, string passengerType = "Normal")
        {
            var route = await _db.Routes.FindAsync(routeId);
            if (route == null) return RedirectToAction("SelectRoute");

            var vm = new ConfirmTicketViewModel
            {
                RouteId = route.Id,
                TransportType = route.TransportType,
                PassengerType = passengerType,
                Origin = route.Origin,
                Destination = route.Destination,
                Price = passengerType == "Student" ? route.StudentPrice : route.Price
            };
            return View(vm);
        }

        // Step 3: Select payment method
        [HttpGet]
        public async Task<IActionResult> SelectPayment(int routeId, string passengerType = "Normal")
        {
            var route = await _db.Routes.FindAsync(routeId);
            if (route == null) return RedirectToAction("SelectRoute");

            var price = passengerType == "Student" ? route.StudentPrice : route.Price;
            var vm = new SelectPaymentViewModel
            {
                TicketId = 0,
                Amount = price
            };

            TempData["RouteId"] = routeId;
            TempData["PassengerType"] = passengerType;
            TempData["Amount"] = price.ToString();

            return View(vm);
        }

        // Step 4: Process payment
        [HttpPost]
        public async Task<IActionResult> Payment(string paymentMethod, string? provider)
        {
            var routeId = (int)(TempData["RouteId"] ?? 0);
            var passengerType = TempData["PassengerType"]?.ToString() ?? "Normal";
            var amountStr = TempData["Amount"]?.ToString() ?? "0";
            var amount = decimal.Parse(amountStr);

            if (routeId == 0) return RedirectToAction("SelectRoute");

            var ticket = await _ticketService.CreateTicketAsync(routeId, passengerType, paymentMethod);
            var transaction = await _ticketService.ProcessPaymentAsync(ticket.Id, provider ?? paymentMethod, amount);

            var vm = new PaymentViewModel
            {
                TicketId = ticket.Id,
                PaymentMethod = paymentMethod,
                Provider = provider,
                Amount = amount,
                TransactionCode = transaction.TransactionCode
            };

            if (paymentMethod == "QRCode" && !string.IsNullOrEmpty(provider))
            {
                vm.QRCodeBase64 = await _ticketService.GenerateQRCodeAsync(ticket.Id, provider);
            }

            return View(vm);
        }

        // Step 5: Confirm payment & show result
        [HttpPost]
        public async Task<IActionResult> ConfirmPayment(int ticketId, string transactionCode)
        {
            var isSuccess = await _ticketService.ConfirmPaymentAsync(ticketId, transactionCode);
            var ticket = await _ticketService.GetTicketAsync(ticketId);
            var transaction = _db.Transactions.FirstOrDefault(t => t.TransactionCode == transactionCode);

            var vm = new PaymentResultViewModel
            {
                IsSuccess = isSuccess,
                Ticket = ticket,
                Transaction = transaction,
                ErrorMessage = isSuccess ? null : "Thanh toán thất bại. Vui lòng thử lại."
            };

            return View("PaymentResult", vm);
        }

        // Simulate payment confirmation (for Cash/Card)
        [HttpPost]
        public async Task<IActionResult> SimulatePayment(int ticketId, string transactionCode)
        {
            return await ConfirmPayment(ticketId, transactionCode);
        }

        public IActionResult PrintTicket(int ticketId)
        {
            var ticket = _db.Tickets.Find(ticketId);
            if (ticket == null) return NotFound();
            return View(ticket);
        }
    }
}
