using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EntryEngine.UI;
using EntryEngine.Serialize;
using EntryEngine;
using System.Reflection;
using EntryEditor;
using System.Collections;

namespace EntryEditor
{
    public abstract class EditorVariable : Panel
    {
        public static float WIDTH = 100;
        public static float HEIGHT = 21;
        public static FONT FONT = FONT.Default;
        public static ContentManager CONTENT = Entry._ContentManager;
        public static Generator GENERATOR = new Generator();

        private IVariable variable;
        private object valueMonitor;
        public event Action<IVariable> OnSetVariable;
        public event Action<EditorVariable> ValueChanged;
        public event Func<object, object> OnGetValue;

        public IVariable Variable
        {
            get { return variable; }
            set
            {
                if (variable == value)
                    return;
                variable = value;
                if (OnSetVariable != null)
                    OnSetVariable(value);
                if (value == null || value.Instance == null)
                    return;
                SetValue();
            }
        }
        public object VariableValue
        {
            get
            {
                if (variable == null || variable.Instance == null)
                    return null;
                object value = variable.GetValue();
                if (OnGetValue != null)
                    value = OnGetValue(value);
                return value;
            }
            set
            {
                if (variable == null || object.Equals(VariableValue, value))
                    //if (variable == null || VariableValue.Equals(value))
                    return;
                variable.SetValue(value);
                SetValue();
                if (ValueChanged != null)
                    ValueChanged(this);
            }
        }
        public virtual string VariableStringValue
        {
            get
            {
                object value = VariableValue;
                if (value == null)
                    //return string.Empty;
                    return null;
                else
                    return value.ToString();
            }
        }

        /// <summary>内部更改值，会一并修改valueMonitor</summary>
        protected void InternalSetValue(object value)
        {
            variable.SetValue(value);
            valueMonitor = value;
        }
        protected abstract void SetValue();
        protected override void InternalUpdate(Entry e)
        {
            CheckValueChanged();
        }
        public void CheckValueChanged()
        {
            object value = VariableValue;
            if (!object.Equals(valueMonitor, value))
            {
                SetValue();
                valueMonitor = value;
            }
        }
    }
    public class EditorNullable : EditorVariable
    {
        private EditorVariable valueEditor;
        private CheckBox valueCheckBox;

        public bool IsNull
        {
            get { return VariableValue == null; }
        }
        public Type ValueType
        {
            get { return Nullable.GetUnderlyingType(Variable.Type); }
        }

        public EditorNullable(EditorVariable valueEditor)
        {
            if (valueEditor == null)
                throw new ArgumentNullException("valueEditor");

            valueCheckBox = new CheckBox();
            valueCheckBox.SourceNormal = PATCH.GetNinePatch(COLOR.TransparentBlack, COLOR.Gold, 1);
            valueCheckBox.SourceClicked = PATCH.GetNinePatch(COLOR.GreenYellow, COLOR.TransparentBlack, 1);
            valueCheckBox.CheckedChanged += SetValue;
            valueCheckBox.Height = HEIGHT;
            valueCheckBox.Width = HEIGHT;
            valueCheckBox.Pivot = EPivot.TopRight;
            Insert(valueCheckBox, 0);

            this.valueEditor = valueEditor;
            valueEditor.Background = null;
            valueEditor.X = HEIGHT;
            valueEditor.ValueChanged += ResetValue;
            Insert(valueEditor, 0);
        }

        protected override void SetValue()
        {
            bool hasValue = !IsNull;
            if (hasValue)
            {
                valueEditor.Visible = true;
                valueEditor.Variable = new VariableValue(VariableValue);
            }
            else
            {
                valueEditor.Visible = false;
            }
            valueCheckBox.Checked = hasValue;
        }
        private void SetValue(Button sender, Entry e)
        {
            if (sender.Checked)
            {
                if (IsNull)
                {
                    VariableValue = Activator.CreateInstance(ValueType);
                }
            }
            else
            {
                VariableValue = null;
            }
        }
        private void ResetValue(EditorVariable sender)
        {
            if (valueCheckBox.Checked)
            {
                VariableValue = sender.VariableValue;
            }
        }
    }
    public class EditorObject : EditorVariable
    {
        public IEnumerable<EditorVariable> Editors
        {
            get
            {
                //foreach (UIElement child in Childs)
                for (int i = 0; i < Childs.Count; i++)
                {
                    var child = Childs[i];
                    EditorVariable editor = child as EditorVariable;
                    if (editor != null)
                    {
                        yield return editor;
                    }
                }
            }
        }

        public EditorObject()
        {
            OnSetVariable += new Action<IVariable>(EditorObject_OnSetVariable);
        }

