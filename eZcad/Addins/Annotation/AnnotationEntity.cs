using System.Collections.Generic;
using System.Linq;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Runtime;
using eZcad.Addins;
using eZcad.Utility;

[assembly: CommandClass(typeof (Ec_EntityAnnotationEditor))]

namespace eZcad.Addins
{
    /// <summary> 对界面中的元素的注释信息进行操作 </summary>
    public class AnnotationEntity
    {
        /// <summary> 元素注释信息对应的定义名 </summary>
        public const string Appname_ElementAnnotation = "msdi_EntityAnnotation"; //"msdi_EntityAnnotation";

        private readonly Entity _underlyingEntity;

        /// <summary> 元素中对应的注释信息，其中不包括 ResultBuffer 中第1个的 ExtendedDataRegAppName </summary>
        public List<string> Annotations { get; private set; }

        /// <summary> 构造函数 </summary>
        public AnnotationEntity(Entity underlyingEntity)
        {
            _underlyingEntity = underlyingEntity;
            Annotations = new List<string>();
        }

        /// <summary> 从元素中提取注释信息 </summary>
        /// <returns></returns>
        public List<string> ExtractAnnotsFromXdata()
        {
            Annotations = GetAnnotsFromXdata(_underlyingEntity, Appname_ElementAnnotation);
            return Annotations;
        }

        /// <summary> 从元素中提取注释信息 </summary>
        /// <returns></returns>
        public void SetAnnotsToXdata(string annot)
        {
            Annotations = new List<string>() {annot};
            SetAnnotsToXdata(_underlyingEntity, Appname_ElementAnnotation, Annotations);
        }

        /// <summary> 从元素中提取注释信息 </summary>
        /// <returns></returns>
        public void SetAnnotsToXdata(List<string> annotList)
        {
            Annotations = annotList;
            SetAnnotsToXdata(_underlyingEntity, Appname_ElementAnnotation, Annotations);
        }

        /// <summary> 在 Editor 中显示出 注释信息 </summary>
        public void WriteAnnotationsOnEditor(DocumentModifier docMdf)
        {
            if (Annotations.Count != 0)
            {
                docMdf.WriteNow("\n" + _underlyingEntity.Handle + " 的注释信息：");
                object[] ann = Annotations.Select(r => (object) r).ToArray();
                docMdf.WriteLinesNow(ann);
            }
            else
            {
                // 集合中没有元素
                docMdf.WriteNow(_underlyingEntity.Handle + " 中没有注释信息");
            }
        }

        /// <summary> 清除注释信息 </summary>
        public void ClearAnnotations()
        {
            ClearAnnotations(_underlyingEntity, Appname_ElementAnnotation);
        }

        /// <summary> 在进行XData数据的读写之前，必须先定义好 AppName </summary>
        public static void SetAppName(DocumentModifier docMdf)
        {
            var app = SymbolTableUtils.GetOrCreateAppName(docMdf.acDataBase, docMdf.acTransaction,
                Appname_ElementAnnotation);
        }

        /// <summary> 从元素中提取注释信息 </summary>
        /// <param name="ent"></param>
        /// <param name="appName"></param>
        /// <returns></returns>
        private static List<string> GetAnnotsFromXdata(Entity ent, string appName)
        {
            var annots = new List<string>();
            var buff = ent.GetXDataForApplication(appName);
            if (buff == null) return annots;

            var buffs = buff.AsArray();
            if (buffs.Length == 0) return annots;
            // var appName = buffs[0].Value.ToString();
            for (int i = 1; i < buffs.Length; i++)
            {
                annots.Add(buffs[i].Value as string);
            }
            return annots;
        }

        /// <summary> 从元素中提取注释信息 </summary>
        /// <param name="ent"></param>
        /// <param name="appName"></param>
        /// <param name="annotations"></param>
        /// <returns></returns>
        private static void SetAnnotsToXdata(Entity ent, string appName, List<string> annotations)
        {
            ent.UpgradeOpen();
            var typedValues = new Queue<TypedValue>();
            typedValues.Enqueue(new TypedValue((int) DxfCode.ExtendedDataRegAppName, appName));
            for (int i = 0; i < annotations.Count; i++)
            {
                typedValues.Enqueue(new TypedValue((int) DxfCode.ExtendedDataAsciiString, annotations[i]));
            }
            //
            ent.XData = new ResultBuffer(typedValues.ToArray());
            ent.DowngradeOpen();
        }

        /// <summary> 清除元素中 appName 下对应的注释信息 </summary>
        public static void ClearAnnotations(Entity ent, string appName)
        {
            ent.UpgradeOpen();
            var buff = new ResultBuffer(new TypedValue((int) DxfCode.ExtendedDataRegAppName, appName));
            ent.XData = buff;
            ent.DowngradeOpen();
        }
    }
}