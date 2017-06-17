using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Forms;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Runtime;
using eZcad.AddinManager;
using eZcad.Addins.SlopeProtection;
using eZcad.Utility;

[assembly: CommandClass(typeof(SlopeWalker))]

namespace eZcad.Addins.SlopeProtection
{
    [EcDescription("边坡信息漫游")]
    public class SlopeWalker
    {
        #region --- 命令设计

        /// <summary> 提取所有的横断面块参照的信息 </summary>
        [CommandMethod(eZConstants.eZGroupCommnad, "SlopeWalk", CommandFlags.UsePickSet)
            , DisplayName(@"边坡信息漫游"), Description("查看指定的边坡对象的信息")]
        public void EcSlopeWalk()
        {
            DocumentModifier.ExecuteCommand(SlopeWalk);
        }

        /// <summary> 提取所有的横断面块参照的信息 </summary>
        public void SlopeWalk(DocumentModifier docMdf, SelectionSet impliedSelection)
        {
            ResultBuffer xd;
            bool cont;
            var pl = GetSlopeLine(docMdf.acEditor, out xd, out cont);

            while (cont)
            {
                if (pl != null)
                {
                    //
                    var slpData = SlopeData.FromResultBuffer(xd);

                    var formAddDefinition = new SlopeDataEditor(slpData);
                    //
                    var res = formAddDefinition.ShowDialog();
                    if (res == DialogResult.OK)
                    {
                        // var newSlpDa = formAddDefinition.Instance;
                        pl.UpgradeOpen();
                        pl.XData = slpData.ToResultBuffer();
                        pl.DowngradeOpen();
                    }
                }
                //
                pl = GetSlopeLine(docMdf.acEditor, out xd, out cont);
            }
        }

        #endregion

        private Polyline GetSlopeLine(Editor ed, out ResultBuffer xd, out bool cont)
        {
            //// 创建一个 TypedValue 数组，用于定义过滤条件
            //var acTypValAr = SlopeLine.SlopeLineFilter;
            //var lst = new List<TypedValue>(acTypValAr)
            //{
            //    new TypedValue((int) DxfCode.ExtendedDataRegAppName, SlopeData.AppName)
            //};
            //acTypValAr = lst.ToArray();
            //// 将过滤条件赋值给SelectionFilter对象
            //var acSelFtr = new SelectionFilter(acTypValAr);
            var op = new PromptEntityOptions("\n选择一条边坡线以进行信息读写");
            op.SetRejectMessage($"\n选择的多段线必须包含{SlopeData.AppName}的外部扩展数据");
            op.AddAllowedClass(typeof(Polyline), exactMatch: true);
            var res = ed.GetEntity(op);

            cont = true;
            if (res.Status == PromptStatus.OK)
            {
                var pl = res.ObjectId.GetObject(OpenMode.ForRead) as Polyline;
                if (pl != null)
                {
                    xd = pl.GetXDataForApplication(SlopeData.AppName);
                    if (xd != null)
                    {
                        cont = true;
                        return pl;
                    }
                }
            }
            else if (res.Status == PromptStatus.Cancel)
            {
                cont = false;
            }
            xd = null;
            return null;
        }
    }
}