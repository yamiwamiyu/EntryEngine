using System;
using System.Collections.Generic;
using System.Linq;
using EntryEditor;
using EntryEngine;
using EntryEngine.Serialize;
using EntryEngine.UI;
using EntryEngine.Xna;
using System.IO;
using BinaryReader = EntryEngine.Serialize.ByteReader;

namespace EditorUI
{
    public partial class EditorUI
    {
        private static EPivot[] pivots;

        class View : Widget
        {
            public OperationLog OperationLog = new OperationLog();

            public override bool Save()
            {
                //if (!string.IsNullOrEmpty(FilePath))
                //{
                //    if (OperationLog.HasModified)
                //    {
                //        if (!UtilityEditor.Confirm("Sys0001"))
                //        {
                //            return false;
                //        }
                //    }
                //    else
                //    {
                //        return false;
                //    }
                //}

                if (!base.Save())
                {
                    _LOG.Debug("没有保存？导致没有生成新代码");
                    return false;
                }

                OperationLog.Save();

                // save uiscene.cs
                GeneratorUICode.Generate(this);

                return true;
            }
        }
        enum EEditMode
        {
            None,
            Offset,
            Scale,
        }
        class NearAlign
        {
            public bool Vertical;
            public float Alignment;
            public int Pivot;
            public float Result;
            public List<UIElement> AlignTargets = new List<UIElement>();
        }

        private bool drawBorder = true;
        private View view;
        private Panel pvc;
        private Panel pv;
        private UIElement selected;
        private EPivot? editScaleMode;
        private RECT editStartParam;
        private VECTOR2 editStartPoint;
        private List<NearAlign> nears = new List<NearAlign>();
        private UIElement copy;

		private PATCH patchViewportElementBorder;
		private PATCH patchViewportSelectedParentBorder;
		private PATCH patchViewportFocusBorder;
		private PATCH patchViewportAlign;

        public UIElement Selected
        {
            get { return selected; }
            set
            {
                if (selected == value)
                    return;
                selected = value;
                pp.Clear();
                PropertyPanelLayout(VECTOR2.NaN);
            }
        }
        public UIScene EditingScene
        {
            get
            {
                if (view == null)
                    return null;
                else
                    return (UIScene)view.Target.Instance;
            }
        }
        private bool Editing
        {
            get { return selected != null && editScaleMode.HasValue; }
        }
        private EEditMode EditMode
        {
            get
            {
                if (editScaleMode.HasValue)
                    if (editScaleMode.Value == EPivot.MiddleCenter)
                        return EEditMode.Offset;
                    else
                        return EEditMode.Scale;
                else
                    return EEditMode.None;
            }
        }
        internal EditMode CurrentEditMode
        {
            private get;
            set;
        }
        private MATRIX2x3 ScaleMatrix
        {
            get
            {
                var clip = EditingScene.ViewClip;
                return MATRIX2x3.CreateTransform(0,
                    clip.X + clip.Width / 2, clip.Y + clip.Height / 2,
                    Project.Document.Scale, Project.Document.Scale,
                    clip.X + clip.Width / 2, clip.Y + clip.Height / 2);
                //return MATRIX2x3.Identity;
            }
        }
        private VECTOR2 ScaledPointerPosition
        {
            get
            {
                VECTOR2 position = __INPUT.PointerPosition;
                if (view != null)
                {
                    var matrix = ScaleMatrix;
                    VECTOR2.Transform(ref position, ref matrix);
                }
                return position;
            }
        }

