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
    public partial class frmLogin : Form
    {
        public frmLogin()
        {
            InitializeComponent();
        }

        private void btnLogin_Click(object sender, EventArgs e)
        {
            if (ValidarCampos())
            {
                //Continua o código

            }
        }

        #region MÉTODOS
        private bool ValidarCampos()
        {
            //Se mudar para false, algum campo não está preenchido
            bool flag = true;
            //serve para armazanar os Nomes dos campos obg vazios!
            string campos = string.Empty;

            if(txtDas.Text.Trim() == string.Empty)
            {
                flag = false;
                //\n para quebrar a linha entre os campos de alerta
                campos = "\n- DAS";
            }
            
            if(txtPassword.Text.Trim() == string.Empty)
            {
                flag = false;
                //para pegar o valor dela + o da vez
                campos +=  "\n- Password";
            }

            //Se não é verdadeiro
            if (!flag)
            {
                MessageBox.Show($"Preencher o(s) campos(s): {campos}", "Atenção", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }



            return flag;

        }
        #endregion

        private void frmLogin_Load(object sender, EventArgs e)
        {

        }
    }
}
