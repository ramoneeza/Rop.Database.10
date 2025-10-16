using System.Data;
using System.Data.Common;
using System.Diagnostics;

namespace Rop.Database;
public partial class AbsDatabase
{
    private R _UnitOfWorkWithTr<R,T>(Func<DbConnection,DbTransaction,T> funcion,Func<T,R> map,Func<Exception,R> mapex)
    {
        using var conn = FactoryConnection();
        try
        {
            conn.Open();
            using var tr = conn.BeginTransaction();
            try
            {
                var res = funcion(conn, tr);
                tr.Commit(); 
                return map(res);
            }
            catch (Exception ex)
            {
                tr.Rollback();
                return mapex(ex);
            }
        }
        finally
        {
            conn.Close();
        }
    }
    private R _UnitOfWorkNoTr<R, T>(Func<DbConnection, T> funcion, Func<T, R> map, Func<Exception, R> mapex)
    {
        using var conn = FactoryConnection();
        try
        {
            conn.Open();
            var res = funcion(conn);
            return map(res);
        }
        catch (Exception ex)
        {
            return mapex(ex);
        }
        finally
        {
            conn.Close();
        }
    }
    private async Task<R> _UnitOfWorkWithTrAsync<R, T>(Func<DbConnection, DbTransaction, Task<T>> funcion, Func<T, R> map, Func<Exception, R> mapex)
    {
        try
        {
            await using var conn = FactoryConnection();
            await conn.OpenAsync();
            try
            {
                await using var tr = await conn.BeginTransactionAsync();
                try
                {
                    var res = await funcion(conn, tr);
                    await tr.CommitAsync();
                    return map(res);
                }
                catch (Exception ex)
                {
                    await tr.RollbackAsync();
                    return mapex(ex);
                }
            }
            finally
            {
                await conn.CloseAsync();
            }
        }
        catch (Exception ex)
        {
            return mapex(ex);
        }
    }
    private async Task<R> _UnitOfWorkNoTrAsync<R, T>(Func<DbConnection, Task<T>> funcion, Func<T, R> map, Func<Exception, R> mapex)
    {
        try
        {
            await using var conn = FactoryConnection();
            await conn.OpenAsync();
            try
            {
                var res = await funcion(conn);
                return map(res);
            }
            catch (Exception ex)
            {
                return mapex(ex);
            }
            finally
            {
                await conn.CloseAsync();
            }
        }
        catch (Exception ex)
        {
            return mapex(ex);
        }
    }

    /// <summary>
    /// Executes a transactional unit of work that returns a <see cref="Result{T}"/>.
    /// The function is invoked with an open <see cref="DbConnection"/> and an active <see cref="DbTransaction"/>.
    /// The transaction is committed when the result is successful; otherwise it is rolled back.
    /// </summary>
    /// <typeparam name="T">Type returned by the operation.</typeparam>
    /// <param name="funcion">Function that performs the operation using the connection and transaction.</param>
    /// <returns>A <see cref="Result{T}"/> produced by the function or an exception result if an error occurs.</returns>
    public Result<T> UnitOfWorkSingle<T>(Func<DbConnection, DbTransaction, Result<T>> funcion)
    {
        return _UnitOfWorkWithTr<Result<T>, Result<T>>(funcion, r => r, ex => new ExceptionError(ex));
    }
    
    /// <summary>
    /// Executes a transactional unit of work that returns a plain value of type <typeparamref name="T"/>.
    /// The function is invoked with an open <see cref="DbConnection"/> and an active <see cref="DbTransaction"/>.
    /// On success the transaction is committed; on exception it is rolled back and an exception result is returned.
    /// </summary>
    /// <typeparam name="T">Type returned by the operation.</typeparam>
    /// <param name="funcion">Function that performs the operation using the connection and transaction.</param>
    /// <returns>A <see cref="Result{T}"/> wrapping the returned value or an exception result.</returns>
    public Result<T> UnitOfWorkSingle<T>(Func<DbConnection, DbTransaction, T> funcion)
    {
        return _UnitOfWorkWithTr<Result<T>, T>(funcion, r => new Result<T>(r), ex => new ExceptionError(ex));
    }
    /// <summary>
    /// Executes a transactional unit of work that returns an <see cref="EnumerableResult{T}"/>.
    /// The function receives an open connection and a transaction.
    /// Transaction is committed when the result is successful; otherwise it is rolled back.
    /// </summary>
    /// <typeparam name="T">Element type of the enumerable result.</typeparam>
    /// <param name="funcion">Function that performs the operation using the connection and transaction.</param>
    /// <returns>An <see cref="EnumerableResult{T}"/> produced by the function or an exception result.</returns>
    public EnumerableResult<T> UnitOfWork<T>(Func<DbConnection, DbTransaction, EnumerableResult<T>> funcion)
    {
        return _UnitOfWorkWithTr<EnumerableResult<T>, EnumerableResult<T>>(funcion, r => r, ex => new ExceptionError(ex));
    }
    /// <summary>
    /// Executes a transactional unit of work that returns a <see cref="List{T}"/>.
    /// The function receives an open connection and a transaction. The transaction is committed on success.
    /// </summary>
    /// <typeparam name="T">Element type of the list.</typeparam>
    /// <param name="funcion">Function that performs the operation using the connection and transaction.</param>
    /// <returns>An <see cref="EnumerableResult{T}"/> wrapping the returned list or an exception result.</returns>
    public EnumerableResult<T> UnitOfWork<T>(Func<DbConnection, DbTransaction, IReadOnlyList<T>> funcion)
    {
        return _UnitOfWorkWithTr<EnumerableResult<T>, IReadOnlyList<T>>(funcion, r => new EnumerableResult<T>(r), ex => new ExceptionError(ex));
    }

