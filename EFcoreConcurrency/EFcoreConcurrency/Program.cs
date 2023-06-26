using EFcoreConcurrency;
using Microsoft.EntityFrameworkCore;

var tasks = new List<Task>();
for (int i = 0; i < 5; i++)
{
    var task = Task.Run(() =>
    {
        for (int j = 0; j < 10; j++)
        {
            UpdateStock();
        }
    });
    tasks.Add(task);
}

Task.WaitAll(tasks.ToArray());

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
    Console.WriteLine($"【{Task.CurrentId}】【更新成功】");
}

//更新庫存DbUpdateConcurrencyException
async Task UpdateStockWithLock()
{
    var saved = false;
    while (!saved)
    {
        try
        {
            //樂觀鎖
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

            //悲觀鎖
            //using var dbContext = new TestContext();
            //using var tran = dbContext.Database.BeginTransaction();
            //var p = dbContext.EFstocks.FromSqlRaw<EFstock>("select * from EFstocks with(updlock) where ProductId = 'P01'").FirstOrDefault();
            //p.Quantity -= 1;
            //var result = dbContext.SaveChanges();
            //tran.Commit();
            //saved = true;

            Console.WriteLine($"【{Task.CurrentId}】【更新成功】");
        }
        catch (DbUpdateConcurrencyException ex)
        {
            Console.WriteLine($"【{Task.CurrentId}】【發生衝突!!!!!】{ex.Message}");
            Thread.Sleep(10);
        }
    }
}


