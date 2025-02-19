using System;
using Vintagestory.API.Client;
using Vintagestory.API.MathTools;

namespace DecoClock
{
    internal class CuckooRenderer(ICoreClientAPI capi, BlockPos pos) : IRenderer
    {
        private readonly Matrixf modelMat = new();
        MultiTextureMeshRef? cuckoo;
        MultiTextureMeshRef? doorR;
        MultiTextureMeshRef? doorL;
        float cuckooDy;
        float cuckooDz;
        float doorDx;
        float doorDy;
        float doorDz;
        float meshAngle;
        bool cu = false;
        float x = 0;
        float timeAnimation = 0;
        public double RenderOrder => 0.5;
        public int RenderRange => 24;
        public bool Cu => cuckoo != null;

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
                float y = AnimationY(deltaTime);
                float angle = y * 2 / 3 * (float)Math.PI;
                DoorRender(rpi, camPos, cuckooClockShader, doorL, -doorDx, -angle);
                DoorRender(rpi, camPos, cuckooClockShader, doorR, doorDx, angle);
                CuckooRender(rpi, camPos, cuckooClockShader, y * 0.1875f);
            }
            else
            {
                DoorRender(rpi, camPos, cuckooClockShader, doorL, -doorDx);
                DoorRender(rpi, camPos, cuckooClockShader, doorR, doorDx);           
            }
            cuckooClockShader.Stop();
        }

        float AnimationY(float deltaTime)
        {
            float durationAnimation = 0.5f;
            float durationSound = 0.5f;


            if (timeAnimation > 2 * durationAnimation + durationSound)
            {
                cu = false;
                x = 0;
                timeAnimation = 0;
                cu = false;
                return 0;
            }
            else if (timeAnimation < durationAnimation || timeAnimation > durationAnimation + durationSound)
            {
                //Ку-ку ёпта
                timeAnimation += deltaTime;
                x += deltaTime;
                return 1 - (float)(Math.Cos(6*x) + 1) / 2f;
            }
            timeAnimation += deltaTime;
            return 1;
        }

        private void DoorRender(IRenderAPI rpi, Vec3d camPos, IStandardShaderProgram doorShader,
            MultiTextureMeshRef door, float shift, float angle)
        {
            doorShader.ModelMatrix = modelMat
               .Identity()
               .Translate(pos.X - camPos.X, pos.Y - camPos.Y, pos.Z - camPos.Z)
               .Translate(0.5f, 0.5f + doorDy, 0.5f)
               .RotateY(meshAngle)
               .Translate(shift, 0, -doorDz)
               .RotateY(angle)
               .Translate(-0.5f, -0.5f, -0.5)
               .Values;

            doorShader.ViewMatrix = rpi.CameraMatrixOriginf;
            doorShader.ProjectionMatrix = rpi.CurrentProjectionMatrix;
            rpi.RenderMultiTextureMesh(door, "tex");
        }

        private void DoorRender(IRenderAPI rpi, Vec3d camPos, IStandardShaderProgram doorShader,
           MultiTextureMeshRef door, float shift)
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
            rpi.RenderMultiTextureMesh(door, "tex");
        }

        private void CuckooRender(IRenderAPI rpi, Vec3d camPos, IStandardShaderProgram cuckooShader,
            float translateZ)
        {
            cuckooShader.ModelMatrix = modelMat
              .Identity()
              .Translate(pos.X - camPos.X, pos.Y - camPos.Y, pos.Z - camPos.Z)
              .Translate(0.5f, cuckooDy, 0.5f )
              .RotateY(meshAngle)
              .Translate(-0.5f, -0.5f, -0.5 - cuckooDz + translateZ)
              .Values;

            cuckooShader.ViewMatrix = rpi.CameraMatrixOriginf;
            cuckooShader.ProjectionMatrix = rpi.CurrentProjectionMatrix;
            rpi.RenderMultiTextureMesh(cuckoo, "tex");
        }

        public void Update(MeshData? cuckoo, float cuckooDy, float cuckooDz,
            MeshData? doorR, MeshData? doorL, float doorDx, float doorDy, float doorDz,
            float meshAngle)
        {
            this.meshAngle = meshAngle;

            if (cuckoo != null)
            {
                this.cuckoo = capi.Render.UploadMultiTextureMesh(cuckoo);
                this.cuckooDy = cuckooDy;
                this.cuckooDz = cuckooDz;
            }

            if (doorR != null && doorL != null)
            {
                this.doorR = capi.Render.UploadMultiTextureMesh(doorR);
                this.doorL = capi.Render.UploadMultiTextureMesh(doorL);
                this.doorDx = doorDx;
                this.doorDy = doorDy;
                this.doorDz = doorDz;
            }
        }


        public void Dispose()
        {
            capi.Event.UnregisterRenderer(this, EnumRenderStage.Opaque);
            cuckoo?.Dispose();
            doorR?.Dispose();
            doorL?.Dispose();
        }
    }
}
