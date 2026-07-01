using System;
using System.Windows.Forms;

namespace Zitn_exe_App.Forms
{
    partial class IndexForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(IndexForm));
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.toolStripMenuItem3 = new System.Windows.Forms.ToolStripMenuItem();
            this.tsmi_history = new System.Windows.Forms.ToolStripMenuItem();
            this.tsmi_unaudit = new System.Windows.Forms.ToolStripMenuItem();
            this.tsmi_unaudit_gys = new System.Windows.Forms.ToolStripMenuItem();
            this.tsmi_unaudit_all = new System.Windows.Forms.ToolStripMenuItem();
            this.tsmi_history_purunaudit_info = new System.Windows.Forms.ToolStripMenuItem();
            this.dgvMain = new System.Windows.Forms.DataGridView();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.toolStripStatusLabel1 = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.btn_ins = new System.Windows.Forms.ToolStripButton();
            this.btn_del = new System.Windows.Forms.ToolStripButton();
            this.btn_clear = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripButton2 = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripButNext = new System.Windows.Forms.ToolStripButton();
            this.toolStripButPre = new System.Windows.Forms.ToolStripButton();
            this.Source = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Supplier = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.MaterialId = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.MaterialName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.MaterialSpec = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.up = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Down = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.price = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.unit = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.qty = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.GuidSN = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.menuStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvMain)).BeginInit();
            this.statusStrip1.SuspendLayout();
            this.toolStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // menuStrip1
            // 
            this.menuStrip1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItem3,
            this.tsmi_history,
            this.tsmi_unaudit,
            this.tsmi_history_purunaudit_info});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(967, 25);
            this.menuStrip1.TabIndex = 0;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // toolStripMenuItem3
            // 
            this.toolStripMenuItem3.Name = "toolStripMenuItem3";
            this.toolStripMenuItem3.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Alt | System.Windows.Forms.Keys.Q)));
            this.toolStripMenuItem3.Size = new System.Drawing.Size(62, 21);
            this.toolStripMenuItem3.Text = "退出(Q)";
            this.toolStripMenuItem3.Click += new System.EventHandler(this.toolStripMenuItem3_Click);
            // 
            // tsmi_history
            // 
            this.tsmi_history.Name = "tsmi_history";
            this.tsmi_history.Size = new System.Drawing.Size(128, 21);
            this.tsmi_history.Text = "供应商历史采购情况";
            this.tsmi_history.Click += new System.EventHandler(this.tsmi_history_Click);
            // 
            // tsmi_unaudit
            // 
            this.tsmi_unaudit.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsmi_unaudit_gys,
            this.tsmi_unaudit_all});
            this.tsmi_unaudit.Name = "tsmi_unaudit";
            this.tsmi_unaudit.Size = new System.Drawing.Size(68, 21);
            this.tsmi_unaudit.Text = "免审信息";
            // 
            // tsmi_unaudit_gys
            // 
            this.tsmi_unaudit_gys.Name = "tsmi_unaudit_gys";
            this.tsmi_unaudit_gys.Size = new System.Drawing.Size(136, 22);
            this.tsmi_unaudit_gys.Text = "供应商维度";
            this.tsmi_unaudit_gys.Click += new System.EventHandler(this.tsmi_unaudit_gys_Click);
            // 
            // tsmi_unaudit_all
            // 
            this.tsmi_unaudit_all.Name = "tsmi_unaudit_all";
            this.tsmi_unaudit_all.Size = new System.Drawing.Size(136, 22);
            this.tsmi_unaudit_all.Text = "全部";
            this.tsmi_unaudit_all.Click += new System.EventHandler(this.tsmi_unaudit_all_Click);
            // 
            // tsmi_history_purunaudit_info
            // 
            this.tsmi_history_purunaudit_info.Name = "tsmi_history_purunaudit_info";
            this.tsmi_history_purunaudit_info.Size = new System.Drawing.Size(140, 21);
            this.tsmi_history_purunaudit_info.Text = "历史采购订单免审记录";
            this.tsmi_history_purunaudit_info.Click += new System.EventHandler(this.tsmi_history_purunaudit_info_Click);
            // 
            // dgvMain
            // 
            this.dgvMain.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.dgvMain.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvMain.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.Source,
            this.Supplier,
            this.MaterialId,
            this.MaterialName,
            this.MaterialSpec,
            this.up,
            this.Down,
            this.price,
            this.unit,
            this.qty,
            this.GuidSN});
            this.dgvMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvMain.Location = new System.Drawing.Point(0, 50);
            this.dgvMain.MultiSelect = false;
            this.dgvMain.Name = "dgvMain";
            this.dgvMain.RowTemplate.Height = 23;
            this.dgvMain.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvMain.Size = new System.Drawing.Size(967, 483);
            this.dgvMain.TabIndex = 1;
            // 
            // timer1
            // 
            this.timer1.Enabled = true;
            this.timer1.Interval = 1111;
            this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
            // 
            // statusStrip1
            // 
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripStatusLabel1});
            this.statusStrip1.Location = new System.Drawing.Point(0, 533);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(967, 22);
            this.statusStrip1.TabIndex = 3;
            this.statusStrip1.Text = "显示日期和时间";
            // 
            // toolStripStatusLabel1
            // 
            this.toolStripStatusLabel1.Name = "toolStripStatusLabel1";
            this.toolStripStatusLabel1.Padding = new System.Windows.Forms.Padding(695, 0, 0, 0);
            this.toolStripStatusLabel1.Size = new System.Drawing.Size(826, 17);
            this.toolStripStatusLabel1.Text = "toolStripStatusLabel1";
            // 
            // toolStrip1
            // 
            this.toolStrip1.BackColor = System.Drawing.Color.LightBlue;
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.btn_ins,
            this.btn_del,
            this.btn_clear,
            this.toolStripSeparator1,
            this.toolStripButton2,
            this.toolStripSeparator2,
            this.toolStripButNext,
            this.toolStripButPre});
            this.toolStrip1.Location = new System.Drawing.Point(0, 25);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.Size = new System.Drawing.Size(967, 25);
            this.toolStrip1.TabIndex = 4;
            this.toolStrip1.Text = "toolStrip1";
            // 
            // btn_ins
            // 
            this.btn_ins.Image = global::Zitn_exe_App.Properties.Resources.add;
            this.btn_ins.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btn_ins.Name = "btn_ins";
            this.btn_ins.Size = new System.Drawing.Size(64, 22);
            this.btn_ins.Text = "插入行";
            this.btn_ins.Click += new System.EventHandler(this.btn_ins_Click);
            // 
            // btn_del
            // 
            this.btn_del.Image = global::Zitn_exe_App.Properties.Resources.cancel;
            this.btn_del.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btn_del.Name = "btn_del";
            this.btn_del.Size = new System.Drawing.Size(64, 22);
            this.btn_del.Text = "刪除行";
            this.btn_del.Click += new System.EventHandler(this.btn_del_Click);
            // 
            // btn_clear
            // 
            this.btn_clear.Image = global::Zitn_exe_App.Properties.Resources.delete;
            this.btn_clear.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btn_clear.Name = "btn_clear";
            this.btn_clear.Size = new System.Drawing.Size(76, 22);
            this.btn_clear.Text = "清空数据";
            this.btn_clear.Click += new System.EventHandler(this.btn_clear_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(6, 25);
            // 
            // toolStripButton2
            // 
            this.toolStripButton2.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.toolStripButton2.Image = global::Zitn_exe_App.Properties.Resources.disk;
            this.toolStripButton2.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton2.Name = "toolStripButton2";
            this.toolStripButton2.Size = new System.Drawing.Size(52, 22);
            this.toolStripButton2.Text = "保存";
            this.toolStripButton2.Click += new System.EventHandler(this.toolStripMenuItem1_Click);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(6, 25);
            // 
            // toolStripButNext
            // 
            this.toolStripButNext.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.toolStripButNext.Image = global::Zitn_exe_App.Properties.Resources.resultset_next;
            this.toolStripButNext.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButNext.Name = "toolStripButNext";
            this.toolStripButNext.Size = new System.Drawing.Size(64, 22);
            this.toolStripButNext.Text = "下一組";
            this.toolStripButNext.Click += new System.EventHandler(this.toolStripButton3_Click);
            // 
            // toolStripButPre
            // 
            this.toolStripButPre.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.toolStripButPre.Enabled = false;
            this.toolStripButPre.Image = global::Zitn_exe_App.Properties.Resources.resultset_previous;
            this.toolStripButPre.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButPre.Name = "toolStripButPre";
            this.toolStripButPre.Size = new System.Drawing.Size(64, 22);
            this.toolStripButPre.Text = "上一組";
            this.toolStripButPre.Click += new System.EventHandler(this.toolStripButPre_Click);
            // 
            // Source
            // 
            this.Source.HeaderText = "数据来源";
            this.Source.Name = "Source";
            // 
            // Supplier
            // 
            this.Supplier.HeaderText = "供应商";
            this.Supplier.Name = "Supplier";
            // 
            // MaterialId
            // 
            this.MaterialId.HeaderText = "物料";
            this.MaterialId.Name = "MaterialId";
            // 
            // MaterialName
            // 
            this.MaterialName.HeaderText = "物料名称";
            this.MaterialName.Name = "MaterialName";
            // 
            // MaterialSpec
            // 
            this.MaterialSpec.HeaderText = "规格/型号";
            this.MaterialSpec.Name = "MaterialSpec";
            // 
            // up
            // 
            this.up.HeaderText = "上限";
            this.up.Name = "up";
            // 
            // Down
            // 
            this.Down.DataPropertyName = "Down";
            this.Down.HeaderText = "下限";
            this.Down.Name = "Down";
            this.Down.Visible = false;
            // 
            // price
            // 
            this.price.HeaderText = "含税单价";
            this.price.Name = "price";
            // 
            // unit
            // 
            this.unit.HeaderText = "单位";
            this.unit.Name = "unit";
            // 
            // qty
            // 
            this.qty.HeaderText = "本次采购数量";
            this.qty.Name = "qty";
            // 
            // GuidSN
            // 
            this.GuidSN.DataPropertyName = "GuidSN";
            this.GuidSN.HeaderText = "GuidSN";
            this.GuidSN.Name = "GuidSN";
            this.GuidSN.ReadOnly = true;
            this.GuidSN.Visible = false;
            // 
            // IndexForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(967, 555);
            this.Controls.Add(this.dgvMain);
            this.Controls.Add(this.toolStrip1);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.menuStrip1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "IndexForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "首页";
            this.Load += new System.EventHandler(this.IndexForm_Load);
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvMain)).EndInit();
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        

        #endregion

        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem3;
        private System.Windows.Forms.DataGridView dgvMain;
        private Timer timer1;
        private StatusStrip statusStrip1;
        private ToolStripStatusLabel toolStripStatusLabel1;
        private ToolStripMenuItem tsmi_history;
        private ToolStripMenuItem tsmi_unaudit;
        private ToolStripMenuItem tsmi_unaudit_gys;
        private ToolStripMenuItem tsmi_unaudit_all;
        private ToolStripMenuItem tsmi_history_purunaudit_info;
        private ToolStrip toolStrip1;
        private ToolStripButton btn_ins;
        private ToolStripButton btn_del;
        private ToolStripButton btn_clear;
        private ToolStripSeparator toolStripSeparator1;
        private ToolStripButton toolStripButPre;
        private ToolStripButton toolStripButton2;
        private ToolStripButton toolStripButNext;
        private ToolStripSeparator toolStripSeparator2;
        private DataGridViewTextBoxColumn Source;
        private DataGridViewTextBoxColumn Supplier;
        private DataGridViewTextBoxColumn MaterialId;
        private DataGridViewTextBoxColumn MaterialName;
        private DataGridViewTextBoxColumn MaterialSpec;
        private DataGridViewTextBoxColumn up;
        private DataGridViewTextBoxColumn Down;
        private DataGridViewTextBoxColumn price;
        private DataGridViewTextBoxColumn unit;
        private DataGridViewTextBoxColumn qty;
        private DataGridViewTextBoxColumn GuidSN;
    }
}