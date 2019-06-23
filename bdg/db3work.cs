using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SQLite;
using System.Diagnostics;
using System.IO;
using System.Windows;

namespace bdg
{
    class db3work
    {
        private string _pathToBdg = Environment.CurrentDirectory + @"\bdg.db3";
        private string _msg;
        private SQLiteConnection _conn;
        private SQLiteCommand _comm;
        private string _sql;

        public db3work(string sql)
        {
            _sql = sql;
            _msg = "Нет файла базы данные \n" + _pathToBdg;
            if (!File.Exists(_pathToBdg))
            {
                MessageBox.Show(_msg);
                Environment.Exit(0);
            }

            ConnectDb3();
        }

        private void ConnectDb3()
        {

            string strConnect = $@"Data Source={_pathToBdg}; Version=3;";
            SQLiteConnection conn = new SQLiteConnection(strConnect);

            try
            {
                conn.Open();
                _conn = conn;
                CommandDb3();
            }
            catch (SQLiteException ex)
            {
                MessageBox.Show(ex.Message);
                Environment.Exit(0);
            }
        }

        private void CommandDb3()
        {
            SQLiteCommand cmd = new SQLiteCommand(_conn);
            cmd.CommandText = _sql;
            _comm = cmd;
        }

        public void RunSql()
        {
            _comm.ExecuteNonQuery();
        }

        public DataTable SelectSql()
        {
            SQLiteDataReader reader = _comm.ExecuteReader();

            DataTable table = new DataTable();
            table.Load(reader);
            return table;
        }

        public string ScalarSql()
        {
            string res;
            try
            {
                res = _comm.ExecuteScalar().ToString();
            }
            catch
            {
                res = null;
            }
            return res;
        }

        public Dictionary<String, string> ValuesRow()
        {
            //TODO: Сделать проверку на кол-во строк, если больше одной, возвращаем ошибку
            Dictionary<string, string> dic = new Dictionary<string, string>();
            SQLiteDataReader rd = _comm.ExecuteReader();

            if (rd.StepCount != 1)
            {
                string msg = $"Ошибка! Метод вернул {rd.StepCount.ToString()} строк";
                return null;
            }

            if (rd.HasRows)
            {
                while (rd.Read())
                {
                    //Console.WriteLine("{0}\t{1}", rd.GetInt32(0),
                    //    rd.GetString(1));
                    for (int i = 0; i < rd.FieldCount; i++)
                    {
                        //dic.Add(int.Parse(rd[i].ToString()), rd.GetName(i));
                        dic.Add(rd.GetName(i), rd.GetValue(i).ToString());
                    }

                }
            }
            else
            {
                Console.WriteLine("No rows found.");
            }
            rd.Close();


            return dic;
        }
    }
}
