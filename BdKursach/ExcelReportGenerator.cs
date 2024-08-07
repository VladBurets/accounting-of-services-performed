
using System;
using System.Data.SqlClient;
using OfficeOpenXml;
using System.IO;
using OfficeOpenXml.Style;
using System.Drawing;
using DocumentFormat.OpenXml.Spreadsheet;

namespace BdKursach
{
    public class ExcelReportGenerator
    {
        private string connectionString = "Data Source=DESKTOP-G94EAHJ\\SQLEXPRESS;Initial Catalog=диплом4;Integrated Security=True";


        public void GenerateExcelReport(string filePath, int orderId)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            using (ExcelPackage package = new ExcelPackage())
            {
                // Создание нового листа
                ExcelWorksheet worksheet = package.Workbook.Worksheets.Add("Отчет");

                // Получение данных из базы данных
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    string query = @"
                    SELECT 
                        З.Название_заказа,
                        У.Название_услуги,
                        С.ФИО AS Сотрудник,
                        З.Стоимость,
                        З.Дата_принятия_заказа,
                        З.Дата_завершения_заказа,
                        STRING_AGG(CASE WHEN ТЗ.Выполнено = 1 THEN ТЗ.Пункты END, ', ') AS Выполненные_пункты,
                        STRING_AGG(CASE WHEN ТЗ.Выполнено = 0 THEN ТЗ.Пункты END, ', ') AS Невыполненные_пункты
                    FROM 
                        Заказ З
                    JOIN 
                        Сотрудники С ON З.ID_Сотрудника = С.ID_Сотрудника
                    JOIN 
                        Услуги У ON З.ID_Услуги = У.ID_Услуги
                    LEFT JOIN 
                        Техническое_задание ТЗ ON З.ID_Заказа = ТЗ.ID_Заказа
                    WHERE
                        З.ID_Заказа = @OrderId
                    GROUP BY 
                        З.ID_Заказа, З.Название_заказа, У.Название_услуги, С.ФИО, З.Стоимость, З.Дата_принятия_заказа, З.Дата_завершения_заказа";

                    SqlCommand command = new SqlCommand(query, connection);
                    command.Parameters.AddWithValue("@OrderId", orderId);
                    SqlDataReader reader = command.ExecuteReader();

                    if (reader.Read())
                    {
                        // Название заказа
                        worksheet.Cells[1, 1].Value = "Название заказа:";
                        worksheet.Cells[1, 2].Value = reader["Название_заказа"].ToString();
                        worksheet.Cells[1, 1, 1, 2].Style.Font.Bold = true;

                        // Сотрудник
                        worksheet.Cells[3, 1].Value = "Сотрудник:";
                        worksheet.Cells[3, 2].Value = reader["Сотрудник"].ToString();
                        worksheet.Cells[3, 1, 3, 2].Style.Font.Bold = true;

                        // Услуга
                        worksheet.Cells[4, 1].Value = "Услуга:";
                        worksheet.Cells[4, 2].Value = reader["Название_услуги"].ToString();
                        worksheet.Cells[4, 1, 4, 2].Style.Font.Bold = true;

                        // Дата начала заказа
                        worksheet.Cells[6, 1].Value = "Дата начала заказа:";
                        worksheet.Cells[6, 2].Value = Convert.ToDateTime(reader["Дата_принятия_заказа"]).ToString("dd.MM.yyyy");
                        worksheet.Cells[6, 1, 6, 2].Style.Font.Bold = true;

                        // Дата завершения заказа
                        worksheet.Cells[7, 1].Value = "Дата завершения заказа:";
                        worksheet.Cells[7, 2].Value = Convert.ToDateTime(reader["Дата_завершения_заказа"]).ToString("dd.MM.yyyy");
                        worksheet.Cells[7, 1, 7, 2].Style.Font.Bold = true;

                        // Таблица с выполненными и невыполненными пунктами
                        worksheet.Cells[9, 1].Value = "Выполненные пункты";
                        worksheet.Cells[9, 2].Value = "Невыполненные пункты";
                        worksheet.Cells[9, 1, 9, 2].Style.Font.Bold = true;
                        worksheet.Cells[9, 1, 9, 2].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                        worksheet.Cells[9, 1, 9, 2].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                        string completedItem = reader["Выполненные_пункты"] != DBNull.Value ? reader["Выполненные_пункты"].ToString() : string.Empty;
                        string unfulfilledItems = reader["Невыполненные_пункты"] != DBNull.Value ? reader["Невыполненные_пункты"].ToString() : string.Empty;

                        worksheet.Cells[10, 1].Value = completedItem;
                        worksheet.Cells[10, 2].Value = unfulfilledItems;

                        worksheet.Cells[10, 1, 10, 2].Style.Border.BorderAround(ExcelBorderStyle.Thin);
                        worksheet.Cells[10, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                        worksheet.Cells[10, 2].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                        worksheet.Cells[10, 1].Style.VerticalAlignment = ExcelVerticalAlignment.Top;
                        worksheet.Cells[10, 2].Style.VerticalAlignment = ExcelVerticalAlignment.Top;

                        // Wrap text in cells
                        worksheet.Cells[10, 1].Style.WrapText = true;
                        worksheet.Cells[10, 2].Style.WrapText = true;
                    }

                    reader.Close();
                }

                // Установка ширины столбцов
                worksheet.Column(1).Width = 30;
                worksheet.Column(2).Width = 50;

                // Сохранение Excel-документа на рабочем столе
                string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
                string desktopFilePath = Path.Combine(desktopPath, filePath);
                FileInfo excelFile = new FileInfo(desktopFilePath);
                package.SaveAs(excelFile);
            }
        }
    }
}
