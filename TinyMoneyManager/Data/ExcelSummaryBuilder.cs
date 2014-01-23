namespace TinyMoneyManager.Data
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using TinyMoneyManager.Component;

    public class ExcelSummaryBuilder
    {
        public const string AccountItemRowBackgroundColorLightGrayFirstRowFormatter = "\r\n <tr height=3D22 style=3D'height:16.5pt'>\r\n  <td rowspan=3D{7} class=3Dxl83_middle style=3D'height:16.5pt'>{0}</td>\r\n  <td class=3Dxl83_date style=3D'border-left:none'>{1}</td>\r\n  <td class=3Dxl83 style=3D'border-left:none'>{2}</td>\r\n  <td class=3Dxl83 style=3D'border-left:none'>{3}</td>\r\n  <td class=3Dxl83 style=3D'border-left:none'>{4}</td>\r\n  <td class=3Dxl83 style=3D'border-left:none'>{5}</td>\r\n  <td class=3Dxl83 style=3D'border-left:none'>{6}</td>\r\n </tr>\r\n";
        public const string AccountItemRowBackgroundColorLightGrayFormatter = "\r\n <tr height=3D22 style=3D'height:16.5pt'>\r\n  <td class=3Dxl83_date style=3D'border-left:none'>{0}</td>\r\n  <td class=3Dxl83 style=3D'border-left:none'>{1}</td>\r\n  <td class=3Dxl83 style=3D'border-left:none'>{2}</td>\r\n  <td class=3Dxl83 style=3D'border-left:none'>{3}</td>\r\n  <td class=3Dxl83 style=3D'border-left:none'>{4}</td>\r\n  <td class=3Dxl83 style=3D'border-left:none'>{5}</td>\r\n </tr>\r\n";
        public const string AccountItemRowBackgroundColorLightGrayFullFormatter = "\r\n <tr height=3D22 style=3D'height:16.5pt'>\r\n  <td class=3Dxl83 style=3D'height:16.5pt'>{0}</td>\r\n  <td class=3Dxl83 style=3D'border-left:none'>{1}</td>\r\n  <td class=3Dxl83 style=3D'border-left:none'>{2}</td>\r\n  <td class=3Dxl83_number style=3D'border-left:none'>{3}</td>\r\n  <td class=3Dxl83 style=3D'border-left:none'>{4}</td>\r\n  <td class=3Dxl83 style=3D'border-left:none'>{5}</td>\r\n  <td class=3Dxl83 style=3D'border-left:none'>{6}</td>\r\n </tr>\r\n";
        public const string AccountItemRowBackgroundColorMoreGrayAndMergeAllRowFormatter = "\r\n <tr height=3D22 style=3D'height:16.5pt'>\r\n  <td height=3D22 class=3Dxl82 style=3D'height:16.5pt'>{0}</td>\r\n  <td class=3Dxl82 style=3D'border-left:none'>{1}</td>\r\n  <td class=3Dxl82 style=3D'border-left:none'>{2}</td>\r\n  <td class=3Dxl82 style=3D'border-left:none'>{3}</td>\r\n  <td class=3Dxl82 style=3D'border-left:none'>{4}</td>\r\n  <td class=3Dxl82 style=3D'border-left:none'>{5}</td>\r\n  <td class=3Dxl82 style=3D'border-left:none'>{6}</td>\r\n </tr>\r\n";
        public const string AccountItemRowBackgroundColorMoreGrayFirstRowFormatter = "\r\n <tr height=3D22 style=3D'height:16.5pt'>\r\n  <td rowspan=3D{7} class=3Dxl84_middle style=3D'height:16.5pt'>{0}</td>\r\n  <td class=3Dxl84_date style=3D'border-left:none'>{1}</td>\r\n  <td class=3Dxl84 style=3D'border-left:none'>{2}</td>\r\n  <td class=3Dxl84 style=3D'border-left:none'>{3}</td>\r\n  <td class=3Dxl84 style=3D'border-left:none'>{4}</td>\r\n  <td class=3Dxl84 style=3D'border-left:none'>{5}</td>\r\n  <td class=3Dxl84 style=3D'border-left:none'>{6}</td>\r\n </tr>\r\n";
        public const string AccountItemRowBackgroundColorMoreGrayFormatter = "\r\n <tr height=3D22 style=3D'height:16.5pt'> \r\n  <td class=3Dxl84_date style=3D'border-left:none'>{0}</td>\r\n  <td class=3Dxl84 style=3D'border-left:none'>{1}</td>\r\n  <td class=3Dxl84 style=3D'border-left:none'>{2}</td>\r\n  <td class=3Dxl84 style=3D'border-left:none'>{3}</td>\r\n  <td class=3Dxl84 style=3D'border-left:none'>{4}</td>\r\n  <td class=3Dxl84 style=3D'border-left:none'>{5}</td>\r\n </tr>\r\n";
        public const string AccountItemRowBackgroundColorMoreGrayFullFormatter = "\r\n <tr height=3D22 style=3D'height:16.5pt'>\r\n  <td class=3Dxl84 style=3D'height:16.5pt'>{0}</td>\r\n  <td class=3Dxl84 style=3D'border-left:none'>{1}</td>\r\n  <td class=3Dxl84 style=3D'border-left:none'>{2}</td>\r\n  <td class=3Dxl84_number style=3D'border-left:none'>{3}</td>\r\n  <td class=3Dxl84 style=3D'border-left:none'>{4}</td>\r\n  <td class=3Dxl84 style=3D'border-left:none'>{5}</td>\r\n  <td class=3Dxl84 style=3D'border-left:none'>{6}</td>\r\n </tr>\r\n";
        public const string AccountItemRowFormatter = "\r\n <tr height=3D22 style=3D'height:16.5pt'>\r\n  <td height=3D22 class=3Dxl80 style=3D'height:16.5pt'>{0}</td>\r\n  <td class=3Dxl70 style=3D'border-left:none'>{1}</td>\r\n  <td class=3Dxl70 style=3D'border-left:none'>{2}</td>\r\n  <td class=3Dxl70 style=3D'border-left:none'>{3}</td>\r\n  <td class=3Dxl70 style=3D'border-left:none'>{4}</td>\r\n  <td class=3Dxl79 style=3D'border-left:none'>{5}</td>\r\n  <td class=3Dxl70 style=3D'border-left:none'>{6}</td>\r\n </tr>\r\n";
        public const string AccountItemSummaryExcelTempleteFilePath = "Resources/templetes/summaryTemplete.xls";
        public const string AccountNamePlaceHolder = "$AccountName$";
        public const string AmountValuePlaceHolder = "$Amount$";
        public const string CategoryNamePlaceHolder = "$Category$";
        public const string ColumnHeaderPlaceHolder = "$ColumnHeaderPlaceHodler$";
        public const string DatePlaceHolder = "$Date$";
        public const string DescriptionPlaceHolder = "$Description$";
        public const string ExcelTitleRowPlaceHolder = "$Title$";
        public const string ExpenseTypePlaceHolder = "$Type$";
        public const string RowContentPlaceHolder = "$RowsPlaceHolder$";
        public const string SubCategoryCategoryNamePlaceHolder = "$SubCategory$";
        public const string TableColumnRowFormatter = "\r\n <tr class=3Dxl78 height=3D22 style=3D'height:16.5pt'>\r\n  <td height=3D22 class=3Dxl77 style=3D'height:16.5pt'>{0}</td>\r\n  <td class=3Dxl77>{1}</td>\r\n  <td class=3Dxl77>{2}</td>\r\n  <td class=3Dxl77>{3}</td>\r\n  <td class=3Dxl77>{4}</td>\r\n  <td class=3Dxl77>{5}</td>\r\n  <td class=3Dxl77>{6}</td>\r\n </tr>";

        public static string Build(string templeteFilePath, string title, string columnHeader, string rowsContent)
        {
            return ApplicationHelper.GetApplicationFileContentFrom(templeteFilePath).Replace("$RowsPlaceHolder$", rowsContent).Replace("$ColumnHeaderPlaceHodler$", columnHeader).Replace("$Title$", title);
        }

        public static string Build<T>(string templeteFilePath, string title, System.Collections.Generic.IEnumerable<T> dataSource, string columnHeader, System.Func<T, String> rowBuilder)
        {
            System.Text.StringBuilder builder = new System.Text.StringBuilder();
            foreach (T local in dataSource)
            {
                builder.AppendLine(rowBuilder(local));
            }
            return ApplicationHelper.GetApplicationFileContentFrom(templeteFilePath).Replace("$RowsPlaceHolder$", builder.ToString()).Replace("$ColumnHeaderPlaceHodler$", columnHeader).Replace("$Title$", title);
        }
    }
}

