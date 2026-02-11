using Apontamento.Comum;
using DAO;
using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using static Apontamento.Comum.Util;

namespace Apontamento
{
    public partial class frmCadastroColaborador : Form
    {
        // Guarda o ID do colaborador selecionado na grid
        private int _idSelecionado = 0;

        public frmCadastroColaborador()
        {
            InitializeComponent();
        }

        #region EVENTOS

        private void frmCadastroColaborador_Load(object sender, EventArgs e)
        {
            //Preenche combo de operações
            CarregarOperacoes();
            //Preenche combo de cargos
            CarregarCargos();
            //Preenche combo de escalas
            CarregarEscalas();
            //Preenche combo de GCMs
            CarregarGcms();
            //Configura aparência e comportamento da grid
            ConfigurarGrid();
            //Carrega dados na grid
            CarregarGrid();

            Util.ConfigurarBotoesTela(EstadoTela.Nova, btnCadastrar, btnAlterar, btnExcluir);
        }

        private void btnCadastrar_Click(object sender, EventArgs e)
        {
            //Valida os campos obrigatórios
            if (!ValidarCampos())
                return;

            //Cria objeto colaborador com dados da tela
            var objColaborador = new tb_colaborador
            {
                nome_colaborador = txtNome.Text,
                id_operacao = (int)cbOperacao.SelectedValue,
                id_cargo = (int)cbCargo.SelectedValue,
                id_escala = (int)cbEscala.SelectedValue,
                id_gcm = (int)cbGcm.SelectedValue,
                status_colaborador = chkAtivo.Checked,

                //Define o gestor logado como dono do colaborador
                id_gestor = Core.UtilCORE.CodigoLogado
            };

            //Salva o colaborador no banco
            new ColaboradorDAO().Cadastrar(objColaborador);

            //Mensagem de sucesso
            MessageBox.Show("Colaborador cadastrado com sucesso!",
                "Sucesso", MessageBoxButtons.OK, MessageBoxIcon.Information);

            //Limpa os campos da tela
            LimparCampos();
            //Recarrega a grid
            CarregarGrid();
            //Volta os botões para o estado de tela "Novo"
            Util.ConfigurarBotoesTela(EstadoTela.Nova, btnCadastrar, btnAlterar, btnExcluir);
        }

        private void btnAlterar_Click(object sender, EventArgs e)
        {
            if (!ValidarCampos())
                return;

            //Cria objeto com ID recuperado da linha selecionada na grid
            var objColaborador = new tb_colaborador
            {
                id_colaborador = _idSelecionado,
                nome_colaborador = txtNome.Text,
                id_operacao = (int)cbOperacao.SelectedValue,
                id_cargo = (int)cbCargo.SelectedValue,
                id_escala = (int)cbEscala.SelectedValue,
                id_gcm = (int)cbGcm.SelectedValue,
                status_colaborador = chkAtivo.Checked
            };

            //CAPTURA o retorno do MessageBox
            DialogResult resposta = MessageBox.Show("Deseja alterar as informações?", "Atenção",
                                               MessageBoxButtons.YesNo, MessageBoxIcon.Question);


            if (resposta == DialogResult.Yes)
            {
                //Atualiza no banco
                new ColaboradorDAO().Alterar(objColaborador);

                //Atualiza grid e limpa a tela
                CarregarGrid();
                LimparCampos();

                //Volta os botões para o estado de tela "Novo"
                Util.ConfigurarBotoesTela(EstadoTela.Nova, btnCadastrar, btnAlterar, btnExcluir);
            }

        }
        private void btnExcluir_Click(object sender, EventArgs e)
        {
            //Confirmação antes de excluir/inativar
            DialogResult resposta = MessageBox.Show("Deseja realmente excluir este colaborador?", "Atenção",
                                                            MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

            //Se o usuário cancelar, não faz nada
            if (resposta != DialogResult.Yes)
                return;

            //Inativa o colaborador pelo ID recuperado da grid (linha selecionada)
            new ColaboradorDAO().Inativar(_idSelecionado);

            //Atualiza a tela e limpa os campos, caso haja algo neles
            CarregarGrid();
            LimparCampos();
            //Volta os botões para o estado de tela "Novo"
            Util.ConfigurarBotoesTela(EstadoTela.Nova, btnCadastrar, btnAlterar, btnExcluir);
        }

        private void btnCancelar_Click(object sender, EventArgs e)
        {
            //Limpa os campos
            LimparCampos();
            //Volta os botões para o estado de tela "Novo"
            Util.ConfigurarBotoesTela(EstadoTela.Nova, btnCadastrar, btnAlterar, btnExcluir);
        }

        private void grdColaboradores_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            //Ignora clique no cabeçalho
            if (e.RowIndex < 0) return;
            //Recupera o ID escondido da grid
            _idSelecionado = Convert.ToInt32(grdColaboradores.Rows[e.RowIndex].Cells["Id"].Value);

            //Busca o colaborador completo no banco
            var col = new ColaboradorDAO().BuscarPorId(_idSelecionado);

            //Preenche os campos da tela
            txtNome.Text = col.nome_colaborador;
            cbOperacao.SelectedValue = col.id_operacao;
            cbCargo.SelectedValue = col.id_cargo;
            cbEscala.SelectedValue = col.id_escala;
            cbGcm.SelectedValue = col.id_gcm;

            chkAtivo.Checked = col.status_colaborador;

            //Muda os botões para estado de tela "Edição"
            Util.ConfigurarBotoesTela(EstadoTela.Edicao, btnCadastrar, btnAlterar, btnExcluir);
        }

