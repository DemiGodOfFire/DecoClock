using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.MathTools;

namespace DecoClock
{
    internal class GuiDialogClock: GuiDialogClockBase
    {
        public GuiDialogClock(InventoryClock inventory, BlockPos blockEntityPos, ICoreClientAPI capi) : base(inventory, blockEntityPos, capi)
        {

        }

        public override void ComposeDialog()
        {

            ElementBounds clockBounds = ElementBounds.Fixed(0.0, 0.0, 200.0, 290.0);
            ElementBounds tickMarksSlotBounds = ElementStdBounds.SlotGrid(EnumDialogArea.None, 153.0, 30.0, 1, 1);
            ElementBounds hourHandBounds = ElementStdBounds.SlotGrid(EnumDialogArea.None, 0.0, 110.0, 1, 1);
            ElementBounds minuteHandBounds = ElementStdBounds.SlotGrid(EnumDialogArea.None, 153.0, 110.0, 1, 1);
            ElementBounds disguiseSlotBounds = ElementStdBounds.SlotGrid(EnumDialogArea.None, 0.0, 270.0, 1, 1);


            ElementBounds bgBounds = ElementBounds.Fill.WithFixedPadding(GuiStyle.ElementToDialogPadding);
            bgBounds.BothSizing = ElementSizing.FitToChildren;
            bgBounds.WithChildren(clockBounds);

            ElementBounds dialogBounds = ElementStdBounds.AutosizedMainDialog;
            IGuiAPI gui = this.capi.Gui;


            SingleComposer = gui.CreateCompo(Core.ModId + ":fatherclock" + Pos, dialogBounds)
                .AddDialogBG(bgBounds, true)
            .AddDialogTitleBar(DialogTitle, () => TryClose())
                .BeginChildElements(bgBounds)
                    .AddItemSlotGrid(Inventory, SendInvPacket, 1, new int[] { 0 }, hourHandBounds)
                    .AddItemSlotGrid(Inventory, SendInvPacket, 1, new int[] { 1 }, minuteHandBounds)
                    .AddItemSlotGrid(Inventory, SendInvPacket, 1, new int[] { 2 }, disguiseSlotBounds)
                    .AddItemSlotGrid(Inventory, SendInvPacket, 1, new int[] { 3 }, tickMarksSlotBounds)

                .EndChildElements()
                .Compose();
        }
    }
}
