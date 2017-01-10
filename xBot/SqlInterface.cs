using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.SqlClient;

namespace xBot
{
    class SqlInterface
    {
        private static SqlConnection connection = null;

        public static void Connect()
        {
            connection = new SqlConnection("Data Source='" + Settings.ARIA_SERVER + "';UID='" + Settings.ARIA_USERNAME + "';PWD='" + Settings.ARIA_PASSWORD + "';Database='" + Settings.ARIA_DATABASE + "';");
            connection.Open();
        }

        public static void Disconnect()
        {
            connection.Close();
        }

        public static DataTable Query(string queryString)
        {
            DataTable dataTable = new DataTable();
            try
            {
                SqlDataAdapter adapter = new SqlDataAdapter(queryString, connection) { MissingMappingAction = MissingMappingAction.Passthrough, MissingSchemaAction = MissingSchemaAction.Add };
                adapter.Fill(dataTable);
                adapter.Dispose();
            }
            catch (Exception exception)
            {
                //System.Windows.Forms.MessageBox.Show(exception.Message, "SQL Error", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
                Console.WriteLine(exception.Message);
            }

            return dataTable;
        }
    }
}
