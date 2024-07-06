using System;
using UnityEngine;

namespace CiGA.Character
{
    public class CharacterBallHolder : MonoBehaviour
    {
        [SerializeField] protected CharacterControllerBehaviour Controller;
        [SerializeField] public Collider Collider;
    }
}