    /// <summary>
    /// Asynchronous wrapper for <see cref="UnitOfWorkAtomic{T}(Func{IDbConnection,IDbTransaction,Result{T}})"/>.
    /// Runs the synchronous implementation on a thread-pool thread.
    /// </summary>
    /// <typeparam name="T">Type returned by the operation.</typeparam>
    /// <param name="funcion">Function that performs the operation using the connection and transaction.</param>
    /// <returns>A task that returns a <see cref="Result{T}"/>.</returns>
    public async Task<Result<T>> UnitOfWorkSingleAsync<T>(Func<DbConnection, DbTransaction, Task<Result<T>>> funcion)
    {
        return await _UnitOfWorkWithTrAsync(funcion, r => r, ex => new ExceptionError(ex));
    }

    /// <summary>
    /// Asynchronous wrapper for <see cref="UnitOfWorkAtomic{T}(Func{IDbConnection,IDbTransaction,T})"/>.
    /// Runs the synchronous implementation on a thread-pool thread.
    /// </summary>
    /// <typeparam name="T">Type returned by the operation.</typeparam>
    /// <param name="funcion">Function that performs the operation using the connection and transaction.</param>
    /// <returns>A task that returns a <see cref="Result{T}"/>.</returns>
    public async Task<Result<T>> UnitOfWorkSingleAsync<T>(Func<DbConnection, DbTransaction, Task<T>> funcion)
    {
        return await _UnitOfWorkWithTrAsync(funcion, r => new Result<T>(r), ex => new ExceptionError(ex));
    }

    /// <summary>
    /// Asynchronous wrapper for <see cref="UnitOfWork{T}(Func{IDbConnection,IDbTransaction,EnumerableResult{T}})"/>.
    /// Runs the synchronous implementation on a thread-pool thread.
    /// </summary>
    /// <typeparam name="T">Element type of the enumerable result.</typeparam>
    /// <param name="funcion">Function that performs the operation using the connection and transaction.</param>
    /// <returns>A task that returns an <see cref="EnumerableResult{T}"/>.</returns>
    public async Task<EnumerableResult<T>> UnitOfWorkAsync<T>(Func<DbConnection,DbTransaction,Task<EnumerableResult<T>>> funcion)
    {
        return await _UnitOfWorkWithTrAsync(funcion, r => r, ex => new ExceptionError(ex));
    }
    public async Task<EnumerableResult<T>> UnitOfWorkAsync<T>(Func<DbConnection,DbTransaction,Task<IReadOnlyList<T>>> funcion)
    {
                return await _UnitOfWorkWithTrAsync(funcion, r => new EnumerableResult<T>(r), ex => new ExceptionError(ex));
    }

    /// <summary>
    /// Executes a unit of work against an open connection and returns a <see cref="Result{T}"/>.
    /// The connection is opened and closed by this method.
    /// </summary>
    /// <typeparam name="T">Type returned by the operation.</typeparam>
    /// <param name="funcion">Function executed with the open connection.</param>
    /// <returns>A <see cref="Result{T}"/> produced by the function or an exception result.</returns>
    public Result<T> UnitOfWorkSingle<T>(Func<DbConnection,Result<T>> funcion)
    {
        return _UnitOfWorkNoTr(funcion, r => r, ex => new ExceptionError(ex));
    }

