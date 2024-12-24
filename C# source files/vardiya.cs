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
    public partial class Form11 : Form
    {
        private NpgsqlConnection conn;
        public Form11()
        {
            InitializeComponent();
            ConnectToDatabase();
            LoadVardiyaData();
            LoadCalisanData();
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
        private void Form11_Load(object sender, EventArgs e)
        {
            string imagePath = @"C:\Users\w11tr\Desktop\VSC\coding_c++\RestaurantManagementSystem\images\genel_form.jpg";
            this.BackgroundImage = Image.FromFile(imagePath);
            this.BackgroundImageLayout = ImageLayout.Stretch;
            
        }
        private void ClearInputFields()
        {
            cmbCalisanId.SelectedIndex = -1; 
            dtpTarih.Value = DateTime.Now;
        }
        private void LoadVardiyaData()
        {
            try
            {
                if (conn.State != ConnectionState.Open)
                    conn.Open();

                string query = "SELECT * FROM vardiya";
                using (var cmd = new NpgsqlCommand(query, conn))
                {
                    NpgsqlDataAdapter da = new NpgsqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    da.Fill(dt);
                    dataGridViewVardiya.DataSource = dt;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Hata: {ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                conn.Close();
            }
        }
        private void LoadCalisanData()
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
                    cmbCalisanId.Items.Clear();

                    while (reader.Read())
                    {
                        int calisanId = Convert.ToInt32(reader["calisan_id"]);
                        cmbCalisanId.Items.Add(calisanId);
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
        private void ekleVardiya_Click(object sender, EventArgs e)
        {
            if (cmbCalisanId.SelectedIndex == -1 || dtpTarih.Value == null)
            {
                MessageBox.Show("Lütfen tüm alanları doldurun!", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            int calisanId = Convert.ToInt32(cmbCalisanId.SelectedItem);
            DateTime tarih = dtpTarih.Value;

            string query = "INSERT INTO vardiya (calisan_id, tarih, baslangic_saati, bitis_saati) VALUES (@calisan_id, @tarih, @baslangic_saati, @bitis_saati)";

            try
            {
                using (var cmd = new NpgsqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("calisan_id", calisanId);
                    cmd.Parameters.AddWithValue("tarih", tarih);

                    cmd.Parameters.AddWithValue("baslangic_saati", TimeSpan.Parse("13:00:00"));
                    cmd.Parameters.AddWithValue("bitis_saati", TimeSpan.Parse("22:00:00"));

                    conn.Open();
                    cmd.ExecuteNonQuery();
                    MessageBox.Show("Vardiya başarıyla eklendi!", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    LoadVardiyaData();
                    ClearInputFields();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Hata: {ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                conn.Close();
            }
        }
        private void guncelleVardiya_Click(object sender, EventArgs e)
        {
            if (dataGridViewVardiya.SelectedRows.Count == 0)
            {
                MessageBox.Show("Lütfen güncellemek istediğiniz vardiyayı seçin!", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            int selectedVardiyaId;
            if (!int.TryParse(dataGridViewVardiya.SelectedRows[0].Cells["vardiya_id"].Value.ToString(), out selectedVardiyaId))
            {
                MessageBox.Show("Geçerli bir vardiya seçiniz!", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (cmbCalisanId.SelectedIndex == -1)
            {
                MessageBox.Show("Lütfen bir çalışan seçin!", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            DateTime tarih = dtpTarih.Value;

            string query = "UPDATE vardiya SET calisan_id = @calisan_id, tarih = @tarih, baslangic_saati = @baslangic_saati, bitis_saati = @bitis_saati WHERE vardiya_id = @vardiya_id";

            try
            {
                using (var cmd = new NpgsqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("calisan_id", (int)cmbCalisanId.SelectedItem); 
                    cmd.Parameters.AddWithValue("tarih", tarih);

                    cmd.Parameters.AddWithValue("baslangic_saati", TimeSpan.Parse("13:00:00"));
                    cmd.Parameters.AddWithValue("bitis_saati", TimeSpan.Parse("22:00:00"));

                    conn.Open();
                    int rowsAffected = cmd.ExecuteNonQuery();

                    if (rowsAffected > 0)
                    {
                        MessageBox.Show("Vardiya başarıyla güncellendi!", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        LoadVardiyaData();
                        ClearInputFields();
                    }
                    else
                    {
                        MessageBox.Show("Güncelleme başarısız oldu!", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Hata: {ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                conn.Close();
            }
        }
        private void silVardiya_Click(object sender, EventArgs e)
        {
            if (dataGridViewVardiya.SelectedRows.Count == 0)
            {
                MessageBox.Show("Lütfen silmek istediğiniz vardiyayı seçin!", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            int selectedVardiyaId;
            if (!int.TryParse(dataGridViewVardiya.SelectedRows[0].Cells["vardiya_id"].Value.ToString(), out selectedVardiyaId))
            {
                MessageBox.Show("Geçerli bir vardiya seçiniz!", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            string query = "DELETE FROM vardiya WHERE vardiya_id = @vardiya_id";

            try
            {
                using (var cmd = new NpgsqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("vardiya_id", selectedVardiyaId);

                    conn.Open();
                    int rowsAffected = cmd.ExecuteNonQuery();

                    if (rowsAffected > 0)
                    {
                        MessageBox.Show("Vardiya başarıyla silindi!", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        LoadVardiyaData();
                        ClearInputFields();
                    }
                    else
                    {
                        MessageBox.Show("Silme işlemi başarısız oldu!", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Hata: {ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                conn.Close();
            }
        }
        private void geriDon_Click(object sender, EventArgs e)
        {
            this.Close();
            Form9 form9 = new Form9();
            form9.Show();
        }
        private void dataGridViewVardiya_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                DataGridViewRow selectedRow = dataGridViewVardiya.Rows[e.RowIndex];

                object calisanIdValue = selectedRow.Cells["calisan_id"].Value;
                if (calisanIdValue != null)
                {
                    cmbCalisanId.SelectedValue = Convert.ToInt32(calisanIdValue);
                }
                else
                {
                    cmbCalisanId.SelectedIndex = -1;
                }

                dtpTarih.Value = DateTime.Parse(selectedRow.Cells["tarih"].Value?.ToString() ?? DateTime.Now.ToString());
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
