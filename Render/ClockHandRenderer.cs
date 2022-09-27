using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Client;
using Vintagestory.API.MathTools;

namespace DecoClock
{
    public class ClockHandRenderer : IRenderer
    {
        internal bool ShouldRender;

        private ICoreClientAPI api;
        private BlockPos pos;
        internal float AngleRad;

        MeshRef meshref;
        public Matrixf ModelMat = new Matrixf();

        public ClockHandRenderer(ICoreClientAPI coreClientAPI, BlockPos pos)
        {
            api = coreClientAPI;
            this.pos = pos;
            MarkDirty();
        }

        public double RenderOrder => 0.5;

        public int RenderRange => 24;

        public void MarkDirty()
        {
            //   meshref = coreClientAPI.Render.UploadMesh(mesh);
            var be = api.World.BlockAccessor.GetBlockEntity(pos) as BEClock;
            
            
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

        //void UpdateHandState()
        //{
        //    if (Api?.World == null) return;

        //    if (rendererHand != null)
        //    {
        //        rendererHand.AngleRad = MinuteAngle();
        //    }

        //    Api.World.BlockAccessor.MarkBlockDirty(Pos, OnRetesselatedMinuteHand);

        //    //if (nowGrinding)
        //    //{
        //    //    ambientSound?.Start();
        //    //}
        //    //else
        //    //{
        //    //    ambientSound?.Stop();
        //    //}

        //    if (Api.Side == EnumAppSide.Server)
        //    {
        //        MarkDirty();
        //    }

        //}

        //private void OnRetesselatedMinuteHand()
        //{
        //    if (rendererHand == null) return; // Maybe already disposed


        //    rendererHand.ShouldRender = minuteHandRotate;

        //}

        //private float MinuteAngle()
        //{
        //    return 2 * (float)Math.PI * minutes / 60; // check values
        //}


        public void OnRenderFrame(float deltaTime, EnumRenderStage stage)
        {
            if (meshref == null || !ShouldRender) return;

            IRenderAPI rpi = api.Render;
            Vec3d camPos = api.World.Player.Entity.CameraPos;

            rpi.GlDisableCullFace();
            rpi.GlToggleBlend(true);

            IStandardShaderProgram prog = rpi.PreparedStandardShader(pos.X, pos.Y, pos.Z);
            prog.Tex2D = api.BlockTextureAtlas.AtlasTextureIds[0];


            prog.ModelMatrix = ModelMat
                .Identity()
                .Translate(pos.X - camPos.X, pos.Y - camPos.Y, pos.Z - camPos.Z)
                .Translate(0.5f, 1f, 0.5f)
                .RotateZ(AngleRad)
                .Translate(-0.5f, 0f, -0.5f)
                .Values
               ;

            prog.ViewMatrix = rpi.CameraMatrixOriginf;
            prog.ProjectionMatrix = rpi.CurrentProjectionMatrix;
            rpi.RenderMesh(meshref);
            prog.Stop();

            ShouldRender = false;
        }

        public void Dispose()
        {
            api.Event.UnregisterRenderer(this, EnumRenderStage.Opaque);

            meshref.Dispose();
        }

    }
}