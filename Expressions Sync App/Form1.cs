using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Tridion.Extensions.ContextExpressions;

namespace Expressions_Sync_App {
    public partial class Form1 : Form {
        public Form1() {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e) {
            string folder = txtFolder.Text;

            using (TargetGroupSynchronizer tgSync = new TargetGroupSynchronizer("tcm:3-66-2")) {
                string name = txtExpressionName.Text;
                string expression = txtExpression.Text;

                
                tgSync.SyncTargetGroup(name, "v1", expression);
                MessageBox.Show(this, "Expression successfully created", "Expressions", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
    }
}
