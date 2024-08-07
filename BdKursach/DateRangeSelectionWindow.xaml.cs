using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;
using System.Windows;
using ClosedXML.Excel;
using System.IO;

namespace BdKursach
{
    /// <summary>
    /// Логика взаимодействия для DateRangeSelectionWindow.xaml
    /// </summary>
    public partial class DateRangeSelectionWindow : Window
    {
        private string connectionString = "Data Source=DESKTOP-G94EAHJ\\SQLEXPRESS;Initial Catalog=диплом4;Integrated Security=True";
        public DateTime SelectedStartDate { get; private set; }
        public DateTime SelectedEndDate { get; private set; }
        public int CustomerId { get; private set; }
        public DateRangeSelectionWindow(int customerId)
        {
            InitializeComponent();
            CustomerId = customerId;
        }

        private void otchet_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                SelectedStartDate = StartDatePicker.SelectedDate ?? DateTime.MinValue;
                SelectedEndDate = EndDatePicker.SelectedDate ?? DateTime.MaxValue;

                // Проверка, что дата начала периода меньше даты конца периода
                if (SelectedStartDate >= SelectedEndDate)
                {
                    MessageBox.Show("Дата начала периода должна быть меньше даты конца периода.");
                    return;
                }

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    string query = @"
            SELECT 
                Заказ.ID_Заказа,
                Заказ.Название_заказа,
                Заказчики.Название AS Название_заказчика,
                Сотрудники.ФИО AS Сотрудник,
                Заказ.Процент_выполнения,
                Заказ.Стоимость,
                Заказ.Дата_принятия_заказа,
                Заказ.Дата_завершения_заказа 
            FROM 
                Заказ
            INNER JOIN 
                Заказчики ON Заказ.ID_заказчика = Заказчики.ID_заказчика
            INNER JOIN 
                Сотрудники ON Заказ.ID_Сотрудника = Сотрудники.ID_Сотрудника
            WHERE 
                Заказ.ID_заказчика = @CustomerId
                AND Заказ.Дата_принятия_заказа BETWEEN @StartDate AND @EndDate";

                    SqlCommand command = new SqlCommand(query, connection);
                    command.Parameters.AddWithValue("@CustomerId", this.CustomerId);
                    command.Parameters.AddWithValue("@StartDate", SelectedStartDate);
                    command.Parameters.AddWithValue("@EndDate", SelectedEndDate);

                    SqlDataAdapter adapter = new SqlDataAdapter(command);
                    DataTable dataTable = new DataTable();
                    adapter.Fill(dataTable);

                    if (dataTable.Rows.Count == 0)
                    {
                        MessageBox.Show("Нет данных для создания отчета в выбранном периоде.");
                        return;
                    }

                    // Название вашей организации
                    string organizationName = "ООО 'ПВЗ'";

