using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using static Apontamento.Comum.Mensagem;

namespace Apontamento.Comum
{
    public static class Util
    {

        public static string pathFileXml = "C:\\Curso C#\\arquivo xml\\xml";



        //statica são do médoto, são dependente a classe, não precisa stanciar
        public enum TipoMensagem
        {
            Erro,
            Sucesso,
            Obrigatorio,
            Confirmacao,
            EmailDuplicado,
            EmailIncorreto,
            RegistroNaoEncontrado,
            UsuarioNaoEncontrado,
            ItemJaCadastrado
        }

        public enum EstadoTela
        {
            Nova,
            Edicao

        }

        public enum Tela
        {
            Colaborador,
            Apontamento,
            WorkSchedule
        }

        private const string pathXml = @"C:\Users\jadso\Desktop\Apontamento_Teste\XML\";
        private const string FileColaborador = "Colaborador.xml";
        private const string FileApontamento = "Apontamento.xml";
        private const string FileWorkSchedule = "WorkSchedule.xml";

        public static string PathFileXML(Tela tela)
        {
            string path = string.Empty;
            switch (tela)
            {
                case Tela.Colaborador:
                    path = pathXml + FileColaborador;
                    break;
                case Tela.Apontamento:
                    path = pathXml + FileApontamento;
                    break;
                case Tela.WorkSchedule:
                    path = pathXml + FileWorkSchedule;
                    break;
            }
            return path;

        }

        public static void ConfigurarBotoesTela(EstadoTela estado, Button btnCadastrar, Button btnAlterar, Button btnExcluir)
        {
            switch (estado)
            {
                case EstadoTela.Nova:
                    btnCadastrar.Enabled = true;
                    btnAlterar.Enabled = false;
                    btnExcluir.Enabled = false;

                    btnCadastrar.BackColor = Color.DodgerBlue;
                    btnAlterar.BackColor = Color.Gray;
                    btnExcluir.BackColor = Color.Gray;


                    break;
                case EstadoTela.Edicao:
                    btnCadastrar.Enabled = false;
                    btnAlterar.Enabled = true;
                    btnExcluir.Enabled = true;

                    btnCadastrar.BackColor = Color.Gray;
                    btnAlterar.BackColor = Color.Blue;
                    btnExcluir.BackColor = Color.Red;
                    break;

            }

        }

