using Pepper_Music.Model;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace Pepper_Music
{
    public partial class Form1 : Form
    {
        private readonly string connectionString = @"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=C:\Users\cavad\OneDrive\Belgeler\pepperdata.mdf;Integrated Security=True;Connect Timeout=30";
        private SqlConnection sqlConnection;

        WMPLib.WindowsMediaPlayer player = new WMPLib.WindowsMediaPlayer();
        List<Song> songs = new List<Song>();
        int count = 0;

        public Form1()
        {
            InitializeComponent();
            InitializeDatabase();
            LoadSongsFromDatabase(); // Database'den şarkıları yükle
            UpdateListBox(); // Listbox'ı güncelle

            // Yeni eklenen kısım
            CheckAndAddNewSongs();
        }

        private void InitializeDatabase()
        {
            sqlConnection = new SqlConnection(connectionString);
            try
            {
                sqlConnection.Open();
                // Create your Songs table if it doesn't exist
                CreateSongsTable();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error connecting to the database: {ex.Message}", "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void CreateSongsTable()
        {
            try
            {
                using (SqlCommand command = new SqlCommand(
                    "CREATE TABLE IF NOT EXISTS Songs (ID INT PRIMARY KEY IDENTITY, FilePath NVARCHAR(MAX), FileName NVARCHAR(MAX))",
                    sqlConnection))
                {
                    command.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error creating the Songs table: {ex.Message}", "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadSongsFromDatabase()
        {
            try
            {
                using (SqlCommand command = new SqlCommand("SELECT * FROM Songs", sqlConnection))
                {
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string filePath = reader["FilePath"].ToString();
                            string fileName = reader["FileName"].ToString();
                            songs.Add(new Song(filePath, fileName));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading songs from the database: {ex.Message}", "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void UpdateListBox()
        {
            listBox1.Items.Clear();
            foreach (var song in songs)
            {
                listBox1.Items.Add(song.Name);
            }
        }

        private void CheckAndAddNewSongs()
        {
            string musicFolderPath = @"C:\Users\cavad\Downloads"; // Kullanıcının müzik dosyalarının bulunduğu klasörü belirtin
            if (Directory.Exists(musicFolderPath))
            {
                var mp3Files = Directory.GetFiles(musicFolderPath, "*.mp3", SearchOption.TopDirectoryOnly);
                foreach (var mp3File in mp3Files)
                {
                    if (songs.All(song => song.FilePath != mp3File))
                    {
                        // Eğer bu dosya zaten listede yoksa ekleyin
                        string fileName = Path.GetFileNameWithoutExtension(mp3File);
                        songs.Add(new Song(mp3File, fileName));

                        // Veritabanına da ekleyin
                        InsertSongIntoDatabase(mp3File, fileName);
                    }
                }

                // Listbox'ı güncelle
                UpdateListBox();
            }
        }

        private void InsertSongIntoDatabase(string filePath, string fileName)
        {
            try
            {
                using (SqlCommand command = new SqlCommand(
                    "INSERT INTO Songs (FilePath, FileName) VALUES (@filePath, @fileName)",
                    sqlConnection))
                {
                    command.Parameters.AddWithValue("@filePath", filePath);
                    command.Parameters.AddWithValue("@fileName", fileName);
                    command.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error inserting song into the database: {ex.Message}", "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            sqlConnection.Close(); // Form kapatıldığında veritabanı bağlantısını kapat
        }
    }
}
