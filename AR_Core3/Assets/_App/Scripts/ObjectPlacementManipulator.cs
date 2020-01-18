///---------------------------------------------------------------------
/// This is a modified version of file: 
/// <copyright file="AndyPlacementManipulator.cs" company="Google">
/// Please refer to this file for Copyright.
// </copyright>
//---------------------------------------------------------------------
namespace GoogleARCore.Examples.ObjectManipulation
{
    using GoogleARCore;
    using UnityEngine;

    /// <summary>
    /// Controls the placement of Andy objects via a tap gesture.
    /// </summary>
    public class ObjectPlacementManipulator : Manipulator
    {
        /// <summary>
        /// The first-person camera being used to render the passthrough camera image (i.e. AR
        /// background).
        /// </summary>
        public Camera FirstPersonCamera;

        /// <summary>
        /// A model to place when a raycast from a user touch hits a plane.
        /// </summary>
        public GameObject objectPrefab;

        /// <summary>
        /// Archor stop lists
        /// </summary>
        public GameObject Stops;


        /// <summary>
        /// Manipulator prefab to attach placed objects to.
        /// </summary>
        public GameObject ManipulatorPrefab;

        private int count = 1; // counter

        /// <summary>
        /// Returns true if the manipulation can be started for the given gesture.
        /// </summary>
        /// <param name="gesture">The current gesture.</param>
        /// <returns>True if the manipulation can be started.</returns>
        protected override bool CanStartManipulationForGesture(TapGesture gesture)
        {
            if (gesture.TargetObject == null)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Function called when the manipulation is ended.
        /// </summary>
        /// <param name="gesture">The current gesture.</param>
        protected override void OnEndManipulation(TapGesture gesture)
        {
            if (gesture.WasCancelled)
            {
                return;
            }

            // If gesture is targeting an existing object we are done.
            if (gesture.TargetObject != null)
            {
                return;
            }

            // Raycast against the location the player touched to search for planes.
            TrackableHit hit;
            TrackableHitFlags raycastFilter = TrackableHitFlags.PlaneWithinPolygon;

            if (Frame.Raycast(
                gesture.StartPosition.x, gesture.StartPosition.y, raycastFilter, out hit))
            {
                // Use hit pose and camera pose to check if hittest is from the
                // back of the plane, if it is, no need to create the anchor.
                if ((hit.Trackable is DetectedPlane) &&
                    Vector3.Dot(FirstPersonCamera.transform.position - hit.Pose.position,
                        hit.Pose.rotation * Vector3.up) < 0)
                {
                    Debug.Log("Hit at back of the current DetectedPlane");
                }
                else
                {
                    // Instantiate Andy model at the hit pose.
                    var andyObject = Instantiate(objectPrefab, hit.Pose.position, hit.Pose.rotation);
                    andyObject.transform.Find("TPawn").GetComponent<TextMesh>().text = ""+count;
                    count++;

                    // Instantiate manipulator.
                    var manipulator =
                        Instantiate(ManipulatorPrefab, hit.Pose.position, hit.Pose.rotation);

                    // Make Andy model a child of the manipulator.
                    andyObject.transform.parent = manipulator.transform;

                    // Create an anchor to allow ARCore to track the hitpoint as understanding of
                    // the physical world evolves.
                    var anchor = hit.Trackable.CreateAnchor(hit.Pose);

                    // Make manipulator a child of the anchor.
                    manipulator.transform.parent = anchor.transform;

                    // Set Anchor as child of stops  - AH
                    anchor.transform.parent = Stops.transform;

                    // Select the placed object.
                    manipulator.GetComponent<Manipulator>().Select();
                }
            }
        }
    }
}
