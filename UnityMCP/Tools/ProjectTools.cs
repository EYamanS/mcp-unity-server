// Tools/ProjectTools.cs
// Project information tools
#if UNITY_EDITOR
using UnityEngine;

namespace UnityMCP
{
    public static class ProjectTools
    {
        public static object GetProjectInfo() => new
        {
            projectName = Application.productName,
            unityVersion = Application.unityVersion,
            platform = Application.platform.ToString(),
            dataPath = Application.dataPath,
            companyName = Application.companyName
        };
    }
}
#endif

