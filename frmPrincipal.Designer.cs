namespace Apontamento
{
    partial class frmPrincipal
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
            System.Windows.Forms.MenuStrip menuStrip1;
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmPrincipal));
            this.cadastroDeColaboradorToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.apontamentoToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.sairToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.escalasToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.escalasEspeciaisToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.gcmToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            menuStrip1 = new System.Windows.Forms.MenuStrip();
            menuStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.SuspendLayout();
            // 
            // menuStrip1
            // 
            menuStrip1.AllowDrop = true;
            menuStrip1.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            menuStrip1.ImageScalingSize = new System.Drawing.Size(20, 20);
            menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.cadastroDeColaboradorToolStripMenuItem,
            this.apontamentoToolStripMenuItem,
            this.sairToolStripMenuItem,
            this.escalasToolStripMenuItem,
            this.gcmToolStripMenuItem});
            menuStrip1.Location = new System.Drawing.Point(0, 0);
            menuStrip1.Name = "menuStrip1";
            menuStrip1.Padding = new System.Windows.Forms.Padding(8, 3, 0, 3);
            menuStrip1.RenderMode = System.Windows.Forms.ToolStripRenderMode.Professional;
            menuStrip1.Size = new System.Drawing.Size(1924, 38);
            menuStrip1.TabIndex = 0;
            menuStrip1.Text = "menuStrip1";
            // 
            // cadastroDeColaboradorToolStripMenuItem
            // 
            this.cadastroDeColaboradorToolStripMenuItem.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold);
            this.cadastroDeColaboradorToolStripMenuItem.Name = "cadastroDeColaboradorToolStripMenuItem";
            this.cadastroDeColaboradorToolStripMenuItem.Size = new System.Drawing.Size(258, 32);
            this.cadastroDeColaboradorToolStripMenuItem.Text = "Cadastro de colaborador";
            this.cadastroDeColaboradorToolStripMenuItem.Click += new System.EventHandler(this.cadastroDeColaboradorToolStripMenuItem_Click);
            // 
            // apontamentoToolStripMenuItem
            // 
            this.apontamentoToolStripMenuItem.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold);
            this.apontamentoToolStripMenuItem.Name = "apontamentoToolStripMenuItem";
            this.apontamentoToolStripMenuItem.Size = new System.Drawing.Size(156, 32);
            this.apontamentoToolStripMenuItem.Text = "Apontamento";
            this.apontamentoToolStripMenuItem.Click += new System.EventHandler(this.apontamentoToolStripMenuItem_Click);
            // 
            // sairToolStripMenuItem
            // 
            this.sairToolStripMenuItem.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold);
            this.sairToolStripMenuItem.Name = "sairToolStripMenuItem";
            this.sairToolStripMenuItem.Size = new System.Drawing.Size(62, 32);
            this.sairToolStripMenuItem.Text = "Sair";
            this.sairToolStripMenuItem.Click += new System.EventHandler(this.sairToolStripMenuItem_Click);
            // 
            // escalasToolStripMenuItem
            // 
            this.escalasToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.escalasEspeciaisToolStripMenuItem});
            this.escalasToolStripMenuItem.Name = "escalasToolStripMenuItem";
            this.escalasToolStripMenuItem.Size = new System.Drawing.Size(93, 32);
            this.escalasToolStripMenuItem.Text = "Escalas";
            // 
            // escalasEspeciaisToolStripMenuItem
            // 
            this.escalasEspeciaisToolStripMenuItem.Name = "escalasEspeciaisToolStripMenuItem";
            this.escalasEspeciaisToolStripMenuItem.Size = new System.Drawing.Size(256, 32);
            this.escalasEspeciaisToolStripMenuItem.Text = "Escalas Especiais";
            this.escalasEspeciaisToolStripMenuItem.Click += new System.EventHandler(this.escalasEspeciaisToolStripMenuItem_Click);
            // 
            // gcmToolStripMenuItem
            // 
            this.gcmToolStripMenuItem.Name = "gcmToolStripMenuItem";
            this.gcmToolStripMenuItem.Size = new System.Drawing.Size(68, 32);
            this.gcmToolStripMenuItem.Text = "Gcm";
            this.gcmToolStripMenuItem.Click += new System.EventHandler(this.gcmToolStripMenuItem_Click);
            // 
            // pictureBox1
            // 
            this.pictureBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pictureBox1.Image = ((System.Drawing.Image)(resources.GetObject("pictureBox1.Image")));
            this.pictureBox1.Location = new System.Drawing.Point(0, 38);
            this.pictureBox1.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(1924, 1017);
            this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pictureBox1.TabIndex = 1;
            this.pictureBox1.TabStop = false;
            // 
            // frmPrincipal
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(12F, 24F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.ActiveCaption;
            this.ClientSize = new System.Drawing.Size(1924, 1055);
            this.Controls.Add(this.pictureBox1);
            this.Controls.Add(menuStrip1);
            this.Font = new System.Drawing.Font("Arial", 12F, System.Drawing.FontStyle.Bold);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MainMenuStrip = menuStrip1;
            this.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "frmPrincipal";
            this.Text = "Apontamento";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            menuStrip1.ResumeLayout(false);
            menuStrip1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.ToolStripMenuItem cadastroDeColaboradorToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem apontamentoToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem sairToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem escalasToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem escalasEspeciaisToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem gcmToolStripMenuItem;
    }
}