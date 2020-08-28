// (C) Copyright 2020 by  
//
using System;
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.EditorInput;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.IO;
using Autodesk.AutoCAD.ApplicationServices.Core;

// This line is not mandatory, but improves loading performances
[assembly: CommandClass(typeof(ExtractLength.MyCommands))]
// This line is not mandatory, but improves loading performances
[assembly: ExtensionApplication(null)]

namespace ExtractLength
{
    public class MyCommands
    {
        [CommandMethod("ComputeLength", CommandFlags.Modal)]
        public void ComputeLength()
        {
         
            JArray jarray = new JArray();
            Document mdiActiveDocument = Application.DocumentManager.MdiActiveDocument;
            Database database = mdiActiveDocument.Database;
            using (Transaction transaction = mdiActiveDocument.TransactionManager.StartTransaction())
            {
                BlockTable blockTable = transaction.GetObject(database.BlockTableId, 0) as BlockTable;
                BlockTableRecord blockTableRecord = transaction.GetObject(blockTable[BlockTableRecord.ModelSpace], 0) as BlockTableRecord;
                foreach (ObjectId objectId in blockTableRecord)
                {
                    Entity entity = transaction.GetObject(objectId, OpenMode.ForRead) as Entity;
                    switch (RXClass.GetClass(entity.GetType()).DxfName)
                    {
                        case "ARC":
                            {
                                Arc arc = entity as Arc;
                                jarray.Add(arc.Length.ToString());
                            }
                            break;
                        case "LINE":
                            {
                                Line line = entity as Line;
                                jarray.Add(line.Length.ToString());
                            }
                            break;
                        case "LWPOLYLINE":
                            {
                                Polyline pline = entity as Polyline;
                                jarray.Add(pline.Length.ToString());
                            }
                            break;

                        default:
                            break;

                    }
                }
            }
            using (StreamWriter streamWriter = File.CreateText("result.json"))
            {
                using (JsonTextWriter jsonTextWriter = new JsonTextWriter(streamWriter))
                {
                    jsonTextWriter.Formatting = Formatting.Indented;
                    jarray.WriteTo(jsonTextWriter, Array.Empty<JsonConverter>());
                }
            }
        }

        //[CommandMethod("ListLayers", CommandFlags.Modal)]
        //static public void ListLayers()
        //{
        //    JArray jarray = new JArray();
        //    var doc = Application.DocumentManager.MdiActiveDocument;
        //    var ed = doc.Editor;
        //    try
        //    {
        //        //extract layer names and save them to result.json
        //        var db = doc.Database;
        //        dynamic layers = db.LayerTableId;
        //        foreach (dynamic layer in layers)
        //            jarray.Add(layer.Name);
        //        using (StreamWriter streamWriter = File.CreateText("result.json"))
        //        {
        //            using (JsonTextWriter jsonTextWriter = new JsonTextWriter(streamWriter))
        //            {
        //                jsonTextWriter.Formatting = Formatting.Indented;
        //                jarray.WriteTo(jsonTextWriter, Array.Empty<JsonConverter>());
        //            }
        //        }                      
                
        //    }
        //    catch (System.Exception e)
        //    {
        //        ed.WriteMessage("Error: {0}", e);
        //    }
        //}
    }

}
