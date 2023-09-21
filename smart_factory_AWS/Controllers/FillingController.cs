using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace smart_factory_AWS.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class FillingController
    {

        private readonly IConfiguration _configuration;

        public FillingController(IConfiguration configuration)
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
                        // Other properties from OrderTable
                    };

                    return orderTable;
                }
            }

            return null;
        }

        private void UpdateFillingCustomerIds(long customer_Id)
        {
            using (var connection = new SqlConnection(_configuration.GetConnectionString("EmployeeDatabase")))
            {
                var sql = $"UPDATE Filling " +
                          $"SET Customer_Id = '{customer_Id}' " +
                          $"WHERE id = (SELECT MAX(id) FROM Filling)";
                connection.Open();
                using SqlCommand command = new SqlCommand(sql, connection);
                command.ExecuteNonQuery();
            }
        }

        private IEnumerable<Filling> GetFillings()
        {
            var Fillings = new List<Filling>();

            using (var connection = new SqlConnection(_configuration.GetConnectionString("EmployeeDatabase")))
            {
                // var sql = "SELECT id, RawMaterial_Defects, Filling_Defects, Lablling_Defects, date_field, Customer_Id FROM Defects_Table";
                var sql = "SELECT TOP 1 * FROM Filling ORDER BY id DESC";
                connection.Open();
                using SqlCommand command = new SqlCommand(sql, connection);
                using SqlDataReader reader = command.ExecuteReader();

                while (reader.Read())
                {
                    var Filling = new Filling()
                    {
                        id = (long)reader["id"],
                        IN2V = reader["IN2V"].ToString(),
                        IN2C = reader["IN2C"].ToString(),
                        IN2P = reader["IN2P"].ToString(),
                        V2 = reader["V2"].ToString(),
                        C2 = reader["C2"].ToString(),
                        P2 = reader["P2"].ToString(),
                        T2 = reader["T2"].ToString(),
                        VB2 = reader["VB2"].ToString(),
                        Customer_Id = reader["Customer_Id"].ToString(),
                    };

                    Fillings.Add(Filling);
                }
            }

            return Fillings;
        }

        [HttpGet]
        public IActionResult Get()
        {
            var latestOrder = GetLatestOrder();
            if (latestOrder.Current_status == "Pending" || latestOrder.Current_status == "Active" || latestOrder.Current_status == "pending" || latestOrder.Current_status == "active")
            {
                UpdateFillingCustomerIds(latestOrder.Customer_Id);
                var Fillings = GetFillings();
                return new JsonResult(Fillings);
            }
            else
            {
                var Fillings = GetFillings();
                return new JsonResult(Fillings);
            }
        }
    }
}

