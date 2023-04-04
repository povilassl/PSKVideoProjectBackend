﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace PSKVideoProjectBackend.Properties {
    using System;
    
    
    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "17.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    public class Resources {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal Resources() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        public static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("PSKVideoProjectBackend.Properties.Resources", typeof(Resources).Assembly);
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }
        
        /// <summary>
        ///   Overrides the current thread's CurrentUICulture property for all
        ///   resource lookups using this strongly typed resource class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        public static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Comment with the supplied Id not found.
        /// </summary>
        public static string ErrCommentNotFoundById {
            get {
                return ResourceManager.GetString("ErrCommentNotFoundById", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Error: &apos;count&apos; can not be less or equal to 0.
        /// </summary>
        public static string ErrCountLessOrEqualZero {
            get {
                return ResourceManager.GetString("ErrCountLessOrEqualZero", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Error: index can not be less than 0.
        /// </summary>
        public static string ErrIndexLessThanZero {
            get {
                return ResourceManager.GetString("ErrIndexLessThanZero", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to There was an error while inserting data into the database..
        /// </summary>
        public static string ErrInsertToDB {
            get {
                return ResourceManager.GetString("ErrInsertToDB", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Error: Not all info supplied.
        /// </summary>
        public static string ErrNotAllInfo {
            get {
                return ResourceManager.GetString("ErrNotAllInfo", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Error retrieving data from database. Index out of range..
        /// </summary>
        public static string ErrRetrieveDbOutOfRange {
            get {
                return ResourceManager.GetString("ErrRetrieveDbOutOfRange", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to There was an error while retrieving information from the database..
        /// </summary>
        public static string ErrRetrieveFromDB {
            get {
                return ResourceManager.GetString("ErrRetrieveFromDB", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Video with the supplied Id not found .
        /// </summary>
        public static string ErrVideoNotFoundById {
            get {
                return ResourceManager.GetString("ErrVideoNotFoundById", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Exception.
        /// </summary>
        public static string Exception {
            get {
                return ResourceManager.GetString("Exception", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Comment.
        /// </summary>
        public static string FillerComment {
            get {
                return ResourceManager.GetString("FillerComment", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Video Description.
        /// </summary>
        public static string FillerVideoDescription {
            get {
                return ResourceManager.GetString("FillerVideoDescription", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Video Name.
        /// </summary>
        public static string FillerVideoName {
            get {
                return ResourceManager.GetString("FillerVideoName", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Video URL.
        /// </summary>
        public static string FillerVideoURL {
            get {
                return ResourceManager.GetString("FillerVideoURL", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Username.
        /// </summary>
        public static string FillerVideoUsername {
            get {
                return ResourceManager.GetString("FillerVideoUsername", resourceCulture);
            }
        }
    }
}
