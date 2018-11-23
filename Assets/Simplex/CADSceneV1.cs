using System;
using UnityEngine;
using System.Collections.Generic;
using f3;
using g3;
using gs;
using simplex;

public class CADSceneV1 : BaseSceneConfig
{
    public GameObject VRCameraRig;

    FContext context;
    public override FContext Context { get { return context; } }


    // Use this for initialization
    void Awake()
    {
        // if we need to auto-configure Rift vs Vive vs (?) VR, we need
        // to do this before any other F3 setup, because MainCamera will change
        // and we are caching that in a lot of places...
        if (AutoConfigVR) {
            VRCameraRig = gs.VRPlatform.AutoConfigureVR();
        }

        // restore any settings
        SceneGraphConfig.RestorePreferences();

        // set up some defaults
        SceneGraphConfig.InitialSceneTranslate = -4.0f * Vector3f.AxisY;
        SceneGraphConfig.DefaultSceneCurveVisualDegrees = 0.5f;
        SceneGraphConfig.DefaultPivotVisualDegrees = 2.3f;
        SceneGraphConfig.DefaultAxisGizmoVisualDegrees = 25.0f;

        SceneOptions options = new SceneOptions();
        options.UseSystemMouseCursor = false;
        options.Use2DCockpit = false;
        options.EnableTransforms = true;
        options.EnableCockpit = true;
        bool bDebugSplashScreen = false;
        bool bShowSplashScreen = (Application.isEditor == false || bDebugSplashScreen);
        if ( bShowSplashScreen == false )
            options.CockpitInitializer = new SetupCADCockpit_V1();
        else
            options.CockpitInitializer = new SplashScreenCockpit();
        options.MouseCameraControls = new MayaCameraHotkeys();
        options.SpatialCameraRig = VRCameraRig;

        // very verbose
        options.LogLevel = 2;

        context = new FContext();
        context.Start(options);

        context.TransformManager.RegisterGizmoType("snap_drag", new SnapDragGizmoBuilder());
        context.TransformManager.RegisterGizmoType("object_edit", new EditObjectGizmoBuilder());
        //controller.TransformManager.SetActiveGizmoType("snap_drag");
        //controller.TransformManager.SetActiveGizmoType("object_edit");

        context.ToolManager.RegisterToolType(SnapDrawPrimitivesTool.Identifier, new SnapDrawPrimitivesToolBuilder());
        context.ToolManager.RegisterToolType(DrawTubeTool.Identifier, new DrawTubeToolBuilder());
        context.ToolManager.RegisterToolType(DrawCurveTool.Identifier, new DrawCurveToolBuilder());
        context.ToolManager.RegisterToolType(RevolveTool.Identifier, new RevolveToolBuilder());
        context.ToolManager.RegisterToolType(SculptCurveTool.Identifier, new SculptCurveToolBuilder() 
            { InitialRadius = 0.1f });
        context.ToolManager.RegisterToolType(DrawSurfaceCurveTool.Identifier, new DrawSurfaceCurveToolBuilder() 
            { DefaultSamplingRate = 0.1f, AttachCurveToSurface = true, Closed = false } );
        context.ToolManager.RegisterToolType(TwoPointMeasureTool.Identifier, new TwoPointMeasureToolBuilder() 
            { SnapThresholdAngle = 5.0f  });
        context.ToolManager.RegisterToolType(PlaneCutTool.Identifier, new PlaneCutToolBuilder() {
            GenerateFillSurface = true
        });
        context.ToolManager.SetActiveToolType(SnapDrawPrimitivesTool.Identifier, ToolSide.Left);
        context.ToolManager.SetActiveToolType(SnapDrawPrimitivesTool.Identifier, ToolSide.Right);


        // Set up standard scene lighting if requested
        if ( options.EnableDefaultLighting ) {
            GameObject lighting = GameObject.Find("SceneLighting");
            if (lighting == null)
                lighting = new GameObject("SceneLighting");
            SceneLightingSetup setup = lighting.AddComponent<SceneLightingSetup>();
            setup.Context = context;
            setup.LightDistance = 20.0f; // related to total scene scale...
        }


        // set up ground plane geometry
        GameObject groundPlane = GameObject.Find("GroundPlane");
        if ( groundPlane != null )
        context.Scene.AddWorldBoundsObject( new fGameObject(groundPlane) );


        //RMSTest.TestInflate();


        // [TODO] need to do this at cockpit level!!
        GameObject head = GameObject.Find("VRHead");
        if (bShowSplashScreen == false) {
            if (head != null && head.IsVisible()) {
                head.transform.position = Vector3f.Zero;
                head.transform.rotation = Quaternionf.Identity;
                context.ActiveCamera.AddChild(head, false);

                GameObject mesh = head.FindChildByName("head_mesh", false);
                Colorf c = mesh.GetColor();
                SmoothCockpitTracker tracker = context.ActiveCockpit.CustomTracker as SmoothCockpitTracker;
                tracker.OnTrackingStateChange += (eState) => {
                    if (eState == SmoothCockpitTracker.TrackingState.NotTracking)
                        mesh.SetColor(c);
                    else if (eState == SmoothCockpitTracker.TrackingState.Tracking)
                        mesh.SetColor(Colorf.VideoRed);
                    else if (eState == SmoothCockpitTracker.TrackingState.TrackingWarmup || eState == SmoothCockpitTracker.TrackingState.TrackingCooldown)
                        mesh.SetColor(Colorf.Orange);
                };
            }
        } else {
            if (head != null)
                head.Hide();
        }


        // [RMS] circle that is roughly at edge of VR viewport (but tweak the 1.4 depending on type of screenshot...)
        //fCircleGameObject go = GameObjectFactory.CreateCircleGO("circ", 1.0f, Colorf.Red, 0.025f, LineWidthType.World);
        //Frame3f f = context.ActiveCamera.GetWorldFrame();
        //go.SetLocalPosition(context.ActiveCamera.GetPosition() + 1.4f * f.Z);
        //go.SetLocalRotation(Quaternionf.AxisAngleD(Vector3f.AxisX, 90));
        //context.ActiveCamera.AddChild(go, true);



    }



}