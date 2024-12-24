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
    public partial class Form10 : Form
    {
        private Dictionary<TextBox, string> defaultTexts = new Dictionary<TextBox, string>();
        private NpgsqlConnection conn;
        public Form10()
        {
            InitializeComponent();
            ConnectToDatabase();
            LoadPositionComboBox();
            LoadEmployeeData();
            InitializeDefaultTexts();

            txtPhone.KeyPress += new KeyPressEventHandler(txtPhone_KeyPress);
            txtPhone.TextChanged += new EventHandler(txtPhone_TextChanged);
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
        private void Form10_Load(object sender, EventArgs e)
        {
            string imagePath = @"C:\Users\w11tr\Desktop\VSC\coding_c++\RestaurantManagementSystem\images\genel_form.jpg";
            this.BackgroundImage = Image.FromFile(imagePath);
            this.BackgroundImageLayout = ImageLayout.Stretch;
        }        
        private void LoadPositionComboBox()
        {
            cmbPosition.Items.Add("Çalışan Pozisyonu Seçiniz");
            cmbPosition.Items.Add("garson");
            cmbPosition.Items.Add("asci");
            cmbPosition.Items.Add("personel");

            if (cmbPosition.Items.Count > 0)
            {
                cmbPosition.SelectedIndex = 0;
            }
        }
        private void LoadEmployeeData()
        {
            if (conn.State != ConnectionState.Open)
            {
                conn.Open();
            }

            try
            {
                string query = "SELECT * FROM calisan";
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
        private void InitializeDefaultTexts()
        {
            defaultTexts[txtName] = "Adını girin...";
            defaultTexts[txtSurname] = "Soyadını girin...";
            defaultTexts[txtPhone] = "+90 5__ ___ __ __";

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
        private void ClearInputFields()
        {
            InitializeDefaultTexts(); 
            dtpStartDate.Value = DateTime.Now;

            if (cmbPosition.Items.Count > 0)
            {
                cmbPosition.SelectedIndex = 0; 
            }
        }
        private void dataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0) 
            {
                DataGridViewRow row = dataGridView1.Rows[e.RowIndex];

                txtName.Text = row.Cells["isim"].Value.ToString();
                txtSurname.Text = row.Cells["soyisim"].Value.ToString();
                txtPhone.Text = row.Cells["iletisim_numarasi"].Value.ToString();
                dtpStartDate.Value = Convert.ToDateTime(row.Cells["baslangic_tarihi"].Value);

                string position = row.Cells["pozisyon"].Value.ToString();
                if (cmbPosition.Items.Contains(position))
                {
                    cmbPosition.SelectedItem = position;
                }
                else
                {
                    cmbPosition.SelectedIndex = 0; 
                }
            }
        } 
        private void ekleCalisan_Click(object sender, EventArgs e)
        {
            string isim = txtName.Text;
            string soyisim = txtSurname.Text;
            string pozisyon = cmbPosition.SelectedItem.ToString();
            string iletisimNumarasi = txtPhone.Text;
            DateTime baslangicTarihi = dtpStartDate.Value;

            if (pozisyon == "Çalışanın Pozisyonunu Seçiniz" || string.IsNullOrEmpty(pozisyon))
            {
                MessageBox.Show("Lütfen bir pozisyon seçiniz!", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (conn.State != ConnectionState.Open)
            {
                conn.Open();
            }

            try
            {
                string calisanInsertQuery = @"
            INSERT INTO calisan (isim, soyisim, pozisyon, iletisim_numarasi, baslangic_tarihi)
            VALUES (@isim, @soyisim, @pozisyon, @iletisimNumarasi, @baslangicTarihi)";

                using (var cmd = new NpgsqlCommand(calisanInsertQuery, conn))
                {
                    cmd.Parameters.AddWithValue("@isim", isim);
                    cmd.Parameters.AddWithValue("@soyisim", soyisim);
                    cmd.Parameters.AddWithValue("@pozisyon", pozisyon);
                    cmd.Parameters.AddWithValue("@iletisimNumarasi", iletisimNumarasi);
                    cmd.Parameters.AddWithValue("@baslangicTarihi", baslangicTarihi);

                    cmd.ExecuteNonQuery();
                }

                MessageBox.Show("Çalışan başarıyla eklendi!", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);

                LoadEmployeeData();
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
        private void silButon_Click(object sender, EventArgs e)
        {
            if (dataGridView1.SelectedRows.Count == 0)
            {
                MessageBox.Show("Lütfen silmek istediğiniz çalışanı seçin!", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int selectedEmployeeId = Convert.ToInt32(dataGridView1.SelectedRows[0].Cells["calisan_id"].Value);

            if (conn.State != ConnectionState.Open)
            {
                conn.Open();
            }

            try
            {
                string deleteQuery = "DELETE FROM calisan WHERE calisan_id = @calisanId";

                using (var cmd = new NpgsqlCommand(deleteQuery, conn))
                {
                    cmd.Parameters.AddWithValue("@calisanId", selectedEmployeeId);
                    int rowsAffected = cmd.ExecuteNonQuery();

                    if (rowsAffected > 0)
                    {
                        MessageBox.Show("Çalışan başarıyla silindi!", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);

                        LoadEmployeeData();
                        ClearInputFields();
                    }
                    else
                    {
                        MessageBox.Show("Çalışan silinemedi. Lütfen tekrar deneyin.", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
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
        private void guncelleButon_Click(object sender, EventArgs e)
        {
            string isim = txtName.Text;
            string soyisim = txtSurname.Text;
            string pozisyon = cmbPosition.SelectedItem.ToString();
            string iletisimNumarasi = txtPhone.Text;
            DateTime baslangicTarihi = dtpStartDate.Value;

            if (dataGridView1.CurrentRow != null && dataGridView1.CurrentRow.Cells["calisan_id"].Value != null)
            {
                int calisanId = Convert.ToInt32(dataGridView1.CurrentRow.Cells["calisan_id"].Value);

                if (conn.State != ConnectionState.Open)
                {
                    conn.Open();
                }

                try
                {
                    string updateQuery = @"
                UPDATE calisan
                SET isim = @isim, 
                    soyisim = @soyisim, 
                    pozisyon = @pozisyon, 
                    iletisim_numarasi = @iletisimNumarasi, 
                    baslangic_tarihi = @baslangicTarihi
                WHERE calisan_id = @calisanId";

                    using (var cmd = new NpgsqlCommand(updateQuery, conn))
                    {
                        cmd.Parameters.AddWithValue("@isim", isim);
                        cmd.Parameters.AddWithValue("@soyisim", soyisim);
                        cmd.Parameters.AddWithValue("@pozisyon", pozisyon);
                        cmd.Parameters.AddWithValue("@iletisimNumarasi", iletisimNumarasi);
                        cmd.Parameters.AddWithValue("@baslangicTarihi", baslangicTarihi);
                        cmd.Parameters.AddWithValue("@calisanId", calisanId);

                        int affectedRows = cmd.ExecuteNonQuery();

                        if (affectedRows > 0)
                        {
                            MessageBox.Show("Çalışan başarıyla güncellendi!", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                        else
                        {
                            MessageBox.Show("Güncelleme işlemi başarısız oldu!", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }

                    LoadEmployeeData();
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
            else
            {
                MessageBox.Show("Lütfen güncellenecek bir çalışan seçin!", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
        private void txtPhone_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
            {
                e.Handled = true; 
            }
        }
        private void txtPhone_TextChanged(object sender, EventArgs e)
        {
            if (!txtPhone.Text.StartsWith("+90"))
            {
                txtPhone.Text = "+90" + txtPhone.Text.Replace("+", "").Replace(" ", ""); 
            }

            if (txtPhone.Text.Length > 13) 
            {
                txtPhone.Text = txtPhone.Text.Substring(0, 13);
            }

            txtPhone.SelectionStart = txtPhone.Text.Length;
        }
        private void button1_Click(object sender, EventArgs e)
        {
            if (dataGridView1.SelectedRows.Count > 0)
            {
                int selectedEmployeeId = Convert.ToInt32(dataGridView1.SelectedRows[0].Cells["calisan_id"].Value);
                string selectedPosition = dataGridView1.SelectedRows[0].Cells["pozisyon"].Value.ToString();

                if (selectedPosition != "garson")
                {
                    MessageBox.Show("Bu işlem yalnızca garsonlar için geçerlidir!", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return; 
                }

                if (conn.State != ConnectionState.Open)
                {
                    conn.Open();
                }

                try
                {
                    string query = @"
            SELECT garson.servis_sayisi
            FROM garson
            WHERE garson.calisan_id = @calisanId";

                    using (var cmd = new NpgsqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@calisanId", selectedEmployeeId);

                        object result = cmd.ExecuteScalar(); 

                        if (result != null)
                        {
                            int servisSayisi = Convert.ToInt32(result);
                            MessageBox.Show($"Seçilen garsonun servis sayısı: {servisSayisi}", "Servis Sayısı", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                        else
                        {
                            MessageBox.Show("Seçilen garsonun servis sayısı bulunamadı.", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        }
                    }
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
            else
            {
                MessageBox.Show("Lütfen bir çalışan seçin!", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
        private void button2_Click(object sender, EventArgs e)
        {
            if (dataGridView1.SelectedRows.Count > 0)
            {
                int selectedEmployeeId = Convert.ToInt32(dataGridView1.SelectedRows[0].Cells["calisan_id"].Value);
                string selectedPosition = dataGridView1.SelectedRows[0].Cells["pozisyon"].Value.ToString();

                if (conn.State != ConnectionState.Open)
                {
                    conn.Open();
                }

                try
                {
                    if (selectedPosition == "personel")
                    {
                        string query = @"
                SELECT personel.gorev
                FROM personel
                WHERE personel.calisan_id = @calisanId";

                        using (var cmd = new NpgsqlCommand(query, conn))
                        {
                            cmd.Parameters.AddWithValue("@calisanId", selectedEmployeeId);

                            object result = cmd.ExecuteScalar(); 

                            if (result != null)
                            {
                                string gorev = result.ToString();
                                MessageBox.Show($"Personelin Görevi: {gorev}", "Personel Görev Bilgisi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            }
                            else
                            {
                                MessageBox.Show("Seçilen personelin görev bilgisi bulunamadı.", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            }
                        }
                    }
                    else
                    {
                        MessageBox.Show("Görev bilgisi yalnızca personel için gösterilebilir.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
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
            else
            {
                MessageBox.Show("Lütfen bir çalışan seçin!", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
        private void button3_Click(object sender, EventArgs e)
        {
            if (dataGridView1.SelectedRows.Count > 0)
            {
                int selectedEmployeeId = Convert.ToInt32(dataGridView1.SelectedRows[0].Cells["calisan_id"].Value);
                string selectedPosition = dataGridView1.SelectedRows[0].Cells["pozisyon"].Value.ToString();

                if (conn.State != ConnectionState.Open)
                {
                    conn.Open();
                }

                try
                {
                    if (selectedPosition == "asci")
                    {
                        string query = @"
                SELECT asci.deneyim_derecesi
                FROM asci
                WHERE asci.calisan_id = @calisanId";

                        using (var cmd = new NpgsqlCommand(query, conn))
                        {
                            cmd.Parameters.AddWithValue("@calisanId", selectedEmployeeId);

                            object result = cmd.ExecuteScalar(); 

                            if (result != null)
                            {
                                string deneyimDerecesi = result.ToString();
                                MessageBox.Show($"Aşçının Deneyim Derecesi: {deneyimDerecesi}", "Aşçı Deneyim Derecesi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            }
                            else
                            {
                                MessageBox.Show("Seçilen aşçının deneyim derecesi bulunamadı.", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            }
                        }
                    }
                    else
                    {
                        MessageBox.Show("Deneyim derecesi yalnızca aşçı için gösterilebilir.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
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
            else
            {
                MessageBox.Show("Lütfen bir çalışan seçin!", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
        private void geriButon_Click(object sender, EventArgs e)
        {
            this.Close();
            Form9 form9 = new Form9();
            form9.Show();
        }

        private void cikis_Click(object sender, EventArgs e)
        {
            this.Close();
            Form1 form1 = new Form1();
            form1.Show();
        }
    }
}