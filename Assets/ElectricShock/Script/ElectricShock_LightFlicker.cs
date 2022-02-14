using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace ElectricShock
{
    public class ElectricShock_LightFlicker : MonoBehaviour
    {
        public Light FlickerLight;//Used for flashing lights
        public Vector2 LightPowerClamp;//The range of brightness in which a light flashes
        public Renderer ElectricShockObj;
        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {
            if (ElectricShockObj.material.GetFloat("_IsShock") > 0)
            {
                FlickerLight.intensity = Random.Range(LightPowerClamp.x, LightPowerClamp.y);
            }
            else
            {
                FlickerLight.intensity = 0;
            }
        }
    }
}
