using System;
using Vintagestory.API.Client;
using Vintagestory.API.MathTools;

namespace DecoClock
{
    public class ClockHandRenderer : IRenderer
    {
        private int time = 25;
        private int hourMemory;
        private int minuteMemory;
        private bool isWork;
        private float meshAngle;
        private readonly ICoreClientAPI capi;
        private readonly BlockPos pos;
        private readonly Matrixf modelMat = new();
        private MeshRef? hourHand;
        private MeshRef? minuteHand;
        public event Action<int>? HourTick;
        public event Action? MinuteTick;

        public double RenderOrder => 0.37;
        public int RenderRange => 24;

        public ClockHandRenderer(ICoreClientAPI coreClientAPI, BlockPos pos)
        {
            capi = coreClientAPI;
            this.pos = pos;
        }

        public void OnRenderFrame(float deltaTime, EnumRenderStage stage)
        {
            if (hourHand == null && minuteHand == null)
            {
                return;
            }
            float hourRad;
            float minuteRad;


            if (isWork)
            {
                time = (int)Math.Round(capi.World.Calendar.HourOfDay / capi.World.Calendar.HoursPerDay * 24f * 1000);
            }
            int hour = time / 1000;
            int minute = time % 1000;
            int hourM12 = hour % 12;
            int minute60 = (minute * 6 + 50) / 100;
            hourRad = ((hourM12 * 60 + minute60) * 0.5f) * (float)Math.PI / 180;
            minuteRad = minute60 * (6f) * (float)Math.PI / 180;

            //if (minute60 == 60)
            //{
            //    minute60 = 0;
            //}

            if (isWork)
            {
                if (hourMemory != hourM12)
                {
                    hourMemory = hourM12;
                    if (minute60 == 0)
                    {
                        HourTick?.Invoke(hour);
                    }

                }


                if (minuteMemory != minute60)
                {
                    minuteMemory = (int)minute60;
                    MinuteTick?.Invoke();
                }
            }

            IRenderAPI rpi = capi.Render;
            Vec3d camPos = capi.World.Player.Entity.CameraPos;
            rpi.GlDisableCullFace();
            rpi.GlToggleBlend(true);

            IStandardShaderProgram handShader = rpi.PreparedStandardShader(pos.X, pos.Y, pos.Z);
            handShader.Tex2D = capi.BlockTextureAtlas.AtlasTextures[0].TextureId;

            if (minuteHand != null)
            {
                handShader.ModelMatrix = modelMat
                .Identity()
                .Translate(pos.X - camPos.X, pos.Y - camPos.Y, pos.Z - camPos.Z)
                .Translate(0.5f, 1.5f, 0.5f)
                .RotateY(meshAngle)
                .RotateZ(-minuteRad)
                .Translate(-0.5f, 0f, -0.601f)
                .Values;
                handShader.ViewMatrix = rpi.CameraMatrixOriginf;
                handShader.ProjectionMatrix = rpi.CurrentProjectionMatrix;
                rpi.RenderMesh(minuteHand);
            }

            if (hourHand != null)
            {

                handShader.ModelMatrix = modelMat
                .Identity()
                .Translate(pos.X - camPos.X, pos.Y - camPos.Y, pos.Z - camPos.Z)
                .Translate(0.5f, 1.5f, 0.5f)
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

        public void Update(MeshData? hourHand, MeshData? minuteHand, float meshAngle, bool isWork)
        {
            this.isWork = isWork;
            this.meshAngle = meshAngle;
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
