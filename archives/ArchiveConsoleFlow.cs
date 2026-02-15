using System;
using System.Data;
using System.Linq;
using Logika.Comms.Connections;
using Logika.Comms.Protocols.M4;
using Logika.Meters;
using Spectre.Console;
using System.Collections.Generic;
using System.Threading;

class ArchiveConsoleFlow
{
  private readonly ArchiveReader _reader;

  public ArchiveConsoleFlow(ArchiveReader reader)
  {
    _reader = reader;
  }

  public void Run(Logika4M meter, byte nt)
  {
    var start = AskDate("Дата начала");
    var end = AskDate("Дата окончания");

    if (end < start)
    {
      AnsiConsole.MarkupLine("[red]Дата окончания меньше даты начала[/]");
      return;
    }

    var templateTable = _reader.CreateTemplate(meter, nt);
    var selectedColumns = AskColumns(templateTable);

    var result = templateTable.Clone();

    var days = Enumerable.Range(0, (end - start).Days + 1)
        .Select(offset => start.AddDays(offset))
        .ToList();

    AnsiConsole.Progress()
        .Start(ctx =>
        {
          var task = ctx.AddTask("[green]Чтение архива[/]");

          foreach (var day in days)
          {
            try
            {
              var dayTable = _reader.ReadDay(meter, nt, day);

              foreach (DataRow row in dayTable.Rows)
                result.ImportRow(row);
            }
            catch (Exception ex)
            {
              AnsiConsole.MarkupLine(
                      $"[yellow] День {day:yyyy-MM-dd} пропущен: {ex.Message}[/]");
            }

            task.Increment(100.0 / days.Count);
            Thread.Sleep(200);
          }
        });

    TableRenderer.Render(result, selectedColumns);
  }

  private DateTime AskDate(string title)
  {
    return AnsiConsole.Prompt(
        new TextPrompt<DateTime>($"[green]{title}[/] (yyyy-MM-dd)")
            .PromptStyle("yellow"));
  }

  private List<string> AskColumns(DataTable table)
  {
    return AnsiConsole.Prompt(
        new MultiSelectionPrompt<string>()
            .Title("[green]Выберите колонки[/]")
            .AddChoices(table.Columns
                .Cast<DataColumn>()
                .Select(c => c.ColumnName)));
  }
}
