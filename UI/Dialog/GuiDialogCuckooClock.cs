using Vintagestory.API.Client;
using Vintagestory.API.Config;
using Vintagestory.API.MathTools;

namespace DecoClock
{
    internal class GuiDialogCuckooClock : GuiDialogClockBase
    {
        public GuiDialogCuckooClock(string dialogTitle, InventoryClock inventory, BlockPos blockEntityPos, ICoreClientAPI capi)
            : base(dialogTitle, inventory, blockEntityPos, capi)
        {
        }

        public override string[] Parts { get; } = new string[]
        {
            "clockwork",
            "tickmarks",
            "hourhand",
            "minutehand",
            "dialglass",
            "clockparts",
            "cuckoo"
        };

        public override void ComposeDialog()
        {
            SingleComposer?.Dispose();

            ElementBounds clockBounds = ElementBounds.Fixed(0.0, 0.0, 200.0, 290.0);
            ElementBounds clockworkSlotBounds = ElementStdBounds.SlotGrid(EnumDialogArea.None, 76.0, 110.0, 1, 1);
            ElementBounds tickMarksSlotBounds = ElementStdBounds.SlotGrid(EnumDialogArea.None, 0.0, 190.0, 1, 1);
            ElementBounds cuckooSlotBounds = ElementStdBounds.SlotGrid(EnumDialogArea.None, 76.0, 30.0, 1, 1);
            ElementBounds hourHandBounds = ElementStdBounds.SlotGrid(EnumDialogArea.None, 0.0, 110.0, 1, 1);
            ElementBounds minuteHandBounds = ElementStdBounds.SlotGrid(EnumDialogArea.None, 153.0, 110.0, 1, 1);
            ElementBounds dialGlassSlotBounds = ElementStdBounds.SlotGrid(EnumDialogArea.None, 153.0, 190.0, 1, 1);
            ElementBounds clockPartsSlotBounds = ElementStdBounds.SlotGrid(EnumDialogArea.None, 76.0, 190.0, 1, 1);
            ElementBounds hoverBounds = ElementBounds.Fixed(0, 0, 0, 26);
            ElementBounds typeDialBounds = ElementBounds.Fixed(0, 255, 50, 30);
            ElementBounds muteSoundsBounds = ElementBounds.Fixed(168, 255, 50, 50);


            ElementBounds bgBounds = ElementBounds.Fill.WithFixedPadding(GuiStyle.ElementToDialogPadding);
            bgBounds.BothSizing = ElementSizing.FitToChildren;
            bgBounds.WithChildren(clockBounds);

            ElementBounds dialogBounds = ElementStdBounds.AutosizedMainDialog;
            IGuiAPI gui = this.capi.Gui;

            SingleComposer = gui.CreateCompo(Core.ModId + ":сuckooclock" + Pos, dialogBounds)
                .AddDialogBG(bgBounds, true)
                .AddDialogTitleBar(DialogTitle, () => TryClose())
                .BeginChildElements(bgBounds)
                    .AddItemSlotGrid(Inventory, SendInvPacket, 1, new int[] { 0 }, clockworkSlotBounds)
                    .AddItemSlotGrid(Inventory, SendInvPacket, 1, new int[] { 1 }, tickMarksSlotBounds)
                    .AddItemSlotGrid(Inventory, SendInvPacket, 1, new int[] { 2 }, hourHandBounds)
                    .AddItemSlotGrid(Inventory, SendInvPacket, 1, new int[] { 3 }, minuteHandBounds)
                    .AddItemSlotGrid(Inventory, SendInvPacket, 1, new int[] { 4 }, dialGlassSlotBounds)
                    .AddItemSlotGrid(Inventory, SendInvPacket, 1, new int[] { 5 }, clockPartsSlotBounds)
                    .AddItemSlotGrid(Inventory, SendInvPacket, 1, new int[] { 6 }, cuckooSlotBounds)
                    .AddSlider(OnDialChanged, typeDialBounds, "typedial")
                    .AddSwitch(OnMuteChanged, muteSoundsBounds,"mutesounds")

                    .AddAutoSizeHoverText(Lang.Get($"{Core.ModId}:typedial"), CairoFont.WhiteSmallText(), 200, typeDialBounds)
                    .AddAutoSizeHoverText(Lang.Get($"{Core.ModId}:mute"), CairoFont.WhiteSmallText(), 200, muteSoundsBounds)
                    .AddAutoSizeHoverText("", CairoFont.WhiteSmallText(), 200, hoverBounds, "hover")
                .EndChildElements()
                .Compose();
            SingleComposer.GetSlider("typedial").SetValues(GetTypeDial(), 1, 9, 1);
            SingleComposer.GetSwitch("mutesounds").SetValue(GetMuteSounds());
        }
        public override int GetTypeDial()
        {
            BECuckooClock be = (BECuckooClock)capi.World.BlockAccessor.GetBlockEntity(Pos);
            return be.TypeDial;
        }

        public override bool GetMuteSounds()
        {
            BECuckooClock be = (BECuckooClock)capi.World.BlockAccessor.GetBlockEntity(Pos);
            return be.MuteSounds;
        }
    }
}
