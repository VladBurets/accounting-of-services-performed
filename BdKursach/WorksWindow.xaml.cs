using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using static BdKursach.OrdersWindow;

namespace BdKursach
{
    /// <summary>
    /// Логика взаимодействия для WorksWindow.xaml
    /// </summary>
    public partial class WorksWindow : Window
    {
        private string connectionString = "Data Source=DESKTOP-G94EAHJ\\SQLEXPRESS;Initial Catalog=диплом4;Integrated Security=True";

        int procent = 0;
        decimal totalCost = 0;
        public int OrderId { get; set; }

        public event EventHandler WorkItemCompletionUpdated;

        // Создаем список работ для отображения в окне
        private List<WorkItem> workItems = new List<WorkItem>();

        public WorksWindow(int OrderId)
        {
            InitializeComponent();
            this.OrderId = OrderId;
            LoadWorkItems();
        }

        private void LoadWorkItems()
        {
            // Очищаем предыдущие данные
            workItems.Clear();

            string query = "SELECT ID_тз, Пункты, Стоимость, Выполнено, ID_Услуги FROM Техническое_задание WHERE ID_Заказа = @OrderId";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@OrderId", OrderId);

                try
                {
                    connection.Open();
                    SqlDataReader reader = command.ExecuteReader();

                    while (reader.Read())
                    {
                        int workId = (int)reader["ID_тз"];
                        string workName = (string)reader["Пункты"];
                        decimal cost = (decimal)reader["Стоимость"];
                        bool completed = (bool)reader["Выполнено"];
                        int serviceId = (int)reader["ID_Услуги"];

                        WorkItem workItem = new WorkItem(workId, workName, cost, completed, serviceId);
                        workItems.Add(workItem);
                    }

                    reader.Close();

                    workItemsListView.ItemsSource = workItems;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ошибка при загрузке работ: " + ex.Message);
                }
            }
        }

        private void AddServiceButton_Click(object sender, RoutedEventArgs e)
        {
            SelectServiceWindow selectServiceWindow = new SelectServiceWindow();
            if (selectServiceWindow.ShowDialog() == true)
            {
                Service selectedService = selectServiceWindow.SelectedService;
                if (selectedService != null)
                {
                    // Проверяем, существует ли уже услуга в таблице Техническое_задание
                    using (SqlConnection connection = new SqlConnection(connectionString))
                    {
                        connection.Open();

                        string checkQuery = "SELECT COUNT(*) FROM Техническое_задание WHERE Пункты = @WorkName AND ID_Заказа = @OrderId";
                        SqlCommand checkCommand = new SqlCommand(checkQuery, connection);
                        checkCommand.Parameters.AddWithValue("@WorkName", selectedService.Name);
                        checkCommand.Parameters.AddWithValue("@OrderId", OrderId);

                        int count = (int)checkCommand.ExecuteScalar();

                        if (count > 0)
                        {
                            MessageBox.Show("Эта услуга уже добавлена в заказ.");
                            return;
                        }

                        // Добавляем выбранную услугу в список работ
                        WorkItem workItem = new WorkItem(0, selectedService.Name, selectedService.Cost, false, selectedService.Id);
                        workItems.Add(workItem);

                        // Обновляем источник данных для ListView
                        workItemsListView.ItemsSource = null;
                        workItemsListView.ItemsSource = workItems;

                        // Сохраняем новую услугу в базу данных
                        string insertQuery = "INSERT INTO Техническое_задание (Пункты, Стоимость, Выполнено, ID_Заказа, ID_Услуги) VALUES (@WorkName, @Cost, @Completed, @OrderId, @ServiceId)";
                        SqlCommand insertCommand = new SqlCommand(insertQuery, connection);
                        insertCommand.Parameters.AddWithValue("@WorkName", selectedService.Name);
                        insertCommand.Parameters.AddWithValue("@Cost", selectedService.Cost);
                        insertCommand.Parameters.AddWithValue("@Completed", false);
                        insertCommand.Parameters.AddWithValue("@OrderId", OrderId);
                        insertCommand.Parameters.AddWithValue("@ServiceId", selectedService.Id);

                        insertCommand.ExecuteNonQuery();
                    }
                }
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            // Пересчитываем детали заказа перед сохранением
            RecalculateOrderDetails();

            decimal totalCost = CalculateTotalCost();

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                string query = "UPDATE Заказ SET Процент_выполнения = @Procent, Стоимость = @TotalCost WHERE ID_Заказа = @OrderId";
                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@Procent", procent);
                command.Parameters.AddWithValue("@TotalCost", totalCost);
                command.Parameters.AddWithValue("@OrderId", OrderId);

                command.ExecuteNonQuery();
            }
            DialogResult = true;
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            CheckBox checkBox = sender as CheckBox;
            if (checkBox != null)
            {
                WorkItem workItem = checkBox.DataContext as WorkItem;
                if (workItem != null)
                {
                    // Выполнено
                    UpdateWorkItemCompletion(workItem.WorkId, true);
                    UpdateOrderCompletion();
                    UpdateWorkItemCompletionDate(workItem.WorkId);
                }
            }
        }

