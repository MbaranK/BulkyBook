using BulkyBook.DataAccess.Repository;
using BulkyBook.DataAccess.Repository.IRepository;
using BulkyBook.Models;
using BulkyBook.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Security.Claims;

namespace BulkyBookWeb.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IUnitofWork _unitofWork; //to access all the products.

        public HomeController(ILogger<HomeController> logger,IUnitofWork unitofwork)
        {
            _logger = logger;
            _unitofWork = unitofwork;
        }

        public IActionResult Index()
        {
            IEnumerable<Product> productList = _unitofWork.Product.GetAll(includeProperties:"Category,CoverType"); // Product'ın içindeki bütün verilere sahip product listesi oluşturduk. Category ve CoverType'larınında gelmesini istediğimiz için onları includeladık.
            return View(productList);
        }

        //Get Method
        public IActionResult Details(int productId)
        {
            ShoppingCart cartObj = new()
            {
                Count = 1,
                ProductId = productId,
                Product = _unitofWork.Product.GetFİrstOrDefault(u => u.Id == productId, includeProperties: "Category,CoverType"),
            };

            return View(cartObj);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize] // sadece giriş yapan kişilerin shopping cartlarına birşey eklemesini istediğimiz için girdik.
        public IActionResult Details(ShoppingCart shoppingCart)
        {
            //User ID yi almak için gerçekleştirdiğimiz kodlar
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
            shoppingCart.ApplicationUserId = claim.Value;
            //

            //Eğer aynı üründen tekrar karta ekleme yapmak istersek yeni bir kayıt oluşturmasın üstüne eklesin diye yazdığımız kod.
            ShoppingCart cartFromDb = _unitofWork.ShoppingCart.GetFİrstOrDefault(u=> u.ApplicationUserId == claim.Value && u.ProductId == shoppingCart.ProductId);

            
            if(cartFromDb == null)
            {
                _unitofWork.ShoppingCart.Add(shoppingCart);
                _unitofWork.Save();
                //Session ekleme
                HttpContext.Session.SetInt32(SD.SessionCart, _unitofWork.ShoppingCart.GetAll(u => u.ApplicationUserId == claim.Value).ToList().Count);
            }
            else
            {
                _unitofWork.ShoppingCart.IncrementCount(cartFromDb, shoppingCart.Count);
                _unitofWork.Save();
            }

            
            _unitofWork.Save();

            //ShoppingCart cartObj = new()
            //{
            //    Count = 1,
            //    //Product = _unitofWork.Product.GetFİrstOrDefault(u => u.Id == id, includeProperties: "Category,CoverType"),
            //};

            return RedirectToAction(nameof(Index));
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}