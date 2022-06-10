using UnityEngine;

namespace pow.addy
{
    public class BaseAdController : MonoBehaviour
    {
        [SerializeField] protected AdEventHandler adEventHandler;
        
        protected string adID;

        internal void SetAdID(string id)
        {
            adID = id;
        }
    }
}