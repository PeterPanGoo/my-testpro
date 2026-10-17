using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using AntdUI;
using Message = AntdUI.Message;

namespace winform_antbli
{
    /// <summary>
    /// AntdUI 组件展示 · 组件画廊。
    /// 左侧导航切换组件分类页面，顶部搜索可过滤分类，右上角开关可切换深/浅主题。
    /// </summary>
    public partial class MainForm : AntdUI.Window
    {
        // ---------- 主题配色（浅色 / 深色） ----------
        Color _pageBg;          // 内容区背景
        Color _cardBg;          // 卡片背景
        Color _textPrimary;     // 主文字
        Color _textSecondary;   // 次要文字
        Color _divider;         // 分割线

        // 强调色
        static readonly Color Blue = Color.FromArgb(22, 119, 255);
        static readonly Color Green = Color.FromArgb(82, 196, 26);
        static readonly Color Orange = Color.FromArgb(250, 140, 22);
        static readonly Color Purple = Color.FromArgb(114, 46, 209);
        static readonly Color Red = Color.FromArgb(245, 34, 45);
        static readonly Color Cyan = Color.FromArgb(19, 194, 194);

        // ---------- 运行时页面控件 ----------
        AntdUI.Panel _heroCard;

        // 需要随主题切换颜色的对象
        readonly List<AntdUI.Panel> _cards = new List<AntdUI.Panel>();
        readonly List<AntdUI.Label> _titleLabels = new List<AntdUI.Label>();
        readonly List<AntdUI.Label> _primaryLabels = new List<AntdUI.Label>();
        readonly List<AntdUI.Label> _mutedLabels = new List<AntdUI.Label>();

        // 需要随卡片宽度拉伸的控件（左/右各留 pad 边距）
        class Stretch { public AntdUI.Panel Card; public Control Control; public int Pad; }
        readonly List<Stretch> _stretchControls = new List<Stretch>();
        readonly List<AntdUI.Button> _quickNavButtons = new List<AntdUI.Button>();

        // 布局行：每行若干卡片（12 栅格），行高固定
        class CardLayout { public AntdUI.Panel Panel; public int Span; }
        class RowLayout { public int Height; public List<CardLayout> Cards = new List<CardLayout>(); }

        // 页面：一个页面 = 若干布局行
        class Page
        {
            public string Key;
            public List<RowLayout> Rows = new List<RowLayout>();
        }
        readonly Dictionary<string, Page> _pages = new Dictionary<string, Page>();
        string _activeKey = "overview";

        // 侧边栏导航项（用于搜索过滤与选中态）
        class NavItem { public string Key; public string Keywords; public AntdUI.MenuItem Item; }
        readonly List<NavItem> _navItems = new List<NavItem>();

        readonly Font _baseFont = new Font("Microsoft YaHei UI", 9F);

        // VS 设计器使用此构造函数，只加载 Designer.cs 中的固定框架。
        public MainForm()
        {
            AntdUI.Style.SetPrimary(Blue);
            InitializeComponent();
        }

        // 程序运行时使用此构造函数，再填充菜单、页面和交互事件。
        public MainForm(string[] args)
            : this()
        {
            InitializeRuntimeUi();
            BuildPages();
            ApplyTheme(false);

            // 支持 --page <key> 指定启动页、--shot <path> 自截屏后退出（便于调试/演示）
            string start = "overview";
            string shot = null;
            if (args != null)
                for (int i = 0; i < args.Length - 1; i++)
                {
                    if (args[i] == "--page") start = args[i + 1];
                    else if (args[i] == "--shot") shot = args[i + 1];
                }
            SwitchPage(start);

            Resize += (s, e) => LayoutContent();
            Shown += (s, e) => LayoutContent();   // 停靠布局最终确定后再算一次

            if (shot != null) SetupSelfShot(shot);
        }

        void InitializeRuntimeUi()
        {
            _titleLabels.Add(_appTitle);
            _titleLabels.Add(_userName);
            _mutedLabels.Add(_appSubTitle);
            _mutedLabels.Add(_navTitle);
            _mutedLabels.Add(_userRole);

            AddNav("overview", "概览", "overview 首页 统计 指标 概览");
            AddNav("button", "按钮", "button 按钮");
            AddNav("form", "表单", "form 表单 input 输入 select 选择 switch 开关 checkbox 单选 rate 评分");
            AddNav("data", "数据展示", "data 表格 table 标签 tag 徽标 badge 头像 avatar 时间线 timeline");
            AddNav("chart", "图表", "chart 图表 折线 柱状 饼图 环形 雷达 line bar pie radar");
            AddNav("feedback", "反馈", "feedback 提示 alert 进度 progress 消息 message 通知 notification 弹窗 modal 抽屉 drawer");
            AddNav("navigation", "导航", "navigation 菜单 menu 面包屑 breadcrumb 分页 pagination 步骤 steps 分段 segmented 标签页 tabs");

            _menu.ItemClick += (s, e) =>
            {
                if (e.Item.Tag is string key) SwitchPage(key);
            };
            _ghBtn.Click += (s, e) => Message.info(this, "打开 GitHub 仓库", null, null);
            _themeSwitch.CheckedChanged += (s, e) => ApplyTheme(_themeSwitch.Checked);
            _search.TextChanged += (s, e) => FilterMenu(_search.Text);
            _search.KeyDown += (s, e) =>
            {
                if (e.KeyCode == Keys.Enter) OpenFirstVisible();
            };
            _header.Resize += (s, e) => LayoutHeaderRight();
            _appTitle.SizeChanged += (s, e) => LayoutHeaderRight();
            _sidebar.Resize += (s, e) => LayoutSidebar();

            _windowHeader.BringToFront();
            _header.BringToFront();
            _sidebar.BringToFront();
            _content.BringToFront();
            LayoutHeaderRight();
            LayoutSidebar();
        }

        // 调试用：直接渲染窗体到位图，避免被其它窗口遮挡或受屏幕捕获权限影响
        void SetupSelfShot(string path)
        {
            TopMost = true;
            var timer = new System.Windows.Forms.Timer { Interval = 1600 };
            timer.Tick += (s, e) =>
            {
                timer.Stop();
                try
                {
                    using (var bmp = new Bitmap(Width, Height))
                    {
                        DrawToBitmap(bmp, new Rectangle(0, 0, Width, Height));
                        bmp.Save(path, System.Drawing.Imaging.ImageFormat.Png);
                    }
                }
                catch { }
                Close();
            };
            timer.Start();
        }

