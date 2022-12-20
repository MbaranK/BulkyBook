using BulkyBookWeb.Models;
using Microsoft.EntityFrameworkCore;

namespace BulkyBookWeb.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {

        }

        //Sql üzerinde kategori tablosunu oluşturmak için girilen kod. Entity CodeFirst approach
        public DbSet<Category> Categories { get; set; }
    }
}
