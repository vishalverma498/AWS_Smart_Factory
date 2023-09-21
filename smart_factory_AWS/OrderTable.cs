using System;
namespace smart_factory_AWS
{
	public class OrderTable
	{
        public long Customer_Id { get; set; }

        public string Color { get; set; }

        public string Quantity { get; set; }

        public string Current_status { get; set; }
        public string Customer_Name { get; set; }
        public string Delivery_city { get; set; }
        public DateTime Order_Time { get; set; }
    }
}

