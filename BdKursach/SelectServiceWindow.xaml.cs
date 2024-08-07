using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Windows;

namespace BdKursach
{
    /// <summary>
    /// Логика взаимодействия для SelectServiceWindow.xaml
    /// </summary>
    public partial class SelectServiceWindow : Window
    {
        public Service SelectedService { get; private set; }
        private string connectionString = "Data Source=DESKTOP-G94EAHJ\\SQLEXPRESS;Initial Catalog=диплом4;Integrated Security=True";
        private List<Service> services = new List<Service>();

        public SelectServiceWindow()
        {
            InitializeComponent();
            LoadServices();
        }

        private void LoadServices()
        {
            services.Clear();

            string query = "SELECT ID_Услуги, Название_услуги, Стоимость FROM Услуги";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                SqlCommand command = new SqlCommand(query, connection);

                try
                {
                    connection.Open();
                    SqlDataReader reader = command.ExecuteReader();

                    while (reader.Read())
                    {
                        int serviceId = (int)reader["ID_Услуги"];
                        string serviceName = (string)reader["Название_услуги"];
                        decimal cost = (decimal)reader["Стоимость"];

                        Service service = new Service(serviceId, serviceName, cost);
                        services.Add(service);
                    }

                    reader.Close();
                    servicesListView.ItemsSource = services;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ошибка при загрузке услуг: " + ex.Message);
                }
            }
        }

        private void SelectServiceButton_Click(object sender, RoutedEventArgs e)
        {
            if (servicesListView.SelectedItem != null)
            {
                SelectedService = (Service)servicesListView.SelectedItem;
                this.DialogResult = true;
            }
            else
            {
                MessageBox.Show("Выберите услугу.");
            }
        }
    }
}
