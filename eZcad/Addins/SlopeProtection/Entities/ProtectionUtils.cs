using System;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Windows;
using Application = Microsoft.Office.Interop.Excel.Application;

namespace eZcad.Addins.SlopeProtection
{
    /// <summary> 与边坡防护相关的一些通用性的操作 </summary>
    public static partial class ProtectionUtils
    {
        #region --- Excel 程序

        private static Application _workingApp;

        /// <summary> 获取全局的 Excel 程序 </summary>
        /// <param name="visible"></param>
        /// <returns>获取失败则返回 null</returns>
        public static Application GetExcelApp(bool visible = false)
        {
            if (_workingApp != null)
            {
                int processId = 0;
                var threadId = eZstd.API.Windows.GetWindowThreadProcessId(_workingApp.Hwnd, ref processId);
                var pr = Process.GetProcessById(processId);
                if (pr == null || pr.HasExited)
                {
                    _workingApp = null;
                }
                else
                {
                    _workingApp.Visible = visible;
                    return _workingApp;
                }
            }
            if (_workingApp == null)
            {
                _workingApp = new Application() { Visible = visible };
            }
            if (_workingApp == null)
            {
                throw new NullReferenceException($"无法打开 Excel 程序!");
            }
            return _workingApp;
        }

        /// <returns>成功则返回 true</returns>
        public static bool KillActiveExcelApp(Application appToKill)
        {
            if (appToKill != null)
            {
                try
                {
                    // excelApp.Quit();
                    int processId = 0;
                    var threadId = eZstd.API.Windows.GetWindowThreadProcessId(appToKill.Hwnd, ref processId);
                    var pr = Process.GetProcessById(processId);
                    pr.Kill();
                    //
                    if (appToKill.Equals(_workingApp))
                    {
                        _workingApp = null;
                    }
                }
                catch (Exception)
                {
                    return false;
                }

            }
            return true;
        }

        #endregion

        #region --- 桩号处理

        private static readonly Regex MileageReg = new Regex(@"K(\d+)\+(\d*.*)"); // K51+223.392

        /// <summary> 将表示里程的字符数据转换为对应的数值 </summary>
        /// <param name="mileage"></param>
        /// <returns>如果无法正常解析，则返回 null</returns>
        public static double? GetMileageFromString(string mileage)
        {
            var mt = MileageReg.Match(mileage);
            if (mt.Success)
            {
                int k;
                double m;
                if (int.TryParse(mt.Groups[1].Value, out k) && double.TryParse(mt.Groups[2].Value, out m))
                {
                    return k * 1000 + m;
                }
            }
            return null;
        }

        #endregion
    }
}