using Vintagestory.API.Client;
using Vintagestory.API.MathTools;

namespace DecoClock
{
    internal class CuckooRenderer : IRenderer
    {
        private readonly ICoreClientAPI capi;
        private readonly BlockPos pos;
        private readonly Matrixf modelMat = new();
        MeshRef? cuckoo;
        MeshRef? doorR;
        MeshRef? doorL;
        float cuckooDy;
        float cuckooDz;
        float doorDx;
        float doorDy;
        float doorDz;
        float meshAngle;
        bool cu;
        int i = 0;
        public double RenderOrder => 0.5;
        public int RenderRange => 24;

        public CuckooRenderer(ICoreClientAPI coreClientAPI, BlockPos pos)
        {
            capi = coreClientAPI;
            this.pos = pos;
        }

        public void OnRenderFrame(float deltaTime, EnumRenderStage stage)
        {
            if (doorR == null && doorL == null && cuckoo == null)
            {
                return;
            }

            IRenderAPI rpi = capi.Render;
            Vec3d camPos = capi.World.Player.Entity.CameraPos;
            rpi.GlDisableCullFace();
            rpi.GlToggleBlend(true);

            IStandardShaderProgram cuckooClockShader = rpi.PreparedStandardShader(pos.X, pos.Y, pos.Z);
            cuckooClockShader.Tex2D = capi.BlockTextureAtlas.AtlasTextures[0].TextureId;

            if (cu && cuckoo != null)
            {
                DoorRender(rpi, camPos, cuckooClockShader, doorL!, -doorDx, -i);
                DoorRender(rpi, camPos, cuckooClockShader, doorR!, doorDx, i);
                CuckooRender(rpi, camPos, cuckooClockShader, 0);
            }
            else
            {
                DoorRender(rpi, camPos, cuckooClockShader, doorL!, -doorDx);
                DoorRender(rpi, camPos, cuckooClockShader, doorR!, doorDx);
            }
            i++;

            cuckooClockShader.Stop();
        }

        private void DoorRender(IRenderAPI rpi, Vec3d camPos, IStandardShaderProgram doorShader,
            MeshRef door, float shift, float angle)
        {
            doorShader.ModelMatrix = modelMat
               .Identity()
               .Translate(pos.X - camPos.X, pos.Y - camPos.Y, pos.Z - camPos.Z)
               .Translate(0.5f, 0.5f + doorDy, 0.5f)
               .RotateY(meshAngle)
               .Translate(shift, 0, -doorDz)
               .RotateYDeg(angle)
               .Translate(-0.5f, -0.5f, -0.5)
               .Values;

            doorShader.ViewMatrix = rpi.CameraMatrixOriginf;
            doorShader.ProjectionMatrix = rpi.CurrentProjectionMatrix;
            rpi.RenderMesh(door);
        }

        private void DoorRender(IRenderAPI rpi, Vec3d camPos, IStandardShaderProgram doorShader,
           MeshRef door, float shift)
        {
            doorShader.ModelMatrix = modelMat
               .Identity()
               .Translate(pos.X - camPos.X, pos.Y - camPos.Y, pos.Z - camPos.Z)
               .Translate(0.5f, 0.5f + doorDy, 0.5f)
               .RotateY(meshAngle)
               .Translate(-0.5f + shift, -0.5f, -0.5 - doorDz)
               .Values;

            doorShader.ViewMatrix = rpi.CameraMatrixOriginf;
            doorShader.ProjectionMatrix = rpi.CurrentProjectionMatrix;
            rpi.RenderMesh(door);
        }

        private void CuckooRender(IRenderAPI rpi, Vec3d camPos, IStandardShaderProgram cuckooShader,
            float translateZ)
        {
            cuckooShader.ModelMatrix = modelMat
              .Identity()
              .Translate(pos.X - camPos.X, pos.Y - camPos.Y, pos.Z - camPos.Z)
              .Translate(0.5f, cuckooDy, 0.5f + translateZ)
              .RotateY(meshAngle)
              .Translate(-0.5f, -0.5f, -0.5 - cuckooDz)
              .Values;

            cuckooShader.ViewMatrix = rpi.CameraMatrixOriginf;
            cuckooShader.ProjectionMatrix = rpi.CurrentProjectionMatrix;
            rpi.RenderMesh(cuckoo);
        }

        public void Update(MeshData? cuckoo, float cuckooDy, float cuckooDz,
            MeshData? doorR, MeshData? doorL, float doorDx, float doorDy, float doorDz,
            float meshAngle)
        {
            this.meshAngle = meshAngle;

            if (cuckoo != null)
            {
                this.cuckoo = capi.Render.UploadMesh(cuckoo);
                this.cuckooDy = cuckooDy;
                this.cuckooDz = cuckooDz;
            }

            if (doorR != null && doorL != null)
            {
                this.doorR = capi.Render.UploadMesh(doorR);
                this.doorL = capi.Render.UploadMesh(doorL);
                this.doorDx = doorDx;
                this.doorDy = doorDy;
                this.doorDz = doorDz;
            }
        }

        public void Cu()
        {
            cu = true;
        }

        public void Dispose()
        {
            capi.Event.UnregisterRenderer(this, EnumRenderStage.Opaque);
        }
    }
}
