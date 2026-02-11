using Apontamento.Comum;
using ClosedXML.Excel;
using Core;
using Core.Enums;
using Core.VO;
using DAO;
using System;
using System.Data;
using System.Collections.Generic;
using System.Data.Entity.Validation;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Text.RegularExpressions;

namespace Apontamento
{
    public partial class frmTimeSheet : Form
    {
        //
        private List<Core.VO.TimeSheetMensalLinha> _linhas = new List<Core.VO.TimeSheetMensalLinha>();
        private bool _carregando = false;
        //
        private const int COLS_FIXAS = 9;
        //Lista de dias do timesheet carregado
        private List<TimeSheetDiaVO> _dias = new List<TimeSheetDiaVO>();
        //Guarda WorkType + horas UA/ML por (colaborador, data)
        private readonly Dictionary<(int idColab, DateTime data), DiaState> _state = new Dictionary<(int, DateTime), DiaState>();
        //Cache do mês por colaborador (evita recalcular muitas vezes no salvar)
        private readonly Dictionary<int, List<TimeSheetDiaVO>> _cacheDiasMes = new Dictionary<int, List<TimeSheetDiaVO>>();
        private HashSet<DateTime> _feriadosMes = new HashSet<DateTime>();
        //Drag
        private bool _isDragging = false;
        private WorkType? _tipoSelecionado = null;
        private sealed class DiaState
        {
            public WorkType Tipo { get; set; }
            public decimal HorasUAouML { get; set; }
        }

        //Classe para identificar cadastro pendente
        private readonly Dictionary<int, PendenteCadastro> _pendentes = new Dictionary<int, PendenteCadastro>();

        private sealed class PendenteCadastro
        {
            public string Analyst { get; set; }
            public string Operation { get; set; }
            public string DayOff1 { get; set; }
            public string DayOff2 { get; set; }
            public string SundayOff { get; set; }

            public TimeSpan? Entry { get; set; }
            public TimeSpan? LunchIni { get; set; }
            public TimeSpan? LunchFim { get; set; }
            public TimeSpan? End { get; set; }
        }
        private int _idColaborador;
        private readonly db_apontamento objBanco;


        public frmTimeSheet()
        {
            InitializeComponent();

        }

        #region LOAD
        //Desenha uma borda preta ao redor dos Panels
        private void Panel_Paint(object sender, PaintEventArgs e)
        {
            Panel p = sender as Panel;

            if (p == null) return;

            ControlPaint.DrawBorder(e.Graphics, p.ClientRectangle,
                Color.Black, ButtonBorderStyle.Solid);
        }
        private void frmTimeSheet_Load(object sender, EventArgs e)
        {
            //Configura o DateTimePicker para selecionar apenas mês/ano
            dtCompetencia.Format = DateTimePickerFormat.Custom;
            dtCompetencia.CustomFormat = "MM/yyyy";
            dtCompetencia.ShowUpDown = true;
            dtCompetencia.Value = DateTime.Now;

            ConfigurarMenuContextoWorkType();

            //grid começa vazia, mas com layout
            PrepararGridEstiloExcel(dtCompetencia.Value.Month, dtCompetencia.Value.Year);

            AtualizarLabelFeriados(dtCompetencia.Value.Month, dtCompetencia.Value.Year);

            grdTimesheet.CellValueChanged += grdTimesheet_CellValueChanged;
            ZerarLabels();

            grdTimesheet.CellClick += grdTimesheet_CellClick;
            grdTimesheet.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        }

        private void ZerarLabels()
        {
            lblHorasTrabalhadas.Text = "Horas trabalhadas: 0,00h";
            //lblBancoDeHoras.Text = "Banco de horas: 0,00h";
            lblAtestados.Text = "Atestados: 0,00h";
            lblValorReceber.Text = "Valor de custo ao projeto: R$ 0,00";
            lblHorasFeriado.Text = "Horas trabalhadas em feriados: 0,00h";
            lblPaidLeave.Text = "Paid Leave: 0,00h";
            lblUnpaidLeave.Text = "Unpaid Leave: 0,00h";
        }
        private void AtualizarLabelFeriados(int mes, int ano)
        {
            var feriados = UtilCORE.FeriadosBrasil.ObterFeriados(ano)
                .Where(d => d.Month == mes)
                .OrderBy(d => d)
                .ToList();

            lblFeriados.Text = feriados.Count == 0 ? "Feriados: (nenhum)" : "Feriados: " + string.Join(", ", feriados.Select(f => f.ToString("dd/MM")));
        }

        #endregion
        #region Config Load
        private void AtualizarLabelsDoColaboradorSelecionado()
        {
            if (grdTimesheet.CurrentCell == null) { ZerarLabels(); return; }

            int rowIndex = grdTimesheet.CurrentCell.RowIndex;
            if (rowIndex < 3) { ZerarLabels(); return; } // clicou no cabeçalho fake

            int idColab = Convert.ToInt32(grdTimesheet.Rows[rowIndex].Cells["ID_COLAB"].Value ?? 0);
            if (idColab <= 0) { ZerarLabels(); return; }

            if (!_cacheDiasMes.TryGetValue(idColab, out var diasMes))
            {
                ZerarLabels();
                return;
            }

            // Recalcula os dias a partir da grid/state e depois soma
            RecalcularDiasDoColaboradorPeloGrid(rowIndex, idColab, diasMes);

            _dias = diasMes;
            CalcularTotais();
        }

        private void dtCompetencia_ValueChanged(object sender, EventArgs e)
        {
            PrepararGridEstiloExcel(dtCompetencia.Value.Month, dtCompetencia.Value.Year);
            AtualizarLabelFeriados(dtCompetencia.Value.Month, dtCompetencia.Value.Year);
            ZerarLabels();
        }
        #endregion

        #region GRID
        //GRID 
        private void PrepararGridEstiloExcel(int mes, int ano)
        {
            _state.Clear();
            _cacheDiasMes.Clear();

            grdTimesheet.Columns.Clear();
            grdTimesheet.Rows.Clear();

            grdTimesheet.AllowUserToAddRows = false;
            grdTimesheet.RowHeadersVisible = false;
            grdTimesheet.SelectionMode = DataGridViewSelectionMode.CellSelect;
            grdTimesheet.MultiSelect = false;
            grdTimesheet.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;
            grdTimesheet.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;

            //Coluna ID oculto
            grdTimesheet.Columns.Add("ID_COLAB", "ID");
            grdTimesheet.Columns["ID_COLAB"].Visible = false;

            //Fixas
            grdTimesheet.Columns.Add("ANALYST", "ANALYST");
            grdTimesheet.Columns.Add("OPERATION", "OPERATION");
            grdTimesheet.Columns.Add("ENTRY_TIME", "ENTRY TIME");
            grdTimesheet.Columns.Add("LUNCH_TIME", "LUNCH TIME");
            grdTimesheet.Columns.Add("END_TIME", "END TIME");
            grdTimesheet.Columns.Add("DAYOFF1", "1º Day Off");
            grdTimesheet.Columns.Add("DAYOFF2", "2º Day Off");
            grdTimesheet.Columns.Add("SUNDAYOFF", "Sunday Off");

            int diasNoMes = DateTime.DaysInMonth(ano, mes);
            for (int d = 1; d <= diasNoMes; d++)
            {
                grdTimesheet.Columns.Add($"D{d:00}_S", ""); //Start
                grdTimesheet.Columns.Add($"D{d:00}_E", ""); //End
            }

            grdTimesheet.Columns["ANALYST"].Frozen = true;
            grdTimesheet.Columns["OPERATION"].Frozen = true;


            //3 linhas “header fake”
            CriarCabecalho3Linhas(mes, ano);
        }

        private void CriarCabecalho3Linhas(int mes, int ano)
        {
            grdTimesheet.Rows.Add();
            grdTimesheet.Rows.Add();
            grdTimesheet.Rows.Add();

            for (int i = 0; i <= 2; i++)
            {
                grdTimesheet.Rows[i].ReadOnly = true;
                grdTimesheet.Rows[i].DefaultCellStyle.BackColor = System.Drawing.Color.Gainsboro;
                grdTimesheet.Rows[i].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            }

            int diasNoMes = DateTime.DaysInMonth(ano, mes);

            for (int d = 1; d <= diasNoMes; d++)
            {
                DateTime data = new DateTime(ano, mes, d);
                int colStart = ColIndexDiaStart(d);

                grdTimesheet.Rows[0].Cells[colStart].Value = d.ToString();
                grdTimesheet.Rows[0].Cells[colStart + 1].Value = d.ToString();

                grdTimesheet.Rows[1].Cells[colStart].Value = data.DayOfWeek.ToString();
                grdTimesheet.Rows[1].Cells[colStart + 1].Value = data.DayOfWeek.ToString();

                grdTimesheet.Rows[2].Cells[colStart].Value = "Start";
                grdTimesheet.Rows[2].Cells[colStart + 1].Value = "End";
            }
        }

