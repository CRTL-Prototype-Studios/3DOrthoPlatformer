using System;
using UnityEngine;

namespace CiGA.Character
{
    [RequireComponent(typeof(Rigidbody)), RequireComponent(typeof(CharacterControllerInput))]
    public class CharacterControllerBehaviour : MonoBehaviour
    {
        [SerializeField] protected Rigidbody Rigidbody;
        [SerializeField] protected Collider Collider;
        [SerializeField] protected CharacterControllerInput Input;

        protected void Awake()
        {
            if (!Rigidbody) Rigidbody = GetComponent<Rigidbody>();
            if (!Collider) Collider = GetComponent<Collider>();
            if (!Input) Input = GetComponent<CharacterControllerInput>();
        }
    }
}