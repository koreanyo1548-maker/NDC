using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OPS.Editor.Report
{
    /// <summary>
    /// A report for a project.
    /// </summary>
    public class ProjectReport : AReport
    {
        // Constructor
        #region Constructor

        public ProjectReport(String _Company, String _Product, String _ReportName, String _ReportDescription, String _FilePath)
            :base(_ReportName, _ReportDescription, _FilePath)
        {
            this.Company = _Company;
            this.Product = _Product;
        }

        #endregion

        // Company
        #region Company

        public string Company { get; private set; }

        #endregion

        // Product
        #region Product

        public string Product { get; private set; }

        #endregion

        // Log
        #region Log

        protected override string GetReportLinePrefix()
        {
            StringBuilder var_Builder = new StringBuilder();

            var_Builder.Append("[");
            var_Builder.Append(this.Company);
            var_Builder.Append(".");
            var_Builder.Append(this.Product);
            var_Builder.Append("]");
            var_Builder.Append(base.GetReportLinePrefix());

            return var_Builder.ToString();
        }

        #endregion
    }
}