        private void EditorObject_OnSetVariable(IVariable obj)
        {
            foreach (EditorVariable item in Editors)
            {
                item.ValueChanged -= ResetValue;
                item.ValueChanged += ResetValue;
            }
        }
        protected override void SetValue()
        {
            ResetLayout(VECTOR2.NaN);
        }
        private void ResetValue(EditorVariable sender)
        {
            if (Variable.Type.IsValueType)
            {
                VariableValue = sender.Variable.Instance;
            }
        }
        private void ResetLayout(VECTOR2 size)
        {
            float y = 0;
            foreach (EditorVariable item in Editors)
            {
                item.ContentSizeChanged -= ResetLayout;
                item.ContentSizeChanged += ResetLayout;

                item.Y = y;
                y += item.ContentSize.Y;
                if (size.IsNaN() && VariableValue != null)
                {
                    item.Variable = new VariableObject(VariableValue, item.Variable.VariableName);
                }
            }
        }
    }
    /// <summary>若Variable.Member == null则是ValueEditor的创建</summary>
    public class EditorAllObject : EditorComboBox
    {
        private EditorVariable valueEditor;
        private bool singleType;
        private CheckBox collapse;
        public event Action<EditorVariable> OnCreateValueEditor;

        public EditorVariable ValueEditor
        {
            get { return valueEditor; }
            set
            {
                if (valueEditor != null)
                    Remove(valueEditor);
                valueEditor = value;
                if (valueEditor != null)
                {
                    //valueEditor.ValueChanged -= ThrowValue;
                    //valueEditor.ValueChanged += ThrowValue;
                    Add(valueEditor);
                }
            }
        }
        public bool SingleType
        {
            get { return singleType; }
        }

        /// <summary>创建一个多态类型的编辑器</summary>
        /// <param name="types">总共可创建的类型</param>
        /// <param name="isNullable">非值类型的情况下，是否允许为null</param>
        public EditorAllObject(IList<Type> types, bool isNullable)
            : base((Array)null)
        {
            collapse = new CheckBox();
            var texture = PATCH.GetNinePatch(COLOR.TransparentBlack, COLOR.DeepSkyBlue, 1);

            texture.Left = 2;
            texture.Top = 2;
            texture.Right = texture.Width;
            texture.Bottom = texture.Height;
            collapse.SourceNormal = texture;

            texture = PATCH.GetNinePatch(COLOR.TransparentBlack, COLOR.Yellow, 1);
            texture.Left = 0;
            texture.Top = 0;
            texture.Right = texture.Width - 2;
            texture.Bottom = texture.Height - 2;
            collapse.SourceClicked = texture;

            collapse.X = -WIDTH;
            collapse.Width = WIDTH;
            collapse.Height = HEIGHT;
            collapse.CheckedOverlayNormal = ECheckedOverlay.不覆盖;
            collapse.CheckedChanged += CollapseSwitch;
            AddChildFirst(collapse);

            object[] array = types.Select(t => Activator.CreateInstance(t)).ToArray();
            if (array.Length == 1)
            {
                object target = array[0];
                Type type = target.GetType();
                singleType = type.IsValueType;
            }

            if (!singleType && isNullable)
            {
                array = array.Insert(0, (object)null);
            }

            SetArray(array);

            this.OnSetVariable += new Action<IVariable>(EditorAllObject_OnSetVariable);
        }

