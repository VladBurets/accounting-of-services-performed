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
using System.Text.RegularExpressions;
using System.Security.Principal;
using static BdKursach.OrdersWindow;
using System.Net;

namespace BdKursach
{
    
    public partial class ClientsWindow : Window
    {
        private string connectionString = "Data Source=DESKTOP-G94EAHJ\\SQLEXPRESS;Initial Catalog=диплом4;Integrated Security=True";


        public ClientsWindow()
        {
            InitializeComponent();
            LoadClients();
            BankComboBox();

        }

        private void ClientsMenuItem_Click(object sender, RoutedEventArgs e)
        {
            ClientsWindow clientsWindow = new ClientsWindow();
            clientsWindow.Show();
            this.Hide(); 
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

        public class Bank
        {
            public string BankId { get; set; }
            public string BankName { get; set; }
        }

       

        private void BankComboBox()
        {
            cmbBank.Items.Clear();
            newCmbBank.Items.Clear();

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string query = "SELECT ID_Банка, Название_банка FROM Банк";

                SqlCommand cmd = new SqlCommand(query, connection);
                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    Bank bank = new Bank
                    {
                        BankId = reader["ID_Банка"].ToString(),
                        BankName = reader["Название_банка"].ToString()
                    };
                    cmbBank.Items.Add(bank);
                    newCmbBank.Items.Add(bank);
                }
                cmbBank.DisplayMemberPath = "BankName";
                newCmbBank.DisplayMemberPath = "BankName";
                connection.Close();
            }
        }


        private void LoadClients()
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    string query = @"SELECT Заказчики.Название, Заказчики.Контактное_лицо, Заказчики.Телефон, Заказчики.Электронная_почта, Заказчики.Адрес, Заказчики.Расчетный_счет, Банк.Название_банка AS Банк, Заказчики.ID_заказчика, Банк.ID_Банка
                     FROM Заказчики
                     INNER JOIN Банк ON Заказчики.ID_Банка = Банк.ID_Банка";

                    SqlDataAdapter adapter = new SqlDataAdapter(query, connection);
                    DataTable dataTable = new DataTable();
                    adapter.Fill(dataTable);

