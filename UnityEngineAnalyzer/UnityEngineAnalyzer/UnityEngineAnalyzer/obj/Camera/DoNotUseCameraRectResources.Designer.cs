﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Reflection;
using System.Resources;
using System.Runtime.CompilerServices;

namespace UnityEngineAnalyzer.Camera
{
    /// <summary>
    ///     A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [GeneratedCode("System.Resources.Tools.StronglyTypedResourceBuilder", "4.0.0.0")]
    [DebuggerNonUserCode()]
    [CompilerGenerated()]
    internal class DoNotUseCameraRectResources
    {
        private static ResourceManager resourceMan;

        private static CultureInfo resourceCulture;

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal DoNotUseCameraRectResources()
        {
        }

        /// <summary>
        ///     Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Advanced)]
        internal static ResourceManager ResourceManager
        {
            get
            {
                if (ReferenceEquals(resourceMan, null))
                {
                    ResourceManager temp = new ResourceManager("UnityEngineAnalyzer.Camera.DoNotUseCameraRectResources",
                        typeof(DoNotUseCameraRectResources).GetTypeInfo().Assembly);
                    resourceMan = temp;
                }

                return resourceMan;
            }
        }

        /// <summary>
        ///     Overrides the current thread's CurrentUICulture property for all
        ///     resource lookups using this strongly typed resource class.
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Advanced)]
        internal static CultureInfo Culture
        {
            get { return resourceCulture; }
            set { resourceCulture = value; }
        }

        /// <summary>
        ///     Looks up a localized string similar to Do not set Camera.rect, as this will create a new render target.
        /// </summary>
        internal static string Description
        {
            get { return ResourceManager.GetString("Description", resourceCulture); }
        }

        /// <summary>
        ///     Looks up a localized string similar to Do not set Camera.rect.
        /// </summary>
        internal static string MessageFormat
        {
            get { return ResourceManager.GetString("MessageFormat", resourceCulture); }
        }

        /// <summary>
        ///     Looks up a localized string similar to Do not set Camera.rect.
        /// </summary>
        internal static string Title
        {
            get { return ResourceManager.GetString("Title", resourceCulture); }
        }
    }
}