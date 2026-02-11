using ClosedXML.Excel;
using Core.Enums;
using Core.VO;
using System;
using System.Collections.Generic;
using System.Data.Entity.Validation;
using System.Linq;
using System.Text;

namespace DAO
{
    public class TimeSheetDAO
    {
        private readonly db_apontamento objBanco;

        public TimeSheetDAO()
        {
            objBanco = new db_apontamento();
        }

        #region GERAR MÊS
        //Gera a estrutura de dias de um mês para um colaborador
        public List<TimeSheetDiaVO> GerarMes(int idColaborador, int mes, int ano)
        {
            var lista = new List<TimeSheetDiaVO>();

            // Busca o colaborador e carrega a escala
            var colaborador = objBanco.tb_colaborador
                .Include("tb_escala")
                .FirstOrDefault(c => c.id_colaborador == idColaborador);

            int diasNoMes = DateTime.DaysInMonth(ano, mes);

            // Se não achou colaborador ou não tem escala, ainda assim gera o mês sem horários
            var escala = colaborador?.tb_escala;
            if (colaborador == null || escala == null)
            {
                for (int d = 1; d <= diasNoMes; d++)
                {
                    DateTime data = new DateTime(ano, mes, d);

                    lista.Add(new TimeSheetDiaVO
                    {
                        Data = data,
                        DiaSemana = data.DayOfWeek.ToString(),

                        // Sem escala, assume WD (ou DO se você quiser outra regra)
                        Codigo = WorkType.WD.ToString(),

                        HoraEntrada = null,
                        HoraAlmocoInicio = null,
                        HoraAlmocoFim = null,
                        HoraSaida = null,

                        HorasTrabalhadas = 0m,
                        BancoHoras = 0m,
                        ValorDia = 0m,
                        HorasAtestado = 0m,
                        AdicionalFeriado = 0m,

                        PossuiExcecao = false,
                        IsFeriado = false
                    });
                }

                return lista;
            }

            // Com escala: monta o mês usando os horários padrão
            for (int d = 1; d <= diasNoMes; d++)
            {
                DateTime data = new DateTime(ano, mes, d);

                // Folga conforme escala
                bool isDayOff =
                    (int)data.DayOfWeek == escala.dayoff_1 ||
                    (int)data.DayOfWeek == escala.dayoff_2;

                lista.Add(new TimeSheetDiaVO
                {
                    Data = data,
                    DiaSemana = data.DayOfWeek.ToString(),

                    // DO = folga / WD = dia normal
                    Codigo = isDayOff ? WorkType.DO.ToString() : WorkType.WD.ToString(),

                    // Horários padrão da escala
                    HoraEntrada = escala.hora_entrada,
                    HoraAlmocoInicio = escala.hora_almoco_inicio,
                    HoraAlmocoFim = escala.hora_almoco_fim,
                    HoraSaida = escala.hora_saida,

                    // Campos calculados começam zerados (cálculo é feito depois no CalcularDia)
                    HorasTrabalhadas = 0m,
                    BancoHoras = 0m,
                    ValorDia = 0m,
                    HorasAtestado = 0m,
                    AdicionalFeriado = 0m,

                    PossuiExcecao = false,
                    IsFeriado = false
                });
            }

            return lista;
        }
        #endregion

        #region EXCEÇÕES
        //Aplica exceções de horários/códigos salvos no banco
        public void AplicarExcecoes(int idColaborador, List<TimeSheetDiaVO> dias)
        {
            //Monta uma lista datas contendo somente as datas de cada item em dias.
            var datas = dias.Select(d => d.Data).ToList();

            //Consulta o banco (tb_timesheet) e traz para memória (ToList()):
            var excecoes = objBanco.tb_timesheet
                .Where(t => t.id_colaborador == idColaborador && datas.Contains(t.data))
                .ToList();

            //Percorre cada item da lista dias para aplicar a exceção correspondente.
            foreach (var dia in dias)
            {
                //Procura a primeira exceção na lista excecoes cuja data seja igual à dia.Data.
                //Se achar, ts recebe o registro.
                //Se não achar, ts vira null.
                var ts = excecoes.FirstOrDefault(e => e.data == dia.Data);
                //Se não existe exceção para aquele dia, pula para o próximo(continue).
                if (ts == null) continue;

                // ⚠️ SÓ sobrescreve se tiver valor
                if (ts.hora_entrada.HasValue)
                    dia.HoraEntrada = ts.hora_entrada;

                if (ts.hora_saida.HasValue)
                    dia.HoraSaida = ts.hora_saida;

                if (ts.hora_almoco_inicio.HasValue)
                    dia.HoraAlmocoInicio = ts.hora_almoco_inicio;

                if (ts.hora_almoco_fim.HasValue)
                    dia.HoraAlmocoFim = ts.hora_almoco_fim;
                //Se tipo_apontamento no banco não é nulo, nem vazio, nem só espaços, então sobrescreve dia.Codigo.
                if (!string.IsNullOrWhiteSpace(ts.tipo_apontamento))
                    dia.Codigo = ts.tipo_apontamento;
                //Marca no objeto do dia que existe exceção aplicada.
                dia.PossuiExcecao = true;
            }
        }
        #endregion

