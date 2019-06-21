using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace bdg
{
    class Ctg
    {
        //private DataGrid _dataGrid;
        public string CtgId { get; set; }
        public string CtgName { get; set; }
        public string CtgField { get; set; }

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
        public Ctg(DataGrid dataGrid)
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

    }

}
