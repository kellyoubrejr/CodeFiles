namespace Zitn_exe_App.Forms
{
    partial class ChangePasswordForm
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
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.txt_user = new System.Windows.Forms.TextBox();
            this.txt_oldPwd = new System.Windows.Forms.TextBox();
            this.txt_newPwd = new System.Windows.Forms.TextBox();
            this.txt_confirmPwd = new System.Windows.Forms.TextBox();
            this.btn_cancal = new System.Windows.Forms.Button();
            this.btn_ok = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("宋体", 18F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label1.Location = new System.Drawing.Point(301, 94);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(110, 24);
            this.label1.TabIndex = 0;
            this.label1.Text = "重置密码";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(260, 153);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(53, 12);
            this.label2.TabIndex = 1;
            this.label2.Text = "用户名：";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(260, 186);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(89, 12);
            this.label3.TabIndex = 2;
            this.label3.Text = "请输入旧密码：";
            this.label3.Click += new System.EventHandler(this.label3_Click);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(260, 216);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(89, 12);
            this.label4.TabIndex = 3;
            this.label4.Text = "请输入新密码：";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(260, 251);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(89, 12);
            this.label5.TabIndex = 4;
            this.label5.Text = "请确认新密码：";
            // 
            // txt_user
            // 
            this.txt_user.Location = new System.Drawing.Point(364, 144);
            this.txt_user.Name = "txt_user";
            this.txt_user.Size = new System.Drawing.Size(100, 21);
            this.txt_user.TabIndex = 5;
            // 
            // txt_oldPwd
            // 
            this.txt_oldPwd.Location = new System.Drawing.Point(364, 176);
            this.txt_oldPwd.Name = "txt_oldPwd";
            this.txt_oldPwd.Size = new System.Drawing.Size(100, 21);
            this.txt_oldPwd.TabIndex = 6;
            this.txt_oldPwd.UseSystemPasswordChar = true;
            // 
            // txt_newPwd
            // 
            this.txt_newPwd.Location = new System.Drawing.Point(364, 206);
            this.txt_newPwd.Name = "txt_newPwd";
            this.txt_newPwd.Size = new System.Drawing.Size(100, 21);
            this.txt_newPwd.TabIndex = 7;
            this.txt_newPwd.UseSystemPasswordChar = true;
            // 
            // txt_confirmPwd
            // 
            this.txt_confirmPwd.Location = new System.Drawing.Point(364, 242);
            this.txt_confirmPwd.Name = "txt_confirmPwd";
            this.txt_confirmPwd.Size = new System.Drawing.Size(100, 21);
            this.txt_confirmPwd.TabIndex = 8;
            this.txt_confirmPwd.UseSystemPasswordChar = true;
            // 
            // btn_cancal
            // 
            this.btn_cancal.Location = new System.Drawing.Point(271, 296);
            this.btn_cancal.Name = "btn_cancal";
            this.btn_cancal.Size = new System.Drawing.Size(75, 23);
            this.btn_cancal.TabIndex = 9;
            this.btn_cancal.Text = "退出";
            this.btn_cancal.UseVisualStyleBackColor = true;
            this.btn_cancal.Click += new System.EventHandler(this.btn_cancal_Click);
            // 
            // btn_ok
            // 
            this.btn_ok.Location = new System.Drawing.Point(416, 295);
            this.btn_ok.Name = "btn_ok";
            this.btn_ok.Size = new System.Drawing.Size(75, 23);
            this.btn_ok.TabIndex = 10;
            this.btn_ok.Text = "确认";
            this.btn_ok.UseVisualStyleBackColor = true;
            this.btn_ok.Click += new System.EventHandler(this.btn_ok_Click);
            // 
            // ChangePasswordForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(709, 408);
            this.Controls.Add(this.btn_ok);
            this.Controls.Add(this.btn_cancal);
            this.Controls.Add(this.txt_confirmPwd);
            this.Controls.Add(this.txt_newPwd);
            this.Controls.Add(this.txt_oldPwd);
            this.Controls.Add(this.txt_user);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Name = "ChangePasswordForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "重置密码";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox txt_user;
        private System.Windows.Forms.TextBox txt_oldPwd;
        private System.Windows.Forms.TextBox txt_newPwd;
        private System.Windows.Forms.TextBox txt_confirmPwd;
        private System.Windows.Forms.Button btn_cancal;
        private System.Windows.Forms.Button btn_ok;
    }
}