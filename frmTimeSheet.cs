    using Apontamento.Comum;
using ClosedXML.Excel;
using Core.Enums;
using Core.VO;
using DAO;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Xml;
using static Core.UtilCORE;

namespace Apontamento
{
    public partial class frmTimeSheet : Form
    {
        //Lista de dias do timesheet carregado
        private List<TimeSheetDiaVO> _dias = new List<TimeSheetDiaVO>();

        //Controla se o usuário está arrastando um código na grid
        bool isDragging = false;

        //Código (WorkType) atualmente selecionado para drag
        WorkType? tipoSelecionado = null;

        //Armazena o ID do colaborador selecionado
        private int _idColaborador;

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
            //Configura as colunas e comportamento da grid
            ConfigurarGrid();
            //Carrega os colaboradores no ComboBox
            CarregarColaboradores();

            //Configura o DateTimePicker para selecionar apenas mês/ano
            dtCompetencia.Format = DateTimePickerFormat.Custom;
            dtCompetencia.CustomFormat = "MM/yyyy";
            dtCompetencia.ShowUpDown = true;
        }

        #endregion

        #region GRID

        private void ConfigurarGrid()
        {
            grdTimesheet.AutoGenerateColumns = false;
            grdTimesheet.AllowUserToAddRows = false;
            grdTimesheet.SelectionMode = DataGridViewSelectionMode.CellSelect;
            grdTimesheet.RowHeadersVisible = false;
            grdTimesheet.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

            grdTimesheet.Columns.Clear();

            //Coluna Data
            grdTimesheet.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "colData",
                DataPropertyName = "Data",
                HeaderText = "Data",
                ReadOnly = true,
                DefaultCellStyle = { Format = "dd/MM" }
            });

            //Coluna Dia da Semana
            grdTimesheet.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "colDia",
                DataPropertyName = "DiaSemana",
                HeaderText = "Dia",
                ReadOnly = true
            });

            //Colunas de Horários
            grdTimesheet.Columns.Add(CriarHoraColuna("HoraEntrada", "Entrada"));
            grdTimesheet.Columns.Add(CriarHoraColuna("HoraAlmocoInicio", "Almoço\nInício"));
            grdTimesheet.Columns.Add(CriarHoraColuna("HoraAlmocoFim", "Almoço Fim"));
            grdTimesheet.Columns.Add(CriarHoraColuna("HoraSaida", "Saída"));

            //Coluna Código (ComboBox)
            grdTimesheet.Columns.Add(new DataGridViewComboBoxColumn
            {
                Name = "colCodigo",
                DataPropertyName = "Codigo",
                HeaderText = "Código",
                DataSource = Enum.GetNames(typeof(WorkType))
            });

            //Coluna Horas Trabalhadas
            grdTimesheet.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "colHoras",
                DataPropertyName = "HorasTrabalhadas",
                HeaderText = "Horas",
                ReadOnly = true
            });

            //Coluna Banco de Horas
            grdTimesheet.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "colBanco",
                DataPropertyName = "BancoHoras",
                HeaderText = "Banco",
                ReadOnly = true
            });

            //Coluna valor do dia
            //grdTimesheet.Columns.Add(new DataGridViewTextBoxColumn
            //{
            //    Name = "colValor",
            //    DataPropertyName = "ValorDia",
            //    HeaderText = "Valor",
            //    ReadOnly = true,
            //    DefaultCellStyle = { Format = "C2" }
            //});
        }

        //Método auxiliar para criar colunas de horário
        private DataGridViewTextBoxColumn CriarHoraColuna(string prop, string titulo)
        {
            return new DataGridViewTextBoxColumn
            {
                DataPropertyName = prop,
                HeaderText = titulo
            };
        }

        #endregion

        #region CARREGAMENTO
        //Carrega colaboradores ativos no ComboBox
        private void CarregarColaboradores()
        {
            cbColaborador.DataSource = new ColaboradorDAO().ListarAtivos(Core.UtilCORE.CodigoLogado);

            cbColaborador.DisplayMember = "nome_colaborador";
            cbColaborador.ValueMember = "id_colaborador";
            cbColaborador.SelectedIndex = -1;
        }

        private void btnCarregar_Click(object sender, EventArgs e)
        {
            if (ValidarCampos())
                return;

            if (cbColaborador.SelectedIndex < 0) return;

            _idColaborador = (int)cbColaborador.SelectedValue;

            var dao = new TimeSheetDAO();

            ////Gera os dias do mês
            //_dias = dao.GerarMes(_idColaborador, dtCompetencia.Value.Month, dtCompetencia.Value.Year);

            ////Aplica exceções (feriados, férias, etc.)
            //dao.AplicarExcecoes(_idColaborador, _dias);

            ////Calcula cada dia
            //foreach (var d in _dias)
            //    dao.CalcularDia(_idColaborador, d);
            // >>> ADIÇÃO: OBTÉM FERIADOS DO ANO <<<
            var feriados = FeriadosBrasil.ObterFeriados(dtCompetencia.Value.Year);

            //Gera os dias do mês
            _dias = dao.GerarMes(
                _idColaborador,
                dtCompetencia.Value.Month,
                dtCompetencia.Value.Year);

            //Aplica exceções (férias, afastamentos, etc.)
            dao.AplicarExcecoes(_idColaborador, _dias);

            // >>> CORREÇÃO: MARCA FERIADO ANTES DO CÁLCULO <<<
            foreach (var d in _dias)
            {
                if (feriados.Contains(d.Data.Date))
                    d.IsFeriado = true;

                dao.CalcularDia(_idColaborador, d);
            }

            //Vincula a lista à grid
            grdTimesheet.DataSource = new BindingList<TimeSheetDiaVO>(_dias);

            //Aplica cores conforme o código
            AplicarCoresNaGrid();
            //Atualiza totais
            CalcularTotais();

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
            //Recupera o objeto TimeSheetDiaVO vinculado à linha editada
            var dia = grdTimesheet.Rows[e.RowIndex].DataBoundItem as TimeSheetDiaVO;
            //Se não houver objeto associado, interrompe
            if (dia == null) return;

            //Instancia o DAO responsável pelos cálculos do timesheet
            var dao = new TimeSheetDAO();

            //Verifica se a célula editada é a coluna de Código
            if (grdTimesheet.Columns[e.ColumnIndex].Name == "colCodigo")
            {
                //Converte o código selecionado para o enum WorkType
                if (!Enum.TryParse(dia.Codigo, out WorkType tipo))
                    return;
                //Aplica a cor correspondente ao tipo de trabalho
                grdTimesheet.Rows[e.RowIndex].DefaultCellStyle.BackColor =
                    Util.GetColor(tipo);

                //UA (Ausência injustificada) + pergunta horas ausentes
                if (tipo == WorkType.UA)
                {
                    decimal horasUA = PerguntarHorasAusentes(dia.Data);
                    decimal horasParaCalculo = ConverterHora(horasUA);
                    dia.HorasAtestado = horasUA;
                    dao.CalcularDia(_idColaborador, dia, horasUA);
                }
                //ML (Atestado médico) + pergunta horas de atestado
                else if (tipo == WorkType.ML)
                {
                    decimal horasML = PerguntarHorasAtestado(dia.Data);
                    decimal horasParaCalculo = ConverterHora(horasML);
                    dia.HorasAtestado = horasML;
                    dao.CalcularDia(_idColaborador, dia, horasML);
                }
                //SL (Desligamento) + aplica o código SL em cascata
                else if (tipo == WorkType.SL)
                {
                    AplicarSLApartirDoDia(dia.Data);
                }
                //Qualquer outro código + cálculo padrão
                else
                {
                    dao.CalcularDia(_idColaborador, dia);
                }
            }
            //Caso a edição não seja no código (horários, por exemplo)
            else
            {
                dao.CalcularDia(_idColaborador, dia);
            }

            //Atualiza os totais exibidos na tela
            CalcularTotais();
            //Atualiza visualmente a grid
            //O refresh atualiza visualmente a grid após as alterações, e não diretamente o DataSource vinculado a ela
            grdTimesheet.Refresh();
        }

        //Método genérico para perguntar horas ao usuário (UA, ML, etc.)
        private decimal PerguntarHoras(string titulo, DateTime data)
        {
            //Cria um formulário modal simples
            using (Form dialog = new Form())
            {
                //Configurações básicas do diálogo
                dialog.Text = titulo;
                dialog.StartPosition = FormStartPosition.CenterParent;
                dialog.FormBorderStyle = FormBorderStyle.FixedDialog;
                dialog.Width = 320;
                dialog.Height = 160;
                dialog.MaximizeBox = false;
                dialog.MinimizeBox = false;

                //Label com instruções
                Label lbl = new Label();
                lbl.Text = $"Informe as horas\nData: {data:dd/MM/yyyy}";
                lbl.Left = 10;
                lbl.Top = 10;
                lbl.Width = 280;

                //TextBox para entrada das horas
                TextBox txtHoras = new TextBox();
                txtHoras.Left = 10;
                txtHoras.Top = 50;
                txtHoras.Width = 280;
                txtHoras.Text = "0";

                //Botão OK
                Button btnOk = new Button();
                btnOk.Text = "OK";
                btnOk.Left = 130;
                btnOk.Top = 80;
                btnOk.DialogResult = DialogResult.OK;

                //Adiciona controles ao formulário
                dialog.Controls.Add(lbl);
                dialog.Controls.Add(txtHoras);
                dialog.Controls.Add(btnOk);

                //Define o botão padrão
                dialog.AcceptButton = btnOk;

                //Exibe o diálogo e valida o retorno
                if (dialog.ShowDialog(this) == DialogResult.OK)
                {
                    if (decimal.TryParse(
                        txtHoras.Text.Replace(",", "."),
                        System.Globalization.NumberStyles.Any,
                        System.Globalization.CultureInfo.InvariantCulture,
                        out decimal horas))
                    {
                        //Garante que não seja valor negativo
                        return horas < 0 ? 0 : horas;
                    }
                }
                //Valor padrão caso cancele ou erro
                return 0;
            }
        }

        //Pergunta horas de ausência injustificada (UA)
        private decimal PerguntarHorasAusentes(DateTime data)
        {
            return PerguntarHoras(
                "Falta injustificada (UA)",
                data);
        }
        //Pergunta horas de atestado médico (ML)
        private decimal PerguntarHorasAtestado(DateTime data)
        {
            return PerguntarHoras("Atestado médico (ML)", data);
        }


        //Aplica o código SL (desligamento) a partir de uma data em diante
        private void AplicarSLApartirDoDia(DateTime dataInicio)
        {
            var dao = new TimeSheetDAO();
            //Percorre todos os dias do timesheet
            foreach (var dia in _dias)
            {
                //Se a data for maior ou igual à data de desligamento
                if (dia.Data >= dataInicio)
                {
                    dia.Codigo = WorkType.SL.ToString();
                    dao.CalcularDia(_idColaborador, dia);
                }
            }
            //Atualiza as cores da grid
            AplicarCoresNaGrid();
        }

        #endregion

        #region TOTAIS
        //Calcula e exibe os totais do mês
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

            lblHorasTrabalhadas.Text =
                $"Horas trabalhadas: {DecimalParaHoraUsuario(totalHorasDecimal)}h";

            lblBancoDeHoras.Text =
                $"Banco de horas: {DecimalParaHoraUsuario(totalBancoDecimal)}h";

            lblAtestados.Text =
                $"Horas em atestados: {DecimalParaHoraUsuario(totalAtestadoDecimal)}h";

            lblValorReceber.Text =
                $"Valor a receber: {_dias.Sum(d => d.ValorDia):C2}";
        }
        private decimal HoraUsuarioParaDecimal(decimal horaUsuario)
        {
            int horas = (int)horaUsuario;
            int minutos = (int)Math.Round((horaUsuario - horas) * 100);
            return horas + (minutos / 60m);
        }

        private decimal DecimalParaHoraUsuario(decimal decimalReal)
        {
            int horas = (int)decimalReal;
            int minutos = (int)Math.Round((decimalReal - horas) * 60);
            return horas + (minutos / 100m);
        }


        #endregion

        #region Excel
        //Exporta o timesheet atual para um arquivo Excel
        private void ExportarParaExcel(string caminho)
        {
            using (var wb = new XLWorkbook())
            {
                //Cria uma planilha chamada Timesheet
                var ws = wb.Worksheets.Add("Timesheet");

                int linha = 1;

                // =========================
                // CABEÇALHO
                // =========================
                ws.Cell(linha, 1).Value = "Data";
                ws.Cell(linha, 2).Value = "Dia";
                ws.Cell(linha, 3).Value = "Entrada";
                ws.Cell(linha, 4).Value = "Alm. Início";
                ws.Cell(linha, 5).Value = "Alm. Fim";
                ws.Cell(linha, 6).Value = "Saída";
                ws.Cell(linha, 7).Value = "Código";
                ws.Cell(linha, 8).Value = "Horas Trabalhadas";
                ws.Cell(linha, 9).Value = "Banco Horas";
                ws.Cell(linha, 10).Value = "Horas Atestado";
                ws.Cell(linha, 11).Value = "Valor Dia";



                ws.Range(linha, 1, linha, 11).Style.Font.Bold = true;

                linha++;

                // =========================
                // DADOS
                // =========================
                foreach (var d in _dias)
                {
                    ws.Cell(linha, 1).Value = d.Data;
                    ws.Cell(linha, 1).Style.DateFormat.Format = "dd/MM/yyyy";

                    ws.Cell(linha, 2).Value = d.DiaSemana;
                    ws.Cell(linha, 3).Value = d.HoraEntrada?.ToString();
                    ws.Cell(linha, 4).Value = d.HoraAlmocoInicio?.ToString();
                    ws.Cell(linha, 5).Value = d.HoraAlmocoFim?.ToString();
                    ws.Cell(linha, 6).Value = d.HoraSaida?.ToString();
                    ws.Cell(linha, 7).Value = d.Codigo;
                    ws.Cell(linha, 8).Value = d.HorasTrabalhadas;
                    ws.Cell(linha, 9).Value = d.BancoHoras;
                    ws.Cell(linha, 10).Value = d.HorasAtestado;
                    ws.Cell(linha, 11).Value = d.ValorDia;
                    ws.Cell(linha, 11).Style.NumberFormat.Format = "#,##0.00";


                    linha++;
                }

                // =========================
                // TOTAIS
                // =========================
                linha++;

                ws.Cell(linha, 7).Value = "TOTAIS:";
                ws.Cell(linha, 8).Value = _dias.Sum(d => d.HorasTrabalhadas);
                ws.Cell(linha, 9).Value = _dias.Sum(d => d.BancoHoras);
                ws.Cell(linha, 10).Value = _dias.Sum(d => d.HorasAtestado);
                ws.Cell(linha, 11).Value = _dias.Sum(d => d.ValorDia);
                ws.Cell(linha, 11).Style.NumberFormat.Format = "#,##0.00";

                ws.Range(linha, 7, linha, 11).Style.Font.Bold = true;

                //Ajusta automaticamente o tamanho das colunas
                ws.Columns().AdjustToContents();
                //Salva o arquivo no caminho informado
                wb.SaveAs(caminho);
            }
        }

        #endregion

        #region DRAG CÓDIGO
        //Início do arraste do código
        private void grdTimesheet_CellMouseDown(object sender, DataGridViewCellMouseEventArgs e)
        {
            //Só permite arrastar se for na coluna Código
            if (e.RowIndex < 0 || grdTimesheet.Columns[e.ColumnIndex].Name != "colCodigo")
                return;

            isDragging = true;
            //Guarda o código selecionado
            if (Enum.TryParse(grdTimesheet.Rows[e.RowIndex].Cells["colCodigo"].Value?.ToString(), out WorkType tipo))
                tipoSelecionado = tipo;
        }

        //Aplica o código enquanto o mouse passa pelas células
        private void grdTimesheet_CellMouseEnter(object sender, DataGridViewCellEventArgs e)
        {
            if (!isDragging) return;
            if (!tipoSelecionado.HasValue) return;
            if (e.RowIndex < 0) return;
            if (grdTimesheet.Columns[e.ColumnIndex].Name != "colCodigo") return;

            var row = grdTimesheet.Rows[e.RowIndex];
            var dia = row.DataBoundItem as TimeSheetDiaVO;
            if (dia == null) return;

            //Atualiza o código do dia
            dia.Codigo = tipoSelecionado.Value.ToString();

            var dao = new TimeSheetDAO();

            //Se for UA, pergunta horas
            if (tipoSelecionado.Value == WorkType.UA)
            {
                decimal horasUA = PerguntarHorasAusentes(dia.Data);
                dao.CalcularDia(_idColaborador, dia, horasUA);
            }
            else
            {
                dao.CalcularDia(_idColaborador, dia);
            }
            //Atualiza a cor da linha
            row.DefaultCellStyle.BackColor = Util.GetColor(tipoSelecionado.Value);

            //Atualiza visualmente a grid
            grdTimesheet.Refresh();
        }

        //Finaliza o arraste
        private void grdTimesheet_CellMouseUp(object sender, DataGridViewCellMouseEventArgs e)
        {
            isDragging = false;
            tipoSelecionado = null;
            //Atualiza totais após o drag
            CalcularTotais();
        }

        #endregion

        #region DEMAIS EVENTOS
        private void btnSalvar_Click(object sender, EventArgs e)
        {
           
                if (cbColaborador.SelectedIndex < 0) return;

                int idColaborador = (int)cbColaborador.SelectedValue;
                var dao = new TimeSheetDAO();

                //Salva dia a dia
                foreach (var dia in _dias)
                    dao.SalvarDia(idColaborador, dia);

                MessageBox.Show("Timesheet salvo com sucesso!", "Sucesso",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            

        }

        //Abre tela de relatório mensal
        private void btnMensal_Click(object sender, EventArgs e)
        {
            new frmTimeSheetMensal().ShowDialog();
        }

        //Exporta para Excel
        private void btnExportar_Click(object sender, EventArgs e)
        {
            if (_dias == null || !_dias.Any())
            {
                MessageBox.Show("Nenhum dado para exportar.");
                return;
            }

            using (SaveFileDialog sfd = new SaveFileDialog())
            {
                sfd.Filter = "Excel (*.xlsx)|*.xlsx";
                sfd.FileName =
                    $"Timesheet_{cbColaborador.Text}_{dtCompetencia.Value:MM_yyyy}.xlsx";

                if (sfd.ShowDialog() != DialogResult.OK)
                    return;

                ExportarParaExcel(sfd.FileName);

                MessageBox.Show("Arquivo exportado com sucesso!");
            }
        }
        #endregion

        #region MÉTODOS AUXILIARES
        //Valida campos obrigatórios da tela
        private bool ValidarCampos()
        {
            if (cbColaborador.SelectedIndex < 0)
                MessageBox.Show("Selecione um colaborador", "Atenção", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return false;
        }
        #endregion

        #region XML (CADASTRAR OFFLINE)
        //private void CadastrarOffline()
        //{
        //    XmlDocument xml = new XmlDocument();

        //    if (!File.Exists(Util.PathFileXML(Util.Tela.Apontamento)))
        //    {
        //        XmlElement noApontamento = xml.CreateElement("Apontamento");
        //        xml.AppendChild(noApontamento);
        //    }

        //    XmlElement noItem = xml.CreateElement("Item");


        //}
        private void CadastrarOffline()
        {
            XmlDocument xml = new XmlDocument();
            string caminho = Util.PathFileXML(Util.Tela.Apontamento);

            // =========================
            // CRIA OU CARREGA XML
            // =========================
            if (!File.Exists(caminho))
            {
                XmlElement noApontamento = xml.CreateElement("Apontamento");
                xml.AppendChild(noApontamento);
            }
            else
            {
                xml.Load(caminho);
            }

            XmlNode raiz = xml.SelectSingleNode("Apontamento");

            // =========================
            // ITEM (TIMESHEET)
            // =========================
            XmlElement noItem = xml.CreateElement("Item");

            XmlElement noIdColaborador = xml.CreateElement("IdColaborador");
            noIdColaborador.InnerText = _idColaborador.ToString();
            noItem.AppendChild(noIdColaborador);

            XmlElement noNomeColaborador = xml.CreateElement("Colaborador");
            noNomeColaborador.InnerText = cbColaborador.Text;
            noItem.AppendChild(noNomeColaborador);

            XmlElement noCompetencia = xml.CreateElement("Competencia");
            noCompetencia.InnerText = dtCompetencia.Value.ToString("MM/yyyy");
            noItem.AppendChild(noCompetencia);

            // =========================
            // DIAS
            // =========================
            foreach (var dia in _dias)
            {
                XmlElement noDia = xml.CreateElement("Dia");

                XmlElement noData = xml.CreateElement("Data");
                noData.InnerText = dia.Data.ToString("yyyy-MM-dd");
                noDia.AppendChild(noData);

                XmlElement noCodigo = xml.CreateElement("Codigo");
                noCodigo.InnerText = dia.Codigo ?? "";
                noDia.AppendChild(noCodigo);

                XmlElement noHorasTrabalhadas = xml.CreateElement("HorasTrabalhadas");
                noHorasTrabalhadas.InnerText = dia.HorasTrabalhadas.ToString("0.00");
                noDia.AppendChild(noHorasTrabalhadas);

                XmlElement noBancoHoras = xml.CreateElement("BancoHoras");
                noBancoHoras.InnerText = dia.BancoHoras.ToString("0.00");
                noDia.AppendChild(noBancoHoras);

                XmlElement noHorasAtestado = xml.CreateElement("HorasAtestado");
                noHorasAtestado.InnerText = dia.HorasAtestado.ToString("0.00");
                noDia.AppendChild(noHorasAtestado);

                XmlElement noValorDia = xml.CreateElement("ValorDia");
                noValorDia.InnerText = dia.ValorDia.ToString("0.00");
                noDia.AppendChild(noValorDia);

                noItem.AppendChild(noDia);
            }

            // =========================
            // SALVA
            // =========================
            raiz.AppendChild(noItem);
            xml.Save(caminho);
        }


        #endregion

        #region Horas/Minutos
        private decimal ConverterHora(decimal valor)
        {
            int horas = (int)valor;
            int minutos = (int)Math.Round((valor - horas) * 100);

            if (minutos > 59)
                minutos = 59;

            return horas + (minutos / 60m);
        }


        #endregion
    }
}
