using System;
using System.Collections.Generic;
using System.Linq;
using EntryEditor;
using EntryEngine;
using EntryEngine.UI;

namespace EditorUI
{
    public partial class EditorUI
    {
        private Panel ppc;
        private Panel pp;

        private void InitializeProperty()
        {
            ppc = new Panel();
            ppc.SortZ = 10;
            ppc.Anchor = EAnchor.Top | EAnchor.Bottom | EAnchor.Right;
            ppc.Name = "Panel Property Container";
            ppc.X = Entry.GRAPHICS.ScreenSize.X;
            ppc.Y = ms.Height;
            ppc.Width = 300;
            ppc.Height = Entry.GRAPHICS.ScreenSize.Y - ms.Height;
            ppc.Pivot = EPivot.TopRight;
            ppc.Background = GetNinePatchTitleColor();
            //panelPropertyContainer.BackgroundInner = GetNinePatchBodyColor();
            //panelPropertyContainer.BackgroundCrossScrollBar = GetNinePatchTitleColor();
            ppc.ScrollOrientation = EScrollOrientation.VerticalAuto;
            ScrollBarBase scrollbar = GetScrollBar(true);
            scrollbar.Pivot = EPivot.TopRight;
            scrollbar.Anchor = EAnchor.Top | EAnchor.Right | EAnchor.Bottom;
            scrollbar.X = ppc.Width;
            scrollbar.Height = ppc.Height;
            ppc.ScrollBarVertical = scrollbar;
            ppc.Add(scrollbar);

            pp = new Panel();
            pp.Name = "Panel Property";
            pp.Width = 300;
            //panelProperty.Height = Entry.ScreenSize.Y - menuStrip.Height;
            pp.Background = GetNinePatchTitleColor();
            //panelProperty.BackgroundInner = GetNinePatchBodyColor();
            //panelProperty.BackgroundCrossScrollBar = GetNinePatchTitleColor();
            pp.ScrollOrientation = EScrollOrientation.VerticalAuto;
            pp.ContentSizeChanged += PropertyPanelLayout;

            ppc.Width += ppc.ScrollBarVertical.Width;
            ppc.Add(pp);
            this.Add(ppc);
        }

        private static Widget SetElement(UIElement element)
        {
            if (element.Tag is Widget)
                return (Widget)element.Tag;

            IEditorType editorType = BuildPropertyEditor(element);
            foreach (var item in editorType.GetEditors())
                item.ContentSizeChanged += Instance.PropertyPanelLayout;
            element.Eventable = false;

            Widget widget;
            if (element is UIScene)
                widget = new View();
            else
                widget = new Widget();
            widget.Target = editorType;
            if (element.Tag != null && element.Tag is string)
                widget.FilePath = element.Tag.ToString();
            element.Tag = widget;
            for (int i = 0; i < element.ChildCount; i++)
            {
                var child = element[i];
                if (child.Tag == null)
                {
                    SetElement(child);
                    if (string.IsNullOrEmpty(child.Name))
                    {
                        Instance.BuildElementName(child);
                    }
                }
            }
            return widget;
        }
        private void PropertyPanelLayout(VECTOR2 size)
        {
            if (selected != null)
            {
                IEditorType editorType = (selected.Tag as Widget).Target;

                float y = 0;
                foreach (var item in editorType.GetEditors())
                {
                    item.Width = pp.Width;
                    item.Y = y;
                    if (size.IsNaN())
                        pp.Add(item);
                    y += item.ContentSize.Y;
                }
            }
        }
    }
}
