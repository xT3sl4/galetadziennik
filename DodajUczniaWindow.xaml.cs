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

namespace dziennik
{
    /// <summary>
    /// Logika interakcji dla klasy DodajUczniaWindow.xaml
    /// </summary>
    public partial class DodajUczniaWindow : Window
    {
        private string connectionString;

        public DodajUczniaWindow()
        {
            InitializeComponent();
            connectionString = "Data Source=10.1.49.186;Initial Catalog=Szkola;User ID=admin2;Password=zaq1@WSX;Connect Timeout=30;Encrypt=True;TrustServerCertificate=True;";
        }

        private void DodajButton_Click(object sender, RoutedEventArgs e)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string query = "INSERT INTO uczniowie (imie, nazwisko, klasa, haslo, punkty) VALUES (@imie, @nazwisko, @klasa, @haslo, 250)";
                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@imie", ImieTextBox.Text);
                command.Parameters.AddWithValue("@nazwisko", NazwiskoTextBox.Text);
                command.Parameters.AddWithValue("@klasa", KlasaTextBox.Text);
                command.Parameters.AddWithValue("@haslo", HasloTextBox.Text);
                command.ExecuteNonQuery();
            }

            MessageBox.Show("Uczeń został dodany.");
            this.Close();
        }
    }
}
