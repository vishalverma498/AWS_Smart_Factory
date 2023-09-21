using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json;

namespace smart_factory_AWS.Controllers
{
    [Route("/Quality_Control_Dispatching")]
    [ApiController]
    public class Quality_Control_Dispatching2Controller : ControllerBase
    {

        private readonly IConfiguration _configuration;

        public Quality_Control_Dispatching2Controller(IConfiguration configuration)
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

        private void UpdateQuality_Control_Dispatching2CustomerIds(long customer_Id)
        {
            using (var connection = new SqlConnection(_configuration.GetConnectionString("EmployeeDatabase")))
            {
                var sql = $"UPDATE Quality_Control_Dispatching2 " +
                          $"SET Customer_Id = '{customer_Id}' " +
                          $"WHERE id = (SELECT MAX(id) FROM Quality_Control_Dispatching2)";
                connection.Open();
                using SqlCommand command = new SqlCommand(sql, connection);
                command.ExecuteNonQuery();
            }
        }


        private IEnumerable<Quality_Control_Dispatching2> GetQuality_Control_Dispatching2s()
        {
            var Quality_Control_Dispatching2s = new List<Quality_Control_Dispatching2>();

            using (var connection = new SqlConnection(_configuration.GetConnectionString("EmployeeDatabase")))
            {
                // var sql = "SELECT id, RawMaterial_Defects, Filling_Defects, Lablling_Defects, date_field, Customer_Id FROM Defects_Table";
                var sql = "SELECT TOP 1 * FROM Quality_Control_Dispatching2 ORDER BY id DESC";
                connection.Open();
                using SqlCommand command = new SqlCommand(sql, connection);
                using SqlDataReader reader = command.ExecuteReader();

                while (reader.Read())
                {
                    var Quality_Control_Dispatching2 = new Quality_Control_Dispatching2()
                    {
                        id = (long)reader["id"],

                        Img = reader["Img"].ToString(),
                        Final_Result = reader["Final_Result"].ToString(),
                        date_field = (DateTime)reader["date_field"],
                        Customer_Id = reader["Customer_Id"].ToString(),
                        Batch_No = reader["Batch_No"].ToString(),
                        Website = reader["Website"].ToString(),
                        Vendor_Name = reader["Vendor_Name"].ToString(),
                        Product_Status = reader["Product_Status"].ToString(),
                        Color = reader["Color"].ToString()

                    };


                    Quality_Control_Dispatching2s.Add(Quality_Control_Dispatching2);
                }
            }

            return Quality_Control_Dispatching2s;
        }

        [HttpGet]
        public IActionResult Get()
        {
            var latestOrder = GetLatestOrder();
            if (latestOrder.Current_status == "Pending" || latestOrder.Current_status == "Active" || latestOrder.Current_status == "pending" || latestOrder.Current_status == "active")
            {
                UpdateQuality_Control_Dispatching2CustomerIds(latestOrder.Customer_Id);
                var Quality_Control_Dispatching2s = GetQuality_Control_Dispatching2s();

                var response = Quality_Control_Dispatching2s.Select(q => new
                {
                    q.id,
                    q.Img,
                    q.Final_Result,
                    q.Product_Status,
                    q.Customer_Id,
                    QR_Code = new
                    {
                        Batch_No = q.Batch_No,
                        Website = q.Website,
                        Vendor_Name = q.Vendor_Name,
                        Color = q.Color
                    }
                });
                return new JsonResult(response);
            }
            else
            {
                var Quality_Control_Dispatching2s = GetQuality_Control_Dispatching2s();

                var response = Quality_Control_Dispatching2s.Select(q => new
                {
                    q.id,
                    q.Img,
                    q.Final_Result,
                    q.Product_Status,
                    q.Customer_Id,
                    QR_Code = new
                    {
                        Batch_No = q.Batch_No,
                        Website = q.Website,
                        Vendor_Name = q.Vendor_Name,
                        Color = q.Color
                    }
                });
                return new JsonResult(response);
            }
        }

        private string GenerateQRCode(Quality_Control_Dispatching2 dispatching)
        {
            var qrCodeData = new
            {
                Batch_No = dispatching.Batch_No,
                Website = dispatching.Website,
                Vendor_Name = dispatching.Vendor_Name
            };

            var json = JsonConvert.SerializeObject(qrCodeData);

            // Code to generate QR code using the JSON data

            return json;
        }

        [HttpPut("{customerId}")]
        public IActionResult UpdateQualityControlDispatching(long customerId, [FromBody] Quality_Control_Dispatching2 updateDto)
        {
            using (var connection = new SqlConnection(_configuration.GetConnectionString("EmployeeDatabase")))
            {
                var getCurrentValuesSql = $"SELECT Img, Final_Result FROM Quality_Control_Dispatching2 WHERE Customer_Id = '{customerId}'";
                connection.Open();
                using SqlCommand getCurrentValuesCommand = new SqlCommand(getCurrentValuesSql, connection);
                using SqlDataReader reader = getCurrentValuesCommand.ExecuteReader();

                if (reader.Read())
                {
                    reader.Close();

                    var sql = $"UPDATE Quality_Control_Dispatching2 " +
                              $"SET Img = '{updateDto.Img}', " +
                              $"Final_Result = '{updateDto.Final_Result}', " +
                              $"Color = '{updateDto.Color}', " +
                              $"Batch_No = '{updateDto.Batch_No}', " +
                              $"Website = '{updateDto.Website}', " +
                              $"Vendor_Name = '{updateDto.Vendor_Name}', " +
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
        public IActionResult UpdateProductStatus(long customerId, [FromBody] Quality_Control_Dispatching2 updateDto)
        {
            using (var connection = new SqlConnection(_configuration.GetConnectionString("EmployeeDatabase")))
            {
                var getCurrentStatusSql = $"SELECT Product_Status FROM Quality_Control_Dispatching2 WHERE Customer_Id = '{customerId}'";
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
                        var sql = $"UPDATE Quality_Control_Dispatching2 " +
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

