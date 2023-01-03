using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Data;
using System.Windows.Controls;

namespace ChainbotStudio.Views
{
    public partial class DataExtractorPageFourWindow : Page
    {
        public DataExtractorPageFourWindow()
        {
            InitializeComponent();
            dt = new DataTable();
        }

        public void InitPageData(string col,string strData)
        {
            JObject jo = (JObject)JsonConvert.DeserializeObject(strData);
            JObject data = jo.Value<JObject>("data");
            JArray arr = data.Value<JArray>("grid");
            DataTable dtIndex = new DataTable();

            col = GetColumnsByDataTable(col);
            dtIndex.Columns.Add(col);

            foreach (var item in arr)
            {
                if (item.Type.ToString() == "String")
                    dtIndex.Rows.Add(item);
                else if (item.Type.ToString() == "JArray" || item.Type.ToString() == "Array")
                    dtIndex.Rows.Add(item[0]);
            }
            dt = UniteDataTable(dt, dtIndex);
            dataGrid.ItemsSource = dt.DefaultView;
        }

        private void ResizeGridViewColumn(GridViewColumn column)
        {
            if (double.IsNaN(column.Width))
            {
                column.Width = column.ActualWidth;
            }

            column.Width = double.NaN;
        }

        public void InitPageData(string col,string col2, string strData)
        {
            JObject jo = (JObject)JsonConvert.DeserializeObject(strData);
            JObject data = jo.Value<JObject>("data");
            JArray arr = data.Value<JArray>("grid");

            DataTable dtIndex = new DataTable();

            col = GetColumnsByDataTable(col);
            dtIndex.Columns.Add(col);

            col2 = GetColumnsByDataTable(col2);
            dtIndex.Columns.Add(col2);
            foreach (var item in arr)
            {
                dtIndex.Rows.Add(item[0],item[1]);
            }
            dt = UniteDataTable(dt,dtIndex);
            dataGrid.ItemsSource = dt.DefaultView;
        }


        private DataTable UniteDataTable(DataTable DataTable1, DataTable DataTable2)
        {
            DataTable newDataTable = DataTable1.Clone();
            for (int i = 0; i < DataTable2.Columns.Count; i++)
            {
                newDataTable.Columns.Add(DataTable2.Columns[i].ColumnName);
            }
            object[] obj = new object[newDataTable.Columns.Count];
            for (int i = 0; i < DataTable1.Rows.Count; i++)
            {
                DataTable1.Rows[i].ItemArray.CopyTo(obj, 0);
                newDataTable.Rows.Add(obj);
            }

            if (DataTable1.Rows.Count >= DataTable2.Rows.Count)
            {
                for (int i = 0; i < DataTable2.Rows.Count; i++)
                {
                    for (int j = 0; j < DataTable2.Columns.Count; j++)
                    {
                        newDataTable.Rows[i][j + DataTable1.Columns.Count] = DataTable2.Rows[i][j].ToString();
                    }
                }
            }
            else
            {
                DataRow dr3;

                for (int i = 0; i < DataTable2.Rows.Count - DataTable1.Rows.Count; i++)
                {
                    dr3 = newDataTable.NewRow();
                    newDataTable.Rows.Add(dr3);
                }
                for (int i = 0; i < DataTable2.Rows.Count; i++)
                {
                    for (int j = 0; j < DataTable2.Columns.Count; j++)
                    {
                        newDataTable.Rows[i][j + DataTable1.Columns.Count] = DataTable2.Rows[i][j].ToString();
                    }
                }
            }
            return newDataTable;
        }

        public string GetColumnsByDataTable(string col)
        {
            if (dt.Columns.Count > 0)
            {
                int columnNum = 0;
                columnNum = dt.Columns.Count;
                for (int i = 0; i < dt.Columns.Count; i++)
                {
                    string colName = dt.Columns[i].ColumnName;
                    if(col == colName)
                    {
                        col = col + 1;
                        GetColumnsByDataTable(col);
                    }
                }
            }
            return col;
        }

        public void InitPageData(string strData)
        {
            JObject jo = (JObject)JsonConvert.DeserializeObject(strData);
            JObject data = jo.Value<JObject>("data");
            JArray headArr = data.Value<JArray>("headers");
            JArray arr = data.Value<JArray>("grid");
            if (dt == null)
                dt = new DataTable();
            for (int i = 0;i<headArr.Count;i++)
            {
                dt.Columns.Add("Column" + i);
            }
            List<string> listArr = null;
            foreach (var item in arr)
            {
                if ((item is JObject && (item as JObject).Count > 0) || (item is JArray && (item as JArray).Count > 0))
                {
                    listArr = new List<string>();
                    foreach (string iter in item)
                    {
                        listArr.Add(iter);
                    }
                    dt.Rows.Add(listArr.ToArray());
                }
            }
            dataGrid.ItemsSource = dt.DefaultView;
        }

        public void ClearDataTable()
        {
            dt.Rows.Clear();
            dt.Columns.Clear();
        }

        private DataTable dt = null;
    }
}
