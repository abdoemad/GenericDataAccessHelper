using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenericDAL
{
    internal static class DALHelper
    {
        //--------- Querying / Transacting --------

        // 1. Looking tables
        public static bool GenericRetrival<T>(Action<T> action) where T : DbContext, new()
        {
            try
            {
                using (var context = new T())
                {
                    action(context);
                    return true;
                }
            }
            catch (Exception ex)
            {
                // Log Error
                return false;
            }
        }

        // 2. Generic Result
        public static TResult GenericResultRetrival<T, TResult>(Func<T, TResult> func) where T : DbContext, new()
            where TResult : new()
        {
            try
            {
                using (var context = new T())
                {
                    TResult res = func(context);
                    return res;
                }
            }
            catch (Exception ex)
            {
                // Log Error
                return default(TResult);
            }
        }

        // 3. Asynchrouncly
        public static async Task<TResult> GenericResultRetrivalAsync<T, TResult>(Func<T, Task<TResult>> func)
            where T : DbContext, new()
            where TResult : new()
        {
            try
            {
                using (var context = new T())
                {
                    return await func(context);
                }
            }
            catch (Exception ex)
            {
                // Log Error
                return default(TResult);
            }
        }

        // 4. Long With no locking tables Asynchrouncly
        public static async Task<TResult> GenericResultNoLockLongRetrivalAsync<T, TResult>(Func<T, Task<TResult>> func)
            where T : DbContext, new()
            where TResult : new()
        {
            try
            {
                using (var context = new T())
                {
                    ((IObjectContextAdapter) context).ObjectContext.CommandTimeout = 0;
                    using (var dbContextTransaction = context.Database.BeginTransaction(IsolationLevel.ReadUncommitted))
                    {
                        return await func(context);
                    }
                }
            }
            catch (Exception exception)
            {
                // Log Error
                return default(TResult);
            }
        }

        // 5. Twice contexts asynchronously
        public static async Task<object> GenericTwiceContextsRetrivalAsync<T1, T2>(Func<T1, T2, Task<object>> func)
            where T1 : DbContext, new()
            where T2 : DbContext, new()
        {
            try
            {
                using (var context1 = new T1())
                {
                    using (
                        var dbContextTransaction1 = context1.Database.BeginTransaction(IsolationLevel.ReadUncommitted))
                    {
                        using (var context2 = new T2())
                        {
                            using (
                                var dbContextTransaction2 =
                                    context2.Database.BeginTransaction(IsolationLevel.ReadUncommitted)
                                )
                            {
                                return await func(context1, context2);
                            }
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                // Log Error

                return null;
            }
        }

        public static object GenericTwiceContextsRetrival<T1, T2, TResult>(Func<T1, T2, object> func)
            where T1 : DbContext, new()
            where T2 : DbContext, new()
        {
            try
            {
                using (var context1 = new T1())
                {
                    using (
                        var dbContextTransaction1 = context1.Database.BeginTransaction(IsolationLevel.ReadUncommitted))
                    {
                        using (var context2 = new T2())
                        {
                            using (
                                var dbContextTransaction2 =
                                    context2.Database.BeginTransaction(IsolationLevel.ReadUncommitted)
                                )
                            {
                                return func(context1, context2);
                            }
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                // Log Error

                return default(TResult);
            }
        }

        // ------------------- Saving ---------------

        // 6. Transactions as atom
        public static bool GenericSafeTransaction<T>(Action<T> action) where T : DbContext, new()
        {
            using (var context = new T())
            {
                using (var dbContextTransaction = context.Database.BeginTransaction())
                {
                    try
                    {
                        action(context);
                        dbContextTransaction.Commit();
                        return true;
                    }
                    catch (Exception ex)
                    {
                        dbContextTransaction.Rollback();
                        // Log Error
                        return false;
                    }
                }
            }
        }

        // 7. Asyncrounsly
        public static async Task<int?> GenericSafeTransactionAsync<T>(Action<T> action)
            where T : DbContext, new()
        {
            using (var context = new T())
            {
                using (var dbContextTransaction = context.Database.BeginTransaction())
                {
                    try
                    {
                        action(context);
                        int affectedRecords = await context.SaveChangesAsync();
                        dbContextTransaction.Commit();
                        return affectedRecords;
                    }
                    catch (Exception ex)
                    {
                        dbContextTransaction.Rollback();
                        // Log Error
                        return null;
                    }
                }
            }
        }

        public static async Task<int?> GenericSafeTransactionAsync2<T>(Func<T, Task<int>> func)
            where T : DbContext, new()
        {
            using (var context = new T())
            {
                using (var dbContextTransaction = context.Database.BeginTransaction())
                {
                    try
                    {
                        int affectedRecords = await func(context);
                        dbContextTransaction.Commit();
                        return affectedRecords;
                    }
                    catch (Exception ex)
                    {
                        dbContextTransaction.Rollback();
                        // Log Error
                        return null;
                    }
                }
            }
        }

        
    }
}
