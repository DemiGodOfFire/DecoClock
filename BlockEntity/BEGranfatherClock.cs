using DecoClock.Render;
using System.Collections.Generic;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Datastructures;
using Vintagestory.API.MathTools;
using Vintagestory.API.Server;

namespace DecoClock
{
    internal class BEGrandfatherClock : BlockEntity, ITexPositionSource
    {
        ITexPositionSource textureSource = null!;
        ILoadedSound tickSound = null!;
        ILoadedSound openSound = null!;
        ILoadedSound closeSound = null!;
        ILoadedSound chimeSound = null!;

        MeshData? baseMesh;
        InventoryClock inventory = null!;
        GuiDialogGrandfatherClock dialogClock = null!;
        ClockHandRenderer rendererHand = null!;
        PendulumRenderer rendererPendulum = null!;
        GrandfatherClockDoorRenderer rendererDoor = null!;

        public Size2i AtlasSize => textureSource.AtlasSize;
        List<ClockItem> Parts { get { if (_parts.Count == 0) { AddParts(); } return _parts; } }

        readonly List<ClockItem> _parts = new();

        private bool isWork;
        public float meshAngle;
        public float timeWork;
        
        protected virtual void AddParts()
        {
            _parts.Add(new("clockwork"));
            _parts.Add(new("tickmarks"));
            _parts.Add(new("hourhand"));
            _parts.Add(new("minutehand"));
            _parts.Add(new("dialglass"));
            _parts.Add(new("clockparts"));
            _parts.Add(new("doorglass"));
        }


        public virtual TextureAtlasPosition? this[string textureCode]
        {
            //get
            //{
            //    if (inventory.TryGetPart("clockparts") != null)
            //    {
            //        if (textureCode == "thread") return textureSource["string"];
            //    }

            //    ItemStack stack = inventory.TryGetPart(textureCode);
            //    if (stack is not null)
            //    {
            //        var capi = Api as ICoreClientAPI;
            //        if (stack.Class == EnumItemClass.Item)
            //        {
            //            var tex = stack.Item.FirstTexture;
            //            AssetLocation texturePath = tex.Baked.BakedName;
            //            // return capi.ItemTextureAtlas[tex.Base];
            //            //   return capi.ItemTextureAtlas.GetPosition(stack.Item);
            //            return GetOrCreateTexPos(texturePath, capi);
            //        }
            //        else
            //        {
            //            // var tex = stack.Block.FirstTexture;
            //            return capi.BlockTextureAtlas.GetPosition(stack.Block);
            //        }
            //    }
            //    return textureSource[textureCode];
            //}
            get
            {
                if (inventory.TryGetPart("clockparts") != null)
                {
                    if (textureCode == "thread") { return textureSource["string"]; }
                }

                ItemStack? stack = inventory.TryGetPart(textureCode);
                if (stack is not null)
                {
                    var capi = (ICoreClientAPI)Api;
                    if (stack.Class == EnumItemClass.Item)
                    {
                        var tex = stack.Item.FirstTexture;
                        AssetLocation texturePath = tex.Baked.BakedName;
                        return GetOrCreateTexPos(texturePath, capi);
                    }
                    else
                    {
                        var tex = stack.Block.FirstTextureInventory;
                        AssetLocation texturePath = tex.Baked.BakedName;
                        return GetOrCreateTexPos(texturePath, capi);
                    }
                }
                return textureSource[textureCode];
            }
        }

        protected TextureAtlasPosition? GetOrCreateTexPos(AssetLocation texturePath, ICoreClientAPI capi)
        {
            TextureAtlasPosition? texpos = capi.BlockTextureAtlas[texturePath];

            if (texpos == null)
            {
                IAsset texAsset = capi.Assets.TryGet(texturePath.Clone().WithPathPrefixOnce("textures/").WithPathAppendixOnce(".png"));
                if (texAsset != null)
                {
                    capi.BlockTextureAtlas.GetOrInsertTexture(texturePath, out _, out texpos, () => texAsset.ToBitmap(capi));
                }
                else
                {
                    capi.World.Logger.Warning("For render in block " + Block.Code + ", item {0} defined texture {1}, no such texture found.", texturePath);
                }
            }
            return texpos;
        }

        private void InitInventory()
        {
            inventory ??= new InventoryClock(Parts.ToArray(), Pos, Api);
            inventory.SlotModified += OnSlotModifid;
        }

