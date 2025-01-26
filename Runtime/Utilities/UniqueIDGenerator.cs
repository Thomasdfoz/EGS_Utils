using UnityEngine;


namespace EGS.Utils 
{
    public static class UniqueIDGenerator
    {
        /// <summary>
        /// Creates a Unique consistent ID based on a gameObject name and 3D position
        /// </summary>
        /// <param name="gameObject">GameObject reference to generate the uniqueID</param>
        /// <returns></returns>
        public static string GetIDByGameObject(Vector3 position)
        {
            // You might want to round the position if objects snap to a grid or similar
            // This reduces the risk of floating point precision issues affecting uniqueness
            string posString = $"{Mathf.Round(position.x * 1000f)}, {Mathf.Round(position.y * 1000f)}, {Mathf.Round(position.z * 1000f)}";
    
            // Hash the position string to get a unique ID
            int hash = posString.GetHashCode();
    
            // Convert the hash to a hexadecimal string if preferred
            string uniqueID = hash.ToString("X");
    
            return uniqueID;
        }
    }
}