        private void EditorAllObject_OnSetVariable(IVariable obj)
        {
            SetVariableValue();
        }
        private void CollapseSwitch(Button sender, Entry e)
        {
            if (valueEditor != null)
                valueEditor.Visible = collapse.Checked;
        }
        private void SetVariableValue()
        {
            for (int i = 0; i < array.Length; i++)
            {
                object obj = array.GetValue(i);
                if (obj != null && obj.GetType() == Variable.Type && VariableValue != null)
                {
                    array.SetValue(VariableValue, i);
                    break;
                }
            }
        }
        protected override void SelectedIndexChanged(Selectable sender)
        {
            if (Variable.Type.IsValueType)
            {
                SetVariableValue();
            }
            base.SelectedIndexChanged(sender);
        }
        protected override string ShowValue(object value)
        {
            //if (singleType)
            //    return value.ToString();
            if (value == null)
                return "null";
            Type type = value.GetType();
            return type.Name;
        }
        protected override int Select(Array array, object value)
        {
            if (singleType || value == null)
                return 0;
            for (int i = 1; i < array.Length; i++)
            {
                object item = array.GetValue(i);
                if (item.GetType() == value.GetType())
                {
                    return i;
                }
            }
            return 0;
        }
        protected override void SetValue()
        {
            base.SetValue();
            bool changed;
            bool null1 = valueEditor == null;
            bool null2 = SelectedItem == null;
            if (null1 == null2 && null1)
            {
                changed = false;
            }
            else if (null1 != null2)
            {
                changed = true;
            }
            else
            {
                changed = Variable.Type != SelectedItem.GetType();
            }
            if (valueEditor != null && changed)
            {
                Remove(valueEditor);
            }
            if (changed)
            {
                if (VariableValue == null)
                {
                    valueEditor = null;
                }
                else
                {
                    valueEditor = GENERATOR.GenerateEditor(new VariableValue(VariableValue));
                    if (valueEditor != null)
                    {
                        if (valueEditor is EditorAllObject)
                        {
                            valueEditor = ((EditorAllObject)valueEditor).valueEditor;
                        }
                        Add(valueEditor);
                        if (OnCreateValueEditor != null)
                            OnCreateValueEditor(valueEditor);
                    }
                }
            }

            if (valueEditor != null)
            {
                valueEditor.Y = HEIGHT;
                valueEditor.Variable = Variable;

                if (valueEditor != null)
                    valueEditor.Visible = collapse.Checked;

                valueEditor.ValueChanged -= ThrowValue;
                valueEditor.ValueChanged += ThrowValue;
            }
            if (singleType)
                comboBox.Text = VariableValue.ToString();
        }
        private void ThrowValue(EditorVariable sender)
        {
            if (Variable.Instance.GetType().IsValueType)
            {
                EditorObject parent = this.Parent as EditorObject;
                if (parent != null)
                {
                    parent.VariableValue = Variable.Instance;
                }
            }
        }
    }
    public class EditorTextBox : EditorVariable
    {
        /// <summary>False: 空 / 数字 / #或_开头</summary>
        //public bool Translate
        //{
        //    get
        //    {
        //        string value = VariableStringValue;

        //        if (string.IsNullOrEmpty(value))
        //            return false;

        //        bool result = true;
        //        double temp;
        //        result = !double.TryParse(value, out temp);

        //        if (value.StartsWith("#") || value.StartsWith("_"))
        //            result = false;

        //        return result;
        //    }
        //}
        public TextBox TextBox
        {
            get;
            private set;
        }

        public EditorTextBox()
        {
            TextBox = InitTextBox();
            Add(TextBox);
            Initialized();
        }

        protected virtual TextBox InitTextBox()
        {
            var box = new TextBox();
            box.Width = WIDTH;
            box.Height = HEIGHT;
            box.SourceNormal = PATCH.GetNinePatch(COLOR.White, COLOR.Gray, 1);
            box.UIText.Padding = new VECTOR2(10, 4);
            box.UIText.FontColor = COLOR.Black;
            return box;
        }
        protected virtual void Initialized()
        {
            TextBox.Blur += TextChangedModifyValue;
        }
        protected virtual void TextChangedModifyValue(UIElement sender, Entry e)
        {
            VariableValue = TextBox.Text;
        }
        protected override void SetValue()
        {
            TextBox.Text = VariableStringValue;
        }
    }
    public class EditorNumber : EditorTextBox
    {
        private decimal minValue;
        private decimal maxValue;
        /// <summary>y+1px时对应的Value变化</summary>
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
            OnSetVariable += new Action<IVariable>(EditorNumber_OnSetVariable);
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
            Initialized();
        }