        #region CÁLCULO DE HORAS
        //Calcula horas trabalhadas considerando almoço
        public decimal CalcularHoras(TimeSpan? entrada, TimeSpan? saida, TimeSpan? almocoIni, TimeSpan? almocoFim)
        {
            //Se entrada ou saída estiverem null, não dá pra calcular. Então retorna 0m (0 decimal).
            if (!entrada.HasValue || !saida.HasValue)
                return 0m;

            // Diferença bruta
            //Calcula a diferença entre saída e entrada (tempo total bruto).
            //Usa.Value porque já garantiu que não é null.
            TimeSpan total = saida.Value - entrada.Value;

            // Se ficou negativo, pode ser turno virando o dia (ex.: 22:00 -> 06:00)
            // Se isso NÃO existir no seu negócio, em vez disso você pode retornar 0 ou lançar erro.
            if (total.TotalMinutes < 0)
                total = total.Add(TimeSpan.FromHours(24));

            // Desconta almoço apenas se for válido
            //Só tenta descontar almoço se os dois horários do almoço existirem (não forem null).
            if (almocoIni.HasValue && almocoFim.HasValue)
            {
                //Calcula a duração do almoço.
                TimeSpan almoco = almocoFim.Value - almocoIni.Value;

                // Se almoço ficou negativo por qualquer motivo, ignora
                //Se a duração do almoço for positiva (ou seja, válida), então desconta do total.
                //Se for 0 ou negativa(ex.: horários invertidos), ignora o almoço.
                if (almoco.TotalMinutes > 0)
                    total -= almoco;
            }

            // Não deixa negativo por conta de almoço maior que o total
            //Se depois de descontar almoço o total ficou negativo (ex.: almoço maior que o tempo trabalhado), força para zero.
            if (total.TotalMinutes < 0)
                total = TimeSpan.Zero;

            //Retorna o total em horas (pode ter casas decimais), convertido para decimal.
            return (decimal)total.TotalHours; // decimal real
        }
        #endregion

