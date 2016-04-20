using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Net;


// Xref2Html.cs
// This file includes a method to make an HTML file containing a cross reference between
// ECAD component reference designators, such as "R1" or "C11", and GME component instance paths.

namespace CyPhy2Schematic
{
    public class XrefItem
    {
        public string ReferenceDesignator;
        public string GmePath;
    }

    public class Xref2Html
    {

        /// <summary>
        /// Make an HTML-page string with a cross-reference-table, and optionally save it to a file.
        /// </summary>
        /// <param name="designName">The name of the GME design</param>
        /// <param name="tableData">A list of cross-reference items that will become an HTML table.</param>
        /// <param name="OutputFileName"></param>
        /// <seealso>MOT-307 Build GME Path --> EAGLE Reference Designator map as part of CyPhy2Schematic's EAGLE generation</seealso>/>
        /// <returns></returns>
        static public string makeHtmlFile(
            string designName,
            List<XrefItem> tableData,
            string OutputFileName = ""
        )
        {
            string rVal = "";
            designName = designName != null ? designName : "";
            List<string> pathList = tableData.Select( x => x.GmePath ).ToList();
            string subtitle = findLongestLeftCommonSubstring(pathList);
            string title = designName + " Component Reference Designator Cross Reference";
            List<string> colHeaders = new List<string>() { "Reference Designator", "GME path" };
            List<List<string>> tableList = new List<List<string>>();
            int startIndex = subtitle.Length;

            foreach (var item in tableData)
            {
                List<string> rowList = new List<string>();
                rowList.Add(item.ReferenceDesignator);
                int newPathLength = item.GmePath.Length - startIndex;
                rowList.Add( item.GmePath.Substring( startIndex, newPathLength ) );
                tableList.Add(rowList);
            }
            rVal = makeHtmlString(title, subtitle, colHeaders, tableList);

            // To do: Write the file, if filename isn't null.
            if (OutputFileName.Length > 0)
            {
                try
                {
                    File.WriteAllText(OutputFileName, rVal);
                }
                catch (Exception e)
                {
                    Console.WriteLine("Exception: " + e.Message);
                    throw e;
                }
            }

            return rVal;
        }

        /// <summary>
        /// Find the longest substring, starting at the left, common to a list of strings.
        /// </summary>
        /// <param name="stringList">The input list of strings</param>
        /// <returns>The longest substring, starting at the left, common to the input list of strings</returns>
        static string findLongestLeftCommonSubstring(List<string> stringList)
        {
            string rVal = "";
            if (stringList.Count() > 0)
            {
                string firstString = stringList[0];
                int maxMatch = firstString.Length;

                foreach (string item in stringList)
                {
                    for (int matched = 0; matched < maxMatch; matched++)
                    {
                        if (item[matched] != firstString[matched])
                        {
                            maxMatch = matched;
                        }
                    }
                }
                rVal = firstString.Substring(0, maxMatch);
            }
            return rVal;
        }

        /// <summary>
        /// Make an HTML-page string for a sortable table.
        /// </summary>
        /// <param name="pageTitle">Appears as text on the top line of the HTML page.</param>
        /// <param name="pageSubtitle">Appears as text below the title of the HTML page.</param>
        /// <param name="columnHeaders">List of strings for the column headers, starting on the left.</param>
        /// <param name="tableData">List for each row, containing a list of column data for that row.</param>
        /// <returns>A string containing the generated HTML.</returns>
        static string makeHtmlString(
            string pageTitle = "",  // Appears as text on the top line of the HTML page.
            string pageSubtitle = "", // Appears as text below the title of the HTML page.
            List<string> columnHeaders = null,  // List of strings for the column headers, starting on the left.
            List<List<string>> tableData = null // List for each row, containing a list of column data for that row.         
            )
        {
            string rVal = "";
            const string headString = @"<html><!-- DataTables CSS -->
                <link rel=""stylesheet"" type=""text/css"" 
                href=""http://ajax.aspnetcdn.com/ajax/jquery.dataTables/1.9.4/css/jquery.dataTables.css"">  
                <!-- jQuery --> <script type=""text/javascript"" charset=""utf8""
                src=""http://ajax.aspnetcdn.com/ajax/jQuery/jquery-1.8.2.min.js""></script> 
                <!-- DataTables --> <script type=""text/javascript"" charset=""utf8"" 
                src=""http://ajax.aspnetcdn.com/ajax/jquery.dataTables/1.9.4/jquery.dataTables.min.js""></script> 
                <script type=""text/javascript""> 
                $(document).ready(function() { 
                    $(""table"").dataTable( {
                        ""bAutoWidth"": false,
                        ""iDisplayLength"": -1,
                        ""aLengthMenu"": [[-1, 100, 50, 25, 10, 5], [""All"", 100, 50, 25, 10, 5 ]]
                    }  ); } );
                </script>
                <style>
                p {margin-left:20px;}
                body {font: 90%/1.45em Consolas, ""Lucida Console"", Verdana, Arial, sans-serif;
                    padding: 10px;
                    margin: 10px;
                }
                td.col1 {text-align:center; width: 200px;}
                th.col1 {text-align:center; width: 200px;}
                td.col2 {text-align:left;}
                th.col2 {text-align:left;}
                </style>
                <body>
                <div style=""position:absolute; left:5%; right:5%"";padding-top:10px;>";
            const string headString2 =
                @"<div style=""font-weight: bold; font-size: x-large;"">{0}</div>
                <div style=""font-weight: bold; font-size: medium;"">{1}</div><p>
                <div id=""table""><table>";
            // Start with the HTML header and page title.
            rVal = headString;
            rVal += string.Format(headString2, WebUtility.HtmlEncode(pageTitle), WebUtility.HtmlEncode(pageSubtitle));
            // Add the column headers
            rVal += "\n<thead>\n<tr>\n";
            int colNum = 1;
            foreach (string header in columnHeaders)
            {
                rVal += string.Format("<th class=\"col{1}\">{0}</th>", WebUtility.HtmlEncode(header), colNum++);
            }
            rVal += "\n</tr>\n</thead>\n";
            // Add the table data
            foreach (var row in tableData)
            {
                rVal += "<tr>";
                colNum = 1;
                foreach (var datum in row)
                {
                    rVal += string.Format("<td class=\"col{1}\">{0}</td>\n", WebUtility.HtmlEncode(datum), colNum++);
                }
                rVal += "</tr>";
            }
            // Add the HTML tail boilerplate
            rVal += @"</table></div><div class=""clear""/></div></body></html>";
            return rVal;
        }
    }

}
