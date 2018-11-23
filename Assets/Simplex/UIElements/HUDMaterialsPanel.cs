using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using g3;

namespace f3
{
    public class HUDMaterialsPanel : HUDCollection
    {

        const float fIndicatorSize = 0.035f;
        const float fIndicatorShift = 0.025f;


        fGameObject indicatorGO;
        DropMaterialButton indicatorButton;
        List<DropMaterialButton> buttons = new List<DropMaterialButton>();


        DropMaterialButton add_material_button(Cockpit cockpit, string sName, 
            float fHUDRadius, float dx, float dy,
            float fButtonRadius,
            Material bgMaterial, SOMaterial material )
        {
            DropMaterialButton button = new DropMaterialButton() {
                TargetScene = cockpit.Scene,
                Material = material
            };
            button.Create(fButtonRadius, bgMaterial);
            HUDUtil.PlaceInSphere(button, fHUDRadius, dx, dy);
            button.Name = sName;

            button.OnClicked += (s, e) => {
                cockpit.Scene.DefaultSOMaterial = material;
                if (indicatorButton != null)
                    indicatorButton.RemoveGO(indicatorGO);
                indicatorButton = button;

                indicatorGO.SetPosition(Vector3f.Zero);
                indicatorGO.SetRotation(Quaternionf.Identity);
                indicatorGO.SetLocalScale(fIndicatorSize * Vector3f.One);
                indicatorGO.SetLocalPosition(
                    indicatorGO.GetLocalPosition() + fIndicatorShift * (Vector3f.AxisY - 4*Vector3f.AxisZ + Vector3f.AxisX));
                indicatorButton.AppendNewGO(indicatorGO, indicatorButton.RootGameObject, false);
            };

            button.OnDoubleClicked += (s, e) => {
                if (cockpit.Scene.Selected.Count > 0) {
                    button.DoSetMaterial(cockpit.Scene.Selected, button.Material);
                } 
            };
            return button;
        }





        public void Create(Cockpit cockpit)
        {
            base.Create();

            float fHUDRadius = 0.75f;
            float fButtonRadius = 0.06f;

            Color bgColor = new Color(0.7f, 0.7f, 1.0f, 0.7f);

            Material bgMaterial = (bgColor.a == 1.0f) ?
                MaterialUtil.CreateStandardMaterial(bgColor) : MaterialUtil.CreateTransparentMaterial(bgColor);

            List<SOMaterial> materials = new List<SOMaterial>() {
                SOMaterial.CreateStandard("default", ColorUtil.StandardBeige),
                SOMaterial.CreateStandard("standard_white", Colorf.VideoWhite),
                SOMaterial.CreateStandard("standard_black", Colorf.VideoBlack),
                SOMaterial.CreateStandard("middle_grey", new Colorf(0.5f) ),

                SOMaterial.CreateStandard("standard_green", Colorf.VideoGreen),
                SOMaterial.CreateStandard("forest_green", Colorf.ForestGreen),
                SOMaterial.CreateStandard("teal", Colorf.Teal),
                SOMaterial.CreateStandard("light_green", Colorf.LightGreen ),

                SOMaterial.CreateStandard("standard_blue", Colorf.VideoBlue),
                SOMaterial.CreateStandard("navy", Colorf.Navy),
                SOMaterial.CreateStandard("cornflower_blue", Colorf.CornflowerBlue ),
                SOMaterial.CreateStandard("light_steel_blue", Colorf.LightSteelBlue),

                SOMaterial.CreateStandard("standard_red", Colorf.VideoRed),
                SOMaterial.CreateStandard("fire_red", Colorf.FireBrick),
                SOMaterial.CreateStandard("hot_pink", Colorf.HotPink ),
                SOMaterial.CreateStandard("light_pink", Colorf.LightPink),


                SOMaterial.CreateStandard("standard_yellow", Colorf.VideoYellow),
                SOMaterial.CreateStandard("standard_orange", Colorf.Orange),
                SOMaterial.CreateStandard("saddle_brown", Colorf.SaddleBrown ),
                SOMaterial.CreateStandard("wheat", Colorf.Wheat ),


                SOMaterial.CreateStandard("standard_cyan", Colorf.VideoCyan),
                SOMaterial.CreateStandard("standard_magenta", Colorf.VideoMagenta),
                SOMaterial.CreateStandard("silver", Colorf.Silver ),
                SOMaterial.CreateStandard("dark_slate_grey", Colorf.DarkSlateGrey)

            };


            float fRight = -41.0f;
            float df = -7.25f;
            float df_fudge = -0.2f;
            List<float> AngleSteps = new List<float>() { fRight, fRight + df, fRight + 2 * df, fRight + 3*df };
            float fVertStep = 6.75f;
            float fTop = 2.0f;

            int ri = 0, ci = 0;
            foreach ( SOMaterial m in materials ) {
                float fXFudge = df_fudge * (float)ri * (float)ci;
                float fX = AngleSteps[ci++] + fXFudge;
                float fY = fTop - (float)ri * fVertStep;
                if ( ci == AngleSteps.Count ) {
                    ci = 0; ri++;
                }

                var button = add_material_button(cockpit, m.Name, fHUDRadius,
                    fX, fY, fButtonRadius, bgMaterial, m);
                AddChild(button);
                buttons.Add(button);
            }


            fMesh iconMesh = null;
            Material iconMaterial = null;
            try {
                iconMesh = new fMesh(Resources.Load<Mesh>("tool_icons/star"));
                iconMaterial = MaterialUtil.CreateStandardVertexColorMaterial(Color.white);
            } catch { }
            if (iconMesh == null) {
                iconMesh = new fMesh(UnityUtil.GetPrimitiveMesh(PrimitiveType.Sphere));
                iconMaterial = MaterialUtil.CreateStandardMaterial(Color.yellow);
            }
            indicatorGO = new fMeshGameObject(iconMesh);
            indicatorGO.SetName( "active_star" );
            indicatorGO.SetMesh(iconMesh);
            indicatorGO.SetMaterial(iconMaterial);
            indicatorGO.SetLocalScale( fIndicatorSize * Vector3f.One );
            indicatorGO.SetLocalPosition( indicatorGO.GetLocalPosition() + 
                fIndicatorShift * (Vector3f.AxisY - 4*Vector3f.AxisZ + Vector3f.AxisX));

            indicatorButton = buttons[0];   // this is default material
            indicatorButton.AppendNewGO(indicatorGO, indicatorButton.RootGameObject, false);


        }




    }
}