        #region CÁLCULO DO DIA
        //Calcula valores de um único dia
        public void CalcularDia(int idColaborador, TimeSheetDiaVO dia, decimal horasAusenciaUA = 0, decimal horasAtestado = 0)
        {
            //Busca colaborador
            var colaborador = objBanco.tb_colaborador.FirstOrDefault(c => c.id_colaborador == idColaborador);

            //Busca escala
            var escala = objBanco.tb_escala.FirstOrDefault(e => e.id_escala == colaborador.id_escala);

            //Busca GCM (valor da hora)
            var gcm = objBanco.tb_gcm.FirstOrDefault(g => g.id_gcm == colaborador.id_gcm);

            //Converte código do dia para enum
            Enum.TryParse(dia.Codigo, out WorkType tipo);

            //Converte o valor da hora para decimal.
            decimal valorHora = Convert.ToDecimal(gcm.valor_hora);

            //Calcula a jornada diária com base na escala.
            decimal jornadaDia = CalcularHoras(escala.hora_entrada, escala.hora_saida, escala.hora_almoco_inicio, escala.hora_almoco_fim);
            //jornadaDecimal é usada para cálculos posteriores.
            decimal jornadaDecimal = jornadaDia;

            //Zera valores
            dia.HorasTrabalhadas = 0;
            dia.BancoHoras = 0;
            dia.ValorDia = 0;
            dia.HorasAtestado = 0;
            dia.AdicionalFeriado = 0;

            //Verifica folga
            bool isDayOff =
                (int)dia.Data.DayOfWeek == escala.dayoff_1 ||
                (int)dia.Data.DayOfWeek == escala.dayoff_2;

            //Se for folga mas veio como dia trabalhado (WD),
            //converte automaticamente para Day Off(DO).
            if (isDayOff && tipo == WorkType.WD)
                tipo = WorkType.DO;

            if (tipo == WorkType.DO || tipo == WorkType.DN || tipo == WorkType.SL)
            {
                dia.HoraEntrada = null;
                dia.HoraSaida = null;
                dia.HoraAlmocoInicio = null;
                dia.HoraAlmocoFim = null;
            }
            //SL → desligamento
            if (tipo == WorkType.SL)
                return;

            //AL → férias (NÃO dobra em feriado)
            if (tipo == WorkType.AL)
            {
                decimal valorDia = jornadaDia * valorHora;
                dia.ValorDia = valorDia + (valorDia / 3);
                return;
            }

            //BT → banco de horas
            //if (tipo == WorkType.BT)
            //{
            //    decimal horasDecimal = CalcularHoras(dia.HoraEntrada, dia.HoraSaida, dia.HoraAlmocoInicio, dia.HoraAlmocoFim);

            //    dia.BancoHoras = ConverterDecimalParaHoraUsuario(horasDecimal);
            //    dia.ValorDia = horasDecimal * valorHora;
            //    return;
            //}
            if (tipo == WorkType.BT)
            {
                //Calcula quantas horas foram apontadas no dia.
                decimal horasDecimal = CalcularHoras(dia.HoraEntrada, dia.HoraSaida, dia.HoraAlmocoInicio, dia.HoraAlmocoFim);

                //BT deve DESCONTAR do banco (negativo)
                dia.BancoHoras = -ConverterDecimalParaHoraUsuario(horasDecimal);

                // normalmente BT não muda pagamento do dia (depende da sua regra).
                // Se você quiser manter como estava:
                dia.ValorDia = horasDecimal * valorHora;

                return;
            }

            //UA → falta injustificada
            if (tipo == WorkType.UA)
            {
                decimal ausenciaDecimal = horasAusenciaUA; // já é decimal real

                if (ausenciaDecimal > jornadaDecimal)
                    throw new Exception("Horas de ausência excedem a jornada diária.");

                decimal horasTrabalhadasDecimal = jornadaDecimal - ausenciaDecimal;

                dia.HorasTrabalhadas = ConverterDecimalParaHoraUsuario(horasTrabalhadasDecimal);
                dia.BancoHoras = -ConverterDecimalParaHoraUsuario(ausenciaDecimal);
                dia.ValorDia = horasTrabalhadasDecimal * valorHora;
                return;
            }

            //ML → atestado médico
            if (tipo == WorkType.ML)
            {
                decimal atestadoDecimal = horasAtestado; // ✅ parâmetro correto

                decimal horasTrabalhadasDecimal = jornadaDecimal - atestadoDecimal;
                if (horasTrabalhadasDecimal < 0)
                    horasTrabalhadasDecimal = 0;

                dia.HorasAtestado = ConverterDecimalParaHoraUsuario(atestadoDecimal);
                dia.HorasTrabalhadas = ConverterDecimalParaHoraUsuario(horasTrabalhadasDecimal);

                // normalmente atestado paga a jornada cheia
                dia.ValorDia = jornadaDecimal * valorHora;
                return;
            }

            //Códigos remunerados normais
            if (tipo == WorkType.WD || tipo == WorkType.TR || tipo == WorkType.SC || tipo == WorkType.DN)
            {
                decimal horasDecimal = CalcularHoras(dia.HoraEntrada, dia.HoraSaida, dia.HoraAlmocoInicio, dia.HoraAlmocoFim);

                decimal bancoDecimal = horasDecimal - jornadaDecimal;

                dia.HorasTrabalhadas = ConverterDecimalParaHoraUsuario(horasDecimal);
                dia.BancoHoras = ConverterDecimalParaHoraUsuario(bancoDecimal);
                dia.ValorDia = horasDecimal * valorHora;

                //Editar aqui se for remunerar em dobro feriados
                if (dia.IsFeriado && tipo == WorkType.WD || dia.IsFeriado && tipo == WorkType.TR)
                {
                    dia.AdicionalFeriado = dia.ValorDia;
                    dia.ValorDia += dia.AdicionalFeriado;
                }
            }
        }
        #endregion

        #region SALVAR
        //Salva ou atualiza um dia no banco
        //Esse método salva ou atualiza o apontamento diário de um colaborador.
        //Ele verifica se já existe um registro para o dia, cria se não existir, atualiza os horários e salva no banco usando Entity Framework, tratando erros de validação.”
        public void SalvarDia(int idColaborador, TimeSheetDiaVO dia)
        {
            //Busca no banco um registro de timesheet:
            var reg = objBanco.tb_timesheet
                .FirstOrDefault(t => t.id_colaborador == idColaborador && t.data == dia.Data);

            //Verifica se não existe registro para esse colaborador nesse dia.
            if (reg == null)
            {
                //Cria um novo registro de timesheet com colaborador e data.
                reg = new tb_timesheet
                {
                    id_colaborador = idColaborador,
                    data = dia.Data
                };
                //Marca o novo registro para inserção no banco.
                objBanco.tb_timesheet.Add(reg);
            }

            //Atualiza os campos do registro com os dados vindos do VO
            reg.hora_entrada = dia.HoraEntrada;
            reg.hora_saida = dia.HoraSaida;
            reg.hora_almoco_inicio = dia.HoraAlmocoInicio;
            reg.hora_almoco_fim = dia.HoraAlmocoFim;
            reg.tipo_apontamento = dia.Codigo;

            try
            {
                //Tenta salvar as alterações no banco de dados.
                objBanco.SaveChanges();
            }
            //Captura erros de validação do Entity Framework, como:
            catch (DbEntityValidationException ex)
            {
                //Cria uma mensagem de erro detalhada.
                var sb = new StringBuilder();
                sb.AppendLine("ERRO DE VALIDAÇÃO EF:");

                //Lista campo por campo que falhou na validação e o motivo.
                foreach (var eve in ex.EntityValidationErrors)
                {
                    sb.AppendLine($"Entidade: {eve.Entry.Entity.GetType().Name} | State: {eve.Entry.State}");
                    foreach (var ve in eve.ValidationErrors)
                        sb.AppendLine($" - Campo: {ve.PropertyName} | Erro: {ve.ErrorMessage}");
                }
                // joga tudo na exception para a UI mostrar
                //Lança uma nova exception com mensagem detalhada,
                throw new Exception(sb.ToString(), ex);
            }
        }
        #endregion

