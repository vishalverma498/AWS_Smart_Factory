using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace smart_factory_AWS.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class Quality_Control_FillingController : ControllerBase
    {

        private readonly IConfiguration _configuration;

        public Quality_Control_FillingController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        private IEnumerable<Quality_Control_Filling> GetQuality_Control_Fillings()
        {
            var Quality_Control_Fillings = new List<Quality_Control_Filling>();

            using (var connection = new SqlConnection(_configuration.GetConnectionString("EmployeeDatabase")))
            {
                // var sql = "SELECT id, RawMaterial_Defects, Filling_Defects, Lablling_Defects, date_field, Customer_Id FROM Defects_Table";
                var sql = "SELECT TOP 1 * FROM Quality_Control_Filling ORDER BY id DESC";
                connection.Open();
                using SqlCommand command = new SqlCommand(sql, connection);
                using SqlDataReader reader = command.ExecuteReader();

                while (reader.Read())
                {
                    var Quality_Control_Filling = new Quality_Control_Filling()
                    {
                        id = (long)reader["id"],
                        //Img = reader["Img"].ToString(),
                        Img = reader["Img"].ToString(),
                        Product_Status = reader["Product_Status"].ToString(),
                        Color = reader["Color"].ToString(),
                        Final_Result = reader["Final_Result"].ToString(),
                        date_field = (DateTime)reader["date_field"],
                        Customer_Id = reader["Customer_Id"].ToString(),

                    };

                    Quality_Control_Fillings.Add(Quality_Control_Filling);
                }
            }

            return Quality_Control_Fillings;
        }

        [HttpGet]
        public IActionResult Get()
        {
            var Quality_Control_Fillings = GetQuality_Control_Fillings();
            return new JsonResult(Quality_Control_Fillings);

        }

        [HttpPut("{customerId}")]
        public IActionResult UpdateQualityControlFilling(long customerId, [FromBody] Quality_Control_Filling updateDto)
        {
            using (var connection = new SqlConnection(_configuration.GetConnectionString("EmployeeDatabase")))
            {
                // Retrieve the current values of Img and Final_Result from the database
                var getCurrentValuesSql = $"SELECT Img, Final_Result FROM Quality_Control_Filling WHERE Customer_Id = '{customerId}'";
                connection.Open();
                using SqlCommand getCurrentValuesCommand = new SqlCommand(getCurrentValuesSql, connection);
                using SqlDataReader reader = getCurrentValuesCommand.ExecuteReader();

                if (reader.Read())
                {
                    reader.Close();

                    var sql = $"UPDATE Quality_Control_Filling " +
                              $"SET Img = '{updateDto.Img}', " +
                              $"Final_Result = '{updateDto.Final_Result}', " +
                              $"Color = '{updateDto.Color}', " +
                              $"Product_Status = '{updateDto.Product_Status}' " +
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
        }

        [HttpPut("UpdateProductStatus/{customerId}")]
        public IActionResult UpdateProductStatus(long customerId, [FromBody] Quality_Control_Filling updateDto)
        {
            using (var connection = new SqlConnection(_configuration.GetConnectionString("EmployeeDatabase")))
            {
                var getCurrentStatusSql = $"SELECT Product_Status FROM Quality_Control_Filling WHERE Customer_Id = '{customerId}'";
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
                        var sql = $"UPDATE Quality_Control_Filling " +
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

