using System;
using Autodesk.AutoCAD.ApplicationServices.Core;
using Autodesk.AutoCAD.DatabaseServices;

namespace eZcad.Utility
{
    /// <summary> 将多个块的属性值进行统一编辑 </summary>
    public static class LayoutUtil
    {
        /// <summary> 创建一个新的布局 </summary>
        public static ObjectId CreateLayout(string layoutName)
        {
            var lm = LayoutManager.Current;
            // var layoutName = "NewLayout" + DateTime.Now.Minute + DateTime.Now.Second;
            var id = lm.GetLayoutId(layoutName);

            // If it doesn't exist, we create it
            if (!id.IsValid)
            {
                id = lm.CreateLayout(layoutName);
            }
            if (!id.IsValid)
            {
                throw new InvalidOperationException("创建布局失败！");
            }
            return id;
        }


        /// <summary> 切换到模型空间 </summary>
        public static void SwitchLayout()
        {
            Application.SetSystemVariable("TILEMODE", 1);
            // 或者是 LayoutManager.Current.CurrentLayout = "Model";
        }

        /// <summary> 切换到图纸空间 </summary>
        /// <param name="layoutId">除模型空间之外的 Layout 的Id</param>
        public static void SwitchLayout(ObjectId layoutId)
        {
            LayoutManager.Current.SetCurrentLayoutId(layoutId);
        }

        /// <summary> 切换到图纸空间 </summary>
        /// <param name="layoutName">除模型空间之外的 Layout 的名称，其值不可能为 Model</param>
        public static void SwitchLayout(string layoutName)
        {
            LayoutManager.Current.CurrentLayout = layoutName;
        }


        /// <summary> Apply plot settings to the provided layout. </summary>
        /// <param name="layout">要进行设置的布局</param>
        /// <param name="pageSize">The canonical media name for our page size. 比如 "ANSI_B_(11.00_x_17.00_Inches)"</param>
        /// <param name="styleSheet">The pen settings file (ctb or stb). 比如 "monochrome.ctb"</param>
        /// <param name="device">The name of the output device. 比如 "DWF6 ePlot.pc3"</param>
        public static void SetPlotSettings(this Layout layout, string pageSize, string styleSheet, string device)
        {
            using (var ps = new PlotSettings(layout.ModelType))
            {
                ps.CopyFrom(layout);
                var psv = PlotSettingsValidator.Current;

                // Set the device
                var devs = psv.GetPlotDeviceList();
                if (devs.Contains(device))
                {
                    psv.SetPlotConfigurationName(ps, device, null);
                    psv.RefreshLists(ps);
                }

                // Set the media name/size
                var mns = psv.GetCanonicalMediaNameList(ps);
                if (mns.Contains(pageSize))
                {
                    psv.SetCanonicalMediaName(ps, pageSize);
                }

                // Set the pen settings
                var ssl = psv.GetPlotStyleSheetList();
                if (ssl.Contains(styleSheet))
                {
                    psv.SetCurrentStyleSheet(ps, styleSheet);
                }

                // Copy the PlotSettings data back to the Layout，即将打印设置应用到布局
                var upgraded = false;
                if (!layout.IsWriteEnabled)
                {
                    layout.UpgradeOpen();
                    upgraded = true;
                }

                layout.CopyFrom(ps);
                if (upgraded)
                {
                    layout.DowngradeOpen();
                }
            }
        }

    }
}