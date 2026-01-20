using DAO.VO;
using System.Collections.Generic;
using System.Drawing;
using ClosedXML.Excel;
using Core.Enums;


namespace DAO
{
    public class ExcelEscalaDAO
    {
        private Dictionary<XLColor, WorkType> MapaCores = new Dictionary<XLColor, WorkType>()
        {
                { XLColor.FromColor(Color.White), WorkType.WD },
                { XLColor.FromColor(Color.Gray), WorkType.DO },
                { XLColor.FromColor(Color.Blue), WorkType.AL },
                { XLColor.FromColor(Color.Green), WorkType.BT },
                { XLColor.FromColor(Color.Orange), WorkType.ML },
                { XLColor.FromColor(Color.Purple), WorkType.SC },
                { XLColor.FromColor(Color.Yellow), WorkType.TR },
                { XLColor.FromColor(Color.Red), WorkType.UA },
                { XLColor.FromColor(Color.Black), WorkType.SL },
                { XLColor.FromColor(Color.Magenta), WorkType.DN }
        };




        public List<AnalistaResumoVO> LerEscala(string caminhoExcel)
        {
            var resultado = new List<AnalistaResumoVO>();

            using (var wb = new XLWorkbook(caminhoExcel))
            {
                var ws = wb.Worksheet(1);
                int linha = 6;     // primeira linha dos analistas
                int colDia1 = 11;  // coluna do dia 1 (START)

                while (!ws.Cell(linha, 1).IsEmpty())
                {
                    var analista = new AnalistaResumoVO
                    {
                        Nome = ws.Cell(linha, 1).GetString()
                    };

                    int diasNoMes = 31;

                    for (int dia = 1; dia <= diasNoMes; dia++)
                    {
                        int col = colDia1 + ((dia - 1) * 2);

                        var cell = ws.Cell(linha, col);
                        if (cell.IsEmpty())
                            continue;

                        var cor = cell.Style.Fill.BackgroundColor;

                        if (MapaCores.TryGetValue(cor, out WorkType tipo))
                        {
                            analista.Dias[dia] = tipo;
                            analista.Totais[tipo]++;
                        }
                    }

                    resultado.Add(analista);
                    linha++;
                }
                



            return resultado;
        }
    }
}

    }

