using System.Collections.Generic;
using System.Data.SQLite;
using System.Windows;

namespace WpfApp2
{
    public partial class ResultsWindow : Window
    {
        private string dbConnectionString = "Data Source=normal_distribution_results.db;Version=3;";

        public ResultsWindow()
        {
            InitializeComponent();
            LoadResults();
        }

        private void LoadResults()
        {
            List<Result> results = new List<Result>();

            using (SQLiteConnection connection = new SQLiteConnection(dbConnectionString))
            {
                connection.Open();

                string selectQuery = "SELECT * FROM Results";

                using (SQLiteCommand command = new SQLiteCommand(selectQuery, connection))
                {
                    using (SQLiteDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            results.Add(new Result
                            {
                                Id = reader.GetInt32(0),
                                Mean = reader.GetDouble(1),
                                StdDev = reader.GetDouble(2),
                                X = reader.GetDouble(3),
                                CDF = reader.GetDouble(4)
                            });
                        }
                    }
                }
            }

            ResultsDataGrid.ItemsSource = results;
        }
    }

    public class Result
    {
        public int Id { get; set; }
        public double Mean { get; set; }
        public double StdDev { get; set; }
        public double X { get; set; }
        public double CDF { get; set; }
    }
}
