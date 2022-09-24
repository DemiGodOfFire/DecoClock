using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using Vintagestory.API.Common;
using Vintagestory.API.Datastructures;
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
        public string Dependency { get; private set; }
        public AssetLocation[] Codes { get; set; }

        public ClockItem() { }
        public ClockItem(string type, string dependency = null, AssetLocation[] codes = null)
        {
            Type = type;
            Codes = codes;
            Dependency = dependency;
        }
    }

    public class InventoryClock : InventoryGeneric
    {
        private ClockItem[] codes;
        public InventoryClock(ClockItem[] codes, BlockPos pos, ICoreAPI api) : base(codes.Length, "DecoClock-ClockInv", pos + "", api)
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
                    return slots[i].Itemstack?.Clone();

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
            if (item != null)
            {
                for (int i = 0; i < codes.Length; i++)
                {
                    if (codes[i].Codes != null)
                    {
                        foreach (var code in codes[i].Codes)
                        {
                            if (code == item.Collectible.Code)
                            {
                                content = slots[i].Itemstack?.Clone();
                                slots[i].Itemstack = item.Clone();
                                slots[i].Itemstack.StackSize = 1;
                                return true;
                            }
                        }
                    }
                }
            }
            content = null;
            return false;
        }

        //public override void ToTreeAttributes(ITreeAttribute invtree)
        //{
        //    base.ToTreeAttributes(invtree);
        //    var formatter = new BinaryFormatter();
        //    using (MemoryStream stream = new MemoryStream())
        //    {
        //        formatter.Serialize(stream, codes);
        //        invtree.SetBytes("clockitems", stream.ToArray());
        //    }
        //}

        //public override void FromTreeAttributes(ITreeAttribute invtree)
        //{
        //    base.FromTreeAttributes(invtree);
        //    byte[] bytes = invtree.GetBytes("clockitems", null);

        //    if (codes == null && bytes != null)
        //    {
        //        var formatter = new BinaryFormatter();
        //        using (MemoryStream stream = new MemoryStream(bytes))
        //        {
        //            codes = (ClockItem[])formatter.Deserialize(stream);
        //        }
        //    }
        //}
    }
}
