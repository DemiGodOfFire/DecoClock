using System;
using System.Collections.Generic;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Datastructures;
using Vintagestory.API.MathTools;
using Vintagestory.API.Server;

namespace DecoClock
{
    internal class BEClock : BlockEntity, ITexPositionSource
    {

        InventoryClock inventory;
        ILoadedSound ambientSound;
        ClockHandRenderer rendererHand;
        ITexPositionSource textureSource;


        public Size2i AtlasSize => textureSource.AtlasSize;

        protected List<ClockItem> Parts = new ();
        //{
        //    new("hourhand"),
        //    new("minutehand"),
        //    new("dialglass"),
        //    new("clockparts"),
        //    new("clockwork"),
        //    new("tickmarks")
        //};

        string curMatMHand = "silver";
        string curMatHHand = "meteoriciron";
    

       
        bool minuteHandRotate;
        public float MeshAngle;

        float minutes;



        #region Getters


        private void AddCodeParts()
        {

            AddParts();
            ClockManager manager = Api.ModLoader.GetModSystem<ClockManager>();
            foreach (var part in Parts)
            {
                part.Codes ??= manager.Parts[part.Type].ToArray();
            }
        }

        protected virtual void AddParts()
        {
            Parts.Add(new("hourhand"));
            Parts.Add(new("minutehand"));
            Parts.Add(new("dialglass"));
            Parts.Add(new("clockparts"));
            Parts.Add(new("clockwork"));
            Parts.Add(new("tickmarks"));
        }

      
        public virtual TextureAtlasPosition this[string textureCode]
        {
            get
            {
                if (textureCode == "thread") return textureSource["string"];

                ItemStack stack = inventory.TryGetPart(textureCode);
                if (stack is not null)
                {
                    var capi = (ICoreClientAPI)Api;
                    if (stack.Class == EnumItemClass.Item)
                    {
                        return capi.ItemTextureAtlas.GetPosition(stack.Item);
                    }
                    else
                    {
                        return capi.BlockTextureAtlas.GetPosition(stack.Block);
                    }
                }
                return textureSource[textureCode];
            }
        }

        MeshData ClockHourHandMesh
        {
            get
            {
                object value = null;
                Api.ObjectCache.TryGetValue("clockhourhandmesh-" + curMatHHand, out value);
                return (MeshData)value;
            }
            set { Api.ObjectCache["clockhourhandmesh-" + curMatHHand] = value; }
        }

        MeshData ClockMinuteHandMesh
        {
            get
            {
                object value = null;
                Api.ObjectCache.TryGetValue("clockminutehandmesh-" + curMatMHand, out value);
                return (MeshData)value;
            }
            set { Api.ObjectCache["clockminutehandmesh-" + curMatMHand] = value; }
        }

        #endregion


        public override void FromTreeAttributes(ITreeAttribute tree, IWorldAccessor worldForResolving)
        {
            base.FromTreeAttributes(tree, worldForResolving);

            MeshAngle = tree.GetFloat("meshAngle", MeshAngle);

            InitInventory();
            inventory.FromTreeAttributes(tree);
        }

        public override void ToTreeAttributes(ITreeAttribute tree)
        {
            base.ToTreeAttributes(tree);

            tree.SetFloat("meshAngle", MeshAngle);
            inventory?.ToTreeAttributes(tree);
        }

        private void InitInventory()
        {
            if (inventory == null)
            {
                AddCodeParts();
                inventory = new InventoryClock(Parts.ToArray(), Pos, Api);
            }
        }

        public override void Initialize(ICoreAPI api)
        {
            base.Initialize(api);
            if (api is ICoreClientAPI capi)
            {
                textureSource = capi.Tesselator.GetTexSource(Block);
            }
            InitInventory();
        }

        //private void OneMinute(float dt)
        //{
        //    float hourOfDay = api.World.Calendar.HourOfDay;
        //    float hourUpdate = api.World.Calendar.FullHourOfDay / api.World.Calendar.HoursPerDay * 24;


        //    float minutesFloat = hourOfDay - hour;
        //    float minutesUpdate = minutesFloat * 60f;

