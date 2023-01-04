using BulkyBook.DataAccess.Repository.IRepository;
using BulkyBook.Models.ViewModels;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using BulkyBook.Utility;

namespace BulkyBookWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin)] // UI DA SADECE ADMİN GÖREBİLECEK.
    public class ProductController : Controller
    {
        private readonly IUnitofWork _unitofWork;
        // wwroot'a ulaşmak için girdiğimiz kod.
        private readonly IWebHostEnvironment _hostEnvironment;

        public ProductController(IUnitofWork unitofwork, IWebHostEnvironment hostEnvironment)
        {
            _unitofWork = unitofwork;
            _hostEnvironment = hostEnvironment;
        }
        public IActionResult Index()
        {
            return View();
        }


        // UPSERT GET 
        //Upsert = Update ve insert metodlarını birlikte kullandık burada.(update,create)
        public IActionResult Upsert(int? id)
        {
            //Editleme(update) işlemi yapılabilmesi için; Id'nin null olmaması gereki yani ID null olduğu zaman create işlemini yaptırabiliriz.
            // Product oluştururken kategori ve cover type seçmek istiyoruz. Bu yüzden CategoryList adında bir liste oluşturdurk.Category ve Covertype _unitofwork'in içinde olan repositoryler olduğu için onu kullanarak erişim sağlayabiliyoruz.(Bütün repositorylere _unitofworkden erişim sağlayabiliriz.)        

            //VİEWBAG transfers data from the controller to view, not vice-versa. Ideal for situations in which the temporary data is not in a model(Model içinde tanımlamadığımız şeyleri controllerden view'a aktarmak için kullanıyoruz.)
            //VİEWBAG Any number of properties and values can be assigned to Viewbag.
            //VİEWBAG Temporary data taşıdıkları için sayfayı yenilediğimizde içindeki değerler sıfırlanır.

            //VİEWDATA transfers data from the controller to view, not vice-versa. Ideal for situations in which the temporary data is not in a model(Model içinde tanımlamadığımız şeyleri controllerden view'a aktarmak için kullanıyoruz.)
            //VİEWDATA is derived from ViewDataDictionary which is a dictionary type.
            //VİEWDATA value must be TYPE CAST before use.
            //VİEWDATA Temporary data taşıdıkları için sayfayı yenilediğimizde içindeki değerler sıfırlanır.
            //ViewData and ViewBag is identical as functional bias.
            //ViewBag internally inserts data into ViewData dictionary.So the key of ViewData and property of ViewBag must not MATCH.

            //The ideal thing to do is create a ViewModel first. bknz: Model-ViewModel-PRODUCTVM

            ProductVM productVM = new()
            {
                product = new(),
                CategoryList = _unitofWork.Category.GetAll().Select(i => new SelectListItem
                {
                    Text = i.Name,
                    Value = i.Id.ToString()
                }),
                CoverTypeList = _unitofWork.CoverType.GetAll().Select(i => new SelectListItem
                {
                    Text = i.Name,
                    Value = i.Id.ToString()
                }),
            };

            if (id == null || id == 0)
            {
                //CREATE PRODUCT
                //ViewBag.CategoryList = CategoryList;
                //ViewData["CoverTypeList"] = CoverTypeList;

                return View(productVM);
            }
            else
            {
                //UPDATE PRODUCT
                productVM.product = _unitofWork.Product.GetFİrstOrDefault(u => u.Id == id);
                return View(productVM);
            }


        }

        //Upsert POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Upsert(ProductVM obj, IFormFile file)
        {
            if (ModelState.IsValid)
            {
                //resmi yükleyeceğimiz roota ulaşmak için girilen kod.
                string wwwRootPath = _hostEnvironment.WebRootPath;
                if (file != null)
                {
                    string fileName = Guid.NewGuid().ToString();
                    var uploads = Path.Combine(wwwRootPath, @"Images\Product");
                    var extension = Path.GetExtension(file.FileName);
                    if (obj.product.ImageUrl != null)
                    {
                        //upload yolunda \ olmadığı için onu trimledik. Resim updatelediğimzde eski resmin silinmesini sağladık.
                        var oldImagePath = Path.Combine(wwwRootPath, obj.product.ImageUrl.TrimStart('\\'));
                        if (System.IO.File.Exists(oldImagePath))
                        {
                            System.IO.File.Delete(oldImagePath);
                        }
                    }

                    //copying to file uploaded to Images,Products
                    using (var fileStreams = new FileStream(Path.Combine(uploads, fileName + extension), FileMode.Create))
                    {
                        file.CopyTo(fileStreams);
                    }
                    obj.product.ImageUrl = @"\Images\Product\" + fileName + extension;
                }
                if (obj.product.Id == 0)
                {
                    _unitofWork.Product.Add(obj.product);
                }
                else
                {
                    _unitofWork.Product.Update(obj.product);
                }
                _unitofWork.Product.Add(obj.product);
                _unitofWork.Save();
                TempData["success"] = "Product added  successfully";
                return RedirectToAction("Index");
            }

            return View(obj);
        }



        #region API CALLS
        [HttpGet]
        public IActionResult GetAll()
        {
            var productList = _unitofWork.Product.GetAll(includeProperties: "Category,CoverType");
            return Json(new { data = productList });
        }

        //Delete Post
        [HttpDelete]

        public IActionResult Delete(int? id)
        {
            var obj = _unitofWork.Product.GetFİrstOrDefault(u => u.Id == id);
            if (obj == null)
            {
                return Json(new { success = false, message = "Error while deleting" });
            }

            var oldImagePath = Path.Combine(_hostEnvironment.WebRootPath, obj.ImageUrl.TrimStart('\\'));
            if (System.IO.File.Exists(oldImagePath))
            {
                System.IO.File.Delete(oldImagePath);
            }


            _unitofWork.Product.Remove(obj);
            _unitofWork.Save();
            return Json(new { success = true, message = "Delete Successfull" });
            return RedirectToAction("Index");
        }
        #endregion
    }


}
