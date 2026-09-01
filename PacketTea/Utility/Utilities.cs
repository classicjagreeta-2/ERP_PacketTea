using System;
using System.Collections.Generic;
using System.Data.OleDb;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Web;
using CrystalDecisions.CrystalReports.Engine;
using CrystalDecisions.Shared;

namespace PacketTea.Utility
{
    public static class Utilities
    {
        private static string ConvertDataTableToHTML(DataTable dt)
        {
            string html = "<table>";
            //add header row
            html += "<tr>";
            for (int i = 0; i < dt.Columns.Count; i++)
                html += "<td>" + dt.Columns[i].ColumnName + "</td>";
            html += "</tr>";
            //add rows
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                html += "<tr>";
                for (int j = 0; j < dt.Columns.Count; j++)
                    html += "<td>" + dt.Rows[i][j].ToString() + "</td>";
                html += "</tr>";
            }
            html += "</table>";
            return html;
        }
        public static DataTable ConvertCSVtoDataTable(string strFilePath)
        {
            DataTable dt = new DataTable();
            using (StreamReader sr = new StreamReader(strFilePath))
            {
                string[] headers = sr.ReadLine().Split(',');
                foreach (string header in headers)
                {
                    dt.Columns.Add(header);
                }

                while (!sr.EndOfStream)
                {
                    string[] rows = sr.ReadLine().Split(',');
                    if (rows.Length > 1)
                    {
                        DataRow dr = dt.NewRow();
                        for (int i = 0; i < headers.Length; i++)
                        {
                            dr[i] = rows[i].Trim();
                        }
                        dt.Rows.Add(dr);
                    }
                }
            }

            return dt;
        }

        public static DataTable ConvertXSLXtoDataTable(string strFilePath, string connString)
        {
            OleDbConnection oledbConn = new OleDbConnection(connString);
            DataTable dt = new DataTable();
            try
            {
                oledbConn.Open();
                DataTable dbSchema = oledbConn.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, null);
                string firstSheetName = dbSchema.Rows[0]["TABLE_NAME"].ToString();
                //using (OleDbCommand cmd = new OleDbCommand("SELECT * FROM [Sheet1$]", oledbConn))
                using (OleDbCommand cmd = new OleDbCommand("SELECT * FROM [" + firstSheetName + "]", oledbConn))
                {
                    OleDbDataAdapter oleda = new OleDbDataAdapter();
                    oleda.SelectCommand = cmd;
                    DataSet ds = new DataSet();
                    oleda.Fill(ds);

                    dt = ds.Tables[0];
                }
            }
            catch
            {
                // Handle exceptions
            }
            finally
            {
                oledbConn.Close();
            }

            return dt;
        }

        public static List<T> ConvertDataTable<T>(DataTable dt)
        {
            List<T> data = new List<T>();
            foreach (DataRow row in dt.Rows)
            {
                T item = GetItem<T>(row);
                data.Add(item);
            }
            return data;
        }

        private static T GetItem<T>(DataRow dr)
        {
            Type temp = typeof(T);
            T obj = Activator.CreateInstance<T>();

            foreach (DataColumn column in dr.Table.Columns)
            {
                foreach (PropertyInfo pro in temp.GetProperties())
                {
                    if (pro.Name == column.ColumnName)
                        try
                        {
                            pro.SetValue(obj, dr[column.ColumnName], null);
                        }
                        catch (Exception)
                        {
                            pro.SetValue(obj, "", null);
                        }
                    else
                        continue;
                }
            }
            return obj;
        }
        public static Dictionary<object, IList<dynamic>> DataTable2Dictionary(DataTable dt)
        {
            Dictionary<object, IList<dynamic>> dict = new Dictionary<dynamic, IList<dynamic>>();

            foreach (DataColumn column in dt.Columns)
            {
                IList<dynamic> ts = dt.AsEnumerable()
                                      .Select(r => r.Field<dynamic>(column.ToString()))
                                      .ToList();
                dict.Add(column, ts);
            }
            return dict;
        }

        public static void GenerateXsd<T>(string xsdPath, string tableName)
        {
            try
            {
                DataTable dt = new DataTable(tableName);
                foreach (PropertyInfo prop in typeof(T).GetProperties())
                {
                    Type type = Nullable.GetUnderlyingType(prop.PropertyType)
                                ?? prop.PropertyType;
                    dt.Columns.Add(prop.Name, type);
                }
                DataSet ds = new DataSet("ReportDataSet");
                ds.Tables.Add(dt);
                ds.WriteXmlSchema(xsdPath);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }



        public static DataSet CreateDataSetFromReport(string reportPath)
        {
            ReportDocument rpt = new ReportDocument();
            rpt.Load(reportPath);
            DataSet ds = new DataSet("ReportDataSet");
            foreach (Table table in rpt.Database.Tables)
            {
                DataTable dt = new DataTable(table.Name);
                foreach (DatabaseFieldDefinition field in table.Fields)
                {
                    // Default to string because Crystal doesn't reliably expose .NET types
                    dt.Columns.Add(field.Name, typeof(string));
                }
                ds.Tables.Add(dt);
            }
            rpt.Close();
            rpt.Dispose();
            return ds;
        }

        public static void ExportReportSchemaToXsd(string rptFilePath, string outputXsdPath)
        {
            ReportDocument reportDoc = new ReportDocument();
            reportDoc.Load(rptFilePath);

            DataSet schemaDataSet = new DataSet("CrystalReportSchema");

            // Iterate through all tables defined in the Crystal Report
            foreach (Table crTable in reportDoc.Database.Tables)
            {
                DataTable dt = new DataTable(crTable.Name);

                // Iterate through every database field in the table
                foreach (DatabaseFieldDefinition field in crTable.Fields)
                {
                    Type dotnetType = MapCrystalTypeToDotNet(field.ValueType);

                    DataColumn column = new DataColumn(field.Name, dotnetType)
                    {
                        AllowDBNull = true
                    };

                    dt.Columns.Add(column);
                }

                schemaDataSet.Tables.Add(dt);
            }

            // Export the dataset schema structure directly to an XSD file
            schemaDataSet.WriteXmlSchema(outputXsdPath);

            reportDoc.Close();
            reportDoc.Dispose();
        }

        private static Type MapCrystalTypeToDotNet(FieldValueType crystalType)
        {
            switch (crystalType)
            {
                case FieldValueType.StringField:
                    return typeof(string);

                case FieldValueType.Int8sField:
                case FieldValueType.Int8uField:
                case FieldValueType.Int16sField:
                case FieldValueType.Int16uField:
                    return typeof(short);

                case FieldValueType.Int32sField:
                case FieldValueType.Int32uField:
                    return typeof(int);

                case FieldValueType.NumberField:
                case FieldValueType.CurrencyField:
                    return typeof(decimal);

                case FieldValueType.BooleanField:
                    return typeof(bool);

                case FieldValueType.DateField:
                case FieldValueType.DateTimeField:
                case FieldValueType.TimeField:
                    return typeof(DateTime);

                case FieldValueType.BlobField:
                    return typeof(byte[]);

                default:
                    return typeof(string);
            }
        }

    }
}