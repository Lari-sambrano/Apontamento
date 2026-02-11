namespace Apontamento
{
    partial class frmGcm
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
            this.grdGcm = new System.Windows.Forms.GroupBox();
            this.btnSalvar = new System.Windows.Forms.Button();
            this.txtValor = new System.Windows.Forms.TextBox();
            this.cbGcm = new System.Windows.Forms.ComboBox();
            this.label2 = new System.Windows.Forms.Label();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.grdGcm1 = new System.Windows.Forms.DataGridView();
            this.grdGcm.SuspendLayout();
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.grdGcm1)).BeginInit();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Arial", 12F, System.Drawing.FontStyle.Bold);
            this.label1.Location = new System.Drawing.Point(461, 50);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(196, 24);
            this.label1.TabIndex = 0;
            this.label1.Text = "Selecione um GCM:";
            // 
            // grdGcm
            // 
            this.grdGcm.Controls.Add(this.btnSalvar);
            this.grdGcm.Controls.Add(this.txtValor);
            this.grdGcm.Controls.Add(this.cbGcm);
            this.grdGcm.Controls.Add(this.label2);
            this.grdGcm.Controls.Add(this.label1);
            this.grdGcm.Font = new System.Drawing.Font("Arial", 12F, System.Drawing.FontStyle.Bold);
            this.grdGcm.Location = new System.Drawing.Point(56, 32);
            this.grdGcm.Name = "grdGcm";
            this.grdGcm.Size = new System.Drawing.Size(1636, 225);
            this.grdGcm.TabIndex = 1;
            this.grdGcm.TabStop = false;
            this.grdGcm.Text = "Altere o valor do GCM";
            // 
            // btnSalvar
            // 
            this.btnSalvar.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(123)))), ((int)(((byte)(255)))));
            this.btnSalvar.ForeColor = System.Drawing.Color.White;
            this.btnSalvar.Location = new System.Drawing.Point(674, 152);
            this.btnSalvar.Name = "btnSalvar";
            this.btnSalvar.Size = new System.Drawing.Size(177, 40);
            this.btnSalvar.TabIndex = 3;
            this.btnSalvar.Text = "Salvar";
            this.btnSalvar.UseVisualStyleBackColor = false;
            this.btnSalvar.Click += new System.EventHandler(this.btnSalvar_Click);
            // 
            // txtValor
            // 
            this.txtValor.Location = new System.Drawing.Point(674, 95);
            this.txtValor.Name = "txtValor";
            this.txtValor.Size = new System.Drawing.Size(178, 30);
            this.txtValor.TabIndex = 2;
            // 
            // cbGcm
            // 
            this.cbGcm.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbGcm.FormattingEnabled = true;
            this.cbGcm.Location = new System.Drawing.Point(673, 42);
            this.cbGcm.Name = "cbGcm";
            this.cbGcm.Size = new System.Drawing.Size(358, 32);
            this.cbGcm.TabIndex = 1;
            this.cbGcm.SelectedIndexChanged += new System.EventHandler(this.cbGcm_SelectedIndexChanged);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Arial", 12F, System.Drawing.FontStyle.Bold);
            this.label2.Location = new System.Drawing.Point(592, 95);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(65, 24);
            this.label2.TabIndex = 1;
            this.label2.Text = "Valor:";
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.grdGcm1);
            this.groupBox1.Font = new System.Drawing.Font("Arial", 12F, System.Drawing.FontStyle.Bold);
            this.groupBox1.Location = new System.Drawing.Point(53, 307);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(1631, 411);
            this.groupBox1.TabIndex = 2;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "GCM\'s Cadastrados";
            // 
            // grdGcm1
            // 
            this.grdGcm1.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.grdGcm1.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.grdGcm1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.grdGcm1.Location = new System.Drawing.Point(3, 26);
            this.grdGcm1.Name = "grdGcm1";
            this.grdGcm1.ReadOnly = true;
            this.grdGcm1.RowHeadersWidth = 51;
            this.grdGcm1.RowTemplate.Height = 24;
            this.grdGcm1.Size = new System.Drawing.Size(1625, 382);
            this.grdGcm1.TabIndex = 0;
            // 
            // frmGcm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1738, 743);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.grdGcm);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "frmGcm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Gerenciar GCM\'s";
            this.Load += new System.EventHandler(this.frmGcm_Load);
            this.grdGcm.ResumeLayout(false);
            this.grdGcm.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.grdGcm1)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.GroupBox grdGcm;
        private System.Windows.Forms.Button btnSalvar;
        private System.Windows.Forms.TextBox txtValor;
        private System.Windows.Forms.ComboBox cbGcm;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.DataGridView grdGcm1;
    }
}