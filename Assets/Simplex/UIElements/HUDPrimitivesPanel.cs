using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using g3;

namespace f3
{
    class HUDPrimitivesPanel : HUDCollection
    {

        const float fIndicatorSize = 0.035f;
        const float fIndicatorShift = 0.045f;


        fGameObject indicatorGO;
        CreateDropPrimitiveButton indicatorButton;
        List<CreateDropPrimitiveButton> buttons = new List<CreateDropPrimitiveButton>();
        Dictionary<CreateDropPrimitiveButton, SOType> buttonTypes = new Dictionary<CreateDropPrimitiveButton, SOType>();


        void update_indicator(CreateDropPrimitiveButton button, FScene scene)
        {
            if ( button == null || buttonTypes.ContainsKey(button) == false ) {
                return;
            }
            SOType t = buttonTypes[button];
            if ( t.hasTag(SOType.TagPrimitive) == false ) {
                return;
            }

            scene.DefaultPrimitiveType = t;

            if (indicatorButton != null)
                indicatorButton.RemoveGO(indicatorGO);
            indicatorButton = button;

            indicatorGO.SetPosition(Vector3f.Zero);
            indicatorGO.SetRotation(Quaternionf.Identity);
            indicatorGO.SetLocalScale(fIndicatorSize * Vector3f.One);
            indicatorGO.SetLocalPosition(
                indicatorGO.GetLocalPosition() + fIndicatorShift * (Vector3f.AxisY - 1*Vector3f.AxisZ + Vector3f.AxisX));

            indicatorButton.AppendNewGO(indicatorGO, indicatorButton.RootGameObject, false);
            indicatorGO.Show();
        }



        class primitiveIconGenerator : IGameObjectGenerator
        {
            public PrimitiveType PrimType { get; set; }
            public Material PrimMaterial { get; set; }
            public float PrimSize { get; set; }
            virtual public List<fGameObject> Generate()
            {
                GameObject primGO = UnityUtil.CreatePrimitiveGO("primitive", PrimType, PrimMaterial, true);
                primGO.transform.localScale = new Vector3(PrimSize, PrimSize, PrimSize);
                primGO.transform.Translate(0.0f, 0.0f, -PrimSize);
                primGO.transform.Rotate(-15.0f, 45.0f, 0.0f, Space.Self);
                return new List<fGameObject>() { primGO };
            }
        }
        class pivotIconGenerator : IGameObjectGenerator
        {
            public Material SphereMaterial { get; set; }
            public Material FrameMaterial { get; set; }
            public float PrimSize { get; set; }
            virtual public List<fGameObject> Generate()
            {
                GameObject primGO = UnityUtil.CreatePrimitiveGO("primitive", PrimitiveType.Sphere, SphereMaterial, true);
                GameObject meshGO = UnityUtil.CreateMeshGO("primitive", "icon_meshes/axis_frame", 1.0f,
                    UnityUtil.MeshAlignOption.NoAlignment, FrameMaterial, false);
                meshGO.transform.SetParent(primGO.transform, true);
                primGO.transform.localScale = new Vector3(PrimSize, PrimSize, PrimSize);
                primGO.transform.Translate(0.0f, 0.0f, -PrimSize);
                primGO.transform.Rotate(15.0f, 135.0f, 0.0f, Space.Self);
                return new List<fGameObject>() { primGO };
            }
        }
        class meshIconGenerator : IGameObjectGenerator
        {
            public string MeshPath { get; set; }
            public Material UseMaterial { get; set; }
            public float UseSize { get; set; }
            virtual public List<fGameObject> Generate()
            {
                GameObject meshGO = UnityUtil.CreateMeshGO("primitive", MeshPath, 1.0f,
                    UnityUtil.MeshAlignOption.AllAxesCentered, UseMaterial, true);
                meshGO.transform.localScale *= UseSize;
                meshGO.transform.Translate(0.0f, 0.0f, -UseSize);
                meshGO.transform.Rotate(-15.0f, 45.0f, 0.0f, Space.Self);
                //return new List<fGameObject>() { meshGO };
                return new List<fGameObject>() { };
            }
        }


