using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
            CountyId = CountyId;
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

                    //System.Data.Objects.RefreshMode refresh = new System.Data.Objects.RefreshMode();

                    ctx.Customers.AddObject(customerInstance);
                    ctx.ObjectStateManager.ChangeObjectState(customerInstance, System.Data.EntityState.Added);
                    //ctx.Refresh(refresh, AddCustomersToList());
                    ctx.SaveChanges();

                    Customers = AddCustomersToList();

                    MessageBox.Show($"New County ID: {customerInstance.Name} has been added to the IVS Customer Library", "New Customer Added");
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error with inserting data to the database \n\n{ex.Message} \n\n Please contact system administrator",
                        "ERROR MESSAGE", MessageBoxButton.OK, MessageBoxImage.Error);
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
                MessageBox.Show(ex.Message, "Something Happened");
            }
            return Customers;
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
