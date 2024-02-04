using UnityEngine;

namespace Game.Entities.Character
{
    [ System.Serializable ]
    public sealed class CharacterAvatar
    {
        public Transform Root;
        public Transform RootModel;
        public Transform CameraModel;
        [ Space ]
        public HumanoidArmsRig ArmsRig;
        public HumanoidLegsRig LegsRig;
    }
}