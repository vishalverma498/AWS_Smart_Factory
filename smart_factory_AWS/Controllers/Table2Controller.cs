using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace smart_factory_AWS.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class Table2Controller : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public Table2Controller(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpGet("LatestRow")]
        public IActionResult GetLatestRow()
        {
            using (var connection = new SqlConnection(_configuration.GetConnectionString("EmployeeDatabase")))
            {
                var sql = "SELECT TOP 1 * FROM Table2 ORDER BY id DESC";
                connection.Open();
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            var table2 = new Table2
                            {
                                id = (long)reader["id"],
                                turnTable_Home_Sensor = reader["HomeSensor"].ToString(),
                                filling_Sensor = reader["FillingSensor"].ToString(),
                                t2Vision_Sensor = reader["VisionSensor"].ToString(),
                                push_Out_Sensor = reader["PushOutSensor"].ToString(),
                                filling_Motor_Yellow = reader["Motor1"].ToString(),
                                filling_Motor_Pink = reader["Motor2"].ToString(),
                                filling_Motor_Red = reader["Motor3"].ToString(),
                                turnTable_Motor = reader["TurnTableMotor"].ToString(),
                                vision_Flipper = reader["VisionFlipper"].ToString(),
                                push_Flipper = reader["PushFlipper"].ToString(),
                                Customer_Id = reader["Customer_Id"].ToString()
                            };

                            return Ok(table2);
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

