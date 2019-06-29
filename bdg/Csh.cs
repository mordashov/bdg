using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace bdg
{
    class Csh
    {
        private string _cshSum;

        public string CshId { get; set; }
        public Stt SttIdFrom { get; set; }
        public Stt SttIdTo { get; set; }
        public string CshSum
        {
            get => _cshSum; set
            {
                value = value.Replace(",", ".");
                value = value.Replace(" ", "");
                try
                {
                    //Для парсера меняю точку на запятую(в системе разделитель запятая)
                    double d = double.Parse(value.Replace(".", ","));
                    value = d.ToString();
                }
                catch (Exception)
                {
                    value = null;
                    //MessageBox.Show("Неверная сумма!");
                }

                _cshSum = value;
            }
        }
        public DateTime CshDate { get; set; }
        public string CshPln { get; set; }
        public string CshNote { get; set; }

        public Dictionary<string, string> GetValues(string csh_id)
        {
            string sql = $"SELECT csh_id, csh_dt, stt_id_from, stt_id_to, csh_sum, sch_pln, csh_note FROM csh WHERE csh_id = {csh_id}";
            Dictionary<string, string> dic = new db3work(sql).ValuesRow();
            return dic;
        }

        public string GetTotalSum(Stt stt)
        {
            // Получаю остаток по Дт и Кт

            //List<string> nameStt = new List<string>();
            //if (SttIdFrom != null) nameStt.Add("stt_id_from");
            //if (SttIdTo != null) nameStt.Add("stt_id_to");

            string[] nameStt = new string[2];
            nameStt[0] = "stt_id_to";
            nameStt[1] = "stt_id_from";

            double[] sum = new double[2];

            for (int i = 0; i < nameStt.Length; i++)
            {
                string sql = $@"
                            SELECT SUM([csh_sum])
                            FROM [csh]
                            WHERE {nameStt[i]} = '{stt.SttId}'
                            GROUP BY {nameStt[i]}";
                string resSql = new db3work(sql).ScalarSql();
                resSql = resSql ?? "0";
                double res = double.Parse(resSql);
                sum[i] = res;
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

        public void Add()
        {
            if (SttIdFrom.SttId == null) GetStt(SttIdFrom);
            if (SttIdTo.SttId == null) GetStt(SttIdTo);

            string sql;
            sql = $@"
                    INSERT INTO csh
                    (csh_dt, stt_id_from, stt_id_to, csh_sum, csh_pln, csh_note)
                    VALUES(
                        '{CshDate:yyyy-MM-dd}'
                        ,{SttIdFrom.SttId}
                        ,{SttIdTo.SttId}
                        ,{CshSum}
                        ,IFNULL({CshPln},0)
                        ,'{CshNote}'
                    );";
            new db3work(sql).RunSql();
        }

        public void Edit()
        {
            string sql = $@"
                            UPDATE csh SET 
                            [csh_dt] = '{CshDate:yyyy-MM-dd}'
                            ,[stt_id_from] = {SttIdFrom.SttId} 
                            ,[stt_id_to] = {SttIdTo.SttId}  
                            ,[csh_sum] = {CshSum}
                            ,[csh_pln] = {CshPln}
                            ,[csh_note] = '{CshNote}'
                            WHERE csh_id = {CshId}";
            new db3work(sql).RunSql();
        }

        public void GetRowValues()
        {
            string sql = $@"SELECT 
                                csh_id
                                ,csh_dt
                                ,stt_id_from
                                ,stt_id_to
                                ,csh_sum
                                ,csh_pln
                                ,csh_note 
                            FROM csh 
                            WHERE csh_id = {CshId}";
            Dictionary<string, string> rowValues = new db3work(sql).ValuesRow();
            CshDate = DateTime.Parse(rowValues["csh_dt"]);
            Stt stt;
            stt = new Stt(rowValues["stt_id_from"], "stt_id_from");
            SttIdFrom = stt;
            stt = new Stt(rowValues["stt_id_to"], "stt_id_to");
            SttIdTo = stt;
            CshSum = rowValues["csh_sum"];
            CshPln = rowValues["csh_pln"];
            CshNote = rowValues["csh_note"];
        }

        private Stt GetStt(Stt stt)
        {
            string sql = $"INSERT INTO stt (ctg_id, prj_id) VALUES ({stt.CtgId},{stt.PrjId})";
            new db3work(sql).RunSql();
            sql = $"SELECT stt_id FROM stt WHERE ctg_id = {stt.CtgId} AND prj_id = {stt.PrjId}";
            stt.SttId = new db3work(sql).ScalarSql();
            return stt;
        }
    }
}
