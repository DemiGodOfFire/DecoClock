using System;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;

namespace DecoClock
{
    internal class WallClockBlock : ClockBlock
    {
        int code;
        float meshAngle;

        public override bool OnBlockInteractStart(IWorldAccessor world, IPlayer byPlayer, BlockSelection blockSel)
        {
            if (!world.Claims.TryAccess(byPlayer, blockSel.Position, EnumBlockAccessFlags.BuildOrBreak)) { return false; }
            if (world.BlockAccessor.GetBlockEntity(blockSel.Position) is BEWallClock be)
            {
                return be.OnInteract(byPlayer, blockSel);
            }
            return base.OnBlockInteractStart(world, byPlayer, blockSel);
        }

        public bool AbleToAttach(IWorldAccessor world, int rotation, BlockPos blockPos)
        {
            if (rotation == 0) return world.BlockAccessor.GetBlock(blockPos.NorthCopy()).SideSolid[BlockFacing.SOUTH.Index];
            if (rotation == 1) return world.BlockAccessor.GetBlock(blockPos.WestCopy()).SideSolid[BlockFacing.EAST.Index];
            if (rotation == 2) return world.BlockAccessor.GetBlock(blockPos.SouthCopy()).SideSolid[BlockFacing.NORTH.Index];
            if (rotation == 3) return world.BlockAccessor.GetBlock(blockPos.EastCopy()).SideSolid[BlockFacing.WEST.Index];
            return false;
        }

        public override bool DoPlaceBlock(IWorldAccessor world, IPlayer byPlayer, BlockSelection blockSel, ItemStack byItemStack)
        {
            bool val = base.DoPlaceBlock(world, byPlayer, blockSel, byItemStack);

            if (val)
            {
                if (world.BlockAccessor.GetBlockEntity(blockSel.Position) is BEWallClock be)
                {
                    be.MeshAngle = meshAngle;
                    if (world.Side == EnumAppSide.Client)
                    {
                        be.UpdateMesh();
                    }
                    be.MarkDirty(true);
                }
            }
            return val;
        }
        public override bool TryPlaceBlock(IWorldAccessor world, IPlayer byPlayer, ItemStack itemstack, BlockSelection blockSel, ref string failureCode)
        {
            float deg90 = GameMath.PIHALF;
            BlockFacing[] horVer = SuggestedHVOrientation(byPlayer, blockSel);
            code = (int)Math.Round(byPlayer.Entity.Pos.Yaw / deg90) - 1;
            meshAngle = code * deg90;
            bool ret = AbleToAttach(world, code, blockSel.Position);
            if (!ret)
            {
                failureCode = "requirehorizontalattachable";
                return false;
            }
            return base.TryPlaceBlock(world, byPlayer, itemstack, blockSel, ref failureCode);
        }

        public override void OnNeighbourBlockChange(IWorldAccessor world, BlockPos pos, BlockPos neibpos)
        {
            if (!AbleToAttach(world, code, pos))
                world.BlockAccessor.BreakBlock(pos, null);
        }

    }
}