        #endregion

        #region MÉTODOS

        private bool ValidarCampos()
        {
            string campos = string.Empty;
            bool flag = true;

            if (string.IsNullOrWhiteSpace(txtNome.Text))
                campos += "- Nome\n";

            if (cbOperacao.SelectedIndex < 0)
                campos += "- Operação\n";

            if (cbCargo.SelectedIndex < 0)
                campos += "- Cargo\n";

            if (cbEscala.SelectedIndex < 0)
                campos += "- Escala\n";

            if (cbGcm.SelectedIndex < 0)
                campos += "- GCM\n";

            //Se houver campos inválidos, exibe mensagem
            if (!string.IsNullOrEmpty(campos))
            {
                Util.MostrarMensagem(Util.TipoMensagem.Obrigatorio, campos);
                return false;
            }

            //Retorna o estado da validação
            return flag;
        }

        private void LimparCampos()
        {
            txtNome.Clear();
            cbOperacao.SelectedIndex = -1;
            cbCargo.SelectedIndex = -1;
            cbEscala.SelectedIndex = -1;
            cbGcm.SelectedIndex = -1;
            chkAtivo.Checked = true;

            _idSelecionado = 0;
        }

        //Carrega a combo de operações
        private void CarregarOperacoes()
        {
            cbOperacao.DataSource = new OperacaoDAO().ListarAtivas();
            cbOperacao.DisplayMember = "nome_operacao";
            cbOperacao.ValueMember = "id_operacao";
            cbOperacao.SelectedIndex = -1;
        }
        //Carrega a combo de cargos
        private void CarregarCargos()
        {
            cbCargo.DataSource = new CargoDAO().Listar();
            cbCargo.DisplayMember = "descricao";
            cbCargo.ValueMember = "id_cargo";
            cbCargo.SelectedIndex = -1;
        }
        //Carrega a combo de escalas
        private void CarregarEscalas()
        {
            cbEscala.DataSource = new EscalaDAO().ListarSemImportadas();
            cbEscala.DisplayMember = "descricao";
            cbEscala.ValueMember = "id_escala";
            cbEscala.SelectedIndex = -1;
        }
        //Carrega a combo de GCMs
        private void CarregarGcms()
        {
            cbGcm.DataSource = new GcmDAO().Listar();
            cbGcm.DisplayMember = "descricao";
            cbGcm.ValueMember = "id_gcm";
            cbGcm.SelectedIndex = -1;
        }

        private void CarregarGrid()
        {
            //Consulta os colaboradores do gestor logado
            var lista = new ColaboradorDAO().Consultar(Core.UtilCORE.CodigoLogado)
                .Select(c => new
                {
                    //Esse id fica oculto na grid, mas é usado para recuperar o colaborador ao clicar na linha
                    Id = c.id_colaborador,
                    Colaborador = c.nome_colaborador,
                    Operacao = c.tb_operacao.nome_operacao,
                    Cargo = c.tb_cargo.descricao,
                    Escala = c.tb_escala.descricao,
                    GCM = c.tb_gcm.descricao
                })
                .ToList();

            grdColaboradores.AutoGenerateColumns = true;
            grdColaboradores.DataSource = lista;

            //Oculta a coluna ID
            grdColaboradores.Columns["Id"].Visible = false;

            //Ajusta colunas automaticamente
            grdColaboradores.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        }

        //Configura aparência e comportamento da grid
        private void ConfigurarGrid()
        {
            grdColaboradores.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            grdColaboradores.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;
            grdColaboradores.RowHeadersVisible = false;

            grdColaboradores.AllowUserToResizeColumns = false;
            grdColaboradores.AllowUserToResizeRows = false;

            grdColaboradores.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            grdColaboradores.MultiSelect = false;
            grdColaboradores.ReadOnly = true;

            grdColaboradores.BackgroundColor = Color.White;
            grdColaboradores.BorderStyle = BorderStyle.FixedSingle;
        }

        #endregion
    }
}
