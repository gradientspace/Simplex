using System;
using System.Collections.Generic;
using System.Xml;
using System.Linq;
using UnityEngine;
using g3;
using gs;

namespace f3
{
	//
	// Cockpit object will create an instance of this and call Initialize once.
	// You can use this to configure your HUD, if you want. 
	//
	public class SetupCADCockpit_V1 : ICockpitInitializer
    {
		public SetupCADCockpit_V1 ()
		{
		}

        float fDiscButtonRadius1 = 0.05f;
        float fCockpitRadiusButton = 0.7f;
        float fCockpitRadiusPanel = 0.75f;


        void AddUndoRedo(Cockpit cockpit)
        {
            HUDButton undoButton = HUDBuilder.CreateIconClickButton(
                new HUDShape(HUDShapeType.Disc, fDiscButtonRadius1) { Slices = 32 },
                fCockpitRadiusButton, -6.5f, -50.0f, "icons/undo_v1");
            //UnityUtil.TranslateInFrame(undoButton.RootGameObject, -fDiscButtonRadius1*1.1f, 0, 0, CoordSpace.WorldCoords);
            undoButton.Name = "undo";
            undoButton.OnClicked += (o, e) => { cockpit.Scene.History.InteractiveStepBack(); };
            cockpit.AddUIElement(undoButton, true);

            HUDButton redoButton = HUDBuilder.CreateIconClickButton(
                new HUDShape(HUDShapeType.Disc, fDiscButtonRadius1) { Slices = 32 },
                fCockpitRadiusButton, 6.5f, -50.0f, "icons/redo_v1");
            //UnityUtil.TranslateInFrame(redoButton.RootGameObject, fDiscButtonRadius1*1.1f, 0, 0, CoordSpace.WorldCoords);
            redoButton.Name = "redo";
            redoButton.OnClicked += (o, e) => { cockpit.Scene.History.InteractiveStepForward(); };
            cockpit.AddUIElement(redoButton, true);
        }




        void AddTransformToolToggleGroup(Cockpit cockpit)
        {
            float va = -42.0f;
            float vb = -32.0f;
            float vbdelta = 12.0f;

            HUDToggleButton widgetButton = HUDBuilder.CreateToggleButton(
                fDiscButtonRadius1, fCockpitRadiusButton, vb-vbdelta, va,
                "cockpit_icons/transform_modes/transformmode_widget_enabled_v1", 
                "cockpit_icons/transform_modes/transformmode_widget_disabled_v1");
            widgetButton.Name = "transformWidgetToggle";
            cockpit.AddUIElement(widgetButton, true);

            HUDToggleButton snapButton = HUDBuilder.CreateToggleButton(
                fDiscButtonRadius1, fCockpitRadiusButton, vb, va,
                "cockpit_icons/transform_modes/transformmode_snap_enabled_v1", 
                "cockpit_icons/transform_modes/transformmode_snap_disabled_v1");
            snapButton.Name = "transformSnapToggle";
            cockpit.AddUIElement(snapButton, true);

            HUDToggleButton editButton = HUDBuilder.CreateToggleButton(
                fDiscButtonRadius1, fCockpitRadiusButton, vb+vbdelta, va,
                "cockpit_icons/transform_modes/transformmode_edit_enabled_v1", 
                "cockpit_icons/transform_modes/transformmode_edit_disabled_v1");
            editButton.Name = "transformEditToggle";
            cockpit.AddUIElement(editButton, true);


            HUDToggleGroup group = new HUDToggleGroup();
            group.AddButton(widgetButton);
            group.AddButton(snapButton);
            group.AddButton(editButton);
            group.Selected = 0;
            group.OnToggled += (sender, nSelected) => {
                if (nSelected == 1) cockpit.Context.TransformManager.SetActiveGizmoType("snap_drag");
                else if ( nSelected == 2 ) cockpit.Context.TransformManager.SetActiveGizmoType("object_edit");
                else cockpit.Context.TransformManager.SetActiveGizmoType("default");
            };
            // [TODO] remember last setting? per object?
        }




