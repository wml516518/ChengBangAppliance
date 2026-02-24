using ApplianceRepair.Models;
using SqlSugar;

namespace ApplianceRepair.Services;

public class SqlSugarService
{
    private readonly IConfiguration _config;
    private SqlSugarScope? _db;

    public SqlSugarService(IConfiguration config)
    {
        _config = config;
    }

    public SqlSugarScope Db
    {
        get
        {
            if (_db == null)
            {
                var conn = _config.GetConnectionString("Default") ?? "Data Source=appliance.db";
                _db = new SqlSugarScope(new ConnectionConfig
                {
                    DbType = DbType.Sqlite,
                    ConnectionString = conn,
                    IsAutoCloseConnection = true
                });
            }
            return _db;
        }
    }

    public void InitDb()
    {
        Db.CodeFirst.InitTables<User>();
        Db.CodeFirst.InitTables<Category>();
        Db.CodeFirst.InitTables<Product>();
        Db.CodeFirst.InitTables<Order>();
        Db.CodeFirst.InitTables<OrderItem>();
        Db.CodeFirst.InitTables<ServiceType>();
        Db.CodeFirst.InitTables<ServiceItem>();
        MigrateManualOrdersTable();
        Db.CodeFirst.InitTables<ManualOrder>();
        try { Db.Ado.ExecuteCommand("ALTER TABLE Products ADD COLUMN ImagePath TEXT;"); } catch { }
        try { Db.Ado.ExecuteCommand("ALTER TABLE Orders ADD COLUMN PaymentMethod INTEGER DEFAULT 0;"); } catch { }
        try { Db.Ado.ExecuteCommand("ALTER TABLE Users ADD COLUMN IsTechnician INTEGER DEFAULT 0;"); } catch { }
        try { Db.Ado.ExecuteCommand("ALTER TABLE ManualOrders ADD COLUMN WarrantyType INTEGER DEFAULT 0;"); } catch { }
        try { Db.Ado.ExecuteCommand("ALTER TABLE ManualOrders ADD COLUMN Amount REAL DEFAULT 0;"); } catch { }
        try { Db.Ado.ExecuteCommand("ALTER TABLE ManualOrders ADD COLUMN AppointmentStart TEXT;"); } catch { }
        try { Db.Ado.ExecuteCommand("ALTER TABLE ManualOrders ADD COLUMN AppointmentEnd TEXT;"); } catch { }
        try { Db.Ado.ExecuteCommand("ALTER TABLE ManualOrders ADD COLUMN Photos TEXT;"); } catch { }

        SeedServiceTypes();
    }

    private void MigrateManualOrdersTable()
    {
        try
        {
            var cols = Db.Ado.SqlQuery<dynamic>("PRAGMA table_info('ManualOrders')");
            if (cols == null || cols.Count == 0) return;
            var needRebuild = false;
            foreach (var col in cols)
            {
                var dict = (IDictionary<string, object>)col;
                var name = dict["name"]?.ToString();
                var notNull = Convert.ToInt32(dict["notnull"]);
                if ((name == "AssignTime" || name == "CompleteTime") && notNull == 1)
                {
                    needRebuild = true;
                    break;
                }
            }
            if (needRebuild)
            {
                Db.Ado.ExecuteCommand("DROP TABLE ManualOrders");
            }
        }
        catch { }
    }

    private void SeedServiceTypes()
    {
        if (Db.Queryable<ServiceType>().Any()) return;

        var types = new[]
        {
            new ServiceType { Name = "安装服务", SortOrder = 1, IsEnabled = true },
            new ServiceType { Name = "维修服务", SortOrder = 2, IsEnabled = true },
            new ServiceType { Name = "拆装移机", SortOrder = 3, IsEnabled = true },
            new ServiceType { Name = "上门设计咨询", SortOrder = 4, IsEnabled = true },
            new ServiceType { Name = "拆旧回收服务", SortOrder = 5, IsEnabled = true },
            new ServiceType { Name = "商用设备维保服务", SortOrder = 6, IsEnabled = true },
        };
        Db.Insertable(types).ExecuteCommand();

        var installTypeId = Db.Queryable<ServiceType>().First(x => x.Name == "安装服务")?.Id ?? 1;
        var repairTypeId = Db.Queryable<ServiceType>().First(x => x.Name == "维修服务")?.Id ?? 2;

        var items = new[]
        {
            new ServiceItem { ServiceTypeId = installTypeId, Name = "空调安装", SortOrder = 1, IsEnabled = true },
            new ServiceItem { ServiceTypeId = installTypeId, Name = "热水器安装", SortOrder = 2, IsEnabled = true },
            new ServiceItem { ServiceTypeId = installTypeId, Name = "洗衣机安装", SortOrder = 3, IsEnabled = true },
            new ServiceItem { ServiceTypeId = installTypeId, Name = "油烟机安装", SortOrder = 4, IsEnabled = true },
            new ServiceItem { ServiceTypeId = repairTypeId, Name = "空调维修", SortOrder = 1, IsEnabled = true },
            new ServiceItem { ServiceTypeId = repairTypeId, Name = "冰箱维修", SortOrder = 2, IsEnabled = true },
            new ServiceItem { ServiceTypeId = repairTypeId, Name = "洗衣机维修", SortOrder = 3, IsEnabled = true },
        };
        Db.Insertable(items).ExecuteCommand();
    }
}
