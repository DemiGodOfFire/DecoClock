using System;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;

namespace DecoClock
{
    internal class WallClockBlock : ClockBlock
    {
        string code;
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

        public bool AbleToAttach(IWorldAccessor world, string rotation, BlockPos blockPos)
        {
            if (rotation == "north") return world.BlockAccessor.GetBlock(blockPos.SouthCopy()).SideSolid[BlockFacing.NORTH.Index];
            if (rotation == "east") return world.BlockAccessor.GetBlock(blockPos.WestCopy()).SideSolid[BlockFacing.EAST.Index];
            if (rotation == "south") return world.BlockAccessor.GetBlock(blockPos.NorthCopy()).SideSolid[BlockFacing.SOUTH.Index];
            if (rotation == "west") return world.BlockAccessor.GetBlock(blockPos.EastCopy()).SideSolid[BlockFacing.WEST.Index];
            return false;
        }

        public override bool DoPlaceBlock(IWorldAccessor world, IPlayer byPlayer, BlockSelection blockSel, ItemStack byItemStack)
        {
            bool val = base.DoPlaceBlock(world, byPlayer, blockSel, byItemStack);

            if (val)
            {
                if (world.BlockAccessor.GetBlockEntity(blockSel.Position) is BEWallClock be)
                {
                    float deg90 = GameMath.PIHALF;
                    be.MeshAngle = ((int)Math.Round(byPlayer.Entity.Pos.Yaw / deg90) - 1) * deg90; ;
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
            BlockFacing[] horVer = SuggestedHVOrientation(byPlayer, blockSel);
            code = horVer[0].Opposite.Code;
            meshAngle =  horVer[0].Opposite.Index *(float) Math.PI / 2f;
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

