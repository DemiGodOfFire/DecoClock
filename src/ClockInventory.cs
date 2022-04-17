using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Common;

namespace DecoClock.src
{
    internal class ClockInventory : InventoryGeneric
    {
        public ClockInventory(string[] listCode , int quantitySlots, string invId, ICoreAPI api, NewSlotDelegate onNewSlot = null): base(listCode.Length, invId, api, onNewSlot)
        {

        }

      
    }
}
