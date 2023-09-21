using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace smart_factory_AWS.Controllers
{
    [Route("/Defects_Table")]
    [ApiController]
    public class Defects_Table_Controller : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public Defects_Table_Controller(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        private OrderTable GetLatestOrder()
        {
            using (var connection = new SqlConnection(_configuration.GetConnectionString("EmployeeDatabase")))
            {

                var sql = "SELECT TOP 1 * FROM OrderTable ORDER BY Customer_Id DESC";
                connection.Open();
                using SqlCommand command = new SqlCommand(sql, connection);
                using SqlDataReader reader = command.ExecuteReader();

                if (reader.Read())
                {
                    var orderTable = new OrderTable()
                    {
                        Customer_Id = (long)reader["Customer_Id"],
                        Current_status = reader["Current_status"].ToString(),
                        // Other properties from OrderTable /Defects_Table/defects
                    };

                    return orderTable;
                }
            }

            return null;
        }

        private void UpdateDefectsTableCustomerIds(long customer_Id)
        {
            using (var connection = new SqlConnection(_configuration.GetConnectionString("EmployeeDatabase")))
            {
                var sql = $"UPDATE Defects_Table " +
                          $"SET Customer_Id = '{customer_Id}' " +
                          $"WHERE id = (SELECT MAX(id) FROM Defects_Table)";
                connection.Open();
                using SqlCommand command = new SqlCommand(sql, connection);
                command.ExecuteNonQuery();
            }
        }

        private IEnumerable<Defects_Table> GetDefectsTables()
        {
            var defectsTables = new List<Defects_Table>();

            using (var connection = new SqlConnection(_configuration.GetConnectionString("EmployeeDatabase")))
            {
                var thirtyDaysAgoUnixTimestamp = DateTimeOffset.UtcNow.AddDays(-30).ToUnixTimeSeconds();
                var sql = $"SELECT id, RawMaterial_Defects, Filling_Defects, Lablling_Defects, date_field, Customer_Id " +
                          $"FROM Defects_Table " +
                          $"WHERE date_field >= DATEADD(day, -30, SYSUTCDATETIME())";
                connection.Open();
                using SqlCommand command = new SqlCommand(sql, connection);
                using SqlDataReader reader = command.ExecuteReader();

                while (reader.Read())
                {
                    var defectsTable = new Defects_Table()
                    {
                        id = (long)reader["id"],
                        RawMaterial_Defects = reader["RawMaterial_Defects"].ToString(),
                        Filling_Defects = reader["Filling_Defects"].ToString(),
                        Lablling_Defects = reader["Lablling_Defects"].ToString(),
                        date_field = (DateTime)reader["date_field"],
                        Customer_Id = reader["Customer_Id"].ToString()
                    };

                    defectsTables.Add(defectsTable);
                }
            }

            return defectsTables;
        }

        [HttpGet]
        public IActionResult Get()
        {
            var latestOrder = GetLatestOrder();
            var defectsTables = GetDefectsTables();
            return new JsonResult(defectsTables);

        }

        [HttpPost("defects")]
        public IActionResult CreateLabelingDefect([FromBody] Defects_Table defect)
        {
            using (var connection = new SqlConnection(_configuration.GetConnectionString("EmployeeDatabase")))
            {
                var sql = $"INSERT INTO Defects_Table (RawMaterial_Defects, Filling_Defects, Lablling_Defects, Customer_Id) " +
                          $"VALUES ('{defect.RawMaterial_Defects}', '{defect.Filling_Defects}', '{defect.Lablling_Defects}', '{defect.Customer_Id}')";

                connection.Open();
                using SqlCommand command = new SqlCommand(sql, connection);
                command.ExecuteNonQuery();
            }

            return Ok();
        }
    }
}