        private void InitializeViewport()
        {
            pvc = new Panel();
            pvc.Anchor = EAnchor.Left | EAnchor.Top | EAnchor.Right | EAnchor.Bottom;
            pvc.X = pec.Width;
            pvc.Y = ms.Height;
            pvc.Width = Entry.GRAPHICS.ScreenSize.X - pec.Width - ppc.Width;
            pvc.Height = Entry.GRAPHICS.ScreenSize.Y - ms.Height;
            pvc.Background = GetNinePatch(COLOR.Gray, C.ViewportBGColor);
            pvc.ScrollOrientation = EScrollOrientation.VerticalAuto | EScrollOrientation.HorizontalAuto;
            var scrollbar = GetScrollBar(true);
            scrollbar.Pivot = EPivot.TopRight;
            scrollbar.Anchor = EAnchor.Top | EAnchor.Right | EAnchor.Bottom;
            scrollbar.X = pvc.Width;
            scrollbar.Height = pvc.Height;
            pvc.ScrollBarVertical = scrollbar;
            pvc.Add(scrollbar);
            scrollbar = GetScrollBar(false);
            scrollbar.StepScroll = 0;
            scrollbar.Pivot = EPivot.BottomLeft;
            scrollbar.Anchor = EAnchor.Left | EAnchor.Bottom | EAnchor.Right;
            scrollbar.Y = pvc.Height;
            scrollbar.Width = pvc.Width;
            pvc.ScrollBarHorizontal = scrollbar;
            pvc.DragMode = EDragMode.None;
            pvc.Add(scrollbar);
            this.Add(pvc);

            pv = new Panel();
            pv.Anchor = EAnchor.Left | EAnchor.Top;
			pv.Background = TEXTURE.Pixel;
			pv.Color = C.ViewportColor;

            Document.ScaleChanged += new Action<float>(Document_ScaleChanged);
            pv.DrawBeforeEnd += ViewportDrawLines;
            pv.DrawAfterBegin = BeginScale;
            pv.DrawBeforeEnd += EndScale;

            pvc.Add(pv);

            patchViewportElementBorder = GetNinePatch(C.ColorBorder, COLOR.TransparentBlack);
            patchViewportSelectedParentBorder = GetNinePatch(C.ColorFocusBorder, C.ColorFocusBody);
            patchViewportFocusBorder = GetNinePatch(C.ColorFocusBorder, C.ColorFocusBody);
            patchViewportAlign = GetNinePatch(C.ColorAlignLine, COLOR.TransparentBlack);

            pvc.Hover += ScaleViewport;
        }

        // 鼠标中键并且滚动滑轮可以缩放视图
        private void ScaleViewport(UIElement sender, Entry e)
        {
            if (Project != null && Project.Document != null && e.INPUT.Pointer.IsPressed(2) && e.INPUT.Mouse.ScrollWheelValue != 0)
            {
                Project.Document.Expand(_MATH.Sign(e.INPUT.Mouse.ScrollWheelValue));
                Handle();
                ResetViewport();
            }
        }
        private void Document_ScaleChanged(float obj)
        {
            //pv.Size = EditingScene.Size * obj;
            //pvc.ContentScope = pv.Size + pvc.Size * 2;
        }
        private void BeginScale(UIElement sender, GRAPHICS spriteBatch, Entry e)
        {
            if (view == null) return;
            //var scale = Project.Document.ScaleMatrix;
            //var viewport = sender.FinalViewClip;
            //viewport.Size = VECTOR2.Transform(viewport.Size, scale);

            spriteBatch.BeginFromPrevious(ScaleMatrix);
        }
        private void EndScale(UIElement sender, GRAPHICS spriteBatch, Entry e)
        {
            if (view == null) return;
            spriteBatch.End();
        }
        private void ViewportDrawLines(UIElement sender, GRAPHICS spriteBatch, Entry e)
        {
            if (view == null)
                return;

            BeginScale(sender, spriteBatch, e);

            if (!drawBorder)
            {
                // draw border line
                ForParentPriority(EditingScene, null,
                    element =>
                    {
                        //if (element.IsVisible)
						spriteBatch.Draw(patchViewportElementBorder, element.ViewClip, GRAPHICS.NullSource, C.ColorBorder);
                    });
            }

            if (Editing)
            {
                UIElement currentParent = UIElement.FindChildPriority(EditingScene, ViewFindSkipSelected, t => t.IsContains(e.INPUT.Pointer.Position));
                if (currentParent != null)
                {
                    spriteBatch.Draw(patchViewportSelectedParentBorder, currentParent.ViewClip, GRAPHICS.NullSource, C.ColorFocusBorder);
                }
            }

            if (Selected != null)
            {
                EPivot mode = GetScaleMode(Selected, e);
                RECT focusArea = Selected.ViewClip;
                VECTOR2 scalerPosition = VECTOR2.Zero;
                VECTOR2 pivot = new VECTOR2(C.ScaleAnchorRadius);
                RECT scalerSource = new RECT(scalerPosition, pivot);
                // draw scaler
                for (int i = 0; i < 3; i++)
                {
                    scalerPosition.Y = focusArea.Y + focusArea.Height / 2.0f * i;

                    scalerSource.Y = i == 0 ? C.ScaleAnchorRadius : 0;
                    scalerSource.Height = i == 1 ? C.ScaleAnchorRadius * 2 : C.ScaleAnchorRadius;

                    pivot.Y = i * 0.5f;

                    for (int j = 0; j < 3; j++)
                    {
                        if (i == 1 && j == 1)
                            continue;

                        EPivot p = (EPivot)(j + (i << 4));
                        scalerPosition.X = focusArea.X + focusArea.Width / 2.0f * j;

                        scalerSource.X = j == 0 ? C.ScaleAnchorRadius : 0;
                        scalerSource.Width = j == 1 ? C.ScaleAnchorRadius * 2 : C.ScaleAnchorRadius;

                        pivot.X = j * 0.5f;

                        if (p == mode)
                            spriteBatch.Draw(Scaler, scalerPosition, scalerSource, C.ColorFocusBorder, 0, pivot, VECTOR2.One, EFlip.None);
                        else
                            spriteBatch.Draw(Scaler, scalerPosition, scalerSource, C.ColorBorder, 0, pivot, VECTOR2.One, EFlip.None);
                    }
                }
            }

            if (selectedPreview != null && selectedElement != null)
            {
                spriteBatch.Draw(selectedPreview,
                    e.INPUT.Pointer.Position - selectedElement.PivotPoint,
                    C.PreViewColor);
            }

            EndScale(sender, spriteBatch, e);
        }

