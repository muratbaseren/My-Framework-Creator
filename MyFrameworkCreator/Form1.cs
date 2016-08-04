using AdoNetHelper;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MyFrameworkCreator
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void btnBaglan_Click(object sender, EventArgs e)
        {
            string connStr = GetConnectionString("master");

            Database db = new Database(connStr);
            DataTable dt = db.GetTable("SELECT name, database_id FROM sys.databases ORDER BY name", null);

            cmbVeritabanlari.Items.Clear();

            foreach (DataRow dr in dt.Rows)
            {
                Veritabani vt = new Veritabani()
                {
                    id = (int)dr["database_id"],
                    Name = dr["name"].ToString()
                };

                cmbVeritabanlari.Items.Add(vt);
            }
        }

        private string GetConnectionString(string dbname)
        {
            string connStr = "Server=" + txtSunucu.Text + "; Database=" + dbname + "; ";

            if (cmbYontem.Text == "Windows Auth.")
            {
                connStr += "Trusted_connection=true;";
            }
            else if (cmbYontem.Text == "SQL Auth.")
            {
                connStr += "User Id=" + txtKullanici.Text + "; Password=" + txtSifre.Text;
            }

            return connStr;
        }

        private void cmbYontem_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmbYontem.Text == "Windows Auth.")
            {
                txtKullanici.Enabled = false;
                txtSifre.Enabled = false;
            }
            else if (cmbYontem.Text == "SQL Auth.")
            {
                txtKullanici.Enabled = true;
                txtSifre.Enabled = true;
            }
        }

        private void cmbVeritabanlari_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmbVeritabanlari.SelectedIndex > -1)
            {
                string connStr = GetConnectionString(cmbVeritabanlari.Text);

                Database db = new Database(connStr);
                DataTable dt = db.GetTable("SELECT name, object_id FROM sys.tables", null);

                clbTablolar.Items.Clear();

                foreach (DataRow dr in dt.Rows)
                {
                    Tablo tbl = new Tablo()
                    {
                        id = (int)dr["object_id"],
                        Name = dr["name"].ToString()
                    };

                    clbTablolar.Items.Add(tbl);
                }
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            txtSunucu.Text = Environment.MachineName;
            cmbYontem.SelectedIndex = 1;
            txtKullanici.Text = "sa";
            txtSifre.Focus();
        }

        private void btnOlustur_Click(object sender, EventArgs e)
        {
            tabControl1.TabPages.Clear();

            if (clbTablolar.CheckedItems.Count > 0)
            {
                string connStr = GetConnectionString(cmbVeritabanlari.Text);

                foreach (object item in clbTablolar.CheckedItems)
                {
                    Tablo t = item as Tablo;

                    string query = "SELECT C.column_id, C.name AS column_name, T.name AS ctype_name, C.is_nullable, C.max_length, C.is_identity FROM sys.columns AS C INNER JOIN sys.types AS T ON T.system_type_id = C.system_type_id WHERE C.object_id = " + t.id.ToString() + " AND T.name != 'sysname'";

                    Database db = new Database(connStr);
                    DataTable dt = db.GetTable(query, null);    // columns bilgisi geldi.

                    TabPage tp = new TabPage(t.Name);
                    RichTextBox rtb = new RichTextBox();
                    rtb.Dock = DockStyle.Fill;

                    List<Kolon> kolonlar = new List<Kolon>();

                    foreach (DataRow dr in dt.Rows)
                    {
                        Kolon k = new Kolon()
                        {
                            Id = int.Parse(dr["column_id"].ToString()),
                            Name = dr["column_name"].ToString(),
                            MaxLength = (dr["max_length"] == DBNull.Value) ? null : (int?)int.Parse(dr["max_length"].ToString()),
                            TypeName = dr["ctype_name"].ToString(),
                            IsIdentity = (bool)dr["is_identity"],
                            IsNullable = (bool)dr["is_nullable"]
                        };

                        kolonlar.Add(k);
                    }

                    rtb.Tag = kolonlar;
                    CreateClassFile(rtb, t);

                    tp.Controls.Add(rtb);
                    tabControl1.TabPages.Add(tp);
                }
            }
        }

        private void CreateClassFile(RichTextBox rtb, Tablo t)
        {
            List<Kolon> kolonlar = rtb.Tag as List<Kolon>;

            rtb.Text += "using System;\nusing System.Collections.Generic;\nusing System.Linq;\nusing System.Text;\nusing System.Threading.Tasks;\n\nnamespace Entities\n{\n\tpublic class " + t.Name + "\n\t{\n";

            foreach (Kolon kolon in kolonlar)
            {
                rtb.Text += "\t\t" + kolon.GetCSharpPropText() + Environment.NewLine;
            }

            rtb.Text += "\n\t}\n} ";
        }

        private void saveToolStripButton_Click(object sender, EventArgs e)
        {
            if(tabControl1.TabPages.Count > 0)
            {
                FolderBrowserDialog fbd = new FolderBrowserDialog();
                fbd.SelectedPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                fbd.ShowNewFolderButton = true;
                fbd.Description = "C# class dosyalarının oluşturulacağı klasörü seçiniz.";

                if(fbd.ShowDialog() == DialogResult.OK)
                {
                    foreach (TabPage tp in tabControl1.TabPages)
                    {
                        RichTextBox rtb = tp.Controls[0] as RichTextBox;
                        File.WriteAllText(fbd.SelectedPath + "/" + tp.Text + ".cs", rtb.Text);
                    }

                    MessageBox.Show("Dosyalar oluşturuldu.");
                }
                
            }
        }
    }
}
