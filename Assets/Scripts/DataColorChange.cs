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
    public class DataColorChange : InletFloatSamples
    {
        public enum RGB
        {
            r, g, b
        }
		public RGB rgb = RGB.r;

        public Light targetLight;

        public bool useX;
        public bool useY;
        public bool useZ;

		[Range(0.0f, 1.0f)]
		public float rNormal;
		[Range(0.0f, 1.0f)]
		public float gNormal;
		[Range(0.0f, 1.0f)]
		public float bNormal;
		[Range(0.0f, 1.0f)]
		public float rStress;
		[Range(0.0f, 1.0f)]
		public float gStress;
		[Range(0.0f, 1.0f)]
		public float bStress;

		[Range(0.0f, 1.0f)]
		public float a;

		private float rRange;
		private float bRange;
		private float gRange;

		private float currR = -1f;
		private float currB = -1f;
		private float currG = -1f;

        private bool pullSamplesContinuously = false;

        private readonly float maxHRV = 200f;
        private readonly float minHRV = 0f;
        private float HRVRange;

        private readonly float maxIntensity = 1f;
        private readonly float minIntensity = 0;
        private readonly float unitIntensity = 0.1f;    // How much to increase or decrease color intensity with each update
        private float intensityRange;

        private float currIntensity = -1f;
        private float aim;

        private readonly float secPerUpdate = 0.5f;
        private float prevUpdate;

        void Start()
        {
            // [optional] call this only, if your gameobject hosting this component
            // got instantiated during runtime

            // registerAndLookUpStream();
            prevUpdate = Time.timeSinceLevelLoad;
            HRVRange = maxHRV - minHRV;
            intensityRange = maxIntensity - minIntensity;

			rRange = rStress - rNormal;
			gRange = gStress - gNormal;
			bRange = bStress - bNormal;
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

            // Shift x within range of minHRV and maxHRV
            if (x < minHRV)
            {
                x = minHRV;
            }
            else if (x > maxHRV)
            {
                x = maxHRV;
            }

            /* Model for changing light intensity */
            float scaledHRV = x / HRVRange;

            // First update
            if (currR < 0.0f)
            {
                //currIntensity = intensityRange * (1 - scaledHRV) + minIntensity;
                //aim = currIntensity;

				currR = rRange * (1 - scaledHRV) + rNormal;
				currG = gRange * (1 - scaledHRV) + gNormal;
				currB = bRange * (1 - scaledHRV) + bNormal;
				aim = currB;
            }

            // Update every few seconds
            if (Time.time - prevUpdate < secPerUpdate) return;
            prevUpdate = Time.time;

            // Update currIntensity relative to aim
            /*if (aim > currIntensity && currIntensity < maxIntensity)
            {
                currIntensity += unitIntensity;
            }
			else if (aim <= currIntensity && currIntensity > minIntensity)
            {
                currIntensity -= unitIntensity;
            }*/

			if (aim > currB && currB < bStress)
			{
				currR += unitIntensity * (rRange/intensityRange);
				currG += unitIntensity * (gRange/intensityRange);
				currB += unitIntensity * (bRange/intensityRange);
			}
			else if (aim <= currB && currB > bNormal)
			{
				currR -= unitIntensity * (rRange/intensityRange);
				currG -= unitIntensity * (gRange/intensityRange);
				currB -= unitIntensity * (bRange/intensityRange);
			}

            // Update aim
            //aim = intensityRange * (1 - scaledHRV) + minIntensity;
			aim = bRange * (1 - scaledHRV) + bNormal;
            // Update light intensity
            /*switch (rgb)
            {
                case RGB.r:
                    targetLight.color = new Color(currIntensity, targetLight.color.g, targetLight.color.b, 0.6f);
                    break;
                case RGB.g:
				targetLight.color = new Color(targetLight.color.r, currIntensity, targetLight.color.b, 0.6f);
                    break;
                case RGB.b:
				targetLight.color = new Color(targetLight.color.r, targetLight.color.g, currIntensity, 0.6f);
                    break;
                default:
                    break;
            }*/
			targetLight.color = new Color(currR, currG, currB, a);

            Debug.Log("HRV: " + x);
			//Debug.Log("RGB: " + currR + " " + currG + " " + currB);
			//Debug.Log("aim: " + aim);

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

           /* if (Input.GetKeyDown(KeyCode.R))
            {
                rgb = RGB.r;
            }
            if (Input.GetKeyDown(KeyCode.G))
            {
                rgb = RGB.g;
            }
            if (Input.GetKeyDown(KeyCode.B))
            {
                rgb = RGB.b;
            }*/

			rRange = rStress - rNormal;
			gRange = gStress - gNormal;
			bRange = bStress - bNormal;
        }
    }

}