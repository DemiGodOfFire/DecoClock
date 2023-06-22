using Vintagestory.API.Client;
using Vintagestory.API.MathTools;

namespace DecoClock.Render
{
    internal class GrandfatherClockDoorRenderer : IRenderer
    {
        private ICoreClientAPI capi;
        private BlockPos pos;
        private MeshRef? door;
        private Matrixf modelMat = new Matrixf();
        private float meshAngle;
        private float i = 0;
        private bool open = false;
        private bool close = true;
        public GrandfatherClockDoorRenderer(ICoreClientAPI capi, BlockPos pos)
        {
            this.capi = capi;
            this.pos = pos;
        }

        public double RenderOrder => 0.5;
        public int RenderRange => 24;

        public void OnRenderFrame(float deltaTime, EnumRenderStage stage)
        {
            if (door == null) { return; }
            if (open && i <= 120) { i += 11; }
            else if (close && i > 0) { i -= 11; }
            IRenderAPI rpi = capi.Render;
            Vec3d camPos = capi.World.Player.Entity.CameraPos;
            rpi.GlDisableCullFace();
            rpi.GlToggleBlend(true);

            IStandardShaderProgram doorShader = rpi.PreparedStandardShader(pos.X, pos.Y, pos.Z);
            doorShader.Tex2D = capi.BlockTextureAtlas.AtlasTextureIds[0];
            doorShader.ModelMatrix = modelMat
               .Identity()
               .Translate(pos.X - camPos.X, pos.Y - camPos.Y, pos.Z - camPos.Z)
               .Translate(0.5f, 0.0f, 0.5f)
               .RotateY(meshAngle)
               .Translate(0.3125f, 0f, -0.0563f)
               .RotateYDeg(i)
               .Translate(-0.8125f, 0f, -0.4437) 
               .Values;

            doorShader.ViewMatrix = rpi.CameraMatrixOriginf;
            doorShader.ProjectionMatrix = rpi.CurrentProjectionMatrix;
            rpi.RenderMesh(door);
            doorShader.Stop();

        }

        public void Update(MeshData? door, float meshAngle)
        {
            this.meshAngle = meshAngle;
            this.door?.Dispose();
            this.door = null;

            if (door != null)
            {
                this.door = capi.Render.UploadMesh(door);
            }
        }

        public void Open()
        {
            open = true;
            close = false;
        }

        public void Close()
        {
            close = true;
            open = false;
        }

        public void Dispose()
        {
            capi.Event.UnregisterRenderer(this, EnumRenderStage.Opaque);
            door?.Dispose();
        }
    }
}
