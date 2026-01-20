using Core.Enums;
using Core.VO;
using System;
using System.Collections.Generic;
using System.Linq;

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
            //Lista que irá conter todos os dias do mês
            var lista = new List<TimeSheetDiaVO>();

            //Busca o colaborador e já carrega a escala relacionada
            var colaborador = objBanco.tb_colaborador.Include("tb_escala").FirstOrDefault(c => c.id_colaborador == idColaborador);

            //Obtem a escala do colaborador
            var escala = colaborador.tb_escala;
            //Calcula quantos dias o mês possui
            int diasNoMes = DateTime.DaysInMonth(ano, mes);

            //Percorre todos os dias do mês
            for (int d = 1; d <= diasNoMes; d++)
            {
                //Monta a data completa
                DateTime data = new DateTime(ano, mes, d);

                //Verifica se o dia é folga conforme a escala
                bool isDayOff = (int)data.DayOfWeek == escala.dayoff_1 || (int)data.DayOfWeek == escala.dayoff_2;

                //Adiciona o dia na lista
                lista.Add(new TimeSheetDiaVO
                {
                    //Data do dia
                    Data = data,
                    //Nome do dia da semana (ex: Monday)
                    DiaSemana = data.DayOfWeek.ToString(),

                    //Código do tipo do dia (DO = folga / WD = dia normal)
                    Codigo = isDayOff ? WorkType.DO.ToString() : WorkType.WD.ToString(),

                    //Horários padrão vindos da escala
                    HoraEntrada = escala.hora_entrada,
                    HoraAlmocoInicio = escala.hora_almoco_inicio,
                    HoraAlmocoFim = escala.hora_almoco_fim,
                    HoraSaida = escala.hora_saida,

                    //Inicializa valores zerados
                    HorasTrabalhadas = 0,
                    BancoHoras = 0,
                    ValorDia = 0
                });
            }
            //Retorna o mês completo
            return lista;
        }


        #endregion

        #region EXCEÇÕES
        //Aplica exceções de horários/códigos salvos no banco
        public void AplicarExcecoes(int idColaborador, List<TimeSheetDiaVO> dias)
        {
            //Lista apenas as datas dos dias
            var datas = dias.Select(d => d.Data).ToList();

            //Busca no banco todas as exceções do colaborador para essas datas
            var excecoes = objBanco.tb_timesheet.Where(t => t.id_colaborador == idColaborador
                                                          && datas.Contains(t.data)).ToList();
            //Percorre cada dia do mês
            foreach (var dia in dias)
            {
                //Verifica se existe uma exceção para o dia atual
                var ts = excecoes.FirstOrDefault(e => e.data == dia.Data);
                //Se não existir, pula para o próximo
                if (ts == null) continue;

                //Sobrescreve os horários padrão
                dia.HoraEntrada = ts.hora_entrada;
                dia.HoraSaida = ts.hora_saida;
                dia.HoraAlmocoInicio = ts.hora_almoco_inicio;
                dia.HoraAlmocoFim = ts.hora_almoco_fim;
                //Atualiza o tipo de apontamento
                dia.Codigo = ts.tipo_apontamento;
                //Marca que o dia possui exceção
                dia.PossuiExcecao = true;
            }
        }

        #endregion

        #region CÁLCULO DE HORAS
        //Calcula horas trabalhadas considerando almoço
        public decimal CalcularHoras(TimeSpan? entrada, TimeSpan? saida, TimeSpan? almocoIni, TimeSpan? almocoFim)
        {
            //Se não tiver entrada ou saída, não há horas
            if (!entrada.HasValue || !saida.HasValue)
                return 0;

            //Total bruto (saída - entrada)
            var total = saida.Value - entrada.Value;

            //Desconta o intervalo de almoço, se existir
            if (almocoIni.HasValue && almocoFim.HasValue)
                total -= (almocoFim.Value - almocoIni.Value);

            //Retorna em horas decimais
            return (decimal)total.TotalHours;
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

            //Valor da hora vem do GCM
            decimal valorHora = Convert.ToDecimal(gcm.valor_hora);

            //Jornada calculada pela escala
            decimal jornadaDia = CalcularHoras(escala.hora_entrada, escala.hora_saida, escala.hora_almoco_inicio, escala.hora_almoco_fim);

            //Zera todos os valores antes do cálculo
            dia.HorasTrabalhadas = 0;
            dia.BancoHoras = 0;
            dia.ValorDia = 0;
            dia.HorasAtestado = 0;
            dia.AdicionalFeriado = 0;

            //Verifica se o dia é folga
            bool isDayOff = (int)dia.Data.DayOfWeek == escala.dayoff_1 || (int)dia.Data.DayOfWeek == escala.dayoff_2;

            //Ajusta código caso seja folga
            if (isDayOff && tipo == WorkType.WD)
                tipo = WorkType.DO;

            //SL → desligamento (não calcula nada)
            if (tipo == WorkType.SL)
                return;

            //AL → férias (recebe + 1/3)
            if (tipo == WorkType.AL)
            {
                decimal valorDia = jornadaDia * valorHora;
                dia.ValorDia = valorDia + (valorDia / 3);
                return;
            }

            //BT → banco de horas
            if (tipo == WorkType.BT)
            {
                decimal horasDecimal = CalcularHoras(dia.HoraEntrada, dia.HoraSaida, dia.HoraAlmocoInicio, dia.HoraAlmocoFim);

                 dia.BancoHoras = ConverterDecimalParaHoraUsuario(horasDecimal);
                //TESTE
                dia.ValorDia = horasDecimal * valorHora;

                return;
            }

            //UA → falta injustificada parcial
            if (tipo == WorkType.UA)
            {
                decimal ausenciaDecimal = ConverterHoraUsuarioParaDecimal(horasAusenciaUA);
                decimal jornadaDecimal = ConverterHoraUsuarioParaDecimal(jornadaDia);

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
                decimal atestadoDecimal = ConverterHoraUsuarioParaDecimal(horasAusenciaUA);
                decimal jornadaDecimal = ConverterHoraUsuarioParaDecimal(jornadaDia);

                decimal horasTrabalhadasDecimal = jornadaDecimal - atestadoDecimal;

                if (horasTrabalhadasDecimal < 0)
                    horasTrabalhadasDecimal = 0;

                dia.HorasAtestado = ConverterDecimalParaHoraUsuario(atestadoDecimal);

                dia.HorasTrabalhadas = ConverterDecimalParaHoraUsuario(horasTrabalhadasDecimal);

                dia.ValorDia = jornadaDecimal * valorHora;

                return;
            }

            //Códigos remunerados normais
            if (tipo == WorkType.WD || tipo == WorkType.DO || tipo == WorkType.TR || tipo == WorkType.SC || tipo == WorkType.DN)
            {
                decimal horasDecimal = CalcularHoras(dia.HoraEntrada, dia.HoraSaida, dia.HoraAlmocoInicio, dia.HoraAlmocoFim);

                decimal jornadaDecimal = ConverterHoraUsuarioParaDecimal(jornadaDia);

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
        public void SalvarDia(int idColaborador, TimeSheetDiaVO dia)
        {
            //Busca registro existente
            var reg = objBanco.tb_timesheet.FirstOrDefault(t => t.id_colaborador == idColaborador && t.data == dia.Data);

            //Se não existir, cria um novo
            if (reg == null)
            {
                reg = new tb_timesheet
                {
                    id_colaborador = idColaborador,
                    data = dia.Data
                };
                objBanco.tb_timesheet.Add(reg);
            }

            //Atualiza os campos
            reg.hora_entrada = dia.HoraEntrada;
            reg.hora_saida = dia.HoraSaida;
            reg.hora_almoco_inicio = dia.HoraAlmocoInicio;
            reg.hora_almoco_fim = dia.HoraAlmocoFim;
            reg.tipo_apontamento = dia.Codigo;

            objBanco.SaveChanges();
        }

        #endregion

        //Calculos horas
        private decimal ConverterHoraUsuarioParaDecimal(decimal horas)
        {
            int h = (int)horas;
            int m = (int)Math.Round((horas - h) * 100);
            return h + (m / 60m);
        }

        //1.5 → 1.3 (para exibição)
        private decimal ConverterDecimalParaHoraUsuario(decimal valor)
        {
            int h = (int)valor;
            int m = (int)Math.Round((valor - h) * 60);
            return h + (m / 100m);
        }

    }
}
