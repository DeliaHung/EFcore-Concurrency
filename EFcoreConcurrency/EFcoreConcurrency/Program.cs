using EFcoreConcurrency;
using Microsoft.EntityFrameworkCore;

var tasks = new List<Task>();
for (int i = 0; i < 10; i++)
{
    var task = Task.Run(() =>
    {
        for (int j = 0; j < 10; j++)
        {
            UpdateStockWithOptimistic();
        }
    });
    tasks.Add(task);
}

Task.WaitAll(tasks.ToArray());

foreach (Task t in tasks)
    Console.WriteLine("Task {0} Status: {1}", t.Id, t.Status);

Console.WriteLine("Number of files read: {0}", tasks.Count);


//樂觀鎖
async Task UpdateStockWithOptimistic()
{
    var saved = false;
    while (!saved)
    {
        try
        {
            using var dbContext = new TestContext();

            var p = dbContext.EFstocks.FirstOrDefault(f => f.ProductId == "P01");

            if (p.Quantity <= 0)
            {
                Console.WriteLine($"【{Task.CurrentId}】【庫存不足】");
                break;
            }
            p.Quantity -= 1;

            dbContext.SaveChanges();

            saved = true;
            Console.WriteLine($"【{Task.CurrentId}】【成功下單】");
        }
        catch (DbUpdateConcurrencyException ex)
        {
            Console.WriteLine($"【{Task.CurrentId}】【發生衝突!!!!!】");
            Thread.Sleep(10);
        }
    }
}

//悲觀鎖
async Task UpdateStockWithPessimistic()
{
    var saved = false;
    while (!saved)
    {
        try
        {
            using var dbContext = new TestContext();
            using var tran = dbContext.Database.BeginTransaction();
            var p = dbContext.EFstocks.FromSqlRaw("select * from EFstocks with(updlock) where ProductId = 'P01'").FirstOrDefault();

            if (p.Quantity <= 0)
            {
                Console.WriteLine($"【{Task.CurrentId}】【庫存不足】");
                break;
            }

            p.Quantity -= 1;

            var result = dbContext.SaveChanges();
            tran.Commit();

            saved = true;

            Console.WriteLine($"【{Task.CurrentId}】【成功下單】");
        }
        catch (DbUpdateConcurrencyException ex)
        {
            Console.WriteLine($"【{Task.CurrentId}】【發生衝突!!!!!】");
            Thread.Sleep(10);
        }
    }
}


//一般更新庫存
async Task UpdateStock()
{
    using var dbContext = new TestContext();
    var p = dbContext.EFstocks.FirstOrDefault(f => f.ProductId == "P01");
    if (p.Quantity <= 0)
    {
        Console.WriteLine($"【{Task.CurrentId}】【庫存不足】");
        return;
    }
    p.Quantity -= 1;
    dbContext.SaveChanges();
    Console.WriteLine($"【{Task.CurrentId}】【成功下單】");
}


