using BulkyBook.DataAccess.Repository.IRepository;
using BulkyBook.Models;
using Microsoft.AspNetCore.Mvc;
using System.Drawing.Text;

namespace BulkyBookWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class CoverTypeController : Controller
    {
        private readonly IUnitofWork _unitofWork;

        public CoverTypeController(IUnitofWork unitofwork)
        {
            _unitofWork = unitofwork;
        }
        public IActionResult Index()
        {
            IEnumerable<CoverType> objCoverTypeList = _unitofWork.CoverType.GetAll();
            return View(objCoverTypeList);
        }

        //Create GET
        public IActionResult Create()
        {
            return View();
        }

        //CREATE POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(CoverType obj)
        {
            if(ModelState.IsValid)
            {
                _unitofWork.CoverType.Add(obj);
                _unitofWork.Save();
                TempData["success"] = "Cover Type created successfully";
                return RedirectToAction("Index");

            }
            return View(obj);
        }

        //Edit Get
        public IActionResult Edit(int? id)
        {
            if(id == null || id == 0)
            {
                return NotFound();
            }

            var coverTypefromDbFirst = _unitofWork.CoverType.GetFİrstOrDefault(u => u.Id == id);

            if(coverTypefromDbFirst == null)
            {
                return NotFound();
            }

            return View(coverTypefromDbFirst);
        }

        //Edit POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(CoverType obj)
        {
            if(ModelState.IsValid)
            {
                _unitofWork.CoverType.Update(obj);
                _unitofWork.Save();
                TempData["success"] = "Cover Type updated successfully";
                return RedirectToAction("Index");
            }

            return View(obj);
        }

        //Delete Get
        public IActionResult Delete(int? id)
        {
            if(id ==null || id == 0)
            {
                return NotFound();
            }

            var coverTypeFromDbFirst = _unitofWork.CoverType.GetFİrstOrDefault(u => u.Id == id);

            if(coverTypeFromDbFirst == null)
            {
                return NotFound();
            }
            return View(coverTypeFromDbFirst);
        }

        //Delete Post
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeletePost(int? id)
        {
            var obj = _unitofWork.CoverType.GetFİrstOrDefault(u => u.Id == id);
            if(obj== null)
            {
                return NotFound();
            }

            _unitofWork.CoverType.Remove(obj);
            _unitofWork.Save();
            TempData["success"] = "Cover Type created successfully";
            return RedirectToAction("Index");
        }
    }
}