        // 把右上角控件排到窗口按钮（最小化/最大化/关闭）的左侧
        void LayoutHeaderRight()
        {
            if (_header == null || _avatar == null) return;
            int cy = 14;
            _appTitle.Location = new Point(22, 13);
            _appSubTitle.Location = new Point(_appTitle.Right + 12, 18);
            int right = Math.Max(300, _header.ClientSize.Width - 22);
            _avatar.Location = new Point(right - 36, cy);
            _ghBtn.Location = new Point(_avatar.Left - 10 - 92, cy);
            _themeSwitch.Location = new Point(_ghBtn.Left - 14 - 64, cy + 5);
            _search.Location = new Point(_themeSwitch.Left - 18 - 236, cy);

            bool compact = _header.ClientSize.Width < 1080;
            _ghBtn.Visible = !compact;
            _avatar.Visible = !compact;
            _search.Visible = _header.ClientSize.Width >= 900;
        }

        void LayoutSidebar()
        {
            if (_sidebar == null || _menu == null || _userCard == null) return;
            int userTop = _sidebar.ClientSize.Height - _userCard.Height - 14;
            _userCard.Location = new Point(12, userTop);
            _menu.Size = new Size(_sidebar.ClientSize.Width - 24, Math.Max(120, userTop - _menu.Top - 16));
        }

        void AddNav(string key, string title, string keywords)
        {
            var item = new AntdUI.MenuItem { Text = title, Tag = key };
            _menu.Items.Add(item);
            _navItems.Add(new NavItem { Key = key, Keywords = keywords.ToLowerInvariant(), Item = item });
        }

        // ============================================================
        // 页面注册
        // ============================================================
        void BuildPages()
        {
            Register(BuildOverviewPage());
            Register(BuildButtonPage());
            Register(BuildFormPage());
            Register(BuildDataPage());
            Register(BuildChartPage());
            Register(BuildFeedbackPage());
            Register(BuildNavigationPage());
        }

        Page NewPage(string key)
        {
            var p = new Page { Key = key };
            _pages[key] = p;
            return p;
        }

        void Register(Page page)
        {
            foreach (var row in page.Rows)
                foreach (var c in row.Cards)
                    _content.Controls.Add(c.Panel);
        }

        void AddRow(Page page, int height, params CardLayout[] cards)
        {
            var row = new RowLayout { Height = height };
            row.Cards.AddRange(cards);
            page.Rows.Add(row);
        }

        CardLayout CL(AntdUI.Panel panel, int span)
        {
            return new CardLayout { Panel = panel, Span = span };
        }

        // ============================================================
        // 页面切换与搜索
        // ============================================================
        void SwitchPage(string key)
        {
            if (!_pages.ContainsKey(key)) key = "overview";
            _activeKey = key;

            foreach (var kv in _pages)
            {
                bool active = kv.Key == key;
                foreach (var row in kv.Value.Rows)
                    foreach (var c in row.Cards)
                        c.Panel.Visible = active;
            }

            foreach (var n in _navItems)
                n.Item.Select = n.Key == key;

            LayoutContent();
        }

        void FilterMenu(string text)
        {
            string q = (text ?? "").Trim().ToLowerInvariant();
            int visible = 0;
            foreach (var n in _navItems)
            {
                bool show = q.Length == 0 || n.Keywords.Contains(q) || n.Item.Text.ToLowerInvariant().Contains(q);
                n.Item.Visible = show;
                if (show) visible++;
            }
            _navTitle.Text = q.Length == 0 ? "组件导航" : "搜索结果 · " + visible;
        }

        void OpenFirstVisible()
        {
            foreach (var n in _navItems)
            {
                if (n.Item.Visible)
                {
                    SwitchPage(n.Key);
                    _search.Focus();
                    return;
                }
            }
        }

        // ============================================================
        // 布局：按行重排卡片位置与宽度（窗口尺寸变化时自动调整）
        // ============================================================
        void LayoutContent()
        {
            if (_content == null || !_pages.ContainsKey(_activeKey)) return;
            int pad = 22, gap = 18;
            int avail = _content.ClientSize.Width - pad * 2;
            if (avail < 100) return;

            int y = pad;
            foreach (var row in _pages[_activeKey].Rows)
            {
                int n = row.Cards.Count;
                if (n == 4 && avail < 900)
                {
                    int w = (avail - gap) / 2;
                    for (int i = 0; i < n; i++)
                    {
                        int column = i % 2;
                        int line = i / 2;
                        row.Cards[i].Panel.Location = new Point(pad + column * (w + gap), y + line * (row.Height + gap));
                        row.Cards[i].Panel.Size = new Size(w, row.Height);
                    }
                    y += row.Height * 2 + gap * 2;
                    continue;
                }
                int totalGap = gap * (n - 1);
                int x = pad;
                foreach (var c in row.Cards)
                {
                    int w = (int)Math.Round((avail - totalGap) * c.Span / 12.0);
                    c.Panel.Location = new Point(x, y);
                    c.Panel.Size = new Size(w, row.Height);
                    x += w + gap;
                }
                y += row.Height + gap;
            }

            foreach (var s in _stretchControls)
                s.Control.Width = Math.Max(1, s.Card.Width - s.Pad * 2);

            LayoutQuickNav();
        }

        void LayoutQuickNav()
        {
            if (_quickNavButtons.Count == 0) return;
            int available = _quickNavButtons[0].Parent.ClientSize.Width - 40;
            int gap = 10;
            int width = Math.Max(88, (available - gap * (_quickNavButtons.Count - 1)) / _quickNavButtons.Count);
            int x = 20;
            foreach (var button in _quickNavButtons)
            {
                button.Location = new Point(x, 58);
                button.Size = new Size(width, 46);
                x += width + gap;
            }
        }

        // ============================================================
        // 工具方法
        // ============================================================
        AntdUI.Panel MakeCard(string title)
        {
            var card = new AntdUI.Panel
            {
                Radius = 14,
                Shadow = 8,
                ShadowColor = Color.FromArgb(18, 15, 23, 42),
                ShadowOpacity = 0.05F,
            };
            if (!string.IsNullOrEmpty(title))
            {
                var t = new AntdUI.Label
                {
                    Text = title,
                    Font = new Font(_baseFont.FontFamily, 12F, FontStyle.Bold),
                    AutoSize = true,
                    Location = new Point(20, 16),
                };
                card.Controls.Add(t);
                _titleLabels.Add(t);
            }
            _cards.Add(card);
            return card;
        }

        AntdUI.Label NewMuted(string text, float size)
        {
            var l = new AntdUI.Label { Text = text, Font = new Font(_baseFont.FontFamily, size), AutoSize = true };
            _mutedLabels.Add(l);
            return l;
        }