                    using (XLWorkbook workbook = new XLWorkbook())
                    {
                        var worksheet = workbook.Worksheets.Add("История заказов");

                        // Установка заголовка отчета
                        string title = $"Отчет о заказах клиентов {organizationName}";
                        worksheet.Cell(1, 1).Value = title;
                        worksheet.Row(1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                        worksheet.Row(1).Style.Font.Bold = true;

                        // Установка заголовка периода с учетом названия заказчика
                        string periodTitle = $"История заказов организации {dataTable.Rows[0]["Название_заказчика"]} за период с {SelectedStartDate:yyyy-MM-dd} по {SelectedEndDate:yyyy-MM-dd}";
                        worksheet.Cell(2, 1).Value = periodTitle;
                        worksheet.Row(2).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                        worksheet.Row(2).Style.Font.Bold = true;

                        // Добавление разделителя для выполненных и не выполненных заказов
                        int startRowCompleted = 4;
                        int startRowNotCompleted = startRowCompleted + dataTable.Rows.Count + 2; // Отступ между разделами

                        worksheet.Cell(startRowCompleted - 1, 1).Value = "Выполненные заказы";
                        worksheet.Cell(startRowNotCompleted - 1, 1).Value = "Не выполненные заказы";

                        // Названия столбцов (с добавлением стрелочек)
                        worksheet.Cell(startRowCompleted, 1).Value = "⬇ Номер заказа";
                        worksheet.Cell(startRowCompleted, 2).Value = "⬇ Сотрудник";
                        worksheet.Cell(startRowCompleted, 3).Value = "⬇ Процент выполнения";
                        worksheet.Cell(startRowCompleted, 4).Value = "⬇ Стоимость";
                        worksheet.Cell(startRowCompleted, 5).Value = "⬇ Дата принятия";
                        worksheet.Cell(startRowCompleted, 6).Value = "⬇ Планированная дата завершения";

                        worksheet.Cell(startRowNotCompleted, 1).Value = "⬇ Номер заказа";
                        worksheet.Cell(startRowNotCompleted, 2).Value = "⬇ Сотрудник";
                        worksheet.Cell(startRowNotCompleted, 3).Value = "⬇ Процент выполнения";
                        worksheet.Cell(startRowNotCompleted, 4).Value = "⬇ Стоимость";
                        worksheet.Cell(startRowNotCompleted, 5).Value = "⬇ Дата принятия";
                        worksheet.Cell(startRowNotCompleted, 6).Value = "⬇ Планированная дата завершения";

                        // Установка стиля для заголовков столбцов
                        var headerRange1 = worksheet.Range(startRowCompleted, 1, startRowCompleted, 6);
                        var headerRange2 = worksheet.Range(startRowNotCompleted, 1, startRowNotCompleted, 6);
                        var headerStyle = workbook.Style;
                        headerStyle.Fill.BackgroundColor = XLColor.Gray;
                        headerStyle.Font.FontColor = XLColor.White;
                        headerStyle.Font.Bold = true;
                        headerStyle.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                        headerRange1.Style = headerStyle;
                        headerRange2.Style = headerStyle;

                        // Заполнение данных
                        for (int i = 0; i < dataTable.Rows.Count; i++)
                        {
                            int row = dataTable.Rows[i]["Процент_выполнения"].ToString() == "100" ? ++startRowCompleted : ++startRowNotCompleted;
                            worksheet.Cell(row, 1).Value = dataTable.Rows[i]["ID_Заказа"].ToString();
                            worksheet.Cell(row, 2).Value = dataTable.Rows[i]["Сотрудник"].ToString();
                            worksheet.Cell(row, 3).Value = dataTable.Rows[i]["Процент_выполнения"].ToString();
                            worksheet.Cell(row, 4).Value = Convert.ToDecimal(dataTable.Rows[i]["Стоимость"]);
                            worksheet.Cell(row, 5).Value = Convert.ToDateTime(dataTable.Rows[i]["Дата_принятия_заказа"]).ToString("yyyy-MM-dd");
                            worksheet.Cell(row, 6).Value = Convert.ToDateTime(dataTable.Rows[i]["Дата_завершения_заказа"]).ToString("yyyy-MM-dd");
                        }

                        // Автоподстройка ширины столбцов
                        worksheet.Columns().AdjustToContents();

                        // Установка ширины столбцов вручную для лучшей видимости данных
                        worksheet.Column(1).Width = 15; // Номер заказа
                        worksheet.Column(2).Width = 30; // Сотрудник
                        worksheet.Column(3).Width = 20; // Процент выполнения
                        worksheet.Column(4).Width = 15; // Стоимость
                        worksheet.Column(5).Width = 15; // Дата принятия
                        worksheet.Column(6).Width = 15; // Планированная дата завершения

                        // Сохранение файла на рабочем столе
                        string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                        string fileName = $"История_заказов_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
                        string filePath = Path.Combine(desktopPath, fileName);

                        workbook.SaveAs(filePath);

                        MessageBox.Show($"Отчет успешно сохранен на рабочем столе: {filePath}");
                    }
                }

                // Закрытие окна после выбора дат и передача управления обратно
                this.DialogResult = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при формировании отчета: " + ex.Message);
            }
        }

        
    }
}










