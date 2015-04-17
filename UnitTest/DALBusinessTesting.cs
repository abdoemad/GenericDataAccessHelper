using System;
using System.IO;
using GenericDAL;
using GenericDAL.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTest
{
    [TestClass]
    public class DALBusinessTesting
    {
        public DALBusinessTesting()
        {
            AppDomain.CurrentDomain.SetData("DataDirectory", Directory.GetParent(Directory.GetParent(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "")).FullName).FullName+"\\App_Data");
        }

        [TestMethod]
        public void GetAllEmployees()
        {
            try
            {
                DALBusiness dal = new DALBusiness();
                var list = dal.GetAllEmployees_GenericRetrival();
                Assert.IsNotNull(list,list.ToString());
                if (list != null)
                    Assert.IsTrue(list.Count > 0, "");

                Assert.IsFalse(list.Count == 0,list.Count.ToString());

            }
            catch (Exception)
            {
                
                throw;
            }
           
        }

        [TestMethod]
        public void GetEmployeesAsync()
        {
            try
            {
                DALBusiness dal = new DALBusiness();
                var task = dal.GetAllEmployeesAsync();
                task.Wait();
                var res = task.Result;
            }
            catch (Exception)
            {
                
                throw;
            }
        }

        [TestMethod]
        public void GettDistributedObject()
        {
            DALBusiness dal = new DALBusiness();
            var task = dal.GetDistributedDataAsync();
            task.Wait();
            var res = task.Result;
        }

        [TestMethod]
        public void AddEmployee()
        {
            DALBusiness dal=new DALBusiness();
            var task = dal.AddEmployeeAsync2(new Employee {FirstName = "Abdo", LastName = "Emad"});
            task.Wait();
        }
    }
}
