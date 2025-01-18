using System;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;
using Vintagestory.API.Util;

namespace DecoClock
{

    public class ClockBlock : Block
    {

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
                    float yRotRad = ((int)Math.Round(byPlayer.Entity.Pos.Yaw / GameMath.PIHALF) - 2) * GameMath.PIHALF;
                    if (yRotRad <= -(float)(2 * Math.PI))
                        throw new Exception("Argh!");                                                                               //Check for less than -180 degrees
                    be.MeshAngle = yRotRad < 0 ? yRotRad + (float)(2 * Math.PI) : yRotRad;

                    if (world.Side == EnumAppSide.Client)
                    {
                        be.UpdateMesh();
                    }

                    be.MarkDirty(true);
                }
            }
            return val;
        }

        public override WorldInteraction[] GetPlacedBlockInteractionHelp(IWorldAccessor world, BlockSelection selection, IPlayer forPlayer)
        {
            return new WorldInteraction[]
            {
                new() {
                    ActionLangCode = "blockhelp-chest-open",
                    MouseButton = EnumMouseButton.Right
                }
            }.Append(base.GetPlacedBlockInteractionHelp(world, selection, forPlayer));
        }
    }
}
