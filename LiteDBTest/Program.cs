using System.Drawing;
using LiteDB;
using Yitter.IdGenerator;

namespace LiteDBTest;

public class Program
{
    public static void Main()
    {
        Lock @lock = new Lock();
        // ManualResetEventSlim _manualResetEventSlim = new(false);
        var taskList1 = new List<Task>();
        for (int i = 0; i < 6; i++)
        {
            taskList1.Add(Task.Run(() =>
            {
                using var _ = @lock.EnterScope();
                Thread.Sleep(100);
                // _manualResetEventSlim.Reset();
                // using var timeOutCts = new CancellationTokenSource(TimeSpan.FromSeconds(2));
                // using var cts = CancellationTokenSource.CreateLinkedTokenSource(timeOutCts.Token, CancellationToken.None);
                // _manualResetEventSlim.Wait(cts.Token);
            }));
        }

        Task.WaitAll(taskList1.ToArray());

        YitIdHelper.SetIdGenerator(new IdGeneratorOptions
        {
            WorkerId = 1,
            WorkerIdBitLength = 6,
            SeqBitLength = 6
        });

        var customAttributes = typeof(TestA).GetProperty(nameof(TestA.Id))!.GetCustomAttributes(true);

        BsonMapper.Global.EmptyStringToNull = false;
        BsonMapper.Global.SerializeNullValues = true;
        BsonMapper.Global.EnumAsInteger = true;
        BsonMapper.Global.RegisterType(t => new BsonValue(t.ToString("yyyyMMddHHmmssfffffff")),
            t => DateTime.ParseExact(t.AsString, "yyyyMMddHHmmssfffffff", null));

        var db = new LiteDatabase(new ConnectionString(@"MyDatabase.db") { Connection = ConnectionType.Direct });
        // db.Dispose();
        {
            // 获取 "TestA" 集合（如果不存在则创建）
            var testACollection = db.GetCollection<TestA>("Core_Models_Models_Microscope_Focus_MicroscopeFocusItemDto_IdListCache", BsonAutoId.Int64);
            var testACollectionList = db.GetCollection("Core_Models_Models_Microscope_Focus_MicroscopeFocusItemDto_IdListCache1", BsonAutoId.Int64);
            var testACollectionList1 = db.GetCollection("abc", BsonAutoId.Int64);
            testACollection.DeleteMany(t => t.Id > 0);

            var taskList = new List<Task>();
            for (int i = 0; i < 1000; i++)
            {
                var i1 = i;
                taskList.Add(new Task(() =>
                {
                    try
                    {
                        var a = db.BeginTrans();
                        var a1 = db.BeginTrans();
                        var a2 = db.BeginTrans();
                        // var a3 = db.Commit();
                        Thread.Sleep(10);
                        var count1 = testACollectionList.Find(Query.All(Query.Descending)).Count();
                        var count2 = testACollection.Find(Query.All()).Count();
                        var bsonDocument = new BsonDocument { { "id", i1 } };
                        testACollectionList1.Insert(bsonDocument);
                        if (i1 is > 10 and < 20)
                            throw new Exception("Test" + i1);
                        var a4 = db.Commit();
                        // Console.WriteLine(a4);
                    }
                    catch (Exception ex)
                    {
                        var a4 = db.Rollback();
                        Console.WriteLine(ex.Message + a4);
                    }
                }));
            }

            // taskList执行
            taskList.ForEach(t => t.Start());

            Task.WaitAll(taskList.ToArray());

            var testA1 = RandomDataGenerator.GenerateTestA();
            // if (i < 5)
            //     testA.Id = YitIdHelper.NextId();
            // Thread.Sleep(500);
            testA1.Id = YitIdHelper.NextId();
            testA1.CreateDateTime = DateTime.Now;
            testA1.IsDelete = true;
            testA1.Bitmap = new Bitmap($"C:\\Users\\DELL\\Pictures\\PixPin_2024-12-25_14-47-56.bmp");
            // Thread.Sleep(1000);
            testACollection.Insert(testA1);
            var testAs = testACollection.Query().Where(t=>t.Id == testA1.Id).Where(t=>t.IsDelete == false).ToList();
            testA1.IsDelete = false;
            testACollection.Upsert(testA1);
            testAs = testACollection.Query().Where(t=>t.Id == testA1.Id).Where(t=>t.IsDelete == false).ToList();
            var document = testACollectionList1.Find(Query.All(Query.Descending)).First();
            document["id"] = -1;
            var upsert = testACollectionList1.Upsert(document);

            // 插入 10 条 TestA 数据
            List<TestA> insertedTestAs = new List<TestA>();
            BsonDocument doc = new BsonDocument();
            var list = new List<BsonValue>();
            for (int i = 0; i < 5; i++)
            {
                var testA = RandomDataGenerator.GenerateTestA();
                // if (i < 5)
                //     testA.Id = YitIdHelper.NextId();
                // Thread.Sleep(500);
                testA.CreateDateTime = DateTime.Now;
                // Thread.Sleep(1000);
                testACollection.Insert(testA);
                insertedTestAs.Add(testA); // 保存插入的数据以便验证
                Console.WriteLine($"TestA {i + 1} 已插入到数据库！");
                list.Add(testA.Id);
            }

            doc.Add("id", YitIdHelper.NextId());
            doc.Add("Ids", new BsonArray(list));
            testACollectionList.Insert(doc);
            testACollectionList.EnsureIndex("Ids");

            var longs = testACollectionList.Find(Query.All(Query.Descending)).Take(1).First()["Ids"].AsArray;
            var allTestAs = testACollection.Find(Query.In("_id", longs)).ToList().OrderByDescending(t => t.Id).ToList();

            // 打印插入的数据和查询的数据，验证一致性
            Console.WriteLine("\n插入的数据：");
            foreach (var item in insertedTestAs)
            {
                Console.WriteLine(item); // 自动打印所有属性
            }

            Console.WriteLine("\n查询的数据：");
            foreach (var item in allTestAs)
            {
                Console.WriteLine(item); // 自动打印所有属性
            }

            // 验证一致性（插入数据与查询数据应该一致）
            Console.WriteLine("\n验证一致性：");
            bool isConsistent = true;
            insertedTestAs = insertedTestAs.OrderByDescending(t => t.CreateDateTime).ToList();
            for (int i = 0; i < insertedTestAs.Count; i++)
            {
                var inserted = insertedTestAs[i];
                var queried = allTestAs[i];

                if (!inserted.Equals(queried))
                {
                    isConsistent = false;
                    Console.WriteLine($"数据不一致：插入数据与查询数据不匹配 (索引 {i})");
                }
            }

            if (isConsistent)
            {
                Console.WriteLine("\n所有数据一致！");
            }
            else
            {
                Console.WriteLine("\n数据存在不一致！");
            }
        }
    }
}

