using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data;
using System.Linq;
using System.Web;
using System.Configuration;

namespace PacketTea
{
    public static class DbFactory
    {
        public static IDbConnection CreateConnection()
        {
            string provider = ConfigurationManager.AppSettings["DatabaseType"];

            string connName = provider.ToUpper() == "Oracle".ToUpper()
                ? "OracleDb"
                : "SqlServerDb";

            var connStrSettings =
                ConfigurationManager.ConnectionStrings[connName];

            DbProviderFactory factory =
                DbProviderFactories.GetFactory(connStrSettings.ProviderName);

            DbConnection conn = factory.CreateConnection();
            conn.ConnectionString = connStrSettings.ConnectionString;

            return conn;
        }
    }
}