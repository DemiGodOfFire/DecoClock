using Vintagestory.API.Client;
using Vintagestory.API.MathTools;

namespace DecoClock
{
    internal class BigClockRenderer(ICoreClientAPI capi, BlockPos pos) : ClockRenderer(capi, pos)
    {
        private readonly Matrixf modelMat = new();
        MultiTextureMeshRef? tribe;
        float scale;
        float shiftZ;//0.0625f
        int i = 0;

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
                    .Translate(0.0f, 0.0f, 0.5f + shiftZ)
                    .Scale(scale, scale, scale)
                    .Translate(-0.5f, -0.5f, -0.5f)
                    .Values;
                clockShader.ViewMatrix = rpi.CameraMatrixOriginf;
                clockShader.ProjectionMatrix = rpi.CurrentProjectionMatrix;
                rpi.RenderMultiTextureMesh(tribe, "tex");
            }
        }

        public override void HandRender(IRenderAPI rpi, Vec3d camPos, IStandardShaderProgram clockShader,
            MultiTextureMeshRef mesh, float angleRad, float dz)
        {
            if (i == 360) i = 0;

            clockShader.ModelMatrix = modelMat
              .Identity()
              .Translate(Pos.X - camPos.X, Pos.Y - camPos.Y, Pos.Z - camPos.Z)
              .Translate(0.5f, 0.5f + DyHand, 0.5f)
              .RotateY(MeshAngle)
              .RotateZ(-angleRad)
              .Translate(0.0f, 0.0f, (dz - 0.5) * scale + 0.5 + shiftZ)

              .Scale(scale, scale, scale)
              .Translate(-0.5f, -0.5f, -0.5f)
              .Values;
            clockShader.ViewMatrix = rpi.CameraMatrixOriginf;
            clockShader.ProjectionMatrix = rpi.CurrentProjectionMatrix;
            rpi.RenderMultiTextureMesh(mesh, "tex");
            i++;

        }

        public override void DialRender(IRenderAPI rpi, Vec3d camPos, IStandardShaderProgram clockShader)
        {
            clockShader.ModelMatrix = modelMat
               .Identity()
               .Translate(Pos.X - camPos.X, Pos.Y - camPos.Y, Pos.Z - camPos.Z)
               .Translate(0.5f, 0.5f + DyDial, 0.5f)
               .RotateY(MeshAngle)
               .Translate(0.0f, 0.0f, DzDial + shiftZ)
               .Scale(scale, scale, scale)
               .Translate(-0.5f, -0.5f, -0.5f)
               .Values;
            clockShader.ViewMatrix = rpi.CameraMatrixOriginf;
            clockShader.ProjectionMatrix = rpi.CurrentProjectionMatrix;
            rpi.RenderMultiTextureMesh(Dial, "tex");
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
            int radius,
            int shiftZ,
            float meshAngle)
        {
            base.Update(hourHand, dzHour, minuteHand, dzMinute, dyHand, dial, dzDial, dyDial, meshAngle);
            scale = radius / 7f;
            this.shiftZ = shiftZ / 100f * 0.115f * radius;
            this.tribe?.Dispose();
            this.tribe = null;

            if (tribe != null)
            {
                this.tribe = capi.Render.UploadMultiTextureMesh(tribe);
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
