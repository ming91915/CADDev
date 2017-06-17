using System;
using System.Windows;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;

namespace eZcad.Addins.SlopeProtection
{
    /// <summary> 横断面的的竖向中心轴线 </summary>
    public class CenterAxis
    {
        #region --- Fields

        private readonly DocumentModifier _docMdf;

        public readonly BlockReference InfoBlock;


        public readonly string Mileage;

        /// <summary> 道路中心线 </summary>
        public Line Line { get; set; }

        /// <summary>
        /// 中心轴线在AutoCAD中的几何X坐标
        /// </summary>
        private readonly double _centerX;

        /// <summary> 道路中心的标高 </summary>
        public double CenterEle { get; private set; }
        private double _baseY;
        private bool _baseEleFound;

        #endregion

        #region --- 构造函数

        /// <summary>
        /// 不能成功构造，则返回 null
        /// </summary>
        /// <param name="docMdf"></param>
        /// <param name="line"></param>
        /// <returns></returns>
        public static CenterAxis Create(DocumentModifier docMdf, Line line)
        {
            //
            var infoBlock = FindInfoBlock(docMdf.acEditor, line);
            if (infoBlock != null)
            {
                string mileage = GetMileage(infoBlock);
                if (mileage != null)
                {
                    var slp = new CenterAxis(docMdf, line, infoBlock, mileage);
                    return slp;
                }
            }
            return null;
        }

        private CenterAxis(DocumentModifier docMdf, Line line, BlockReference infoBlock, string mileage)
        {
            _docMdf = docMdf;
            Line = line;
            _centerX = line.StartPoint.X;
            InfoBlock = infoBlock;
            Mileage = mileage;
            //
            GetCorrespondingLocation();
        }

        #endregion

        #region --- 在界面中搜索并过滤相关对象

        /// <summary>
        /// 
        /// </summary>
        private static BlockReference FindInfoBlock(Editor ed, Line l)
        {
            var filterWall = new[]
            {
                    new TypedValue((int) DxfCode.Start, "INSERT"),
                    new TypedValue((int) DxfCode.LayerName, ProtectionOptions.LayerName_SectionInfo),
                    new TypedValue((int) DxfCode.BlockName, ProtectionOptions.BlockName_SectionInfo), // 黄色 HDMD.HDM.SHUJ..横断面绘图模板 - 3点带横坡  "HDMD.HDM.SHUJ..横断面绘图模板-带标高"
                };
            var bottomP = l.EndPoint;
            var pt1 = new Point3d(bottomP.X - 5, bottomP.Y - 5, bottomP.Z);
            var pt2 = new Point3d(bottomP.X + 5, bottomP.Y, bottomP.Z);

            var res = ed.SelectCrossingWindow(
                pt1: pt1,
                pt2: pt2,
                filter: new SelectionFilter(filterWall)); // 确保想要被选中的图元的一部分出现在屏幕中
            if (res.Status == PromptStatus.OK)
            {
                // 可能会选中多个块参照
                var ids = res.Value.GetObjectIds();

                BlockReference closestBlk = null;
                double minDis = double.MaxValue;
                var lineBottom = l.StartPoint.Y < l.EndPoint.Y ? l.StartPoint : l.EndPoint;
                // 可能找到多条轴线，比较轴线中点到边坡线两个端点距离最小的那个轴线
                foreach (var id in ids)
                {
                    var blk = id.GetObject(OpenMode.ForRead) as BlockReference;
                    var blkTop = blk.Position;

                    var dist = lineBottom.DistanceTo(blkTop);

                    if (dist < minDis)
                    {
                        closestBlk = blk;
                        minDis = dist;
                    }
                }
                return closestBlk;
            }
            return null;
        }

        /// <summary>
        /// 轴线所对应的里程
        /// </summary>
        /// <returns></returns>
        private static string GetMileage(BlockReference infoBlock)
        {
            foreach (ObjectId attId in infoBlock.AttributeCollection)
            {
                var att = attId.GetObject(OpenMode.ForRead) as AttributeReference;
                if (att.Tag ==ProtectionOptions.MileageFieldDef)
                {

                    return att.TextString;
                }
            }
            return null;
        }

