using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using g3;

namespace f3
{
    public class HUDParametersPanel : HUDPanel
    {
        Cockpit cockpit;

        bool panels_need_update = false;

        HUDParameterList ParametersList;

        public float ParameterRowHeight = 1.0f;
        public float ParametersPadding = 1.0f;


        public HUDParametersPanel(Cockpit cockpit)
        {
            this.cockpit = cockpit;
            cockpit.Scene.SelectionChangedEvent += Scene_SelectionChangedEvent;
        }

        public override void Create()
        {
            panels_need_update = true;
            base.Create();
        }

        private void Scene_SelectionChangedEvent(object sender, EventArgs e) {
            panels_need_update = true;
        }

        public override void PreRender()
        {
            if (panels_need_update) {
                update_panels();
                panels_need_update = false;
            }
        }




        void update_panels()
        {
            // discard existing panels
            if (ParametersList != null) {
                ParametersList.Disconnect();
                Children.Remove(ParametersList);
                ParametersList.RootGameObject.Destroy();
                ParametersList = null;
            }

            if ( cockpit.Scene.Selected.Count == 1 ) {
                SceneObject so = cockpit.Scene.Selected[0];
                if ( so is IParameterSource ) {
                    ParametersList = new HUDParameterList(so as IParameterSource, this.Width, ParameterRowHeight, ParametersPadding);
                    ParametersList.Create();
                    Children.Add(ParametersList, false);
                }
            }

            update_layout();
        }


        void update_layout()
        {
            FixedBoxModelElement frameBounds = BoxModel.PaddedBounds(this, Padding);
            Vector2f topLeft = BoxModel.GetBoxPosition(frameBounds, BoxPosition.TopLeft);
            float fZ = -0.01f * Width;
            if ( ParametersList != null )
                BoxModel.SetObjectPosition(ParametersList, BoxPosition.TopLeft, topLeft, fZ);
        }


    }
}
