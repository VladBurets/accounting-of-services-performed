using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ClosedXML.Excel;


namespace BdKursach
{
    /// <summary>
    /// Логика взаимодействия для OrdersWindow.xaml
    /// </summary>


    public partial class OrdersWindow : Window
    {
        public OrdersWindow()
        {
            InitializeComponent();
            LoadOrders();
            ClientComboBox();
            EmployeComboBox();
            
        }

        private void ClientsMenuItem_Click(object sender, RoutedEventArgs e)
        {
            ClientsWindow clientsWindow = new ClientsWindow();
            clientsWindow.Show();
            this.Close();
        }

        private void EmployeesMenuItem_Click(object sender, RoutedEventArgs e)
        {
            EmployeesWindow employeesWindow = new EmployeesWindow();
            employeesWindow.Show();
            this.Close();
        }

        private void ServicesMenuItem_Click(object sender, RoutedEventArgs e)
        {
            ServicesWindow servicesWindow = new ServicesWindow();
            servicesWindow.Show();
            this.Close();
        }

        private void OrdersMenuItem_Click(object sender, RoutedEventArgs e)
        {
            OrdersWindow ordersWindow = new OrdersWindow();
            ordersWindow.Show();
            this.Close();
        }

        private void ServiceStatisticsMenuItem_Click(object sender, RoutedEventArgs e)
        {
            ServiceStatisticsWindow serviceStatisticsWindow = new ServiceStatisticsWindow();
            serviceStatisticsWindow.Show();

        }

        private void PeriodSelectionMenuItem_Click(object sender, RoutedEventArgs e)
        {
            PeriodSelectionWindow periodSelectionWindow = new PeriodSelectionWindow();
            periodSelectionWindow.Show();

        }


        private string connectionString = "Data Source=DESKTOP-G94EAHJ\\SQLEXPRESS;Initial Catalog=диплом4;Integrated Security=True";

        public class Client
        {
            public string ID { get; set; }
            public string ClientName { get; set; }
        }

        public class Employe
        {
            public string ID { get; set; }
            public string EmployeName { get; set; }

        }

        private void ClientComboBox()
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string query = "SELECT ID_заказчика, Название FROM Заказчики";
                SqlCommand cmd = new SqlCommand(query, connection);
                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    Client name = new Client
                    {
                        ID = reader["ID_заказчика"].ToString(),
                        ClientName = reader["Название"].ToString()
                    };
                    cmbClient.Items.Add(name);


                }
                cmbClient.DisplayMemberPath = "ClientName";

