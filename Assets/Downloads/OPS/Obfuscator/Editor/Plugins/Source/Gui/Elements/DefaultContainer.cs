// Unity
using UnityEngine;
using UnityEditor;

namespace OPS.Editor.Gui
{
    public class DefaultContainer : AContainer
    {
        public DefaultContainer(AHeader _Header, ADescription _Description, AContent _Content)
            : base(_Header, _Description, _Content)
        {
        }
    }
}