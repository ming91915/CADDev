using System.Collections.Generic;
using eZcad.SubgradeQuantity.Entities;

namespace eZcad.SubgradeQuantity.DataExport
{
    public partial class Exporter_SlopeProtection
    {
        /// <summary> 每一个桩号的某一侧边坡的所有子边坡与子平台所占据的几何宽度与面积 </summary>
        private class SlopeExpands
        {
            #region ---   Fields

            public double Station { get; }

            /// <summary> 其值可能为 null，表示此桩号断面上没有对应边坡 </summary>
            public SlopeData XData { get; }

            //
            /// <summary> key 表示子边坡的Index。集合中可能没有任何元素，表示此边坡中没有任何子边坡对象。 </summary>
            public Dictionary<double, SlopeSegInfo> SlopeInfo { get; private set; }

            /// <summary> key 表示平台的Index。集合中可能没有任何元素，表示此边坡中没有任何子平台对象。 </summary>
            public Dictionary<double, SlopeSegInfo> PlatformInfo { get; private set; }

            #endregion

            /// <summary> 构造函数 </summary>
            /// <param name="station"></param>
            /// <param name="slopeLine">此参数的值可能为 null ，表示此桩号断面上没有对应边坡 </param>
            public SlopeExpands(double station, SlopeLine slopeLine)
            {
                //
                Station = station;
                SlopeInfo = new Dictionary<double, SlopeSegInfo>();
                PlatformInfo = new Dictionary<double, SlopeSegInfo>();
                if (slopeLine != null)
                {
                    XData = slopeLine.XData;
                }
                else
                {
                    XData = new SlopeData(station);
                }
                // ConstructSlopeSegInfo();
                foreach (var sd in XData.Slopes)
                {
                    SlopeInfo.Add(sd.Index, new SlopeSegInfo(0, 0, 0, 0));
                }
                foreach (var sd in XData.Platforms)
                {
                    PlatformInfo.Add(sd.Index, new SlopeSegInfo(0, 0, 0, 0));
                }
            }

            public override string ToString()
            {
                return $"{Station}";
            }
        }

        /// <summary> 每一个子边坡或子平台所占据的桩号区域，以及对应的几何面积 </summary>
        private class SlopeSegInfo
        {
            /// <summary> 后方桩号，即小桩号 </summary>
            public double BackStation { get; set; }
            /// <summary> 前方桩号，即大桩号 </summary>
            public double FrontStation { get; set; }

            public double BackArea { get; set; }
            public double FrontArea { get; set; }
            
            public SlopeSegInfo(double frontStation, double frontArea, double backStation, double backArea)
            {
                FrontStation = frontStation;
                FrontArea = frontArea;
                BackStation = backStation;
                BackArea = backArea;
            }

            public override string ToString()
            {
                return $"桩号({BackStation}~{FrontStation})，左右面积({BackArea},{FrontArea})";
            }
        }

        /// <summary> 在某一断面的边坡中，指定类型的防护方式所点的范围 </summary>
        private enum ProtectionRange
        {
            /// <summary> 边坡中没有任何一个子边坡的防护方式与指定的防护相匹配 </summary>
            None = 0,

            /// <summary> 边坡中所有的子边坡 </summary>
            AllSlopes = 1,

            /// <summary> 边坡中所有的子平台 </summary>
            AllPlatforms = 2,

            /// <summary> 边坡中所有的子边坡与子平台 </summary>
            AllSection = AllSlopes + AllPlatforms,

            /// <summary> 边坡中只有部分边坡或平台应用对应的防护方式 </summary>
            PartialSlopeSegs = 4,
        }

    }
}