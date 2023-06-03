using System;
using Vintagestory.API.Client;
using Vintagestory.API.Config;
using Vintagestory.API.MathTools;
using Vintagestory.GameContent;

namespace DecoClock
{
    internal class GuiDialogBigClock: GuiDialogClockBase
    {
        public GuiDialogBigClock(string dialogTitle, InventoryClock inventory, BlockPos blockEntityPos, ICoreClientAPI capi) : base(dialogTitle, inventory, blockEntityPos, capi)
        {

        }


        public override void ComposeDialog()
        {
            ElementBounds clockBounds = ElementBounds.Fixed(0.0, 0.0, 200.0, 310.0);
            ElementBounds tickMarksSlotBounds = ElementStdBounds.SlotGrid(EnumDialogArea.None, 80.0, 30.0, 1, 1);
            ElementBounds hourHandBounds = ElementStdBounds.SlotGrid(EnumDialogArea.None, 0.0, 110.0, 1, 1);
            ElementBounds minuteHandBounds = ElementStdBounds.SlotGrid(EnumDialogArea.None, 160.0, 110.0, 1, 1);
            ElementBounds disguiseSlotBounds = ElementStdBounds.SlotGrid(EnumDialogArea.None, 80.0, 190.0, 1, 1);
            ElementBounds radiusBounds = ElementBounds.Fixed(0, 250, 40, 30);
            ElementBounds textBounds = ElementBounds.Fixed(0.0, 0.0, 20.0, 50.0);
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
                  
                    .AddHoverText(Lang.Get("Hour hand"), CairoFont.WhiteSmallText(), Lang.Get("Hour hand").Length*9, hourHandBounds)
                    .AddHoverText(Lang.Get("Minute hand"), CairoFont.WhiteSmallText(), Lang.Get("Minute hand").Length*9, minuteHandBounds)
                    .AddHoverText(Lang.Get("Disguise"), CairoFont.WhiteSmallText(), Lang.Get("Disguise").Length*9, disguiseSlotBounds)
                    .AddHoverText(Lang.Get("Dial"), CairoFont.WhiteSmallText(), Lang.Get("Dial").Length*9, tickMarksSlotBounds)
                    //.AddNumberInput(radiusBounds,OnRadiusChanged)
                    .AddSlider(OnRadiusChanged,radiusBounds,"radius")
                .EndChildElements()
                .Compose();
            SingleComposer.GetSlider("radius").SetValues(1, 1, 5, 1);
        }

        private bool OnRadiusChanged(int value)
        {
            BEBigClock be = (BEBigClock)capi.World.BlockAccessor.GetBlockEntity(Pos);
            //be.
            return true;
        }
    }
}
