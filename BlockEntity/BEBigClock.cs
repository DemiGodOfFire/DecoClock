using Vintagestory.API.Client;
using Vintagestory.API.Common;

namespace DecoClock
{
    internal class BEBigClock : BEClock
    {
        GuiDialogBigClock? dialogClock;
        ClockRenderer? rendererClock;
        public override string PathBlock => "decoclock:shapes/block/clock/";
        public int Radius { get; set; }

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



        public override void UpdateMesh(ITesselatorAPI? tesselator = null)
        {
            base.UpdateMesh(tesselator);
            rendererClock?.Update(GetItemMesh("hourhand"), 0.55f, GetItemMesh("minutehand"), 0.62f, 0f, MeshAngle);
        }


        #endregion

        public override void LoadSound(ICoreClientAPI capi)
        {
            TickSound ??= ((IClientWorldAccessor)capi.World).LoadSound(new SoundParams
            {
                Location = new AssetLocation("decoclock:sounds/ticking"),
                ShouldLoop = false,
                Position = Pos.ToVec3f().Add(0.5f, 1.5f, 0.5f),
                DisposeOnFinish = false,
                Volume = 1f,
                Range = 48f,
                SoundType = EnumSoundType.Ambient
            });

        }

        public override void RegisterRenderer(ICoreClientAPI capi)
        {
            capi.Event.RegisterRenderer(rendererClock = new ClockRenderer(capi, Pos), EnumRenderStage.Opaque);
            rendererClock.MinuteTick += () =>
            {
                TickSound?.Start();
            };
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
