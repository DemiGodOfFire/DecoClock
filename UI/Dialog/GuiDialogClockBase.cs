using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.MathTools;

namespace DecoClock
{
    internal abstract class GuiDialogClockBase : GuiDialogGeneric
    {
        protected BlockPos Pos { get; set; }
        protected InventoryClock Inventory { get; set; }
        public abstract string[] Parts { get;}

        public GuiDialogClockBase(string dialogTitle,InventoryClock inventory, BlockPos blockEntityPos, ICoreClientAPI capi) :
            base(Lang.Get(dialogTitle), capi)
        {
            Inventory = inventory;
            Pos = blockEntityPos;
        }
        public override double DrawOrder => 0.2;
        public override bool PrefersUngrabbedMouse => false;

        public override bool TryOpen()
        {
            ComposeDialog();
            Inventory.SlotModified += OnSlotModified;
            return base.TryOpen();
        }
        public abstract void ComposeDialog();

        public override void OnFinalizeFrame(float dt)
        {
            base.OnFinalizeFrame(dt);
            if (!IsInRangeOfBlock(Pos))
            {
                capi.Event.EnqueueMainThreadTask(delegate
                {
                    TryClose();
                }, "closedlg");
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
                    hoverText.SetNewText(Lang.Get($"{Core.ModId}:{Parts[i]}"));
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

        private void OnSlotModified(int slot)
        {
            if (Inventory[slot].Empty)
            {
                var hoverText = SingleComposer.GetHoverText("hover");
                hoverText.SetNewText(Lang.Get($"{Core.ModId}:{Parts[slot]}"));
                hoverText.SetVisible(true);
            }
            else
            {
                var hoverText = SingleComposer.GetHoverText("hover");
                hoverText.SetVisible(false);
            }
        }

        public override void OnGuiOpened()
        {
            capi.Network.SendBlockEntityPacket(Pos.X, Pos.Y, Pos.Z, 1000, null);
            capi.World.Player.InventoryManager.OpenInventory(Inventory);
        }

        public override void OnGuiClosed()
        {

            Inventory.SlotModified -= OnSlotModified;
            capi.World.Player.InventoryManager.CloseInventory(Inventory);
            capi.Network.SendBlockEntityPacket(Pos.X, Pos.Y, Pos.Z, 1001, null);
        }
        public void SendInvPacket(object p)
        {
            capi.Network.SendBlockEntityPacket(Pos.X, Pos.Y, Pos.Z, p);
        }

    }
}
