using System;
using Npgsql;
using NpgsqlTypes;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace RestaurantManagementSystem
{
    public partial class Form3 : Form
    {
        private NpgsqlConnection conn;
        public Form3()
        {
            InitializeComponent();
            ConnectToDatabase();
            LoadMasaNumbers();
            LoadCalisanlar();
            LoadOrders();
        }
        private void Form3_Load(object sender, EventArgs e)
        {
            string imagePath = @"C:\Users\w11tr\Desktop\VSC\coding_c++\RestaurantManagementSystem\images\genel_form.jpg";
            this.BackgroundImage = Image.FromFile(imagePath);
            this.BackgroundImageLayout = ImageLayout.Stretch;

            dgvSiparisler.SelectionChanged += dgvSiparisler_SelectionChanged;
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
        private void dgvSiparisler_SelectionChanged(object sender, EventArgs e)
        {
            if (dgvSiparisler.SelectedRows.Count > 0)
            {
                var masaIdValue = dgvSiparisler.SelectedRows[0].Cells["masa_id"].Value;
                var calisanIdValue = dgvSiparisler.SelectedRows[0].Cells["calisan_id"].Value;

                if (masaIdValue != DBNull.Value && calisanIdValue != DBNull.Value)
                {
                    int masaId = Convert.ToInt32(masaIdValue);
                    int calisanId = Convert.ToInt32(calisanIdValue);

                    cmbMasaID.SelectedValue = masaId;
                    cmbCalisanID.SelectedValue = calisanId;
                }
                else
                {
                    MessageBox.Show("Seçilen satırda masa veya çalışan bilgisi eksik.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
        }
        private void ClearForm()
        {
            cmbMasaID.SelectedIndex = 0;
            cmbCalisanID.SelectedIndex = 0;
        }
        private void LoadOrders()
        {
            try
            {
                string query = "SELECT siparis_id, masa_id, calisan_id, toplam_tutar, siparis_tarihi FROM siparis";
                NpgsqlDataAdapter da = new NpgsqlDataAdapter(query, conn);
                DataTable dt = new DataTable();
                da.Fill(dt);

                dgvSiparisler.DataSource = dt;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Siparişler yüklenirken hata oluştu: {ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void LoadMasaNumbers()
        {
            try
            {
                string query = "SELECT masa_id FROM masa";
                NpgsqlDataAdapter da = new NpgsqlDataAdapter(query, conn);
                DataTable dt = new DataTable();
                da.Fill(dt);

                cmbMasaID.DisplayMember = "masa_id";  
                cmbMasaID.ValueMember = "masa_id"; 
                cmbMasaID.DataSource = dt;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Masa numaraları yüklenirken hata oluştu: {ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void LoadCalisanlar()
        {
            try
            {
                string query = "SELECT calisan_id, isim, soyisim FROM calisan";
                NpgsqlDataAdapter da = new NpgsqlDataAdapter(query, conn);
                DataTable dt = new DataTable();
                da.Fill(dt);

                dt.Columns.Add("CalisanAdSoyad", typeof(string), "calisan_id + '-' + isim + ' ' + soyisim");

                cmbCalisanID.DisplayMember = "CalisanAdSoyad";
                cmbCalisanID.ValueMember = "calisan_id";
                cmbCalisanID.DataSource = dt;

            }
            catch (Exception ex)
            {
                MessageBox.Show($"Çalışanlar yüklenirken hata oluştu: {ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void ekleSiparis_Click(object sender, EventArgs e)
        {
            try
            {
                if (cmbMasaID.SelectedIndex == -1 || cmbCalisanID.SelectedIndex == -1)
                {
                    MessageBox.Show("Lütfen bir masa ve çalışan seçin!", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                string query = @"
            INSERT INTO siparis (masa_id, calisan_id)
            VALUES (@masaID, @calisanID) RETURNING siparis_id";  

                using (var cmd = new NpgsqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@masaID", cmbMasaID.SelectedValue);
                    cmd.Parameters.AddWithValue("@calisanID", cmbCalisanID.SelectedValue);

                    if (conn.State == ConnectionState.Open)
                    {
                        conn.Close();
                    }

                    conn.Open();

                    int siparisId = (int)cmd.ExecuteScalar();

                    conn.Close();

                    MessageBox.Show("Yeni sipariş başarıyla eklendi!", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    LoadOrders();
                    ClearForm();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Sipariş eklerken hata oluştu: {ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void silSiparis_Click(object sender, EventArgs e)
        {
            try
            {
                if (dgvSiparisler.SelectedRows.Count == 0)
                {
                    MessageBox.Show("Lütfen silmek istediğiniz siparişi seçin!", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                int siparisId = Convert.ToInt32(dgvSiparisler.SelectedRows[0].Cells["siparis_id"].Value);

                string query = "DELETE FROM siparis WHERE siparis_id = @siparisID";

                using (var cmd = new NpgsqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@siparisID", siparisId);

                    if (conn.State != ConnectionState.Open)
                    {
                        conn.Open();
                    }

                    cmd.ExecuteNonQuery();
                    conn.Close();

                    MessageBox.Show("Sipariş başarıyla silindi!", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    LoadOrders();
                    ClearForm();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Sipariş silinirken hata oluştu: {ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void guncelleSiparis_Click(object sender, EventArgs e)
        {
            try
            {
                if (dgvSiparisler.SelectedRows.Count == 0)
                {
                    MessageBox.Show("Lütfen güncellemek istediğiniz siparişi seçin!", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                int siparisId = Convert.ToInt32(dgvSiparisler.SelectedRows[0].Cells["siparis_id"].Value);

                string query = @"
                UPDATE siparis
                SET masa_id = @masaID, calisan_id = @calisanID
                WHERE siparis_id = @siparisID";

                using (var cmd = new NpgsqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@masaID", cmbMasaID.SelectedValue);
                    cmd.Parameters.AddWithValue("@calisanID", cmbCalisanID.SelectedValue);
                    cmd.Parameters.AddWithValue("@siparisID", siparisId);

                    if (conn.State != ConnectionState.Open)
                    {
                        conn.Open();
                    }

                    cmd.ExecuteNonQuery();
                    conn.Close();

                    MessageBox.Show("Sipariş başarıyla güncellendi!", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    LoadOrders();
                    ClearForm();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Sipariş güncellenirken hata oluştu: {ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void button4_Click(object sender, EventArgs e)
        {
            this.Close(); 
            Form4 form4 = new Form4(); 
            form4.Show();
        }
        private void yemekEkle_Click(object sender, EventArgs e)
        {
            this.Close();
            yemekEkle yemekEkle = new yemekEkle();
            yemekEkle.Show();
        }
        private void cikis_Click(object sender, EventArgs e)
        {
            this.Close();
            Form1 form1 = new Form1();
            form1.Show();
        }
    }
}
