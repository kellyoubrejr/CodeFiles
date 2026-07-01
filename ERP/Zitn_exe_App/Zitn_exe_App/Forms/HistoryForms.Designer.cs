namespace Zitn_exe_App.Forms
{
    partial class HistoryForms
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
            this.dgv_gys = new System.Windows.Forms.DataGridView();
            this.chkSelect = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.wlnum = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.wlname = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.wlspec = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.qty = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.taxprice = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.btnConfirm = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.dgv_gys)).BeginInit();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(117, 39);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(53, 12);
            this.label1.TabIndex = 0;
            this.label1.Text = "供应商：";
            // 
            // lblSupplierName
            // 
            this.lblSupplierName.AutoSize = true;
            this.lblSupplierName.ForeColor = System.Drawing.SystemColors.Highlight;
            this.lblSupplierName.Location = new System.Drawing.Point(189, 39);
            this.lblSupplierName.Margin = new System.Windows.Forms.Padding(5, 0, 5, 0);
            this.lblSupplierName.Name = "lblSupplierName";
            this.lblSupplierName.Size = new System.Drawing.Size(41, 12);
            this.lblSupplierName.TabIndex = 1;
            this.lblSupplierName.Text = "111111";
            // 
            // dgv_gys
            // 
            this.dgv_gys.AllowUserToAddRows = false;
            this.dgv_gys.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dgv_gys.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgv_gys.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.chkSelect,
            this.wlnum,
            this.wlname,
            this.wlspec,
            this.qty,
            this.taxprice});
            this.dgv_gys.Location = new System.Drawing.Point(72, 88);
            this.dgv_gys.Name = "dgv_gys";
            this.dgv_gys.RowTemplate.Height = 23;
            this.dgv_gys.Size = new System.Drawing.Size(645, 360);
            this.dgv_gys.TabIndex = 2;
            this.dgv_gys.CellContentClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgv_gys_CellContentClick);
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
            // qty
            // 
            this.qty.HeaderText = "采购数量";
            this.qty.Name = "qty";
            // 
            // taxprice
            // 
            this.taxprice.HeaderText = "含税单价";
            this.taxprice.Name = "taxprice";
            // 
            // btnConfirm
            // 
            this.btnConfirm.Location = new System.Drawing.Point(400, 34);
            this.btnConfirm.Name = "btnConfirm";
            this.btnConfirm.Size = new System.Drawing.Size(75, 23);
            this.btnConfirm.TabIndex = 3;
            this.btnConfirm.Text = "确认";
            this.btnConfirm.UseVisualStyleBackColor = true;
            this.btnConfirm.Click += new System.EventHandler(this.btnConfirm_Click);
            // 
            // HistoryForms
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(799, 450);
            this.Controls.Add(this.btnConfirm);
            this.Controls.Add(this.dgv_gys);
            this.Controls.Add(this.lblSupplierName);
            this.Controls.Add(this.label1);
            this.Name = "HistoryForms";
            this.Text = "供应商历史采购物料情况";
            this.Load += new System.EventHandler(this.HistoryForms_Load);
            ((System.ComponentModel.ISupportInitialize)(this.dgv_gys)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label lblSupplierName;
        private System.Windows.Forms.DataGridView dgv_gys;
        private System.Windows.Forms.Button btnConfirm;
        private System.Windows.Forms.DataGridViewCheckBoxColumn chkSelect;
        private System.Windows.Forms.DataGridViewTextBoxColumn wlnum;
        private System.Windows.Forms.DataGridViewTextBoxColumn wlname;
        private System.Windows.Forms.DataGridViewTextBoxColumn wlspec;
        private System.Windows.Forms.DataGridViewTextBoxColumn qty;
        private System.Windows.Forms.DataGridViewTextBoxColumn taxprice;
    }
}