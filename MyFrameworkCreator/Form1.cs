﻿using AdoNetHelper;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
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

                    foreach (DataRow dr in dt.Rows)
                    {
                        rtb.Text += dr["column_name"].ToString() + Environment.NewLine;
                    }

                    tp.Controls.Add(rtb);
                    tabControl1.TabPages.Add(tp);
                }
            }
        }
    }
}
