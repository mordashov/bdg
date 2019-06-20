using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace bdg
{
    class Csh
    {
        public Stt _ctgPrj;
        public string CshId { get; set; }
        public string SttIdFrom { get; set; }
        public string SttIdTo { get; set; }
        public string CshSum { get; set; }
        public string CshPln { get; set; }
        public string CshNote { get; set; }

        public Dictionary<int, string> GetValues(string csh_id)
        {
            string sql = $"SELECT csh_id, csh_dt, stt_id_from, stt_id_to, csh_sum, sch_pln, csh_note FROM csh WHERE csh_id = {csh_id}";
            Dictionary<int, string> db = new db3work(sql).ValuesRow();
            return db;
        }

        private string GetTotalSum(string sttId)
        {
            // Получаю остаток по Дт и Кт
            string[] nameStt = new string[2];
            nameStt[0] = "stt_id_to";
            nameStt[1] = "stt_id_from";

            double[] sum = new double[2];

            for (int i = 0; i < nameStt.Length; i++)
            {
                string sql = $@"
                            SELECT 
                                  SUM([csh_sum])
                              FROM [csh]
                              INNER JOIN stt ON stt.stt_id = csh.{nameStt[i]}
                              INNER JOIN ctg ON ctg.ctg_id = stt.ctg_id
                              INNER JOIN prj ON prj.prj_id = stt.prj_id
                              WHERE stt.stt_id = '{sttId}'
                              GROUP BY ctg.ctg_id";
                sum[i] = double.Parse(new db3work(sql).ScalarSql());
            }

            return $"{sum[0] - sum[1]:C}";
        }

        public void Fill(DataGrid dataGrid) //Заполняю основную DataGrid с перечнем движения денежных средств
        {
            string sql = @"
            SELECT [csh_id]
                  ,strftime('%d.%m.%Y', csh_dt) AS 'date'
                  ,[stt_id_from]
                  ,[ctg_from].[ctg_nm] || ' / ' || [prj_from].[prj_nm] AS 'from'
                  ,[stt_id_to]
                  ,[ctg_to].[ctg_nm] || ' / ' || [prj_to].[prj_nm] AS 'to'
                  ,[csh_sum]
                  ,[csh_pln]
                  ,[csh_note]
              FROM [csh]
              INNER JOIN stt AS stt_from ON stt_from.stt_id=csh.stt_id_from
              INNER JOIN ctg AS ctg_from ON ctg_from.ctg_id = stt_from.ctg_id
              INNER JOIN prj AS prj_from ON prj_from.prj_id = stt_from.prj_id
              INNER JOIN stt AS stt_to ON stt_to.stt_id=csh.stt_id_to
              INNER JOIN ctg AS ctg_to ON ctg_to.ctg_id = stt_to.ctg_id
              INNER JOIN prj AS prj_to ON prj_to.prj_id = stt_to.prj_id
              ORDER BY csh_dt DESC, csh_id DESC
              ;";
            System.Data.DataTable table = new db3work(sql).SelectSql();
            dataGrid.DataContext = table.DefaultView;
        }


    }
}
