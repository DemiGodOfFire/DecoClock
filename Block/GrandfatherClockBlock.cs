using System;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;

namespace DecoClock
{
    public class GrandfatherClockBlock : VariableClockBlock
    {
       

        public override string Key => "grandfatherclock";

        public override bool OnBlockInteractStart(IWorldAccessor world, IPlayer byPlayer, BlockSelection blockSel)
        {
            if (!world.Claims.TryAccess(byPlayer, blockSel.Position, EnumBlockAccessFlags.BuildOrBreak)) { return false; }
            if (world.BlockAccessor.GetBlockEntity(blockSel.Position) is BEGrandfatherClock be)
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
                if (world.BlockAccessor.GetBlockEntity(blockSel.Position) is BEGrandfatherClock be)
                {
                    BlockPos targetPos = blockSel.DidOffset ? blockSel.Position.AddCopy(blockSel.Face.Opposite) : blockSel.Position;
                    double dx = byPlayer.Entity.Pos.X - (targetPos.X + blockSel.HitPosition.X);
                    double dz = (float)byPlayer.Entity.Pos.Z - (targetPos.Z + blockSel.HitPosition.Z);
                    float angleHor = (float)Math.Atan2(dx, dz);
                    float deg22dot5rad = GameMath.PIHALF / 4;
                    float roundRad = (int)Math.Round(angleHor / deg22dot5rad) * deg22dot5rad;
                    be.MeshAngle = roundRad;
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

        public override Cuboidf[] GetSelectionBoxes(IBlockAccessor blockAccessor, BlockPos pos)
        {
            if (blockAccessor.GetBlockEntity(pos) is BEGrandfatherClock be)
            {
                Cuboidf[] selectionBoxes = new Cuboidf[1];
                float angleDeg = be.MeshAngle * GameMath.RAD2DEG;
                float roundedAngle = (float)Math.Round(angleDeg / 90f) * 90f;
                if (roundedAngle < 0)
                {
                    roundedAngle += 360;
                }
                switch (roundedAngle)
                {
                    case 0:
                        selectionBoxes[0] = SelectionBoxes[0];
                        break;

                    case 90:
                        selectionBoxes[0] = SelectionBoxes[1];
                        break;

                    case 180:
                        selectionBoxes[0] = SelectionBoxes[2];
                        break;

                    case 270:
                        selectionBoxes[0] = SelectionBoxes[3];
                        break;

                    default:
                        selectionBoxes[0] = SelectionBoxes[0];
                        api.Logger.Error("Achtung! Angle not correct!");
                        break;
                }

                return selectionBoxes;
            }

            return base.GetSelectionBoxes(blockAccessor, pos);
        }

        public override Cuboidf[] GetCollisionBoxes(IBlockAccessor blockAccessor, BlockPos pos)
        {
            return GetSelectionBoxes(blockAccessor, pos);
        }
    }
}
