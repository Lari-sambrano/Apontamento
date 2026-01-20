namespace Apontamento
{
    partial class frmSubirOnline
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
            this.gbCadastros = new System.Windows.Forms.GroupBox();
            this.cbTela = new System.Windows.Forms.ComboBox();
            this.grdSubirTela = new System.Windows.Forms.GroupBox();
            this.btnGravarOnline = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.grdOffline = new System.Windows.Forms.DataGridView();
            this.gbCadastros.SuspendLayout();
            this.grdSubirTela.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.grdOffline)).BeginInit();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(174, 34);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(186, 29);
            this.label1.TabIndex = 0;
            this.label1.Text = "Selecione a tela";
            // 
            // gbCadastros
            // 
            this.gbCadastros.Controls.Add(this.cbTela);
            this.gbCadastros.Controls.Add(this.label1);
            this.gbCadastros.Location = new System.Drawing.Point(136, 42);
            this.gbCadastros.Name = "gbCadastros";
            this.gbCadastros.Size = new System.Drawing.Size(865, 100);
            this.gbCadastros.TabIndex = 1;
            this.gbCadastros.TabStop = false;
            this.gbCadastros.Text = "Telas Off-line";
            // 
            // cbTela
            // 
            this.cbTela.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbTela.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cbTela.FormattingEnabled = true;
            this.cbTela.Items.AddRange(new object[] {
            "Colaboradores",
            "Apontamento",
            "Work Schedule"});
            this.cbTela.Location = new System.Drawing.Point(380, 30);
            this.cbTela.Name = "cbTela";
            this.cbTela.Size = new System.Drawing.Size(359, 33);
            this.cbTela.TabIndex = 1;
            this.cbTela.SelectedIndexChanged += new System.EventHandler(this.cbTela_SelectedIndexChanged);
            // 
            // grdSubirTela
            // 
            this.grdSubirTela.Controls.Add(this.btnGravarOnline);
            this.grdSubirTela.Controls.Add(this.label2);
            this.grdSubirTela.Controls.Add(this.grdOffline);
            this.grdSubirTela.Location = new System.Drawing.Point(44, 181);
            this.grdSubirTela.Name = "grdSubirTela";
            this.grdSubirTela.Size = new System.Drawing.Size(1061, 581);
            this.grdSubirTela.TabIndex = 2;
            this.grdSubirTela.TabStop = false;
            this.grdSubirTela.Text = "Dados Off-line";
            // 
            // btnGravarOnline
            // 
            this.btnGravarOnline.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(123)))), ((int)(((byte)(255)))));
            this.btnGravarOnline.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnGravarOnline.ForeColor = System.Drawing.Color.White;
            this.btnGravarOnline.Location = new System.Drawing.Point(472, 503);
            this.btnGravarOnline.Name = "btnGravarOnline";
            this.btnGravarOnline.Size = new System.Drawing.Size(165, 44);
            this.btnGravarOnline.TabIndex = 2;
            this.btnGravarOnline.Text = "Gravar Online";
            this.btnGravarOnline.UseVisualStyleBackColor = false;
            this.btnGravarOnline.Click += new System.EventHandler(this.btnGravarOnline_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.BackColor = System.Drawing.Color.Transparent;
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.ForeColor = System.Drawing.Color.Red;
            this.label2.Location = new System.Drawing.Point(21, 517);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(282, 16);
            this.label2.TabIndex = 1;
            this.label2.Text = "Para excluir clique 2x na linha desejada";
            // 
            // grdOffline
            // 
            this.grdOffline.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.grdOffline.Location = new System.Drawing.Point(24, 27);
            this.grdOffline.Name = "grdOffline";
            this.grdOffline.RowHeadersWidth = 51;
            this.grdOffline.RowTemplate.Height = 24;
            this.grdOffline.Size = new System.Drawing.Size(1011, 465);
            this.grdOffline.TabIndex = 0;
            // 
            // frmSubirOnline
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.Window;
            this.ClientSize = new System.Drawing.Size(1142, 774);
            this.Controls.Add(this.grdSubirTela);
            this.Controls.Add(this.gbCadastros);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "frmSubirOnline";
            this.Text = "SubirOnline";
            this.Load += new System.EventHandler(this.frmSubirOnline_Load);
            this.gbCadastros.ResumeLayout(false);
            this.gbCadastros.PerformLayout();
            this.grdSubirTela.ResumeLayout(false);
            this.grdSubirTela.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.grdOffline)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.GroupBox gbCadastros;
        private System.Windows.Forms.ComboBox cbTela;
        private System.Windows.Forms.GroupBox grdSubirTela;
        private System.Windows.Forms.DataGridView grdOffline;
        private System.Windows.Forms.Button btnGravarOnline;
        private System.Windows.Forms.Label label2;
    }
}