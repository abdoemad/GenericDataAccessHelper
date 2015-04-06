<h2>Introduction</h2>

<p> The goal of this helper is to develop a generic - so reusable - data access helper using entity framework. While the motivation of it is to eliminate redundant code using Generics and Delegates. 
</p>
<p>The helper divided into two sections, the first is querying (Retrieval) business, the second is transact (Update/Insert/Delete) database witch are implemented synchronously and asynchronously 
</p>
<p>In details, the generic here means apply <b>Generics</b> over methods that takes a delegate - of type <code>Action</code> in case it doesn't return, or of type <code>Func</code> in case it returns result - as input witch in turn takes an input of type <code>DBContext</code> as a base class inherited by your concrete class
</p>
Each method - depends on its role - encapsulates:
<ul>
<li> Initiating a concrete entity framework context with <code>using</code></li>
<li> Begin the transaction scope with locking (Default) or unlocking (IsolationLevel.ReadUncommitted) tables </li>
<li> Try / Catch body</li>
<li> Delegate execution</li>
<li> The Commit and Roll-back logic, in case it is an atomic transaction that consist of a set of transactions</li>
<li> Result returning, boolean type indicates querying / transaction state, or output type that is specified as generic </li>
<li> Asynchronous logic</li>
<li> Log exceptions </li>
</ul>
<p>All those logics not going to be written into your business-based data access logic, so the redundant code is eliminated</p>

<h3> Motivation</h3>
<pre>
public static List&ltEmployee&gt; GeAllEmployees()
{
    try
    {
        using (var northwindContext = new NorthwindDBContext())
        {
            var query = from e in northwindContext.Employees select e;
            return query.ToList();
        }
    }
    catch (Exception ex)
    {
        // Log Error
    }
}
</pre>
<p> So, I have to <b>redundant</b> this code with a new business (e.g. GetEmployeeOrders). Also, if I have to access another database witch means another <code>DBContext</code> I have to redundant this logic!!</p>
<p> Here, the <b>Generics</b> and <b>Delegates</b> comes as a solution for those two issues. So I created <code>public static class</code> called <code><a href="https://github.com/abdoemad/GenericDataAccessHelper/blob/master/GenericDAL/DALHelper.cs">DALHelper</a></code> contains the following static methods</p>

<h3> 1. Retrieval</h3>
<p> All retrieval's methods could also be used for transacting database.</p>
<h4> 1. <u>Locking</u> tables</h4>
<pre>
public static bool GenericRetrival&lt;T&gt;(Action&lt;T&gt; action) where T : DbContext, new()
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
</pre>
<h5>Usage</h5>
<pre>
public List&ltEmployee&gt; GeAllEmployees()
{
	List&ltEmployee&gt; result= null;
	bool success = DALHelper.GenericRetrival&ltNorthwindDBContext&gt;((northwindContext) =>
	{
		result = (from e in northwindContext.Employees select e).ToList();
	});
	return result;
}
</pre>
<h4> 2. With a <u>Generic Result</u></h4>
<pre>
public static TResult GenericResultRetrival&ltT, TResult&gt;(Func&ltT, TResult&gt; func) where T : DbContext, new()
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
</pre>
<h5>Usage</h5>
<pre>
public List&ltEmployee&gt; GeAllEmployees()
{
	List&ltEmployee&gt; result = DALHelper.GenericResultRetrival&ltNorthwindDBContext,List&ltEmployee&gt;&gt;((northwindContext) =>
	{
		return (from e in northwindContext.Employees select e).ToList();
	});
	return result;
}
</pre>
<h4> 3. <u>Asynchrouncly</u></h4>
<pre>
public static async Task&ltTResult&gt; GenericRetrivalAsync&ltT, TResult&gt;(Func&ltT, Task&ltTResult&gt;&gt; func)
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
</pre>
<h5>Usage</h5>
<pre>
public async Task&ltList&ltEmployee&gt;&gt; GetAllEmployeesAsync()
{
    return await DALHelper.GenericRetrivalAsync&ltNorthwindDBContext, List&ltEmployee&gt;&gt;(async (northwindContext) =>
    {
        return await (from e in northwindContext.Employees select e).ToListAsync();
    });
}
</pre>
<h4> 4. <u>Long</u> With <u>no locking</u> tables Asynchrouncly</h4>
<pre>
public static async Task&ltTResult&gt; GenericResultNoLockLongRetrivalAsync&ltT,TResult&gt;(Func&ltT, Task&ltTResult&gt;&gt; func)
    where T : DbContext, new()
    where TResult : new()
{
    try
    {
        using (var context = new T())
        {
            ((IObjectContextAdapter)context).ObjectContext.CommandTimeout = 0;
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
</pre>
<h4> 5. <u>Twice</u> contexts asynchronously</h4>
<pre>
public static async Task&ltobject&gt; GenericTwiceContextsRetrivalAsync&ltT1, T2&gt;(Func&ltT1, T2, Task&ltobject&gt;&gt; func)
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
</pre>
<h5>Usage</h5>
<pre>
public async Task&ltobject&gt; GetDistributedDataAsync()
{
    return await DALHelper.GenericTwiceContextsRetrivalAsync&ltNorthwindDBContext, AdventureWorkDBContext&gt;(async
        (northwindContext, advantureContext) =>
        {
            var employees = (from e in northwindContext.Employees select e).ToListAsync();
            var cutomers = (from c in advantureContext.Customers select c).ToListAsync();

            await Task.WhenAll(employees, cutomers);
            return new
            {
                EmployeeList = employees.Result,
                PersonList = cutomers.Result
            };
        });
}
</pre>
<h2> 2. Saving</h2>
<h4> 6. Transactions as atom</h4>
<pre>
public static bool GenericSafeTransaction&ltT&gt;(Action&ltT&gt; action) where T : DbContext, new()
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
</pre>
<h5>Usage</h5>
<pre>
public bool AddMultipleRecords(Employee newEmp, Supplier newSup)
{
    return DALHelper.GenericSafeTransaction&ltNorthwindDBContextgt;(northwindContext =>
    {
        northwindContext.Employees.Add(newEmp);
        northwindContext.SaveChanges();
        northwindContext.Suppliers.Add(newSup);
        northwindContext.SaveChanges();
    });
}
</pre>
<h4> 7. Asyncrounsly</h4>
<pre>
public static async Task&ltint?&gt; GenericSafeTransactionAsync&ltT&gt;(Action&ltT&gt; action)
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
</pre>
<h5>Usage</h5>
<pre>
return await DALHelper.GenericSafeTransactionAsync&ltNorthwindDBContext&gt;( async (northwindContext) =>
{
	northwindContext.Employees.Add(newEmp);
});
</pre>
