using System;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Datastructures;
using Vintagestory.API.MathTools;
using Vintagestory.API.Server;

namespace DecoClock
{
    internal class BEClock : BlockEntity, ITexPositionSource
    {
        ICoreAPI api;
        ClockBlock ownBlock;
        InventoryClock inventory;
        ILoadedSound ambientSound;
        ClockHandRenderer rendererHand;
        MeshData currentMesh;
        ITexPositionSource tmpTextureSource;

        public Size2i AtlasSize => tmpTextureSource.AtlasSize;

        string[] lis = { "sdsa" };

        private static string[] listCode = {"hourhand",
            "minutehand",
            "parts",
            "clockWork",
            "tickmarks" };



        public ClockItem[] listGrandfatherClockItems;

        string curMatMHand = "silver";
        string curMatHHand = "meteoriciron";
        string curMatClockParts = "brass";
        string curMatClockWork = "gold";
        string curMatDial = "blackbronze";
        string curMatDialGlass = "blue";
        string curMatDoorGlass = "red";

        bool minuteHand;
        bool hourHand;
        bool clockParts;
        bool clockWork;
        bool minuteHandRotate;
        bool hourHandRotate;
        public float MeshAngle;

        float hour;
        float minutes;
        long? listenerid;

        #region Getters


        public void addItem()
        {
            ClockItem clockWork = new ClockItem("clockwork");
            ClockItem tickmarks = new ClockItem("clockwork", zavs: 0);
            ClockItem clockParts = new ClockItem("parts");
            ClockItem minuteHand = new ClockItem("minutehand", zavs: 0);
            ClockItem hourhand = new ClockItem("hourhand", zavs: 0);
            listGrandfatherClockItems = new ClockItem[5];
            listGrandfatherClockItems[0] = clockWork;
            listGrandfatherClockItems[1] = tickmarks;
            listGrandfatherClockItems[2] = clockParts;
            listGrandfatherClockItems[3] = minuteHand;
            listGrandfatherClockItems[4] = hourhand;
        }

        public string Material
        {
            get { return Block.LastCodePart(); }
        }

        public TextureAtlasPosition this[string textureCode]
        {
            get
            {
                if (textureCode == "hour") return tmpTextureSource["metal-" + curMatHHand];

                if (textureCode == "minute") return tmpTextureSource["metal-" + curMatMHand];
                if (textureCode == "thread") return tmpTextureSource["string"];
                if (textureCode == "doorglass") return tmpTextureSource["glass-" + curMatDoorGlass];
                if (textureCode == "dialglass") return tmpTextureSource["glass-" + curMatDialGlass];
                if (textureCode == "clockparts") return tmpTextureSource["metal-" + curMatClockParts];
                if (textureCode == "clockwork") return tmpTextureSource["metal-" + curMatClockWork];
                if (textureCode == "tickmarks") return tmpTextureSource["metal-" + curMatDial];
                return tmpTextureSource[textureCode];
            }
        }

        MeshData ClockBaseMesh
        {
            get
            {
                object value;
                Api.ObjectCache.TryGetValue("clockbasemesh-" + Material, out value);
                return (MeshData)value;
            }
            set { Api.ObjectCache["clockbasemesh-" + Material] = value; }
        }



        MeshData ClockWorkMesh
        {
            get
            {
                object value;
                Api.ObjectCache.TryGetValue("clockworkmesh-" + Material, out value);
                return (MeshData)value;
            }
            set { Api.ObjectCache["clockwork-" + Material] = value; }
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


            //inventory.FromTreeAttributes(tree.GetTreeAttribute("inventory"));

            //if (Api != null)
            //{
            //    inventory.AfterBlocksLoaded(Api.World);
            //}

        }

        public override void ToTreeAttributes(ITreeAttribute tree)
        {
            base.ToTreeAttributes(tree);

            tree.SetFloat("meshAngle", MeshAngle);
        }

        private void InitInventory()
        {
            if (inventory == null)
            {
                addItem();
                //inventory = new InventoryClock(listGrandfatherClockItem, "DecoClock-grandfather_clock", Pos, api);
                inventory = new InventoryClock(codes, api, Pos);

            }
        }

