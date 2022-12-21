using BulkyBook.DataAccess.Repository.IRepository;
using BulkyBook.Models;
using BulkyBookWeb.DataAccess;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BulkyBook.DataAccess.Repository
{
    //Repository<Category> bu kodu yazmassak ICategoryRepository nin içindeki metotların dışındaki diğer metotlar da geliyordu. Repository<Category> yazdığımız zaman sadece 2 metod yani ICategoryRepository'nin içinde tanımladığımız metodlar geldi.
    public class CategoryRepository : Repository<Category>, ICategoryRepository
    {
        private ApplicationDbContext _db;

        public CategoryRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        

        public void Update(Category obj)
        {
            _db.Categories.Update(obj);
        }
    }
}
