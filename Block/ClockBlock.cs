using System;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;

namespace DecoClock
{

    internal class ClockBlock : Block
    {
        //public override ItemStack[] GetDrops(IWorldAccessor world, BlockPos pos, IPlayer byPlayer, float dropQuantityMultiplier = 1)
        //{
        //    return new ItemStack[] { OnPickBlock(world, pos) };
        //}

        public override bool OnBlockInteractStart(IWorldAccessor world, IPlayer byPlayer, BlockSelection blockSel)
        {
            if (!world.Claims.TryAccess(byPlayer, blockSel.Position, EnumBlockAccessFlags.BuildOrBreak)) { return false; }
            if (world.BlockAccessor.GetBlockEntity(blockSel.Position) is BEBigClock be)
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
                if (world.BlockAccessor.GetBlockEntity(blockSel.Position) is BEBigClock be)
                {
                    float deg90 = GameMath.PIHALF;
                    be.MeshAngle = ((int)Math.Round(byPlayer.Entity.Pos.Yaw / deg90) - 1) * deg90;
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
