using BulkyBook.DataAccess.Repository.IRepository;
using BulkyBook.Models;
using BulkyBook.Models.ViewModels;
using BulkyBook.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Stripe;
using Stripe.Checkout;
using Stripe.Issuing;
using System.Security.Claims;

namespace BulkyBookWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize] // order controller sadece yetkisi olan kişiler görebilmeli.
    public class OrderController : Controller
    {
        private readonly IUnitofWork _unitofWork;
        [BindProperty]
        public OrderVM orderVM { get; set; }
        public OrderController(IUnitofWork unitofWork)
        {
            _unitofWork = unitofWork;
        }

        public IActionResult Index()
        {
            
            return View();
        }

        public IActionResult Details(int orderId)
        {
            orderVM = new OrderVM()
            {
                OrderHeader = _unitofWork.OrderHeader.GetFİrstOrDefault(u => u.Id == orderId, includeProperties: "ApplicationUser"),
                OrderDetails = _unitofWork.OrderDetail.GetAll(u => u.Id == orderId, includeProperties: "Product"),

            };
            return View(orderVM);
        }

        [ActionName("Details")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Details_PAYNOW()
        {
            //stripe sayfasına yönlendiricez. Id details sayfasının başında var hidden olarak form post edildiği zaman oradan alıyoruz id'yi
            orderVM.OrderHeader = _unitofWork.OrderHeader.GetFİrstOrDefault(u => u.Id == orderVM.OrderHeader.Id, includeProperties: "ApplicationUser");
            orderVM.OrderDetails = _unitofWork.OrderDetail.GetAll(u => u.OrderId == orderVM.OrderHeader.Id, includeProperties: "Product");

            var domain = "https://localhost:44324/";
            var options = new SessionCreateOptions
            {
                LineItems = new List<SessionLineItemOptions>(),

                Mode = "payment",
                SuccessUrl = domain + $"admin/order/PaymentConfirmation?orderHeaderid={orderVM.OrderHeader.Id}",
                CancelUrl = domain + $"admin/order/details?orderId={orderVM.OrderHeader.Id}",
            };
            foreach (var item in orderVM.OrderDetails)
            {

                var sessionLineItem = new SessionLineItemOptions
                {
                    PriceData = new SessionLineItemPriceDataOptions
                    {
                        UnitAmount = (long)(item.Price * 100), // 20.00 --> 2000
                        Currency = "usd",
                        ProductData = new SessionLineItemPriceDataProductDataOptions
                        {
                            Name = item.Product.Title,
                        },
                    },
                    Quantity = item.Count,
                };
                options.LineItems.Add(sessionLineItem);
            }

            var service = new SessionService();
            Session session = service.Create(options);
            _unitofWork.OrderHeader.UpdateStripePaymentId(orderVM.OrderHeader.Id, session.Id, session.PaymentIntentId);
            _unitofWork.Save();
            Response.Headers.Add("Location", session.Url);
            return new StatusCodeResult(303);
        }

        public IActionResult PaymentConfirmation(int orderHeaderid)
        {
            //orderheader id yi alma sebebimiz stripe durumunu kontrol etmek.
            OrderHeader orderHeader = _unitofWork.OrderHeader.GetFİrstOrDefault(u => u.Id == orderHeaderid);
            if (orderHeader.PaymentStatus == SD.PaymentStatusDelayedPayment)
            {
                var service = new SessionService();
                Session session = service.Get(orderHeader.SessionId);
                if (session.PaymentStatus.ToLower() == "paid")
                {
                    _unitofWork.OrderHeader.UpdateStatus(orderHeaderid, orderHeader.OrderStatus, SD.PaymentStatusApproved);
                    _unitofWork.Save();
                }
            }
            return View(orderHeaderid);
        }



        [HttpPost]
        [Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee)] // we want this action to be executed only from admin or employee
        [ValidateAntiForgeryToken]
        public IActionResult UpdateOrderDetail() // bind property attributundan dolayı parametre girmemize gerek yok.
        {
            var orderHeaderFromDb = _unitofWork.OrderHeader.GetFİrstOrDefault(u => u.Id == orderVM.OrderHeader.Id, tracked:false);
            // bütün propertyleri update etmek istemiyoruz.Bu yüzden Direk update methodunu çağırmaktansa  tek tek ekledik.
            orderHeaderFromDb.Name = orderVM.OrderHeader.Name;
            orderHeaderFromDb.PhoneNumber = orderVM.OrderHeader.PhoneNumber;
            orderHeaderFromDb.StreetAddress = orderVM.OrderHeader.StreetAddress;
            orderHeaderFromDb.City = orderVM.OrderHeader.City;
            orderHeaderFromDb.State = orderVM.OrderHeader.State;
            orderHeaderFromDb.PostalCode = orderVM.OrderHeader.PostalCode;
            if(orderVM.OrderHeader.Carrier != null)
            {
                orderHeaderFromDb.Carrier = orderVM.OrderHeader.Carrier;
            }
            if(orderVM.OrderHeader.TrackingNumber != null)
            {
                orderHeaderFromDb.TrackingNumber = orderVM.OrderHeader.TrackingNumber;
            }
            _unitofWork.OrderHeader.Update(orderHeaderFromDb);
            _unitofWork.Save();
            TempData["success"] = "Order details Updated Successfully";
            //details action order controller, we are passing the id.
            return RedirectToAction("Details", "Order", new { orderId = orderHeaderFromDb.Id });
        }

        [HttpPost]
        [Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee)] // we want this action to be executed only from admin or employee
        [ValidateAntiForgeryToken]
        public IActionResult StartProcessing() 
        {
            _unitofWork.OrderHeader.UpdateStatus(orderVM.OrderHeader.Id, SD.StatusInProcess);
            _unitofWork.Save();
            TempData["success"] = "Order Status Updated Successfully";
            //details action order controller, we are passing the id.
            return RedirectToAction("Details", "Order", new { orderId = orderVM.OrderHeader.Id });
        }

        [HttpPost]
        [Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee)] // we want this action to be executed only from admin or employee
        [ValidateAntiForgeryToken]
        public IActionResult ShipOrder()
        {
            var orderHeader = _unitofWork.OrderHeader.GetFİrstOrDefault(u => u.Id == orderVM.OrderHeader.Id, tracked: false);
            orderHeader.TrackingNumber = orderVM.OrderHeader.TrackingNumber;
            orderHeader.Carrier = orderVM.OrderHeader.Carrier;
            orderHeader.OrderStatus = SD.StatusShipped;
            orderHeader.ShippingDate = DateTime.Now;
            if(orderHeader.PaymentStatus == SD.PaymentStatusDelayedPayment)
            {
                orderHeader.PaymentDueDate = DateTime.Now.AddDays(30);
            }
            _unitofWork.OrderHeader.Update(orderHeader);
            _unitofWork.Save();
            TempData["success"] = "Order Shipped Successfully";
            //details action order controller, we are passing the id.
            return RedirectToAction("Details", "Order", new { orderId = orderVM.OrderHeader.Id });
        }
        [HttpPost]
        [Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee)] // we want this action to be executed only from admin or employee
        [ValidateAntiForgeryToken]
        public IActionResult CancelOrder()
        {
            var orderHeader = _unitofWork.OrderHeader.GetFİrstOrDefault(u => u.Id == orderVM.OrderHeader.Id, tracked: false);
            if(orderHeader.PaymentStatus == SD.PaymentStatusApproved)// if the payment was already made
            {
                //stripa gelen yeni özellik nedeniyle aşağıdaki kodlar eklendi.
                var service = new SessionService();
                Session session = service.Get(orderHeader.SessionId);
                if(session.PaymentStatus.ToLower() == "paid")
                {
                    var paymentIntentId = session.PaymentIntentId;
                    if(paymentIntentId != null)
                    {
                        //RefundCreateOptions ==> stripe' dan geliyor.
                        var options = new RefundCreateOptions
                        {
                            Reason = RefundReasons.RequestedByCustomer,
                            PaymentIntent = paymentIntentId //Geri ödenecek tutarı default olarak kendisi halledecek. 
                        };

                        var rservice = new RefundService();
                        Refund refund = rservice.Create(options);
                        //id, orderstatus, payment status
                        _unitofWork.OrderHeader.UpdateStatus(orderHeader.Id, SD.StatusCancelled, SD.StatusRefunded);
                    }
                }
                

                
            }
            else
            {
                _unitofWork.OrderHeader.UpdateStatus(orderHeader.Id, SD.StatusCancelled, SD.StatusCancelled);
            }

            _unitofWork.Save();
            TempData["success"] = "Order Cancelled Successfully";
            //details action order controller, we are passing the id.
            return RedirectToAction("Details", "Order", new { orderId = orderVM.OrderHeader.Id });
        }

        #region API CALLS
        [HttpGet]
        public IActionResult GetAll(string status)
        {
            IEnumerable<OrderHeader> orderHeaders;

            if(User.IsInRole(SD.Role_Admin) || User.IsInRole(SD.Role_Employee))
            {
                orderHeaders = _unitofWork.OrderHeader.GetAll(includeProperties: "ApplicationUser");
            }
            else
            {
                //log olan kullanıcının id sini almak için claims kullandık.
                var claimsIdentity = (ClaimsIdentity)User.Identity;
                var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
                orderHeaders = _unitofWork.OrderHeader.GetAll(u=>u.ApplicationUserId == claim.Value,includeProperties: "ApplicationUser");
            }

            switch(status)
            {
                //Ienumerable filterlamak için where kullandık.(LİNQ)
                case "pending":
                    orderHeaders = orderHeaders.Where(u => u.PaymentStatus == SD.PaymentStatusDelayedPayment);
                    break;
                case "inprocess":
                    orderHeaders = orderHeaders.Where(u => u.OrderStatus == SD.StatusInProcess);
                    break;
                case "completed":
                    orderHeaders = orderHeaders.Where(u => u.OrderStatus == SD.StatusShipped);
                    break;
                case "approved":
                    orderHeaders = orderHeaders.Where(u => u.OrderStatus == SD.StatusApproved);
                    break;
                default:
                    break;
            }

            return Json(new { data = orderHeaders });
        }
        #endregion
    }
}
