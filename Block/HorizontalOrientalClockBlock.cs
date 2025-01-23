using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.MathTools;

namespace DecoClock
{
    public abstract class HorizontalOrientalClockBlock : VariableClockBlock
    {
        public override string GetHeldItemName(ItemStack itemStack)
        {
            string material = itemStack.Attributes.GetString("material", "oak");
            return Lang.Get($"{Core.ModId}:block-{Key}-{material}-north");
        }

        public override string GetPlacedBlockName(IWorldAccessor world, BlockPos pos)
        {
            if (world.BlockAccessor.GetBlockEntity(pos) is not BEClock beclock) return base.GetPlacedBlockName(world, pos);

            return Lang.Get($"{Core.ModId}:block-{Key}-{beclock.Material}-north");
        }

    }
}
