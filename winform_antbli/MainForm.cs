using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using AntdUI;
using Message = AntdUI.Message;

namespace winform_antbli
{
    /// <summary>
    /// 一个基于 AntdUI 的 WinForms 组件展示界面。
    /// 左侧导航 + 顶部工具条 + 可滚动的组件卡片，用于快速浏览 AntdUI 常用控件与代码。
    /// </summary>
    public class MainForm : AntdUI.Window
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

        // ---------- 布局控件 ----------
        AntdUI.Panel _header;
        AntdUI.Panel _sidebar;
        AntdUI.Panel _userCard;
        System.Windows.Forms.Panel _headerBorder;    // 顶部分割线
        System.Windows.Forms.Panel _sidebarBorder;   // 侧边分割线
        System.Windows.Forms.Panel _content;         // 可滚动内容区

        // 需要随主题切换颜色的对象
        readonly List<AntdUI.Panel> _cards = new List<AntdUI.Panel>();
        readonly List<AntdUI.Label> _titleLabels = new List<AntdUI.Label>();
        readonly List<AntdUI.Label> _primaryLabels = new List<AntdUI.Label>();
        readonly List<AntdUI.Label> _mutedLabels = new List<AntdUI.Label>();

        // 需要随卡片宽度拉伸的控件（左/右各留 pad 边距）
        class Stretch { public AntdUI.Panel Card; public Control Control; public int Pad; }
        readonly List<Stretch> _stretchControls = new List<Stretch>();

        // 布局行：每行若干卡片（12 栅格），行高固定
        class CardLayout { public AntdUI.Panel Panel; public int Span; }
        class RowLayout { public int Height; public List<CardLayout> Cards = new List<CardLayout>(); }
        readonly List<RowLayout> _rows = new List<RowLayout>();
        private PageHeader pageHeader1;
        private PageHeader pageHeader2;
        readonly Font _baseFont = new Font("Microsoft YaHei UI", 9F);

        public MainForm()
        {
            InitializeWindow();
            BuildHeader();
            BuildSidebar();
            BuildContent();
            ApplyTheme(false);
            Resize += (s, e) => LayoutContent();
        }

        // ============================================================
        // 窗口基础设置
        // ============================================================
        void InitializeWindow()
        {
            Text = "AntdUI 组件展示 · WinForms";
            ClientSize = new Size(1240, 820);
            StartPosition = FormStartPosition.CenterScreen;
            MinimumSize = new Size(980, 640);
            Resizable = true;
            Font = _baseFont;
        }

