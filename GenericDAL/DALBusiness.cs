using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GenericDAL.Contexts;

namespace GenericDAL
{
    public class DALBusiness
    {
        //--------- Querying / Transacting --------

        // 1
        public List<Employee> GetAllEmployees_GenericRetrival()
        {
            List<Employee> result = null;
            bool success = DALHelper.GenericRetrival<NorthwindDBContext>((northwindContext) =>
            {
                result = (from e in northwindContext.Employees select e).ToList();
            });
            return result;
        }

        // 2
        public List<Employee> GetAllEmployees_GenericResultRetrival()
        {
            List<Employee> result =
                DALHelper.GenericResultRetrival<NorthwindDBContext, List<Employee>>((northwindContext) =>
                {
                    return (from e in northwindContext.Employees select e).ToList();
                });
            return result;
        }

        // 3
        public async Task<List<Employee>> GetAllEmployeesAsync()
        {
            return await DALHelper.GenericResultRetrivalAsync<NorthwindDBContext, List<Employee>>(async (northwindContext) =>
            {
                return await (from e in northwindContext.Employees select e).ToListAsync();
            });
        }

        // 5
        public async Task<object> GetDistributedDataAsync()
        {
            return await DALHelper.GenericTwiceContextsRetrivalAsync<NorthwindDBContext, AdventureWorkDBContext>(async
                (northwindContext, advantureContext) =>
                {
                    var employees = (from e in northwindContext.Employees select e).ToListAsync();
                    var customers = (from p in advantureContext.Customers select p).ToListAsync();

                    await Task.WhenAll(employees, customers);
                    return new
                    {
                        EmployeeList = employees.Result,
                        CustomerList = customers.Result
                    };
                });
        }

        public object GetDistributedData()
        {
            return DALHelper.GenericTwiceContextsRetrival<NorthwindDBContext, AdventureWorkDBContext, object>(
                (northwindContext, advantureContext) =>
                {
                    var t1 = (from e in northwindContext.Employees select e).ToListAsync();
                    var t2 = (from p in advantureContext.Customers select p).ToListAsync();

                    var res = Task.WhenAll(t1, t2);
                    return new
                    {
                        EmployeeList = t1.Result,
                        CustomerList = t2.Result
                    };
                });
        }
        // ------------------- Saving ---------------

        // 6
        public bool AddMultipleRecords(Employee newEmp, Supplier newSup)
        {
            return DALHelper.GenericSafeTransaction<NorthwindDBContext>(northwindContext =>
            {
                northwindContext.Employees.Add(newEmp);
                northwindContext.SaveChanges();
                northwindContext.Suppliers.Add(newSup);
                northwindContext.SaveChanges();
            });
        }
        public int AddEmployee(Employee newEmp)
        {
            int affectedRecords = 0;
            DALHelper.GenericSafeTransaction<NorthwindDBContext>((northwindContext) =>
            {
                northwindContext.Employees.Add(newEmp);
                affectedRecords = northwindContext.SaveChanges();
            });
            return affectedRecords;
        }

        // 7
        public async Task<int?> AddEmployeeAsync(Employee newEmp)
        {
            return await DALHelper.GenericSafeTransactionAsync<NorthwindDBContext>(async (northwindContext) =>
            {
                northwindContext.Employees.Add(newEmp);
            });
        }
        public async Task<int?> AddEmployeeAsync2(Employee newEmp)
        {
            return await DALHelper.GenericSafeTransactionAsync2<NorthwindDBContext>( async (northwindContext) =>
            {
                northwindContext.Employees.Add(newEmp);
                return await northwindContext.SaveChangesAsync();
            });
        }
    }
}
