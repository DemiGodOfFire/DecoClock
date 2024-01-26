using System;
using System.Collections.Generic;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Datastructures;
using Vintagestory.API.MathTools;
using Vintagestory.API.Server;

namespace DecoClock
{
    public abstract class BEClock : BlockEntity, ITexPositionSource
    {
        public ITexPositionSource TextureSource { get; set; } = null!;
        public ILoadedSound? TickSound { get; set; }
        //ILoadedSound chimeSound = null!;

        public MeshData? BaseMesh { get; set; }
        public InventoryClock Inventory { get; set; } = null!;

        public Size2i AtlasSize => TextureSource.AtlasSize;
        List<ClockItem> Parts { get { if (_parts.Count == 0) { AddParts(); } return _parts; } }

        public readonly List<ClockItem> _parts = new();

        public int TypeDial { get; set; } = 1;
        public float MeshAngle { get; set; }
        public abstract string PathBlock { get; }
        public bool MuteSounds { get; set; } = false;

        public abstract void AddParts();


        public virtual TextureAtlasPosition? this[string textureCode]
        {
            get
            {               
                ItemStack? stack = Inventory.TryGetPart(textureCode);

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
                return TextureSource[textureCode];
            }
        }

        protected virtual TextureAtlasPosition? GetOrCreateTexPos(AssetLocation texturePath, ICoreClientAPI capi)
        {
            TextureAtlasPosition? texpos = capi.BlockTextureAtlas[texturePath];

            if (texpos == null)
            {
                IAsset texAsset = capi.Assets.TryGet(texturePath.Clone().
                    WithPathPrefixOnce("textures/").
                    WithPathAppendixOnce(".png"));
                if (texAsset != null)
                {
                    capi.BlockTextureAtlas.GetOrInsertTexture(texturePath, out _, out texpos,

                        () => texAsset.ToBitmap(capi));
                }
                else
                {
                    capi.World.Logger.Warning("For render in block " + Block.Code +
                        ", item {0} defined texture {1}, no such texture found.", texturePath);
                }
            }
            return texpos;
        }

        private void InitInventory()
        {
            Inventory ??= new InventoryClock(Parts.ToArray(), Pos, Api);
            Inventory.SlotModified += OnSlotModifid;
        }

        public override void Initialize(ICoreAPI api)
        {
            base.Initialize(api);

            if (Inventory != null)
            {
                Inventory.LateInitialize(Inventory.InventoryID, api);

            }
            else
            {
                InitInventory();
            }

            if (api is ICoreClientAPI capi)
            {
                LoadSound(capi);
                RegisterRenderer(capi);
                TextureSource = capi.Tesselator.GetTextureSource(Block);
                //rendererHand.HourTick += (_) => { chimeSound?.Start(); };
                UpdateMesh();
            }
        }

        public abstract bool OnInteract(IPlayer byPlayer, BlockSelection blockSel);

        #region meshing

        public virtual MeshData GenBaseMesh(ITesselatorAPI tesselator)
        {
            AssetLocation assetLocation = Block.Shape.Base.WithPathPrefixOnce("shapes/").WithPathAppendixOnce(".json");
            Shape shape = Api.Assets.TryGet(assetLocation).ToObject<Shape>();
            tesselator.TesselateShape("BeClock", shape, out MeshData mesh, this);
            return mesh;
        }

        public MeshData? GetItemMesh(string item)
        {
            if (Inventory != null)
            {
                var inv = Inventory.TryGetPart(item);
                if (inv != null)
                {
                    ITesselatorAPI tesselator = ((ICoreClientAPI)Api).Tesselator;
                    string path = this.PathBlock + $"{item}.json";
                    Shape shape = Api.Assets.TryGet(path).ToObject<Shape>();
                    tesselator.TesselateShape("BeClock", shape, out MeshData mesh, this);
                    return mesh;
                }
            }
            return null;
        }

        public MeshData? GetItemMesh(string item, int type)
        {
            if (Inventory != null)
            {
                var inv = Inventory.TryGetPart(item);
                if (inv != null)
                {
                    ITesselatorAPI tesselator = ((ICoreClientAPI)Api).Tesselator;
                    string path = this.PathBlock + $"{item}-{type}.json";
                    Shape shape = Api.Assets.TryGet(path).ToObject<Shape>();
                    tesselator.TesselateShape("BeClock", shape, out MeshData mesh, this);
                    return mesh;
                }
            }
            return null;
        }


