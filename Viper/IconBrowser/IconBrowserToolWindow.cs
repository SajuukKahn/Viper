namespace Viper.IconBrowser
{
    using Microsoft.VisualStudio.Shell;
    using System;
    using System.Runtime.InteropServices;

    /// <summary>
    /// This class implements the tool window exposed by this package and hosts a user control.
    /// </summary>
    /// <remarks>
    /// In Visual Studio tool windows are composed of a frame (implemented by the shell) and a pane,
    /// usually implemented by the package implementer.
    /// <para>
    /// This class derives from the ToolWindowPane class provided from the MPF in order to use its
    /// implementation of the IVsUIElementPane interface.
    /// </para>
    /// </remarks>
    [Guid("21a65932-ab8b-4040-bad9-97aa79720aeb")]
    public class IconBrowserToolWindow : ToolWindowPane
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="IconBrowserToolWindow"/> class.
        /// </summary>
        public IconBrowserToolWindow() : base(null)
        {
            Caption = "Icon Browser";

            // This is the user control hosted by the tool window; Note that, even if this class implements IDisposable,
            // we are not calling Dispose on this object. This is because ToolWindowPane calls Dispose on
            // the object returned by the Content property.
            Content = new IconBrowserControl();
        }
    }
}
