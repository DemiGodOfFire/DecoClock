using Vintagestory.API.Client;
using Vintagestory.API.MathTools;

namespace DecoClock
{
    internal class BigClockRenderer : ClockRenderer
    {
        private readonly Matrixf modelMat = new();
        MeshRef? tribe;
      

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
            
        }
        public override bool IsNotRender()
        {
            return base.IsNotRender() && tribe == null;
        }

        public void Update(
            MeshData? hourHand, float dzHour,
            MeshData? minuteHand, float dzMinute, float dyHand,
            MeshData? dial, float dzDial, float dyDial,
            MeshData? tribe,
            float meshAngle)
        {
            base.Update(hourHand, dzHour, minuteHand, dzMinute, dyHand, dial, dzDial, dyDial, meshAngle);
            this.tribe?.Dispose();
            this.tribe = null;

            if (tribe != null)
            {
                this.tribe = capi.Render.UploadMesh(tribe);
            }                  
        }
        public override void AddRenderer(float hourRad, float minuteRad)
        {
            base.AddRenderer(hourRad, minuteRad);
        }

        public override void Dispose()
        {
            base.Dispose();
            tribe?.Dispose();
        }
    }
}
