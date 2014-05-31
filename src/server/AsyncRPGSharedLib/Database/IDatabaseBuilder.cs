using System.Data.Linq.Mapping;

namespace AsyncRPGSharedLib.Database
{
    public interface IDatabaseBuilder
    {
        string CreateDatabasePreambleSQL();
        string DeleteTableSQL(MetaTable table);
        string CreateTableSQL(MetaTable table);
        string CreateColomnSQL(MetaTable table, MetaDataMember member);
    }
}
