using System;
using System.Data.SqlClient;
using System.Windows;

namespace dziennik
{
    public partial class DodajOceneWindow : Window
    {
        private string connectionString;
        private string fullName;

        public DodajOceneWindow(string connectionString, string fullName)
        {
            InitializeComponent();
            this.connectionString = connectionString;
            this.fullName = fullName;
        }

        private void DodajOcene_Click(object sender, RoutedEventArgs e)
        {
            int ocena;
            string przedmiot = PrzedmiotTextBox.Text;

            if (int.TryParse(OcenaTextBox.Text, out ocena) && ocena >= 1 && ocena <= 6)
            {
                DodajOceneDoBazy(fullName, przedmiot, ocena);
                MessageBox.Show("Ocena została dodana.");
                this.Close();
            }
            else
            {
                MessageBox.Show("Proszę podać poprawne dane.");
            }
        }

        private void DodajOceneDoBazy(string fullName, string przedmiot, int ocena)
        {
            string[] nameParts = fullName.Split(' ');
            if (nameParts.Length < 2) return;

            string imie = nameParts[0];
            string nazwisko = nameParts[1];

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string query = @"
                        INSERT INTO oceny (Id_ucznia, Id_przedmiotu, ocena)
                        VALUES (
                            (SELECT PESEL FROM uczniowie WHERE imie = @imie AND nazwisko = @nazwisko), 
                            (SELECT TOP (1) Id FROM przedmioty WHERE nazwa = @przedmiot), 
                            @ocena)";

                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@imie", imie);
                command.Parameters.AddWithValue("@nazwisko", nazwisko);
                command.Parameters.AddWithValue("@przedmiot", przedmiot);
                command.Parameters.AddWithValue("@ocena", ocena);

                command.ExecuteNonQuery();
            }
        }
    }
}
