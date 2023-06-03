using System;
using System.IO.Ports;
using Vintagestory.API.Client;
using Vintagestory.API.MathTools;

namespace DecoClock
{
    public class ClockRenderer : IRenderer
    {
        private int time = 25;
        private float dy;
        private float dzHour;
        private float dzMinute;
        private int hourMemory;
        private int minuteMemory;
        private float meshAngle;
        private readonly BlockPos pos;
        private readonly Matrixf modelMat = new();
        private MeshRef? hourHand;
        private MeshRef? minuteHand;
        public event Action<int>? HourTick;
        public event Action? MinuteTick;
        private readonly ICoreClientAPI capi;

        public double RenderOrder => 0.37;
        public int RenderRange => 24;

        public ClockRenderer(ICoreClientAPI coreClientAPI, BlockPos pos)
        {
            capi = coreClientAPI;
            this.pos = pos;
        }

        public virtual void OnRenderFrame(float deltaTime, EnumRenderStage stage)
        {
            if (hourHand == null && minuteHand == null)
            {
                return;
            }
            float hourRad;
            float minuteRad;


            time = (int)Math.Round(capi.World.Calendar.HourOfDay / capi.World.Calendar.HoursPerDay * 24f * 1000);


            int hour = time / 1000;
            int minute = time % 1000;
            int hourM12 = hour % 12;
            int minute60 = (minute * 6 + 50) / 100;
            hourRad = ((hourM12 * 60 + minute60) * 0.5f) * (float)Math.PI / 180;
            minuteRad = minute60 * (6f) * (float)Math.PI / 180;

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

            AddRenderer(hourRad, minuteRad);
        }

        public virtual void Update(MeshData? hourHand, float dzHour, MeshData? minuteHand, float dzMinute, float dy, float meshAngle)
        {
            this.dy = dy;
            this.dzHour = dzHour;
            this.dzMinute = dzMinute;
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

        public virtual void AddRenderer(float hourRad, float minuteRad)
        {
            IRenderAPI rpi = capi.Render;
            Vec3d camPos = capi.World.Player.Entity.CameraPos;
            rpi.GlDisableCullFace();
            rpi.GlToggleBlend(true);

            IStandardShaderProgram handShader = rpi.PreparedStandardShader(pos.X, pos.Y, pos.Z);
            handShader.Tex2D = capi.BlockTextureAtlas.AtlasTextures[0].TextureId;

            BuildShader(rpi, camPos, handShader, hourRad, minuteRad);

            handShader.Stop();
        }

        public virtual void BuildShader(IRenderAPI rpi, Vec3d camPos, IStandardShaderProgram handShader, float hourRad, float minuteRad)
        {
            if (hourHand != null)
            {
                HandRender(rpi, camPos, handShader, hourHand, hourRad, dzHour);
            }

            if (minuteHand != null)
            {
                HandRender(rpi, camPos, handShader, minuteHand, minuteRad, dzMinute);
            }
        }

        public virtual void HandRender(IRenderAPI rpi, Vec3d camPos, IStandardShaderProgram handShader, MeshRef mesh, float angleRad, float dz)
        {
            handShader.ModelMatrix = modelMat
               .Identity()
               .Translate(pos.X - camPos.X, pos.Y - camPos.Y, pos.Z - camPos.Z)
               .Translate(0.5f, 0.5f + dy, 0.5f)
               .RotateY(meshAngle)
               .RotateZ(-angleRad)
               //.Translate(-0.5f, 0f, -0.601f)
               .Translate(-0.5f, -0.5f, -0.5f + dz)
               .Values;
            handShader.ViewMatrix = rpi.CameraMatrixOriginf;
            handShader.ProjectionMatrix = rpi.CurrentProjectionMatrix;
            rpi.RenderMesh(mesh);
        }

        public virtual void Dispose()
        {
            capi.Event.UnregisterRenderer(this, EnumRenderStage.Opaque);
            hourHand?.Dispose();
            minuteHand?.Dispose();
        }
    }
}
