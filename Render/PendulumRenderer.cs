using System;
using Vintagestory.API.Client;
using Vintagestory.API.MathTools;

namespace DecoClock.Render
{
    internal class PendulumRenderer : IRenderer
    {
        private ICoreClientAPI capi;
        private BlockPos pos;
        private MeshRef? weight;
        private MeshRef? pendulum; 
        private Matrixf modelMat = new ();
        private float meshAngle;
        private int directions = 1;

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
            float hourOfDay = capi.World.Calendar.HourOfDay;
            float hour = capi.World.Calendar.FullHourOfDay / capi.World.Calendar.HoursPerDay * 24f;
            float minutesFloat = hourOfDay - hour;
            float cosMinute = (float)(Math.Cos((double)minutesFloat*1200 / Math.PI));
            float angleDeg = 15f * cosMinute;
            IRenderAPI rpi = capi.Render;
            Vec3d camPos = capi.World.Player.Entity.CameraPos;
            rpi.GlDisableCullFace();
            rpi.GlToggleBlend(true);

            IStandardShaderProgram pendulumShader = rpi.PreparedStandardShader(pos.X, pos.Y, pos.Z);
            pendulumShader.Tex2D = capi.BlockTextureAtlas.AtlasTextures[0].TextureId;
            pendulumShader.ModelMatrix = modelMat
               .Identity()
               .Translate(pos.X - camPos.X, pos.Y - camPos.Y, pos.Z - camPos.Z)
               .Translate(0.5f, 1.125f, 0.5f)
               .RotateY(meshAngle)
               .RotateZDeg(angleDeg)
               .Translate(-0.5f, -1.125f, -0.5625f)
               .Values;
            pendulumShader.ViewMatrix = rpi.CameraMatrixOriginf;
            pendulumShader.ProjectionMatrix = rpi.CurrentProjectionMatrix;
            rpi.RenderMesh(pendulum);
            pendulumShader.Stop();

        }

        public void Update(MeshData? pendulum, MeshData? weight, float angleMesh)
        {
            this.meshAngle = angleMesh;
            this.pendulum?.Dispose();
            this.pendulum = null;

            if (pendulum != null)
            {
                this.pendulum = capi.Render.UploadMesh(pendulum);
            }

            this.weight?.Dispose();
            this.weight = null;

            if (weight != null)
            {
                this.weight = capi.Render.UploadMesh(weight);
            }
        }

        public void Dispose()
        {
            capi.Event.UnregisterRenderer(this, EnumRenderStage.Opaque);
            pendulum?.Dispose();
        }
    }
}
