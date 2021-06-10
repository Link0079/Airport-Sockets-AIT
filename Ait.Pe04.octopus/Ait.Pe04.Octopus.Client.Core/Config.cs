using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Text;

namespace Ait.Pe04.Octopus.Client.Core
{
    public class Config
    {
        public static void GetConfig(out string clientIP, out string serverIP, out int port, out string planeName)
        {
            string xmlFile = Directory.GetCurrentDirectory() + "/config.xml";
            if (!File.Exists(xmlFile))
            {
                MakeConfigFile();
            }
            DataSet ds = new DataSet();
            ds.ReadXml(xmlFile, XmlReadMode.ReadSchema);
            clientIP = ds.Tables[0].Rows[0][0].ToString();
            serverIP = ds.Tables[0].Rows[0][1].ToString();
            port = int.Parse(ds.Tables[0].Rows[0][2].ToString());
            planeName = ds.Tables[0].Rows[0][3].ToString();
        }

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
        public static void WriteConfig(string clientIP, string serverIP, int port, string planeName)
        {
            string xmlBestand = Directory.GetCurrentDirectory() + "/config.xml";
            if (!File.Exists(xmlBestand))
            {
                MakeConfigFile();
            }
            DataSet ds = new DataSet();
            ds.ReadXml(xmlBestand, XmlReadMode.ReadSchema);
            ds.Tables[0].Rows[0][0] = clientIP;
            ds.Tables[0].Rows[0][1] = serverIP;
            ds.Tables[0].Rows[0][2] = port;
            ds.Tables[0].Rows[0][3] = planeName;
            ds.WriteXml(xmlBestand, XmlWriteMode.WriteSchema);
        }
    }
}
