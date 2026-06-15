using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

// OPS - IO
using OPS.Editor.IO.File;

namespace OPS.Editor.Report
{
    /// <summary>
    /// An object storing logs as an report.
    /// </summary>
    public abstract class AReport : IFileReference
    {
        // Constructor
        #region Constructor

        public AReport(String _ReportName, String _ReportDescription, String _FilePath)
        {
            // 
            this.Name = _ReportName;
            this.Description = _ReportDescription;

            //
            this.FilePath = _FilePath;

            //
            this.ReportLevel = EReportLevel.Debug;
        }

        #endregion        

        // Name
        #region Name

        public string Name { get; private set; }

        #endregion

        // Description
        #region Description

        public string Description { get; private set; }

        #endregion

        // ReportLevel
        #region ReportLevel

        public EReportLevel ReportLevel { get; private set; }

        #endregion

        // Header
        #region Header

        public string Header { get; private set; }

        public void SetHeader(String _Header)
        {
            this.Header = _Header;
        }

        #endregion

        // FilePath
        #region FilePath

        public string FilePath { get; private set; }

        #endregion

        // Log
        #region Log

        private StringBuilder reportStringBuilder = new StringBuilder();

        protected virtual String GetReportLinePrefix()
        {
            StringBuilder var_Builder = new StringBuilder();

            var_Builder.Append("[");
            var_Builder.Append(this.Header);
            var_Builder.Append("]");

            return var_Builder.ToString();
        }

        public void Append(EReportLevel _ReportLevel, String _Text, bool _ForceDisplay = false)
        {
            try
            {
                // Continue only if _ReportLevel is higher than this.ReportLevel.
                if ((int)_ReportLevel < (int)this.ReportLevel)
                {
                    return;
                }

                // Create the String Builder.
                StringBuilder var_Builder = new StringBuilder();

                var_Builder.Append("[");
                var_Builder.Append(_ReportLevel.ToString());
                var_Builder.Append("]");
                var_Builder.Append(this.GetReportLinePrefix());
                var_Builder.Append(" ");
                var_Builder.Append(_Text);

                // Get the line as single string.
                String var_Line = var_Builder.ToString();

                // Add to report.
                this.reportStringBuilder.AppendLine(var_Line);

                // Write to a file, if warning or error.
                if ((int)_ReportLevel >= (int)EReportLevel.Warning)
                {
                    this.SaveToFile(true);
                }

                // Log in Unity editor.
                if (_ReportLevel == EReportLevel.Error || _ForceDisplay)
                {
                    if (_ReportLevel == EReportLevel.Debug || _ReportLevel == EReportLevel.Info)
                    {
                        UnityEngine.Debug.Log(var_Line);
                    }
                    else if (_ReportLevel == EReportLevel.Warning)
                    {
                        UnityEngine.Debug.LogWarning(var_Line);
                    }
                    else if (_ReportLevel == EReportLevel.Error)
                    {
                        UnityEngine.Debug.LogError(var_Line);
                    }
                }
            }
            catch(Exception e)
            {
                UnityEngine.Debug.LogWarning("Could append to log file at '" + this.FilePath + "'. Exception: " + e.ToString());
            }
        }

        public void Append(EReportLevel _ReportLevel, String _Text, List<String> _ItemList, bool _ForceDisplay = false)
        {
            StringBuilder var_StringBuilder = new StringBuilder();

            var_StringBuilder.Append(_Text);
            var_StringBuilder.Append(" (");
            var_StringBuilder.Append(_ItemList.Count);
            var_StringBuilder.Append(")");
            var_StringBuilder.AppendLine(":");

            foreach (String var_Item in _ItemList)
            {
                var_StringBuilder.AppendLine("  - " + var_Item);
            }

            this.Append(_ReportLevel, var_StringBuilder.ToString());
        }

        #endregion

        // Save
        #region Save

        public void SaveToFile(bool _Append)
        {
            if (string.IsNullOrEmpty(this.FilePath))
            {
                return;
            }

            String var_DirectoryPath = System.IO.Path.GetDirectoryName(this.FilePath);

            try
            {
                if (!String.IsNullOrEmpty(var_DirectoryPath) && !System.IO.Directory.Exists(var_DirectoryPath))
                {
                    System.IO.Directory.CreateDirectory(var_DirectoryPath);
                }

                using (System.IO.StreamWriter var_File = new System.IO.StreamWriter(this.FilePath, _Append))
                {
                    var_File.Write(this.reportStringBuilder.ToString());
                }
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogWarning("[OPS] Could not create a log file at '" + this.FilePath + "'. Exception: " + e.ToString());
            }
            finally
            {
                this.reportStringBuilder = new StringBuilder();
            }
        }

        #endregion
    }
}
