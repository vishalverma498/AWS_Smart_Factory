using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace smart_factory_AWS.Controllers
{
    [Route("/Dispatching100Controller")]
    [ApiController]
    public class Dispatching100Controller
    {
        private readonly IConfiguration _configuration;

        public Dispatching100Controller(IConfiguration configuration)
        {
            _configuration = configuration;
        }


        [HttpGet]
        public IEnumerable<Dispatching> Get()
        {
            var Dispatchings = GetDispatchings();
            return Dispatchings;
        }

        private IEnumerable<Dispatching> GetDispatchings()
        {
            var Dispatchings = new List<Dispatching>();

            using (var connection = new SqlConnection(_configuration.GetConnectionString("EmployeeDatabase")))
            {
                //var sql = "SELECT id, IN3V, IN3C, IN3P, V3, C3, P3, T3, VB3  FROM Dispatching";
                var sql = "SELECT TOP 100 * FROM Dispatching ORDER BY id DESC";
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



    }
}

