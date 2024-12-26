using LiteDB;

namespace LiteDBTest;

public class Program
{
    public static void Main()
    {
        using (var db = new LiteDatabase(@"MyDatabase.db"))
        {
            // 获取 "TestA" 集合（如果不存在则创建）
            var testACollection = db.GetCollection<TestA>("TestA");

            // 插入 10 条 TestA 数据
            List<TestA> insertedTestAs = new List<TestA>();
            for (int i = 0; i < 10; i++)
            {
                var testA = RandomDataGenerator.GenerateTestA();
                testACollection.Insert(testA);
                insertedTestAs.Add(testA); // 保存插入的数据以便验证
                Console.WriteLine($"TestA {i + 1} 已插入到数据库！");
            }

            // 查询所有 TestA 对象
            var allTestAs = testACollection.FindAll().ToArray();

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

public sealed record TestA
{
    public string Name { get; set; } = string.Empty;
    public int Age { get; set; }
    public double Length { get; set; }
    public TestEnum TestEnum { get; set; }
    public TestB TestB { get; set; } = new();
    public List<TestB> TestBs { get; set; } = new();

    public bool Equals(TestA? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        return Name == other.Name && Age == other.Age && Length.Equals(other.Length) && TestEnum == other.TestEnum && TestBs.Select((t, i) => t.Equals(other.TestBs[i])).All(t => t);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Name, Age, Length, (int)TestEnum, TestB, TestBs);
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
            TestB = GenerateTestB(),
            TestBs = new List<TestB> { GenerateTestB(), GenerateTestB() }
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