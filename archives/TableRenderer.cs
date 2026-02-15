using System;
using System.Data;
using System.Linq;
using Spectre.Console;
using System.Collections.Generic;

static class TableRenderer
{
  public static void Render(DataTable table, List<string> selectedColumns)
  {
    var consoleTable = new Table()
        .Border(TableBorder.Rounded)
        .Expand();

    var columns = table.Columns
        .Cast<DataColumn>()
        .Where(c => selectedColumns.Count == 0 ||
                    selectedColumns.Contains(c.ColumnName))
        .ToList();

    foreach (var col in columns)
      consoleTable.AddColumn($"[green]{col.ColumnName}[/]");

    foreach (DataRow row in table.Rows)
    {
      var values = columns.Select(col =>
      {
        if (row[col] == DBNull.Value)
          return "";

        if (col.DataType == typeof(DateTime))
          return ((DateTime)row[col]).ToString("yyyy-MM-dd");

        return row[col].ToString();
      }).ToArray();

      consoleTable.AddRow(values);
    }

    AnsiConsole.Write(consoleTable);
  }
}

