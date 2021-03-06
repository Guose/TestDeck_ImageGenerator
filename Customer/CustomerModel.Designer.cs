﻿//------------------------------------------------------------------------------
// <auto-generated>
//    This code was generated from a template.
//
//    Manual changes to this file may cause unexpected behavior in your application.
//    Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using System;
using System.ComponentModel;
using System.Data.EntityClient;
using System.Data.Objects;
using System.Data.Objects.DataClasses;
using System.Linq;
using System.Runtime.Serialization;
using System.Xml.Serialization;

[assembly: EdmSchemaAttribute()]
namespace Customer
{
    #region Contexts
    
    /// <summary>
    /// No Metadata Documentation available.
    /// </summary>
    public partial class CustomerEntities : ObjectContext
    {
        #region Constructors
    
        /// <summary>
        /// Initializes a new CustomerEntities object using the connection string found in the 'CustomerEntities' section of the application configuration file.
        /// </summary>
        public CustomerEntities() : base("name=CustomerEntities", "CustomerEntities")
        {
            this.ContextOptions.LazyLoadingEnabled = true;
            OnContextCreated();
        }
    
        /// <summary>
        /// Initialize a new CustomerEntities object.
        /// </summary>
        public CustomerEntities(string connectionString) : base(connectionString, "CustomerEntities")
        {
            this.ContextOptions.LazyLoadingEnabled = true;
            OnContextCreated();
        }
    
        /// <summary>
        /// Initialize a new CustomerEntities object.
        /// </summary>
        public CustomerEntities(EntityConnection connection) : base(connection, "CustomerEntities")
        {
            this.ContextOptions.LazyLoadingEnabled = true;
            OnContextCreated();
        }
    
        #endregion
    
        #region Partial Methods
    
        partial void OnContextCreated();
    
        #endregion
    
        #region ObjectSet Properties
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        public ObjectSet<Customer> Customers
        {
            get
            {
                if ((_Customers == null))
                {
                    _Customers = base.CreateObjectSet<Customer>("Customers");
                }
                return _Customers;
            }
        }
        private ObjectSet<Customer> _Customers;

        #endregion

        #region AddTo Methods
    
        /// <summary>
        /// Deprecated Method for adding a new object to the Customers EntitySet. Consider using the .Add method of the associated ObjectSet&lt;T&gt; property instead.
        /// </summary>
        public void AddToCustomers(Customer customer)
        {
            base.AddObject("Customers", customer);
        }

        #endregion

    }

    #endregion

    #region Entities
    
    /// <summary>
    /// No Metadata Documentation available.
    /// </summary>
    [EdmEntityTypeAttribute(NamespaceName="CustomerModel", Name="Customer")]
    [Serializable()]
    [DataContractAttribute(IsReference=true)]
    public partial class Customer : EntityObject
    {
        #region Factory Method
    
        /// <summary>
        /// Create a new Customer object.
        /// </summary>
        /// <param name="custID">Initial value of the CustID property.</param>
        /// <param name="name">Initial value of the Name property.</param>
        /// <param name="countyName">Initial value of the CountyName property.</param>
        /// <param name="state">Initial value of the State property.</param>
        public static Customer CreateCustomer(global::System.Int32 custID, global::System.String name, global::System.String countyName, global::System.String state)
        {
            Customer customer = new Customer();
            customer.CustID = custID;
            customer.Name = name;
            customer.CountyName = countyName;
            customer.State = state;
            return customer;
        }

        #endregion

        #region Simple Properties
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [EdmScalarPropertyAttribute(EntityKeyProperty=true, IsNullable=false)]
        [DataMemberAttribute()]
        public global::System.Int32 CustID
        {
            get
            {
                return _CustID;
            }
            set
            {
                if (_CustID != value)
                {
                    OnCustIDChanging(value);
                    ReportPropertyChanging("CustID");
                    _CustID = StructuralObject.SetValidValue(value, "CustID");
                    ReportPropertyChanged("CustID");
                    OnCustIDChanged();
                }
            }
        }
        private global::System.Int32 _CustID;
        partial void OnCustIDChanging(global::System.Int32 value);
        partial void OnCustIDChanged();
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [EdmScalarPropertyAttribute(EntityKeyProperty=false, IsNullable=false)]
        [DataMemberAttribute()]
        public global::System.String Name
        {
            get
            {
                return _Name;
            }
            set
            {
                OnNameChanging(value);
                ReportPropertyChanging("Name");
                _Name = StructuralObject.SetValidValue(value, false, "Name");
                ReportPropertyChanged("Name");
                OnNameChanged();
            }
        }
        private global::System.String _Name;
        partial void OnNameChanging(global::System.String value);
        partial void OnNameChanged();
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [EdmScalarPropertyAttribute(EntityKeyProperty=false, IsNullable=false)]
        [DataMemberAttribute()]
        public global::System.String CountyName
        {
            get
            {
                return _CountyName;
            }
            set
            {
                OnCountyNameChanging(value);
                ReportPropertyChanging("CountyName");
                _CountyName = StructuralObject.SetValidValue(value, false, "CountyName");
                ReportPropertyChanged("CountyName");
                OnCountyNameChanged();
            }
        }
        private global::System.String _CountyName;
        partial void OnCountyNameChanging(global::System.String value);
        partial void OnCountyNameChanged();
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [EdmScalarPropertyAttribute(EntityKeyProperty=false, IsNullable=false)]
        [DataMemberAttribute()]
        public global::System.String State
        {
            get
            {
                return _State;
            }
            set
            {
                OnStateChanging(value);
                ReportPropertyChanging("State");
                _State = StructuralObject.SetValidValue(value, false, "State");
                ReportPropertyChanged("State");
                OnStateChanged();
            }
        }
        private global::System.String _State;
        partial void OnStateChanging(global::System.String value);
        partial void OnStateChanged();
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [EdmScalarPropertyAttribute(EntityKeyProperty=false, IsNullable=true)]
        [DataMemberAttribute()]
        public global::System.String TabulationSystem
        {
            get
            {
                return _TabulationSystem;
            }
            set
            {
                OnTabulationSystemChanging(value);
                ReportPropertyChanging("TabulationSystem");
                _TabulationSystem = StructuralObject.SetValidValue(value, true, "TabulationSystem");
                ReportPropertyChanged("TabulationSystem");
                OnTabulationSystemChanged();
            }
        }
        private global::System.String _TabulationSystem;
        partial void OnTabulationSystemChanging(global::System.String value);
        partial void OnTabulationSystemChanged();

        #endregion

    }

    #endregion

}
