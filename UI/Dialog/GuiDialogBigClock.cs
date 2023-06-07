using Newtonsoft.Json.Linq;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;

namespace DecoClock
{
    internal class GuiDialogBigClock: GuiDialogClockBase
    {
        public GuiDialogBigClock(string dialogTitle, InventoryClock inventory, BlockPos blockEntityPos, ICoreClientAPI capi) : base(dialogTitle, inventory, blockEntityPos, capi)
        {
        }

        public override string[] Parts { get; } = new string[]
       {
            "hourhand",
            "minutehand",
            "disguise",
            "tickmarks"
       };

        public override void ComposeDialog()
        {
            ElementBounds clockBounds = ElementBounds.Fixed(0.0, 0.0, 200.0, 310.0);
            ElementBounds tickMarksSlotBounds = ElementStdBounds.SlotGrid(EnumDialogArea.None, 80.0, 30.0, 1, 1);
            ElementBounds hourHandBounds = ElementStdBounds.SlotGrid(EnumDialogArea.None, 0.0, 110.0, 1, 1);
            ElementBounds minuteHandBounds = ElementStdBounds.SlotGrid(EnumDialogArea.None, 160.0, 110.0, 1, 1);
            ElementBounds disguiseSlotBounds = ElementStdBounds.SlotGrid(EnumDialogArea.None, 80.0, 190.0, 1, 1);
            ElementBounds radiusBounds = ElementBounds.Fixed(0, 250, 40, 30);
            ElementBounds hoverBounds = ElementBounds.Fixed(0, 0, 0, 26);

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
                    .AddAutoSizeHoverText("", CairoFont.WhiteSmallText(), 200, hoverBounds, "hover")

                    //.AddNumberInput(radiusBounds,OnRadiusChanged)
                    .AddSlider(OnRadiusChanged,radiusBounds,"radius")
                .EndChildElements()
                .Compose();
            SingleComposer.GetSlider("radius").SetValues(GetRadius(), 1, 3, 1);
        }

        private bool OnRadiusChanged(int value)
        {
            BEBigClock be = (BEBigClock)capi.World.BlockAccessor.GetBlockEntity(Pos);
            be.Radius = value;
            if (capi.World.Side == EnumAppSide.Client)
            {
                be.UpdateMesh();
            }
            be.MarkDirty(true);
            return true;
        }

        private int GetRadius()
        {
            BEBigClock be = (BEBigClock)capi.World.BlockAccessor.GetBlockEntity(Pos);
            return be.Radius;
        }
    }
}
