using System;
using System.Collections;
using System.Linq;
using System.Text;
using UnityEngine;
using DG.Tweening;

namespace f3
{
    public class SplashScreenCockpit : ICockpitInitializer
    {

        public void Initialize(Cockpit cockpit)
        {
            cockpit.Name = "splashCockpit";
            cockpit.DefaultCursorDepth = 3.0f;
            cockpit.PositionMode = Cockpit.MovementMode.Static;

            // create black sphere around camera to hide existing scene
            fGameObject blackBackground = new fGameObject(
                UnityUtil.CreateMeshGO("background_delete", "icon_meshes/inverted_sphere_tiny", 1.0f,
                    UnityUtil.MeshAlignOption.AllAxesCentered, MaterialUtil.CreateFlatMaterial(Color.black), false));

            // add as bounds object to do mouse hit-test
            cockpit.Context.Scene.AddWorldBoundsObject(blackBackground);

            // re-parent from Scene so the sphere tracks the camera
            cockpit.FixedCameraTrackingGO.AddChild(blackBackground, false);

            HUDButton title = HUDBuilder.CreateIconClickButton(
                new HUDShape(HUDShapeType.Rectangle, 2.0f, 0.8f ),
                3.0f, 0.0f, 2.5f, "simplex_startup/simplex_v2");
            title.Name = "title";
            title.Enabled = false;
            cockpit.AddUIElement(title, true);

            HUDButton start = HUDBuilder.CreateIconClickButton(
                new HUDShape( HUDShapeType.Rectangle, 0.5f, 0.25f ),
                3.0f, 8.0f, -10, "simplex_startup/start_v1");
            start.Name = "start";
            start.OnClicked += (s, e) => {
                cockpit.ActiveCamera.Animator().DoActionDuringDipToBlack( () => {
                    cockpit.Context.Scene.RemoveWorldBoundsObject(blackBackground);
                    UnityEngine.GameObject.Destroy(blackBackground);
                    cockpit.Context.PushCockpit(
                        new SetupCADCockpit_V1());
                }, 1.0f);
            };
            cockpit.AddUIElement(start, true);

            HUDButton quit = HUDBuilder.CreateIconClickButton(
                new HUDShape(HUDShapeType.Rectangle, 0.5f, 0.25f ),
                3.0f, -8.0f, -10, "simplex_startup/quit_v1");
            quit.Name = "quit";
            quit.OnClicked += (s, e) => {
                FPlatform.QuitApplication();
            };
            cockpit.AddUIElement(quit, true);

            HUDButton logo = HUDBuilder.CreateIconClickButton(
                new HUDShape(HUDShapeType.Rectangle, 0.8f, 0.2f ),
                3.0f, 0.0f, -25.0f, "simplex_startup/gradientspace_splash");
            logo.Name = "logo";
            logo.Enabled = false;
            cockpit.AddUIElement(logo, true);



            HUDButton about_arrow = HUDBuilder.CreateIconClickButton(
                new HUDShape(HUDShapeType.Rectangle, 0.75f, 0.75f ),
                3.0f, -30.0f, -10.0f, "simplex_startup/about_arrow");
            about_arrow.Name = "about_arrow";
            about_arrow.Enabled = false;
            cockpit.AddUIElement(about_arrow, true);


            float about_text_scale = 3.0f;
            float angle_left = -65.0f;
            HUDButton about_text = HUDBuilder.CreateIconClickButton(
                new HUDShape(HUDShapeType.Rectangle,
                    0.8f * about_text_scale, 1.0f * about_text_scale) { UseUVSubRegion = false },
                3.0f, angle_left, 0.0f, "simplex_startup/about_text");
            UnityUtil.TranslateInFrame(about_text.RootGameObject, 0.0f, -0.5f, 0, CoordSpace.WorldCoords);
            about_text.Name = "about_text";
            about_text.Enabled = false;
            cockpit.AddUIElement(about_text, true);



            HUDButton controls_arrow = HUDBuilder.CreateIconClickButton(
                new HUDShape( HUDShapeType.Rectangle, 0.75f, 0.75f ),
                3.0f, 30.0f, -10.0f, "simplex_startup/controls_arrow");
            controls_arrow.Name = "controls_arrow";
            controls_arrow.Enabled = false;
            cockpit.AddUIElement(controls_arrow, true);



            float text_scale = 3.0f;
            float angle_right = 65.0f;
            HUDButton controls_mouse = HUDBuilder.CreateIconClickButton(
                new HUDShape(HUDShapeType.Rectangle, 0.7f*text_scale, 0.7f*text_scale) { UseUVSubRegion = false },
                3.0f, angle_right, 0.0f, "simplex_startup/controls_mouse");
            UnityUtil.TranslateInFrame(controls_mouse.RootGameObject, 0.25f, 0.1f, 0, CoordSpace.WorldCoords);
            controls_mouse.Name = "controls_mouse";
            controls_mouse.Enabled = false;
            cockpit.AddUIElement(controls_mouse, true);

            HUDButton controls_gamepad = HUDBuilder.CreateIconClickButton(
                new HUDShape(HUDShapeType.Rectangle, 0.7f * text_scale, 0.7f * text_scale) { UseUVSubRegion = false },
                3.0f, angle_right, 0.0f, "simplex_startup/controls_gamepad");
            UnityUtil.TranslateInFrame(controls_gamepad.RootGameObject, 0.25f, 0.1f, 0, CoordSpace.WorldCoords);
            controls_gamepad.Name = "controls_gamepad";
            controls_gamepad.Enabled = false;
            cockpit.AddUIElement(controls_gamepad, true);
            controls_gamepad.RootGameObject.Hide();



            HUDToggleButton mouse_button = HUDBuilder.CreateToggleButton(
                new HUDShape(HUDShapeType.Rectangle, 1.0f, 1.0f ),
                2.9f, angle_right, 0.0f, "simplex_startup/mouse", "simplex_startup/mouse_disabled");
            UnityUtil.TranslateInFrame(mouse_button.RootGameObject, -0.5f, -1.0f, 0, CoordSpace.WorldCoords);
            mouse_button.Name = "mouse_button";
            cockpit.AddUIElement(mouse_button, true);

            HUDToggleButton gamepad_button = HUDBuilder.CreateToggleButton(
                new HUDShape(HUDShapeType.Rectangle, 1.0f, 1.0f ),
                2.9f, angle_right, 0.0f, "simplex_startup/gamepad_with_labels", "simplex_startup/gamepad_disabled");
            UnityUtil.TranslateInFrame(gamepad_button.RootGameObject, 0.5f, -1.0f, 0, CoordSpace.WorldCoords);
            gamepad_button.Name = "gamepad_button";
            cockpit.AddUIElement(gamepad_button, true);


            HUDToggleGroup group = new HUDToggleGroup();
            group.AddButton(mouse_button);
            group.AddButton(gamepad_button);
            group.Selected = 0;
            group.OnToggled += (sender, nSelected) => {
                if (nSelected == 0) {
                    controls_gamepad.RootGameObject.Hide();
                    controls_mouse.RootGameObject.Show();
                } else {
                    controls_gamepad.RootGameObject.Show();
                    controls_mouse.RootGameObject.Hide();
                }
            };




            // setup key handlers (need to move to behavior...)
            cockpit.AddKeyHandler(new SplashScreenKeyHandler(cockpit.Context));

            cockpit.InputBehaviors.Add(new VRMouseUIBehavior(cockpit.Context) { Priority = 0 });
            cockpit.InputBehaviors.Add(new VRGamepadUIBehavior(cockpit.Context) { Priority = 0 });
            cockpit.InputBehaviors.Add(new VRSpatialDeviceUIBehavior(cockpit.Context) { Priority = 0 });
        }
    }



    public class SplashScreenKeyHandler : IShortcutKeyHandler
    {
        //SceneController controller;
        public SplashScreenKeyHandler(FContext c)
        {
            //controller = c;
        }
        public bool HandleShortcuts()
        {
            if (Input.GetKeyUp(KeyCode.Q)) {
                FPlatform.QuitApplication();
                return true;
            } else if (Input.GetKeyUp("escape")) {
                FPlatform.QuitApplication();
                return true;
            }

            return false;
        }
    }

}