        private void ResetViewport()
        {
            //pv.Pivot = EPivot.MiddleCenter;
            pv.Size = EditingScene.Size;
            pv.Location = pvc.Size;
            pvc.ContentScope = pv.Size + pvc.Size * 2;
            pvc.Offset = pvc.OffsetScope / 2;

            //pv.Pivot = EPivot.MiddleCenter;
            //pv.Size = EditingScene.Size;
            ////var size = EditingScene.Size * Project.Document.Scale;
            //var size = EditingScene.Size;
            //pvc.ContentScope = size + pvc.Size * 2;
            //pv.Location = pvc.ContentScope / 2;
            //pvc.Offset = pvc.Size - (pvc.Size - size) / 2;
        }
        private void BuildView(UIScene scene)
        {
            if (view != null)
            {
                //SaveView();
                UIScene current = EditingScene;
                pv.Remove(current);
                current.Dispose();
                Selected = null;
            }

            view = SetElement(scene) as View;
            
            Document_ScaleChanged(Project.Document.Scale);
            // ResetViewport后再添加场景，否则停靠会改变场景尺寸
            pv.Add(scene);
            ResetViewport();
            
        }
        private bool ViewFindSkip(UIElement e)
        {
            return !e.IsVisible ||
                e == pvc.ScrollBarHorizontal ||
                e == pvc.ScrollBarVertical;
        }
        private bool ViewFindSkipSelected(UIElement e)
        {
            return e == selected || !e.IsVisible;
        }
        private bool ViewFindSkipOther(UIElement e)
        {
            return e == selected || e == pvc || ViewFindSkip(e);
        }
        private void SetElement(UIElement target, VECTOR2 location)
        {
            UIElement container = (UIElement)UIElement.FindChildPriority(pv, ViewFindSkip,
                ui => ui.IsContains(location));
            if (container != null)
            {
                container.Add(target);
                if (!(target.Tag is Widget))
                {
                    SetElement(target);
                }
                // 设置坐标要和编辑对象使用同一机制，刷新编辑器Clip的值
                SetElementLocation(target, container.ConvertGraphicsToLocal(location));
            }
        }
        private void SetElementLocation(UIElement element, VECTOR2 location)
        {
            if (location.IsNaN())
                return;
            element.Location = location;
            RefreshElementClip(element);
        }
        private void SetElementSize(UIElement element, VECTOR2 size)
        {
            element.Size = size;
            RefreshElementClip(element);
        }
        private void RefreshElementClip(UIElement element)
        {
            IEditorType editor = ((Widget)element.Tag).Target;
            EditorVariable target = editor.GetEditors().FirstOrDefault(e => e.Variable.VariableName == "Clip");
            if (target != null)
            {
                target.CheckValueChanged();
            }
        }
        private EPivot GetScaleMode(UIElement element, Entry e)
        {
            if (pivots == null)
            {
                Array array = Enum.GetValues(typeof(EPivot));
                pivots = new EPivot[array.Length];
                for (int i = 0; i < array.Length; i++)
                {
                    pivots[i] = (EPivot)array.GetValue(i);
                }
                pivots = pivots.Remove(array.Length / 2);
            }
            VECTOR2 location = element.ConvertGraphicsToLocal(e.INPUT.Pointer.Position);

            VECTOR2 size = element.Size;
            for (int i = 0; i < pivots.Length; i++)
            {
                CIRCLE circle = new CIRCLE(C.ScaleAnchorRadius, UIElement.CalcPivotPoint(size, pivots[i]));
                if (circle.Contains(location))
                {
                    return pivots[i];
                }
            }
            return EPivot.MiddleCenter;
        }
        private void AutoAlign(Entry e)
        {
            EPivot pivot = editScaleMode.Value;
            int x = Utility.EnumLow4((int)pivot);
            int y = Utility.EnumHigh4((int)pivot);
            VECTOR2 cp = e.INPUT.Pointer.Position;
            NearAlign[] near = new NearAlign[6];
            if (pivot == EPivot.MiddleCenter)
            {
                // 按住Shift只横向或纵向移动
                if (e.INPUT.Keyboard.IsPressed((int)PCKeys.LeftShift))
                {
                    VECTOR2 refference = e.INPUT.Pointer.ClickPosition;
                    VECTOR2 difference;
                    difference.X = _MATH.Distance(cp.X, refference.X);
                    difference.Y = _MATH.Distance(cp.Y, refference.Y);
                    if (difference.X > difference.Y)
                        cp.Y = refference.Y;
                    else
                        cp.X = refference.X;
                }
                cp += (selected.PivotPoint - editStartPoint);
                VECTOR2 ltScreenPoint = e.INPUT.Pointer.Position - editStartPoint;
                for (int i = 0; i < 3; i++)
                {
                    near[i] = new NearAlign()
                    {
                        Vertical = true,
                        Pivot = i,
                        Alignment = ltScreenPoint.X + selected.Width / 2 * i,
                    };
                }
                for (int i = 0; i < 3; i++)
                {
                    near[3 + i] = new NearAlign()
                    {
                        Vertical = false,
                        Pivot = i,
                        Alignment = ltScreenPoint.Y + selected.Height / 2 * i,
                    };
                }
            }
            else
            {
                VECTOR2 target = cp - editStartPoint + UIElement.CalcPivotPoint(editStartParam.Size, pivot);
                // 缩放时，只针对缩放的边进行对齐
                if (x != 1)
                {
                    near[x] = new NearAlign()
                    {
                        Vertical = true,
                        Pivot = x,
                        Alignment = target.X
                    };
                }
                if (y != 1)
                {
                    near[3 + y] = new NearAlign()
                    {
                        Vertical = false,
                        Pivot = y,
                        Alignment = target.Y
                    };
                }
            }
            // 根据这个控件在屏幕的坐标（左上，编辑后的）与其它控件对齐坐标
            List<UIElement> all = UIElement.Elements(EditingScene, ViewFindSkipOther);
            VECTOR2 nearMin = new VECTOR2(C.NearDistance);
            NearAlign temp;
            foreach (UIElement item in all)
            {
                VECTOR2 lt = item.ConvertLocalToGraphics(VECTOR2.Zero);
                for (int j = 0; j < 3; j++)
                {
                    VECTOR2 value = new VECTOR2(
                        lt.X + item.Width / 2 * j,
                        lt.Y + item.Height / 2 * j);
                    // x
                    for (int i = 0; i < 3; i++)
                    {
                        temp = near[i];
                        if (temp == null)
                            continue;
                        float diff = _MATH.Distance(value.X, temp.Alignment);
                        if (diff < nearMin.X)
                        {
                            temp.AlignTargets.Clear();
                            nearMin.X = diff;
                        }
                        if (diff == nearMin.X)
                        {
                            temp.Result = value.X;
                            temp.AlignTargets.Add(item);
                        }
                    }
                    // y
                    for (int i = 0; i < 3; i++)
                    {
                        temp = near[i + 3];
                        if (temp == null)
                            continue;
                        float diff = _MATH.Distance(value.Y, temp.Alignment);
                        if (diff < nearMin.Y)
                        {
                            temp.AlignTargets.Clear();
                            nearMin.Y = diff;
                        }
                        if (diff == nearMin.Y)
                        {
                            temp.Result = value.Y;
                            temp.AlignTargets.Add(item);
                        }
                    }
                }
            }

            // 按住Ctrl取消自动对齐
            bool cancelAuto = e.INPUT.Keyboard.IsPressed((int)PCKeys.LeftControl) ||
                e.INPUT.Keyboard.IsPressed((int)PCKeys.RightControl);

            nears = near.Where(n => n != null && n.AlignTargets.Count > 0 &&
                (!cancelAuto || _MATH.Distance(n.Result, n.Alignment) < 1)).ToList();

            VECTOR2 result = cp;
            if (nears.Count > 0 && !cancelAuto)
            {
                temp = nears.FirstOrDefault(n => n.Vertical);
                if (temp != null)
                {
                    result.X = temp.Result;
                    if (pivot == EPivot.MiddleCenter)
                    {
                        result.X -= selected.Width / 2 * (temp.Pivot - selected.PivotAlignmentX);
                    }
                }
                temp = nears.FirstOrDefault(n => !n.Vertical);
                if (temp != null)
                {
                    result.Y = temp.Result;
                    if (pivot == EPivot.MiddleCenter)
                    {
                        result.Y -= selected.Height / 2 * (temp.Pivot - selected.PivotAlignmentY);
                    }
                }
            }

            if (pivot == EPivot.MiddleCenter)
            {
                SetElementLocation(selected, selected.ConvertGraphicsToLocalView(result));
            }
            else
            {
                RECT clip;
                VECTOR2 scale = result + editStartPoint - UIElement.CalcPivotPoint(editStartParam.Size, pivot) - e.INPUT.Pointer.ClickPosition;

                clip.X = scale.X * (x - 1) * ((selected.PivotAlignmentX - (2 - x)) / 2.0f);
                clip.Y = scale.Y * (y - 1) * ((selected.PivotAlignmentY - (2 - y)) / 2.0f);
                clip.Width = scale.X * (x - 1);
                clip.Height = scale.Y * (y - 1);

                // 按住Shift等比缩放
                if (x != 1 && y != 1 &&
                    editStartParam.Width != 0 && editStartParam.Height != 0 &&
                    e.INPUT.Keyboard.IsPressed((int)PCKeys.LeftShift))
                {
                    float rate = editStartParam.Width / editStartParam.Height;
                    if (_MATH.Abs(clip.Width) > _MATH.Abs(clip.Height))
                    {
                        float target = clip.Width / rate;
                        if (clip.Y != 0)
                            clip.Y = target / (clip.Height / clip.Y);
                        clip.Height = target;
                    }
                    else
                    {
                        float target = clip.Height * rate;
                        if (clip.X != 0)
                            clip.X = target / (clip.Width / clip.X);
                        clip.Width = target;
                    }
                }

                clip.X += editStartParam.X;
                clip.Y += editStartParam.Y;
                clip.Width += editStartParam.Width;
                clip.Height += editStartParam.Height;

                SetElementSize(selected, clip.Size);
                SetElementLocation(selected, clip.Location);
            }
        }
        private UIElement CopyElement(UIElement element)
        {
            Widget widget = element.Tag as Widget;
            if (widget == null)
                return null;

            byte[] data = SaveUI(widget, true);
            //throw new NotImplementedException();
            //return InternalLoadUI(new ByteReader(data), true);
            return LoadUI(data, true);
        }
        private void DeleteSelected()
        {
            selected.Parent.Remove(selected);
            selected = null;
        }

