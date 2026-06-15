using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

// OPS - Mono - Cecil
using OPS.Mono.Cecil;

// OPS - Obfuscator - Assembly
using OPS.Obfuscator.Editor.Assembly.Mono.Member.Helper;

namespace OPS.Obfuscator.Editor.Assembly.Mono.Member.Attribute.Helper
{
    /// <summary>
    /// Helper for CustomAttribute and ICustomAttributeProvider.
    /// </summary>
    public class AttributeHelper
    {
        // Contains
        #region Contains

        /// <summary>
        /// True: The _Attributes collection contains a attribute with _AttributeName or (_AttributeName + "Attribute")
        /// </summary>
        /// <param name="_AttributeProvider"></param>
        /// <param name="_AttributeName"></param>
        /// <returns></returns>
        public static bool HasCustomAttribute(ICustomAttributeProvider _AttributeProvider, String _AttributeName)
        {
            foreach (CustomAttribute var_CustomAttribute in _AttributeProvider.CustomAttributes)
            {
                if (var_CustomAttribute.AttributeType.Name == _AttributeName)
                {
                    return true;
                }
                else if (var_CustomAttribute.AttributeType.Name == _AttributeName + "Attribute")
                {
                    return true;
                }
                else if (var_CustomAttribute.AttributeType.FullName == _AttributeName)
                {
                    return true;
                }
                else if (var_CustomAttribute.AttributeType.FullName == _AttributeName + "Attribute")
                {
                    return true;
                }
            }
            return false;
        }

        #endregion

        // Get
        #region Get

        /// <summary>
        /// If there is a CustomAttribute with _AttributeName in _AttributeProvider,
        /// returns true and the _CustomAttribute.
        /// </summary>
        /// <param name="_AttributeProvider"></param>
        /// <param name="_AttributeName"></param>
        /// <param name="_CustomAttribute"></param>
        /// <returns></returns>
        public static bool TryGetCustomAttribute(ICustomAttributeProvider _AttributeProvider, String _AttributeName, out CustomAttribute _CustomAttribute)
        {
            foreach (CustomAttribute var_CustomAttribute in _AttributeProvider.CustomAttributes)
            {
                if (var_CustomAttribute.AttributeType.Name == _AttributeName)
                {
                    _CustomAttribute = var_CustomAttribute;
                    return true;
                }
                else if (var_CustomAttribute.AttributeType.Name == _AttributeName + "Attribute")
                {
                    _CustomAttribute = var_CustomAttribute;
                    return true;
                }
            }

            _CustomAttribute = null;
            return false;
        }

        #endregion

        // Remove
        #region Remove

        /// <summary>
        /// Remove all custom attributes in _AttributeProvider with _AttributeFullName.
        /// </summary>
        /// <param name="_AttributeProvider"></param>
        /// <param name="_AttributeFullName"></param>
        public static void RemoveCustomAttributes(ICustomAttributeProvider _AttributeProvider, String _AttributeFullName)
        {
            for(int c = 0; c < _AttributeProvider.CustomAttributes.Count; c++)
            {
                CustomAttribute var_CustomAttribute = _AttributeProvider.CustomAttributes[c];

                if (var_CustomAttribute.AttributeType.FullName == _AttributeFullName)
                {
                    _AttributeProvider.CustomAttributes.RemoveAt(c--);
                }
            }
        }

        #endregion

        // Resolve
        #region Resolve

        /// <summary>
        /// Get resolveable CustomAttributes.
        /// </summary>
        /// <param name="_CustomAttributeProvider"></param>
        /// <returns></returns>
        public static IEnumerable<CustomAttribute> Get_ResolveAble_CustomAttributes(ICustomAttributeProvider _CustomAttributeProvider)
        {
            return _CustomAttributeProvider.CustomAttributes.Where(delegate (CustomAttribute ca)
            {
                return MemberReferenceHelper.TryToResolve<TypeDefinition>(ca.AttributeType, out TypeDefinition var_TypeDefinition);
            });
        }

        #endregion

        // CustomAttributeArgument
        #region CustomAttributeArgument

        /// <summary>
        /// Iterate all resolve able arguments in all custom attributes, includes children.
        /// </summary>
        /// <param name="_CustomAttributeProvider"></param>
        /// <returns></returns>
        public static IEnumerable<CustomAttributeArgument> Iterate_CustomAttributeArguments(ICustomAttributeProvider _CustomAttributeProvider)
        {
            CustomAttribute[] var_CustomAttributeArray = Get_ResolveAble_CustomAttributes(_CustomAttributeProvider).ToArray();

            for(int c = 0; c < var_CustomAttributeArray.Length; c++)
            {
                foreach (CustomAttributeArgument var_CustomAttributeArgument in Iterate_CustomAttributeArguments(var_CustomAttributeArray[c].ConstructorArguments))
                {
                    if(var_CustomAttributeArgument != null)
                    {
                        yield return var_CustomAttributeArgument;
                    }
                }
                foreach (CustomAttributeArgument var_CustomAttributeArgument in Iterate_CustomAttributeArguments(from f in var_CustomAttributeArray[c].Fields select f.Argument))
                {
                    if (var_CustomAttributeArgument != null)
                    {
                        yield return var_CustomAttributeArgument;
                    }
                }
                foreach (CustomAttributeArgument var_CustomAttributeArgument in Iterate_CustomAttributeArguments(from p in var_CustomAttributeArray[c].Properties select p.Argument))
                {
                    if (var_CustomAttributeArgument != null)
                    {
                        yield return var_CustomAttributeArgument;
                    }
                }
            }
        }

