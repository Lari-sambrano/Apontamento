using Apontamento.Comum;
using DAO;
using DAO.VO;
using System;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using static Apontamento.Comum.Util;

namespace Apontamento
{
    public partial class frmGcm : Form
    {

        public frmGcm()
        {
            InitializeComponent();
        }

        private void frmGcm_Load(object sender, EventArgs e)
        {

            CarregarGrid();
            CarregarCombo();

            LimparCampos();
            txtValor.Text = "0,00";
        }
        private void cbGcm_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cbGcm.SelectedIndex == -1)
                return;

            if (cbGcm.SelectedItem is GcmVO gcm)
            {
                txtValor.Text = (gcm.valor_hora ?? 0m).ToString("N2");
            }
        }
        private void btnSalvar_Click(object sender, EventArgs e)
        {
            if (!ValidarCampos())
                return;

            int idGcm = Convert.ToInt32(cbGcm.SelectedValue);

            decimal valorHora;
            if (!decimal.TryParse(txtValor.Text, NumberStyles.Number, new CultureInfo("pt-BR"), out valorHora))
            {
                MessageBox.Show("Valor inválido.", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var dao = new GcmDAO();
            dao.AtualizarValorHora(idGcm, valorHora);

            MessageBox.Show("Valor atualizado com sucesso!", "Sucesso", MessageBoxButtons.OK, MessageBoxIcon.Information);

            CarregarGrid();
            CarregarCombo();
            LimparCampos();
        }

        #region Métodos Privados
        private void CarregarGrid()
        {
            var dao = new GcmDAO();

            grdGcm1.DataSource = dao.Listar();

            grdGcm1.Columns["id_gcm"].Visible = false;
            grdGcm1.Columns["descricao"].HeaderText = "GCM";
            grdGcm1.Columns["valor_hora"].HeaderText = "Valor Hora";
            grdGcm1.Columns["tb_colaborador"].Visible = false;
            grdGcm1.Columns["tb_gestor"].Visible = false;

        }
        private void CarregarCombo()
        {
            var dao = new GcmDAO();

            cbGcm.DataSource = dao.ListarParaCombo();
            cbGcm.DisplayMember = "descricao";
            cbGcm.ValueMember = "id_gcm";
            cbGcm.SelectedIndex = -1;
        }

        private void LimparCampos()
        {
            cbGcm.SelectedIndex = -1;
            txtValor.Clear();
        }

        private bool ValidarCampos()
        {
            bool flag = true;
            if (cbGcm.SelectedIndex == -1)
            {
                MessageBox.Show("Selecione uma GCM.", "Atenção", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }
            if (string.IsNullOrWhiteSpace(txtValor.Text))
            {
                MessageBox.Show("Informe o valor hora.", "Atenção", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            return flag;
        }

        #endregion

       
    }
}