        private int ColIndexDiaStart(int d) => COLS_FIXAS + (d - 1) * 2;

        private void AplicarEscalaNosDiasDoMes(int rowIndex, int idColab, tb_escala escala, List<TimeSheetDiaVO> diasMes)
        {
            if (escala == null || diasMes == null) return;

            int mes = dtCompetencia.Value.Month;
            int ano = dtCompetencia.Value.Year;

            foreach (var dia in diasMes)
            {
                if (dia.Data.Month != mes || dia.Data.Year != ano) continue;

                // Descobre o WorkType do dia a partir do que está salvo
                ParseCodigoPersistente(dia.Codigo, out WorkType tipo, out decimal horasUAouML);

                bool deveSeguirEscala = tipo == WorkType.WD || tipo == WorkType.TR || tipo == WorkType.BT || tipo == WorkType.DO ||
                                        tipo == WorkType.AL || tipo == WorkType.DN || tipo == WorkType.ML || tipo == WorkType.UA ||
                                        tipo == WorkType.SL;

                bool naoTemHorario = tipo == WorkType.SC;

                int d = dia.Data.Day;
                int colStart = ColIndexDiaStart(d);

                if (naoTemHorario)
                {
                    // limpa na VO
                    dia.HoraEntrada = null;
                    dia.HoraSaida = null;

                    // limpa na grid
                    LimparStartEnd(rowIndex, colStart);
                }
                else if (deveSeguirEscala)
                {
                    // aplica na VO
                    dia.HoraEntrada = escala.hora_entrada;
                    dia.HoraSaida = escala.hora_saida;
                    dia.HoraAlmocoInicio = escala.hora_almoco_inicio;
                    dia.HoraAlmocoFim = escala.hora_almoco_fim;

                    // aplica na grid
                    grdTimesheet.Rows[rowIndex].Cells[colStart].Value = escala.hora_entrada.ToString(@"hh\:mm");
                    grdTimesheet.Rows[rowIndex].Cells[colStart + 1].Value = escala.hora_saida.ToString(@"hh\:mm");
                }

                // mantém o state consistente (para cores e export/salvar)
                _state[(idColab, dia.Data.Date)] = new DiaState { Tipo = tipo, HorasUAouML = horasUAouML };

                // pinta conforme tipo
                PintarCelulasDia(rowIndex, dia.Data.Date, tipo);
            }
        }

        //CARREGAR COLABORADORES DO BD (COM EXCEÇÕES + WORKTYPE) É O QUE PINTA AS CELULAS sobre os códigos
        private void CarregarColaboradoresNoGrid(int mes, int ano)
        {
            // remove linhas abaixo do cabeçalho fake (0..2)
            while (grdTimesheet.Rows.Count > 3)
                grdTimesheet.Rows.RemoveAt(grdTimesheet.Rows.Count - 1);

            _state.Clear();
            _cacheDiasMes.Clear();

            // feriados do mês
            _feriadosMes = UtilCORE.FeriadosBrasil.ObterFeriados(ano)
                .Where(d => d.Month == mes)
                .Select(d => d.Date)
                .ToHashSet();

            var colabDao = new ColaboradorDAO();
            var lista = colabDao.ListarAtivos(UtilCORE.CodigoLogado);

            var tsDao = new TimeSheetDAO();
            int diasNoMes = DateTime.DaysInMonth(ano, mes);

            foreach (var c in lista)
            {
                int rowIndex = grdTimesheet.Rows.Add();
                var row = grdTimesheet.Rows[rowIndex];

                // ==========================
                // Fixas
                // ==========================
                row.Cells["ID_COLAB"].Value = c.id_colaborador;
                row.Cells["ANALYST"].Value = c.nome_colaborador ?? "";
                row.Cells["OPERATION"].Value = c.tb_operacao?.nome_operacao ?? "";

                if (c.tb_escala != null)
                {
                    row.Cells["ENTRY_TIME"].Value = c.tb_escala.hora_entrada.ToString(@"hh\:mm");
                    row.Cells["LUNCH_TIME"].Value = $"{c.tb_escala.hora_almoco_inicio:hh\\:mm} - {c.tb_escala.hora_almoco_fim:hh\\:mm}";
                    row.Cells["END_TIME"].Value = c.tb_escala.hora_saida.ToString(@"hh\:mm");
                    row.Cells["DAYOFF1"].Value = ((DayOfWeek)c.tb_escala.dayoff_1).ToString();
                    row.Cells["DAYOFF2"].Value = ((DayOfWeek)c.tb_escala.dayoff_2).ToString();
                    row.Cells["SUNDAYOFF"].Value = c.tb_escala.domingo_off?.ToString() ?? "";
                }
                else
                {
                    row.Cells["ENTRY_TIME"].Value = "";
                    row.Cells["LUNCH_TIME"].Value = "";
                    row.Cells["END_TIME"].Value = "";
                    row.Cells["DAYOFF1"].Value = "";
                    row.Cells["DAYOFF2"].Value = "";
                    row.Cells["SUNDAYOFF"].Value = "";
                }

                // ==========================
                // Gera mês + exceções
                // ==========================
                var diasMes = tsDao.GerarMes(c.id_colaborador, mes, ano);
                tsDao.AplicarExcecoes(c.id_colaborador, diasMes);

                // marca feriado na VO
                foreach (var dia in diasMes)
                    dia.IsFeriado = _feriadosMes.Contains(dia.Data.Date);

                // guarda no cache
                _cacheDiasMes[c.id_colaborador] = diasMes;

                // ==========================
                // APLICA ESCALA NOS DIAS (mesmo com evento)
                // ==========================
                // -> isso é o que corrige seu problema (antes só mudava WD)
                if (c.tb_escala != null)
                    AplicarEscalaNosDiasDoMes(rowIndex, c.id_colaborador, c.tb_escala, diasMes);

                // ==========================
                // Preenche dias na grid + state + cores
                // ==========================
                foreach (var dia in diasMes)
                {
                    // código persistente (pode vir "ML|8" etc)
                    ParseCodigoPersistente(dia.Codigo, out WorkType tipo, out decimal horasUAouML);

                    _state[(c.id_colaborador, dia.Data.Date)] = new DiaState
                    {
                        Tipo = tipo,
                        HorasUAouML = horasUAouML
                    };

                    int d = dia.Data.Day;
                    int colStart = ColIndexDiaStart(d);

                    // horários na grid (já podem ter sido ajustados por AplicarEscalaNosDiasDoMes)
                    //row.Cells[colStart].Value = dia.HoraEntrada?.ToString(@"hh\:mm") ?? "";
                    //row.Cells[colStart + 1].Value = dia.HoraSaida?.ToString(@"hh\:mm") ?? "";
                    if(tipo == WorkType.DO){

                        row.Cells[colStart].Value = "";
                        row.Cells[colStart + 1].Value = "";
                    }
                   

                    // pinta conforme tipo (feriado tem prioridade dentro do PintarCelulasDia somente quando é WD)
                    PintarCelulasDia(rowIndex, dia.Data.Date, tipo);
                }
            }
        }

        //MENU CONTEXTO WORKTYPE + DRAG
        private void ConfigurarMenuContextoWorkType()
        {
            var menu = new ContextMenuStrip();

            foreach (WorkType t in Enum.GetValues(typeof(WorkType)))
            {
                var item = new ToolStripMenuItem(t.ToString());
                item.Click += (s, e) =>
                {
                    if (grdTimesheet.CurrentCell == null) return;

                    AplicarWorkTypeNaCelula(grdTimesheet.CurrentCell.RowIndex, grdTimesheet.CurrentCell.ColumnIndex, t);
                };
                menu.Items.Add(item);
            }

            menu.Items.Add(new ToolStripSeparator());

            var itemDrag = new ToolStripMenuItem("Ativar modo arrastar com este tipo");
            itemDrag.Click += (s, e) =>
            {
                if (grdTimesheet.CurrentCell == null) return;
                // pega o tipo do item atualmente selecionado no menu pelo texto do parent? (não dá)
                // então: para drag você seta manualmente no código / legenda.
                MessageBox.Show("Para usar drag: selecione um WorkType na legenda/código e arraste.\n" + "Você também pode clicar em um item do menu para aplicar em uma célula.");
            };
            menu.Items.Add(itemDrag);

            grdTimesheet.ContextMenuStrip = menu;
        }

