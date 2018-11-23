using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using g3;
using gs;

namespace f3
{
    class HUDToolsPanel : HUDCollection
    {
        const float fIndicatorSize = 0.035f;
        const float fIndicatorShiftXY = 0.045f;
        const float fIndicatorShiftZ = 0.04f;

        Cockpit cockpit;
        List<ActivateToolButton> toolButtons;

        fMaterial defaultMaterial;
        fMaterial activeMaterial;


        GameObject[] indicator = { null, null };
        ActivateToolButton[] indicatorButton = {null, null};
        void remove_indicator(int nSide)
        {
            if (indicatorButton[nSide] != null) {
                indicatorButton[nSide].RemoveGO(indicator[nSide]);
                indicatorButton[nSide].StandardMaterial = defaultMaterial;
                indicatorButton[nSide] = null;
                indicator[nSide].transform.position = Vector3.zero;
                indicator[nSide].transform.rotation = Quaternion.identity;
                indicator[nSide].Hide();
            }
        }
        void add_indicator(ActivateToolButton button, int nSide)
        {
            indicatorButton[nSide] = button;
            indicatorButton[nSide].StandardMaterial = activeMaterial;
            indicator[nSide].transform.position = Vector3.zero;
            indicator[nSide].transform.rotation = Quaternion.identity;
            indicator[nSide].transform.localScale = fIndicatorSize * Vector3.one;
            indicator[nSide].transform.localPosition +=
                fIndicatorShiftXY * (Vector3.up + Vector3.right * ((nSide == 0) ? -1 : 1)) - fIndicatorShiftZ * Vector3.forward;
            indicatorButton[nSide].AppendNewGO(indicator[nSide], button.RootGameObject, false);
            indicator[nSide].Show();
        }


        struct toolInfo
        {
            public ActivateToolButton button;
            public string identifier;
            public string sMeshPath;
            public float fMeshScaleFudge;
        }
        List<toolInfo> toolButtonInfo;


        ActivateToolButton add_tool_button(Cockpit cockpit, string sName,
            float fHUDRadius, float dx, float dy,
            float fButtonRadius, toolInfo info)
        {
            ActivateToolButton button = new ActivateToolButton() {
                TargetScene = cockpit.Scene,
                ToolType = info.identifier
            };
            button.CreateMeshIconButton(fButtonRadius, info.sMeshPath, defaultMaterial, info.fMeshScaleFudge);
            HUDUtil.PlaceInSphere(button, fHUDRadius, dx, dy);
            button.Name = sName;

            if (info.identifier == "cancel") {
                button.OnClicked += (s, e) => {
                    bool bIsSpatial = InputState.IsDevice(e.device, InputDevice.AnySpatialDevice);
                    int nSide = bIsSpatial ? (int)e.side : 1;
                    cockpit.Context.ToolManager.DeactivateTool((ToolSide)nSide);

                    // remove icon from hand
                    if (bIsSpatial) 
                        cockpit.Context.SpatialController.ClearHandIcon(nSide);

                    // hide indicator
                    remove_indicator(nSide);

                };
            } else {
                button.OnClicked += (s, e) => {
                    bool bIsSpatial = InputState.IsDevice(e.device, InputDevice.AnySpatialDevice);
                    int nSide = bIsSpatial ? (int)e.side : 1;
                    remove_indicator(nSide);

                    cockpit.Context.ToolManager.SetActiveToolType(info.identifier, (ToolSide)nSide);
                    if ( cockpit.Context.ToolManager.ActivateTool((ToolSide)nSide) ) {

                        if ( bIsSpatial ) {
                            Mesh iconmesh = Resources.Load<Mesh>(info.sMeshPath);
                            if (iconmesh != null)
                                cockpit.Context.SpatialController.SetHandIcon(iconmesh, nSide);
                        }

                        add_indicator(button, nSide);
                    }
                };
            }
            return button;
        }




        void on_tool_changed(ITool tool, ToolSide eSide, bool bActivated)
        {
            if ( bActivated == false ) {
                try {
                    //toolInfo ti = toolButtonInfo.Find((x) => x.identifier == tool.TypeIdentifier);
                    remove_indicator((int)eSide);
                    if ( (this.cockpit.Context.ActiveInputDevice & InputDevice.AnySpatialDevice) != 0  )
                        cockpit.Context.SpatialController.ClearHandIcon((int)eSide);
                } catch { }
            }
        }




