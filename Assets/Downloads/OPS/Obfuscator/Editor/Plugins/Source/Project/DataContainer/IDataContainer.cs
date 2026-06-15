using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OPS.Editor.Project.DataContainer
{
    /// <summary>
    /// Contains key to value data.
    /// </summary>
    public interface IDataContainer
    {
        // Data
        #region Data

        /// <summary>
        /// Returns true if the data container has data for a key.
        /// </summary>
        /// <param name="_Key"></param>
        /// <returns></returns>
        bool Has(String _Key);

        /// <summary>
        /// Add _Data for _Key.
        /// If the _Key already exists, decide if it should be overriden.
        /// </summary>
        /// <typeparam name="TDataType"></typeparam>
        /// <param name="_Key"></param>
        /// <param name="_Data"></param>
        /// <param name="_Override"></param>
        /// <returns></returns>
        bool Add<TDataType>(String _Key, TDataType _Data, bool _Override);

        /// <summary>
        /// Return the data at _Key. If the data does not exist, return _Default.
        /// </summary>
        /// <typeparam name="TDataType"></typeparam>
        /// <param name="_Key"></param>
        /// <param name="_Default"></param>
        /// <returns></returns>
        TDataType Get<TDataType>(String _Key, TDataType _Default = default(TDataType));

        #endregion
    }
}
