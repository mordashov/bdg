using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace bdg
{
    class Stt
    {
        public string CtgId { get; set; } // Критерий
        public string CtgName { get; set; } //Критерий наименование
        public string PrjId { get; set; } // Проект
        public string PrjName { get; set; } //Проект наименование
        public string SttId { get; set; }
        public string SttName { get; set; }
        public string SttFromOrTo { get; set; }

        public Stt(string sttId, string sttFromOrTo)
        {
            SttId = sttId;

            Dictionary<string, string> dic;
            string sql;
            sql = $@"SELECT 
                        prj.prj_id
                        ,prj_nm
                    FROM prj 
                    JOIN stt ON stt.prj_id = prj.prj_id
                    WHERE stt_id = {sttId}";
            dic = new db3work(sql).ValuesRow();
            PrjId = dic["prj_id"];
            PrjName = dic["prj_nm"];
            dic.Clear();

            sql = $@"SELECT 
                        ctg.ctg_id
                        ,ctg_nm
                    FROM ctg 
                    JOIN stt ON stt.ctg_id = ctg.ctg_id
                    WHERE stt_id = {sttId}";
            dic = new db3work(sql).ValuesRow();
            CtgId = dic["ctg_id"];
            CtgName = dic["ctg_nm"];

            SttName = CtgName + " / " + PrjName;
            SttFromOrTo = sttFromOrTo;
            dic.Clear();
        }

        public Stt(Ctg ctg)
        {
            InitCtg(ctg);
        }

        public Stt(Ctg ctg, Prj prj)
        {
            InitCtg(ctg);
            PrjId = prj.PrjId;
            PrjName = prj.PrjName;
            string sql = $"SELECT stt_id FROM stt WHERE ctg_id = {CtgId} AND prj_id = {PrjId}";
            SttId = new db3work(sql).ScalarSql();
            SttName = CtgName + " / " + PrjName;
        }

        public void InitCtg(Ctg ctg)
        {
            CtgId = ctg.CtgId;
            CtgName = ctg.CtgName;
            SttFromOrTo = "stt_id_from";
            if (ctg.CtgField == "ctg_id_to") SttFromOrTo = "stt_id_to";
        }
    }
}
