using System;
using System.Linq;
using System.Windows.Forms;

namespace eZcad.Addins.Utilities
{
    /// <summary>
    /// 提供一些基础性的操作工具
    /// </summary>
    /// <remarks></remarks>
    public static class Utils
    {
        /// <summary>
        /// 返回Nullable所对应的泛型。如果不是Nullable泛型，则返回null。
        /// </summary>
        /// <param name="typeIn"></param>
        /// <returns></returns>
        public static Type GetNullableGenericArgurment(Type typeIn)
        {
            // We need to check whether the property is NULLABLE
            if (typeIn.IsGenericType && typeIn.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                // If it is NULLABLE, then get the underlying type. eg if "Nullable<int>" then this will return just "int"
                return typeIn.GetGenericArguments()[0];
            }
            else
            {
                return null;
            }
        }
        
        /// <summary> 指定的字符串中是否包含有非英文字符 </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static bool StringHasNonEnglish(string str)
        {
            // 1、用ASCII码判断：在 ASCII码表中，英文的范围是0 - 127，而汉字则是大于127。
            return str.Any(t => (int)t > 127);
        }

        #region ---   文件的打开与保存
        /// <summary> 选择一个或多个要打开的文件。成功选择，则返回对应文件的绝对路径，否则返回 null </summary>
        /// <param name="title">对话框的标题</param>
        /// <param name="filter"> 文件过滤规则，比如 
        /// “材料库(*.txt)| *.txt”、
        /// “Excel文件(*.xls; *.xlsx; *.xlsb)| *.xls; *.xlsx; *.xlsb”、
        /// “Excel工作簿(*.xlsx)|*.xlsx| Excel二进制工作簿(*.xlsb) |*.xlsb| Excel 97-2003 工作簿(*.xls)|*.xls” </param>
        /// <param name="multiselect"> 是否支持多选 </param>
        /// <returns> 成功选择，则返回对应文件的绝对路径，如果没有选择任何文件，则返回 null </returns>
        public static string[] ChooseOpenFile(string title, string filter, bool multiselect)
        {
            OpenFileDialog ofd = new OpenFileDialog
            {
                Title = title,
                CheckFileExists = true,
                AddExtension = true,
                Filter = filter,
                FilterIndex = 0,
                Multiselect = multiselect,
            };

            if (ofd.ShowDialog() == DialogResult.OK)
            {
                if (ofd.FileNames.Length > 0)
                {
                    return ofd.FileNames;
                }
                else
                {
                    return null;
                }
            }
            return null;
        }

        /// <summary> 选择要将数据保存到哪个文件。成功选择，则返回对应文件的绝对路径，否则返回 null </summary>
        /// <param name="title">对话框的标题</param>
        /// <param name="filter"> 文件过滤规则，比如 
        /// “材料库(*.txt)| *.txt”、
        /// “Excel文件(*.xls; *.xlsx; *.xlsb)| *.xls; *.xlsx; *.xlsb”、
        /// “Excel工作簿(*.xlsx)|*.xlsx| Excel二进制工作簿(*.xlsb) |*.xlsb| Excel 97-2003 工作簿(*.xls)|*.xls” </param>
        /// <returns> 成功选择，则返回对应文件的绝对路径，否则返回 null </returns>
        public static string ChooseSaveFile(string title, string filter)
        {
            var ofd = new SaveFileDialog()
            {
                Title = title,
                // CheckFileExists = true, // 文件不存在则不能作为有效路径
                //  CheckPathExists = true,
                AddExtension = true,
                Filter = filter,
                FilterIndex = 0,
            };

            if (ofd.ShowDialog() == DialogResult.OK)
            {
                return ofd.FileName.Length > 0 ? ofd.FileName : null;
            }
            return null;
        }
        #endregion
    }
}