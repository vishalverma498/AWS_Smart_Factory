using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace smart_factory_AWS.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class Table1Controller : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public Table1Controller(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpGet("LatestRow")]
        public IActionResult GetLatestRow()
        {
            using (var connection = new SqlConnection(_configuration.GetConnectionString("EmployeeDatabase")))
            {
                var sql = "SELECT TOP 1 * FROM Table1 ORDER BY id DESC";
                connection.Open();
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            var table1 = new Table1
                            {
                                id = (long)reader["id"],
                                loading_Photo_Sensor = reader["loadsensor"].ToString(),
                                loading_Camera_Sensor = reader["CameraSensor"].ToString(),
                                loading_Stopper = reader["StoperStatus"].ToString(),
                                conveyor_Status = reader["ConveyorStatus"].ToString(),
                                loading_Flipper = reader["FLipperStatus"].ToString(),
                                Customer_Id = reader["Customer_Id"].ToString()
                            };

                            return Ok(table1);
                        }
                        else
                        {
                            return NotFound(); // No rows found in the table
                        }
                    }
                }
            }
        }
    }
}

