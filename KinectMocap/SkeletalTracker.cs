﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Kinect;

namespace KinectMocap {

    class SkeletonSensor {
        public KinectSensor kinect;
        public Skeleton[] skeletonData;
    };

    class SkeletalTracker {
        private SkeletonSensor[] sensors;
        private const int NUM_SENSORS = 3;
        //private KinectSensor kinect;
        //private Skeleton[] skeletonData;

        public void StartKinectST() {
            sensors = new SkeletonSensor[NUM_SENSORS];

            for (int i = 0; i < NUM_SENSORS; i++)
                sensors[i] = new SkeletonSensor();

            for ( int i = 0, j = 0; i < KinectSensor.KinectSensors.Count; i++ ) {
                if (j >= NUM_SENSORS)
                    break;

                //TODO: Initialization of kinects is not correct. Might be setting all three to same kinect
                if (KinectSensor.KinectSensors[i].Status == KinectStatus.Connected) {
                    sensors[j].kinect = KinectSensor.KinectSensors[i];
                    j++;
                }
            }

            for (int i = 0; i < sensors.Length; i++) {
                sensors[i].kinect = KinectSensor.KinectSensors.FirstOrDefault(s => s.Status == KinectStatus.Connected); // Get first Kinect Sensor
                sensors[i].kinect.SkeletonStream.Enable(); // Enable skeletal tracking

                sensors[i].skeletonData = new Skeleton[sensors[i].kinect.SkeletonStream.FrameSkeletonArrayLength]; // Allocate ST data

                sensors[i].kinect.SkeletonFrameReady += new EventHandler<SkeletonFrameReadyEventArgs>(kinect_SkeletonFrameReady); // Get Ready for Skeleton Ready Events

                sensors[i].kinect.Start(); // Start Kinect sensor
            }
        }

        private void kinect_SkeletonFrameReady(object sender, SkeletonFrameReadyEventArgs e) {
            using (SkeletonFrame skeletonFrame = e.OpenSkeletonFrame()) { // Open the Skeleton frame

                SkeletonSensor temp = null;

                foreach (SkeletonSensor sensor in sensors) {
                    if (sender == sensor.kinect) {
                        temp = sensor;
                        break;
                    }
                }

                if (temp == null) {
                    Console.WriteLine("sender didn't match any active kinects");
                    return;
                }

                if (skeletonFrame != null && temp.skeletonData != null) // check that a frame is available
                    skeletonFrame.CopySkeletonDataTo(temp.skeletonData); // get the skeletal information in this frame
            }
        }

        public void Debug() {
            for( int i = 0, j = NUM_SENSORS; i < j; i++ ) {
                Console.WriteLine("Sensor " + i.ToString() + ":");
                Console.WriteLine(sensors[i].kinect.UniqueKinectId.ToString());

                if (sensors[i] != null){

                    if( sensors[i].kinect.Status == KinectStatus.Connected ) {

                        for (int k = 0, m = 6; k < m; k++) {
                            if( sensors[i].skeletonData[k] != null )
                                Console.WriteLine("Skeleton " + k.ToString() + "- " + sensors[i].skeletonData[k].TrackingState.ToString());
                        }
                    } else {
                         Console.WriteLine( "Error- " + sensors[i].kinect.Status.ToString() );
                    }
                }

                Console.Write("\n");
            }
        }

        //private void DrawSkeletons() {
        //    foreach (Skeleton skeleton in this.skeletonData) {
        //        if (skeleton.TrackingState == SkeletonTrackingState.Tracked) {
        //            DrawTrackedSkeletonJoints(skeleton.Joints);
        //        }
        //        else if (skeleton.TrackingState == SkeletonTrackingState.PositionOnly) {
        //            //DrawSkeletonPosition(skeleton.Position);
        //        }
        //    }
        //}

