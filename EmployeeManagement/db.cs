using System;
using System.Data.SqlClient;
using System.Windows.Forms;
using System.Data;

namespace EmployeeManagement
{
    class db
    {

        public static SqlConnection con = new SqlConnection("Data " +
            "Source=localhost;Initial Catalog=EmployeeDatabase;User " +
            "ID=sa;Password=p@ssword");

        public static SqlCommand cmd = new SqlCommand("", con);
        public static DataSet ds;
        public static SqlDataAdapter da;
        public static BindingSource bs;
        public static string sql;

        public static void openConnection()
        {
            try
            {

                if(con.State == ConnectionState.Closed)
                {
                    con.Open();
                    //MessageBox.Show("The connection is: " + con.State.ToString());
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show("Opem connection Failed: " + ex.Message);
            }
        }

        public static void closeConnection()
        {

            try
            {

                if (con.State == ConnectionState.Open)
                {
                    con.Close();
                    //MessageBox.Show("The connection is: " + con.State.ToString());
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show("Close connection error: " + ex.Message);
            }

        }

    }
}
