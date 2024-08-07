
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;
using System.Windows;
using ClosedXML.Excel;
using System.IO;

namespace BdKursach
{
    /// <summary>
    /// Логика взаимодействия для HistoryOrdersWindow.xaml
    /// </summary>
    public partial class HistoryOrdersWindow : Window
    {
        

        private string connectionString = "Data Source=DESKTOP-G94EAHJ\\SQLEXPRESS;Initial Catalog=диплом4;Integrated Security=True";
        public int CustomerId { get; set; }
        public HistoryOrdersWindow(int customerId)
        {
            InitializeComponent();
            this.CustomerId = customerId;
            LoadHistoryOrders();
        }

        private void LoadHistoryOrders()
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    string query = @"
                SELECT 
                    Заказ.ID_Заказа,
                    Заказ.Название_заказа,
                    Заказчики.Название AS Название_заказчика,
                    Сотрудники.ФИО AS Сотрудник,
                    Заказ.Процент_выполнения,
                    Заказ.Стоимость,
                    Заказ.Дата_принятия_заказа,
                    Заказ.Дата_завершения_заказа 
                FROM 
                    Заказ
                INNER JOIN 
                    Заказчики ON Заказ.ID_заказчика = Заказчики.ID_заказчика
                INNER JOIN 
                    Сотрудники ON Заказ.ID_Сотрудника = Сотрудники.ID_Сотрудника
                WHERE 
                    Заказ.ID_заказчика = @CustomerId";

                    SqlCommand command = new SqlCommand(query, connection);
                    command.Parameters.AddWithValue("@CustomerId", this.CustomerId);
                    SqlDataAdapter adapter = new SqlDataAdapter(command);
                    DataTable dataTable = new DataTable();
                    adapter.Fill(dataTable);
                    HistoryOrdersDataGrid.ItemsSource = dataTable.DefaultView;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при загрузке истории заказов: " + ex.Message);
            }
        }

        private void OrderHistoryReport(object sender, RoutedEventArgs e)
        {
            DateRangeSelectionWindow dateRangeWindow = new DateRangeSelectionWindow(CustomerId);
            dateRangeWindow.ShowDialog();

        }
    }
}
