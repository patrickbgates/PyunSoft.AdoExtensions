using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace PyunSoft.AdoExtensions
{
    /// <summary>
    /// Defines extension methods for the <see cref="DbCommand"/> class.
    /// </summary>
    public static class DbCommandExtensions
    {
        /// <summary>
        /// Adds a parameter with a specific name and value to this command's parameter
        /// collection.
        /// </summary>
        /// <param name="command">The command whose parameter collection to add..</param>
        /// <param name="parameterName">The parameter's name.</param>
        /// <param name="value">The parameter's value.</param>
        /// <param name="parameterDirection">
        /// Optional. A value that indicates whether the paramter is input-only,
        /// bidirectional, output-only, or a stored procedure return value.
        /// </param>
        public static void AddParameterWithValue(
            this DbCommand command,
            string parameterName,
            object value,
            ParameterDirection parameterDirection = ParameterDirection.Input)
        {
            if (command == null)
            {
                throw new ArgumentNullException(nameof(command));
            }

            if (parameterName == null)
            {
                throw new ArgumentNullException(nameof(parameterName));
            }

            var parameter = command.CreateParameter();
            parameter.ParameterName = parameterName;
            parameter.Value = value;
            parameter.Direction = parameterDirection;
            command.Parameters.Add(parameter);
        }

        /// <summary>
        /// Executes the query and returns the result set as a <see cref="DataTable"/>.
        /// </summary>
        /// <param name="command">The command to execute.</param>
        /// <returns>
        /// A <see cref="DataTable"/> that contains the query's result set.
        /// </returns>
        public static DataTable ExecuteDataTable(this DbCommand command)
        {
            if (command == null)
            {
                throw new ArgumentNullException(nameof(command));
            }

            var factory = DbProviderFactories.GetFactory(command.Connection);
            using var adapter = factory.CreateDataAdapter();
            adapter.SelectCommand = command;
            var dataTable = new DataTable();
            adapter.Fill(dataTable);
            return dataTable;
        }

        /// <summary>
        /// Asynchronously executes the query and returns the result set as a <see
        /// cref="DataTable"/>.
        /// </summary>
        /// <param name="command">The command to execute.</param>
        /// <param name="cancellationToken">
        /// A token to cancel the asynchronous operation.
        /// </param>
        /// <returns>A task representing the asynchronous operation.</returns>
        /// <remarks>
        /// At the time of writing, there is no asynchronous counterpart to
        /// <c>DbDataAdapter.Fill</c>. This method uses a <see cref="DbDataReader"/> as a
        /// workaround, and therefore may not peform as well as the synchronous method in
        /// some situations.
        /// </remarks>
        public static async Task<DataTable> ExecuteDataTableAsync(this DbCommand command, CancellationToken cancellationToken = default)
        {
            if (command == null)
            {
                throw new ArgumentNullException(nameof(command));
            }

            using var reader = await command.ExecuteReaderAsync(cancellationToken);
            var dataTable = new DataTable();
            dataTable.Load(reader);
            return dataTable;
        }

        /// <summary>
        /// Executes the query and returns the result set as a <see
        /// cref="Dictionary{TKey, TValue}"/>.
        /// </summary>
        /// <typeparam name="TKey">The dictionary's key's data type.</typeparam>
        /// <typeparam name="TValue">The dictionary's value's data type.</typeparam>
        /// <param name="command">The command to execute.</param>
        /// <param name="keyColumnName">
        /// The name of the column in the query's result set to use as the dictionary's
        /// key.
        /// </param>
        /// <param name="valueColumnName">
        /// The name of the column in the query's result set to use as the dictionary's
        /// value.
        /// </param>
        /// <returns>
        /// A dictionary whose members' keys and values correspond to the result set
        /// columns specified by <paramref name="keyColumnName"/> and <paramref
        /// name="valueColumnName"/>, respectively.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="keyColumnName"/> is <see langword="null"/> -or- <paramref
        /// name="valueColumnName"/> is <see langword="null"/>.
        /// </exception>
        public static Dictionary<TKey, TValue> ExecuteDictionary<TKey, TValue>(
            this DbCommand command,
            string keyColumnName,
            string valueColumnName) where TKey : IConvertible where TValue : IConvertible
        {
            if (command == null)
            {
                throw new ArgumentNullException(nameof(command));
            }

            if (keyColumnName == null)
            {
                throw new ArgumentNullException(nameof(keyColumnName));
            }

            if (valueColumnName == null)
            {
                throw new ArgumentNullException(nameof(valueColumnName));
            }

            var data = new Dictionary<object, object>();
            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                data.Add(reader[keyColumnName], reader[valueColumnName]);
            }

            return data.ToDictionary(kvp => (TKey)Convert.ChangeType(kvp.Key, typeof(TKey)), kvp => (TValue)Convert.ChangeType(kvp.Value, typeof(TValue)));
        }

        /// <summary>
        /// Asynchronously executes the query and returns the result set as a <see
        /// cref="Dictionary{TKey, TValue}"/>.
        /// </summary>
        /// <typeparam name="TKey">The dictionary's key's data type.</typeparam>
        /// <typeparam name="TValue">The dictionary's value's data type.</typeparam>
        /// <param name="command">The command to execute.</param>
        /// <param name="keyColumnName">
        /// The name of the column in the query's result set to use as the dictionary's
        /// key.
        /// </param>
        /// <param name="valueColumnName">
        /// The name of the column in the query's result set to use as the dictionary's
        /// value.
        /// </param>
        /// <param name="cancellationToken">
        /// A token to cancel the asynchronous operation.
        /// </param>
        /// <returns>A task representing the asynchronous operation.</returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="keyColumnName"/> is <see langword="null"/> -or- <paramref
        /// name="valueColumnName"/> is <see langword="null"/>.
        /// </exception>
        public static async Task<Dictionary<TKey, TValue>> ExecuteDictionaryAsync<TKey, TValue>(
            this DbCommand command,
            string keyColumnName,
            string valueColumnName,
            CancellationToken cancellationToken = default) where TKey : IConvertible where TValue : IConvertible
        {
            if (command == null)
            {
                throw new ArgumentNullException(nameof(command));
            }

            if (keyColumnName == null)
            {
                throw new ArgumentNullException(nameof(keyColumnName));
            }

            if (valueColumnName == null)
            {
                throw new ArgumentNullException(nameof(valueColumnName));
            }

            var data = new Dictionary<object, object>();
            using var reader = await command.ExecuteReaderAsync(cancellationToken);
            while (await reader.ReadAsync(cancellationToken))
            {
                data.Add(reader[keyColumnName], reader[valueColumnName]);
            }

            return data.ToDictionary(kvp => (TKey)Convert.ChangeType(kvp.Key, typeof(TKey)), kvp => (TValue)Convert.ChangeType(kvp.Value, typeof(TValue)));
        }

        /// <summary>
        /// Executes the query and returns the result set as a <see cref="List{T}"/>.
        /// </summary>
        /// <typeparam name="T">The return value's data type.</typeparam>
        /// <param name="command">The command to execute.</param>
        /// <param name="columnName">
        /// The name of the column in the query's result set to include in the list.
        /// </param>
        /// <returns>
        /// A list whose members correspond to the result set column specified by
        /// <paramref name="columnName"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="columnName"/> is <see langword="null"/>.
        /// </exception>
        public static List<T> ExecuteList<T>(this DbCommand command, string columnName) where T : IConvertible
        {
            if (columnName == null)
            {
                throw new ArgumentNullException(nameof(columnName));
            }

            var data = new List<object>();
            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                data.Add(reader[columnName]);
            }

            return data.ConvertAll(datum => (T)Convert.ChangeType(datum, typeof(T)));
        }

        /// <summary>
        /// Asynchronously executes the query and returns the result set as a <see
        /// cref="List{T}"/>.
        /// </summary>
        /// <typeparam name="T">The return value's data type.</typeparam>
        /// <param name="command">The command to execute.</param>
        /// <param name="columnName">
        /// The name of the column in the query's result set to include in the list.
        /// </param>
        /// <param name="cancellationToken">
        /// A token to cancel the asynchronous operation.
        /// </param>
        /// <returns>A task representing the asynchronous operation.</returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="columnName"/> is <see langword="null"/>.
        /// </exception>
        public static async Task<List<T>> ExecuteListAsync<T>(
            this DbCommand command,
            string columnName,
            CancellationToken cancellationToken = default) where T : IConvertible
        {
            if (command == null)
            {
                throw new ArgumentNullException(nameof(command));
            }

            if (columnName == null)
            {
                throw new ArgumentNullException(nameof(columnName));
            }

            var data = new List<object>();
            using var reader = await command.ExecuteReaderAsync(cancellationToken);

            while (await reader.ReadAsync(cancellationToken))
            {
                data.Add(reader[columnName]);
            }

            return data.ConvertAll(datum => (T)Convert.ChangeType(datum, typeof(T)));
        }

        /// <summary>
        /// Executes the query, and returns the first column of the first row in the
        /// result set returned by the query as <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The data type to return.</typeparam>
        /// <param name="command">The command to execute.</param>
        /// <returns>
        /// The first column of the first row in the result set returned by the query as
        /// <typeparamref name="T"/>.
        /// </returns>
        public static T ExecuteScalar<T>(this DbCommand command) where T : IConvertible
        {
            if (command == null)
            {
                throw new ArgumentNullException(nameof(command));
            }

            var result = command.ExecuteScalar();

            if (result == null || DBNull.Value.Equals(result))
            {
                return default;
            }

            return (T)Convert.ChangeType(result, typeof(T));
        }

        /// <summary>
        /// Asynchronously executes the query, and returns the first column of the first
        /// row in the result set returned by the query as <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The data type to return.</typeparam>
        /// <param name="command">The command to execute.</param>
        /// <param name="cancellationToken">
        /// A token to cancel the asynchronous operation.
        /// </param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public static async Task<T> ExecuteScalarAsync<T>(this DbCommand command, CancellationToken cancellationToken = default) where T : IConvertible
        {
            if (command == null)
            {
                throw new ArgumentNullException(nameof(command));
            }

            var result = await command.ExecuteScalarAsync(cancellationToken);

            if (result == null || DBNull.Value.Equals(result))
            {
                return default;
            }

            return (T)Convert.ChangeType(result, typeof(T));
        }

        /// <summary>
        /// Executes the query, and returns the first column of the first row in the
        /// result set returned by the query as <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The data type to return.</typeparam>
        /// <param name="command">The command to execute.</param>
        /// <returns>
        /// The first column of the first row in the result set returned by the query as
        /// <typeparamref name="T"/>.
        /// </returns>
        public static T? ExecuteScalarNullable<T>(this DbCommand command) where T : struct, IConvertible
        {
            if (command == null)
            {
                throw new ArgumentNullException(nameof(command));
            }

            var result = command.ExecuteScalar();

            if (result == null || DBNull.Value.Equals(result))
            {
                return null;
            }

            return (T)Convert.ChangeType(result, typeof(T));
        }

        /// <summary>
        /// Asynchronously executes the query, and returns the first column of the first
        /// row in the result set returned by the query as <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The data type to return.</typeparam>
        /// <param name="command">The command to execute.</param>
        /// <param name="cancellationToken">
        /// A token to cancel the asynchronous operation.
        /// </param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public static async Task<T?> ExecuteScalarNullableAsync<T>(this DbCommand command, CancellationToken cancellationToken = default) where T : struct, IConvertible
        {
            if (command == null)
            {
                throw new ArgumentNullException(nameof(command));
            }

            var result = await command.ExecuteScalarAsync(cancellationToken);

            if (result == null || DBNull.Value.Equals(result))
            {
                return null;
            }

            return (T)Convert.ChangeType(result, typeof(T));
        }
    }
}
