using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Windows;
using System.Windows.Controls;

namespace dziennik
{
    /// <summary>
    /// Logika interakcji dla klasy Uczenlogin.xaml
    /// </summary>
    public partial class Uczenlogin : Window
    {
        private string connectionString;
        private string pesel;

        public Uczenlogin(string pesel)
        {
            InitializeComponent();
            this.pesel = pesel;
            connectionString = "Data Source=10.1.49.186;Initial Catalog=Szkola;User ID=admin2;Password=zaq1@WSX;Connect Timeout=30;Encrypt=True;TrustServerCertificate=True;";
            Ucenyuczen();
            WyswietlSzczegoly();
        }

        private void Ucenyuczen()
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string query = @"
                        SELECT oceny.ocena, przedmioty.nazwa 
                        FROM oceny 
                        JOIN przedmioty ON oceny.Id_przedmiotu = przedmioty.Id
                        WHERE oceny.Id_ucznia = @pesel";

                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@pesel", pesel);
                SqlDataReader reader = command.ExecuteReader();

                Dictionary<string, TreeViewItem> drzewkoocen = new Dictionary<string, TreeViewItem>();
                while (reader.Read())
                {
                    string ocena = reader["ocena"].ToString();
                    string przedmiot = reader["nazwa"].ToString();

                    if (!drzewkoocen.ContainsKey(przedmiot))
                    {
                        TreeViewItem subjectNode = new TreeViewItem() { Header = przedmiot };
                        drzewkoocen[przedmiot] = subjectNode;
                        DrzewkoOcen.Items.Add(subjectNode);
                    }

                    TreeViewItem ocenaNode = new TreeViewItem() { Header = ocena };
                    drzewkoocen[przedmiot].Items.Add(ocenaNode);
                }
                reader.Close();
            }
        }

        private void WyswietlSzczegoly()
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string query = @"
                        SELECT imie, nazwisko, klasa, punkty
                        FROM uczniowie 
                        WHERE PESEL = @pesel";

                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@pesel", pesel);
                SqlDataReader reader = command.ExecuteReader();

                if (reader.Read())
                {
                    string imie = reader["imie"].ToString();
                    string nazwisko = reader["nazwisko"].ToString();
                    string klasa = reader["klasa"].ToString();
                    string punkty = reader["punkty"].ToString();

                    ImieTextBlock.Text = imie;
                    NazwiskoTextBlock.Text = nazwisko;
                    PeselTextBlock.Text = pesel;
                    KlasaTextBlock.Text = klasa;
                    PunktyTextBlock.Text = punkty;
                }
                reader.Close();
            }
        }
    }
}