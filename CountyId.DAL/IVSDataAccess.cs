using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Customer;
using System.Windows;

namespace CountyId.DAL
{
    public class IVSDataAccess : IDisposable
    {
        private CustomerEntities ctx;
        public IVSDataAccess(string countyName, string countyId, string state) : this()
        {
            CountyName = countyName;
            CountyId = countyId;
            State = state;
        }

        public IVSDataAccess()
        {

        }

        public string Tabulation { get; set; }
        public string CountyName { get; set; }
        public string CountyId { get; set; }
        public string State { get; set; }
        public List<string> Customers { get; set; }        

        public void AddCountyToDBSource()
        {
            using (ctx = new CustomerEntities())
            {
                try
                {
                    Customer.Customer customerInstance = new Customer.Customer
                    {
                        CountyName = CountyName,
                        State = State,
                        Name = CountyId,
                        TabulationSystem = Tabulation
                    };

                    ctx.Customers.AddObject(customerInstance);
                    ctx.ObjectStateManager.ChangeObjectState(customerInstance, System.Data.EntityState.Added);
                    ctx.SaveChanges();

                    Customers = AddCustomersToList();

                    MessageBox.Show($"New County ID: {customerInstance.Name} has been added to the IVS Customer Library", "New Customer Added");
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error with inserting data to the database \n\n{ex.Message} \n\n Please contact system administrator",
                        "IVS Data Access Class ERROR", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }            
        }

        public List<string> AddCustomersToList()
        {
            try
            {
                using (ctx = new CustomerEntities())
                {
                    Customers = (from cust in ctx.Customers
                                 orderby cust.Name
                                 select cust.Name).ToList();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error with connecting to the database \n\n{ex.Message} \n\n Please contact system administrator",
                        "IVS Data Access Class ERROR", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            return Customers ?? null;
        }

        public string PopulateCountyId(string state, string name)
        {
            StringBuilder sb = new StringBuilder();
            string countyId = string.Empty;
            try
            {                
                sb.Append(state, 0, 2);
                sb.Append(name);

                countyId = sb.ToString();
            }
            catch (Exception ex)
            {
               return ex.Message;
            }

            return countyId.ToUpper();
        }

        public void Dispose()
        {
            ctx = null;
        }
    }
}
