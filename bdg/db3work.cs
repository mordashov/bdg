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
        public string CtgId { get; set; } // Критерий временный
        public string CtgText { get; set; } //Критерий наименование временный
        public string PrjId { get; set; } // Проект временный
        public string PrjText { get; set; } //Проект наименование временный
        public string CtgIdFrom { get; set; } // Критерий откуда
        public string CtgIdTo { get; set; } // Критерий куда
        public string PrjIdFrom { get; set; } // Проект откуда
        public string PrjIdTo { get; set; } // Проект куда
        public string CshId { get; set; } // ID таблицы с расходами
        public string SttIdFrom { get; set; } // Статья расходов (откуда)
        public string SttIdTo { get; set; } // Статья расходов (куда)

        public SQLiteConnection ConnectDb3()
        {
            string bdg = Environment.CurrentDirectory + @"\bdg.db3";

            if (!File.Exists(bdg))
            {
                MessageBox.Show(
                    "Не могу найти фал базы данных.\n" +
                    "Он должен находиться в одной папке с исполняемым файлом: \n" +
                    Environment.CurrentDirectory);
                Environment.Exit(0);
            }

            string strConnect = $@"Data Source={bdg}; Version=3;";
            SQLiteConnection conn = new SQLiteConnection(strConnect);

            try
            {
                conn.Open();
            }
            catch (SQLiteException ex)
            {
                MessageBox.Show(ex.Message);
                Environment.Exit(0);
            }
            return conn;
        }
        public void RunSql(string sql)
        {
            SQLiteConnection conn = ConnectDb3();
            SQLiteCommand cmd = new SQLiteCommand(conn);
            cmd.CommandText = sql;
            cmd.ExecuteNonQuery();
            conn.Dispose();
        }
        public DataTable SelectSql(string sql)
        {
            SQLiteConnection conn = ConnectDb3();
            SQLiteCommand cmd = new SQLiteCommand(conn);
            cmd.CommandText = sql;
            SQLiteDataReader reader = cmd.ExecuteReader();

            DataTable table = new DataTable();
            table.Load(reader);
            return table;
        }
        public string ScalarSql(string sql)
        {
            SQLiteConnection conn = ConnectDb3();
            SQLiteCommand cmd = new SQLiteCommand(conn);
            cmd.CommandText = sql;
            string value;
            try
            {
                cmd.ExecuteScalar().ToString();
            }
            catch
            {
                value = "0";
                return value;
            }
            value = cmd.ExecuteScalar().ToString();
            return value;
        }
    }
}
