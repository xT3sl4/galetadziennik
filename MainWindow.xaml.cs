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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Data.SqlClient;
using System.Runtime.Remoting.Messaging;


namespace dziennik
{
    /// <summary>
    /// Logika interakcji dla klasy MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        string connectionString;
        public MainWindow()
        {
            InitializeComponent();

            connectionString = "Data Source=10.1.49.186;Initial Catalog=Szkola;User ID=admin2;Password=zaq1@WSX;Connect Timeout=30;Encrypt=True;TrustServerCertificate=True;";

            WyswietlUczniow();

        }
        private void WyswietlUczniow()
        {

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string query = "SELECT imie, nazwisko, klasa FROM uczniowie";

                SqlCommand command = new SqlCommand(query, connection);
                SqlDataReader reader = command.ExecuteReader();

                Dictionary<string, TreeViewItem> drzewko = new Dictionary<string, TreeViewItem>();

                while (reader.Read())
                {
                    string klasa = reader["klasa"].ToString();
                    string fullName = $"{reader["imie"]} {reader["nazwisko"]}";

                    if (!drzewko.ContainsKey(klasa))
                    {

                        TreeViewItem classNode = new TreeViewItem() { Header = klasa };
                        drzewko[klasa] = classNode;
                        DrzewkoUczniowie.Items.Add(classNode);
                    }


                    drzewko[klasa].Items.Add(new TreeViewItem() { Header = fullName });
                }
                reader.Close();
            }

        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            string pesel = Pesel.Text;
            string haslo = Haslo.Text;
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string query = $"SELECT COUNT(*) FROM uczniowie WHERE pesel = {pesel} AND haslo = {haslo}";
                string query2 = $"SELECT COUNT(*) FROM Nauczyciele WHERE pesel = {pesel} AND haslo = {haslo} ";

                SqlCommand command = new SqlCommand(query, connection);

                int countOfUczen = (int)command.ExecuteScalar();
                
                if(countOfUczen > 0)
                {
                    MessageBox.Show("Zalogowano Uczeń");

                }


                SqlCommand command2 = new SqlCommand(query2, connection);

                int countOfNauczyciel = (int)command2.ExecuteScalar();
               

                if (countOfNauczyciel > 0)
                {
                    MessageBox.Show("Zalogowano Nauczyciel");

                }
                



            }
        }
    }
  
}
