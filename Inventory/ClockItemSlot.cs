using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Common;

namespace DecoClock
{
    internal class ClockItemSlot : ItemSlot
    {
        public ClockItemSlot(int slotId, InventoryClock inventory) : base(inventory)
        {
        }
        public override bool CanTake()
        {
            if (inventory?.TakeLocked ?? false)
            {
                return false;
            }

            return itemstack != null && ((InventoryClock)inventory).CanTake(this);
        }
    }
}
