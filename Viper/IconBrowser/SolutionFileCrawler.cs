namespace Viper.IconBrowser
{
    using Microsoft.VisualStudio;
    using Microsoft.VisualStudio.Shell;
    using Microsoft.VisualStudio.Shell.Interop;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Threading.Tasks;
    using Task = System.Threading.Tasks.Task;

    public static class SolutionCrawler
    {
        public static async Task<List<string>> GetAllXamlFilesAsync()
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            var files = new List<string>();
            var solution = (IVsSolution)Package.GetGlobalService(typeof(SVsSolution));
            if (solution == null)
                return files;

            foreach (var hierarchy in EnumerateProjects(solution))
            {
                // Cast to IVsProject to access GetMkDocument
                if (hierarchy is IVsProject project)
                {
                    string projFile = GetProjectFile(project);
                    if (!string.IsNullOrEmpty(projFile))
                    {
                        var folder = Path.GetDirectoryName(projFile);
                        if (Directory.Exists(folder))
                        {
                            files.AddRange(Directory.EnumerateFiles(folder, "*.xaml", SearchOption.AllDirectories));
                        }
                    }
                }
            }

            return files;
        }

        private static IEnumerable<IVsHierarchy> EnumerateProjects(IVsSolution solution)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            Guid guid = Guid.Empty;
            solution.GetProjectEnum((uint)__VSENUMPROJFLAGS.EPF_LOADEDINSOLUTION,
                                    ref guid,
                                    out IEnumHierarchies enumHier);
            var hierarchy = new IVsHierarchy[1];
            uint fetched;
            while (enumHier.Next(1, hierarchy, out fetched) == VSConstants.S_OK && fetched == 1)
            {
                yield return hierarchy[0];
            }
        }

        private static string GetProjectFile(IVsProject project)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            // VSITEMID_ROOT gives the project file itself
            if (project.GetMkDocument(VSConstants.VSITEMID_ROOT, out string path) == VSConstants.S_OK)
                return path;

            return null;
        }
    }
}