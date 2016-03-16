using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;
using System.Data;
using System.Data.SqlClient;

namespace WcfCrimShopService
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the class name "Service1" in code, svc and config file together.
    // NOTE: In order to launch WCF Test Client for testing this service, please select Service1.svc or Service1.svc.cs at the Solution Explorer and start debugging.
    public class Service1 : IService1
    {
        public string GetData(int value)
        {
            return string.Format("You entered: {0}", value);
        }

        public string InsertOrderDetails(OrderDetails orderInfo)
        {
            string Message;
            SqlConnection con = new SqlConnection(@"Data Source=GMTWKS13\GMTWKS13DB;Initial Catalog=CRIMShopManagement;Trusted_Connection=Yes;");
            con.Open();
            string queryString = "INSERT into dbo.Orders (ControlNumber,PaymentRespone,Description)" +
                                "VALUES (@control,@response,@description)";
            SqlCommand cmd = new SqlCommand(queryString, con);
            cmd.Parameters.AddWithValue("@control", orderInfo.ControlNumber);
            cmd.Parameters.AddWithValue("@response", orderInfo.PaymentResponse);
            cmd.Parameters.AddWithValue("@description", orderInfo.Description);
            int result = cmd.ExecuteNonQuery();
            if(result == 1)
            {
                Message = "Order : " + orderInfo.ControlNumber + " added successfully";
            }
            else
            {
                Message = "Order : " + orderInfo.ControlNumber + " not added";
            }
            con.Close();
            return Message;

        }

        //public CompositeType GetDataUsingDataContract(CompositeType composite)
        //{
        //    if (composite == null)
        //    {
        //        throw new ArgumentNullException("composite");
        //    }
        //    if (composite.BoolValue)
        //    {
        //        composite.StringValue += "Suffix";
        //    }
        //    return composite;
        //}
    }
}
