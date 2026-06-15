using System;

namespace Utils
{
    public abstract class Singleton<T> where T : class, new()
    {
        private static T instance;

        public static T I
        {
            get
            {
                if (instance == null) instance = new T();
                return instance;
            }
        }

        static Singleton()
        {
            instance = new T();
        }
    }
}