using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Data.SqlClient;

namespace dziennik
{
    public partial class LoginWindow : Window
    {
        private string connectionString;

        public LoginWindow()
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
                string query = "SELECT imie, nazwisko, klasa FROM uczniowie ";

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

                // Create a new window to display the student's details
                Window studentDetailsWindow = new Window
                {
                    Title = $"Details of {fullName}",
                    Width = 400,
                    Height = 500
                };

                StackPanel stackPanel = new StackPanel();
                studentDetailsWindow.Content = stackPanel;

                // Display student's basic information
                if (reader.Read())
                {
                    string pesel = reader["PESEL"].ToString();
                    string punkty = reader["punkty"].ToString();

                    stackPanel.Children.Add(new TextBlock
                    {
                        Text = $"Imię: {imie}",
                        FontWeight = FontWeights.Bold,
                        FontSize = 16
                    });
                    stackPanel.Children.Add(new TextBlock
                    {
                        Text = $"Nazwisko: {nazwisko}",
                        FontWeight = FontWeights.Bold,
                        FontSize = 16
                    });
                    stackPanel.Children.Add(new TextBlock { Text = $"PESEL: {pesel}" });
                    stackPanel.Children.Add(new TextBlock { Text = $"Punkty: {punkty}" });
                }

                // Reset the reader to read the grades
                reader.Close();
                reader = command.ExecuteReader();

                TreeView treeView = new TreeView();
                stackPanel.Children.Add(treeView);

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

                studentDetailsWindow.Show();
            }
        }
    }
}
