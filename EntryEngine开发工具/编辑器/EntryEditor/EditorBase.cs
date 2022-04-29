using System;
using System.Collections.Generic;
using System.Reflection;
using EntryEngine;
using EntryEngine.UI;
using EntryEngine.Serialize;
using EntryEngine.Xna;
using System.IO;

namespace EntryEditor
{
	// Log
	public interface IOperation
	{
		string OperationName { get; }
        string OperationContent { get; }

		void Operate();
		void Undo();
	}
	public static class OperationLog
	{
		private static List<IOperation> operations = new List<IOperation>();
		private static Stack<IOperation> redos = new Stack<IOperation>();

		public static IOperation[] OperationHistory
		{
			get { return operations.ToArray(); }
		}
		public static IOperation[] OperationHistoryUndone
		{
			get { return redos.ToArray(); }
		}

		public static void Operate(IOperation operation)
		{
			InternalOperate(operation, true);
		}
		private static void InternalOperate(IOperation operation, bool clearRedo)
		{
			if (clearRedo)
			{
				redos.Clear();
			}
			if (operations.Count >= EditorConfig.OperationLogCount)
			{
				operations.RemoveAt(0);
			}
			operations.Add(operation);
		}
		public static void Undo()
		{
			Undo(1);
		}
		public static void Undo(int count)
		{
			count = Utility.Min(count, operations.Count);
			for (int i = 0; i < count; i++)
			{
				IOperation operation = operations.Last();
				operations.RemoveLast();

				operation.Undo();

				redos.Push(operation);
			}
		}
		public static void Redo()
		{
			Redo(1);
		}
		public static void Redo(int count)
		{
			count = Utility.Min(count, redos.Count);
			for (int i = 0; i < count; i++)
			{
				InternalOperate(redos.Pop(), false);
			}
		}
		public static void Clear()
		{
			operations.Clear();
			redos.Clear();
		}
	}
    public class OperationHistoryScene : UIScene
    {
    }

	// Gate
    public class MenuStrip : UIElement
    {
    }
    public abstract class EditorEntry : UIScene
    {
        public EditorEntry()
        {
			Initialize(CreateMenuStrip());
            Entry.AddMainScene(this);
        }
        protected virtual MenuStrip CreateMenuStrip()
        {
			return null;
			//throw new NotImplementedException();
        }
        protected virtual void Initialize(MenuStrip strip)
        {
			//AddChildFirst(strip);
        }

        private static Assembly[] plugins;
        public static Assembly[] LoadPlugins()
        {
            if (plugins == null)
            {
                string[] files = Directory.GetFiles("Plug-ins", "*.dll", SearchOption.AllDirectories);
                int count = files.Length;
                plugins = new Assembly[count];
                for (int i = 0; i < count; i++)
                {
                    try
                    {
						files[i] = Path.GetFullPath(files[i]);
						plugins[i] = Assembly.LoadFile(files[i]);
                    }
                    catch (Exception ex)
                    {
                        Log.Error("load plug-in {0} error! msg={1}", files[i], ex.Message);
                    }
                }
            }
            return plugins;
        }
    }
    public class EditorUI : EditorEntry
    {
		protected override void Initialize(MenuStrip strip)
        {
			base.Initialize(strip);

			if (NinePatch == null)
			{
				var t2d = new Microsoft.Xna.Framework.Graphics.Texture2D(EntryXna.Entry.GraphicsDevice, 3, 3);
				byte[] color = new byte[36];
				for (int i = 0; i < 36; i++)
				{
					color[i] = 255;
				}
				t2d.SetData(color);
				NinePatch = new TextureXna(t2d);
			}
        }

		protected static EETexture NinePatch;

		public static UIPatch GetNinePatch(EEColor? borderColor, EEColor? bodyColor)
		{
			UIPatch patch = new UIPatch();
			patch.Texture = NinePatch;
			patch.NinePatch = new NinePatch(new EERectangle(0, 0, 3, 3), 1, 1, 2, 2);
			patch.Color = borderColor;
			patch.BodyColor = bodyColor;
			return patch;
		}