        // ============================================================
        // 顶部工具条
        // ============================================================
        void BuildHeader()
        {
            _header = new AntdUI.Panel { Dock = DockStyle.Top, Height = 58, Back = Color.White, Radius = 0, Shadow = 0 };

            // 左侧标题
            var logo = new AntdUI.Label
            {
                Text = "🦄 AntdUI",
                Font = new Font(_baseFont.FontFamily, 16F, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(20, 15),
                Anchor = AnchorStyles.Top | AnchorStyles.Left,
            };
            var sub = NewMuted("组件展示 · Component Gallery", 9F);
            sub.Location = new Point(110, 22);
            sub.Anchor = AnchorStyles.Top | AnchorStyles.Left;
            _header.Controls.Add(logo);
            _header.Controls.Add(sub);
            _titleLabels.Add(logo);

            // 右侧：搜索框 / 主题开关 / GitHub / 头像（从右向左排，锚定右缘）
            var avatar = new AntdUI.Avatar
            {
                Text = "U",
                Round = true,
                BackColor = Blue,
                Size = new Size(34, 34),
                Location = new Point(_header.Width - 50, 12),
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
            };

            var gh = new AntdUI.Button
            {
                Text = "GitHub",
                Type = TTypeMini.Default,
                Radius = 6,
                Size = new Size(84, 34),
                Location = new Point(_header.Width - 144, 12),
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
            };
            gh.Click += (s, e) => Message.info(this, "打开 GitHub 仓库", null, null);

            var darkSwitch = new AntdUI.Switch
            {
                CheckedText = "深色",
                UnCheckedText = "浅色",
                Location = new Point(_header.Width - 250, 17),
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
            };
            darkSwitch.CheckedChanged += (s, e) => ApplyTheme(darkSwitch.Checked);

            var search = new AntdUI.Input
            {
                PrefixText = "🔍",
                PlaceholderText = "搜索组件…",
                Radius = 6,
                Size = new Size(220, 34),
                Location = new Point(_header.Width - 482, 12),
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
            };

            _header.Controls.Add(search);
            _header.Controls.Add(darkSwitch);
            _header.Controls.Add(gh);
            _header.Controls.Add(avatar);

            _headerBorder = new System.Windows.Forms.Panel { Dock = DockStyle.Bottom, Height = 1, BackColor = _divider };
            _header.Controls.Add(_headerBorder);
            _headerBorder.BringToFront();

            Controls.Add(_header);
        }

        // ============================================================
        // 左侧导航
        // ============================================================
        void BuildSidebar()
        {
            _sidebar = new AntdUI.Panel { Dock = DockStyle.Left, Width = 220, Back = Color.White, Radius = 0, Shadow = 0 };

            var navTitle = NewMuted("导航", 8F);
            navTitle.Location = new Point(24, 24);
            _sidebar.Controls.Add(navTitle);

            var menu = new AntdUI.Menu
            {
                Mode = TMenuMode.Inline,
                Indent = false,
                Radius = 8,
                Location = new Point(12, 52),
                Size = new Size(196, 300),
            };
            menu.Items.Add(new AntdUI.MenuItem { Text = "📊 概览", Tag = "overview" });
            menu.Items.Add(new AntdUI.MenuItem { Text = "🔘 按钮", Tag = "button" });
            menu.Items.Add(new AntdUI.MenuItem { Text = "📝 表单", Tag = "form" });
            menu.Items.Add(new AntdUI.MenuItem { Text = "📈 进度", Tag = "progress" });
            menu.Items.Add(new AntdUI.MenuItem { Text = "🏷️ 标签", Tag = "tag" });
            menu.Items.Add(new AntdUI.MenuItem { Text = "📋 表格", Tag = "table" });
            menu.ItemClick += (s, e) => Message.info(this, "你点击了「" + e.Item.Text + "」", null, null);
            _sidebar.Controls.Add(menu);

            // 底部用户卡片
            _userCard = new AntdUI.Panel
            {
                Radius = 10,
                Location = new Point(12, _sidebar.Height - 78),
                Size = new Size(196, 62),
                Anchor = AnchorStyles.Bottom | AnchorStyles.Left,
            };
            var ua = new AntdUI.Avatar
            {
                Text = "😎", Round = true, BackColor = Purple, Size = new Size(38, 38), Location = new Point(10, 12),
            };
            var un = new AntdUI.Label
            {
                Text = "AntdUI 用户",
                Font = new Font(_baseFont.FontFamily, 9.5F, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(58, 12),
            };
            var ur = NewMuted("管理员 · Admin", 8F);
            ur.Location = new Point(58, 34);
            _userCard.Controls.Add(ua);
            _userCard.Controls.Add(un);
            _userCard.Controls.Add(ur);
            _titleLabels.Add(un);
            _sidebar.Controls.Add(_userCard);

            _sidebarBorder = new System.Windows.Forms.Panel { Dock = DockStyle.Right, Width = 1, BackColor = _divider };
            _sidebar.Controls.Add(_sidebarBorder);
            _sidebarBorder.BringToFront();

            Controls.Add(_sidebar);

            // 内容区放在最后，填充剩余空间
            _content = new System.Windows.Forms.Panel { Dock = DockStyle.Fill, AutoScroll = true, BackColor = _pageBg };
            Controls.Add(_content);
        }

        // ============================================================
        // 内容区：把所有卡片加入布局行
        // ============================================================
        void BuildContent()
        {
            // 1. 概览统计（4 张小卡）
            var r1 = new RowLayout { Height = 150 };
            r1.Cards.Add(new CardLayout { Panel = MakeStatCard("总访问量", "1,024,326", "↑ 12.5% 较上周", Blue, "👀", TTypeMini.Success), Span = 3 });
            r1.Cards.Add(new CardLayout { Panel = MakeStatCard("销售额", "¥ 682,040", "↑ 8.2% 较上周", Green, "💰", TTypeMini.Success), Span = 3 });
            r1.Cards.Add(new CardLayout { Panel = MakeStatCard("订单数", "8,846", "↓ 3.1% 较上周", Orange, "🧾", TTypeMini.Error), Span = 3 });
            r1.Cards.Add(new CardLayout { Panel = MakeStatCard("活跃用户", "48,220", "↑ 21.9% 较上周", Purple, "🙂", TTypeMini.Success), Span = 3 });
            _rows.Add(r1);

            // 2. 按钮
            _rows.Add(new RowLayout { Height = 260, Cards = { new CardLayout { Panel = BuildButtonsCard(), Span = 12 } } });

            // 3. 表单
            _rows.Add(new RowLayout { Height = 340, Cards = { new CardLayout { Panel = BuildFormCard(), Span = 12 } } });

            // 4. 进度 + 标签/徽标/头像
            _rows.Add(new RowLayout { Height = 260, Cards = {
                new CardLayout { Panel = BuildProgressCard(), Span = 6 },
                new CardLayout { Panel = BuildTagCard(), Span = 6 },
            } });

            // 5. 提示 Alert
            _rows.Add(new RowLayout { Height = 280, Cards = { new CardLayout { Panel = BuildAlertCard(), Span = 12 } } });

            // 6. 步骤 + 时间线
            _rows.Add(new RowLayout { Height = 270, Cards = {
                new CardLayout { Panel = BuildStepsCard(), Span = 6 },
                new CardLayout { Panel = BuildTimelineCard(), Span = 6 },
            } });

            // 7. 分段 / 面包屑 / 分页
            _rows.Add(new RowLayout { Height = 180, Cards = { new CardLayout { Panel = BuildSegmentCard(), Span = 12 } } });

            // 8. 选项卡
            _rows.Add(new RowLayout { Height = 260, Cards = { new CardLayout { Panel = BuildTabsCard(), Span = 12 } } });

            // 9. 表格
            _rows.Add(new RowLayout { Height = 380, Cards = { new CardLayout { Panel = BuildTableCard(), Span = 12 } } });

            // 10. 反馈
            _rows.Add(new RowLayout { Height = 180, Cards = { new CardLayout { Panel = BuildFeedbackCard(), Span = 12 } } });

            foreach (var row in _rows)
                foreach (var c in row.Cards)
                    _content.Controls.Add(c.Panel);

            LayoutContent();
        }

        // ============================================================
        // 布局：按行重排卡片位置与宽度（窗口尺寸变化时自动调整）
        // ============================================================
        void LayoutContent()
        {
            if (_content == null || _rows.Count == 0) return;
            int pad = 16, gap = 16;
            int avail = _content.ClientSize.Width - pad * 2;
            if (avail < 100) return;

            int y = pad;
            foreach (var row in _rows)
            {
                int n = row.Cards.Count;
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
        }

        // ============================================================
        // 工具方法
        // ============================================================
        /// <summary>创建一个带标题的卡片。</summary>
        AntdUI.Panel MakeCard(string title)
        {
            var card = new AntdUI.Panel
            {
                Radius = 12,
                Shadow = 6,
                ShadowColor = Color.FromArgb(22, 0, 0, 0),
                ShadowOpacity = 0.06F,
            };
            var t = new AntdUI.Label
            {
                Text = title,
                Font = new Font(_baseFont.FontFamily, 14F, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(20, 16),
            };
            card.Controls.Add(t);
            _titleLabels.Add(t);
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

        /// <summary>小标题（卡片内的分组说明文字）。</summary>
        AntdUI.Label GroupLabel(AntdUI.Panel card, string text, int x, int y)
        {
            var l = NewMuted(text, 8F);
            l.Location = new Point(x, y);
            card.Controls.Add(l);
            return l;
        }

        /// <summary>登记一个需要随卡片宽度拉伸的控件（左右各留 pad 边距）。</summary>
        Control FullWidth(AntdUI.Panel card, Control control, int y, int height, int pad = 20)
        {
            control.Location = new Point(pad, y);
            control.Size = new Size(1, height);
            _stretchControls.Add(new Stretch { Card = card, Control = control, Pad = pad });
            return control;
        }

        AntdUI.Panel MakeStatCard(string title, string value, string trend, Color accent, string emoji, TTypeMini trendType)
        {
            var card = new AntdUI.Panel
            {
                Radius = 12,
                Shadow = 6,
                ShadowColor = Color.FromArgb(22, 0, 0, 0),
                ShadowOpacity = 0.06F,
            };
            _cards.Add(card);

            var icon = new AntdUI.Avatar
            {
                Text = emoji,
                Round = true,
                BackColor = Blend(accent, Color.White, 0.82f),
                Size = new Size(44, 44),
                Location = new Point(16, 16),
            };
            var stat = new AntdUI.Statistic
            {
                Text = title,
                Value = value,
                TextRatio = 0.42f,
                ValueRatio = 0.58f,
                Location = new Point(16, 64),
                Size = new Size(170, 52),
            };
            var trendTag = new AntdUI.Tag
            {
                Text = trend,
                Type = trendType,
                Radius = 6,
                AutoSize = true,
                Location = new Point(16, 120),
            };
            card.Controls.Add(icon);
            card.Controls.Add(stat);
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
        // 2. 按钮
        // ============================================================
        AntdUI.Panel BuildButtonsCard()
        {
            var card = MakeCard("按钮 Button");
            int x = 20, y = 52;

            GroupLabel(card, "类型", x, y); y += 24;

            var types = new[] {
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
            y += 46;

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
            y += 46;

            GroupLabel(card, "禁用", x, y); y += 24;
            cx = x;
            var bD1 = new AntdUI.Button { Text = "主要按钮", Type = TTypeMini.Primary, Radius = 6, Enabled = false, Size = new Size(96, 34), Location = new Point(cx, y) }; cx += 106;
            var bD2 = new AntdUI.Button { Text = "默认按钮", Type = TTypeMini.Default, Radius = 6, Enabled = false, Size = new Size(96, 34), Location = new Point(cx, y) }; cx += 106;
            var bD3 = new AntdUI.Button { Text = "文字按钮", IsLink = true, Enabled = false, AutoSize = true, Location = new Point(cx, y + 6) };
            card.Controls.Add(bD1);
            card.Controls.Add(bD2);
            card.Controls.Add(bD3);

            return card;
        }

        // ============================================================
        // 3. 表单
        // ============================================================
        AntdUI.Panel BuildFormCard()
        {
            var card = MakeCard("表单输入 Form");
            int x = 20, y = 48;

            // 第一行：输入框
            GroupLabel(card, "输入框 Input", x, y); y += 24;
            var i1 = new AntdUI.Input { PlaceholderText = "请输入用户名", Radius = 6, Size = new Size(200, 34), Location = new Point(x, y) };
            var i2 = new AntdUI.Input { PlaceholderText = "请输入密码", PasswordChar = '●', Radius = 6, Size = new Size(200, 34), Location = new Point(x + 210, y) };
            var i3 = new AntdUI.Input { Text = "可清空的内容", AllowClear = true, Radius = 6, Size = new Size(200, 34), Location = new Point(x + 420, y) };
            var i4 = new AntdUI.InputNumber { Value = 5, Minimum = 0, Maximum = 100, Increment = 1, ShowControl = true, Radius = 6, Size = new Size(120, 34), Location = new Point(x + 630, y) };
            card.Controls.Add(i1);
            card.Controls.Add(i2);
            card.Controls.Add(i3);
            card.Controls.Add(i4);
            y += 68;

            // 第二行：选择 / 开关
            GroupLabel(card, "选择 Select / 开关 Switch", x, y); y += 24;
            var sel = new AntdUI.Select { PlaceholderText = "请选择城市", Radius = 6, Size = new Size(200, 34), Location = new Point(x, y) };
            sel.Items.Add(new AntdUI.SelectItem("北京", "bj"));
            sel.Items.Add(new AntdUI.SelectItem("上海", "sh"));
            sel.Items.Add(new AntdUI.SelectItem("广州", "gz"));
            sel.Items.Add(new AntdUI.SelectItem("深圳", "sz"));
            sel.SelectedIndex = 1;
            sel.SelectedValueChanged += (s, e) => Message.info(this, "选择了城市", null, null);

            var sw1 = new AntdUI.Switch { Checked = true, Location = new Point(x + 240, y + 6) };
            var sw1Label = NewText("开启推送", 9F); sw1Label.Location = new Point(x + 292, y + 9);
            var sw2 = new AntdUI.Switch { Checked = false, CheckedText = "开", UnCheckedText = "关", Location = new Point(x + 400, y + 6) };
            var sw2Label = NewText("带文字开关", 9F); sw2Label.Location = new Point(x + 458, y + 9);
            var sw3 = new AntdUI.Switch { Checked = true, Loading = true, Location = new Point(x + 570, y + 6) };
            var sw3Label = NewText("加载中", 9F); sw3Label.Location = new Point(x + 622, y + 9);
            card.Controls.Add(sel);
            card.Controls.Add(sw1); card.Controls.Add(sw1Label);
            card.Controls.Add(sw2); card.Controls.Add(sw2Label);
            card.Controls.Add(sw3); card.Controls.Add(sw3Label);
            y += 66;

            // 第三行：勾选 / 单选 / 评分
            GroupLabel(card, "勾选 Checkbox / 单选 Radio / 评分 Rate", x, y); y += 26;
            var ck1 = new AntdUI.Checkbox { Text = "选项 A", Checked = true, Location = new Point(x, y) };
            var ck2 = new AntdUI.Checkbox { Text = "选项 B", Location = new Point(x + 110, y) };
            var ck3 = new AntdUI.Checkbox { Text = "禁用", Enabled = false, Location = new Point(x + 220, y) };
            var ra1 = new AntdUI.Radio { Text = "单选 A", Checked = true, Location = new Point(x + 320, y) };
            var ra2 = new AntdUI.Radio { Text = "单选 B", Location = new Point(x + 430, y) };
            var rate = new AntdUI.Rate { Value = 3.5f, AllowHalf = true, Location = new Point(x + 540, y - 4) };
            var rateLabel = NewText("3.5 / 5", 9F); rateLabel.Location = new Point(x + 700, y + 1);
            rate.ValueChanged += (s, e) => rateLabel.Text = rate.Value + " / 5";
            card.Controls.Add(ck1); card.Controls.Add(ck2); card.Controls.Add(ck3);
            card.Controls.Add(ra1); card.Controls.Add(ra2);
            card.Controls.Add(rate); card.Controls.Add(rateLabel);
            y += 58;

            // 第四行：滑块 / 日期
            GroupLabel(card, "滑块 Slider / 日期 DatePicker", x, y); y += 24;
            var slider = new AntdUI.Slider { MinValue = 0, MaxValue = 100, Value = 30, ShowValue = true, Size = new Size(280, 24), Location = new Point(x, y + 4) };
            var dp = new AntdUI.DatePicker { Format = "yyyy-MM-dd", Value = DateTime.Today, Size = new Size(150, 34), Radius = 6, Location = new Point(x + 320, y) };
            var tp = new AntdUI.TimePicker { Format = "HH:mm:ss", Value = DateTime.Now.TimeOfDay, Size = new Size(150, 34), Radius = 6, Location = new Point(x + 480, y) };
            card.Controls.Add(slider);
            card.Controls.Add(dp);
            card.Controls.Add(tp);

            return card;
        }

        // ============================================================
        // 4. 进度
        // ============================================================
        AntdUI.Panel BuildProgressCard()
        {
            var card = MakeCard("进度 Progress");
            int x = 20, y = 56;

            var p1 = new AntdUI.Progress { Value = 0.3f, Shape = TShapeProgress.Round, Radius = 4, Size = new Size(200, 24), Location = new Point(x, y) };
            var p1l = NewMuted("30%", 8F); p1l.Location = new Point(x + 210, y + 4);
            var p2 = new AntdUI.Progress { Value = 0.5f, State = TType.Warn, Shape = TShapeProgress.Round, Radius = 4, Size = new Size(200, 24), Location = new Point(x, y + 30) };
            var p2l = NewMuted("50%", 8F); p2l.Location = new Point(x + 210, y + 34);
            var p3 = new AntdUI.Progress { Value = 1f, State = TType.Success, Shape = TShapeProgress.Round, Radius = 4, Size = new Size(200, 24), Location = new Point(x, y + 60) };
            var p3l = NewMuted("100%", 8F); p3l.Location = new Point(x + 210, y + 64);
            var p4 = new AntdUI.Progress { Value = 0.7f, State = TType.Error, Shape = TShapeProgress.Round, Radius = 4, Size = new Size(200, 24), Location = new Point(x, y + 90) };
            var p4l = NewMuted("70%", 8F); p4l.Location = new Point(x + 210, y + 94);

            // 圆形进度
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

        // ============================================================
        // 4b. 标签 / 徽标 / 头像
        // ============================================================
        AntdUI.Panel BuildTagCard()
        {
            var card = MakeCard("标签 Tag / 徽标 Badge / 头像 Avatar");
            int x = 20, y = 56;

            GroupLabel(card, "标签", x, y); y += 24;
            var tags = new[] {
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
            y += 46;

            GroupLabel(card, "徽标", x, y); y += 26;
            cx = x;
            var badges = new[] { TState.Success, TState.Processing, TState.Warn, TState.Error, TState.Default };
            string[] btext = { "5", "99+", "3", "新", "已读" };
            for (int i = 0; i < badges.Length; i++)
            {
                var bd = new AntdUI.Badge { State = badges[i], Text = btext[i], Location = new Point(cx, y) };
                card.Controls.Add(bd);
                cx += 60;
            }
            y += 46;

            GroupLabel(card, "头像", x, y); y += 26;
            cx = x;
            var av1 = new AntdUI.Avatar { Text = "A", BackColor = Blue, Size = new Size(40, 40), Location = new Point(cx, y) }; cx += 52;
            var av2 = new AntdUI.Avatar { Text = "🐧", BackColor = Green, Size = new Size(40, 40), Location = new Point(cx, y) }; cx += 52;
            var av3 = new AntdUI.Avatar { Text = "安", BackColor = Orange, Round = true, Size = new Size(40, 40), Location = new Point(cx, y) }; cx += 52;
            var av4 = new AntdUI.Avatar { Text = "🙂", BackColor = Purple, Round = true, Size = new Size(40, 40), Location = new Point(cx, y) }; cx += 52;
            var av5 = new AntdUI.Avatar { Text = "团队", BackColor = Red, Square = true, Size = new Size(40, 40), Location = new Point(cx, y) };
            card.Controls.Add(av1); card.Controls.Add(av2); card.Controls.Add(av3); card.Controls.Add(av4); card.Controls.Add(av5);

            return card;
        }

        // ============================================================
        // 5. 提示 Alert
        // ============================================================
        AntdUI.Panel BuildAlertCard()
        {
            var card = MakeCard("提示 Alert");
            int y = 52;

            var a1 = new AntdUI.Alert { Icon = TType.Success, TextTitle = "操作成功", Text = "数据已保存，页面将在稍后自动刷新。", Radius = 8 };
            var a2 = new AntdUI.Alert { Icon = TType.Info, TextTitle = "新版本可用", Text = "发现新版本 2.5.0，建议升级以获取最新特性。", Radius = 8 };
            var a3 = new AntdUI.Alert { Icon = TType.Warn, TextTitle = "存储空间不足", Text = "当前剩余空间不足 10%，请及时清理。", CloseIcon = true, Radius = 8 };
            var a4 = new AntdUI.Alert { Icon = TType.Error, TextTitle = "连接失败", Text = "无法连接到远程服务器，请检查网络后重试。", Radius = 8 };
            card.Controls.Add(FullWidth(card, a1, y, 50));
            card.Controls.Add(FullWidth(card, a2, y + 56, 50));
            card.Controls.Add(FullWidth(card, a3, y + 112, 50));
            card.Controls.Add(FullWidth(card, a4, y + 168, 50));
            return card;
        }

        // ============================================================
        // 6. 步骤 / 时间线
        // ============================================================
        AntdUI.Panel BuildStepsCard()
        {
            var card = MakeCard("步骤条 Steps");
            int x = 20, y = 56;

            var steps = new AntdUI.Steps { Current = 1 };
            steps.Items.Add(new AntdUI.StepsItem("填写信息", "第一步", "填写基本资料"));
            steps.Items.Add(new AntdUI.StepsItem("进行中", "第二步", "正在处理"));
            steps.Items.Add(new AntdUI.StepsItem("完成", "第三步", "全部完成"));
            steps.Items.Add(new AntdUI.StepsItem("反馈", "第四步", "等待反馈"));
            card.Controls.Add(FullWidth(card, steps, y, 96));

            var stepsV = new AntdUI.Steps { Current = 2, Vertical = true, Size = new Size(180, 110), Location = new Point(x, y + 104) };
            stepsV.Items.Add(new AntdUI.StepsItem("已完成"));
            stepsV.Items.Add(new AntdUI.StepsItem("已完成"));
            stepsV.Items.Add(new AntdUI.StepsItem("进行中"));
            card.Controls.Add(stepsV);

            return card;
        }

        AntdUI.Panel BuildTimelineCard()
        {
            var card = MakeCard("时间线 Timeline");
            int y = 56;

            var tl = new AntdUI.Timeline();
            tl.Items.Add(new AntdUI.TimelineItem("创建服务", "2025-01-10 09:30", null) { Type = TTypeMini.Success });
            tl.Items.Add(new AntdUI.TimelineItem("发布上线", "2025-02-01 18:00", null) { Type = TTypeMini.Primary });
            tl.Items.Add(new AntdUI.TimelineItem("版本迭代", "2025-03-15 10:00", null) { Type = TTypeMini.Info });
            tl.Items.Add(new AntdUI.TimelineItem("等待发布", "进行中…", null) { Type = TTypeMini.Warn });
            card.Controls.Add(FullWidth(card, tl, y, 170));
            return card;
        }

        // ============================================================
        // 7. 分段 / 面包屑 / 分页
        // ============================================================
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

        // ============================================================
        // 8. 选项卡
        // ============================================================
        AntdUI.Panel BuildTabsCard()
        {
            var card = MakeCard("选项卡 Tabs");
            int y = 52;

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
            var sw3 = new AntdUI.Switch { Checked = true, Location = new Point(20, 70) };
            p3.Controls.Add(c3);
            p3.Controls.Add(sw3);

            tabs.Pages.Add(p1);
            tabs.Pages.Add(p2);
            tabs.Pages.Add(p3);
            tabs.SelectedIndexChanged += (s, e) => Message.info(this, "切换到：" + tabs.Pages[tabs.SelectedIndex].Text, null, null);

            card.Controls.Add(FullWidth(card, tabs, y, 170));
            return card;
        }

        // ============================================================
        // 9. 表格
        // ============================================================
        AntdUI.Panel BuildTableCard()
        {
            var card = MakeCard("数据表格 Table");
            int y = 52;

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

            card.Controls.Add(FullWidth(card, table, y, 290));
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
        // 10. 反馈
        // ============================================================
        AntdUI.Panel BuildFeedbackCard()
        {
            var card = MakeCard("反馈 Feedback");
            int x = 20, y = 54;

            GroupLabel(card, "全局提示 Message", x, y); y += 24;
            int cx = x;
            void AddMsgBtn(string text, TTypeMini type)
            {
                var b = new AntdUI.Button { Text = text, Type = type, Radius = 6, Size = new Size(104, 34), Location = new Point(cx, y) };
                cx += 114;
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
            AddMsgBtn("成功", TTypeMini.Success);
            AddMsgBtn("信息", TTypeMini.Info);
            AddMsgBtn("警告", TTypeMini.Warn);
            AddMsgBtn("错误", TTypeMini.Error);
            y += 48;

            GroupLabel(card, "通知 Notification / 对话框 Modal / 抽屉 Drawer", x, y); y += 24;
            cx = x;
            var bn = new AntdUI.Button { Text = "右上角通知", Type = TTypeMini.Primary, Radius = 6, Size = new Size(110, 34), Location = new Point(cx, y) }; cx += 120;
            bn.Click += (s, e) => Notification.success(this, "通知标题", "这是一条从右上角弹出的通知。", TAlignFrom.TR, null, null);

            var bm = new AntdUI.Button { Text = "打开对话框", Type = TTypeMini.Primary, Radius = 6, Size = new Size(110, 34), Location = new Point(cx, y) }; cx += 120;
            bm.Click += (s, e) => AntdUI.Modal.open(this, "确认操作", "确定要执行此操作吗？");

            var bd = new AntdUI.Button { Text = "打开抽屉", Type = TTypeMini.Default, Radius = 6, Size = new Size(110, 34), Location = new Point(cx, y) }; cx += 120;
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

            var bl = new AntdUI.Button { Text = "加载提示", Type = TTypeMini.Default, Radius = 6, Size = new Size(110, 34), Location = new Point(cx, y) };
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
        // 主题切换
        // ============================================================
        void ApplyTheme(bool dark)
        {
            if (dark)
            {
                _pageBg = Color.FromArgb(20, 20, 20);
                _cardBg = Color.FromArgb(31, 31, 31);
                _textPrimary = Color.FromArgb(229, 229, 229);
                _textSecondary = Color.FromArgb(140, 140, 140);
                _divider = Color.FromArgb(56, 56, 56);
            }
            else
            {
                _pageBg = Color.FromArgb(240, 242, 245);
                _cardBg = Color.White;
                _textPrimary = Color.FromArgb(38, 38, 38);
                _textSecondary = Color.FromArgb(140, 140, 140);
                _divider = Color.FromArgb(232, 232, 232);
            }

            Dark = dark;

            if (_content != null) _content.BackColor = _pageBg;
            if (_header != null) _header.Back = _cardBg;
            if (_sidebar != null) _sidebar.Back = _cardBg;
            if (_userCard != null) _userCard.Back = _pageBg;
            if (_headerBorder != null) _headerBorder.BackColor = _divider;
            if (_sidebarBorder != null) _sidebarBorder.BackColor = _divider;

            foreach (var c in _cards) c.Back = _cardBg;
            foreach (var l in _titleLabels) l.ForeColor = _textPrimary;
            foreach (var l in _primaryLabels) l.ForeColor = _textPrimary;
            foreach (var l in _mutedLabels) l.ForeColor = _textSecondary;
        }

        private void InitializeComponent()
        {
            this.pageHeader1 = new AntdUI.PageHeader();
            this.pageHeader2 = new AntdUI.PageHeader();
            this.pageHeader1.SuspendLayout();
            this.SuspendLayout();
            // 
            // pageHeader1
            // 
            this.pageHeader1.Controls.Add(this.pageHeader2);
            this.pageHeader1.Location = new System.Drawing.Point(0, 0);
            this.pageHeader1.Name = "pageHeader1";
            this.pageHeader1.ShowButton = true;
            this.pageHeader1.ShowIcon = true;
            this.pageHeader1.Size = new System.Drawing.Size(1065, 43);
            this.pageHeader1.TabIndex = 0;
            this.pageHeader1.Text = "pageHeader1";
            // 
            // pageHeader2
            // 
            this.pageHeader2.Location = new System.Drawing.Point(0, 3);
            this.pageHeader2.Name = "pageHeader2";
            this.pageHeader2.ShowButton = true;
            this.pageHeader2.ShowIcon = true;
            this.pageHeader2.Size = new System.Drawing.Size(1065, 40);
            this.pageHeader2.TabIndex = 1;
            this.pageHeader2.Text = "pageHeader2";
            // 
            // MainForm
            // 
            this.ClientSize = new System.Drawing.Size(1065, 300);
            this.Controls.Add(this.pageHeader1);
            this.Name = "MainForm";
            this.pageHeader1.ResumeLayout(false);
            this.ResumeLayout(false);

        }
    }
}
