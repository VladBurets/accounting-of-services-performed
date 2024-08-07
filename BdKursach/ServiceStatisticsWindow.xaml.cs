using System;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Windows;
using LiveCharts;
using LiveCharts.Wpf;
using ClosedXML.Excel;
using System.IO;
using System.Windows.Media.Imaging;
using System.Windows.Controls;
using System.Windows.Media;

namespace BdKursach
{
    public partial class ServiceStatisticsWindow : Window
    {
        private DataTable serviceStatisticsDataTable;
        private string connectionString = "Data Source=DESKTOP-G94EAHJ\\SQLEXPRESS;Initial Catalog=диплом4;Integrated Security=True";

        public ServiceStatisticsWindow()
        {
            InitializeComponent();
            LoadServiceStatistics();
            DisplayChart();
        }

        private void LoadServiceStatistics()
        {
            serviceStatisticsDataTable = new DataTable();
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    string query = @"
        SELECT 
            Услуги.Название_услуги,
            COUNT(Техническое_задание.ID_тз) AS Количество_заказов,
            STRING_AGG(Заказчики.Название, ', ') AS Заказчики,
            STRING_AGG(Сотрудники.ФИО, ', ') AS Сотрудники,
            STRING_AGG(CONVERT(varchar, Техническое_задание.Дата_выполнения, 23), ', ') AS Даты_выполнения,
            STRING_AGG(CONVERT(varchar, Заказ.ID_Заказа), ', ') AS Заказы
        FROM 
            Услуги
        JOIN 
            Техническое_задание ON Услуги.ID_Услуги = Техническое_задание.ID_Услуги
        JOIN
            Заказ ON Техническое_задание.ID_Заказа = Заказ.ID_Заказа
        JOIN
            Заказчики ON Заказ.ID_заказчика = Заказчики.ID_заказчика
        JOIN
            Сотрудники ON Заказ.ID_Сотрудника = Сотрудники.ID_Сотрудника
        WHERE 
            Техническое_задание.Выполнено = 1
        GROUP BY 
            Услуги.Название_услуги";

                    SqlDataAdapter adapter = new SqlDataAdapter(query, connection);
                    adapter.Fill(serviceStatisticsDataTable);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при загрузке статистики услуг: " + ex.Message);
            }
        }



        private void DisplayChart()
        {
            ServiceStatisticsChart.Series.Clear();  // Очистка предыдущих данных графика

            if (serviceStatisticsDataTable.Rows.Count > 0)
            {
                var lineSeries = new LineSeries
                {
                    Title = "Количество выполнений",
                    Values = new ChartValues<int>()
                };

                foreach (DataRow row in serviceStatisticsDataTable.Rows)
                {
                    lineSeries.Values.Add(Convert.ToInt32(row["Количество_заказов"]));
                }

                ServiceStatisticsChart.Series.Add(lineSeries);

                int maxValue = lineSeries.Values.Cast<int>().Max();


                ServiceStatisticsChart.AxisX.Clear();
                ServiceStatisticsChart.AxisX.Add(new Axis
                {
                    Title = "Услуги",
                    Labels = serviceStatisticsDataTable.AsEnumerable()
                        .Select(row => row["Название_услуги"].ToString()).ToList()
                });

                ServiceStatisticsChart.AxisY.Clear();
                ServiceStatisticsChart.AxisY.Add(new Axis
                {
                    Title = "Количество выполнений",
                     MinValue = 1,
                     MaxValue = maxValue + 2,
                });

                // Добавим подсказку с именами заказчиков и статусами выполнения
                ServiceStatisticsChart.DataTooltip = new DefaultTooltip
                {
                    SelectionMode = TooltipSelectionMode.SharedXValues,
                    Content = new System.Windows.Controls.TextBlock()
                };

                ServiceStatisticsChart.DataClick += (sender, chartPoint) =>
                {
                    var row = serviceStatisticsDataTable.Rows[(int)chartPoint.X];
                    string заказчик = row["Заказчики"].ToString();
                    string услуга = row["Название_услуги"].ToString();

                    var tooltip = (DefaultTooltip)ServiceStatisticsChart.DataTooltip;
                    tooltip.Content = new System.Windows.Controls.TextBlock
                    {
                        Text = $"{услуга}\nЗаказчики: {заказчик}"
                    };
                };
            }
            else
            {
                MessageBox.Show("Нет данных для отображения на графике.");
            }
        }