        CreateDropPrimitiveButton add_primitive_button(Cockpit cockpit, string sName, float fHUDRadius, float dx, float dy,
            PrimitiveType primType, float fPrimRadiusScale,
            Material bgMaterial, Material primMaterial,
            Func<TransformableSO> CreatePrimitiveF,
            IGameObjectGenerator customGenerator = null
            )
        {
            float fButtonRadius = 0.08f;

            CreateDropPrimitiveButton button = new CreateDropPrimitiveButton() {
                TargetScene = cockpit.Scene,
                CreatePrimitive = CreatePrimitiveF
            };
            button.Create(fButtonRadius, bgMaterial);
            var gen = (customGenerator != null) ? customGenerator :
                new primitiveIconGenerator() { PrimType = primType, PrimMaterial = primMaterial, PrimSize = fButtonRadius * fPrimRadiusScale };
            button.AddVisualElements(gen.Generate(), true);
            HUDUtil.PlaceInSphere(button, fHUDRadius, dx, dy);
            button.Name = sName;
            button.OnClicked += (s, e) => {
                update_indicator(button, cockpit.Scene);
            };
            button.OnDoubleClicked += (s, e) => {
                // [TODO] could have a lighter record here because we can easily recreate primitive...
                cockpit.Scene.History.PushChange(
                    new AddSOChange() { scene = cockpit.Scene, so = CreatePrimitiveF() });
                cockpit.Scene.History.PushInteractionCheckpoint();
            };
            return button;
        }



        CreateDropPrimitiveButton add_curve_button(Cockpit cockpit, string sName, float fHUDRadius, float dx, float dy,
            Material bgMaterial, Material primMaterial,
            Func<TransformableSO> CreateCurveF
            )
        {
            float fButtonRadius = 0.08f;

            CreateDropPrimitiveButton button = new CreateDropPrimitiveButton() {
                TargetScene = cockpit.Scene,
                CreatePrimitive = CreateCurveF
            };
            button.Create(fButtonRadius, bgMaterial);
            //var gen = (customGenerator != null) ? customGenerator :
            //    new primitiveIconGenerator() { PrimType = primType, PrimMaterial = primMaterial, PrimSize = fButtonRadius * fPrimRadiusScale };
            //button.AddVisualElements(gen.Generate(), true);
            HUDUtil.PlaceInSphere(button, fHUDRadius, dx, dy);
            button.Name = sName;
            button.OnDoubleClicked += (s, e) => {
                // [TODO] could have a lighter record here because we can easily recreate primitive...
                cockpit.Scene.History.PushChange(
                    new AddSOChange() { scene = cockpit.Scene, so = CreateCurveF() });
                cockpit.Scene.History.PushInteractionCheckpoint();
            };
            return button;
        }


