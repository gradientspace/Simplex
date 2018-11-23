using System;
using System.Collections.Generic;
using UnityEngine;

namespace f3
{
    //
    // This class extends a standard HUDButton with a drag action that drops the input
    //  pivot in the scene
    //
    public class DropMaterialButton : HUDButton
    {
        public FScene TargetScene { get; set; }
        public SOMaterial Material { get; set; }

        Material tempMaterial;

        public DropMaterialButton()
        {
        }


        // creates a button with a floating primitive in front of the button shape
        public void Create(float fRadius, Material bgMaterial)
        {
            Shape = new HUDShape(HUDShapeType.Disc, fRadius );
            tempMaterial = MaterialUtil.ToUnityMaterial(Material);
            base.Create(PrimitiveType.Sphere, tempMaterial, 1.2f);
            ((GameObject)button).transform.localRotation = Quaternion.identity;
        }


        enum CaptureState
        {
            ClickType,
            DragType
        }
        CaptureState eState;

        SceneObject lastHitObject;

        override public bool WantsCapture(InputEvent e)
        {
            return (Enabled && HasGO(e.hit.hitGO));
        }

        override public bool BeginCapture(InputEvent e)
        {
            eState = CaptureState.ClickType;
            lastHitObject = null;
            return true;
        }

        override public bool UpdateCapture(InputEvent e)
        {
            if (eState == CaptureState.ClickType && FindHitGO(e.ray) != null)
                return true;

            // otherwise we fall into drag state
            eState = CaptureState.DragType;

            SORayHit hit = null;
            if (TargetScene.FindSORayIntersection(e.ray, out hit)) {
                if (hit.hitSO != lastHitObject) {
                    if (lastHitObject != null)
                        lastHitObject.PopOverrideMaterial();
                    lastHitObject = hit.hitSO;
                    if (lastHitObject.GetActiveMaterial() != tempMaterial)
                        lastHitObject.PushOverrideMaterial(tempMaterial);
                }
            } else {
                if (lastHitObject != null)
                    lastHitObject.PopOverrideMaterial();
                lastHitObject = null;
            }

            return true;
        }


        // perhaps should be utility function?
        public void DoSetMaterial(IEnumerable<SceneObject> vObjects, SOMaterial material)
        {
            // if we don't use this it just gets GC'd
            SOMaterial pivotVersion =
                SOMaterial.CreateTransparentVariant(Material, TargetScene.PivotSOMaterial.RGBColor.a);
            foreach (SceneObject so in vObjects) {
                SOMaterial setMaterial = (so is PivotSO) ? pivotVersion : material;
                SOMaterialChange change = new SOMaterialChange() {
                    so = so,
                    before = so.GetAssignedSOMaterial(),
                    after = setMaterial
                };
                TargetScene.History.PushChange(change, false);
            }
            TargetScene.History.PushInteractionCheckpoint();
        }


        override public bool EndCapture(InputEvent e)
        {
            // do this here in case we ended up back in clicktype
            if (lastHitObject != null)
                lastHitObject.PopOverrideMaterial();

            if (eState == CaptureState.ClickType) {
                return base.EndCapture(e);
            }

            if (lastHitObject != null) {
                DoSetMaterial(new List<SceneObject>() { lastHitObject }, Material);
                lastHitObject = null;
            }

            return true;
        }

    }
}
