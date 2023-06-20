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

        // private int directions = 1;
        private float dyPendulum;
        private float dzPendulum;
        private float dxWeight;
        private float dyWeight;
        private float dzWeight;

        public GrandfatherClockRenderer(ICoreClientAPI coreClientAPI, BlockPos pos) : base(coreClientAPI, pos)
        {
        }

        public override void BuildShader(IRenderAPI rpi, Vec3d camPos, IStandardShaderProgram clockShader,
            float hourRad, float minuteRad)
        {
            base.BuildShader(rpi, camPos, clockShader, hourRad, minuteRad);
            if (pendulum != null)
            {
                float cosMinute = (float)(Math.Cos(Time % 10000 * 0.12f / Math.PI + Math.PI / 2));
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
            if (weight != null)
            {
                RenderWeight(rpi, camPos, clockShader, -dxWeight, 0.5f);
                RenderWeight(rpi, camPos, clockShader, dxWeight, 0.5f);
            }
        }

        void RenderWeight(IRenderAPI rpi, Vec3d camPos, IStandardShaderProgram clockShader, float dx, float dy)
        {
            clockShader.ModelMatrix = modelMat
               .Identity()
               .Translate(Pos.X - camPos.X, Pos.Y - camPos.Y, Pos.Z - camPos.Z)
               .Translate(0.5f, 0.5f + dyWeight - dy, 0.5f)
               .RotateY(MeshAngle)
               .Translate(-0.5f + dx, -0.5f, -0.5f + dzWeight)
               .Values;
            clockShader.ViewMatrix = rpi.CameraMatrixOriginf;
            clockShader.ProjectionMatrix = rpi.CurrentProjectionMatrix;
            rpi.RenderMesh(weight);
        }

        public override bool IsNotRender()
        {
            return base.IsNotRender() && pendulum == null;
        }

        public void Update(
            MeshData? hourHand, float dzHour,
            MeshData? minuteHand, float dyHand, float dzMinute,
            MeshData? dial, float dyDial, float dzDial,
            MeshData? pendulum, float dyPendulum, float dzPendulum,
            MeshData? weight, float dxWeight, float dyWeight, float dzWeight,
            float meshAngle)
        {
            base.Update(hourHand, dzHour, minuteHand, dyHand, dzMinute, dial, dyDial, dzDial, meshAngle);

            this.pendulum?.Dispose();
            this.pendulum = null;
            this.dyPendulum = dyPendulum;
            this.dzPendulum = dzPendulum;

            if (pendulum != null)
            {
                this.pendulum = capi.Render.UploadMesh(pendulum);
                IfWork = true;
            }
            else
            {
                IfWork = false;

            }

            this.weight?.Dispose();
            this.weight = null;
            this.dxWeight = dxWeight;
            this.dyWeight = dyWeight;
            this.dzWeight = dzWeight;

            if (weight != null)
            {
                this.weight = capi.Render.UploadMesh(weight);
            }
        }

        public override void Dispose()
        {
            base.Dispose();
            pendulum?.Dispose();
        }
    }
}
