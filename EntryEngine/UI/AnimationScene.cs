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
}
