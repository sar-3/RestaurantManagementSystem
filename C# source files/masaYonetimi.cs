using Npgsql;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using NpgsqlTypes;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RestaurantManagementSystem
{
    public partial class Form7 : Form
    {
        private readonly Dictionary<ComboBox, string> defaultComboBoxTexts = new Dictionary<ComboBox, string>();
        private NpgsqlConnection conn;
        public Form7()
        {
            InitializeComponent();
            ConnectToDatabase();
            LoadTables();
            InitializeDefaultComboBoxes();
            InitializeComboBoxes();
        }

        private void InitializeDefaultComboBoxes()
        {
            defaultComboBoxTexts[cmbBolge] = "Bölge seçin...";
            defaultComboBoxTexts[cmbKapasite] = "Kapasite seçin...";
            defaultComboBoxTexts[cmbDurum] = "Durum seçin...";

            foreach (var item in defaultComboBoxTexts)
            {
                var comboBox = item.Key;
                var defaultText = item.Value;

                comboBox.Items.Insert(0, defaultText);
                comboBox.SelectedIndex = 0;
                comboBox.ForeColor = Color.Gray;

                comboBox.SelectedIndexChanged += ComboBox_SelectedIndexChanged;
                comboBox.GotFocus += ComboBox_GotFocus;
                comboBox.LostFocus += ComboBox_LostFocus;
            }
        }
        private void ComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            var comboBox = sender as ComboBox;
            if (comboBox != null && defaultComboBoxTexts.ContainsKey(comboBox))
            {
                if (comboBox.SelectedIndex != 0)
                {
                    comboBox.ForeColor = Color.Black;
                }
            }
        }
        private void ComboBox_GotFocus(object sender, EventArgs e)
        {
            var comboBox = sender as ComboBox;
            if (comboBox != null && defaultComboBoxTexts.ContainsKey(comboBox))
            {
                if (comboBox.SelectedIndex == 0)
                {
                    comboBox.ForeColor = Color.Black;
                }
            }
        }
        private void ComboBox_LostFocus(object sender, EventArgs e)
        {
            var comboBox = sender as ComboBox;
            if (comboBox != null && defaultComboBoxTexts.ContainsKey(comboBox))
            {
                if (comboBox.SelectedIndex == 0)
                {
                    comboBox.ForeColor = Color.Gray;
                }
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
        private void LoadTables()
        {
            try
            {
                string query = "SELECT * FROM masa ORDER BY masa_id";
                using (var cmd = new NpgsqlCommand(query, conn))
                using (var adapter = new NpgsqlDataAdapter(cmd))
                {
                    DataTable masaTable = new DataTable();
                    adapter.Fill(masaTable);
                    dgvMasalar.DataSource = masaTable;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Masalar yüklenirken hata oluştu: {ex.Message}");
            }
        }
        private void InitializeComboBoxes()
        {
            cmbBolge.Items.AddRange(new string[] { "iç mekan", "bahçe", "teras" });

            cmbKapasite.Items.AddRange(new object[] { 2, 4, 5, 6, 8 });

            cmbDurum.Items.AddRange(new string[] { "bos", "dolu", "rezerve" });

            cmbBolge.SelectedIndex = 0;
            cmbKapasite.SelectedIndex = 0;
            cmbDurum.SelectedIndex = 0;
        }
        private void ClearFields()
        {
            cmbBolge.SelectedIndex = 0; 
            cmbKapasite.SelectedIndex = 0;  
            cmbDurum.SelectedIndex = 0; 

        }
        private void masaEkle_Click(object sender, EventArgs e)
        {
            try
            {
                if (conn.State != ConnectionState.Open)
                {
                    conn.Open();
                }

                string query = "SELECT ekle_masa(@kapasite, @bolge, @durum)";
                using (var cmd = new NpgsqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@kapasite", int.Parse(cmbKapasite.SelectedItem.ToString()));
                    cmd.Parameters.AddWithValue("@bolge", cmbBolge.SelectedItem.ToString());
                    cmd.Parameters.AddWithValue("@durum", cmbDurum.SelectedItem.ToString());

                    cmd.ExecuteNonQuery();
                    MessageBox.Show("Masa başarıyla eklendi!");
                    LoadTables();
                    ClearFields();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Masa eklenirken hata oluştu: {ex.Message}");
            }
            finally
            {
                if (conn.State == ConnectionState.Open)
                {
                    conn.Close();
                }
            }
        }
        private void masaSil_Click(object sender, EventArgs e)
        {
            if (dgvMasalar.SelectedRows.Count == 0)
            {
                MessageBox.Show("Lütfen silmek istediğiniz masayı seçin.", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                int masaId = Convert.ToInt32(dgvMasalar.SelectedRows[0].Cells["masa_id"].Value);

                if (conn.State != ConnectionState.Open)
                {
                    conn.Open();
                }

                string query = "SELECT sil_masa(@masa_id)";
                using (var cmd = new NpgsqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@masa_id", masaId); 
                    cmd.ExecuteNonQuery();
                    MessageBox.Show("Masa başarıyla silindi!");
                    LoadTables();
                    ClearFields();
                }
            }
            catch (NpgsqlException npgEx)
            {
                MessageBox.Show($"PostgreSQL hatası: {npgEx.Message}\nDetay: {npgEx.InnerException?.Message}");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Masa silinirken hata oluştu: {ex.Message}");
            }
            finally
            {
                if (conn.State == ConnectionState.Open)
                {
                    conn.Close();
                }
            }
        }
        private void masaGuncelle_Click(object sender, EventArgs e)
        {
            if (dgvMasalar.SelectedRows.Count == 0)
            {
                MessageBox.Show("Lütfen güncellemek istediğiniz masayı seçin.", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                int masaId = Convert.ToInt32(dgvMasalar.SelectedRows[0].Cells["masa_id"].Value);

                if (conn.State != ConnectionState.Open)
                {
                    conn.Open();
                }

                string query = "SELECT masa_guncelle(@masa_id, @kapasite, @bolge, @durum)";
                using (var cmd = new NpgsqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@masa_id", masaId);
                    cmd.Parameters.AddWithValue("@kapasite", int.Parse(cmbKapasite.SelectedItem.ToString()));
                    cmd.Parameters.AddWithValue("@bolge", cmbBolge.SelectedItem.ToString());
                    cmd.Parameters.AddWithValue("@durum", cmbDurum.SelectedItem.ToString());

                    cmd.ExecuteNonQuery();
                    MessageBox.Show("Masa başarıyla güncellendi!");
                    LoadTables();
                    ClearFields();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Masa güncellenirken hata oluştu: {ex.Message}");
            }
            finally
            {
                if (conn.State == ConnectionState.Open)
                {
                    conn.Close();
                }
            }
        }
        private void Form7_Load(object sender, EventArgs e)
        {
            string imagePath = @"C:\Users\w11tr\Desktop\VSC\coding_c++\RestaurantManagementSystem\images\genel_form.jpg";
            this.BackgroundImage = Image.FromFile(imagePath);
            this.BackgroundImageLayout = ImageLayout.Stretch;

        }
        private void geriButton_Click(object sender, EventArgs e)
        {
            this.Close();
            Form4 form4 = new Form4();
            form4.Show();
        }
        private void dgvMasalar_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return; 

            var selectedRow = dgvMasalar.Rows[e.RowIndex];

            if (selectedRow.Cells["kapasite"].Value != DBNull.Value && selectedRow.Cells["kapasite"].Value != null)
            {
                int kapasite = (int)selectedRow.Cells["kapasite"].Value;

                cmbKapasite.SelectedItem = kapasite.ToString();
            }
            else
            {
                cmbKapasite.SelectedIndex = 0;  
            }

            if (selectedRow.Cells["bolge"].Value != DBNull.Value && selectedRow.Cells["bolge"].Value != null)
            {
                cmbBolge.SelectedItem = selectedRow.Cells["bolge"].Value.ToString();
            }

            if (selectedRow.Cells["durum"].Value != DBNull.Value && selectedRow.Cells["durum"].Value != null)
            {
                cmbDurum.SelectedItem = selectedRow.Cells["durum"].Value.ToString();
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