    /// <summary>
    /// Executes a unit of work against an open connection and returns a plain value wrapped into <see cref="Result{T}"/>.
    /// The connection is opened and closed by this method.
    /// </summary>
    /// <typeparam name="T">Type returned by the operation.</typeparam>
    /// <param name="funcion">Function executed with the open connection.</param>
    /// <returns>A <see cref="Result{T}"/> wrapping the returned value or an exception result.</returns>
    public Result<T> UnitOfWorkSingle<T>(Func<DbConnection,T?> funcion)
    {
        return _UnitOfWorkNoTr(funcion, r => new Result<T>(r), ex => new ExceptionError(ex));
    }

    /// <summary>
    /// Asynchronous wrapper for <see cref="UnitOfWorkSingle{T}(Func{IDbConnection,Result{T}})"/>.
    /// </summary>
    /// <typeparam name="T">Type returned by the operation.</typeparam>
    /// <param name="funcion">Function executed with the open connection.</param>
    /// <returns>Task that returns a <see cref="Result{T}"/>.</returns>
    
    public Task<Result<T>> UnitOfWorkSingleAsync<T>(Func<DbConnection,Task<Result<T>>> funcion)
    {
        return _UnitOfWorkNoTrAsync(funcion, r => r, ex => new ExceptionError(ex));
    }
    /// <summary>
    /// Asynchronous wrapper for <see cref="UnitOfWorkSingle{T}(Func{IDbConnection,T?})"/>.
    /// </summary>
    /// <typeparam name="T">Type returned by the operation.</typeparam>
    /// <param name="funcion">Function executed with the open connection.</param>
    /// <returns>Task that returns a <see cref="Result{T}"/>.</returns>
    public Task<Result<T>> UnitOfWorkSingleAsync<T>(Func<DbConnection,Task<T?>> funcion)
    {
        return _UnitOfWorkNoTrAsync(funcion, r => new Result<T>(r), ex => new ExceptionError(ex));
    }
    /// <summary>
    /// Executes a unit of work against an open connection and returns an <see cref="EnumerableResult{T}"/>.
    /// The connection is opened and closed by this method.
    /// </summary>
    /// <typeparam name="T">Element type of the enumerable result.</typeparam>
    /// <param name="funcion">Function executed with the open connection.</param>
    /// <returns>An <see cref="EnumerableResult{T}"/> produced by the function or an exception result.</returns>
    public EnumerableResult<T> UnitOfWork<T>(Func<DbConnection,EnumerableResult<T>> funcion)
    {
       return _UnitOfWorkNoTr(funcion, r => r, ex => new ExceptionError(ex));
    }
    /// <summary>
    /// Executes a unit of work against an open connection and returns a <see cref="List{T}"/>.
    /// The connection is opened and closed by this method.
    /// </summary>
    /// <typeparam name="T">Element type of the list result.</typeparam>
    /// <param name="funcion">Function executed with the open connection.</param>
    /// <returns>An <see cref="EnumerableResult{T}"/> wrapping the returned list or an exception result.</returns>
    public EnumerableResult<T> UnitOfWork<T>(Func<DbConnection,IEnumerable<T>> funcion)
    {
        return _UnitOfWorkNoTr(funcion, r => new EnumerableResult<T>(r), ex => new ExceptionError(ex));
    }
    /// <summary>
    /// Asynchronous wrapper for <see cref="UnitOfWork{T}(Func{IDbConnection,EnumerableResult{T}})"/>.
    /// Runs the synchronous implementation on a thread-pool thread.
    /// </summary>
    /// <typeparam name="T">Element type of the enumerable result.</typeparam>
    /// <param name="funcion">Function executed with the open connection.</param>
    /// <returns>Task that returns an <see cref="EnumerableResult{T}"/>.</returns>
    public Task<EnumerableResult<T>> UnitOfWorkAsync<T>(Func<DbConnection,Task<EnumerableResult<T>>> funcion)
    {
        return _UnitOfWorkNoTrAsync(funcion, r => r, ex => new ExceptionError(ex));
    }
    /// <summary>
    /// Asynchronous wrapper for <see cref="UnitOfWork{T}(Func{IDbConnection,List{T}})"/>.
    /// Runs the synchronous implementation on a thread-pool thread.
    /// </summary>
    /// <typeparam name="T">Element type of the list result.</typeparam>
    /// <param name="funcion">Function executed with the open connection.</param>
    /// <returns>Task that returns an <see cref="EnumerableResult{T}"/>.</returns>
    public Task<EnumerableResult<T>> UnitOfWorkAsync<T>(Func<DbConnection,Task<IEnumerable<T>>> funcion)
    {
        return _UnitOfWorkNoTrAsync(funcion, r => new EnumerableResult<T>(r), ex => new ExceptionError(ex));
    }
    /// <summary>
    /// Create a UnitOfWork over a function that returns a bool and is executed in a IDbConnection and a IDbTransaction.
    /// The boolean result is converted to a <see cref="VoidResult"/>.
    /// </summary>
    /// <param name="funcion">Function executed with the connection and transaction.</param>
    /// <returns>A <see cref="VoidResult"/> representing success or failure.</returns>
    public VoidResult UnitOfWorkAction(Func<DbConnection,DbTransaction,bool> funcion)
    {
        return _UnitOfWorkWithTr<VoidResult,bool>(funcion,VoidResult.FromBool,ex=>Error.Exception(ex) );
    }
    /// <summary>
    /// Asynchronous wrapper for <see cref="UnitOfWork(Func{IDbConnection,IDbTransaction,bool})"/>.
    /// </summary>
    /// <param name="funcion">Function executed with the connection and transaction.</param>
    /// <returns>Task that returns a <see cref="VoidResult"/>.</returns>
    public Task<VoidResult> UnitOfWorkActionAsync(Func<DbConnection, DbTransaction, Task<bool>> funcion)
    {
        return _UnitOfWorkWithTrAsync<VoidResult,bool>(funcion,VoidResult.FromBool, ex => Error.Exception(ex));
    }
    /// <summary>
    /// Create a UnitOfWork over a function that returns a bool and is executed in a IDbConnection.
    /// The boolean result is converted to a <see cref="VoidResult"/>.
    /// </summary>
    /// <param name="funcion">Function executed with the connection.</param>
    /// <returns>A <see cref="VoidResult"/> representing success or failure.</returns>
    public VoidResult UnitOfWorkAction(Func<DbConnection,bool> funcion)
    {
        return _UnitOfWorkNoTr(funcion, VoidResult.FromBool, ex => Error.Exception(ex));
    }
    /// <summary>
    /// Asynchronous wrapper for <see cref="UnitOfWork(Func{IDbConnection,bool})"/>.
    /// </summary>
    /// <param name="funcion">Function executed with the connection.</param>
    /// <returns>Task that returns a <see cref="VoidResult"/>.</returns>
    public Task<VoidResult> UnitOfWorkActionAsync(Func<DbConnection, Task<bool>> funcion)
    {
        return _UnitOfWorkNoTrAsync(funcion, VoidResult.FromBool, ex => Error.Exception(ex));
    }
    /// <summary>
    /// Create a UnitOfWork over an action that is executed in a IDbConnection and a IDbTransaction.
    /// The action is executed inside a transaction; success is represented by a <see cref="VoidResult"/>.
    /// </summary>
    /// <param name="funcion">Action executed with the connection and transaction.</param>
    /// <returns>A <see cref="VoidResult"/> representing success.</returns>
    public VoidResult UnitOfWorkAction(Action<DbConnection,DbTransaction> funcion)
    {
        return UnitOfWorkAction((c,t) => {
            funcion(c,t);
            return true;
        });
    }
    /// <summary>
    /// Asynchronous wrapper for <see cref="UnitOfWork(Action{IDbConnection,IDbTransaction})"/>.
    /// </summary>
    /// <param name="funcion">Action executed with the connection and transaction.</param>
    /// <returns>Task that returns a <see cref="VoidResult"/>.</returns>
    public Task<VoidResult> UnitOfWorkActionAsync(Func<DbConnection,DbTransaction,Task> funcion)
    {
        return UnitOfWorkActionAsync(async (c,t) => {
            await funcion(c,t);
            return true;
        });
    }
    /// <summary>
    /// Create a UnitOfWork over an action that is executed in a IDbConnection.
    /// The action is executed with an open connection and success is represented by a <see cref="VoidResult"/>.
    /// </summary>
    /// <param name="funcion">Action executed with the open connection.</param>
    /// <returns>A <see cref="VoidResult"/> representing success.</returns>
    public VoidResult UnitOfWorkAction(Action<DbConnection> funcion)
    {
        return UnitOfWorkAction(c => {
            funcion(c);
            return true;
        });
    }
    /// <summary>
    /// Asynchronous wrapper for <see cref="UnitOfWork(Action{IDbConnection})"/>.
    /// </summary>
    /// <param name="funcion">Action executed with the open connection.</param>
    /// <returns>Task that returns a <see cref="VoidResult"/>.</returns>
    public Task<VoidResult> UnitOfWorkActionAsync(Func<DbConnection,Task> funcion)
    {
        return UnitOfWorkActionAsync(async c => {
            await funcion(c);
            return true;
        });
    }
    
}