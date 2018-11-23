using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using g3;

namespace f3
{
    public class HUDParameterList : HUDElementList
    {
        IParameterSource source;
        public IParameterSource Source
        {
            get { return source; }
        }

        public float RowHeight = 1.0f;
        public float LabelTextHeight = 1.0f;
        public float EntryTextHeight = 1.0f;
        public float HorzParamLabelWidth = 0.5f;

        Dictionary<string, Action> ModifiedHandlers = new Dictionary<string, Action>();


        HUDShapeElement Background;


        public HUDParameterList(IParameterSource source, float width, float row_height, float padding)
        {
            this.source = source;

            Width = width;
            Padding = padding;
            Spacing = padding;
            Height = row_height + 2 * padding;
            RowHeight = row_height;
            LabelTextHeight = RowHeight * 0.9f;
            EntryTextHeight = LabelTextHeight;
            HorzParamLabelWidth = PaddedWidth * 0.5f;
        }


        public override void Create()
        {
            build_parameters();

            Background = new HUDShapeElement() {
                Shape = new HUDShape(HUDShapeType.Rectangle, this.Width, this.Height),
                Color = Colorf.DarkYellow
            };
            Background.Create();

            base.Create();
            Children.Add(Background, false);
            update_layout();
        }

        public override void Disconnect()
        {
            base.Disconnect();
            Source.Parameters.OnParameterModified -= on_parameter_modified;
        }

        void build_parameters()
        {
            if (Source == null)
                return;

            Source.Parameters.OnParameterModified += on_parameter_modified;

            foreach ( AnyParameter param in Source.Parameters ) {
                if (param is FloatParameter)
                    add_float_param(param as FloatParameter);
                else if (param is IntParameter)
                    add_int_param(param as IntParameter);
                else if (param is BoolParameter)
                    add_bool_param(param as BoolParameter);
            }
        }


        void on_parameter_modified(ParameterSet pset, string sParamName)
        {
            Action handler;
            if (ModifiedHandlers.TryGetValue(sParamName, out handler))
                handler();
        }



        void add_float_param(FloatParameter p)
        {
            HUDTextEntry entry = new HUDTextEntry() {
                Width = PaddedWidth - HorzParamLabelWidth,
                Height = RowHeight,
                TextHeight = EntryTextHeight,
                AlignmentHorz = HorizontalAlignment.Right,
                TextValidatorF = StringValidators.SignedRealEdit
            };
            entry.OnTextEdited += (sender, newtext) => {
                float fNewValue = float.Parse(newtext);
                p.setValue(fNewValue);
            };
            Action on_float_param_changed = () => {
                entry.Text = p.getValue().ToString();
            };
            on_float_param_changed();
            ModifiedHandlers.Add(p.name, on_float_param_changed);
           
            entry.Create();
            HUDElementList row = make_horz_labeled_param(p.name, entry);
            AddListItem(row);
        }

        void add_int_param(IntParameter p)
        {
            // ignore
        }

        void add_bool_param(BoolParameter p)
        {
            // ignore
        }





        HUDElementList make_horz_labeled_param(string textLabel, HUDStandardItem item)
        {
            HUDElementList container = new HUDElementList() {
                Direction = ListDirection.Horizontal,
                Width = this.Width,
                Height = this.RowHeight,
                Spacing = this.Padding
            };
            HUDLabel label = new HUDLabel() {
                Shape = new HUDShape(HUDShapeType.Rectangle, HorzParamLabelWidth, LabelTextHeight),
                TextHeight = LabelTextHeight,
                Text = textLabel
            };
            label.Create();
            container.AddListItem(label);
            container.AddListItem(item);
            container.Create();
            return container;
        } 




        protected override void update_layout()
        {
            base.update_layout();

            FixedBoxModelElement contentBounds = BoxModel.PaddedContentBounds(this, Padding);
            Vector2f topLeft = BoxModel.GetBoxPosition(contentBounds, BoxPosition.TopLeft);

            float fZ = 0.05f * Width;
            Background.Shape = new HUDShape(HUDShapeType.Rectangle, this.VisibleListWidth, this.VisibleListHeight);
            BoxModel.SetObjectPosition(Background, BoxPosition.TopLeft, topLeft, fZ);
        }

    }
}
