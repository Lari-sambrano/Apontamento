using Apontamento.Comum;
using Core.VO;
using DAO;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using static Apontamento.Comum.Util;

namespace Apontamento
{
    public partial class frmSubirOnline : Form
    {
        public frmSubirOnline()
        {
            InitializeComponent();
        }


        #region Métodos
        private bool ValidarCampos()
        {
            if (cbTela.SelectedIndex == -1)
            {
                MessageBox.Show("Selecione uma tela.");
                return false;
            }

            return true;
        }
        private void CarregarXML()
        {
            grdOffline.DataSource = null;
            btnGravarOnline.Visible = false;

            if (!ValidarCampos())
                return;

            XmlDocument xml = new XmlDocument();

            switch ((Util.Tela)cbTela.SelectedIndex)
            {
                case Util.Tela.Colaborador:
                    break;

                case Util.Tela.Apontamento:
                    if (File.Exists(Util.PathFileXML(Util.Tela.Apontamento)))
                    {
                        DataSet ds = new DataSet();
                        ds.ReadXml(Util.PathFileXML(Util.Tela.Apontamento));

                        if (ds != null && ds.Tables.Count > 0)
                        {
                            grdOffline.DataSource = ds.Tables[0];
                            btnGravarOnline.Visible = true;
                        }
                    }
                    break;

                case Util.Tela.WorkSchedule:
                    break;
            }

        }
        private void Cadastrar()
        {
            switch ((Util.Tela)cbTela.SelectedIndex)
            {
                case Util.Tela.Colaborador:
                    break;

                case Util.Tela.Apontamento:
                    TimeSheetDAO dao = new TimeSheetDAO();
                    XmlDocument xml = new XmlDocument();
                    xml.Load(Util.PathFileXML(Util.Tela.Apontamento));

                    foreach (DataGridViewRow row in grdOffline.Rows)
                    {
                        if (row.IsNewRow) continue;

                        int idColaborador = Convert.ToInt32(row.Cells["IdColaborador"].Value);
                        DateTime competencia = Convert.ToDateTime(row.Cells["Competencia"].Value);

                        // Localiza o Item no XML
                        XmlNode item = xml.SelectSingleNode(
                            $"//Item[IdColaborador='{idColaborador}' and Competencia='{competencia:yyyy-MM}']");

                        if (item == null) continue;

                        XmlNodeList dias = item.SelectNodes("Dias/Dia");

                        foreach (XmlNode d in dias)
                        {
                            TimeSheetDiaVO dia = new TimeSheetDiaVO
                            {
                                Data = Convert.ToDateTime(d["Data"].InnerText),
                                Codigo = d["Codigo"].InnerText
                            };

                            // Campos opcionais
                            if (d["HorasAtestado"] != null)
                                dia.HorasAtestado = Convert.ToDecimal(d["HorasAtestado"].InnerText);

                            // Calcula tudo corretamente
                            dao.CalcularDia(idColaborador, dia);
                            dao.SalvarDia(idColaborador, dia);
                        }

                        // Remove o item após subir
                        item.ParentNode.RemoveChild(item);
                    }

                    // Salva XML atualizado
                    xml.Save(Util.PathFileXML(Util.Tela.Apontamento));


                    MessageBox.Show("Timesheet sincronizado com sucesso!");
                    break;

                case Util.Tela.WorkSchedule:
                    break;
            }
        }
        private void ExcluirItemXML(int idColaborador, DateTime data)
        {
            XmlDocument xml = new XmlDocument();

            if (!File.Exists(Util.PathFileXML(Util.Tela.Apontamento)))
                return;

            xml.Load(Util.PathFileXML(Util.Tela.Apontamento));

            XmlNode no = xml.SelectSingleNode(
                $"//Item[IdColaborador='{idColaborador}' and Data='{data:yyyy-MM-dd}']"
            );

            if (no != null)
            {
                no.ParentNode.RemoveChild(no);
                xml.Save(Util.PathFileXML(Util.Tela.Apontamento));
            }
        }
        private void Limpar()
        {
            cbTela.SelectedIndex = -1;
            grdOffline.DataSource = null;
            btnGravarOnline.Visible = false;
        }

        #endregion

        #region Eventos

        #endregion

        private void frmSubirOnline_Load(object sender, EventArgs e)
        {
            Limpar();
        }

        private void cbTela_SelectedIndexChanged(object sender, EventArgs e)
        {
            CarregarXML();
        }

        private void btnGravarOnline_Click(object sender, EventArgs e)
        {
            Cadastrar();
            CarregarXML();
        }
    }
}