        public MeshData? GetPartItemMesh(string item, string part)
        {
            if (Inventory != null)
            {
                var inv = Inventory.TryGetPart(item);
                if (inv != null)
                {
                    ITesselatorAPI tesselator = ((ICoreClientAPI)Api).Tesselator;
                    string path = this.PathBlock + $"{part}.json";
                    Shape shape = Api.Assets.TryGet(path).ToObject<Shape>();
                    tesselator.TesselateShape("BeClock", shape, out MeshData mesh, this);
                    return mesh;
                }
            }
            return null;
        }


        public MeshData? GetMesh(string part)
        {
            ITesselatorAPI tesselator = ((ICoreClientAPI)Api).Tesselator;
            string path = this.PathBlock + $"{part}.json";
            Shape? shape = Api.Assets.TryGet(path)?.ToObject<Shape>();
            //Api.World.Logger.Warning($"{Core.ModId} {part}: " + path);
            if ( shape == null)
                return null;
            tesselator.TesselateShape("BeClock", shape, out MeshData mesh, this);
            return mesh;
        }

        public override bool OnTesselation(ITerrainMeshPool mesher, ITesselatorAPI tesselator)
        {
            if (BaseMesh != null)
            {
                mesher.AddMeshData(BaseMesh);
            }
            return true;
        }

        public virtual void UpdateMesh(ITesselatorAPI? tesselator = null)
        {
            tesselator ??= ((ICoreClientAPI)Api).Tesselator;
            MeshData mesh = GenBaseMesh(tesselator);
            BaseMesh = mesh.Clone().Rotate(new Vec3f(0.5f, 0.5f, 0.5f), 0, MeshAngle, 0);
        }

        private void OnSlotModifid(int slot)
        {
            MarkDirty(true);
        }

        #endregion

        public virtual void LoadSound(ICoreClientAPI capi)
        {
            TickSound ??= ((IClientWorldAccessor)capi.World).LoadSound(new SoundParams
            {
                Location = new AssetLocation("decoclock:sounds/ticking"),
                ShouldLoop = false,
                Position = Pos.ToVec3f().Add(0.5f, 1.5f, 0.5f),
                DisposeOnFinish = false,
                Volume = 0.5f,
                Range = 8f,
                SoundType = EnumSoundType.Ambient
            });

        }
        public abstract void RegisterRenderer(ICoreClientAPI capi);


        public void DropContents()
        {
            Inventory?.DropAll(Pos.ToVec3d().Add(0.5, 0.5, 0.5));
        }

        #region Events


        public override void OnReceivedClientPacket(IPlayer player, int packetid, byte[] data)
        {
            ClientPackets(player, packetid, data);
            base.OnReceivedClientPacket(player, packetid, data);

        }

        public virtual void ClientPackets(IPlayer player, int packetid, byte[] data)
        {
            if (packetid < 1000)
            {
                Inventory.InvNetworkUtil.HandleClientPacket(player, packetid, data);
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
                inventoryManager.OpenInventory(Inventory);
            }

            if (packetid == 1001)
            {
                IPlayerInventoryManager inventoryManager = player.InventoryManager;
                inventoryManager?.CloseInventory(Inventory);
            }

            if (packetid == Constants.TypeDial)
            {
                TypeDial = BitConverter.ToInt32(data, 0);
                MarkDirty(true);
            }

            if(packetid == Constants.MuteSounds)                //Silence, my brother.
            {
                MuteSounds = BitConverter.ToBoolean(data, 0);
                MarkDirty(true);
            }
        }


        public virtual void GetVariablesFromTreeAttributes(ITreeAttribute tree)
        {
            MeshAngle = tree.GetFloat("meshAngle", MeshAngle);
            TypeDial = tree.GetInt("typedial", TypeDial);
            MuteSounds = tree.GetBool("mutesoudns", MuteSounds);
        }

        public override void FromTreeAttributes(ITreeAttribute tree, IWorldAccessor worldForResolving)
        {
            base.FromTreeAttributes(tree, worldForResolving);
            GetVariablesFromTreeAttributes(tree);
            InitInventory();
            Inventory.FromTreeAttributes(tree);
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
            tree.SetFloat("meshAngle", MeshAngle);
            tree.SetInt("typedial", TypeDial);
            tree.SetBool("mutesoudns", MuteSounds);
            Inventory?.ToTreeAttributes(tree);
        }

        public override void OnBlockRemoved()
        {
            base.OnBlockRemoved();
            TickSound?.Stop();
            TickSound?.Dispose();
        }

        public override void OnBlockBroken(IPlayer? byPlayer = null)
        {
            base.OnBlockBroken(byPlayer);
            if (Api.World is IServerWorldAccessor)
            {
                Inventory?.DropAll(Pos.ToVec3d().Add(0.5, 0.5, 0.5));
            }
        }

        #endregion

        public override void OnBlockUnloaded()
        {
            base.OnBlockUnloaded();
            TickSound?.Dispose();
        }
    }
}
