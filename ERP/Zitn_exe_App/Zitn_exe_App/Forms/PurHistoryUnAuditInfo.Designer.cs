namespace Zitn_exe_App.Forms
{
    partial class PurHistoryUnAuditInfo
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
            this.dgv_purall = new System.Windows.Forms.DataGridView();
            this.dtpStart = new System.Windows.Forms.DateTimePicker();
            this.btnConfirm = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.pur = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.purdate = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.sqr = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.syb = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.buyer = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.supplier = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.wlnum = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.wlname = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.wlspec = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.qty = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.taxprice = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.note = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.fdate = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.setBtn = new System.Windows.Forms.DataGridViewButtonColumn();
            ((System.ComponentModel.ISupportInitialize)(this.dgv_purall)).BeginInit();
            this.SuspendLayout();
            // 
            // dgv_purall
            // 
            this.dgv_purall.AllowUserToAddRows = false;
            this.dgv_purall.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dgv_purall.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgv_purall.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.pur,
            this.purdate,
            this.sqr,
            this.syb,
            this.buyer,
            this.supplier,
            this.wlnum,
            this.wlname,
            this.wlspec,
            this.qty,
            this.taxprice,
            this.note,
            this.fdate,
            this.setBtn});
            this.dgv_purall.Location = new System.Drawing.Point(-1, 77);
            this.dgv_purall.Name = "dgv_purall";
            this.dgv_purall.RowTemplate.Height = 23;
            this.dgv_purall.Size = new System.Drawing.Size(1444, 569);
            this.dgv_purall.TabIndex = 0;
            // 
            // dtpStart
            // 
            this.dtpStart.Location = new System.Drawing.Point(310, 32);
            this.dtpStart.Name = "dtpStart";
            this.dtpStart.Size = new System.Drawing.Size(126, 21);
            this.dtpStart.TabIndex = 1;
            // 
            // btnConfirm
            // 
            this.btnConfirm.Location = new System.Drawing.Point(477, 32);
            this.btnConfirm.Name = "btnConfirm";
            this.btnConfirm.Size = new System.Drawing.Size(75, 23);
            this.btnConfirm.TabIndex = 2;
            this.btnConfirm.Text = "确定";
            this.btnConfirm.UseVisualStyleBackColor = true;
            this.btnConfirm.Click += new System.EventHandler(this.btnConfirm_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(243, 37);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(65, 12);
            this.label1.TabIndex = 3;
            this.label1.Text = "开始时间：";
            // 
            // pur
            // 
            this.pur.HeaderText = "采购订单";
            this.pur.Name = "pur";
            // 
            // purdate
            // 
            this.purdate.HeaderText = "采购日期";
            this.purdate.Name = "purdate";
            // 
            // sqr
            // 
            this.sqr.HeaderText = "申请人";
            this.sqr.Name = "sqr";
            // 
            // syb
            // 
            this.syb.HeaderText = "事业部";
            this.syb.Name = "syb";
            // 
            // buyer
            // 
            this.buyer.HeaderText = "采购员";
            this.buyer.Name = "buyer";
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
            // note
            // 
            this.note.HeaderText = "备注";
            this.note.Name = "note";
            // 
            // fdate
            // 
            this.fdate.HeaderText = "规则日期";
            this.fdate.Name = "fdate";
            // 
            // setBtn
            //
            this.setBtn.HeaderText = "设置免审";
            this.setBtn.Name = "setBtn";
            this.setBtn.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            this.setBtn.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Automatic;
            this.setBtn.Text = "设置免审";
            // 
            // PurHistoryUnAuditInfo
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1466, 642);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.btnConfirm);
            this.Controls.Add(this.dtpStart);
            this.Controls.Add(this.dgv_purall);
            this.Name = "PurHistoryUnAuditInfo";
            this.Text = "PurHistoryUnAuditInfo";
            ((System.ComponentModel.ISupportInitialize)(this.dgv_purall)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.DataGridView dgv_purall;
        private System.Windows.Forms.DateTimePicker dtpStart;
        private System.Windows.Forms.Button btnConfirm;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.DataGridViewTextBoxColumn pur;
        private System.Windows.Forms.DataGridViewTextBoxColumn purdate;
        private System.Windows.Forms.DataGridViewTextBoxColumn sqr;
        private System.Windows.Forms.DataGridViewTextBoxColumn syb;
        private System.Windows.Forms.DataGridViewTextBoxColumn buyer;
        private System.Windows.Forms.DataGridViewTextBoxColumn supplier;
        private System.Windows.Forms.DataGridViewTextBoxColumn wlnum;
        private System.Windows.Forms.DataGridViewTextBoxColumn wlname;
        private System.Windows.Forms.DataGridViewTextBoxColumn wlspec;
        private System.Windows.Forms.DataGridViewTextBoxColumn qty;
        private System.Windows.Forms.DataGridViewTextBoxColumn taxprice;
        private System.Windows.Forms.DataGridViewTextBoxColumn note;
        private System.Windows.Forms.DataGridViewTextBoxColumn fdate;
        private System.Windows.Forms.DataGridViewButtonColumn setBtn;
    }
}