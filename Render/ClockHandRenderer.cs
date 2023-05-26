using System;
using Vintagestory.API.Client;
using Vintagestory.API.MathTools;

namespace DecoClock
{
    public class ClockHandRenderer : IRenderer
    {
        private ICoreClientAPI capi;
        private BlockPos pos;
        float meshAngle;

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

        public void OnRenderFrame(float deltaTime, EnumRenderStage stage)
        {
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
            IStandardShaderProgram handShader = rpi.PreparedStandardShader(pos.X, pos.Y, pos.Z);
            handShader.Tex2D = capi.BlockTextureAtlas.AtlasTextures[0].TextureId;

            if (minuteHand != null)
            {
                handShader.ModelMatrix = ModelMat
                .Identity()
                .Translate(pos.X - camPos.X, pos.Y - camPos.Y, pos.Z - camPos.Z)
                .Translate(0.5f, 1.5f, 0.5f)
                .RotateY(meshAngle)
                .RotateZ(-mminureRad)
                .Translate(-0.5f, 0f, -0.601f)
                .Values;
                handShader.ViewMatrix = rpi.CameraMatrixOriginf;
                handShader.ProjectionMatrix = rpi.CurrentProjectionMatrix;
                rpi.RenderMesh(minuteHand);
            }
            if (hourHand != null)
            {
               
                handShader.ModelMatrix = ModelMat
                .Identity()
                .Translate(pos.X - camPos.X, pos.Y - camPos.Y, pos.Z - camPos.Z)
                .Translate(0.5f, 1.5f,0.5f)
                .RotateY(meshAngle)
                .RotateZ(-hourRad)
                .Translate(-0.5f, 0f, -0.61f)
                .Values;
                handShader.ViewMatrix = rpi.CameraMatrixOriginf;
                handShader.ProjectionMatrix = rpi.CurrentProjectionMatrix;
                rpi.RenderMesh(hourHand);
            }
           
            handShader.Stop();

        }

        public void Update(MeshData? hourHand, MeshData? minuteHand, float angleRad)
        {
            this.meshAngle = angleRad;
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
