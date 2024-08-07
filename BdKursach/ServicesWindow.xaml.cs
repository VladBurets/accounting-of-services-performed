using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using ClosedXML.Excel;
using System.IO;


namespace BdKursach
{
   
    public partial class ServicesWindow : Window
    {
        private string connectionString = "Data Source=DESKTOP-G94EAHJ\\SQLEXPRESS;Initial Catalog=диплом4;Integrated Security=True";
       

        public ServicesWindow()
        {
            InitializeComponent();
            LoadServices();
           

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

        private void LoadServices()
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();



                    string query = "SELECT Услуги.Название_услуги, Услуги.Стоимость, Услуги.Количество, Услуги.ID_Услуги " +
                                   "FROM Услуги";

                    SqlDataAdapter adapter = new SqlDataAdapter(query, connection);
                    DataTable dataTable = new DataTable();
                    adapter.Fill(dataTable);

                    ServicesDataGrid.ItemsSource = dataTable.DefaultView;                    

                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при загрузке услуг: " + ex.Message);
            }
        }


        private void SortComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (SortComboBox.SelectedItem == null)
                return;

            string selectedSortOption = ((ComboBoxItem)SortComboBox.SelectedItem).Content.ToString();

            if (ServicesDataGrid.ItemsSource is DataView dataView)
            {
                if (selectedSortOption == "По возрастанию")
                {
                    dataView.Sort = "Количество DESC";
                }
                else if (selectedSortOption == "По убыванию")
                {
                    dataView.Sort = "Количество ASC";
                }
            }
        }

        private void AddServicesMenuItem_Click(object sender, RoutedEventArgs e)
        {
            inputGrid.Visibility = Visibility.Visible;
            ElementsGrid.Visibility = Visibility.Collapsed;
            ServiceNameTextBox.Text = string.Empty;
           
            ServiceCostTextBox.Text = string.Empty;

        }

        private void AddServiceButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string serviceName = ServiceNameTextBox.Text.Trim();
               
                decimal serviceCost;

                // Проверка на пустое название услуги
                if (string.IsNullOrEmpty(serviceName))
                {
                    MessageBox.Show("Введите название услуги.");
                    return;
                }

                // Проверка корректности стоимости услуги
                if (!decimal.TryParse(ServiceCostTextBox.Text, out serviceCost) || serviceCost <= 0)
                {
                    MessageBox.Show("Введите корректную стоимость услуги (больше 0).");
                    return;
                }

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    // Проверка на уникальность названия услуги
                    string checkQuery = "SELECT COUNT(*) FROM Услуги WHERE Название_услуги = @ServiceName";
                    SqlCommand checkCommand = new SqlCommand(checkQuery, connection);
                    checkCommand.Parameters.AddWithValue("@ServiceName", serviceName);
                    int count = (int)checkCommand.ExecuteScalar();

                    if (count > 0)
                    {
                        MessageBox.Show("Услуга с таким названием уже существует.");
                        return;
                    }

                    // Вставка новой услуги
                    string query = "INSERT INTO Услуги (Название_услуги, Стоимость) VALUES (@ServiceName, @Cost)";
                    SqlCommand command = new SqlCommand(query, connection);
                    command.Parameters.AddWithValue("@ServiceName", serviceName);
                   
                    command.Parameters.AddWithValue("@Cost", serviceCost);

                    command.ExecuteNonQuery();

                    MessageBox.Show("Услуга успешно добавлена!");

                    inputGrid.Visibility = Visibility.Collapsed;
                    ElementsGrid.Visibility = Visibility.Visible;

                    LoadServices();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при добавлении услуги: " + ex.Message);
            }
        }

    

        private void EditServicesMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (ServicesDataGrid.SelectedItem != null)
            {
                DataRowView selectedRow = (DataRowView)ServicesDataGrid.SelectedItem;
                string serviceName = selectedRow["Название_услуги"].ToString();
               
                decimal serviceCost = Convert.ToDecimal(selectedRow["Стоимость"]);

               

                TextBox newServiceNameTextBox_Edit = (TextBox)editGrid.FindName("newServiceNameTextBox_Edit");
               
                TextBox newServiceCostTextBox_Edit = (TextBox)editGrid.FindName("newServiceCostTextBox_Edit");


                if (serviceCost <= 0)
                {
                    MessageBox.Show("Введите корректную стоимость услуги (больше 0).");
                    return;
                }

                if (newServiceNameTextBox_Edit != null  && newServiceCostTextBox_Edit != null)
                {
                    newServiceNameTextBox_Edit.Text = serviceName;
                   
                    newServiceCostTextBox_Edit.Text = serviceCost.ToString("F2");

                    editGrid.Visibility = Visibility.Visible;
                    ElementsGrid.Visibility = Visibility.Collapsed;
                }
            }
            else
            {
                MessageBox.Show("Выберите услугу для редактирования.");
            }
        }

        private void ApplyChangesButton_Click(object sender, RoutedEventArgs e)
        {
            if (ServicesDataGrid.SelectedItem != null)
            {
                try
                {
                    DataRowView selectedRow = (DataRowView)ServicesDataGrid.SelectedItem;
                    string oldServiceName = selectedRow["Название_услуги"].ToString();

                    TextBox newServiceNameTextBox_Edit = (TextBox)editGrid.FindName("newServiceNameTextBox_Edit");
                   
                    TextBox newServiceCostTextBox_Edit = (TextBox)editGrid.FindName("newServiceCostTextBox_Edit");

                    string newServiceName = newServiceNameTextBox_Edit.Text.Trim();
                   
                    decimal newServiceCost;

                    // Проверка на пустое название услуги
                    if (string.IsNullOrEmpty(newServiceName))
                    {
                        MessageBox.Show("Введите название услуги.");
                        return;
                    }

                    // Проверка корректности стоимости услуги
                    if (!decimal.TryParse(newServiceCostTextBox_Edit.Text, out newServiceCost) || newServiceCost <= 0)
                    {
                        MessageBox.Show("Введите корректную стоимость услуги (больше 0).");
                        return;
                    }

                    using (SqlConnection connection = new SqlConnection(connectionString))
                    {
                        connection.Open();

                        // Проверка на уникальность нового названия услуги
                        if (!newServiceName.Equals(oldServiceName, StringComparison.OrdinalIgnoreCase))
                        {
                            string checkQuery = "SELECT COUNT(*) FROM Услуги WHERE Название_услуги = @NewServiceName";
                            SqlCommand checkCommand = new SqlCommand(checkQuery, connection);
                            checkCommand.Parameters.AddWithValue("@NewServiceName", newServiceName);
                            int count = (int)checkCommand.ExecuteScalar();

                            if (count > 0)
                            {
                                MessageBox.Show("Услуга с таким названием уже существует.");
                                return;
                            }
                        }

                        // Обновление данных услуги
                        string query = @"UPDATE Услуги 
                                 SET Название_услуги = @NewServiceName,
                                     Стоимость = @NewServiceCost 
                                 WHERE Название_услуги = @OldServiceName";

                        SqlCommand command = new SqlCommand(query, connection);
                        command.Parameters.AddWithValue("@NewServiceName", newServiceName);
                       
                        command.Parameters.AddWithValue("@NewServiceCost", newServiceCost);
                        command.Parameters.AddWithValue("@OldServiceName", oldServiceName);

                        int affectedRows = command.ExecuteNonQuery();

                        if (affectedRows > 0)
                        {
                            MessageBox.Show("Информация обновлена успешно!");
                            editGrid.Visibility = Visibility.Collapsed;
                            LoadServices();
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
                    MessageBox.Show("Ошибка при изменении информации о услуге: " + ex.Message);
                }
            }
            else
            {
                MessageBox.Show("Выберите услугу для изменения.");
            }
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

                        string query = "SELECT * FROM Услуги WHERE ";

                        List<string> conditions = new List<string>();
                        for (int i = 0; i < searchTerms.Length; i++)
                        {
                            string parameterName = "@SearchText" + i;
                            conditions.Add($"(Название_услуги LIKE {parameterName} OR Стоимость LIKE {parameterName} OR Количество LIKE {parameterName})");
                        }

                        query += string.Join(" AND ", conditions);

                        SqlDataAdapter adapter = new SqlDataAdapter(query, connection);
                        for (int i = 0; i < searchTerms.Length; i++)
                        {
                            string parameterName = "@SearchText" + i;
                            adapter.SelectCommand.Parameters.AddWithValue(parameterName, "%" + searchTerms[i] + "%");
                        }

                        DataTable dataTable = new DataTable();
                        adapter.Fill(dataTable);

                        if (dataTable.Columns.Contains("ID_Услуги"))
                        {
                            dataTable.Columns.Remove("ID_Услуги");
                        }

                        ServicesDataGrid.ItemsSource = dataTable.DefaultView;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ошибка при выполнении поиска: " + ex.Message);
                }
            }
            else
            {
                LoadServices(); // Показать все данные, если поисковый запрос пуст
            }
        }

        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            SearchTextBox.Text = string.Empty;
            LoadServices();
        }

        private void BackButtonEdit_Click(object sender, RoutedEventArgs e)
        {
            editGrid.Visibility = Visibility.Collapsed;
            ElementsGrid.Visibility = Visibility.Visible;
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            inputGrid.Visibility = Visibility.Collapsed;
            ElementsGrid.Visibility = Visibility.Visible;
        }

        private void ExportButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                DataTable dataTable = ((DataView)ServicesDataGrid.ItemsSource).ToTable();

                if (dataTable.Rows.Count == 0)
                {
                    MessageBox.Show("Нет данных для экспорта.");
                    return;
                }

                using (XLWorkbook workbook = new XLWorkbook())
                {
                    var worksheet = workbook.Worksheets.Add("Услуги");

                    // Добавление заголовка
                    worksheet.Cell(1, 1).Value = "Услуги ООО 'ПВЗ'";
                    worksheet.Range(1, 1, 1, 3).Merge().Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                    // Названия столбцов
                    worksheet.Cell(2, 1).Value = "Название услуги";
                    worksheet.Cell(2, 2).Value = "Стоимость";
                    worksheet.Cell(2, 3).Value = "Количество выполнений";

                    // Заполнение данных
                    for (int i = 0; i < dataTable.Rows.Count; i++)
                    {
                        worksheet.Cell(i + 3, 1).Value = dataTable.Rows[i]["Название_услуги"].ToString();
                        worksheet.Cell(i + 3, 2).Value = Convert.ToDecimal(dataTable.Rows[i]["Стоимость"]);
                        worksheet.Cell(i + 3, 3).Value = Convert.ToInt32(dataTable.Rows[i]["Количество"]);
                    }

                    // Автоподстройка ширины столбцов
                    worksheet.Columns().AdjustToContents();

                    // Установка границ
                    var allCells = worksheet.RangeUsed();
                    allCells.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    allCells.Style.Border.InsideBorder = XLBorderStyleValues.Thin;

                    // Сохранение файла на рабочем столе
                    string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                    string fileName = $"Услуги_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
                    string filePath = Path.Combine(desktopPath, fileName);

                    workbook.SaveAs(filePath);

                    MessageBox.Show($"Отчет успешно сохранен на рабочем столе: {filePath}");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при экспорте данных: " + ex.Message);
            }
        }

       
    }








}


