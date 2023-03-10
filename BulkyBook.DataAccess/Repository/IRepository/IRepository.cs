using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace BulkyBook.DataAccess.Repository.IRepository
{
    // Update method is not something that would be consistent across all the models that we have. Therefore we will not implement a update method inside Repository.
    public interface IRepository<T> where T : class //generic repository olduğunu söylemek için girdiğimiz kod.
    {
        //T- CATEGORY
        //Edit işlemi yaparken Id ye göre prop değerlerini bulmamızı sağlayan kod.
        //Find da kullanabalirdik ancak o method sadece primary key olunca kullanılıyor.O yüzden aşağıdaki kod daha genel bir şekilde işlemi gerçekleştirmemizi sağlıyor. (EDİT GET METHOD- LİNE15)
        T GetFİrstOrDefault(Expression<Func<T,bool>>filter, string? includeProperties = null, bool tracked = true); // order controller update methodunda ef core obje tracklediği için değişiklikleri kaydettiğimizde update methodunu çağırmasak bile kaydediyordu. Bunun önüne geçmek için tracked isimli bir bool değer oluşturduk ve true değerini atadık.
        IEnumerable<T> GetAll(Expression<Func<T, bool>>? filter=null, string? includeProperties = null); // Category(mesela) sınıfının içindeki (table) bütün herşeyi getirmek için kullandığımız kod
        void Add(T entity); // entity yazmak zorundayız. // obje ekleme metodu
        void Remove(T entity);

        void RemoveRange(IEnumerable<T> entity); //Birden fazla entity 'yi silmek istediğimizde kullanacağımız method.
    }
}
