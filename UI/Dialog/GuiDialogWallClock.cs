using Vintagestory.API.Client;
using Vintagestory.API.MathTools;

namespace DecoClock
{
    internal class GuiDialogWallClock : GuiDialogClockBase
    {
        public GuiDialogWallClock(string dialogTitle, InventoryClock inventory, BlockPos blockEntityPos, ICoreClientAPI capi) : base(dialogTitle, inventory, blockEntityPos, capi)
        {
        }
        public override string[] Parts { get; } = new string[]
        {
            "clockwork",
            "tickmarks",
            "hourhand",
            "minutehand",
            "dialglass"
        };

        public override void ComposeDialog()
        {
            ElementBounds clockBounds = ElementBounds.Fixed(0.0, 0.0, 200.0, 290.0);
            ElementBounds tickMarksSlotBounds = ElementStdBounds.SlotGrid(EnumDialogArea.None, 76.0, 190.0, 1, 1);
            ElementBounds clockWork = ElementStdBounds.SlotGrid(EnumDialogArea.None, 76.0, 110.0, 1, 1);
            ElementBounds hourHandBounds = ElementStdBounds.SlotGrid(EnumDialogArea.None, 0.0, 110.0, 1, 1);
            ElementBounds minuteHandBounds = ElementStdBounds.SlotGrid(EnumDialogArea.None, 153.0, 110.0, 1, 1);
            ElementBounds dialGlassSlotBounds = ElementStdBounds.SlotGrid(EnumDialogArea.None, 76.0, 30.0, 1, 1);
            ElementBounds hoverBounds = ElementBounds.Fixed(0, 0, 0, 26);

            ElementBounds bgBounds = ElementBounds.Fill.WithFixedPadding(GuiStyle.ElementToDialogPadding);
            bgBounds.BothSizing = ElementSizing.FitToChildren;
            bgBounds.WithChildren(clockBounds);

            ElementBounds dialogBounds = ElementStdBounds.AutosizedMainDialog;
            IGuiAPI gui = this.capi.Gui;


            SingleComposer = gui.CreateCompo(Core.ModId + ":fatherclock" + Pos, dialogBounds)
                .AddDialogBG(bgBounds, true)
                .AddDialogTitleBar(DialogTitle, () => TryClose())
                .BeginChildElements(bgBounds)
                    .AddItemSlotGrid(Inventory, SendInvPacket, 1, new int[] { 0 }, clockWork)
                    .AddItemSlotGrid(Inventory, SendInvPacket, 1, new int[] { 1 }, tickMarksSlotBounds)
                    .AddItemSlotGrid(Inventory, SendInvPacket, 1, new int[] { 2 }, hourHandBounds)
                    .AddItemSlotGrid(Inventory, SendInvPacket, 1, new int[] { 3 }, minuteHandBounds)
                    .AddItemSlotGrid(Inventory, SendInvPacket, 1, new int[] { 4 }, dialGlassSlotBounds)
                    .AddAutoSizeHoverText("", CairoFont.WhiteSmallText(), 200, hoverBounds, "hover")
                .EndChildElements()
                .Compose();
        }
    }
}
