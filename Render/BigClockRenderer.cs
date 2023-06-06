using Vintagestory.API.Client;
using Vintagestory.API.MathTools;

namespace DecoClock
{
    internal class BigClockRenderer : ClockRenderer
    {
        private readonly Matrixf modelMat = new();
        MeshRef? tribe;
        MeshRef? dial;
        float dyDial;
        float dzDial;

        public BigClockRenderer(ICoreClientAPI coreClientAPI, BlockPos pos) : base(coreClientAPI, pos)
        {
        }
        public override void BuildShader(IRenderAPI rpi, Vec3d camPos, IStandardShaderProgram clockShader, float hourRad, float minuteRad)
        {
            base.BuildShader(rpi, camPos, clockShader, hourRad, minuteRad);
            if (tribe != null)
            {
                clockShader.ModelMatrix = modelMat
                .Identity()
                .Translate(Pos.X - camPos.X, Pos.Y - camPos.Y, Pos.Z - camPos.Z)
                .Translate(0.5f, 0.5f, 0.5f)
                .RotateY(MeshAngle)
                .Translate(-0.5f, -0.5f, -0.5f)
                .Values;
                clockShader.ViewMatrix = rpi.CameraMatrixOriginf;
                clockShader.ProjectionMatrix = rpi.CurrentProjectionMatrix;
                rpi.RenderMesh(tribe);
            }
            if (dial != null)
            {
                clockShader.ModelMatrix = modelMat
               .Identity()
               .Translate(Pos.X - camPos.X, Pos.Y - camPos.Y, Pos.Z - camPos.Z)
               .Translate(0.5f, 0.5f + dyDial, 0.5f)
               .RotateY(MeshAngle)
               .Translate(-0.5f, -0.5f, -0.5f + dzDial)
               .Values;
                clockShader.ViewMatrix = rpi.CameraMatrixOriginf;
                clockShader.ProjectionMatrix = rpi.CurrentProjectionMatrix;
                rpi.RenderMesh(dial);
            }
        }
        public override bool IsNotRender()
        {
            return base.IsNotRender() && tribe == null && dial == null;
        }

        public void Update(MeshData? hourHand, float dzHour,
            MeshData? minuteHand, float dzMinute, float dyHand,
            MeshData? tribe,
            MeshData? dial, float dzDial, float dyDial, float meshAngle)
        {
            base.Update(hourHand, dzHour, minuteHand, dzMinute, dyHand, meshAngle);
            this.tribe?.Dispose();
            this.tribe = null;

            if (tribe != null)
            {
                this.tribe = capi.Render.UploadMesh(tribe);
            }

            this.dial?.Dispose();
            this.dial = null;
            this.dzDial = dzDial;
            this.dyDial = dyDial;

            if (dial != null)
            {
                this.dial = capi.Render.UploadMesh(dial);
            }

        }
        public override void AddRenderer(float hourRad, float minuteRad)
        {
            base.AddRenderer(hourRad, minuteRad);
        }

        public override void Dispose()
        {
            base.Dispose();
            dial?.Dispose();
            tribe?.Dispose();
        }
    }
}
