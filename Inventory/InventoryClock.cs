using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.MathTools;

namespace DecoClock
{
    //internal enum listItems
    //{
    //    clockWork,
    //    minutehand,
    //    parts,
    //    hourhand,
    //    tickmarks
    //};

    // [System.Serializable]
    public class ClockItem
    {
        public string Type { get; private set; } 
        public string? Dependency { get; private set; }
        public AssetLocation[]? Codes { get; set; }

        public ClockItem(string type, string? dependency = null, AssetLocation[]? codes = null)
        {
            Type = type;
            Codes = codes;
            Dependency = dependency;
        }
    }

    public class InventoryClock : InventoryGeneric
    {
        private readonly ClockItem[] codes;
        public InventoryClock(ClockItem[] codes, BlockPos pos, ICoreAPI api) :
            base(codes.Length, "DecoClock-ClockInv", pos + "", api, OnNewSlot)
        {
            this.codes = codes;
            if (api != null)
            {
                ResolveCodes();
            }
        }

        private void ResolveCodes()
        {
            ClockManager manager = Api.ModLoader.GetModSystem<ClockManager>();
            foreach (var code in codes)
            {
                if (manager.Parts.ContainsKey(code.Type))
                {
                    code.Codes ??= manager.Parts[code.Type].ToArray();
                }
                else
                {
                    Api.Logger.Error("Achtung!" + " not found code " + code.Type);
                }
            }
        }
      
        private static ItemSlot OnNewSlot(int slotId, InventoryGeneric self)
        {
            return new ClockItemSlot(slotId, (InventoryClock)self)
            {
                MaxSlotStackSize = 1
        };
        }

        /// <summary>
        /// Show content if exist
        /// </summary>
        /// <param name="type"></param>
        /// <returns>true/false</returns>
        public bool IsExist(string type)
        {
            for (int i = 0; i < codes.Length; i++)
            {
                if (codes[i].Type == type)
                {
                    if (!slots[i].Empty) { return true; }
                }
            }
            return false;
        }

        /// <summary>
        /// Show content if exist
        /// </summary>
        /// <param name="type"></param>
        /// <returns>ItemStack</returns>
        public ItemStack? TryGetPart(string type)
        {
            for (int i = 0; i < codes.Length; i++)
            {
                if (codes[i].Type == type)
                    return slots[i].Itemstack?.Clone();
            }
            return null;
        }

        /// <summary>
        /// Finds inventory slot number by type
        /// </summary>
        /// <param name="type"></param>
        /// <returns>Id</returns>
        public int IdSlot(string type)
        {
            for (int i = 0; i < codes.Length; i++)
            {
                if (codes[i].Type == type)
                {
                    return i;
                }
            }
            return -1;
        }

        private bool CanAddPart(int id)
        {        
            if(codes[id].Dependency!=null)
            {
                if (slots[IdSlot(codes[id].Dependency)].Empty)
                {
                    if (Api is ICoreClientAPI capi)
                    {
                        capi.TriggerIngameError(this, null, Lang.Get($"{Core.ModId}:clock-error-missingclocwork"));
                    }
                    return false;
                }
            }
            return true;
        }

        ///// <summary>
        ///// Take content and remove it if exist
        ///// </summary>
        ///// <param name="type"></param>
        ///// <returns></returns>
        //public ItemStack? TryTakePart(string type)
        //{
        //    for (int i = 0; i < codes.Length; i++)
        //    {
        //        if (codes[i].Type == type)
        //        {
        //            if (slots[i].Empty)
        //                return null;
        //            return slots[i].TakeOutWhole();
        //        }
        //    }
        //    return null;
        //}

        ///// <summary>
        ///// Add part if suitable and return original content if exist 
        ///// </summary>
        ///// <param name="item"></param>
        ///// <param name="content"></param>
        ///// <returns></returns>
        //public bool TryAddPart(ItemStack item, out ItemStack? content)
        //{
        //    if (item != null)
        //    {
        //        for (int i = 0; i < codes.Length; i++)
        //        {
        //            if (codes[i].Codes != null)
        //            {
        //                foreach (var code in codes[i].Codes)
        //                {
        //                    if (code == item.Collectible.Code)
        //                    {
        //                        content = slots[i].Itemstack?.Clone();
        //                        slots[i].Itemstack = item.Clone();
        //                        slots[i].Itemstack.StackSize = 1;
        //                        return true;
        //                    }
        //                }
        //            }
        //        }
        //    }
        //    content = null;
        //    return false;
        //}

        public override void LateInitialize(string inventoryID, ICoreAPI api)
        {
            base.LateInitialize(inventoryID, api);
            ResolveCodes();
            //for (int i = 0; i < slots.Length; i++)
            //{
            //    slots[i].MaxSlotStackSize = 1;
            //}

        }

        public override bool CanContain(ItemSlot sinkSlot, ItemSlot sourceSlot)
        {
            int id = sinkSlot.Inventory.GetSlotId(sinkSlot);
            var code = codes[id].Type;
            return code switch
            {
                "dialglass" or "doorglass" => "glass" == sourceSlot.Itemstack.Collectible.Code.FirstCodePart(),
                "disguise" => (sourceSlot.Itemstack.Class == EnumItemClass.Block)&&
                (MaxContentDimensions?.CanContain(sourceSlot.Itemstack.Collectible.Dimensions) ?? true),
                _ => code == sourceSlot.Itemstack.Collectible.Code.FirstCodePart() && CanAddPart(id)
            };
        }

        public bool CanTake(ItemSlot slot)
        {

            string type = codes[slot.Inventory.GetSlotId(slot)].Type;
            for (int i = 0; i < codes.Length; i++)
            {
                if (codes[i].Dependency == type && !slot.Inventory[i].Empty)
                {
                    if (Api is ICoreClientAPI capi)
                    {
                        capi.TriggerIngameError(this, null, Lang.Get($"{Core.ModId}:clock-error-notemptydependency"));
                    }
                    return false;
                }
            }
            return true;
        }
    }
}
