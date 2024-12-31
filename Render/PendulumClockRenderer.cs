using System;
using Vintagestory.API.Client;
using Vintagestory.API.MathTools;

namespace DecoClock.Render
{
    internal class PendulumClockRenderer(ICoreClientAPI capi, BlockPos pos) : ClockRenderer(capi, pos)
    {
        private MultiTextureMeshRef? weight;
        private MultiTextureMeshRef? pendulum;
        private readonly Matrixf modelMat = new();

        // private int directions = 1;
        private float dyPendulum;
        private float dzPendulum;
        private float dxWeight;
        private float dyWeight;
        private float dzWeight;

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
                rpi.RenderMultiTextureMesh(pendulum, "tex");
            }
            if (weight != null)
            {
                RenderWeight(rpi, camPos, clockShader, -dxWeight, 0.0f, 0);
                RenderWeight(rpi, camPos, clockShader, dxWeight, 0.5f, 90);
            }
        }

        void RenderWeight(IRenderAPI rpi, Vec3d camPos, IStandardShaderProgram clockShader, float dx, float dy, int rotate)
        {
            clockShader.ModelMatrix = modelMat
               .Identity()
               .Translate(Pos.X - camPos.X, Pos.Y - camPos.Y, Pos.Z - camPos.Z)
               .Translate(0.5f, dyWeight - dy, 0.5f)
               .RotateY(MeshAngle)
               .Translate(dx, 0,  dzWeight)
               .RotateYDeg(rotate)
               .Translate(-0.5f  , 0f, -0.5f )
               .Values;
            clockShader.ViewMatrix = rpi.CameraMatrixOriginf;
            clockShader.ProjectionMatrix = rpi.CurrentProjectionMatrix;
            rpi.RenderMultiTextureMesh(weight, "tex");
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
                this.pendulum = capi.Render.UploadMultiTextureMesh(pendulum);
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
                this.weight = capi.Render.UploadMultiTextureMesh(weight);
            }
        }

        public override void Dispose()
        {
            base.Dispose();
            pendulum?.Dispose();
            weight?.Dispose();
        }
    }
}
