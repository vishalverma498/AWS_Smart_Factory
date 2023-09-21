using System;
namespace smart_factory_AWS
{
	public class Defects_Table
	{
        public long id { get; set; }

        public string RawMaterial_Defects { get; set; }
        public string Filling_Defects { get; set; }
        public string Lablling_Defects { get; set; }
        public DateTime date_field { get; set; }
        public string Customer_Id { get; set; }
    }
}

