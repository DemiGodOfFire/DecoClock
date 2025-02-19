using System.Collections.Generic;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.MathTools;

namespace DecoClock
{
    public class WallClockBlock : VariableClockBlock
    {
        public override string Key => "wallclock";

        public override void CreateCreativeInventoryStacks(List<JsonItemStack> stacks)
        {
            if (this.LastCodePart() == "north")
            {
                base.CreateCreativeInventoryStacks(stacks);
            }
        }

        public override bool OnBlockInteractStart(IWorldAccessor world, IPlayer byPlayer, BlockSelection blockSel)
        {
            if (!world.Claims.TryAccess(byPlayer, blockSel.Position, EnumBlockAccessFlags.BuildOrBreak)) { return false; }
            if (world.BlockAccessor.GetBlockEntity(blockSel.Position) is BEWallClock be)
            {
                return be.OnInteract(byPlayer, blockSel);
            }
            return base.OnBlockInteractStart(world, byPlayer, blockSel);
        }

        public override bool DoPlaceBlock(IWorldAccessor world, IPlayer byPlayer, BlockSelection blockSel, ItemStack byItemStack)
        {
            bool val = base.DoPlaceBlock(world, byPlayer, blockSel, byItemStack);

            if (val)
            {
                if (world.BlockAccessor.GetBlockEntity(blockSel.Position) is BEWallClock be)
                {
                    float deg = GameMath.PI / 180;
                    switch (this.LastCodePart())
                    {
                        case "north": be.MeshAngle = 0 * deg; break;
                        case "west": be.MeshAngle = 90 * deg; break;
                        case "south": be.MeshAngle = 180 * deg; break;
                        case "east": be.MeshAngle = 270 * deg; break;
                    }

                    be.Material = byItemStack.Attributes.GetString("material", "oak");

                    if (world.Side == EnumAppSide.Client)
                    {
                        be.UpdateMesh();
                    }
                    be.MarkDirty(true);
                }
            }
            return val;
        }
    }
}