        private void CheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            CheckBox checkBox = sender as CheckBox;
            if (checkBox != null)
            {
                WorkItem workItem = checkBox.DataContext as WorkItem;
                if (workItem != null)
                {
                    // Не выполнено
                    UpdateWorkItemCompletion(workItem.WorkId, false);
                    UpdateOrderCompletion();
                    ClearWorkItemCompletionDate(workItem.WorkId);
                }
            }
        }

        private void UpdateWorkItemCompletionDate(int workItemId)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string query = "UPDATE Техническое_задание SET Дата_выполнения = GETDATE() WHERE ID_тз = @WorkItemId";
                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@WorkItemId", workItemId);
                command.ExecuteNonQuery();
            }
        }

        private void ClearWorkItemCompletionDate(int workItemId)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string query = "UPDATE Техническое_задание SET Дата_выполнения = NULL WHERE ID_тз = @WorkItemId";
                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@WorkItemId", workItemId);
                command.ExecuteNonQuery();
            }
        }

        private void UpdateWorkItemCompletion(int workItemId, bool completed)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string query = "UPDATE Техническое_задание SET Выполнено = @Completed WHERE ID_тз = @WorkItemId";
                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@Completed", completed);
                command.Parameters.AddWithValue("@WorkItemId", workItemId);
                command.ExecuteNonQuery();
            }
            WorkItemCompletionUpdated?.Invoke(this, EventArgs.Empty);
        }

        private void UpdateOrderCompletion()
        {
            RecalculateOrderDetails();

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string query = "UPDATE Заказ SET Процент_выполнения = @CompletionPercent WHERE ID_Заказа = @OrderId";
                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@CompletionPercent", procent);
                command.Parameters.AddWithValue("@OrderId", OrderId);
                command.ExecuteNonQuery();
            }
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (workItemsListView.SelectedItem is WorkItem selectedItem)
            {
                MessageBoxResult result = MessageBox.Show(
                    "Вы уверены, что хотите удалить эту работу?",
                    "Подтверждение удаления",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    string query = "DELETE FROM Техническое_задание WHERE ID_тз = @WorkId";

                    using (SqlConnection connection = new SqlConnection(connectionString))
                    {
                        SqlCommand command = new SqlCommand(query, connection);
                        command.Parameters.AddWithValue("@WorkId", selectedItem.WorkId);

                        try
                        {
                            connection.Open();
                            command.ExecuteNonQuery();

                            // Удаляем элемент из списка workItems
                            workItems.Remove(selectedItem);

                            // Обновляем источник данных ListView
                            workItemsListView.ItemsSource = null;
                            workItemsListView.ItemsSource = workItems;

                            // Пересчитываем и обновляем детали заказа
                            RecalculateOrderDetails();
                            UpdateOrderDetails();

                            MessageBox.Show("Работа успешно удалена.");
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show("Ошибка при удалении работы: " + ex.Message);
                        }
                    }
                }
            }
            else
            {
                MessageBox.Show("Выберите элемент для удаления.");
            }
        }

        private void RecalculateOrderDetails()
        {
            int totalWorkItems = workItems.Count;
            int completedWorkItems = workItems.Count(w => w.Completed);

            if (totalWorkItems > 0)
            {
                procent = (int)Math.Round((double)completedWorkItems / totalWorkItems * 100);
            }
            else
            {
                procent = 0;
            }
        }

        private void UpdateOrderDetails()
        {
            decimal totalCost = CalculateTotalCost();

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string query = "UPDATE Заказ SET Процент_выполнения = @Procent, Стоимость = @TotalCost WHERE ID_Заказа = @OrderId";
                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@Procent", procent);
                command.Parameters.AddWithValue("@TotalCost", totalCost);
                command.Parameters.AddWithValue("@OrderId", OrderId);

                connection.Open();
                command.ExecuteNonQuery();
            }
        }

        private decimal CalculateTotalCost()
        {
            totalCost = 0;

            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    string query = "SELECT Стоимость FROM Техническое_задание WHERE ID_Заказа = @OrderId";
                    SqlCommand command = new SqlCommand(query, connection);
                    command.Parameters.AddWithValue("@OrderId", OrderId);

                    SqlDataReader reader = command.ExecuteReader();

                    while (reader.Read())
                    {
                        decimal taskCost = (decimal)reader["Стоимость"];
                        totalCost += taskCost;
                    }

                    reader.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при вычислении общей стоимости: " + ex.Message);
            }

            return totalCost;
        }
    }

    public class WorkItem
    {
        public int WorkId { get; set; }
        public string WorkName { get; set; }
        public decimal Cost { get; set; }
        public bool Completed { get; set; }
        public int ServiceId { get; set; }

        public WorkItem(int workId, string workName, decimal cost, bool completed, int serviceId)
        {
            WorkId = workId;
            WorkName = workName;
            Cost = cost;
            Completed = completed;
            ServiceId = serviceId;
        }
    }
}   

