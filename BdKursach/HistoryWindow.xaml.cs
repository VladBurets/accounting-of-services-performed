using System;
using System.Collections.Generic;
using System.Windows;
using System.Data.SqlClient;
using System.Data;

namespace BdKursach
{
    /// <summary>
    /// Логика взаимодействия для HistoryWindow.xaml
    /// </summary>
    public partial class HistoryWindow : Window
    {
        private string connectionString = "Data Source=DESKTOP-G94EAHJ\\SQLEXPRESS;Initial Catalog=диплом4;Integrated Security=True";
        public HistoryWindow()
        {
            InitializeComponent();
            LoadHistory();
        }

        private void LoadHistory()
        {
            try
            {
                string query = "SELECT * FROM История";
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    SqlCommand command = new SqlCommand(query, connection);
                    connection.Open();
                    SqlDataAdapter adapter = new SqlDataAdapter(command);
                    DataTable dataTable = new DataTable();
                    adapter.Fill(dataTable);
                    historyDataGrid.ItemsSource = dataTable.DefaultView;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка загрузки истории: " + ex.Message);
            }
        }
    }
}