        // creates world/local frame toggle buttons, that are grouped so clicking
        // one disables the other. Connect up to TransformManager for events/etc.
        void AddFrameToggleGroup(Cockpit cockpit)
        {
            HUDToggleButton worldButton = HUDBuilder.CreateToggleButton(
                fDiscButtonRadius1, fCockpitRadiusButton, -38.0f, -50.0f, 
                "icons/frame_world_enabled", "icons/frame_world_disabled",
                new IconMeshGenerator() { Path = "icon_meshes/axis_frame",
                    Scale = 0.08f, Translate = new Vector3(0.06f, -0.06f, 0.05f), Color = ColorUtil.ForestGreen }
                );
            worldButton.Name = "worldFrameToggle";
            cockpit.AddUIElement(worldButton, true);

            HUDToggleButton localButton = HUDBuilder.CreateToggleButton(
                fDiscButtonRadius1, fCockpitRadiusButton, -25.0f, -50.0f, "icons/frame_local_enabled", "icons/frame_local_disabled");
            localButton.Name = "localFrameToggle";
            cockpit.AddUIElement(localButton, true);

            HUDToggleGroup group = new HUDToggleGroup();
            group.AddButton(worldButton);
            group.AddButton(localButton);
            group.Selected = 1;
            group.OnToggled += (sender, nSelected) => {
                FrameType eSetType = (nSelected == 0) ?
                    FrameType.WorldFrame : FrameType.LocalFrame;
                cockpit.Context.TransformManager.ActiveFrameType = eSetType;
            };
            cockpit.Context.TransformManager.OnActiveGizmoModified += (s, e) => {
                FrameType eSet = cockpit.Context.TransformManager.ActiveFrameType;
                group.Selected = (eSet == FrameType.WorldFrame) ? 0 : 1;
            };

            // set initial state
            FrameType eInitial = cockpit.Context.TransformManager.ActiveFrameType;
            group.Selected = (eInitial == FrameType.WorldFrame) ? 0 : 1;
        }




        // creates navigation mode toggle group
        void AddNavModeToggleGroup(Cockpit cockpit)
        {
            float h = 25.0f;
            float dh = 13.0f;
            float v = -50.0f;
            float dv = 13.0f;

            HUDToggleButton tumbleButton = HUDBuilder.CreateToggleButton(
                fDiscButtonRadius1, fCockpitRadiusButton, h, v,
                "cockpit_icons/view_controls/bunny_enabled", "cockpit_icons/view_controls/bunny_disabled",
                new IconMeshGenerator() { Path = "icon_meshes/camera",
                    Scale = 0.07f, Color = ColorUtil.make(30,30,30),
                    Translate = new Vector3(0.04f, -0.07f, 0.04f),
                    Rotate = Quaternion.AngleAxis(30.0f, Vector3.up) * Quaternion.AngleAxis(-90.0f, Vector3.right) }
                );
            tumbleButton.Name = "tumbleNavMode";
            cockpit.AddUIElement(tumbleButton, true);

            HUDToggleButton flyButton = HUDBuilder.CreateToggleButton(
                fDiscButtonRadius1, fCockpitRadiusButton, h+dh, v, 
                "cockpit_icons/view_controls/sponza_enabled", "cockpit_icons/view_controls/sponza_disabled");
            flyButton.Name = "flyNavMode";
            cockpit.AddUIElement(flyButton, true);

            HUDToggleGroup group = new HUDToggleGroup();
            group.AddButton(tumbleButton);
            group.AddButton(flyButton);
            group.Selected = 1;
            group.OnToggled += (sender, nSelected) => {
                if (nSelected == 0)
                    cockpit.Context.MouseCameraController = new MayaCameraHotkeys();
                else
                    cockpit.Context.MouseCameraController = new RateControlledEgocentricCamera();
            };

            // set initial state
            group.Selected = 0;


            HUDButton resetButton = HUDBuilder.CreateRectIconClickButton(
                0.1f, 0.05f, 0.7f, h+0.3f*dh, v-0.7f*dv,
                 "icons/reset_v1");
            resetButton.Name = "export";
            cockpit.AddUIElement(resetButton, true);
            resetButton.OnClicked += (s, e) => {
                cockpit.Context.ResetView(true);
            };


        }



