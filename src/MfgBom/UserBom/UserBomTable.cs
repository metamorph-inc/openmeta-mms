using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;

// UserBomTable.cs
// This file has classes related to creating a Bill Of Materials (BOM) spreadsheet
// for a Printed Circuit Board Assembly (PCBA).

/////////////////////////////////////////////////////////////////////////////////////////////////////////
// Notes on what goes into a BOM:
//
// From Advanced Assembly http://www.aa-pcbassembly.com/PCB-Assembly-FAQ.htm
// and http://www.pcbassemblyexpress.com/pcb-assembly-faqs.html:
// "The BOM needs reference designators, value, package/decal, description, and part number."
//
// From http://www.arenasolutions.com/resources/articles/bill-of-materials-example:
// "The bill of materials typically includes part names, part numbers, part revisions 
// and the quantities required to build an assembly. Thorough BOMs can include more 
// descriptive information too, for example, the unit of measure or procurement type. 
// BOMs that have printed circuit board assemblies (PCBAs) contain a column for listing
// reference designators."
//
// From Newbury Electronics http://www.newburyelectronics.co.uk/contact-newbury/pcb-assembly-enquiry/:
// "Quantity*, Designator*, Description, Manufacturer*, Manufacturer Part Number*, Supplier 1, Supplier Part Number 1, Notes"
// (* = Compulsory columns)
//
// From Innovative Manufacturing Source http://www.imsmfg.ca/quotes/:
// "Part Number, Description, Package, Total Quantity, Qty Top, Qty Bottom, Reference Designator"
//
// From http://vunics.com/printed_circuit_board_assembly.html#.U0RhpvldX6s:
// Bill of Materials (BOM) - preferably in spreadsheet format containing the following information:
//    Manufacturer’s part number
//    Part description
//    Reference designator
//    Quantity
/////////////////////////////////////////////////////////////////////////////////////////////////////////
// 
namespace MfgBom
{
    /// <summary>
    /// Contains fields representing a row of a BOM spreadsheet.
    /// There is one row per part type, as indicated by the pair (manufacturer, manufacturer part number).
    /// </summary>
    public class UserBomTableRow
    {
        public int Quantity;                    // The total number of parts of this type needed on one of these assemblies.
        public String ReferenceDesignators;     // Each reference designator usually consists of one or two letters followed by a number, e.g. R13, C1002. 
        public String Manufacturer;             // Who makes the part.
        public String ManufacturerPartNumber;   // How the manufacturer identifies the part.
        public String Description;              // Text describing the part, such as "RES 1.0K OHM 1/10W 5% 0603 SMD".
        public String Package;                  // The package designation, e.g. 0603, 8-PDIP, 32-TQFP.
        public String Supplier1;                // Who sells the part, e.g. Digi-Key, Mouser, Newark.
        public String Supplier1PartNumber;      // How the supplier identifies this part.
        public float? Supplier1UnitPrice;       // The price per unit from Supplier1.  Unit of measure is "each".  This nullable float
                                                // leaves the field empty if the BOM if null, for MOT-338.
        public String Notes;                    // Misc. info, e.g. "Lead Free / RoHS Compliant".

        /// <summary>
        /// Produces a CSV row string for this spreadsheet row.
        /// </summary>
        /// <returns></returns>
        public String ToCsv()
        {
            return String.Join(",", 
                Quantity,
                Csv.Escape(ReferenceDesignators),
                Csv.Escape(Manufacturer),
                Csv.Escape(ManufacturerPartNumber),
                Csv.Escape(Description),
                Csv.Escape(Package),
                Csv.Escape(Supplier1),
                Csv.Escape(Supplier1PartNumber),
                Csv.Escape(Supplier1UnitPrice),
                (Supplier1UnitPrice.HasValue) ? Csv.Escape(Supplier1UnitPrice * Quantity) : Csv.Escape(Supplier1UnitPrice),
                Csv.Escape(Notes)
            );
        }


