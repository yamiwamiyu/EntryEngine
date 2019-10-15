using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EntryEngine.UI
{
    /// <summary>屏幕中央弹出提示文字，常用于提示错误信息</summary>
    public class STextHint : UIScene
    {
        TIME showTime = new TIME(2000);
        public Label HintLabel;
        public string HintContent
        {
            get { return HintLabel.Text; }
            set { HintLabel.Text = value; }
        }
        public int ShowTime
        {
            get { return showTime.Interval; }
            set { showTime.Interval = value; }
        }
        
        public STextHint()
        {
            this.ShowPosition = EShowPosition.ParentCenter;
            this.Height = 34;

            HintLabel = new Label();
            HintLabel.Pivot = EPivot.MiddleLeft;
            HintLabel.UIText.TextAlignment = EPivot.MiddleCenter;
            HintLabel.UIText.Padding.X = 40;
            HintLabel.Width = Width;
            HintLabel.FontSize = 28;
            HintLabel.BreakLine = true;
            Add(HintLabel);

            this.PhaseShowing += HintScene_PhaseShowing;
        }

        float y
        {
            get { return HintLabel.Y - (Height * 0.5f); }
            set { HintLabel.Y = value + (Height * 0.5f); }
        }
        void HintScene_PhaseShowing(UIScene arg1)
        {
            y = 30;
            HintLabel.UIText.FontColor.A = 15;
            Close(EState.None, false);
        }
        protected internal override IEnumerable<ICoroutine> Ending()
        {
            while (y > -10)
            {
                y -= 5;
                HintLabel.UIText.FontColor.A += 15;
                yield return null;
            }
            while (y < 0)
            {
                y += 5;
                HintLabel.UIText.FontColor.A += 15;
                yield return null;
            }
            y = 0;
            HintLabel.UIText.FontColor.A = 255;
            showTime.Reset();
            yield return showTime;
            while (y < 30)
            {
                y += 5;
                HintLabel.UIText.FontColor.A -= 35;
                yield return null;
            }
        }

        public static STextHint ShowHint(string content)
        {
            STextHint scene = Entry.Instance.ShowDialogScene<STextHint>(EState.None);
            scene.HintLabel.Text = content;
            return scene;
        }
    }
    /// <summary>屏幕全黑或全白的淡入淡出，常用于切换场景</summary>
    public class SFade : UIScene
    {
        private float fadeSpeed = 255;
        /// <summary>渐变完成所需时间，单位秒</summary>
        public float FadeOverSecond { get { return 255 / fadeSpeed; } set { fadeSpeed = 255 / value; } }
        public Action OnFadeOutOver;
        public SFade()
        {
            Background = TEXTURE.Pixel;
            Color = COLOR.Black;
        }

        float alpha;

        protected internal override IEnumerable<ICoroutine> Preparing()
        {
            alpha = 0;
            Color.A = 0;
            if (FadeOverSecond == 0)
            {
                alpha = 255;
                Color.A = 255;
            }
            else
            {
                while (Color.A < 255)
                {
                    alpha += Entry.GameTime.ElapsedSecond * fadeSpeed;
                    Color.A = (byte)(alpha > 255 ? 255 : alpha);
                    yield return null;
                }
            }
            Close(EState.None, false);
            if (OnFadeOutOver != null)
                OnFadeOutOver();
        }
        protected internal override IEnumerable<ICoroutine> Ending()
        {
            // 等待一帧，防止同步加载时时间过长，直接导致菜单Alpha减到0
            yield return null;
            while (Entry.Scene.Phase < EPhase.Preparing)
                yield return null;
            if (FadeOverSecond == 0)
            {
                alpha = 0;
                Color.A = 0;
            }
            else
            {
                alpha = 255;
                while (Color.A > 0)
                {
                    alpha -= Entry.GameTime.ElapsedSecond * fadeSpeed;
                    Color.A = (byte)(alpha < 0 ? 0 : alpha);
                    yield return null;
                }
            }
        }

        public static SFade ShowBlack(Action onFadeOut)
        {
            SFade fade = Entry.Instance.ShowDialogScene<SFade>(EState.Dialog);
            fade.Color = COLOR.Black;
            fade.OnFadeOutOver = onFadeOut;
            return fade;
        }
        public static SFade ShowWhite(Action onFadeOut)
        {
            SFade fade = Entry.Instance.ShowDialogScene<SFade>(EState.Dialog);
            fade.Color = COLOR.White;
            fade.OnFadeOutOver = onFadeOut;
            return fade;
        }
    }
    /// <summary>弹出子场景，子场景以外的部分遮罩</summary>
    public class SMask : UIScene
    {
        public UIScene Scene;

        public SMask()
        {
            this.Background = TEXTURE.Pixel;
            this.Color = new COLOR(0, 0, 0, 128);
        }

        protected override void InternalEvent(Entry e)
        {
            base.InternalEvent(e);
            if (Scene != null && !Scene.IsHover && e.INPUT.Pointer.IsRelease(0))
            {
                Scene.Close(true);
                this.Close(true);
                //Handled = true;
                Handle();
            }
        }
        protected override void InternalUpdate(Entry e)
        {
            base.InternalUpdate(e);
            if (Scene != null && !Scene.IsInStage)
            {
                this.Close(true);
            }
        }

        public static T Show<T>() where T : UIScene, new()
        {
            SMask gray = Entry.Instance.ShowDialogScene<SMask>(EState.Dialog);
            T scene = Entry.Instance.ShowDialogScene<T>(EState.None);
            gray.Scene = scene;
            return scene;
        }
    }
    /// <summary>翻页按钮场景</summary>
    public class SPages : UIScene
    {
        private Button bPrev;
        private Button bNext;
        private TextBox tbGotoPage;
        private Label lTotal;
        private Panel pPages;

        public Button BPrev
        {
            get { return bPrev; }
            set
            {
                if (bPrev == value) return;
                if (value == null)
                    throw new ArgumentNullException();
                if (bPrev != null)
                    Remove(bPrev);
                bPrev = value;
                value.Clicked += TBPrev_Click;
                Add(value);
            }
        }
        public Button BNext
        {
            get { return bNext; }
            set
            {
                if (bNext == value) return;
                if (value == null)
                    throw new ArgumentNullException();
                if (bNext != null)
                    Remove(bNext);
                bNext = value;
                value.Clicked += TBNext_Click;
                Add(value);
            }
        }
        public TextBox TBGotoPage
        {
            get { return tbGotoPage; }
            set
            {
                if (tbGotoPage == value) return;
                if (value == null)
                    throw new ArgumentNullException();
                if (tbGotoPage != null)
                    Remove(tbGotoPage);
                tbGotoPage = value;
                value.TextEditOver += TBGotoPage_TextEditOver;
                Add(value);
            }
        }
        public Label LTotal
        {
            get { return lTotal; }
            set
            {
                if (lTotal == value) return;
                if (value == null)
                    throw new ArgumentNullException();
                if (lTotal != null)
                    Remove(lTotal);
                lTotal = value;
                Add(value);
            }
        }
        public Panel PPages { get { return pPages; } }

        const float BUTTON_SPACE = 10;
        const float BUTTON_SIZE = 30;
        private int page;
        private int pageMax;
        public Action<int> OnChangePage;
        public Action<CheckBox> OnCreatePage;

        public int Page
        {
            get { return page; }
            set
            {
                if (value < 0)
                    value = 0;
                else if (value > pageMax)
                    value = pageMax;
                page = value;
                if (OnChangePage != null)
                    OnChangePage(value);
                Relayout();
            }
        }

        public SPages()
        {
            BPrev = new Button();
            BNext = new Button();
            TBGotoPage = new TextBox();
            LTotal = new Label();
            pPages = new Panel();
            Add(pPages);

            this.Width = 0;
            this.Height = 70;
            this.Pivot = EPivot.BottomCenter;
            this.Anchor = EAnchor.Bottom | EAnchor.Left | EAnchor.Right;

            SetDefaultClip(bPrev);
            SetDefaultClip(bNext);
            SetDefaultClip(tbGotoPage);
            SetDefaultClip(lTotal);
            SetDefaultClip(pPages);

            lTotal.UIText.FontSize = 12;
            lTotal.UIText.FontColor = new COLOR(51, 51, 51);
            lTotal.UIText.TextAlignment = EPivot.MiddleCenter;
            lTotal.Width = 0;

            bPrev.UIText.FontSize = 15;
            bPrev.UIText.FontColor = new COLOR(153, 153, 153);
            bPrev.Text = "<";
            bPrev.SourceNormal = PATCH.GetNinePatch(COLOR.TransparentBlack, new COLOR(220, 220, 220), 1);
            bPrev.SourceHover = PATCH.GetNinePatch(COLOR.TransparentBlack, new COLOR(77, 160, 254), 1);
            bPrev.SourceClick = PATCH.GetNinePatch(COLOR.TransparentBlack, UIStyle.ToLight(new COLOR(77, 160, 254)), 1);

            bNext.UIText.FontSize = bPrev.UIText.FontSize;
            bNext.UIText.FontColor = bPrev.UIText.FontColor;
            bNext.Text = ">";
            bNext.SourceNormal = bPrev.SourceNormal;
            bNext.SourceHover = bPrev.SourceHover;
            bNext.SourceClick = bPrev.SourceClick;

            tbGotoPage.UIText.FontSize = 12;
            tbGotoPage.UIText.FontColor = new COLOR(51, 51, 51);
            tbGotoPage.UIText.TextAlignment = EPivot.MiddleCenter;
            tbGotoPage.SourceNormal = bPrev.SourceNormal;
            tbGotoPage.SourceHover = bPrev.SourceHover;
            tbGotoPage.SourceClick = bPrev.SourceClick;
            tbGotoPage.SourceClicked = bPrev.SourceHover;
        }

        void SetDefaultClip(UIElement element)
        {
            element.Pivot = EPivot.MiddleLeft;
            element.Y = Height * 0.5f;
            element.Width = BUTTON_SIZE;
            element.Height = BUTTON_SIZE;
        }

        void TBGotoPage_TextEditOver(Label sender)
        {
            if (string.IsNullOrEmpty(TBGotoPage.Text))
                return;

            int page;
            if (int.TryParse(TBGotoPage.Text, out page))
            {
                this.Page = page - 1;
            }
            else
            {
                sender.Text = null;
            }
        }
        void TBNext_Click(UIElement sender, Entry e)
        {
            if (page < pageMax)
                this.Page = page + 1;
        }
        void TBPrev_Click(UIElement sender, Entry e)
        {
            if (page > 0)
                this.Page = page - 1;
        }

        public void Relayout()
        {
            // 向前翻页
            BPrev.X = LTotal.X + LTotal.ContentSize.X + BUTTON_SPACE;
            // 页码
            pPages.Clear();
            if (pageMax >= 0)
            {
                pPages.X = BPrev.X + BPrev.Width + BUTTON_SPACE;
                int showCount = 5;
                int showHalf = showCount >> 1;
                // 首页
                CreatePageNumber(0);
                if (pageMax >= 1)
                {
                    // 向前快速翻页
                    if (page > showCount - 1)
                        // ~
                        CreatePageNumber(page - showHalf - 1, "~");
                    else
                        // 正数第二页
                        CreatePageNumber(1);
                    if (pageMax >= 2)
                    {
                        // 中间页码
                        for (int i = Math.Max(page - showHalf, 2), end = Math.Min(i + showCount, pageMax - 1); i < end; i++)
                        {
                            CreatePageNumber(i);
                        }
                        if (pageMax >= 3)
                        {
                            // 向后快速翻页
                            if (pageMax - page > showCount - 1)
                                // ~
                                CreatePageNumber(page + showHalf + 1, "~");
                            else
                                // 倒数第二页
                                CreatePageNumber(pageMax - 1);
                        }
                        // 末页
                        CreatePageNumber(pageMax);
                    }
                }
            }
            pPages.Width = pPages.Last.X + pPages.Last.Width;

            // 向后翻页
            BNext.X = pPages.X + pPages.Width + BUTTON_SPACE;

            // 跳转页数
            TBGotoPage.X = BNext.X + BNext.Width + BUTTON_SPACE;
            TBGotoPage.Text = null;
        }
        CheckBox CreatePageNumber(int number)
        {
            return CreatePageNumber(number, (number + 1).ToString());
        }
        CheckBox CreatePageNumber(int number, string text)
        {
            CheckBox pageItem = new CheckBox();
            pageItem.Width = BUTTON_SIZE;
            pageItem.Height = BUTTON_SIZE;
            pageItem.X = (pageItem.Width + BUTTON_SPACE) * pPages.ChildCount;
            pageItem.CheckedOverlayNormal = true;
            pageItem.IsRadioButton = true;
            pageItem.SourceNormal = PATCH.GetNinePatch(COLOR.TransparentBlack, new COLOR(220, 220, 220), 1);
            pageItem.SourceClicked = PATCH.GetNinePatch(new COLOR(77, 160, 254), COLOR.TransparentBlack, 1);
            pageItem.UpdateEnd = Page_ColorChange;
            pageItem.UIText.FontSize = 12;
            pageItem.UIText.TextAlignment = EPivot.MiddleCenter;
            pageItem.Text = text;
            pageItem.Tag = number;
            pageItem.CheckedChanged += Page_CheckedChanged;
            if (this.page == number)
                pageItem.Checked = true;
            pPages.Add(pageItem);
            if (OnCreatePage != null)
                OnCreatePage(pageItem);
            return pageItem;
        }
        void Page_ColorChange(UIElement sender, Entry e)
        {
            CheckBox cb = (CheckBox)sender;
            if (cb.Checked)
                cb.UIText.FontColor = COLOR.White;
            else
                cb.UIText.FontColor = new COLOR(102, 102, 102);
        }
        public void Page_CheckedChanged(Button sender, Entry e)
        {
            int page = (int)sender.Tag;
            if (this.page != page)
            {
                this.Page = page;
            }
        }
        public void SetParameter(int totalCount, int pageSize)
        {
            int tempMax = (totalCount - 1) / pageSize;
            bool changeMax = tempMax != pageMax;
            // 总页数
            pageMax = tempMax;
            // 共n条
            lTotal.Text = totalCount.ToString();
            // 刷新页面因为条数减少而导致页数减少时，显示到最后一夜
            if (page > pageMax)
            {
                this.Page = pageMax;
            }
            else
            {
                if (changeMax)
                    Relayout();
            }
        }
    }
    /// <summary>提示框</summary>
    public class STip : UIScene
    {
        private Dictionary<UIElement, Func<string>> tips = new Dictionary<UIElement, Func<string>>();
        public Label Label;
        public TEXTURE BG;
        public float MaxWidth = 400;

        public STip()
        {
            Color = new COLOR(228, 224, 110);
            Clip = RECT.Empty;
            BG = PATCH.GetNinePatch(new COLOR(242, 255, 172), COLOR.Black, 1);

            Label = new Label();
            Label.BreakLine = true;
            Label.UIText.FontColor = new COLOR(16, 16, 0);
            Add(Label);
        }

        public void SetTip(UIElement element, string text)
        {
            tips[element] = () => text;
        }
        public void SetTip(UIElement element, Func<string> getText)
        {
            tips[element] = getText;
        }
        public void ClearTips()
        {
            tips.Clear();
        }

        protected override void InternalEvent(EntryEngine.Entry e)
        {
            base.InternalEvent(e);

            Label.Visible = false;
            foreach (var item in tips)
            {
                if (item.Key.IsHover)
                {
                    Label.Visible = true;
                    Label.Text = item.Value();
                    var size = Label.TextContentSize;
                    if (size.X > MaxWidth)
                    {
                        Label.Width = MaxWidth;
                        Label.ResetDisplay();
                        size = Label.TextContentSize;
                    }
                    else
                        Label.Width = 0;

                    var position = __INPUT.PointerPosition;
                    var graphics = __GRAPHICS.GraphicsSize;
                    if (position.X + size.X > graphics.X)
                        position.X -= size.X;
                    else
                        position.X += 10;
                    if (position.Y + size.Y > graphics.Y)
                        position.Y -= size.Y;
                    Location = position;
                    break;
                }
            }

            Background = Label.Visible ? BG : null;
        }
    }
}
