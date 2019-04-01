using System;
using System.Collections.Generic;
using System.IO;
using EntryEditor;
using EntryEngine;
using EntryEngine.Serialize;
using EntryEngine.UI;
using EntryEngine.Xna;

namespace EditorUI
{
    public partial class EditorUI
    {
        private static List<Type> typeEditors;
		protected static TEXTURE TreeNodeOpenPatch;
		protected static TEXTURE TreeNodeClosePatch;
        protected static TEXTURE Scaler;

        private void InitializeEditorResource()
        {
            if (File.Exists(FILE_CONSTANT))
                new XmlReader(File.ReadAllText(FILE_CONSTANT)).ReadStatic(typeof(C));
            else
                File.WriteAllText(FILE_CONSTANT, new XmlWriter().WriteStatic(typeof(C)));
            UtilityEditor.OnExit = () =>
            {
                Environment.CurrentDirectory = DirectoryEditor;
                File.WriteAllText(FILE_CONSTANT, new XmlWriter().WriteStatic(typeof(C)));

                if (Project != null)
                {
                    //Environment.CurrentDirectory = DirectoryProject;
                    SaveProject();
                }

            };

            if (!string.IsNullOrEmpty(C.TypeEditorGenerator))
            {
                // generate Generator from type
            }

            if (typeEditors == null)
            {
                typeEditors = UtilityEditor.LoadPlugins().GetTypes(typeof(IEditorType));
            }

            Scaler = EditorContent.Load<TEXTURE>("Scaler.png");
        }

		public static PATCH GetNinePatchTitleColor()
        {
            return GetNinePatch(new COLOR(90, 90, 90, 255), new COLOR(222, 222, 222, 255));
        }
		public static PATCH GetNinePatchBodyColor()
        {
            return GetNinePatch(new COLOR(90, 90, 90, 255), new COLOR(247, 247, 247, 255));
        }
		public static PATCH GetNinePatch(COLOR? borderColor, COLOR? bodyColor)
        {
			return PATCH.GetNinePatch(borderColor.HasValue ? bodyColor.Value : PATCH.NullColor, borderColor.HasValue ? borderColor.Value : PATCH.NullColor, 1);
        }
		public static TEXTURE GetTreeNodeOpenPatch()
        {
            if (TreeNodeOpenPatch == null)
            {
                TreeNodeOpenPatch = EditorContent.Load<TEXTURE>("open.png");
            }
            return TreeNodeOpenPatch;
        }
		public static TEXTURE GetTreeNodeClosePatch()
        {
            if (TreeNodeClosePatch == null)
            {
                TreeNodeClosePatch = EditorContent.Load<TEXTURE>("close.png");
            }
            return TreeNodeClosePatch;
        }
        public static ScrollBarBase GetScrollBar(bool vertical)
        {
            ScrollBarBase bar;
            if (vertical)
            {
                bar = new ScrollBarVertical();
                bar.Height = 200;
                bar.Width = 20;
                bar.Bar = GetNinePatch(new COLOR(90, 90, 90, 255), new COLOR(112, 112, 112, 255));
				Button body = new Button();
                body.SourceNormal = GetNinePatchTitleColor();
                bar.Body = body;
                body.Width = 18;
            }
            else
            {
                bar = new ScrollBarHorizontal();
                bar.Width = 200;
                bar.Height = 20;
                bar.Bar = GetNinePatch(new COLOR(90, 90, 90, 255), new COLOR(112, 112, 112, 255));
				Button body = new Button();
                body.SourceNormal = GetNinePatchTitleColor();
                bar.Body = body;
                body.Height = 18;
            }
            bar.StepClickBar = 200;
            bar.StepScroll = bar.StepClickBar;
            return bar;
        }
    }
}