        #endregion


        #region CORES
        //Aplica cores na grid conforme o WorkType
        //private void AplicarCoresNaGrid()
        //{
        //    foreach (DataGridViewRow row in grdTimesheet.Rows)
        //    {
        //        string codigo = row.Cells["colCodigo"].Value?.ToString();
        //        if (Enum.TryParse(codigo, out WorkType tipo))
        //            row.DefaultCellStyle.BackColor = Util.GetColor(tipo);
        //    }
        //}
        //Aplica cores na grid conforme o WorkType
        private void AplicarCoresNaGrid()
        {
            foreach (DataGridViewRow row in grdTimesheet.Rows)
            {
                var dia = row.DataBoundItem as TimeSheetDiaVO;
                if (dia == null) continue;

                // Feriado tem prioridade visual
                if (dia.IsFeriado)
                {
                    row.DefaultCellStyle.BackColor = Color.Gold;
                    row.Cells["colCodigo"].ToolTipText = "Feriado Nacional";
                    continue;
                }

                if (Enum.TryParse(dia.Codigo, out WorkType tipo))
                    row.DefaultCellStyle.BackColor = Util.GetColor(tipo);
            }
        }
        #endregion

        #region EDIÇÃO
        //Evento disparado quando o usuário termina de editar uma célula da grid
        private void grdTimesheet_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {

        }

        #endregion

        #region DRAG CÓDIGO
        //Início do arraste do código
        private void grdTimesheet_CellMouseDown(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.RowIndex < 3 || e.ColumnIndex < COLS_FIXAS) return;
            if (!_tipoSelecionado.HasValue) return;

            _isDragging = true;
            AplicarWorkTypeNaCelula(e.RowIndex, e.ColumnIndex, _tipoSelecionado.Value);
        }

        //Aplica o código enquanto o mouse passa pelas células
        private void grdTimesheet_CellMouseEnter(object sender, DataGridViewCellEventArgs e)
        {
            if (!_isDragging) return;
            if (!_tipoSelecionado.HasValue) return;
            if (e.RowIndex < 3 || e.ColumnIndex < COLS_FIXAS) return;

            AplicarWorkTypeNaCelula(e.RowIndex, e.ColumnIndex, _tipoSelecionado.Value);
        }

        //Finaliza o arraste
        private void grdTimesheet_CellMouseUp(object sender, DataGridViewCellMouseEventArgs e)
        {
            _isDragging = false;
        }
        //APLICAR WORKTYPE EM UM DIA (PAR START/END)
        private void AplicarWorkTypeNaCelula(int rowIndex, int colIndex, WorkType tipo)
        {
            // Ignora cabeçalho fake
            if (rowIndex < 3) return;

            // Só colunas de dia (Start/End)
            if (colIndex < COLS_FIXAS) return;

            // ID do colaborador na linha
            int idColab = Convert.ToInt32(grdTimesheet.Rows[rowIndex].Cells["ID_COLAB"].Value ?? 0);
            if (idColab <= 0) return;

            // Descobre o dia do mês baseado na coluna clicada
            int dia = DiaDoMesPorColuna(colIndex);
            if (dia <= 0) return;

            DateTime data = new DateTime(dtCompetencia.Value.Year, dtCompetencia.Value.Month, dia).Date;

            // Coluna Start do dia
            int colStart = ColIndexDiaStart(dia);

            // Garante state
            if (!_state.TryGetValue((idColab, data), out var st))
            {
                st = new DiaState { Tipo = WorkType.WD, HorasUAouML = 0m };
                _state[(idColab, data)] = st;
            }

            // =========================
            // Regras especiais por tipo
            // =========================

            if (tipo == WorkType.UA)
            {
                // Pergunta horas (você decide se é "1.5" ou "1.30")
                decimal horas = PerguntarHoras("Falta injustificada (UA)", data);
                st.HorasUAouML = horas;

                // UA normalmente não tem horário
                LimparStartEnd(rowIndex, colStart);
            }
            else if (tipo == WorkType.ML)
            {
                decimal horas = PerguntarHoras("Atestado médico (ML)", data);
                st.HorasUAouML = horas;

                // ML normalmente não tem horário
                LimparStartEnd(rowIndex, colStart);
            }
            else if (tipo == WorkType.SL)
            {
                // SL em cascata: do dia selecionado até o fim do mês
                AplicarSLCascata(rowIndex, idColab, data);
                return;
            }
            else
            {
                // Para qualquer outro WorkType, zera horas extras de UA/ML
                st.HorasUAouML = 0m;

                // Se quiser limpar horários em DO/AL também
                if (tipo == WorkType.DO || tipo == WorkType.AL || tipo == WorkType.DN)
                    LimparStartEnd(rowIndex, colStart);
            }

            // Atualiza tipo no state
            st.Tipo = tipo;

            // Pinta as células Start/End do dia
            PintarCelulasDia(rowIndex, data, tipo);

            // ⚠️ Não atualiza labels aqui (você pediu para atualizar só ao clicar no nome).
            // Se amanhã você quiser atualizar automaticamente quando editar o colaborador selecionado,
            // a gente liga isso aqui com uma checagem do "id selecionado".

        }
        private decimal PerguntarHoras(string titulo, DateTime data)
        {
            using (Form dialog = new Form())
            {
                dialog.Text = titulo;
                dialog.StartPosition = FormStartPosition.CenterParent;
                dialog.FormBorderStyle = FormBorderStyle.FixedDialog;
                dialog.Width = 320;
                dialog.Height = 160;
                dialog.MaximizeBox = false;
                dialog.MinimizeBox = false;

                Label lbl = new Label
                {
                    Text = $"Informe as horas\nData: {data:dd/MM/yyyy}",
                    Left = 10,
                    Top = 10,
                    Width = 280
                };

                TextBox txtHoras = new TextBox
                {
                    Left = 10,
                    Top = 50,
                    Width = 280,
                    Text = "0"
                };

                Button btnOk = new Button
                {
                    Text = "OK",
                    Left = 130,
                    Top = 80,
                    DialogResult = DialogResult.OK
                };

                dialog.Controls.Add(lbl);
                dialog.Controls.Add(txtHoras);
                dialog.Controls.Add(btnOk);
                dialog.AcceptButton = btnOk;

                if (dialog.ShowDialog(this) == DialogResult.OK)
                {
                    if (decimal.TryParse(txtHoras.Text.Replace(",", "."), NumberStyles.Any, CultureInfo.InvariantCulture, out decimal horas))
                    {
                        return horas < 0 ? 0 : horas;
                    }
                }

                return 0m;
            }
        }

        private void AtualizarCacheELabelsAposMudanca(int rowIndex, int idColab, DateTime data)
        {
            if (!_cacheDiasMes.TryGetValue(idColab, out var diasMes))
                return;

            RecalcularUmDiaPeloGrid(rowIndex, idColab, diasMes, data.Day);

            if (GetIdColaboradorSelecionado() == idColab)
            {
                _dias = diasMes;
                CalcularTotais();
            }
        }
        private int GetIdColaboradorSelecionado()
        {
            if (grdTimesheet.CurrentCell == null) return 0;
            int r = grdTimesheet.CurrentCell.RowIndex;
            if (r < 3) return 0;
            return Convert.ToInt32(grdTimesheet.Rows[r].Cells["ID_COLAB"].Value);
        }

        private void RecalcularDiasDoColaboradorPeloGrid(int rowIndex, int idColab, List<TimeSheetDiaVO> diasMes)
        {
            int mes = dtCompetencia.Value.Month;
            int ano = dtCompetencia.Value.Year;

            var dao = new TimeSheetDAO();
            int diasNoMes = DateTime.DaysInMonth(ano, mes);

            for (int d = 1; d <= diasNoMes; d++)
                RecalcularUmDiaPeloGrid(rowIndex, idColab, diasMes, d);
        }

