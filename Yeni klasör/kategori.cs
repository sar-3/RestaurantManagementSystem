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
    public partial class kategori : Form
    {
        private NpgsqlConnection conn;
        private Dictionary<TextBox, string> defaultTexts = new Dictionary<TextBox, string>();
        public kategori()
        {
            InitializeComponent();
            ConnectToDatabase();
            InitializeDefaultTexts();
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
        private void kategori_Load(object sender, EventArgs e)
        {
            string imagePath = @"C:\Users\w11tr\Desktop\VSC\coding_c++\RestaurantManagementSystem\images\genel_form.jpg";
            this.BackgroundImage = Image.FromFile(imagePath);
            this.BackgroundImageLayout = ImageLayout.Stretch;

            LoadKategoriler();
            LoadMalzemeler();
            dgvKategoriler.SelectionChanged += dgvKategoriler_SelectionChanged;
            dgvMalzemeler.SelectionChanged += dgvMalzemeler_SelectionChanged;
        }
        private void dgvKategoriler_SelectionChanged(object sender, EventArgs e)
        {
            if (dgvKategoriler.SelectedRows.Count > 0)
            {
                string kategoriAdi = dgvKategoriler.SelectedRows[0].Cells["kategori_adi"].Value?.ToString();

                txtKategoriAdi.Text = kategoriAdi;
            }
        }
        private void dgvMalzemeler_SelectionChanged(object sender, EventArgs e)
        {
            if (dgvMalzemeler.SelectedRows.Count > 0)
            {
                var selectedRow = dgvMalzemeler.SelectedRows[0];

                txtMalzemeAdi.Text = selectedRow.Cells["malzeme_adi"].Value == DBNull.Value
                    ? string.Empty
                    : selectedRow.Cells["malzeme_adi"].Value.ToString();

                numMiktar.Value = selectedRow.Cells["miktar"].Value == DBNull.Value
                    ? 0
                    : Convert.ToInt32(selectedRow.Cells["miktar"].Value);
            }
        }
        private void ClearInputFields()
        {
            txtKategoriAdi.Clear();
        }
        private void ClearMalzemeFields()
        {
            txtMalzemeAdi.Clear();
            numMiktar.Value = 0;
        }
        private void InitializeDefaultTexts()
        {
            defaultTexts[txtKategoriAdi] = "Kategori adını girin...";
            defaultTexts[txtMalzemeAdi] = "Malzeme adını girin...";

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
                // Varsayılan metinse temizle
                if (textBox.Text == defaultTexts[textBox])
                {
                    textBox.Text = "";
                    textBox.ForeColor = Color.Black; // Yazı rengini siyah yap
                }
            }
        }
        private void TextBox_LostFocus(object sender, EventArgs e)
        {
            var textBox = sender as TextBox;
            if (textBox != null && defaultTexts.ContainsKey(textBox))
            {
                // Boşsa varsayılan metni geri getir
                if (string.IsNullOrWhiteSpace(textBox.Text))
                {
                    textBox.Text = defaultTexts[textBox];
                    textBox.ForeColor = Color.Gray; // Yazı rengini gri yap
                }
            }
        }
        private void LoadKategoriler()
        {
            string query = "SELECT kategori_id, kategori_adi FROM kategori ORDER BY kategori_id";
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
                MessageBox.Show($"Verileri yüklerken hata oluştu: {ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                conn.Close();
            }

            dgvKategoriler.DataSource = dt; 
            dgvKategoriler.Columns["kategori_id"].HeaderText = "Kategori Numarası:";
            dgvKategoriler.Columns["kategori_adi"].HeaderText = "Kategori Adı";
        }
        private void LoadMalzemeler()
        {
            string query = "SELECT malzeme_id, malzeme_adi, miktar FROM malzeme ORDER BY malzeme_id";
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
                MessageBox.Show($"Malzemeler yüklenirken hata oluştu: {ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                conn.Close();
            }

            dgvMalzemeler.DataSource = dt;
            dgvMalzemeler.Columns["malzeme_id"].HeaderText = "Malzeme Numarası";
            dgvMalzemeler.Columns["malzeme_adi"].HeaderText = "Malzeme Adı";
            dgvMalzemeler.Columns["miktar"].HeaderText = "Miktar";
        }
        private void ekleKategori_Click(object sender, EventArgs e)
        {
            string kategoriAdi = txtKategoriAdi.Text;

            if (string.IsNullOrEmpty(kategoriAdi))
            {
                MessageBox.Show("Lütfen kategori adını girin.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string query = "SELECT ekle_kategori(@kategoriAdi)";

            try
            {
                using (NpgsqlCommand cmd = new NpgsqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@kategoriAdi", kategoriAdi);

                    conn.Open();
                    cmd.ExecuteNonQuery();
                    MessageBox.Show("Kategori başarıyla eklendi.", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    ClearInputFields();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ekleme hatası: {ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                conn.Close();
                LoadKategoriler();
            }
        }
        private void guncelleRezervasyon_Click(object sender, EventArgs e)
        {
            if (dgvKategoriler.SelectedRows.Count == 0)
            {
                MessageBox.Show("Lütfen güncellemek istediğiniz kategoriyi seçin.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int kategoriId = Convert.ToInt32(dgvKategoriler.SelectedRows[0].Cells["kategori_id"].Value);
            string yeniKategoriAdi = txtKategoriAdi.Text;

            if (string.IsNullOrEmpty(yeniKategoriAdi))
            {
                MessageBox.Show("Lütfen yeni kategori adını girin.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string query = "UPDATE kategori SET kategori_adi = @yeniKategoriAdi WHERE kategori_id = @kategoriId";

            try
            {
                using (NpgsqlCommand cmd = new NpgsqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@yeniKategoriAdi", yeniKategoriAdi);
                    cmd.Parameters.AddWithValue("@kategoriId", kategoriId);

                    conn.Open();
                    cmd.ExecuteNonQuery();
                    MessageBox.Show("Kategori başarıyla güncellendi.", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    ClearInputFields();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Güncelleme hatası: {ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                conn.Close();
                LoadKategoriler();
            }
        }
        private void silRezervasyon_Click(object sender, EventArgs e)
        {
            if (dgvKategoriler.SelectedRows.Count == 0)
            {
                MessageBox.Show("Lütfen silmek istediğiniz kategoriyi seçin.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int kategoriId = Convert.ToInt32(dgvKategoriler.SelectedRows[0].Cells["kategori_id"].Value);

            string query = "SELECT sil_kategori(@kategoriId)";

            try
            {
                using (NpgsqlCommand cmd = new NpgsqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@kategoriId", kategoriId);

                    conn.Open();
                    cmd.ExecuteNonQuery();
                    MessageBox.Show("Kategori başarıyla silindi.", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    ClearInputFields();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Silme hatası: {ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                conn.Close();
                LoadKategoriler();
            }
        }
        private void malzemeSil_Click(object sender, EventArgs e)
        {
            if (dgvMalzemeler.SelectedRows.Count == 0)
            {
                MessageBox.Show("Lütfen silmek istediğiniz malzemeyi seçin.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int malzemeId = Convert.ToInt32(dgvMalzemeler.SelectedRows[0].Cells["malzeme_id"].Value);

            string query = "DELETE FROM malzeme WHERE malzeme_id = @malzemeId";

            try
            {
                using (NpgsqlCommand cmd = new NpgsqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@malzemeId", malzemeId);

                    conn.Open();
                    cmd.ExecuteNonQuery();
                    MessageBox.Show("Malzeme başarıyla silindi.", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    ClearMalzemeFields();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Silme hatası: {ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                conn.Close();
                LoadMalzemeler();
            }
        }
        private void malzemeGuncelle_Click(object sender, EventArgs e)
        {
            if (dgvMalzemeler.SelectedRows.Count == 0)
            {
                MessageBox.Show("Lütfen güncellemek istediğiniz malzemeyi seçin.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int malzemeId = Convert.ToInt32(dgvMalzemeler.SelectedRows[0].Cells["malzeme_id"].Value);
            string malzemeAdi = txtMalzemeAdi.Text.Trim();
            int miktar = (int)numMiktar.Value;

            if (string.IsNullOrEmpty(malzemeAdi))
            {
                MessageBox.Show("Lütfen malzeme adını girin.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string query = "UPDATE malzeme SET malzeme_adi = @malzemeAdi, miktar = @miktar WHERE malzeme_id = @malzemeId";

            try
            {
                using (NpgsqlCommand cmd = new NpgsqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@malzemeAdi", malzemeAdi);
                    cmd.Parameters.AddWithValue("@miktar", miktar);
                    cmd.Parameters.AddWithValue("@malzemeId", malzemeId);

                    conn.Open();
                    cmd.ExecuteNonQuery();
                    MessageBox.Show("Malzeme başarıyla güncellendi.", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    ClearMalzemeFields();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Güncelleme hatası: {ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                conn.Close();
                LoadMalzemeler();
            }
        }
        private void geriButton_Click(object sender, EventArgs e)
        {
            this.Close();
            Form8 form8 = new Form8();
            form8.Show();
        }
        private void cikis_Click(object sender, EventArgs e)
        {
            this.Close();
            Form1 form1 = new Form1();
            form1.Show();
        }
        private void malzemeEkle_Click_1(object sender, EventArgs e)
        {
            string malzemeAdi = txtMalzemeAdi.Text.Trim();
            int miktar = (int)numMiktar.Value;

            if (string.IsNullOrEmpty(malzemeAdi))
            {
                MessageBox.Show("Lütfen malzeme adını girin.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string query = "INSERT INTO malzeme (malzeme_adi, miktar) VALUES (@malzemeAdi, @miktar)";

            try
            {
                using (NpgsqlCommand cmd = new NpgsqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@malzemeAdi", malzemeAdi);
                    cmd.Parameters.AddWithValue("@miktar", miktar);

                    conn.Open();
                    cmd.ExecuteNonQuery();
                    MessageBox.Show("Malzeme başarıyla eklendi.", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    ClearMalzemeFields();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ekleme hatası: {ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                conn.Close();
                LoadMalzemeler();
            }
        }
    }
}