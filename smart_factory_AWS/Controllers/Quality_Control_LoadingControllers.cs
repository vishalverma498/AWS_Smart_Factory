using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace smart_factory_AWS.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class Quality_Control_LoadingControllers : ControllerBase
    {

        private readonly IConfiguration _configuration;

        public Quality_Control_LoadingControllers(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        private IEnumerable<Quality_Control_Loading> GetQuality_Control_Loadings()
        {
            var Quality_Control_Loadings = new List<Quality_Control_Loading>();

            using (var connection = new SqlConnection(_configuration.GetConnectionString("EmployeeDatabase")))
            {
                // var sql = "SELECT id, RawMaterial_Defects, Filling_Defects, Lablling_Defects, date_field, Customer_Id FROM Defects_Table";
                var sql = "SELECT TOP 1 * FROM Quality_Control_Loading ORDER BY id DESC";
                connection.Open();
                using SqlCommand command = new SqlCommand(sql, connection);
                using SqlDataReader reader = command.ExecuteReader();

                while (reader.Read())
                {
                    var Quality_Control_Loading = new Quality_Control_Loading()
                    {
                        id = (long)reader["id"],
                        Img = reader["Img"].ToString(),
                        Product_Status = reader["Product_Status"].ToString(),
                        Final_Result = reader["Final_Result"].ToString(),
                        date_field = (DateTime)reader["date_field"],
                        Customer_Id = reader["Customer_Id"].ToString(),
                    };

                    Quality_Control_Loadings.Add(Quality_Control_Loading);
                }
            }

            return Quality_Control_Loadings;
        }

        [HttpGet]
        public IActionResult Get()
        {
            var Quality_Control_Loadings = GetQuality_Control_Loadings();
            return new JsonResult(Quality_Control_Loadings);
        }


        [HttpPut("{customerId}")]
        public IActionResult UpdateQualityControlLoading(long customerId, [FromBody] Quality_Control_Loading updateDto)
        {
            using (var connection = new SqlConnection(_configuration.GetConnectionString("EmployeeDatabase")))
            {
                var sql = $"UPDATE Quality_Control_Loading " +
                          $"SET Img = '{updateDto.Img}', " +
                          $"Product_Status = '{updateDto.Product_Status}', " +
                          $"Final_Result = '{updateDto.Final_Result}' " +
                          $"WHERE Customer_Id = '{customerId}'";

                connection.Open();
                using SqlCommand updateCommand = new SqlCommand(sql, connection);
                int rowsAffected = updateCommand.ExecuteNonQuery();

                if (rowsAffected > 0)
                {
                    return Ok(); // 200 OK response
                }
                else
                {
                    return NotFound(); // 404 Not Found response
                }
            }
        }

        [HttpPut("UpdateProductStatus/{customerId}")]
        public IActionResult UpdateProductStatus(long customerId, [FromBody] Quality_Control_Loading updateDto)
        {
            using (var connection = new SqlConnection(_configuration.GetConnectionString("EmployeeDatabase")))
            {
                var getCurrentStatusSql = $"SELECT Product_Status FROM Quality_Control_Loading WHERE Customer_Id = '{customerId}'";
                connection.Open();
                using SqlCommand getCurrentStatusCommand = new SqlCommand(getCurrentStatusSql, connection);
                using SqlDataReader reader = getCurrentStatusCommand.ExecuteReader();

                if (reader.Read())
                {
                    var currentProductStatus = reader["Product_Status"].ToString();
                    reader.Close();

                    // Check if the current Product_Status value is not empty
                    if (!string.IsNullOrEmpty(currentProductStatus))
                    {
                        var sql = $"UPDATE Quality_Control_Loading " +
                                  $"SET Product_Status = '{updateDto.Product_Status}' " +
                                  $"WHERE Customer_Id = '{customerId}'";
                        using SqlCommand updateCommand = new SqlCommand(sql, connection);
                        int rowsAffected = updateCommand.ExecuteNonQuery();

                        if (rowsAffected > 0)
                        {
                            return Ok(); // 200 OK response
                        }
                        else
                        {
                            return NotFound(); // 404 Not Found response
                        }
                    }
                    else
                    {
                        return NotFound(); // 404 Not Found response if the record doesn't exist
                    }
                }
                else
                {
                    return NotFound(); //


                }
            }
        }
    }
}

