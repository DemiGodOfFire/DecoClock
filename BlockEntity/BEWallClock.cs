using Vintagestory.API.Client;
using Vintagestory.API.Common;

namespace DecoClock
{
    internal class BEWallClock : BEClock
    {
        GuiDialogWallClock? dialogClock;
        ClockRenderer? rendererClock;
        public override string PathBlock => "decoclock:shapes/block/wallclock/";

        public override void AddParts()
        {
            _parts.Add(new("clockwork"));
            _parts.Add(new("tickmarks", "clockwork"));
            _parts.Add(new("hourhand", "clockwork"));
            _parts.Add(new("minutehand", "clockwork"));
            _parts.Add(new("dialglass"));
        }

        public override bool OnInteract(IPlayer byPlayer, BlockSelection blockSel)
        {
            if (dialogClock == null && Api.Side == EnumAppSide.Client)
            {
                dialogClock = new GuiDialogWallClock(Core.ModId + ":wallclock-title", Inventory, Pos, (ICoreClientAPI)Api);
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
        public override void UpdateMesh(ITesselatorAPI? tesselator = null)
        {
            base.UpdateMesh(tesselator);
            rendererClock?.Update(GetItemMesh("hourhand"), 0.005f, GetItemMesh("minutehand"), 0.005f, 0f, MeshAngle);
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
