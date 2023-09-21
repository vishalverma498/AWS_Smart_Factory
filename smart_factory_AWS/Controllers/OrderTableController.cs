using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace smart_factory_AWS.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class OrderTableController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public OrderTableController(IConfiguration configuration)
        {
            _configuration = configuration;
        }


        [HttpGet]
        public IEnumerable<OrderTable> Get()
        {
            var Orders = GetOrders();
            return Orders;
        }

        private IEnumerable<OrderTable> GetOrders()
        {
            var Orders = new List<OrderTable>();

            using (var connection = new SqlConnection(_configuration.GetConnectionString("EmployeeDatabase")))
            {
                //var sql = "SELECT Customer_id, Color, Quantity, Current_status, Customer_Name, Delivery_city FROM OrderTable";
                var sql = "SELECT TOP 1 * FROM OrderTable ORDER BY Customer_id DESC";
                connection.Open();
                using SqlCommand command = new SqlCommand(sql, connection);
                using SqlDataReader reader = command.ExecuteReader();

                while (reader.Read())
                {
                    var OrderTable = new OrderTable()
                    {
                        Customer_Id = (long)reader["Customer_Id"],
                        Color = reader["Color"].ToString(),
                        Quantity = reader["Quantity"].ToString(),
                        Current_status = reader["Current_status"].ToString(),
                        Customer_Name = reader["Customer_Name"].ToString(),
                        Delivery_city = reader["Delivery_city"].ToString(),
                        Order_Time = (DateTime)reader["Order_Time"],
                    };

                    Orders.Add(OrderTable);
                }
            }

            return Orders;

        }
        [HttpPost]
        [Route("/Orderpost")]
        public IActionResult Post([FromBody] OrderTable order)
        {
            var currentStatus = GetCurrentStatus();

            if (currentStatus == "Pending" || currentStatus == "pending" || currentStatus == "Active" || currentStatus == "active")
            {
                return Ok("Order in Progress");
            }
            else
            {
                InsertOrder(order);
                var customerId = GetCustomerID(); // Get the Customer_id
                string orderId = $"Order_Id_{customerId}";

                return Ok(new { order_Id = orderId });
            }
            // Insert the order into the database


            // Return a 201 Created status with the newly created order

        }
        private string GetCurrentStatus()
        {
            using (var connection = new SqlConnection(_configuration.GetConnectionString("EmployeeDatabase")))
            {
                var sql = "SELECT TOP 1 Current_status FROM OrderTable ORDER BY Customer_id DESC";
                connection.Open();
                using SqlCommand command = new SqlCommand(sql, connection);
                var currentStatus = command.ExecuteScalar()?.ToString();
                return currentStatus;
            }
        }
        private long GetCustomerID()
        {
            using (var connection = new SqlConnection(_configuration.GetConnectionString("EmployeeDatabase")))
            {
                var sql = "SELECT TOP 1 Customer_Id FROM OrderTable ORDER BY Customer_id DESC";

                connection.Open();
                using SqlCommand command = new SqlCommand(sql, connection);
                var customerId = (long)command.ExecuteScalar();

                return customerId;
            }
        }
        private void InsertOrder(OrderTable order)
        {
            using (var connection = new SqlConnection(_configuration.GetConnectionString("EmployeeDatabase")))
            {
                var sql = "INSERT INTO OrderTable (Color, Quantity, Current_Status, Customer_Name, Delivery_City) VALUES (@Color, @Quantity, @Current_Status, @Customer_Name, @Delivery_City)";

                connection.Open();
                using SqlCommand command = new SqlCommand(sql, connection);

                // Set parameter values
                command.Parameters.AddWithValue("@Color", order.Color);
                command.Parameters.AddWithValue("@Quantity", order.Quantity);
                command.Parameters.AddWithValue("@Current_Status", order.Current_status);
                command.Parameters.AddWithValue("@Customer_Name", order.Customer_Name);
                command.Parameters.AddWithValue("@Delivery_City", order.Delivery_city);

                command.ExecuteNonQuery();

                // After inserting into OrderTable, get the Customer_Id
                sql = "SELECT TOP 1 Customer_Id FROM OrderTable ORDER BY Customer_id DESC";
                command.CommandText = sql;
                long customerId = (long)command.ExecuteScalar();

                // Insert Customer_Id and Product_Status into another table (e.g., Quality_Control_Loading)
                sql = "INSERT INTO Quality_Control_Loading (Customer_Id, Product_Status) " +
                      "VALUES (@Customer_Id, @Product_Status)";
                command.CommandText = sql;

                // Set parameter values for the new insert operation
                command.Parameters.Clear();
                command.Parameters.AddWithValue("@Customer_Id", customerId);
                command.Parameters.AddWithValue("@Product_Status", "WaitingForPart"); // Replace with the appropriate value

                command.ExecuteNonQuery();
                sql = "INSERT INTO Quality_Control_Filling (Customer_Id, Product_Status) " +
                      "VALUES (@Customer_Id, @Product_Status)";
                command.CommandText = sql;

                // Set parameter values for the new insert operation
                command.Parameters.Clear();
                command.Parameters.AddWithValue("@Customer_Id", customerId);
                command.Parameters.AddWithValue("@Product_Status", "WaitingForPart"); // Replace with the appropriate value

                command.ExecuteNonQuery();

                sql = "INSERT INTO Quality_Control_Dispatching2 (Customer_Id, Product_Status) " +
                      "VALUES (@Customer_Id, @Product_Status)";
                command.CommandText = sql;

                // Set parameter values for the new insert operation
                command.Parameters.Clear();
                command.Parameters.AddWithValue("@Customer_Id", customerId);
                command.Parameters.AddWithValue("@Product_Status", "WaitingForPart"); // Replace with the appropriate value

                command.ExecuteNonQuery();
            }
        }

        [HttpPut("{customerId}")]
        public IActionResult UpdateStatus(long customerId, [FromBody] string newStatus)
        {
            // Update the status in the database
            UpdateOrderStatus(customerId, newStatus);

            // Return a 200 OK status
            return Ok();
        }

        private void UpdateOrderStatus(long customerId, string newStatus)
        {
            using (var connection = new SqlConnection(_configuration.GetConnectionString("EmployeeDatabase")))
            {
                var sql = "UPDATE OrderTable SET Current_status = @NewStatus WHERE Customer_Id = @CustomerId";

                connection.Open();
                using SqlCommand command = new SqlCommand(sql, connection);

                // Set parameter values
                command.Parameters.AddWithValue("@NewStatus", newStatus);
                command.Parameters.AddWithValue("@CustomerId", customerId);

                command.ExecuteNonQuery();
            }
        }


    }
}

