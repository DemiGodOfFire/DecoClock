using Cairo;
using System.Collections.Generic;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Datastructures;
using Vintagestory.API.MathTools;
using Vintagestory.API.Server;

namespace DecoClock
{
    internal class BEBigClock :BEClock
    {
        GuiDialogBigClock dialogClock;

        public override string PathBlock => "decoclock:shapes/block/clock/";

        public override void AddParts()
        {
            _parts.Add(new("hourhand"));
            _parts.Add(new("minutehand"));
            _parts.Add(new("disguise"));
            _parts.Add(new("tickmarks"));
        }

        public override void Initialize(ICoreAPI api)
        {
            base.Initialize(api);

            if (Inventory != null)
            {
                Inventory.LateInitialize(inventory.InventoryID, api);

            }
            else
            {
                InitInventory();
            }

            if (api is ICoreClientAPI capi)
            {
                LoadSound(capi);


                capi.Event.RegisterRenderer(rendererHand =
                    new ClockRenderer(capi, Pos), EnumRenderStage.Opaque);

                TextureSource = capi.Tesselator.GetTextureSource(Block);
                rendererHand.MinuteTick += () => { tickSound?.Start(); };
                //rendererHand.HourTick += (_) => { chimeSound?.Start(); };

                UpdateMesh();
            }
        }

