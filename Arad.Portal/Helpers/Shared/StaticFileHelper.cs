using Microsoft.AspNetCore.Hosting;
using System.Collections.Generic;
using System.IO;

namespace Arad.Portal.Helpers.Shared
{
    public class StaticFileHelper
    {
            public static IEnumerable<string> GetScriptsAndStyles(IWebHostEnvironment env, string folderPath)
            {
                var fullPath = Path.Combine(env.WebRootPath, folderPath);
                if (!Directory.Exists(fullPath))
                {
                    yield break;
                }

                foreach (var filePath in Directory.GetFiles(fullPath))
                {
                    var relativePath = filePath.Replace(env.WebRootPath, "").Replace("\\", "/");
                    yield return relativePath;
                }
            }
    }
}