        /// <summary>
        /// Produces a HTML row code for this spreadsheet row.
        /// </summary>
        /// <returns></returns>
        public String ToHtml()
        {
            String supplier1ExtPrice = (Supplier1UnitPrice.HasValue) ?
                WebUtility.HtmlEncode((Supplier1UnitPrice * Quantity).ToString()) :
                WebUtility.HtmlEncode((Supplier1UnitPrice).ToString());

            return "<td>" + Quantity + "</td>\n" +
                "<td>" + WebUtility.HtmlEncode(ReferenceDesignators) + "</td>\n" +
                "<td>" + WebUtility.HtmlEncode(Manufacturer) + "</td>\n" +
                "<td>" + WebUtility.HtmlEncode(ManufacturerPartNumber) + "</td>\n" +
                "<td>" + WebUtility.HtmlEncode(Description) + "</td>\n" +
                "<td>" + WebUtility.HtmlEncode(Package) + "</td>\n" +
                "<td>" + WebUtility.HtmlEncode(Supplier1) + "</td>\n" +
                "<td>" + WebUtility.HtmlEncode(Supplier1PartNumber) + "</td>\n" +
                "<td>" + WebUtility.HtmlEncode((Supplier1UnitPrice).ToString()) + "</td>\n" +
                "<td>" + supplier1ExtPrice + "</td>\n" +
                "<td>" + WebUtility.HtmlEncode(Notes) + "</td>\n";
        }
     }

    /// <summary>
    /// Contains a list holding all the UserBomTableRows.
    /// </summary>
    public class UserBomTable
    {
        // Design-related info for BOM title, etc.  See MOT-256.
        private string bomTitle;             // Design name

        public string BomTitle
        {
            get { return bomTitle; }
            set { bomTitle = value; }
        }
        private string queryDateTime;        // Timestamp indicating when part queries were made

        public string QueryDateTime
        {
            get { return queryDateTime; }
            set { queryDateTime = value; }
        }
        private uint howManyBoards;          // Number of designs expected to be built (the basis of our choices of suppliers & prices)

        public uint HowManyBoards
        {
            get { return howManyBoards; }
            set { howManyBoards = value; }
        }

        private List<UserBomTableRow> rows; // Field holding all the UserBomTableRows.

        public List<UserBomTableRow> Rows   // Property allowing public access to the list.
        {
            get { return rows; }
            set { rows = value; }
        }

        /// <summary>
        /// Creates a CSV string that pads a BOM row to skip a certain number of columns.
        /// </summary>
        /// <param name="n">The number of columns to skip.</param>
        /// <returns>The CSV padding string.</returns>
        private string skipColumns( int n )
        {
            string rVal = "";
            for( int i = 0; i < n; ++i )
            {
                rVal += Csv.Escape("") + ",";
            }
            return rVal;
        }

        /// <summary>
        /// Converts the UserBomTable into a CSV spreadsheet file.
        /// </summary>
        /// <returns></returns>
        public String ToCsv()            
        {
            String rVal = "";
            string skipToDescriptionField = skipColumns(2);   // CSV padding string to skip 2 columns, to start at the "Reference Designator" column.

            // Create a title line
            if (null != bomTitle)
            {
                rVal += skipToDescriptionField;
                rVal += "Design Name:,";
                rVal += Csv.Escape( bomTitle ) + Environment.NewLine;
            }

            // Create a subtitle line with the queryDateTime
            if (null != queryDateTime)
            {
                rVal += skipToDescriptionField;
                rVal += "Pricing Timestamp:,";
                rVal += Csv.Escape(queryDateTime) + Environment.NewLine;
            }

            // Calculate the total cost
            float totalCost = rows.Where(r => r.Supplier1UnitPrice.HasValue)
                                  .Sum(r => r.Supplier1UnitPrice.Value * r.Quantity);
            // Print the total cost
            rVal += skipToDescriptionField;
            rVal += String.Format("Cost per Unit (based on {0} unit{1}):,", 
                                  howManyBoards, 
                                  howManyBoards > 1 ? "s" : "");
            rVal += totalCost.ToString("C2") + Environment.NewLine;            
            
            // Skip a row to separate the title lines from the headings
            rVal += Environment.NewLine;

            // Add a headings line for the BOM columns.
            var headings = new List<String>()
            {
                Csv.Escape("Item #"),
                Csv.Escape("Qty"),
                Csv.Escape("Reference Designator"),
                Csv.Escape("Manufacturer"),
                Csv.Escape("Manufacturer Part #"),
                Csv.Escape("Description"),
                Csv.Escape("Package"),
                Csv.Escape("Supplier 1"),
                Csv.Escape("Supplier 1 SKU"),
                Csv.Escape("Supplier 1 Price/Unit"),
                Csv.Escape("Supplier 1 Extended Price"),
                Csv.Escape("Notes")
            };

            rVal += String.Join(",", headings) + Environment.NewLine;
            
            // Add the rows with component data.
            for (int i = 0; i < rows.Count; i++)
            {
                // Each row in the CSV BOM starts with the item (row) number, then the rest of the row values.
                rVal += string.Format("{0},{1}" + Environment.NewLine, i + 1, rows[i].ToCsv());
            }

            return rVal;
        }

