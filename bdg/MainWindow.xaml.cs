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
using System.Windows.Media;
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
            if (_stt.CtgId == null) return;
            new Prj(_stt).Fill(DataGridPrj);
        }

        private void DataGridCtg_KeyUp(object sender, KeyEventArgs e)
        {
            CtgSelect((DataGrid)sender);
            new Prj(_stt).Fill(DataGridPrj);
        }

        private void PrjSelect() //Изменение выбора в проектах
        {
            DataRowView drv = (DataRowView)DataGridPrj.SelectedItem;
            if (drv == null) return;
            _prj.PrjId = drv.Row[0].ToString();
            _stt = new Stt(_ctg, _prj);

            switch (_stt.SttFromOrTo)
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
            switch (ButtonAdd.Content)
            {
                case "Добавить":
                    DateTime dt = Convert.ToDateTime(DateCsh.Text);
                    _csh.CshDate = dt;
                    _csh.CshNote = TextBoxComment.Text;
                    _csh.CshPln = CheckBoxPln.IsChecked == false ? "0" : "1";
                    _csh.Add();
                    break;
                case "Изменить":
                    _csh.Edit();
                    break;
            }
            Refresh();
        }

        private void textBoxSum_LostFocus(object sender, RoutedEventArgs e)
        {
            _csh.CshSum = TextBoxSum.Text;
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

        private void CtgNew_Click(object sender, RoutedEventArgs e)
        {
            //string sql = "INSERT INTO ctg (ctg_nm) VALUES ('-= Новая строка =-')";
            //db3.RunSql(sql);
            //CrtFill();
            new Ctg().Add(DataGridCtg);
        }

        private void CtgDel_Click(object sender, RoutedEventArgs e)
        {
            new Ctg().Del(DataGridCtg);
        }

        private void PrjNew_Click(object sender, RoutedEventArgs e)
        {
            new Prj(_stt).Add(DataGridPrj);
        }

        private void PrjDel_Click(object sender, RoutedEventArgs e)
        {
            new Prj(_stt).Del(DataGridPrj);
        }

        private void CshDel_Click(object sender, RoutedEventArgs e)
        {
            new Csh().Del(DataGridCsh);
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
            GetCshRowValues();
            ButtonAdd.Content = "Изменить";
            ButtonAdd.IsEnabled = true;
        }

        private void GetCshRowValues()
        {
            DataRowView dataRow = (DataRowView)DataGridCsh.SelectedItem;
            if (dataRow == null) return;
            _csh = null;
            _csh = new Csh();
            _csh.CshId = dataRow.Row.ItemArray[0].ToString();
            _csh.GetRowValues();
            DateCsh.Text = _csh.CshDate.ToShortDateString();
            TextBoxFrom.Text = _csh.SttIdFrom.SttName;
            TextBoxTo.Text = _csh.SttIdTo.SttName;
            TextBoxSum.Text = _csh.CshSum.ToString(CultureInfo.InvariantCulture);
            TextBoxComment.Text = _csh.CshNote;
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
            GetCshRowValues();
            ButtonAdd.Content = "Добавить";
            ButtonAdd.IsEnabled = true;
        }

        private void DataGridCtg_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
        }

        private void DataGridCtg_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            DataGrid dataGrid = (DataGrid)sender;
            DataGridRow dataRow = SelectionOnRightClick(e);
            if (dataRow == null) return;
            dataGrid.SelectedItem = dataRow.DataContext;
        }

        private void SttDel_Click(object sender, RoutedEventArgs e)
        {
            new Csh().DelStt(_stt);
        }

        private void DataGridPrj_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            DataGrid dataGrid = (DataGrid)sender;
            DataGridRow dataRow = SelectionOnRightClick(e);
            if (dataRow == null) return;
            dataGrid.SelectedItem = dataRow.DataContext;
        }

        private DataGridRow SelectionOnRightClick(MouseButtonEventArgs e)
        {
            DependencyObject dep = (DependencyObject)e.OriginalSource;
            while ((dep != null) && !(dep is DataGridCell))
            {
                dep = VisualTreeHelper.GetParent(dep);
            }
            if (dep == null) return null;

            if (dep is DataGridCell)
            {
                DataGridCell cell = dep as DataGridCell;
                cell.Focus();

                while ((dep != null) && !(dep is DataGridRow))
                {
                    dep = VisualTreeHelper.GetParent(dep);
                }

                DataGridRow dataRow = dep as DataGridRow;
                return dataRow;
            }

            return null;
        }

        private void DataGridCtg_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void DataGridCsh_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            DataGrid dataGrid = (DataGrid)sender;
            DataGridRow dataRow = SelectionOnRightClick(e);
            if (dataRow == null) return;
            dataGrid.SelectedItem = dataRow.DataContext;
        }
    }
}