        public static string SaveUIElement(IEditorType target)
        {
            if (target == null)
                throw new ArgumentNullException();

            XmlWriter writer = new XmlWriter();
			writer.WriteNode("Type", target.Instance.GetType().AssemblyQualifiedName);
			writer.WriteNode(EEXml.ROOT);
            foreach (IVariable item in target.GetVariables())
            {
				writer.WriteNode(item.VariableName);
				writer.WriteObject(item.GetValue(), item.Type);
				writer.WriteNodeClose(item.VariableName);
            }
			writer.WriteNodeClose(EEXml.ROOT);
            return writer.Result;
        }
        public static UIElement LoadUIElement(string file)
        {
            XmlReader reader = new XmlReader(System.IO.File.ReadAllText(file));
            return (UIElement)reader.ReadObject();
        }
		public static EditorVariable BuildTypeEditor(Type type)
		{
			throw new NotImplementedException();
		}
    }
	public class SceneLoad : UIScene
	{
		private static ContentManager content;
		private Label lblProcess = new Label();
		private TextureBox bg = new TextureBox();

		public SceneLoad()
		{
			ContentType = ESceneContent.New;

			bg.Location = Entry.GraphicsSize / 2;
			bg.Pivot = EPivot.MiddleCenter;

			lblProcess.Font = new FontGUIP();

			if (content == null)
			{
				content = Entry.Content();
			}
		}

		public IEnumerable<ICoroutine> LoadQueue(IEnumerable<CoroutineLoadProcess> loadQueue, string bgFile)
		{
			if (Entry.Scene != null)
			{
				Entry.RemoveMainScene(Entry.Scene);
			}

			State = ESceneState.Block;
			Show();

			try
			{
				if (!string.IsNullOrEmpty(bgFile))
				{
					bg.Texture = content.LoadTexture(bgFile);
				}
			}
			catch (Exception ex)
			{
				Log.Error("SceneLoad load bg error! bg={0} ex={1}", bgFile, ex.Message);
			}
			yield return null;

			foreach (CoroutineLoadProcess item in loadQueue)
			{
				lblProcess.Text = item.ProcessName;
				yield return item;
			}

			content.Dispose();
			lblProcess.Font.Dispose();

			State = ESceneState.Dispose;
		}
	}
	public struct CoroutineLoadProcess : ICoroutine
	{
		public string ProcessName;
		public bool IsEnd
		{
			get { return true; }
		}
		public CoroutineLoadProcess(string processName)
		{
			this.ProcessName = processName;
		}
		public void Update(GameTime time)
		{
		}
	}

	// Type
    public interface IEditorType
    {
        object Instance { get; set; }
        IEnumerable<IVariable> GetVariables();
		IEnumerable<EditorVariable> CreateEditor();
    }
    public abstract class EditorType<T> : IEditorType
    {
        public T InstanceTyped;

        public object Instance
        {
            get { return InstanceTyped; }
            set { InstanceTyped = (T)value; }
        }
		public Type Type
		{
			get { return InstanceTyped == null ? typeof(T) : InstanceTyped.GetType(); }
		}

        public virtual IEnumerable<IVariable> GetVariables()
        {
            yield return new VariableValue<T>(InstanceTyped);
        }
		public virtual IEnumerable<EditorVariable> CreateEditor()
        {
            throw new NotImplementedException();
        }
    }
    public class EditorBaseType<T> : EditorType<T>
    {
		private Dictionary<IVariable, EditorVariable> variables;
		private Dictionary<IVariable, EditorVariable> auto;

		public EditorBaseType()
		{
		}
		public EditorBaseType(T target)
		{
			InstanceTyped = target;
		}

