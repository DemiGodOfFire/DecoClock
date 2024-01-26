using System;
using Vintagestory.API.Client;
using Vintagestory.API.Config;
using Vintagestory.API.MathTools;

namespace DecoClock
{
    internal class GuiDialogBigClock : GuiDialogClockBase
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
            ElementBounds radiusBounds = ElementBounds.Fixed(0, 250, 50, 30);
            ElementBounds shiftBounds = ElementBounds.Fixed(80, 250, 50, 30);
            ElementBounds typeDialBounds = ElementBounds.Fixed(80, 90, 50, 30);
            ElementBounds hoverBounds = ElementBounds.Fixed(0, 0, 0, 26);
            ElementBounds muteSoundsBounds = ElementBounds.Fixed(168, 255, 50, 50);

            //ElementBounds textBounds = ElementBounds.Fixed(0.0, 0.0, 20.0, 50.0);
            ElementBounds bgBounds = ElementBounds.Fill.WithFixedPadding(GuiStyle.ElementToDialogPadding);
            bgBounds.BothSizing = ElementSizing.FitToChildren;
            bgBounds.WithChildren(clockBounds);

            ElementBounds dialogBounds = ElementStdBounds.AutosizedMainDialog;
            IGuiAPI gui = this.capi.Gui;


            SingleComposer = gui.CreateCompo(Core.ModId + ":bigclock" + Pos, dialogBounds)
                .AddDialogBG(bgBounds, true)
                .AddDialogTitleBar(DialogTitle, () => TryClose())
                .BeginChildElements(bgBounds)
                    .AddItemSlotGrid(Inventory, SendInvPacket, 1, new int[] { 0 }, hourHandBounds)
                    .AddItemSlotGrid(Inventory, SendInvPacket, 1, new int[] { 1 }, minuteHandBounds)
                    .AddItemSlotGrid(Inventory, SendInvPacket, 1, new int[] { 2 }, disguiseSlotBounds)
                    .AddItemSlotGrid(Inventory, SendInvPacket, 1, new int[] { 3 }, tickMarksSlotBounds)
                    .AddAutoSizeHoverText("", CairoFont.WhiteSmallText(), 200, hoverBounds, "hover")
                    .AddSlider(OnRadiusChanged, radiusBounds, "radius")
                    .AddSlider(OnShiftChanged, shiftBounds, "shift")
                    .AddSlider(OnDialChanged, typeDialBounds, "typedial")
                    .AddSwitch(OnMuteChanged, muteSoundsBounds, "mutesounds")
                    .AddAutoSizeHoverText(Lang.Get($"{Core.ModId}:typedial"), CairoFont.WhiteSmallText(), 200, typeDialBounds)
                    .AddAutoSizeHoverText(Lang.Get($"{Core.ModId}:radius"), CairoFont.WhiteSmallText(), 200, radiusBounds)
                    .AddAutoSizeHoverText(Lang.Get($"{Core.ModId}:mute"), CairoFont.WhiteSmallText(), 200, muteSoundsBounds)
                    
                .EndChildElements()
                .Compose();
            SingleComposer.GetSlider("radius").SetValues(GetRadius(), 1, 7, 1);
            SingleComposer.GetSlider("shift").SetValues(GetShiftZ(), 0, 100, 1,"%");
            SingleComposer.GetSlider("typedial").SetValues(GetTypeDial(), 1, 9, 1);
        }

        private bool OnRadiusChanged(int value)
        {
            capi.Network.SendBlockEntityPacket(Pos.X, Pos.Y, Pos.Z, Constants.Radius, BitConverter.GetBytes(value));
            return true;
        }

        private bool OnShiftChanged(int value)
        {
            capi.Network.SendBlockEntityPacket(Pos.X, Pos.Y, Pos.Z, Constants.ShiftZ, BitConverter.GetBytes(value));
            return true;
        }

        private int GetRadius()
        {
            if (capi.World.BlockAccessor.GetBlockEntity(Pos) is BEBigClock be)
            {
                return be.Radius;
            }
            return 0;
        }

        private int GetShiftZ()
        {
            if (capi.World.BlockAccessor.GetBlockEntity(Pos) is BEBigClock be)
            {
                return be.ShiftZ;
            }
            return 0;
        }

        public override int GetTypeDial()
        {
            if (capi.World.BlockAccessor.GetBlockEntity(Pos) is BEBigClock be)
            {
                return be.TypeDial;
            }
            return 0;
        }


        public override bool GetMuteSounds()
        {
            if (capi.World.BlockAccessor.GetBlockEntity(Pos) is BEBigClock be)
            {
                return be.MuteSounds;
            }
            return false;
        }
    }
}
