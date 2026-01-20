using Apontamento.Forms;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Apontamento
{
    public partial class frmPrincipal : Form
    {
        public frmPrincipal()
        {
            InitializeComponent();
        }

        private void cadastroDeColaboradorToolStripMenuItem_Click(object sender, EventArgs e)
        {
            new frmCadastroColaborador().ShowDialog();
        }

        private void apontamentoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            new frmTimeSheet().ShowDialog();
        }



        private void sairToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void escalasEspeciaisToolStripMenuItem_Click(object sender, EventArgs e)
        {
            new FrmEscala().ShowDialog();
        }

        private void gcmToolStripMenuItem_Click(object sender, EventArgs e)
        {
            new frmGcm().ShowDialog();  
        }
    }
}