        //se não passar o valor de campos não tem problema, pois´já assume com vazio, pois tipamos ele como receber vazio, 
        //para não ser obrigatório passar ele .
        public static bool MostrarMensagem(TipoMensagem tipo, string campos = "")
        {
            //adaptado para poder ter trabalhado a informação de retorno ou não
            bool flag = true;
            switch (tipo)
            {
                case TipoMensagem.Erro:
                    MessageBox.Show(Mensagens.MSG_ERRO, "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);

                    break;
                case TipoMensagem.Sucesso:
                    MessageBox.Show(Mensagens.MSG_SUCESSO, "Sucesso", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    break;
                case TipoMensagem.Obrigatorio:
                    MessageBox.Show(Mensagens.MSG_OBRIGATORIA + campos, "Atenção", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    break;
                case TipoMensagem.Confirmacao:
                    flag = MessageBox.Show(Mensagens.MSG_CONFIRMACAO + campos, "Confirmação", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes;
                    break;
                case TipoMensagem.EmailDuplicado:
                    MessageBox.Show(Mensagens.MSG_EMAIL_DUPLICADO, "Atenção", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    break;
                case TipoMensagem.EmailIncorreto:
                    MessageBox.Show(Mensagens.MSG_EMAIL_INVALIDO, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    break;
                case TipoMensagem.RegistroNaoEncontrado:
                    MessageBox.Show(Mensagens.MSG_REGISTRO_NAO_ENCONTRADO, "Resultado", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    break;
                case TipoMensagem.UsuarioNaoEncontrado:
                    MessageBox.Show(Mensagens.MSG_USUARIO_NAO_ENCONTRADO, "Resultado", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    break;
                case TipoMensagem.ItemJaCadastrado:
                    MessageBox.Show(Mensagens.MSG_ITEM_JA_CADASTRADO, "Atenção", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    break;
            }
            return flag;

        }


        //Tradamento das GRIDVIWE para não ter que tratar todas elas
        //GENÉRICO DA CONFIGURAÇÃO.

        public static void ConfigurarGrid(DataGridView grd)
        {
            //grd.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            //grd.ReadOnly = true;
            //grd.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            //grd.MultiSelect = false;
            //grd.AllowUserToAddRows = false;


            //grdJourney

            grd.RowHeadersVisible = true;
            grd.ColumnHeadersVisible = false;
            grd.AllowUserToAddRows = false;
            grd.AllowUserToResizeRows = false;
            grd.AllowUserToResizeColumns = false;
            grd.AllowUserToDeleteRows = false;

            //
            grd.AllowUserToAddRows = false;

            // Centraliza o texto da coluna de horas (RowHeader)
            grd.RowHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;

            // Centraliza o conteúdo das células
            grd.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;

            //Define o tamanho/largura das colunas
            grd.RowHeadersWidth = 300; // <- ajusta o tamanho da coluna com o horario
            grd.ColumnCount = 1;
            grd.Columns[0].Name = "Journey";
            grd.Columns[0].Width = 250;



            //
            grd.SelectionMode = DataGridViewSelectionMode.CellSelect;
            grd.MultiSelect = false;

            grd.Columns.Clear();

            grd.Columns.Add("status", "Situação");
            grd.Columns[0].Width = grd.Width - 3;

            grd.RowHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            grd.RowHeadersWidthSizeMode = DataGridViewRowHeadersWidthSizeMode.DisableResizing;
            grd.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
        }
        public static string CriarSenha(string palavra)
        {
            //verificar qual parametro quero encontrar, no caso é o @
            //pega a palavra que vai chegar, pegando pelo @ na posição 0.
            //ele quebra ao encontrar o @ e pega a letra da 1° posição.
            // o ' significa char que é somente 1 caracter
            //a senha fica o que está antes do arroba
            string senha = palavra.Split('@')[0];
            //cripitografa na mesmo método
            return CriptografarSenha(senha);
        }

        public static bool VerificarDigitoEmail(string email)
        {
            string strModelo = "^([0-9a-zA-Z]([-.\\w]*[0-9a-zA-Z])*@([0-9a-zA-Z][-\\w]*[0-9a-zA-Z]\\.)+[a-zA-Z]{2,9})$";
            if (System.Text.RegularExpressions.Regex.IsMatch(email, strModelo))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        public static void ConfigurarCombo(ComboBox combo, string display, string value)
        {
            combo.DisplayMember = display;
            combo.ValueMember = value;
        }

        public static string CriptografarSenha(string password)
        {
            //sha256 é a hash de segurança
            using (var sha256 = SHA256.Create())
            {
                // Converte a senha para um array de bytes
                var passwordBytes = Encoding.UTF8.GetBytes(password);

                // Gera o hash da senha
                var hash = sha256.ComputeHash(passwordBytes);

                // Retorna o hash como string Base64
                return Convert.ToBase64String(hash);
            }
        }

        public static bool VerificarSenha(string senhaDigitada, string senhaHash)
        {
            // Gera o hash da senha fornecida
            string hashedPassword = CriptografarSenha(senhaDigitada);

            // Compara o hash gerado com o hash armazenado
            return hashedPassword == senhaHash;
        }


        //Projeto Apontamento


        public static Color GetColor(Core.Enums.WorkType type)
        {
            switch (type)
            {
                case Core.Enums.WorkType.WD: return Color.White;
                case Core.Enums.WorkType.DO: return Color.DarkGray;
                case Core.Enums.WorkType.AL: return Color.Blue;
                case Core.Enums.WorkType.BT: return Color.Green;
                case Core.Enums.WorkType.ML: return Color.Orange;
                case Core.Enums.WorkType.SC: return Color.Purple;
                case Core.Enums.WorkType.TR: return Color.Yellow;
                case Core.Enums.WorkType.UA: return Color.Red;
                case Core.Enums.WorkType.SL: return Color.Black;
                case Core.Enums.WorkType.DN: return Color.Violet;
                default: return Color.White;
            }
        }
        public static void RestringirNumeros(object sender, KeyPressEventArgs e)
        {
            //Permite apenas numeros e a tecla backspace no textbox
            if (char.IsDigit(e.KeyChar) || e.KeyChar.Equals((char)Keys.Back))
            {
                //seta o textbox como sender
                TextBox t = (TextBox)sender;
                //Limpa pontos e virgulas do textbox
                string w = Regex.Replace(t.Text, "[^0-9]", string.Empty);
                //Se apagar todo o conteúdo do campo, a string recebe "00"
                if (w == string.Empty) w = "00";
                //Se o backspace for acionado
                if (e.KeyChar.Equals((char)Keys.Back))
                    //remove o caracter mais a direita
                    w = w.Substring(0, w.Length - 1);
                else
                    w += e.KeyChar;
                //Formata o texto para unidade monetaria
                t.Text = string.Format("{0:#,##0.00}", Double.Parse(w) / 100);
                //Faz o cursor ficar a direita
                t.Select(t.Text.Length, 0);
                //return;
            }
            e.Handled = true;
        }


    }
}

