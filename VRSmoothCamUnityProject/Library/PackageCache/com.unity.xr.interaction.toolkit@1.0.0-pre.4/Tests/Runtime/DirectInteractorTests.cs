﻿using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine.TestTools;

namespace UnityEngine.XR.Interaction.Toolkit.Tests
{
    [TestFixture]
    class DirectInteractorTests
    {
        [TearDown]
        public void TearDown()
        {
            TestUtilities.DestroyAllSceneObjects();
        }

        [UnityTest]
        public IEnumerator DirectInteractorCanHoverInteractable()
        {
            var manager = TestUtilities.CreateInteractionManager();
            var interactable = TestUtilities.CreateGrabInteractable();
            var directInteractor = TestUtilities.CreateDirectInteractor();

            yield return new WaitForFixedUpdate();
            yield return null;

            var validTargets = new List<XRBaseInteractable>();
            manager.GetValidTargets(directInteractor, validTargets);
            Assert.That(validTargets, Has.Exactly(1).EqualTo(interactable));

            var hoverTargetList = new List<XRBaseInteractable>();
            directInteractor.GetHoverTargets(hoverTargetList);
            Assert.That(hoverTargetList, Has.Exactly(1).EqualTo(interactable));
        }

        [UnityTest]
        public IEnumerator DirectInteractorHandlesUnregisteredInteractable()
        {
            var manager = TestUtilities.CreateInteractionManager();
            var interactable = TestUtilities.CreateGrabInteractable();
            var directInteractor = TestUtilities.CreateDirectInteractor();

            yield return new WaitForFixedUpdate();
            yield return null;

            var validTargets = new List<XRBaseInteractable>();
            manager.GetValidTargets(directInteractor, validTargets);
            Assert.That(validTargets, Is.EqualTo(new[] { interactable }));
            directInteractor.GetValidTargets(validTargets);
            Assert.That(validTargets, Is.EqualTo(new[] { interactable }));

            var hoverTargets = new List<XRBaseInteractable>();
            directInteractor.GetHoverTargets(hoverTargets);
            Assert.That(hoverTargets, Is.EqualTo(new[] { interactable }));

            Object.Destroy(interactable);

            yield return null;
            Assert.That(interactable == null, Is.True);

            manager.GetValidTargets(directInteractor, validTargets);
            Assert.That(validTargets, Is.Empty);
            directInteractor.GetValidTargets(validTargets);
            Assert.That(validTargets, Is.Empty);

            directInteractor.GetHoverTargets(hoverTargets);
            Assert.That(hoverTargets, Is.Empty);
        }

        [UnityTest]
        public IEnumerator DirectInteractorCanSelectInteractable()
        {
            TestUtilities.CreateInteractionManager();
            var interactable = TestUtilities.CreateGrabInteractable();
            var directInteractor = TestUtilities.CreateDirectInteractor();
            var controller = directInteractor.GetComponent<XRController>();
            var controllerRecorder = TestUtilities.CreateControllerRecorder(controller, (recording) =>
            {
                recording.AddRecordingFrame(0.0f, Vector3.zero, Quaternion.identity,
                    true, false, false); 
                recording.AddRecordingFrame(float.MaxValue, Vector3.zero, Quaternion.identity,
                    true, false, false);
            });
            controllerRecorder.isPlaying = true;

            yield return new WaitForSeconds(0.1f);

            Assert.That(directInteractor.selectTarget, Is.EqualTo(interactable));
        }

