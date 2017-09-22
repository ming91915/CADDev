using System.Collections.Generic;
using System.Linq;
using Autodesk.AutoCAD.DatabaseServices;
using eZcad.SubgradeQuantity.Entities;
using eZcad.SubgradeQuantity.Utility;
using eZcad.Utility;

namespace eZcad.SubgradeQuantity.SlopeProtection
{
    /// <summary> 与每一种边坡防护形式的具体数据相关的标识 </summary>
    public class ProtectionTags
    {

        #region --- 提取 防护方式集合（没有重复项）

        /// <summary> 提取指定边坡集合中的所有的防护形式（挂网喷锚_6 与 挂网喷锚_9 并不会合并） </summary>
        public static string[] GetProtectionTypes(IEnumerable<SlopeData> slopeDatas)
        {
            return slopeDatas
                .SelectMany(r => SlopeData.Combine(r.Slopes, r.Platforms, false))
                .Select(r => r.ProtectionMethod)
                .Where(r => !string.IsNullOrEmpty(r))
                .Distinct()
                .ToArray();
        }
        /// <summary> 提取指定边坡集合中的所有的防护形式（挂网喷锚_6 与 挂网喷锚_9 并不会合并） </summary>
        public static string[] GetProtectionTypes(IEnumerable<SlopeLine> slopelines)
        {
            return GetProtectionTypes(slopelines.Select(r => r.XData));
        }

        /// <summary> 将所有相互不同的防护方式进行分类。返回的字典中，Key代表某个基本防护方式，其对应的值代表基本类型下的不同规格 </summary>
        /// <param name="allProtMethods">集合中没有相同的元素，但是有类似的，比如“挂网喷锚_6”与“挂网喷锚_8”</param>
        /// <returns></returns>
        public static Dictionary<string, List<string>> CategorizeProtectionMethods(string[] allProtMethods)
        {
            var baseProts = new Dictionary<string, List<string>>();
            string baseProt;
            foreach (var pt in allProtMethods)
            {
                var i = pt.IndexOf(ProtectionConstants.ProtectionMethodStyleSeperator);
                if (i >= 0) // 说明是 “挂网喷锚_6” 的形式
                {
                    baseProt = pt.Substring(0, i);
                    if (!baseProts.Keys.Contains(baseProt))
                    {
                        var bps = new List<string>() { pt };
                        baseProts.Add(baseProt, bps);
                    }
                    else
                    {
                        baseProts[baseProt].Add(pt);
                    }
                }
                else // 说明是 “挂网喷锚” 的形式
                {
                    if (!baseProts.Keys.Contains(pt))
                    {
                        var bps = new List<string>() { pt };
                        baseProts.Add(pt, bps);
                    }
                    else
                    {
                        baseProts[pt].Add(pt);
                    }
                }
            }
            return baseProts;
        }

        #endregion

        #region --- 防护方式 所对应的图层

        /// <summary> 提取每一种边坡防护形式所对应的图层 </summary>
        public static Dictionary<string, ObjectId> MapProtectionLayers(DocumentModifier docMdf, IEnumerable<SlopeLine> slopes)
        {
            // 
            Utils.GetOrCreateAppName(docMdf.acDataBase, docMdf.acTransaction, ProtTextData.RegAppName_SlopeText);
            // Utils.GetOrCreateAppName(docMdf.acDataBase, docMdf.acTransaction, ProtTextData.RegAppName_Platform);
            //
            var protectionTypes = GetProtectionTypes(slopes);
            var categorizedProtMtd = CategorizeProtectionMethods(protectionTypes);
            //
            var protLayers = new Dictionary<string, ObjectId>();
            foreach (var protName in categorizedProtMtd.Keys)
            {
                var layer = Utils.GetOrCreateLayer(docMdf, GetLayerNameFromProtection(protName));
                // 设置图层线宽
                if (layer.LineWeight != LineWeight.LineWeight050)
                {
                    layer.UpgradeOpen();
                    layer.LineWeight = LineWeight.LineWeight050;
                    layer.DowngradeOpen();
                }
                foreach (var prot in categorizedProtMtd[protName])
                {
                    protLayers.Add(prot, layer.Id);
                }
            }
            return protLayers;
        }

        private static string GetLayerNameFromProtection(string protectionName)
        {
            return ProtectionConstants.SubgradeQuantityTag + "_P_" + protectionName;
        }

        /// <summary> 提取数据库中所有的边坡防护图层。键表示 防护形式，值代表对应的 图层名，二者是一一对应的，值比键多个了字符前缀  </summary>
        /// <param name="db"></param>
        /// <returns> 键表示 防护形式，值代表对应的 图层名，二者是一一对应的，值比键多个了字符前缀 </returns>
        public static Dictionary<string, string> GetProtLayersInDatabase(Database db)
        {
            LayerTable layers = db.LayerTableId.GetObject(OpenMode.ForRead) as LayerTable;
            var protLayers = new Dictionary<string, string>();
            const string prefix = ProtectionConstants.SubgradeQuantityTag + "_P_";
            foreach (var layerId in layers)
            {
                var layerName = (layerId.GetObject(OpenMode.ForRead) as LayerTableRecord).Name;
                if (layerName.StartsWith(prefix))
                {
                    var protName = layerName.Substring(prefix.Length);
                    protLayers.Add(protName, layerName);
                }
            }
            return protLayers;
        }

        #endregion



        /// <summary> 根据指定的条件设置选择过滤参数 </summary>
        /// <param name="layersName"></param>
        /// <param name="allProtLayers"></param>
        /// <returns></returns>
        public static TypedValue[] GetProtTextFilter(IList<string> allProtLayers, IList<string> layersName)
        {
            var filterTypes = new List<TypedValue>();
            filterTypes.Add(new TypedValue((int)DxfCode.Start, "TEXT"));
            filterTypes.Add(new TypedValue((int)DxfCode.ExtendedDataRegAppName, ProtTextData.RegAppName_SlopeText));

            // 哪些图层：指定了就用指定的，没指定的就用所有的
            var wantedLayers = (layersName == null || layersName.Count == 0) ? allProtLayers : layersName;
            filterTypes.Add(new TypedValue((int)DxfCode.Operator, "<OR"));
            foreach (var l in wantedLayers)
            {
                filterTypes.Add(new TypedValue((int)DxfCode.LayerName, l));
            }
            filterTypes.Add(new TypedValue((int)DxfCode.Operator, "OR>"));

            //

            return filterTypes.ToArray();
        }
    }
}
