using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Text;

namespace Ait.Pe04.Octopus.Client.Core
{
    public class Config
    {
        
        private static void MakeConfigFile()
        {
            DataSet ds = new DataSet();
            DataTable dt = ds.Tables.Add();
            DataColumn dc;
            dc = new DataColumn();
            dc.ColumnName = "ClientIP";
            dc.DataType = typeof(string);
            dt.Columns.Add(dc);
            dc = new DataColumn();
            dc.ColumnName = "ServerIP";
            dc.DataType = typeof(string);
            dt.Columns.Add(dc);
            dc = new DataColumn();
            dc.ColumnName = "Port";
            dc.DataType = typeof(int);
            dt.Columns.Add(dc);
            dc = new DataColumn();
            dc.ColumnName = "PlaneName";
            dc.DataType = typeof(string);
            dt.Columns.Add(dc);
            DataRow dr = dt.NewRow();
            dr[0] = "127.0.0.1";
            dr[1] = "127.0.0.1";
            dr[2] = 49200;
            dr[3] = "unknown";
            dt.Rows.Add(dr);
            string xmlBestand = Directory.GetCurrentDirectory() + "/config.xml";
            ds.WriteXml(xmlBestand, XmlWriteMode.WriteSchema);
        }
    }
}
