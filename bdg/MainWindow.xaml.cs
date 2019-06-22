using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;
using Remotion.Linq.Clauses;

namespace bdg
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>

    public partial class MainWindow
    {

        private Stt _stt;
        private Csh _csh = new Csh();
        private Ctg _ctg;
        private Prj _prj;
        private bool _textBoxFromIsFocused = false;
        private bool _textBoxToIsFocused = false;

        private void CtgSelect(DataGrid dataGrid)
        {
            if (_textBoxToIsFocused)
            {
                _ctg = new Ctg(dataGrid, TextBoxTo);
                _stt = new Stt(_ctg);
            }
            if (_textBoxFromIsFocused) 
            {
                _ctg = new Ctg(dataGrid, TextBoxFrom);
                _stt = new Stt(_ctg);
            }
            ButtonAdd.IsEnabled = false;


        }

        private void DataGridCtg_MouseUp(object sender, MouseButtonEventArgs e)
        {
            CtgSelect((DataGrid)sender);
            _prj.Fill(_stt, DataGridPrj);
        }

        private void DataGridCtg_KeyUp(object sender, KeyEventArgs e)
        {
            CtgSelect((DataGrid)sender);
            _prj.Fill(_stt, DataGridPrj);
        }

        private void PrjSelect() //Изменение выбора в проектах
        {
            DataRowView drv = (DataRowView)DataGridPrj.SelectedItem;
            _prj.PrjId = drv.Row[0].ToString();
            _stt = new Stt(_ctg, _prj);

            string toFullname = "";
            string fromFullName = "";

            switch (_stt.NameField)
            {
                case "stt_id_from":
                    _csh.SttIdFrom = _stt;
                    TextBoxSumFrom.Text = _csh.GetTotalSum(_csh.SttIdFrom);
                    TextBoxFrom.Text = _csh.SttIdFrom.SttName;
                    TextBoxTo.Focus();
                    break;
                case "stt_id_to":
                    _csh.SttIdTo = _stt;
                    TextBoxSumTo.Text = _csh.GetTotalSum(_csh.SttIdTo);
                    TextBoxTo.Text = _csh.SttIdTo.SttName;
                    TextBoxSum.Focus();
                    break;
            }
            try
            {
                if (_csh.SttIdFrom.SttName != "" && _csh.SttIdTo.SttName != "")
                {
                    ButtonAdd.IsEnabled = true;
                }

            }
            catch
            {
                ButtonAdd.IsEnabled = false;
            }
        }

        private void DataGridPrj_MouseUp(object sender, MouseButtonEventArgs e)
        {
            PrjSelect();
        }

        private void DataGridPrj_KeyUp(object sender, KeyEventArgs e)
        {
            PrjSelect();
        }

        private void textBoxTo_GotFocus(object sender, RoutedEventArgs e)
        {
            _textBoxToIsFocused = true;
            _textBoxFromIsFocused = false;
        }

        private void textBoxFrom_GotFocus(object sender, RoutedEventArgs e)
        {
            _textBoxToIsFocused = false;
            _textBoxFromIsFocused = true;
        }

        private void buttonAdd_Click(object sender, RoutedEventArgs e) //Добавление/изменение данных в csh
        {

            //string sttIdFrom;
            //string sttIdTo;

            //DateTime dt;
            //dt = Convert.ToDateTime(DateCsh.Text);
            //string plan = CheckBoxPln.IsChecked.ToString() == "0" ? "1" : "0";
            //string sql;

            //switch (ButtonAdd.Content)
            //{
            //    //Добавление новой строки
            //    case "Добавить":
            //        sql = $@"
            //                    INSERT INTO csh
            //                    (csh_dt, stt_id_from, stt_id_to, csh_sum, csh_pln, csh_note)
            //                    VALUES('{dt:yyyy-MM-dd}', {SttId[0]}, {SttId[1]}, {TextBoxSum.Text}, {plan}, '{TextBoxComment.Text}');";
            //        db3.RunSql(sql);
            //        break;
            //    //Добавление новой строки
            //    case "Изменть":
            //        sql = $@"
            //                    UPDATE csh SET 
            //                    [csh_dt] = '{dt:yyyy-MM-dd}'
            //                    ,[stt_id_from] = {SttId[0]} 
            //                    ,[stt_id_to] = {SttId[1]}  
            //                    ,[csh_sum] = {TextBoxSum.Text}
            //                    ,[csh_pln] = {plan}
            //                    ,[csh_note] = '{TextBoxComment.Text}'
            //                    WHERE csh_id = {_sttId}";
            //        db3.RunSql(sql);
            //        break;
            //}
            ////Обновляем гриды, переменные и текстбоксы
            //Refresh();

            ////}

        }

        //        private string  SttInsert(string crtId, string prjId) //Добавление новой статьи
        //        {
        //            // Проверка наличия связки категори и проекта в stt
        //            string sqlSttId = $@"
        //                SELECT stt_id
        //                FROM stt
        //                WHERE ctg_id = {crtId} AND prj_id = {prjId};
        //            ";
        //            string sttId = db3.ScalarSql(sqlSttId);
        //            MessageBoxResult msg = MessageBoxResult.No;
        //            if (sttId == "0")
        //                msg = MessageBox.Show("Добавить новую связку Категрия-проект?", "", MessageBoxButton.YesNo);

        //            if (msg == MessageBoxResult.Yes)
        //            {
        //                //Если связки нет - добавляем
        //                if (sttId == "0")
        //                {
        //                    string sql = $@"
        //                INSERT INTO stt
        //                (ctg_id, prj_id)
        //                VALUES({crtId}, {prjId});
        //                ";
        //                    db3.RunSql(sql);
        //                }
        //            }

        //            return db3.ScalarSql(sqlSttId);

        //        }

        //        private void CshSelect()
        //        {
        //            DataRowView dataRow = (DataRowView)DataGridCsh.SelectedItem ?? (DataRowView)DataGridCsh.Items[0];
        //            int countColumn = dataRow.Row.ItemArray.Length; 
        //            for (int i = 0; i < countColumn; i++)
        //            {
        //                string columnName = DataGridCsh.Columns[i].Header.ToString();
        //                string cellValue = dataRow.Row.ItemArray[i].ToString();

        //                switch (columnName)
        //                {
        //                    case "stt_id_from":
        //                        SttId[0] = cellValue;
        //                        break;
        //                    case "stt_id_to":
        //                        SttId[1] = cellValue;
        //                        break;
        //                    case "От куда":
        //                        TextBoxFrom.Text = cellValue;
        //                        break;
        //                    case "Куда":
        //                        TextBoxTo.Text = cellValue;
        //                        break;
        //                    case "Сумма":
        //                        TextBoxSum.Text = cellValue;
        //                        break;
        //                    case "Примечение":
        //                        TextBoxComment.Text = cellValue;
        //                        break;
        //                    case "ID":
        //                        _cshId = cellValue;
        //                        break;
        //                }
        //            }
        //        }

        private void textBoxSum_LostFocus(object sender, RoutedEventArgs e)
        {
            //Для sqlite меняю запятую на точку
            //TextBoxSum.Text = TextBoxSum.Text.Replace(",", ".");
            //TextBoxSum.Text = TextBoxSum.Text.Replace(" ", "");
            //try
            //{
            //    Для парсера меняю точку на запятую (в системе разделитель запятая)
            //    double d = double.Parse(TextBoxSum.Text.Replace(".", ","));
            //}
            //catch (Exception)
            //{
            //    MessageBox.Show("Неверная сумма!");
            //}

        }
        private void TextBoxSum_GotFocus(object sender, RoutedEventArgs e)
        {
            if (TextBoxSum.Text == "0.00")
                TextBoxSum.Text = null;
        }
        private void DataGridCtg_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            //TextBox newValue = e.EditingElement as TextBox;
            //string oldValue = db3.CtgText;
            //if (newValue != null && newValue.Text != oldValue)
            //{
            //    string id = ((DataRowView)DataGridCtg.SelectedItem).Row["ctg_id"].ToString();
            //    string nm = ((DataRowView)DataGridCtg.SelectedItem).Row["ctg_nm"].ToString();
            //    if (nm == oldValue) nm = newValue.Text;
            //    //Обновляю в базе
            //    string sql = $@"
            //                UPDATE ctg SET ctg_nm = '{nm}'
            //                WHERE ctg_id = " + id;
            //    db3.RunSql(sql);
            //    CshFill();
            //}
        }

        private void CtgDel_Click(object sender, RoutedEventArgs e)
        {
            //            DataRowView drv = (DataRowView)DataGridCtg.SelectedCells[0].Item;
            //            string ctdDelId = drv.Row[1].ToString();
            //            //TODO: Получил id теперь надо проверить используется в csh или нет

            //            //string dgr = dg.SelectedCells.ToString();
            //            //Проверка используется ли категория в основной таблице
            //            string sql = $@"
            //                                SELECT COUNT(ctg.ctg_id)
            //                                FROM ctg
            //                                INNER JOIN stt ON stt.ctg_id = ctg.ctg_id
            //                                LEFT JOIN csh AS f ON f.stt_id_from = stt.stt_id
            //                                LEFT JOIN csh AS t ON t.stt_id_to = stt.stt_id
            //                                WHERE stt.stt_id = {_sttId[0]} or stt.stt_id = {_sttId[1]}
            //                                GROUP BY ctg.ctg_id
            //                                ;";
            //            /*
            //SELECT t.stt_id_to, COUNT(ctg.ctg_id)
            //FROM ctg
            //JOIN stt ON stt.ctg_id = ctg.ctg_id
            //LEFT JOIN csh AS f ON f.stt_id_from = stt.stt_id
            //LEFT JOIN csh AS t ON t.stt_id_to = stt.stt_id
            //WHERE stt.ctg_id = 28
            //GROUP BY ctg.ctg_id
            //             */
            //            string rowsCount = db3.ScalarSql(sql);
            //            if (rowsCount == "0")
            //            {
            //                //Удаление связки в stt
            //                sql = $@"DELETE FROM stt WHERE stt.stt_id = {_sttId[0]} OR stt.stt_id = {_sttId[1]}";
            //                db3.RunSql(sql);

            //                //Удаление категории
            //                //sql = $@"DELETE FROM ctg WHERE ctg_id = {_ctgId}";
            //                //db3.RunSql(sql);
            //                //Заполнение DataGrid
            //                CrtFill();
            //            }
            //            db3.RunSql(sql);
        }

        private void CtgNew_Click(object sender, RoutedEventArgs e)
        {
            //string sql = "INSERT INTO ctg (ctg_nm) VALUES ('-= Новая строка =-')";
            //db3.RunSql(sql);
            //CrtFill();
        }

        //private void PrjNew_Click(object sender, RoutedEventArgs e)
        //{
        //    string sql = "INSERT INTO prj (prj_nm) VALUES ('-= Новая строка =-')";
        //    db3.RunSql(sql);
        //    PrjFill();
        //}

        //private void PrjDel_Click(object sender, RoutedEventArgs e)
        //{
        //    DataRowView dataRow = (DataRowView)DataGridPrj.SelectedItem;
        //    string prjId = dataRow.Row.ItemArray[0].ToString();

        //    //Проверка используется ли категория в основной таблице
        //    string sql = $@"
        //    SELECT COUNT(csh_from.stt_id_from)
        //      FROM [prj]
        //      INNER JOIN stt ON stt.prj_id = prj.prj_id
        //      LEFT JOIN csh AS csh_from ON csh_from.stt_id_from = stt.stt_id
        //      LEFT JOIN csh AS csh_to ON csh_to.stt_id_to = stt.stt_id
        //      WHERE [prj].[prj_id] = {prjId}
        //      GROUP BY csh_from.stt_id_from
        //      ;";
        //    string rowsCount = db3.ScalarSql(sql);
        //    if (rowsCount == "0")
        //    {
        //        //Удаление связки в stt
        //        sql = $@"DELETE FROM stt WHERE prj_id = {prjId}";
        //        db3.RunSql(sql);

        //        //Удаление категории
        //        sql = $@"DELETE FROM prj WHERE prj_id = {prjId}";
        //        db3.RunSql(sql);
        //        //Заполнение DataGrid
        //        PrjFill();
        //    }
        //    else
        //        MessageBox.Show("Внимание, проект используется в основной таблие!\nЕго удалить нельзя!");
        //}

        private void CshDel_Click(object sender, RoutedEventArgs e)
        {
            //MessageBoxResult msg = MessageBox.Show("Будет удалена выбранная строка!\nПродолжить?", "", MessageBoxButton.YesNo);
            //if (msg == MessageBoxResult.Yes)
            //{
            //    DataRowView dataRow = (DataRowView)DataGridCsh.SelectedItem;
            //    string cshId = dataRow.Row.ItemArray[0].ToString();
            //    string sql = $@"DELETE FROM csh WHERE csh_id = {cshId}";
            //    db3.RunSql(sql);
            //    CshFill();
            //}
        }

        private void DataGridPrj_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            //TextBox newValue = e.EditingElement as TextBox;
            //string oldValue = db3.PrjText;
            //if (string.IsNullOrEmpty(newValue.Text) && newValue.Text != oldValue)
            //{
            //    string id = ((DataRowView)DataGridPrj.SelectedItem).Row["prj_id"].ToString();
            //    string nm = ((DataRowView)DataGridPrj.SelectedItem).Row["prj_nm"].ToString();
            //    if (nm == oldValue) nm = newValue.Text;
            //    //Обновляю в базе
            //    string sql = $@"
            //                UPDATE prj SET prj_nm = '{nm}'
            //                WHERE prj_id = " + id;
            //    db3.RunSql(sql);
            //    CshFill();
            //}
        }

        private void CshEdit_Click(object sender, RoutedEventArgs e)
        {
            //DataRowView dataRow = (DataRowView)DataGridCsh.SelectedItem;
            //int cshId = Int32.Parse(dataRow.Row.ItemArray[0].ToString());
            //string dateCsh = dataRow.Row.ItemArray[1].ToString();
            //int sttIdFrom = Int32.Parse(dataRow.Row.ItemArray[2].ToString());
            //string sttFrom = dataRow.Row.ItemArray[3].ToString();
            //int sttIdTo = Int32.Parse(dataRow.Row.ItemArray[4].ToString());
            //string sttTo = dataRow.Row.ItemArray[5].ToString();
            //double cshSum = Double.Parse(dataRow.Row.ItemArray[6].ToString());
            //string cshComment = dataRow.Row.ItemArray[8].ToString();
            ButtonAdd.Content = "Изменить";
            //db3.CshId = cshId.ToString();
            //DateCsh.Text = dateCsh;
            //TextBoxFrom.Text = sttFrom;
            //db3.SttIdFrom = sttIdFrom.ToString();
            //TextBoxTo.Text = sttTo;
            //db3.SttIdTo = sttIdTo.ToString();
            //TextBoxSum.Text = cshSum.ToString(CultureInfo.InvariantCulture);
            //TextBoxComment.Text = cshComment;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Thread.CurrentThread.CurrentCulture = new CultureInfo("ru-Ru");
            Thread.CurrentThread.CurrentUICulture = new CultureInfo("ru-Ru");
            LanguageProperty.OverrideMetadata(typeof(FrameworkElement), new FrameworkPropertyMetadata(
            XmlLanguage.GetLanguage(CultureInfo.CurrentCulture.IetfLanguageTag)));
            Refresh();
        }

        private void TextBoxSumFrom_TextChanged(object sender, TextChangedEventArgs e)
        {
            try { TextBoxTo.Focus(); }
            catch { }
        }

        private void Refresh()
        {
            //Очищаю DataGrid критериев и проектов
            DataGridCtg.DataContext = null;
            DataGridPrj.DataContext = null;
            DataGridCsh.DataContext = null;
            //Заполняю основную таблицу
            _csh = new Csh();
            _csh.Fill(DataGridCsh);
            ButtonAdd.Content = "Добавить";
            //Заполняю критерии
            _ctg = new Ctg(DataGridCtg, TextBoxFrom);
            _prj = new Prj(_ctg);
            _stt = new Stt(_ctg);
            //Обнуляем переменные и сумму
            ButtonAdd.IsEnabled = false;
            TextBoxSum.Text = "0.00";
            TextBoxComment.Text = "";
            DateCsh.Text = DateTime.Now.ToString(CultureInfo.CurrentCulture);
            TextBoxTo.Text = "Куда";
            TextBoxFrom.Text = "Откуда";
            TextBoxSumFrom.Text = "";
            TextBoxSumTo.Text = "";
            TextBoxFrom.Focus();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Refresh();
        }

        private void DataGridCsh_MouseUp(object sender, MouseButtonEventArgs e)
        {
            //CshSelect();
        }
    }
}
