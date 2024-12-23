using Npgsql;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RestaurantManagementSystem
{
    public partial class Form13 : Form
    {
        private NpgsqlConnection conn;
        public Form13()
        {
            InitializeComponent();
            ConnectToDatabase();
        }
        private void ConnectToDatabase()
        {
            string connectionString = "Host=localhost;Port=5432;Username=postgres;Password=sare1234;Database=RestaurantManagementSystem_";

            conn = new NpgsqlConnection(connectionString);

            try
            {
                conn.Open();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Bağlantı hatası: {ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                conn?.Close();
            }
        }
        private void Form13_Load(object sender, EventArgs e)
        {
            string imagePath = @"C:\Users\w11tr\Desktop\VSC\coding_c++\RestaurantManagementSystem\images\genel_form.jpg";
            this.BackgroundImage = Image.FromFile(imagePath);
            this.BackgroundImageLayout = ImageLayout.Stretch;

            LoadDeletedEmployees();
        }
        private void LoadDeletedEmployees()
        {
            if (conn.State != System.Data.ConnectionState.Open)
            {
                conn.Open();
            }

            try
            {
                string query = "SELECT * FROM silinen_calisan";

                using (var cmd = new NpgsqlCommand(query, conn))
                {
                    NpgsqlDataAdapter da = new NpgsqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    da.Fill(dt);

                    dataGridViewDeletedEmployees.DataSource = dt;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Veri yükleme hatası: {ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                conn.Close();
            }
        }
        private void geriButon_Click(object sender, EventArgs e)
        {
            Form9 form9 = new Form9();
            this.Hide(); 
            form9.ShowDialog();
            this.Close();
        }

        private void cikis_Click(object sender, EventArgs e)
        {
            this.Close();
            Form1 form1 = new Form1();
            form1.Show();
        }
    }
}
