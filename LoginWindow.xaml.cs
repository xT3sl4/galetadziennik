using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Data.SqlClient;
using System.Xml.Serialization;

namespace dziennik
{
    public partial class LoginWindow : Window
    {
        private string connectionString;

        public LoginWindow(string PESEL)
        {
            string pesel = PESEL;
            InitializeComponent();
            connectionString = "Data Source=10.1.49.186;Initial Catalog=Szkola;User ID=admin2;Password=zaq1@WSX;Connect Timeout=30;Encrypt=True;TrustServerCertificate=True;";
            bool isDirector = false;

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string query = "SELECT COUNT(*) FROM nauczyciele WHERE pesel = @pesel AND czy_dyrektor = 1";
                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@pesel", pesel);
                int count = (int)command.ExecuteScalar();
                if (count > 0)
                {
                    isDirector = true;
                    DodajUczniaButton.Visibility = Visibility.Visible;
                }
                else
                {
                    DodajUczniaButton.Visibility = Visibility.Collapsed;
                }
            }

            WyswietlUczniow(pesel, isDirector);
        }

        private void WyswietlUczniow(string pesel, bool isDirector)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string query1;
                SqlCommand command1;

                if (isDirector)
                {
                    query1 = "SELECT id FROM dbo.klasy";
                    command1 = new SqlCommand(query1, connection);
                }
                else
                {
                    query1 = "SELECT id FROM dbo.klasy WHERE wychowawca = @pesel";
                    command1 = new SqlCommand(query1, connection);
                    command1.Parameters.AddWithValue("@pesel", pesel);
                }

                SqlDataReader reader1 = command1.ExecuteReader();
                List<string> klasy = new List<string>();

                while (reader1.Read())
                {
                    klasy.Add(reader1["id"].ToString());
                }
                reader1.Close();

                Dictionary<string, TreeViewItem> drzewko = new Dictionary<string, TreeViewItem>();

                foreach (string klasa in klasy)
                {
                    string query = "SELECT imie, nazwisko FROM uczniowie WHERE klasa=@klasa";
                    SqlCommand command = new SqlCommand(query, connection);
                    command.Parameters.AddWithValue("@klasa", klasa);
                    SqlDataReader reader = command.ExecuteReader();

                    while (reader.Read())
                    {
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
        }

        private void DrzewkoUczniowie_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            TreeViewItem selectedItem = DrzewkoUczniowie.SelectedItem as TreeViewItem;
            if (selectedItem != null && selectedItem.Parent is TreeViewItem)
            {
                string fullName = selectedItem.Header.ToString();
                WyswietlSzczegolyUcznia(fullName);
            }
        }

