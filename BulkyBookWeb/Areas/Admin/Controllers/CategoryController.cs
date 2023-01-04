
using BulkyBook.DataAccess.Repository.IRepository;
using BulkyBook.Models;
using BulkyBook.Utility;
using BulkyBookWeb.DataAccess;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BulkyBookWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin)] // UI DA SADECE ADMİN GÖREBİLECEK.
    public class CategoryController : Controller
    {
        private readonly IUnitofWork _unitOfWork;

        public CategoryController(IUnitofWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public IActionResult Index()
        {
            IEnumerable<Category> objCategoryList = _unitOfWork.Category.GetAll();
            return View(objCategoryList);
        }

        //GET
        public IActionResult Create() // create sayfası açıldığında databaseden bir veri çekmesini istemediğimiz için boş.
        {

            return View();
        }

        //Post
        [HttpPost] // We have to write this in post methods
        [ValidateAntiForgeryToken]
        public IActionResult Create(Category obj)
        {
            if (obj.Name == obj.DisplayOrder.ToString())
            {
                ModelState.AddModelError("name", "DisplayOrder cannot exactly match the Name");
            }
            if (ModelState.IsValid)
            {
                _unitOfWork.Category.Add(obj);
                _unitOfWork.Save(); //Databasede değişikleri kaydetmek için yazıyoruz
                TempData["success"] = "Category created successfully";
                return RedirectToAction("Index");
            }

            return View(obj);
        }

        //GET
        public IActionResult Edit(int? id)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }
            //Databaseden veri çekme işlemleri(Edite tıkladığımızda id ye göre bize Name'ini getirmesi için yazdık.)
            //var categoryFromDb = _db.Categories.Find(id);
            var categoryFromDbFirst = _unitOfWork.Category.GetFİrstOrDefault(u => u.Id == id);
            //var categoryFromDbSingle = _db.Categories.SingleOrDefault(u=>u.Id == id);
            if (categoryFromDbFirst == null)
            {
                return NotFound();
            }
            return View(categoryFromDbFirst);
        }

        //Post
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Category obj)
        {
            if (obj.Name == obj.DisplayOrder.ToString())
            {
                ModelState.AddModelError("name", "DisplayOrder cannot exactly match the Name");
            }
            if (ModelState.IsValid)
            {
                _unitOfWork.Category.Update(obj);
                _unitOfWork.Save(); //Databasede değişikleri kaydetmek için yazıyoruz
                TempData["success"] = "Category updated successfully";
                return RedirectToAction("Index");
            }

            return View(obj);
        }
        //GET
        public IActionResult Delete(int? id)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }
            //Databaseden veri çekme işlemleri(Edite tıkladığımızda id ye göre bize Name'ini getirmesi için yazdık.)
            //var categoryFromDb = _db.Categories.Find(id);
            var categoryFromDbFirst = _unitOfWork.Category.GetFİrstOrDefault(u => u.Id == id);
            //var categoryFromDbSingle = _db.Categories.SingleOrDefault(u=>u.Id == id);
            if (categoryFromDbFirst == null)
            {
                return NotFound();
            }
            return View(categoryFromDbFirst);
        }

        //Post
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeletePost(int? id)
        {
            var obj = _unitOfWork.Category.GetFİrstOrDefault(u => u.Id == id);
            if (obj == null)
            {
                return NotFound();
            }


            _unitOfWork.Category.Remove(obj);
            _unitOfWork.Save(); //Databasede değişikleri kaydetmek için yazıyoruz
            TempData["success"] = "Category deleted successfully";
            return RedirectToAction("Index");


        }
    }
}
