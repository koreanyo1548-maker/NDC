using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Spine;
using Spine.Unity;

#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteInEditMode]
public class SimpleSpineSkinAssigner : MonoBehaviour
{
    [SpineSkin(dataField: "skeletonDataAsset")] public string bodySkin;
    [SpineSkin(dataField: "skeletonDataAsset")] public string hairSkin;
    [SpineSkin(dataField: "skeletonDataAsset")] public string headSkin;
    [SpineSkin(dataField: "skeletonDataAsset")] public string leftHandWeaponSkin;
    [SpineSkin(dataField: "skeletonDataAsset")] public string rightHandWeaponSkin;

    private ISkeletonComponent skeletonComponent;
    private SkeletonDataAsset skeletonDataAsset;
    private Skin combinedSkin;

    void Start()
    {
        if (!Application.isPlaying) return;
        InitializeSkeleton();
        AssignSkins();
    }

    // ������ ��忡���� ����� �����ϵ��� �߰�
    void OnEnable()
    {
        if (Application.isPlaying) return;

        InitializeSkeleton();
        AssignSkins();
        UpdateSkeletonPose();

#if UNITY_EDITOR
        // Editor���� �� ������ ������Ʈ�� ���� ���̷����� ������ ������Ʈ
        EditorApplication.update += EditorUpdate;
#endif
    }

    void OnDisable()
    {
#if UNITY_EDITOR
        // Editor���� ������Ʈ �ݹ� ����
        EditorApplication.update -= EditorUpdate;
#endif
    }

    // �� �����Ӹ��� ���̷��� ���¸� �����ϴ� �޼���
    private void EditorUpdate()
    {
        if (!Application.isPlaying)
        {
            InitializeSkeleton();
            AssignSkins();
            UpdateSkeletonPose();
        }
    }

    void OnValidate()
    {
        InitializeSkeleton();
        AssignSkins();
        UpdateSkeletonPose();

#if UNITY_EDITOR
        EditorApplication.delayCall += () => EditorApplication.QueuePlayerLoopUpdate();
#endif
    }

    void InitializeSkeleton()
    {
        skeletonComponent = GetComponent<ISkeletonComponent>();
        skeletonDataAsset = (skeletonComponent as IHasSkeletonDataAsset)?.SkeletonDataAsset;

        if (skeletonComponent == null)
        {
            Debug.LogWarning("SkeletonAnimation or SkeletonMecanim component is missing. Please add one to the GameObject.");
            return;
        }

        if (skeletonDataAsset == null)
        {
            Debug.LogWarning("SkeletonDataAsset could not be found on the component. Please check the assigned component.");
            return;
        }

        if (skeletonComponent.Skeleton == null)
        {
            if (skeletonComponent is SkeletonAnimation skeletonAnimation)
            {
                skeletonAnimation.Initialize(true);  // ���̷����� ������ �ʱ�ȭ
            }
            else if (skeletonComponent is SkeletonMecanim skeletonMecanim)
            {
                skeletonMecanim.Initialize(true);  // ���̷����� ������ �ʱ�ȭ
            }
        }

        if (skeletonComponent.Skeleton == null)
        {
            Debug.LogWarning("Skeleton could not be initialized. Please check the SkeletonDataAsset.");
            return;
        }

        combinedSkin = new Skin("combinedSkin");
    }

    void AssignSkins()
    {
        if (skeletonDataAsset == null)
        {
            Debug.LogWarning("SkeletonDataAsset is missing. Please assign it in the inspector.");
            return;
        }

        var skeletonData = skeletonDataAsset.GetSkeletonData(true);
        if (skeletonData == null)
        {
            Debug.LogError("SkeletonData could not be loaded.");
            return;
        }

        if (combinedSkin == null)
        {
            combinedSkin = new Skin("combinedSkin");
        }
        else
        {
            combinedSkin.Clear();
        }

        AddSkinIfExists(bodySkin);
        AddSkinIfExists(hairSkin);
        AddSkinIfExists(headSkin);
        AddSkinIfExists(leftHandWeaponSkin);
        AddSkinIfExists(rightHandWeaponSkin);

        if (skeletonComponent.Skeleton != null)
        {
            skeletonComponent.Skeleton.SetSkin(combinedSkin);
            skeletonComponent.Skeleton.SetSlotsToSetupPose();
            UpdateSkeletonPose();
        }
    }

    void AddSkinIfExists(string skinName)
    {
        if (!string.IsNullOrEmpty(skinName))
        {
            var skin = skeletonComponent.Skeleton?.Data.FindSkin(skinName);
            if (skin != null)
            {
                combinedSkin.AddSkin(skin);
            }
            else
            {
                Debug.LogWarning($"Skin not found: {skinName}");
            }
        }
    }

    void UpdateSkeletonPose()
    {
        if (skeletonComponent != null && skeletonComponent.Skeleton != null)
        {
            skeletonComponent.Skeleton.SetSlotsToSetupPose();
            if (skeletonComponent is SkeletonAnimation skeletonAnimation)
            {
                skeletonAnimation.Update(0);  // ������ ������Ʈ
                skeletonAnimation.LateUpdate();  // LateUpdate ȣ��� ���̷��� ���¸� ����
            }
            else if (skeletonComponent is SkeletonMecanim skeletonMecanim)
            {
                skeletonMecanim.Update();  // ������ ������Ʈ
                skeletonMecanim.LateUpdate();  // LateUpdate ȣ��� ���̷��� ���¸� ����
            }
        }
    }
}
