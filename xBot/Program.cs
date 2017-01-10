using EvilDICOM.Network;
using EvilDICOM.Network.Querying;
using EvilDICOM.Network.DIMSE.IOD;
using System;
using System.Data;
using System.Collections.Generic;
using System.Xml;

namespace xBot
{
    /// <summary>
    /// xBot sends instructions to a ARIA Database Daemon service.
    /// The information is used to conduct a DICOM CMOVE operation from the ARIA OIS to third party systems.
    /// The EvilDICOM (https://github.com/rexcardan/Evil-DICOM) library is used as a middle hand.
    /// </summary>
    public class Program
    {

        //static variables for the DB Daemon
        static string AEtitleDB = "ARIADB";
        static int portDB = 57347;
        
       
        [STAThread]
        public static void Main()
        {
            // define the DB Deamon entity
            Entity daemon = Entity.CreateLocal(AEtitleDB, portDB);

            // define the local service class
            var me = Entity.CreateLocal("EvilDICOMC", 50400);
            var scu = new DICOMSCU(me);

            // define the query builder
            var qb = new QueryBuilder(scu, daemon);
            ushort msgId = 1;

            // load the xporter xml
            XmlDocument root = new XmlDocument();
            root.Load(Settings.xPorterPath);

            // loop through each xporter
            XmlNodeList xPortNodes = root.SelectNodes("//xports/xporter");
            foreach (XmlNode xPortNode in xPortNodes)
            {
                // init xPorter
                xPorter xPort = new xPorter(xPortNode.CloneNode(true));

                // Query plan
                DataTable plans = new DataTable();
                if (!String.IsNullOrEmpty(xPort.SqlString))
                { 
                    SqlInterface.Connect();
                    plans = xPort.Query();
                    SqlInterface.Disconnect();
                }
                // loop through plans
                List<CFindImageIOD> iods = new List<CFindImageIOD>();
                foreach (DataRow row in plans.Rows)
                {
                    var patId = (string)row["PatientId"];
                    var planUID = (string)row["UID"];
                    iods.Add(new CFindImageIOD() { PatientId = patId, SOPInstanceUID = planUID });

                    // loop through items and query based on type
                    foreach (string item in xPort.Items)
                    {
                        string itemSqlString = string.Empty;
                        switch (item)
                        {
                            case "planDose":
                                itemSqlString = "select distinct UID = DoseMatrix.DoseUID from DoseMatrix where DoseMatrix.PlanSetupSer = " + (Int64)row["PlanSer"];
                                break;
                            case "fieldDoses":
                                itemSqlString = "select distinct UID = DoseMatrix.DoseUID from DoseMatrix, Radiation where Radiation.PlanSetupSer = " + (Int64)row["PlanSer"] + " and DoseMatrix.RadiationSer = Radiation.RadiationSer";
                                break;
                            case "slices":
                                itemSqlString = "select UID=Slice.SliceUID from Slice, Image, StructureSet, PlanSetup where PlanSetup.PlanSetupSer=" + (Int64)row["PlanSer"] + " and StructureSet.StructureSetSer=PlanSetup.StructureSetSer and Image.ImageSer=StructureSet.ImageSer and Slice.SeriesSer=Image.SeriesSer";
                                break;
                            case "structures":
                                itemSqlString = "select UID=StructureSet.StructureSetUID from PlanSetup, StructureSet where PlanSetup.PlanSetupSer=" + (Int64)row["PlanSer"] + " and StructureSet.StructureSetSer=PlanSetup.StructureSetSer";
                                break;
                            case "images":
                                itemSqlString = "select UID=Slice.SliceUID from Slice, ImageSlice, Radiation where Radiation.PlanSetupSer = " + (Int64)row["PlanSer"] + " and ImageSlice.ImageSer = Radiation.RefImageSer and Slice.SliceSer = ImageSlice.SliceSer";
                                break;
                            case "records":
                                itemSqlString = "select UID=TreatmentRecord.TreatmentRecordUID from TreatmentRecord, RTPlan where RTPlan.PlanSetupSer= " + (Int64)row["PlanSer"] + " and TreatmentRecord.RTPlanSer=RTPlan.RTPlanSer";
                                break;
                            default:
                                itemSqlString = String.Empty;
                                break;
                        }
                        if (!String.IsNullOrEmpty(itemSqlString))
                        {
                            DataTable includeItem = SqlInterface.Query(itemSqlString);
                            foreach (DataRow itemRow in includeItem.Rows)
                            {                              
                                iods.Add(new CFindImageIOD() { PatientId = patId, SOPInstanceUID = (string)itemRow["UID"] });
                            }
                        }
                    }
                    

                }

                // overwrite lastActivity
                if (plans.Rows.Count > 0)
                {
                    DateTime lastPlan = (DateTime)plans.Rows[plans.Rows.Count - 1]["DateTime"];
                    XmlNode actNode = xPortNode.SelectSingleNode("lastActivity");
                    actNode.InnerText = lastPlan.ToString("yyyy-MM-dd HH:mm:ss.fff");
                }

                if (xPort.Active)
                {
                    // Remove duplicate UIDs
                    if (!xPort.Doublets)
                        iods = ListHandler.Unique(iods);
                    foreach (var iod in iods)
                    {
                        // Send it
                        scu.SendCMoveImage(daemon, iod, xPort.Scp.ApplicationEntity.AeTitle, ref msgId);
                    }
                }
            }

            // write xml
            root.Save(Settings.xPorterPath);
        }

    }
}