        private void RecalcularUmDiaPeloGrid(int rowIndex, int idColab, List<TimeSheetDiaVO> diasMes, int diaDoMes)
        {
            int mes = dtCompetencia.Value.Month;
            int ano = dtCompetencia.Value.Year;

            var dao = new TimeSheetDAO();

            DateTime data = new DateTime(ano, mes, diaDoMes).Date;

            int colStart = ColIndexDiaStart(diaDoMes);
            string startTxt = grdTimesheet.Rows[rowIndex].Cells[colStart].Value?.ToString();
            string endTxt = grdTimesheet.Rows[rowIndex].Cells[colStart + 1].Value?.ToString();

            var vo = diasMes.FirstOrDefault(x => x.Data.Date == data);
            if (vo == null) return;

            vo.HoraEntrada = ParseHora(startTxt);
            vo.HoraSaida = ParseHora(endTxt);

            // código do state
            if (_state.TryGetValue((idColab, data), out var st))
                vo.Codigo = st.Tipo.ToString();
            else
                vo.Codigo = WorkType.WD.ToString();

            // feriado
            vo.IsFeriado = _feriadosMes.Contains(data);

            // >>> IMPORTANTE:
            // No seu DAO, ML usa "horasAusenciaUA" por engano no parâmetro.
            // Então aqui passamos st.HorasUAouML para ambos os casos.
            decimal horasUAouML = (_state.TryGetValue((idColab, data), out st)) ? st.HorasUAouML : 0m;

            dao.CalcularDia(idColab, vo, horasAusenciaUA: horasUAouML, horasAtestado: horasUAouML);

            PintarCelulasDia(rowIndex, data, st != null ? st.Tipo : WorkType.WD);
        }
        private int DiaDoMesPorColuna(int colIndex)
        {
            int offset = colIndex - COLS_FIXAS;
            return (offset / 2) + 1;
        }

        private void LimparStartEnd(int rowIndex, int colStart)
        {
            grdTimesheet.Rows[rowIndex].Cells[colStart].Value = "";
            grdTimesheet.Rows[rowIndex].Cells[colStart + 1].Value = "";
        }

        private void AplicarSLCascata(int rowIndex, int idColab, DateTime dataInicio)
        {
            int mes = dtCompetencia.Value.Month;
            int ano = dtCompetencia.Value.Year;
            int diasNoMes = DateTime.DaysInMonth(ano, mes);

            for (int d = dataInicio.Day; d <= diasNoMes; d++)
            {
                DateTime data = new DateTime(ano, mes, d).Date;

                if (!_state.TryGetValue((idColab, data), out var st))
                {
                    st = new DiaState();
                    _state[(idColab, data)] = st;
                }

                st.Tipo = WorkType.SL;
                st.HorasUAouML = 0m;

                int colStart = ColIndexDiaStart(d);
                LimparStartEnd(rowIndex, colStart);

                PintarCelulasDia(rowIndex, data, WorkType.SL);
            }
        }

        private void PintarCelulasDia(int rowIndex, DateTime data, WorkType tipo)
        {
            int dia = data.Day;
            int colStart = ColIndexDiaStart(dia);

            bool isFeriado = _feriadosMes.Contains(data.Date);

            //Regras de pintura:
            // - Se for feriado E o tipo for "WD" (dia normal), pinta como feriado (Gold)
            // - Se for feriado E tiver qualquer evento (tipo != WD), pinta com a cor do evento
            // - Se não for feriado, pinta normal conforme o tipo
            System.Drawing.Color cor;
            if (isFeriado && tipo == WorkType.WD)
                cor = System.Drawing.Color.Gold;
            else
                cor = Util.GetColor(tipo);

            grdTimesheet.Rows[rowIndex].Cells[colStart].Style.BackColor = cor;
            grdTimesheet.Rows[rowIndex].Cells[colStart + 1].Style.BackColor = cor;

            // texto branco se SL (preto), senão preto
            var fg = (tipo == WorkType.SL) ? System.Drawing.Color.White : System.Drawing.Color.Black;
            grdTimesheet.Rows[rowIndex].Cells[colStart].Style.ForeColor = fg;
            grdTimesheet.Rows[rowIndex].Cells[colStart + 1].Style.ForeColor = fg;
        }
        #endregion

