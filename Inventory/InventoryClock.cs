using Vintagestory.API.Common;
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

    public class ClockItem
    {
        public string Type { get; }
        public string Dependency { get; }
        public AssetLocation[] Codes { get; }

        public ClockItem(string type, AssetLocation[] codes, string dependency = null)
        {
            Type = type;
            Codes = codes;
            Dependency = dependency;
        }
    }

    public class InventoryClock : InventoryGeneric
    {
        private ClockItem[] codes;
        public InventoryClock(ClockItem[] codes, BlockPos pos, ICoreAPI api) : base(codes.Length, "clockInvDeco", pos + "", api)
        {
            this.codes = codes;
        }

        /// <summary>
        /// Show content if exist
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public ItemStack TryGetPart(string type)
        {
            for (int i = 0; i < codes.Length; i++)
            {
                if (codes[i].Type == type)
                    return slots[i].Itemstack.Clone();

            }
            return null;
        }

        /// <summary>
        /// Take content and remove it if exist
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public ItemStack TryTakePart(string type)
        {
            for (int i = 0; i < codes.Length; i++)
            {
                if (codes[i].Type == type)
                {
                    if (slots[i].Empty)
                        return null;
                    return slots[i].TakeOutWhole();
                }
            }
            return null;
        }

        /// <summary>
        /// Add part if suitable and return original content if exist 
        /// </summary>
        /// <param name="item"></param>
        /// <param name="content"></param>
        /// <returns></returns>
        public bool TryAddPart(ItemStack item, out ItemStack content)
        {
            for (int i = 0; i < codes.Length; i++)
            {
                foreach(var code in codes[i].Codes)
                {
                    if (code == item.Collectible.Code)
                    {
                        content = slots[i].Itemstack.Clone();
                        slots[i].Itemstack = item.Clone();
                        slots[i].Itemstack.StackSize = 1;
                        return true;
                    }
                }
            }

            content = null;
            return false;
        }
    }
}