        //    if (minuteHandRotate = minutes != minutesUpdate)
        //    {
        //        minutes = minutesUpdate;
        //    }
        //    else return;

        //    if (hourHandRotate = hour != hourUpdate)
        //    {
        //        hour = hourUpdate;
        //    }

        //    int hourM12 = (int)hourOfDay % 12;
        //    UpdateHandState();

        //}

        public bool OnInteract(IPlayer byPlayer, BlockSelection blockSel)
        {
            ItemSlot handslot = byPlayer.InventoryManager.ActiveHotbarSlot;


            if (inventory.TryAddPart(handslot.Itemstack, out ItemStack content))

            {
                var pos = Pos.ToVec3d().Add(0.5, 0.25, 0.5);
                Api.World.PlaySoundAt(Block.Sounds.Place, pos.X, pos.Y, pos.Z, byPlayer);
                (Api as ICoreClientAPI)?.World.Player.TriggerFpAnimation(EnumHandInteract.HeldItemInteract);
                //delete one item from player
                return true;
            }

            return false;
        }

        void UpdateHandState()
        {
            if (Api?.World == null) return;

            if (rendererHand != null)
            {
                rendererHand.AngleRad = MinuteAngle();
            }

            Api.World.BlockAccessor.MarkBlockDirty(Pos, OnRetesselatedMinuteHand);

            //if (nowGrinding)
            //{
            //    ambientSound?.Start();
            //}
            //else
            //{
            //    ambientSound?.Stop();
            //}

            if (Api.Side == EnumAppSide.Server)
            {
                MarkDirty();
            }

        }

        private void OnRetesselatedMinuteHand()
        {
            if (rendererHand == null) return; // Maybe already disposed


            rendererHand.ShouldRender = minuteHandRotate;

        }

        private float MinuteAngle()
        {
            return 2 * (float)Math.PI * minutes / 60; // check values
        }

        internal MeshData GenBaseMesh()
        {

            ITesselatorAPI mesher = ((ICoreClientAPI)Api).Tesselator;
            AssetLocation assetLocation = Block.Shape.Base.WithPathPrefixOnce("shapes/").WithPathAppendixOnce(".json");
            Shape shape = Api.Assets.TryGet(assetLocation).ToObject<Shape>();
            mesher.TesselateShape("BeClock", shape, out MeshData mesh, this);

            return mesh;
        }

        internal MeshData GenMesh(string type)
        {


            var capi = Api as ICoreClientAPI;
            Shape shape = Api.Assets.TryGet("decoclock:shapes/block/grandfatherclock/" + type + ".json").ToObject<Shape>();
            capi.Tesselator.TesselateShape("BeClock", shape, out MeshData mesh, this);
            return mesh;
        }

        #region meshing

        MeshData baseMesh;

        public override bool OnTesselation(ITerrainMeshPool mesher, ITesselatorAPI tesselator)
        {
            if (baseMesh is null)
            {
                UpdateMesh();
            }
            mesher.AddMeshData(baseMesh);

            return true;
        }

        private void UpdateMesh()
        {
            baseMesh = GenBaseMesh().Clone().Rotate(new Vec3f(0.5f, 0.5f, 0.5f), 0, MeshAngle, 0);
        }


        #endregion

        public override void OnBlockRemoved()
        {
            base.OnBlockRemoved();

            //if (ambientSound != null)
            //{
            //    ambientSound.Stop();
            //    ambientSound.Dispose();
            //}

            if (Api?.World == null) return;


            rendererHand?.Dispose();
            rendererHand = null;
        }

        public override void OnBlockBroken(IPlayer byPlayer = null)                 // need OnBlockBroken or GetDrops?
        {
            base.OnBlockBroken(byPlayer);
            if (Api.World is IServerWorldAccessor)
            {
                inventory?.DropAll(Pos.ToVec3d().Add(0.5, 0.5, 0.5));
            }
        }

        public override void OnBlockUnloaded()
        {
            base.OnBlockUnloaded();
            rendererHand?.Dispose();
            ambientSound?.Dispose();
        }
    }

}
