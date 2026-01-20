using Apontamento.Comum;
using Core.Enums;
using Core.VO;
using DAO;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using static Core.UtilCORE;

namespace Apontamento
{
    public partial class frmTimeSheetMensal : Form
    {
        private List<Core.VO.TimeSheetMensalLinha> _linhas = new List<Core.VO.TimeSheetMensalLinha>();
        private bool isDragging = false;
        private WorkType? tipoSelecionado = null;
        //Flag para controlar carregamento inicial do combo box
        private bool _carregando = false;
        public frmTimeSheetMensal()
        {
            InitializeComponent();
        }

        #region LOAD
        private void frmTimeSheetMensal_Load(object sender, EventArgs e)
        {

            dtCompetencia.Format = DateTimePickerFormat.Custom;
            dtCompetencia.CustomFormat = "MM/yyyy";
            dtCompetencia.ShowUpDown = true;

            ConfigurarGrid();

            //Teste
            grdMensal.CurrentCellDirtyStateChanged += grdMensal_CurrentCellDirtyStateChanged;
            grdMensal.CellValueChanged += grdMensal_CellValueChanged;

            grdMensal.CellPainting += grdMensal_CellPainting;


        }
        #endregion

        #region GRID
        private void ConfigurarGrid()
        {
            grdMensal.AutoGenerateColumns = false;
            grdMensal.AllowUserToAddRows = false;
            grdMensal.RowHeadersVisible = false;
            grdMensal.SelectionMode = DataGridViewSelectionMode.CellSelect;
            grdMensal.Columns.Clear();

            grdMensal.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "colNome",
                HeaderText = "Colaborador",
                ReadOnly = true,
                Width = 220
            });

            int diasNoMes = DateTime.DaysInMonth(
                dtCompetencia.Value.Year,
                dtCompetencia.Value.Month);

            for (int d = 1; d <= diasNoMes; d++)
            {
                grdMensal.Columns.Add(new DataGridViewComboBoxColumn
                {
                    Name = $"D{d}",
                    HeaderText = d.ToString("00"),
                    DataSource = Enum.GetNames(typeof(WorkType)),
                    Width = 45
                });
            }

