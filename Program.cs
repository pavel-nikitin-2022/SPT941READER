using System;
using System.Text;
using Logika.Comms.Connections;
using Logika.Comms.Protocols;
using Logika.Comms.Protocols.M4;
using Logika.Meters;

class Program
{
  static byte GetDeviceAddress(M4Protocol protocol, Logika4M meter)
  {
    try
    {
      M4Packet hsPkt = protocol.Handshake(M4Protocol.BROADCAST, 0, false);
      byte nt = hsPkt.NT;
      Console.WriteLine($"Прибор ответил с NT = {nt}");

      var detectedType = M4Protocol.MeterTypeFromResponse(
          hsPkt.Data[0], hsPkt.Data[1], hsPkt.Data[2]);

      if (detectedType != meter)
        Console.WriteLine("Внимание: тип прибора не совпадает с автоопределением!");

      return nt;
    }
    catch
    {
      Console.WriteLine("Не удалось определить NT, использую 1");
      return 1;
    }
  }

  static void Main()
  {
    Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

    var connection = new TCPConnection(15000, "91.209.59.238", 8002);

    connection.Open();

    Meter meter = Protocol.AutodetectSPT(
        connection,
        BaudRate.Undefined,
        15000,
        true, false, false,
        null, null,
        out _, out _, out _);

    var protocol = new M4Protocol { connection = connection };

    if (meter is not Logika4M m4Meter)
      return;

    byte nt = GetDeviceAddress(protocol, m4Meter);

    var reader = new ArchiveReader("91.209.59.238", 8002);
    var flow = new ArchiveConsoleFlow(reader);

    flow.Run(m4Meter, nt);

    connection.Close();

    Console.WriteLine("\nPress any key to exit...");
    Console.ReadKey();
  }

}
