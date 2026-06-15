using System;
using Controller.Utils;

namespace UIs.Utils
{
    public abstract class UIField
    {
        public Action ValueChanged;

        protected void OnValueChanged()
        {
            var raiseEvent = ValueChanged;
            raiseEvent?.Invoke();
        }
    }
    
    [Serializable]
    public class UIField<T>: UIField
    {
        private T value;

        public T Value
        {
            get => value;
            set
            {
                this.value = value;
                OnValueChanged();
            }
        }

        public UIField(T value)
        {
            this.value = value;
        }
    }
}