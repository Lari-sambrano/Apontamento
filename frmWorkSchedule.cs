using DAO;
using System;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;

namespace Apontamento.Forms
{
    public partial class FrmEscala : Form
    {
        //Instância do DAO responsável por persistir Escalas
        //readonly: não pode ser alterada após a inicialização
        private readonly EscalaDAO _escalaDAO = new EscalaDAO();
        //Editar Escala
        private int? _idEscalaEditando = null;
        public FrmEscala()
        {
            InitializeComponent();
        }


        #region Métodos Privados

        private void InicializarCamposHorario()
        {
            DateTime agora = DateTime.Now;

            dtEntrada.Value = agora;
            dtSaida.Value = agora;
            dtAlmocoInicio.Value = agora;
            dtAlmocoFim.Value = agora;
        }
      

        private void CarregarCombos()
        {
            //Tipos de escala disponíveis
            cbTipoEscala.Items.AddRange(new[] { "HC", "FC", "MD" });

            //Combo de DayOff 1 recebe todos os valores do enum DayOfWeek
            cbDayOff1.DataSource = Enum.GetValues(typeof(DayOfWeek));
            //Combo de DayOff 2 recebe todos os valores do enum DayOfWeek
            cbDayOff2.DataSource = Enum.GetValues(typeof(DayOfWeek));

            //Primeiro item: nenhum domingo de folga
            cbDomingoOff.Items.Add("Nenhum");

            //Adiciona opções de 1º ao 5º domingo de folga
            for (int i = 1; i <= 5; i++)
                cbDomingoOff.Items.Add($"{i}º Domingo");

            // Define "Nenhum" como seleção padrão
            cbDomingoOff.SelectedIndex = 0;
        }

        //Carrega as escalas cadastradas no grid
        private void CarregarEscalas()
        {
            //Busca no banco e atribui diretamente ao DataGridView
            var lista = _escalaDAO.Listar().Select(e => new
            {
                e.id_escala,
                e.descricao,
                e.hora_entrada,
                e.hora_almoco_inicio,
                e.hora_almoco_fim,
                e.hora_saida,
                e.tipo_escala,

                // valores crus p/ edição
                dayoff_1 = e.dayoff_1,
                dayoff_2 = e.dayoff_2,
                domingo_off = e.domingo_off,

                // valores “bonitos” p/ grid
                DayOff1 = ((DayOfWeek)e.dayoff_1).ToString(),
                DayOff2 = ((DayOfWeek)e.dayoff_2).ToString(),
                DomingoOff = e.domingo_off.HasValue ? $"{e.domingo_off}º Domingo" : "Nenhum",

                e.id_gestor
            }).ToList();

            grdEscalas.DataSource = lista;

            grdEscalas.Columns["id_escala"].Visible = false;
            grdEscalas.Columns["tipo_escala"].Visible = false;
            grdEscalas.Columns["id_gestor"].Visible = false;

            // esconda os crus também
            grdEscalas.Columns["dayoff_1"].Visible = false;
            grdEscalas.Columns["dayoff_2"].Visible = false;
            grdEscalas.Columns["domingo_off"].Visible = false;

            grdEscalas.Columns["DayOff1"].HeaderText = "Day-Off 1";
            grdEscalas.Columns["DayOff2"].HeaderText = "Day-Off 2";
            grdEscalas.Columns["descricao"].HeaderText = "Descrição";
            grdEscalas.Columns["hora_entrada"].HeaderText = "Hora Entrada";
            grdEscalas.Columns["hora_almoco_inicio"].HeaderText = "Saída\nalmoço";
            grdEscalas.Columns["hora_almoco_fim"].HeaderText = "Retorno almoço";
            grdEscalas.Columns["hora_saida"].HeaderText = "Hora Saída";
            grdEscalas.Columns["DomingoOff"].HeaderText = "Domingo Off";
        }

