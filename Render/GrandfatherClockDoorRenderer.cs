using System;
using Vintagestory.API.Client;
using Vintagestory.API.MathTools;

namespace DecoClock.Render
{
    internal class GrandfatherClockDoorRenderer : IRenderer
    {
        private readonly ICoreClientAPI capi;
        private readonly BlockPos pos;
        private MeshRef? door;
        private readonly Matrixf modelMat = new();
        private float meshAngle;
        private float x = 0;
        private bool open = false;
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
            float angle;
            float timeAnimation = 0.43f;
            if (open && x < timeAnimation)
            {
                x += deltaTime;
                angle = Angle(timeAnimation);
            }
            else if (open && x >= timeAnimation) { angle = (float)Math.PI * 2 / 3; }
            else if (!open && x > 0)
            {
                x -= deltaTime;
                angle = Angle(timeAnimation);
            }
            else angle = 0;

            IRenderAPI rpi = capi.Render;
            Vec3d camPos = capi.World.Player.Entity.CameraPos;
            rpi.GlDisableCullFace();
            rpi.GlToggleBlend(true);

            IStandardShaderProgram doorShader = rpi.PreparedStandardShader(pos.X, pos.Y, pos.Z);
            doorShader.Tex2D = capi.BlockTextureAtlas.AtlasTextures[0].TextureId;
            doorShader.ModelMatrix = modelMat
               .Identity()
               .Translate(pos.X - camPos.X, pos.Y - camPos.Y, pos.Z - camPos.Z)
               .Translate(0.5f, 0.0f, 0.5f)
               .RotateY(meshAngle)
               .Translate(0.3125f, 0f, -0.0563f)
               .RotateY(angle)
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

        float Angle(float durationAnimation)
        {
            return (float)((0.5 * (-Math.Cos(x * Math.PI / durationAnimation) - 1) + 1) * 2 / 3 * Math.PI);
        }

        public void Open()
        {
            open = true;
        }

        public void Close()
        {
            open = false;
        }

        public void Dispose()
        {
            capi.Event.UnregisterRenderer(this, EnumRenderStage.Opaque);
            door?.Dispose();
        }
    }
}
