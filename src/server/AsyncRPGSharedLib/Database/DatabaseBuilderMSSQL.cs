using System.Text;
using System.Data.Linq.Mapping;

namespace AsyncRPGSharedLib.Database
{
    public class DatabaseBuilderMSSQL : IDatabaseBuilder
    {
        public string CreateDatabasePreambleSQL()
        {
            return "";
        }

        public string DeleteTableSQL(
            MetaTable table)
        {
            StringBuilder sqlText = new StringBuilder();

            sqlText.AppendFormat("IF OBJECT_ID('{0}', 'U') IS NOT NULL\r\n", table.TableName);
            sqlText.AppendFormat("DROP TABLE {0};\r\n", table.TableName);

            return sqlText.ToString();
        }

        public string CreateTableSQL(
            MetaTable table)
        {
            StringBuilder sqlText = new StringBuilder();

            sqlText.AppendFormat("CREATE TABLE {0}(\r\n", table.TableName);

            for (int memberIndex = 0; memberIndex < table.RowType.DataMembers.Count; memberIndex++)
            {
                MetaDataMember member = table.RowType.DataMembers[memberIndex];
                bool isLastColomn = memberIndex >= table.RowType.DataMembers.Count - 1;

                if (!member.IsAssociation)
                {
                    sqlText.AppendFormat("\t{0}{1}\r\n",
                        CreateColomnSQL(table, member),
                        isLastColomn ? "" : ",");
                }
            }

            sqlText.Append(");\r\n");

            return sqlText.ToString();
        }

        public string CreateColomnSQL(
            MetaTable table,
            MetaDataMember member)
        {
            StringBuilder sqlText = new StringBuilder();

            sqlText.AppendFormat("{0} {1} ", member.MappedName, FilterDBType(member.DbType));

            if (member.IsPrimaryKey)
            {
                sqlText.AppendFormat("IDENTITY(1,1) CONSTRAINT PK_{0} PRIMARY KEY CLUSTERED ", table.TableName);
            }

            // TODO: Foreign key associations

            if (!member.CanBeNull)
            {
                sqlText.Append("NOT NULL");
            }

            return sqlText.ToString();
        }

        private string FilterDBType(string dbType)
        {
            if (dbType == "blob")
            {
                return "varbinary(max)";
            }
            else
            {
                return dbType;
            }
        }
    }
}
