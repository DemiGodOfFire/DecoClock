
using System;
using System.Collections.Generic;
using Vintagestory.API.Client;

namespace DecoClock
{
    internal abstract class GuiDialogClockBase : GuiDialogGeneric
    {
        public GuiDialogClockBase(string DialogTitle, ICoreClientAPI capi) : base(DialogTitle, capi)
        {
        }

        public virtual void OnTitleBarClose()
        {
            this.TryClose();
        }
    }
}
