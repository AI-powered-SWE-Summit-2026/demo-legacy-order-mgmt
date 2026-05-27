using LegacyOrderMgmt.Core.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Data;
using System.IO;
using System.Text;

namespace LegacyOrderMgmt.Web.Services
{
    public class ReportExportService
    {
        private readonly OrderDbContext _db;

        public ReportExportService(OrderDbContext db)
        {
            _db = db;
        }

        // Legacy: DataSet/DataTable for report generation — ADO.NET holdover
        // Modern approach: projections to DTOs + CSV/Excel library
        public byte[] GenerateSalesReportCsv(DateTime from, DateTime to)
        {
            var fromStr = from.ToString("yyyy-MM-dd");
            var toStr = to.ToString("yyyy-MM-dd");

            // Legacy: raw SQL string concatenation for report query
            var sql = "SELECT o.OrderNumber, c.CompanyName, o.OrderDate, o.TotalAmount, o.Status " +
                      "FROM Orders o JOIN Customers c ON o.CustomerId = c.Id " +
                      "WHERE o.OrderDate >= '" + fromStr + "' AND o.OrderDate <= '" + toStr + "' " +
                      "ORDER BY o.OrderDate";

            // Legacy: DataTable populated via ADO.NET directly — bypasses EF entirely
            var dt = new DataTable();
            var conn = _db.Database.GetDbConnection();

            try
            {
                conn.Open();
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = sql;
                    using (var adapter = new System.Data.SqlClient.SqlDataAdapter((System.Data.SqlClient.SqlCommand)cmd))
                    {
                        adapter.Fill(dt);
                    }
                }
            }
            finally
            {
                conn.Close();
            }

            // Legacy: manual CSV construction via StringBuilder — no library, no proper quoting
            var sb = new StringBuilder();
            sb.AppendLine("OrderNumber,Customer,OrderDate,Total,Status");

            foreach (DataRow row in dt.Rows)
            {
                sb.AppendLine($"{row["OrderNumber"]},{row["CompanyName"]},{row["OrderDate"]:yyyy-MM-dd},{row["TotalAmount"]},{row["Status"]}");
            }

            return Encoding.UTF8.GetBytes(sb.ToString());
        }

        // Legacy: Windows path construction — not cross-platform
        public string GetReportFilePath(string reportName)
        {
            var basePath = @"C:\OrderReports\";
            if (!Directory.Exists(basePath))
                Directory.CreateDirectory(basePath);

            return Path.Combine(basePath, $"{reportName}_{DateTime.Now:yyyyMMdd_HHmmss}.csv");
        }
    }
}
