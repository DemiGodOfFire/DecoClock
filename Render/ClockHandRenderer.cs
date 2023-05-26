using System;
using System.Runtime.InteropServices;
using Vintagestory.API.Client;
using Vintagestory.API.MathTools;

namespace DecoClock
{
    public class ClockHandRenderer : IRenderer
    {
        internal bool ShouldRender;

        private ICoreClientAPI capi;
        private BlockPos pos;
        float angleRad;

        public MeshRef? hourHand;
        public MeshRef? minuteHand;

        Matrixf ModelMat = new Matrixf();

        public ClockHandRenderer(ICoreClientAPI coreClientAPI, BlockPos pos)
        {
            capi = coreClientAPI;
            this.pos = pos;
        }

        public double RenderOrder => 0.5;
        public int RenderRange => 24;



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
            //if (meshref == null || !ShouldRender) return;
            if (hourHand == null && minuteHand == null)
            {
                return;
            }
            float hourOfDay = capi.World.Calendar.HourOfDay;
            float hour = capi.World.Calendar.FullHourOfDay / capi.World.Calendar.HoursPerDay * 24;


            float minutesFloat = hourOfDay - hour;

            float hourM12 = hourOfDay % 12f;
            float hourRad = hourM12 * (float)Math.PI/6;
            float mminureRad = 2 * (float)Math.PI * minutesFloat;

            IRenderAPI rpi = capi.Render;
            Vec3d camPos = capi.World.Player.Entity.CameraPos;
            rpi.GlDisableCullFace();
            rpi.GlToggleBlend(true);
            IStandardShaderProgram hourHandShader = rpi.PreparedStandardShader(pos.X, pos.Y, pos.Z);
            hourHandShader.Tex2D = capi.BlockTextureAtlas.AtlasTextures[0].TextureId;

            if (minuteHand != null)
            {
                hourHandShader.ModelMatrix = ModelMat
                .Identity()
                .Translate(pos.X - camPos.X, pos.Y - camPos.Y, pos.Z - camPos.Z)
                .Translate(0.5f, 1.5f, 0.5f)
                .RotateY(angleRad)
                .RotateZ(-mminureRad)
                .Translate(-0.5f, 0f, -0.601f)
                .Values;
                hourHandShader.ViewMatrix = rpi.CameraMatrixOriginf;
                hourHandShader.ProjectionMatrix = rpi.CurrentProjectionMatrix;
                rpi.RenderMesh(minuteHand);
            }
            if (hourHand != null)
            {
               
                hourHandShader.ModelMatrix = ModelMat
                .Identity()
                .Translate(pos.X - camPos.X, pos.Y - camPos.Y, pos.Z - camPos.Z)
                .Translate(0.5f, 1.5f,0.5f)
                .RotateY(angleRad)
                .RotateZ(-hourRad)
                .Translate(-0.5f, 0f, -0.61f)
                .Values;
                hourHandShader.ViewMatrix = rpi.CameraMatrixOriginf;
                hourHandShader.ProjectionMatrix = rpi.CurrentProjectionMatrix;
                rpi.RenderMesh(hourHand);
            }

           
            hourHandShader.Stop();



        }

        public void Update(MeshData? hourHand, MeshData? minuteHand, float angleRad)
        {
            this.angleRad = angleRad;
            this.hourHand?.Dispose();
            this.hourHand = null;

            if (hourHand != null)
            {
                this.hourHand = capi.Render.UploadMesh(hourHand);
            }

            this.minuteHand?.Dispose();
            this.minuteHand = null;

            if (minuteHand != null)
            {
                this.minuteHand = capi.Render.UploadMesh(minuteHand);
            }
        }



        public void Dispose()
        {
            capi.Event.UnregisterRenderer(this, EnumRenderStage.Opaque);

            hourHand?.Dispose();
            minuteHand?.Dispose();
        }

    }
}
