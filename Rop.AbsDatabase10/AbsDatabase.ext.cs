using System.Collections;

namespace Rop.Database;

public partial class AbsDatabase
{
    // Delete partial-key
    /// <summary>
    /// Elimina filas de la tabla asociada al tipo <typeparamref name="T"/> usando una clave parcial.
    /// </summary>
    /// <typeparam name="T">Tipo de entidad cuya tabla contiene la clave parcial.</typeparam>
    /// <param name="id">Valor de la clave parcial a eliminar.</param>
    /// <returns>Resultado con el número de filas afectadas.</returns>
    public Result<int> DeleteByPartialKey<T>(object id) where T : class
        => UnitOfWorkSingle(conn => conn.DeleteByPartialKey<T>(id));

    /// <summary>
    /// Elimina filas (async) de la tabla asociada al tipo <typeparamref name="T"/> usando una clave parcial.
    /// </summary>
    /// <typeparam name="T">Tipo de entidad cuya tabla contiene la clave parcial.</typeparam>
    /// <param name="id">Valor de la clave parcial a eliminar.</param>
    /// <returns>Resultado asíncrono con el número de filas afectadas.</returns>
    public Task<Result<int>> DeleteByPartialKeyAsync<T>(object id) where T : class
        => UnitOfWorkSingleAsync(conn => conn.DeleteByPartialKeyAsync<T>(id));

    // Insert no key attributes
    /// <summary>
    /// Inserta una entidad omitiendo atributos de clave (inserción que ignora atributos de key etiquetados).
    /// </summary>
    /// <typeparam name="T">Tipo de la entidad a insertar.</typeparam>
    /// <param name="item">Entidad a insertar.</param>
    /// <returns>Resultado con el número de filas afectadas.</returns>
    public Result<int> InsertNoKeyAtt<T>(T item) where T : class
        => UnitOfWorkSingle(conn => conn.InsertNoKeyAtt<T>(item));

    /// <summary>
    /// Inserta una entidad (async) omitiendo atributos de clave.
    /// </summary>
    /// <typeparam name="T">Tipo de la entidad a insertar.</typeparam>
    /// <param name="item">Entidad a insertar.</param>
    /// <returns>Resultado asíncrono con el número de filas afectadas.</returns>
    public Task<Result<int>> InsertNoKeyAttAsync<T>(T item) where T : class
        => UnitOfWorkSingleAsync<int>(conn => conn.InsertNoKeyAttAsync<T>(item));

    // Insert or update with MERGE
    /// <summary>
    /// Inserta o actualiza una entidad usando la estrategia MERGE definida en las extensiones.
    /// </summary>
    /// <typeparam name="T">Tipo de la entidad.</typeparam>
    /// <param name="item">Entidad a insertar o actualizar.</param>
    /// <returns>Resultado con la clave o número de filas afectadas según la implementación.</returns>
    public Result<int> InsertOrUpdateMerge<T>(T item) where T : class
        => UnitOfWorkSingle(conn => conn.InsertOrUpdateMerge<T>(item));

    /// <summary>
    /// Inserta o actualiza una entidad usando MERGE (async).
    /// </summary>
    /// <typeparam name="T">Tipo de la entidad.</typeparam>
    /// <param name="item">Entidad a insertar o actualizar.</param>
    /// <returns>Resultado asíncrono con la clave o número de filas afectadas según la implementación.</returns>
    public Task<Result<int>> InsertOrUpdateMergeAsync<T>(T item) where T : class
        => UnitOfWorkSingleAsync(conn => conn.InsertOrUpdateMergeAsync<T>(item));

    // InsertOrUpdatePartial
    /// <summary>
    /// Reemplaza (borra y vuelve a insertar) las filas relacionadas con la clave parcial <paramref name="id"/>
    /// y luego inserta la lista de <paramref name="items"/> dentro de una transacción.
    /// </summary>
    /// <typeparam name="T">Tipo de las entidades parciales.</typeparam>
    /// <param name="id">Valor de la clave parcial a reemplazar.</param>
    /// <param name="items">Lista de entidades a insertar.</param>
    /// <returns>Resultado con true si todas las inserciones fueron realizadas correctamente.</returns>
    public Result<bool> InsertOrUpdatePartial<T>(object id, IReadOnlyList<T> items) where T : class
        => UnitOfWorkSingle((conn, tr) =>
        {
            var v = conn.InsertOrUpdatePartial<T>(id, items, tr);
            return true;
        }); // ejemplo; adapte tx handling

