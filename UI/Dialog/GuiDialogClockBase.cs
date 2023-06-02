
using Vintagestory.API.Client;
using Vintagestory.API.MathTools;

namespace DecoClock
{
    internal abstract class GuiDialogClockBase : GuiDialogGeneric
    {
        internal InventoryClock Inventory { get; set; }
        internal BlockPos Pos { get; set; }

        public GuiDialogClockBase(string dialogTitle,InventoryClock inventory, BlockPos blockEntityPos, ICoreClientAPI capi) : base(dialogTitle, capi)
        {
            Inventory = inventory;
            Pos = blockEntityPos;
        }
        public override double DrawOrder => 0.2;
        public override bool PrefersUngrabbedMouse => false;

        public override bool TryOpen()
        {
            ComposeDialog();
            // inventory.SlotModified += OnSlotModifid;
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

        public override void OnGuiOpened()
        {
            //inventory.Open(capi.World.Player);
            capi.Network.SendBlockEntityPacket(Pos.X, Pos.Y, Pos.Z, 1000, null);
            capi.World.Player.InventoryManager.OpenInventory(Inventory);
        }

        public override void OnGuiClosed()
        {

            //inventory.SlotModified -= OnSlotModifid;
            //inventory.Close(capi.World.Player);
            capi.World.Player.InventoryManager.CloseInventory(Inventory);
            capi.Network.SendBlockEntityPacket(Pos.X, Pos.Y, Pos.Z, 1001, null);
        }
        public void SendInvPacket(object p)
        {
            capi.Network.SendBlockEntityPacket(Pos.X, Pos.Y, Pos.Z, p);
        }

    }
}
