using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;

namespace DAO
{
    public class ColaboradorDAO
    {
        private readonly db_apontamento objBanco;

        public ColaboradorDAO()
        {
            objBanco = new db_apontamento();
        }


        // CREATE
        public void Cadastrar(tb_colaborador obj)
        {
            objBanco.tb_colaborador.Add(obj);
            objBanco.SaveChanges();
        }

        //READ (LISTAR)
        public List<tb_colaborador> Consultar(int idGestor)
        {
            //Lista todos os colaboradores ativos de um gestor, incluindo dados relacionados
            return objBanco.tb_colaborador
                .Include(c => c.tb_operacao)
                .Include(c => c.tb_cargo)
                .Include(c => c.tb_escala)
                .Include(c => c.tb_gcm)
                .Where(c => c.id_gestor == idGestor && c.status_colaborador == true)
                .OrderBy(c => c.nome_colaborador).ToList();
        }

        //READ (POR ID)

        public tb_colaborador BuscarPorId(int idColaborador)
        {
            //Busca um colaborador específico por ID, incluindo dados relacionados
            return objBanco.tb_colaborador
                           .Include(c => c.tb_operacao)
                           .Include(c => c.tb_escala)
                           .Include(c => c.tb_cargo)
                           .Include(c => c.tb_gcm).FirstOrDefault(c => c.id_colaborador == idColaborador);
        }

        //UPDATE/ALTERAR
        public void Alterar(tb_colaborador obj)
        {
            //Busca o colaborador existente no banco de dados
            tb_colaborador objResgate = objBanco.tb_colaborador.First(c => c.id_colaborador == obj.id_colaborador);

            //Atualiza os campos do colaborador com os novos valores
            objResgate.nome_colaborador = obj.nome_colaborador;
            objResgate.id_operacao = obj.id_operacao;
            objResgate.id_cargo = obj.id_cargo;
            objResgate.id_escala = obj.id_escala;
            objResgate.id_gcm = obj.id_gcm;
            objResgate.status_colaborador = obj.status_colaborador;
            //Salva as alterações no banco de dados
            objBanco.SaveChanges();
        }

        //DELETE LÓGICO (INATIVAR)
        public void Inativar(int idColaborador)
        {
            //Busca o colaborador existente no banco de dados
            tb_colaborador objResgate = objBanco.tb_colaborador.First(c => c.id_colaborador == idColaborador);
            //Define o status do colaborador como inativo (false)
            objResgate.status_colaborador = false;
            //Salva as alterações no banco de dados
            objBanco.SaveChanges();
        }

        //Listar Colaboradores Ativos
        public List<tb_colaborador> ListarAtivos(int idGestor)
        {
            //Lista todos os colaboradores ativos de um gestor específico
            return objBanco.tb_colaborador
                .Include(c => c.tb_operacao)
                .Include(c => c.tb_escala)
                .Where(c => c.status_colaborador == true && c.id_gestor == idGestor)
                .OrderBy(c => c.nome_colaborador).ToList();
        }

        #region IMPORTAÇÃO