        void AddMenuButtonsGroup(Cockpit cockpit)
        {
            HUDButton importButton = HUDBuilder.CreateRectIconClickButton(
                0.1f, 0.05f, 0.7f, 55.0f, -50.0f,
                //0.1f, 0.05f, 0.7f, 5.0f, -5.0f,       // for debugging
                 "icons/import_v1");
            importButton.Name = "import";
            cockpit.AddUIElement(importButton, true);
            importButton.OnClicked += (s, e) => {
                var cp = new FileImportCockpit() {
                    InitialPath = SceneGraphConfig.LastFileOpenPath
                };
                cockpit.Context.PushCockpit(cp);
            };

            HUDButton exportButton = HUDBuilder.CreateRectIconClickButton(
                0.1f, 0.05f,   0.7f, 55.0f, -50.0f,
                //0.1f, 0.05f, 0.7f, 5.0f, -5.0f,       // for debugging convenience
                 "icons/export_v1");
            UnityUtil.TranslateInFrame(exportButton.RootGameObject, 0, -0.055f, 0, CoordSpace.WorldCoords);
            exportButton.Name = "export";
            cockpit.AddUIElement(exportButton, true);
            exportButton.OnClicked += (s, e) => {
                var cp = new FileExportCockpit() {
                    InitialPath = SceneGraphConfig.LastFileOpenPath
                };
                cockpit.Context.PushCockpit(cp);
            };


            HUDButton janusButton = HUDBuilder.CreateRectIconClickButton(
                0.1f, 0.05f,   0.7f, 55.0f, -50.0f,
                //0.1f, 0.05f, 0.7f, 5.0f, -5.0f,       // for debugging convenience
                 "cockpit_icons/menu_buttons/janus_v1");
            UnityUtil.TranslateInFrame(janusButton.RootGameObject, 0, 2.0f*-0.055f, 0, CoordSpace.WorldCoords);
            janusButton.Name = "export";
            cockpit.AddUIElement(janusButton, true);
            janusButton.OnClicked += (s, e) => {
                var cp = new JanusVRExportCockpit() {
                    InitialPath = SceneGraphConfig.LastFileOpenPath
                };
                cockpit.Context.PushCockpit(cp);
            };


            HUDButton loadButton = HUDBuilder.CreateRectIconClickButton(
                0.1f, 0.05f, 0.7f, 55.0f, -50.0f,
                // 0.1f, 0.05f, 0.7f, 5.0f, -5.0f,       // for debugging convenience
                 "icons/load_v1");
            UnityUtil.TranslateInFrame(loadButton.RootGameObject, 0.12f, 0, 0, CoordSpace.WorldCoords);
            loadButton.Name = "load";
            cockpit.AddUIElement(loadButton, true);
            loadButton.OnClicked += (s, e) => {
                var cp = new FileLoadSceneCockpit() {
                    InitialPath = SceneGraphConfig.LastFileOpenPath
                };
                cockpit.Context.PushCockpit(cp);
            };

            HUDButton saveButton = HUDBuilder.CreateRectIconClickButton(
                0.1f, 0.05f, 0.7f, 55.0f, -50.0f,
                // 0.1f, 0.05f, 0.7f, 5.0f, -5.0f,       // for debugging
                 "icons/save_v1");
            UnityUtil.TranslateInFrame(saveButton.RootGameObject, 0.12f, -0.055f, 0, CoordSpace.WorldCoords);
            saveButton.Name = "save";
            cockpit.AddUIElement(saveButton, true);
            saveButton.OnClicked += (s, e) => {
                var cp = new FileSaveSceneCockpit() {
                    InitialPath = SceneGraphConfig.LastFileOpenPath
                };
                cockpit.Context.PushCockpit(cp);
            };



            HUDButton newButton = HUDBuilder.CreateRectIconClickButton(
                0.1f, 0.05f, 0.7f, 55.0f, -50.0f,
                 "icons/new_v1");
            UnityUtil.TranslateInFrame(newButton.RootGameObject, 0.24f, 0, 0, CoordSpace.WorldCoords);
            newButton.Name = "new";
            cockpit.AddUIElement(newButton, true);
            newButton.OnClicked += (s, e) => {
                cockpit.Context.NewScene(true);
            };


            HUDButton quitButton = HUDBuilder.CreateRectIconClickButton(
                0.1f, 0.05f, 0.7f, 55.0f, -50.0f,
                 "icons/quit_v1");
            UnityUtil.TranslateInFrame(quitButton.RootGameObject, 0.24f, -0.055f, 0, CoordSpace.WorldCoords);
            quitButton.Name = "quit";
            cockpit.AddUIElement(quitButton, true);
            quitButton.OnClicked += (s, e) => { FPlatform.QuitApplication(); };
        }






