using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace smart_factory_AWS.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class DispatchingController
    {

        private readonly IConfiguration _configuration;

        public DispatchingController(IConfiguration configuration)
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

        private void UpdateDispatchingCustomerIds(long customer_Id)
        {
            using (var connection = new SqlConnection(_configuration.GetConnectionString("EmployeeDatabase")))
            {
                var sql = $"UPDATE Dispatching " +
                          $"SET Customer_Id = '{customer_Id}' " +
                          $"WHERE id = (SELECT MAX(id) FROM Dispatching)";
                connection.Open();
                using SqlCommand command = new SqlCommand(sql, connection);
                command.ExecuteNonQuery();
            }
        }

        private IEnumerable<Dispatching> GetDispatchings()
        {
            var Dispatchings = new List<Dispatching>();

            using (var connection = new SqlConnection(_configuration.GetConnectionString("EmployeeDatabase")))
            {
                // var sql = "SELECT id, RawMaterial_Defects, Filling_Defects, Lablling_Defects, date_field, Customer_Id FROM Defects_Table";
                var sql = "SELECT TOP 1 * FROM Dispatching ORDER BY id DESC";
                connection.Open();
                using SqlCommand command = new SqlCommand(sql, connection);
                using SqlDataReader reader = command.ExecuteReader();

                while (reader.Read())
                {
                    var Dispatching = new Dispatching()
                    {
                        id = (long)reader["id"],
                        IN3V = reader["IN3V"].ToString(),
                        IN3C = reader["IN3C"].ToString(),
                        IN3P = reader["IN3P"].ToString(),
                        V3 = reader["V3"].ToString(),
                        C3 = reader["C3"].ToString(),
                        P3 = reader["P3"].ToString(),
                        T3 = reader["T3"].ToString(),
                        VB3 = reader["VB3"].ToString(),
                        Customer_Id = reader["Customer_Id"].ToString(),

                    };

                    Dispatchings.Add(Dispatching);
                }
            }

            return Dispatchings;
        }

        [HttpGet]
        public IActionResult Get()
        {
            var latestOrder = GetLatestOrder();
            if (latestOrder.Current_status == "Pending" || latestOrder.Current_status == "Active" || latestOrder.Current_status == "pending" || latestOrder.Current_status == "active")
            {
                UpdateDispatchingCustomerIds(latestOrder.Customer_Id);
                var Dispatchings = GetDispatchings();
                return new JsonResult(Dispatchings);
            }
            else
            {
                var Dispatchings = GetDispatchings();
                return new JsonResult(Dispatchings);
            }
        }
    }
}

