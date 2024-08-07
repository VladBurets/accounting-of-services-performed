using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using static BdKursach.OrdersWindow;
using System.Text.RegularExpressions;

namespace BdKursach
{
    /// <summary>
    /// Логика взаимодействия для EmployeesWindow.xaml
    /// </summary>
    public partial class EmployeesWindow : Window
    {
        private string connectionString = "Data Source=DESKTOP-G94EAHJ\\SQLEXPRESS;Initial Catalog=диплом4;Integrated Security=True";
        

        public EmployeesWindow()
        {
            InitializeComponent();
            LoadEmployees();
           
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

        private DataView originalDataView;

        private void LoadEmployees()
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    string query = @"SELECT Сотрудники.ФИО, Сотрудники.Должность, Сотрудники.Стаж, Сотрудники.ID_Сотрудника  FROM Сотрудники";
                            
                             
                            
                    SqlDataAdapter adapter = new SqlDataAdapter(query, connection);
                    DataTable dataTable = new DataTable();
                    adapter.Fill(dataTable);

                    EmployeesDataGrid.ItemsSource = dataTable.DefaultView;
                    originalDataView = dataTable.DefaultView;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при загрузке сотрудников: " + ex.Message);
            }
        }

        private void AddMenuItem_Click(object sender, RoutedEventArgs e)
        {
            inputGrid.Visibility = Visibility.Visible;
            ElementGrid.Visibility = Visibility.Collapsed;
            EmployeeNameTextBox.Text = string.Empty;
 
            ExperienceTextBox.Text = string.Empty;
        }

        private void AddEmployeesButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string employeeName = EmployeeNameTextBox.Text.Trim(); // Убираем пробелы в начале и конце

                ComboBoxItem selectedJobTitleItem = cmbJobTitle.SelectedItem as ComboBoxItem;
                string jobTitle = selectedJobTitleItem.Content.ToString();
               