		private EditorVariable AutoBuildEditor()
		{
			// to create the default editor of the type
			return EditorUI.BuildTypeEditor(Type);
		}
		private void AutoBuildType()
		{
			auto = new Dictionary<IVariable, EditorVariable>();

			SerializeSetting setting = SerializeSetting.DefaultSetting;
			setting.Property = true;
			setting.Static = InstanceTyped == null;
			setting.SerializeField(Type, field =>
				{
					InternalAddVariable(field.Name, EditorUI.BuildTypeEditor(field.FieldType), auto);
				});
			setting.SerializeProperty(Type, field =>
				{
					InternalAddVariable(field.Name, EditorUI.BuildTypeEditor(field.PropertyType), auto);
				});
		}
		private void InternalAddVariable(string name, EditorVariable editor, Dictionary<IVariable, EditorVariable> target)
		{
			if (editor == null)
			{
				throw new NotImplementedException(string.Format("can not find the editor by type={0} field={1}", typeof(T).Name, name));
			}
			target.Add(new VariableObject<T>(InstanceTyped, name), editor);
		}
        public void AddVariable(string name)
        {
            AddVariable(name, AutoBuildEditor());
        }
		public void AddVariable(string name, EditorVariable editor)
        {
			if (variables == null)
			{
				variables = new Dictionary<IVariable, EditorVariable>();
				if (auto != null)
				{
					auto.Clear();
					auto = null;
				}
			}
			if (editor == null)
			{
				editor = AutoBuildEditor();
			}
			InternalAddVariable(name, editor, variables);
        }
        public sealed override IEnumerable<IVariable> GetVariables()
        {
			if (variables == null)
			{
				if (auto == null)
				{
					AutoBuildType();
				}
				return auto.Keys;
			}
            return variables.Keys;
        }
		public sealed override IEnumerable<EditorVariable> CreateEditor()
        {
			if (variables == null)
			{
				if (auto == null)
				{
					AutoBuildType();
				}
				return auto.Values;
			}
            return variables.Values;
        }
    }
	public class TypeUIElement : EditorBaseType<UIElement>
	{
		public TypeUIElement(UIElement element) : base(element)
		{
			AddVariable("Name", null);
			AddVariable("Enable", null);
			AddVariable("Eventable", null);
			AddVariable("Visible", null);
			AddVariable("Anchor", null);
			AddVariable("Shader", null);
			AddVariable("Clip", null);
			AddVariable("Pivot", null);
			AddVariable("Angle", null);
			AddVariable("FlipH", null);
			AddVariable("FlipV", null);
			AddVariable("SortZ", null);
			AddVariable("Alpha", null);
		}
	}

	// Editor
	public abstract class EditorVariable : Panel
	{
		private IVariable variable;
		private object valueMonitor;

		public IVariable Variable
		{
			get { return variable; }
			set
			{
				variable = value;
				SetValue();
			}
		}
		public object VariableValue
		{
			get
			{
				if (variable == null)
					return null;
				return variable.GetValue();
			}
			set
			{
				if (variable == null)
					return;
				variable.SetValue(value);
				SetValue();
			}
		}
		public virtual string VariableStringValue
		{
			get
			{
				object value = VariableValue;
				if (value == null)
					return string.Empty;
				else
					return value.ToString();
			}
		}

		public void Load(ContentManager content)
		{
			if (content == null)
				throw new ArgumentNullException("content");
			InternalLoad(content);
			SetValue();
		}
		protected virtual void InternalLoad(ContentManager content)
		{
		}
		protected abstract void SetValue();
		protected override void InternalUpdate(EEEventArgs e)
		{
			object value = VariableValue;
			if (!object.Equals(valueMonitor, value))
			{
				SetValue();
				valueMonitor = value;
			}
		}
	}
	public abstract class EditorLabelLayout : EditorVariable
	{
		private Label label = new Label();

		public EditorLabelLayout()
		{
			this.Background = EditorUI.GetNinePatch(BGBorderColor, BGBodyColor);
			this.Hover += HoverArea;
			this.UnHover += UnHoverArea;

			label.X = 4;
			label.Pivot = EPivot.MiddleLeft;
			AddChildFirst(label);
		}

		protected override void SetValue()
		{
			if (Variable != null)
			{
				label.Text = Variable.VariableName;
				label.Y = label.ContentSize.Y / 2;
			}
		}

		public static EEColor BGBorderColor = new EEColor(128, 128, 128, 222);
		public static EEColor BGBorderHoverColor = EEColor.Silver;
		public static EEColor BGBodyColor = new EEColor(128, 128, 128, 255);

		private static void HoverArea(UIElement sender, EEEventArgs e)
		{
			((Panel)sender).Background.Color = BGBorderHoverColor;
		}
		private static void UnHoverArea(UIElement sender, EEEventArgs e)
		{
			((Panel)sender).Background.Color = BGBorderColor;
		}
	}
	public class EditorTextBox : EditorLabelLayout
	{
		private TextBox textBox = new TextBox();

