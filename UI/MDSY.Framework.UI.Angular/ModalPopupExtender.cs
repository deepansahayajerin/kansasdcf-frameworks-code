using System;
using System.Collections.Generic;
using System.Text;

namespace MDSY.Framework.UI.Angular
{
    public class ModalPopupExtender : IControl
    {
        private string _id;
        private string _popupControlId;
        private string _targetControlId;
        private string _behaviorId;
        private bool _dropShadow;
        private string _okControlId;
        private string _backgroundCssClass;
        private int _x;
        private int _y;
        private string _cancelControlId;

        public ModalPopupExtender()
        {

        }

        public string Id
        {
            get
            {
                return _id;
            }

            set
            {
                _id = value;
            }
        }

        public string Type
        {
            get
            {
                return this.GetType().Name;
            }
        }

        public Dictionary<string, IControl> Controls
        {
            get { return null; }
        }

        public Dictionary<string, object> GetControlMap()
        {
            Dictionary<string, object> mpe = new Dictionary<string, object>();
            mpe.Add("type", Type);
            mpe.Add("id", Id);
            mpe.Add("backgroundCssClass", _backgroundCssClass);
            mpe.Add("popupControlId", _popupControlId);
            mpe.Add("targetControlId", _targetControlId);
            mpe.Add("behaviorId", _behaviorId);
            mpe.Add("dropShadow", _dropShadow);
            mpe.Add("okControlId", _okControlId);
            mpe.Add("x", _x);
            mpe.Add("y", _y);

            return mpe;
        }

        public string PopupControlId
        {
            set
            {
                _popupControlId = value;
            }
        }

        public string TargetControlId
        {
            set
            {
                _targetControlId = value;
            }
        }

        public string BehaviorId
        {
            get
            {
                return _behaviorId;
            }
            set
            {
                _behaviorId = value;
            }
        }

        public bool DropShadow
        {
            set
            {
                _dropShadow = value;
            }
        }

        public string OkControlId
        {
            set
            {
                _okControlId = value;
            }
        }

        public string BackgroundCssClass
        {
            set
            {
                _backgroundCssClass = value;
            }
        }

        public int X
        {
            get { return _x; }
            set { _x = value; }
        }

        public int Y
        {
            get { return _y; }
            set { _y = value; }
        }

        public string CancelControlId
        {
            set
            {
                _cancelControlId = value;
            }
        }

        public void Show()
        {

        }

        public void Hide()
        {

        }
    }
}
