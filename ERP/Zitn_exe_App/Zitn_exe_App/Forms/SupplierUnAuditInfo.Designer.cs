namespace Zitn_exe_App.Forms
{
    partial class SupplierUnAuditInfo
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
            this.label1 = new System.Windows.Forms.Label();
            this.lblSupplierName = new System.Windows.Forms.Label();
            this.dgv_unAudit = new System.Windows.Forms.DataGridView();
            this.chkSelect = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.wlnum = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.wlname = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.wlspec = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.up = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.taxprice = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.unit = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.btnConfirm = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.txtb_wl = new System.Windows.Forms.TextBox();
            this.btnSearch = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.dgv_unAudit)).BeginInit();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(63, 33);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(53, 12);
            this.label1.TabIndex = 0;
            this.label1.Text = "供应商：";
            // 
            // lblSupplierName
            // 
            this.lblSupplierName.AutoSize = true;
            this.lblSupplierName.Location = new System.Drawing.Point(122, 33);
            this.lblSupplierName.Name = "lblSupplierName";
            this.lblSupplierName.Size = new System.Drawing.Size(41, 12);
            this.lblSupplierName.TabIndex = 1;
            this.lblSupplierName.Text = "label2";
            // 
            // dgv_unAudit
            // 
            this.dgv_unAudit.AllowUserToAddRows = false;
            this.dgv_unAudit.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dgv_unAudit.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgv_unAudit.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.chkSelect,
            this.wlnum,
            this.wlname,
            this.wlspec,
            this.up,
            this.taxprice,
            this.unit});
            this.dgv_unAudit.Location = new System.Drawing.Point(65, 79);
            this.dgv_unAudit.Name = "dgv_unAudit";
            this.dgv_unAudit.RowTemplate.Height = 23;
            this.dgv_unAudit.Size = new System.Drawing.Size(744, 359);
            this.dgv_unAudit.TabIndex = 2;
            // 
            // chkSelect
            // 
            this.chkSelect.HeaderText = "选择";
            this.chkSelect.Name = "chkSelect";
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
            this.btnConfirm.Location = new System.Drawing.Point(65, 57);
            this.btnConfirm.Name = "btnConfirm";
            this.btnConfirm.Size = new System.Drawing.Size(75, 23);
            this.btnConfirm.TabIndex = 3;
            this.btnConfirm.Text = "确认";
            this.btnConfirm.UseVisualStyleBackColor = true;
            this.btnConfirm.Click += new System.EventHandler(this.btnConfirm_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(269, 33);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(65, 12);
            this.label2.TabIndex = 4;
            this.label2.Text = "物料编码：";
            // 
            // txtb_wl
            // 
            this.txtb_wl.Location = new System.Drawing.Point(338, 30);
            this.txtb_wl.Name = "txtb_wl";
            this.txtb_wl.Size = new System.Drawing.Size(100, 21);
            this.txtb_wl.TabIndex = 5;
            // 
            // btnSearch
            // 
            this.btnSearch.Location = new System.Drawing.Point(482, 30);
            this.btnSearch.Name = "btnSearch";
            this.btnSearch.Size = new System.Drawing.Size(75, 23);
            this.btnSearch.TabIndex = 6;
            this.btnSearch.Text = "搜索";
            this.btnSearch.UseVisualStyleBackColor = true;
            this.btnSearch.Click += new System.EventHandler(this.btnSearch_Click);
            // 
            // SupplierUnAuditInfo
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(812, 450);
            this.Controls.Add(this.btnSearch);
            this.Controls.Add(this.txtb_wl);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.btnConfirm);
            this.Controls.Add(this.dgv_unAudit);
            this.Controls.Add(this.lblSupplierName);
            this.Controls.Add(this.label1);
            this.Name = "SupplierUnAuditInfo";
            this.Text = "免审信息(供应商)";
            ((System.ComponentModel.ISupportInitialize)(this.dgv_unAudit)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label lblSupplierName;
        private System.Windows.Forms.DataGridView dgv_unAudit;
        private System.Windows.Forms.Button btnConfirm;
        private System.Windows.Forms.DataGridViewCheckBoxColumn chkSelect;
        private System.Windows.Forms.DataGridViewTextBoxColumn wlnum;
        private System.Windows.Forms.DataGridViewTextBoxColumn wlname;
        private System.Windows.Forms.DataGridViewTextBoxColumn wlspec;
        private System.Windows.Forms.DataGridViewTextBoxColumn up;
        private System.Windows.Forms.DataGridViewTextBoxColumn taxprice;
        private System.Windows.Forms.DataGridViewTextBoxColumn unit;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox txtb_wl;
        private System.Windows.Forms.Button btnSearch;
    }
}