        public tb_colaborador BuscarPorNome(string nome)
        {
            if (string.IsNullOrWhiteSpace(nome))
                return null;

            nome = nome.Trim();

            return objBanco.tb_colaborador.FirstOrDefault(c => c.nome_colaborador.Trim().ToUpper() == nome.ToUpper());
        }
        ///
        public int CadastrarPeloTimesheet(string nome, string operacaoDescricao, TimeSpan? entry, TimeSpan? lunchIni, TimeSpan? lunchFim,
            TimeSpan? end, string dayOff1, string dayOff2, string sundayOff, int codigoLogado)
        {
            // =========================
            // 1) OPERAÇÃO
            // =========================
            var operacao = objBanco.tb_operacao
                .FirstOrDefault(o => o.nome_operacao == operacaoDescricao);

            if (operacao == null)
            {
                operacao = new tb_operacao
                {
                    nome_operacao = operacaoDescricao
                };
                objBanco.tb_operacao.Add(operacao);
                objBanco.SaveChanges();
            }

            // =========================
            // 2) ESCALA (COM FK id_gestor)
            // =========================

            // garante que o gestor existe no banco atual (evita FK)
            if (!objBanco.tb_gestor.Any(g => g.id_gestor == codigoLogado))
            {
                var dbName = objBanco.Database.Connection.Database;
                var server = objBanco.Database.Connection.DataSource;
                throw new Exception($"Gestor id={codigoLogado} não existe em tb_gestor. Conectado em: {server} | {dbName}");
            }

            // defaults de horário se vier vazio
            var horaEntrada = entry ?? TimeSpan.Zero;
            var horaSaida = end ?? TimeSpan.Zero;
            var almocoIni = lunchIni ?? TimeSpan.Zero;
            var almocoFim = lunchFim ?? TimeSpan.Zero;

            int do1 = ConverterDiaSemanaSeguro(dayOff1, (int)DayOfWeek.Saturday);
            int do2 = ConverterDiaSemanaSeguro(dayOff2, (int)DayOfWeek.Sunday);
            int? domingoOff = ConverterDomingoOffSeguro(sundayOff);

            // se você tiver "HC/FC/MD" do Excel, use aqui. Senão, define padrão:
            string tipoEscala = "HC"; // padrão seguro

            var escala = new tb_escala
            {
                // 🔥 ESSENCIAL pro FK
                id_gestor = codigoLogado,

                // campos obrigatórios
                descricao = $"Escala importada - {horaEntrada:hh\\:mm} às {horaSaida:hh\\:mm}",
                tipo_escala = tipoEscala,

                // horários
                hora_entrada = horaEntrada,
                hora_almoco_inicio = almocoIni,
                hora_almoco_fim = almocoFim,
                hora_saida = horaSaida,

                // dayoffs
                dayoff_1 = do1,
                dayoff_2 = do2,
                domingo_off = domingoOff,

                // existe na sua tabela e pode ser NOT NULL
                ativo = true
            };

            objBanco.tb_escala.Add(escala);
            objBanco.SaveChanges();


            // =========================
            // 3) COLABORADOR
            // =========================
            var colaborador = new tb_colaborador
            {
                nome_colaborador = nome,
                id_operacao = operacao.id_operacao,
                id_escala = escala.id_escala,

                //PADRÕES QUANDO VEM DO TIMESHEET
                id_gcm = 1,
                id_cargo = 1,

                id_gestor = codigoLogado,
                status_colaborador = true
            };

            objBanco.tb_colaborador.Add(colaborador);
            objBanco.SaveChanges();

            return colaborador.id_colaborador;
        }

        // ===========================
        // Helpers (parsing)
        // ===========================
        private int ConverterDiaSemanaSeguro(string dia, int fallback)
        {
            if (string.IsNullOrWhiteSpace(dia))
                return fallback;

            dia = dia.Trim();

            // tenta inglês/enum direto (Saturday, Sunday, etc)
            if (Enum.TryParse(dia, true, out DayOfWeek dow))
                return (int)dow;

            // tenta pt-br básico
            string s = dia.ToLowerInvariant();
            if (s.StartsWith("dom")) return (int)DayOfWeek.Sunday;
            if (s.StartsWith("seg")) return (int)DayOfWeek.Monday;
            if (s.StartsWith("ter") || s.StartsWith("tue")) return (int)DayOfWeek.Tuesday;
            if (s.StartsWith("qua") || s.StartsWith("wed")) return (int)DayOfWeek.Wednesday;
            if (s.StartsWith("qui") || s.StartsWith("thu")) return (int)DayOfWeek.Thursday;
            if (s.StartsWith("sex") || s.StartsWith("fri")) return (int)DayOfWeek.Friday;
            if (s.StartsWith("sáb") || s.StartsWith("sab") || s.StartsWith("sat")) return (int)DayOfWeek.Saturday;

            return fallback;
        }

        private int? ConverterDomingoOffSeguro(string valor)
        {
            if (string.IsNullOrWhiteSpace(valor))
                return null;

            valor = valor.Trim().Replace("º", "").Replace("°", "");

            // "Nenhum"
            if (valor.Equals("Nenhum", StringComparison.OrdinalIgnoreCase))
                return null;

            // "1 Domingo", "1"
            var digits = new string(valor.Where(char.IsDigit).ToArray());
            if (int.TryParse(digits, out int n) && n >= 1 && n <= 5)
                return n;

            return null;
        }
        #endregion
    }
}
