using System.Data.Entity;
using GenericDAL.Models;

namespace GenericDAL.Contexts
{
    internal class NorthwindDBContext : DbContext
    {
        public virtual DbSet<Employee> Employees { get; set; }
        public virtual DbSet<Supplier> Suppliers { get; set; }

        public NorthwindDBContext()
            : base("name=NorthwindDBConnection")
        {
        }
    }
}