            grdMensal.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "colTotal",
                HeaderText = "Total Mês",
                ReadOnly = true,
                DefaultCellStyle = { Format = "C2" },
                Width = 110
            });
        }

        //Teste
        private void grdMensal_CurrentCellDirtyStateChanged(object sender, EventArgs e)
        {
            if (grdMensal.IsCurrentCellDirty)
                grdMensal.CommitEdit(DataGridViewDataErrorContexts.Commit);
        }

        private void grdMensal_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            if (_carregando) return;

            if (e.RowIndex < 0) return;

            if (!grdMensal.Columns[e.ColumnIndex].Name.StartsWith("D")) return;

            int dia = int.Parse(grdMensal.Columns[e.ColumnIndex].Name.Substring(1));

            var linha = _linhas[e.RowIndex];
            var diaVO = linha.Dias[dia];

            if (!Enum.TryParse(grdMensal.Rows[e.RowIndex].Cells[e.ColumnIndex].Value?.ToString(), out WorkType tipo))
                return;

            diaVO.Codigo = tipo.ToString();

            var dao = new TimeSheetDAO();

            if (tipo == WorkType.UA)
            {
                if (diaVO.HorasTrabalhadas == 0)
                {
                    decimal h = PerguntarHoras("Falta injustificada (UA)", diaVO.Data);


                    diaVO.HorasTrabalhadas = h;

                    dao.CalcularDia(linha.IdColaborador, diaVO, h);
                }

                return;
            }

            else if (tipo == WorkType.ML)
            {
                if (diaVO.HorasAtestado == 0)
                {
                    decimal h = PerguntarHoras("Atestado médico (ML)", diaVO.Data);
                    dao.CalcularDia(linha.IdColaborador, diaVO, h);
                }
            }

            else
            {
                dao.CalcularDia(linha.IdColaborador, diaVO);
            }

            grdMensal.Rows[e.RowIndex].Cells[e.ColumnIndex].Style.BackColor = Util.GetColor(tipo);

            RecalcularTotais();
        }
        #endregion

        #region CARREGAR
        private void btnCarregar_Click(object sender, EventArgs e)
        {
            _carregando = true;

            try
            {
                _linhas.Clear();
                grdMensal.Rows.Clear();
                ConfigurarGrid();

                var colaboradores = new ColaboradorDAO()
                    .ListarAtivos(Core.UtilCORE.CodigoLogado);

                var dao = new TimeSheetDAO();


                var feriados = FeriadosBrasil.ObterFeriados(dtCompetencia.Value.Year);

                foreach (var c in colaboradores)
                {
                    var dias = dao.GerarMes(
                        c.id_colaborador,
                        dtCompetencia.Value.Month,
                        dtCompetencia.Value.Year);

                    dao.AplicarExcecoes(c.id_colaborador, dias);

                    foreach (var d in dias)
                    {
                        if (feriados.Contains(d.Data.Date))
                            d.IsFeriado = true;

                        dao.CalcularDia(c.id_colaborador, d);
                    }

                    var linha = new TimeSheetMensalLinha
                    {
                        IdColaborador = c.id_colaborador,
                        Nome = c.nome_colaborador,
                        Dias = dias.ToDictionary(x => x.Data.Day)
                    };

                    linha.TotalMes = linha.Dias.Values.Sum(d => d.ValorDia);
                    _linhas.Add(linha);

                    int row = grdMensal.Rows.Add();
                    grdMensal.Rows[row].Cells["colNome"].Value = linha.Nome;


                    foreach (var dia in linha.Dias)
                    {
                        var cell = grdMensal.Rows[row].Cells[$"D{dia.Key}"];
                        var data = dia.Value.Data;

                        cell.Value = dia.Value.Codigo;
                        cell.Tag = dia.Value.ValorDia;

                        if (feriados.Contains(data.Date))
                        {
                            cell.Style.BackColor = Color.Gold;
                            cell.ToolTipText = "Feriado Nacional";


                            dia.Value.IsFeriado = true;
                        }
                        else if (Enum.TryParse(dia.Value.Codigo, out WorkType tipo))
                        {
                            cell.Style.BackColor = Util.GetColor(tipo);
                        }
                    }

                    grdMensal.Rows[row].Cells["colTotal"].Value = linha.TotalMes;
                }
            }
            finally
            {
                _carregando = false;
            }
        }
        #endregion

        #region EDIÇÃO / DRAG
        private void grdMensal_CellMouseDown(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.RowIndex < 0 || !grdMensal.Columns[e.ColumnIndex].Name.StartsWith("D"))
                return;

            isDragging = true;

            if (Enum.TryParse(grdMensal.Rows[e.RowIndex].Cells[e.ColumnIndex].Value?.ToString(), out WorkType tipo))
                tipoSelecionado = tipo;
        }

        private void grdMensal_CellMouseEnter(object sender, DataGridViewCellEventArgs e)
        {
            if (!isDragging || !tipoSelecionado.HasValue) return;
            if (e.RowIndex < 0) return;
            if (!grdMensal.Columns[e.ColumnIndex].Name.StartsWith("D")) return;

            grdMensal.Rows[e.RowIndex].Cells[e.ColumnIndex].Value =
                tipoSelecionado.Value.ToString();
        }

        private void grdMensal_CellMouseUp(object sender, DataGridViewCellMouseEventArgs e)
        {
            isDragging = false;
            tipoSelecionado = null;

            RecalcularTotais();
        }


        #endregion

        #region TOTAIS
        private void RecalcularTotais()
        {
            for (int i = 0; i < _linhas.Count; i++)
            {
                _linhas[i].TotalMes = _linhas[i].Dias.Values.Sum(d => d.ValorDia);
                grdMensal.Rows[i].Cells["colTotal"].Value = _linhas[i].TotalMes;
            }
        }
        #endregion

        #region SALVAR
        private void btnSalvar_Click(object sender, EventArgs e)
        {
            var dao = new TimeSheetDAO();

            foreach (var linha in _linhas)
            {
                foreach (var dia in linha.Dias.Values)
                    dao.SalvarDia(linha.IdColaborador, dia);
            }

            MessageBox.Show("Timesheet mensal salvo com sucesso!", "Sucesso");
        }
        #endregion

        #region DIÁLOGO HORAS
        private decimal PerguntarHoras(string titulo, DateTime data)
        {
            using (Form f = new Form())
            {
                f.Text = titulo;
                f.Width = 300;
                f.Height = 150;
                f.StartPosition = FormStartPosition.CenterParent;

                Label lbl = new Label
                {
                    Text = $"Horas ({data:dd/MM/yyyy})",
                    Left = 10,
                    Top = 10,
                    Width = 250
                };

                TextBox txt = new TextBox
                {
                    Left = 10,
                    Top = 40,
                    Width = 250,
                    Text = "0"
                };

                Button ok = new Button
                {
                    Text = "OK",
                    Left = 180,
                    Top = 70,
                    DialogResult = DialogResult.OK
                };

                f.Controls.Add(lbl);
                f.Controls.Add(txt);
                f.Controls.Add(ok);
                f.AcceptButton = ok;

                if (f.ShowDialog() == DialogResult.OK && decimal.TryParse(txt.Text.Replace(",", "."), System.Globalization.NumberStyles.Any,
                    System.Globalization.CultureInfo.InvariantCulture, out decimal h))
                    return h;
            }
            return 0;
        }


        #endregion

        private void grdMensal_CellPainting(object sender, DataGridViewCellPaintingEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0)
                return;

            var col = grdMensal.Columns[e.ColumnIndex];

            if (!col.Name.StartsWith("D"))
                return;

            var value = e.Value?.ToString();
            if (string.IsNullOrEmpty(value))
                return;

            if (!Enum.TryParse(value, out WorkType tipo))
                return;

            e.Handled = true;

            Color cor = Util.GetColor(tipo);

            if (_linhas[e.RowIndex].Dias[int.Parse(col.Name.Substring(1))].IsFeriado)
                cor = Color.Gold;

            using (Brush back = new SolidBrush(cor))
                e.Graphics.FillRectangle(back, e.CellBounds);

            e.Paint(e.CellBounds, DataGridViewPaintParts.Border);

            TextRenderer.DrawText(e.Graphics, value, e.CellStyle.Font, e.CellBounds, Color.Black,
                            TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
        }

        #region EXCEL IMPORTAR
        private void btnImportar_Click(object sender, EventArgs e)
        {

        }
        #endregion
    }
}  

