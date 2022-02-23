using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;
using Vintagestory.GameContent;

namespace decoclock.src
{


    internal class GrandfatherClock :Block

    {
        Cuboidf[] colBox;

        public override ItemStack OnPickBlock(IWorldAccessor world, BlockPos pos)
        {
            AssetLocation blockCode = CodeWithVariants(new Dictionary<string, string>() {
                    { "horizontalorientation", "north" }
                });

            Block block = world.BlockAccessor.GetBlock(blockCode);

            return new ItemStack(block);
        }

        public override ItemStack[] GetDrops(IWorldAccessor world, BlockPos pos, IPlayer byPlayer, float dropQuantityMultiplier = 1)
        {
            return new ItemStack[] { OnPickBlock(world, pos) };
        }

        //public bool AbleToAttach(IWorldAccessor world, string facingCode, BlockPos blockPos)
        //{

        //    if (facingCode == "north") return world.BlockAccessor.GetBlock(blockPos.NorthCopy()).SideSolid[BlockFacing.SOUTH.Index];
        //    if (facingCode == "east") return world.BlockAccessor.GetBlock(blockPos.EastCopy()).SideSolid[BlockFacing.WEST.Index];
        //    if (facingCode == "south") return world.BlockAccessor.GetBlock(blockPos.SouthCopy()).SideSolid[BlockFacing.NORTH.Index];
        //    if (facingCode == "west") return world.BlockAccessor.GetBlock(blockPos.WestCopy()).SideSolid[BlockFacing.EAST.Index];

        //    return false;
        //}
        //public override bool TryPlaceBlock(IWorldAccessor world, IPlayer byPlayer, ItemStack itemstack, BlockSelection blockSel, ref string failureCode)
        //{
        //    BlockFacing[] horVer = Block.SuggestedHVOrientation(byPlayer, blockSel);

        //    bool ret = AbleToAttach(world, horVer[0].Code, blockSel.Position);
        //    if (!ret)
        //    {
        //        failureCode = "requirehorizontalattachable";
        //        return false;
        //    }
        //    return base.TryPlaceBlock(world, byPlayer, itemstack, blockSel, ref failureCode);
        //}

        //public override void OnNeighbourBlockChange(IWorldAccessor world, BlockPos pos, BlockPos neibpos)
        //{
        //    if (!AbleToAttach(world, Variant["horizontalorientation"], pos))
        //        world.BlockAccessor.BreakBlock(pos, null);
        //}

        //public override bool DoPlaceBlock(IWorldAccessor world, IPlayer byPlayer, BlockSelection blockSel, ItemStack byItemStack)
        //{
        //    bool val = base.DoPlaceBlock(world, byPlayer, blockSel, byItemStack);

        //    if (val)
        //    {
        //        BEClock bect = world.BlockAccessor.GetBlockEntity(blockSel.Position) as BEClock;
        //        if (bect != null)
        //        {
        //            BlockPos targetPos = blockSel.DidOffset ? blockSel.Position.AddCopy(blockSel.Face.Opposite) : blockSel.Position;
        //            double dx = byPlayer.Entity.Pos.X - (targetPos.X + blockSel.HitPosition.X);
        //            double dz = byPlayer.Entity.Pos.Z - (targetPos.Z + blockSel.HitPosition.Z);
        //            float angleHor = (float)Math.Atan2(dx, dz);

        //            float deg22dot5rad = GameMath.PIHALF / 4;
        //            float roundRad = ((int)Math.Round(angleHor / deg22dot5rad)) * deg22dot5rad;
        //            bect.MeshAngle = roundRad;
        //        }
        //    }

        //    return val;
        //}

        public override bool DoPlaceBlock(IWorldAccessor world, IPlayer byPlayer, BlockSelection blockSel, ItemStack byItemStack)
        {
            bool val = base.DoPlaceBlock(world, byPlayer, blockSel, byItemStack);

            if (val)
            {
                BEGrandfatherClock bect = world.BlockAccessor.GetBlockEntity(blockSel.Position) as BEGrandfatherClock;
                if (bect != null)
                {
                    BlockPos targetPos = blockSel.DidOffset ? blockSel.Position.AddCopy(blockSel.Face.Opposite) : blockSel.Position;
                    double dx = byPlayer.Entity.Pos.X - (targetPos.X + blockSel.HitPosition.X);
                    double dz = (float)byPlayer.Entity.Pos.Z - (targetPos.Z + blockSel.HitPosition.Z);
                    float angleHor = (float)Math.Atan2(dx, dz);
                    float deg22dot5rad = GameMath.PIHALF / 4;
                    float roundRad = ((int)Math.Round(angleHor / deg22dot5rad)) * deg22dot5rad;
                    bect.MeshAngle = roundRad;
                }
            }

            return val;
        }

    }
}
