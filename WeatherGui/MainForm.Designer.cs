namespace WeatherGui
{
    partial class MainForm
    {
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.ComboBox comboCities;
        private System.Windows.Forms.Button btnGetWeather;
        private System.Windows.Forms.Label lblResult;

        /// <summary>
        /// освободить используемые ресурсы
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) components.Dispose();
            base.Dispose(disposing);
        }

        #region Код, автоматически созданный конструктором форм Windows

        /// <summary>
        /// инициализация компонентов формы
        /// </summary>
        private void InitializeComponent()
        {
            this.comboCities = new System.Windows.Forms.ComboBox();
            this.btnGetWeather = new System.Windows.Forms.Button();
            this.lblResult = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // comboCities
            // 
            this.comboCities.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboCities.FormattingEnabled = true;
            this.comboCities.Location = new System.Drawing.Point(12, 12);
            this.comboCities.Name = "comboCities";
            this.comboCities.Size = new System.Drawing.Size(360, 23);
            this.comboCities.TabIndex = 0;
            // 
            // btnGetWeather
            // 
            this.btnGetWeather.Location = new System.Drawing.Point(378, 12);
            this.btnGetWeather.Name = "btnGetWeather";
            this.btnGetWeather.Size = new System.Drawing.Size(150, 30);
            this.btnGetWeather.TabIndex = 1;
            this.btnGetWeather.Text = "Получить погоду";
            this.btnGetWeather.UseVisualStyleBackColor = true;
            this.btnGetWeather.Click += new System.EventHandler(this.btnGetWeather_Click);
            // 
            // lblResult
            // 
            this.lblResult.AutoSize = true;
            this.lblResult.Location = new System.Drawing.Point(12, 55);
            this.lblResult.Name = "lblResult";
            this.lblResult.Size = new System.Drawing.Size(222, 15);
            this.lblResult.TabIndex = 2;
            this.lblResult.Text = "Выберите город и нажмите кнопку";
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(540, 95);
            this.Controls.Add(this.lblResult);
            this.Controls.Add(this.btnGetWeather);
            this.Controls.Add(this.comboCities);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.Name = "MainForm";
            this.Text = "Погода (OpenWeatherMap)";
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        #endregion
    }
}