        AntdUI.Label NewText(string text, float size = 9F)
        {
            var l = new AntdUI.Label { Text = text, Font = new Font(_baseFont.FontFamily, size), AutoSize = true };
            _primaryLabels.Add(l);
            return l;
        }

        AntdUI.Label GroupLabel(AntdUI.Panel card, string text, int x, int y)
        {
            var l = NewMuted(text, 8F);
            l.Location = new Point(x, y);
            card.Controls.Add(l);
            return l;
        }

        Control FullWidth(AntdUI.Panel card, Control control, int y, int height, int pad = 20)
        {
            control.Location = new Point(pad, y);
            control.Size = new Size(1, height);
            _stretchControls.Add(new Stretch { Card = card, Control = control, Pad = pad });
            return control;
        }

        AntdUI.Panel MakeStatCard(string title, string value, string trend, Color accent, string icon, TTypeMini trendType)
        {
            var card = new AntdUI.Panel
            {
                Radius = 14,
                Shadow = 8,
                ShadowColor = Color.FromArgb(18, 15, 23, 42),
                ShadowOpacity = 0.05F,
            };
            _cards.Add(card);

            var iconBox = new AntdUI.Avatar
            {
                Text = icon,
                Round = true,
                BackColor = Blend(accent, Color.White, 0.82f),
                Size = new Size(40, 40),
                Location = new Point(18, 18),
            };
            var titleLabel = NewMuted(title, 8F);
            titleLabel.Location = new Point(18, 66);
            var valueLabel = new AntdUI.Label
            {
                Text = value,
                Font = new Font(_baseFont.FontFamily, 18F, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(18, 84),
            };
            _titleLabels.Add(valueLabel);
            var trendTag = new AntdUI.Tag
            {
                Text = trend,
                Type = trendType,
                Radius = 6,
                AutoSize = true,
                Location = new Point(18, 116),
            };
            card.Controls.Add(iconBox);
            card.Controls.Add(titleLabel);
            card.Controls.Add(valueLabel);
            card.Controls.Add(trendTag);
            return card;
        }

        static Color Blend(Color a, Color b, float t)
        {
            return Color.FromArgb(
                (int)(a.R + (b.R - a.R) * t),
                (int)(a.G + (b.G - a.G) * t),
                (int)(a.B + (b.B - a.B) * t));
        }

        // ============================================================
        // 1. 概览
        // ============================================================
        Page BuildOverviewPage()
        {
            var p = NewPage("overview");
            AddRow(p, 164, CL(BuildHeroCard(), 12));
            AddRow(p, 150,
                CL(MakeStatCard("总访问量", "1,024,326", "↑ 12.5% 较上周", Blue, "总", TTypeMini.Success), 3),
                CL(MakeStatCard("销售额", "¥ 682,040", "↑ 8.2% 较上周", Green, "销", TTypeMini.Success), 3),
                CL(MakeStatCard("订单数", "8,846", "↓ 3.1% 较上周", Orange, "订", TTypeMini.Error), 3),
                CL(MakeStatCard("活跃用户", "48,220", "↑ 21.9% 较上周", Purple, "活", TTypeMini.Success), 3));
            AddRow(p, 130, CL(BuildQuickNavCard(), 12));
            return p;
        }

        AntdUI.Panel BuildHeroCard()
        {
            var card = new AntdUI.Panel
            {
                Radius = 16,
                Shadow = 8,
                ShadowColor = Color.FromArgb(22, 22, 119, 255),
                ShadowOpacity = 0.08F,
            };
            _heroCard = card;
            _cards.Add(card);

            var icon = new AntdUI.Avatar
            {
                Text = "A",
                Round = true,
                BackColor = Blend(Blue, Color.White, 0.82f),
                Size = new Size(68, 68),
                Location = new Point(28, 30),
            };
            var t = new AntdUI.Label
            {
                Text = "把 WinForms 做得更现代",
                Font = new Font(_baseFont.FontFamily, 20F, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(120, 28),
            };
            _titleLabels.Add(t);
            var d = NewMuted("统一的视觉规范、清晰的信息层级，以及开箱即用的交互组件。", 10F);
            d.Location = new Point(120, 68);

            var b1 = new AntdUI.Button { Text = "浏览组件", Type = TTypeMini.Primary, Radius = 8, Size = new Size(108, 36), Location = new Point(120, 108) };
            b1.Click += (s, e) => SwitchPage("button");
            var b2 = new AntdUI.Button { Text = "数据可视化", Type = TTypeMini.Default, Radius = 8, Size = new Size(116, 36), Location = new Point(238, 108) };
            b2.Click += (s, e) => SwitchPage("chart");

            card.Controls.Add(icon);
            card.Controls.Add(t);
            card.Controls.Add(d);
            card.Controls.Add(b1);
            card.Controls.Add(b2);
            return card;
        }

        AntdUI.Panel BuildQuickNavCard()
        {
            var card = MakeCard("快速开始");
            var items = new[]
            {
                new { K = "button", T = "按钮", I = "01" },
                new { K = "form", T = "表单", I = "02" },
                new { K = "data", T = "数据展示", I = "03" },
                new { K = "chart", T = "图表", I = "04" },
                new { K = "feedback", T = "反馈", I = "05" },
                new { K = "navigation", T = "导航", I = "06" },
            };
            int x = 20;
            foreach (var it in items)
            {
                var b = new AntdUI.Button { Text = it.I + "  " + it.T, Type = TTypeMini.Default, Radius = 8, Size = new Size(112, 46), Location = new Point(x, 58) };
                b.Click += (s, e) => SwitchPage(it.K);
                card.Controls.Add(b);
                _quickNavButtons.Add(b);
                x += 122;
            }
            return card;
        }

        // ============================================================
        // 2. 按钮
        // ============================================================
        Page BuildButtonPage()
        {
            var p = NewPage("button");
            AddRow(p, 360, CL(BuildButtonsCard(), 12));
            return p;
        }

        AntdUI.Panel BuildButtonsCard()
        {
            var card = MakeCard("按钮 Button");
            int x = 20, y = 52;

            GroupLabel(card, "类型", x, y); y += 24;
            var types = new[]
            {
                new { T = "主要", Ty = TTypeMini.Primary },
                new { T = "默认", Ty = TTypeMini.Default },
                new { T = "成功", Ty = TTypeMini.Success },
                new { T = "警告", Ty = TTypeMini.Warn },
                new { T = "错误", Ty = TTypeMini.Error },
                new { T = "信息", Ty = TTypeMini.Info },
            };
            int cx = x;
            foreach (var it in types)
            {
                var b = new AntdUI.Button { Text = it.T + "按钮", Type = it.Ty, Radius = 6, Size = new Size(96, 34), Location = new Point(cx, y) };
                b.Click += (s, e) => Message.info(this, "点击了 " + it.T + " 按钮", null, null);
                card.Controls.Add(b);
                cx += 106;
            }
            y += 48;

            GroupLabel(card, "变体", x, y); y += 24;
            cx = x;
            var bGhost = new AntdUI.Button { Text = "幽灵按钮", Type = TTypeMini.Primary, Ghost = true, Radius = 6, Size = new Size(96, 34), Location = new Point(cx, y) }; cx += 106;
            var bLink = new AntdUI.Button { Text = "链接按钮", Type = TTypeMini.Primary, IsLink = true, AutoSize = true, Location = new Point(cx, y + 6) }; cx += 86;
            var bLoad = new AntdUI.Button { Text = "加载中", Type = TTypeMini.Primary, Loading = true, Radius = 6, Size = new Size(96, 34), Location = new Point(cx, y) }; cx += 106;
            var bCircle = new AntdUI.Button { Text = "＋", Shape = TShape.Circle, Type = TTypeMini.Primary, Size = new Size(34, 34), Location = new Point(cx, y) }; cx += 44;
            var bRound = new AntdUI.Button { Text = "圆角", Shape = TShape.Round, Type = TTypeMini.Success, Size = new Size(80, 34), Location = new Point(cx, y) }; cx += 90;
            var bIcon = new AntdUI.Button { Text = "下拉", IconSvg = SvgDb.IcoDown, IconPosition = TAlignMini.Right, Type = TTypeMini.Default, Radius = 6, Size = new Size(96, 34), Location = new Point(cx, y) }; cx += 106;
            card.Controls.Add(bGhost);
            card.Controls.Add(bLink);
            card.Controls.Add(bLoad);
            card.Controls.Add(bCircle);
            card.Controls.Add(bRound);
            card.Controls.Add(bIcon);
            y += 48;

            GroupLabel(card, "尺寸", x, y); y += 26;
            var s1 = new AntdUI.Button { Text = "大按钮", Type = TTypeMini.Primary, Radius = 6, Size = new Size(112, 44), Location = new Point(x, y) };
            var s2 = new AntdUI.Button { Text = "默认按钮", Type = TTypeMini.Primary, Radius = 6, Size = new Size(104, 34), Location = new Point(x + 122, y + 5) };
            var s3 = new AntdUI.Button { Text = "小按钮", Type = TTypeMini.Primary, Radius = 6, Size = new Size(88, 26), Location = new Point(x + 236, y + 9) };
            card.Controls.Add(s1);
            card.Controls.Add(s2);
            card.Controls.Add(s3);
            y += 62;

            GroupLabel(card, "禁用", x, y); y += 24;
            var bD1 = new AntdUI.Button { Text = "主要按钮", Type = TTypeMini.Primary, Radius = 6, Enabled = false, Size = new Size(96, 34), Location = new Point(x, y) };
            var bD2 = new AntdUI.Button { Text = "默认按钮", Type = TTypeMini.Default, Radius = 6, Enabled = false, Size = new Size(96, 34), Location = new Point(x + 106, y) };
            var bD3 = new AntdUI.Button { Text = "文字按钮", IsLink = true, Enabled = false, AutoSize = true, Location = new Point(x + 212, y + 6) };
            card.Controls.Add(bD1);
            card.Controls.Add(bD2);
            card.Controls.Add(bD3);

            return card;
        }

        // ============================================================
        // 3. 表单
        // ============================================================
        Page BuildFormPage()
        {
            var p = NewPage("form");
            AddRow(p, 200, CL(BuildInputCard(), 12));
            AddRow(p, 330, CL(BuildPickerCard(), 12));
            return p;
        }

        AntdUI.Panel BuildInputCard()
        {
            var card = MakeCard("输入框 Input");
            int x = 20, y = 52;

            var i1 = new AntdUI.Input { PlaceholderText = "请输入用户名", Radius = 6, Size = new Size(200, 34), Location = new Point(x, y) };
            var i2 = new AntdUI.Input { PlaceholderText = "请输入密码", PasswordChar = '●', Radius = 6, Size = new Size(200, 34), Location = new Point(x + 210, y) };
            var i3 = new AntdUI.Input { Text = "可清空的内容", AllowClear = true, Radius = 6, Size = new Size(200, 34), Location = new Point(x + 420, y) };
            var i4 = new AntdUI.Input { PlaceholderText = "禁用状态", Enabled = false, Radius = 6, Size = new Size(200, 34), Location = new Point(x + 350, y + 56) };
            card.Controls.Add(i1);
            card.Controls.Add(i2);
            card.Controls.Add(i3);
            card.Controls.Add(i4);

            var n1 = new AntdUI.InputNumber { Value = 5, Minimum = 0, Maximum = 100, Increment = 1, ShowControl = true, Radius = 6, Size = new Size(120, 34), Location = new Point(x, y + 56) };
            var i5 = new AntdUI.Input { PrefixText = "¥", PlaceholderText = "金额", Radius = 6, Size = new Size(200, 34), Location = new Point(x + 130, y + 56) };
            card.Controls.Add(n1);
            card.Controls.Add(i5);
            return card;
        }

        AntdUI.Panel BuildPickerCard()
        {
            var card = MakeCard("选择与日期 Select / Switch / DatePicker");
            int x = 20, y = 52;

            var sel = new AntdUI.Select { PlaceholderText = "请选择城市", Radius = 6, Size = new Size(200, 34), Location = new Point(x, y) };
            sel.Items.Add(new AntdUI.SelectItem("北京", "bj"));
            sel.Items.Add(new AntdUI.SelectItem("上海", "sh"));
            sel.Items.Add(new AntdUI.SelectItem("广州", "gz"));
            sel.Items.Add(new AntdUI.SelectItem("深圳", "sz"));
            sel.SelectedIndex = 1;
            sel.SelectedValueChanged += (s, e) => Message.info(this, "选择了城市", null, null);

            var sw1 = new AntdUI.Switch { Checked = true, Size = new Size(44, 22), Location = new Point(x + 240, y + 6) };
            var sw1Label = NewText("开启推送", 9F); sw1Label.Location = new Point(x + 292, y + 9);
            var sw2 = new AntdUI.Switch { Checked = false, CheckedText = "开", UnCheckedText = "关", Size = new Size(60, 22), Location = new Point(x + 400, y + 6) };
            var sw2Label = NewText("带文字开关", 9F); sw2Label.Location = new Point(x + 458, y + 9);
            var sw3 = new AntdUI.Switch { Checked = true, Loading = true, Size = new Size(44, 22), Location = new Point(x + 570, y + 6) };
            var sw3Label = NewText("加载中", 9F); sw3Label.Location = new Point(x + 622, y + 9);
            card.Controls.Add(sel);
            card.Controls.Add(sw1); card.Controls.Add(sw1Label);
            card.Controls.Add(sw2); card.Controls.Add(sw2Label);
            card.Controls.Add(sw3); card.Controls.Add(sw3Label);
            y += 64;

            GroupLabel(card, "勾选 / 单选 / 评分", x, y); y += 26;
            var ck1 = new AntdUI.Checkbox { Text = "选项 A", Checked = true, Size = new Size(100, 24), Location = new Point(x, y) };
            var ck2 = new AntdUI.Checkbox { Text = "选项 B", Size = new Size(100, 24), Location = new Point(x + 110, y) };
            var ck3 = new AntdUI.Checkbox { Text = "禁用", Enabled = false, Size = new Size(80, 24), Location = new Point(x + 220, y) };
            var ra1 = new AntdUI.Radio { Text = "单选 A", Checked = true, Size = new Size(100, 24), Location = new Point(x + 320, y) };
            var ra2 = new AntdUI.Radio { Text = "单选 B", Size = new Size(100, 24), Location = new Point(x + 430, y) };
            var rate = new AntdUI.Rate { Value = 3.5f, AllowHalf = true, Size = new Size(140, 28), Location = new Point(x + 540, y - 4) };
            var rateLabel = NewText("3.5 / 5", 9F); rateLabel.Location = new Point(x + 700, y + 1);
            rate.ValueChanged += (s, e) => rateLabel.Text = rate.Value + " / 5";
            card.Controls.Add(ck1); card.Controls.Add(ck2); card.Controls.Add(ck3);
            card.Controls.Add(ra1); card.Controls.Add(ra2);
            card.Controls.Add(rate); card.Controls.Add(rateLabel);
            y += 58;

            GroupLabel(card, "滑块 / 日期 / 时间", x, y); y += 24;
            var slider = new AntdUI.Slider { MinValue = 0, MaxValue = 100, Value = 30, ShowValue = true, Size = new Size(280, 24), Location = new Point(x, y + 4) };
            var dp = new AntdUI.DatePicker { Format = "yyyy-MM-dd", Value = DateTime.Today, Size = new Size(150, 34), Radius = 6, Location = new Point(x + 320, y) };
            var tp = new AntdUI.TimePicker { Format = "HH:mm:ss", Value = DateTime.Now.TimeOfDay, Size = new Size(150, 34), Radius = 6, Location = new Point(x + 480, y) };
            card.Controls.Add(slider);
            card.Controls.Add(dp);
            card.Controls.Add(tp);

            return card;
        }

        // ============================================================
        // 4. 数据展示
        // ============================================================
        Page BuildDataPage()
        {
            var p = NewPage("data");
            AddRow(p, 380, CL(BuildTableCard(), 12));
            AddRow(p, 260, CL(BuildTagCard(), 6), CL(BuildTimelineCard(), 6));
            return p;
        }

        AntdUI.Panel BuildTagCard()
        {
            var card = MakeCard("标签 Tag / 徽标 Badge / 头像 Avatar");
            int x = 20, y = 52;

            GroupLabel(card, "标签", x, y); y += 24;
            var tags = new[]
            {
                new { T = "默认", Ty = TTypeMini.Default },
                new { T = "主要", Ty = TTypeMini.Primary },
                new { T = "成功", Ty = TTypeMini.Success },
                new { T = "警告", Ty = TTypeMini.Warn },
                new { T = "错误", Ty = TTypeMini.Error },
                new { T = "信息", Ty = TTypeMini.Info },
            };
            int cx = x;
            foreach (var it in tags)
            {
                var tg = new AntdUI.Tag { Text = it.T, Type = it.Ty, Radius = 6, AutoSize = true, Location = new Point(cx, y) };
                card.Controls.Add(tg);
                cx += 66;
            }
            var tgClose = new AntdUI.Tag { Text = "可关闭", Type = TTypeMini.Primary, CloseIcon = true, Radius = 6, AutoSize = true, Location = new Point(cx, y) };
            tgClose.CloseChanged += (s, e) => { Message.info(this, "点击了关闭", null, null); return true; };
            card.Controls.Add(tgClose);
            y += 44;

            GroupLabel(card, "徽标", x, y); y += 26;
            cx = x;
            var badges = new[] { TState.Success, TState.Processing, TState.Warn, TState.Error, TState.Default };
            string[] btext = { "5", "99+", "3", "新", "已读" };
            for (int i = 0; i < badges.Length; i++)
            {
                var bd = new AntdUI.Badge { State = badges[i], Text = btext[i], Size = new Size(56, 24), Location = new Point(cx, y) };
                card.Controls.Add(bd);
                cx += 60;
            }
            y += 44;

            GroupLabel(card, "头像", x, y); y += 26;
            cx = x;
            var av1 = new AntdUI.Avatar { Text = "张", BackColor = Blue, Size = new Size(40, 40), Location = new Point(cx, y) }; cx += 52;
            var av2 = new AntdUI.Avatar { Text = "李", BackColor = Green, Size = new Size(40, 40), Location = new Point(cx, y) }; cx += 52;
            var av3 = new AntdUI.Avatar { Text = "安", BackColor = Orange, Round = true, Size = new Size(40, 40), Location = new Point(cx, y) }; cx += 52;
            var av4 = new AntdUI.Avatar { Text = "周", BackColor = Purple, Round = true, Size = new Size(40, 40), Location = new Point(cx, y) }; cx += 52;
            var av5 = new AntdUI.Avatar { Text = "团队", BackColor = Red, Square = true, Size = new Size(40, 40), Location = new Point(cx, y) };
            card.Controls.Add(av1); card.Controls.Add(av2); card.Controls.Add(av3); card.Controls.Add(av4); card.Controls.Add(av5);

            return card;
        }

        AntdUI.Panel BuildTimelineCard()
        {
            var card = MakeCard("时间线 Timeline");
            var tl = new AntdUI.Timeline();
            tl.Items.Add(new AntdUI.TimelineItem("创建服务", "2025-01-10 09:30", null) { Type = TTypeMini.Success });
            tl.Items.Add(new AntdUI.TimelineItem("发布上线", "2025-02-01 18:00", null) { Type = TTypeMini.Primary });
            tl.Items.Add(new AntdUI.TimelineItem("版本迭代", "2025-03-15 10:00", null) { Type = TTypeMini.Info });
            tl.Items.Add(new AntdUI.TimelineItem("等待发布", "进行中…", null) { Type = TTypeMini.Warn });
            card.Controls.Add(FullWidth(card, tl, 52, 170));
            return card;
        }

        AntdUI.Panel BuildTableCard()
        {
            var card = MakeCard("数据表格 Table");
            var table = new AntdUI.Table
            {
                Bordered = true,
                FixedHeader = true,
                Radius = 8,
                RowHeight = 42,
                EmptyText = "暂无数据",
            };

            table.Columns = new AntdUI.ColumnCollection();
            table.Columns.Add(new AntdUI.Column("name", "姓名"));
            table.Columns.Add(new AntdUI.Column("age", "年龄") { Align = ColumnAlign.Center, Width = "80" });
            table.Columns.Add(new AntdUI.Column("city", "城市"));
            table.Columns.Add(new AntdUI.Column("state", "状态"));
            table.Columns.Add(new AntdUI.Column("progress", "进度"));
            table.Columns.Add(new AntdUI.Column("ops", "操作") { Width = "160" });

            table.DataSource = BuildTableRows();

            table.CellButtonClick += (s, e) =>
            {
                if (e.Btn != null && e.Btn.Id == "del")
                    Message.warn(this, "删除第 " + (e.RowIndex + 1) + " 行", null, null);
                else
                    Message.success(this, "编辑第 " + (e.RowIndex + 1) + " 行", null, null);
            };

            card.Controls.Add(FullWidth(card, table, 52, 300));
            return card;
        }

        List<AntdUI.AntItem[]> BuildTableRows()
        {
            var rows = new List<AntdUI.AntItem[]>();

            AntdUI.AntItem[] Make(string name, int age, string city, TState state, string stateText, float progress)
            {
                return new AntdUI.AntItem[]
                {
                    new AntdUI.AntItem("name", name),
                    new AntdUI.AntItem("age", age),
                    new AntdUI.AntItem("city", city),
                    new AntdUI.AntItem("state", new AntdUI.CellBadge(state, stateText)),
                    new AntdUI.AntItem("progress", new AntdUI.CellProgress(progress)),
                    new AntdUI.AntItem("ops", new AntdUI.CellLink[] {
                        new AntdUI.CellButton("edit", "编辑", TTypeMini.Primary),
                        new AntdUI.CellButton("del", "删除", TTypeMini.Error),
                    }),
                };
            }

            rows.Add(Make("张三", 28, "北京", TState.Success, "已启用", 0.9f));
            rows.Add(Make("李四", 32, "上海", TState.Processing, "处理中", 0.55f));
            rows.Add(Make("王五", 24, "广州", TState.Warn, "待审核", 0.3f));
            rows.Add(Make("赵六", 41, "深圳", TState.Error, "已禁用", 0.1f));
            rows.Add(Make("孙七", 29, "杭州", TState.Success, "已启用", 0.72f));
            rows.Add(Make("周八", 35, "成都", TState.Default, "草稿", 0.0f));
            return rows;
        }

        // ============================================================
        // 5. 图表
        // ============================================================
        Page BuildChartPage()
        {
            var p = NewPage("chart");
            AddRow(p, 330, CL(MakeChartCard("折线图 Line", MakeLineChart()), 6), CL(MakeChartCard("环形图 Doughnut", MakeDoughnutChart()), 6));
            AddRow(p, 330, CL(MakeChartCard("柱状图 Bar", MakeBarChart()), 6), CL(MakeChartCard("面积图 Area", MakeAreaChart()), 6));
            return p;
        }

        AntdUI.Panel MakeChartCard(string title, AntdUI.Chart chart)
        {
            var card = MakeCard(title);
            card.Controls.Add(FullWidth(card, chart, 52, 250));
            return card;
        }

        AntdUI.Chart MakeLineChart()
        {
            var chart = new AntdUI.Chart
            {
                ChartType = TChartType.Line,
                ShowLegend = true,
                ShowGrid = true,
                ShowTooltip = true,
                EnableAnimation = true,
            };
            var d1 = new AntdUI.ChartDataset("访问量", Blue);
            var d2 = new AntdUI.ChartDataset("订单量", Green);
            string[] days = { "周一", "周二", "周三", "周四", "周五", "周六", "周日" };
            double[] v1 = { 120, 200, 150, 280, 240, 320, 300 };
            double[] v2 = { 40, 70, 55, 90, 80, 110, 100 };
            for (int i = 0; i < days.Length; i++)
            {
                d1.AddPoint(days[i], i, v1[i]);
                d2.AddPoint(days[i], i, v2[i]);
            }
            chart.AddDataset(d1);
            chart.AddDataset(d2);
            return chart;
        }

        AntdUI.Chart MakeDoughnutChart()
        {
            var chart = new AntdUI.Chart
            {
                ChartType = TChartType.Doughnut,
                ShowLegend = true,
                ShowTooltip = true,
                EnablePieHoverValue = true,
                EnableAnimation = true,
            };
            var d = new AntdUI.ChartDataset("流量来源");
            d.AddPoint(new AntdUI.ChartDataPoint("搜索引擎", 0, 40) { Color = Blue });
            d.AddPoint(new AntdUI.ChartDataPoint("直接访问", 0, 30) { Color = Green });
            d.AddPoint(new AntdUI.ChartDataPoint("邮件营销", 0, 20) { Color = Orange });
            d.AddPoint(new AntdUI.ChartDataPoint("联盟广告", 0, 10) { Color = Purple });
            chart.AddDataset(d);
            return chart;
        }

        AntdUI.Chart MakeBarChart()
        {
            var chart = new AntdUI.Chart
            {
                ChartType = TChartType.Bar,
                ShowLegend = true,
                ShowGrid = true,
                ShowTooltip = true,
                EnableAnimation = true,
            };
            var d = new AntdUI.ChartDataset("销售额", Cyan);
            string[] months = { "1月", "2月", "3月", "4月", "5月", "6月" };
            double[] v = { 62, 48, 80, 55, 72, 90 };
            for (int i = 0; i < months.Length; i++)
                d.AddPoint(months[i], i, v[i]);
            chart.AddDataset(d);
            return chart;
        }

        AntdUI.Chart MakeAreaChart()
        {
            var chart = new AntdUI.Chart
            {
                ChartType = TChartType.Area,
                ShowLegend = true,
                ShowGrid = true,
                ShowTooltip = true,
                EnableAnimation = true,
            };
            var d = new AntdUI.ChartDataset("本年度", Purple);
            string[] axes = { "速度", "稳定", "易用", "性能", "文档", "生态" };
            double[] v = { 86, 72, 90, 68, 80, 75 };
            for (int i = 0; i < axes.Length; i++)
                d.AddPoint(axes[i], i, v[i]);
            chart.AddDataset(d);
            return chart;
        }

        // ============================================================
        // 6. 反馈
        // ============================================================
        Page BuildFeedbackPage()
        {
            var p = NewPage("feedback");
            AddRow(p, 270, CL(BuildAlertCard(), 12));
            AddRow(p, 270, CL(BuildProgressCard(), 6), CL(BuildMessageCard(), 6));
            return p;
        }

        AntdUI.Panel BuildAlertCard()
        {
            var card = MakeCard("提示 Alert");
            var a1 = new AntdUI.Alert { Icon = TType.Success, TextTitle = "操作成功", Text = "数据已保存，页面将在稍后自动刷新。", Radius = 8 };
            var a2 = new AntdUI.Alert { Icon = TType.Info, TextTitle = "新版本可用", Text = "发现新版本 2.5.0，建议升级以获取最新特性。", Radius = 8 };
            var a3 = new AntdUI.Alert { Icon = TType.Warn, TextTitle = "存储空间不足", Text = "当前剩余空间不足 10%，请及时清理。", CloseIcon = true, Radius = 8 };
            var a4 = new AntdUI.Alert { Icon = TType.Error, TextTitle = "连接失败", Text = "无法连接到远程服务器，请检查网络后重试。", Radius = 8 };
            card.Controls.Add(FullWidth(card, a1, 52, 50));
            card.Controls.Add(FullWidth(card, a2, 108, 50));
            card.Controls.Add(FullWidth(card, a3, 164, 50));
            card.Controls.Add(FullWidth(card, a4, 220, 50));
            return card;
        }

        AntdUI.Panel BuildProgressCard()
        {
            var card = MakeCard("进度 Progress");
            int x = 20, y = 52;

            var p1 = new AntdUI.Progress { Value = 0.3f, Shape = TShapeProgress.Round, Radius = 4, Size = new Size(200, 24), Location = new Point(x, y) };
            var p1l = NewMuted("30%", 8F); p1l.Location = new Point(x + 210, y + 4);
            var p2 = new AntdUI.Progress { Value = 0.5f, State = TType.Warn, Shape = TShapeProgress.Round, Radius = 4, Size = new Size(200, 24), Location = new Point(x, y + 30) };
            var p2l = NewMuted("50%", 8F); p2l.Location = new Point(x + 210, y + 34);
            var p3 = new AntdUI.Progress { Value = 1f, State = TType.Success, Shape = TShapeProgress.Round, Radius = 4, Size = new Size(200, 24), Location = new Point(x, y + 60) };
            var p3l = NewMuted("100%", 8F); p3l.Location = new Point(x + 210, y + 64);
            var p4 = new AntdUI.Progress { Value = 0.7f, State = TType.Error, Shape = TShapeProgress.Round, Radius = 4, Size = new Size(200, 24), Location = new Point(x, y + 90) };
            var p4l = NewMuted("70%", 8F); p4l.Location = new Point(x + 210, y + 94);

            var pc1 = new AntdUI.Progress { Shape = TShapeProgress.Circle, Value = 0.75f, UseTextCenter = true, Size = new Size(72, 72), Location = new Point(x, y + 126) };
            var pc2 = new AntdUI.Progress { Shape = TShapeProgress.Circle, Value = 0.42f, State = TType.Warn, UseTextCenter = true, Size = new Size(72, 72), Location = new Point(x + 84, y + 126) };
            var pc3 = new AntdUI.Progress { Shape = TShapeProgress.Circle, Value = 1f, State = TType.Success, UseTextCenter = true, Size = new Size(72, 72), Location = new Point(x + 168, y + 126) };

            card.Controls.Add(p1); card.Controls.Add(p1l);
            card.Controls.Add(p2); card.Controls.Add(p2l);
            card.Controls.Add(p3); card.Controls.Add(p3l);
            card.Controls.Add(p4); card.Controls.Add(p4l);
            card.Controls.Add(pc1); card.Controls.Add(pc2); card.Controls.Add(pc3);
            return card;
        }

        AntdUI.Panel BuildMessageCard()
        {
            var card = MakeCard("全局反馈 Message / Modal / Drawer");
            int x = 20, y = 54;

            void AddBtn(string text, TTypeMini type)
            {
                var b = new AntdUI.Button { Text = text, Type = type, Radius = 6, Size = new Size(76, 34), Location = new Point(x, y) };
                x += 84;
                card.Controls.Add(b);
                b.Click += (s, e) =>
                {
                    switch (type)
                    {
                        case TTypeMini.Success: Message.success(this, "这是一条成功提示", null, null); break;
                        case TTypeMini.Warn: Message.warn(this, "这是一条警告提示", null, null); break;
                        case TTypeMini.Error: Message.error(this, "这是一条错误提示", null, null); break;
                        default: Message.info(this, "这是一条普通提示", null, null); break;
                    }
                };
            }
            AddBtn("成功", TTypeMini.Success);
            AddBtn("信息", TTypeMini.Info);
            AddBtn("警告", TTypeMini.Warn);
            AddBtn("错误", TTypeMini.Error);
            y += 48;

            x = 20;
            var bn = new AntdUI.Button { Text = "通知", Type = TTypeMini.Primary, Radius = 6, Size = new Size(76, 34), Location = new Point(x, y) };
            bn.Click += (s, e) => Notification.success(this, "通知标题", "这是一条从右上角弹出的通知。", TAlignFrom.TR, null, null);

            var bm = new AntdUI.Button { Text = "对话框", Type = TTypeMini.Primary, Radius = 6, Size = new Size(76, 34), Location = new Point(x + 84, y) };
            bm.Click += (s, e) => AntdUI.Modal.open(this, "确认操作", "确定要执行此操作吗？");

            var bd = new AntdUI.Button { Text = "抽屉", Type = TTypeMini.Default, Radius = 6, Size = new Size(76, 34), Location = new Point(x + 168, y) };
            bd.Click += (s, e) =>
            {
                var panel = new AntdUI.Panel { Back = _cardBg, Dock = DockStyle.Fill };
                var t = NewText("这是一个抽屉 Drawer", 12F);
                t.Font = new Font(_baseFont.FontFamily, 12F, FontStyle.Bold);
                t.Location = new Point(20, 20);
                var d = NewMuted("点击右侧遮罩区域即可关闭。", 9F);
                d.Location = new Point(20, 50);
                panel.Controls.Add(t);
                panel.Controls.Add(d);
                AntdUI.Drawer.open(this, panel, TAlignMini.Right);
            };

            var bl = new AntdUI.Button { Text = "加载中", Type = TTypeMini.Default, Radius = 6, Size = new Size(76, 34), Location = new Point(x + 252, y) };
            bl.Click += (s, e) => Message.loading(this, "正在加载…", async config =>
            {
                await System.Threading.Tasks.Task.Delay(1200);
                config.OK("加载完成");
            }, null, null);

            card.Controls.Add(bn);
            card.Controls.Add(bm);
            card.Controls.Add(bd);
            card.Controls.Add(bl);
            return card;
        }

        // ============================================================
        // 7. 导航
        // ============================================================
        Page BuildNavigationPage()
        {
            var p = NewPage("navigation");
            AddRow(p, 210, CL(BuildSegmentCard(), 12));
            AddRow(p, 280, CL(BuildStepsCard(), 6), CL(BuildTabsCard(), 6));
            return p;
        }

        AntdUI.Panel BuildSegmentCard()
        {
            var card = MakeCard("分段 Segmented / 面包屑 Breadcrumb / 分页 Pagination");
            int x = 20, y = 54;

            var seg = new AntdUI.Segmented { SelectIndex = 1, Size = new Size(280, 34), Location = new Point(x, y) };
            seg.Items.Add(new AntdUI.SegmentedItem { Text = "日" });
            seg.Items.Add(new AntdUI.SegmentedItem { Text = "周" });
            seg.Items.Add(new AntdUI.SegmentedItem { Text = "月" });
            seg.Items.Add(new AntdUI.SegmentedItem { Text = "年" });
            seg.SelectIndexChanged += (s, e) => Message.info(this, "切换视图", null, null);

            var breadcrumb = new AntdUI.Breadcrumb { Separator = "/", Size = new Size(360, 30), Location = new Point(x + 320, y + 2) };
            breadcrumb.Items.Add(new AntdUI.BreadcrumbItem { Text = "首页" });
            breadcrumb.Items.Add(new AntdUI.BreadcrumbItem { Text = "组件" });
            breadcrumb.Items.Add(new AntdUI.BreadcrumbItem { Text = "表单" });
            breadcrumb.Items.Add(new AntdUI.BreadcrumbItem { Text = "当前页" });

            var pagination = new AntdUI.Pagination { Total = 200, Current = 1, PageSize = 10, ShowSizeChanger = true, Size = new Size(360, 30), Location = new Point(x, y + 52) };
            pagination.ValueChanged += (s, e) => Message.info(this, "跳转到第 " + pagination.Current + " 页", null, null);

            card.Controls.Add(seg);
            card.Controls.Add(breadcrumb);
            card.Controls.Add(pagination);
            return card;
        }

        AntdUI.Panel BuildStepsCard()
        {
            var card = MakeCard("步骤条 Steps");
            int x = 20, y = 52;

            var steps = new AntdUI.Steps { Current = 1 };
            steps.Items.Add(new AntdUI.StepsItem("填写信息", "第一步", "填写基本资料"));
            steps.Items.Add(new AntdUI.StepsItem("进行中", "第二步", "正在处理"));
            steps.Items.Add(new AntdUI.StepsItem("完成", "第三步", "全部完成"));
            steps.Items.Add(new AntdUI.StepsItem("反馈", "第四步", "等待反馈"));
            card.Controls.Add(FullWidth(card, steps, y, 96));

            var stepsV = new AntdUI.Steps { Current = 2, Vertical = true, Size = new Size(180, 110), Location = new Point(x, y + 108) };
            stepsV.Items.Add(new AntdUI.StepsItem("已完成"));
            stepsV.Items.Add(new AntdUI.StepsItem("已完成"));
            stepsV.Items.Add(new AntdUI.StepsItem("进行中"));
            card.Controls.Add(stepsV);

            return card;
        }

        AntdUI.Panel BuildTabsCard()
        {
            var card = MakeCard("选项卡 Tabs");
            var tabs = new AntdUI.Tabs { Type = TabType.Card };

            var p1 = new AntdUI.TabPage { Text = "标签页一" };
            var c1 = NewText("这是第一个标签页的内容，可以在这里放任意控件。", 9F);
            c1.Location = new Point(20, 60);
            p1.Controls.Add(c1);

            var p2 = new AntdUI.TabPage { Text = "标签页二" };
            var c2 = NewText("第二个标签页展示表单控件：", 9F);
            c2.Location = new Point(20, 40);
            var in2 = new AntdUI.Input { PlaceholderText = "在这里输入…", Radius = 6, Size = new Size(240, 34), Location = new Point(20, 70) };
            p2.Controls.Add(c2);
            p2.Controls.Add(in2);

            var p3 = new AntdUI.TabPage { Text = "标签页三" };
            var c3 = NewText("第三个标签页展示一个开关。", 9F);
            c3.Location = new Point(20, 40);
            var sw3 = new AntdUI.Switch { Checked = true, Size = new Size(44, 22), Location = new Point(20, 70) };
            p3.Controls.Add(c3);
            p3.Controls.Add(sw3);

            tabs.Pages.Add(p1);
            tabs.Pages.Add(p2);
            tabs.Pages.Add(p3);
            tabs.SelectedIndexChanged += (s, e) => Message.info(this, "切换到：" + tabs.Pages[tabs.SelectedIndex].Text, null, null);

            card.Controls.Add(FullWidth(card, tabs, 52, 180));
            return card;
        }

        // ============================================================
        // 主题切换
        // ============================================================
        void ApplyTheme(bool dark)
        {
            if (dark)
            {
                _pageBg = Color.FromArgb(17, 19, 24);
                _cardBg = Color.FromArgb(27, 30, 37);
                _textPrimary = Color.FromArgb(229, 229, 229);
                _textSecondary = Color.FromArgb(140, 140, 140);
                _divider = Color.FromArgb(56, 56, 56);
            }
            else
            {
                _pageBg = Color.FromArgb(245, 247, 250);
                _cardBg = Color.White;
                _textPrimary = Color.FromArgb(38, 38, 38);
                _textSecondary = Color.FromArgb(140, 140, 140);
                _divider = Color.FromArgb(232, 232, 232);
            }

            Dark = dark;

            if (_content != null) _content.BackColor = _pageBg;
            if (_windowHeader != null) _windowHeader.BackColor = _cardBg;
            if (_header != null) _header.Back = _cardBg;
            if (_sidebar != null) _sidebar.Back = _cardBg;
            if (_userCard != null) _userCard.Back = _pageBg;
            if (_sidebarBorder != null) _sidebarBorder.BackColor = _divider;

            foreach (var c in _cards) c.Back = _cardBg;
            if (_heroCard != null) _heroCard.Back = dark ? Color.FromArgb(30, 38, 55) : Color.FromArgb(238, 245, 255);
            foreach (var l in _titleLabels) l.ForeColor = _textPrimary;
            foreach (var l in _primaryLabels) l.ForeColor = _textPrimary;
            foreach (var l in _mutedLabels) l.ForeColor = _textSecondary;
        }
    }
}
