using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace bdg
{
    class Prj
    {
        private string _prjId;
        public string PrjId
        {
            get => _prjId; set
            {
                _prjId = value;
                string sql = "SELECT prj_nm FROM prj WHERE prj_id = " + _prjId;
                PrjName = new db3work(sql).ScalarSql();
            }
        }
        public string PrjName { get; set; }
        public string PrjField { get; set; }
        private Ctg _ctg { get; set; }

        public Prj(Ctg ctg)
        {
            PrjField = "prj_id_from";
            if (ctg.CtgField == "ctg_id_to") PrjField = "prj_id_to";
            _ctg = ctg;
        }

        public void Fill(Stt stt, DataGrid dataGrid)
        {
            string subSql = $"SUM(CASE c.ctg_id WHEN {stt.CtgId} THEN 1 ELSE 0 END)";
            string sql = $@"
                            SELECT p.prj_id, prj_nm, 
                                {subSql} AS ctg_id,
                                CASE {subSql} WHEN 0 THEN 0 ELSE 1 END AS color 
                            FROM prj p
                            JOIN stt s ON s.prj_id = p.prj_id
                            JOIN ctg c ON c.ctg_id = s.ctg_id
                            JOIN csh h ON h.{stt.SttFromOrTo} = s.stt_id
                            GROUP BY p.prj_id, p.prj_nm
                            ORDER BY {subSql} DESC, prj_nm;
                            ";
            DataTable dt = new db3work(sql).SelectSql();
            dataGrid.DataContext = dt;
        }

        public void Add()
        {
            InputWindow inputWindow = new InputWindow();
            inputWindow.Left = Application.Current.MainWindow.Left;
            inputWindow.TextBoxInput.Text = "";
            inputWindow.Title = "Ввод имени нового проекта";
            inputWindow.ShowDialog();
            string tbValue = inputWindow.TextBoxInput.Text;
            if (inputWindow.IsОК == false) return;

            MessageBoxResult msg;
            msg = MessageBox.Show(
                $"Введенный проект будет связан с категорией {_ctg.CtgName}\nПродолжить?"
                , "Внимание"
                , MessageBoxButton.YesNo);
            if (msg != MessageBoxResult.Yes) return;
            if (tbValue == "") return;

            string sql = $"INSERT INTO prj (prj_nm) VALUES ('{tbValue}')";
            new db3work(sql).RunSql();
            PrjName = tbValue;
            sql = $"SELECT prj_id FROM prj WHERE prj_nm = '{PrjName}'";
            PrjId = new db3work(sql).ScalarSql();
            sql = $"INSERT INTO stt (ctg_id, prj_id) VALUES ({_ctg.CtgId}, {PrjId})";
            new db3work(sql).RunSql();
            inputWindow.Close();
        }
    }
}
