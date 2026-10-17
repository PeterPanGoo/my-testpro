namespace winform_antbli
{
    partial class MainForm
    {
        private System.ComponentModel.IContainer components = null;

        private AntdUI.PageHeader _windowHeader;
        private AntdUI.Panel _header;
        private AntdUI.Label _appTitle;
        private AntdUI.Label _appSubTitle;
        private AntdUI.Input _search;
        private AntdUI.Switch _themeSwitch;
        private AntdUI.Button _ghBtn;
        private AntdUI.Avatar _avatar;
        private AntdUI.Panel _sidebar;
        private AntdUI.Label _navTitle;
        private AntdUI.Menu _menu;
        private AntdUI.Panel _userCard;
        private AntdUI.Avatar _userAvatar;
        private AntdUI.Label _userName;
        private AntdUI.Label _userRole;
        private System.Windows.Forms.Panel _sidebarBorder;
        private System.Windows.Forms.Panel _content;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this._windowHeader = new AntdUI.PageHeader();
            this._header = new AntdUI.Panel();
            this._appTitle = new AntdUI.Label();
            this._appSubTitle = new AntdUI.Label();
            this._search = new AntdUI.Input();
            this._themeSwitch = new AntdUI.Switch();
            this._ghBtn = new AntdUI.Button();
            this._avatar = new AntdUI.Avatar();
            this._sidebar = new AntdUI.Panel();
            this._navTitle = new AntdUI.Label();
            this._menu = new AntdUI.Menu();
            this._userCard = new AntdUI.Panel();
            this._userAvatar = new AntdUI.Avatar();
            this._userName = new AntdUI.Label();
            this._userRole = new AntdUI.Label();
            this._sidebarBorder = new System.Windows.Forms.Panel();
            this._content = new System.Windows.Forms.Panel();
            this._header.SuspendLayout();
            this._sidebar.SuspendLayout();
            this._userCard.SuspendLayout();
            this.SuspendLayout();
            // 
            // _windowHeader
            // 
            this._windowHeader.CancelButton = true;
            this._windowHeader.DividerShow = true;
            this._windowHeader.Dock = System.Windows.Forms.DockStyle.Top;
            this._windowHeader.DragMove = true;
            this._windowHeader.EnableButtonTooltip = true;
            this._windowHeader.EnableDoubleClickMaximize = true;
            this._windowHeader.Height = 42;
            this._windowHeader.MaximizeBox = true;
            this._windowHeader.MinimizeBox = true;
            this._windowHeader.Name = "_windowHeader";
            this._windowHeader.ShowButton = true;
            this._windowHeader.ShowIcon = false;
            this._windowHeader.Size = new System.Drawing.Size(1280, 42);
            this._windowHeader.SubText = "WinForms";
            this._windowHeader.Text = "AntdUI 组件库";
            this._windowHeader.UseTextBold = true;
            this._windowHeader.UseTitleFont = false;
            // 
            // _header
            // 
            this._header.Controls.Add(this._appTitle);
            this._header.Controls.Add(this._appSubTitle);
            this._header.Controls.Add(this._search);
            this._header.Controls.Add(this._themeSwitch);
            this._header.Controls.Add(this._ghBtn);
            this._header.Controls.Add(this._avatar);
            this._header.Dock = System.Windows.Forms.DockStyle.Top;
            this._header.Height = 64;
            this._header.Name = "_header";
            this._header.Radius = 0;
            this._header.Shadow = 0;
            this._header.Size = new System.Drawing.Size(1280, 64);
            // 
            // _appTitle
            // 
            this._appTitle.AutoSize = true;
            this._appTitle.Font = new System.Drawing.Font("Microsoft YaHei UI", 13F, System.Drawing.FontStyle.Bold);
            this._appTitle.Location = new System.Drawing.Point(22, 13);
            this._appTitle.Name = "_appTitle";
            this._appTitle.Text = "AntdUI Studio";
            // 
            // _appSubTitle
            // 
            this._appSubTitle.AutoSize = true;
            this._appSubTitle.Font = new System.Drawing.Font("Microsoft YaHei UI", 8F);
            this._appSubTitle.Location = new System.Drawing.Point(180, 18);
            this._appSubTitle.Name = "_appSubTitle";
            this._appSubTitle.Text = "WinForms 组件工作台";
            // 
            // _search
            // 
            this._search.AllowClear = true;
            this._search.Location = new System.Drawing.Point(790, 14);
            this._search.Name = "_search";
            this._search.PlaceholderText = "搜索组件…";
            this._search.PrefixText = "搜索";
            this._search.Radius = 8;
            this._search.Size = new System.Drawing.Size(236, 36);
            // 
            // _themeSwitch
            // 
            this._themeSwitch.CheckedText = "深色";
            this._themeSwitch.Location = new System.Drawing.Point(1044, 19);
            this._themeSwitch.Name = "_themeSwitch";
            this._themeSwitch.Size = new System.Drawing.Size(64, 24);
            this._themeSwitch.UnCheckedText = "浅色";
            // 
            // _ghBtn
            // 
            this._ghBtn.Location = new System.Drawing.Point(1122, 14);
            this._ghBtn.Name = "_ghBtn";
            this._ghBtn.Radius = 8;
            this._ghBtn.Size = new System.Drawing.Size(92, 36);
            this._ghBtn.Text = "项目主页";
            // 
            // _avatar
            // 
            this._avatar.BackColor = System.Drawing.Color.FromArgb(22, 119, 255);
            this._avatar.Location = new System.Drawing.Point(1222, 14);
            this._avatar.Name = "_avatar";
            this._avatar.Round = true;
            this._avatar.Size = new System.Drawing.Size(36, 36);
            this._avatar.Text = "JJ";
            // 
            // _sidebar
            // 
            this._sidebar.Back = System.Drawing.Color.White;
            this._sidebar.Controls.Add(this._navTitle);
            this._sidebar.Controls.Add(this._menu);
            this._sidebar.Controls.Add(this._userCard);
            this._sidebar.Controls.Add(this._sidebarBorder);
            this._sidebar.Dock = System.Windows.Forms.DockStyle.Left;
            this._sidebar.Name = "_sidebar";
            this._sidebar.Radius = 0;
            this._sidebar.Shadow = 0;
            this._sidebar.Size = new System.Drawing.Size(232, 714);
            // 
            // _navTitle
            // 
            this._navTitle.AutoSize = true;
            this._navTitle.Font = new System.Drawing.Font("Microsoft YaHei UI", 8F);
            this._navTitle.Location = new System.Drawing.Point(24, 22);
            this._navTitle.Name = "_navTitle";
            this._navTitle.Text = "组件导航";
            // 
            // _menu
            // 
            this._menu.Indent = false;
            this._menu.Location = new System.Drawing.Point(12, 48);
            this._menu.Mode = AntdUI.TMenuMode.Inline;
            this._menu.Name = "_menu";
            this._menu.Radius = 8;
            this._menu.Size = new System.Drawing.Size(208, 574);
            // 
            // _userCard
            // 
            this._userCard.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
            this._userCard.Controls.Add(this._userAvatar);
            this._userCard.Controls.Add(this._userName);
            this._userCard.Controls.Add(this._userRole);
            this._userCard.Location = new System.Drawing.Point(12, 636);
            this._userCard.Name = "_userCard";
            this._userCard.Radius = 10;
            this._userCard.Size = new System.Drawing.Size(208, 64);
            // 
            // _userAvatar
            // 
            this._userAvatar.BackColor = System.Drawing.Color.FromArgb(114, 46, 209);
            this._userAvatar.Location = new System.Drawing.Point(10, 12);
            this._userAvatar.Name = "_userAvatar";
            this._userAvatar.Round = true;
            this._userAvatar.Size = new System.Drawing.Size(38, 38);
            this._userAvatar.Text = "管";
            // 
            // _userName
            // 
            this._userName.AutoSize = true;
            this._userName.Font = new System.Drawing.Font("Microsoft YaHei UI", 9.5F, System.Drawing.FontStyle.Bold);
            this._userName.Location = new System.Drawing.Point(58, 12);
            this._userName.Name = "_userName";
            this._userName.Text = "AntdUI 用户";
            // 
            // _userRole
            // 
            this._userRole.AutoSize = true;
            this._userRole.Font = new System.Drawing.Font("Microsoft YaHei UI", 8F);
            this._userRole.Location = new System.Drawing.Point(58, 34);
            this._userRole.Name = "_userRole";
            this._userRole.Text = "管理员 · Admin";
            // 
            // _sidebarBorder
            // 
            this._sidebarBorder.BackColor = System.Drawing.Color.FromArgb(232, 232, 232);
            this._sidebarBorder.Dock = System.Windows.Forms.DockStyle.Right;
            this._sidebarBorder.Location = new System.Drawing.Point(231, 0);
            this._sidebarBorder.Name = "_sidebarBorder";
            this._sidebarBorder.Size = new System.Drawing.Size(1, 714);
            // 
            // _content
            // 
            this._content.AutoScroll = true;
            this._content.BackColor = System.Drawing.Color.FromArgb(245, 247, 250);
            this._content.Dock = System.Windows.Forms.DockStyle.Fill;
            this._content.Name = "_content";
            this._content.Size = new System.Drawing.Size(1048, 714);
            // 
            // MainForm
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(1280, 820);
            this.Controls.Add(this._content);
            this.Controls.Add(this._sidebar);
            this.Controls.Add(this._header);
            this.Controls.Add(this._windowHeader);
            this.Font = new System.Drawing.Font("Microsoft YaHei UI", 9F);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.MinimumSize = new System.Drawing.Size(1000, 660);
            this.Name = "MainForm";
            this.Resizable = true;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "AntdUI 组件库 · WinForms";
            this._header.ResumeLayout(false);
            this._header.PerformLayout();
            this._userCard.ResumeLayout(false);
            this._userCard.PerformLayout();
            this._sidebar.ResumeLayout(false);
            this._sidebar.PerformLayout();
            this.ResumeLayout(false);
        }
    }
}