        [UnityTest]
        public IEnumerator DirectInteractorHandlesCanceledInteractable()
        {
            var manager = TestUtilities.CreateInteractionManager();
            var interactable = TestUtilities.CreateGrabInteractable();
            var directInteractor = TestUtilities.CreateDirectInteractor();
            var controller = directInteractor.GetComponent<XRController>();
            var controllerRecorder = TestUtilities.CreateControllerRecorder(controller, (recording) =>
            {
                recording.AddRecordingFrame(0.0f, Vector3.zero, Quaternion.identity,
                    true, false, false); 
                recording.AddRecordingFrame(float.MaxValue, Vector3.zero, Quaternion.identity,
                    true, false, false);
            });
            controllerRecorder.isPlaying = true;

            yield return new WaitForSeconds(0.1f);

            Assert.That(directInteractor.selectTarget, Is.EqualTo(interactable));

            XRBaseInteractor canceledInteractor = null;
            XRBaseInteractable canceledInteractable = null;
            interactable.selectExited.AddListener(args => canceledInteractor = args.isCanceled ? args.interactor : null);
            directInteractor.selectExited.AddListener(args => canceledInteractable = args.isCanceled ? args.interactable : null);

            Object.Destroy(interactable);

            yield return null;
            // ReSharper disable once ConditionIsAlwaysTrueOrFalse -- overloaded Object operator==
            Assert.That(interactable == null, Is.True);
            Assert.That(directInteractor.selectTarget, Is.Null);

            Assert.That(canceledInteractor, Is.SameAs(directInteractor));
            Assert.That(canceledInteractable, Is.SameAs(interactable));

            var validTargets = new List<XRBaseInteractable>();
            manager.GetValidTargets(directInteractor, validTargets);
            Assert.That(validTargets, Is.Empty);

            var hoverTargetList = new List<XRBaseInteractable>();
            directInteractor.GetHoverTargets(hoverTargetList);
            Assert.That(hoverTargetList, Is.Empty);
        }

        [UnityTest]
        public IEnumerator DirectInteractorCanPassToAnother()
        {
            TestUtilities.CreateInteractionManager();
            var interactable = TestUtilities.CreateGrabInteractable();

            var directInteractor1 = TestUtilities.CreateDirectInteractor();
            directInteractor1.name = "directInteractor1";
            var controller1 = directInteractor1.GetComponent<XRController>();
            var controllerRecorder1 = TestUtilities.CreateControllerRecorder(controller1, (recording) =>
            {
                recording.AddRecordingFrame(0.0f, Vector3.zero, Quaternion.identity,
                    false, false, false);
                recording.AddRecordingFrame(0.1f, Vector3.zero, Quaternion.identity,
                    true, false, false);
                recording.AddRecordingFrame(0.2f, Vector3.zero, Quaternion.identity,
                    true, false, false);
                recording.AddRecordingFrame(0.3f, Vector3.zero, Quaternion.identity,
                    true, false, false);
            });

            var directInteractor2 = TestUtilities.CreateDirectInteractor();
            directInteractor2.name = "directInteractor2";
            var controller2 = directInteractor2.GetComponent<XRController>();
            var controllerRecorder2 = TestUtilities.CreateControllerRecorder(controller2, (recording) =>
            {
                recording.AddRecordingFrame(0.0f, Vector3.zero, Quaternion.identity,
                    false, false, false);
                recording.AddRecordingFrame(0.1f, Vector3.zero, Quaternion.identity,
                    false, false, false);
                recording.AddRecordingFrame(0.2f, Vector3.zero, Quaternion.identity,
                    true, false, false);
                recording.AddRecordingFrame(0.3f, Vector3.zero, Quaternion.identity,
                    true, false, false);
            });

            controllerRecorder1.isPlaying = true;
            controllerRecorder2.isPlaying = true;

            yield return new WaitForSeconds(0.1f);
  
            // directInteractor1 grabs the interactable
            Assert.That(interactable.selectingInteractor, Is.EqualTo(directInteractor1), "In first frame, controller 1 should grab the interactable. Instead got " + interactable.selectingInteractor.name);

            // Wait for the proper interaction that signifies the handoff
            yield return new WaitForSeconds(0.2f);
            
            // directInteractor2 grabs the interactable from directInteractor1
            Assert.That(interactable.selectingInteractor, Is.EqualTo(directInteractor2), "In second frame, controller 2 should grab the interactable. Instead got " + interactable.selectingInteractor.name);
        }
    }
}