		protected TextBox TextBox
		{
			get { return textBox; }
		}

		public EditorTextBox()
		{
			textBox.Width = 120;
			textBox.Height = 30;
			textBox.Background = EditorUI.GetNinePatch(EEColor.Gray, EEColor.White);
			textBox.X = 20;
			textBox.Y = 30;
			textBox.Padding = new EEVector2(10, 4);
			textBox.FontColor = EEColor.Black;
			AddChildFirst(textBox);

			InternalInit();
		}

		protected virtual void InternalInit()
		{
			TextBox.TextChanged += TextChangedModifyValue;
		}
		protected virtual void TextChangedModifyValue(Label sender, EEEventArgs e)
		{
			VariableValue = sender.Text;
		}
		protected override void SetValue()
		{
			base.SetValue();
			textBox.Text = VariableStringValue;
		}
	}
	public class EditorNumber : EditorTextBox
	{
		private decimal minValue;
		private decimal maxValue;
		/// <summary>
		/// y-1px时对应的Value变化
		/// </summary>
		public decimal DragStep = 1;

		public decimal MinValue
		{
			get { return minValue; }
			set
			{
				minValue = value;

				if (minValue > maxValue)
					maxValue = minValue;

				if (minValue > Value)
					Value = minValue;
			}
		}
		public decimal MaxValue
		{
			get { return maxValue; }
			set
			{
				maxValue = value;

				if (maxValue < minValue)
					minValue = maxValue;

				if (maxValue < Value)
					Value = maxValue;
			}
		}
		public decimal Value
		{
			get
			{
				object value = VariableValue;
				if (value == null)
					return 0;
				else
					return Convert.ToDecimal(value);
			}
			set
			{
				value = value > maxValue ? maxValue : value;
				value = value < minValue ? minValue : value;
				VariableValue = Convert.ChangeType(value, Variable.Type);
			}
		}

		public EditorNumber()
		{
			InternalInit();
		}
		public EditorNumber(IVariable variable)
			: this()
		{
			this.Variable = variable;
		}
		public EditorNumber(decimal step)
			: this()
		{
			this.DragStep = step;
		}
		public EditorNumber(IVariable variable, decimal step)
			: this()
		{
			this.DragStep = step;
			this.Variable = variable;
		}
		public EditorNumber(IVariable variable, decimal min, decimal max, decimal step)
		{
			this.DragStep = step;
			this.MinValue = min;
			this.MaxValue = max;
			this.Variable = variable;
			InternalInit();
		}

		protected override void InternalInit()
		{
			base.InternalInit();
			this.TextBox.Drag += DragModifyValue;
		}
		protected void SetDefaultScope()
		{
			Type type = Variable.Type;
			try
			{
				if (type == typeof(float) || type == typeof(double))
				{
					MinValue = long.MinValue;
					MaxValue = long.MaxValue;
				}
				else
				{
					FieldInfo const1 = type.GetField("MaxValue");
					FieldInfo const2 = type.GetField("MinValue");

					MaxValue = (decimal)const1.GetValue(null);
					MinValue = (decimal)const2.GetValue(null);
				}
			}
			catch (Exception ex)
			{
				Log.Error("typeof({0}) get MaxValue and MinValue error! msg={1}", type.Name, ex.Message);
			}
		}
		protected void DragModifyValue(UIElement sender, EEEventArgs e)
		{
			EEVector2 moved = e.Pointer.Moved;
			if (moved != EEVector2.Zero && !moved.IsNaN())
			{
				Value += (decimal)moved.Y * DragStep;
			}
		}
		protected override void TextChangedModifyValue(Label sender, EEEventArgs e)
		{
			decimal value;
			if (decimal.TryParse(sender.Text, out value))
			{
				Value = value;
			}
			else
			{
				SetValue();
			}
		}
		protected override void SetValue()
		{
			if (Variable != null)
			{
				if (minValue == 0 && maxValue == 0)
				{
					SetDefaultScope();
				}
			}

			base.SetValue();
		}
	}

	// Container
}
