using Vintagestory.API.Client;
using Vintagestory.API.MathTools;

namespace DecoClock.Render
{
    internal class PendulumRenderer : IRenderer
    {
        private ICoreClientAPI capi;
        private BlockPos pos;
        private MeshRef? pendulum;
        private Matrixf modelMat = new Matrixf();
        private float meshAngle;
        private float i = 0;
        private int delta = 1;

        public PendulumRenderer(ICoreClientAPI capi, BlockPos pos)
        {
            this.capi = capi;
            this.pos = pos;
        }

        public double RenderOrder => 0.5;
        public int RenderRange => 24;

        public void OnRenderFrame(float deltaTime, EnumRenderStage stage)
        {
            if (pendulum == null) { return; }
            if (i == -15 || i == 15) { delta *= -1; }
            IRenderAPI rpi = capi.Render;
            Vec3d camPos = capi.World.Player.Entity.CameraPos;
            rpi.GlDisableCullFace();
            rpi.GlToggleBlend(true);

            IStandardShaderProgram doorShader = rpi.PreparedStandardShader(pos.X, pos.Y, pos.Z);
            doorShader.Tex2D = capi.BlockTextureAtlas.AtlasTextures[0].TextureId;
            doorShader.ModelMatrix = modelMat
               .Identity()
               .Translate(pos.X - camPos.X, pos.Y - camPos.Y, pos.Z - camPos.Z)
               .Translate(0.5f, 1.125f, 0.5f)
               .RotateY(meshAngle)
               .RotateZDeg(i)
               .Translate(-0.5f, -1.125f, -0.5625f)
               .Values;
            doorShader.ViewMatrix = rpi.CameraMatrixOriginf;
            doorShader.ProjectionMatrix = rpi.CurrentProjectionMatrix;
            rpi.RenderMesh(pendulum);

            i += 0.5f * (float)delta;

            doorShader.Stop();

        }

        public void Update(MeshData? door, float angleMesh)
        {
            this.meshAngle = angleMesh;
            this.pendulum?.Dispose();
            this.pendulum = null;

            if (door != null)
            {
                this.pendulum = capi.Render.UploadMesh(door);
            }
        }

        public void Dispose()
        {
            capi.Event.UnregisterRenderer(this, EnumRenderStage.Opaque);
            pendulum?.Dispose();
        }
    }
}
