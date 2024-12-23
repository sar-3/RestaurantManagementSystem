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
using System.Windows.Forms;


namespace RestaurantManagementSystem
{
    public partial class Form1 : Form
    {
        private Dictionary<TextBox, string> defaultTexts = new Dictionary<TextBox, string>();
        private NpgsqlConnection conn;
        public Form1()
        {
            InitializeComponent();
            ConnectToDatabase();
            InitializeDefaultTexts();
            txtPassword.GotFocus += TxtPassword_GotFocus;
            txtPassword.LostFocus += TxtPassword_LostFocus;
        }
        private void InitializeDefaultTexts()
        {
            defaultTexts[txtUsername] = "Kullanıcı adınızı girin...";
            defaultTexts[txtPassword] = "Şifrenizi girin...";

            foreach (var item in defaultTexts)
            {
                var textBox = item.Key;
                var defaultText = item.Value;

                textBox.Text = defaultText; 
                textBox.ForeColor = Color.Gray; 

                // Olayları bağla
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
                if (string.IsNullOrWhiteSpace(textBox.Text))
                {
                    textBox.Text = defaultTexts[textBox];
                    textBox.ForeColor = Color.Gray;

                    // txtPassword için PasswordChar'ı kaldır
                    if (textBox == txtPassword)
                    {
                        txtPassword.PasswordChar = '\0';
                    }
                }
            }
        }
        private void TxtPassword_GotFocus(object sender, EventArgs e)
        {
            var textBox = sender as TextBox;
            if (textBox != null)
            {
                // PasswordChar'ı ayarla
                textBox.PasswordChar = '•';
                if (textBox.Text == defaultTexts[textBox])
                {
                    textBox.Text = "";
                    textBox.ForeColor = Color.Black; // Yazı rengini siyah yap
                }
            }
        }
        private void TxtPassword_LostFocus(object sender, EventArgs e)
        {
            var textBox = sender as TextBox;
            if (textBox != null)
            {
                // Eğer TextBox boşsa varsayılan metni geri yükle
                if (string.IsNullOrWhiteSpace(textBox.Text))
                {
                    textBox.Text = defaultTexts[textBox];
                    textBox.ForeColor = Color.Gray;
                    textBox.PasswordChar = '\0'; // PasswordChar'ı kaldır
                }
            }
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
        private void Form1_Load(object sender, EventArgs e)
        {
            string imagePath = @"C:\Users\w11tr\Desktop\VSC\coding_c++\RestaurantManagementSystem\images\genel_form.jpg";
            this.BackgroundImage = Image.FromFile(imagePath);
            this.BackgroundImageLayout = ImageLayout.Stretch;
        }
        private void AdminCheck_Click(object sender, EventArgs e)
        {
            string username = txtUsername.Text;
            string password = txtPassword.Text;

            if (username == "sar3bucuk" && password == "sar3")
            {
                this.Hide(); 
                Form4 form4 = new Form4(); 
                form4.Show();
            }
            else
            {
                MessageBox.Show("Hatalı kullanıcı adı veya şifre!");
                txtUsername.Text = "";
                txtPassword.Text = "";
                txtPassword.PasswordChar = '\0';
                txtUsername.ForeColor = Color.Gray;
                txtPassword.ForeColor = Color.Gray;
                txtUsername.Text = defaultTexts[txtUsername];
                txtPassword.Text = defaultTexts[txtPassword];
            }
        }
        private void cikis_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }
    }
}