        private void WyswietlSzczegolyUcznia(string fullName)
        {
            string[] nameParts = fullName.Split(' ');
            if (nameParts.Length < 2) return;

            string imie = nameParts[0];
            string nazwisko = nameParts[1];

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string query = @"
                            SELECT uczniowie.PESEL, uczniowie.punkty, oceny.ocena, przedmioty.nazwa 
                            FROM uczniowie
                            JOIN oceny ON uczniowie.PESEL = oceny.Id_ucznia
                            JOIN przedmioty ON oceny.Id_przedmiotu = przedmioty.Id
                            WHERE uczniowie.imie = @imie AND uczniowie.nazwisko = @nazwisko";

                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@imie", imie);
                command.Parameters.AddWithValue("@nazwisko", nazwisko);

                SqlDataReader reader = command.ExecuteReader();

                SzczegolyUczniaPanel.Children.Clear();

                if (reader.Read())
                {
                    string pesel = reader["PESEL"].ToString();
                    int punkty = Convert.ToInt32(reader["punkty"]);

                    SzczegolyUczniaPanel.Children.Add(new TextBlock
                    {
                        Text = $"Imię: {imie}",
                        FontWeight = FontWeights.Bold,
                        FontSize = 16
                    });
                    SzczegolyUczniaPanel.Children.Add(new TextBlock
                    {
                        Text = $"Nazwisko: {nazwisko}",
                        FontWeight = FontWeights.Bold,
                        FontSize = 16
                    });
                    SzczegolyUczniaPanel.Children.Add(new TextBlock { Text = $"PESEL: {pesel}" });
                    SzczegolyUczniaPanel.Children.Add(new TextBlock { Text = $"Punkty: {punkty}" });

                    Button dodaj1PunktButton = new Button { Content = "Dodaj 1 Punkt", Width = 360, Margin = new Thickness(10, 5, 10, 5) };
                    dodaj1PunktButton.Click += (s, e) => ZmienPunkty(pesel, 1, imie, nazwisko);
                    SzczegolyUczniaPanel.Children.Add(dodaj1PunktButton);

                    Button odejmij1PunktButton = new Button { Content = "Odejmij 1 Punkt", Width = 360, Margin = new Thickness(10, 5, 10, 5) };
                    odejmij1PunktButton.Click += (s, e) => ZmienPunkty(pesel, -1, imie, nazwisko);
                    SzczegolyUczniaPanel.Children.Add(odejmij1PunktButton);

                    Button dodaj10PunktowButton = new Button { Content = "Dodaj 10 Punktów", Width = 360, Margin = new Thickness(10, 5, 10, 5) };
                    dodaj10PunktowButton.Click += (s, e) => ZmienPunkty(pesel, 10, imie, nazwisko);
                    SzczegolyUczniaPanel.Children.Add(dodaj10PunktowButton);

                    Button odejmij10PunktowButton = new Button { Content = "Odejmij 10 Punktów", Width = 360, Margin = new Thickness(10, 5, 10, 5) };
                    odejmij10PunktowButton.Click += (s, e) => ZmienPunkty(pesel, -10, imie, nazwisko);
                    SzczegolyUczniaPanel.Children.Add(odejmij10PunktowButton);
                }

                reader.Close();
                reader = command.ExecuteReader();

                TreeView treeView = new TreeView();
                SzczegolyUczniaPanel.Children.Add(treeView);

                Dictionary<string, TreeViewItem> drzewkoOcen = new Dictionary<string, TreeViewItem>();

                while (reader.Read())
                {
                    string ocena = reader["ocena"].ToString();
                    string przedmiot = reader["nazwa"].ToString();

                    if (!drzewkoOcen.ContainsKey(przedmiot))
                    {
                        TreeViewItem subjectNode = new TreeViewItem() { Header = przedmiot };
                        drzewkoOcen[przedmiot] = subjectNode;
                        treeView.Items.Add(subjectNode);
                    }

                    TreeViewItem gradeNode = new TreeViewItem() { Header = ocena };
                    drzewkoOcen[przedmiot].Items.Add(gradeNode);
                }
                reader.Close();

                Button dodajOceneButton = new Button
                {
                    Content = "Dodaj Ocene",
                    Width = 360,
                    Margin = new Thickness(10, 10, 10, 10)
                };
                dodajOceneButton.Click += (s, e) =>
                {
                    DodajOceneWindow dodajOceneWindow = new DodajOceneWindow(connectionString, fullName);
                    dodajOceneWindow.Show();
                };
                SzczegolyUczniaPanel.Children.Add(dodajOceneButton);
            }
        }

        private void ZmienPunkty(string pesel, int zmiana, string imie, string nazwisko)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string query = "UPDATE uczniowie SET punkty = punkty + @zmiana WHERE PESEL = @pesel";

                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@zmiana", zmiana);
                command.Parameters.AddWithValue("@pesel", pesel);

                command.ExecuteNonQuery();
            }

            WyswietlSzczegolyUcznia($"{imie} {nazwisko}");
        }


        private void DodajOceneButton_Click(object sender, RoutedEventArgs e)
        {
            TreeViewItem selectedItem = DrzewkoUczniowie.SelectedItem as TreeViewItem;
            if (selectedItem != null && selectedItem.Parent is TreeViewItem)
            {
                string fullName = selectedItem.Header.ToString();
                DodajOceneWindow dodajOceneWindow = new DodajOceneWindow(connectionString, fullName);
                dodajOceneWindow.Show();
            }
            else
            {
                MessageBox.Show("Proszę wybrać ucznia.");
            }
        }

        private void Dodaj_Ucznia(object sender, RoutedEventArgs e)
        {
            DodajUczniaWindow dodajUczniaWindow = new DodajUczniaWindow();
            dodajUczniaWindow.Show();
        }
    }
}