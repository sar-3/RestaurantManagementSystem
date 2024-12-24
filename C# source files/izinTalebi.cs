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
    public partial class Form12 : Form
    {
        private NpgsqlConnection conn;
        public Form12()
        {
            InitializeComponent();
            ConnectToDatabase();
            LoadIzinData();
            LoadCalisanlar();
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
        private void Form12_Load(object sender, EventArgs e)
        {
            string imagePath = @"C:\Users\w11tr\Desktop\VSC\coding_c++\RestaurantManagementSystem\images\genel_form.jpg";
            this.BackgroundImage = Image.FromFile(imagePath);
            this.BackgroundImageLayout = ImageLayout.Stretch;
        }
        private void LoadIzinData()
        {
            if (conn.State != System.Data.ConnectionState.Open)
            {
                conn.Open();
            }

            try
            {
                string query = "SELECT * FROM izin_talebi";
                using (var cmd = new NpgsqlCommand(query, conn))
                {
                    NpgsqlDataAdapter da = new NpgsqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    da.Fill(dt);
                    dataGridView1.DataSource = dt;
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
        private void LoadCalisanlar()
        {
            if (conn.State != ConnectionState.Open)
            {
                conn.Open();
            }

            try
            {
                string query = "SELECT calisan_id FROM calisan"; 
                using (var cmd = new NpgsqlCommand(query, conn))
                {
                    NpgsqlDataReader reader = cmd.ExecuteReader();
                    comboBoxCalisanlar.Items.Clear();  

                    while (reader.Read())
                    {
                        int calisanId = Convert.ToInt32(reader["calisan_id"]);
                        comboBoxCalisanlar.Items.Add(calisanId); 
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Çalışanları yükleme hatası: {ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                conn.Close();
            }
        }
        private void ClearInputFields()
        {
            txtAciklama.Clear(); 
            dtpBaslangicTarihi.Value = DateTime.Now; 
            dtpBitisTarihi.Value = DateTime.Now; 
            comboBoxCalisanlar.SelectedIndex = -1; 
        }
        private void izinEkle_Click(object sender, EventArgs e)
        {
            if (comboBoxCalisanlar.SelectedItem == null)
            {
                MessageBox.Show("Lütfen bir çalışan seçin!", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int calisanId = (int)comboBoxCalisanlar.SelectedItem;
            DateTime baslangicTarihi = dtpBaslangicTarihi.Value;
            DateTime bitisTarihi = dtpBitisTarihi.Value;
            string aciklama = txtAciklama.Text;

            if (conn.State != ConnectionState.Open)
            {
                conn.Open();
            }

            try
            {
                string insertQuery = @"
                    INSERT INTO izin_talebi (calisan_id, baslangic_tarihi, bitis_tarihi, aciklama)
                    VALUES (@calisanId, @baslangicTarihi, @bitisTarihi, @aciklama)";

                using (var cmd = new NpgsqlCommand(insertQuery, conn))
                {
                    cmd.Parameters.AddWithValue("@calisanId", calisanId);
                    cmd.Parameters.AddWithValue("@baslangicTarihi", baslangicTarihi);
                    cmd.Parameters.AddWithValue("@bitisTarihi", bitisTarihi);
                    cmd.Parameters.AddWithValue("@aciklama", aciklama);

                    cmd.ExecuteNonQuery();
                }

                MessageBox.Show("İzin talebi başarıyla eklendi!", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);

                LoadIzinData();
                ClearInputFields();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Hata oluştu: {ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                conn.Close();
            }
        }
        private void izinGuncelle_Click(object sender, EventArgs e)
        {
            if (comboBoxCalisanlar.SelectedItem == null)
            {
                MessageBox.Show("Lütfen bir çalışan seçin!", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (dataGridView1.SelectedRows.Count == 0)
            {
                MessageBox.Show("Lütfen güncellemek istediğiniz izin talebini seçin.", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int izinId = Convert.ToInt32(dataGridView1.SelectedRows[0].Cells["izin_id"].Value);
            int calisanId = (int)comboBoxCalisanlar.SelectedItem;
            DateTime baslangicTarihi = dtpBaslangicTarihi.Value;
            DateTime bitisTarihi = dtpBitisTarihi.Value;
            string aciklama = txtAciklama.Text;

            if (conn.State != ConnectionState.Open)
            {
                conn.Open();
            }

            try
            {
                string updateQuery = @"
                    UPDATE izin_talebi
                    SET calisan_id = @calisanId, baslangic_tarihi = @baslangicTarihi, bitis_tarihi = @bitisTarihi, aciklama = @aciklama
                    WHERE izin_id = @izinId";

                using (var cmd = new NpgsqlCommand(updateQuery, conn))
                {
                    cmd.Parameters.AddWithValue("@izinId", izinId);
                    cmd.Parameters.AddWithValue("@calisanId", calisanId);
                    cmd.Parameters.AddWithValue("@baslangicTarihi", baslangicTarihi);
                    cmd.Parameters.AddWithValue("@bitisTarihi", bitisTarihi);
                    cmd.Parameters.AddWithValue("@aciklama", aciklama);

                    cmd.ExecuteNonQuery();
                }

                MessageBox.Show("İzin talebi başarıyla güncellendi!", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);

                LoadIzinData();
                ClearInputFields();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Hata oluştu: {ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                conn.Close();
            }
        }
        private void izinSil_Click(object sender, EventArgs e)
        {
            if (dataGridView1.SelectedRows.Count == 0)
            {
                MessageBox.Show("Lütfen silmek istediğiniz izin talebini seçin.", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int izinId = Convert.ToInt32(dataGridView1.SelectedRows[0].Cells["izin_id"].Value);

            DialogResult result = MessageBox.Show("Seçilen izin talebini silmek istediğinizden emin misiniz?", "Onay", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                if (conn.State != ConnectionState.Open)
                {
                    conn.Open();
                }

                try
                {
                    string deleteQuery = "DELETE FROM izin_talebi WHERE izin_id = @izinId";
                    using (var cmd = new NpgsqlCommand(deleteQuery, conn))
                    {
                        cmd.Parameters.AddWithValue("@izinId", izinId);
                        cmd.ExecuteNonQuery();
                    }

                    MessageBox.Show("İzin talebi başarıyla silindi!", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    LoadIzinData();
                    ClearInputFields();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Hata oluştu: {ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                finally
                {
                    conn.Close();
                }
            }
        }
        private void geriDon_Click(object sender, EventArgs e)
        {
            this.Close();
            Form9 form9 = new Form9();
            form9.Show();
        }
        private void dataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                DataGridViewRow row = dataGridView1.Rows[e.RowIndex];

                txtAciklama.Text = row.Cells["aciklama"].Value.ToString();
                dtpBaslangicTarihi.Value = Convert.ToDateTime(row.Cells["baslangic_tarihi"].Value);
                dtpBitisTarihi.Value = Convert.ToDateTime(row.Cells["bitis_tarihi"].Value);
                comboBoxCalisanlar.SelectedItem = Convert.ToInt32(row.Cells["calisan_id"].Value);
            }
        }
        private void cikis_Click(object sender, EventArgs e)
        {
            this.Close();
            Form1 form1 = new Form1();
            form1.Show();
        }
    }
}
