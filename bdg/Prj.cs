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
        private Stt _stt { get; set; }

        public Prj(Ctg ctg)
        {
            PrjField = "prj_id_from";
            if (ctg.CtgField == "ctg_id_to") PrjField = "prj_id_to";
            _ctg = ctg;
        }

        public Prj(Stt stt)
        {
            _stt = stt;
        }

        public void Fill(DataGrid dataGrid)
        {
            string subSql = $"SUM(CASE c.ctg_id WHEN {_stt.CtgId} THEN 1 ELSE 0 END)";
            string sql = $@"
                            SELECT p.prj_id, prj_nm, 
                                {subSql} AS ctg_id,
                                CASE {subSql} WHEN 0 THEN 0 ELSE 1 END AS color 
                            FROM prj p
                            LEFT JOIN stt s ON s.prj_id = p.prj_id
                            LEFT JOIN ctg c ON c.ctg_id = s.ctg_id
                            LEFT JOIN csh h ON h.{_stt.SttFromOrTo} = s.stt_id
                            GROUP BY p.prj_id, p.prj_nm
                            ORDER BY {subSql} DESC, prj_nm;
                            ";
            DataTable dt = new db3work(sql).SelectSql();
            dataGrid.DataContext = dt;
        }

        public void Add(DataGrid dataGrid)
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
                $"Введенный проект будет связан с категорией {_stt.CtgName}\nПродолжить?"
                , "Внимание"
                , MessageBoxButton.YesNo);
            if (msg != MessageBoxResult.Yes) return;

            string sql = $"INSERT INTO prj (prj_nm) VALUES ('{tbValue}')";
            new db3work(sql).RunSql();
            PrjName = tbValue;
            sql = $"SELECT prj_id FROM prj WHERE prj_nm = '{PrjName}'";
            PrjId = new db3work(sql).ScalarSql();
            sql = $"INSERT INTO stt (ctg_id, prj_id) VALUES ({_stt.CtgId}, {PrjId})";
            new db3work(sql).RunSql();
            inputWindow.Close();
            Fill(dataGrid);
        }

        public void Edit(DataGrid dataGrid)
        {
            DataRowView drv = (DataRowView)dataGrid.SelectedItem;
            if (drv == null) return;
            PrjId = drv.Row.ItemArray[0].ToString();

            InputWindow inputWindow = new InputWindow();
            inputWindow.Left = Application.Current.MainWindow.Left;
            inputWindow.Title = "Изменение имени проекта";

            string sql = $"SELECT prj_nm FROM prj WHERE prj_id  = '{PrjId}'";
            PrjName = new db3work(sql).ScalarSql();
            inputWindow.TextBoxInput.Text = PrjName;

            inputWindow.ShowDialog();
            string tbValue = inputWindow.TextBoxInput.Text;
            if (inputWindow.IsОК == false) return;

            PrjName = tbValue;
            sql = $"UPDATE prj SET prj_nm = '{PrjName}' WHERE prj_id={PrjId}";
            new db3work(sql).RunSql();
            inputWindow.Close();
            Fill(dataGrid);
        }

        public void Del(DataGrid dataGrid)
        {
            DataRowView drv = (DataRowView)dataGrid.SelectedItem;
            if (drv == null) return;
            string prjId = drv.Row.ItemArray[0].ToString();

            //Проверка используется ли проект в основной таблице
            string sql = $@"
                SELECT COUNT(stt.stt_id)
                  FROM [prj]
                  INNER JOIN stt ON stt.prj_id = prj.prj_id
                  LEFT JOIN csh AS csh_from ON csh_from.stt_id_from = stt.stt_id
                  LEFT JOIN csh AS csh_to ON csh_to.stt_id_to = stt.stt_id
                  WHERE [prj].[prj_id] = {prjId}
                  GROUP BY csh_from.stt_id_from
                  ;";
            string rowsCount = new db3work(sql).ScalarSql();
            if (rowsCount == "0")
            {
                //Удаление связки в stt
                sql = $@"DELETE FROM stt WHERE prj_id = {prjId}";
                new db3work(sql).RunSql();

                //Удаление категории
                sql = $@"DELETE FROM prj WHERE prj_id = {prjId}";
                new db3work(sql).RunSql();
                
                //Заполнение DataGrid
                Fill(dataGrid);
            }
            else
            {
                MessageBox.Show("Внимание, проект используется в основной таблие!\nЕго удалить нельзя!");
            }
        }
    }
}
