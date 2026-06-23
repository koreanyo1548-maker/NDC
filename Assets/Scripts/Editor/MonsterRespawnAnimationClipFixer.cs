using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using Object = UnityEngine.Object;

public static class MonsterRespawnAnimationClipFixer
{
    private const string CharacterPrefabDirectory = "Assets/Resources/Prefabs/Characters";
    private static readonly HashSet<string> TargetStateNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        "idle",
        "walk",
        "attack"
    };

    [MenuItem("Tools/NDC/Monsters/Dry Run Respawn Animation Fix")]
    private static void DryRun()
    {
        FixClips(dryRun: true);
    }

    [MenuItem("Tools/NDC/Monsters/Fix Respawn Animation Clips")]
    private static void Fix()
    {
        FixClips(dryRun: false);
    }

    private static void FixClips(bool dryRun)
    {
        var prefabGuids = AssetDatabase.FindAssets("t:Prefab", new[] { CharacterPrefabDirectory });
        var processed = 0;
        var candidateClips = new HashSet<AnimationClip>();
        var changedClips = new HashSet<AnimationClip>();
        var plannedFloatBindings = new Dictionary<AnimationClip, HashSet<string>>();
        var plannedObjectBindings = new Dictionary<AnimationClip, HashSet<string>>();
        var addedFloatCurves = 0;
        var addedObjectCurves = 0;
        var skippedBindings = 0;

        foreach (var guid in prefabGuids)
        {
            var prefabPath = AssetDatabase.GUIDToAssetPath(guid);
            if (prefabPath.IndexOf("/Backup/", StringComparison.OrdinalIgnoreCase) >= 0) continue;

            var prefabRoot = PrefabUtility.LoadPrefabContents(prefabPath);
            try
            {
                var animator = prefabRoot.GetComponentInChildren<Animator>(true);
                var controller = GetAnimatorController(animator != null ? animator.runtimeAnimatorController : null);
                if (animator == null || controller == null) continue;

                var states = GetStates(controller);
                var dieClips = GetStateClips(states, state => IsNamed(state, "die"));
                var targetClips = GetStateClips(states, IsTargetState);
                if (dieClips.Count == 0 || targetClips.Count == 0) continue;

                processed++;
                foreach (var dieClip in dieClips)
                {
                    var floatBindings = AnimationUtility.GetCurveBindings(dieClip);
                    var objectBindings = AnimationUtility.GetObjectReferenceCurveBindings(dieClip);

                    foreach (var targetClip in targetClips)
                    {
                        if (targetClip == null || targetClip == dieClip) continue;

                        var targetFloatBindings = GetOrCreateFloatBindingSet(plannedFloatBindings, targetClip);
                        foreach (var binding in floatBindings)
                        {
                            var bindingKey = GetBindingKey(binding);
                            if (targetFloatBindings.Contains(bindingKey)) continue;
                            if (!TryGetDefaultFloatValue(animator.transform, binding, out var value))
                            {
                                skippedBindings++;
                                continue;
                            }

                            addedFloatCurves++;
                            candidateClips.Add(targetClip);
                            targetFloatBindings.Add(bindingKey);
                            if (dryRun) continue;

                            AnimationUtility.SetEditorCurve(targetClip, binding, MakeConstantCurve(targetClip, value));
                            changedClips.Add(targetClip);
                        }

                        var targetObjectBindings = GetOrCreateObjectBindingSet(plannedObjectBindings, targetClip);
                        foreach (var binding in objectBindings)
                        {
                            var bindingKey = GetBindingKey(binding);
                            if (targetObjectBindings.Contains(bindingKey)) continue;
                            if (!TryGetDefaultObjectValue(animator.transform, binding, out var value))
                            {
                                skippedBindings++;
                                continue;
                            }

                            addedObjectCurves++;
                            candidateClips.Add(targetClip);
                            targetObjectBindings.Add(bindingKey);
                            if (dryRun) continue;

                            AnimationUtility.SetObjectReferenceCurve(targetClip, binding, MakeConstantObjectCurve(targetClip, value));
                            changedClips.Add(targetClip);
                        }
                    }
                }
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(prefabRoot);
            }
        }

        if (!dryRun)
        {
            foreach (var clip in changedClips)
            {
                EditorUtility.SetDirty(clip);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        Debug.Log(
            $"[MonsterRespawnAnimationClipFixer] dryRun={dryRun} prefabs={processed} candidateClips={candidateClips.Count} changedClips={changedClips.Count} " +
            $"addedFloatCurves={addedFloatCurves} addedObjectCurves={addedObjectCurves} skippedBindings={skippedBindings}");
    }

    private static AnimatorController GetAnimatorController(RuntimeAnimatorController controller)
    {
        if (controller is AnimatorController animatorController) return animatorController;
        if (controller is AnimatorOverrideController overrideController)
        {
            return GetAnimatorController(overrideController.runtimeAnimatorController);
        }

        return null;
    }

    private static List<AnimatorState> GetStates(AnimatorController controller)
    {
        var states = new List<AnimatorState>();
        foreach (var layer in controller.layers)
        {
            CollectStates(layer.stateMachine, states);
        }

        return states;
    }

    private static void CollectStates(AnimatorStateMachine stateMachine, List<AnimatorState> states)
    {
        foreach (var childState in stateMachine.states)
        {
            states.Add(childState.state);
        }

        foreach (var childStateMachine in stateMachine.stateMachines)
        {
            CollectStates(childStateMachine.stateMachine, states);
        }
    }

    private static HashSet<AnimationClip> GetStateClips(List<AnimatorState> states, Func<AnimatorState, bool> predicate)
    {
        var clips = new HashSet<AnimationClip>();
        foreach (var state in states)
        {
            if (!predicate(state)) continue;
            CollectMotionClips(state.motion, clips);
        }

        return clips;
    }

    private static void CollectMotionClips(Motion motion, HashSet<AnimationClip> clips)
    {
        switch (motion)
        {
            case null:
                return;
            case AnimationClip clip:
                clips.Add(clip);
                return;
            case BlendTree blendTree:
                foreach (var child in blendTree.children)
                {
                    CollectMotionClips(child.motion, clips);
                }
                break;
        }
    }

    private static bool IsTargetState(AnimatorState state)
    {
        if (TargetStateNames.Contains(state.name)) return true;
        return state.motion != null && TargetStateNames.Contains(state.motion.name);
    }

    private static bool IsNamed(AnimatorState state, string name)
    {
        if (state.name.Equals(name, StringComparison.OrdinalIgnoreCase)) return true;
        return state.motion != null && state.motion.name.Equals(name, StringComparison.OrdinalIgnoreCase);
    }

    private static HashSet<string> GetFloatBindingSet(AnimationClip clip)
    {
        var keys = new HashSet<string>();
        foreach (var binding in AnimationUtility.GetCurveBindings(clip))
        {
            keys.Add(GetBindingKey(binding));
        }

        return keys;
    }

    private static HashSet<string> GetOrCreateFloatBindingSet(Dictionary<AnimationClip, HashSet<string>> bindingSets, AnimationClip clip)
    {
        if (bindingSets.TryGetValue(clip, out var bindings)) return bindings;

        bindings = GetFloatBindingSet(clip);
        bindingSets.Add(clip, bindings);
        return bindings;
    }

    private static HashSet<string> GetObjectBindingSet(AnimationClip clip)
    {
        var keys = new HashSet<string>();
        foreach (var binding in AnimationUtility.GetObjectReferenceCurveBindings(clip))
        {
            keys.Add(GetBindingKey(binding));
        }

        return keys;
    }

    private static HashSet<string> GetOrCreateObjectBindingSet(Dictionary<AnimationClip, HashSet<string>> bindingSets, AnimationClip clip)
    {
        if (bindingSets.TryGetValue(clip, out var bindings)) return bindings;

        bindings = GetObjectBindingSet(clip);
        bindingSets.Add(clip, bindings);
        return bindings;
    }

    private static string GetBindingKey(EditorCurveBinding binding)
    {
        var typeName = binding.type != null ? binding.type.FullName : string.Empty;
        return $"{typeName}|{binding.path}|{binding.propertyName}";
    }

    private static bool TryGetDefaultFloatValue(Transform animatorRoot, EditorCurveBinding binding, out float value)
    {
        value = 0f;
        var target = ResolveTarget(animatorRoot, binding.path);
        if (target == null) return false;

        if (binding.type == typeof(Transform))
        {
            return TryGetTransformFloat(target, binding.propertyName, out value);
        }

        if (binding.type == typeof(GameObject))
        {
            return TryGetGameObjectFloat(target.gameObject, binding.propertyName, out value);
        }

        if (binding.type == null) return false;
        var component = target.GetComponent(binding.type);
        if (component == null) return false;

        if (component is Behaviour behaviour && binding.propertyName == "m_Enabled")
        {
            value = behaviour.enabled ? 1f : 0f;
            return true;
        }

        if (component is SpriteRenderer spriteRenderer && TryGetSpriteRendererFloat(spriteRenderer, binding.propertyName, out value))
        {
            return true;
        }

        return TryGetFieldFloat(component, binding.propertyName, out value);
    }

    private static bool TryGetDefaultObjectValue(Transform animatorRoot, EditorCurveBinding binding, out Object value)
    {
        value = null;
        var target = ResolveTarget(animatorRoot, binding.path);
        if (target == null) return false;

        if (binding.type == null) return false;
        var component = target.GetComponent(binding.type);
        if (component == null) return false;

        if (component is SpriteRenderer spriteRenderer && binding.propertyName == "m_Sprite")
        {
            value = spriteRenderer.sprite;
            return true;
        }

        return TryGetFieldObject(component, binding.propertyName, out value);
    }

    private static Transform ResolveTarget(Transform animatorRoot, string path)
    {
        return string.IsNullOrEmpty(path) ? animatorRoot : animatorRoot.Find(path);
    }

    private static bool TryGetTransformFloat(Transform transform, string propertyName, out float value)
    {
        value = 0f;
        switch (propertyName)
        {
            case "m_LocalPosition.x":
                value = transform.localPosition.x;
                return true;
            case "m_LocalPosition.y":
                value = transform.localPosition.y;
                return true;
            case "m_LocalPosition.z":
                value = transform.localPosition.z;
                return true;
            case "m_LocalScale.x":
                value = transform.localScale.x;
                return true;
            case "m_LocalScale.y":
                value = transform.localScale.y;
                return true;
            case "m_LocalScale.z":
                value = transform.localScale.z;
                return true;
            case "m_LocalRotation.x":
                value = transform.localRotation.x;
                return true;
            case "m_LocalRotation.y":
                value = transform.localRotation.y;
                return true;
            case "m_LocalRotation.z":
                value = transform.localRotation.z;
                return true;
            case "m_LocalRotation.w":
                value = transform.localRotation.w;
                return true;
        }

        if (propertyName.StartsWith("localEulerAnglesRaw.", StringComparison.Ordinal)
            || propertyName.StartsWith("m_LocalEulerAngles.", StringComparison.Ordinal))
        {
            return TryGetVectorComponent(transform.localEulerAngles, propertyName, out value);
        }

        return false;
    }

    private static bool TryGetGameObjectFloat(GameObject gameObject, string propertyName, out float value)
    {
        value = 0f;
        if (propertyName != "m_IsActive") return false;

        value = gameObject.activeSelf ? 1f : 0f;
        return true;
    }

    private static bool TryGetSpriteRendererFloat(SpriteRenderer spriteRenderer, string propertyName, out float value)
    {
        value = 0f;
        switch (propertyName)
        {
            case "m_Enabled":
                value = spriteRenderer.enabled ? 1f : 0f;
                return true;
            case "m_Color.r":
                value = spriteRenderer.color.r;
                return true;
            case "m_Color.g":
                value = spriteRenderer.color.g;
                return true;
            case "m_Color.b":
                value = spriteRenderer.color.b;
                return true;
            case "m_Color.a":
                value = spriteRenderer.color.a;
                return true;
            case "m_FlipX":
                value = spriteRenderer.flipX ? 1f : 0f;
                return true;
            case "m_FlipY":
                value = spriteRenderer.flipY ? 1f : 0f;
                return true;
            case "m_SortingOrder":
                value = spriteRenderer.sortingOrder;
                return true;
        }

        return false;
    }

    private static bool TryGetFieldFloat(Component component, string propertyName, out float value)
    {
        value = 0f;
        var fieldName = GetFieldName(propertyName);
        var field = GetField(component.GetType(), fieldName);
        if (field == null) return false;

        var rawValue = field.GetValue(component);
        if (TryConvertToFloat(rawValue, propertyName, out value)) return true;

        return false;
    }

    private static bool TryGetFieldObject(Component component, string propertyName, out Object value)
    {
        value = null;
        var field = GetField(component.GetType(), propertyName);
        if (field == null || !typeof(Object).IsAssignableFrom(field.FieldType)) return false;

        value = field.GetValue(component) as Object;
        return true;
    }

    private static FieldInfo GetField(Type type, string fieldName)
    {
        while (type != null)
        {
            var field = type.GetField(fieldName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (field != null) return field;
            type = type.BaseType;
        }

        return null;
    }

    private static string GetFieldName(string propertyName)
    {
        var dotIndex = propertyName.IndexOf('.');
        return dotIndex >= 0 ? propertyName.Substring(0, dotIndex) : propertyName;
    }

    private static bool TryConvertToFloat(object rawValue, string propertyName, out float value)
    {
        value = 0f;
        switch (rawValue)
        {
            case float floatValue:
                value = floatValue;
                return true;
            case int intValue:
                value = intValue;
                return true;
            case bool boolValue:
                value = boolValue ? 1f : 0f;
                return true;
            case Color color:
                return TryGetColorComponent(color, propertyName, out value);
            case Vector2 vector2:
                return TryGetVectorComponent(vector2, propertyName, out value);
            case Vector3 vector3:
                return TryGetVectorComponent(vector3, propertyName, out value);
            case Vector4 vector4:
                return TryGetVectorComponent(vector4, propertyName, out value);
            case Quaternion quaternion:
                return TryGetQuaternionComponent(quaternion, propertyName, out value);
            default:
                return false;
        }
    }

    private static bool TryGetVectorComponent(Vector2 vector, string propertyName, out float value)
    {
        value = 0f;
        if (propertyName.EndsWith(".x", StringComparison.Ordinal))
        {
            value = vector.x;
            return true;
        }

        if (propertyName.EndsWith(".y", StringComparison.Ordinal))
        {
            value = vector.y;
            return true;
        }

        return false;
    }

    private static bool TryGetVectorComponent(Vector3 vector, string propertyName, out float value)
    {
        value = 0f;
        if (propertyName.EndsWith(".x", StringComparison.Ordinal))
        {
            value = vector.x;
            return true;
        }

        if (propertyName.EndsWith(".y", StringComparison.Ordinal))
        {
            value = vector.y;
            return true;
        }

        if (propertyName.EndsWith(".z", StringComparison.Ordinal))
        {
            value = vector.z;
            return true;
        }

        return false;
    }

    private static bool TryGetVectorComponent(Vector4 vector, string propertyName, out float value)
    {
        value = 0f;
        if (propertyName.EndsWith(".x", StringComparison.Ordinal))
        {
            value = vector.x;
            return true;
        }

        if (propertyName.EndsWith(".y", StringComparison.Ordinal))
        {
            value = vector.y;
            return true;
        }

        if (propertyName.EndsWith(".z", StringComparison.Ordinal))
        {
            value = vector.z;
            return true;
        }

        if (propertyName.EndsWith(".w", StringComparison.Ordinal))
        {
            value = vector.w;
            return true;
        }

        return false;
    }

    private static bool TryGetColorComponent(Color color, string propertyName, out float value)
    {
        value = 0f;
        if (propertyName.EndsWith(".r", StringComparison.Ordinal))
        {
            value = color.r;
            return true;
        }

        if (propertyName.EndsWith(".g", StringComparison.Ordinal))
        {
            value = color.g;
            return true;
        }

        if (propertyName.EndsWith(".b", StringComparison.Ordinal))
        {
            value = color.b;
            return true;
        }

        if (propertyName.EndsWith(".a", StringComparison.Ordinal))
        {
            value = color.a;
            return true;
        }

        return false;
    }

    private static bool TryGetQuaternionComponent(Quaternion quaternion, string propertyName, out float value)
    {
        value = 0f;
        if (propertyName.EndsWith(".x", StringComparison.Ordinal))
        {
            value = quaternion.x;
            return true;
        }

        if (propertyName.EndsWith(".y", StringComparison.Ordinal))
        {
            value = quaternion.y;
            return true;
        }

        if (propertyName.EndsWith(".z", StringComparison.Ordinal))
        {
            value = quaternion.z;
            return true;
        }

        if (propertyName.EndsWith(".w", StringComparison.Ordinal))
        {
            value = quaternion.w;
            return true;
        }

        return false;
    }

    private static AnimationCurve MakeConstantCurve(AnimationClip clip, float value)
    {
        var endTime = Mathf.Max(clip.length, 1f / Mathf.Max(clip.frameRate, 1f));
        return AnimationCurve.Constant(0f, endTime, value);
    }

    private static ObjectReferenceKeyframe[] MakeConstantObjectCurve(AnimationClip clip, Object value)
    {
        var endTime = Mathf.Max(clip.length, 1f / Mathf.Max(clip.frameRate, 1f));
        return new[]
        {
            new ObjectReferenceKeyframe { time = 0f, value = value },
            new ObjectReferenceKeyframe { time = endTime, value = value }
        };
    }
}
