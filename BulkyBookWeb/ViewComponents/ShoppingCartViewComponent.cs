using BulkyBook.DataAccess.Repository.IRepository;
using BulkyBook.Utility;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BulkyBookWeb.ViewComponents
{
    public class ShoppingCartViewComponent : ViewComponent
    {
        private readonly IUnitofWork _unitofWork;

        public ShoppingCartViewComponent(IUnitofWork unitofWork)
        {
            _unitofWork = unitofWork;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
            if(claim != null) // means a user logged in
            {
                if(HttpContext.Session.GetInt32(SD.SessionCart)!= null) // means session is already set.
                {
                    return View(HttpContext.Session.GetInt32(SD.SessionCart));
                }
                else
                {
                    // we need to go to database and retrive the count.
                    HttpContext.Session.SetInt32(SD.SessionCart,
                        _unitofWork.ShoppingCart.GetAll(u => u.ApplicationUserId == claim.Value).ToList().Count);
                    return View(HttpContext.Session.GetInt32(SD.SessionCart));
                }
            }
            else // user is not signed in or user signed out
            {
                HttpContext.Session.Clear();
                return View(0);
            }
        }
    }
}
