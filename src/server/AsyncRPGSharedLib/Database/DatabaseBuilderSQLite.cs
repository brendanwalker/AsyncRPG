using System;
using System.Text;
using System.Data.Linq.Mapping;

namespace AsyncRPGSharedLib.Database
{
    public class DatabaseBuilderSQLite : IDatabaseBuilder
    {
        public string CreateDatabasePreambleSQL()
        {
            StringBuilder sqlText = new StringBuilder();

            sqlText.Append("pragma foreign_keys = ON;\r\n");

            return sqlText.ToString();
        }

        public string DeleteTableSQL(
            MetaTable table)
        {
            return string.Format("DROP TABLE IF EXISTS {0};", table.TableName);
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

            // TODO: Foreign key associations

            sqlText.Append(");\r\n");

            return sqlText.ToString();
        }

        public string CreateColomnSQL(
            MetaTable table,
            MetaDataMember member)
        {
            StringBuilder sqlText = new StringBuilder();

            if (member.IsPrimaryKey)
            {
                sqlText.AppendFormat("{0} INTEGER PRIMARY KEY AUTOINCREMENT ", member.MappedName);
            }
            else
            {
                sqlText.AppendFormat("{0} {1} ", member.MappedName, member.DbType);
            }

            if (!member.CanBeNull)
            {
                sqlText.Append("NOT NULL");
            }

            return sqlText.ToString();
        }
    }
}
