using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Apontamento.Comum
{
    public class Mensagem
    {
        public static class Mensagens
        {
            //constante não muda o valor
            public const string MSG_OBRIGATORIA = "Revise o(s) campo(s):\n ";
            public const string MSG_SUCESSO = "Operação realizada com sucesso";
            public const string MSG_ERRO = "Ocorreu um erro na operação";
            public const string MSG_CONFIRMACAO = "Deseja excluir o registro?";
            public const string MSG_EMAIL_DUPLICADO = "Email já existente!";
            public const string MSG_EMAIL_INVALIDO = "Email inválido!";
            public const string MSG_REGISTRO_NAO_ENCONTRADO = "Registro não encontrado";
            public const string MSG_USUARIO_NAO_ENCONTRADO = "Usuário não encontrado";
            public const string MSG_ITEM_JA_CADASTRADO = "Item já cadastrado!";

        }

    }
}
