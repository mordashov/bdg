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
        private db3work db3 = new db3work();

        private string _ctgId; //ID Каталога
        public string CtgId { get => _ctgId; set => _ctgId = value; }

        private string _ctgTxt; //Наименование каталога
        public string CtgTxt { get => _ctgTxt; set => _ctgTxt = value; }

        private string _prjId; //ID проекта
        public string PrjId { get => _prjId; set => _prjId = value; }

        private string _prjTxt; //Наименование проекта
        public string PrjTxt { get => _prjTxt; set => _prjTxt = value; }

        //_sttId[0] - хранит значение для stt_id_from
        //_sttId[1] - хранит значение для stt_id_to
        private string[] _sttId = new string[2];
        public string[] SttId { get => _sttId; set => _sttId = value; }

        //Для сохранения имени заполняемого поля (откуда/куда)
        private string _activeTextBox = "TextBoxFrom";

        private void CrtFill() //Заполняю DataGrid с критериями
        {
            string sql = @"
                SELECT [ctg_id]
                      ,[ctg_nm]
                  FROM [ctg]
                  ORDER BY [ctg_nm];
            ";
            DataTable table = db3.SelectSql(sql);
            DataGridCrt.DataContext = table.DefaultView;
            if (DataGridCrt.Items.Count != 0)
            {
                DataGridCrt.SelectedIndex = 0;
                DataGridCrt.ScrollIntoView(DataGridCrt.Items[0]);
            }

        }

        private void GetStt(DataGrid activeDataGrid) // Получение stt_id_to либо stt_id_from
        {
            // Параметры:
            // DataGrid activeDataGrid - DataGrid с категорией или DataGrid с проектом

            int i = 0;
            string nameSttId = "";
            switch (_activeTextBox)
            {
                case "TextBoxFrom":
                    nameSttId = "stt_id_from";
                    i = 0;
                    break;
                case "TextBoxTo":
                    nameSttId = "stt_id_to";
                    i = 1;
                    break;
            }

            DataRowView drv = (DataRowView)activeDataGrid.SelectedItem;

            switch (activeDataGrid.Name)
            {
                case "DataGridCrt":
                    _ctgId = drv[0].ToString();
                    _ctgTxt = drv[1].ToString();
                    ((TextBox)StackPanelEnter.FindName(_activeTextBox)).Text = _ctgTxt;
                    _prjId = null;
                    _prjTxt = null;
                    FillProjects(_ctgId, nameSttId);
                    break;
                case "DataGridPrj":
                    _prjId = drv[0].ToString();
                    _prjTxt = drv[1].ToString();
                    string sql = $"SELECT stt_id FROM stt WHERE ctg_id = {_ctgId} and prj_id = {_prjId}";
                    SttId[i] = db3.ScalarSql(sql);
                    ((TextBox)StackPanelEnter.FindName(_activeTextBox)).Text = _ctgTxt + " / " + _prjTxt;
                    if (SttId[i] == "0")
                    {
                        MessageBoxResult msg = MessageBox.Show($"Добавить связь {_ctgTxt} - {_prjTxt}?", "", MessageBoxButton.YesNo);
                        if (msg == MessageBoxResult.Yes)
                            if (SttId[i] == "0")
                            {
                                db3.RunSql($"INSERT INTO stt (ctg_id, prj_id) VALUES({_ctgId}, {_prjId});");
                                SttId[i] = db3.ScalarSql(sql);
                            }

                        if (msg == MessageBoxResult.No)
                        {
                            SttId[i] = null;
                            break;
                        }
                    }
                    break;
            }
            //Если есть неопределенный sttId, то кнопка добавить - disable
            for (int n = 0; n < _sttId.Length; n++)
            {
                ButtonAdd.IsEnabled = true;
                if (_sttId[n] == null)
                {
                    ButtonAdd.IsEnabled = false;
                    break;
                }
            }

        }

        private void FillProjects(string ctgId, string nameSttId)
        {
            //Заполнения грида с проектами
            //Параметры:
            // ctgId - id категории
            // nameSttId - имя поля stt_id_from либо stt_id_to

            string subSql = $"SUM(CASE c.ctg_id WHEN {ctgId} THEN 1 ELSE 0 END)";
            string sql = $@"
                            SELECT p.prj_id, prj_nm, 
                            {subSql} AS ctg_id,
                            CASE {subSql} WHEN 0 THEN 0 ELSE 1 END AS color 
                            FROM prj p
                            LEFT JOIN stt s ON s.prj_id = p.prj_id
                            LEFT JOIN ctg c ON c.ctg_id = s.ctg_id
                            LEFT JOIN csh h ON h.{nameSttId} = s.stt_id
                            GROUP BY p.prj_id, p.prj_nm
                            ORDER BY {subSql} DESC, prj_nm;
                            ";
            DataTable table = db3.SelectSql(sql);
            DataGridPrj.DataContext = table;
        }

        private void CshFill() //Заполняю основную DataGrid с перечнем движения денежных средств
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
            DataTable table = db3.SelectSql(sql);
            DataGridCsh.DataContext = table.DefaultView;
            ButtonAdd.Content = "Добавить";
        }

        private void DataGridCrt_MouseUp(object sender, MouseButtonEventArgs e)
        {
            GetStt((DataGrid)sender);
        }

        private void DataGridCrt_KeyUp(object sender, KeyEventArgs e)
        {
            GetStt((DataGrid)sender);
        }

        private void DataGridPrjSelect() //Изменение выбора в проектах
        {
            DataRowView drv = (DataRowView)DataGridPrj.SelectedItem;
            _prjId = drv.Row[0].ToString();
            _prjTxt = drv.Row[1].ToString();

            GetStt(DataGridPrj);
            GetTotalSum(_sttId[0], TextBoxSumFrom);
            GetTotalSum(_sttId[1], TextBoxSumTo);
        }

        private void DataGridPrj_MouseUp(object sender, MouseButtonEventArgs e)
        {
            DataGridPrjSelect();
        }

        private void DataGridPrj_KeyUp(object sender, KeyEventArgs e)
        {
            DataGridPrjSelect();
        }

        private void textBoxTo_GotFocus(object sender, RoutedEventArgs e)
        {
            _activeTextBox = ((TextBox) sender).Name;
        }

        private void textBoxFrom_GotFocus(object sender, RoutedEventArgs e)
        {
            db3.PrjIdTo = "%";
            _activeTextBox = ((TextBox)sender).Name;
        }

        private void GetTotalSum(string sttId, TextBox TextBoxSum)
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
                sum[i] = double.Parse(db3.ScalarSql(sql));
            }

            TextBoxSum.Text = $"{sum[0] - sum[1]:C}";
        }

        private void buttonAdd_Click(object sender, RoutedEventArgs e) //Добавление/изменение данных в csh
        {

            string sttIdFrom;
            string sttIdTo;
            DateTime dt;

            //если изменяем строку
            //if (ButtonAdd.Content.ToString() == "Изменить")
            //{
            //    string cshId = db3.CshId;
            //    dt = Convert.ToDateTime(DateCsh.Text);
            //    string cshDate = dt.ToString("yyyy-MM-dd");
            //    sttIdFrom = db3.SttIdFrom;
            //    sttIdTo = db3.SttIdTo;
            //    string cshSum = TextBoxSum.Text;
            //    string cshNote = TextBoxComment.Text;
            //    string sql = $@"UPDATE csh SET 
            //                    [csh_dt] = '{cshDate}'
            //                    , [stt_id_from] = {sttIdFrom} 
            //                    , [stt_id_to] = {sttIdTo}  
            //                    , [csh_sum] = {cshSum}
            //                    , [csh_pln] = 0
            //                    , [csh_note] = '{cshNote}'
            //                    WHERE csh_id = {cshId}";
            //    db3.RunSql(sql);
            //    CshFill();
            //    return;
            //}

            //если добавляем строку
            //sttIdFrom = SttInsert(db3.CtgIdFrom, db3.PrjIdFrom);
            //sttIdTo = SttInsert(db3.CtgIdTo, db3.PrjIdTo);
            //if (sttIdFrom == "0" || sttIdTo == "0")
            //{
            //    MessageBox.Show("Данные не добавлены!");
            //}
            //else
            //{
                //Добавление новой строки
                dt = Convert.ToDateTime(DateCsh.Text);
                string plan = CheckBoxPln.IsChecked.ToString() == "0" ? "1" : "0";
                string sql = $@"
                INSERT INTO csh
                (csh_dt, stt_id_from, stt_id_to, csh_sum, csh_pln, csh_note)
                VALUES('{dt:yyyy-MM-dd}', {SttId[0]}, {SttId[1]}, {TextBoxSum.Text}, {plan}, '{TextBoxComment.Text}');";
                db3.RunSql(sql);
                //Обновляем гриды, переменные и текстбоксы
                Refresh();

            //}
            
        }

        private string  SttInsert(string crtId, string prjId) //Добавление новой статьи
        {
            // Проверка наличия связки категори и проекта в stt
            string sqlSttId = $@"
                SELECT stt_id
                FROM stt
                WHERE ctg_id = {crtId} AND prj_id = {prjId};
            ";
            string sttId = db3.ScalarSql(sqlSttId);
            MessageBoxResult msg = MessageBoxResult.No;
            if (sttId == "0")
                msg = MessageBox.Show("Добавить новую связку Категрия-проект?", "", MessageBoxButton.YesNo);

            if (msg == MessageBoxResult.Yes)
            {
                //Если связки нет - добавляем
                if (sttId == "0")
                {
                    string sql = $@"
                INSERT INTO stt
                (ctg_id, prj_id)
                VALUES({crtId}, {prjId});
                ";
                    db3.RunSql(sql);
                }
            }

            return db3.ScalarSql(sqlSttId);

        }

        private void CshSelect()
        {
            DataRowView dataRow = (DataRowView)DataGridCsh.SelectedItem ?? (DataRowView)DataGridCsh.Items[0];
            int countColumn = dataRow.Row.ItemArray.Length; 
            for (int i = 0; i < countColumn; i++)
            {
                string columnName = DataGridCsh.Columns[i].Header.ToString();
                string cellValue = ((DataRowView)DataGridCsh.Items[i]).ToString();
                //string[,] fieldsArray = new string[6,2];
                //fieldsArray[0, 0] = "stt_id_from";
                //fieldsArray[0, 1] = SttId[0];
                //fieldsArray[1, 0] = "stt_id_to";
                //fieldsArray[1, 1] = SttId[1];
                //fieldsArray[2, 0] = "От куда";
                //fieldsArray[2, 1] = "От куда";
                //fieldsArray[3, 0] = "Куда";
                //fieldsArray[3, 1] = "Куда";
                //fieldsArray[4, 0] = "Сумма";
                //fieldsArray[4, 1] = "Сумма";
                //fieldsArray[5, 0] = "Примечение";
                //fieldsArray[5, 1] = "Примечение";

                switch (columnName)
                {
                    case "stt_id_from":
                        SttId[0] = cellValue;
                        break;
                    case "stt_id_to":
                        SttId[1] = cellValue;
                        break;
                    case "От куда":
                        TextBoxFrom.Text = cellValue;
                        break;
                    case "Куда":
                        TextBoxTo.Text = cellValue;
                        break;
                    case "Сумма":
                        TextBoxSum.Text = cellValue;
                        break;
                    case "Примечение":
                        TextBoxComment.Text = cellValue;
                        break;
                }
            }
            string cshId = dataRow.Row.ItemArray[0].ToString();
        }

        //private void dataGridCsh_SelectionChanged(object sender, SelectionChangedEventArgs e)
        //{
        //    DataRowView dataRow = (DataRowView)DataGridCsh.SelectedItem ?? (DataRowView)DataGridCsh.Items[0];
        //    string cshId = dataRow.Row.ItemArray[0].ToString();

        //    string sql = $@"
        //    SELECT [csh_id]
        //          ,[csh_dt]
        //          ,[stt_id_from]
        //          ,ctg_from.ctg_id as ctg_from
        //          ,ctg_from.ctg_nm as ctg_from_nm
        //          ,prj_from.prj_id as prj_from
        //          ,prj_from.prj_nm as prj_from_nm
        //          ,[stt_id_to]
        //          ,ctg_to.ctg_id as ctg_to
        //          ,ctg_to.ctg_nm as ctg_to_nm
        //          ,prj_to.prj_id as prj_to
        //          ,prj_to.prj_nm as prj_to_nm
        //          ,[csh_sum]
        //          ,[csh_pln]
        //          ,[csh_note]
        //    FROM [csh]
        //    INNER JOIN stt as stt_from ON stt_from.stt_id = csh.stt_id_from
        //    INNER JOIN ctg as ctg_from ON ctg_from.ctg_id = stt_from.ctg_id
        //    INNER JOIN prj as prj_from ON prj_from.prj_id = stt_from.prj_id

        //    INNER JOIN stt as stt_to ON stt_to.stt_id = csh.stt_id_to
        //    INNER JOIN ctg as ctg_to ON ctg_to.ctg_id = stt_to.ctg_id
        //    INNER JOIN prj as prj_to ON prj_to.prj_id = stt_to.prj_id

        //    WHERE csh_id = {cshId}";

        //    DataTable table = db3.SelectSql(sql);
        //    int itemCount = table.Rows[0].ItemArray.Length;
        //    for (int i = 0; i < itemCount; i++)
        //    {
        //        string columnName = table.Columns[i].ColumnName;
        //        string fieldValue = table.Rows[0].ItemArray[i].ToString();
        //        switch (columnName)
        //        {
        //            case "ctg_from":
        //                db3.CtgId = fieldValue;
        //                break;
        //            case "ctg_from_nm":
        //                db3.CtgText = fieldValue;
        //                break;
        //            case "prj_from":
        //                db3.PrjId = fieldValue;
        //                break;
        //            case "prj_from_nm":
        //                db3.PrjText = fieldValue;
        //                TextBoxFromTo.Text = "Откуда:";
        //                RestExpenditure();
        //                break;
        //            case "ctg_to":
        //                db3.CtgId = fieldValue;
        //                break;
        //            case "ctg_to_nm":
        //                db3.CtgText = fieldValue;
        //                break;
        //            case "prj_to":
        //                db3.PrjId = fieldValue;
        //                break;
        //            case "prj_to_nm":
        //                db3.PrjText = fieldValue;
        //                //TextBoxFromTo.Text = "Куда:";
        //                RestExpenditure();
        //                //TextBoxFromTo.Text = "Откуда:";
        //                break;
        //        }
        //        ButtonAdd.Content = "Добавить";
        //    }

        //    db3.ConnectDb3().Close();
        //    TextBoxFrom.Focus();
        //}

        private void textBoxSum_LostFocus(object sender, RoutedEventArgs e)
        {
            //Для sqlite меняю запятую на точку
            TextBoxSum.Text = TextBoxSum.Text.Replace(",", ".");
            TextBoxSum.Text = TextBoxSum.Text.Replace(" ", "");
            try
            {
                //Для парсера меняю точку на запятую (в системе разделитель запятая)
                double d = double.Parse(TextBoxSum.Text.Replace(".", ","));
            }
            catch (Exception)
            {
                MessageBox.Show("Неверная сумма!");
            }

        }
        private void TextBoxSum_GotFocus(object sender, RoutedEventArgs e)
        {
            if (TextBoxSum.Text == "0.00")
                TextBoxSum.Text = null;
        }
        private void dataGridCrt_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            TextBox newValue = e.EditingElement as TextBox;
            string oldValue = db3.CtgText;
            if (newValue != null && newValue.Text != oldValue)
            {
                string id = ((DataRowView) DataGridCrt.SelectedItem).Row["ctg_id"].ToString();
                string nm = ((DataRowView) DataGridCrt.SelectedItem).Row["ctg_nm"].ToString();
                if (nm == oldValue) nm = newValue.Text;
                //Обновляю в базе
                string sql = $@"
                    UPDATE ctg SET ctg_nm = '{nm}'
                    WHERE ctg_id = " + id;
                db3.RunSql(sql);
                CshFill();
            }
        }

        private void CtgDel_Click(object sender, RoutedEventArgs e)
        {
            DataRowView dataRow = (DataRowView)DataGridCrt.SelectedItem;
            string ctgId = dataRow.Row.ItemArray[0].ToString();

            //Проверка используется ли категория в основной таблице
            string sql = $@"
            SELECT COUNT(csh_from.stt_id_from)
              FROM [ctg]
              INNER JOIN stt ON stt.ctg_id = ctg.ctg_id
              LEFT JOIN csh AS csh_from ON csh_from.stt_id_from = stt.stt_id
              LEFT JOIN csh AS csh_to ON csh_to.stt_id_to = stt.stt_id
              WHERE [ctg].[ctg_id] = {ctgId}
              GROUP BY csh_from.stt_id_from
              ;";
            string rowsCount = db3.ScalarSql(sql);
            if (rowsCount == "0")
            {
                //Удаление связки в stt
                sql = $@"DELETE FROM stt WHERE ctg_id = {ctgId}";
                db3.RunSql(sql);

                //Удаление категории
                sql = $@"DELETE FROM ctg WHERE ctg_id = {ctgId}";
                db3.RunSql(sql);
                //Заполнение DataGrid
                CrtFill();
            }
            else
                MessageBox.Show("Внимание, категория используется в основной таблие!\nЕе удалить нельзя!");
        }

        private void CtgNew_Click(object sender, RoutedEventArgs e)
        {
            string sql = "INSERT INTO ctg (ctg_nm) VALUES ('-= Новая строка =-')";
            db3.RunSql(sql);
            CrtFill();
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
            MessageBoxResult msg = MessageBox.Show("Будет удалена выбранная строка!\nПродолжить?", "", MessageBoxButton.YesNo);
            if (msg == MessageBoxResult.Yes)
            {
                DataRowView dataRow = (DataRowView)DataGridCsh.SelectedItem;
                string cshId = dataRow.Row.ItemArray[0].ToString();
                string sql = $@"DELETE FROM csh WHERE csh_id = {cshId}";
                db3.RunSql(sql);
                CshFill();
            }
        }

        private void DataGridPrj_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            TextBox newValue = e.EditingElement as TextBox;
            string oldValue = db3.PrjText;
            if ( string.IsNullOrEmpty(newValue.Text) && newValue.Text != oldValue)
            {
                string id = ((DataRowView)DataGridPrj.SelectedItem).Row["prj_id"].ToString();
                string nm = ((DataRowView)DataGridPrj.SelectedItem).Row["prj_nm"].ToString();
                if (nm == oldValue) nm = newValue.Text;
                //Обновляю в базе
                string sql = $@"
                    UPDATE prj SET prj_nm = '{nm}'
                    WHERE prj_id = " + id;
                db3.RunSql(sql);
                CshFill();
            }
        }

        private void CshEdit_Click(object sender, RoutedEventArgs e)
        {
            DataRowView dataRow = (DataRowView)DataGridCsh.SelectedItem;
            int cshId = Int32.Parse(dataRow.Row.ItemArray[0].ToString());
            string dateCsh = dataRow.Row.ItemArray[1].ToString();
            int sttIdFrom = Int32.Parse(dataRow.Row.ItemArray[2].ToString());
            string sttFrom = dataRow.Row.ItemArray[3].ToString();
            int sttIdTo = Int32.Parse(dataRow.Row.ItemArray[4].ToString());
            string sttTo = dataRow.Row.ItemArray[5].ToString();
            double cshSum = Double.Parse(dataRow.Row.ItemArray[6].ToString());
            string cshComment = dataRow.Row.ItemArray[8].ToString();
            ButtonAdd.Content = "Изменить";
            db3.CshId = cshId.ToString();
            DateCsh.Text = dateCsh;
            TextBoxFrom.Text = sttFrom;
            db3.SttIdFrom = sttIdFrom.ToString();
            TextBoxTo.Text = sttTo;
            db3.SttIdTo = sttIdTo.ToString();
            TextBoxSum.Text = cshSum.ToString(CultureInfo.InvariantCulture);
            TextBoxComment.Text = cshComment;
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
            DataGridCrt.DataContext = null;
            DataGridPrj.DataContext = null;
            //Заполняю основную таблицу
            CshFill();
            //Заполняю критерии
            CrtFill();
            //Обнуляем переменные и сумму
            SttId[0] = "0";
            SttId[1] = "0";
            _ctgId = null;
            _ctgTxt = null;
            _prjId = null;
            _ctgId = null;
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
            CshSelect();
        }
    }
}