public interface ICacheItem
{
    [BsonId(autoId: false)]
    long Id { get; set; }

    DateTime CreateDateTime { get; set; }
}

public sealed record TestA : ICacheItem
{
    public long Id { get; set; }
    public long Id1 { get; } = 1234;
    public Bitmap Bitmap { get; set; }
    public bool IsDelete { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Age { get; set; }
    public double Length { get; set; }
    public TestEnum TestEnum { get; set; }

    public string TestB { get; set; } = null;

    // public List<TestB> TestBs { get; set; } = new();
    public List<TestB> TestBs1 { get; set; } = new();

    public DateTime CreateDateTime { get; set; }

    public Dictionary<string, List<double>> Test1 { get; set; } = [];
    public Dictionary<string, List<TestB>> Test2 { get; set; } = [];

    public Dictionary<string, (TestEnum, double)> Test3 { get; set; } = [];

    public bool Equals(TestA? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        return Name == other.Name && Age == other.Age && Length.Equals(other.Length) && TestEnum == other.TestEnum && CreateDateTime == other.CreateDateTime && TestBs1.Select((t, i) => t.Equals(other.TestBs1[i])).All(t => t);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Name, Age, Length, (int)TestEnum, TestB, TestBs1);
    }
}

public sealed record TestB
{
    public string Name { get; set; } = string.Empty;
    public int Age { get; set; }
    public double Length { get; set; }
    public TestEnum TestEnum { get; set; }
    public List<TestC> TestCs { get; set; } = new();

