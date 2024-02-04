using UnityEngine.Animations.Rigging;

namespace Game.Entities.Character
{
    [ System.Serializable ]
    public sealed class HumanoidArmsRig
    {
        public Rig Rig;
        public TwoBoneIKConstraint LeftConstraint;
        public TwoBoneIKConstraint RightConstraint;
    }
}