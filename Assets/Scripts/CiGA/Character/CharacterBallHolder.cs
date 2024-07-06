using System;
using UnityEngine;

namespace CiGA.Character
{
    public class CharacterBallHolder : MonoBehaviour
    {
        [SerializeField] protected CharacterControllerBehaviour Controller;
        [SerializeField] protected Collider Collider;

        private void OnCollisionEnter(Collision other)
        {
            if(other.gameObject.layer == Controller.pushableLayers)
                Controller.OnBallHolderCollide();
        }
    }
}