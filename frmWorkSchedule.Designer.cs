namespace Apontamento.Forms
{
    partial class FrmEscala
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
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.cbDomingoOff = new System.Windows.Forms.ComboBox();
            this.dtAlmocoInicio = new System.Windows.Forms.DateTimePicker();
            this.cbDayOff2 = new System.Windows.Forms.ComboBox();
            this.lblDomingoOff = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.cbDayOff1 = new System.Windows.Forms.ComboBox();
            this.label8 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.dtSaida = new System.Windows.Forms.DateTimePicker();
            this.dtAlmocoFim = new System.Windows.Forms.DateTimePicker();
            this.dtEntrada = new System.Windows.Forms.DateTimePicker();
            this.label6 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.cbTipoEscala = new System.Windows.Forms.ComboBox();
            this.label2 = new System.Windows.Forms.Label();
            this.txtDescricao = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.btnSalvar = new System.Windows.Forms.Button();
            this.btnDesativar = new System.Windows.Forms.Button();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.grdEscalas = new System.Windows.Forms.DataGridView();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.grdEscalas)).BeginInit();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.cbDomingoOff);
            this.groupBox1.Controls.Add(this.dtAlmocoInicio);
            this.groupBox1.Controls.Add(this.cbDayOff2);
            this.groupBox1.Controls.Add(this.lblDomingoOff);
            this.groupBox1.Controls.Add(this.label5);
            this.groupBox1.Controls.Add(this.cbDayOff1);
            this.groupBox1.Controls.Add(this.label8);
            this.groupBox1.Controls.Add(this.label7);
            this.groupBox1.Controls.Add(this.label4);
            this.groupBox1.Controls.Add(this.dtSaida);
            this.groupBox1.Controls.Add(this.dtAlmocoFim);
            this.groupBox1.Controls.Add(this.dtEntrada);
            this.groupBox1.Controls.Add(this.label6);
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Controls.Add(this.cbTipoEscala);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.txtDescricao);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Font = new System.Drawing.Font("Arial", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.groupBox1.Location = new System.Drawing.Point(12, 2);
            this.groupBox1.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Padding = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.groupBox1.Size = new System.Drawing.Size(1439, 340);
            this.groupBox1.TabIndex = 2;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Escalas";
            // 
            // cbDomingoOff
            // 
            this.cbDomingoOff.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbDomingoOff.FormattingEnabled = true;
            this.cbDomingoOff.Location = new System.Drawing.Point(423, 249);
            this.cbDomingoOff.Name = "cbDomingoOff";
            this.cbDomingoOff.Size = new System.Drawing.Size(200, 32);
            this.cbDomingoOff.TabIndex = 5;
            // 
            // dtAlmocoInicio
            // 
            this.dtAlmocoInicio.Format = System.Windows.Forms.DateTimePickerFormat.Time;
            this.dtAlmocoInicio.Location = new System.Drawing.Point(806, 101);
            this.dtAlmocoInicio.Name = "dtAlmocoInicio";
            this.dtAlmocoInicio.ShowUpDown = true;
            this.dtAlmocoInicio.Size = new System.Drawing.Size(200, 30);
            this.dtAlmocoInicio.TabIndex = 6;
            // 
            // cbDayOff2
            // 
            this.cbDayOff2.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbDayOff2.FormattingEnabled = true;
            this.cbDayOff2.Location = new System.Drawing.Point(805, 258);
            this.cbDayOff2.Name = "cbDayOff2";
            this.cbDayOff2.Size = new System.Drawing.Size(201, 32);
            this.cbDayOff2.TabIndex = 9;
            // 
            // lblDomingoOff
            // 
            this.lblDomingoOff.AutoSize = true;
            this.lblDomingoOff.Location = new System.Drawing.Point(268, 257);
            this.lblDomingoOff.Name = "lblDomingoOff";
            this.lblDomingoOff.Size = new System.Drawing.Size(130, 24);
            this.lblDomingoOff.TabIndex = 8;
            this.lblDomingoOff.Text = "Domingo Off";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(673, 154);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(115, 24);
            this.label5.TabIndex = 2;
            this.label5.Text = "Almoço fim";
            // 
            // cbDayOff1
            // 
            this.cbDayOff1.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbDayOff1.FormattingEnabled = true;
            this.cbDayOff1.Location = new System.Drawing.Point(422, 199);
            this.cbDayOff1.Name = "cbDayOff1";
            this.cbDayOff1.Size = new System.Drawing.Size(201, 32);
            this.cbDayOff1.TabIndex = 4;
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(688, 261);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(100, 24);
            this.label8.TabIndex = 6;
            this.label8.Text = "Day-Off 2";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(298, 207);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(100, 24);
            this.label7.TabIndex = 6;
            this.label7.Text = "Day-Off 1";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(652, 104);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(136, 24);
            this.label4.TabIndex = 2;
            this.label4.Text = "Almoço inicio";
            // 
            // dtSaida
            // 
            this.dtSaida.Format = System.Windows.Forms.DateTimePickerFormat.Time;
            this.dtSaida.Location = new System.Drawing.Point(425, 153);
            this.dtSaida.Name = "dtSaida";
            this.dtSaida.ShowUpDown = true;
            this.dtSaida.Size = new System.Drawing.Size(200, 30);
            this.dtSaida.TabIndex = 3;
            // 
            // dtAlmocoFim
            // 
            this.dtAlmocoFim.Format = System.Windows.Forms.DateTimePickerFormat.Time;
            this.dtAlmocoFim.Location = new System.Drawing.Point(806, 149);
            this.dtAlmocoFim.Name = "dtAlmocoFim";
            this.dtAlmocoFim.ShowUpDown = true;
            this.dtAlmocoFim.Size = new System.Drawing.Size(200, 30);
            this.dtAlmocoFim.TabIndex = 7;
            // 
            // dtEntrada
            // 
            this.dtEntrada.Format = System.Windows.Forms.DateTimePickerFormat.Time;
            this.dtEntrada.Location = new System.Drawing.Point(426, 100);
            this.dtEntrada.Name = "dtEntrada";
            this.dtEntrada.ShowUpDown = true;
            this.dtEntrada.Size = new System.Drawing.Size(200, 30);
            this.dtEntrada.TabIndex = 2;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(668, 207);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(120, 24);
            this.label6.TabIndex = 4;
            this.label6.Text = "Tipo Escala";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(335, 158);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(63, 24);
            this.label3.TabIndex = 2;
            this.label3.Text = "Saída";
            // 
            // cbTipoEscala
            // 
            this.cbTipoEscala.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbTipoEscala.FormattingEnabled = true;
            this.cbTipoEscala.Location = new System.Drawing.Point(804, 203);
            this.cbTipoEscala.Name = "cbTipoEscala";
            this.cbTipoEscala.Size = new System.Drawing.Size(201, 32);
            this.cbTipoEscala.TabIndex = 8;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(314, 106);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(84, 24);
            this.label2.TabIndex = 2;
            this.label2.Text = "Entrada";
            // 
            // txtDescricao
            // 
            this.txtDescricao.Location = new System.Drawing.Point(426, 53);
            this.txtDescricao.Name = "txtDescricao";
            this.txtDescricao.Size = new System.Drawing.Size(579, 30);
            this.txtDescricao.TabIndex = 1;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(294, 56);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(104, 24);
            this.label1.TabIndex = 0;
            this.label1.Text = "Descrição";
            // 
            // btnSalvar
            // 
            this.btnSalvar.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(123)))), ((int)(((byte)(255)))));
            this.btnSalvar.Font = new System.Drawing.Font("Arial", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnSalvar.ForeColor = System.Drawing.Color.White;
            this.btnSalvar.Location = new System.Drawing.Point(593, 740);
            this.btnSalvar.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.btnSalvar.Name = "btnSalvar";
            this.btnSalvar.Size = new System.Drawing.Size(147, 44);
            this.btnSalvar.TabIndex = 0;
            this.btnSalvar.Text = "Salvar";
            this.btnSalvar.UseVisualStyleBackColor = false;
            this.btnSalvar.Click += new System.EventHandler(this.btnSalvar_Click);
            // 
            // btnDesativar
            // 
            this.btnDesativar.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(123)))), ((int)(((byte)(255)))));
            this.btnDesativar.Font = new System.Drawing.Font("Arial", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnDesativar.ForeColor = System.Drawing.Color.White;
            this.btnDesativar.Location = new System.Drawing.Point(746, 740);
            this.btnDesativar.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.btnDesativar.Name = "btnDesativar";
            this.btnDesativar.Size = new System.Drawing.Size(147, 44);
            this.btnDesativar.TabIndex = 0;
            this.btnDesativar.Text = "Desativar";
            this.btnDesativar.UseVisualStyleBackColor = false;
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.grdEscalas);
            this.groupBox2.Font = new System.Drawing.Font("Arial", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.groupBox2.Location = new System.Drawing.Point(13, 348);
            this.groupBox2.Margin = new System.Windows.Forms.Padding(4);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Padding = new System.Windows.Forms.Padding(4);
            this.groupBox2.Size = new System.Drawing.Size(1438, 382);
            this.groupBox2.TabIndex = 1;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Escalas existentes";
            // 
            // grdEscalas
            // 
            this.grdEscalas.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.grdEscalas.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.grdEscalas.Dock = System.Windows.Forms.DockStyle.Fill;
            this.grdEscalas.Location = new System.Drawing.Point(4, 27);
            this.grdEscalas.Margin = new System.Windows.Forms.Padding(4);
            this.grdEscalas.Name = "grdEscalas";
            this.grdEscalas.RowHeadersWidth = 51;
            this.grdEscalas.Size = new System.Drawing.Size(1430, 351);
            this.grdEscalas.TabIndex = 0;
            this.grdEscalas.CellClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.grdEscalas_CellClick);
            // 
            // FrmEscala
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1478, 795);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.btnDesativar);
            this.Controls.Add(this.btnSalvar);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FrmEscala";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Cadastro de Escalas";
            this.Load += new System.EventHandler(this.FrmEscala_Load);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.grdEscalas)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Button btnSalvar;
        private System.Windows.Forms.Button btnDesativar;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.DataGridView grdEscalas;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox txtDescricao;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.DateTimePicker dtAlmocoFim;
        private System.Windows.Forms.DateTimePicker dtSaida;
        private System.Windows.Forms.DateTimePicker dtAlmocoInicio;
        private System.Windows.Forms.DateTimePicker dtEntrada;
        private System.Windows.Forms.ComboBox cbTipoEscala;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.ComboBox cbDayOff2;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.ComboBox cbDayOff1;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.ComboBox cbDomingoOff;
        private System.Windows.Forms.Label lblDomingoOff;
    }
}