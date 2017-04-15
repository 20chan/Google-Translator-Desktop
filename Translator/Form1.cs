using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using RavSoft.GoogleTranslator;

namespace Translator
{
    public partial class Form1 : Form
    {
        private GTranslator t;
        public Form1()
        {
            InitializeComponent();
            var c = new CheckBox()
            {
                Text = "항상 최상위",
                Checked = true,
                BackColor = menuStrip1.BackColor
            };
            c.CheckedChanged += (s, ch) => this.TopMost = c.Checked;
            this.TopMost = true;
            this.menuStrip1.Items.Add(new ToolStripControlHost(c));

            t = new GTranslator();
        }

        private void textBox1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                textBox2.Text = t.Translate(textBox1.Text, "English", "Korean");
            }
            else if (e.KeyCode == Keys.Escape)
                textBox1_Click(null, null);
        }

        private void textBox1_Click(object sender, EventArgs e)
        {
            textBox1.Text = "";
        }
    }
}
