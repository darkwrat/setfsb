using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace SetFSB {
    public partial class Form1 : Form {
        private readonly List<Pll> pll;
        private Pll CurrentPll;
        public Form1(List<Pll> pll ){
            this.pll = pll;
            
            
            InitializeComponent();
            comboBox1.Items.Clear();
            foreach (var p in pll){
                comboBox1.Items.Add(p.GetType());
            }
            
            
           
        }

        private void button1_Click(object sender, EventArgs e){
            textBoxCurrentFSB.Text = "";
            Refresh();
            var fsb = CurrentPll.GetFSB();
            //
                textBoxCurrentFSB.Text = fsb.ToString();
            if (fsb > 0){    
            button2.Enabled = true;
            }

        }

        private void trackBarFSBs_Scroll(object sender, EventArgs e) {
            if(CurrentPll != null)
            textBoxNewFSB.Text = CurrentPll.SupportedFSBs[trackBarFSBs.Value].ToString();
        }

        private void button2_Click(object sender, EventArgs e) {
            var fsb = int.Parse(textBoxNewFSB.Text);
            if (fsb > 0) CurrentPll.SetFSB(fsb);
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e) {
            foreach (var p in pll ){
                if(p.GetType().ToString()  ==  comboBox1.Text ){
                    CurrentPll = p;
                    trackBarFSBs.Minimum = 0;
                    trackBarFSBs.Maximum = CurrentPll.SupportedFSBs.Count -1 ;
                    return;
                }
            }
        }
    }
}
