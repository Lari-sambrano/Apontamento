using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core
{
    public class UtilCORE
    {
        //Fixo por enquanto para simular a garagem cod 1 logada.
        public static int CodigoLogado = 1;

        public static class FeriadosBrasil
        {
            public static HashSet<DateTime> ObterFeriados(int ano)
            {
                var feriados = new HashSet<DateTime>
                {
                    new DateTime(ano, 1, 1),   // Confraternização Universal
                    new DateTime(ano, 4, 21),  // Tiradentes
                    new DateTime(ano, 5, 1),   // Dia do Trabalho
                    new DateTime(ano, 9, 7),   // Independência
                    new DateTime(ano, 10, 12), // N. Sra Aparecida
                    new DateTime(ano, 11, 2),  // Finados
                    new DateTime(ano, 11, 15), // Proclamação da República
                    new DateTime(ano, 12, 25)  // Natal
                };

                DateTime pascoa = CalcularPascoa(ano);

                feriados.Add(pascoa.AddDays(-47)); // Carnaval
                feriados.Add(pascoa.AddDays(-2));  // Sexta-feira Santa
                feriados.Add(pascoa.AddDays(60));  // Corpus Christi

                return feriados;
            }

            private static DateTime CalcularPascoa(int ano)
            {
                int a = ano % 19;
                int b = ano / 100;
                int c = ano % 100;
                int d = b / 4;
                int e = b % 4;
                int f = (b + 8) / 25;
                int g = (b - f + 1) / 3;
                int h = (19 * a + b - d - g + 15) % 30;
                int i = c / 4;
                int k = c % 4;
                int l = (32 + 2 * e + 2 * i - h - k) % 7;
                int m = (a + 11 * h + 22 * l) / 451;
                int mes = (h + l - 7 * m + 114) / 31;
                int dia = ((h + l - 7 * m + 114) % 31) + 1;

                return new DateTime(ano, mes, dia);
            }
        }
    }
}
