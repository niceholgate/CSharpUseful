// // using Microsoft.VisualStudio.TestTools.UnitTesting;
// using System;
// using System.Collections.Generic;
// using System.Linq;
// using System.Text;
// using System.Threading.Tasks;
//
// namespace NicUtils.FiniteStateMachines {
//     public class PathFinder {
//         private readonly FiniteStateMachine<PathFinderState, PathFinderEvent> stateMachine;
//
//         private enum PathFinderState {
//             Pathing,
//             NoTarget,
//             RecalcSubPath,
//             CalcFullPath,
//             NoPathExists
//         }
//
//         private enum PathFinderEvent {
//             DeleteTarget,
//             NewTarget,
//             CalcFailed,
//             CalcFinished,
//             StalePath,
//             BlockedPath,
//             GoodSubPath,
//             BadSubPath,
//             Collision
//
//         }
//
//         private readonly Dictionary<(PathFinderState currentState, PathFinderEvent evnt), (PathFinderState newState, Action action)> stateTransitions;
//
//         public PathFinder() {
//             stateTransitions = new() {
//                     { (PathFinderState.Pathing, PathFinderEvent.BlockedPath), (PathFinderState.RecalcSubPath, DoNothing) },
//                     { (PathFinderState.Pathing, PathFinderEvent.NewTarget), (PathFinderState.CalcFullPath, DoNothing) },
//                     { (PathFinderState.Pathing, PathFinderEvent.StalePath), (PathFinderState.RecalcSubPath, DoNothing) },
//                     { (PathFinderState.NoTarget, PathFinderEvent.NewTarget), (PathFinderState.CalcFullPath, DoNothing) },
//                     // TODO: complete these
//             };
//             stateMachine = new(stateTransitions, PathFinderState.NoTarget, null);
//         }
//
//         private void DoNothing() { }
//
//     }
//
//    
// }
