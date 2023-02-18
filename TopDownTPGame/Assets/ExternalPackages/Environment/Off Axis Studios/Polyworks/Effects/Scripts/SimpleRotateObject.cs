using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OffAxisStudios
{
    public class SimpleRotateObject : MonoBehaviour
    {
        [Tooltip("Rotation speed.")]
        public float degreesPerSecond = 20;

        [Tooltip("Which axis to rotate on. Select only one!")]
        public bool X;
        public bool Y;
        public bool Z;

        [Tooltip("Set rotation in reverse.")]
        public bool reverse;

        void Update()
        {
            if (X)
            {
                if (reverse)
                {
                    transform.Rotate(new Vector3(-degreesPerSecond, 0, 0) * Time.deltaTime);
                }
                else
                {
                    transform.Rotate(new Vector3(degreesPerSecond, 0, 0) * Time.deltaTime);
                }
            }
            else if (Y)
            {
                if (reverse)
                {
                    transform.Rotate(new Vector3(0, -degreesPerSecond, 0) * Time.deltaTime);
                }
                else
                {
                    transform.Rotate(new Vector3(0, degreesPerSecond, 0) * Time.deltaTime);
                }
            }
            else if (Z)
            {
                if (reverse)
                {
                    transform.Rotate(new Vector3(0, 0, -degreesPerSecond) * Time.deltaTime);
                }
                else
                {
                    transform.Rotate(new Vector3(0, 0, degreesPerSecond) * Time.deltaTime);
                }
            }
            else
            {
                //
            }
        }
    }
}