﻿using UnityEngine;
using System.Collections;
using System.IO;
using System;


namespace FBCapture
{
    public class CaptureOption : MonoBehaviour
    {      
        [Header("Capture Option")]
        public bool doSurroundCapture;

        [Header("Capture Hotkeys")]
        public KeyCode screenShotKey = KeyCode.None;
        public KeyCode encodingStartShotKey = KeyCode.None;  
        public KeyCode encodingStopShotKey = KeyCode.None;  

        [Header("Image and Video Size")]
        public int screenShotWidth = 8192;
        public int screenShotHeight = 4096;
        public int surroundVideoWidth = 3840;
        public int viewVideoWidth = 2160;
        public int videoHeight = 2160;
               
        private SurroundCapture surroundCapture = null;
        private NonSurroundCapture nonSurroundCapture = null;

        // [RMS] added this option
        public bool TrackMainCamera = false;

        public float TrackingAlpha = 0.1f;

        public string outputPath;  // Path where created files will be saved 
        private bool liveStreaming = false; // Set false by force because not fully implemented
        
        void Start()
        {            
            if (string.IsNullOrEmpty(outputPath)) {
                outputPath = System.IO.Path.Combine(Directory.GetCurrentDirectory(), "Gallery");
                // create the directory
                if (!Directory.Exists(outputPath)) {
                    Directory.CreateDirectory(outputPath);
                }
            }

            surroundCapture = GetComponent<SurroundCapture>();
            nonSurroundCapture = GetComponent<NonSurroundCapture>();

            if (doSurroundCapture) {               
                surroundCapture.enabled = true;
                nonSurroundCapture.enabled = false;
                surroundCapture.isLiveStreaming = liveStreaming;
            }
            else {
                nonSurroundCapture.enabled = true;
                surroundCapture.enabled = false;
                nonSurroundCapture.isLiveStreaming = liveStreaming;
            }
        }

        void Update()
        {

            // [RMS] added this tracking
            if (doSurroundCapture) { 
                if (surroundCapture.IsEncoding() == false) {
                    var found = GameObject.Find("FBCaptureEncoder");
                    if (found != null)
                        found.transform.position = Camera.main.transform.position;
                }
            } else {
                if (nonSurroundCapture.IsEncoding() == false || TrackMainCamera) { 
                    var found = GameObject.Find("FBCaptureEncoder");
                    if (found != null) {
                        float alpha = Math.Max(0, Math.Min(1, TrackingAlpha));
                        found.transform.position =
                            Vector3.Lerp(found.transform.position, Camera.main.transform.position, alpha);
                        found.transform.rotation =
                            Quaternion.Lerp(found.transform.rotation, Camera.main.transform.rotation, alpha);
                    }
                }
            }


            // 360 screen capturing
            if (Input.GetKeyDown(screenShotKey) && doSurroundCapture) {
                surroundCapture.TakeScreenshot(screenShotWidth, screenShotHeight, ScreenShotName(screenShotWidth, screenShotHeight));
            }

            else if (Input.GetKeyDown(encodingStartShotKey) && doSurroundCapture) {
                surroundCapture.StartEncodingVideo(surroundVideoWidth, videoHeight, MovieName(surroundVideoWidth, videoHeight));
            }

            else if (Input.GetKeyDown(encodingStopShotKey) && doSurroundCapture) {
                surroundCapture.StopEncodingVideo();
            }

            // 2D screen capturing
            if (Input.GetKeyDown(screenShotKey) && !doSurroundCapture) {
                nonSurroundCapture.TakeScreenshot(screenShotWidth, screenShotHeight, ScreenShotName(screenShotWidth, screenShotHeight));
            }

            else if (Input.GetKeyDown(encodingStartShotKey) && !doSurroundCapture) {
                nonSurroundCapture.StartEncodingVideo(viewVideoWidth, videoHeight, MovieName(viewVideoWidth, videoHeight));
            }

            else if (Input.GetKeyDown(encodingStopShotKey) && !doSurroundCapture) {
                nonSurroundCapture.StopEncodingVideo();
            }
        }




        public bool EnableSurroundCapture {
            get { return doSurroundCapture; }
            set {
                doSurroundCapture = value;
                if (doSurroundCapture) {
                    surroundCapture.enabled = true;
                    nonSurroundCapture.enabled = false;
                } else {
                    nonSurroundCapture.enabled = true;
                    surroundCapture.enabled = false;
                }
            }
        }



        // [RMS] added these functions
        public void CaptureScreen()
        {
            if (doSurroundCapture) {
                surroundCapture.TakeScreenshot(screenShotWidth, screenShotHeight, ScreenShotName(screenShotWidth, screenShotHeight));
            } else {
                nonSurroundCapture.TakeScreenshot(screenShotWidth, screenShotHeight, ScreenShotName(screenShotWidth, screenShotHeight));
            }
        }
        public void BeginVideoCapture()
        {
            if ( doSurroundCapture) {
                surroundCapture.StartEncodingVideo(surroundVideoWidth, videoHeight, MovieName(surroundVideoWidth, videoHeight));
            } else {
                nonSurroundCapture.StartEncodingVideo(viewVideoWidth, videoHeight, MovieName(viewVideoWidth, videoHeight));
            }
        }
        public void EndVideoCapture()
        {
            if ( doSurroundCapture) {
                surroundCapture.StopEncodingVideo();
            } else {
                nonSurroundCapture.StopEncodingVideo();
            }
        }





        string MovieName(int width, int height)
        {
            return string.Format("{0}/movie_{1}x{2}_{3}.h264",
                                outputPath,
                                width, height,
                                DateTime.Now.ToString("yyyy-MM-dd hh_mm_ss"));
        }

        string ScreenShotName(int width, int height)
        {            
            return string.Format("{0}/screenshot_{1}x{2}_{3}.jpg",
                                outputPath,
                                width, height,
                                DateTime.Now.ToString("yyyy-MM-dd hh_mm_ss"));
        }
    }
}
