using BulkyBook.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BulkyBook.DataAccess.Repository.IRepository
{
    public interface ICategoryRepository : IRepository<Category> // Irepository generic class bekliyor. Bu interface de Category sınıfı için yapılacğı için böyle yazdık. The model will be category , inside the generic repository , we dont have method for update. So we will implement that method and that will be specific to this repository.(interface)
    {
        void Update(Category obj);
       
    }
}
