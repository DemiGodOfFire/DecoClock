using System;
using Vintagestory.API.Client;
using Vintagestory.API.MathTools;

namespace DecoClock
{
    public class ClockRenderer : IRenderer
    {
        private float dyHand;
        private float dzHourHand;
        private float dzMinuteHand;
        private int hourMemory;
        private int minuteMemory;
        private readonly Matrixf modelMat = new();
        private MeshRef? hourHand;
        private MeshRef? minuteHand;
        public event Action<int>? HourTick;
        public event Action? MinuteTick;
        public readonly ICoreClientAPI capi;
        public int Time { get; set; } = 50;
        public float MeshAngle { get; set; }
        public bool IfWork { get; set; } = true;
        public BlockPos Pos { get; }

        public double RenderOrder => 0.37;
        public int RenderRange => 24;

        public ClockRenderer(ICoreClientAPI coreClientAPI, BlockPos pos)
        {
            capi = coreClientAPI;
            this.Pos = pos;
        }

        public virtual void OnRenderFrame(float deltaTime, EnumRenderStage stage)
        {
            if (IsNotRender())
            {
                return;
            }

            float hourRad=0;
            float minuteRad=0;

            if (IfWork)
            {
                Time = (int)Math.Round(capi.World.Calendar.HourOfDay / capi.World.Calendar.HoursPerDay * 24f * 10000);
            }

            if (hourHand != null )
            {
                int hour = Time / 10000;
                int minute = Time % 10000;
                int hourM12 = hour % 12;
                int minute60 = (minute * 6 + 50) / 1000;
                hourRad = ((hourM12 * 60 + minute60) * 0.5f) * (float)Math.PI / 180;

                if (hourMemory != hourM12)
                {
                    hourMemory = hourM12;
                    if (minute60 == 0)
                    {
                        HourTick?.Invoke(hour);
                    }
                }

                if (minuteHand != null)
                {
                    minuteRad = minute60 * (6f) * (float)Math.PI / 180;
                    if (minuteMemory != minute60)
                    {
                        minuteMemory = (int)minute60;
                        MinuteTick?.Invoke();
                    }
                }
            }
            AddRenderer(hourRad, minuteRad);
        }

        public virtual bool IsNotRender()
        {
            return hourHand == null && minuteHand == null;
        }

        public virtual void Update(MeshData? hourHand, float dzHour, MeshData? minuteHand, float dzMinute,
            float dyHand, float meshAngle)
        {
            this.dyHand = dyHand;
            this.dzHourHand = dzHour;
            this.dzMinuteHand = dzMinute;
            MeshAngle = meshAngle;
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

            IStandardShaderProgram clockShader = rpi.PreparedStandardShader(Pos.X, Pos.Y, Pos.Z);
            clockShader.Tex2D = capi.BlockTextureAtlas.AtlasTextures[0].TextureId;

            BuildShader(rpi, camPos, clockShader, hourRad, minuteRad);

            clockShader.Stop();
        }

        public virtual void BuildShader(IRenderAPI rpi, Vec3d camPos, IStandardShaderProgram clockShader,
            float hourRad, float minuteRad)
        {
            if (hourHand != null)
            {
                HandRender(rpi, camPos, clockShader, hourHand, hourRad, dzHourHand);
            }

            if (minuteHand != null)
            {
                HandRender(rpi, camPos, clockShader, minuteHand, minuteRad, dzMinuteHand);
            }
        }

        public virtual void HandRender(IRenderAPI rpi, Vec3d camPos, IStandardShaderProgram clockShader,
            MeshRef mesh, float angleRad, float dz)
        {
            clockShader.ModelMatrix = modelMat
               .Identity()
               .Translate(Pos.X - camPos.X, Pos.Y - camPos.Y, Pos.Z - camPos.Z)
               .Translate(0.5f, 0.5f + dyHand, 0.5f)
               .RotateY(MeshAngle)
               .RotateZ(-angleRad)
               .Translate(-0.5f, -0.5f, -0.5f + dz)
               .Values;
            clockShader.ViewMatrix = rpi.CameraMatrixOriginf;
            clockShader.ProjectionMatrix = rpi.CurrentProjectionMatrix;
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
