using System;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;

namespace DecoClock
{
    internal class GuiDialogClock : GuiDialogGeneric
    {
        InventoryClock inventory;
        BlockPos BlockEntityPosition;

        public GuiDialogClock(InventoryClock inventory, BlockPos blockEntityPos, ICoreClientAPI capi) : base(Core.ModId + ":fatherclock-title", capi)
        {
            this.inventory = inventory;
            BlockEntityPosition = blockEntityPos;
        }

        public override double DrawOrder => 0.2;
        public override bool PrefersUngrabbedMouse => false;

        public override bool TryOpen()
        {
            ComposeDialog();
            return base.TryOpen();
        }
        private void ComposeDialog()
        {
            ElementBounds clockBounds = ElementBounds.Fixed(0.0, 0.0, 200.0, 290.0);
            ElementBounds clockworkSlotBounds = ElementStdBounds.SlotGrid(EnumDialogArea.None, 0.0, 30.0, 1, 1);
            ElementBounds tickMarksSlotBounds = ElementStdBounds.SlotGrid(EnumDialogArea.None, 153.0, 30.0, 1, 1);
            ElementBounds hourHandBounds = ElementStdBounds.SlotGrid(EnumDialogArea.None, 0.0, 110.0, 1, 1);
            ElementBounds minuteHandBounds = ElementStdBounds.SlotGrid(EnumDialogArea.None, 153.0, 110.0, 1, 1);
            ElementBounds dialGlassSlotBounds = ElementStdBounds.SlotGrid(EnumDialogArea.None, 0.0, 190.0, 1, 1);
            ElementBounds clockPartsSlotBounds = ElementStdBounds.SlotGrid(EnumDialogArea.None, 153.0, 190.0, 1, 1);
            ElementBounds doorGlassSlotBounds = ElementStdBounds.SlotGrid(EnumDialogArea.None, 0.0, 270.0, 1, 1);


            ElementBounds bgBounds = ElementBounds.Fill.WithFixedPadding(GuiStyle.ElementToDialogPadding);
            bgBounds.BothSizing = ElementSizing.FitToChildren;
            bgBounds.WithChildren(clockBounds);

            ElementBounds dialogBounds = ElementStdBounds.AutosizedMainDialog;
            IGuiAPI gui = this.capi.Gui;


            SingleComposer = gui.CreateCompo(Core.ModId + ":fatherclock", dialogBounds)
                .AddDialogBG(bgBounds, true)
                .AddDialogTitleBar(DialogTitle, () => TryClose())
                .BeginChildElements(bgBounds)
                    .AddItemSlotGrid(inventory, SendInvPacket, 1, new int[] { 0 }, clockworkSlotBounds)
                    .AddItemSlotGrid(inventory, SendInvPacket, 1, new int[] { 1 }, tickMarksSlotBounds)
                    .AddItemSlotGrid(inventory, SendInvPacket, 1, new int[] { 2 }, hourHandBounds)
                    .AddItemSlotGrid(inventory, SendInvPacket, 1, new int[] { 3 }, minuteHandBounds)
                    .AddItemSlotGrid(inventory, SendInvPacket, 1, new int[] { 4 }, dialGlassSlotBounds)
                    .AddItemSlotGrid(inventory, SendInvPacket, 1, new int[] { 5 }, clockPartsSlotBounds)
                    .AddItemSlotGrid(inventory, SendInvPacket, 1, new int[] { 6 }, doorGlassSlotBounds)

                .EndChildElements()
                .Compose();

        }
        private void SendInvPacket(object p)
        {
            capi.Network.SendBlockEntityPacket(BlockEntityPosition.X, BlockEntityPosition.Y, BlockEntityPosition.Z, p);
        }

    }
}
