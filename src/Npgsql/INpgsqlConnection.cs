using System;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Net.Security;
using System.Threading;
using System.Threading.Tasks;
using System.Transactions;
using IsolationLevel = System.Data.IsolationLevel;

namespace Npgsql
{
    /// <summary>
    /// Blah
    /// </summary>
    public interface INpgsqlConnection : IDbConnection, IDisposable, IComponent
    {
        /// <summary>
        /// This is the asynchronous version of <see cref="IDbConnection.Open"/>.
        /// </summary>
        /// <remarks>
        /// Do not invoke other methods and properties of the <see cref="NpgsqlConnection"/> object until the returned Task is complete.
        /// </remarks>
        /// <param name="cancellationToken">The cancellation instruction.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task OpenAsync(CancellationToken cancellationToken);

        /// <summary>
        /// Backend server host name.
        /// </summary>
        string Host { get; }

        /// <summary>
        /// Backend server port.
        /// </summary>
        int Port { get; }

        /// <summary>
        /// If true, the connection will attempt to use SslStream instead of an internal TlsClientStream.
        /// </summary>
        bool UseSslStream { get; }
        /// <summary>
        /// Gets the time to wait while trying to execute a command
        /// before terminating the attempt and generating an error.
        /// </summary>
        /// <value>The time (in seconds) to wait for a command to complete. The default value is 20 seconds.</value>
        int CommandTimeout { get; }
        /// <summary>
        /// Gets the time to wait before closing unused connections in the pool if the count
        /// of all connections exeeds MinPoolSize.
        /// </summary>
        /// <remarks>
        /// If connection pool contains unused connections for ConnectionLifeTime seconds,
        /// the half of them will be closed. If there will be unused connections in a second
        /// later then again the half of them will be closed and so on.
        /// This strategy provide smooth change of connection count in the pool.
        /// </remarks>
        /// <value>The time (in seconds) to wait. The default value is 15 seconds.</value>
        int ConnectionLifeTime { get; }
        /// <summary>
        /// Gets the database server name.
        /// </summary>
        string DataSource { get; }
        /// <summary>
        /// Gets flag indicating if we are using Synchronous notification or not.
        /// The default value is false.
        /// </summary>
        bool ContinuousProcessing { get; }
        /// <summary>
        /// Whether to use Windows integrated security to log in.
        /// </summary>
        bool IntegratedSecurity { get; }
        /// <summary>
        /// User name.
        /// </summary>
        string UserName { get; }
        /// <summary>
        /// Gets the current state of the connection.
        /// </summary>
        /// <value>A bitwise combination of the <see cref="System.Data.ConnectionState">ConnectionState</see> values. The default is <b>Closed</b>.</value>
        ConnectionState FullState { get; }
        /// <summary>
        /// Creates and returns a <see cref="NpgsqlCommand">NpgsqlCommand</see>
        /// object associated with the <see cref="NpgsqlConnection">NpgsqlConnection</see>.
        /// </summary>
        /// <returns>A <see cref="NpgsqlCommand">NpgsqlCommand</see> object.</returns>
        new NpgsqlCommand CreateCommand();
        /// <summary>
        /// Begins a database transaction.
        /// </summary>
        /// <returns>A <see cref="NpgsqlTransaction">NpgsqlTransaction</see>
        /// object representing the new transaction.</returns>
        /// <remarks>
        /// Currently there's no support for nested transactions.
        /// </remarks>
        new NpgsqlTransaction BeginTransaction();
        /// <summary>
        /// Enlist transation.
        /// </summary>
        /// <param name="transaction"></param>
        void EnlistTransaction(Transaction transaction);
        /// <summary>
        /// Occurs on NoticeResponses from the PostgreSQL backend.
        /// </summary>
        event NoticeEventHandler Notice;
        /// <summary>
        /// Occurs on NotificationResponses from the PostgreSQL backend.
        /// </summary>
        event NotificationEventHandler Notification;
        /// <summary>
        /// Selects the local Secure Sockets Layer (SSL) certificate used for authentication.
        /// </summary>
        /// <remarks>
        /// See <see href="https://msdn.microsoft.com/en-us/library/system.net.security.localcertificateselectioncallback(v=vs.110).aspx"/>
        /// </remarks>
        ProvideClientCertificatesCallback ProvideClientCertificatesCallback { get; set; }
        /// <summary>
        /// Verifies the remote Secure Sockets Layer (SSL) certificate used for authentication.
        /// Ignored if <see cref="NpgsqlConnectionStringBuilder.TrustServerCertificate"/> is set.
        /// </summary>
        /// <remarks>
        /// See <see href="https://msdn.microsoft.com/en-us/library/system.net.security.remotecertificatevalidationcallback(v=vs.110).aspx"/>
        /// </remarks>
        RemoteCertificateValidationCallback UserCertificateValidationCallback { get; set; }
        /// <summary>
        /// Version of the PostgreSQL backend.
        /// This can only be called when there is an active connection.
        /// </summary>
        Version PostgreSqlVersion { get; }
        /// <summary>
        /// PostgreSQL server version.
        /// </summary>
        string ServerVersion { get; }
        /// <summary>
        /// Process id of backend server.
        /// This can only be called when there is an active connection.
        /// </summary>
        int ProcessID { get; }
        /// <summary>
        /// Report whether the backend is expecting standard conformant strings.
        /// In version 8.1, Postgres began reporting this value (false), but did not actually support standard conformant strings.
        /// In version 8.2, Postgres began supporting standard conformant strings, but defaulted this flag to false.
        /// As of version 9.1, this flag defaults to true.
        /// </summary>
        bool UseConformantStrings { get; }
        /// <summary>
        /// Report whether the backend understands the string literal E prefix (>= 8.1).
        /// </summary>
        bool Supports_E_StringPrefix { get; }
        /// <summary>
        /// Report whether the backend understands the hex byte format (>= 9.0).
        /// </summary>
        bool SupportsHexByteFormat { get; }
        /// <summary>
        /// Begins a binary COPY FROM STDIN operation, a high-performance data import mechanism to a PostgreSQL table.
        /// </summary>
        /// <param name="copyFromCommand">A COPY FROM STDIN SQL command</param>
        /// <returns>A <see cref="NpgsqlBinaryImporter"/> which can be used to write rows and columns</returns>
        /// <remarks>
        /// See http://www.postgresql.org/docs/current/static/sql-copy.html.
        /// </remarks>
        NpgsqlBinaryImporter BeginBinaryImport(string copyFromCommand);
        /// <summary>
        /// Begins a binary COPY TO STDIN operation, a high-performance data export mechanism from a PostgreSQL table.
        /// </summary>
        /// <param name="copyToCommand">A COPY TO STDIN SQL command</param>
        /// <returns>A <see cref="NpgsqlBinaryExporter"/> which can be used to read rows and columns</returns>
        /// <remarks>
        /// See http://www.postgresql.org/docs/current/static/sql-copy.html.
        /// </remarks>
        NpgsqlBinaryExporter BeginBinaryExport(string copyToCommand);
        /// <summary>
        /// Begins a textual COPY FROM STDIN operation, a data import mechanism to a PostgreSQL table.
        /// It is the user's responsibility to send the textual input according to the format specified
        /// in <paramref name="copyFromCommand"/>.
        /// </summary>
        /// <param name="copyFromCommand">A COPY FROM STDIN SQL command</param>
        /// <returns>
        /// A TextWriter that can be used to send textual data.</returns>
        /// <remarks>
        /// See http://www.postgresql.org/docs/current/static/sql-copy.html.
        /// </remarks>
        TextWriter BeginTextImport(string copyFromCommand);
        /// <summary>
        /// Begins a textual COPY FROM STDIN operation, a data import mechanism to a PostgreSQL table.
        /// It is the user's responsibility to parse the textual input according to the format specified
        /// in <paramref name="copyToCommand"/>.
        /// </summary>
        /// <param name="copyToCommand">A COPY TO STDIN SQL command</param>
        /// <returns>
        /// A TextReader that can be used to read textual data.</returns>
        /// <remarks>
        /// See http://www.postgresql.org/docs/current/static/sql-copy.html.
        /// </remarks>
        TextReader BeginTextExport(string copyToCommand);
        /// <summary>
        /// Begins a raw binary COPY operation (TO or FROM), a high-performance data export/import mechanism to a PostgreSQL table.
        /// Note that unlike the other COPY API methods, <see cref="BeginRawBinaryCopy"/> doesn't implement any encoding/decoding
        /// and is unsuitable for structured import/export operation. It is useful mainly for exporting a table as an opaque
        /// blob, for the purpose of importing it back later.
        /// </summary>
        /// <param name="copyCommand">A COPY FROM STDIN or COPY TO STDIN SQL command</param>
        /// <returns>A <see cref="NpgsqlRawCopyStream"/> that can be used to read or write raw binary data.</returns>
        /// <remarks>
        /// See http://www.postgresql.org/docs/current/static/sql-copy.html.
        /// </remarks>
        NpgsqlRawCopyStream BeginRawBinaryCopy(string copyCommand);
        /// <summary>
        /// Registers an enum type for use with this connection.
        ///
        /// Enum labels are mapped by string. The .NET enum labels must correspond exactly to the PostgreSQL labels;
        /// if another label is used in the database, this can be specified for each label with a EnumLabelAttribute.
        /// If there is a discrepancy between the .NET and database labels while an enum is read or written,
        /// an exception will be raised.
        ///
        /// Can only be invoked on an open connection; if the connection is closed the registration is lost.
        /// </summary>
        /// <remarks>
        /// To avoid registering the type for each connection, use the <see cref="NpgsqlConnection.RegisterEnumGlobally{T}"/> method.
        /// </remarks>
        /// <param name="pgName">
        /// A PostgreSQL type name for the corresponding enum type in the database.
        /// If null, the .NET type's name in lowercase will be used
        /// </param>
        /// <typeparam name="TEnum">The .NET enum type to be registered</typeparam>
        void RegisterEnum<TEnum>(string pgName = null)
            where TEnum : struct;
        /// <summary>
        /// Returns the supported collections
        /// </summary>
        DataTable GetSchema();
        /// <summary>
        /// Returns the schema collection specified by the collection name.
        /// </summary>
        /// <param name="collectionName">The collection name.</param>
        /// <returns>The collection specified.</returns>
        DataTable GetSchema(string collectionName);
        /// <summary>
        /// Returns the schema collection specified by the collection name filtered by the restrictions.
        /// </summary>
        /// <param name="collectionName">The collection name.</param>
        /// <param name="restrictions">
        /// The restriction values to filter the results.  A description of the restrictions is contained
        /// in the Restrictions collection.
        /// </param>
        /// <returns>The collection specified.</returns>
        DataTable GetSchema(string collectionName, string[] restrictions);
    }
}