        private void ExportToExcel_Click(object sender, RoutedEventArgs e)
        {
            if (serviceStatisticsDataTable == null || serviceStatisticsDataTable.Rows.Count == 0)
            {
                Console.WriteLine("Нет данных для экспорта.");
                return;
            }

            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Статистика услуг");

                // Заголовок "Статистика выполненных услуг ООО 'ПВЗ'"
                worksheet.Cell(1, 1).Value = "Статистика выполненных услуг ООО 'ПВЗ'";
                worksheet.Cell(1, 1).Style.Font.Bold = true;
                worksheet.Cell(1, 1).Style.Font.FontSize = 16;
                worksheet.Range("A1:D1").Merge(); // Объединяем ячейки для заголовка

                // Дата создания отчета
                worksheet.Cell(2, 1).Value = "Дата создания отчета:";
                worksheet.Cell(2, 2).Value = DateTime.Now.Date.ToString("yyyy-MM-dd");

                int currentRow = 4; // Первая строка после заголовков

                foreach (DataRow serviceRow in serviceStatisticsDataTable.Rows)
                {
                    string serviceName = serviceRow["Название_услуги"].ToString();
                    int serviceCount = Convert.ToInt32(serviceRow["Количество_заказов"]);
                    string customers = serviceRow["Заказчики"].ToString();
                    string employees = serviceRow["Сотрудники"].ToString();
                    string completionDates = serviceRow["Даты_выполнения"].ToString();
                    string orders = serviceRow["Заказы"].ToString();

                    // Заголовок для каждой услуги
                    worksheet.Cell(currentRow, 1).Value = serviceName;
                    worksheet.Cell(currentRow, 1).Style.Font.Bold = true;
                    worksheet.Cell(currentRow, 1).Style.Font.FontSize = 14;
                    worksheet.Range(currentRow, 1, currentRow, 4).Merge();
                    currentRow++;

                    // Подзаголовки для таблицы
                    worksheet.Cell(currentRow, 1).Value = "Заказчики";
                    worksheet.Cell(currentRow, 2).Value = "Сотрудники";
                    worksheet.Cell(currentRow, 3).Value = "Даты выполнения";
                    worksheet.Cell(currentRow, 4).Value = "Номера заказов";

                    var headerRange = worksheet.Range(currentRow, 1, currentRow, 4);
                    var headerStyle = workbook.Style;
                    headerStyle.Fill.BackgroundColor = XLColor.Gray;
                    headerStyle.Font.FontColor = XLColor.White;
                    headerStyle.Font.Bold = true;
                    headerStyle.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    headerRange.Style = headerStyle;
                    currentRow++;

                    // Данные услуги
                    var customersList = customers.Split(new[] { ", " }, StringSplitOptions.None);
                    var employeesList = employees.Split(new[] { ", " }, StringSplitOptions.None);
                    var datesList = completionDates.Split(new[] { ", " }, StringSplitOptions.None);
                    var ordersList = orders.Split(new[] { ", " }, StringSplitOptions.None);

                    for (int i = 0; i < customersList.Length; i++)
                    {
                        worksheet.Cell(currentRow, 1).Value = customersList[i];
                        worksheet.Cell(currentRow, 2).Value = employeesList[i];
                        worksheet.Cell(currentRow, 3).Value = datesList[i];
                        worksheet.Cell(currentRow, 4).Value = ordersList[i];
                        currentRow++;
                    }

                    // Добавление строки с количеством выполнений
                    worksheet.Cell(currentRow, 1).Value = $"Количество выполнений: {serviceCount}";
                    worksheet.Cell(currentRow, 1).Style.Font.Bold = true;
                    worksheet.Range(currentRow, 1, currentRow, 4).Merge();
                    currentRow += 2; // Оставляем строку для разделения между услугами
                }

                worksheet.Columns().AdjustToContents();

                // Сохранение файла на рабочем столе
                string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                string fileName = $"Статистика_услуг_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
                string filePath = Path.Combine(desktopPath, fileName);

                workbook.SaveAs(filePath);

                MessageBox.Show($"Отчет успешно сохранен на рабочем столе: {filePath}");
            }
        }





    }
}