                    ClientsDataGrid.ItemsSource = dataTable.DefaultView;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при загрузке заказчиков: " + ex.Message);
            }
        }


        private void AddClientsMenuItem_Click(object sender, RoutedEventArgs e)
        {
            inputGrid.Visibility = Visibility.Visible;
            ElementsGrid.Visibility = Visibility.Collapsed;


            CustomerNameTextBox.Text = string.Empty;
            ClientNameTextBox.Text = string.Empty;
            ContactInfoTextBox.Text = string.Empty;
            CityTextBox.Text = string.Empty;
            StreetTextBox.Text = string.Empty;
            HouseNumberTextBox.Text = string.Empty;
            EmailTextBox.Text = string.Empty;
            AccountTextBox.Text = string.Empty;
            
        }

        private void AddClientsButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string customerName = CustomerNameTextBox.Text.Trim();
                string clientName = ClientNameTextBox.Text.Trim();
                string contactInfo = ContactInfoTextBox.Text.Trim();
                string email = EmailTextBox.Text.Trim();
                string city = CityTextBox.Text.Trim();
                string street = StreetTextBox.Text.Trim();
                string houseNumber = HouseNumberTextBox.Text.Trim();
                string account = AccountTextBox.Text.Trim();

                // Объединение адреса в нужный формат
                string address = $"г. {city} ул. {street} д. {houseNumber}";

                // Проверка на пустые поля
                if (string.IsNullOrWhiteSpace(customerName) || string.IsNullOrWhiteSpace(clientName) ||
                    string.IsNullOrWhiteSpace(contactInfo) || string.IsNullOrWhiteSpace(city) ||
                    string.IsNullOrWhiteSpace(street) || string.IsNullOrWhiteSpace(houseNumber) ||
                    string.IsNullOrWhiteSpace(account))
                {
                    MessageBox.Show("Все поля должны быть заполнены.");
                    return;
                }

                // Проверка, что город состоит только из букв
                if (!Regex.IsMatch(city, @"^[А-Яа-яA-Za-z\s]+$"))
                {
                    MessageBox.Show("Город должен содержать только буквы.");
                    return;
                }

                // Проверка, что улица состоит только из букв
                if (!Regex.IsMatch(street, @"^[А-Яа-яA-Za-z\s]+$"))
                {
                    MessageBox.Show("Улица должна содержать только буквы.");
                    return;
                }

                Bank selectedBank = cmbBank.SelectedItem as Bank;
                if (selectedBank == null)
                {
                    MessageBox.Show("Выберите банк.");
                    return;
                }

                // Проверка контактной информации
                if (!Regex.IsMatch(contactInfo, @"^\+37529\d{7}$|^\+37544\d{7}$"))
                {
                    MessageBox.Show("Введите корректный номер в поле 'Контактная информация'. Формат: +37529****** или +37544******.");
                    return;
                }

                // Проверка ФИО клиента
                if (!Regex.IsMatch(clientName, @"^[A-ZА-Я][a-zа-я]+(?:\s+[A-ZА-ЯЁ][a-zа-яё]+){0,3}$"))
                {
                    MessageBox.Show("Введите корректное ФИО.");
                    return;
                }

                // Проверка электронной почты (не обязательно)
                if (!string.IsNullOrEmpty(email) && !Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
                {
                    MessageBox.Show("Введите корректный адрес электронной почты.");
                    return;
                }

                // Проверка расчетного счета на наличие только букв и цифр и его длины
                if (!string.IsNullOrEmpty(account) && (!Regex.IsMatch(account, @"^[A-Za-z0-9]{28}$")))
                {
                    MessageBox.Show("Введите корректный расчетный счет. Расчетный счет должен состоять из 28 букв и цифр.");
                    return;
                }

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    // Проверка на уникальность названия заказчика
                    string checkQuery = "SELECT COUNT(*) FROM Заказчики WHERE Название = @Name";
                    SqlCommand checkCommand = new SqlCommand(checkQuery, connection);
                    checkCommand.Parameters.AddWithValue("@Name", customerName);
                    int existingCount = (int)checkCommand.ExecuteScalar();
                    if (existingCount > 0)
                    {
                        MessageBox.Show("Заказчик с таким названием уже существует.");
                        return;
                    }

                    // Вставка нового заказчика
                    string query = "INSERT INTO Заказчики (Название, Контактное_лицо, Телефон, Электронная_почта, Адрес, Расчетный_счет, ID_Банка) " +
                                   "VALUES (@Name, @ContactInfo, @Phone, @Email, @Address, @Account, @BankId)";
                    SqlCommand command = new SqlCommand(query, connection);
                    command.Parameters.AddWithValue("@Name", customerName);
                    command.Parameters.AddWithValue("@ContactInfo", clientName);
                    command.Parameters.AddWithValue("@Phone", contactInfo);
                    command.Parameters.AddWithValue("@Email", email);
                    command.Parameters.AddWithValue("@Address", address);
                    command.Parameters.AddWithValue("@Account", account);
                    command.Parameters.AddWithValue("@BankId", selectedBank.BankId);

                    command.ExecuteNonQuery();

                    MessageBox.Show("Заказчик успешно добавлен!");

                    inputGrid.Visibility = Visibility.Collapsed;
                    ElementsGrid.Visibility = Visibility.Visible;

                    LoadClients();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при добавлении заказчика: " + ex.Message);
            }
        }



        private void EditClientsMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (ClientsDataGrid.SelectedItem != null)
            {
                DataRowView selectedRow = (DataRowView)ClientsDataGrid.SelectedItem;
                string customerName = selectedRow["Название"].ToString();
                string clientContactInfo = selectedRow["Телефон"].ToString();
                string clientName = selectedRow["Контактное_лицо"].ToString();
                string email = selectedRow["Электронная_почта"].ToString();
                string address = selectedRow["Адрес"].ToString();
                string account = selectedRow["Расчетный_счет"].ToString();

                // Разбор адреса на компоненты
                string city = "", street = "", houseNumber = "";
                var match = Regex.Match(address, @"г\. (.+?) ул\. (.+?) д\. (.+)");
                if (match.Success)
                {
                    city = match.Groups[1].Value;
                    street = match.Groups[2].Value;
                    houseNumber = match.Groups[3].Value;
                }

                TextBox newCustomerNameTextBox_Edit = (TextBox)editGrid.FindName("newCustomerNameTextBox_Edit");
                TextBox newClientNameTextBox_Edit = (TextBox)editGrid.FindName("newClientNameTextBox_Edit");
                TextBox newContactInfoTextBox_Edit = (TextBox)editGrid.FindName("newContactInfoTextBox_Edit");
                TextBox newEmailTextBox_Edit = (TextBox)editGrid.FindName("newEmailTextBox_Edit");
                TextBox cityTextBox_Edit = (TextBox)editGrid.FindName("CityTextBox_Edit");
                TextBox streetTextBox_Edit = (TextBox)editGrid.FindName("StreetTextBox_Edit");
                TextBox houseNumberTextBox_Edit = (TextBox)editGrid.FindName("HouseNumberTextBox_Edit");
                TextBox newAccountTextBox_Edit = (TextBox)editGrid.FindName("newAccountTextBox_Edit");

                if (newCustomerNameTextBox_Edit != null && newContactInfoTextBox_Edit != null && newClientNameTextBox_Edit != null && newEmailTextBox_Edit != null
                    && cityTextBox_Edit != null && streetTextBox_Edit != null && houseNumberTextBox_Edit != null && newAccountTextBox_Edit != null)
                {
                    newCustomerNameTextBox_Edit.Text = customerName;
                    newContactInfoTextBox_Edit.Text = clientContactInfo;
                    newClientNameTextBox_Edit.Text = clientName;
                    newEmailTextBox_Edit.Text = email;
                    cityTextBox_Edit.Text = city;
                    streetTextBox_Edit.Text = street;
                    houseNumberTextBox_Edit.Text = houseNumber;
                    newAccountTextBox_Edit.Text = account;

                    editGrid.Visibility = Visibility.Visible;
                    ElementsGrid.Visibility = Visibility.Collapsed;
                }
            }
            else
            {
                MessageBox.Show("Выберите клиента для редактирования.");
            }
        }


        private string oldBank;

        private void ClientsDataGridRow_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (ClientsDataGrid.SelectedItem != null)
            {
                DataRowView selectedRow = (DataRowView)ClientsDataGrid.SelectedItem;

                oldBank = selectedRow["id_Банка"].ToString();

            }
        }

        private void ApplyChangesClientsButton_Click(object sender, RoutedEventArgs e)
        {
            if (ClientsDataGrid.SelectedItem != null)
            {
                try
                {
                    DataRowView selectedRow = (DataRowView)ClientsDataGrid.SelectedItem;
                    string oldCustomerName = selectedRow["Название"].ToString();

                    TextBox newCustomerNameTextBox_Edit = (TextBox)editGrid.FindName("newCustomerNameTextBox_Edit");
                    TextBox newClientNameTextBox_Edit = (TextBox)editGrid.FindName("newClientNameTextBox_Edit");
                    TextBox newContactInfoTextBox_Edit = (TextBox)editGrid.FindName("newContactInfoTextBox_Edit");
                    TextBox newEmailTextBox_Edit = (TextBox)editGrid.FindName("newEmailTextBox_Edit");
                    TextBox cityTextBox_Edit = (TextBox)editGrid.FindName("CityTextBox_Edit");
                    TextBox streetTextBox_Edit = (TextBox)editGrid.FindName("StreetTextBox_Edit");
                    TextBox houseNumberTextBox_Edit = (TextBox)editGrid.FindName("HouseNumberTextBox_Edit");
                    TextBox newAccountTextBox_Edit = (TextBox)editGrid.FindName("newAccountTextBox_Edit");

                    string newCustomerName = newCustomerNameTextBox_Edit.Text.Trim();
                    string newClientName = newClientNameTextBox_Edit.Text.Trim();
                    string newContactInfo = newContactInfoTextBox_Edit.Text.Trim();
                    string newEmail = newEmailTextBox_Edit.Text.Trim();
                    string city = cityTextBox_Edit.Text.Trim();
                    string street = streetTextBox_Edit.Text.Trim();
                    string houseNumber = houseNumberTextBox_Edit.Text.Trim();
                    string newAccount = newAccountTextBox_Edit.Text.Trim();

                    string newBankId;
                    if (newCmbBank.SelectedItem != null)
                    {
                        newBankId = ((Bank)newCmbBank.SelectedItem).BankId;
                    }
                    else
                    {
                        newBankId = oldBank;
                    }

                    // Объединение адреса в нужный формат
                    string newAddress = $"г. {city} ул. {street} д. {houseNumber}";

                    // Проверка на пустые поля
                    if (string.IsNullOrWhiteSpace(newCustomerName) || string.IsNullOrWhiteSpace(newClientName) ||
                        string.IsNullOrWhiteSpace(newContactInfo) || string.IsNullOrWhiteSpace(city) ||
                        string.IsNullOrWhiteSpace(street) || string.IsNullOrWhiteSpace(houseNumber) || string.IsNullOrWhiteSpace(newAccount))
                    {
                        MessageBox.Show("Пожалуйста, заполните все поля.");
                        return;
                    }

                    // Проверка города на наличие только букв
                    if (!Regex.IsMatch(city, @"^[а-яА-ЯёЁa-zA-Z\s]+$"))
                    {
                        MessageBox.Show("Город должен содержать только буквы.");
                        return;
                    }

                    // Проверка улицы на наличие только букв
                    if (!Regex.IsMatch(street, @"^[а-яА-ЯёЁa-zA-Z\s]+$"))
                    {
                        MessageBox.Show("Улица должна содержать только буквы.");
                        return;
                    }

                    if (!Regex.IsMatch(newClientName, @"^[A-ZА-Я][a-zа-я]+(?:\s+[A-ZА-ЯЁ][a-zа-яё]+){0,3}$"))
                    {
                        MessageBox.Show("Введите корректное ФИО в поле 'Новое ФИО'.");
                        return;
                    }

                    if (!Regex.IsMatch(newContactInfo, @"^\+37529\d{7}$|^\+37544\d{7}$"))
                    {
                        MessageBox.Show("Введите корректный номер в поле 'Новая контактная информация'.");
                        return;
                    }

                    if (!Regex.IsMatch(newEmail, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
                    {
                        MessageBox.Show("Введите корректный адрес электронной почты.");
                        return;
                    }

                    // Проверка расчетного счета на наличие только букв и цифр и его длины
                    if (!Regex.IsMatch(newAccount, @"^[A-Za-z0-9]{28}$"))
                    {
                        MessageBox.Show("Расчетный счет должен содержать 28 букв и цифр.");
                        return;
                    }

                    using (SqlConnection connection = new SqlConnection(connectionString))
                    {
                        connection.Open();

                        // Проверка на уникальность названия заказчика
                        string checkQuery = "SELECT COUNT(*) FROM Заказчики WHERE Название = @NewName AND Название != @OldName";
                        SqlCommand checkCommand = new SqlCommand(checkQuery, connection);
                        checkCommand.Parameters.AddWithValue("@NewName", newCustomerName);
                        checkCommand.Parameters.AddWithValue("@OldName", oldCustomerName);
                        int existingCount = (int)checkCommand.ExecuteScalar();
                        if (existingCount > 0)
                        {
                            MessageBox.Show("Заказчик с таким названием уже существует.");
                            return;
                        }

                        // Обновление информации о заказчике
                        string query = "UPDATE Заказчики SET Название = @NewName, Контактное_лицо = @ContactInfo, Телефон = @Phone, " +
                                       "Электронная_почта = @Email, Адрес = @Address, Расчетный_счет = @Account, ID_Банка = @Bank " +
                                       "WHERE Название = @OldName";
                        SqlCommand command = new SqlCommand(query, connection);
                        command.Parameters.AddWithValue("@NewName", newCustomerName);
                        command.Parameters.AddWithValue("@ContactInfo", newClientName);
                        command.Parameters.AddWithValue("@Phone", newContactInfo);
                        command.Parameters.AddWithValue("@Email", newEmail);
                        command.Parameters.AddWithValue("@Address", newAddress);
                        command.Parameters.AddWithValue("@Account", newAccount);
                        command.Parameters.AddWithValue("@Bank", newBankId);
                        command.Parameters.AddWithValue("@OldName", oldCustomerName);

                        int affectedRows = command.ExecuteNonQuery();

                        if (affectedRows > 0)
                        {
                            MessageBox.Show("Информация о заказчике обновлена успешно!");
                            editGrid.Visibility = Visibility.Collapsed;
                            ElementsGrid.Visibility = Visibility.Visible;
                            LoadClients();
                        }
                        else
                        {
                            MessageBox.Show("Ошибка при обновлении информации.");
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ошибка при изменении информации о заказчике: " + ex.Message);
                }
            }
            else
            {
                MessageBox.Show("Выберите заказчика для изменения.");
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

                        string query = @"SELECT Заказчики.Название, Заказчики.Контактное_лицо, Заказчики.Телефон, Заказчики.Электронная_почта, Заказчики.Адрес, Заказчики.Расчетный_счет, Банк.Название_банка AS Банк, Заказчики.ID_заказчика, Банк.ID_Банка
                                 FROM Заказчики
                                 INNER JOIN Банк ON Заказчики.ID_Банка = Банк.ID_Банка
                                 WHERE ";

                        List<string> conditions = new List<string>();
                        for (int i = 0; i < searchTerms.Length; i++)
                        {
                            string parameterName = "@SearchText" + i;
                            conditions.Add($"(Заказчики.Название LIKE {parameterName} OR Заказчики.Контактное_лицо LIKE {parameterName} OR Заказчики.Телефон LIKE {parameterName} OR Заказчики.Электронная_почта LIKE {parameterName} OR Заказчики.Адрес LIKE {parameterName} OR Заказчики.Расчетный_счет LIKE {parameterName} OR Банк.Название_банка LIKE {parameterName})");
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

                        ClientsDataGrid.ItemsSource = dataTable.DefaultView;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ошибка при выполнении поиска: " + ex.Message);
                }
            }
            else
            {
                LoadClients(); // Показать все данные, если поисковый запрос пуст
            }
        }

        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            SearchTextBox.Text = string.Empty;
            LoadClients();
        }

        private void HistoryOrdersButton_Click(object sender, RoutedEventArgs e)
        {
            if (ClientsDataGrid.SelectedItem != null)
            {
                DataRowView selectedRow = (DataRowView)ClientsDataGrid.SelectedItem;
                int customerId = (int)selectedRow["ID_заказчика"];

                HistoryOrdersWindow historyOrdersWindow = new HistoryOrdersWindow(customerId);
                historyOrdersWindow.Owner = this;
                historyOrdersWindow.CustomerId = customerId; // Устанавливаем значение свойства
                historyOrdersWindow.ShowDialog();
                if (historyOrdersWindow.DialogResult == true)
                {
                    LoadClients();
                }
            }
            else
            {
                MessageBox.Show("Выберите заказчика для просмотра истории заказов.");
            }
        }

        private void BankButton_Click(object sender, RoutedEventArgs e)
        {
            AddBankWindow addBankWindow = new AddBankWindow();
            addBankWindow.BankAdded += OnBankAdded; // Подписка на событие
            addBankWindow.ShowDialog();
        }

                                                                                                                                                                                                                                                                                                                                                                                                                                                                                              
        private void OnBankAdded()
        {
            BankComboBox(); // Обновление списка банков
        }
    }



}