                // Проверка на корректность ввода ФИО с использованием регулярного выражения
                if (!string.IsNullOrWhiteSpace(employeeName) && Regex.IsMatch(employeeName, @"^[А-ЯЁ][а-яё]+\s[А-ЯЁ][а-яё]+\s[А-ЯЁ][а-яё]+$"))
                {
                    // Проверка на корректность ввода должности
                    if (!string.IsNullOrWhiteSpace(jobTitle))
                    {
                        // Проверка на корректность ввода стажа
                        if (int.TryParse(ExperienceTextBox.Text, out int experience) && experience >= 1 && experience <= 60)
                        {
                           

                            
                                using (SqlConnection connection = new SqlConnection(connectionString))
                                {
                                    connection.Open();

                                    string query = "INSERT INTO Сотрудники (ФИО, Должность, Стаж) VALUES (@Name, @JobTitle, @Experience)";
                                    SqlCommand command = new SqlCommand(query, connection);
                                    command.Parameters.AddWithValue("@Name", employeeName);
                                    command.Parameters.AddWithValue("@JobTitle", jobTitle);
                                    command.Parameters.AddWithValue("@Experience", experience);
                                   
                                    command.ExecuteNonQuery();

                                    MessageBox.Show("Сотрудник успешно добавлен!");

                                    inputGrid.Visibility = Visibility.Collapsed;
                                    ElementGrid.Visibility = Visibility.Visible;

                                    LoadEmployees();
                                   
                                }
                           
                        }
                        else
                        {
                            MessageBox.Show("Введите корректный стаж в поле 'Стаж' от 1 до 60.");
                        }
                    }
                    else
                    {
                        MessageBox.Show("Введите корректную должность в поле 'Должность'.");
                    }
                }
                else
                {
                    MessageBox.Show("Введите корректное ФИО в поле 'ФИО'.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при добавлении сотрудника: " + ex.Message);
            }
        }


       

        private void EditMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (EmployeesDataGrid.SelectedItem != null)
            {
                DataRowView selectedRow = (DataRowView)EmployeesDataGrid.SelectedItem;
                string employeeName = selectedRow["ФИО"].ToString();
               
                int employeeExperience = int.Parse(selectedRow["Стаж"].ToString());

                TextBox newExperienceTextBox_Edit = (TextBox)editGrid.FindName("newExperienceTextBox_Edit");


                if (newEmployeeNameTextBox_Edit != null && newExperienceTextBox_Edit != null)
                {
                    newEmployeeNameTextBox_Edit.Text = employeeName;

                    newExperienceTextBox_Edit.Text = employeeExperience.ToString();


                    editGrid.Visibility = Visibility.Visible;
                    ElementGrid.Visibility = Visibility.Collapsed;
                }

            }
            else
            {
                MessageBox.Show("Выберите сотрудника для редактирования.");
            }
        }

        private string oldEmploye;

       


        private void EmployeDataGridRow_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (EmployeesDataGrid.SelectedItem != null)
            {
                DataRowView selectedRow = (DataRowView)EmployeesDataGrid.SelectedItem;

                oldEmploye = selectedRow["Должность"].ToString();

            }
        }

        private void ApplyChangesButton_Click(object sender, RoutedEventArgs e)
        {
            if (EmployeesDataGrid.SelectedItem != null)
            {

                DataRowView selectedRow = (DataRowView)EmployeesDataGrid.SelectedItem;
                string employeeName = selectedRow["ФИО"].ToString();

                TextBox newEmployeeNameTextBox_Edit = (TextBox)editGrid.FindName("newEmployeeNameTextBox_Edit");

                TextBox newExperienceTextBox_Edit = (TextBox)editGrid.FindName("newExperienceTextBox_Edit");

                



                ComboBox cmbNewJobTitle = (ComboBox)editGrid.FindName("cmbNewJobTitle");
                ComboBoxItem selectedJobItem = cmbNewJobTitle.SelectedItem as ComboBoxItem;
                string newJobTitle;

                if (cmbNewJobTitle.SelectedItem != null)
                {
                    newJobTitle = selectedJobItem.Content.ToString();
                }
                else
                {
                    newJobTitle = oldEmploye;
                }


                string newEmployeeName = newEmployeeNameTextBox_Edit.Text.Trim();


                // Проверка на корректность ввода ФИО с использованием регулярного выражения
                if (!string.IsNullOrWhiteSpace(newEmployeeName) && Regex.IsMatch(newEmployeeName, @"^[А-ЯЁ][а-яё]+\s[А-ЯЁ][а-яё]+\s[А-ЯЁ][а-яё]+$"))
                {
                    // Проверка на корректность ввода должности
                    if (!string.IsNullOrWhiteSpace(newJobTitle))
                    {
                        // Проверка на корректность ввода стажа
                        if (int.TryParse(newExperienceTextBox_Edit.Text, out int newExperience) && newExperience >= 1 && newExperience <= 60)
                        {
                            //Otdel selectedOtdel = newCmbOtdel.SelectedItem as Otdel;
                           

                            // Проверка на выбор отдела

                            using (SqlConnection connection = new SqlConnection(connectionString))
                                {
                                    connection.Open();

                                    string query = "UPDATE Сотрудники SET ФИО = @NewName, Должность = @JobTitle, Стаж = @Experience " +
                                                   "WHERE ФИО = @OldName";

                                    SqlCommand command = new SqlCommand(query, connection);
                                    command.Parameters.AddWithValue("@NewName", newEmployeeName);
                                    command.Parameters.AddWithValue("@JobTitle", newJobTitle);
                                    command.Parameters.AddWithValue("@Experience", newExperience);
                                   
                                    command.Parameters.AddWithValue("@OldName", employeeName);


                                    int affectedRows = command.ExecuteNonQuery();

                                    if (affectedRows > 0)
                                    {
                                        MessageBox.Show("Информация обновлена успешно!");
                                        editGrid.Visibility = Visibility.Collapsed;
                                        ElementGrid.Visibility = Visibility.Visible;
                                        LoadEmployees();

                                    }
                                    else
                                    {
                                        MessageBox.Show("Ошибка при обновлении информации.");
                                    }
                                }
                            
                        }
                        else
                        {
                            MessageBox.Show("Введите корректный стаж в поле 'Новый стаж' от 1 до 60.");
                        }
                    }
                    else
                    {
                        MessageBox.Show("Введите корректную должность в поле 'Новая должность'.");
                    }
                }
                else
                {
                    MessageBox.Show("Введите корректное ФИО в поле 'Новое ФИО'.");
                }


            }
            else
            {
                MessageBox.Show("Выберите сотрудника для изменения.");
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

                        string query = "SELECT * FROM Сотрудники WHERE ";

                        List<string> conditions = new List<string>();
                        for (int i = 0; i < searchTerms.Length; i++)
                        {
                            string parameterName = "@SearchText" + i;
                            conditions.Add($"(ФИО LIKE {parameterName} OR Должность LIKE {parameterName} OR Стаж LIKE {parameterName})");
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

                        if (dataTable.Columns.Contains("ID_Сотрудника"))
                        {
                            dataTable.Columns.Remove("ID_Сотрудника");
                        }

                        EmployeesDataGrid.ItemsSource = dataTable.DefaultView;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ошибка при выполнении поиска: " + ex.Message);
                }
            }
            else
            {
                LoadEmployees(); // Показать все данные, если поисковый запрос пуст
            }
        }

        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            SearchTextBox.Text = string.Empty;
            LoadEmployees();
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            inputGrid.Visibility = Visibility.Collapsed;
            ElementGrid.Visibility = Visibility.Visible;
        }

        private void BackButtonEdit_Click(object sender, RoutedEventArgs e)
        {
            editGrid.Visibility = Visibility.Collapsed;
            ElementGrid.Visibility = Visibility.Visible;
        }

       

        private void ArchiveEmployeesWindow_EmployeeRestored(object sender, EventArgs e)
        {
            LoadEmployees(); 
        }


    }
}