        private void EditorNumber_OnSetVariable(IVariable obj)
        {
            Type type = obj.Type;
            try
            {
                if (type == typeof(float) || type == typeof(double))
                {
                    MinValue = decimal.MinValue;
                    MaxValue = decimal.MaxValue;
                    DragStep = 0.1M;
                }
                else
                {
                    FieldInfo const1 = type.GetField("MaxValue");
                    FieldInfo const2 = type.GetField("MinValue");

                    MaxValue = Convert.ToDecimal(const1.GetValue(null));
                    MinValue = Convert.ToDecimal(const2.GetValue(null));
                }
            }
            catch (Exception ex)
            {
                _LOG.Error("typeof({0}) get MaxValue and MinValue error! msg={1}", type.Name, ex.Message);
            }
        }
        protected override void Initialized()
        {
            base.Initialized();
            this.TextBox.Drag += DragModifyValue;
        }
        protected void DragModifyValue(UIElement sender, Entry e)
        {
            VECTOR2 moved = e.INPUT.Pointer.DeltaPosition;
            if (moved != VECTOR2.Zero && !moved.IsNaN())
            {
                TextBox.SetFocus(false);
                Value += (decimal)-moved.Y * DragStep;
            }
        }
        protected override void TextChangedModifyValue(UIElement sender, Entry e)
        {
            decimal value;
            if (decimal.TryParse(TextBox.Text, out value))
            {
                Value = value;
            }
            else
            {
                SetValue();
            }
        }
    }
    public class EditorComboBox : EditorVariable
    {
        protected DropDown comboBox = new DropDown();
        protected Array array;

        //public int SelectedIndex
        //{
        //    get
        //    {
        //        object value = VariableValue;
        //        for (int i = 0; i < array.Length; i++)
        //        {
        //            if (object.Equals(array.GetValue(i), value))
        //            {
        //                return i;
        //            }
        //        }
        //        return -1;
        //    }
        //}
        public int SelectedIndex
        {
            get { return comboBox.DropDownList.SelectedIndex; }
        }
        public object SelectedItem
        {
            get
            {
                int index = comboBox.DropDownList.SelectedIndex;
                if (index == -1)
                    return null;
                else
                    return array.GetValue(index);
            }
        }

        public EditorComboBox(Array array)
        {
            comboBox.SetDefaultControl(WIDTH, HEIGHT);
            comboBox.DropDownText.SourceNormal = PATCH.GetNinePatch(COLOR.Black, COLOR.Gray, 1);
            comboBox.DropDownText.UIText.Padding = new VECTOR2(10, 0);
            comboBox.DropDownText.UIText.TextAlignment = EPivot.MiddleLeft;
            comboBox.Color = COLOR.Black;
            comboBox.DropDownList.SelectedIndexChanged += SelectedIndexChanged;
            comboBox.DropDownList.Background = PATCH.GetNinePatch(COLOR.Black, COLOR.Gray, 1);
            Add(comboBox);

            if (array != null)
                SetArray(array);
        }
        public EditorComboBox(Type enumType)
            : this(Enum.GetValues(enumType))
        {
        }

        protected void SetArray(Array array)
        {
            if (array == null)
                throw new ArgumentNullException("array");
            this.array = array;
            comboBox.DropDownList.Clear();
            for (int i = 0; i < array.Length; i++)
            {
                var item = comboBox.DropDownList.AddItem(ShowValue(array.GetValue(i)));
            }
            comboBox.DropDownList.Height = comboBox.DropDownList.ChildClip.Height;
            //comboBox.DisplayItemCount = array.Length;
        }
        protected virtual string ShowValue(object value)
        {
            if (value == null)
                return "null";
            return value.ToString();
        }
        protected virtual void SelectedIndexChanged(Selectable sender)
        {
            VariableValue = array.GetValue(sender.SelectedIndex);
        }
        protected override void SetValue()
        {
            object value = VariableValue;
            int index = Select(array, value);
            if (comboBox.DropDownList.SelectedIndex != index)
            {
                if (index != -1)
                {
                    // 外部替换数组内的实例
                    if (value != array.GetValue(index))
                    {
                        array.SetValue(value, index);
                    }
                }
                comboBox.DropDownList.SelectedIndex = index;
            }
        }
        protected virtual int Select(Array array, object value)
        {
            return Array.IndexOf(array, value);
        }
    }
    public class EditorAsset : EditorVariable
    {
        private Label path = new Label();
        private Panel patch = new Panel();
        private TextureBox cborder = new TextureBox();
        private NumberBox alpha = new NumberBox();
        private NumberBox bold = new NumberBox();

        public override string VariableStringValue
        {
            get
            {
                return path.Text;
            }
        }

        public EditorAsset()
        {
            this.Hover += RightClickToChange;

            path.Width = WIDTH;
            path.Height = HEIGHT;
            path.Text = string.Empty;
            path.Clicked += new DUpdate<UIElement>(label_Clicked);
            Add(path);

            cborder.X = path.X;
            cborder.Width = 50;
            cborder.Height = HEIGHT;
            cborder.Texture = TEXTURE.Pixel;
            cborder.Clicked += new DUpdate<UIElement>(cborder_Clicked);

            alpha.X = path.X + 50;
            alpha.Width = 40;
            alpha.Height = HEIGHT;
            alpha.MinValue = 0;
            alpha.MaxValue = 255;
            alpha.DefaultText.TextAlignment = EPivot.MiddleCenter;
            alpha.DefaultText.Text = "A";
            alpha.UIText.TextAlignment = EPivot.MiddleCenter;
            alpha.ValueChanged += new DUpdate<NumberBox>(alpha_ValueChanged);

            bold.X = path.X + 100;
            bold.Width = 20;
            bold.Height = HEIGHT;
            bold.MinValue = 1;
            bold.MaxValue = 3;
            bold.UIText.TextAlignment = EPivot.MiddleCenter;
            bold.ValueChanged += new DUpdate<NumberBox>(bold_ValueChanged);

            patch.Add(cborder);
            patch.Add(alpha);
            patch.Add(bold);
            Add(patch);
        }

        void bold_ValueChanged(NumberBox sender, Entry e)
        {
            COLOR color;
            byte bold;
            ParsePatch(out color, out bold);
            SetPatch(ref color, (byte)this.bold.Value);
        }
        void alpha_ValueChanged(NumberBox sender, Entry e)
        {
            COLOR color;
            byte bold;
            ParsePatch(out color, out bold);
            color.A = (byte)alpha.Value;
            SetPatch(ref color, bold);
        }
        void cborder_Clicked(UIElement sender, Entry e)
        {
            COLOR color;
            byte bold;
            ParsePatch(out color, out bold);
            COLOR? result = UtilityEditor.SelectColor(color);
            if (result.HasValue)
            {
                byte a = color.A;
                color = result.Value;
                color.A = a;
                SetPatch(ref color, bold);
            }
        }
        void SetPatch(ref COLOR color, byte bold)
        {
            VariableValue = PATCH.GetNinePatch(PATCH.NullColor, color, bold);
        }
        void RightClickToChange(UIElement sender, Entry e)
        {
            if (!e.INPUT.Pointer.IsClick(1))
                return;

            if (Variable.Type == typeof(TEXTURE))
            {
                var value = VariableValue;
                if (value == null)
                {
                    VariableValue = TEXTURE.Pixel;
                }
                else if (value == TEXTURE.Pixel)
                {
                    if (UtilityEditor.Confirm("", "是否要图片换成默认的九宫格？"))
                        VariableValue = PATCH.GetNinePatch(PATCH.NullColor, COLOR.Black, 2);
                }
                else
                {
                    if (UtilityEditor.Confirm("", "是否要清空图片？"))
                        VariableValue = null;
                }
                return;
            }
            else if (Variable.Type == typeof(FONT))
            {
                if (UtilityEditor.Confirm("", "是否要使用默认字体？"))
                {
                    VariableValue = FONT.Default;
                }
            }
        }
        void label_Clicked(UIElement sender, Entry e)
        {
            string dir = System.IO.Path.GetFullPath(_IO.DirectoryWithEnding(CONTENT.RootDirectory));
            string file = dir;
            if (!string.IsNullOrEmpty(VariableStringValue))
                file += VariableStringValue;
            if (UtilityEditor.OpenFile(ref file, CONTENT.LoadableContentFileSuffix.ToArray()))
            {
                file = _IO.RelativePathForward(file, dir);
                if (file != null)
                {
                    VariableValue = CONTENT.Load(file);
                }
            }
        }
        void ParsePatch(out COLOR border, out byte bold)
        {
            PATCH patch = (PATCH)VariableValue;
            border = patch.ColorBorder;
            bold = (byte)patch.Anchor.X;
        }
        protected override void SetValue()
        {
            path.Visible = true;
            patch.Visible = false;
            var value = VariableValue;
            if (value == null)
                path.Text = "";
            else
            {
                path.Text = ((Content)value).Key;
                if (value is PATCH && ((PATCH)value).IsDefaultPatch)
                {
                    path.Visible = false;
                    patch.Visible = true;
                    COLOR c;
                    byte b;
                    ParsePatch(out c, out b);
                    cborder.Color = c;
                    alpha.Value = c.A;
                    bold.Value = b;
                }
            }
        }
    }
    public class EditorCheckBox : EditorVariable
    {
        private CheckBox checkBox = new CheckBox();

        public EditorCheckBox()
        {
            checkBox.Width = HEIGHT;
            checkBox.Height = HEIGHT;
            checkBox.SourceNormal = PATCH.GetNinePatch(COLOR.TransparentBlack, COLOR.Gold, 1);
            checkBox.SourceClicked = PATCH.GetNinePatch(COLOR.GreenYellow, COLOR.TransparentBlack, 1);
            checkBox.CheckedChanged += checkBox_CheckedChanged;
            Add(checkBox);
        }

        void checkBox_CheckedChanged(Button sender, Entry e)
        {
            VariableValue = sender.Checked;
        }
        protected override void SetValue()
        {
            checkBox.Checked = (bool)VariableValue;
        }
    }
    public class EditorCheckBoxMultiple : EditorVariable
    {
        private Array array;

        public EditorCheckBoxMultiple(Type enumType)
        {
            Type utype = Enum.GetUnderlyingType(enumType);
            array = Enum.GetValues(enumType);
            for (int i = 0; i < array.Length; i++)
            {
                int value = Convert.ToInt32(array.GetValue(i));

                CheckBox checkBox = new CheckBox();
                checkBox.Y = HEIGHT * i;
                checkBox.Width = HEIGHT;
                checkBox.Height = HEIGHT;
                checkBox.SourceNormal = PATCH.GetNinePatch(COLOR.TransparentBlack, COLOR.Black, 1);
                checkBox.SourceClicked = PATCH.GetNinePatch(COLOR.LawnGreen, COLOR.TransparentBlack, 1);
                checkBox.CheckedChanged += (sender, e) =>
                {
                    if (sender.Checked)
                    {
                        VariableValue = Convert.ChangeType(Convert.ToInt32(VariableValue) | value, utype);
                    }
                    else
                    {
                        VariableValue = Convert.ChangeType(Utility.EnumRemove(Convert.ToInt32(VariableValue), value), utype);
                    }
                };
                Add(checkBox);

                checkBox.Text = array.GetValue(i).ToString();
            }
        }
        protected override void SetValue()
        {
            int index = 0;
            int value = Convert.ToInt32(VariableValue);
            foreach (var item in Childs)
            {
                CheckBox target = item as CheckBox;
                if (target == null)
                    continue;
                bool result = Utility.EnumContains(value, Convert.ToInt32(array.GetValue(index)));
                if (target.Checked != result)
                    target.Checked = result;
                index++;
            }
        }
    }
    public class EditorColor : EditorVariable
    {
        private TextureBox box;
        private NumberBox alpha;

        public EditorColor()
        {
            box = new TextureBox();
            box.Width = WIDTH - HEIGHT;
            box.Height = HEIGHT;
            box.Clicked += box_Clicked;
            box.Texture = TEXTURE.Pixel;
            box.DisplayMode = EViewport.Strength;
            Add(box);

            alpha = new NumberBox();
            alpha.MinValue = 0;
            alpha.MaxValue = 255;
            alpha.X = box.X + box.Width;
            alpha.Width = HEIGHT;
            alpha.Height = HEIGHT;
            alpha.UIText.Font = FONT;
            alpha.DefaultText.Font = FONT;
            alpha.DefaultText.TextAlignment = EPivot.TopCenter;
            alpha.DefaultText.Text = "A";
            alpha.UIText.TextAlignment = EPivot.TopCenter;
            alpha.ValueChanged += alpha_ValueChanged;
            Add(alpha);
        }

        void alpha_ValueChanged(Label sender, Entry e)
        {
            COLOR color = (COLOR)VariableValue;
            color.A = (byte)alpha.Value;
            VariableValue = color;
        }
        void box_Clicked(UIElement sender, Entry e)
        {
            COLOR? color = UtilityEditor.SelectColor(box.Color);
            if (color.HasValue)
            {
                COLOR value = color.Value;
                value.A = (byte)alpha.Value;
                VariableValue = value;
            }
        }
        protected override void SetValue()
        {
            box.Color = (COLOR)VariableValue;
            alpha.Value = box.Color.A;
        }
    }
    public class EditorArray : EditorVariable
    {
        private Label label;
        private TabPage collapse;
        private Type type;
        private Type elementType;

        public EditorArray(Type type, Type elementType)
        {
            this.type = type;
            this.elementType = elementType;

            collapse = new TabPage();
            collapse.IsRadioButton = false;

            var texture = PATCH.GetNinePatch(COLOR.TransparentBlack, COLOR.DeepSkyBlue, 1);

            texture.Left = 2;
            texture.Top = 2;
            texture.Right = texture.Width;
            texture.Bottom = texture.Height;
            collapse.SourceNormal = texture;

            texture = PATCH.GetNinePatch(COLOR.TransparentBlack, COLOR.Yellow, 1);
            texture.Left = 0;
            texture.Top = 0;
            texture.Right = texture.Width - 2;
            texture.Bottom = texture.Height - 2;
            collapse.SourceClicked = texture;

            collapse.X = -WIDTH;
            collapse.Width = WIDTH;
            collapse.Height = HEIGHT;
            collapse.OnChecked += new DUpdate<Button>(collapse_OnChecked);
            Add(collapse);

            Panel page = new Panel();
            page.X = 0;
            page.Y = HEIGHT;
            page.Width = WIDTH * 2;
            page.Height = HEIGHT * 5;
            page.Background = TEXTURE.Pixel;
            page.Color = COLOR.Black;
            collapse.Page = page;

            this.Height = 0;

            label = new Label();
            label.Width = WIDTH;
            label.Height = HEIGHT;
            label.UIText.FontColor = COLOR.White;
            label.Clicked += new DUpdate<UIElement>(EditorArray_Clicked);
            Add(label);
        }

        void ResetPageSize()
        {
            if (collapse.Checked)
            {
                IList list = (IList)VariableValue;
                int size = 0;
                if (list != null)
                    size = list.Count > 5 ? 5 : list.Count;
                collapse.Page.Height = size * HEIGHT;
                ResetLayout(VECTOR2.NaN);
            }
        }
        void collapse_OnChecked(Button sender, Entry e)
        {
            ResetPageSize();
        }
        void EditorArray_Clicked(UIElement sender, Entry e)
        {
            IList list = (IList)VariableValue;
            if (list == null)
                AddValue(0);
            else
                AddValue(((IList)VariableValue).Count);
        }
        void AddValue(int i)
        {
            IList list = (IList)VariableValue;
            if (list == null)
            {
                if (type.IsArray)
                    list = Array.CreateInstance(elementType, 0);
                else
                    list = (IList)Activator.CreateInstance(type);
                Variable.SetValue(list);
            }
            if (type.IsArray)
            {
                Array array = Array.CreateInstance(elementType, list.Count + 1);
                Array.Copy((Array)list, array, list.Count);
                list = array;
                InternalSetValue(list);
            }
            else
                list.Add(elementType.DefaultValue());
            //IVariable obj = new VariableExpression(elementType, () => list[i], (v) => list[i] = v);
            IVariable obj = new VariableExpression(null, elementType, (list.Count - 1).ToString(),
                (a1, a2) => list[i], (a1, a2, v) => list[i] = v);
            var editor = GENERATOR.GenerateEditor(obj);
            //if (!(editor is EditorAllObject))
            //{
            //    Label label = new Label();
            //    //label.X = WIDTH;
            //    label.Width = 0;
            //    label.Height = HEIGHT;
            //    label.Pivot = EPivot.TopRight;
            //    label.UIText.TextAlignment = EPivot.MiddleRight;
            //    label.UIText.FontColor = COLOR.Black;
            //    label.Text = string.Format("[{0}]", i);
            //    editor.Add(label);
            //}
            collapse.Page.Add(editor);
            editor.Hover += new DUpdate<UIElement>(editor_Hover);

            ResetPage();
        }
        void editor_Hover(UIElement sender, Entry e)
        {
            if (e.INPUT.Pointer.IsClick(2))
                RemoveAt(collapse.Page.IndexOf(sender));
        }
        void RemoveAt(int i)
        {
            IList list = (IList)VariableValue;
            if (type.IsArray)
            {
                Array array = Array.CreateInstance(elementType, list.Count - 1);
                Array.Copy((Array)list, 0, array, 0, i);
                Array.Copy((Array)list, i + 1, array, i, list.Count - i - 1);
                InternalSetValue(array);
            }
            else
                list.RemoveAt(i);

            collapse.Page.Remove(i);

            ResetPage();
        }
        void ResetPage()
        {
            IList list = (IList)VariableValue;
            this.label.Text = string.Format("{0}[{1}]", elementType.Name, list == null ? "null" : list.Count.ToString());
            ResetPageSize();
        }
        protected override void SetValue()
        {
            collapse.Page.Clear();
            IList list = (IList)VariableValue;
            if (list != null)
                for (int i = 0; i < list.Count; i++)
                    AddValue(i);
            ResetLayout(VECTOR2.NaN);
            ResetPage();
        }
        private void ResetLayout(VECTOR2 size)
        {
            float y = 0;
            foreach (EditorVariable item in collapse.Page)
            {
                item.ContentSizeChanged -= ResetLayout;
                item.ContentSizeChanged += ResetLayout;

                //item.X = WIDTH;
                item.Y = y;
                y += item.ContentSize.Y;
            }
        }
    }


    public class FromFile : Button
    {
        public event Action<string> OnDragFile;
        public string DefaultFile;
        public string[] Suffix = new string[0];

        public FromFile()
        {
            SourceNormal = PATCH.GetNinePatch(COLOR.White, COLOR.Gray, 1);
            SourceHover = PATCH.GetNinePatch(COLOR.Silver, COLOR.Gray, 1);
            Color = COLOR.Black;
            Click += new DUpdate<UIElement>(FromFile_Click);
        }

        void FromFile_Click(UIElement sender, Entry e)
        {
            string file = DefaultFile;
            if (UtilityEditor.OpenFile(ref file, Suffix))
            {
                DragFile(file);
            }
        }
        public virtual void DragFile(string file)
        {
            if (OnDragFile != null)
            {
                OnDragFile(file);
            }
        }
    }


    public class Generator
    {
        private static Dictionary<Type, List<Type>> cache = new Dictionary<Type, List<Type>>();

        public SerializeSetting Setting = SerializeSetting.DefaultSetting;
        public Func<IVariable, EditorVariable> Generate;
        public Func<IVariable, EditorVariable> PropertyGenerate;
        public event Action<IVariable, EditorVariable> OnPropertyGenerated;
        public event Action<IVariable, EditorVariable> OnGenerated;

        public virtual EditorVariable GenerateEnum(IVariable variable)
        {
            Type type = variable.Type;
            if (type.HasAttribute<FlagsAttribute>())
            {
                return new EditorCheckBoxMultiple(type);
            }
            else
            {
                return new EditorComboBox(type);
            }
        }
        public virtual EditorVariable GenerateArray(IVariable variable)
        {
            var type = variable.Type;
            if (type.IsArray)
                return new EditorArray(type, type.GetElementType());
            else
            {
                var interfaces = type.GetAllInterfaces();
                foreach (var i in interfaces)
                    if (i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IList<>))
                        return new EditorArray(type, i.GetGenericArguments()[0]);
                throw new NotImplementedException(string.Format("暂未实现{0}类型的编辑器", type.FullName));
            }
        }
        public virtual EditorVariable GenerateBool(IVariable variable)
        {
            return new EditorCheckBox();
        }
        public virtual EditorVariable GenerateNumber(IVariable variable)
        {
            return new EditorNumber();
        }
        public virtual EditorVariable GenerateString(IVariable variable)
        {
            return new EditorTextBox();
        }
        public virtual EditorVariable GenerateColor(IVariable variable)
        {
            return new EditorColor();
        }
        public virtual EditorVariable GenerateNullable(IVariable variable)
        {
            Type type = Nullable.GetUnderlyingType(variable.Type);
            EditorVariable valueEditor = GenerateEditor(new VariableValue(Activator.CreateInstance(type)));
            return new EditorNullable(valueEditor);
        }
        public EditorVariable GenerateEditor(IVariable variable)
        {
            var generator = InternalGenerate(variable);
            if (generator != null)
            {
                if (OnGenerated != null)
                    OnGenerated(variable, generator);
                generator.Variable = variable;
            }
            return generator;
        }
        protected virtual EditorVariable InternalGenerate(IVariable variable)
        {
            if (Generate != null)
            {
                var generate = Generate(variable);
                if (generate != null)
                    return generate;
            }

            Type type = variable.Type;
            if (type.IsEnum)
            {
                return GenerateEnum(variable);
            }
            //else if (type.IsArray || type.Is(typeof(IList)))
            //{
            //    return GenerateArray(variable);
            //}
            else if (type == typeof(bool))
            {
                return GenerateBool(variable);
            }
            else if (type.IsNumber())
            {
                return GenerateNumber(variable);
            }
            else if (type == typeof(string))
            {
                return GenerateString(variable);
            }
            else if (type == typeof(COLOR))
            {
                return GenerateColor(variable);
            }
            //else if (type == typeof(TEXTURE))
            //{
            //}
            else if (type == typeof(TEXTURE)
                || type == typeof(FONT)
                || type == typeof(SHADER))
            {
                return new EditorAsset();
            }
            else if (type.IsNullable())
            {
                return GenerateNullable(variable);
            }
            else
            {
                IList<Type> types;
                if (type.IsValueType)
                {
                    types = new Type[] { type };
                }
                else
                {
                    List<Type> temp;
                    if (!cache.TryGetValue(type, out temp))
                    {
                        temp = UtilityEditor.LoadedAssemblies().GetTypes(type);
                        cache[type] = temp;
                    }
                    types = temp;
                }

                if (types.Count == 0)
                    return null;

                var value = variable.GetValue();
                EditorAllObject all = new EditorAllObject(types, value == null);
                if (!all.SingleType && value == null)
                    return all;

                EditorVariable editor = new EditorObject();
                Setting.Serialize(type, value,
                        v =>
                        {
                            EditorVariable childEditor = null;
                            if (PropertyGenerate != null)
                                childEditor = PropertyGenerate(v);
                            if (childEditor == null)
                                childEditor = GenerateEditor(v);
                            if (OnPropertyGenerated != null)
                                OnPropertyGenerated(v, childEditor);
                            childEditor.Variable = v;
                            editor.Add(childEditor);
                        });
                all.ValueEditor = editor;
                return all;
            }
        }
    }
}