        /// <summary>
        /// Default Constructor
        /// </summary>
        public UserBomTable()
        {
            rows = new List<UserBomTableRow>();
        }

        /// <summary>
        /// Converts the UserBomTable into a HTML table string.
        /// </summary>
        /// <returns></returns>
        public String ToHtml()            
        {
            // Add a verbatim string literal to begin the HTML page.  Doubled quotes are escaped.

            String rVal = @"<html><!-- DataTables CSS --> <link rel=""stylesheet""             type=""text/css"" href=""http://ajax.aspnetcdn.com/ajax/jquery.dataTables/1.9.4/css/jquery.dataTables.css"">              <!-- jQuery --> <script type=""text/javascript"" charset=""utf8""             src=""http://ajax.aspnetcdn.com/ajax/jQuery/jquery-1.8.2.min.js""></script>              <!-- DataTables --> <script type=""text/javascript"" charset=""utf8""             src=""http://ajax.aspnetcdn.com/ajax/jquery.dataTables/1.9.4/jquery.dataTables.min.js""></script>             <script type=""text/javascript""> $(document).ready(function() {                     $(""table"").dataTable(); } ); </script>            ";            // Create title and subtitle strings

            // Create a title string
            string htmlTitle = "Bill of Materials";
            if (null != bomTitle)
            {
                htmlTitle += WebUtility.HtmlEncode(" -- " + bomTitle);
            }

            string htmlSubtitle = "";
            // Create a subtitle line with the queryDateTime
            if (null != queryDateTime)
            {
                htmlSubtitle = WebUtility.HtmlEncode("Pricing Time: " + queryDateTime);
            }

            // Calculate the total cost
            float totalCost = rows.Where(r => r.Supplier1UnitPrice.HasValue)
                                  .Sum(r => r.Supplier1UnitPrice.Value * r.Quantity);

            // See http://stackoverflow.com/questions/105770/net-string-format-to-add-commas-in-thousands-place-for-a-number
            string htmlTotalCost = String.Format("Cost per Unit (based on {0:n0} unit{1}): ",
                                  howManyBoards,
                                  howManyBoards > 1 ? "s" : "");
            htmlTotalCost += totalCost.ToString("C2") + Environment.NewLine;            

            // Add title and subtitle strings to the HTML.
            rVal += String.Format("<body><p style=\"font-weight: bold;\">{0}</p>\n<p>{1}<br>{2}</p>\n",
                htmlTitle,
                htmlSubtitle,
                htmlTotalCost );                        // Add the component table division            rVal += "<div id=\"component_table\"><table>\n";

            // Add column headers
            rVal += "<thead>\n<tr>\n";  
			rVal += "<th>Item #</th>\n";
			rVal += "<th>Qty</th>\n";
			rVal += "<th>Reference Designator</th>\n";
			rVal += "<th>Manufacturer</th>\n";
			rVal += "<th>Manufacturer Part #</th>\n";
			rVal += "<th>Description</th>\n";
			rVal += "<th>Package</th>\n";
			rVal += "<th>Supplier 1</th>\n";
			rVal += "<th>Supplier 1 SKU</th>\n";
			rVal += "<th>Supplier 1 Price/Unit</th>\n";
			rVal += "<th>Supplier 1 Extended Price</th>\n";
			rVal += "<th>Note</th>\n";
            rVal += "</tr>\n</thead>\n";  // End of the column headers

            // Add the rows with component data.
            for (int i = 0; i < rows.Count; i++)
            {
                rVal += "<tr>\n";
                // Each row in the CSV BOM starts with the item (row) number, 
                rVal += string.Format("<td>{0}</td>\n", i + 1 );
                // Then, the rest of the row values.
                rVal += rows[i].ToHtml();
                rVal += "</tr>\n";
            }

            rVal += @"</table></div><div class=""clear""/></body></html>";
            rVal += "\n";
            return rVal;
        }
    }
}
