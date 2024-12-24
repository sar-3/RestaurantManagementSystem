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
    public partial class Form6 : Form
    {
        private NpgsqlConnection conn;
        public Form6()
        {
            InitializeComponent();
            ConnectToDatabase();
        }
        private void ConnectToDatabase()
        {
            string connectionString = "Host=localhost;Port=5432;Username=postgres;Password=******;Database=RestaurantManagementSystem_sar3";

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
        private void Form6_Load(object sender, EventArgs e)
        {
            string imagePath = @"C:\Users\w11tr\Desktop\VSC\coding_c++\RestaurantManagementSystem\images\genel_form.jpg";
            this.BackgroundImage = Image.FromFile(imagePath);
            this.BackgroundImageLayout = ImageLayout.Stretch;

            LoadOdemeData();
            LoadOdemeTuruComboBox();
        }
        private void LoadOdemeData()
        {
            try
            {
                conn.Open();
                string query = "SELECT odeme_id, odeme_tutari, odeme_turu, odeme_tarihi, siparis_id FROM odeme";
                NpgsqlDataAdapter da = new NpgsqlDataAdapter(query, conn);
                DataTable dt = new DataTable();
                da.Fill(dt);
                dataGridViewOdeme.DataSource = dt;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ödeme verileri yüklenirken hata oluştu: {ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                conn?.Close();
            }
        }
        private void LoadOdemeTuruComboBox()
        {
            cmbOdemeTuru.Items.Add("nakit");
            cmbOdemeTuru.Items.Add("kredi karti");
            cmbOdemeTuru.SelectedIndex = 0; 
        }
        private void ekleOdeme_Click(object sender, EventArgs e)
        {
            try
            {
                string odemeTuru = cmbOdemeTuru.SelectedItem.ToString();  
                int siparisId = Convert.ToInt32(txtSiparisId.Text); 

                decimal odemeTutari = 0;

                string query = "INSERT INTO odeme (odeme_tutari, odeme_turu, siparis_id) VALUES (@odeme_tutari, @odeme_turu, @siparis_id)";

                using (var cmd = new NpgsqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@odeme_tutari", odemeTutari); 
                    cmd.Parameters.AddWithValue("@odeme_turu", odemeTuru);      
                    cmd.Parameters.AddWithValue("@siparis_id", siparisId);     

                    conn.Open();
                    cmd.ExecuteNonQuery();

                    MessageBox.Show("Ödeme başarıyla yapıldı.", "Başarılı", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ödeme eklenirken hata oluştu: {ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                conn?.Close();

                LoadOdemeData();
            }
        }
        private void dataGridViewOdeme_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                DataGridViewRow row = dataGridViewOdeme.Rows[e.RowIndex];
                cmbOdemeTuru.SelectedItem = row.Cells["odeme_turu"].Value.ToString();
                txtSiparisId.Text = row.Cells["siparis_id"].Value.ToString();
            }
        }
        private void silOdeme_Click(object sender, EventArgs e)
        {
            try
            {
                if (dataGridViewOdeme.SelectedRows.Count == 0)
                {
                    MessageBox.Show("Lütfen silmek istediğiniz ödeme kaydını seçin.", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                int odemeId = Convert.ToInt32(dataGridViewOdeme.SelectedRows[0].Cells["odeme_id"].Value);

                string query = "DELETE FROM odeme WHERE odeme_id = @odeme_id";

                using (var cmd = new NpgsqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@odeme_id", odemeId);  

                    conn.Open();
                    int rowsAffected = cmd.ExecuteNonQuery();

                    if (rowsAffected > 0)
                    {
                        MessageBox.Show("Ödeme başarıyla silindi.", "Başarılı", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        LoadOdemeData();
                    }
                    else
                    {
                        MessageBox.Show("Silme işlemi başarısız oldu.", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Silme işlemi sırasında hata oluştu: {ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                conn?.Close();
            }
        }
        private void guncelleOdeme_Click(object sender, EventArgs e)
        {
            try
            {
                if (dataGridViewOdeme.SelectedRows.Count == 0)
                {
                    MessageBox.Show("Lütfen güncellemek istediğiniz ödeme kaydını seçin.", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                int odemeId = Convert.ToInt32(dataGridViewOdeme.SelectedRows[0].Cells["odeme_id"].Value);
                string odemeTuru = cmbOdemeTuru.SelectedItem.ToString();
                int siparisId = Convert.ToInt32(txtSiparisId.Text);

                decimal odemeTutari = 0; 

                string query = "UPDATE odeme SET odeme_turu = @odeme_turu, siparis_id = @siparis_id, odeme_tutari = @odeme_tutari WHERE odeme_id = @odeme_id";

                if (conn.State != ConnectionState.Open)
                {
                    conn.Open();
                }

                using (var cmd = new NpgsqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@odeme_turu", odemeTuru);
                    cmd.Parameters.AddWithValue("@siparis_id", siparisId);
                    cmd.Parameters.AddWithValue("@odeme_tutari", odemeTutari); 
                    cmd.Parameters.AddWithValue("@odeme_id", odemeId);

                    int rowsAffected = cmd.ExecuteNonQuery();

                    if (rowsAffected > 0)
                    {
                        MessageBox.Show("Ödeme başarıyla güncellendi.", "Başarılı", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        LoadOdemeData();
                    }
                    else
                    {
                        MessageBox.Show("Güncelleme işlemi başarısız oldu.", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Güncelleme işlemi sırasında hata oluştu: {ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                if (conn.State == ConnectionState.Open)
                    conn.Close();
            }
        }
        private void geriDon_Click(object sender, EventArgs e)
        {
            this.Close();
            Form4 form4 = new Form4();
            form4.Show();
        }

        private void cikis_Click(object sender, EventArgs e)
        {
            this.Close();
            Form1 form1 = new Form1();
            form1.Show();
        }
    }
}
