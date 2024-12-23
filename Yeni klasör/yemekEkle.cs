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
using System.Xml.Linq;

namespace RestaurantManagementSystem
{
    public partial class yemekEkle : Form
    {
        private Dictionary<TextBox, string> defaultTexts = new Dictionary<TextBox, string>();
        private NpgsqlConnection conn;
        public yemekEkle()
        {
            InitializeComponent();
            ConnectToDatabase();
            LoadSiparisDetayData();
            LoadKategoriData();
            InitializeDefaultTexts();

            comboBoxKategori.SelectedIndexChanged += new EventHandler(comboBoxKategori_SelectedIndexChanged);
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
        private void InitializeDefaultTexts()
        {
            defaultTexts[txtSiparisIdSearch] = "Sipariş numarasına göre arama yapın...";

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
        private void yemekEkle_Load(object sender, EventArgs e)
        {
            string imagePath = @"C:\Users\w11tr\Desktop\VSC\coding_c++\RestaurantManagementSystem\images\genel_form.jpg";
            this.BackgroundImage = Image.FromFile(imagePath);
            this.BackgroundImageLayout = ImageLayout.Stretch;
        }
        private void LoadKategoriData()
        {
            try
            {
                conn.Open();
                string query = "SELECT kategori_id, kategori_adi FROM kategori"; 
                NpgsqlDataAdapter da = new NpgsqlDataAdapter(query, conn);
                DataTable dt = new DataTable();
                da.Fill(dt);

                comboBoxKategori.DisplayMember = "kategori_adi"; 
                comboBoxKategori.ValueMember = "kategori_id";   
                comboBoxKategori.DataSource = dt;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Kategori verileri yüklenirken hata oluştu: {ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                conn?.Close();
            }
        }
        private void comboBoxKategori_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBoxKategori.SelectedValue != null)
            {
                int selectedKategoriId = (int)comboBoxKategori.SelectedValue;
                LoadYemekData(selectedKategoriId); // Kategoriyi parametre olarak geçiyoruz
            }
        }
        private void LoadYemekData(int kategoriId)
        {
            try
            {
                conn.Open();
                string query = "SELECT yemek_id, isim FROM yemek WHERE kategori_id = @kategori_id"; // Yemek tablosunda kategoriye göre filtreleme
                NpgsqlDataAdapter da = new NpgsqlDataAdapter(query, conn);
                da.SelectCommand.Parameters.AddWithValue("@kategori_id", kategoriId);
                DataTable dt = new DataTable();
                da.Fill(dt);

                comboBoxYemek.DisplayMember = "isim"; 
                comboBoxYemek.ValueMember = "yemek_id"; 
                comboBoxYemek.DataSource = dt;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Yemek verileri yüklenirken hata oluştu: {ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                conn?.Close();
            }
        }
        private void LoadSiparisDetayData()
        {
            try
            {
                conn.Open();
                string query = "SELECT * FROM siparis_detay";
                NpgsqlDataAdapter da = new NpgsqlDataAdapter(query, conn);
                DataTable dt = new DataTable();
                da.Fill(dt);
                dataGridViewSiparisDetay.DataSource = dt;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Veri yükleme hatası: {ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                conn?.Close();
            }
        }
        private void yemekekleEkle_Click(object sender, EventArgs e)
        {
            string connectionString = "Host=localhost;Port=5432;Username=postgres;Password=sare1234;Database=RestaurantManagementSystem_";

            try
            {
                using (var conn = new NpgsqlConnection(connectionString))
                {
                    conn.Open();

                    string fiyatQuery = "SELECT fiyat FROM yemek WHERE yemek_id = @yemek_id";
                    decimal fiyat = 0;

                    using (var fiyatCmd = new NpgsqlCommand(fiyatQuery, conn))
                    {
                        fiyatCmd.Parameters.AddWithValue("@yemek_id", (int)comboBoxYemek.SelectedValue);
                        fiyat = (decimal)fiyatCmd.ExecuteScalar();
                    }

                    string query = "INSERT INTO siparis_detay (siparis_id, yemek_id, miktar, fiyat) VALUES (@siparis_id, @yemek_id, @miktar, @fiyat)";
                    using (var cmd = new NpgsqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@siparis_id", int.Parse(txtSiparisId.Text));
                        cmd.Parameters.AddWithValue("@yemek_id", (int)comboBoxYemek.SelectedValue);
                        cmd.Parameters.AddWithValue("@miktar", (int)numericUpDownMiktar.Value);
                        cmd.Parameters.AddWithValue("@fiyat", fiyat);
                        cmd.ExecuteNonQuery();
                    }

                    MessageBox.Show("Sipariş detayı başarıyla eklendi.", "Başarılı", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }

                LoadSiparisDetayData();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ekleme hatası: {ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void geriButton_Click(object sender, EventArgs e)
        {
            this.Close();
            Form3 form3 = new Form3();
            form3.Show();
        }
        private void yemekSil_Click(object sender, EventArgs e)
        {
            if (dataGridViewSiparisDetay.SelectedRows.Count == 0)
            {
                MessageBox.Show("Lütfen silmek istediğiniz sipariş detayını seçin.", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                int siparisDetayId = Convert.ToInt32(dataGridViewSiparisDetay.SelectedRows[0].Cells["siparis_detay_id"].Value);

                string connectionString = "Host=localhost;Port=5432;Username=postgres;Password=sare1234;Database=RestaurantManagementSystem_";

                using (var conn = new NpgsqlConnection(connectionString))
                {
                    conn.Open();

                    string query = "DELETE FROM siparis_detay WHERE siparis_detay_id = @siparis_detay_id";
                    using (var cmd = new NpgsqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@siparis_detay_id", siparisDetayId);

                        int rowsAffected = cmd.ExecuteNonQuery();

                        if (rowsAffected > 0)
                        {
                            MessageBox.Show("Sipariş detayı başarıyla silindi.", "Başarılı", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            LoadSiparisDetayData(); // Veriyi güncelle
                        }
                        else
                        {
                            MessageBox.Show("Silme işlemi başarısız oldu.", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Silme hatası: {ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void PopulateFieldsFromSelectedRow(int rowIndex)
        {
            // Seçilen satırdaki hücre değerlerini al
            DataGridViewRow selectedRow = dataGridViewSiparisDetay.Rows[rowIndex];

            txtSiparisId.Text = selectedRow.Cells["siparis_id"].Value.ToString();

            comboBoxYemek.SelectedValue = selectedRow.Cells["yemek_id"].Value;

            numericUpDownMiktar.Value = Convert.ToDecimal(selectedRow.Cells["miktar"].Value);
        }
        private void dataGridViewSiparisDetay_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                PopulateFieldsFromSelectedRow(e.RowIndex);
            }
        }
        private void yemekGuncelle_Click(object sender, EventArgs e)
        {
            if (dataGridViewSiparisDetay.SelectedRows.Count == 0)
            {
                MessageBox.Show("Lütfen güncellemek istediğiniz sipariş detayını seçin.", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(txtSiparisId.Text) || !int.TryParse(txtSiparisId.Text, out int siparisId))
            {
                MessageBox.Show("Geçerli bir sipariş ID giriniz.", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (comboBoxYemek.SelectedValue == null || !int.TryParse(comboBoxYemek.SelectedValue.ToString(), out int yemekId))
            {
                MessageBox.Show("Geçerli bir yemek seçiniz.", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int miktar = (int)numericUpDownMiktar.Value;
            int siparisDetayId = Convert.ToInt32(dataGridViewSiparisDetay.SelectedRows[0].Cells["siparis_detay_id"].Value);

            try
            {
                string connectionString = "Host=localhost;Port=5432;Username=postgres;Password=sare1234;Database=RestaurantManagementSystem_";

                using (var conn = new NpgsqlConnection(connectionString))
                {
                    conn.Open();

                    string query = "UPDATE siparis_detay SET siparis_id = @siparis_id, yemek_id = @yemek_id, miktar = @miktar WHERE siparis_detay_id = @siparis_detay_id";

                    using (var cmd = new NpgsqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@siparis_id", siparisId);
                        cmd.Parameters.AddWithValue("@yemek_id", yemekId);
                        cmd.Parameters.AddWithValue("@miktar", miktar);
                        cmd.Parameters.AddWithValue("@siparis_detay_id", siparisDetayId);

                        int rowsAffected = cmd.ExecuteNonQuery();

                        if (rowsAffected > 0)
                        {
                            MessageBox.Show("Sipariş detayı başarıyla güncellendi.", "Başarılı", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            LoadSiparisDetayData(); // Veriyi güncelle
                        }
                        else
                        {
                            MessageBox.Show("Güncelleme işlemi başarısız oldu.", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Güncelleme hatası: {ex.Message}\n{ex.StackTrace}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void cikis_Click(object sender, EventArgs e)
        {
            this.Close();
            Form1 form1 = new Form1();
            form1.Show();
        }
        private void LoadAndDisplaySiparisDetay(int siparisId)
        {
            try
            {
                conn.Open();
                string query = "SELECT sd.siparis_detay_id, y.isim AS yemek_adi, sd.miktar, sd.fiyat, sd.toplam_tutar " +
                               "FROM siparis_detay sd " +
                               "JOIN yemek y ON sd.yemek_id = y.yemek_id " +
                               "WHERE sd.siparis_id = @siparis_id";

                NpgsqlDataAdapter da = new NpgsqlDataAdapter(query, conn);
                da.SelectCommand.Parameters.AddWithValue("@siparis_id", siparisId);
                DataTable dt = new DataTable();
                da.Fill(dt);

                StringBuilder message = new StringBuilder();
                if (dt.Rows.Count > 0)
                {
                    foreach (DataRow row in dt.Rows)
                    {
                        string yemekAdi = row["yemek_adi"].ToString();
                        int miktar = Convert.ToInt32(row["miktar"]);
                        decimal fiyat = Convert.ToDecimal(row["fiyat"]);
                        decimal toplamTutar = Convert.ToDecimal(row["toplam_tutar"]);

                        message.AppendLine($"Yemek Adı: {yemekAdi}, Miktar: {miktar}, Fiyat: {fiyat:C}, Toplam Tutar: {toplamTutar:C}");
                    }

                    MessageBox.Show(message.ToString(), "Sipariş Detayları", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    MessageBox.Show("Bu sipariş ID'ye ait detay bulunamadı.", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Veri yükleme hatası: {ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                conn?.Close();
            }
        }
        private void araButton_Click(object sender, EventArgs e)
        {
            string siparisIdInput = txtSiparisIdSearch.Text.Trim();

            if (string.IsNullOrEmpty(siparisIdInput))
            {
                MessageBox.Show("Lütfen bir sipariş ID girin.", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!int.TryParse(siparisIdInput, out int siparisId))
            {
                MessageBox.Show("Geçersiz sipariş ID. Lütfen geçerli bir sayı girin.", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Sipariş detaylarını yükle ve MessageBox'ta göster
            LoadAndDisplaySiparisDetay(siparisId);
        }
    }
}
