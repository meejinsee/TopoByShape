using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Windows.Forms;

using Autodesk.DesignScript.Geometry;
using Autodesk.DesignScript.Interfaces;
using Autodesk.DesignScript.Runtime;

using Autodesk.Revit.DB;

using DotSpatial.Projections;
using DotSpatial.Data;
using DotSpatial.Topology;
using DotSpatial.Symbology;
using DotSpatial.Serialization;
using System.Threading;
using Point = Autodesk.DesignScript.Geometry.Point;

namespace TopoByShape
{
    public class CreateTopo
    {
        [MultiReturn(new[] {"Shape", "Feature", "CRS", "Points", "PolyCurves" })]
        public static Dictionary<string, object> GetShapeInfor(string Files, string crs)
        {
            Dictionary<string, object> returndix = new Dictionary<string, object>();
            List<string> filepaths = new List<string>();
            string filepath = Files;
            //fe
            //if(Files == true)
            //{
            //    OpenFileDialog ofd = new OpenFileDialog();
            //    ofd.Filter = "Shapefiles|*.shp";
            //    ofd.Multiselect = true;

            //    if (ofd.ShowDialog() != DialogResult.OK) return returndix;

            //    foreach (string item in ofd.FileNames)
            //    {
            //        filepaths.Add(item);
            //    }
            //}

            var sf = Shapefile.OpenFile(filepath, null);

            string sss = "";
            if(crs == "")
            {
                sss = sf.ProjectionString;
            }
            else
            {
                sss = crs;
            }

            List<string> Colnams = new List<string>();
            DataColumn[] Cols = sf.GetColumns();
            foreach (DataColumn item in Cols)
            {
                Colnams.Add(item.ColumnName);
            }

            returndix.Add("Feature", Colnams);
            returndix.Add("CRS", sss);
            returndix.Add("Shape", sf);

            var Features1 = sf.Features;

            List<List<Point>> pts = new List<List<Point>>();

            foreach (var item in Features1)
            {
                IList<Coordinate> fe = item.Coordinates;
                List<Point> pts_in = new List<Point>();

                foreach (Coordinate xy in fe)
                {
                    double x = (xy.X*1000) / 304.8;
                    double y = (xy.Y*1000) / 304.8;

                    Point p1 = Point.ByCoordinates(x,y);
                    pts_in.Add(p1);
                }

                List<Autodesk.DesignScript.Geometry.Point> pupu = removeDuplicatePoints(pts_in);
                pts.Add(pupu);
            }

            List<PolyCurve> pc = new List<PolyCurve>();
            foreach (var item in pts)
            {
                PolyCurve pc1 = PolyCurve.ByPoints(item, false);
                pc.Add(pc1);
            }

            returndix.Add("Points", pts);
            returndix.Add("PolyCurves", pc);

            return returndix;
        }

        [MultiReturn(new[] { "Points", "PLine","AttValues" })]
        public static Dictionary<string, object> Result(Shapefile shape, string Value)
        {
            Dictionary<string, object> returndix = new Dictionary<string, object>();
            List<string> attValues = new List<string>();

            DataTable dt = shape.DataTable;
            object[] strs = dt.Select().Select(x => x[Value]).ToArray();
            returndix.Add("AttValues", strs);

            Dictionary<DataRow, IFeature> dix = shape.FeatureLookup;
            List<Autodesk.DesignScript.Geometry.Point> pts = new List<Autodesk.DesignScript.Geometry.Point>();
            List<PolyCurve> poly = new List<PolyCurve>();

            foreach (KeyValuePair<DataRow, IFeature> item in dix)
            {
                IList<Coordinate> fe = item.Value.Coordinates;
                List<Autodesk.DesignScript.Geometry.Point> pts_in = new List<Autodesk.DesignScript.Geometry.Point>();

                foreach (Coordinate xy in fe)
                {
                    Autodesk.DesignScript.Geometry.Point p1 = Autodesk.DesignScript.Geometry.Point.ByCoordinates(xy.X, xy.Y, 0);
                    pts.Add(p1);
                    pts_in.Add(p1);
                }

                PolyCurve pc = PolyCurve.ByPoints(pts_in, false);
                poly.Add(pc);
            }

            returndix.Add("Points", pts);
            returndix.Add("PLine", poly);

            return returndix;
        }

        [IsVisibleInDynamoLibrary(false)]
        public static List<Autodesk.DesignScript.Geometry.Point> removeDuplicatePoints(List<Autodesk.DesignScript.Geometry.Point> list)
        {
            List<Autodesk.DesignScript.Geometry.Point> returnpts = new List<Autodesk.DesignScript.Geometry.Point>();
            Dictionary<string, string> strs = new Dictionary<string, string>();

            for (int i = 0; i < list.Count; i++)
            {
                if(i == 0)
                {
                    strs.Add(list[i].ToString(), list[i].ToString());
                    returnpts.Add(list[i]);
                }
                else
                {
                    if(strs.ContainsKey(list[i].ToString()) != true)
                    {
                        strs.Add(list[i].ToString(), list[i].ToString());
                        returnpts.Add(list[i]);
                    }
                }
            }

            return returnpts;
        }
    }
}
