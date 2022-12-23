using BulkyBook.DataAccess.Repository;
using BulkyBook.DataAccess.Repository.IRepository;
using BulkyBook.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace BulkyBookWeb.Areas.Customer.Controllers
{
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

        public IActionResult Details(int id)
        {
            ShoppingCart cartObj = new()
            {
                Count = 1,
                Product = _unitofWork.Product.GetFİrstOrDefault(u => u.Id == id, includeProperties: "Category,CoverType"),
            };

            return View(cartObj);
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