        public void Create(Cockpit cockpit)
        {
            base.Create();

            float fHUDRadius = 0.7f;
            float fButtonRadius = 0.08f;
            Color bgColor = new Color(0.7f, 0.7f, 1.0f, 0.7f);
            Material bgMaterial = (bgColor.a == 1.0f) ?
                MaterialUtil.CreateStandardMaterial(bgColor) : MaterialUtil.CreateTransparentMaterial(bgColor);
            Material primMaterial = MaterialUtil.CreateStandardMaterial(Color.yellow);


            var addCylinderButton = add_primitive_button(cockpit, "addCylinder", fHUDRadius, -45.0f, 0.0f,
                PrimitiveType.Cylinder, 0.7f, bgMaterial, primMaterial,
                () => {
                    return new CylinderSO().Create(cockpit.Scene.DefaultSOMaterial);
                });
            AddChild(addCylinderButton);
            buttons.Add(addCylinderButton);
            buttonTypes[addCylinderButton] = SOTypes.Cylinder;


            var addBoxButton = add_primitive_button(cockpit, "addBox", fHUDRadius, -45.0f, -15.0f,
                PrimitiveType.Cube, 0.7f, bgMaterial, primMaterial,
                () => {
                    return new BoxSO().Create(cockpit.Scene.DefaultSOMaterial);
                });
            AddChild(addBoxButton);
            buttons.Add(addBoxButton);
            buttonTypes[addBoxButton] = SOTypes.Box;


            var addSphereButton = add_primitive_button(cockpit, "addSphere", fHUDRadius, -45.0f, -30.0f,
                PrimitiveType.Sphere, 0.85f, bgMaterial, primMaterial,
                () => {
                    return new SphereSO().Create(cockpit.Scene.DefaultSOMaterial);
                });
            AddChild(addSphereButton);
            buttons.Add(addSphereButton);
            buttonTypes[addSphereButton] = SOTypes.Sphere;


            var addPivotButton = add_primitive_button(cockpit, "addPivot", fHUDRadius, -60.0f, 0.0f,
                PrimitiveType.Sphere, 0.7f, bgMaterial, primMaterial,
                () => {
                    return new PivotSO().Create(cockpit.Scene.PivotSOMaterial, cockpit.Scene.FrameSOMaterial,
                        FPlatform.WidgetOverlayLayer);
                },
                new pivotIconGenerator() {
                    SphereMaterial = cockpit.Scene.SelectedMaterial,
                    FrameMaterial = cockpit.Scene.FrameMaterial, PrimSize = fButtonRadius * 0.7f
                });
            AddChild(addPivotButton);
            buttons.Add(addPivotButton);
            buttonTypes[addPivotButton] = SOTypes.Pivot;


            //var addCurveButton = add_curve_button(cockpit, "addCurve", fHUDRadius, -60.0f, -15.0f,
            //    bgMaterial, primMaterial,
            //    () => {
            //        return new PolyCurveSO().Create(cockpit.ActiveScene.DefaultSOMaterial);
            //    });
            //AddChild(addCurveButton);


            /*
                        HUDButton addBunnyButton = HUDBuilder.CreateGeometryIconClickButton(
                            new HUDShape(HUDShapeType.Disc, fButtonRadius ),
                            fHUDRadius, -45.0f, -30.0f, bgColor,
                            new meshIconGenerator() { MeshPath = "icon_meshes/bunny_1k", UseMaterial = primMaterial, UseSize = fButtonRadius * 0.7f });
                        addBunnyButton.Name = "addBunnyButton";
                        addBunnyButton.OnClicked += (s, e) => {
                            // TODO add existing mesh to scene
                            cockpit.Parent.Scene.AddBox();
                        };
                        cockpit.AddUIElement(addBunnyButton, true);
            */



            // initialize selected-primitive indicator icon
            fMesh iconMesh = null;
            fMaterial iconMaterial = null;
            try {
                iconMesh = new fMesh(Resources.Load<Mesh>("tool_icons/star"));
                iconMaterial = MaterialUtil.CreateStandardVertexColorMaterialF(Color.white);
            } catch { }
            if (iconMesh == null) {
                iconMesh = new fMesh(UnityUtil.GetPrimitiveMesh(PrimitiveType.Sphere));
                iconMaterial = MaterialUtil.CreateStandardMaterialF(Color.yellow);
            }
            indicatorGO = new fMeshGameObject(iconMesh);
            indicatorGO.SetName( "active_star" );
            indicatorGO.SetMesh(iconMesh);
            indicatorGO.SetMaterial(iconMaterial);
            indicatorGO.SetLocalScale( fIndicatorSize * Vector3f.One );
            indicatorGO.SetLocalPosition( indicatorGO.GetLocalPosition() + 
                fIndicatorShift * (Vector3f.AxisY - 1*Vector3f.AxisZ + Vector3f.AxisX));

            indicatorButton = buttons[0];   // this is default material
            indicatorButton.AppendNewGO(indicatorGO, indicatorButton.RootGameObject, false);

        }


    }
}
