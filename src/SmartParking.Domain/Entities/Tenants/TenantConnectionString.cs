using System;
using System.Net;
using Volo.Abp.Domain.Entities;
using Volo.Abp.TenantManagement;

namespace SmartParking.Entities
{
    public class TenantConnectionString : Entity<Guid>
    {
        public Guid TenantId { get; set; }
        public string IpAddress { get; set; }
        public string Port { get; set; } 
        public string DatabaseName { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public string ConnectionString  { get; set; }
        public virtual Tenant Tenant { get; set; }


        public TenantConnectionString
                                          (
                                               Guid id
                                             , Guid tenantId
                                             , string ipAddress
                                             , string port
                                             , string databaseName
                                             , string userName
                                             , string password
                                           )
        {
            TenantId = tenantId;
            IpAddress = ipAddress;
            Port = port;
            DatabaseName = databaseName;
            UserName = userName;
            Password = password;
            ConnectionString = $"Server={ipAddress};Port={port};Database={databaseName};Username={userName};Password={password};Include Error Detail=true";
            Port = port;
        }
    }
}
