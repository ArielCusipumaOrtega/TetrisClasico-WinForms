namespace TetrisApp
{
    partial class GameForm
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            picTablero = new PictureBox();
            pnlLateral = new Panel();
            lblSigTitulo = new Label();
            picSiguiente = new PictureBox();
            lblScore = new Label();
            lblLineas = new Label();
            lblNivel = new Label();
            btnIniciar = new Button();
            btnPausa = new Button();
            ((System.ComponentModel.ISupportInitialize)picTablero).BeginInit();
            pnlLateral.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)picSiguiente).BeginInit();
            SuspendLayout();
            // 
            // picTablero
            // 
            picTablero.BackColor = Color.FromArgb(17, 19, 24);
            picTablero.BorderStyle = BorderStyle.FixedSingle;
            picTablero.Location = new Point(10, 10);
            picTablero.Name = "picTablero";
            picTablero.Size = new Size(300, 512);
            picTablero.TabIndex = 0;
            picTablero.TabStop = false;
            // 
            // pnlLateral
            // 
            pnlLateral.BackColor = Color.FromArgb(33, 37, 47);
            pnlLateral.Controls.Add(lblSigTitulo);
            pnlLateral.Controls.Add(picSiguiente);
            pnlLateral.Controls.Add(lblScore);
            pnlLateral.Controls.Add(lblLineas);
            pnlLateral.Controls.Add(lblNivel);
            pnlLateral.Controls.Add(btnIniciar);
            pnlLateral.Controls.Add(btnPausa);
            pnlLateral.Location = new Point(316, 10);
            pnlLateral.Name = "pnlLateral";
            pnlLateral.Size = new Size(180, 512);
            pnlLateral.TabIndex = 1;
            // 
            // lblSigTitulo
            // 
            lblSigTitulo.AutoSize = true;
            lblSigTitulo.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            lblSigTitulo.ForeColor = Color.FromArgb(148, 163, 184);
            lblSigTitulo.Location = new Point(25, 20);
            lblSigTitulo.Name = "lblSigTitulo";
            lblSigTitulo.Size = new Size(121, 19);
            lblSigTitulo.TabIndex = 0;
            lblSigTitulo.Text = "SIGUIENTE PIEZA";
            // 
            // picSiguiente
            // 
            picSiguiente.BackColor = Color.FromArgb(17, 19, 24);
            picSiguiente.BorderStyle = BorderStyle.FixedSingle;
            picSiguiente.Location = new Point(30, 50);
            picSiguiente.Name = "picSiguiente";
            picSiguiente.Size = new Size(120, 120);
            picSiguiente.TabIndex = 1;
            picSiguiente.TabStop = false;
            // 
            // lblScore
            // 
            lblScore.AutoSize = true;
            lblScore.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            lblScore.ForeColor = Color.FromArgb(56, 189, 248);
            lblScore.Location = new Point(15, 200);
            lblScore.Name = "lblScore";
            lblScore.Size = new Size(115, 21);
            lblScore.TabIndex = 2;
            lblScore.Text = "Puntuación: 0";
            // 
            // lblLineas
            // 
            lblLineas.AutoSize = true;
            lblLineas.Font = new Font("Segoe UI", 11F);
            lblLineas.ForeColor = Color.FromArgb(74, 222, 128);
            lblLineas.Location = new Point(15, 240);
            lblLineas.Name = "lblLineas";
            lblLineas.Size = new Size(65, 20);
            lblLineas.TabIndex = 3;
            lblLineas.Text = "Líneas: 0";
            // 
            // lblNivel
            // 
            lblNivel.AutoSize = true;
            lblNivel.Font = new Font("Segoe UI", 11F);
            lblNivel.ForeColor = Color.FromArgb(251, 191, 36);
            lblNivel.Location = new Point(15, 280);
            lblNivel.Name = "lblNivel";
            lblNivel.Size = new Size(58, 20);
            lblNivel.TabIndex = 4;
            lblNivel.Text = "Nivel: 1";
            // 
            // btnIniciar
            // 
            btnIniciar.BackColor = Color.FromArgb(2, 132, 199);
            btnIniciar.FlatAppearance.BorderSize = 0;
            btnIniciar.FlatStyle = FlatStyle.Flat;
            btnIniciar.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            btnIniciar.ForeColor = Color.White;
            btnIniciar.Location = new Point(15, 383);
            btnIniciar.Name = "btnIniciar";
            btnIniciar.Size = new Size(150, 40);
            btnIniciar.TabIndex = 5;
            btnIniciar.Text = "Iniciar Juego";
            btnIniciar.UseVisualStyleBackColor = false;
            btnIniciar.Click += btnIniciar_Click;
            // 
            // btnPausa
            // 
            btnPausa.BackColor = Color.FromArgb(71, 85, 105);
            btnPausa.FlatAppearance.BorderSize = 0;
            btnPausa.FlatStyle = FlatStyle.Flat;
            btnPausa.Font = new Font("Segoe UI", 9F);
            btnPausa.ForeColor = Color.White;
            btnPausa.Location = new Point(15, 447);
            btnPausa.Name = "btnPausa";
            btnPausa.Size = new Size(150, 35);
            btnPausa.TabIndex = 6;
            btnPausa.Text = "Pausar (P)";
            btnPausa.UseVisualStyleBackColor = false;
            btnPausa.Click += btnPausa_Click;
            // 
            // GameForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.FromArgb(24, 26, 32);
            ClientSize = new Size(504, 533);
            Controls.Add(pnlLateral);
            Controls.Add(picTablero);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            Name = "GameForm";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Tetris Clásico";
            ((System.ComponentModel.ISupportInitialize)picTablero).EndInit();
            pnlLateral.ResumeLayout(false);
            pnlLateral.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)picSiguiente).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private PictureBox picTablero;
        private Panel pnlLateral;
        private Label lblSigTitulo;
        private PictureBox picSiguiente;
        private Label lblScore;
        private Label lblLineas;
        private Label lblNivel;
        private Button btnIniciar;
        private Button btnPausa;
    }
}
