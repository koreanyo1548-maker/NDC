using System;
using Data.Utils;

namespace Controller.Utils
{
    public abstract class ControllerField
    {
        public event EventHandler<DbEventArgs> ValueChanged;

        protected void OnValueChanged()
        {
            var raiseEvent = ValueChanged;

            if (raiseEvent != null)
            {
                raiseEvent(null, null);
            }
        }
    }
    
    [Serializable]
    public class ControllerField<T>: ControllerField
    {
        private T value;
        public bool check = true;

        public T Value
        {
            get => value;
            set
            {
                this.value = value;
                OnValueChanged();
            }
        }

        public ControllerField(T value)
        {
            this.value = value;
        }
    }
}