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
        public string NameField { get; set; }

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
            NameField = "stt_id_from";
            if (ctg.CtgField == "ctg_id_to") NameField = "stt_id_to";
        }
    }
}
