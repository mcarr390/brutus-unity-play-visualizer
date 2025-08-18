using tasks_and_resources.Core;
using UnityEngine;

namespace Game
{
    public class StartScript : MonoBehaviour
    {
        void Start()
        {
            ResourceRegistry.Init();
            
            TaskRegistry.Init(ResourceRegistry.Registry.Values);

            foreach (var res in ResourceRegistry.Registry)
            {
                Debug.Log(res.Value.Name);
            }
        }

    
    }
}
