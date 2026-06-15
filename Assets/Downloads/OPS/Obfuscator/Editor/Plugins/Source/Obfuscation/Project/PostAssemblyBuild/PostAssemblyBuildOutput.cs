using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

// OPS - Project - Step
using OPS.Editor.Project.Step;

namespace OPS.Obfuscator.Editor.Project.PostAssemblyBuild
{
    /// <summary>
    /// Output object for the post assembly build step.
    /// </summary>
    public class PostAssemblyBuildOutput : IStepOutput
    {
        // Data
        #region Data

        /// <summary>
        /// Stores dynamic data in a string key and object value format.
        /// </summary>
        public Dictionary<String, System.Object> Data { get; private set; } = new Dictionary<string, object>();

        /// <summary>
        /// Returns true if the data container has data for a key.
        /// </summary>
        /// <param name="_Key"></param>
        /// <returns></returns>
        public bool Has(String _Key)
        {
            return this.Data.ContainsKey(_Key);
        }

        /// <summary>
        /// Add _Data for _Key.
        /// If the _Key already exists, decide if it should be overriden.
        /// </summary>
        /// <typeparam name="TDataType"></typeparam>
        /// <param name="_Key"></param>
        /// <param name="_Data"></param>
        /// <param name="_Override"></param>
        /// <returns></returns>
        public bool Add<TDataType>(String _Key, TDataType _Data, bool _Override)
        {
            if (this.Data.ContainsKey(_Key))
            {
                if (!_Override)
                {
                    return false;
                }
            }

            this.Data[_Key] = _Data;

            return true;
        }

        /// <summary>
        /// Return the data at _Key. If the data does not exist, return _Default.
        /// </summary>
        /// <typeparam name="TDataType"></typeparam>
        /// <param name="_Key"></param>
        /// <param name="_Default"></param>
        /// <returns></returns>
        public TDataType Get<TDataType>(String _Key, TDataType _Default = default(TDataType))
        {
            if (this.Data.TryGetValue(_Key, out System.Object var_Value))
            {
                return (TDataType)var_Value;
            }
            return _Default;
        }

        #endregion

        // Failed
        #region Failed

        /// <summary>
        /// Returns if the pipeline processing failed!
        /// </summary>
        public bool Failed { get; set; }

        #endregion
    }
}
