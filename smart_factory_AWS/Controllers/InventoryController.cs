using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json;

namespace smart_factory_AWS.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class InventoryController : ControllerBase
    {

        private readonly IConfiguration _configuration;

        public InventoryController(IConfiguration configuration)
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

        private void UpdateInventoryCustomerIds(long customer_Id)
        {
            using (var connection = new SqlConnection(_configuration.GetConnectionString("EmployeeDatabase")))
            {
                var sql = $"UPDATE Inventory " +
                          $"SET Customer_Id = '{customer_Id}' " +
                          $"WHERE id = (SELECT MAX(id) FROM Inventory)";
                connection.Open();
                using SqlCommand command = new SqlCommand(sql, connection);
                command.ExecuteNonQuery();
            }
        }

        private IEnumerable<Inventory> GetInventorys()
        {
            var Inventorys = new List<Inventory>();

            using (var connection = new SqlConnection(_configuration.GetConnectionString("EmployeeDatabase")))
            {
                // var sql = "SELECT id, RawMaterial_Defects, Filling_Defects, Lablling_Defects, date_field, Customer_Id FROM Defects_Table";
                var sql = "SELECT id, Inventoroy_position, Customer_Name, Product_Color, Batch_No, Delivery_city, Customer_Id FROM Inventory";
                //var sql = "SELECT TOP 1 * FROM Inventory ORDER BY id DESC";
                connection.Open();
                //using SqlCommand command = new SqlCommand(sql, connection);
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var id = (long)reader["id"];

                            if (id > 9)
                            {
                                reader.Close();
                                ResetTable(connection);
                                break;
                            }

                            var Inventory = new Inventory()
                            {
                                id = (long)reader["id"],
                                Inventoroy_position = reader["Inventoroy_position"].ToString(),
                                Customer_Name = reader["Customer_Name"].ToString(),
                                Product_Color = reader["Product_Color"].ToString(),
                                Batch_No = reader["Batch_No"].ToString(),
                                Delivery_city = reader["Delivery_city"].ToString(),
                                Customer_Id = reader["Customer_Id"].ToString()
                            };

                            Inventorys.Add(Inventory);
                        }
                    }
                }
            }
            return Inventorys;
        }

        private void ResetTable(SqlConnection connection)
        {
            var resetSql = "TRUNCATE TABLE Inventory"; // Reset the table by truncating it

            using (SqlCommand resetCommand = new SqlCommand(resetSql, connection))
            {
                resetCommand.ExecuteNonQuery();
            }
        }

        [HttpGet]
        public IActionResult Get()
        {
            var latestOrder = GetLatestOrder();
            if (latestOrder.Current_status == "Pending" || latestOrder.Current_status == "Active" || latestOrder.Current_status == "pending" || latestOrder.Current_status == "active")
            {
                UpdateInventoryCustomerIds(latestOrder.Customer_Id);
                var Inventorys = GetInventorys();
                return new JsonResult(Inventorys);
            }
            else
            {
                var Inventorys = GetInventorys();
                return new JsonResult(Inventorys);
            }
        }


        [HttpPost]
        public IActionResult Post([FromBody] Inventory inventory)
        {
            if (inventory == null)
            {
                return BadRequest("Invalid data");
            }
            int inventoryPosition = 0;

            using (var connection = new SqlConnection(_configuration.GetConnectionString("EmployeeDatabase")))
            {
                connection.Open();

                // Get the latest row from the Table3 API
                var latestRowUrl = "https://smartdashboardapi.azurewebsites.net/api/Table3/LatestRow";
                using (var httpClient = new HttpClient())
                {
                    var latestRowResponse = httpClient.GetAsync(latestRowUrl).Result;
                    if (latestRowResponse.IsSuccessStatusCode)
                    {
                        var latestRowContent = latestRowResponse.Content.ReadAsStringAsync().Result;
                        var latestRow = JsonConvert.DeserializeObject<Table3>(latestRowContent);

                        // Count the number of positions with a value of 1 in Table3
                        int filledPositionsCount = 0;
                        inventoryPosition = 0;

                        for (int i = 1; i <= 9; i++)
                        {
                            var property = typeof(Table3).GetProperty("inventory_Position_" + i);
                            var positionValue = property?.GetValue(latestRow)?.ToString();
                            if (positionValue == "1")
                            {
                                filledPositionsCount++;
                                Console.WriteLine($"Position{i} value: {positionValue}");
                                inventoryPosition = filledPositionsCount + 1;

                            }
                        }

                        // Get the number of rows in the Inventory table
                        var inventoryRowCountSql = "SELECT COUNT(*) FROM Inventory";
                        using (var rowCountCommand = new SqlCommand(inventoryRowCountSql, connection))
                        {
                            int inventoryRowCount = (int)rowCountCommand.ExecuteScalar();

                            if (filledPositionsCount < inventoryRowCount)
                            {
                                // Delete excess rows in the Inventory table
                                var deleteExcessRowsSql = $"DELETE FROM Inventory WHERE id > {filledPositionsCount}";
                                using (var deleteCommand = new SqlCommand(deleteExcessRowsSql, connection))
                                {
                                    deleteCommand.ExecuteNonQuery();
                                }
                                var resetIdSql = $"DBCC CHECKIDENT('Inventory', RESEED, {filledPositionsCount})";
                                using (SqlCommand resetIdCommand = new SqlCommand(resetIdSql, connection))
                                {
                                    resetIdCommand.ExecuteNonQuery();
                                }
                            }
                            else if (filledPositionsCount > inventoryRowCount)
                            {
                                // Truncate the Inventory table
                                var truncateTableSql = "TRUNCATE TABLE Inventory";
                                using (var truncateCommand = new SqlCommand(truncateTableSql, connection))
                                {
                                    truncateCommand.ExecuteNonQuery();
                                }
                                var resetIdSql = "DBCC CHECKIDENT('Inventory', RESEED, 1)";
                                using (SqlCommand resetIdCommand = new SqlCommand(resetIdSql, connection))
                                {
                                    resetIdCommand.ExecuteNonQuery();
                                }
                            }
                        }
                        if (inventoryPosition == 0)
                        {
                            inventoryPosition = 1;
                        }
                        // Insert the new inventory record
                        var sql = $"INSERT INTO Inventory (Inventoroy_position, Customer_Name, Product_Color, Batch_No, Delivery_city, Customer_Id) " +
                                  $"VALUES ('{inventoryPosition}', '{inventory.Customer_Name}', '{inventory.Product_Color}', '{inventory.Batch_No}', '{inventory.Delivery_city}', '{inventory.Customer_Id}')";

                        using (SqlCommand command = new SqlCommand(sql, connection))
                        {
                            command.ExecuteNonQuery();
                        }
                    }
                }
            }

            return Ok();
        }

    }
}

