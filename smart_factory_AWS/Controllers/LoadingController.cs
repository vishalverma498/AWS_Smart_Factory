using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace smart_factory_AWS.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class LoadingController
    {

        private readonly IConfiguration _configuration;

        public LoadingController(IConfiguration configuration)
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

        private void UpdateLoadingCustomerIds(long customer_Id)
        {
            using (var connection = new SqlConnection(_configuration.GetConnectionString("EmployeeDatabase")))
            {
                var sql = $"UPDATE Loading " +
                          $"SET Customer_Id = '{customer_Id}' " +
                          $"WHERE id = (SELECT MAX(id) FROM Loading)";
                connection.Open();
                using SqlCommand command = new SqlCommand(sql, connection);
                command.ExecuteNonQuery();
            }
        }

        private IEnumerable<Loading> GetLoadings()
        {
            var Loadings = new List<Loading>();

            using (var connection = new SqlConnection(_configuration.GetConnectionString("EmployeeDatabase")))
            {
                // var sql = "SELECT id, RawMaterial_Defects, Filling_Defects, Lablling_Defects, date_field, Customer_Id FROM Defects_Table";
                var sql = "SELECT TOP 1 * FROM Loading ORDER BY id DESC";
                connection.Open();
                using SqlCommand command = new SqlCommand(sql, connection);
                using SqlDataReader reader = command.ExecuteReader();

                while (reader.Read())
                {
                    var Loading = new Loading()
                    {
                        id = (long)reader["id"],
                        IN1V = reader["IN1V"].ToString(),
                        IN1C = reader["IN1C"].ToString(),
                        IN1P = reader["IN1P"].ToString(),
                        V1 = reader["V1"].ToString(),
                        C1 = reader["C1"].ToString(),
                        P1 = reader["P1"].ToString(),
                        T1 = reader["T1"].ToString(),
                        VB1 = reader["VB1"].ToString(),
                        Customer_Id = reader["Customer_Id"].ToString(),
                    };

                    Loadings.Add(Loading);
                }
            }

            return Loadings;
        }

        [HttpGet]
        public IActionResult Get()
        {
            var latestOrder = GetLatestOrder();
            if (latestOrder.Current_status == "Pending" || latestOrder.Current_status == "Active" || latestOrder.Current_status == "pending" || latestOrder.Current_status == "active")
            {
                UpdateLoadingCustomerIds(latestOrder.Customer_Id);
                var Loadings = GetLoadings();
                return new JsonResult(Loadings);
            }
            else
            {
                var Loadings = GetLoadings();
                return new JsonResult(Loadings);
            }
        }
    }
}

