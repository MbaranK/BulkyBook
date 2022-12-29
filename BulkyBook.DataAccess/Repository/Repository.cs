using BulkyBook.DataAccess.Repository.IRepository;
using BulkyBookWeb.DataAccess;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace BulkyBook.DataAccess.Repository
{
    public class Repository<T> : IRepository<T> where T : class // This is a general way od implementing a genetic repository
    {
        //So with this, we will be getting the implementation of our database just like we did inside categoryController
        // Buranın içinde database save işlemi yapmak iyi bir yaklaşım değil.Bu yüzden model özelinde oluşturulan interfacelerin içinde database de kaydetme işlemi yapacağız. Bknz: ICategoryRepository
        private readonly ApplicationDbContext _db;
        internal DbSet<T> dbSet;

        public Repository(ApplicationDbContext db)
        {
            _db = db;
            //_db.Products.Include(u => u.Category);
            this.dbSet = _db.Set<T>(); // This is just the basic setup that we have to do to implement a solid repository that will work with all the conditions. Bu generic class olduğu için herhangi bir sınıfa implement edebiliriz. Bu yüzden bu işlemi yapmak zorundayız.
            // dbSet, CategoryController daki _db.Category ile birebir aynı işlevi görüyor.
        }

        public void Add(T entity)
        {
            dbSet.Add(entity);
        }

        //includeProp - "Category,CoverType" gibi.
        public IEnumerable<T> GetAll(Expression<Func<T, bool>>? filter=null,string? includeProperties = null)
        {
            IQueryable<T> query = dbSet;
            if(filter != null)
            {
                query = query.Where(filter);
            }
            
            //Foreign keyler null değer olarak geldi diye line38-44 girdik.
            if (includeProperties != null)
            {
                foreach(var includeProp in includeProperties.Split(new char[] { ','},StringSplitOptions.RemoveEmptyEntries))
                {
                    query = query.Include(includeProp);
                }
            }
            return query.ToList();
        }

        public T GetFİrstOrDefault(Expression<Func<T, bool>> filter, string? includeProperties = null)
        {
            IQueryable<T> query = dbSet;

            query = query.Where(filter);
            //Foreign keyler null değer olarak geldi diye girdik.
            if (includeProperties != null)
            {
                foreach (var includeProp in includeProperties.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    query = query.Include(includeProp);
                }
            }

            return query.FirstOrDefault();
        }

        public void Remove(T entity)
        {
            dbSet.Remove(entity);
        }

        public void RemoveRange(IEnumerable<T> entity)
        {
            dbSet.RemoveRange(entity);
        }
    }
}
