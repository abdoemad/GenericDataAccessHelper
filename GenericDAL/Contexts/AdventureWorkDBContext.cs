using System.Data.Entity;
using GenericDAL.Models;

namespace GenericDAL.Contexts
{
    internal class AdventureWorkDBContext: DbContext
    {
        public virtual DbSet<Customer> Customers { get; set; } 
        public AdventureWorkDBContext()
            : base("name=AdventureWorksDBConnection")
        {
        }
    }
}
