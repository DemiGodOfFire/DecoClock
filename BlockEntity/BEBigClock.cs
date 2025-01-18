using System;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Datastructures;
using Vintagestory.API.MathTools;
using Vintagestory.GameContent;

namespace DecoClock
{
    internal class BEBigClock : BEClock
    {
        GuiDialogBigClock? dialogClock;
        BigClockRenderer? rendererClock;
        public override string PathBlock => "decoclock:shapes/block/clock/";
        public int Radius { get; set; } = 1;
        public int ShiftZ { get; set; } = 0;

        public override void AddParts()
        {
            _parts.Add(new("hourhand"));
            _parts.Add(new("minutehand"));
            _parts.Add(new("disguise"));
            _parts.Add(new("tickmarks"));
        }

        public override bool OnInteract(IPlayer byPlayer, BlockSelection blockSel)
        {

            if (dialogClock == null && Api.Side == EnumAppSide.Client)
            {
                dialogClock = new GuiDialogBigClock(Core.ModId + ":bigclock-title", Inventory, Pos, (ICoreClientAPI)Api);
                //dialogClock.OnOpened += () =>
                //{
                //    //openSound?.Start();
                //};
                //dialogClock.OnClosed += () =>
                //{
                //    //closeSound?.Start();
                //};
            }

            if (Api.Side == EnumAppSide.Client)
            {
                dialogClock?.TryOpen();
            }
            return false;
        }

        #region meshing

        public MeshData GenBaseMesh(ITesselatorAPI tesselator)
        {
            AssetLocation assetLocation = Block.Shape.Base.WithPathPrefixOnce("shapes/").WithPathAppendixOnce(".json");
            Shape shape = Api.Assets.TryGet(assetLocation).ToObject<Shape>();
            tesselator.TesselateShape("BeClock", shape, out MeshData mesh, this);
            return mesh;
        }

        //public MeshData? GetItemMesh(string item, int type)
        //{
        //    if (Inventory != null)
        //    {
        //        var inv = Inventory.TryGetPart(item);
        //        if (inv != null)
        //        {
        //            ITesselatorAPI tesselator = ((ICoreClientAPI)Api).Tesselator;
        //            string path = this.PathBlock + $"{item}-{type}.json";
        //            Shape shape = Api.Assets.TryGet(path).ToObject<Shape>();
        //            tesselator.TesselateShape("BeClock", shape, out MeshData mesh, this);
        //            return mesh;
        //        }
        //    }
        //    return null;
        //}

        public override void UpdateMesh(ITesselatorAPI? tesselator = null)
        {
            tesselator ??= ((ICoreClientAPI)Api).Tesselator;
            MeshData mesh;
            if (Inventory.IsExist("disguise"))
            {
                ItemStack itemStack = Inventory.TryGetPart("disguise")!;
                if (itemStack.Block is BlockChisel)
                {
                    var bem = new BlockEntityMicroBlock();
                    bem.FromTreeAttributes(itemStack.Attributes, Api.World);
                    mesh = BlockEntityMicroBlock.CreateMesh((ICoreClientAPI)Api, bem.VoxelCuboids, bem.BlockIds, null, Pos);
                }
                else
                {
                    try
                    {
                        tesselator.TesselateBlock(itemStack.Block, out mesh);
                    }
                    catch
                    {
                        mesh = GenBaseMesh(tesselator);
                    }
                }
            }
            else
            {
                mesh = GenBaseMesh(tesselator);
            }
            BaseMesh = mesh.Clone().Rotate(new Vec3f(0.5f, 0.5f, 0.5f), 0, MeshAngle, 0);
            rendererClock?.Update(GetItemMesh("hourhand"), 0.6f,
                GetItemMesh("minutehand"), 0f, 0.75f,
                GetItemMesh("tickmarks", TypeDial), 0f, 0.5f,
                GetMesh("tribe"),
                Radius,
                ShiftZ,
                MeshAngle);
        }

        #endregion

        public override void GetVariablesFromTreeAttributes(ITreeAttribute tree)
        {
            base.GetVariablesFromTreeAttributes(tree);
            Radius = tree.GetInt("radius", Radius);
            ShiftZ = tree.GetInt("shift", ShiftZ);

        }

        public override void ToTreeAttributes(ITreeAttribute tree)
        {
            base.ToTreeAttributes(tree);
            tree.SetInt("radius", Radius);
            tree.SetInt("shift", ShiftZ);
        }

        public override void ClientPackets(IPlayer player, int packetid, byte[] data)
        {
            base.ClientPackets(player, packetid, data);
            if (packetid == Constants.Radius)
            {
                Radius = BitConverter.ToInt32(data, 0);
                MarkDirty(true);
            }
            if (packetid == Constants.ShiftZ)
            {
                ShiftZ = BitConverter.ToInt32(data, 0);
                MarkDirty(true);
            }
        }

        public override void LoadSound(ICoreClientAPI capi)
        {
            TickSound ??= ((IClientWorldAccessor)capi.World).LoadSound(new SoundParams
            {
                Location = new AssetLocation("decoclock:sounds/ticking"),
                ShouldLoop = false,
                Position = Pos.ToVec3f().Add(0.5f, 1.5f, 0.5f),
                DisposeOnFinish = false,
                Volume = 1f,
                Range = 16f,
                SoundType = EnumSoundType.Ambient
            });
            
        }

        public override void RegisterRenderer(ICoreClientAPI capi)
        {
            capi.Event.RegisterRenderer(rendererClock = new BigClockRenderer(capi, Pos), EnumRenderStage.Opaque);
            rendererClock.MinuteTick += () => { if (!MuteSounds) TickSound?.Start(); };
        }

        public override void OnBlockRemoved()
        {
            base.OnBlockRemoved();
            dialogClock?.TryClose();
            rendererClock?.Dispose();
        }

        public override void OnBlockUnloaded()
        {
            base.OnBlockUnloaded();
            rendererClock?.Dispose();
        }
    }
}
