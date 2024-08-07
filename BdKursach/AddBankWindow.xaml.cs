using System;
using System.Collections.Generic;
using System.Data.SqlClient;
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

namespace BdKursach
{
    /// <summary>
    /// Логика взаимодействия для AddBankWindow.xaml
    /// </summary>
    public partial class AddBankWindow : Window
    {
        private string connectionString = "Data Source=DESKTOP-G94EAHJ\\SQLEXPRESS;Initial Catalog=диплом4;Integrated Security=True";

        public AddBankWindow()
        {
            InitializeComponent();
            LoadBanks();
        }

        public event Action BankAdded;

        private void AddBankButton_Click(object sender, RoutedEventArgs e)
        {
            string bankName = BankNameTextBox.Text.Trim();

            // Проверка на пустое поле
            if (string.IsNullOrWhiteSpace(bankName))
            {
                MessageBox.Show("Пожалуйста введите название банка");
                return;
            }

            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    string query = "INSERT INTO Банк (Название_банка) VALUES (@BankName)";
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@BankName", bankName);
                        command.ExecuteNonQuery();
                    }
                }

                MessageBox.Show("Банк успешно добавлен");

                BankAdded?.Invoke(); // Вызов события
                LoadBanks(); // Обновление списка банков

                BankNameTextBox.Clear();

                this.Close(); // Закрываем окно после добавления
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при добавлении банка : " + ex.Message);
            }
        }

        private void LoadBanks()
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    string query = "SELECT ID_Банка, Название_банка FROM Банк";
                    SqlCommand cmd = new SqlCommand(query, connection);
                    SqlDataReader reader = cmd.ExecuteReader();

                    cmbBankList.Items.Clear(); // Очистка текущих элементов ComboBox

                    while (reader.Read())
                    {
                        ComboBoxItem item = new ComboBoxItem
                        {
                            Content = reader["Название_банка"].ToString(),
                            Tag = reader["ID_Банка"] // Сохраняем ID банка в Tag для удобства удаления
                        };
                        cmbBankList.Items.Add(item);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при загрузке банков: " + ex.Message);
            }
        }

        private void DeleteBankButton_Click(object sender, RoutedEventArgs e)
        {
            ComboBoxItem selectedItem = cmbBankList.SelectedItem as ComboBoxItem;

            if (selectedItem == null)
            {
                MessageBox.Show("Пожалуйста, выберите банк для удаления");
                return;
            }

            int bankId = (int)selectedItem.Tag;

            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    // Проверка наличия связанных записей в таблице Заказчики
                    string checkQuery = "SELECT COUNT(*) FROM Заказчики WHERE ID_Банка = @BankId";
                    SqlCommand checkCommand = new SqlCommand(checkQuery, connection);
                    checkCommand.Parameters.AddWithValue("@BankId", bankId);
                    int relatedRecordsCount = (int)checkCommand.ExecuteScalar();

                    if (relatedRecordsCount > 0)
                    {
                        MessageBox.Show("Невозможно удалить банк, так как он связан с заказчиками.");
                        return;
                    }

                    // Удаление банка
                    string deleteQuery = "DELETE FROM Банк WHERE ID_Банка = @BankId";
                    using (SqlCommand deleteCommand = new SqlCommand(deleteQuery, connection))
                    {
                        deleteCommand.Parameters.AddWithValue("@BankId", bankId);
                        deleteCommand.ExecuteNonQuery();
                    }
                }

                MessageBox.Show("Банк успешно удален");

                BankAdded?.Invoke(); // Вызов события для обновления списка банков в основном окне

                LoadBanks(); // Обновление списка банков
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при удалении банка: " + ex.Message);
            }
        }



    }
}
