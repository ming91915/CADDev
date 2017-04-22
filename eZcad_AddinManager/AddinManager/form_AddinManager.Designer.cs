namespace eZcad.AddinManager
{
    partial class form_AddinManager
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(form_AddinManager));
            this.treeView1 = new System.Windows.Forms.TreeView();
            this.buttonLoad = new System.Windows.Forms.Button();
            this.buttonRemove = new System.Windows.Forms.Button();
            this.buttonRun = new System.Windows.Forms.Button();
            this.label_Description = new System.Windows.Forms.Label();
            this.button_Reload = new System.Windows.Forms.Button();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.SuspendLayout();
            // 
            // treeView1
            // 
            this.treeView1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.treeView1.Font = new System.Drawing.Font("SimSun", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.treeView1.Indent = 19;
            this.treeView1.ItemHeight = 18;
            this.treeView1.Location = new System.Drawing.Point(17, 16);
            this.treeView1.Margin = new System.Windows.Forms.Padding(4);
            this.treeView1.Name = "treeView1";
            this.treeView1.Size = new System.Drawing.Size(424, 216);
            this.treeView1.TabIndex = 0;
            this.treeView1.NodeMouseClick += new System.Windows.Forms.TreeNodeMouseClickEventHandler(this.ShowExCommandDescription);
            this.treeView1.NodeMouseDoubleClick += new System.Windows.Forms.TreeNodeMouseClickEventHandler(this.treeView1_NodeMouseDoubleClick);
            // 
            // buttonLoad
            // 
            this.buttonLoad.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.buttonLoad.Location = new System.Drawing.Point(125, 282);
            this.buttonLoad.Margin = new System.Windows.Forms.Padding(4);
            this.buttonLoad.Name = "buttonLoad";
            this.buttonLoad.Size = new System.Drawing.Size(100, 29);
            this.buttonLoad.TabIndex = 1;
            this.buttonLoad.Text = "Load";
            this.buttonLoad.UseVisualStyleBackColor = true;
            this.buttonLoad.Click += new System.EventHandler(this.buttonLoad_Click);
            // 
            // buttonRemove
            // 
            this.buttonRemove.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.buttonRemove.Location = new System.Drawing.Point(341, 282);
            this.buttonRemove.Margin = new System.Windows.Forms.Padding(4);
            this.buttonRemove.Name = "buttonRemove";
            this.buttonRemove.Size = new System.Drawing.Size(100, 29);
            this.buttonRemove.TabIndex = 1;
            this.buttonRemove.Text = "Remove";
            this.toolTip1.SetToolTip(this.buttonRemove, "移除指定的方法或者程序集");
            this.buttonRemove.UseVisualStyleBackColor = true;
            this.buttonRemove.Click += new System.EventHandler(this.buttonRemove_Click);
            // 
            // buttonRun
            // 
            this.buttonRun.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.buttonRun.Location = new System.Drawing.Point(16, 282);
            this.buttonRun.Margin = new System.Windows.Forms.Padding(4);
            this.buttonRun.Name = "buttonRun";
            this.buttonRun.Size = new System.Drawing.Size(100, 29);
            this.buttonRun.TabIndex = 1;
            this.buttonRun.Text = "Run";
            this.buttonRun.UseVisualStyleBackColor = true;
            this.buttonRun.Click += new System.EventHandler(this.buttonRun_Click);
            // 
            // label_Description
            // 
            this.label_Description.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label_Description.AutoSize = true;
            this.label_Description.ForeColor = System.Drawing.Color.Gray;
            this.label_Description.Location = new System.Drawing.Point(16, 250);
            this.label_Description.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label_Description.Name = "label_Description";
            this.label_Description.Size = new System.Drawing.Size(52, 15);
            this.label_Description.TabIndex = 2;
            this.label_Description.Text = "描述：";
            // 
            // button_Reload
            // 
            this.button_Reload.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.button_Reload.Location = new System.Drawing.Point(233, 282);
            this.button_Reload.Margin = new System.Windows.Forms.Padding(4);
            this.button_Reload.Name = "button_Reload";
            this.button_Reload.Size = new System.Drawing.Size(100, 29);
            this.button_Reload.TabIndex = 3;
            this.button_Reload.Text = "Reload";
            this.toolTip1.SetToolTip(this.button_Reload, "重新加载指定方法所对应的程序集");
            this.button_Reload.UseVisualStyleBackColor = true;
            this.button_Reload.Click += new System.EventHandler(this.button_Reload_Click);
            // 
            // form_AddinManager
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(459, 326);
            this.Controls.Add(this.button_Reload);
            this.Controls.Add(this.label_Description);
            this.Controls.Add(this.buttonRun);
            this.Controls.Add(this.buttonRemove);
            this.Controls.Add(this.buttonLoad);
            this.Controls.Add(this.treeView1);
            this.HelpButton = true;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(4);
            this.MinimumSize = new System.Drawing.Size(474, 363);
            this.Name = "form_AddinManager";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Add-In Manager";
            this.HelpButtonClicked += new System.ComponentModel.CancelEventHandler(this.form_AddinManager_HelpButtonClicked);
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.form_AddinManager_FormClosing);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.form_AddinManager_KeyDown);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TreeView treeView1;
        private System.Windows.Forms.Button buttonLoad;
        private System.Windows.Forms.Button buttonRemove;
        private System.Windows.Forms.Button buttonRun;
        private System.Windows.Forms.Label label_Description;
        private System.Windows.Forms.Button button_Reload;
        private System.Windows.Forms.ToolTip toolTip1;
    }
}