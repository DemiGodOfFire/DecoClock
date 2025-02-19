using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;

namespace DecoClock
{
    internal class BEWallClock : BEVariableClock
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
            }

            if (Api.Side == EnumAppSide.Client)
            {
                dialogClock?.TryOpen();
            }
            return false;
        }

        private MeshData GenBaseMesh(ITesselatorAPI tesselator, int type)
        {
            string path = this.PathBlock + $"complete{type}.json";
            Shape shape = Api.Assets.TryGet(path).ToObject<Shape>();
            tesselator.TesselateShape("BeClock", shape, out MeshData mesh, this);
            return mesh;
        }

        public override void UpdateMesh(ITesselatorAPI? tesselator = null)
        {
            tesselator ??= ((ICoreClientAPI)Api).Tesselator;
            MeshData mesh = GenBaseMesh(tesselator, 1);
            BaseMesh = mesh.Clone().Rotate(new Vec3f(0.5f, 0.5f, 0.5f), 0, MeshAngle, 0);
            rendererClock?.Update(
                GetItemMesh("hourhand"), 0.005f,
                GetItemMesh("minutehand"), 0f, 0.005f,
                GetItemMesh("tickmarks", TypeDial), 0f, -0.439f,
                MeshAngle);
        }

        public override void RegisterRenderer(ICoreClientAPI capi)
        {
            capi.Event.RegisterRenderer(rendererClock = new ClockRenderer(capi, Pos), EnumRenderStage.Opaque);
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
