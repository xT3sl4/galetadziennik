﻿using System;
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

        public Uczenlogin()
        {
            InitializeComponent();
            connectionString = "Data Source=10.1.49.186;Initial Catalog=Szkola;User ID=admin2;Password=zaq1@WSX;Connect Timeout=30;Encrypt=True;TrustServerCertificate=True;";
            Ucenyuczen();
        }

        private void Ucenyuczen()
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string query = @"
                    SELECT oceny.ocena, przedmioty.nazwa 
                    FROM oceny 
                    JOIN przedmioty ON oceny.Id_przedmiotu = przedmioty.Id";

                SqlCommand command = new SqlCommand(query, connection);
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
    }
}