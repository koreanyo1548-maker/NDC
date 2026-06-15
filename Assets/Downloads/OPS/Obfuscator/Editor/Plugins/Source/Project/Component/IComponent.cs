using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OPS.Editor.Project.Component
{
    /// <summary>
    /// A component has a name, description and short description.
    /// </summary>
    public interface IComponent
    {
        // Name
        #region Name

        /// <summary>
        /// Components name.
        /// </summary>
        String Name { get; }

        #endregion

        // Description
        #region Description

        /// <summary>
        /// Components long description.
        /// </summary>
        String Description { get; }

        #endregion

        // Short Description
        #region Short Description

        /// <summary>
        /// Components short description.
        /// </summary>
        String ShortDescription { get; }

        #endregion
    }
}
