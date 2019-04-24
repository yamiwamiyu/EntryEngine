#if CLIENT

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EntryEngine
{
    public static partial class __INPUT
    {
        private static EntryEngine.INPUT __instance { get { return Entry._INPUT; } }
        public static EntryEngine.MOUSE Mouse
        {
            get { return __instance.Mouse; }
            set { __instance.Mouse = value; }
        }
        public static float MouseScrollWheelValue
        {
            get { return __instance.Mouse.ScrollWheelValue; }
        }
        public static EntryEngine.VECTOR2 MousePosition
        {
            get { return __instance.Mouse.Position; }
            set { __instance.Mouse.Position = value; }
        }
        public static EntryEngine.VECTOR2 MouseDeltaPosition
        {
            get { return __instance.Mouse.DeltaPosition; }
        }
        public static EntryEngine.VECTOR2 MousePositionPrevious
        {
            get { return __instance.Mouse.PositionPrevious; }
        }
        public static EntryEngine.VECTOR2 MouseClickPosition
        {
            get { return __instance.Mouse.ClickPosition; }
        }
        public static EntryEngine.VECTOR2 MouseClickPositionRelative
        {
            get { return __instance.Mouse.ClickPositionRelative; }
        }
        public static EntryEngine.IMouseState MouseCurrent
        {
            get { return __instance.Mouse.Current; }
        }
        public static EntryEngine.IMouseState MousePrevious
        {
            get { return __instance.Mouse.Previous; }
        }
        public static EntryEngine.ComboClick MouseComboClick
        {
            get { return __instance.Mouse.ComboClick; }
        }
        public static int MouseDefaultKey
        {
            get { return __instance.Mouse.DefaultKey; }
        }
        public static bool MouseIsTap()
        {
            return __instance.Mouse.IsTap();
        }
        public static bool MouseIsTap(int key)
        {
            return __instance.Mouse.IsTap(key);
        }
        public static bool MouseEnterArea(EntryEngine.RECT area)
        {
            return __instance.Mouse.EnterArea(area);
        }
        public static bool MouseEnterArea(EntryEngine.CIRCLE area)
        {
            return __instance.Mouse.EnterArea(area);
        }
        public static bool MouseLeaveArea(EntryEngine.RECT area)
        {
            return __instance.Mouse.LeaveArea(area);
        }
        public static bool MouseLeaveArea(EntryEngine.CIRCLE area)
        {
            return __instance.Mouse.LeaveArea(area);
        }
        public static EntryEngine.ComboClick MouseGetComboClick(int key)
        {
            return __instance.Mouse.GetComboClick(key);
        }
        public static bool MouseIsClick(int key)
        {
            return __instance.Mouse.IsClick(key);
        }
        public static bool MouseIsRelease(int key)
        {
            return __instance.Mouse.IsRelease(key);
        }
        public static bool MouseIsPressed(int key)
        {
            return __instance.Mouse.IsPressed(key);
        }
        public static void MouseUpdate(EntryEngine.Entry entry)
        {
            __instance.Mouse.Update(entry);
        }
        public static EntryEngine.TOUCH Touch
        {
            get { return __instance.Touch; }
            set { __instance.Touch = value; }
        }
        public static int TouchCount
        {
            get { return __instance.Touch.Count; }
        }
        public static EntryEngine.Pointer<EntryEngine.ITouchState> TouchFirst
        {
            get { return __instance.Touch.First; }
        }
        public static EntryEngine.Pointer<EntryEngine.ITouchState> TouchLast
        {
            get { return __instance.Touch.Last; }
        }
        public static int TouchDefaultKey
        {
            get { return __instance.Touch.DefaultKey; }
        }
        public static bool TouchTouchExpand
        {
            get { return __instance.Touch.TouchExpand; }
        }
        public static bool TouchTouchReduce
        {
            get { return __instance.Touch.TouchReduce; }
        }
        public static float TouchScale
        {
            get { return __instance.Touch.Scale; }
        }
        public static float TouchRotate
        {
            get { return __instance.Touch.Rotate; }
        }
        public static EntryEngine.VECTOR2 TouchPosition
        {
            get { return __instance.Touch.Position; }
            set { __instance.Touch.Position = value; }
        }
        public static EntryEngine.VECTOR2 TouchDeltaPosition
        {
            get { return __instance.Touch.DeltaPosition; }
        }
        public static EntryEngine.VECTOR2 TouchPositionPrevious
        {
            get { return __instance.Touch.PositionPrevious; }
        }
        public static EntryEngine.VECTOR2 TouchClickPosition
        {
            get { return __instance.Touch.ClickPosition; }
        }
        public static EntryEngine.VECTOR2 TouchClickPositionRelative
        {
            get { return __instance.Touch.ClickPositionRelative; }
        }
        public static EntryEngine.ITouchState TouchCurrent
        {
            get { return __instance.Touch.Current; }
        }
        public static EntryEngine.ITouchState TouchPrevious
        {
            get { return __instance.Touch.Previous; }
        }
        public static EntryEngine.ComboClick TouchComboClick
        {
            get { return __instance.Touch.ComboClick; }
        }
        public static EntryEngine.Pointer<EntryEngine.ITouchState> TouchGetTouch(int index)
        {
            return __instance.Touch.GetTouch(index);
        }
        public static bool TouchIsClick(int key)
        {
            return __instance.Touch.IsClick(key);
        }
        public static bool TouchIsPressed(int key)
        {
            return __instance.Touch.IsPressed(key);
        }
        public static bool TouchIsRelease(int key)
        {
            return __instance.Touch.IsRelease(key);
        }
        public static bool TouchIsTap()
        {
            return __instance.Touch.IsTap();
        }
        public static bool TouchIsTap(int key)
        {
            return __instance.Touch.IsTap(key);
        }
        public static bool TouchEnterArea(EntryEngine.RECT area)
        {
            return __instance.Touch.EnterArea(area);
        }
        public static bool TouchEnterArea(EntryEngine.CIRCLE area)
        {
            return __instance.Touch.EnterArea(area);
        }
        public static bool TouchLeaveArea(EntryEngine.RECT area)
        {
            return __instance.Touch.LeaveArea(area);
        }
        public static bool TouchLeaveArea(EntryEngine.CIRCLE area)
        {
            return __instance.Touch.LeaveArea(area);
        }
        public static EntryEngine.ComboClick TouchGetComboClick(int key)
        {
            return __instance.Touch.GetComboClick(key);
        }
        public static void TouchUpdate(EntryEngine.Entry entry)
        {
            __instance.Touch.Update(entry);
        }
        public static EntryEngine.KEYBOARD Keyboard
        {
            get { return __instance.Keyboard; }
            set { __instance.Keyboard = value; }
        }
        public static bool KeyboardFocused
        {
            get { return __instance.Keyboard.Focused; }
        }
        public static bool KeyboardCtrl
        {
            get { return __instance.Keyboard.Ctrl; }
        }
        public static bool KeyboardAlt
        {
            get { return __instance.Keyboard.Alt; }
        }
        public static bool KeyboardShift
        {
            get { return __instance.Keyboard.Shift; }
        }
        public static EntryEngine.IKeyboardState KeyboardCurrent
        {
            get { return __instance.Keyboard.Current; }
        }
        public static EntryEngine.IKeyboardState KeyboardPrevious
        {
            get { return __instance.Keyboard.Previous; }
        }
        public static EntryEngine.ComboClick KeyboardComboClick
        {
            get { return __instance.Keyboard.ComboClick; }
        }
        public static int KeyboardDefaultKey
        {
            get { return __instance.Keyboard.DefaultKey; }
        }
        public static bool KeyboardIsClick(EntryEngine.PCKeys key)
        {
            return __instance.Keyboard.IsClick(key);
        }
        public static bool KeyboardIsRelease(EntryEngine.PCKeys key)
        {
            return __instance.Keyboard.IsRelease(key);
        }
        public static bool KeyboardIsPressed(EntryEngine.PCKeys key)
        {
            return __instance.Keyboard.IsPressed(key);
        }
        public static bool KeyboardIsInputKeyPressed(int key)
        {
            return __instance.Keyboard.IsInputKeyPressed(key);
        }
        public static bool KeyboardIsInputKeyPressed(EntryEngine.PCKeys key)
        {
            return __instance.Keyboard.IsInputKeyPressed(key);
        }
        public static EntryEngine.ComboClick KeyboardGetComboClick(int key)
        {
            return __instance.Keyboard.GetComboClick(key);
        }
        public static bool KeyboardIsClick(int key)
        {
            return __instance.Keyboard.IsClick(key);
        }
        public static bool KeyboardIsRelease(int key)
        {
            return __instance.Keyboard.IsRelease(key);
        }
        public static bool KeyboardIsPressed(int key)
        {
            return __instance.Keyboard.IsPressed(key);
        }
        public static void KeyboardUpdate(EntryEngine.Entry entry)
        {
            __instance.Keyboard.Update(entry);
        }
        public static EntryEngine.InputText InputDevice
        {
            get { return __instance.InputDevice; }
            set { __instance.InputDevice = value; }
        }
        public static string InputDeviceText
        {
            get { return __instance.InputDevice.Text; }
            set { __instance.InputDevice.Text = value; }
        }
        public static int InputDeviceIndex
        {
            get { return __instance.InputDevice.Index; }
        }
        public static int InputDeviceLastIndex
        {
            get { return __instance.InputDevice.LastIndex; }
        }
        public static EntryEngine.ITypist InputDeviceTypist
        {
            get { return __instance.InputDevice.Typist; }
        }
        public static bool InputDeviceIsActive
        {
            get { return __instance.InputDevice.IsActive; }
        }
        public static bool InputDeviceSelectOrder
        {
            get { return __instance.InputDevice.SelectOrder; }
        }
        public static bool InputDeviceHasSelected
        {
            get { return __instance.InputDevice.HasSelected; }
        }
        public static int InputDeviceSelectedFrom
        {
            get { return __instance.InputDevice.SelectedFrom; }
        }
        public static int InputDeviceSelectedTo
        {
            get { return __instance.InputDevice.SelectedTo; }
        }
        public static string InputDeviceSelectedText
        {
            get { return __instance.InputDevice.SelectedText; }
        }
        public static bool InputDeviceCursorShow
        {
            get { return __instance.InputDevice.CursorShow; }
        }
        public static EntryEngine.VECTOR2 InputDeviceCursorLocation
        {
            get { return __instance.InputDevice.CursorLocation; }
        }
        public static System.Collections.Generic.IEnumerable<EntryEngine.RECT> InputDeviceSelectedAreas
        {
            get { return __instance.InputDevice.SelectedAreas; }
        }
        public static string InputDeviceCopied
        {
            get { return __instance.InputDevice.Copied; }
        }
        public static bool InputDeviceImmCapturing
        {
            get { return __instance.InputDevice.ImmCapturing; }
        }
        public static void InputDeviceActive(EntryEngine.ITypist user)
        {
            __instance.InputDevice.Active(user);
        }
        public static void InputDeviceStop()
        {
            __instance.InputDevice.Stop();
        }
        public static void InputDeviceInput(string input)
        {
            __instance.InputDevice.Input(input);
        }
        public static void InputDeviceTab()
        {
            __instance.InputDevice.Tab();
        }
        public static void InputDeviceNewLine()
        {
            __instance.InputDevice.NewLine();
        }
        public static void InputDevicePaste()
        {
            __instance.InputDevice.Paste();
        }
        public static void InputDeviceBackSpace()
        {
            __instance.InputDevice.BackSpace();
        }
        public static void InputDeviceDelete()
        {
            __instance.InputDevice.Delete();
        }
        public static void InputDeviceSelect(int from, int to)
        {
            __instance.InputDevice.Select(from, to);
        }
        public static void InputDeviceSelectAll()
        {
            __instance.InputDevice.SelectAll();
        }
        public static void InputDeviceSelectCancel()
        {
            __instance.InputDevice.SelectCancel();
        }
        public static void InputDeviceCopy()
        {
            __instance.InputDevice.Copy();
        }
        public static void InputDeviceCopy(string copy)
        {
            __instance.InputDevice.Copy(copy);
        }
        public static void InputDeviceCut()
        {
            __instance.InputDevice.Cut();
        }
        public static void InputDeviceLeft()
        {
            __instance.InputDevice.Left();
        }
        public static void InputDeviceHome()
        {
            __instance.InputDevice.Home();
        }
        public static void InputDeviceRight()
        {
            __instance.InputDevice.Right();
        }
        public static void InputDeviceEnd()
        {
            __instance.InputDevice.End();
        }
        public static void InputDeviceUp()
        {
            __instance.InputDevice.Up();
        }
        public static void InputDeviceDown()
        {
            __instance.InputDevice.Down();
        }
        public static void InputDeviceUndo()
        {
            __instance.InputDevice.Undo();
        }
        public static void InputDeviceRedo()
        {
            __instance.InputDevice.Redo();
        }
        public static void InputDeviceEnter()
        {
            __instance.InputDevice.Enter();
        }
        public static void InputDeviceEscape()
        {
            __instance.InputDevice.Escape();
        }
        public static EntryEngine.IPointer Pointer
        {
            get { return __instance.Pointer; }
        }
        public static EntryEngine.VECTOR2 PointerPosition
        {
            get { return __instance.Pointer.Position; }
            set { __instance.Pointer.Position = value; }
        }
        public static EntryEngine.VECTOR2 PointerPositionPrevious
        {
            get { return __instance.Pointer.PositionPrevious; }
        }
        public static EntryEngine.VECTOR2 PointerDeltaPosition
        {
            get { return __instance.Pointer.DeltaPosition; }
        }
        public static EntryEngine.VECTOR2 PointerClickPosition
        {
            get { return __instance.Pointer.ClickPosition; }
        }
        public static EntryEngine.ComboClick PointerComboClick
        {
            get { return __instance.Pointer.ComboClick; }
        }
        public static int PointerDefaultKey
        {
            get { return __instance.Pointer.DefaultKey; }
        }
        public static void PointerUpdate(EntryEngine.Entry entry)
        {
            __instance.Pointer.Update(entry);
        }
        public static EntryEngine.ComboClick PointerGetComboClick(int key)
        {
            return __instance.Pointer.GetComboClick(key);
        }
        public static bool PointerIsClick(int key)
        {
            return __instance.Pointer.IsClick(key);
        }
        public static bool PointerIsTap()
        {
            return __instance.Pointer.IsTap();
        }
        public static bool PointerIsTap(int key)
        {
            return __instance.Pointer.IsTap(key);
        }
        public static bool PointerIsRelease(int key)
        {
            return __instance.Pointer.IsRelease(key);
        }
        public static bool PointerIsPressed(int key)
        {
            return __instance.Pointer.IsPressed(key);
        }
        public static bool PointerEnterArea(EntryEngine.RECT area)
        {
            return __instance.Pointer.EnterArea(area);
        }
        public static bool PointerEnterArea(EntryEngine.CIRCLE area)
        {
            return __instance.Pointer.EnterArea(area);
        }
        public static bool PointerLeaveArea(EntryEngine.RECT area)
        {
            return __instance.Pointer.LeaveArea(area);
        }
        public static bool PointerLeaveArea(EntryEngine.CIRCLE area)
        {
            return __instance.Pointer.LeaveArea(area);
        }
        public static void Update(EntryEngine.Entry entry)
        {
            __instance.Update(entry);
        }
    }
}

#endif