        /// <summary>
        /// Iterate all custom attribute arguments, includes children.
        /// </summary>
        /// <param name="_CustomAttributeArgumentEnumerable"></param>
        /// <returns></returns>
        private static IEnumerable<CustomAttributeArgument> Iterate_CustomAttributeArguments(IEnumerable<CustomAttributeArgument> _CustomAttributeArgumentEnumerable)
        {
            if (_CustomAttributeArgumentEnumerable != null)
            {
                foreach (CustomAttributeArgument var_CustomAttributeArgument in _CustomAttributeArgumentEnumerable)
                {
                    MetadataType metadataType = var_CustomAttributeArgument.Type.MetadataType;

                    switch (metadataType)
                    {
                        case MetadataType.Array:
                            {
                                yield return var_CustomAttributeArgument;

                                foreach (CustomAttributeArgument var_CustomAttributeArgument_InArray in Iterate_CustomAttributeArguments((IEnumerable<CustomAttributeArgument>)var_CustomAttributeArgument.Value))
                                {
                                    yield return var_CustomAttributeArgument_InArray;
                                }
                                break;
                            }
                        default:
                            {
                                yield return var_CustomAttributeArgument;

                                break;
                            }
                    }
                }
            }
        }

        #endregion

        // CustomAttributeNamedArgument
        #region CustomAttributeNamedArgument

        /// <summary>
        /// Iterate all CustomAttributeNamedArgument ( either field or property ) in _CustomAttributeProvider.
        /// </summary>
        /// <param name="_CustomAttributeProvider"></param>
        /// <param name="_Field"></param>
        /// <returns></returns>
        public static IEnumerable<KeyValuePair<CustomAttribute, CustomAttributeNamedArgument>> Iterate_CustomAttributeNamedArguments(ICustomAttributeProvider _CustomAttributeProvider, bool _Field)
        {
            CustomAttribute[] var_CustomAttributeArray = Get_ResolveAble_CustomAttributes(_CustomAttributeProvider).ToArray();

            for (int c = 0; c < var_CustomAttributeArray.Length; c++)
            {
                if (_Field)
                {
                    foreach (CustomAttributeNamedArgument var_CustomAttributeNamedArgument in Iterate_CustomAttributeNamedArguments(var_CustomAttributeArray[c].Fields))
                    {
                        if (var_CustomAttributeNamedArgument != null)
                        {
                            yield return new KeyValuePair<CustomAttribute, CustomAttributeNamedArgument>(var_CustomAttributeArray[c], var_CustomAttributeNamedArgument);
                        }
                    }
                }
                else
                {
                    foreach (CustomAttributeNamedArgument var_CustomAttributeNamedArgument in Iterate_CustomAttributeNamedArguments(var_CustomAttributeArray[c].Properties))
                    {
                        if (var_CustomAttributeNamedArgument != null)
                        {
                            yield return new KeyValuePair<CustomAttribute, CustomAttributeNamedArgument>(var_CustomAttributeArray[c], var_CustomAttributeNamedArgument);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Iterate all CustomAttributeNamedArgument in _CustomAttributeNamedArgumentEnumerable.
        /// </summary>
        /// <param name="_CustomAttributeNamedArgumentEnumerable"></param>
        /// <returns></returns>
        private static IEnumerable<CustomAttributeNamedArgument> Iterate_CustomAttributeNamedArguments(IEnumerable<CustomAttributeNamedArgument> _CustomAttributeNamedArgumentEnumerable)
        {
            if (_CustomAttributeNamedArgumentEnumerable != null)
            {
                foreach (CustomAttributeNamedArgument var_CustomAttributeNamedArgument in _CustomAttributeNamedArgumentEnumerable)
                {
                    yield return var_CustomAttributeNamedArgument;
                }
            }
        }

        /// <summary>
        /// Tries to get a properties or fields value as TObject by its _Name.
        /// </summary>
        /// <typeparam name="TObject"></typeparam>
        /// <param name="_CustomAttribute"></param>
        /// <param name="_Name"></param>
        /// <param name="_Object"></param>
        /// <returns></returns>
        public static bool TryGetAttributeValueByName<TObject>(CustomAttribute _CustomAttribute, String _Name, out TObject _Object)
        {
            // Field
            foreach (CustomAttributeNamedArgument var_CustomAttributeNamedArgument in Iterate_CustomAttributeNamedArguments(_CustomAttribute.Fields))
            {
                if(var_CustomAttributeNamedArgument.Name == _Name)
                {
                    _Object = (TObject)var_CustomAttributeNamedArgument.Argument.Value;

                    return true;
                }
            }

            // Property
            foreach (CustomAttributeNamedArgument var_CustomAttributeNamedArgument in Iterate_CustomAttributeNamedArguments(_CustomAttribute.Properties))
            {
                if (var_CustomAttributeNamedArgument.Name == _Name)
                {
                    _Object = (TObject)var_CustomAttributeNamedArgument.Argument.Value;

                    return true;
                }
            }

            _Object = default(TObject);
            return false;
        }

        #endregion
    }
}