        #region DEMAIS EVENTOS
        private void btnSalvar_Click(object sender, EventArgs e)
        {

            try
            {
                int mes = dtCompetencia.Value.Month;
                int ano = dtCompetencia.Value.Year;

                if (grdTimesheet.Rows.Count == 0)
                {
                    MessageBox.Show("Nenhum dado para salvar.");
                    return;
                }

                //Cadastra automaticamente os colaboradores que vieram do Excel
                SalvarColaboradoresPendentesSemConfirmacao();

                var tsDao = new TimeSheetDAO();
                var colabDao = new ColaboradorDAO();

                int diasNoMes = DateTime.DaysInMonth(ano, mes);

                //Salva dia a dia exatamente como está no Excel
                foreach (DataGridViewRow row in grdTimesheet.Rows)
                {
                    if (row.IsNewRow) continue;

                    //Teste pular linha
                    string analyst = row.Cells["ANALYST"].Value?.ToString()?.Trim();

                    if (string.IsNullOrWhiteSpace(analyst))
                        continue;

                    int idColab = GarantirIdColaboradorDaLinha(row);


                    if (idColab <= 0)
                        continue; //pula se não tiver id

                    //Busca escala (para almoço)
                    var colab = colabDao.BuscarPorId(idColab);
                    var escala = colab?.tb_escala;

                    for (int d = 1; d <= diasNoMes; d++)
                    {
                        int colStart = ColIndexDiaStart(d);

                        string startTxt = row.Cells[colStart].Value?.ToString();
                        string endTxt = row.Cells[colStart + 1].Value?.ToString();

                        DateTime data = new DateTime(ano, mes, d);

                        var dia = new Core.VO.TimeSheetDiaVO
                        {
                            Data = data,

                            //Horários exatamente como no Excel
                            HoraEntrada = ParseHora(startTxt),
                            HoraSaida = ParseHora(endTxt),

                            //Almoço vindo do Excel (se existir) ou da escala
                            HoraAlmocoInicio = escala?.hora_almoco_inicio,
                            HoraAlmocoFim = escala?.hora_almoco_fim,

                            //Código exatamente como aplicado na grid / Excel
                            Codigo = ObterCodigoDiaPersistente(idColab, data)
                        };

                        tsDao.SalvarDia(idColab, dia);
                    }
                }

                MessageBox.Show("Timesheet salvo com sucesso!", "Sucesso", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (DbEntityValidationException ex)
            {
                var sb = new StringBuilder();
                sb.AppendLine("ERRO DE VALIDAÇÃO EF:");

                foreach (var eve in ex.EntityValidationErrors)
                {
                    sb.AppendLine($"Entidade: {eve.Entry.Entity.GetType().Name} | State: {eve.Entry.State}");
                    foreach (var ve in eve.ValidationErrors)
                        sb.AppendLine($" - Campo: {ve.PropertyName} | Erro: {ve.ErrorMessage}");
                }

                MessageBox.Show(sb.ToString(), "Detalhes do erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                var msg = new System.Text.StringBuilder();

                msg.AppendLine("ERRO AO SALVAR:");
                msg.AppendLine(ex.Message);

                Exception inner = ex.InnerException;
                while (inner != null)
                {
                    msg.AppendLine("---- INNER EXCEPTION ----");
                    msg.AppendLine(inner.Message);
                    inner = inner.InnerException;
                }

                MessageBox.Show(msg.ToString(), "Erro ao salvar", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

        }
        private string ObterCodigoDiaPersistente(int idColab, DateTime data)
        {
            if (_state.TryGetValue((idColab, data.Date), out var st))
                return MontarCodigoPersistente(st.Tipo, st.HorasUAouML);

            return WorkType.WD.ToString();
        }
        private int GarantirIdColaboradorDaLinha(DataGridViewRow row)
        {
            //se não tiver analyst, não é linha de colaborador
            string nome = row.Cells["ANALYST"].Value?.ToString()?.Trim();
            if (string.IsNullOrWhiteSpace(nome))
                return 0;

            int id = 0;
            if (row.Cells["ID_COLAB"].Value != null)
                int.TryParse(row.Cells["ID_COLAB"].Value.ToString(), out id);

            if (id > 0)
                return id;

            var colabDao = new ColaboradorDAO();
            var colab = colabDao.BuscarPorNome(nome);

            if (colab != null)
            {
                row.Cells["ID_COLAB"].Value = colab.id_colaborador;
                return colab.id_colaborador;
            }

            //cadastra
            string operacao = row.Cells["OPERATION"].Value?.ToString()?.Trim() ?? "";

            var entry = ParseHora(row.Cells["ENTRY_TIME"].Value?.ToString());
            var end = ParseHora(row.Cells["END_TIME"].Value?.ToString());

            ParseLunch(row.Cells["LUNCH_TIME"].Value?.ToString(), out TimeSpan? lunchIni, out TimeSpan? lunchFim);

            string dayOff1 = row.Cells["DAYOFF1"].Value?.ToString();
            string dayOff2 = row.Cells["DAYOFF2"].Value?.ToString();
            string sundayOff = row.Cells["SUNDAYOFF"].Value?.ToString();

            int novoId = colabDao.CadastrarPeloTimesheet(
                nome: nome,
                operacaoDescricao: operacao,
                entry: entry,
                lunchIni: lunchIni,
                lunchFim: lunchFim,
                end: end,
                dayOff1: dayOff1,
                dayOff2: dayOff2,
                sundayOff: sundayOff,
                codigoLogado: Core.UtilCORE.CodigoLogado
            );

            row.Cells["ID_COLAB"].Value = novoId;
            return novoId;
        }
        private void ParseLunch(string txt, out TimeSpan? ini, out TimeSpan? fim)
        {
            ini = null;
            fim = null;

            if (string.IsNullOrWhiteSpace(txt))
                return;

            txt = txt.Trim();

            //formatos comuns: "10:00:00 - 11:00:00"  | "10:00-11:00"
            var parts = txt.Split('-');
            if (parts.Length != 2) return;

            ini = ParseHora(parts[0]);
            fim = ParseHora(parts[1]);
        }
        private void SalvarColaboradoresPendentesSemConfirmacao()
        {
            if (_pendentes == null || _pendentes.Count == 0)
                return;

            var colabDao = new ColaboradorDAO();

            foreach (var kv in _pendentes.ToList())
            {
                int rowIndex = kv.Key;
                var p = kv.Value;

                int novoId = colabDao.CadastrarPeloTimesheet(
                    nome: p.Analyst,
                    operacaoDescricao: p.Operation,
                    entry: p.Entry,
                    lunchIni: p.LunchIni,
                    lunchFim: p.LunchFim,
                    end: p.End,
                    dayOff1: p.DayOff1,
                    dayOff2: p.DayOff2,
                    sundayOff: p.SundayOff,
                    codigoLogado: Core.UtilCORE.CodigoLogado
                );

                // Atualiza a grid com o novo ID
                grdTimesheet.Rows[rowIndex].Cells["ID_COLAB"].Value = novoId;

                _pendentes.Remove(rowIndex);
            }
        }
        private TimeSpan? ParseHora(string txt)
        {
            if (string.IsNullOrWhiteSpace(txt))
                return null;

            txt = txt.Trim();

            if (TimeSpan.TryParse(txt, out var ts))
                return ts;

            if (DateTime.TryParse(txt, out var dt))
                return dt.TimeOfDay;

            return null;
        }


        //Abre tela de relatório mensal
        private void btnMensal_Click(object sender, EventArgs e)
        {
            new frmTimeSheetMensal().ShowDialog();
        }

        //Exporta para Excel
        private void btnExportar_Click(object sender, EventArgs e)
        {
            using (var sfd = new SaveFileDialog())
            {
                sfd.Filter = "Excel (.xlsx)|.xlsx";
                sfd.FileName = $"Resumo_{dtCompetencia.Value:MM_yyyy}.xlsx";

                if (sfd.ShowDialog() != DialogResult.OK)
                    return;

                ExportarResumoParaExcel(sfd.FileName);

                MessageBox.Show("Arquivo exportado com sucesso!", "Sucesso", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
        private void ExportarResumoParaExcel(string caminho)
        {
            int mes = dtCompetencia.Value.Month;
            int ano = dtCompetencia.Value.Year;

            using (var wb = new XLWorkbook())
            {
                var ws = wb.Worksheets.Add("Resumo");

                // Cabeçalho (ORDEM NOVA)
                ws.Cell(1, 1).Value = "Colaborador";
                ws.Cell(1, 2).Value = "Operação";
                ws.Cell(1, 3).Value = "Cargo";
                ws.Cell(1, 4).Value = "GCM";
                ws.Cell(1, 5).Value = "Total de horas mês vigente";
                ws.Cell(1, 6).Value = "Atestados";
                ws.Cell(1, 7).Value = "Férias";
                ws.Cell(1, 8).Value = "Paid";
                ws.Cell(1, 9).Value = "Unpaid";
                ws.Cell(1, 10).Value = "Feriados";
                ws.Cell(1, 11).Value = "Total de horas apontadas no projeto";
                ws.Cell(1, 12).Value = "Custo mês";

                ws.Range(1, 1, 1, 12).Style.Font.Bold = true;
                ws.Range(1, 1, 1, 12).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                var colabDao = new ColaboradorDAO();

                int linhaExcel = 2;

                // percorre grid (ignora 3 linhas header fake)
                for (int r = 3; r < grdTimesheet.Rows.Count; r++)
                {
                    var row = grdTimesheet.Rows[r];
                    if (row.IsNewRow) continue;

                    string nome = row.Cells["ANALYST"].Value?.ToString()?.Trim();
                    if (string.IsNullOrWhiteSpace(nome)) continue;

                    int idColab = Convert.ToInt32(row.Cells["ID_COLAB"].Value ?? 0);
                    if (idColab <= 0) continue;

                    // pega (ou cria) dias do mês no cache
                    if (!_cacheDiasMes.TryGetValue(idColab, out var diasMes))
                    {
                        var tsDao = new TimeSheetDAO();
                        diasMes = tsDao.GerarMes(idColab, mes, ano);
                        _cacheDiasMes[idColab] = diasMes;
                    }

                    // Recalcula a partir do grid/state
                    RecalcularDiasDoColaboradorPeloGrid(r, idColab, diasMes);

                    // garante feriados (caso algum fluxo não tenha marcado)
                    foreach (var d in diasMes)
                        d.IsFeriado = _feriadosMes.Contains(d.Data.Date);

                    // Busca cargo/operacao/gcm do BD
                    var colab = colabDao.BuscarPorId(idColab);

                    string operacao = row.Cells["OPERATION"].Value?.ToString() ?? (colab?.tb_operacao?.nome_operacao ?? "");
                    string cargo = colab?.tb_cargo?.descricao ?? "";
                    string gcmDesc = colab?.tb_gcm?.descricao ?? "";

                    // Jornada (decimal real) pra cálculos de capacidade/AL/DN
                    decimal jornadaDecimalReal = ObterJornadaDecimalRealDoColaborador(idColab);

                    // ======================
                    // TOTAIS DO RESUMO
                    // ======================
                    decimal totalHorasApontadasProjeto_DecReal = 0m; // horas trabalhadas (sum)
                    decimal custoMes = 0m;

                    decimal atestados_DecReal = 0m; // ML
                    decimal ferias_DecReal = 0m;    // AL (jornada por dia)
                    decimal paid_DecReal = 0m;      // ML + DN
                    decimal unpaid_DecReal = 0m;    // BT
                    decimal feriados_DecReal = 0m;  // horas trabalhadas em feriados

                    // Capacidade do mês: dias "trabalháveis" * jornada
                    // (Regra: conta como trabalhável tudo que NÃO for DO)
                    int diasTrabalhaveis = 0;

                    foreach (var d in diasMes)
                    {
                        // horas trabalhadas / custo
                        totalHorasApontadasProjeto_DecReal += HoraUsuarioParaDecimal(d.HorasTrabalhadas);
                        custoMes += d.ValorDia;

                        // capacidade do mês (não conta DO)
                        if (!string.Equals(d.Codigo, WorkType.DO.ToString(), StringComparison.OrdinalIgnoreCase))
                            diasTrabalhaveis++;

                        // Atestados (ML) -> horas do atestado
                        if (string.Equals(d.Codigo, WorkType.ML.ToString(), StringComparison.OrdinalIgnoreCase))
                        {
                            atestados_DecReal += HoraUsuarioParaDecimal(d.HorasAtestado);
                          //  paid_DecReal += HoraUsuarioParaDecimal(d.HorasAtestado); // Paid inclui ML
                        }

                        // Férias (AL) -> jornada do dia
                        if (string.Equals(d.Codigo, WorkType.AL.ToString(), StringComparison.OrdinalIgnoreCase))
                        {
                            ferias_DecReal += jornadaDecimalReal;
                            // se você quiser que férias também entrem no paid, descomente:
                            // paid_DecReal += jornadaDecimalReal;
                        }

                        // Day Off Anniversary (DN) -> entra no Paid como jornada
                        if (string.Equals(d.Codigo, WorkType.DN.ToString(), StringComparison.OrdinalIgnoreCase))
                        {
                            paid_DecReal += jornadaDecimalReal;
                        }

                        // Unpaid: BT (você pediu BT como unpaid)
                        if (string.Equals(d.Codigo, WorkType.BT.ToString(), StringComparison.OrdinalIgnoreCase))
                        {
                            // no seu modelo, BT joga em BancoHoras
                            unpaid_DecReal += HoraUsuarioParaDecimal(Math.Abs(d.BancoHoras));
                        }

                        // Horas trabalhadas em feriados (não conta DO)
                        if (d.IsFeriado && !string.Equals(d.Codigo, WorkType.DO.ToString(), StringComparison.OrdinalIgnoreCase))
                        {
                            feriados_DecReal += HoraUsuarioParaDecimal(d.HorasTrabalhadas);
                        }
                    }

                    // Total de horas mês vigente (capacidade)
                    decimal totalHorasMesVigente_DecReal = diasTrabalhaveis * jornadaDecimalReal;

                    // ======================
                    // ESCREVE LINHA
                    // ======================
                    ws.Cell(linhaExcel, 1).Value = nome;
                    ws.Cell(linhaExcel, 2).Value = operacao;
                    ws.Cell(linhaExcel, 3).Value = cargo;
                    ws.Cell(linhaExcel, 4).Value = gcmDesc;

                    ws.Cell(linhaExcel, 5).Value = DecimalParaHoraUsuario(totalHorasMesVigente_DecReal);
                    ws.Cell(linhaExcel, 6).Value = DecimalParaHoraUsuario(atestados_DecReal);
                    ws.Cell(linhaExcel, 7).Value = DecimalParaHoraUsuario(ferias_DecReal);
                    ws.Cell(linhaExcel, 8).Value = DecimalParaHoraUsuario(paid_DecReal);
                    ws.Cell(linhaExcel, 9).Value = DecimalParaHoraUsuario(unpaid_DecReal);
                    ws.Cell(linhaExcel, 10).Value = DecimalParaHoraUsuario(feriados_DecReal);
                    ws.Cell(linhaExcel, 11).Value = DecimalParaHoraUsuario(totalHorasApontadasProjeto_DecReal);

                    ws.Cell(linhaExcel, 12).Value = custoMes;
                    ws.Cell(linhaExcel, 12).Style.NumberFormat.Format = "R$ #,##0.00";

                    linhaExcel++;
                }

                ws.Columns().AdjustToContents();
                wb.SaveAs(caminho);
            }
        }
        #endregion
        #region Importar Excel
        private void btnImportar_Click(object sender, EventArgs e)
        {
            int mes = dtCompetencia.Value.Month;
            int ano = dtCompetencia.Value.Year;

            using (var ofd = new OpenFileDialog())
            {
                ofd.Filter = "Excel (*.xlsx)|*.xlsx";
                if (ofd.ShowDialog() != DialogResult.OK)
                    return;

                PrepararGridEstiloExcel(mes, ano);
                //CriarCabecalho3Linhas(mes, ano);

                ImportarExcelParaGrid(ofd.FileName, mes, ano);

                AtualizarLabelFeriados(mes, ano);
            }
        }
        private void ImportarExcelParaGrid(string caminhoExcel, int mes, int ano)
        {
            //Copia pra temp (anti-lock)
            string tempFile = System.IO.Path.Combine(System.IO.Path.GetTempPath(), $"Escala_{Guid.NewGuid():N}.xlsx");

            System.IO.File.Copy(caminhoExcel, tempFile, true);

            try
            {
                using (var wb = new XLWorkbook(tempFile))
                {
                    var ws = wb.Worksheets.First();

                    //Achar linha do cabeçalho ("ANALYST") e mapear colunas fixas
                    int linhaCabec = AcharLinhaCabecalho(ws);
                    var mapFixas = MapearColunasFixas(ws, linhaCabec);

                    //Achar linha de datas (DateTime) acima do cabeçalho
                    int linhaDatas = AcharLinhaDatas(ws, linhaCabec);

                    //Montar mapa: dia -> colStartExcel (end = col+1)
                    var mapaDiaParaColStartExcel = MontarMapaDiasExcel(ws, linhaDatas, mes, ano);

                    //Feriados do mês
                    _feriadosMes = UtilCORE.FeriadosBrasil.ObterFeriados(ano).Where(d => d.Month == mes).Select(d => d.Date).ToHashSet();

                    //Preparar estruturas
                    _pendentes.Clear();
                    _state.Clear();

                    //Ler linhas de colaboradores (abaixo do cabeçalho)
                    int r = linhaCabec + 1;
                    int diasNoMes = DateTime.DaysInMonth(ano, mes);

                    var colabDao = new ColaboradorDAO();

                    while (!ws.Cell(r, mapFixas["ANALYST"]).IsEmpty())
                    {
                        string analyst = GetCell(ws, r, mapFixas, "ANALYST");
                        string operation = GetCell(ws, r, mapFixas, "OPERATION");

                        string entryTxt = GetCell(ws, r, mapFixas, "ENTRY TIME");
                        string lunchTxt = GetCell(ws, r, mapFixas, "LUNCH TIME");
                        string endTxt = GetCell(ws, r, mapFixas, "END TIME");

                        string day1 = GetCell(ws, r, mapFixas, "1º Day Off");
                        string day2 = GetCell(ws, r, mapFixas, "2º Day Off");
                        string sunday = GetCell(ws, r, mapFixas, "Sunday Off");

                        //resolve no BD
                        var colab = colabDao.BuscarPorNome(analyst);
                        int idColab = colab?.id_colaborador ?? 0;

                        //cria linha na grid
                        int rowIndex = grdTimesheet.Rows.Add();
                        var row = grdTimesheet.Rows[rowIndex];

                        row.Cells["ID_COLAB"].Value = idColab;
                        row.Cells["ANALYST"].Value = analyst;
                        row.Cells["OPERATION"].Value = operation;

                        row.Cells["ENTRY_TIME"].Value = entryTxt;
                        row.Cells["LUNCH_TIME"].Value = lunchTxt;
                        row.Cells["END_TIME"].Value = endTxt;

                        row.Cells["DAYOFF1"].Value = day1;
                        row.Cells["DAYOFF2"].Value = day2;
                        row.Cells["SUNDAYOFF"].Value = sunday;

                        //se não existir no BD, marca pendente
                        if (idColab == 0)
                        {
                            PintarLinhaPendente(rowIndex);

                            _pendentes[rowIndex] = new PendenteCadastro
                            {
                                Analyst = analyst,
                                Operation = operation,
                                DayOff1 = day1,
                                DayOff2 = day2,
                                SundayOff = sunday,
                                Entry = ParseHora(entryTxt),
                                End = ParseHora(endTxt),
                                LunchIni = TryParseLunchIni(lunchTxt),
                                LunchFim = TryParseLunchFim(lunchTxt),
                            };
                        }

                        //preencher dias Start/End
                        for (int d = 1; d <= diasNoMes; d++)
                        {
                            if (!mapaDiaParaColStartExcel.TryGetValue(d, out int colStartExcel))
                                continue;

                            //Excel: Start / End
                            string start = ws.Cell(r, colStartExcel).GetFormattedString().Trim();
                            string endd = ws.Cell(r, colStartExcel + 1).GetFormattedString().Trim();

                            //Grid: Start / End
                            int colStartGrid = ColIndexDiaStart(d);
                            row.Cells[colStartGrid].Value = start;
                            row.Cells[colStartGrid + 1].Value = endd;

                            //Estado/cor: se tem id, cria state básico
                            if (idColab > 0)
                            {
                                DateTime data = new DateTime(ano, mes, d).Date;

                                // >>> 1) tenta ler um "código do dia" do Excel <<<
                                // Exemplo: supondo que o código esteja na célula Start (ou em outra coluna):
                                // - Se você tiver uma coluna "CODE" por dia, você vai ler aqui.
                                // Como não temos o layout exato, vamos tentar inferir do Start quando ele for texto curto (UA/ML/DO etc.)
                                string codigoDia = "";

                                // tentativa: se start veio "UA", "ML", "DO" etc e end vazio, tratar como código
                                var startTrim = (start ?? "").Trim();
                                var endTrim = (endd ?? "").Trim();

                                if (startTrim.Length <= 3 && Enum.TryParse<WorkType>(startTrim, true, out _))
                                {
                                    codigoDia = startTrim;
                                    // e nesse caso, horários ficam vazios
                                    row.Cells[ColIndexDiaStart(d)].Value = "";
                                    row.Cells[ColIndexDiaStart(d) + 1].Value = "";
                                    startTrim = "";
                                    endTrim = "";
                                }

                                WorkType tipo = ResolverTipoDoExcel(codigoDia, startTrim, endTrim);

                                // >>> 2) salva no state <<<
                                if (!_state.TryGetValue((idColab, data), out var st))
                                    st = new DiaState();

                                st.Tipo = tipo;

                                // Se for UA/ML e você tiver horas em algum lugar do Excel, atribui aqui.
                                // Por enquanto: 0
                                st.HorasUAouML = (tipo == WorkType.UA || tipo == WorkType.ML) ? 0m : 0m;

                                _state[(idColab, data)] = st;

                                // >>> 3) pinta a célula com o tipo <<<
                                PintarCelulasDia(rowIndex, data, tipo);
                            }
                            else
                            {
                                DateTime data = new DateTime(ano, mes, d).Date;
                                PintarCelulasDia(rowIndex, data, WorkType.WD);
                            }
                        }

                        r++;
                    }
                }
            }
            finally
            {
                try
                {
                    if (System.IO.File.Exists(tempFile))
                        System.IO.File.Delete(tempFile);
                }
                catch { /* ignora */ }
            }
        }
        private void PintarLinhaPendente(int rowIndex)
        {
            var row = grdTimesheet.Rows[rowIndex];
            string[] fixas = { "ANALYST", "OPERATION", "ENTRY_TIME", "LUNCH_TIME", "END_TIME", "DAYOFF1", "DAYOFF2", "SUNDAYOFF" };

            foreach (var c in fixas)
                row.Cells[c].Style.BackColor = System.Drawing.Color.MistyRose;
        }
        private TimeSpan? TryParseLunchIni(string lunch)
        {
            if (string.IsNullOrWhiteSpace(lunch)) return null;
            var parts = lunch.Split('-');
            if (parts.Length < 1) return null;
            return ParseHora(parts[0].Trim());
        }
        private TimeSpan? TryParseLunchFim(string lunch)
        {
            if (string.IsNullOrWhiteSpace(lunch)) return null;
            var parts = lunch.Split('-');
            if (parts.Length < 2) return null;
            return ParseHora(parts[1].Trim());
        }


        private int AcharLinhaCabecalho(IXLWorksheet ws)
        {
            var cell = ws.Column(1).CellsUsed().FirstOrDefault(c => string.Equals(c.GetString().Trim(), "ANALYST", StringComparison.OrdinalIgnoreCase));

            if (cell == null)
                throw new Exception("Não achei o cabeçalho 'ANALYST' no Excel.");

            return cell.Address.RowNumber;
        }
        private int AcharLinhaDatas(IXLWorksheet ws, int linhaCabec)
        {
            for (int r = linhaCabec - 1; r >= Math.Max(1, linhaCabec - 30); r--)
            {
                bool temData = ws.Row(r).CellsUsed().Any(c => c.DataType == XLDataType.DateTime);
                if (temData) return r;
            }
            throw new Exception("Não achei a linha de datas (DateTime) acima do cabeçalho.");
        }
        private Dictionary<int, int> MontarMapaDiasExcel(IXLWorksheet ws, int linhaDatas, int mes, int ano)
        {
            var map = new Dictionary<int, int>();

            foreach (var c in ws.Row(linhaDatas).CellsUsed())
            {
                if (c.DataType != XLDataType.DateTime) continue;

                var dt = c.GetDateTime();
                if (dt.Month != mes || dt.Year != ano) continue;

                map[dt.Day] = c.Address.ColumnNumber; // coluna Start
            }

            return map;
        }
        private Dictionary<string, int> MapearColunasFixas(IXLWorksheet ws, int linhaCabec)
        {
            var map = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

            foreach (var cell in ws.Row(linhaCabec).CellsUsed())
            {
                var titulo = (cell.GetString() ?? "").Trim();

                if (titulo.Equals("ANALYST", StringComparison.OrdinalIgnoreCase))
                    map["ANALYST"] = cell.Address.ColumnNumber;
                else if (titulo.Equals("OPERATION", StringComparison.OrdinalIgnoreCase))
                    map["OPERATION"] = cell.Address.ColumnNumber;
                else if (titulo.Equals("ENTRY TIME", StringComparison.OrdinalIgnoreCase))
                    map["ENTRY TIME"] = cell.Address.ColumnNumber;
                else if (titulo.Equals("LUNCH TIME", StringComparison.OrdinalIgnoreCase))
                    map["LUNCH TIME"] = cell.Address.ColumnNumber;
                else if (titulo.Equals("END TIME", StringComparison.OrdinalIgnoreCase))
                    map["END TIME"] = cell.Address.ColumnNumber;
                else if (titulo.Equals("1º Day Off", StringComparison.OrdinalIgnoreCase))
                    map["1º Day Off"] = cell.Address.ColumnNumber;
                else if (titulo.Equals("2º Day Off", StringComparison.OrdinalIgnoreCase))
                    map["2º Day Off"] = cell.Address.ColumnNumber;
                else if (titulo.Equals("Sunday Off", StringComparison.OrdinalIgnoreCase))
                    map["Sunday Off"] = cell.Address.ColumnNumber;
            }

            if (!map.ContainsKey("ANALYST"))
                throw new Exception("Não achei a coluna 'ANALYST' no cabeçalho.");
            if (!map.ContainsKey("OPERATION"))
                throw new Exception("Não achei a coluna 'OPERATION' no cabeçalho.");


            //map.TryAdd("ENTRY TIME", -1);
            //map.TryAdd("LUNCH TIME", -1);
            //map.TryAdd("END TIME", -1);
            //map.TryAdd("1º Day Off", -1);
            //map.TryAdd("2º Day Off", -1);
            //map.TryAdd("Sunday Off", -1);

            return map;
        }
        private string GetCell(IXLWorksheet ws, int row, Dictionary<string, int> map, string key)
        {
            if (!map.TryGetValue(key, out int col) || col <= 0) return "";
            return ws.Cell(row, col).GetFormattedString().Trim();
        }

        #endregion

        #region Calculos
        private void CalcularTotais()
        {
            decimal totalHorasDecimal = 0;
            decimal totalBancoDecimal = 0;
            decimal totalAtestadoDecimal = 0;
            

            foreach (var d in _dias)
            {
                totalHorasDecimal += HoraUsuarioParaDecimal(d.HorasTrabalhadas);
                totalBancoDecimal += HoraUsuarioParaDecimal(d.BancoHoras);
                totalAtestadoDecimal += HoraUsuarioParaDecimal(d.HorasAtestado);
                
            }

            lblHorasTrabalhadas.Text = $"Horas trabalhadas: {DecimalParaHoraUsuario(totalHorasDecimal)}h";

            //lblBancoDeHoras.Text = $"Banco de horas: {DecimalParaHoraUsuario(totalBancoDecimal):N2}";

            lblAtestados.Text = $"Atestados: {DecimalParaHoraUsuario(totalAtestadoDecimal):N2}h";

            lblValorReceber.Text = $"Valor de custo ao projeto: {_dias.Sum(d => d.ValorDia):C2}";

            AtualizarHorasTrabalhadasEmFeriados();

            AtualizarPaidUnpaidLabels(GetIdColaboradorSelecionado());
        }

        private decimal HoraUsuarioParaDecimal(decimal horasUsuario)
        {
            int sign = Math.Sign(horasUsuario);
            decimal abs = Math.Abs(horasUsuario);

            int h = (int)Math.Floor(abs);
            int m = (int)Math.Round((abs - h) * 100);
            if (m >= 60) m = 60;

            return sign * (h + (m / 60m));
        }
        private decimal DecimalParaHoraUsuario(decimal decimalReal)
        {
            int sign = Math.Sign(decimalReal);
            decimal abs = Math.Abs(decimalReal);

            int h = (int)Math.Floor(abs);
            int m = (int)Math.Round((abs - h) * 60);
            if (m == 60) { h++; m = 0; }

            return sign * (h + (m / 100m));
        }
        private void AtualizarHorasTrabalhadasEmFeriados()
        {
            if (_dias == null || _dias.Count == 0)
            {
                lblHorasFeriado.Text = "Horas trabalhadas em feriados: 0,00h";
                return;
            }

            decimal totalHorasFeriadoDecimal = 0m;

            foreach (var d in _dias)
            {
                if (!d.IsFeriado)
                    continue;

                //Não conta dias com código DO
                if (string.Equals(d.Codigo, WorkType.DO.ToString(), StringComparison.OrdinalIgnoreCase))
                    continue;

                if (string.Equals(d.Codigo, WorkType.AL.ToString(), StringComparison.OrdinalIgnoreCase)) 
                    continue;

                totalHorasFeriadoDecimal += HoraUsuarioParaDecimal(d.HorasTrabalhadas);
            }

            lblHorasFeriado.Text = $"Horas trabalhadas em feriados: {DecimalParaHoraUsuario(totalHorasFeriadoDecimal):N2}h";
        }

        #endregion
        private void label13_Click(object sender, EventArgs e)
        {

        }

        private void lblAtestados_Click(object sender, EventArgs e)
        {



        }

        #region Carregameto
        //quando clicar em carregar, vai ser chamado esses meétodos 
        private void btnCarregar_Click(object sender, EventArgs e)
        {
            int mes = dtCompetencia.Value.Month;
            int ano = dtCompetencia.Value.Year;

            PrepararGridEstiloExcel(mes, ano);
            CarregarColaboradoresNoGrid(mes, ano);
            AtualizarLabelFeriados(mes, ano);
            ZerarLabels();
        }
        private void SelecionarPrimeiroColaborador()
        {
            // primeira linha de colaborador é a 3 (0,1,2 são cabeçalho fake)
            if (grdTimesheet.Rows.Count > 3)
            {
                grdTimesheet.ClearSelection();
                grdTimesheet.CurrentCell = grdTimesheet.Rows[3].Cells["ANALYST"];
                grdTimesheet.Rows[3].Selected = true;
                AtualizarLabelsDoColaboradorSelecionado();
            }
            else
            {
                ZerarLabels();
            }
        }

        #endregion

        #region Gerenciamento da grid
        private void grdTimesheet_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 3) return; // cabeçalho fake
            if (e.ColumnIndex < 0) return;

            // Só atualiza se clicou exatamente na coluna "ANALYST"
            if (grdTimesheet.Columns[e.ColumnIndex].Name != "ANALYST")
                return;

            int idColab = Convert.ToInt32(grdTimesheet.Rows[e.RowIndex].Cells["ID_COLAB"].Value ?? 0);
            if (idColab <= 0)
            {
                ZerarLabels();
                return;
            }

            AtualizarLabelsTotaisMes(idColab, e.RowIndex);
        }
        private void AtualizarLabelsTotaisMes(int idColab, int rowIndex)
        {
            if (!_cacheDiasMes.TryGetValue(idColab, out var diasMes))
            {
                ZerarLabels();
                return;
            }

            // Recalcula todos os dias do mês do colaborador
            RecalcularDiasDoColaboradorPeloGrid(rowIndex, idColab, diasMes);

            // Usa _dias para o seu CalcularTotais()
            _dias = diasMes;
            CalcularTotais();
        }
        private void grdTimesheet_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            // se quiser ignorar tudo, ok:
            // return;

            // opcional: você pode validar formato e limpar erro visual
            if (e.RowIndex < 3 || e.ColumnIndex < COLS_FIXAS) return;

            // Exemplo: validar hora (se você quiser)
            int dia = DiaDoMesPorColuna(e.ColumnIndex);
            int colStart = ColIndexDiaStart(dia);

            var startTxt = grdTimesheet.Rows[e.RowIndex].Cells[colStart].Value?.ToString();
            var endTxt = grdTimesheet.Rows[e.RowIndex].Cells[colStart + 1].Value?.ToString();

            // se inválido, você pode marcar a célula (opcional)
            // mas NÃO atualiza labels aqui.
        }
        #endregion

        #region Resolver Excel
        private WorkType ResolverTipoDoExcel(string codigoTxt, string startTxt, string endTxt)
        {
            // 1) Se veio um código explícito (WD/DO/UA/ML/SL...)
            if (!string.IsNullOrWhiteSpace(codigoTxt))
            {
                codigoTxt = codigoTxt.Trim().ToUpperInvariant();

                // alguns excels podem vir com "WD " etc
                if (Enum.TryParse<WorkType>(codigoTxt, true, out var t))
                    return t;
            }

            // 2) Fallback: se start/end vazios => DO (ou WD, depende da sua regra)
            // Eu recomendaria WD como padrão, mas se seu Excel usa vazio para folga, pode ser DO.
            bool vazio = string.IsNullOrWhiteSpace(startTxt) && string.IsNullOrWhiteSpace(endTxt);
            if (vazio) return WorkType.WD;

            return WorkType.WD;
        }
        private string MontarCodigoPersistente(WorkType tipo, decimal horasUAouML)
        {
            if (tipo == WorkType.UA || tipo == WorkType.ML)
                return $"{tipo}|{horasUAouML.ToString(CultureInfo.InvariantCulture)}";

            return tipo.ToString();
        }
        private void ParseCodigoPersistente(string codigo, out WorkType tipo, out decimal horasUAouML)
        {
            tipo = WorkType.WD;
            horasUAouML = 0m;

            if (string.IsNullOrWhiteSpace(codigo))
                return;

            var parts = codigo.Trim().Split('|');
            if (Enum.TryParse(parts[0], true, out WorkType t))
                tipo = t;

            if (parts.Length > 1)
            {
                var s = parts[1].Trim().Replace(",", ".");
                if (decimal.TryParse(s, NumberStyles.Any, CultureInfo.InvariantCulture, out var v))
                    horasUAouML = v;
            }
        }

        #endregion

        #region Paid/Unpaid
        private void AtualizarPaidUnpaidLabels(int idColab)
        {
            // sem colaborador ou sem dias carregados
            if (idColab <= 0 || _dias == null || _dias.Count == 0)
            {
                lblPaidLeave.Text = "Paid Leave: 0,00h";
                lblUnpaidLeave.Text = "Unpaid Leave: 0,00h";
                return;
            }

            // Jornada (em decimal real) vinda da escala do colaborador
            decimal jornadaDecimalReal = ObterJornadaDecimalRealDoColaborador(idColab);

            decimal paidDecimalReal = 0m;
            decimal unpaidDecimalReal = 0m;

            foreach (var d in _dias)
            {
                // Paid: DN
               
               if (string.Equals(d.Codigo, WorkType.DN.ToString(), StringComparison.OrdinalIgnoreCase))
                {
                    // DN = aniversário -> conta a jornada inteira como paid leave
                    paidDecimalReal += jornadaDecimalReal;
                }

                // Unpaid: BT
                if (string.Equals(d.Codigo, WorkType.BT.ToString(), StringComparison.OrdinalIgnoreCase))
                {
                    // BancoHoras está em "hora-usuário". Se vier negativo em algum caso, soma em absoluto.
                    unpaidDecimalReal += HoraUsuarioParaDecimal(Math.Abs(d.BancoHoras));
                }
            }

            lblPaidLeave.Text = $"Paid Leave: {DecimalParaHoraUsuario(paidDecimalReal):N2}h";
            lblUnpaidLeave.Text = $"Unpaid Leave: {DecimalParaHoraUsuario(unpaidDecimalReal):N2}h";
        }

        private decimal ObterJornadaDecimalRealDoColaborador(int idColab)
        {
            var colabDao = new ColaboradorDAO();
            var colab = colabDao.BuscarPorId(idColab);

            if (colab?.tb_escala == null)
                return 0m;

            // Usa o cálculo do DAO (retorna decimal real)
            var tsDao = new TimeSheetDAO();
            return tsDao.CalcularHoras(
                colab.tb_escala.hora_entrada,
                colab.tb_escala.hora_saida,
                colab.tb_escala.hora_almoco_inicio,
                colab.tb_escala.hora_almoco_fim
            );
        }

        #endregion

      
    }
}