        #region importe excel Calculos horas EXCEL
        //1.5 → 1.3 (para exibição)
        //Declara um método privado que recebe um número decimal e retorna outro decimal.
        private decimal ConverterDecimalParaHoraUsuario(decimal valor)
        {
            //Converte a parte inteira do decimal para int.
            int h = (int)valor;
            //Calcula os minutos:
            //Subtrai as horas do valor total → pega só a parte decimal
            //Multiplica por 60 para converter em minutos
            //Usa Math.Round para arredondar
            //Converte para int
            int m = (int)Math.Round((valor - h) * 60);
            //Monta o valor final no formato decimal de hora:
            return h + (m / 100m);
        }
        public List<TimeSheetDiaVO> ImportarExcelTimeSheet(string caminho)
        {
            //Cria a lista que vai receber os dados
            //Aqui é onde cada linha do Excel vai virar um item da lista.
            var lista = new List<TimeSheetDiaVO>();

            //Abre o arquivo Excel usando ClosedXML
            using (var wb = new XLWorkbook(caminho))
            {
                //Seleciona a planilha
                var ws = wb.Worksheet("Timesheet");

                //Começa a ler a partir da linha 2
                int linha = 2; // pula cabeçalho

                //Loop: lê linha por linha até a coluna A ficar vazia
                //Enquanto tiver data na coluna 1 (A), o loop continua.
                while (!ws.Cell(linha, 1).IsEmpty())
                {
                    //Cada linha do Excel vira um objeto.
                    var dia = new TimeSheetDiaVO();

                    //Lê a data
                    //Coluna A → Data do dia trabalhado.
                    dia.Data = ws.Cell(linha, 1).GetDateTime();

                    //Daqui para baixo precisa de um método auxiliar para converter para "TimeSpan?"
                    //Lê os horários
                    //O método ConverterParaTimeSpan transforma o valor do Excel em TimeSpan?.
                    dia.HoraEntrada = ConverterParaTimeSpan(ws.Cell(linha, 3));
                    dia.HoraAlmocoInicio = ConverterParaTimeSpan(ws.Cell(linha, 4));
                    dia.HoraAlmocoFim = ConverterParaTimeSpan(ws.Cell(linha, 5));
                    dia.HoraSaida = ConverterParaTimeSpan(ws.Cell(linha, 6));

                    //Lê um código(UA / ML, por exemplo)
                    dia.Codigo = ws.Cell(linha, 7).GetString();


                    //Tenta ler um número decimal da coluna J
                    //Se conseguir, usa o valor
                    //Se não, coloca 0
                    // UA / ML
                    dia.HorasAtestado = ws.Cell(linha, 10).TryGetValue(out decimal horas) ? horas : 0;

                    //Adiciona o dia na lista
                    lista.Add(dia);
                    //Vai para próxima linha do Excel
                    linha++;
                }
            }

            return lista;
        }

        //Método auxiliar para converter célula do Excel para TimeSpan?
        private TimeSpan? ConverterParaTimeSpan(IXLCell cell)
        {

            //Verifica se a célula é nula ou vazia.
            if (cell == null || cell.IsEmpty())
                return null;

            // Caso o Excel esteja como horário (DateTime)
            //Se o Excel interpretou o valor como DateTime, o código:
            // Obtém a data / hora da célula
            //Extrai apenas a parte de hora(TimeOfDay)
            //Retorna como TimeSpan
            if (cell.DataType == XLDataType.DateTime)
                return cell.GetDateTime().TimeOfDay;

            // Caso venha como string (ex: "08:00")
            //Se o valor não for DateTime, tenta converter a célula como string.
            if (TimeSpan.TryParse(cell.GetString(), out TimeSpan ts))
                return ts;

            //Caso nenhuma conversão funcione, retorna null indicando valor inválido ou não reconhecido.
            return null;
        }
        #endregion
    }
}
