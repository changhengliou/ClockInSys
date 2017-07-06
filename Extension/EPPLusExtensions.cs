using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using OfficeOpenXml;
using OfficeOpenXml.FormulaParsing.Excel.Functions.RefAndLookup;
using ReactSpa.Controllers;

namespace ReactSpa.Extension
{
    public static class EPPLusExtensions
    {
        public static IEnumerable<T> MapSheetToObjects<T>(this ExcelWorksheet worksheet, int numOfRowSkips = 2,
            int takeRows = 500) where T : new()
        {
            Func<CustomAttributeData, bool> columnOnly = y => y.AttributeType == typeof(ColumnAttribute);

            var columns = typeof(T)
                .GetProperties()
                .Where(x => x.CustomAttributes.Any(columnOnly))
                .Select(p => new
                {
                    Property = p,
                    Column = p.GetCustomAttributes<ColumnAttribute>().First().ColumnIndex
                }).ToList();

            var rows = worksheet.Cells
                .Select(cell => cell.Start.Row)
                .Distinct()
                .OrderBy(x => x);

            var collection = rows.Skip(numOfRowSkips)
                .Take(takeRows)
                .Select(row =>
                {
                    var tnew = new T();
                    DateTime date;
                    for (var i = 0; i < columns.Count; i++)
                    {
                        var val = worksheet.Cells[row, columns[i].Column];
                        if (val.Value == null)
                            continue;
                        if (i == 0 || i == 7)
                        {
                            if (DateTime.TryParse(val.GetValue<string>(), out date))
                                columns[i].Property.SetValue(tnew, date.ToString("yyyy-MM-dd"));
                            continue;
                        }
                        if (i == 2 || i == 3 || i == 6)
                        {
                            if (DateTime.TryParse(val.GetValue<string>(), out date))
                                columns[i].Property.SetValue(tnew, date.ToString("HH:mm:ss"));
                            continue;
                        }
                        columns[i].Property.SetValue(tnew, val.GetValue<string>());
                    }
//                    columns.ForEach(col =>
//                    {
//                        var val = worksheet.Cells[row, col.Column];
//                        if (val.Value == null)
//                        {
//                            col.Property.SetValue(tnew, null);
//                            return;
//                        }
//                        if (col.Property.PropertyType == typeof(Int32))
//                        {
//                            col.Property.SetValue(tnew, val.GetValue<int>());
//                            return;
//                        }
//                        if (col.Property.PropertyType == typeof(double))
//                        {
//                            col.Property.SetValue(tnew, val.GetValue<double>());
//                            return;
//                        }
//                        if (col.Property.PropertyType == typeof(DateTime))
//                        {
//                            col.Property.SetValue(tnew, val.GetValue<DateTime>().ToString("yyyy-MM-dd HH:mm:ss"));
//                            return;
//                        }   
//                        col.Property.SetValue(tnew, val.GetValue<string>());
//                    });

                    return tnew;
                });

            return collection;
        }
    }
}