        public override void Initialize(ICoreAPI api)
        {

            ownBlock = Block as ClockBlock;

            if (api is ICoreClientAPI capi)
            {
                tmpTextureSource = capi.Tesselator.GetTexSource(Block);
            }



            this.api = api;
            base.Initialize(api);
            InitInventory();

            if (api.Side == EnumAppSide.Client)
            {
                base.Initialize(api);

                ownBlock = Block as ClockBlock;
                if (Api.Side == EnumAppSide.Client)
                {
                    currentMesh = GenBaseMesh();
                    MarkDirty(true);
                }
            }


            //if (api.Side == EnumAppSide.Client)
            //{
            //    //rendererHourHand = new ClockHandRenderer(api as ICoreClientAPI, Pos, GenMesh("hour_hand"));

            //    //(api as ICoreClientAPI).Event.RegisterRenderer(rendererHourHand, EnumRenderStage.Opaque, "grandfather_clock");

            //    rendererHand = new ClockHandRenderer(api as ICoreClientAPI, Pos, GenMesh("minute_hand"));

            //    (api as ICoreClientAPI).Event.RegisterRenderer(rendererHand, EnumRenderStage.Opaque, "grandfather_clock");

            //    if (ClockBaseMesh == null)
            //    {
            //        ClockBaseMesh = GenMesh("base");
            //    }
            //    if (ClockMinuteHandMesh == null)
            //    {
            //        ClockMinuteHandMesh = GenMesh("minute_hand");
            //    }
            //    if (ClockHourHandMesh == null)
            //    {
            //        ClockHourHandMesh = GenMesh("hour_hand");
            //    }

            //}
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


            if (TryAddPart(handslot, byPlayer))

            {
                var pos = Pos.ToVec3d().Add(0.5, 0.25, 0.5);
                Api.World.PlaySoundAt(Block.Sounds.Place, pos.X, pos.Y, pos.Z, byPlayer);
                (Api as ICoreClientAPI)?.World.Player.TriggerFpAnimation(EnumHandInteract.HeldItemInteract);
                return true;
            }

            return true;
        }

        private bool TryAddPart(ItemSlot handslot, IPlayer byPlayer)
        {
            if (!clockWork && handslot.Itemstack.Collectible.Code.Path == "clockWork")
            {
                handslot.TakeOut(1);
                handslot.MarkDirty();
                clockWork = true;
                MarkDirty(true);
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
            //   Api.ObjectCache[type + "-" + "iron"] = mesh;

            return mesh;
        }

        //internal MeshData GenMeshes()
        //{

        //    GenBaseMesh();

        //    return 0;
        //}

        #region meshing


        public override bool OnTesselation(ITerrainMeshPool mesher, ITesselatorAPI tesselator)
        {
            string key = Block.Code + MeshAngle.ToString() + curMatDoorGlass + curMatClockWork + curMatDial + curMatClockParts + curMatDialGlass;
            if (!Api.ObjectCache.TryGetValue(key, out object mesh))
            {
                mesh = GenBaseMesh().Clone().Rotate(new Vec3f(0.5f, 0.5f, 0.5f), 0, MeshAngle, 0);
                Api.ObjectCache.Add(key, mesh);
            }
            mesher.AddMeshData((MeshData)mesh);



            // if (ClockWorkMesh != null)
            {
                //         mesher.AddMeshData(GenMesh("clockwork").Clone().Rotate(new Vec3f(0.5f, 0.5f, 0.5f), 0, MeshAngle, 0));
            }
            //bool skipmesh = base.OnTesselation(mesher, tesselator);
            //if (skipmesh) return true;


            //if (ownMesh == null)
            //{
            //    return true;
            //}

            // mesher.AddMeshData(ownMesh);
            //            if (quantityPlayersGrinding == 0 && !automated)
            //            {
            //                mesher.AddMeshData(
            //                    this.clockHandMesh.Clone()
            //                    .Rotate(new API.MathTools.Vec3f(0.5f, 0.5f, 0.5f), 0, renderer.AngleRad, 0)
            //                    .Translate(0 / 16f, 11 / 16f, 0 / 16f)
            //                );
            //            }


            return true;
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

            if (Api.World is IServerWorldAccessor)
            {
                inventory.DropAll(Pos.ToVec3d().Add(0.5, 0.5, 0.5));
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
