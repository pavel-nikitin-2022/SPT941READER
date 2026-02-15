using System;
using System.Data;
using Logika.Comms.Connections;
using Logika.Comms.Protocols.M4;
using Logika.Meters;

class ArchiveReader
{
  private readonly int READ_TIMEOUT = 15000;
  private readonly string _host;
  private readonly ushort _port;

  public ArchiveReader(string host, ushort port)
  {
    _host = host;
    _port = port;
  }

  public DataTable CreateTemplate(Logika4M meter, byte nt)
  {
    using var connection = new TCPConnection(READ_TIMEOUT, _host, _port);
    connection.Open();

    var protocol = new M4Protocol { connection = connection };

    object state;
    var archive = protocol.ReadIntervalArchiveDef(
        meter, null, nt, ArchiveType.Day, out state);

    return archive.Table;
  }

  public DataTable ReadDay(
      Logika4M meter,
      byte nt,
      DateTime day)
  {
    using var connection = new TCPConnection(
        readTimeout: READ_TIMEOUT,
        host: _host,
        port: _port);

    connection.Open();

    var protocol = new M4Protocol
    {
      connection = connection
    };

    object state;
    var archive = protocol.ReadIntervalArchiveDef(
        meter,
        null,
        nt,
        ArchiveType.Day,
        out state);

    float progress = 0;
    bool reading = true;

    while (reading)
    {
      reading = protocol.ReadIntervalArchive(
          meter,
          null,
          nt,
          archive,
          day,
          day,
          ref state,
          out progress);
    }

    return archive.Table;
  }
}