        HUDToggleGroup BuildPanels(Cockpit cockpit)
        {
            float vx = -50.0f;
            float vy = 10.0f;

            float fButtonW = fDiscButtonRadius1;


            HUDPrimitivesPanel primPanel = new HUDPrimitivesPanel();
            primPanel.Create(cockpit);
            primPanel.Name = "PrimitivesPanel";
            cockpit.AddUIElement(primPanel, true);

            HUDToolsPanel toolPanel = new HUDToolsPanel();
            toolPanel.Create(cockpit);
            toolPanel.Name = "ToolPanel";
            cockpit.AddUIElement(toolPanel, true);
            toolPanel.IsVisible = false;

            HUDMaterialsPanel matPanel = new HUDMaterialsPanel();
            matPanel.Create(cockpit);
            matPanel.Name = "MaterialPanel";
            cockpit.AddUIElement(matPanel, true);
            matPanel.IsVisible = false;

            float fIconAspect = 1.25f;

            HUDToggleButton materialsButton = HUDBuilder.CreateToggleButton(
                new HUDShape(HUDShapeType.Rectangle, fButtonW, fButtonW / fIconAspect) { UseUVSubRegion = true },
                fCockpitRadiusButton, vx, vy,
                "panel_tabs/materials_active_v1", "panel_tabs/materials_inactive_v1");
            UnityUtil.TranslateInFrame(materialsButton.RootGameObject, -fButtonW * 1.5f, 0, 0, CoordSpace.WorldCoords);
            materialsButton.Name = "toggleMaterials";
            cockpit.AddUIElement(materialsButton, true);

            HUDToggleButton toolsButton = HUDBuilder.CreateToggleButton(
                new HUDShape(HUDShapeType.Rectangle, fButtonW, fButtonW / fIconAspect) { UseUVSubRegion = true },
                fCockpitRadiusButton, vx, vy,
                "panel_tabs/tools_active_v1", "panel_tabs/tools_inactive_v1");
            UnityUtil.TranslateInFrame(toolsButton.RootGameObject, -fButtonW * 0.5f, 0, 0, CoordSpace.WorldCoords);
            toolsButton.Name = "toggleTools";
            cockpit.AddUIElement(toolsButton, true);


            HUDToggleButton shapesButton = HUDBuilder.CreateToggleButton(
                new HUDShape(HUDShapeType.Rectangle, fButtonW, fButtonW / fIconAspect) { UseUVSubRegion = true },
                fCockpitRadiusButton, vx, vy,
                "panel_tabs/shapes_active_v1", "panel_tabs/shapes_inactive_v1");
            UnityUtil.TranslateInFrame(shapesButton.RootGameObject, fButtonW * 0.5f, 0, 0, CoordSpace.WorldCoords);
            shapesButton.Name = "toggleShapes";
            cockpit.AddUIElement(shapesButton, true);

            HUDToggleGroup group = new HUDToggleGroup();
            group.AddButton(shapesButton);
            group.AddButton(toolsButton);
            group.AddButton(materialsButton);
            group.Selected = 0;
            group.OnToggled += (sender, nSelected) => {
                if (nSelected == 0) {
                    matPanel.IsVisible = toolPanel.IsVisible = false;
                    primPanel.IsVisible = true;
                } else if (nSelected == 1 ){
                    primPanel.IsVisible = matPanel.IsVisible = false;
                    toolPanel.IsVisible = true;
                } else {
                    primPanel.IsVisible = toolPanel.IsVisible = false;
                    matPanel.IsVisible = true;
                }
            };

            return group;
        }





