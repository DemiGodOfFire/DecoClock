using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.MathTools;

namespace DecoClock
{
    internal class GuiDialogGrandfatherClock : GuiDialogClockBase
    {
        readonly string[] parts =
        {
            "clockwork",
            "tickmarks",
            "hourhand",
            "minutehand",
            "dialglass",
            "clockparts",
            "doorglass"
        };

        public GuiDialogGrandfatherClock(string dialogTitle, InventoryClock inventory, BlockPos blockEntityPos, ICoreClientAPI capi)
            : base(dialogTitle, inventory, blockEntityPos, capi)
        {
        }

        public override bool TryOpen()
        {
             Inventory.SlotModified += OnSlotModified;
            return base.TryOpen();
        }

        public override void OnGuiClosed()
        {
            Inventory.SlotModified -= OnSlotModified;
            base.OnGuiClosed();
        }

        private void OnSlotModified(int slot)
        {
            if (Inventory[slot].Empty)
            {
                var hoverText = SingleComposer.GetHoverText("hover");
                hoverText.SetNewText(Lang.Get($"{Core.ModId}:{parts[slot]}"));
                hoverText.SetVisible(true);
            }
            else
            {
                var hoverText = SingleComposer.GetHoverText("hover");
                hoverText.SetVisible(false);
            }
        }

        public override bool OnMouseEnterSlot(ItemSlot slot)
        {
            if (slot.Empty)
            {
                int i = Inventory.GetSlotId(slot);
                if (i != -1)
                {
                    var hoverText = SingleComposer.GetHoverText("hover");
                    hoverText.SetNewText(Lang.Get($"{Core.ModId}:{parts[i]}"));
                    hoverText.SetVisible(true);
                }
            }
            return base.OnMouseEnterSlot(slot);
        }

        public override bool OnMouseLeaveSlot(ItemSlot itemSlot)
        {
            var hoverText = SingleComposer.GetHoverText("hover");
            hoverText.SetVisible(false);
            return base.OnMouseLeaveSlot(itemSlot);
        }

        public override void ComposeDialog()
        {
            SingleComposer?.Dispose();

            ElementBounds clockBounds = ElementBounds.Fixed(0.0, 0.0, 200.0, 290.0);
            ElementBounds clockworkSlotBounds = ElementStdBounds.SlotGrid(EnumDialogArea.None, 76.0, 110.0, 1, 1);
            ElementBounds tickMarksSlotBounds = ElementStdBounds.SlotGrid(EnumDialogArea.None, 76.0, 30.0, 1, 1);
            ElementBounds hourHandBounds = ElementStdBounds.SlotGrid(EnumDialogArea.None, 0.0, 110.0, 1, 1);
            ElementBounds minuteHandBounds = ElementStdBounds.SlotGrid(EnumDialogArea.None, 153.0, 110.0, 1, 1);
            ElementBounds dialGlassSlotBounds = ElementStdBounds.SlotGrid(EnumDialogArea.None, 0.0, 190.0, 1, 1);
            ElementBounds clockPartsSlotBounds = ElementStdBounds.SlotGrid(EnumDialogArea.None, 76.0, 190.0, 1, 1);
            ElementBounds doorGlassSlotBounds = ElementStdBounds.SlotGrid(EnumDialogArea.None, 153.0, 190.0, 1, 1);
            ElementBounds hoverBounds = ElementBounds.Fixed(0, 0, 200, 26);

            ElementBounds bgBounds = ElementBounds.Fill.WithFixedPadding(GuiStyle.ElementToDialogPadding);
            bgBounds.BothSizing = ElementSizing.FitToChildren;
            bgBounds.WithChildren(clockBounds);

            ElementBounds dialogBounds = ElementStdBounds.AutosizedMainDialog;
            IGuiAPI gui = this.capi.Gui;

            SingleComposer = gui.CreateCompo(Core.ModId + ":fatherclock" + Pos, dialogBounds)
                .AddDialogBG(bgBounds, true)
                .AddDialogTitleBar(DialogTitle, () => TryClose())
                .BeginChildElements(bgBounds)
                    .AddItemSlotGrid(Inventory, SendInvPacket, 1, new int[] { 0 }, clockworkSlotBounds)
                    .AddItemSlotGrid(Inventory, SendInvPacket, 1, new int[] { 1 }, tickMarksSlotBounds)
                    .AddItemSlotGrid(Inventory, SendInvPacket, 1, new int[] { 2 }, hourHandBounds)
                    .AddItemSlotGrid(Inventory, SendInvPacket, 1, new int[] { 3 }, minuteHandBounds)
                    .AddItemSlotGrid(Inventory, SendInvPacket, 1, new int[] { 4 }, dialGlassSlotBounds)
                    .AddItemSlotGrid(Inventory, SendInvPacket, 1, new int[] { 5 }, clockPartsSlotBounds)
                    .AddItemSlotGrid(Inventory, SendInvPacket, 1, new int[] { 6 }, doorGlassSlotBounds)

                    .AddAutoSizeHoverText("", CairoFont.WhiteSmallText(), 200, hoverBounds, "hover")
                .EndChildElements()
                .Compose();
        }
    }
}
