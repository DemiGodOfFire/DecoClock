using System;
using Vintagestory.API.Client;
using Vintagestory.API.MathTools;

namespace DecoClock.Render
{
    internal class GrandfatherClockRenderer : ClockRenderer
    {
        private MeshRef? weight;
        private MeshRef? pendulum;
        private readonly Matrixf modelMat = new();

        private int directions = 1;
        private float dzPendulum;
        private float dyPendulum;

        public GrandfatherClockRenderer(ICoreClientAPI coreClientAPI, BlockPos pos) : base(coreClientAPI, pos)
        {
        }

        public override void BuildShader(IRenderAPI rpi, Vec3d camPos, IStandardShaderProgram clockShader,
            float hourRad, float minuteRad)
        {
            base.BuildShader(rpi, camPos, clockShader, hourRad, minuteRad);
            if (pendulum != null)
            {
                float cosMinute = (float)(Math.Cos(Time % 10000 * 0.12f / Math.PI+ Math.PI/2));
                float angleDeg = 15f * cosMinute;
                clockShader.ModelMatrix = modelMat
                .Identity()
                .Translate(Pos.X - camPos.X, Pos.Y - camPos.Y, Pos.Z - camPos.Z)
                .Translate(0.5f, 0.5f + dyPendulum, 0.5f)
                .RotateY(MeshAngle)
                .RotateZDeg(angleDeg)
                .Translate(-0.5f, -0.5f, -0.5f + dzPendulum)
                .Values;
                clockShader.ViewMatrix = rpi.CameraMatrixOriginf;
                clockShader.ProjectionMatrix = rpi.CurrentProjectionMatrix;
                rpi.RenderMesh(pendulum);
            }
        }

        public override bool IsNotRender()
        {
            return base.IsNotRender()&& pendulum == null;
        }

        public void Update(MeshData? hourHand, float dzHour, MeshData? minuteHand, float dzMinute, float dyHand,
            MeshData? pendulum, float dzPendulum, float dyPendulum, float meshAngle)
        {
            base.Update(hourHand, dzHour, minuteHand, dzMinute, dyHand, meshAngle);
            this.pendulum?.Dispose();
            this.pendulum = null;
            this.dzPendulum = dzPendulum;
            this.dyPendulum = dyPendulum;

            if (pendulum != null)
            {
                this.pendulum = capi.Render.UploadMesh(pendulum);
            }

            //this.weight?.Dispose();
            //this.weight = null;

            //if (weight != null)
            //{
            //    this.weight = capi.Render.UploadMesh(weight);
            //}
        }

        //public void OnRenderFrame(float deltaTime, EnumRenderStage stage)
        //{
        //    if (pendulum == null) { return; }
        //    float hourOfDay = capi.World.Calendar.HourOfDay;
        //    float hour = capi.World.Calendar.FullHourOfDay / capi.World.Calendar.HoursPerDay * 24f;
        //    float minutesFloat = hourOfDay - hour;
        //    float cosMinute = (float)(Math.Cos((double)minutesFloat*1200 / Math.PI));
        //    float angleDeg = 15f * cosMinute;
        //    IRenderAPI rpi = capi.Render;
        //    Vec3d camPos = capi.World.Player.Entity.CameraPos;
        //    rpi.GlDisableCullFace();
        //    rpi.GlToggleBlend(true);

        //    IStandardShaderProgram pendulumShader = rpi.PreparedStandardShader(pos.X, pos.Y, pos.Z);
        //    pendulumShader.Tex2D = capi.BlockTextureAtlas.AtlasTextures[0].TextureId;
        //    pendulumShader.ModelMatrix = modelMat
        //       .Identity()
        //       .Translate(pos.X - camPos.X, pos.Y - camPos.Y, pos.Z - camPos.Z)
        //       .Translate(0.5f, 1.125f, 0.5f)
        //       .RotateY(meshAngle)
        //       .RotateZDeg(angleDeg)
        //       .Translate(-0.5f, -1.125f, -0.5625f)
        //       .Values;
        //    pendulumShader.ViewMatrix = rpi.CameraMatrixOriginf;
        //    pendulumShader.ProjectionMatrix = rpi.CurrentProjectionMatrix;
        //    rpi.RenderMesh(pendulum);
        //    pendulumShader.Stop();

        //}

        //public void Update(MeshData? pendulum, MeshData? weight, float meshAngle)
        //{
        //    this.meshAngle = meshAngle;
        //    this.pendulum?.Dispose();
        //    this.pendulum = null;

        //    if (pendulum != null)
        //    {
        //        this.pendulum = capi.Render.UploadMesh(pendulum);
        //    }

        //    this.weight?.Dispose();
        //    this.weight = null;

        //    if (weight != null)
        //    {
        //        this.weight = capi.Render.UploadMesh(weight);
        //    }
        //}

        public override void Dispose()
        {
            base.Dispose();
            pendulum?.Dispose();
        }
    }
}
