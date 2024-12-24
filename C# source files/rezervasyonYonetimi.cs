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
using System.Text.RegularExpressions;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace RestaurantManagementSystem
{
    public partial class Form2 : Form
    {
        private Dictionary<TextBox, string> defaultTexts = new Dictionary<TextBox, string>();

        private NpgsqlConnection conn;
        public Form2()
        {
            InitializeComponent();
            ConnectToDatabase();
            LoadMasaData();
            LoadReservations();
            InitializeDefaultTexts();
        }        
        private void Form2_Load(object sender, EventArgs e)
        {
            string imagePath = @"C:\Users\w11tr\Desktop\VSC\coding_c++\RestaurantManagementSystem\images\genel_form.jpg";
            this.BackgroundImage = Image.FromFile(imagePath);
            this.BackgroundImageLayout = ImageLayout.Stretch;

            txtTelefon.KeyPress += new KeyPressEventHandler(txtTelefon_KeyPress);
            txtTelefon.TextChanged += new EventHandler(txtTelefon_TextChanged);

            mtbBaslangicSaat.Text = "00:00:00";
            mtbBitisSaat.Text = "00:00:00";

            ToolTip toolTip = new ToolTip();
            toolTip.SetToolTip(nudKisiSayisi, "Kişi sayısı giriniz...");
        }
        private void dgvRezervasyonlar_CellClick_1(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;

            var selectedRow = dgvRezervasyonlar.Rows[e.RowIndex];
            txtAdSoyad.Text = selectedRow.Cells["musteri_adSoyad"].Value.ToString();
            txtTelefon.Text = selectedRow.Cells["musteri_telefon"].Value.ToString();
            cmbMasaID.SelectedValue = selectedRow.Cells["masa_id"].Value;
            dtpTarih.Value = Convert.ToDateTime(selectedRow.Cells["tarih"].Value);
            mtbBaslangicSaat.Text = selectedRow.Cells["baslangic_saat"].Value.ToString();
            mtbBitisSaat.Text = selectedRow.Cells["bitis_saat"].Value.ToString();
            nudKisiSayisi.Value = Convert.ToDecimal(selectedRow.Cells["kisi_sayisi"].Value);
        }
        private void InitializeDefaultTexts()
        {
            defaultTexts[txtAdSoyad] = "Müşteri ad soyad girin...";
            defaultTexts[txtTelefon] = "+90 5__ ___ __ __";

            foreach (var item in defaultTexts)
            {
                var textBox = item.Key;
                var defaultText = item.Value;

                textBox.Text = defaultText;
                textBox.ForeColor = Color.Gray; 

                textBox.GotFocus += TextBox_GotFocus;
                textBox.LostFocus += TextBox_LostFocus;
            }
        }
        private void TextBox_GotFocus(object sender, EventArgs e)
        {
            var textBox = sender as TextBox;
            if (textBox != null && defaultTexts.ContainsKey(textBox))
            {
                if (textBox.Text == defaultTexts[textBox])
                {
                    textBox.Text = "";
                    textBox.ForeColor = Color.Black; 
                }
            }
        }
        private void TextBox_LostFocus(object sender, EventArgs e)
        {
            var textBox = sender as TextBox;
            if (textBox != null && defaultTexts.ContainsKey(textBox))
            {
                if (string.IsNullOrWhiteSpace(textBox.Text))
                {
                    textBox.Text = defaultTexts[textBox];
                    textBox.ForeColor = Color.Gray;
                }
            }
        }
        private void ClearInputs()
        {
            txtAdSoyad.Text = string.Empty;
            txtTelefon.Text = string.Empty;
            nudKisiSayisi.Value = 1;
            mtbBaslangicSaat.Text = string.Empty;
            mtbBitisSaat.Text = string.Empty;
            dtpTarih.Value = DateTime.Now;

            cmbMasaID.SelectedIndex = -1;

            InitializeDefaultTexts();
        }
        private void LoadMasaData()
        {
            try
            {
                string query = "SELECT masa_id FROM masa"; 
                NpgsqlDataAdapter da = new NpgsqlDataAdapter(query, conn);
                DataTable dt = new DataTable();
                da.Fill(dt);

                cmbMasaID.DataSource = dt;
                cmbMasaID.DisplayMember = "masa_id";
                cmbMasaID.ValueMember = "masa_id";   
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Masa bilgilerini yüklerken hata oluştu: {ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
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
        private void LoadReservations()
        {
            try
            {
                string query = "SELECT * FROM rezervasyon";
                NpgsqlDataAdapter da = new NpgsqlDataAdapter(query, conn);
                DataTable dt = new DataTable();
                da.Fill(dt);

                dgvRezervasyonlar.DataSource = dt;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Veri yükleme hatası: {ex.Message}");
            }
        }
        private void ekleRezervasyon_Click(object sender, EventArgs e)
        {
            try
            {
                if (conn.State != ConnectionState.Open)
                {
                    conn.Open();
                }
                string baslangicSaati = mtbBaslangicSaat.Text;
                string bitisSaati = mtbBitisSaat.Text;

                string query = @"
            INSERT INTO rezervasyon (musteri_adSoyad, musteri_telefon, masa_id, tarih, baslangic_saat, bitis_saat, kisi_sayisi)
            VALUES (@adSoyad, @telefon, @masaID, @tarih, @baslangicSaat, @bitisSaat, @kisiSayisi)";

                using (var cmd = new NpgsqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@adSoyad", txtAdSoyad.Text);
                    cmd.Parameters.AddWithValue("@telefon", txtTelefon.Text);
                    cmd.Parameters.AddWithValue("@masaID", cmbMasaID.SelectedValue);
                    cmd.Parameters.AddWithValue("@tarih", dtpTarih.Value.Date);
                    cmd.Parameters.AddWithValue("@baslangicSaat", TimeSpan.Parse(baslangicSaati));
                    cmd.Parameters.AddWithValue("@bitisSaat", TimeSpan.Parse(bitisSaati));
                    cmd.Parameters.AddWithValue("@kisiSayisi", (int)nudKisiSayisi.Value);

                    cmd.ExecuteNonQuery();

                    MessageBox.Show("Rezervasyon başarıyla eklendi!", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    LoadReservations();
                    ClearInputs();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Rezervasyon eklerken hata oluştu: {ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                if (conn.State == ConnectionState.Open)
                {
                    conn.Close();
                }
            }
        }
        private void silRezervasyon_Click(object sender, EventArgs e)
        {
            try
            {
                if (conn.State != ConnectionState.Open)
                {
                    conn.Open();
                }

                string query = "DELETE FROM rezervasyon WHERE rezervasyon_id = @rezervasyonID";

                using (var cmd = new NpgsqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@rezervasyonID", Convert.ToInt32(dgvRezervasyonlar.SelectedRows[0].Cells["rezervasyon_id"].Value));

                    cmd.ExecuteNonQuery();

                    MessageBox.Show("Rezervasyon başarıyla silindi!", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    LoadReservations();
                    ClearInputs();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Rezervasyon silerken hata oluştu: {ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                if (conn.State == ConnectionState.Open)
                {
                    conn.Close();
                }
            }
        }
        private void guncelleRezervasyon_Click(object sender, EventArgs e)
        {
            try
            {
                if (conn.State != ConnectionState.Open)
                {
                    conn.Open();
                }
                string baslangicSaati = mtbBaslangicSaat.Text;
                string bitisSaati = mtbBitisSaat.Text;


                string query = @"
            UPDATE rezervasyon
            SET musteri_adSoyad = @adSoyad, musteri_telefon = @telefon, masa_id = @masaID, tarih = @tarih, 
                baslangic_saat = @baslangicSaat, bitis_saat = @bitisSaat, kisi_sayisi = @kisiSayisi
            WHERE rezervasyon_id = @rezervasyonID";

                using (var cmd = new NpgsqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@adSoyad", txtAdSoyad.Text);
                    cmd.Parameters.AddWithValue("@telefon", txtTelefon.Text);
                    cmd.Parameters.AddWithValue("@masaID", cmbMasaID.SelectedValue);
                    cmd.Parameters.AddWithValue("@tarih", dtpTarih.Value.Date);
                    cmd.Parameters.AddWithValue("@baslangicSaat", TimeSpan.Parse(baslangicSaati));
                    cmd.Parameters.AddWithValue("@bitisSaat", TimeSpan.Parse(bitisSaati));
                    cmd.Parameters.AddWithValue("@kisiSayisi", (int)nudKisiSayisi.Value);
                    cmd.Parameters.AddWithValue("@rezervasyonID", Convert.ToInt32(dgvRezervasyonlar.SelectedRows[0].Cells["rezervasyon_id"].Value));

                    cmd.ExecuteNonQuery();

                    MessageBox.Show("Rezervasyon başarıyla güncellendi!", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    LoadReservations();
                    ClearInputs();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Rezervasyon güncellerken hata oluştu: {ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                if (conn.State == ConnectionState.Open)
                {
                    conn.Close();
                }
            }
        }
        private void txtTelefon_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
            {
                e.Handled = true;
            }
        }
        private void txtTelefon_TextChanged(object sender, EventArgs e)
        {
            if (!txtTelefon.Text.StartsWith("+90"))
            {
                txtTelefon.Text = "+90" + txtTelefon.Text.Replace("+", "").Replace(" ", "");
            }

            if (txtTelefon.Text.Length > 13)
            {
                txtTelefon.Text = txtTelefon.Text.Substring(0, 13);
            }

            txtTelefon.SelectionStart = txtTelefon.Text.Length;
        }
        private void geriButton_Click(object sender, EventArgs e)
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
