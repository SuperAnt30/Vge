using Vge.Entity.Player;
using Vge.Renderer.Mesh;
using Vge.Renderer.World;
using WinGL.OpenGL;
using WinGL.Util;

namespace Mvk2.Renderer.World
{
    /// <summary>
    /// Объект отвечает за прорисовку неба для мира с островом
    /// </summary>
    public class SkyIslandRender : SkyRender
    {
        
        public SkyIslandRender(PlayerClientOwner player, WorldRenderer worldRenderer) 
            : base(player, worldRenderer)
        {
            

            

            //float f1 = -38.4F;
            //float f2 = 44.8F;
            //float fy = 24;
            //Vector3 colorSky = _worldRenderer.ColorSky;
            //float r = colorSky.X;
            //float g = colorSky.Y;
            //float b = colorSky.Z;

            //r = 1; g = 1; b = 1;

            //float[] buffer = new float[] {
            //    f1, fy, f1, r, g, b, 1,
            //    f1, fy, f2, r, g, b, 1,

            //    f1, fy, f2, r, g, b, 1,
            //    f2, fy, f2, r, g, b, 1,

            //    f2, fy, f2, r, g, b, 1,
            //    f2, fy, f1, r, g, b, 1,

            //    f2, fy, f1, r, g, b, 1,
            //    f1, fy, f1, r, g, b, 1
            //};
            ////_mesh = new MeshParticle(gl, buffer);

            //_meshLine = new MeshLine(gl, GL.GL_DYNAMIC_DRAW);
            //_meshLine.Reload(buffer);


            //buffer = new float[] {
            //    f1, fy, f1,
            //    f2, fy, f1,

            //    f2, fy, f1,
            //    f2, fy, f2,

            //    f2, fy, f2,
            //    f1, fy, f2,

            //    f1, fy, f2,
            //    f1, fy, f1
            //};

            //_mesh = new MeshSky(gl, buffer);
            
        }

        /// <summary>
        /// Прорисовка неба
        /// </summary>
        //public override void DrawSky(float timeIndex)
        //{
        //    _worldRenderer.Render.DepthOff();

        //    _worldRenderer.Render.ShaderBindSky(Gi.MatrixView, _worldRenderer.ColorSky);
        //    _mesh.Draw();

        //    _worldRenderer.Render.ShaderBindLine(Gi.MatrixView, 0, 0, 0);
        //    _meshLine.Draw();

        //    _worldRenderer.Render.DepthOn();
        //}

        //public override void Dispose()
        //{
        //    _mesh.Dispose();
        //    _meshLine.Dispose();
        //}
    }
}
