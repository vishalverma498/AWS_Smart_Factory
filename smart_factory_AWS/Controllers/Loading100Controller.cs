using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace smart_factory_AWS.Controllers
{
    [Route("/Loading100Controller")]
    [ApiController]
    public class Loading100Controller
    {
        private readonly IConfiguration _configuration;

        public Loading100Controller(IConfiguration configuration)
        {
            _configuration = configuration;
        }


        [HttpGet]
        public IEnumerable<Loading> Get()
        {
            var Loadings = GetLoadings();
            return Loadings;
        }

        private IEnumerable<Loading> GetLoadings()
        {
            var Loadings = new List<Loading>();

            using (var connection = new SqlConnection(_configuration.GetConnectionString("EmployeeDatabase")))
            {
                //var sql = "SELECT id, IN1V, IN1C, IN1P, V1, C1, P1, T1, VB1  FROM Loading";
                var sql = "SELECT TOP 100 * FROM Loading ORDER BY id DESC";
                connection.Open();
                using SqlCommand command = new SqlCommand(sql, connection);
                using SqlDataReader reader = command.ExecuteReader();

                while (reader.Read())
                {
                    var Loading = new Loading()
                    {
                        id = (long)reader["id"],
                        IN1V = reader["IN1V"].ToString(),
                        IN1C = reader["IN1C"].ToString(),
                        IN1P = reader["IN1P"].ToString(),
                        V1 = reader["V1"].ToString(),
                        C1 = reader["C1"].ToString(),
                        P1 = reader["P1"].ToString(),
                        T1 = reader["T1"].ToString(),
                        VB1 = reader["VB1"].ToString(),
                        Customer_Id = reader["Customer_Id"].ToString(),

                    };

                    Loadings.Add(Loading);
                }
            }

            return Loadings;
        }
    }
}

