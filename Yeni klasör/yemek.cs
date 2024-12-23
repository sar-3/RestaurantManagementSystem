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
    public partial class yemek : Form
    {
        private NpgsqlConnection conn;
        private Dictionary<TextBox, string> defaultTexts = new Dictionary<TextBox, string>();
        public yemek()
        {
            InitializeComponent();
            ConnectToDatabase();
            InitializeDefaultTexts();
            LoadKategoriComboBox();
            LoadYemekler();
            dgvYemekler.SelectionChanged += dgvYemekler_SelectionChanged;
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
        private void ResetInputs()
        {
            foreach (var item in defaultTexts)
            {
                var textBox = item.Key;
                var defaultText = item.Value;

                textBox.Text = defaultText;
                textBox.ForeColor = Color.Gray;
            }
            numFiyat.Value = 0;

            if (cmbKategori.Items.Count > 0)
            {
                cmbKategori.SelectedIndex = 0;
            }
        }
        private void InitializeDefaultTexts()
        {
            defaultTexts[txtYemekIsmi] = "Yemek adını girin...";
            defaultTexts[txtIcerik] = "İçerik bilgilerini girin...";
            defaultTexts[txtKategoriAdi] = "Kategoriye göre yemek arayın... ";

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
        private void LoadKategoriComboBox()
        {
            string query = "SELECT kategori_id, kategori_adi FROM kategori";
            DataTable dt = new DataTable();

            try
            {
                using (NpgsqlCommand cmd = new NpgsqlCommand(query, conn))
                {
                    conn.Open();

                    // Türkçe karakterlerin düzgün işlenmesi için karakter setini ayarlayın
                    using (NpgsqlCommand encodingCmd = new NpgsqlCommand("SET CLIENT_ENCODING TO 'UTF8';", conn))
                    {
                        encodingCmd.ExecuteNonQuery(); // Karakter seti ayarını yapın
                    }

                    using (NpgsqlDataReader reader = cmd.ExecuteReader())
                    {
                        dt.Load(reader);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Kategoriler yüklenirken hata oluştu: {ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                conn.Close();
            }

            cmbKategori.DataSource = dt;
            cmbKategori.DisplayMember = "kategori_adi";
            cmbKategori.ValueMember = "kategori_id";
        }
        private void LoadYemekler()
        {
            string query = "SELECT yemek_id, isim, fiyat, kategori_id, icerik FROM yemek ORDER BY yemek_id";
            DataTable dt = new DataTable();

            try
            {
                using (NpgsqlCommand cmd = new NpgsqlCommand(query, conn))
                {
                    conn.Open();
                    using (NpgsqlDataReader reader = cmd.ExecuteReader())
                    {
                        dt.Load(reader);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Yemekler yüklenirken hata oluştu: {ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                conn.Close();
            }

            dgvYemekler.DataSource = dt;
            dgvYemekler.Columns["yemek_id"].HeaderText = "Yemek ID";
            dgvYemekler.Columns["isim"].HeaderText = "Yemek İsmi";
            dgvYemekler.Columns["fiyat"].HeaderText = "Fiyat";
            dgvYemekler.Columns["kategori_id"].HeaderText = "Kategori";
            dgvYemekler.Columns["icerik"].HeaderText = "İçerik";
        }
        private void dgvYemekler_SelectionChanged(object sender, EventArgs e)
        {
            if (dgvYemekler.SelectedRows.Count > 0)
            {
                var row = dgvYemekler.SelectedRows[0];
                txtYemekIsmi.Text = row.Cells["isim"].Value?.ToString();
                numFiyat.Value = row.Cells["fiyat"].Value == DBNull.Value ? 0 : Convert.ToDecimal(row.Cells["fiyat"].Value);
                cmbKategori.SelectedValue = row.Cells["kategori_id"].Value;
                txtIcerik.Text = row.Cells["icerik"].Value?.ToString() ?? string.Empty;
            }
        }
        private void geriButton_Click(object sender, EventArgs e)
        {
            this.Close();
            Form8 form8 = new Form8();
            form8.Show();
        }
        private void yemekEkle_Click(object sender, EventArgs e)
        {
            string yemekIsmi = txtYemekIsmi.Text;
            decimal fiyat = numFiyat.Value;
            int kategoriId = Convert.ToInt32(cmbKategori.SelectedValue);
            string icerik = txtIcerik.Text;

            string query = "INSERT INTO yemek (isim, fiyat, kategori_id, icerik) VALUES (@isim, @fiyat, @kategori_id, @icerik)";

            try
            {
                using (NpgsqlCommand cmd = new NpgsqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@isim", yemekIsmi);
                    cmd.Parameters.AddWithValue("@fiyat", fiyat);
                    cmd.Parameters.AddWithValue("@kategori_id", kategoriId);
                    cmd.Parameters.AddWithValue("@icerik", icerik);

                    conn.Open();
                    cmd.ExecuteNonQuery();
                    MessageBox.Show("Yemek başarıyla eklendi.", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    ResetInputs();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ekleme hatası: {ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                conn.Close();
                LoadYemekler();
            }
        }
        private void yemekGuncelle_Click(object sender, EventArgs e)
        {
            if (dgvYemekler.SelectedRows.Count == 0)
            {
                MessageBox.Show("Lütfen güncellemek istediğiniz yemeği seçin.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int yemekId = Convert.ToInt32(dgvYemekler.SelectedRows[0].Cells["yemek_id"].Value);
            string yemekIsmi = txtYemekIsmi.Text;
            decimal fiyat = numFiyat.Value;
            int kategoriId;

            // Kategori seçimi kontrolü
            if (cmbKategori.SelectedValue == null || !int.TryParse(cmbKategori.SelectedValue.ToString(), out kategoriId))
            {
                MessageBox.Show("Lütfen geçerli bir kategori seçin.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string icerik = txtIcerik.Text;
            try
            {
                string checkQuery = "SELECT COUNT(*) FROM yemek WHERE isim = @isim AND yemek_id != @yemek_id";
                using (NpgsqlCommand checkCmd = new NpgsqlCommand(checkQuery, conn))
                {
                    checkCmd.Parameters.AddWithValue("@isim", yemekIsmi);
                    checkCmd.Parameters.AddWithValue("@yemek_id", yemekId);

                    conn.Open();
                    int count = (int)checkCmd.ExecuteScalar();
                    conn.Close();

                    if (count > 0)
                    {
                        MessageBox.Show("Aynı isme sahip bir yemek zaten var.", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                }

                string updateQuery = "UPDATE yemek SET isim = @isim, fiyat = @fiyat, kategori_id = @kategori_id, icerik = @icerik WHERE yemek_id = @yemek_id";
                using (NpgsqlCommand updateCmd = new NpgsqlCommand(updateQuery, conn))
                {
                    updateCmd.Parameters.AddWithValue("@isim", yemekIsmi);
                    updateCmd.Parameters.AddWithValue("@fiyat", fiyat);
                    updateCmd.Parameters.AddWithValue("@kategori_id", kategoriId);
                    updateCmd.Parameters.AddWithValue("@icerik", icerik);
                    updateCmd.Parameters.AddWithValue("@yemek_id", yemekId);

                    conn.Open();
                    updateCmd.ExecuteNonQuery();
                    conn.Close();

                    MessageBox.Show("Yemek başarıyla güncellendi.", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    ResetInputs();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Güncelleme hatası: {ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                conn.Close(); // burada redundant olabilir, çünkü 'using' bloğu içinde hallediliyor
                LoadYemekler();
            }
        }
        private void yemekSil_Click(object sender, EventArgs e)
        {
            if (dgvYemekler.SelectedRows.Count == 0)
            {
                MessageBox.Show("Lütfen silmek istediğiniz yemeği seçin.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int yemekId = Convert.ToInt32(dgvYemekler.SelectedRows[0].Cells["yemek_id"].Value);

            string query = "DELETE FROM yemek WHERE yemek_id = @yemek_id";

            try
            {
                using (NpgsqlCommand cmd = new NpgsqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@yemek_id", yemekId);

                    conn.Open();
                    cmd.ExecuteNonQuery();
                    MessageBox.Show("Yemek başarıyla silindi.", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    ResetInputs();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Silme hatası: {ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                conn.Close();
                LoadYemekler();
            }
        }
        private void yemek_Load(object sender, EventArgs e)
        {
            string imagePath = @"C:\Users\w11tr\Desktop\VSC\coding_c++\RestaurantManagementSystem\images\genel_form.jpg";
            this.BackgroundImage = Image.FromFile(imagePath);
            this.BackgroundImageLayout = ImageLayout.Stretch;
        }
        private void cikis_Click(object sender, EventArgs e)
        {
            this.Close();
            Form1 form1 = new Form1();
            form1.Show();
        }
        private string LoadKatagoriicinYemekler(string kategoriAdi = null)
        {
            string query = "SELECT yemek_id, isim, fiyat, kategori_id, icerik FROM yemek";

            if (!string.IsNullOrEmpty(kategoriAdi))
            {
                query += " WHERE kategori_id IN (SELECT kategori_id FROM kategori WHERE kategori_adi ILIKE @kategori_adi)";
            }

            query += " ORDER BY yemek_id";

            StringBuilder yemeklerListesi = new StringBuilder(); // Yemekler için bir liste oluşturuluyor

            try
            {
                using (NpgsqlCommand cmd = new NpgsqlCommand(query, conn))
                {
                    if (!string.IsNullOrEmpty(kategoriAdi))
                    {
                        cmd.Parameters.AddWithValue("@kategori_adi", "%" + kategoriAdi + "%");
                    }

                    conn.Open();
                    using (NpgsqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string yemekAdi = reader["isim"].ToString();
                            decimal fiyat = reader.GetDecimal(reader.GetOrdinal("fiyat"));
                            string icerik = reader["icerik"].ToString();

                            // Yemek bilgilerini StringBuilder'a ekleyin
                            yemeklerListesi.AppendLine($"Yemek Adı: {yemekAdi}, Fiyat: {fiyat:C}, İçerik: {icerik}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Yemekler yüklenirken hata oluştu: {ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                conn.Close();
            }

            return yemeklerListesi.ToString(); // Yemek listesini döndürün
        }
        private void araButton_Click(object sender, EventArgs e)
        {
            string kategoriAdi = txtKategoriAdi.Text.Trim();

            string yemekler = string.Empty;

            if (string.IsNullOrEmpty(kategoriAdi))
            {
                yemekler = LoadKatagoriicinYemekler();  // Tüm yemekleri yükler
            }
            else
            {
                yemekler = LoadKatagoriicinYemekler(kategoriAdi);  // Kategoriye göre yemekleri yükler
            }

            if (string.IsNullOrEmpty(yemekler))
            {
                MessageBox.Show("Hiç yemek bulunamadı.", "Sonuç", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                MessageBox.Show(yemekler, "Yemek Listesi", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
    }
}
