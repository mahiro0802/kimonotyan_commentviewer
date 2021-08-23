using System;
using System.IO;
using Codeplex.Data;
using System.Windows.Forms;

namespace kimonoちゃんコメビュ
{
    public partial class SettingForm2 : Form
    {
        public SettingForm2()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            FontDialog ff = new FontDialog();
            ff.Font = Properties.Settings.Default.font;
            ff.FontMustExist = true;
            ff.AllowVerticalFonts = true;
            ff.FontMustExist = true;
            
            ff.ShowEffects = false;
            if (ff.ShowDialog() != DialogResult.Cancel)
            {
                textBox1.Text = ff.Font.Name.ToString();
                textBox2.Text = ff.Font.Size.ToString();
                Properties.Settings.Default.font = ff.Font;
            }
        }

        private void SettingForm2_Load(object sender, EventArgs e)
        {
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            textBox1.Text = Properties.Settings.Default.font.Name;
            textBox2.Text = Properties.Settings.Default.font.Size.ToString();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Properties.Settings.Default.Save();
            this.Close();
        }
    }
}
