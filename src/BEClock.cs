using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Datastructures;
using Vintagestory.GameContent;

namespace decoclock.src
{
    //class BEClock: BlockEntityDisplay
    //{
    //    ILoadedSound minutTickSound;
    //    ILoadedSound hourTickSound;

    //    public override InventoryBase Inventory => inv;
    //    public override string InventoryClassName => "clock";

    //    InventoryClock inv;


    //    bool hasMinuteHand;
    //    bool hasHoureHand;
    //    bool hasDial;
    //    bool hasGlasspanel;

    //    public bool IsComplete => hasDial && hasMinuteHand && hasHoureHand;

    //    public BEClock()
    //    {
    //        inv = new InventoryClock(this, 4);
    //        inv.SlotModified += Inv_SlotModified;
    //        meshes = new MeshData[2];
    //    }

    //    private void Inv_SlotModified(int t1)
    //    {
    //        updateMeshes();
    //    }

    //    public override void FromTreeAttributes(ITreeAttribute tree, IWorldAccessor worldAccessForResolve)
    //    {
    //        base.FromTreeAttributes(tree, worldAccessForResolve);

    //        hasMinuteHand = tree.GetBool("hasMinuteHand");
    //        hasHoureHand = tree.GetBool("hasHoureHand");
    //        hasDial = tree.GetBool("hasDial");
    //        hasDial = tree.GetBool("hasGlasspanel");
    //    }

    //    public override void ToTreeAttributes(ITreeAttribute tree)
    //    {
    //        base.ToTreeAttributes(tree);

    //        tree.SetBool("hasMinuteHand", hasMinuteHand);
    //        tree.SetBool("hasHoureHand", hasHoureHand);
    //        tree.SetBool("hasDial", hasDial);
    //        tree.SetBool("hasGlasspanel", hasGlasspanel);
    //    }


    //}

    //public class InventoryClock : InventoryDisplayed
    //{
    //    public InventoryClock(BlockEntity be, int size) : base(be, size, "clock-0", null)
    //    {
    //        slots = GenEmptySlots(size);
    //        for (int i = 0; i < size; i++) slots[i].MaxSlotStackSize = 1;
    //    }

    //    public override float GetSuitability(ItemSlot sourceSlot, ItemSlot targetSlot, bool isMerge)
    //    {
    //        if (targetSlot == slots[slots.Length - 1]) return 0;  //disallow hoppers/chutes to place any items in the PounderCap slot
    //        return base.GetSuitability(sourceSlot, targetSlot, isMerge);
    //    }
    //}
}
