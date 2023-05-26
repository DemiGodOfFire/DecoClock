using System;
using Vintagestory.API.Client;
using Vintagestory.API.MathTools;

namespace DecoClock
{
    public class ClockHandRenderer : IRenderer
    {
        internal bool ShouldRender;

        private ICoreClientAPI capi;
        private BlockPos pos;
        float angleRad;

        public MeshRef? hourHand;
        public MeshRef? minuteHand;

        Matrixf ModelMat = new Matrixf();

        public ClockHandRenderer(ICoreClientAPI coreClientAPI, BlockPos pos)
        {
            capi = coreClientAPI;
            this.pos = pos;
        }

        public double RenderOrder => 0.5;
        public int RenderRange => 24;
        int i=0;



        //private void OneMinute(float dt)
        //{
        //    float hourOfDay = api.World.Calendar.HourOfDay;
        //    float hourUpdate = api.World.Calendar.FullHourOfDay / api.World.Calendar.HoursPerDay * 24;


        //    float minutesFloat = hourOfDay - hour;
        //    float minutesUpdate = minutesFloat * 60f;

        //    if (minuteHandRotate = minutes != minutesUpdate)
        //    {
        //        minutes = minutesUpdate;
        //    }
        //    else return;

        //    if (hourHandRotate = hour != hourUpdate)
        //    {
        //        hour = hourUpdate;
        //    }

        //    int hourM12 = (int)hourOfDay % 12;
        //    UpdateHandState();

        //}

        //void UpdateHandState()
        //{
        //    if (Api?.World == null) return;

        //    if (rendererHand != null)
        //    {
        //        rendererHand.AngleRad = MinuteAngle();
        //    }

        //    Api.World.BlockAccessor.MarkBlockDirty(Pos, OnRetesselatedMinuteHand);

        //    //if (nowGrinding)
        //    //{
        //    //    ambientSound?.Start();
        //    //}
        //    //else
        //    //{
        //    //    ambientSound?.Stop();
        //    //}

        //    if (Api.Side == EnumAppSide.Server)
        //    {
        //        MarkDirty();
        //    }

        //}

        //private void OnRetesselatedMinuteHand()
        //{
        //    if (rendererHand == null) return; // Maybe already disposed
        //    rendererHand.ShouldRender = minuteHandRotate;
        //}

        //private float MinuteAngle()
        //{
        //    return 2 * (float)Math.PI * minutes / 60; // check values
        //}


        public void OnRenderFrame(float deltaTime, EnumRenderStage stage)
        {
            //if (meshref == null || !ShouldRender) return;
            if (hourHand == null && minuteHand == null)
            {
                return;
            }
            if (hourHand != null)
            {
                if (i == 360)
                    i = 0;
                IRenderAPI rpi = capi.Render;
                Vec3d camPos = capi.World.Player.Entity.CameraPos;

                rpi.GlDisableCullFace();
                rpi.GlToggleBlend(true);

                IStandardShaderProgram hourHandShader = rpi.PreparedStandardShader(pos.X, pos.Y, pos.Z);
                hourHandShader.Tex2D = capi.BlockTextureAtlas.AtlasTextures[0].TextureId;
                //hourHandShader.ModelMatrix = ModelMat
                //    .Identity()
                //    .Translate(pos.X - camPos.X, pos.Y - camPos.Y, pos.Z - camPos.Z)
                //    .Translate(0.5f, -1.5f, 0.5f)
                //    .RotateZDeg(90)
                //    //.Translate(0f, 0f, -0.61f)
                //    //.RotateY(angleRad)
                //    //.Translate(-0.5f, 0f, 0.11f)
                //    //.Translate(-0.5f, 1.5f, -0.61f)
                //    .Values
                //   ;

                ModelMat.Identity();
                ModelMat.Translate(pos.X - camPos.X, pos.Y - camPos.Y, pos.Z - camPos.Z);
                ModelMat.Translate(0.5f, 1.5f,0.5f);
                ModelMat.RotateY(angleRad);
                ModelMat.RotateZ((float)(-i * Math.PI / 180));

                ModelMat.Translate(0f, 0f, -0.11f);
                hourHandShader.ModelMatrix = ModelMat.Values;

                //.Translate(0.5f, 0f, 0f)

                //.RotateY(angleRad)
                //.Translate(0.5,0f,-0.61)


                //.Rotate(0,angleRad,0)//(float)(45 * Math.PI / 180)
                //.Translate(-0.5f, 1.5, -0.61f)

                //.Translate(0.0f, 0f, -0.11f)

                //.Translate(0.5,1.5f,0.61)

                //.Translate(-0.5f, 0f, -0.5f)
                //.Translate(0.5f, 0f, 0.61f)

                //.Translate(-0.5f, 0f, -0.61f)
                //.Translate(0f,1.5f,0f)

                //      prog.ModelMatrix = ModelMat
                //    .Identity()
                //    .Translate(pos.X - camPos.X, pos.Y - camPos.Y, pos.Z - camPos.Z)
                //    .Translate(-origx, 0, -origz)
                //    .RotateYDeg(blockRotation)
                //    .Translate(discPos.X, discPos.Y, discPos.Z)
                //    .Rotate(discRotRad)
                //    .Scale(0.9f, 0.9f, 0.9f)
                //    .Translate(origx, origy, origz)
                //    .Values
                //;

                hourHandShader.ViewMatrix = rpi.CameraMatrixOriginf;
                hourHandShader.ProjectionMatrix = rpi.CurrentProjectionMatrix;
                rpi.RenderMesh(hourHand);
                hourHandShader.Stop();
                i++;
            }

            //ShouldRender = false;
        }

        public void Update(MeshData? hourHand, MeshData? minuteHand, float angleRad)
        {
            this.angleRad = angleRad;
            this.hourHand?.Dispose();
            this.hourHand = null;

            if (hourHand != null)
            {
                this.hourHand = capi.Render.UploadMesh(hourHand);
            }

            this.minuteHand?.Dispose();
            this.minuteHand = null;

            if (minuteHand != null)
            {
                this.minuteHand = capi.Render.UploadMesh(minuteHand);
            }
        }



        public void Dispose()
        {
            capi.Event.UnregisterRenderer(this, EnumRenderStage.Opaque);

            hourHand?.Dispose();
            minuteHand?.Dispose();
        }

    }
}
