using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using eZcad.SubgradeQuantity.Entities;
using eZcad.SubgradeQuantity.SlopeProtection;

namespace eZcad.SubgradeQuantity.ParameterForm
{
    /// <summary> 对AutoCAD图形中与边坡防护相关的文字进行修改 </summary>
    public partial class PF_ModifyProtectionText : ModalPForm
    {
        private SelectionSet _impliedSelection;

        /// <summary> 数据库中所有的边坡防护图层。键表示 防护形式，值代表对应的 图层名，二者是一一对应的，值比键多个了字符前缀 </summary>
        private Dictionary<string, string> _protLayers;

        private List<string> _wantedProtLayer;

        #region ---   窗口的构造、打开与关闭

        private static PF_ModifyProtectionText _uniqueInstance;

        public static PF_ModifyProtectionText GetUniqueInstance(DocumentModifier docMdf, SelectionSet impliedSelection)
        {
            _uniqueInstance = _uniqueInstance ?? new PF_ModifyProtectionText();
            _uniqueInstance._docMdf = docMdf;
            _uniqueInstance._impliedSelection = impliedSelection;
            //
            return _uniqueInstance;
        }

        /// <summary> 构造函数 </summary>
        private PF_ModifyProtectionText() : base()
        {
            InitializeComponent();
            //
        }


        private void PF_ModifyProtText_Load(object sender, EventArgs e)
        {
            _protLayers = ProtectionTags.GetProtLayersInDatabase(_docMdf.acDataBase);
            _protLayers.Add(@"所有", "");
            //
            SetProtectionLister(protLayerLister1);
        }

        #endregion

        #region ---   界面操作

        #endregion

        #region ---   ProtectionLister 设置

        private void SetProtectionLister(ItemLister plister)
        {
            plister.ItemRaised += ProtectionLister1OnProtectionMethodRaised;
            plister.ItemDetermined += ProtectionLister1OnProtectionMethodDetermined;
            //
            plister.ImportItems(_protLayers.Keys.ToArray(), _protLayers.Values.ToArray());
        }

        private void ProtectionLister1OnProtectionMethodDetermined(Control label, string s)
        {
            _wantedProtLayer = new List<string>();
            if (!string.IsNullOrEmpty(s))
            {
                _wantedProtLayer.Add(s);
            }
            //
            GetParameterAndRun();
        }

        private void ProtectionLister1OnProtectionMethodRaised(Control label, string s)
        {
            _wantedProtLayer = new List<string>();
            if (!string.IsNullOrEmpty(s))
            {
                _wantedProtLayer.Add(s);
            }
        }

        #endregion

        protected override void OnCmdRun(bool closeWindow)
        {
            base.OnCmdRun(closeWindowWhenFinished: true);
        }

        private void GetParameterAndRun()
        {
            if (radioButton_highLight.Checked)
            {
                // 高亮所有满足条件的文字
                var txts = SelectAllProtTexts();
                txts = FilterProtTexts(txts);
                _docMdf.WriteNow($"选择的防护文字数量：{txts.Count}");
                //if (txts.Count == 0) return;
                //
                HighlightTxts(txts);
            }
            else if (radioButton_Delete.Checked)
            {
                // 删除选择文字中满足条件的文字
                var txts = SelectProtTexts();
                txts = FilterProtTexts(txts);
                _docMdf.WriteNow($"选择的防护文字数量：{txts.Count}");
                if (txts.Count == 0) return;
                //
                foreach (var txt in txts)
                {
                    txt.UpgradeOpen();
                    txt.Erase();
                    txt.Draw();
                    txt.DowngradeOpen();
                }
            }
            //
            _docMdf.acEditor.UpdateScreen();
        }

        #region ---   从界面中选择文字

        private List<DBText> SelectProtTexts()
        {
            var filter = ProtectionTags.GetProtTextFilter(_protLayers.Values.ToArray(), _wantedProtLayer);
            var selected = new List<DBText>();
            //
            var res = _docMdf.acEditor.GetSelection(new SelectionFilter(filter));
            if (res.Status == PromptStatus.OK)
            {
                foreach (var id in res.Value.GetObjectIds())
                {
                    selected.Add(id.GetObject(OpenMode.ForRead) as DBText);
                }
            }
            return selected;
        }

        private List<DBText> SelectAllProtTexts()
        {
            var filter = ProtectionTags.GetProtTextFilter(_protLayers.Values.ToArray(), _wantedProtLayer);
            var selected = new List<DBText>();
            //
            var res = _docMdf.acEditor.SelectAll(new SelectionFilter(filter));
            if (res.Status == PromptStatus.OK)
            {
                foreach (var id in res.Value.GetObjectIds())
                {
                    selected.Add(id.GetObject(OpenMode.ForRead) as DBText);
                }
            }
            return selected;
        }

        //
        private List<DBText> FilterProtTexts(List<DBText> SelectedTexts)
        {
            // 边坡还是平台
            bool? slopePlatform = null;
            if (radioButton_Slope.Checked)
            {
                slopePlatform = true;
            }
            else if (radioButton_platform.Checked)
            {
                slopePlatform = false;
            }

            // 左右侧
            bool? left = null;
            if (radioButton_left.Checked)
            {
                left = true;
            }
            else if (radioButton_right.Checked)
            {
                left = false;
            }

            var filtered = new List<DBText>();
            // 过滤
            foreach (var txt in SelectedTexts)
            {
                var buff = txt.GetXDataForApplication(ProtTextData.RegAppName_SlopeText);
                if (buff != null)
                {
                    var protData = ProtTextData.FromResultBuffer(buff);
                    if (protData != null)
                    {
                        // 比较过滤条件
                        if (slopePlatform != null && slopePlatform.Value != protData.SlopePlatform)
                        {
                            continue;
                        }
                        if (left != null && left.Value != protData.Left)
                        {
                            continue;
                        }

                        // 满足所有过滤条件
                        filtered.Add(txt);
                    }
                }
            }

            return filtered;
        }
        #endregion

        private List<ObjectId> _highlightedTexts = new List<ObjectId>();

        private void HighlightTxts(List<DBText> txts, bool unHighlightOthers = true)
        {
            var highlightedTexts = new List<ObjectId>();
            if (!unHighlightOthers)
            {
                highlightedTexts = _highlightedTexts;
            }
            _highlightedTexts = highlightedTexts;
            //
            foreach (var txt in txts)
            {
                if (!highlightedTexts.Contains(txt.Id))
                {
                    highlightedTexts.Add(txt.Id);
                }
            }
            //
            _docMdf.acEditor.SetImpliedSelection(highlightedTexts.ToArray());
        }
    }
}