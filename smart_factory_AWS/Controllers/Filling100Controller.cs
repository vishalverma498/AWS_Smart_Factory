using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace smart_factory_AWS.Controllers
{
    [Route("/Filling100Controller")]
    [ApiController]
    public class Filling100Controller
    {
        private readonly IConfiguration _configuration;

        public Filling100Controller(IConfiguration configuration)
        {
            _configuration = configuration;
        }


        [HttpGet]
        public IEnumerable<Filling> Get()
        {
            var Fillings = GetFillings();
            return Fillings;
        }

        private IEnumerable<Filling> GetFillings()
        {
            var Fillings = new List<Filling>();

            using (var connection = new SqlConnection(_configuration.GetConnectionString("EmployeeDatabase")))
            {
                //var sql = "SELECT id, IN2V, IN2C, IN2P, V2, C2, P2, T2, VB2  FROM Filling";
                var sql = "SELECT TOP 100 * FROM Filling ORDER BY id DESC";
                connection.Open();
                using SqlCommand command = new SqlCommand(sql, connection);
                using SqlDataReader reader = command.ExecuteReader();

                while (reader.Read())
                {
                    var Filling = new Filling()
                    {
                        id = (long)reader["id"],
                        IN2V = reader["IN2V"].ToString(),
                        IN2C = reader["IN2C"].ToString(),
                        IN2P = reader["IN2P"].ToString(),
                        V2 = reader["V2"].ToString(),
                        C2 = reader["C2"].ToString(),
                        P2 = reader["P2"].ToString(),
                        T2 = reader["T2"].ToString(),
                        VB2 = reader["VB2"].ToString(),
                        Customer_Id = reader["Customer_Id"].ToString(),

                    };

                    Fillings.Add(Filling);
                }
            }

            return Fillings;
        }
    }
}

