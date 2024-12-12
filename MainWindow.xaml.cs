using System.Windows;
using System.Data.SqlClient;

namespace dziennik
{
    public partial class MainWindow : Window
    {
        private string connectionString;

        public MainWindow()
        {
            InitializeComponent();
            connectionString = "Data Source=10.1.49.186;Initial Catalog=Szkola;User ID=admin2;Password=zaq1@WSX;Connect Timeout=30;Encrypt=True;TrustServerCertificate=True;";
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            string pesel = PeselTextBox.Text;
            string haslo = PasswordBox.Password;

            if (SprawdzLogowanie(pesel, haslo) == "uczen")
            {

                Uczenlogin uczenlogin = new Uczenlogin(pesel);
                uczenlogin.Show();

                this.Close();
            }
            else if (SprawdzLogowanie(pesel, haslo) == "nauczyciel")
            {
                LoginWindow loginWindow = new LoginWindow(pesel);
                loginWindow.Show();
                this.Close();
            }
            else
            {
                MessageBox.Show("Nieprawidłowy PESEL lub hasło.");
            }
        }

        private string SprawdzLogowanie(string pesel, string haslo)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string query = "SELECT COUNT(*) FROM Nauczyciele WHERE Pesel = @Pesel AND Haslo = @Haslo";
                string query2 = "SELECT COUNT(*) FROM uczniowie WHERE Pesel = @Pesel AND Haslo = @Haslo";
                string query3 = "SELECT wychowawca FROM klasy WHERE Pesel = @Pesel AND Haslo = @Haslo";
                SqlCommand command = new SqlCommand(query, connection);
                SqlCommand command2 = new SqlCommand(query2, connection);
                command.Parameters.AddWithValue("@Pesel", pesel);
                command.Parameters.AddWithValue("@Haslo", haslo);
                command2.Parameters.AddWithValue("@Pesel", pesel);
                command2.Parameters.AddWithValue("@Haslo", haslo);

                int count = (int)command.ExecuteScalar();
                int count2 = (int)command2.ExecuteScalar();
                if (count2 > 0)
                {
                    return "uczen";
                }
                else if (count > 0)
                {
                    return "nauczyciel";
                }
                else
                {
                    return "0";
                }
            }
        }
    }
}