                connection.Close();
            }
        }

        private void EmployeComboBox()
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string query = @"SELECT Сотрудники.ID_Сотрудника, Сотрудники.ФИО FROM Сотрудники";


                SqlCommand cmd = new SqlCommand(query, connection);
                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    Employe name = new Employe
                    {
                        ID = reader["ID_Сотрудника"].ToString(),
                        EmployeName = reader["ФИО"].ToString(),
                    };
                    cmbEmploye.Items.Add(name);
                    newEmployeeComboBox.Items.Add(name);
                }
                cmbEmploye.DisplayMemberPath = "EmployeName";
                newEmployeeComboBox.DisplayMemberPath = "EmployeName";
                connection.Close();
            }
        }

        private DataView originalDataView;

        private void LoadOrders()
        {
            try
            {
                OrdersDataGrid.ItemsSource = null;
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    string query = "SELECT Заказ.Название_заказа, Заказ.Процент_выполнения, " +
                                   "Заказ.Стоимость, Заказ.Дата_принятия_заказа, Заказ.Дата_завершения_заказа, " +
                                   "Заказчики.Название, Сотрудники.ФИО AS Сотрудник, Заказ.ID_заказчика, Заказ.ID_Сотрудника, Заказ.ID_Заказа " +
                                   "FROM Заказ " +
                                   "INNER JOIN Заказчики ON Заказ.ID_заказчика = Заказчики.ID_заказчика " +
                                   "INNER JOIN Сотрудники ON Заказ.ID_Сотрудника = Сотрудники.ID_Сотрудника";

                    SqlDataAdapter adapter = new SqlDataAdapter(query, connection);
                    System.Data.DataTable dataTable = new System.Data.DataTable();

                    adapter.Fill(dataTable);

                    OrdersDataGrid.ItemsSource = dataTable.DefaultView;

                    originalDataView = dataTable.DefaultView;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при загрузке заказа: " + ex.Message);
            }
        }

        private void AddOrderMenuItem_Click(object sender, RoutedEventArgs e)
        {
            OrderNameTextBox.Text = string.Empty;

            DatePickerCompletion.SelectedDate = null;


            inputGrid.Visibility = Visibility.Visible;
            ElementsGrid.Visibility = Visibility.Collapsed;
        }

        private void OrderNameTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            // Проверяем, что вводится только цифра
            if (!char.IsDigit(e.Text, 0))
            {
                e.Handled = true; // Останавливаем обработку события, чтобы символ не был введен
            }
        }

        private void AddOrderButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string orderNumberStr = OrderNameTextBox.Text;

                Employe selectedEmploye = cmbEmploye.SelectedItem as Employe;

                if (selectedEmploye == null)
                {
                    MessageBox.Show("Выберите сотрудника.");
                    return;
                }

                if (cmbClient.SelectedItem == null || cmbClient.SelectedItem as Client == null)
                {
                    MessageBox.Show("Выберите клиента.");
                    return;
                }

                Client selectedClient = cmbClient.SelectedItem as Client;
                DateTime currentDate = DateTime.Now;
                DateTime dateAcceptance = currentDate; // Дата принятия заказа всегда текущая
                DateTime dateCompletion = DatePickerCompletion.SelectedDate ?? DateTime.Today;

                // Проверка на выбор даты завершения
                if (DatePickerCompletion.SelectedDate == null)
                {
                    MessageBox.Show("Выберите дату завершения заказа.");
                    return;
                }

                dateCompletion = DatePickerCompletion.SelectedDate.Value;

                if (string.IsNullOrEmpty(orderNumberStr))
                {
                    MessageBox.Show("Введите номер заказа.");
                    return;
                }

                // Проверка на корректность введенного числового значения
                int orderNumber;
                if (!int.TryParse(orderNumberStr, out orderNumber) || orderNumber <= 0)
                {
                    MessageBox.Show("номер заказа должен состоять из чисел.");
                    return;
                }

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    // Проверка на уникальность номера заказа
                    string checkQuery = "SELECT COUNT(*) FROM Заказ WHERE Название_заказа = @OrderName";
                    SqlCommand checkCommand = new SqlCommand(checkQuery, connection);
                    checkCommand.Parameters.AddWithValue("@OrderName", orderNumberStr);

                    int count = (int)checkCommand.ExecuteScalar();
                    if (count > 0)
                    {
                        MessageBox.Show("Заказ с таким номером уже существует.");
                        return;
                    }

                    string query = "INSERT INTO Заказ (Название_заказа, Дата_принятия_заказа, Дата_завершения_заказа, ID_заказчика, ID_Сотрудника) " +
                                   "VALUES (@OrderName, @DateAcceptance, @DateCompletion, @ID_Клиента, @ID_Сотрудника)";
                    SqlCommand command = new SqlCommand(query, connection);
                    command.Parameters.AddWithValue("@OrderName", orderNumberStr);
                    command.Parameters.AddWithValue("@DateAcceptance", dateAcceptance);
                    command.Parameters.AddWithValue("@DateCompletion", dateCompletion);
                    command.Parameters.AddWithValue("@ID_Клиента", selectedClient.ID);
                    command.Parameters.AddWithValue("@ID_Сотрудника", selectedEmploye.ID);

                    command.ExecuteNonQuery();

                    MessageBox.Show("Заказ успешно добавлен!");

                    inputGrid.Visibility = Visibility.Collapsed;
                    ElementsGrid.Visibility = Visibility.Visible;

                    LoadOrders();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при добавлении заказа: " + ex.Message);
            }
        }


        private void EditOrderMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (OrdersDataGrid.SelectedItem != null)
            {
                DataRowView selectedRow = (DataRowView)OrdersDataGrid.SelectedItem;
                string orderName = selectedRow["Название_заказа"].ToString();


                //string cost = selectedRow["Стоимость"].ToString();

                //TextBox NewCostTextBox = (TextBox)editGrid.FindName("NewCostTextBox");
                TextBox NewOrderNameTextBox = (TextBox)editGrid.FindName("NewOrderNameTextBox");


                if (NewOrderNameTextBox != null)
                {
                    NewOrderNameTextBox.Text = orderName;
                    //NewCostTextBox.Text = cost;

                    editGrid.Visibility = Visibility.Visible;
                    ElementsGrid.Visibility = Visibility.Collapsed;
                }
            }
            else
            {
                MessageBox.Show("Выберите заказ для редактирования.");
            }
        }


        private DateTime oldDateCompletion; // Переменная для хранения старой даты завершения заказа



        private string oldEmploye;



        private void OrdersDataGridRow_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (OrdersDataGrid.SelectedItem != null)
            {
                DataRowView selectedRow = (DataRowView)OrdersDataGrid.SelectedItem;
                // Получаем старое значение даты завершения заказа из выбранной строки и сохраните его для использования в ApplyChangesButton_Click
                oldDateCompletion = Convert.ToDateTime(selectedRow["Дата_завершения_заказа"]);

                oldEmploye = selectedRow["ID_Сотрудника"].ToString();


            }
        }

        private void ApplyChangesButton_Click(object sender, RoutedEventArgs e)
        {
            if (OrdersDataGrid.SelectedItem != null)
            {
                try
                {
                    DataRowView selectedRow = (DataRowView)OrdersDataGrid.SelectedItem;
                    int orderId = (int)selectedRow["ID_Заказа"];
                    string currentOrderName = selectedRow["Название_заказа"].ToString();

                    TextBox newOrderNameTextBox = (TextBox)editGrid.FindName("NewOrderNameTextBox");
                    ComboBox newCmbProcentComplete = (ComboBox)editGrid.FindName("newCmbProcentComplete");
                    DatePicker newDateCompletionPicker = (DatePicker)editGrid.FindName("NewDateCompletionPicker");
                    ComboBox newEmployeeComboBox = (ComboBox)editGrid.FindName("newEmployeeComboBox");

                    string newEmployeeId;
                    if (newEmployeeComboBox.SelectedItem != null)
                    {
                        newEmployeeId = ((Employe)newEmployeeComboBox.SelectedItem).ID;
                    }
                    else
                    {
                        newEmployeeId = oldEmploye; // Предполагается, что у вас есть переменная oldEmploye с текущим ID сотрудника
                    }

                    DateTime newDateCompletion = newDateCompletionPicker.SelectedDate ?? oldDateCompletion; // oldDateCompletion - текущая дата завершения заказа

                    DateTime currentDate = DateTime.Now;

                    // Проверка на корректность даты завершения
                    if (newDateCompletion <= currentDate)
                    {
                        MessageBox.Show("Дата завершения заказа должна быть позже сегодняшней даты.");
                        return;
                    }

                    // Проверка на уникальность номера заказа, если он был изменен
                    if (newOrderNameTextBox.Text != currentOrderName)
                    {
                        using (SqlConnection connection = new SqlConnection(connectionString))
                        {
                            connection.Open();

                            // Проверка на уникальность номера заказа
                            string checkQuery = "SELECT COUNT(*) FROM Заказ WHERE Название_заказа = @NewOrderName";
                            SqlCommand checkCommand = new SqlCommand(checkQuery, connection);
                            checkCommand.Parameters.AddWithValue("@NewOrderName", newOrderNameTextBox.Text);

                            int count = (int)checkCommand.ExecuteScalar();
                            if (count > 0)
                            {
                                MessageBox.Show("Заказ с таким номером уже существует.");
                                return;
                            }
                        }
                    }

                    // Обновление информации о заказе
                    using (SqlConnection connection = new SqlConnection(connectionString))
                    {
                        connection.Open();

                        string query = "UPDATE Заказ SET " +
                                       "Название_заказа = @NewOrderName, Дата_завершения_заказа = @NewDateCompletion, " +
                                       "ID_Сотрудника = @NewEmployeeId " +
                                       $"WHERE ID_Заказа = {orderId}";

                        SqlCommand command = new SqlCommand(query, connection);
                        command.Parameters.AddWithValue("@NewOrderName", newOrderNameTextBox.Text);
                        command.Parameters.AddWithValue("@NewDateCompletion", newDateCompletion);
                        command.Parameters.AddWithValue("@NewEmployeeId", newEmployeeId);

                        int affectedRows = command.ExecuteNonQuery();

                        if (affectedRows > 0)
                        {
                            MessageBox.Show("Информация обновлена успешно!");

                            LoadOrders();
                            editGrid.Visibility = Visibility.Collapsed;
                            ElementsGrid.Visibility = Visibility.Visible;
                        }
                        else
                        {
                            MessageBox.Show("Ошибка при обновлении информации.");
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ошибка при изменении информации о заказе: " + ex.Message);
                }
            }
            else
            {
                MessageBox.Show("Выберите заказ для изменения.");
            }
        }



        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            inputGrid.Visibility = Visibility.Collapsed;
            ElementsGrid.Visibility = Visibility.Visible;

        }

        private void BackButtonEdit_Click(object sender, RoutedEventArgs e)
        {
            editGrid.Visibility = Visibility.Collapsed;
            ElementsGrid.Visibility = Visibility.Visible;
        }

        private void ComposForOrderButton_Click(object sender, RoutedEventArgs e)
        {
            if (OrdersDataGrid.SelectedItem != null)
            {
                DataRowView selectedRow = (DataRowView)OrdersDataGrid.SelectedItem;
                int orderId = (int)selectedRow["ID_Заказа"];
                int completionPercentage = (int)selectedRow["Процент_выполнения"];

                if (completionPercentage == 100)
                {
                    MessageBoxResult result = MessageBox.Show("Заказ был завершен. Вы уверены, что хотите внести изменения?", "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question);
                    if (result == MessageBoxResult.No)
                    {
                        return; // Прерываем выполнение метода
                    }
                }

                WorksWindow worksWindow = new WorksWindow(orderId);
                worksWindow.Owner = this;
                worksWindow.OrderId = orderId; // Устанавливаем значение свойства
                worksWindow.ShowDialog();
                if (worksWindow.DialogResult == true)
                {
                    LoadOrders();
                }
            }
            else
            {
                MessageBox.Show("Выберите заказ для просмотра работ.");
            }
        }


       

        private string GetNameClients(string name)
        {
            string contactPerson = "";


            string connectionString = "Data Source=DESKTOP-G94EAHJ\\SQLEXPRESS;Initial Catalog=диплом4;Integrated Security=True";


            string query = "SELECT Контактное_лицо FROM Заказчики WHERE Название = @Name";


            using (SqlConnection connection = new SqlConnection(connectionString))
            using (SqlCommand command = new SqlCommand(query, connection))
            {

                command.Parameters.AddWithValue("@Name", name);

                try
                {

                    connection.Open();
                    SqlDataReader reader = command.ExecuteReader();


                    if (reader.Read())
                    {

                        contactPerson = reader["Контактное_лицо"].ToString();
                    }
                    else
                    {

                        contactPerson = "";
                    }

                    reader.Close();
                }
                catch (Exception ex)
                {

                    MessageBox.Show("Ошибка при выполнении запроса: " + ex.Message);
                }
            }

            return contactPerson;
        }


        //private void СertificateOfPerformedServicesButton_Click(object sender, RoutedEventArgs e)
        //{
        //    if (OrdersDataGrid.SelectedItem != null)
        //    {
        //        DataRowView selectedRow = (DataRowView)OrdersDataGrid.SelectedItem;
        //        int orderId = (int)selectedRow["ID_Заказа"];

        //        DataRowView selectedOrderRoww = (DataRowView)OrdersDataGrid.SelectedItem;
        //        int procent = Convert.ToInt32(selectedOrderRoww["Процент_выполнения"]);
        //        if (procent == 100)
        //        {
        //            try
        //            {

        //                string templatePath = "D:\\учет_выполненных_работ\\BdKursach13\\Акт1.xlsx";
        //                using (var workbook = new XLWorkbook(templatePath))
        //                {
        //                    var worksheet = workbook.Worksheet(1); // Выбираем первый лист (нумерация начинается с 1)

        //                    // Получение данных о выбранном заказе
        //                    DataRowView selectedOrderRow = (DataRowView)OrdersDataGrid.SelectedItem;
        //                    int orderAKTId = Convert.ToInt32(selectedOrderRow["ID_Заказа"]);

        //                    // Заполнение ячеек документа данными о заказе
        //                    string documentNumber = selectedOrderRow["ID_Заказа"].ToString();
        //                    string productName = selectedOrderRow["Название"].ToString();

        //                    string employeeName = selectedOrderRow["Сотрудник"].ToString();
        //                    DateTime shipmentDate = (DateTime)selectedOrderRow["Дата_завершения_заказа"];
        //                    decimal sum = (decimal)selectedOrderRow["Стоимость"];
        //                    decimal nds = sum * 0.1m;
        //                    decimal allsum = sum + nds;
        //                    int wholePart = (int)sum;
        //                    decimal fractionalPart = sum - Math.Truncate(sum);
        //                    int fractional = (int)(fractionalPart * 100);
        //                    int wholePartNDS = (int)nds;
        //                    decimal fractionalPartNDS = nds - Math.Truncate(nds);
        //                    int fractionalNDS = (int)(fractionalPartNDS * 100);
        //                    string getName = GetNameClients(productName);

        //                    worksheet.Cell("E2").Value = documentNumber;
        //                    worksheet.Cell("C4").Value = employeeName;
        //                    worksheet.Cell("C5").Value = productName;
        //                    worksheet.Cell("C28").Value = employeeName;
        //                    worksheet.Cell("G2").Value = shipmentDate.ToString("dd.MM.yyyy");
        //                    worksheet.Cell("I18").Value = sum;
        //                    worksheet.Cell("I19").Value = nds;
        //                    worksheet.Cell("I20").Value = allsum;
        //                    worksheet.Cell("D22").Value = wholePart;
        //                    worksheet.Cell("F22").Value = fractional;
        //                    worksheet.Cell("D24").Value = wholePartNDS;
        //                    worksheet.Cell("F24").Value = fractionalNDS;
        //                    worksheet.Cell("G28").Value = getName;
        //                    // Получение данных о выполненных работах из базы данных
        //                    System.Data.DataTable workItemsDataTable = GetWorkItemsFromDatabase(orderId);
        //                    // Заполнение соответствующих ячеек в документе Excel
        //                    int startRow = 8; // Начальная строка для заполнения данных о выполненных работах
        //                    int i = 1;
        //                    foreach (DataRow row in workItemsDataTable.Rows)
        //                    {
        //                        int id = i;
        //                        string workItemName = row["Пункты"].ToString();
        //                        decimal workItemCost = Convert.ToDecimal(row["Стоимость"]);


        //                        // Заполнение соответствующих ячеек в документе Excel
        //                        worksheet.Cell(startRow, 1).Value = id;
        //                        worksheet.Cell(startRow, 2).Value = workItemName;
        //                        worksheet.Cell(startRow, 9).Value = workItemCost;

        //                        i++;
        //                        startRow++; // Переход к следующей строке
        //                    }

        //                    // Сохранение Excel-файла с заполненными данными
        //                    string fileName = "Акт выполненных работ_" + DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss") + ".xlsx";
        //                    string filePath = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), fileName);
        //                    workbook.SaveAs(filePath);

        //                    MessageBox.Show("Акт успешно создан: " + filePath);
        //                }
        //            }
        //            catch (Exception ex)
        //            {
        //                MessageBox.Show("Ошибка при создании акта: " + ex.Message);
        //            }

        //        }
        //        else
        //        {
        //            MessageBox.Show("Заказ еще не выполнен");
        //        }

        //    }
        //    else
        //    {
        //        MessageBox.Show("Выберите заказ для сотставления акта.");
        //    }

        //}

        private void СertificateOfPerformedServicesButton_Click(object sender, RoutedEventArgs e)
        {
            if (OrdersDataGrid.SelectedItem != null)
            {
                DataRowView selectedRow = (DataRowView)OrdersDataGrid.SelectedItem;
                int orderId = (int)selectedRow["ID_Заказа"];

                DataRowView selectedOrderRoww = (DataRowView)OrdersDataGrid.SelectedItem;
                int procent = Convert.ToInt32(selectedOrderRoww["Процент_выполнения"]);
                if (procent == 100)
                {
                    try
                    {
                        string templatePath = "D:\\учет_выполненных_работ\\BdKursach13\\Акт1.xlsx";
                        using (var workbook = new XLWorkbook(templatePath))
                        {
                            var worksheet = workbook.Worksheet(1); // Выбираем первый лист (нумерация начинается с 1)

                            // Получение данных о выбранном заказе
                            DataRowView selectedOrderRow = (DataRowView)OrdersDataGrid.SelectedItem;
                            int orderAKTId = Convert.ToInt32(selectedOrderRow["ID_Заказа"]);

                            // Заполнение ячеек документа данными о заказе
                            string documentNumber = selectedOrderRow["ID_Заказа"].ToString();
                            string productName = selectedOrderRow["Название"].ToString();
                            string employeeName = selectedOrderRow["Сотрудник"].ToString();

                            // Использование текущей даты вместо даты завершения заказа
                            DateTime currentDate = DateTime.Now;
                            decimal sum = (decimal)selectedOrderRow["Стоимость"];
                            decimal nds = sum * 0.1m;
                            decimal allsum = sum + nds;
                            int wholePart = (int)sum;
                            decimal fractionalPart = sum - Math.Truncate(sum);
                            int fractional = (int)(fractionalPart * 100);
                            int wholePartNDS = (int)nds;
                            decimal fractionalPartNDS = nds - Math.Truncate(nds);
                            int fractionalNDS = (int)(fractionalPartNDS * 100);
                            string getName = GetNameClients(productName);

                            worksheet.Cell("E2").Value = documentNumber;
                            worksheet.Cell("C4").Value = employeeName;
                            worksheet.Cell("C5").Value = productName;
                            worksheet.Cell("C28").Value = employeeName;

                            // Установка текущей даты
                            worksheet.Cell("G2").Value = currentDate.ToString("dd.MM.yyyy");
                            worksheet.Cell("I18").Value = sum;
                            worksheet.Cell("I19").Value = nds;
                            worksheet.Cell("I20").Value = allsum;
                            worksheet.Cell("D22").Value = wholePart;
                            worksheet.Cell("F22").Value = fractional;
                            worksheet.Cell("D24").Value = wholePartNDS;
                            worksheet.Cell("F24").Value = fractionalNDS;
                            worksheet.Cell("G28").Value = getName;

                            // Получение данных о выполненных работах из базы данных
                            System.Data.DataTable workItemsDataTable = GetWorkItemsFromDatabase(orderId);

                            // Заполнение соответствующих ячеек в документе Excel
                            int startRow = 8; // Начальная строка для заполнения данных о выполненных работах
                            int i = 1;
                            foreach (DataRow row in workItemsDataTable.Rows)
                            {
                                int id = i;
                                string workItemName = row["Пункты"].ToString();
                                decimal workItemCost = Convert.ToDecimal(row["Стоимость"]);

                                // Заполнение соответствующих ячеек в документе Excel
                                worksheet.Cell(startRow, 1).Value = id;
                                worksheet.Cell(startRow, 2).Value = workItemName;
                                worksheet.Cell(startRow, 9).Value = workItemCost;

                                i++;
                                startRow++; // Переход к следующей строке
                            }

                            // Сохранение Excel-файла с заполненными данными
                            string fileName = "Акт выполненных работ_" + DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss") + ".xlsx";
                            string filePath = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), fileName);
                            workbook.SaveAs(filePath);

                            MessageBox.Show("Акт успешно создан: " + filePath);
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Ошибка при создании акта: " + ex.Message);
                    }
                }
                else
                {
                    MessageBox.Show("Заказ еще не выполнен");
                }
            }
            else
            {
                MessageBox.Show("Выберите заказ для составления акта.");
            }
        }


        private System.Data.DataTable GetWorkItemsFromDatabase(int orderId)
        {
            System.Data.DataTable dataTable = new System.Data.DataTable();


            string connectionString = "Data Source=DESKTOP-G94EAHJ\\SQLEXPRESS;Initial Catalog=диплом4;Integrated Security=True";

            string query = "SELECT Пункты, Стоимость FROM Техническое_задание WHERE ID_Заказа = @OrderId";

            using (SqlConnection connection = new SqlConnection(connectionString))
            using (SqlCommand command = new SqlCommand(query, connection))
            {

                command.Parameters.AddWithValue("@OrderId", orderId);

                try
                {

                    connection.Open();
                    SqlDataAdapter adapter = new SqlDataAdapter(command);
                    adapter.Fill(dataTable);
                }
                catch (Exception ex)
                {

                    MessageBox.Show("Ошибка при выполнении запроса: " + ex.Message);
                }
            }

            return dataTable;
        }

        private void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            string searchText = SearchTextBox.Text.Trim();
            if (!string.IsNullOrEmpty(searchText))
            {
                try
                {
                    using (SqlConnection connection = new SqlConnection(connectionString))
                    {
                        connection.Open();

                        string[] searchTerms = searchText.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                        string query = "SELECT Заказ.Название_заказа, Заказ.Процент_выполнения, " +
                                       "Заказ.Стоимость, Заказ.Дата_принятия_заказа, Заказ.Дата_завершения_заказа, " +
                                       "Заказчики.Название, Сотрудники.ФИО AS Сотрудник, Заказ.ID_заказчика, Заказ.ID_Сотрудника, Заказ.ID_Заказа " +
                                       "FROM Заказ " +
                                       "INNER JOIN Заказчики ON Заказ.ID_заказчика = Заказчики.ID_заказчика " +
                                       "INNER JOIN Сотрудники ON Заказ.ID_Сотрудника = Сотрудники.ID_Сотрудника " +
                                       "WHERE ";

                        List<string> conditions = new List<string>();
                        for (int i = 0; i < searchTerms.Length; i++)
                        {
                            string parameterName = "@SearchText" + i;
                            conditions.Add($"(Заказ.Название_заказа LIKE {parameterName} " +
                                           $"OR Заказчики.Название LIKE {parameterName} " +
                                           $"OR Сотрудники.ФИО LIKE {parameterName} " +
                                           $"OR CONVERT(NVARCHAR, Заказ.Процент_выполнения) LIKE {parameterName} " +
                                           $"OR CONVERT(NVARCHAR, Заказ.Стоимость) LIKE {parameterName} " +
                                           $"OR CONVERT(NVARCHAR, Заказ.Дата_принятия_заказа, 120) LIKE {parameterName} " +
                                           $"OR CONVERT(NVARCHAR, Заказ.Дата_завершения_заказа, 120) LIKE {parameterName})");
                        }

                        query += string.Join(" AND ", conditions);

                        SqlDataAdapter adapter = new SqlDataAdapter(query, connection);
                        for (int i = 0; i < searchTerms.Length; i++)
                        {
                            string parameterName = "@SearchText" + i;
                            adapter.SelectCommand.Parameters.AddWithValue(parameterName, "%" + searchTerms[i] + "%");
                        }

                        System.Data.DataTable dataTable = new System.Data.DataTable();
                        adapter.Fill(dataTable);

                        OrdersDataGrid.ItemsSource = dataTable.DefaultView;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ошибка при выполнении поиска: " + ex.Message);
                }
            }
            else
            {
                LoadOrders();
            }
        }

        private void ClearButton_Clickk(object sender, RoutedEventArgs e)
        {
            SearchTextBox.Text = string.Empty;
            LoadOrders();
        }

        private void OrderReportButton_Click(object sender, RoutedEventArgs e)
        {
            if (OrdersDataGrid.SelectedItem != null)
            {
                DataRowView selectedOrderRow = (DataRowView)OrdersDataGrid.SelectedItem;
                int orderId = Convert.ToInt32(selectedOrderRow["ID_Заказа"]);
                string filePath = "Отчет.xlsx";

                // Создание экземпляра генератора отчетов
                ExcelReportGenerator generator = new ExcelReportGenerator();

                try
                {
                    // Генерация и сохранение Excel-отчета
                    generator.GenerateExcelReport(filePath, orderId);

                    MessageBox.Show("Отчет успешно создан и сохранен на рабочем столе.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при создании отчета: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                MessageBox.Show("Выберите заказ для составления отчета", "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

       

        private void SortFieldComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ApplySorting();
        }

        private void SortDirectionComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ApplySorting();
        }

        private void ApplySorting()
        {
            if (SortFieldComboBox.SelectedItem == null || SortDirectionComboBox.SelectedItem == null)
                return;

            string selectedField = ((ComboBoxItem)SortFieldComboBox.SelectedItem).Content.ToString();
            string selectedDirection = ((ComboBoxItem)SortDirectionComboBox.SelectedItem).Content.ToString();
            string sortDirection = selectedDirection == "По возрастанию" ? "ASC" : "DESC";

            if (selectedField == "По дате")
            {
                originalDataView.Sort = $"Дата_завершения_заказа {sortDirection}";
            }
            else if (selectedField == "По стоимости")
            {
                originalDataView.Sort = $"Стоимость {sortDirection}";
            }
            else if (selectedField == "По проценту выполнения")
            {
                originalDataView.Sort = $"Процент_выполнения {sortDirection}";
            }
        }

        private void ResetSortButton_Click(object sender, RoutedEventArgs e)
        {
            originalDataView.Sort = ""; // Сброс сортировки
        }



       

       
    }
}
