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
    /// Логика взаимодействия для PeriodSelectionWindow.xaml
    /// </summary>
    public partial class PeriodSelectionWindow : Window
    {
        public DateTime SelectedStartDate { get; private set; }
        public DateTime SelectedEndDate { get; private set; }
        public PeriodSelectionWindow()
        {
            InitializeComponent();
            InitializeYearComboBox();
            InitializeMonthComboBox();
        }

        private void InitializeYearComboBox()
        {
            for (int year = 2000; year <= DateTime.Now.Year; year++)
            {
                YearComboBox.Items.Add(year);
            }
            YearComboBox.SelectedItem = DateTime.Now.Year;
        }

        private void InitializeMonthComboBox()
        {
            // Месяцы уже добавлены в XAML, так что здесь ничего не нужно делать.
        }

       

        private void SelectButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                int selectedYear = (int)YearComboBox.SelectedItem;

                // Преобразуем Tag из строки в int
                int selectedMonth = int.Parse(((ComboBoxItem)MonthComboBox.SelectedItem).Tag.ToString());

                if (selectedYear == 0 || selectedMonth == 0)
                    throw new InvalidOperationException("Пожалуйста, выберите год и месяц.");

                DateTime startDate = new DateTime(selectedYear, selectedMonth, 1);
                DateTime endDate = startDate.AddMonths(1).AddDays(-1); // Последний день месяца

                SelectedStartDate = startDate;
                SelectedEndDate = endDate;

                // Открываем окно ServiceStatisticsPeriodWindow с переданными датами
                ServiceStatisticsPeriodWindow statisticsWindow = new ServiceStatisticsPeriodWindow(SelectedStartDate, SelectedEndDate);
                statisticsWindow.ShowDialog();
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }





    }
}
