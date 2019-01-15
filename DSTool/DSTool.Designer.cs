namespace DSTool
{
    partial class MSTool
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MSTool));
            this.btn_Brand = new System.Windows.Forms.Button();
            this.btn_Area = new System.Windows.Forms.Button();
            this.btn_Dept = new System.Windows.Forms.Button();
            this.btn_Unit = new System.Windows.Forms.Button();
            this.btn_DishType = new System.Windows.Forms.Button();
            this.btn_Dish = new System.Windows.Forms.Button();
            this.btn_OrderMain = new System.Windows.Forms.Button();
            this.btn_OrderDetail = new System.Windows.Forms.Button();
            this.btn_Store = new System.Windows.Forms.Button();
            this.rtxt_message = new System.Windows.Forms.RichTextBox();
            this.btn_MealTime = new System.Windows.Forms.Button();
            this.btn_User = new System.Windows.Forms.Button();
            this.btn_Payway = new System.Windows.Forms.Button();
            this.notifyIcon1 = new System.Windows.Forms.NotifyIcon(this.components);
            this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.display = new System.Windows.Forms.ToolStripMenuItem();
            this.exit = new System.Windows.Forms.ToolStripMenuItem();
            this.label1 = new System.Windows.Forms.Label();
            this.contextMenuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // btn_Brand
            // 
            this.btn_Brand.Location = new System.Drawing.Point(653, 223);
            this.btn_Brand.Name = "btn_Brand";
            this.btn_Brand.Size = new System.Drawing.Size(75, 23);
            this.btn_Brand.TabIndex = 0;
            this.btn_Brand.Text = "品牌";
            this.btn_Brand.UseVisualStyleBackColor = true;
            this.btn_Brand.Click += new System.EventHandler(this.btn_Brand_Click);
            // 
            // btn_Area
            // 
            this.btn_Area.Location = new System.Drawing.Point(653, 252);
            this.btn_Area.Name = "btn_Area";
            this.btn_Area.Size = new System.Drawing.Size(75, 23);
            this.btn_Area.TabIndex = 1;
            this.btn_Area.Text = "区域";
            this.btn_Area.UseVisualStyleBackColor = true;
            this.btn_Area.Click += new System.EventHandler(this.btn_Area_Click);
            // 
            // btn_Dept
            // 
            this.btn_Dept.Location = new System.Drawing.Point(653, 339);
            this.btn_Dept.Name = "btn_Dept";
            this.btn_Dept.Size = new System.Drawing.Size(75, 23);
            this.btn_Dept.TabIndex = 2;
            this.btn_Dept.Text = "部门";
            this.btn_Dept.UseVisualStyleBackColor = true;
            this.btn_Dept.Click += new System.EventHandler(this.btn_Dept_Click);
            // 
            // btn_Unit
            // 
            this.btn_Unit.Location = new System.Drawing.Point(653, 397);
            this.btn_Unit.Name = "btn_Unit";
            this.btn_Unit.Size = new System.Drawing.Size(75, 23);
            this.btn_Unit.TabIndex = 3;
            this.btn_Unit.Text = "品项单位";
            this.btn_Unit.UseVisualStyleBackColor = true;
            this.btn_Unit.Click += new System.EventHandler(this.btn_Unit_Click);
            // 
            // btn_DishType
            // 
            this.btn_DishType.Location = new System.Drawing.Point(653, 368);
            this.btn_DishType.Name = "btn_DishType";
            this.btn_DishType.Size = new System.Drawing.Size(75, 23);
            this.btn_DishType.TabIndex = 4;
            this.btn_DishType.Text = "品项类型";
            this.btn_DishType.UseVisualStyleBackColor = true;
            this.btn_DishType.Click += new System.EventHandler(this.btn_DishType_Click);
            // 
            // btn_Dish
            // 
            this.btn_Dish.Location = new System.Drawing.Point(653, 426);
            this.btn_Dish.Name = "btn_Dish";
            this.btn_Dish.Size = new System.Drawing.Size(75, 23);
            this.btn_Dish.TabIndex = 5;
            this.btn_Dish.Text = "品项";
            this.btn_Dish.UseVisualStyleBackColor = true;
            this.btn_Dish.Click += new System.EventHandler(this.btn_Dish_Click);
            // 
            // btn_OrderMain
            // 
            this.btn_OrderMain.Location = new System.Drawing.Point(653, 455);
            this.btn_OrderMain.Name = "btn_OrderMain";
            this.btn_OrderMain.Size = new System.Drawing.Size(75, 23);
            this.btn_OrderMain.TabIndex = 6;
            this.btn_OrderMain.Text = "订单";
            this.btn_OrderMain.UseVisualStyleBackColor = true;
            this.btn_OrderMain.Click += new System.EventHandler(this.btn_OrderMain_Click);
            // 
            // btn_OrderDetail
            // 
            this.btn_OrderDetail.Location = new System.Drawing.Point(653, 484);
            this.btn_OrderDetail.Name = "btn_OrderDetail";
            this.btn_OrderDetail.Size = new System.Drawing.Size(75, 23);
            this.btn_OrderDetail.TabIndex = 7;
            this.btn_OrderDetail.Text = "订单项";
            this.btn_OrderDetail.UseVisualStyleBackColor = true;
            this.btn_OrderDetail.Visible = false;
            this.btn_OrderDetail.Click += new System.EventHandler(this.btn_OrderDetail_Click);
            // 
            // btn_Store
            // 
            this.btn_Store.Location = new System.Drawing.Point(653, 281);
            this.btn_Store.Name = "btn_Store";
            this.btn_Store.Size = new System.Drawing.Size(75, 23);
            this.btn_Store.TabIndex = 8;
            this.btn_Store.Text = "门店";
            this.btn_Store.UseVisualStyleBackColor = true;
            this.btn_Store.Click += new System.EventHandler(this.btn_Store_Click);
            // 
            // rtxt_message
            // 
            this.rtxt_message.Location = new System.Drawing.Point(5, 186);
            this.rtxt_message.Name = "rtxt_message";
            this.rtxt_message.Size = new System.Drawing.Size(637, 390);
            this.rtxt_message.TabIndex = 9;
            this.rtxt_message.Text = "";
            this.rtxt_message.TextChanged += new System.EventHandler(this.rtxt_message_TextChanged);
            // 
            // btn_MealTime
            // 
            this.btn_MealTime.Location = new System.Drawing.Point(653, 310);
            this.btn_MealTime.Name = "btn_MealTime";
            this.btn_MealTime.Size = new System.Drawing.Size(75, 23);
            this.btn_MealTime.TabIndex = 10;
            this.btn_MealTime.Text = "餐段";
            this.btn_MealTime.UseVisualStyleBackColor = true;
            this.btn_MealTime.Click += new System.EventHandler(this.btn_MealTime_Click);
            // 
            // btn_User
            // 
            this.btn_User.Location = new System.Drawing.Point(653, 513);
            this.btn_User.Name = "btn_User";
            this.btn_User.Size = new System.Drawing.Size(75, 23);
            this.btn_User.TabIndex = 11;
            this.btn_User.Text = "用户";
            this.btn_User.UseVisualStyleBackColor = true;
            this.btn_User.Visible = false;
            this.btn_User.Click += new System.EventHandler(this.btn_User_Click);
            // 
            // btn_Payway
            // 
            this.btn_Payway.Location = new System.Drawing.Point(653, 542);
            this.btn_Payway.Name = "btn_Payway";
            this.btn_Payway.Size = new System.Drawing.Size(75, 23);
            this.btn_Payway.TabIndex = 12;
            this.btn_Payway.Text = "支付方式";
            this.btn_Payway.UseVisualStyleBackColor = true;
            this.btn_Payway.Visible = false;
            this.btn_Payway.Click += new System.EventHandler(this.btn_Payway_Click);
            // 
            // notifyIcon1
            // 
            this.notifyIcon1.ContextMenuStrip = this.contextMenuStrip1;
            this.notifyIcon1.Icon = ((System.Drawing.Icon)(resources.GetObject("notifyIcon1.Icon")));
            this.notifyIcon1.Text = "同步工具";
            this.notifyIcon1.Visible = true;
            this.notifyIcon1.MouseClick += new System.Windows.Forms.MouseEventHandler(this.notifyIcon1_MouseClick);
            // 
            // contextMenuStrip1
            // 
            this.contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.display,
            this.exit});
            this.contextMenuStrip1.Name = "contextMenuStrip1";
            this.contextMenuStrip1.Size = new System.Drawing.Size(101, 48);
            // 
            // display
            // 
            this.display.Name = "display";
            this.display.Size = new System.Drawing.Size(100, 22);
            this.display.Text = "显示";
            this.display.Click += new System.EventHandler(this.display_Click);
            // 
            // exit
            // 
            this.exit.Name = "exit";
            this.exit.Size = new System.Drawing.Size(100, 22);
            this.exit.Text = "退出";
            this.exit.Click += new System.EventHandler(this.exit_Click);
            // 
            // label1
            // 
            this.label1.Location = new System.Drawing.Point(5, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(637, 174);
            this.label1.TabIndex = 14;
            // 
            // MSTool
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(740, 580);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.btn_Payway);
            this.Controls.Add(this.btn_User);
            this.Controls.Add(this.btn_MealTime);
            this.Controls.Add(this.rtxt_message);
            this.Controls.Add(this.btn_Store);
            this.Controls.Add(this.btn_OrderDetail);
            this.Controls.Add(this.btn_OrderMain);
            this.Controls.Add(this.btn_Dish);
            this.Controls.Add(this.btn_DishType);
            this.Controls.Add(this.btn_Unit);
            this.Controls.Add(this.btn_Dept);
            this.Controls.Add(this.btn_Area);
            this.Controls.Add(this.btn_Brand);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.Name = "MSTool";
            this.Text = "MSTool";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MSTool_FormClosing);
            this.Load += new System.EventHandler(this.MSTool_Load);
            this.contextMenuStrip1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button btn_Brand;
        private System.Windows.Forms.Button btn_Area;
        private System.Windows.Forms.Button btn_Dept;
        private System.Windows.Forms.Button btn_Unit;
        private System.Windows.Forms.Button btn_DishType;
        private System.Windows.Forms.Button btn_Dish;
        private System.Windows.Forms.Button btn_OrderMain;
        private System.Windows.Forms.Button btn_OrderDetail;
        private System.Windows.Forms.Button btn_Store;
        private System.Windows.Forms.RichTextBox rtxt_message;
        private System.Windows.Forms.Button btn_MealTime;
        private System.Windows.Forms.Button btn_User;
        private System.Windows.Forms.Button btn_Payway;
        private System.Windows.Forms.NotifyIcon notifyIcon1;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
        private System.Windows.Forms.ToolStripMenuItem display;
        private System.Windows.Forms.ToolStripMenuItem exit;
        private System.Windows.Forms.Label label1;
    }
}