        public void Create(Cockpit cockpit)
        {
            base.Create();
            this.cockpit = cockpit;

            float fHUDRadius = 0.7f;
            float fButtonRadius = 0.06f;

            Colorf bgColor = new Color(0.7f, 0.7f, 1.0f, 0.7f);
            Colorf activeColor = new Colorf(ColorUtil.SelectionGold, 0.7f);

            defaultMaterial = (bgColor.a == 1.0f) ?
                MaterialUtil.CreateStandardMaterialF(bgColor) : MaterialUtil.CreateTransparentMaterialF(bgColor);
            activeMaterial = (activeColor.a == 1.0f) ?
                MaterialUtil.CreateStandardMaterialF(activeColor) : MaterialUtil.CreateTransparentMaterialF(activeColor);

            List<toolInfo> toolNames = new List<toolInfo>() {
                new toolInfo() { identifier = "cancel", sMeshPath = "tool_icons/cancel", fMeshScaleFudge = 1.2f },
                new toolInfo() { identifier = SnapDrawPrimitivesTool.Identifier, sMeshPath = "tool_icons/draw_primitive", fMeshScaleFudge = 1.2f },
                new toolInfo() { identifier = DrawTubeTool.Identifier, sMeshPath = "tool_icons/draw_tube", fMeshScaleFudge = 1.2f },
                new toolInfo() { identifier = DrawCurveTool.Identifier, sMeshPath = "tool_icons/draw_curve", fMeshScaleFudge = 1.2f },
                new toolInfo() { identifier = DrawSurfaceCurveTool.Identifier, sMeshPath = "tool_icons/draw_surface_curve", fMeshScaleFudge = 1.2f },
                new toolInfo() { identifier = RevolveTool.Identifier, sMeshPath = "tool_icons/revolve", fMeshScaleFudge = 1.2f },
                new toolInfo() { identifier = TwoPointMeasureTool.Identifier, sMeshPath = "tool_icons/measure", fMeshScaleFudge = 1.2f },
                new toolInfo() { identifier = SculptCurveTool.Identifier, sMeshPath = "tool_icons/sculpt_curve", fMeshScaleFudge = 1.2f },
                new toolInfo() { identifier = PlaneCutTool.Identifier, sMeshPath = "tool_icons/plane_cut", fMeshScaleFudge = 1.2f }
            };


            float fRight = -43.0f;
            float df = -11.0f;
            List<float> AngleSteps = new List<float>() { fRight, fRight + df, fRight + 2 * df  };
            float fVertStep = 11.0f;
            float fTop = 0.0f;

            int ri = 0, ci = 0;
            toolButtons = new List<ActivateToolButton>();
            toolButtonInfo = new List<toolInfo>();
            foreach (toolInfo t in toolNames) { 
                float fX = AngleSteps[ci++];
                float fY = fTop - (float)ri * fVertStep;
                if (ci == AngleSteps.Count) {
                    ci = 0; ri++;
                }

                var button = add_tool_button(cockpit, t.identifier, fHUDRadius,
                    fX, fY, fButtonRadius, t);
                AddChild(button);
                toolButtons.Add(button);

                toolInfo ti = new toolInfo();
                ti = t;
                ti.button = button;
                toolButtonInfo.Add(ti);
            }


            // build indicators
            string[] paths = { "tool_icons/star_green", "tool_icons/star_red" };
            for (int k = 0; k < 2; ++k) {
                Mesh iconMesh = null;
                Material iconMaterial = null;
                try {
                    iconMesh = Resources.Load<Mesh>(paths[k]);
                    iconMaterial = MaterialUtil.CreateStandardVertexColorMaterial(Color.white);
                } catch { }
                if (iconMesh == null) {
                    iconMesh = UnityUtil.GetPrimitiveMesh(PrimitiveType.Sphere);
                    iconMaterial = MaterialUtil.CreateStandardMaterial((k == 0) ? Color.green : Color.red);
                }
                indicator[k] = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                indicator[k].name = (k == 0) ? "left_tool_star" : "right_tool_star";
                Component.Destroy(indicator[k].GetComponent<Collider>());
                indicator[k].SetMesh(iconMesh);
                indicator[k].GetComponent<Renderer>().material = iconMaterial;


                // have to add to some button because we need them to be in GO tree
                //  when we do AddChild() on cockpit...that sets up layer, size, etc
                indicatorButton[k] = toolButtons[0];
                indicator[k].transform.localScale = fIndicatorSize * Vector3.one;
                indicator[k].transform.localPosition +=
                    fIndicatorShiftXY*(Vector3.up + Vector3.right * ((k == 0) ? -1 : 1)) - fIndicatorShiftZ*Vector3.forward;
                indicatorButton[k].AppendNewGO(indicator[k], indicatorButton[k].RootGameObject, false);
                indicator[k].Hide();
            }


            // listen for changes
            cockpit.Context.ToolManager.OnToolActivationChanged += on_tool_changed;

        }


    }
}