    public bool Equals(TestB? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        return Name == other.Name && Age == other.Age && Length.Equals(other.Length) && TestEnum == other.TestEnum && TestCs.Select((t, i) => t.Equals(other.TestCs[i])).All(t => t);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Name, Age, Length, (int)TestEnum, TestCs);
    }
}

public sealed record TestC
{
    public string Name { get; set; } = string.Empty;
    public int Age { get; set; }
    public double Length { get; set; }
    public TestEnum TestEnum { get; set; }

    public bool Equals(TestC? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        return Name == other.Name && Age == other.Age && Length.Equals(other.Length) && TestEnum == other.TestEnum;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Name, Age, Length, (int)TestEnum);
    }
}

public enum TestEnum
{
    Test1,
    Test2,
    Test3
}

public class RandomDataGenerator
{
    public static TestEnum GetRandomEnumValue()
    {
        var values = Enum.GetValues(typeof(TestEnum)).Cast<TestEnum>().ToArray();
        return values[Random.Shared.Next(values.Length)];
    }

    public static string GetRandomString(int length)
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
        char[] stringChars = new char[length];
        for (int i = 0; i < stringChars.Length; i++)
        {
            stringChars[i] = chars[Random.Shared.Next(chars.Length)];
        }

        return new string(stringChars);
    }

    public static int GetRandomAge()
    {
        return Random.Shared.Next(1, 100); // Random age between 1 and 100
    }

    public static double GetRandomLength()
    {
        return Random.Shared.NextDouble() * 100; // Random length between 0 and 100
    }

    public static TestA GenerateTestA()
    {
        var testA = new TestA
        {
            Name = GetRandomString(10),
            Age = GetRandomAge(),
            Length = GetRandomLength(),
            TestEnum = GetRandomEnumValue(),
            Test1 = new Dictionary<string, List<double>>() { { (0.1d, TestEnum.Test1).ToString(), new List<double>() { 0.1d, 0.2d } }, { (0.3d, TestEnum.Test3).ToString(), new List<double>() { 0.3d, 0.33d } }, },
            Test2 = new Dictionary<string, List<TestB>>() { { (0.1d, TestEnum.Test1).ToString(), [GenerateTestB()] }, { (0.3d, TestEnum.Test3).ToString(), [GenerateTestB()] } },
            Test3 = new Dictionary<string, (TestEnum, double)>() { { (0.1d, TestEnum.Test1).ToString(), (TestEnum.Test2, 100.1d) } },
            // TestB = GenerateTestC(),
            TestBs1 = new List<TestB>
            {
                GenerateTestB(), GenerateTestB()
            }
        };
        return testA;
    }

    public static TestB GenerateTestB()
    {
        var testB = new TestB
        {
            Name = GetRandomString(8),
            Age = GetRandomAge(),
            Length = GetRandomLength(),
            TestEnum = GetRandomEnumValue(),
            TestCs = new List<TestC> { GenerateTestC(), GenerateTestC() }
        };
        return testB;
    }

    public static TestC GenerateTestC()
    {
        return new TestC
        {
            Name = GetRandomString(6),
            Age = GetRandomAge(),
            Length = GetRandomLength(),
            TestEnum = GetRandomEnumValue()
        };
    }
}