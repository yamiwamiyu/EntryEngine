#if CLIENT

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EntryEngine
{
	public static partial class __INPUT
	{
        //public static VECTOR2 RelativeClickPosition(this IPointer pointer)
        //{
        //    // || pointer.ClickPosition.IsNaN()
        //    if (pointer == null)
        //        return VECTOR2.Zero;
        //    return pointer.Position - pointer.ClickPosition;
        //}
        //public static bool Tap(this IPointer pointer)
        //{
        //    if (pointer == null || pointer.ComboClick == null)
        //        return false;
        //    return pointer.IsRelease(pointer.DefaultKey) &&
        //        pointer.RelativeClickPosition().LengthSquared() < INPUT.TapDistance * INPUT.TapDistance &&
        //        pointer.ComboClick.PressedTime < INPUT.TapTime;
        //}
        //public static float ScrollWheelValue(this Pointer<IMouseState> pointer)
        //{
        //    return pointer.Current.ScrollWheelValue - pointer.Previous.ScrollWheelValue;
        //}
	}
}

#endif