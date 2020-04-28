using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApplication1
{
    public class Connect
    {
        public SqlConnection knoi = new SqlConnection();
        public void kn_CSDL()
        {
            string chuoikn = @"Data Source = ERP-CONGLT\SQLEXPRESS; Initial Catalog = QuanLySanPham; Integrated Security = True";
            knoi = new SqlConnection(chuoikn);
            knoi.Open();
        }
    }
}
