using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using eZcad.SubgradeQuantity.Entities;

namespace eZcad.SubgradeQuantity.ParameterForm
{
    public partial class SectionsConstructorForm : Form
    {
        private readonly DocumentModifier _docMdf;
        private readonly IList<Line> _centerLines;
        /// <summary> 成功构造的横断面 </summary>
        public List<SubgradeSection> SectionAxes { get; private set; }

        public SectionsConstructorForm(DocumentModifier docMdf, IList<Line> centerLines)
        {

            InitializeComponent();
            //   //
            _docMdf = docMdf;
            SectionAxes = new List<SubgradeSection>();
            _centerLines = centerLines;
            _count = centerLines.Count;
            // 事件绑定
            bgw_secConstructor.DoWork += ConstructSections_DoWork;
            bgw_secConstructor.ProgressChanged += backgroundWorker1_ProgressChanged;
            bgw_secConstructor.RunWorkerCompleted += backgroundWorker1_RunWorkerCompleted;
        }



        #region ---   线程的启动与取消

        private void SectionsConstructorForm_Load(object sender, EventArgs e)
        {
            StartBackThread();
        }

        private void StartBackThread()
        {
            if (bgw_secConstructor.IsBusy != true)
            {
                //如果在前面的线程还没有结束之前，再次调用“ bgw_secConstructor . RunWorkerAsync(args) ”，则会出现如下报错："此 BackgroundWorker 当前正忙，无法同时运行多个任务。"
                // Start the asynchronous operation.
                bgw_secConstructor.RunWorkerAsync();
            }
        }

        // -----------------------------------------------------------------------------------------------------------
        private void cancelAsyncButton_Click(System.Object sender, System.EventArgs e)
        {
            if (bgw_secConstructor.WorkerSupportsCancellation == true)
            {
                // Cancel the asynchronous operation.
                bgw_secConstructor.CancelAsync();
                // 此方法只是请求取消后台的异步操作，而实际的取消后台线程的操作是通过DoWork事件中的Exit Sub来实现的。
            }
        }

        #endregion


        private readonly int _count;

        #region ---   BackgroundWorker 事件

        // This event handler is where the time-consuming work is done.
        private void ConstructSections_DoWork(System.Object sender, DoWorkEventArgs e)
        {
            BackgroundWorker worker = (BackgroundWorker)sender;
            var errorCenterLine = new List<Line>();
            for (int i = 0; i < _count; i++)
            {
                var axis = _centerLines[i];
                var cenA = SubgradeSection.Create(_docMdf, axis);
                if (cenA != null)
                {
                    cenA.CenterLine.UpgradeOpen();

                    cenA.ClearXData(true);
                    var succ = cenA.CalculateSectionInfoToXData();
                    if (succ)
                    {
                        cenA.FlushXData();
                        SectionAxes.Add(cenA);
                    }
                    cenA.CenterLine.DowngradeOpen();
                }
                else
                {
                    errorCenterLine.Add(axis);
                }
                // 显示进度
                worker.ReportProgress(i);
            }
            // 列出出错的断面
            if (errorCenterLine.Count > 0)
            {
                _docMdf.WriteLineIntoDebuger("提取出错的断面：");
                _docMdf.WriteLineIntoDebuger("序号    起点  终点");
                int index = 0;
                var acPoly = new Polyline();
                foreach (var ecl in errorCenterLine)
                {
                    var pt = new Point2d(ecl.StartPoint.X, ecl.StartPoint.Y);
                    acPoly.AddVertexAt(index, pt, 0, startWidth: 0, endWidth: 0);
                    _docMdf.WriteLineIntoDebuger(index + 1, ecl.StartPoint, ecl.EndPoint);
                    index += 1;
                }
            }
        }
        // -----------------------------------------------------------------------------------------------------------
        // This event handler updates the progress.
        private void backgroundWorker1_ProgressChanged(System.Object sender,
            ProgressChangedEventArgs e)
        {
            label1.Text = e.ProgressPercentage.ToString();// e.ProgressPercentage.ToString();
            var progPercentage = (int)Math.Ceiling(((double)(e.ProgressPercentage + 1) / _count) * 100);
            progressBar1.Value = progPercentage;
        }

        // -----------------------------------------------------------------------------------------------------------
        // This event handler deals with the results of the background operation.
        private void backgroundWorker1_RunWorkerCompleted(System.Object sender,
            RunWorkerCompletedEventArgs e)
        {
            var s = $"提取结束，选择{_count}条轴线，创建{SectionAxes.Count}个横断面；";
            if (SectionAxes.Count < _count)
            {
                s += "\r\n请确保在界面中显示出所有的横断面，并尽可能将图形放大";
            }
            label1.Dock = DockStyle.Fill;
            label1.Text = s;
            label2.Text = "";
            //if (e.Cancelled == true)
            //{
            //    resultLabel.Text = "Canceled!";
            //}
            //else if (e.Error != null)
            //{
            //    resultLabel.Text = "Error: " + e.Error.Message;
            //}
            //else
            //{
            //    resultLabel.Text = "Done!";
            //}
        }
        #endregion


    }
}
