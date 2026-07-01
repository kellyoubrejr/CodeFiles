namespace Zitn_exe_App
{
    partial class LoginForm
    {
        /// <summary>
        /// 必需的设计器变量。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 清理所有正在使用的资源。
        /// </summary>
        /// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows 窗体设计器生成的代码

        /// <summary>
        /// 设计器支持所需的方法 - 不要修改
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(LoginForm));
            this.label1 = new System.Windows.Forms.Label();
            this.btn_qd = new System.Windows.Forms.Button();
            this.btn_rej = new System.Windows.Forms.Button();
            this.txtb_passw = new System.Windows.Forms.TextBox();
            this.txtb_user = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.toolStripStatusLabel1 = new System.Windows.Forms.ToolStripStatusLabel();
            this.statusStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("宋体", 18F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label1.Location = new System.Drawing.Point(160, 106);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(299, 24);
            this.label1.TabIndex = 1;
            this.label1.Text = "采购订单免审规则设置App";
            // 
            // btn_qd
            // 
            this.btn_qd.Location = new System.Drawing.Point(343, 247);
            this.btn_qd.Name = "btn_qd";
            this.btn_qd.Size = new System.Drawing.Size(75, 23);
            this.btn_qd.TabIndex = 12;
            this.btn_qd.Text = "登录";
            this.btn_qd.UseVisualStyleBackColor = true;
            this.btn_qd.Click += new System.EventHandler(this.btn_qd_Click);
            // 
            // btn_rej
            // 
            this.btn_rej.Location = new System.Drawing.Point(194, 248);
            this.btn_rej.Name = "btn_rej";
            this.btn_rej.Size = new System.Drawing.Size(75, 23);
            this.btn_rej.TabIndex = 11;
            this.btn_rej.Text = "重置密码";
            this.btn_rej.UseVisualStyleBackColor = true;
            this.btn_rej.Click += new System.EventHandler(this.btn_rej_Click);
            // 
            // txtb_passw
            // 
            this.txtb_passw.Location = new System.Drawing.Point(273, 200);
            this.txtb_passw.Name = "txtb_passw";
            this.txtb_passw.Size = new System.Drawing.Size(100, 21);
            this.txtb_passw.TabIndex = 10;
            this.txtb_passw.UseSystemPasswordChar = true;
            // 
            // txtb_user
            // 
            this.txtb_user.Location = new System.Drawing.Point(273, 169);
            this.txtb_user.Name = "txtb_user";
            this.txtb_user.Size = new System.Drawing.Size(100, 21);
            this.txtb_user.TabIndex = 9;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(215, 209);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(41, 12);
            this.label3.TabIndex = 8;
            this.label3.Text = "密码：";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(203, 178);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(53, 12);
            this.label2.TabIndex = 7;
            this.label2.Text = "用户名：";
            // 
            // timer1
            // 
            this.timer1.Enabled = true;
            this.timer1.Interval = 1000;
            this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
            // 
            // statusStrip1
            // 
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripStatusLabel1});
            this.statusStrip1.Location = new System.Drawing.Point(0, 354);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(639, 22);
            this.statusStrip1.TabIndex = 13;
            this.statusStrip1.Text = "显示日期和时间";
            // 
            // toolStripStatusLabel1
            // 
            this.toolStripStatusLabel1.Name = "toolStripStatusLabel1";
            this.toolStripStatusLabel1.Padding = new System.Windows.Forms.Padding(478, 0, 0, 0);
            this.toolStripStatusLabel1.Size = new System.Drawing.Size(609, 17);
            this.toolStripStatusLabel1.Text = "toolStripStatusLabel1";
            // 
            // LoginForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(639, 376);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.btn_qd);
            this.Controls.Add(this.btn_rej);
            this.Controls.Add(this.txtb_passw);
            this.Controls.Add(this.txtb_user);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "LoginForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "免审规则App";
            this.Load += new System.EventHandler(this.LoginForm_Load);
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button btn_qd;
        private System.Windows.Forms.Button btn_rej;
        private System.Windows.Forms.TextBox txtb_passw;
        private System.Windows.Forms.TextBox txtb_user;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Timer timer1;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel1;
    }
}

