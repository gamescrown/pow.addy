using UnityEngine;

namespace pow.addy
{
    public class BaseAdController : MonoBehaviour
    {
        protected string adID;

        internal void SetAdID(string id)
        {
            adID = id;
        }
    }
}