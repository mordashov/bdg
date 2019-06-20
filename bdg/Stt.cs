using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace bdg
{
    class Stt
    {
        private string _sql;
        public string CtgId { get; set; } // Критерий
        public string CtgName { get; set; } //Критерий наименование
        public string PrjId { get; set; } // Проект
        public string PrjName { get; set; } //Проект наименование
        public string Result { get; set; }
        public string NameField { get; set; }

        public Stt (Ctg ctg, string prj_id = null)
        {
            CtgId = ctg.CtgId;
            CtgName = ctg.CtgName;
            NameField = "stt_id_from";
            if (ctg.CtgField == "ctg_id_to") NameField = "stt_id_to";

            if (prj_id != null)
            {
                _sql = "SELECT prj_nm FROM prj WHERE prj_id = " + prj_id;
                PrjId = prj_id;
                PrjName = new db3work(_sql).ScalarSql();
            }

            if (prj_id != null && ctg !=null)
            {
                _sql = $"SELECT stt_id FROM stt WHERE ctg_id = {ctg} AND prj_id = {prj_id}";
                Result = new db3work(_sql).ScalarSql();
            }


        }

    }
}
