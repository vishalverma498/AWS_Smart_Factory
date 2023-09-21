using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace smart_factory_AWS.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class Table3Controller : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public Table3Controller(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpGet("LatestRow")]
        public IActionResult GetLatestRow()
        {
            using (var connection = new SqlConnection(_configuration.GetConnectionString("EmployeeDatabase")))
            {
                var sql = "SELECT TOP 1 * FROM Table3 ORDER BY id DESC";
                connection.Open();
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            var table3 = new Table3
                            {
                                id = (long)reader["id"],
                                inventory_Position_1 = reader["Position1"].ToString(),
                                inventory_Position_2 = reader["Position2"].ToString(),
                                inventory_Position_3 = reader["Position3"].ToString(),
                                inventory_Position_4 = reader["Position4"].ToString(),
                                inventory_Position_5 = reader["Position5"].ToString(),
                                inventory_Position_6 = reader["Position6"].ToString(),
                                inventory_Position_7 = reader["Position7"].ToString(),
                                inventory_Position_8 = reader["Position8"].ToString(),
                                inventory_Position_9 = reader["Position9"].ToString(),
                                dispatching_Cap_Sensor = reader["CapSensor"].ToString(),
                                gantory_Sensor = reader["GantrySensor"].ToString(),
                                Customer_Id = reader["Customer_Id"].ToString()
                            };

                            return Ok(table3);
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

