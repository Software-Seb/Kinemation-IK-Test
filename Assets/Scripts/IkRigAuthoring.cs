using Unity.Collections;
using Unity.Entities;
using UnityEngine;

public class IkRigAuthoring : MonoBehaviour {

    public string rightRootBoneName;
    public string rightMidBoneName;
    public string rightTipBoneName;

    public string leftRootBoneName;
    public string leftMidBoneName;
    public string leftTipBoneName;

    public GameObject rightTargetGameObject;
    public GameObject leftTargetGameObject;

    public float positionWeight;
    public float rotationWeight;
    public float hintWeight;

    public float smoothnessSpeed;

    public class Baker : Baker<IkRigAuthoring> {
        public override void Bake(IkRigAuthoring authoring) {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new IkRig {
                rightRootBoneName = authoring.rightRootBoneName,
                rightMidBoneName = authoring.rightMidBoneName,
                rightTipBoneName = authoring.rightTipBoneName,
                leftRootBoneName = authoring.leftRootBoneName,
                leftMidBoneName = authoring.leftMidBoneName,
                leftTipBoneName = authoring.leftTipBoneName,
                rightTargetEntity = GetEntity(authoring.rightTargetGameObject, TransformUsageFlags.Dynamic),
                leftTargetEntity = GetEntity(authoring.leftTargetGameObject, TransformUsageFlags.Dynamic),
                positionWeight = authoring.positionWeight,
                rotationWeight = authoring.rotationWeight,
                hintWeight = authoring.hintWeight,
                smoothnessSpeed = authoring.smoothnessSpeed,
            });
        }
    }

}

public struct IkRig : IComponentData {

    public FixedString32Bytes rightRootBoneName;
    public FixedString32Bytes rightMidBoneName;
    public FixedString32Bytes rightTipBoneName;

    public FixedString32Bytes leftRootBoneName;
    public FixedString32Bytes leftMidBoneName;
    public FixedString32Bytes leftTipBoneName;

    public Entity rightTargetEntity;
    public Entity leftTargetEntity;

    public float positionWeight;
    public float rotationWeight;
    public float hintWeight;

    public float smoothnessSpeed;
}