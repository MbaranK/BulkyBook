using BulkyBook.DataAccess.Repository.IRepository;
using BulkyBook.Models;
using Microsoft.AspNetCore.Mvc;

namespace BulkyBookWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class CompanyController : Controller
    {
        private readonly IUnitofWork _unitofWork;

        public CompanyController(IUnitofWork unitofWork)
        {
            _unitofWork = unitofWork;
        }

        public IActionResult Index()
        {
            return View();
        }
        //GET METHOD
        public IActionResult Upsert(int? id)
        {
            Company company = new();

            if( id == null || id==0)
            {
                return View(company);
            }
            else
            {
                company = _unitofWork.Company.GetFİrstOrDefault(u => u.Id == id);
                return View(company);
            }
        }

        // Post Method
        [HttpPost]
        [ValidateAntiForgeryToken]

        public IActionResult Upsert(Company obj)
        {
            if(ModelState.IsValid)
            {
                if (obj.Id == 0)
                {
                    _unitofWork.Company.Add(obj);
                        TempData["success"] = "Company added successfully";
                }
                else
                {
                    _unitofWork.Company.Update(obj);
                    TempData["success"] = "Company updated successfully";
                }
                _unitofWork.Save();               
                return RedirectToAction("Index");
            }
            return View(obj);
        }

        #region API CALLS
        [HttpGet]

        public IActionResult GetAll()
        {
            var companyList = _unitofWork.Company.GetAll;
            return Json(new { data = companyList });
        }

        //Delete Post
        [HttpDelete]
        public IActionResult Delete(int? id)
        {
            var obj = _unitofWork.Company.GetFİrstOrDefault(u => u.Id == id);
            if(obj == null)
            {
                return Json(new { success = false, message = "Error while deleting" });
            }

            _unitofWork.Company.Remove(obj);
            _unitofWork.Save();
            return Json(new { success = true, message = "Delete successfull" });
            return RedirectToAction("Index");
        }
        #endregion
    }
}
