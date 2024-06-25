using System;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Windows;
using Microsoft.Win32;

namespace WpfApp2
{
    public partial class MainWindow : Window
    {
        private string dbConnectionString = "Data Source=normal_distribution_results.db;Version=3;";

        public MainWindow()
        {
            InitializeComponent();
            CreateDatabaseAndTable();
        }

        // Функция плотности вероятности для нормального распределения
        static double NormalPDF(double x, double mean, double stdDev)
        {
            double expPart = Math.Exp(-0.5 * Math.Pow((x - mean) / stdDev, 2));
            return (1 / (stdDev * Math.Sqrt(2 * Math.PI))) * expPart;
        }

        // Численное интегрирование методом прямоугольников для вычисления CDF
        static double CalculateCDF(double x, double mean, double stdDev)
        {
            double step = 0.001; // шаг интегрирования
            double sum = 0.0;

            for (double i = mean - 10 * stdDev; i < x; i += step)
            {
                sum += NormalPDF(i, mean, stdDev) * step;
            }

            return sum;
        }

        private void CalculateCDF_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                double mean = double.Parse(MeanTextBox.Text);
                double stdDev = double.Parse(StdDevTextBox.Text);
                string[] inputs = ValuesTextBox.Text.Split(new[] { ' ', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
                double[] values = inputs.Select(double.Parse).ToArray();

                ResultTextBox.Clear();
                foreach (double x in values)
                {
                    double cdf = CalculateCDF(x, mean, stdDev);
                    string result = $"CDF для x = {x} с mean = {mean} и stdDev = {stdDev}: {cdf}\n";
                    ResultTextBox.AppendText(result);
                    SaveResultToDatabase(mean, stdDev, x, cdf);
                }
            }
            catch (FormatException)
            {
                MessageBox.Show("Ошибка: введены некорректные данные. Пожалуйста, введите числовые значения.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CreateDatabaseAndTable()
        {
            using (SQLiteConnection connection = new SQLiteConnection(dbConnectionString))
            {
                connection.Open();

                string createTableQuery = @"
                    CREATE TABLE IF NOT EXISTS Results (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        Mean REAL,
                        StdDev REAL,
                        X REAL,
                        CDF REAL
                    )";

                using (SQLiteCommand command = new SQLiteCommand(createTableQuery, connection))
                {
                    command.ExecuteNonQuery();
                }
            }
        }

        private void SaveResultToDatabase(double mean, double stdDev, double x, double cdf)
        {
            using (SQLiteConnection connection = new SQLiteConnection(dbConnectionString))
            {
                connection.Open();

                string insertQuery = @"
                    INSERT INTO Results (Mean, StdDev, X, CDF)
                    VALUES (@Mean, @StdDev, @X, @CDF)";

                using (SQLiteCommand command = new SQLiteCommand(insertQuery, connection))
                {
                    command.Parameters.AddWithValue("@Mean", mean);
                    command.Parameters.AddWithValue("@StdDev", stdDev);
                    command.Parameters.AddWithValue("@X", x);
                    command.Parameters.AddWithValue("@CDF", cdf);
                    command.ExecuteNonQuery();
                }
            }
        }

        private void LoadFile_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*";

            if (openFileDialog.ShowDialog() == true)
            {
                try
                {
                    string[] lines = File.ReadAllLines(openFileDialog.FileName);
                    ValuesTextBox.Text = string.Join(" ", lines);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при чтении файла: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
        private void ShowResults_Click(object sender, RoutedEventArgs e)
        {
            ResultsWindow resultsWindow = new ResultsWindow();
            resultsWindow.Show();
        }

 private void ClearResults_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show("Вы уверены, что хотите очистить все результаты?", "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                ClearDatabase();
                MessageBox.Show("Все результаты были успешно очищены.", "Очистка завершена", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void ClearDatabase()
        {
            using (SQLiteConnection connection = new SQLiteConnection(dbConnectionString))
            {
                connection.Open();

                string deleteQuery = "DELETE FROM Results";

                using (SQLiteCommand command = new SQLiteCommand(deleteQuery, connection))
                {
                    command.ExecuteNonQuery();
                }
            }
        }
    }
}