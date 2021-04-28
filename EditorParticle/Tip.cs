using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EntryEngine.UI;
using EntryEngine;

class Tip : STip
{
    public void SetTip(UIElement element, TEXT.ETEXTKey tip)
    {
        SetTip(element, _TABLE._TEXTByKey[tip].Value);
    }
}