        public override void Initialize(ICoreAPI api)
        {
            base.Initialize(api);

            if (inventory != null)
            {
                inventory.LateInitialize(inventory.InventoryID, api);

            }
            else
            {
                InitInventory();
            }



            if (api is ICoreClientAPI capi)
            {

                tickSound ??= ((IClientWorldAccessor)api.World).LoadSound(new SoundParams
                {
                    Location = new AssetLocation("decoclock:sounds/ticking"),
                    ShouldLoop = false,
                    Position = Pos.ToVec3f().Add(0.5f, 1.5f, 0.5f),
                    DisposeOnFinish = false,
                    Volume = 0.5f,
                    Range = 16f,
                    SoundType = EnumSoundType.Ambient
                });

                chimeSound ??= ((IClientWorldAccessor)api.World).LoadSound(new SoundParams
                {
                    Location = new AssetLocation("decoclock:sounds/chimeend1"),
                    ShouldLoop = false,
                    Position = Pos.ToVec3f().Add(0.5f, 1.5f, 0.5f),
                    DisposeOnFinish = false,
                    Volume = 1f,
                    Range = 48f,
                    SoundType = EnumSoundType.Ambient
                });

                openSound ??= ((IClientWorldAccessor)api.World).LoadSound(new SoundParams
                {
                    Location = new AssetLocation("sounds/block/chestopen"),
                    ShouldLoop = false,
                    Position = Pos.ToVec3f().Add(0.5f, 0.25f, 0.5f),
                    DisposeOnFinish = false,
                    Volume = 1f,
                    Range = 16f
                });

                closeSound ??= ((IClientWorldAccessor)api.World).LoadSound(new SoundParams
                {
                    Location = new AssetLocation("sounds/block/chestclose"),
                    ShouldLoop = false,
                    Position = Pos.ToVec3f().Add(0.5f, 0.25f, 0.5f),
                    DisposeOnFinish = false,
                    Volume = 1f,
                    Range = 16f
                });
               
                capi.Event.RegisterRenderer(rendererHand =
                    new ClockHandRenderer(capi, Pos), EnumRenderStage.Opaque);
                capi.Event.RegisterRenderer(rendererDoor =
                    new GrandfatherClockDoorRenderer(capi, Pos), EnumRenderStage.Opaque);
                capi.Event.RegisterRenderer(rendererPendulum =
                   new PendulumRenderer(capi, Pos), EnumRenderStage.Opaque);
                textureSource = capi.Tesselator.GetTextureSource(Block);
                rendererHand.MinuteTick += () => { tickSound?.Start(); };
                rendererHand.HourTick += (_) => { chimeSound?.Start(); };
                //var angle = MeshAngle;
                //if (MeshAngle != 0)
                //{
                UpdateMesh();
                //}
            }
        }

        public bool OnInteract(IPlayer byPlayer, BlockSelection blockSel)
        {
            if (dialogClock == null && Api.Side == EnumAppSide.Client)
            {
                dialogClock = new GuiDialogGrandfatherClock(Core.ModId + ":fatherclock-title", inventory, Pos, (ICoreClientAPI)Api);
                dialogClock.OnOpened += () =>
                {
                    openSound?.Start();
                    rendererDoor.Open();
                };
                dialogClock.OnClosed += () =>
                {
                    closeSound?.Start();
                    rendererDoor.Close();
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
                    string path = $"decoclock:shapes/block/grandfatherclock/{item}.json";
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
                    string path = $"decoclock:shapes/block/grandfatherclock/{part}.json";
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
            string path = $"decoclock:shapes/block/grandfatherclock/{part}.json";
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

        public void UpdateMesh(ITesselatorAPI? tesselator = null)
        {
            tesselator ??= ((ICoreClientAPI)Api).Tesselator;
            MeshData mesh = GenBaseMesh(tesselator);
            baseMesh = mesh.Clone().Rotate(new Vec3f(0.5f, 0.5f, 0.5f), 0, meshAngle, 0);
            rendererHand.Update(GetItemMesh("hourhand"), GetItemMesh("minutehand"), meshAngle, IsWork());
            rendererDoor.Update(GetMesh("door"), meshAngle);
            rendererPendulum.Update(GetItemMesh("clockparts", "pendulum"),
                GetItemMesh("clockparts", "weight"), meshAngle);
            
        }

        private void OnSlotModifid(int slot)
        {
            MarkDirty(true);
        }

        #endregion

        private bool IsWork()
        {
            return (inventory.IsExist("clockwork") && inventory.IsExist("clockparts"));
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
            openSound?.Stop();
            openSound?.Dispose();
            closeSound?.Stop();
            closeSound?.Dispose();
            chimeSound?.Stop();
            chimeSound?.Dispose();
            dialogClock?.TryClose();
            rendererHand?.Dispose();
            rendererDoor?.Dispose();
            rendererPendulum?.Dispose();
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
            rendererDoor?.Dispose();
            rendererPendulum?.Dispose();
            tickSound?.Dispose();
            openSound?.Dispose();
            closeSound?.Dispose();
            chimeSound?.Dispose();
        }
    }
}
