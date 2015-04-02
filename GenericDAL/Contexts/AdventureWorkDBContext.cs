using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;

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

    [Table("Customer",Schema = "SalesLT")]
    public class Customer
    {
        [Key]
        public int CustomerID { get; set; }
        public string FirstName { get; set; }
    }
}
