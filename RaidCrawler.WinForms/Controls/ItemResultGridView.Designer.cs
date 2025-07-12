namespace RaidCrawler.WinForms.Controls
{
    partial class ItemResultGridView
    {
        /// <summary> 
        /// 必要なデザイナー変数です。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// 使用中のリソースをすべてクリーンアップします。
        /// </summary>
        /// <param name="disposing">マネージド リソースを破棄する場合は true を指定し、その他の場合は false を指定します。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region コンポーネント デザイナーで生成されたコード

        /// <summary> 
        /// デザイナー サポートに必要なメソッドです。このメソッドの内容を 
        /// コード エディターで変更しないでください。
        /// </summary>
        private void InitializeComponent()
        {
            DataGridViewCellStyle dataGridViewCellStyle1 = new DataGridViewCellStyle();
            DGV_View = new DataGridView();
            Count = new DataGridViewTextBoxColumn();
            IMG = new DataGridViewImageColumn();
            ItemName = new DataGridViewTextBoxColumn();
            ((System.ComponentModel.ISupportInitialize)DGV_View).BeginInit();
            SuspendLayout();
            // 
            // DGV_View
            // 
            DGV_View.AllowUserToAddRows = false;
            DGV_View.AllowUserToDeleteRows = false;
            DGV_View.AllowUserToResizeColumns = false;
            DGV_View.AllowUserToResizeRows = false;
            DGV_View.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            DGV_View.Columns.AddRange(new DataGridViewColumn[] { Count, IMG, ItemName });
            DGV_View.Dock = DockStyle.Fill;
            DGV_View.Location = new Point(0, 0);
            DGV_View.Name = "DGV_View";
            DGV_View.RowHeadersVisible = false;
            DGV_View.ScrollBars = ScrollBars.None;
            DGV_View.Size = new Size(235, 250);
            DGV_View.TabIndex = 0;
            // 
            // Count
            // 
            Count.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            dataGridViewCellStyle1.Alignment = DataGridViewContentAlignment.MiddleCenter;
            Count.DefaultCellStyle = dataGridViewCellStyle1;
            Count.HeaderText = "Count";
            Count.Name = "Count";
            Count.ReadOnly = true;
            Count.Width = 64;
            // 
            // IMG
            // 
            IMG.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            IMG.HeaderText = "IMG";
            IMG.Name = "IMG";
            IMG.ReadOnly = true;
            IMG.Width = 35;
            // 
            // ItemName
            // 
            ItemName.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            ItemName.HeaderText = "Name";
            ItemName.Name = "ItemName";
            ItemName.ReadOnly = true;
            // 
            // ItemResultGridView
            // 
            AutoScaleMode = AutoScaleMode.Inherit;
            AutoSizeMode = AutoSizeMode.GrowAndShrink;
            Controls.Add(DGV_View);
            Name = "ItemResultGridView";
            Size = new Size(235, 250);
            ((System.ComponentModel.ISupportInitialize)DGV_View).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private DataGridView DGV_View;
        private DataGridViewTextBoxColumn Count;
        private DataGridViewImageColumn IMG;
        private DataGridViewTextBoxColumn ItemName;
    }
}
