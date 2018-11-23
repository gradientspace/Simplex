using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using g3;
using f3;
using gs;

namespace simplex
{
    public static class RMSTest
    {
        public static void TestInflate()
        {
            Polygon2d poly = Polygon2d.MakeCircle(10.0f, 32);

            MeshInflater inflater = new MeshInflater(poly) {
                TargetEdgeLength = 1.0f
            };

            inflater.Compute();


            DebugUtil.WriteDebugMesh(inflater.ResultMesh, "c:\\scratch\\inflated.obj");
        }

    }
}