        void BuildParameterPanel(Cockpit cockpit)
        {
            float panel_width = 500 * cockpit.GetPixelScale();
            HUDParametersPanel paramPanel = new HUDParametersPanel(cockpit) {
                Width = panel_width,
                Height = panel_width,
                ParameterRowHeight = panel_width / 10.0f,
                ParametersPadding = panel_width * 0.025f,
            };
            paramPanel.Create();
            paramPanel.Name = "ParametersPanel";
            cockpit.AddUIElement(paramPanel, true);
            paramPanel.IsVisible = true;

            cockpit.DefaultLayout.Add(paramPanel, new LayoutOptions() { Flags = LayoutFlags.None,
                PinSourcePoint2D = LayoutUtil.BoxPointF(paramPanel, BoxPosition.TopLeft),
                PinTargetPoint2D = LayoutUtil.BoxPointF(cockpit.DefaultLayout.BoxElement, BoxPosition.TopRight)
                //PinTargetPoint2D = LayoutUtil.BoxPointInterpF(cockpit.DefaultLayout.BoxElement, BoxPosition.TopRight, BoxPosition.BottomRight, 0.5f)
            });

            //UnityUtil.TranslateInFrame(shapesButton.RootGameObject, fButtonW * 0.5f, 0, 0, CoordSpace.WorldCoords);
        }




        public void RegisterMessageHandlers(Cockpit cockpit)
        {
            MessageStream.Get.RegisterHandler(new DelegateMessageHandler(
                m => { return cockpit.IsActive && m.Code == FileBrowserMessageCodes.MESHES_TOO_LARGE; },
                m => { HUDUtil.ShowToastPopupMessage("Sorry, this mesh is too large!", cockpit);
                       return MessageHandlerResult.MessageHandled_Remove; }
            ));
                    
        }




        public void Initialize(Cockpit cockpit) 
		{
            cockpit.Name = "cadSceneCockpit";

            // configure tracking
            SmoothCockpitTracker.Enable(cockpit);
            cockpit.TiltAngle = 10.0f;


            RegisterMessageHandlers(cockpit);

            // set up sphere layout
            SphereBoxRegion region3d = new SphereBoxRegion() {
                Radius = fCockpitRadiusPanel,
                HorzDegreeLeft = 50.0f, HorzDegreeRight = 15.0f,
                VertDegreeBottom = 40.0f, VertDegreeTop = 15.0f
            };
            BoxContainer uiContainer = new BoxContainer(new BoxRegionContainerProvider(cockpit, region3d));
            PinnedBoxes3DLayoutSolver layoutCalc = new PinnedBoxes3DLayoutSolver(uiContainer, region3d);
            PinnedBoxesLayout layout = new PinnedBoxesLayout(cockpit, layoutCalc) {
                StandardDepth = 0   // widgets are on sphere
            };
            cockpit.AddLayout(layout, "PrimarySphere", true);


            HUDToggleGroup panelGroup = BuildPanels(cockpit);


            //BuildParameterPanel(cockpit);


            try {
                AddUndoRedo(cockpit);
                AddTransformToolToggleGroup(cockpit);
                AddFrameToggleGroup(cockpit);
                AddNavModeToggleGroup(cockpit);
                AddMenuButtonsGroup(cockpit);
            } catch ( Exception e ) {
                Debug.Log("[SetupCADCockpit.Initialize::buttons/etc] exception: " + e.Message);
            }

            // setup key handlers (need to move to behavior...)
            cockpit.AddKeyHandler(new CADKeyHandler(cockpit.Context) {
                PanelGroup = panelGroup
            });

            // setup mouse handling
            cockpit.InputBehaviors.Add(new VRMouseUIBehavior(cockpit.Context) { Priority = 0 });
            cockpit.InputBehaviors.Add(new VRGamepadUIBehavior(cockpit.Context) { Priority = 0 });
            cockpit.InputBehaviors.Add(new VRSpatialDeviceUIBehavior(cockpit.Context) { Priority = 0 });

            cockpit.InputBehaviors.Add(new TwoHandViewManipBehavior(cockpit) { Priority = 1 } );
            //cockpit.InputBehaviors.Add(new SpatialDeviceGrabViewBehavior(cockpit) { Priority = 2 });
            cockpit.InputBehaviors.Add(new SpatialDeviceViewManipBehavior(cockpit) { Priority = 2 });

            RemoteGrabBehavior grabBehavior = new RemoteGrabBehavior(cockpit) { Priority = 5 };
            SecondaryGrabBehavior secondGrab = new SecondaryGrabBehavior(cockpit, grabBehavior) { Priority = 4 };
            grabBehavior.OnEndGrab += (sender, target) => {
                if ( secondGrab.InGrab && secondGrab.GrabbedSO is GroupSO ) {
                    cockpit.Context.RegisterNextFrameAction(() => {
                        GroupSO group = secondGrab.GrabbedSO as GroupSO;
                        AddToGroupChange change = new AddToGroupChange(cockpit.Scene, group, target);
                        cockpit.Scene.History.PushChange(change, false);
                        cockpit.Scene.ClearSelection();
                        cockpit.Scene.Select(group, false);
                    });
                }
            };

            cockpit.InputBehaviors.Add(secondGrab);
            cockpit.InputBehaviors.Add(grabBehavior);

            cockpit.InputBehaviors.Add(new MouseMultiSelectBehavior(cockpit.Context) { Priority = 10 });
            cockpit.InputBehaviors.Add(new GamepadMultiSelectBehavior(cockpit.Context) { Priority = 10 });
            cockpit.InputBehaviors.Add(new SpatialDeviceMultiSelectBehavior(cockpit.Context) { Priority = 10 });

            cockpit.InputBehaviors.Add(new MouseDeselectBehavior(cockpit.Context) { Priority = 999 });
            cockpit.InputBehaviors.Add(new GamepadDeselectBehavior(cockpit.Context) { Priority = 999 });
            cockpit.InputBehaviors.Add(new SpatialDeviceDeselectBehavior(cockpit.Context) { Priority = 999 });

            cockpit.InputBehaviors.Add(new SceneRightClickBehavior(cockpit) { Priority = 20 });
            cockpit.InputBehaviors.Add(new ClearBehavior(cockpit.Context) { Priority = 999 });
            cockpit.InputBehaviors.Add(new UndoShortcutBehavior(cockpit.Context) { Priority = 999 });

            cockpit.InputBehaviors.Add(new StickCycleBehavior(cockpit.Context) {
                Priority = 999, Side = CaptureSide.Left,
                Cycle = (n) => { panelGroup.SelectModulo(panelGroup.Selected - n); }
            });


            //cockpit.OverrideBehaviors.Add(new ScreenCaptureBehavior() { Priority = 0,
            //    ScreenshotPath = Environment.GetEnvironmentVariable("homepath") + "\\DropBox\\ScreenShots\\" });
            cockpit.OverrideBehaviors.Add(new FBEncoderCaptureBehavior() { Priority = 0,
                ScreenshotPath = Environment.GetEnvironmentVariable("homepath") + "\\DropBox\\ScreenShots\\" });

            // start auto-update check
            AutoUpdate.DoUpdateCheck(cockpit, 10.0f);

        }


    } // end SetupCADCockpit_V1


