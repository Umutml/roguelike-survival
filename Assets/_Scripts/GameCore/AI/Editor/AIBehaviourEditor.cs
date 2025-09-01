//TODO : This script is not used in the project. It is a custom editor script for WaveData Scriptable Object.
//TODO : Rework this script to be used in the editor. @Batuhan Kanbur
// #if UNITY_EDITOR
// using System.Collections.Generic;
// using GameCore.Scriptables;
// using UnityEditor;
// using UnityEngine;
// [CustomEditor(typeof(WaveData))]
// public class AIBehaviourEditor : Editor
// {
//     private WaveData _waveData;
//     private AnimationCurve _attackerCurve = new AnimationCurve();
//     private AnimationCurve _petrolCurve = new AnimationCurve();
//     private AnimationCurve _waitingCurve = new AnimationCurve();
//     private void OnEnable()
//     {
//         if (_waveData == null)
//         {
//             _waveData = (WaveData)target;
//             SetDefaultCurves(out _attackerCurve, out _petrolCurve, out _waitingCurve);
//         }
//     }
//     private void SetDefaultCurves(out AnimationCurve attackerCurve, out AnimationCurve petrolCurve, out AnimationCurve waitingCurve)
//     {
//         var attackCurveKeyframes = new List<Keyframe>();
//         var petrolCurveKeyframes = new List<Keyframe>();
//         var waitingCurveKeyframes = new List<Keyframe>();
//         for (int i = 0; i < _waveData.waves.Length; i++)
//         {
//             var attackKeyframe = new Keyframe(i, 0);
//             var petrolKeyframe = new Keyframe(i, 0);
//             var waitingKeyframe = new Keyframe(i, 0);
//             foreach (var behaviorState in _waveData.waves[i].behaviorStates)
//             {
//                 switch (behaviorState.behaviorType)
//                 {
//                     case BehaviorType.Attacker:
//                         attackKeyframe.value += behaviorState.probability;
//                         break;
//                     case BehaviorType.Patrolling:
//                         petrolKeyframe.value += behaviorState.probability;
//                         break;
//                     case BehaviorType.Waiting:
//                         waitingKeyframe.value += behaviorState.probability;
//                         break;
//                 }
//             }
//             attackCurveKeyframes.Add(attackKeyframe);
//             petrolCurveKeyframes.Add(petrolKeyframe);
//             waitingCurveKeyframes.Add(waitingKeyframe);
//         }
//         attackerCurve = NormalizeCurve(new AnimationCurve(attackCurveKeyframes.ToArray()));
//         petrolCurve = NormalizeCurve(new AnimationCurve(petrolCurveKeyframes.ToArray()));
//         waitingCurve = NormalizeCurve(new AnimationCurve(waitingCurveKeyframes.ToArray()));
//     }
//     public override void OnInspectorGUI()
//     {
//         DrawDefaultInspector();
//         _attackerCurve = EditorGUILayout.CurveField("Attacker Curve", _attackerCurve);
//         _petrolCurve = EditorGUILayout.CurveField("Petrol Curve", _petrolCurve);
//         _waitingCurve = EditorGUILayout.CurveField("Waiting Curve", _waitingCurve);
//         if (GUILayout.Button("Set Behaviour State"))
//             SetBehaviourState();
//         EditorUtility.SetDirty(_waveData);
//     }
//
//     private void SetBehaviourState()
//     {
//         foreach (var wave in _waveData.waves)
//         {
//             wave.behaviorStates.Clear();
//         }
//         var attackerCurve = ScaleCurve(_attackerCurve);
//         var petrolCurve = ScaleCurve(_petrolCurve);
//         var waitingCurve = ScaleCurve(_waitingCurve);
//         for (int i = 0; i < attackerCurve.length; i++)
//         {
//             _waveData.waves[i].behaviorStates.Add(new SpawnBehaviorState
//             {
//                 behaviorType = BehaviorType.Attacker,
//                 probability = (int)attackerCurve.keys[i].value
//             });
//         }
//         for (int i = 0; i < petrolCurve.length; i++)
//         {
//             _waveData.waves[i].behaviorStates.Add(new SpawnBehaviorState
//             {
//                 behaviorType = BehaviorType.Patrolling,
//                 probability = (int)petrolCurve.keys[i].value
//             });
//         }
//         for (int i = 0; i < waitingCurve.length; i++)
//         {
//             _waveData.waves[i].behaviorStates.Add(new SpawnBehaviorState
//             {
//                 behaviorType = BehaviorType.Waiting,
//                 probability = (int)waitingCurve.keys[i].value
//             });
//         }
//
//         _attackerCurve = attackerCurve;
//         _petrolCurve = petrolCurve;
//         _waitingCurve = waitingCurve;
//         SetDefaultCurves(out _attackerCurve, out _petrolCurve, out _waitingCurve);
//     }
//
//     private AnimationCurve NormalizeCurve(AnimationCurve curve)
//     {
//         if (curve == null || curve.keys.Length == 0)
//             return curve;
//         var newCurve = new AnimationCurve();
//         for (int i = 0; i < curve.keys.Length; i++)
//         {
//             newCurve.AddKey(new Keyframe((float)i/_waveData.waves.Length, curve.keys[i].value * 0.01f));
//         }
//         return newCurve;
//     }
//     private AnimationCurve ScaleCurve(AnimationCurve curve)
//     {
//         if (curve == null || curve.keys.Length == 0)
//             return curve;
//         var newCurve = new AnimationCurve();
//         for (int i = 0; i < _waveData.waves.Length; i++)
//         {
//             newCurve.AddKey(new Keyframe(i, curve.Evaluate((float)i/_waveData.waves.Length) * 100f));
//         }
//         return newCurve;
//     }
// }
// #endif
