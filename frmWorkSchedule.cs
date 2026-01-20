using System;
using System.Linq;
using System.Windows.Forms;
using DAO;

namespace Apontamento.Forms
{
    public partial class FrmEscala : Form
    {
        //Instância do DAO responsável por persistir Escalas
        //readonly: não pode ser alterada após a inicialização
        private readonly EscalaDAO _escalaDAO = new EscalaDAO();

        public FrmEscala()
        {
            InitializeComponent();
        }

        #region Métodos Privados

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
            grdEscalas.DataSource = _escalaDAO.Listar().Select(e => new
            {
                e.id_escala,
                e.descricao,
                e.hora_entrada,
                e.hora_almoco_inicio,
                e.hora_almoco_fim,
                e.hora_saida,
                e.tipo_escala,
                DayOff1 = ((DayOfWeek)e.dayoff_1).ToString(),
                DayOff2 = ((DayOfWeek)e.dayoff_2).ToString(),
                DomingoOff = e.domingo_off.HasValue ? $"{e.domingo_off}º Domingo" : "Nenhum",
                e.id_gestor,
            }).ToList();
            
            grdEscalas.Columns["id_escala"].Visible = false;
            grdEscalas.Columns["tipo_escala"].Visible = false;
            grdEscalas.Columns["id_gestor"].Visible = false;

            grdEscalas.Columns["DayOff1"].HeaderText = "Day-Off 1";
            grdEscalas.Columns["DayOff2"].HeaderText = "Day-Off 2";
            grdEscalas.Columns["descricao"].HeaderText = "Descrição";
            grdEscalas.Columns["hora_entrada"].HeaderText = "Hora Entrada";
            grdEscalas.Columns["hora_almoco_inicio"].HeaderText = "Almoço\nSaída";
            grdEscalas.Columns["hora_almoco_fim"].HeaderText = "Almoço\nRetorno";
            grdEscalas.Columns["hora_saida"].HeaderText = "Hora\nSaída";
            grdEscalas.Columns["DomingoOff"].HeaderText = "Domingo\nOff";

            grdEscalas.Columns["hora_almoco_inicio"].HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
            grdEscalas.Columns["hora_almoco_fim"].HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
            grdEscalas.Columns["hora_entrada"].HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
            grdEscalas.Columns["hora_saida"].HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
            grdEscalas.Columns["DomingoOff"].HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;

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
            //Preenche as Combos
            CarregarCombos();
            //Carrega as escalas existentes no grid
            CarregarEscalas();
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


    }
}