        /// <summary>
        /// 道路路面中心的标高
        /// </summary>
        private BlockReference FindAxisElevation()
        {
            var filterWall = new[]
            {
                new TypedValue((int) DxfCode.Start, "INSERT"),
                new TypedValue((int) DxfCode.LayerName, "0"),
                new TypedValue((int) DxfCode.BlockName, ProtectionOptions.BlockName_CenterElevation), // 黄色  "HDMD.ITEM.ZLJ.ZFG..横断面绘图模板-带标高"
            };

            var topP = Line.StartPoint;
            var bottomP = Line.EndPoint;
            var pt2 = new Point3d(topP.X + 0.5, topP.Y, topP.Z);
            var pt1 = new Point3d(bottomP.X - 0.5, bottomP.Y, bottomP.Z);

            var res = _docMdf.acEditor.SelectCrossingWindow(
                pt1: pt1,
                pt2: pt2,
                filter: new SelectionFilter(filterWall));
            if (res.Status == PromptStatus.OK)
            {
                // 由于界面显示精度，当界面缩放得过小时，可能会搜索到多个对象
                var blrs = res.Value.GetObjectIds();
                // Y 值位于轴线Y值范围之内，而且X值距离轴线最近
                BlockReference closestBlk = null;
                double minDis = double.MaxValue;
                foreach (var id in blrs)
                {
                    var blr = id.GetObject(OpenMode.ForRead) as BlockReference;
                    var p = blr.Position;
                    if (p.Y < topP.Y && p.Y > bottomP.Y)  // 位于轴线的Y值范围之内
                    {
                        var dis = Math.Abs(p.X - _centerX);
                        if (dis < minDis)
                        {
                            minDis = dis;
                            closestBlk = blr;
                        }
                    }
                }
                return closestBlk;
            }
            return null;
        }

        /// <summary>
        /// 道路中心线所对应的原自然地面的标高
        /// </summary>
        /// <returns></returns>
        public double FindNaturalSurfElevation()
        {
            var filterWall = new[]
            {
                new TypedValue((int) DxfCode.Start, "LWPOLYLINE"),
                new TypedValue((int) DxfCode.LayerName, ProtectionOptions.LayerName_GroundSurface),
                new TypedValue((int) DxfCode.Color, 7), // 白色
            };
            var topP = Line.StartPoint;
            var bottomP = Line.EndPoint;
            var pt2 = new Point3d(topP.X + 0.1, topP.Y, topP.Z);
            var pt1 = new Point3d(bottomP.X - 0.1, bottomP.Y, bottomP.Z);

            var res = _docMdf.acEditor.SelectCrossingWindow(
                pt1: pt1,
                pt2: pt2,
                filter: new SelectionFilter(filterWall));
            if (res.Status == PromptStatus.OK)
            {
                // 找到了自然地面线（应该有左右两条）
                var ids = res.Value.GetObjectIds();
                Point3dCollection intersects = new Point3dCollection();
                foreach (var id in ids)
                {
                    var pl = id.GetObject(OpenMode.ForRead) as Polyline;

                    if (pl != null)
                    {
                        Line.IntersectWith(pl, Intersect.OnBothOperands, intersects, new IntPtr(0), new IntPtr(0));
                        // IntersectWith 方法是不依赖与界面的缩放大小与显示分辨率的
                        // OnBothOperands 表示两个对象都不延伸
                        if (intersects.Count > 0)
                        {
                            return GetEleFromY(intersects[0].Y);
                        }
                    }
                }
            }
            return 0.0;
        }

        #endregion

        /// <summary>
        /// 边坡线是否在轴线的左侧
        /// </summary>
        /// <param name="pl"></param>
        /// <returns></returns>
        public bool IsOnLeft(Polyline pl)
        {
            var centerP = pl.GetPointAtParameter((pl.StartParam + pl.EndParam) / 2);
            return centerP.X < _centerX;
        }

        #region --- AutoCAD中几何坐标 与 横断面图中的 标高进行对应

        private bool GetCorrespondingLocation()
        {
            var blr = FindAxisElevation();
            if (blr != null)
            {
                _baseY = blr.Position.Y;
                CenterEle = double.Parse((blr.AttributeCollection[0].GetObject(OpenMode.ForRead) as AttributeReference).TextString);
                _baseEleFound = true;
                return true;
            }
            return false;
        }

        public double GetYFromElev(double elevation)
        {
            if (!_baseEleFound)
            {
                GetCorrespondingLocation();
            }
            return _baseY - CenterEle + elevation;
        }

        public double GetEleFromY(double y)
        {
            if (!_baseEleFound)
            {
                GetCorrespondingLocation();
            }
            return CenterEle - _baseY + y;
        }
        #endregion

        public void DrawWaterLevel(double waterLevel, ObjectId layer)
        {
            var wy = GetYFromElev(waterLevel);
            var l = new Line(new Point3d(_centerX - 15, wy, 0), new Point3d(_centerX + 15, wy, 0));
            //
            var tr = _docMdf.acTransaction;
            var btrT = tr.GetObject(_docMdf.acDataBase.BlockTableId, OpenMode.ForRead) as BlockTable;
            var btr = tr.GetObject(btrT[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;
            //
            l.LayerId = layer;
            btr.AppendEntity(l);
            tr.AddNewlyCreatedDBObject(l, true);
            l.Draw();
        }
    }
}