        public static void PhotoElement(UIElement sender, string fileName)
        {
            PhotoElement(sender,
                texture =>
                {
                    texture.Save(fileName);
                    // thumbnail
                    var parameter = Microsoft.Xna.Framework.Graphics.Texture2D.GetCreationParameters(XnaGate.Gate.GraphicsDevice, fileName);
                    float scale;
                    VECTOR2 offset;
                    __GRAPHICS.ViewAdapt(new VECTOR2(parameter.Width, parameter.Height),
                        new VECTOR2(PREVIEW_WIDTH, PREVIEW_HEIGHT),
                        out scale, out offset);
                    parameter.Width = (int)(parameter.Width * scale);
                    parameter.Height = (int)(parameter.Height * scale);
                    using (var preview = Microsoft.Xna.Framework.Graphics.Texture2D.FromFile(XnaGate.Gate.GraphicsDevice, fileName, parameter))
                        preview.Save(
                            string.Format("{0}\\{1}.{2}.{3}",
                            DIR_PREVIEW,
                            Path.GetFileNameWithoutExtension(fileName),
                            sender.GetType().Name,
                            SUFFIX_TEXTURE), Microsoft.Xna.Framework.Graphics.ImageFileFormat.Png);
                });
        }
        public static void PhotoElement(UIElement sender, Action<TEXTURE> onShot)
        {
            sender.DrawBeforeBegin = (s, sb, e) =>
            {
                sb.BeginScreenshot(sender.ViewClip);
            };
            sender.DrawAfterEnd = (s, sb, e) =>
            {
                sender.DrawBeforeBegin = null;
                sender.DrawAfterEnd = null;
                if (onShot != null)
                    onShot(sb.EndScreenshot());
            };
        }
    }
}
