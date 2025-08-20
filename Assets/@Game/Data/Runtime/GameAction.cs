using System.Collections.Generic;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;

namespace Game.Data.Runtime
{
    public class GameAction : SerializedScriptableObject
    {
        public Dictionary<GameResource, int> produces;
        public Dictionary<GameResource, int> consumes;

    }
}
