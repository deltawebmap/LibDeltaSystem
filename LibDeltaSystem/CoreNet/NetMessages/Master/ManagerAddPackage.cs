using LibDeltaSystem.WebFramework.Extras.HTMLForm.Attributes.Types;
using System;
using System.Collections.Generic;
using System.Text;

namespace LibDeltaSystem.CoreNet.NetMessages.Master
{
    public class ManagerAddPackage
    {
        [FormElementTypeText("Name", "Name", "name")]
        public string name { get; set; }
        [FormElementTypeText("Project Path", "Relative path to the csharp project in the GIT repo", "path")]
        public string project_path { get; set; }
        [FormElementTypeText("Git Repo URL", "URL to the GIT repo", "git")]
        public string git_repo { get; set; }
        [FormElementTypeText("Exec Name", "The name of the compiled DLL to run", "exec")]
        public string exec { get; set; }
        [FormElementTypeNumber("User Ports", "ports", 0, 255, 0)]
        public int required_user_ports { get; set; }
        [FormElementTypeTextArray("Dependencies", "Name packages ", "dependencies")]
        public string[] dependencies { get; set; }
    }
}
