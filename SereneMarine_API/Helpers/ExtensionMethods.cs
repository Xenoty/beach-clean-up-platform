using MongoDB.Driver.Core.Clusters;

namespace WebApi.Helpers
{
    public static class ExtensionMethods
    {
        /// <summary>
        /// Determines if the Cluster State Description is Connected.
        /// </summary>
        public static bool IsConnected(this ClusterState state)
        {
            return state == ClusterState.Connected;
        }
    }
}