        private void DrawTrackedSkeletonJoints(JointCollection jointCollection)
        {
            // Render Head and Shoulders
            DrawBone(jointCollection[JointType.Head], jointCollection[JointType.ShoulderCenter]);
            DrawBone(jointCollection[JointType.ShoulderCenter], jointCollection[JointType.ShoulderLeft]);
            DrawBone(jointCollection[JointType.ShoulderCenter], jointCollection[JointType.ShoulderRight]);

            // Render Left Arm
            DrawBone(jointCollection[JointType.ShoulderLeft], jointCollection[JointType.ElbowLeft]);
            DrawBone(jointCollection[JointType.ElbowLeft], jointCollection[JointType.WristLeft]);
            DrawBone(jointCollection[JointType.WristLeft], jointCollection[JointType.HandLeft]);

            // Render Right Arm
            DrawBone(jointCollection[JointType.ShoulderRight], jointCollection[JointType.ElbowRight]);
            DrawBone(jointCollection[JointType.ElbowRight], jointCollection[JointType.WristRight]);
            DrawBone(jointCollection[JointType.WristRight], jointCollection[JointType.HandRight]);

            // Render other bones...
        }

        private void DrawBone(Joint jointFrom, Joint jointTo) {
            if (jointFrom.TrackingState == JointTrackingState.NotTracked ||
            jointTo.TrackingState == JointTrackingState.NotTracked) {
                return; // nothing to draw, one of the joints is not tracked
            }

            if (jointFrom.TrackingState == JointTrackingState.Inferred ||
            jointTo.TrackingState == JointTrackingState.Inferred) {
                //DrawNonTrackedBoneLine(jointFrom.Position, jointTo.Position);  // Draw thin lines if either one of the joints is inferred
            }

            if (jointFrom.TrackingState == JointTrackingState.Tracked &&
            jointTo.TrackingState == JointTrackingState.Tracked) {
                //DrawTrackedBoneLine(jointFrom.Position, jointTo.Position);  // Draw bold lines if the joints are both tracked
            }
        }

        private void RenderClippedEdges(Skeleton skeleton) {
            if (skeleton.ClippedEdges.HasFlag(FrameEdges.Bottom)) {
                //DrawClippedEdges(FrameEdges.Bottom); // Make the border red to show the user is reaching the border
            }

            if (skeleton.ClippedEdges.HasFlag(FrameEdges.Top)) {
                //DrawClippedEdges(FrameEdges.Top);
            }

            if (skeleton.ClippedEdges.HasFlag(FrameEdges.Left)) {
                //DrawClippedEdges(FrameEdges.Left);
            }

            if (skeleton.ClippedEdges.HasFlag(FrameEdges.Right)) {
                //DrawClippedEdges(FrameEdges.Right);
            }
        }

        //private void TrackClosestSkeleton() {
        //    if (this.kinect != null && this.kinect.SkeletonStream != null) {
        //        if (!this.kinect.SkeletonStream.AppChoosesSkeletons) {
        //            this.kinect.SkeletonStream.AppChoosesSkeletons = true; // Ensure AppChoosesSkeletons is set
        //        }

        //        float closestDistance = 10000f; // Start with a far enough distance
        //        int closestID = 0;

        //        foreach (Skeleton skeleton in this.skeletonData.Where(s => s.TrackingState != SkeletonTrackingState.NotTracked)) {
        //            if (skeleton.Position.Z < closestDistance)
        //            {
        //                closestID = skeleton.TrackingId;
        //                closestDistance = skeleton.Position.Z;
        //            }
        //        }

        //        if (closestID > 0) {
        //            this.kinect.SkeletonStream.ChooseSkeletons(closestID); // Track this skeleton
        //        }
        //    }
        //}

        //private void FindPlayerInDepthPixel(short[] depthFrame) {
        //    foreach (short depthPixel in depthFrame) {
        //        int player = depthPixel & DepthImageFrame.PlayerIndexBitmask;

        //        if (player > 0 && this.skeletonData != null) {
        //            Skeleton skeletonAtPixel = this.skeletonData[player - 1];   // Found the player at this pixel
        //            // ...
        //        }
        //    }
        //}
    }
}
