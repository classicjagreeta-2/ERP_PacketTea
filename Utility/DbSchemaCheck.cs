using PacketTea.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace PacketTea.Utility
{
    public static class DbSchemaCheck
    {
        public const string SalesSchemaMessage =
            "Sales schema is not defined in the Control file. Please check the Control file";

        // Calls API Common/checkdb for the given schema name.
        public static async Task<bool> IsSchemaValidAsync(string schema)
        {
            if (string.IsNullOrWhiteSpace(schema))
                return false;

            var chk = await Services.GetAsync<object>(
                $"/api/Common/checkdb?dbName={Uri.EscapeDataString(schema.Trim())}");

            return chk != null && chk.IsSuccessStatusCode && chk.Data != null;
        }

        // Reads SCHEMA_SALES from the logged-in user and checks it.
        // No schema configured = nothing to check, treated as valid.
        public static async Task<bool> IsSalesSchemaValidAsync()
        {
            var user = SessionHelper.GetUser();
            string schema = user?.ClassicControl?.FirstOrDefault()?.SCHEMA_SALES;

            if (string.IsNullOrEmpty(schema))
                return true;

            return await IsSchemaValidAsync(schema);
        }
    }
}