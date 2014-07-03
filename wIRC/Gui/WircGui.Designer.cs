namespace wIRC.Gui
{
    partial class WIrcGui
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
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.statusLabel1 = new System.Windows.Forms.ToolStripStatusLabel();
            this.contentContainer = new System.Windows.Forms.SplitContainer();
            this.output = new System.Windows.Forms.TextBox();
            this.nickList = new System.Windows.Forms.ListView();
            this.viewContainer = new System.Windows.Forms.SplitContainer();
            this.textInput = new System.Windows.Forms.TextBox();
            this.statusStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.contentContainer)).BeginInit();
            this.contentContainer.Panel1.SuspendLayout();
            this.contentContainer.Panel2.SuspendLayout();
            this.contentContainer.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.viewContainer)).BeginInit();
            this.viewContainer.Panel1.SuspendLayout();
            this.viewContainer.Panel2.SuspendLayout();
            this.viewContainer.SuspendLayout();
            this.SuspendLayout();
            // 
            // statusStrip1
            // 
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.statusLabel1});
            this.statusStrip1.Location = new System.Drawing.Point(0, 464);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(780, 22);
            this.statusStrip1.TabIndex = 0;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // statusLabel1
            // 
            this.statusLabel1.Name = "statusLabel1";
            this.statusLabel1.Size = new System.Drawing.Size(34, 17);
            this.statusLabel1.Text = "none";
            // 
            // contentContainer
            // 
            this.contentContainer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.contentContainer.Location = new System.Drawing.Point(0, 0);
            this.contentContainer.Name = "contentContainer";
            // 
            // contentContainer.Panel1
            // 
            this.contentContainer.Panel1.Controls.Add(this.output);
            this.contentContainer.Panel1.RightToLeft = System.Windows.Forms.RightToLeft.No;
            // 
            // contentContainer.Panel2
            // 
            this.contentContainer.Panel2.Controls.Add(this.nickList);
            this.contentContainer.Panel2.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.contentContainer.Size = new System.Drawing.Size(780, 435);
            this.contentContainer.SplitterDistance = 633;
            this.contentContainer.SplitterWidth = 6;
            this.contentContainer.TabIndex = 1;
            // 
            // output
            // 
            this.output.Dock = System.Windows.Forms.DockStyle.Fill;
            this.output.Font = new System.Drawing.Font("Consolas", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.output.Location = new System.Drawing.Point(0, 0);
            this.output.Multiline = true;
            this.output.Name = "output";
            this.output.ReadOnly = true;
            this.output.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.output.Size = new System.Drawing.Size(633, 435);
            this.output.TabIndex = 0;
            // 
            // nickList
            // 
            this.nickList.Dock = System.Windows.Forms.DockStyle.Fill;
            this.nickList.Location = new System.Drawing.Point(0, 0);
            this.nickList.Name = "nickList";
            this.nickList.Size = new System.Drawing.Size(141, 435);
            this.nickList.TabIndex = 0;
            this.nickList.UseCompatibleStateImageBehavior = false;
            this.nickList.View = System.Windows.Forms.View.List;
            // 
            // viewContainer
            // 
            this.viewContainer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.viewContainer.IsSplitterFixed = true;
            this.viewContainer.Location = new System.Drawing.Point(0, 0);
            this.viewContainer.Name = "viewContainer";
            this.viewContainer.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // viewContainer.Panel1
            // 
            this.viewContainer.Panel1.Controls.Add(this.contentContainer);
            // 
            // viewContainer.Panel2
            // 
            this.viewContainer.Panel2.Controls.Add(this.textInput);
            this.viewContainer.Size = new System.Drawing.Size(780, 464);
            this.viewContainer.SplitterDistance = 435;
            this.viewContainer.TabIndex = 0;
            // 
            // textInput
            // 
            this.textInput.Dock = System.Windows.Forms.DockStyle.Fill;
            this.textInput.Location = new System.Drawing.Point(0, 0);
            this.textInput.Name = "textInput";
            this.textInput.Size = new System.Drawing.Size(780, 20);
            this.textInput.TabIndex = 0;
            this.textInput.KeyDown += new System.Windows.Forms.KeyEventHandler(this.textInput_KeyDown);
            this.textInput.KeyUp += new System.Windows.Forms.KeyEventHandler(this.textInput_KeyUp);
            // 
            // WIrcGui
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(780, 486);
            this.Controls.Add(this.viewContainer);
            this.Controls.Add(this.statusStrip1);
            this.Name = "WIrcGui";
            this.Text = "wIRC";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.WIrcGui_FormClosing);
            this.Load += new System.EventHandler(this.WIrcGui_Load);
            this.Shown += new System.EventHandler(this.WIrcGui_Shown);
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.contentContainer.Panel1.ResumeLayout(false);
            this.contentContainer.Panel1.PerformLayout();
            this.contentContainer.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.contentContainer)).EndInit();
            this.contentContainer.ResumeLayout(false);
            this.viewContainer.Panel1.ResumeLayout(false);
            this.viewContainer.Panel2.ResumeLayout(false);
            this.viewContainer.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.viewContainer)).EndInit();
            this.viewContainer.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripStatusLabel statusLabel1;
        private System.Windows.Forms.SplitContainer contentContainer;
        private System.Windows.Forms.TextBox output;
        private System.Windows.Forms.ListView nickList;
        private System.Windows.Forms.SplitContainer viewContainer;
        private System.Windows.Forms.TextBox textInput;
    }
}