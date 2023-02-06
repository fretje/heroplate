using System.Data;
using Microsoft.Data.SqlClient;

namespace Heroplate.Api.Infrastructure.Persistence.VarBinary;

// See http://www.syntaxwarriors.com/2013/stream-varbinary-data-to-and-from-mssql-using-csharp/
public class VarbinaryStream : Stream
{
    private readonly string _schemaName;
    private readonly string _tableName;
    private readonly string _binaryColumn;
    private readonly string _keyColumn;
    private readonly int _keyValue;
    private readonly string _keyColumn2;
    private readonly string _keyValue2;
    private readonly bool _allowRead;

    private bool _firstWrite = true;
    private long _readPosition;

    private SqlConnection? _connection;
    private SqlCommand? _readCommand;
    private SqlDataReader? _reader;

    public VarbinaryStream(
        string connectionString,
        string schemaName,
        string tableName,
        string binaryColumn,
        string keyColumn,
        int keyValue,
        string keyColumn2,
        string keyValue2,
        bool allowRead = false)
    {
        // create own connection with the connection string.
        _connection = new SqlConnection(connectionString);
        _schemaName = schemaName;
        _tableName = tableName;
        _binaryColumn = binaryColumn;
        _keyColumn = keyColumn;
        _keyValue = keyValue;
        _keyColumn2 = keyColumn2;
        _keyValue2 = keyValue2;

        _allowRead = allowRead;

        // only query the database for a result if we are going to be reading, otherwise skip.
        if (_allowRead)
        {
            if (_connection.State is not ConnectionState.Open)
            {
                _connection.Open();
            }

            _readCommand = new SqlCommand(
                $"SELECT TOP 1 [{_binaryColumn}] " +
                $"FROM [{_schemaName}].[{_tableName}] " +
                $"WHERE [{_keyColumn}] = @id " +
                  $"AND [{_keyColumn2}] = @id2",
                _connection);

            _readCommand.Parameters.Add(new SqlParameter("@id", _keyValue));
            _readCommand.Parameters.Add(new SqlParameter("@id2", _keyValue2));

            _reader = _readCommand.ExecuteReader(
                CommandBehavior.SequentialAccess |
                CommandBehavior.SingleResult |
                CommandBehavior.SingleRow |
                CommandBehavior.CloseConnection);

            if (!_reader.Read())
            {
                throw new ObjectNotFoundException();
            }
        }
    }

    // this method will be called as part of the Stream ímplementation when we try to write to our VarbinaryStream class.
    public override void Write(byte[] buffer, int offset, int count)
    {
        _ = _connection ?? throw new InvalidOperationException("Can't write without a connection.");

        if (_connection.State is not ConnectionState.Open)
        {
            _connection.Open();
        }

        if (_firstWrite)
        {
            // for the first write we just send the bytes to the Column
            using var command = new SqlCommand(
                $"""
                IF EXISTS (SELECT [{_keyColumn}]
                           FROM [{_schemaName}].[{_tableName}]
                           WHERE [{_keyColumn}] = @id
                             AND [{_keyColumn2}] = @id2)
                    UPDATE [{_schemaName}].[{_tableName}]
                    SET [{_binaryColumn}] = @firstchunk
                    WHERE [{_keyColumn}] = @id
                      AND [{_keyColumn2}] = @id2
                ELSE
                    INSERT INTO [{_schemaName}].[{_tableName}]
                        ([{_keyColumn}], [{_keyColumn2}], [{_binaryColumn}])
                    VALUES (@id, @id2, @firstchunk)
                """,
                _connection);

            command.Parameters.Add(new("@firstchunk", buffer));
            command.Parameters.Add(new("@id", _keyValue));
            command.Parameters.Add(new("@id2", _keyValue2));

            command.CommandTimeout = 60 * 5;

            command.ExecuteNonQuery();

            _firstWrite = false;
        }
        else
        {
            // for all updates after the first one we use the TSQL command .WRITE() to append the data in the database
            using var command = new SqlCommand(
                $"""
                UPDATE [{_schemaName}].[{_tableName}]
                SET [{_binaryColumn}].WRITE(@chunk, NULL, @length)
                WHERE [{_keyColumn}] = @id
                  AND [{_keyColumn2}] = @id2
                """,
                _connection);

            command.Parameters.Add(new("@chunk", buffer));
            command.Parameters.Add(new("@length", count));
            command.Parameters.Add(new("@id", _keyValue));
            command.Parameters.Add(new("@id2", _keyValue2));

            command.CommandTimeout = 60 * 5;

            command.ExecuteNonQuery();
        }
    }

    // this method will be called as part of the Stream ímplementation when we try to read from our VarbinaryStream class.
    public override int Read(byte[] buffer, int offset, int count)
    {
        _ = _reader ?? throw new InvalidOperationException("Can't read without a reader.");

        long bytesRead = _reader.GetBytes(0, _readPosition, buffer, offset, count);
        _readPosition += bytesRead;
        return (int)bytesRead;
    }

    public override bool CanRead => _allowRead;
    public override bool CanWrite => true;

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _reader?.Dispose();
            _reader = null;
            _readCommand?.Dispose();
            _readCommand = null;
            _connection?.Dispose();
            _connection = null;
        }

        base.Dispose(disposing);
    }

    #region unimplemented methods
    public override bool CanSeek => false;
    public override void Flush() => throw new NotImplementedException();
    public override long Length => throw new NotImplementedException();
    public override long Position
    {
        get => throw new NotImplementedException();
        set => throw new NotImplementedException();
    }

    public override long Seek(long offset, SeekOrigin origin) => throw new NotImplementedException();
    public override void SetLength(long value) => throw new NotImplementedException();
    #endregion unimplemented methods
}