using System.Collections.Generic;
using Vintagestory.API.Common;

namespace DecoClock

{
    internal class ClockManager : ModSystem
    {
        public Dictionary<string, List<AssetLocation>> Parts { get; } = new();

        public override void AssetsLoaded(ICoreAPI api)
        {
            Parts.Add("hourhand", new());
            Parts.Add("clockwork", new());
            Parts.Add("dialglass", new());
            Parts.Add("tickmarks", new());
            Parts.Add("minutehand", new());
            Parts.Add("clockparts", new());

            foreach (var item in api.World.Items)
            {
                if (item?.Code?.Domain == Mod.Info.ModID)
                {
                    foreach (var key in Parts.Keys)
                    {
                        if (item.FirstCodePart() == key)
                            Parts[key].Add(item.Code);
                    }
                }
            }
            foreach (var block in api.World.Blocks)
            {
                if (block?.Code?.Domain == Mod.Info.ModID)
                {
                    foreach (var key in Parts.Keys)
                    {
                        if (block.FirstCodePart() == key)
                            Parts[key].Add(block.Code);
                    }
                }
            }
        }
    }
}