    public class CADKeyHandler : IShortcutKeyHandler
    {
        FContext context;
        public HUDToggleGroup PanelGroup;
        public CADKeyHandler(FContext c)
        {
            context = c;
        }
        public bool HandleShortcuts()
        {
            bool bShiftDown = Input.GetKey(KeyCode.LeftShift);
            bool bCtrlDown = Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);

            // ESCAPE CLEARS ACTIVE TOOL OR SELECTION
            if (Input.GetKeyUp(KeyCode.Escape)) {
                if (context.ToolManager.HasActiveTool(0) || context.ToolManager.HasActiveTool(1)) {
                    context.ToolManager.DeactivateTool(0);
                    context.ToolManager.DeactivateTool(1);
                } else if (context.Scene.Selected.Count > 0) {
                    context.Scene.ClearSelection();
                }
                return true;


            } else if (Input.GetKeyUp(KeyCode.A)) {
                if (PanelGroup != null)
                    PanelGroup.SelectModulo(PanelGroup.Selected + 1);
                return true;
            } else if (Input.GetKeyUp(KeyCode.S)) {
                if (PanelGroup != null)
                    PanelGroup.SelectModulo(PanelGroup.Selected - 1);
                return true;


                // CENTER TARGET (??)
            } else if (Input.GetKeyUp(KeyCode.C)) {
                Ray3f cursorRay = context.MouseController.CurrentCursorWorldRay();
                AnyRayHit hit = null;
                if (context.Scene.FindSceneRayIntersection(cursorRay, out hit)) {
                    context.ActiveCamera.Manipulator().ScenePanFocus(context.Scene, context.ActiveCamera, hit.hitPos, true);
                }
                return true;

            // TOGGLE FRAME TYPE
            } else if (Input.GetKeyUp(KeyCode.F)) {
                FrameType eCur = context.TransformManager.ActiveFrameType;
                context.TransformManager.ActiveFrameType = (eCur == FrameType.WorldFrame)
                    ? FrameType.LocalFrame : FrameType.WorldFrame;
                return true;

            // DROP A COPY
            } else if (Input.GetKeyUp(KeyCode.D)) {
                foreach (SceneObject so in context.Scene.Selected) {
                    SceneObject copy = so.Duplicate();
                    if (copy != null) {
                        // [TODO] could have a lighter record here because we can just re-run Duplicate() ?
                        context.Scene.History.PushChange(
                            new AddSOChange() { scene = context.Scene, so = copy });
                        context.Scene.History.PushInteractionCheckpoint();
                    }
                }
                return true;

            // VISIBILITY  (V HIDES, SHIFT+V SHOWS)
            } else if (Input.GetKeyUp(KeyCode.V)) {
                // show/hide (should be abstracted somehow?? instead of directly accessing GOs?)
                if (bShiftDown) {
                    foreach (SceneObject so in context.Scene.SceneObjects)
                        so.RootGameObject.Show();
                } else {
                    foreach (SceneObject so in context.Scene.Selected)
                        so.RootGameObject.Hide();
                    context.Scene.ClearSelection();
                }
                return true;

            // UNDO
            } else if (bCtrlDown && Input.GetKeyUp(KeyCode.Z)) {
                context.Scene.History.InteractiveStepBack();
                return true;

            // REDO
            } else if (bCtrlDown && Input.GetKeyUp(KeyCode.Y)) {
                context.Scene.History.InteractiveStepForward();
                return true;

            // FILE OPEN
            } else if (bCtrlDown && Input.GetKeyUp(KeyCode.O)) {
                var cp = new FileOpenCockpit() {
                    InitialPath = SceneGraphConfig.LastFileOpenPath
                };
                context.PushCockpit(cp);
                return true;

            // FILE SAVE
            } else if (bCtrlDown && Input.GetKeyUp(KeyCode.S)) {
                var cp = new FileSaveCockpit() {
                    InitialPath = SceneGraphConfig.LastFileOpenPath
                };
                context.PushCockpit(cp);
                return true;

            // FILE IMPORT
            } else if (bCtrlDown && Input.GetKeyUp(KeyCode.I)) {
                var cp = new FileImportCockpit() {
                    InitialPath = SceneGraphConfig.LastFileOpenPath
                };
                context.PushCockpit(cp);
                return true;

            // FILE EXPORT
            } else if (bCtrlDown && Input.GetKeyUp(KeyCode.E)) {
                var cp = new FileExportCockpit() {
                    InitialPath = SceneGraphConfig.LastFileOpenPath
                };
                context.PushCockpit(cp);
                return true;


            // APPLY CURRENT TOOL IF POSSIBLE
            } else if ( Input.GetKeyUp(KeyCode.Return) ) {
                if ( ((context.ActiveInputDevice & InputDevice.Mouse) != 0)
                    && context.ToolManager.HasActiveTool(ToolSide.Right)
                    && context.ToolManager.ActiveRightTool.CanApply) 
                {
                    context.ToolManager.ActiveRightTool.Apply();
                }
                return true;

            } else if (Input.GetKeyUp(KeyCode.Alpha1)) {
                context.ToolManager.SetActiveToolType(SnapDrawPrimitivesTool.Identifier, 1);
                context.ToolManager.ActivateTool(1);
                return true;
            } else if (Input.GetKeyUp(KeyCode.Alpha2)) {
                context.ToolManager.SetActiveToolType(DrawTubeTool.Identifier, 1);
                context.ToolManager.ActivateTool(1);
                return true;

            } else if (Input.GetKeyUp(KeyCode.Alpha3)) {
                context.ToolManager.SetActiveToolType(DrawCurveTool.Identifier, 1);
                context.ToolManager.ActivateTool(1);
                return true;




            } else if (Input.GetKeyUp(KeyCode.S)) {
                return true;


            } else if ( Input.GetKeyUp(KeyCode.Equals) ) {
                if (Application.isEditor) {
                    string sName = UniqueNames.GetNext("Screenshot");
                    sName = "C:\\scratch\\" + sName + ".png";
                    ScreenCapture.CaptureScreenshot(sName, 4);
                    Debug.Log("Wrote screenshot " + sName);
                }
                return true;

            } else
                return false;
        }
    }
}