        //Valida os campos obrigatórios antes de salvar
        private bool ValidarCampos()
        {
            //String que acumula mensagens de campos inválidos
            string campos = string.Empty;
            // Flag de retorno (inicia como verdadeiro, altera para falso caso algum campo não seja preenchido/selecionado)
            bool flag = true;

            if (string.IsNullOrWhiteSpace(txtDescricao.Text))
            campos += "- Descrição\n";

            if (cbTipoEscala.SelectedIndex == -1)
            campos += "- Tipo de Escala\n";

            //Verifica se os dois Day Offs são iguais
            if (cbDayOff1.SelectedItem.Equals(cbDayOff2.SelectedItem))
            {
                MessageBox.Show("Os Day Offs não podem ser iguais.");
                return false;
            }

            //Se houver campos inválidos, exibe mensagem
            if (!string.IsNullOrEmpty(campos))
            {
                MessageBox.Show("Os seguintes campos são obrigatórios:\n" + campos);
                return false;
            }

            //Retorna o estado atual da flag
            return flag;
        }
        #endregion


        private void FrmEscala_Load(object sender, EventArgs e)
        {
            InicializarCamposHorario();
            //Carrega zerado os horários
           // ConfigurarDataTime();
            //Preenche as Combos
            CarregarCombos();
            //Carrega as escalas existentes no grid
            CarregarEscalas();

            //
            grdEscalas.CellClick += grdEscalas_CellClick;
        }
        private void btnSalvar_Click(object sender, EventArgs e)
        {
            //Valida campos antes de prosseguir
            if (!ValidarCampos())
                return;

            //Cria o objeto de escala com os dados da tela
            var escala = new tb_escala
            {
                //Descrição da escala
                descricao = txtDescricao.Text,

                //Horários (TimeOfDay extrai apenas a hora)
                hora_entrada = dtEntrada.Value.TimeOfDay,
                hora_almoco_inicio = dtAlmocoInicio.Value.TimeOfDay,
                hora_almoco_fim = dtAlmocoFim.Value.TimeOfDay,
                hora_saida = dtSaida.Value.TimeOfDay,

                //Tipo de escala selecionado
                tipo_escala = cbTipoEscala.Text,

                //Day Off 1 convertido para int
                dayoff_1 = (int)(DayOfWeek)cbDayOff1.SelectedItem,
                //Day Off 2 convertido para int
                dayoff_2 = (int)(DayOfWeek)cbDayOff2.SelectedItem,
                //Domingo Off: - Index 0 = Nenhum (null) - Demais índices = número do domingo
                domingo_off = cbDomingoOff.SelectedIndex == 0 ? (int?)null : cbDomingoOff.SelectedIndex,

                ativo = true,
                //Gestor logado
                id_gestor = Core.UtilCORE.CodigoLogado
            };

            //Insere a escala no banco de dados
            _escalaDAO.Inserir(escala);
            //Mensagem de sucesso
            MessageBox.Show("Escala salva com sucesso!");
            //Recarrega o grid
            CarregarEscalas();
        }

        private void grdEscalas_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;

            var row = grdEscalas.Rows[e.RowIndex];

            _idEscalaEditando = Convert.ToInt32(row.Cells["id_escala"].Value);

            txtDescricao.Text = Convert.ToString(row.Cells["descricao"].Value);

            // horários: as colunas são TimeSpan
            dtEntrada.Value = DateTime.Today.Add((TimeSpan)row.Cells["hora_entrada"].Value);
            dtAlmocoInicio.Value = DateTime.Today.Add((TimeSpan)row.Cells["hora_almoco_inicio"].Value);
            dtAlmocoFim.Value = DateTime.Today.Add((TimeSpan)row.Cells["hora_almoco_fim"].Value);
            dtSaida.Value = DateTime.Today.Add((TimeSpan)row.Cells["hora_saida"].Value);

            cbTipoEscala.SelectedItem = Convert.ToString(row.Cells["tipo_escala"].Value);

            // dayoff crus (int) => selecionar enum no combo
            cbDayOff1.SelectedItem = (DayOfWeek)Convert.ToInt32(row.Cells["dayoff_1"].Value);
            cbDayOff2.SelectedItem = (DayOfWeek)Convert.ToInt32(row.Cells["dayoff_2"].Value);

            // domingo_off: null => index 0, senão index = valor
            var dom = row.Cells["domingo_off"].Value;
            if (dom == null || dom == DBNull.Value)
                cbDomingoOff.SelectedIndex = 0;
            else
                cbDomingoOff.SelectedIndex = Convert.ToInt32(dom);

            btnSalvar.Text = "Atualizar";
        }
    }
}
