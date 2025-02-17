﻿using System.Collections;
using UnityEngine;
using Assets.LSL4Unity.Scripts.AbstractInlets;

namespace Assets.LSL4Unity.Scripts.Examples
{

    /// <summary>
    /// Example that works with the Resolver component.
    /// This script waits for the resolver to resolve a Stream which matches the Name and Type.
    /// See the base class for more details. 
    /// 
    /// The specific implementation should only deal with the moment when the samples need to be pulled
    /// and how they should processed in your game logic
    ///
    /// </summary>
    public class DataIntensityChange : InletFloatSamples
    {
        public Light targetLight;

        public bool useX;
        public bool useY;
        public bool useZ;

        private bool pullSamplesContinuously = false;

		private readonly float maxHRV = 200f;
		private readonly float minHRV = 0f;
		private float HRVRange;

		private readonly float maxIntensity = 4.0f;
		private readonly float minIntensity = 2.0f;
		private readonly float unitIntensity = 0.1f;	// How much to increase or decrease intensity with each update
		private float intensityRange;

		private float currIntensity = -1f;
		private float aim;

		private readonly float secPerUpdate = 0.01f;
		private float prevUpdate;

        void Start()
        {
            // [optional] call this only, if your gameobject hosting this component
            // got instantiated during runtime

            // registerAndLookUpStream();
			prevUpdate = Time.timeSinceLevelLoad;
			HRVRange = maxHRV - minHRV;
			intensityRange = maxIntensity - minIntensity;
        }

        protected override bool isTheExpected(LSLStreamInfoWrapper stream)
        {
            // the base implementation just checks for stream name and type
            var predicate = base.isTheExpected(stream);
            // add a more specific description for your stream here specifying hostname etc.
            //predicate &= stream.HostName.Equals("Expected Hostname");
            return predicate;
        }

        /// <summary>
        /// Override this method to implement whatever should happen with the samples...
        /// IMPORTANT: Avoid heavy processing logic within this method, update a state and use
        /// coroutines for more complexe processing tasks to distribute processing time over
        /// several frames
        /// </summary>
        /// <param name="newSample"></param>
        /// <param name="timeStamp"></param>
        protected override void Process(float[] newSample, double timeStamp)
        {
            //Assuming that a sample contains at least 3 values for x,y,z
            float x = useX ? newSample[0] : 1;
            float y = useY ? newSample[1] : 1;
            float z = useZ ? newSample[2] : 1;
            //Debug.Log(x);
            //// we map the data to the scale factors
            //var targetScale = new Vector3(x, y, z);

            //// apply the rotation to the target transform
            //targetTransform.localScale = targetScale;

			/* Change light color */
            //targetLight.color = new Color(x * 0.1f, targetLight.color.g, targetLight.color.b);




			/* Model for changing light intensity */
			float scaledHRV = x/HRVRange;

			// Shift x within range of minHRV and maxHRV
			if(x < minHRV){
				x = minHRV;
			} else if (x > maxHRV) {
				x = maxHRV;
			}

			// First update
			if(currIntensity < 0.0f) {
				currIntensity = intensityRange * (1-scaledHRV) + minIntensity;
				aim = currIntensity;
			}

			// Update every few seconds
			if(Time.timeSinceLevelLoad - prevUpdate < secPerUpdate) return;
			prevUpdate = Time.timeSinceLevelLoad;

			// Update currIntensity relative to aim
			if(aim > currIntensity){
				currIntensity += unitIntensity;
			} else {
				currIntensity -= unitIntensity;
			}

			// Update aim
			aim = intensityRange * (1-scaledHRV) + minIntensity;

			// Update light intensity
			targetLight.intensity = currIntensity;
			Debug.Log("HRV: " + x);
			//Debug.Log("HRV range: " + HRVRange);
			//Debug.Log("current intensity: " + currIntensity);
        }

        protected override void OnStreamAvailable()
        {
            pullSamplesContinuously = true;
        }

        protected override void OnStreamLost()
        {
            pullSamplesContinuously = false;
        }

        private void Update()
        {
            if (pullSamplesContinuously)
                pullSamples();
        }
    }

}