    /// <summary>
    /// Reemplaza las filas relacionadas con la clave parcial y reinserta las <paramref name="items"/> (async).
    /// </summary>
    /// <typeparam name="T">Tipo de las entidades parciales.</typeparam>
    /// <param name="id">Valor de la clave parcial a reemplazar.</param>
    /// <param name="items">Lista de entidades a insertar.</param>
    /// <returns>Resultado asíncrono con true si todas las inserciones fueron realizadas correctamente.</returns>
    public Task<Result<bool>> InsertOrUpdatePartialAsync<T>(object id, IReadOnlyList<T> items) where T : class
        => UnitOfWorkSingleAsync<bool>(async (conn, tr) =>
        {
            var v = await  conn.InsertOrUpdatePartialAsync<T>(id, items, tr);
            return v;
        });

    // UpdateIdValue async
    /// <summary>
    /// Actualiza el valor de un campo para la fila con la clave <paramref name="id"/> (async).
    /// </summary>
    /// <typeparam name="DB">Tipo que describe la tabla objetivo (usado para resolver metadatos).</typeparam>
    /// <typeparam name="T">Tipo del valor a asignar.</typeparam>
    /// <param name="id">Valor de la clave de fila a actualizar.</param>
    /// <param name="value">Nuevo valor a establecer.</param>
    /// <param name="field">Nombre del campo a actualizar.</param>
    /// <returns>Resultado asíncrono con true si se actualizó exactamente una fila.</returns>
    public Task<Result<bool>> UpdateIdValueAsync<DB, T>(object id, T value, string field)
        => UnitOfWorkSingleAsync<bool>(conn => conn.UpdateIdValueAsync<DB, T>(id, value, field));

    // GetPartial / GetPartial2
    /// <summary>
    /// Obtiene una entidad parcial identificada por dos claves (id1, id2).
    /// </summary>
    /// <typeparam name="T">Tipo de la entidad a recuperar.</typeparam>
    /// <param name="id1">Valor de la primera clave.</param>
    /// <param name="id2">Valor de la segunda clave.</param>
    /// <returns>Resultado con la entidad si existe, o null en caso contrario.</returns>
    public Result<T> GetPartial<T>(object id1, object id2) where T : class
        => UnitOfWorkSingle(conn => conn.GetPartial<T>(id1, id2));

    /// <summary>
    /// 
    /// </summary>
    /// <param name="id1"></param>
    /// <param name="id2"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public Task<Result<T>> GetPartialAsync<T>(object id1, object id2) where T : class
        => UnitOfWorkSingleAsync(conn => conn.GetPartialAsync<T>(id1, id2));

    /// <summary>
    /// Obtiene múltiples entidades parciales que coinciden con la clave <paramref name="id"/>.
    /// </summary>
    /// <typeparam name="T">Tipo de las entidades a recuperar.</typeparam>
    /// <param name="id">Valor de la clave parcial.</param>
    /// <returns>Enumeración de entidades encontradas.</returns>
    public EnumerableResult<T> GetPartial<T>(object id) where T : class
        => UnitOfWork(conn => conn.GetPartial<T>(id).ToList());

    public Task<EnumerableResult<T>> GetPartialAsync<T>(object id) where T : class
        => UnitOfWorkAsync(conn => conn.GetPartialAsync<T>(id));

    /// <summary>
    /// Obtiene múltiples entidades parciales que coinciden con la segunda clave parcial.
    /// </summary>
    /// <typeparam name="T">Tipo de las entidades a recuperar.</typeparam>
    /// <param name="id">Valor de la segunda clave parcial.</param>
    /// <returns>Enumeración de entidades encontradas.</returns>
    public EnumerableResult<T> GetPartial2<T>(object id) where T : class
        => UnitOfWork(conn => conn.GetPartial2<T>(id).ToList());
    public Task<EnumerableResult<T>> GetPartial2Async<T>(object id) where T : class
        => UnitOfWorkAsync(conn => conn.GetPartial2Async<T>(id));
}