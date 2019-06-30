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
    class Ctg
    {
        public string CtgId { get; set; }
        public string CtgName { get; set; }
        public string CtgField { get; set; }

        public Ctg()
        {
        }

        public Ctg(DataGrid dataGrid, TextBox textBox)
        {
            DataRowView drv = (DataRowView)dataGrid.SelectedItem;
            if (drv == null)
            {
                this.Fill(dataGrid);
                return;
            }
            CtgId = drv.Row[0].ToString();
            if (textBox.Name == "TextBoxFrom")
            {
                CtgField = "ctg_id_from";
            }

            if (textBox.Name == "TextBoxTo")
            {
                CtgField = "ctg_id_to";
            }
            string sql = "SELECT ctg_nm FROM ctg WHERE ctg_id = " + CtgId;
            CtgName = new db3work(sql).ScalarSql();
            textBox.Text = CtgName;
        }

        public void Fill(DataGrid dataGrid)
        {
            string sql = @"SELECT [ctg_id] ,[ctg_nm] FROM [ctg] ORDER BY [ctg_nm]";
            DataTable table = new db3work(sql).SelectSql();
            dataGrid.DataContext = table.DefaultView;
            if (dataGrid.Items.Count != 0)
            {
                dataGrid.SelectedIndex = 0;
                dataGrid.ScrollIntoView(dataGrid.Items[0]);
            }
        }

        public void Add(DataGrid dataGrid)
        {
            InputWindow inputWindow = new InputWindow();
            inputWindow.Left = Application.Current.MainWindow.Left;
            inputWindow.TextBoxInput.Text = "";
            inputWindow.Title = "Ввод имени новой категории";
            inputWindow.ShowDialog();
            string tbValue = inputWindow.TextBoxInput.Text;
            if (inputWindow.IsОК == false) return;

            string sql = $"INSERT INTO ctg (ctg_nm) VALUES ('{tbValue}')";
            new db3work(sql).RunSql();
            CtgName = tbValue;
            sql = $"SELECT ctg_id FROM ctg WHERE ctg_nm = '{CtgName}'";
            CtgId = new db3work(sql).ScalarSql();
            inputWindow.Close();
            Fill(dataGrid);
        }

        public void Edit(DataGrid dataGrid)
        {
            DataRowView drv = (DataRowView)dataGrid.SelectedItem;
            if (drv == null) return;
            CtgId = drv.Row.ItemArray[0].ToString();

            InputWindow inputWindow = new InputWindow();
            inputWindow.Left = Application.Current.MainWindow.Left;
            inputWindow.Title = "Изменение имени категории";

            string sql = $"SELECT ctg_nm FROM ctg WHERE ctg_id  = '{CtgId}'";
            CtgName = new db3work(sql).ScalarSql();
            inputWindow.TextBoxInput.Text = CtgName;

            inputWindow.ShowDialog();
            string tbValue = inputWindow.TextBoxInput.Text;
            if (inputWindow.IsОК == false) return;

            CtgName = tbValue;
            sql = $"UPDATE ctg SET ctg_nm = '{CtgName}' WHERE ctg_id={CtgId}";
            new db3work(sql).RunSql();
            inputWindow.Close();
            Fill(dataGrid);
        }

        public void Del(DataGrid dataGrid)
        {
            DataRowView drv = (DataRowView)dataGrid.SelectedItem;
            if (drv == null) return;
            string ctgId = drv.Row.ItemArray[0].ToString();

            //Проверка используется ли проект в основной таблице
            string sql = $@"
                SELECT COUNT(stt.stt_id)
                  FROM [ctg]
                  LEFT JOIN stt ON stt.ctg_id = ctg.ctg_id
                  LEFT JOIN csh AS csh_from ON csh_from.stt_id_from = stt.stt_id
                  LEFT JOIN csh AS csh_to ON csh_to.stt_id_to = stt.stt_id
                  WHERE [ctg].[ctg_id] = {ctgId}
                  GROUP BY csh_from.stt_id_from
                  ;";
            string rowsCount = new db3work(sql).ScalarSql();
            if (rowsCount == "0")
            {
                //Удаление связки в stt
                sql = $@"DELETE FROM stt WHERE ctg_id = {ctgId}";
                new db3work(sql).RunSql();

                //Удаление категории
                sql = $@"DELETE FROM ctg WHERE ctg_id = {ctgId}";
                new db3work(sql).RunSql();

                //Заполнение DataGrid
                Fill(dataGrid);
            }
            else
            {
                MessageBox.Show("Внимание, категория используется в основной таблие!\nЕё удалить нельзя!");
            }
        }

    }

}
