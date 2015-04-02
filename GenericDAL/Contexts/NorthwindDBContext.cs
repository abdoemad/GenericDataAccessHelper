using System.ComponentModel.DataAnnotations;
using System.Data.Entity;

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

    public class Employee
    {
        [Key]
        public int EmployeeID { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
    }

    public class Supplier
    {
        [Key]
        public int SupplierID { get; set; }
        public string CompanyName { get; set; }
    }
}
