using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows;
using System.Security.Cryptography;


namespace BdKursach.Pages
{
    /// <summary>
    /// Логика взаимодействия для LoginWindow.xaml
    /// </summary>


    public partial class LoginWindow : Window
    {
        private string connectionString = "Data Source=DESKTOP-G94EAHJ\\SQLEXPRESS;Initial Catalog=диплом4;Integrated Security=True";

        public LoginWindow()
        {
            InitializeComponent();
        }
        public class User
        {
            public string Login { get; set; }
            public string PasswordHash { get; set; }
        }

        private void ComeButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    string query = "SELECT PasswordHash FROM Users WHERE Login = @Login";
                    SqlCommand command = new SqlCommand(query, connection);
                    command.Parameters.AddWithValue("@Login", LoginTextBox.Text);

                    string inputPasswordHash = HashPassword(PasswordTextBox.Password);

                    SqlDataReader reader = command.ExecuteReader();

                    if (reader.Read())
                    {
                        string storedPasswordHash = reader["PasswordHash"].ToString();

                        if (string.Equals(inputPasswordHash, storedPasswordHash, StringComparison.OrdinalIgnoreCase))
                        {
                            User authenticatedClient = new User
                            {
                                Login = LoginTextBox.Text,
                                PasswordHash = storedPasswordHash
                            };

                            // Сообщение об успешном входе
                            MessageBox.Show("Вход выполнен успешно!");

                            ClientsWindow clientsWindow = new ClientsWindow();
                            clientsWindow.Show();
                            this.Close();
                        }
                        else
                        {
                            // Неверный пароль
                            MessageBox.Show("Неверный пароль");
                        }
                    }
                    else
                    {
                        // Пользователь с таким логином не найден
                        MessageBox.Show("Пользователь с таким логином не найден");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка: " + ex.Message);
            }
        }


        private string HashPassword(string password)
        {
            using (SHA256 sha256Hash = SHA256.Create())
            {
                byte[] bytes = sha256Hash.ComputeHash(Encoding.Unicode.GetBytes(password));

                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }
                return builder.ToString();
            }
        }
      
    }



}


