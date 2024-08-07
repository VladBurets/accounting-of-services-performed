using System;
using System.Collections.Generic;
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
    /// Логика взаимодействия для EditWorkItemWindow.xaml
    /// </summary>
    public partial class EditWorkItemWindow : Window
    {
        public string WorkName { get; set; }
        public decimal Cost { get; set; }

        public EditWorkItemWindow(string workName, decimal cost)
        {
            InitializeComponent();
            workNameTextBox.Text = workName;
            costTextBox.Text = cost.ToString();
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            WorkName = workNameTextBox.Text;
            if (decimal.TryParse(costTextBox.Text, out decimal cost))
            {
                Cost = cost;
                DialogResult = true;
            }
            else
            {
                MessageBox.Show("Введите корректное значение стоимости.");
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}