        public override bool OnInteract(IPlayer byPlayer, BlockSelection blockSel)
        {
       
            if (dialogClock == null && Api.Side == EnumAppSide.Client)
            {
                dialogClock = new GuiDialogBigClock(Inventory, Pos, (ICoreClientAPI)Api);
                dialogClock.OnOpened += () =>
                {
                    //openSound?.Start();
                };
                dialogClock.OnClosed += () =>
                {
                    //closeSound?.Start();
                };
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

        //public MeshData GenMesh(string type, ITesselatorAPI? tesselator = null)
        //{
        //    if (tesselator == null)
        //    {
        //        tesselator = ((ICoreClientAPI)Api).Tesselator;
        //    }
        //    AssetLocation assetLocation = Block.Shape.Base.WithPathPrefixOnce("shapes/");
        //    assetLocation.Path = assetLocation.Path.Replace("/complete", $"/{type}.json");
        //    Shape shape = Api.Assets.TryGet(assetLocation).ToObject<Shape>();
        //    tesselator.TesselateShape("BeClock", shape, out MeshData mesh, this);

        //    return mesh;
        //}

        public MeshData? GetItemMesh(string item)
        {
            if (inventory != null)
            {
                var inv = inventory.TryGetPart(item);
                if (inv != null)
                {
                    ITesselatorAPI tesselatorHand = ((ICoreClientAPI)Api).Tesselator;
                    string path = this.path+$"{item}.json";
                    Shape shape = Api.Assets.TryGet(path).ToObject<Shape>();
                    tesselatorHand.TesselateShape("BeClock", shape, out MeshData mesh, this);
                    return mesh;
                }
            }
            return null;
        }

        public MeshData? GetItemMesh(string item, string part)
        {
            if (inventory != null)
            {
                var inv = inventory.TryGetPart(item);
                if (inv != null)
                {
                    ITesselatorAPI tesselatorHand = ((ICoreClientAPI)Api).Tesselator;
                    string path = this.path + $"{part}.json";
                    Shape shape = Api.Assets.TryGet(path).ToObject<Shape>();
                    tesselatorHand.TesselateShape("BeClock", shape, out MeshData mesh, this);
                    return mesh;
                }
            }
            return null;
        }


        public MeshData? GetMesh(string part)
        {
            ITesselatorAPI tesselatorHand = ((ICoreClientAPI)Api).Tesselator;
            string path = this.path + $"{part}.json";
            Shape shape = Api.Assets.TryGet(path).ToObject<Shape>();
            tesselatorHand.TesselateShape("BeClock", shape, out MeshData mesh, this);
            return mesh;
        }

        public override bool OnTesselation(ITerrainMeshPool mesher, ITesselatorAPI tesselator)
        {
            if (baseMesh != null)
            {
                mesher.AddMeshData(baseMesh);
            }
            return true;
        }

        public virtual void UpdateMesh(ITesselatorAPI? tesselator = null)
        {
            tesselator ??= ((ICoreClientAPI)Api).Tesselator;
            MeshData mesh = GenBaseMesh(tesselator);
            baseMesh = mesh.Clone().Rotate(new Vec3f(0.5f, 0.5f, 0.5f), 0, meshAngle, 0);
            //rendererHand.Update(GetItemMesh("hourhand"), 0.11f, GetItemMesh("minutehand"), 0.101f, 1f, meshAngle);
            rendererHand.Update(GetItemMesh("hourhand"), 0.55f, GetItemMesh("minutehand"), 0.62f, 0f, meshAngle);
        }

        private void OnSlotModifid(int slot)
        {
            MarkDirty(true);
        }

        #endregion

        public virtual void LoadSound(ICoreClientAPI capi)
        {
            tickSound ??= ((IClientWorldAccessor)capi.World).LoadSound(new SoundParams
            {
                Location = new AssetLocation("decoclock:sounds/ticking"),
                ShouldLoop = false,
                Position = Pos.ToVec3f().Add(0.5f, 1.5f, 0.5f),
                DisposeOnFinish = false,
                Volume = 0.5f,
                Range = 16f,
                SoundType = EnumSoundType.Ambient
            });

        }

       

        public override void RegisterRenderer()
        {
            throw new System.NotImplementedException();
        }

        public void DropContents()
        {
            inventory?.DropAll(Pos.ToVec3d().Add(0.5, 0.5, 0.5));
        }

        #region Events


        public override void OnReceivedClientPacket(IPlayer player, int packetid, byte[] data)
        {
            if (packetid < 1000)
            {
                inventory.InvNetworkUtil.HandleClientPacket(player, packetid, data);
                // Tell server to save this chunk to disk again
                Api.World.BlockAccessor.GetChunkAtBlockPos(Pos.X, Pos.Y, Pos.Z).MarkModified();
                MarkDirty(true);
                return;
            }

            if (packetid == 1000)
            {
                IPlayerInventoryManager inventoryManager = player.InventoryManager;
                if (inventoryManager == null)
                {
                    return;
                }
                inventoryManager.OpenInventory(inventory);
            }

            if (packetid == 1001)
            {
                IPlayerInventoryManager inventoryManager = player.InventoryManager;
                if (inventoryManager != null)
                {
                    inventoryManager.CloseInventory(inventory);
                }
            }

            base.OnReceivedClientPacket(player, packetid, data);

        }



        public override void FromTreeAttributes(ITreeAttribute tree, IWorldAccessor worldForResolving)
        {
            base.FromTreeAttributes(tree, worldForResolving);
            meshAngle = tree.GetFloat("meshAngle", meshAngle);
            InitInventory();
            inventory.FromTreeAttributes(tree);
            if (Api is ICoreClientAPI)
            {
                UpdateMesh();
                MarkDirty(true);
            }
        }

        public override void ToTreeAttributes(ITreeAttribute tree)
        {
            //  Api.World.Logger.Warning("To tree attributes");
            base.ToTreeAttributes(tree);
            tree.SetFloat("meshAngle", meshAngle);
            inventory?.ToTreeAttributes(tree);
        }

        public override void OnBlockRemoved()
        {
            base.OnBlockRemoved();
            tickSound?.Stop();
            tickSound?.Dispose();
            //openSound?.Stop();
            //openSound?.Dispose();
            //closeSound?.Stop();
            //closeSound?.Dispose();
            //chimeSound?.Stop();
            //chimeSound?.Dispose();
            dialogClock?.TryClose();
            rendererHand?.Dispose();
        }

        public override void OnBlockBroken(IPlayer? byPlayer = null)
        {
            base.OnBlockBroken(byPlayer);
            if (Api.World is IServerWorldAccessor)
            {
                inventory?.DropAll(Pos.ToVec3d().Add(0.5, 0.5, 0.5));
            }
        }

        #endregion

        public override void OnBlockUnloaded()
        {
            base.OnBlockUnloaded();
            rendererHand?.Dispose();
            tickSound?.Dispose();
            //openSound?.Dispose();
            //closeSound?.Dispose();
            //chimeSound?.Dispose();
        }

       
    }
}
