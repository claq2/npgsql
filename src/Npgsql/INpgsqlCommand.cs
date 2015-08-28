using System;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Npgsql
{
    /// <summary>
    /// Represents a SQL statement or function (stored procedure) to execute
    /// against a PostgreSQL database. This class cannot be inherited.
    /// </summary>
    public interface INpgsqlCommand : IDbCommand, IDisposable
    {
        /// <summary>
        /// Design time visible.
        /// </summary>
        bool DesignTimeVisible { get; set; }

        /// <summary>
        /// Returns whether this query will execute as a prepared (compiled) query.
        /// </summary>
        bool IsPrepared { get; }

        /// <summary>
        /// Marks all of the query's result columns as either known or unknown.
        /// Unknown results column are requested them from PostgreSQL in text format, and Npgsql makes no
        /// attempt to parse them. They will be accessible as strings only.
        /// </summary>
        bool AllResultTypesAreUnknown { get; set; }

        /// <summary>
        /// Marks the query's result columns as known or unknown, on a column-by-column basis.
        /// Unknown results column are requested them from PostgreSQL in text format, and Npgsql makes no
        /// attempt to parse them. They will be accessible as strings only.
        /// </summary>
        /// <remarks>
        /// If the query includes several queries (e.g. SELECT 1; SELECT 2), this will only apply to the first
        /// one. The rest of the queries will be fetched and parsed as usual.
        ///
        /// The array size must correspond exactly to the number of result columns the query returns, or an
        /// error will be raised.
        /// </remarks>
        bool[] UnknownResultTypeList { get; set; }

        /// <summary>
        /// Asynchronous version of <see cref="IDbCommand.ExecuteNonQuery"/>
        /// </summary>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <returns>A task representing the asynchronous operation, with the number of rows affected if known; -1 otherwise.</returns>
        Task<int> ExecuteNonQueryAsync(CancellationToken cancellationToken);

        /// <summary>
        /// Asynchronous version of <see cref="IDbCommand.ExecuteScalar"/>
        /// </summary>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <returns>A task representing the asynchronous operation, with the first column of the
        /// first row in the result set, or a null reference if the result set is empty.</returns>
        Task<object> ExecuteScalarAsync(CancellationToken cancellationToken);

        new NpgsqlParameter CreateParameter();
    }
}
