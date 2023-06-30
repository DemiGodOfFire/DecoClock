using System;
using Vintagestory.API.Client;
using Vintagestory.API.MathTools;
using Vintagestory.GameContent;

namespace DecoClock
{
    public class ClockRenderer : IRenderer
    {
        private float dzHourHand;
        private float dzMinuteHand;
        private float multiplier;
        private float count;
        private int hourMemory;
        private int minuteMemory;
        static Random? _rand;
        private readonly Matrixf modelMat = new();
        private MeshRef? hourHand;
        private MeshRef? minuteHand;
        public event Action<int>? HourTick;
        public event Action? MinuteTick;
        public readonly ICoreClientAPI capi;
        public BlockPos Pos { get; }
        public MeshRef? Dial { get; set; }
        public int Time { get; set; } = 100000;
        public float MeshAngle { get; set; }
        public float DyDial { get; set; }
        public float DzDial { get; set; }
        public float DyHand { get; set; }
        public bool IfWork { get; set; } = true;



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

            float hourRad = 0;
            float minuteRad = 0;

            if (IfWork)
            {
                if (capi.ModLoader.GetModSystem<SystemTemporalStability>().StormData.nowStormActive)
                {
                    if (count <= 0)
                    {
                        _rand ??= new Random();
                        count = (float)_rand.NextDouble() * 100;
                        multiplier = ((float)_rand.NextDouble() - 0.5f) * 6f;
                    }
                    Time += (int)(deltaTime * multiplier * 100);
                    count -= deltaTime;
                }
                else
                {
                    Time = (int)Math.Round(capi.World.Calendar.HourOfDay / capi.World.Calendar.HoursPerDay * 24f * 10000);
                }
            }

            if (hourHand != null || minuteHand != null)
            {
                int hour = Time / 10000;
                int minute = Time % 10000;
                int hourM12 = hour % 12;
                int minute60 = (minute * 6 + 50) / 1000;
                hourRad = ((hourM12 * 60 + minute60) * 0.5f) * (float)Math.PI / 180;

                if (hourMemory != hourM12)
                {
                    hourMemory = hourM12;
                    if (minute60 == 0 && IfWork)
                    {
                        HourTick?.Invoke(hour);
                    }
                }
                minuteRad = minute60 * (6f) * (float)Math.PI / 180;
                if (minuteMemory != minute60 && IfWork)
                {
                    minuteMemory = (int)minute60;
                    MinuteTick?.Invoke();
                }
            }
            AddRenderer(hourRad, minuteRad);
        }

        public virtual bool IsNotRender()
        {
            return hourHand == null && minuteHand == null && Dial == null;
        }

        public virtual void Update(
            MeshData? hourHand, float dzHour,
            MeshData? minuteHand, float dyHand, float dzMinute,
            MeshData? dial, float dyDial, float dzDial,
            float meshAngle)
        {
            this.DyHand = dyHand;
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

            this.Dial?.Dispose();
            this.Dial = null;
            this.DyDial = dyDial;
            this.DzDial = dzDial;

            if (dial != null)
            {
                this.Dial = capi.Render.UploadMesh(dial);
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

            if (Dial != null)
            {
                DialRender(rpi, camPos, clockShader);
            }
        }

        public virtual void HandRender(IRenderAPI rpi, Vec3d camPos, IStandardShaderProgram clockShader,
            MeshRef mesh, float angleRad, float dz)
        {
            clockShader.ModelMatrix = modelMat
               .Identity()
               .Translate(Pos.X - camPos.X, Pos.Y - camPos.Y, Pos.Z - camPos.Z)
               .Translate(0.5f, 0.5f + DyHand, 0.5f)
               .RotateY(MeshAngle)
               .RotateZ(-angleRad)
               .Translate(-0.5f, -0.5f, -0.5f + dz)
               .Values;
            clockShader.ViewMatrix = rpi.CameraMatrixOriginf;
            clockShader.ProjectionMatrix = rpi.CurrentProjectionMatrix;
            rpi.RenderMesh(mesh);
        }

        public virtual void DialRender(IRenderAPI rpi, Vec3d camPos, IStandardShaderProgram clockShader)
        {
            clockShader.ModelMatrix = modelMat
           .Identity()
           .Translate(Pos.X - camPos.X, Pos.Y - camPos.Y, Pos.Z - camPos.Z)
           .Translate(0.5f, 0.5f + DyDial, 0.5f)
           .RotateY(MeshAngle)
           .Translate(-0.5f, -0.5f, -0.5f + DzDial)
           .Values;
            clockShader.ViewMatrix = rpi.CameraMatrixOriginf;
            clockShader.ProjectionMatrix = rpi.CurrentProjectionMatrix;
            rpi.RenderMesh(Dial);
        }


        public virtual void Dispose()
        {
            capi.Event.UnregisterRenderer(this, EnumRenderStage.Opaque);
            Dial?.Dispose();
            hourHand?.Dispose();
            minuteHand?.Dispose();
        }
    }
}
