namespace Zitn_exe_App.Forms
{
    partial class AllUnAuditInfo
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
            this.dgv_all = new System.Windows.Forms.DataGridView();
            this.chkSelect = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.supplier = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.wlnum = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.wlname = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.wlspec = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.up = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.taxprice = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.unit = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.btnConfirm = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.btnSearch = new System.Windows.Forms.Button();
            this.comboBox1 = new System.Windows.Forms.ComboBox();
            this.comboBox2 = new System.Windows.Forms.ComboBox();
            ((System.ComponentModel.ISupportInitialize)(this.dgv_all)).BeginInit();
            this.SuspendLayout();
            // 
            // dgv_all
            // 
            this.dgv_all.AllowUserToAddRows = false;
            this.dgv_all.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dgv_all.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgv_all.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.chkSelect,
            this.supplier,
            this.wlnum,
            this.wlname,
            this.wlspec,
            this.up,
            this.taxprice,
            this.unit});
            this.dgv_all.Location = new System.Drawing.Point(2, 62);
            this.dgv_all.Name = "dgv_all";
            this.dgv_all.RowTemplate.Height = 23;
            this.dgv_all.Size = new System.Drawing.Size(982, 388);
            this.dgv_all.TabIndex = 0;
            // 
            // chkSelect
            // 
            this.chkSelect.HeaderText = "选择";
            this.chkSelect.Name = "chkSelect";
            // 
            // supplier
            // 
            this.supplier.HeaderText = "供应商";
            this.supplier.Name = "supplier";
            // 
            // wlnum
            // 
            this.wlnum.HeaderText = "物料编码";
            this.wlnum.Name = "wlnum";
            // 
            // wlname
            // 
            this.wlname.HeaderText = "物料名称";
            this.wlname.Name = "wlname";
            // 
            // wlspec
            // 
            this.wlspec.HeaderText = "规格";
            this.wlspec.Name = "wlspec";
            // 
            // up
            // 
            this.up.HeaderText = "上限";
            this.up.Name = "up";
            // 
            // taxprice
            // 
            this.taxprice.HeaderText = "含税单价";
            this.taxprice.Name = "taxprice";
            // 
            // unit
            // 
            this.unit.HeaderText = "单位";
            this.unit.Name = "unit";
            // 
            // btnConfirm
            // 
            this.btnConfirm.Location = new System.Drawing.Point(2, 40);
            this.btnConfirm.Name = "btnConfirm";
            this.btnConfirm.Size = new System.Drawing.Size(75, 23);
            this.btnConfirm.TabIndex = 1;
            this.btnConfirm.Text = "确认";
            this.btnConfirm.UseVisualStyleBackColor = true;
            this.btnConfirm.Click += new System.EventHandler(this.btnConfirm_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(132, 27);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(53, 12);
            this.label1.TabIndex = 2;
            this.label1.Text = "供应商：";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(340, 27);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(65, 12);
            this.label2.TabIndex = 4;
            this.label2.Text = "物料编码：";
            // 
            // btnSearch
            // 
            this.btnSearch.Location = new System.Drawing.Point(561, 22);
            this.btnSearch.Name = "btnSearch";
            this.btnSearch.Size = new System.Drawing.Size(75, 23);
            this.btnSearch.TabIndex = 6;
            this.btnSearch.Text = "搜索";
            this.btnSearch.UseVisualStyleBackColor = true;
            this.btnSearch.Click += new System.EventHandler(this.btnSearch_Click);
            // 
            // comboBox1
            // 
            this.comboBox1.FormattingEnabled = true;
            this.comboBox1.Location = new System.Drawing.Point(191, 22);
            this.comboBox1.Name = "comboBox1";
            this.comboBox1.Size = new System.Drawing.Size(121, 20);
            this.comboBox1.TabIndex = 7;
            // 
            // comboBox2
            // 
            this.comboBox2.FormattingEnabled = true;
            this.comboBox2.Location = new System.Drawing.Point(411, 22);
            this.comboBox2.Name = "comboBox2";
            this.comboBox2.Size = new System.Drawing.Size(121, 20);
            this.comboBox2.TabIndex = 8;
            // 
            // AllUnAuditInfo
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(987, 450);
            this.Controls.Add(this.comboBox2);
            this.Controls.Add(this.comboBox1);
            this.Controls.Add(this.btnSearch);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.btnConfirm);
            this.Controls.Add(this.dgv_all);
            this.Name = "AllUnAuditInfo";
            this.Text = "AllUnAuditInfo";
            ((System.ComponentModel.ISupportInitialize)(this.dgv_all)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.DataGridView dgv_all;
        private System.Windows.Forms.Button btnConfirm;
        private System.Windows.Forms.DataGridViewCheckBoxColumn chkSelect;
        private System.Windows.Forms.DataGridViewTextBoxColumn supplier;
        private System.Windows.Forms.DataGridViewTextBoxColumn wlnum;
        private System.Windows.Forms.DataGridViewTextBoxColumn wlname;
        private System.Windows.Forms.DataGridViewTextBoxColumn wlspec;
        private System.Windows.Forms.DataGridViewTextBoxColumn up;
        private System.Windows.Forms.DataGridViewTextBoxColumn taxprice;
        private System.Windows.Forms.DataGridViewTextBoxColumn unit;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button btnSearch;
        private System.Windows.Forms.ComboBox comboBox1;
        private System.Windows.Forms.ComboBox comboBox2;
    }
}