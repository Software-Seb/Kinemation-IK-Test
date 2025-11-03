using AclUnity;
using Latios.Kinemation;
using Latios.Transforms;
using System;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using Unity.Transforms;

partial struct IkCharacterTestSystem : ISystem {


    //[BurstCompile]
    public void OnUpdate(ref SystemState state) {

        foreach ((
            RefRO<IkRig> ikRig, Entity entity)
            in SystemAPI.Query<
                RefRO<IkRig>>().WithEntityAccess()) {

            RefRO<SkeletonBindingPathsBlobReference> skeletonBindingPathsBlobReference = SystemAPI.GetComponentRO<SkeletonBindingPathsBlobReference>(entity);

            OptimizedSkeletonAspect optimizedSkeletonAspect = SystemAPI.GetAspect<OptimizedSkeletonAspect>(entity);

            if (!skeletonBindingPathsBlobReference.ValueRO.blob.Value.TryGetFirstPathIndexThatStartsWith(ikRig.ValueRO.rightTipBoneName,
               out var rightTipBoneidx))
                continue;

            if (!skeletonBindingPathsBlobReference.ValueRO.blob.Value.TryGetFirstPathIndexThatStartsWith(ikRig.ValueRO.rightMidBoneName,
                    out var rightMidBoneidx))
                continue;


            if (!skeletonBindingPathsBlobReference.ValueRO.blob.Value.TryGetFirstPathIndexThatStartsWith(ikRig.ValueRO.rightRootBoneName,
                    out var rightRootBoneidx))
                continue;


            if (!skeletonBindingPathsBlobReference.ValueRO.blob.Value.TryGetFirstPathIndexThatStartsWith(ikRig.ValueRO.leftTipBoneName,
               out var leftTipBoneidx))
                continue;

            if (!skeletonBindingPathsBlobReference.ValueRO.blob.Value.TryGetFirstPathIndexThatStartsWith(ikRig.ValueRO.leftMidBoneName,
                    out var leftMidBoneidx))
                continue;


            if (!skeletonBindingPathsBlobReference.ValueRO.blob.Value.TryGetFirstPathIndexThatStartsWith(ikRig.ValueRO.leftRootBoneName,
                    out var leftRootBoneidx))
                continue;


            OptimizedBone rightRootBone = optimizedSkeletonAspect.bones[rightRootBoneidx];
            OptimizedBone rightMidBone = optimizedSkeletonAspect.bones[rightMidBoneidx];
            OptimizedBone rightTipBone = optimizedSkeletonAspect.bones[rightTipBoneidx];

            if (rightTipBone.parent.index != rightMidBone.index || rightMidBone.parent.index != rightRootBone.index) {
                Debug.LogError("Right bones are not in a valid hierarchy.");
                return;
            }

            OptimizedBone leftRootBone = optimizedSkeletonAspect.bones[leftRootBoneidx];
            OptimizedBone leftMidBone = optimizedSkeletonAspect.bones[leftMidBoneidx];
            OptimizedBone leftTipBone = optimizedSkeletonAspect.bones[leftTipBoneidx];

            if (leftTipBone.parent.index != leftMidBone.index || leftMidBone.parent.index != leftRootBone.index) {
                Debug.LogError("Right bones are not in a valid hierarchy.");
                return;
            }

            // Solve 2 bone IK for Right Arm 

            float3 rightHint = UnityRig.SolveHintPositionForTwoBoneIK(rightRootBone.worldPosition, rightMidBone.worldPosition, rightTipBone.worldPosition);

            TransformQvvs rightRootQvvsTransform = new TransformQvvs {
                position = rightRootBone.worldPosition,
                rotation = rightRootBone.worldRotation,
            };

            TransformQvvs rightMidQvvsTransform = new TransformQvvs {
                position = rightMidBone.worldPosition,
                rotation = rightMidBone.worldRotation,
            };

            TransformQvvs rightTipQvvsTransform = new TransformQvvs {
                position = rightTipBone.worldPosition,
                rotation = rightTipBone.worldRotation,
            };

            RefRO<LocalTransform> rightTargetLocalTransform = SystemAPI.GetComponentRO<LocalTransform>(ikRig.ValueRO.rightTargetEntity);

            TransformQvvs rightTargetQvvsTransform = new TransformQvvs {
                position = rightTargetLocalTransform.ValueRO.Position,
                rotation = rightTargetLocalTransform.ValueRO.Rotation,
            };

            UnityRig.SolveTwoBoneIK(ref rightRootQvvsTransform, ref rightMidQvvsTransform, ref rightTipQvvsTransform, rightTargetQvvsTransform, rightHint, ikRig.ValueRO.positionWeight, ikRig.ValueRO.rotationWeight, ikRig.ValueRO.hintWeight);

            // Solve 2 bone IK for Left Arm 

            float3 leftHint = UnityRig.SolveHintPositionForTwoBoneIK(leftRootBone.worldPosition, leftMidBone.worldPosition, leftTipBone.worldPosition);


            TransformQvvs leftRootQvvsTransform = new TransformQvvs {
                position = leftRootBone.worldPosition,
                rotation = leftRootBone.worldRotation,
            };

            TransformQvvs leftMidQvvsTransform = new TransformQvvs {
                position = leftMidBone.worldPosition,
                rotation = leftMidBone.worldRotation,
            };

            TransformQvvs leftTipQvvsTransform = new TransformQvvs {
                position = leftTipBone.worldPosition,
                rotation = leftTipBone.worldRotation,
            };

            RefRO<LocalTransform> leftTargetLocalTransform = SystemAPI.GetComponentRO<LocalTransform>(ikRig.ValueRO.leftTargetEntity);

            TransformQvvs leftTargetQvvsTransform = new TransformQvvs {
                position = leftTargetLocalTransform.ValueRO.Position,
                rotation = leftTargetLocalTransform.ValueRO.Rotation,
            };


            UnityRig.SolveTwoBoneIK(ref leftRootQvvsTransform, ref leftMidQvvsTransform, ref leftTipQvvsTransform, leftTargetQvvsTransform, leftHint, ikRig.ValueRO.positionWeight, ikRig.ValueRO.rotationWeight, ikRig.ValueRO.hintWeight);

            bool rightResultIsValid = math.all(math.isfinite(rightRootQvvsTransform.position)) &&
            math.all(math.isfinite(rightRootQvvsTransform.rotation.value)) &&
            math.all(math.isfinite(rightMidQvvsTransform.position)) &&
            math.all(math.isfinite(rightMidQvvsTransform.rotation.value)) &&
            math.all(math.isfinite(rightTipQvvsTransform.position)) &&
            math.all(math.isfinite(rightTipQvvsTransform.rotation.value));

            bool leftResultIsValid = math.all(math.isfinite(leftRootQvvsTransform.position)) &&
            math.all(math.isfinite(leftRootQvvsTransform.rotation.value)) &&
            math.all(math.isfinite(leftMidQvvsTransform.position)) &&
            math.all(math.isfinite(leftMidQvvsTransform.rotation.value)) &&
            math.all(math.isfinite(leftTipQvvsTransform.position)) &&
            math.all(math.isfinite(leftTipQvvsTransform.rotation.value));

            if (!leftResultIsValid) {
                Debug.LogError("The Left Arm has produced poisoned results: ");
                Debug.LogWarning("=== INPUTS ===");
                Debug.LogWarning("leftRootBone.worldPosition: " + leftRootBone.worldPosition + " leftRootBone.worldRotation: " + leftRootBone.worldRotation);
                Debug.LogWarning("leftMidBone.worldPosition: " + leftMidBone.worldPosition + " leftMidBone.worldRotation: " + leftMidBone.worldRotation);
                Debug.LogWarning("leftTipBone.worldPosition: " + leftTipBone.worldPosition + " leftTipBone.worldRotation: " + leftTipBone.worldRotation);
                Debug.LogWarning("leftTargetQvvsTransform.position: " + leftTargetQvvsTransform.position + " leftTargetQvvsTransform.rotation: " + leftTargetQvvsTransform.rotation);
                Debug.LogWarning("leftHint: " + leftHint);
                Debug.LogWarning("=== OUTPUTS ===");
                Debug.LogWarning("leftRootQvvsTransform.position: " + leftRootQvvsTransform.position + " leftRootQvvsTransform.rotation: " + leftRootQvvsTransform.rotation);
                Debug.LogWarning("leftMidQvvsTransform.position: " + leftMidQvvsTransform.position + " leftMidQvvsTransform.rotation: " + leftMidQvvsTransform.rotation);
                Debug.LogWarning("leftTipQvvsTransform.position: " + leftTipQvvsTransform.position + " leftTipQvvsTransform.rotation: " + leftTipQvvsTransform.rotation);
            }

            if (!rightResultIsValid) {
                Debug.LogError("The Right Arm has produced poisoned results: ");
                Debug.LogWarning("=== INPUTS ===");
                Debug.LogWarning("rightRootBone.worldPosition: " + rightRootBone.worldPosition + " rightRootBone.worldRotation: " + rightRootBone.worldRotation);
                Debug.LogWarning("rightMidBone.worldPosition: " + rightMidBone.worldPosition + " rightMidBone.worldRotation: " + rightMidBone.worldRotation);
                Debug.LogWarning("rightTipBone.worldPosition: " + rightTipBone.worldPosition + " rightTipBone.worldRotation: " + rightTipBone.worldRotation);
                Debug.LogWarning("rightTargetQvvsTransform.position: " + rightTargetQvvsTransform.position + " rightTargetQvvsTransform.rotation: " + rightTargetQvvsTransform.rotation);
                Debug.LogWarning("rightHint: " + rightHint);
                Debug.LogWarning("=== OUTPUTS ===");
                Debug.LogWarning("rightRootQvvsTransform.position: " + rightRootQvvsTransform.position + " rightRootQvvsTransform.rotation: " + rightRootQvvsTransform.rotation);
                Debug.LogWarning("rightMidQvvsTransform.position: " + rightMidQvvsTransform.position + " rightMidQvvsTransform.rotation: " + rightMidQvvsTransform.rotation);
                Debug.LogWarning("rightTipQvvsTransform.position: " + rightTipQvvsTransform.position + " rightTipQvvsTransform.rotation: " + rightTipQvvsTransform.rotation);
            }


            leftRootBone.worldPosition = math.lerp(leftRootBone.worldPosition, leftRootQvvsTransform.position, ikRig.ValueRO.smoothnessSpeed);
            leftRootBone.worldRotation = math.slerp(leftRootBone.worldRotation, leftRootQvvsTransform.rotation, ikRig.ValueRO.smoothnessSpeed);

            leftMidBone.worldPosition = math.lerp(leftMidBone.worldPosition, leftMidQvvsTransform.position, ikRig.ValueRO.smoothnessSpeed);
            leftMidBone.worldRotation = math.slerp(leftMidBone.worldRotation, leftMidQvvsTransform.rotation, ikRig.ValueRO.smoothnessSpeed);

            leftTipBone.worldPosition = math.lerp(leftTipBone.worldPosition, leftTipQvvsTransform.position, ikRig.ValueRO.smoothnessSpeed);
            leftTipBone.worldRotation = math.slerp(leftTipBone.worldRotation, leftTipQvvsTransform.rotation, ikRig.ValueRO.smoothnessSpeed);

            rightRootBone.worldPosition = math.lerp(rightRootBone.worldPosition, rightRootQvvsTransform.position, ikRig.ValueRO.smoothnessSpeed);
            rightRootBone.worldRotation = math.slerp(rightRootBone.worldRotation, rightRootQvvsTransform.rotation, ikRig.ValueRO.smoothnessSpeed);

            rightMidBone.worldPosition = math.lerp(rightMidBone.worldPosition, rightMidQvvsTransform.position, ikRig.ValueRO.smoothnessSpeed);
            rightMidBone.worldRotation = math.slerp(rightMidBone.worldRotation, rightMidQvvsTransform.rotation, ikRig.ValueRO.smoothnessSpeed);

            rightTipBone.worldPosition = math.lerp(rightTipBone.worldPosition, rightTipQvvsTransform.position, ikRig.ValueRO.smoothnessSpeed);
            rightTipBone.worldRotation = math.slerp(rightTipBone.worldRotation, rightTipQvvsTransform.rotation, ikRig.ValueRO.smoothnessSpeed);
            
        }

    }

}

