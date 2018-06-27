using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestDeck_ImageGenerator
{
    public class FileDelimeter
    {
        public static DataTable DataFromTextFile(string location, char delimeter)
        {
            DataTable result;
            string[] LineArray = File.ReadAllLines(location);
            result = FormDataTable(LineArray, delimeter);
            return result;
        }

        private static DataTable FormDataTable(string[] LineArray, char delimeter)
        {
            DataTable dt = new DataTable();
            AddColumnToTable(LineArray, delimeter, ref dt);
            AddRowToTable(LineArray, delimeter, ref dt);
            return dt;
        }

        private static void AddRowToTable(string[] valueCollection, char delimeter, ref DataTable dt)
        {
            for (int i = 1; i < valueCollection.Length; i++)
            {
                string[] values = valueCollection[i].Split(delimeter);
                DataRow dr = dt.NewRow();
                for (int j = 0; j < values.Length; j++)
                {
                    dr[j] = values[j];
                }
                dt.Rows.Add(dr);
            }
        }

        private static void AddColumnToTable(string[] columnCollection, char delimeter, ref DataTable dt)
        {
            string[] columns = columnCollection[0].Split(delimeter);
            foreach (string columnName in columns)
            {
                DataColumn dc = new DataColumn(columnName, typeof(string));
                dt.Columns.Add(dc);
            }
        }
    }
}
